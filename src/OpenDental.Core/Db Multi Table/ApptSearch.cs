using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ApptSearch
{
    public static List<ScheduleOpening> GetSearchResultsForBlockoutAndProvider(List<long> listProvNums, long apptNum, DateTime startDate, DateTime endDate, List<long> listOpNums, List<long> listClinicNums, TimeSpan timeBefore, TimeSpan timeAfter, List<long> listBlockoutTypes, int resultCount = 10)
    {
        List<ScheduleOpening> listOpenings = [];
        //searching for intersection of provider and blockout type. Search needs to be handled differently. 
        List<long> listProvs = listProvNums.FindAll(x => x != 0); //list of providers (excluding the blockout provNum 0)
        List<ScheduleOpening> listOpeningsForProvs = [];
        List<ScheduleOpening> listOpeningForBlockout = [];
        //list of all openings for the given providers. 
        listOpeningsForProvs = GetSearchResults(apptNum, startDate, endDate, listProvs, listOpNums, listClinicNums, timeBefore, timeAfter,
            hasProvAndBlockout: true).OrderBy(x => x.DateTimeAvail).ToList();
        listOpeningsForProvs.RemoveAll(x => x.ProvNum == 0); //blockouts should not be in the list when comparing. 
        //list of all openings for the given blockouts - list of providers is just provNum 0 for blockout purposes.
        listOpeningForBlockout = GetSearchResults(apptNum, startDate, endDate, [0], listOpNums, listClinicNums, timeBefore,
            timeAfter, listBlockoutTypes, true).OrderBy(x => x.DateTimeAvail).ToList();
        HashSet<HItem> hashSetHItems = [];
        for (int i = 0; i < listOpeningForBlockout.Count; i++)
        {
            HItem hItem = new HItem();
            hItem.DateTimeAvailable = listOpeningForBlockout[i].DateTimeAvail;
            hItem.OpNum = listOpeningForBlockout[i].OpNum;
            hItem.ClinicNum = ScheduleOpening.ClinicNum;
            hashSetHItems.Add(hItem); //silently fails if duplicate, which is exactly what we want.
        }

        //Get the first DateTime,OpNum,Clinic combo that are present in both the provider and blockout openings.
        Dictionary<DateTime, List<ScheduleOpening>> dictOpeningsByDate = listOpeningsForProvs
            .Where(x => hashSetHItems.Contains(new HItem {DateTimeAvailable = x.DateTimeAvail, ClinicNum = ScheduleOpening.ClinicNum, OpNum = x.OpNum}))
            .GroupBy(x => x.DateTimeAvail.Date)
            .ToDictionary(x => x.Key, x => x.ToList());
        //Only return one opening per day and limit the results by resultCount that was passed in.
        foreach (DateTime dateKey in dictOpeningsByDate.Keys)
        {
            listOpenings.Add(dictOpeningsByDate[dateKey].First());
            if (listOpenings.Count >= resultCount)
            {
                break;
            }
        }

        return listOpenings;
    }

    private class HItem
    {
        public DateTime DateTimeAvailable = DateTime.MinValue;
        public long OpNum;
        public long ClinicNum;

        public override bool Equals(object obj)
        {
            HItem hItem = obj as HItem;
            if (hItem == null)
            {
                return false;
            }

            return (DateTimeAvailable.Equals(hItem.DateTimeAvailable) && OpNum == hItem.OpNum && ClinicNum == hItem.ClinicNum);
        }

        public override int GetHashCode()
        {
            int retval = 486187739; //Arbitrary prime number
            int prime = 104743;
            unchecked
            {
                //Overflow is fine, just wrap around
                retval += DateTimeAvailable.GetHashCode() + prime * retval;
                retval += OpNum.GetHashCode() + prime * retval;
                retval += ClinicNum.GetHashCode() + prime * retval;
                return retval;
            }
        }
    }

    public static List<ScheduleOpening> GetSearchResults(long aptNum, DateTime dateStart, DateTime dateEnd, List<long> listProvNums, List<long> listOpNums, List<long> listClinicNums, TimeSpan beforeTime, TimeSpan afterTime, List<long> listBlockoutTypes = null, bool hasProvAndBlockout = false, int resultCount = 10, bool isForMakeRecall = false)
    {
        if (listBlockoutTypes is null)
        {
            listBlockoutTypes = [0];
        }

        //if they didn't set a before time, set it to a large timespan so that we can use the same logic for checking appointment times.
        if (beforeTime == TimeSpan.FromSeconds(0))
        {
            beforeTime = TimeSpan.FromHours(25); //bigger than any time of day.
        }

        ApptSearchData data = GetDataForSearch(aptNum, dateStart, dateEnd, listProvNums, listOpNums, listClinicNums, listBlockoutTypes, isForMakeRecall: isForMakeRecall);
        List<ScheduleOpening> retVal = [];
        if (data.AppointmentToAdd == null)
        {
            //appointment was deleted after clicking search. 
            return retVal;
        }

        DateTime dateEvaluating = data.DateEvaluating;
        SearchBehaviorCriteria searchType = (SearchBehaviorCriteria) PrefC.GetInt(PrefName.AppointmentSearchBehavior);
        if (hasProvAndBlockout)
        {
            //searching for intersection of providers and blockouts get as many results as possible.
            while (dateEvaluating < dateEnd)
            {
                List<ScheduleOpening> listPotentialTimeAvailable = GetProvAndOpAvailabilityHelper(listProvNums, dateEvaluating, data, searchType, listOpNums
                    , listBlockoutTypes);
                //At this point listPotentialTimeAvailable is already filtered and only contains appt times that match both provider time and operatory time. 
                List<ScheduleOpening> listOpeningsForEntireDay = AddTimesToSearchResultsHelper(listPotentialTimeAvailable, beforeTime, afterTime);
                retVal.AddRange(listOpeningsForEntireDay);
                dateEvaluating = dateEvaluating.AddDays(1);
            }
        }
        else
        {
            while (retVal.Count < resultCount && dateEvaluating < dateEnd)
            {
                List<ScheduleOpening> listPotentialTimeAvailable = GetProvAndOpAvailabilityHelper(listProvNums, dateEvaluating, data, searchType, listOpNums
                    , listBlockoutTypes);
                //At this point listPotentialTimeAvailable is already filtered and only contains appt times that match both provider time and operatory time. 
                ScheduleOpening firstOpeningForDay = AddTimeToSearchResultsHelper(listPotentialTimeAvailable, beforeTime, afterTime);
                if (firstOpeningForDay != null)
                {
                    retVal.Add(firstOpeningForDay);
                }

                dateEvaluating = dateEvaluating.AddDays(1);
            }
        }

        return retVal;
    }

    private static List<ScheduleOpening> GetProvAndOpAvailabilityHelper(List<long> listProvNums, DateTime dateEvaluating, ApptSearchData data, SearchBehaviorCriteria searchType, List<long> listOpNums, List<long> listBlockoutTypes)
    {
        List<ScheduleOpening> listPotentialTimeAvailable = []; //create or clear
        //Providers---------------------------------------------------------------------------------------------------------------
        List<ApptSearchProviderSchedule> listProvScheds = []; //Provider Bar, ProviderSched Bar, Date and Provider
        listProvScheds = Appointments.GetProviderScheduleForProvidersAndDate(listProvNums, dateEvaluating, data.ListSchedules, data.ListAppointments);
        if (searchType == SearchBehaviorCriteria.ProviderTime)
        {
            //Fill the time the provider is available
            listPotentialTimeAvailable = FillProviderTimeHelper(listProvScheds, data.AppointmentToAdd, dateEvaluating, listBlockoutTypes);
        }

        //Handle Operatories -----------------------------------------------------------------------------------------------------
        if (searchType == SearchBehaviorCriteria.ProviderTimeOperatory)
        {
            //Fill the time the prov and op are available
            List<ApptSearchOperatorySchedule> listOpScheds = []; //filtered based on SearchType
            listOpScheds = GetAllForDate(dateEvaluating, data.ListSchedules, data.ListAppointments, data.ListSchedOps, listOpNums, listProvNums, listBlockoutTypes);
            listPotentialTimeAvailable = FillOperatoryTime(listOpScheds, listProvScheds, data.AppointmentToAdd, dateEvaluating, listProvNums, listBlockoutTypes
                , data.ListSchedOps, data.ListSchedules);
        }

        return listPotentialTimeAvailable;
    }

    private static ScheduleOpening AddTimeToSearchResultsHelper(List<ScheduleOpening> listApptTimeForBehavior, TimeSpan beforeTime, TimeSpan afterTime)
    {
        ScheduleOpening firstAvailability = null;
        listApptTimeForBehavior = listApptTimeForBehavior.OrderBy(x => x.DateTimeAvail).ToList();
        for (int i = 0; i < listApptTimeForBehavior.Count; i++)
        {
            if (listApptTimeForBehavior[i].DateTimeAvail.TimeOfDay > beforeTime || listApptTimeForBehavior[i].DateTimeAvail.TimeOfDay < afterTime)
            {
                continue;
            }

            firstAvailability = listApptTimeForBehavior[i]; //add one for this day (only want one time per day).
            break;
        }

        return firstAvailability;
    }

    private static List<ScheduleOpening> AddTimesToSearchResultsHelper(List<ScheduleOpening> listApptTimeForBehavior, TimeSpan beforeTime, TimeSpan afterTime)
    {
        List<ScheduleOpening> listAvailability = [];
        for (int i = 0; i < listApptTimeForBehavior.Count; i++)
        {
            if (listApptTimeForBehavior[i].DateTimeAvail.TimeOfDay > beforeTime || listApptTimeForBehavior[i].DateTimeAvail.TimeOfDay < afterTime)
            {
                continue;
            }

            listAvailability.Add(listApptTimeForBehavior[i]);
        }

        return listAvailability;
    }

    public static ApptSearchData GetDataForSearch(long aptNum, DateTime dateAfter, DateTime dateBefore, List<long> listProvNums, List<long> listOpNums, List<long> listClinicNums, List<long> listBlockoutTypes, bool isForMakeRecall = false)
    {
        ApptSearchData data = new ApptSearchData();
        data.DateEvaluating = dateAfter.AddDays(1);
        data.AppointmentToAdd = Appointments.GetOneApt(aptNum);
        data.ListSchedules = Schedules.GetSchedulesForAppointmentSearch(data.DateEvaluating, dateBefore, listClinicNums, listOpNums
            , listProvNums, listBlockoutTypes, isForMakeRecall: isForMakeRecall);
        //Get all appointments that exist in the operaotries we will be searching to find an opening, not just for provider we're looking for
        //so we can get conflicts when multiple provs work in a single operaotry.  
        data.ListAppointments = Appointments.GetForPeriodList(data.DateEvaluating, dateBefore, listOpNums, listClinicNums);
        data.ListSchedOps = ScheduleOps.GetForSchedList(data.ListSchedules, listOpNums); //ops filter for case when a prov is scheduled in multiple ops
        return data;
    }

    private static List<ScheduleOpening> FillProviderTimeHelper(List<ApptSearchProviderSchedule> listProvScheds, Appointment appointmentToAdd, DateTime dayEvaluating, List<long> listBlockoutTypes)
    {
        List<ScheduleOpening> listPotentialProvApptTime = []; //clear or create
        if (listProvScheds == null || appointmentToAdd == null)
        {
            return listPotentialProvApptTime;
        }

        foreach (ApptSearchProviderSchedule providerSchedule in listProvScheds)
        {
            for (int j = 0; j < 288; j++)
            {
                //search every 5 minute increment per day
                //listBlockoutTypes should always have at least one value. 0 as the sole listBlockoutTypes value means we are not searching for blockouts, so we only want provider schedules
                bool isProviderSchedulesOnly = (providerSchedule.ProviderNum == 0 && listBlockoutTypes[0] == 0);
                if (j + appointmentToAdd.Pattern.Length > 288)
                {
                    //skip if appointment length spans over a 24 hour period.
                    break;
                }

                if (listPotentialProvApptTime.Select(x => x.DateTimeAvail).Contains(dayEvaluating.AddMinutes(j * 5)))
                {
                    continue; //skip if provider isn't available in this 5 min increment
                }

                bool addDateTime = true;
                for (int k = 0; k < appointmentToAdd.Pattern.Length; k++)
                {
                    if ((providerSchedule.IsProvAvailable[j + k] == false && appointmentToAdd.Pattern[k] == 'X') || providerSchedule.IsProvScheduled[j + k] == false
                                                                                                                 || isProviderSchedulesOnly)
                    {
                        addDateTime = false;
                        break;
                    }
                }

                if (addDateTime)
                {
                    listPotentialProvApptTime.Add(new ScheduleOpening
                    {
                        DateTimeAvail = dayEvaluating.AddMinutes(j * 5),
                        ProvNum = providerSchedule.ProviderNum
                    });
                }
            }
        }

        return listPotentialProvApptTime;
    }

    private static List<ScheduleOpening> FillOperatoryTime(List<ApptSearchOperatorySchedule> listApptSearchOperatorySchedules, List<ApptSearchProviderSchedule> listApptSearchProviderSchedules, Appointment appointmentToAdd, DateTime dateEvaluating, List<long> listProvNums, List<long> listBlockoutTypes, List<ScheduleOp> listScheduleOps, List<Schedule> listSchedules)
    {
        List<ScheduleOpening> listScheduleOpenings = []; //create or clear 
        for (int i = 0; i < 288; i++)
        {
            //search every 5 minute increment per day
            if (i + appointmentToAdd.Pattern.Length > 288)
            {
                //skip if appointment would span across midnight
                break;
            }

            foreach (ApptSearchOperatorySchedule apptSearchOperatorySchedule in listApptSearchOperatorySchedules)
            {
                bool doAddDateTime = true;
                for (int k = 0; k < appointmentToAdd.Pattern.Length; k++)
                {
                    //check appointment against operatories
                    if (apptSearchOperatorySchedule.IsOpAvailable[i + k] == false)
                    {
                        doAddDateTime = false;
                        break;
                    }
                }

                if (!doAddDateTime)
                {
                    continue;
                }

                //check appointment against providers available for the given operatory
                bool isProvAvailable = false;
                long provNumAvail = 0;
                for (int k = 0; k < listProvNums.Count; k++)
                {
                    if (!apptSearchOperatorySchedule.ProviderNums.Contains(listProvNums[k]))
                    {
                        continue;
                    }

                    isProvAvailable = true;
                    provNumAvail = listApptSearchProviderSchedules[k].ProviderNum;
                    for (int m = 0; m < appointmentToAdd.Pattern.Length; m++)
                    {
                        //If provider is not available, and the appointment has an "X" for provider time at this spot in the pattern
                        //OR provider is not scheduled to work
                        //OR current schedule is a blockout, while we're not considering blockouts
                        if ((listApptSearchProviderSchedules[k].IsProvAvailable[i + m] == false && appointmentToAdd.Pattern[m] == 'X')
                            || listApptSearchProviderSchedules[k].IsProvScheduled[i + m] == false
                            || listApptSearchProviderSchedules[k].ProviderNum == 0 && listBlockoutTypes[0] == 0)
                        {
                            isProvAvailable = false;
                            break;
                        }
                        else if (provNumAvail == 0)
                        {
                            //This is a blockout schedule, which we want to consider since we got to this point (provNumAvail is 0 and listBlockoutTypes contains non-zero elements). We only want to return times where blockout types are scheduled.
                            //Get a list of any blockouts that are scheduled within this 5 minute timeframe. 
                            //If none of those blockouts cover our current ApptSearchOperatorySchedule's operatory at this time, then we do not want to consider this time as available.
                            List<Schedule> listSchedulesBlockouts = listSchedules.FindAll(x => x.ProvNum == 0
                                                                                               && x.SchedDate.Date == dateEvaluating.Date
                                                                                               && x.StartTime <= dateEvaluating.AddMinutes(i * 5).TimeOfDay
                                                                                               && x.StopTime >= dateEvaluating.AddMinutes(i * 5).TimeOfDay).ToList();
                            List<long> listOpNumsForSchedule = listScheduleOps.FindAll(x => listSchedulesBlockouts.Any(y => x.ScheduleNum == y.ScheduleNum))
                                .Select(x => x.OperatoryNum).Distinct().ToList();
                            if (!listOpNumsForSchedule.Contains(apptSearchOperatorySchedule.OperatoryNum))
                            {
                                isProvAvailable = false;
                            }
                        }
                        else
                        {
                            //This is an open provider schedule, but they may not be scheduled at this time for this operatory. Verify the provider is open at this time, and if applicable, for this operatory.
                            List<Schedule> listSchedulesForProviderAndOp = listSchedules.FindAll(x => x.ProvNum == provNumAvail
                                                                                                      && (x.Ops.Count == 0 || x.Ops.Contains(apptSearchOperatorySchedule.OperatoryNum))
                                                                                                      && x.SchedDate.Date == dateEvaluating.Date
                                                                                                      && x.StartTime <= dateEvaluating.AddMinutes(i * 5).TimeOfDay
                                                                                                      && x.StopTime >= dateEvaluating.AddMinutes(i * 5).TimeOfDay).ToList();
                            if (listSchedulesForProviderAndOp.Count == 0)
                            {
                                isProvAvailable = false;
                            }
                        }
                    }

                    if (isProvAvailable)
                    {
                        //found a provider with an available time and operatory
                        break;
                    }
                }

                if (isProvAvailable)
                {
                    DateTime timeOpeningStart = dateEvaluating.AddMinutes(i * 5);
                    listScheduleOpenings.Add(new ScheduleOpening {DateTimeAvail = timeOpeningStart, ProvNum = provNumAvail, OpNum = apptSearchOperatorySchedule.OperatoryNum});
                }
            }
        }

        return listScheduleOpenings;
    }

    private static List<ApptSearchOperatorySchedule> GetAllForDate(DateTime scheduleDate, List<Schedule> listSchedules, List<Appointment> listAppointments, List<ScheduleOp> listSchedOps, List<long> listOpNums, List<long> listProvNums, List<long> listBlockoutTypes)
    {
        List<ApptSearchOperatorySchedule> listOpScheds = [];
        List<Operatory> listOps = Operatories.GetWhere(x => listOpNums.Contains(x.OperatoryNum));
        //Remove any ScheduleOps that are not related to the operatories passed in.
        listSchedOps.RemoveAll(x => !listOpNums.Contains(x.OperatoryNum));
        //Create dictionaries that are comprised of every operatory in question and will keep track of all ProviderNums for specific scenarios.
        Dictionary<long, List<long>> dictProvNumsInOpsBySched = listOps.ToDictionary(x => x.OperatoryNum, x => new List<long>());
        Dictionary<long, List<long>> dictProvNumsInOpsByOp = listOps.ToDictionary(x => x.OperatoryNum,
            x => new List<long>() {x.ProvDentist, x.ProvHygienist}); //Could be a list of two 0's if no providers are associated to this op.
        scheduleDate = scheduleDate.Date; //remove time component
        foreach (long opNum in listOpNums)
        {
            ApptSearchOperatorySchedule apptSearchOpSched = new ApptSearchOperatorySchedule();
            apptSearchOpSched.ProviderNums = [];
            apptSearchOpSched.OperatoryNum = opNum;
            apptSearchOpSched.IsOpAvailable = new bool[288];
            for (int j = 0; j < 288; j++)
            {
                apptSearchOpSched.IsOpAvailable[j] = true; //Set entire operatory schedule to true. True=available.
            }

            listOpScheds.Add(apptSearchOpSched);
        }

        #region Fill OpScheds with Providers allowed to work in each operatory

        //Make explicit entries into dictProvNumsInOpsBySched if there are any SchedOps for each schedule OR add an entry to every operatory if none found.
        foreach (Schedule schedule in listSchedules.FindAll(x => x.SchedDate == scheduleDate))
        {
            //use this loop to fill listProvsInOpBySched
            List<ScheduleOp> listSchedOpsForSchedule = listSchedOps.FindAll(x => x.ScheduleNum == schedule.ScheduleNum);
            if (listSchedOpsForSchedule.Count > 0)
            {
                AddProvNumToOps(dictProvNumsInOpsBySched,
                    listSchedOpsForSchedule.Select(x => x.OperatoryNum).Distinct().ToList(),
                    schedule.ProvNum);
            }
            else
            {
                //Provider scheduled to work, but not limited to specific operatory to add providerNum to all ops in opsProvPerSchedules
                AddProvNumToOps(dictProvNumsInOpsBySched,
                    dictProvNumsInOpsBySched.Keys.ToList(),
                    schedule.ProvNum);
            }
        }

        //Set each listOpScheds.ProviderNums to the corresponding providers via operatory OR schedules.
        SearchBehaviorCriteria searchBehaviorCriteria = (SearchBehaviorCriteria) PrefC.GetInt(PrefName.AppointmentSearchBehavior);
        foreach (Operatory op in listOps)
        {
            //If listBlockoutTypes has a non-zero entry, and 0 is only provNum in listProvNums, we are just looking for blockout schedules in ops.
            //Add zero to ProviderNums list for op if op has any blockout for the date we are searching. Unwanted blockouts are filtered out below.
            if (listBlockoutTypes.Exists(x => x > 0) && listProvNums.Max() == 0 && dictProvNumsInOpsBySched[op.OperatoryNum].Contains(0))
            {
                listOpScheds.First(x => x.OperatoryNum == op.OperatoryNum).ProviderNums.Add(0);
            }
            //If the operatory does not have a primary and secondary provider, use all providers from the schedules.
            else if (dictProvNumsInOpsByOp[op.OperatoryNum][0] == 0 && dictProvNumsInOpsByOp[op.OperatoryNum][1] == 0)
            {
                if (searchBehaviorCriteria == SearchBehaviorCriteria.ProviderTimeOperatory && listSchedOps.Count == 0)
                {
                    //We are using ProviderTimeOp search logic and the providers are only assigned to their ops directly, NOT in their schedules.
                    //In this case, an op with no primary or secondary provider means the op is unassigned, so we don't want to assign it.
                    //This effectively excludes unassigned ops from ProviderTimeOp search logic for this specific case.
                    continue;
                }

                listOpScheds.First(x => x.OperatoryNum == op.OperatoryNum).ProviderNums = dictProvNumsInOpsBySched[op.OperatoryNum];
            }
            else
            {
                //Otherwise; only add providers that intersect between schedules and being explicitly assigned to an operatory.
                List<long> listIntersectingProvNums = dictProvNumsInOpsBySched[op.OperatoryNum].Intersect(dictProvNumsInOpsByOp[op.OperatoryNum]).ToList();
                if (listIntersectingProvNums.Count() > 0)
                {
                    listOpScheds.First(x => x.OperatoryNum == op.OperatoryNum).ProviderNums.AddRange(listIntersectingProvNums);
                }
            }
        }

        #endregion

        #region Remove provider availability for current appointments

        List<Appointment> listAppointmentsForDate = listAppointments.FindAll(x => x.Op != 0 && x.AptDateTime.Date == scheduleDate);
        foreach (Appointment appt in listAppointmentsForDate)
        {
            //Remove unavailable slots from schedule
            ApptSearchOperatorySchedule apptSearchOperatorySchedule = listOpScheds.FirstOrDefault(x => x.OperatoryNum == appt.Op);
            if (apptSearchOperatorySchedule == null)
            {
                continue;
            }

            int apptStartIndex = (int) appt.AptDateTime.TimeOfDay.TotalMinutes / 5;
            for (int j = 0; j < appt.Pattern.Length; j++)
            {
                //make unavailable all blocks of time during this appointment
                apptSearchOperatorySchedule.IsOpAvailable[apptStartIndex + j] = false; //set time block to false, meaning something is scheduled here
            }
        }

        #endregion

        #region Remove provider availiabilty for blockouts set to Do Not Schedule

        List<long> listBlockoutsDoNotSchedule = [];
        List<Def> listBlockoutsAll = Defs.GetDefsForCategory(DefCat.BlockoutTypes, true);
        foreach (Def blockout in listBlockoutsAll)
        {
            if (blockout.ItemValue.Contains(BlockoutType.NoSchedule.GetDescription()))
            {
                listBlockoutsDoNotSchedule.Add(blockout.DefNum); //do not return results for blockouts set to 'Do Not Schedule'
                continue;
            }

            if (listBlockoutTypes.Exists(x => x > 0) && !listBlockoutTypes.Contains(blockout.DefNum))
            {
                listBlockoutsDoNotSchedule.Add(blockout.DefNum); //do not return results for blockout types that are not in listBlockoutTypes
            }
        }

        if (listBlockoutsDoNotSchedule.Count > 0)
        {
            List<Schedule> listBlockouts = listSchedules.FindAll(x => x.ProvNum == 0 && x.SchedType == ScheduleType.Blockout && x.SchedDate == scheduleDate
                                                                      && listBlockoutsDoNotSchedule.Contains(x.BlockoutType));
            foreach (Schedule blockout in listBlockouts)
            {
                //get length of blockout (how many 5 minute increments does it span)
                TimeSpan duration = blockout.StopTime.Subtract(blockout.StartTime);
                double fiveMinuteIncrements = Math.Ceiling(duration.TotalMinutes / 5);
                int blockoutStartIndex = (int) blockout.StartTime.TotalMinutes / 5;
                //Set each operatory as unavailable that has this blockout.
                List<ScheduleOp> listSchedOpsForBlockout = listSchedOps.FindAll(x => x.ScheduleNum == blockout.ScheduleNum);
                foreach (ScheduleOp schedOp in listSchedOpsForBlockout)
                {
                    ApptSearchOperatorySchedule apptSearchOperatorySchedule = listOpScheds.FirstOrDefault(x => x.OperatoryNum == schedOp.OperatoryNum);
                    if (apptSearchOperatorySchedule == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < fiveMinuteIncrements; i++)
                    {
                        apptSearchOperatorySchedule.IsOpAvailable[blockoutStartIndex + i] = false;
                    }
                }
            }
        }

        #endregion

        //Return all ApptSearchOperatorySchedules for the providers passed in.
        return listOpScheds.FindAll(x => x.ProviderNums.Any(y => listProvNums.Contains(y)));
    }

    private static void AddProvNumToOps(Dictionary<long, List<long>> dictProvNumsByOp, List<long> listOpNums, long provNum)
    {
        if (listOpNums == null || listOpNums.Count < 1)
        {
            return;
        }

        foreach (long opNum in listOpNums)
        {
            List<long> listProvNums;
            if (dictProvNumsByOp.TryGetValue(opNum, out listProvNums))
            {
                if (listProvNums == null)
                {
                    listProvNums = [];
                }

                if (listProvNums.Contains(provNum))
                {
                    continue;
                }

                listProvNums.Add(provNum);
            }
        }
    }
}

public class ApptSearchOperatorySchedule
{
    public long OperatoryNum;
    public bool[] IsOpAvailable;
    public List<long> ProviderNums;
}

public class ApptSearchData
{
    public DateTime DateEvaluating;
    public Appointment AppointmentToAdd;
    public List<Schedule> ListSchedules = [];
    public List<Appointment> ListAppointments = [];
    public List<ScheduleOp> ListSchedOps = [];
}

public class ScheduleOpening
{
    public DateTime DateTimeAvail = DateTime.MinValue;
    public long ProvNum;
    public const long ClinicNum = 0;
    public long OpNum;
}