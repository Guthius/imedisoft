using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class RepeatCharges
{
    ///<summary>Gets a list of all RepeatCharges for a given patient.  Supply 0 to get a list for all patients.</summary>
    public static RepeatCharge[] Refresh(long patNum)
    {
        var command = "SELECT * FROM repeatcharge";
        if (patNum != 0) command += " WHERE PatNum = " + SOut.Long(patNum);
        command += " ORDER BY DateStart";
        return RepeatChargeCrud.SelectMany(command).ToArray();
    }

    /// <summary>
    ///     Gets a list of all RepeatCharges based on super family patNum, procCode, chargeAmt and date.
    ///     patNumSuperFamily=0 to get a list of all matching repeat charge patients by ProcCode. Used for
    ///     FormRepeatChargeEditMulti.
    /// </summary>
    public static List<RepeatCharge> GetRepeatChargesMulti(long patNumSuperFamily, string procCode, double chargeAmt, DateTime dateGreaterThan)
    {
        var command = "SELECT rc.* FROM repeatcharge rc ";
        if (patNumSuperFamily == 0)
            command += "WHERE rc.ProcCode='" + SOut.String(procCode) + "' "
                       + "AND rc.ChargeAmt=" + SOut.Double(chargeAmt) + " "
                       + "AND rc.DateStart>=" + SOut.DateT(dateGreaterThan);
        else
            command += "INNER JOIN patient p ON p.PatNum=rc.PatNum "
                       + "WHERE p.SuperFamily=" + SOut.Long(patNumSuperFamily) + " "
                       + "AND rc.ProcCode='" + SOut.String(procCode) + "' "
                       + "AND rc.ChargeAmt=" + SOut.Double(chargeAmt) + " "
                       + "AND rc.DateStart>=" + SOut.DateT(dateGreaterThan);
        return RepeatChargeCrud.SelectMany(command);
    }

    /// <summary>
    ///     doCheckIsResellerCustomer is harmless in any case when !false, but should only be true for HQ when updating
    ///     directly from FormRepeatChargeEdit.
    /// </summary>
    public static void Update(RepeatCharge charge, bool doCheckIsResellerCustomer = false)
    {
        RepeatChargeCrud.Update(charge);
    }

    
    public static long Insert(RepeatCharge charge)
    {
        return RepeatChargeCrud.Insert(charge);
    }

    ///<summary>Called from FormRepeatCharge.</summary>
    public static void Delete(RepeatCharge charge)
    {
        var command = "DELETE FROM repeatcharge WHERE RepeatChargeNum =" + SOut.Long(charge.RepeatChargeNum);
        Db.NonQ(command);
    }

    public static void InsertRepeatChargeChangeSecurityLogEntry(RepeatCharge oldCharge, EnumPermType permType, Patient oldPat, RepeatCharge newCharge = null,
        bool isAutomated = false, LogSources source = LogSources.None, Patient newPat = null)
    {
        var hasChanges = false;
        var secLogText = "";
        //RepeatChargeNum
        var newVal = "";
        var oldVal = oldCharge.RepeatChargeNum.ToString();
        if (newCharge != null) newVal = newCharge.RepeatChargeNum.ToString();
        secLogText += GetRepeatChargeChangeString("RepeatChargeNum", oldVal, newVal, permType, ref hasChanges);
        //Frequency
        oldVal = oldCharge.Frequency.ToString();
        if (newCharge != null) newVal = newCharge.Frequency.ToString();
        secLogText += GetRepeatChargeChangeString("Frequency", oldVal, newVal, permType, ref hasChanges);
        //DateStart
        oldVal = oldCharge.DateStart.ToShortDateString();
        if (newCharge != null) newVal = newCharge.DateStart.ToShortDateString();
        secLogText += GetRepeatChargeChangeString("DateStart", oldVal, newVal, permType, ref hasChanges);
        //DateStop
        oldVal = oldCharge.DateStop.ToShortDateString();
        if (newCharge != null) newVal = newCharge.DateStop.ToShortDateString();
        secLogText += GetRepeatChargeChangeString("DateStop", oldVal, newVal, permType, ref hasChanges);
        //BillingCycleDay - This may be okay to take out if causing problems in the future. Check with Accounting. 
        if (PrefC.GetBool(PrefName.BillingUseBillingCycleDay) && oldPat != null)
        {
            oldVal = oldPat.BillingCycleDay.ToString();
            if (newPat != null && newCharge != null)
                newVal = newPat.BillingCycleDay.ToString();
            else
                newVal = oldPat.BillingCycleDay.ToString();
            secLogText += GetRepeatChargeChangeString("BillingCycleDay", oldVal, newVal, permType, ref hasChanges);
        }

        //Amount
        oldVal = oldCharge.ChargeAmt.ToString("c");
        if (newCharge != null) newVal = newCharge.ChargeAmt.ToString("c");
        secLogText += GetRepeatChargeChangeString("ChargeAmt", oldVal, newVal, permType, ref hasChanges);
        //Note
        oldVal = oldCharge.Note;
        if (newCharge != null) newVal = newCharge.Note;
        secLogText += GetRepeatChargeChangeString("Note", oldVal, newVal, permType, ref hasChanges, true);
        //IsEnabled
        oldVal = oldCharge.IsEnabled.ToString();
        if (newCharge != null) newVal = newCharge.IsEnabled.ToString();
        secLogText += GetRepeatChargeChangeString("IsEnabled", oldVal, newVal, permType, ref hasChanges);
        if (isAutomated) //If the change was created automatically by SignUpPortal, eRx, etc
            secLogText += "Changed automatically by system\r\n";
        if (hasChanges || permType == EnumPermType.RepeatChargeCreate || permType == EnumPermType.RepeatChargeDelete) SecurityLogs.MakeLogEntry(permType, oldCharge.PatNum, secLogText, source);
    }

    private static string GetRepeatChargeChangeString(string paramName, string oldVal, string newVal, EnumPermType permType, ref bool hasChanges, bool isNote = false)
    {
        var stringChange = paramName + " '" + oldVal + "'";
        if (permType == EnumPermType.RepeatChargeUpdate)
        {
            if (newVal != oldVal)
            {
                hasChanges = true;
                if (isNote)
                    stringChange = "Note Edited"; //Don't show the changes to notes, only display that the note was edited.
                else
                    stringChange += " changed to '" + newVal + "'";
            }
            else if (newVal == oldVal && paramName != "RepeatChargeNum")
            {
                //Always allow RepeatChargeNum string to be returned on an update.
                return "";
            }
        }

        stringChange += "\r\n";
        return stringChange;
    }

    ///<summary>For internal use only.  Returns all eRx repeating charges for all customers.</summary>
    public static List<RepeatCharge> GetForErx()
    {
        //Does not need to be Oracle compatible because this is an internal tool only.
        var command = "SELECT * FROM repeatcharge WHERE ProcCode REGEXP '^Z[0-9]{3,}$'";
        return RepeatChargeCrud.SelectMany(command);
    }

    ///<summary>Get the list of all RepeatCharge rows. DO NOT REMOVE! Used by OD WebApps solution.</summary>
    // ReSharper disable once UnusedMember.Global
    public static List<RepeatCharge> GetAll()
    {
        return Refresh(0).ToList();
    }

    /// <summary>
    ///     Gets all repeat charges for a family.  A family is currently defined as all accounts that share the same guarantor
    ///     or are
    ///     associated to the same super family.  This is used by the Reseller Portal to get all repeat charges linked to the
    ///     given reseller.
    ///     Optionally pass in a super family in order to broaden the family tree by also including accounts in the same super
    ///     family.
    /// </summary>
    public static List<RepeatCharge> GetByGuarantorOrSuperFamily(long guarantor, long superFamily = 0)
    {
        var command = "SELECT rc.* FROM repeatcharge rc "
                      + "INNER JOIN patient p ON p.PatNum=rc.PatNum "
                      + "WHERE p.Guarantor=" + SOut.Long(guarantor) + " ";
        if (superFamily > 0) command += "OR p.SuperFamily=" + SOut.Long(superFamily);
        return RepeatChargeCrud.SelectMany(command);
    }

    ///<summary>Returns true if there are any active repeating charges on the patient's account, false if there are not.</summary>
    public static bool ActiveRepeatChargeExists(long patNum)
    {
        //Counts the number of repeat charges that a patient has with a valid start date in the past and no stop date or a stop date in the future
        var command = "SELECT COUNT(*) FROM repeatcharge "
                      + "WHERE PatNum=" + SOut.Long(patNum) + " AND DateStart BETWEEN '1880-01-01' AND " + DbHelper.Curdate() + " "
                      + "AND (DateStop='0001-01-01' OR DateStop>=" + DbHelper.Curdate() + ")";
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    /// <summary>
    ///     Returns true if the dates passed in from the corresponding repeat charge are active as of DateTime.Today.
    ///     Mimics the logic within ActiveRepeatChargeExists() in the sense that dateStart is a valid date and is before or on
    ///     DateTime.Today
    ///     and that dateStop is either an invalid date (has yet to be set) OR is a valid date that is in the future.
    /// </summary>
    public static bool IsRepeatChargeActive(DateTime dateStart, DateTime dateStop)
    {
        if (dateStart.Year > 1880 && dateStart.Date <= DateTime.Today
                                  && (dateStop.Year < 1880 || dateStop.Date > DateTime.Today))
            return true;
        return false;
    }

    /// <summary>
    ///     Runs repeating charges for the date passed in, usually today. Can't use 'out' variables because this runs over
    ///     Middle Tier.
    ///     When doComputeAging=true, aging calculations will run for the families that had a repeating charge procedure added
    ///     to the account.
    /// </summary>
    public static RepeatChargeResult RunRepeatingCharges(DateTime dateRun, bool doComputeAging = true)
    {
        var result = new RepeatChargeResult();
        Prefs.UpdateDateT(PrefName.RepeatingChargesBeginDateTime, dateRun);
        try
        {
            var listRepeatingCharges = Refresh(0).ToList();
            if (listRepeatingCharges.Count == 0) return new RepeatChargeResult();
            //Filling a 'cache' of insplan information for all patnums to avoid queries later in a loop
            var listRepeatChargePatNums = listRepeatingCharges.Select(x => x.PatNum).Distinct().ToList(); //Get list of all repeating charge PatNums
            var listAllPatPlans = PatPlans.GetPatPlansForPats(listRepeatChargePatNums); //Get patplans with PatNum in our list of repeating charge PatNums.
            var listAllFamilyPatNums = Patients.GetAllFamilyPatNums(listRepeatChargePatNums); //Get every family member for every PatNum in our list of repeating charge PatNums
            var listFamilies = Patients.GetFamilies(listAllFamilyPatNums); //Organize the list of all family members into Families.
            var listAllInsSubs = InsSubs.GetListInsSubs(listAllFamilyPatNums); //Get all the inssubs for every repeating charge PatNum and their family members
            var listAllInsPlans = InsPlans.GetByInsSubs(listAllInsSubs.Select(x => x.InsSubNum).ToList()); //Lastly, get the insurance plans.
            var listAddedPatNums = new List<long>();
            //Must contain all procedures that affect the date range, safe to contain too many, bad to contain too few.
            var largestFrequency = listRepeatingCharges.Max(x => x.Frequency);
            var dateTimeRun = DateTime.Now;
            if (largestFrequency == EnumRepeatChargeFrequency.Annually)
                dateTimeRun = dateRun.AddYears(-1);
            else if (largestFrequency == EnumRepeatChargeFrequency.Quarterly)
                dateTimeRun = dateRun.AddMonths(-5);
            else
                dateTimeRun = dateRun.AddMonths(-3);
            var listExistingProcs = Procedures.GetCompletedForDateRange(dateTimeRun, dateRun.AddDays(1), listRepeatingCharges.Select(x => x.ProcCode).Distinct().Select(x => ProcedureCodes.GetProcCode(x).CodeNum).ToList(), listRepeatChargePatNums);
            var startedUsingFKs = UpdateHistories.GetDateForVersion(new Version("16.1.0.0")); //We started using FKs from procs to repeat charges in 16.1.
            var didEncounterAvaTaxError = false;
            var listInvalidProcCodes = new List<string>(); //Used to contain all invalid procs that cannot be added to repeating charges.
            var listOrthoCaseProcedureLinkers = OrthoCaseProcedureLinker.CreateManyForPatients(listRepeatChargePatNums);
            foreach (var repeatCharge in listRepeatingCharges)
            {
                if (!repeatCharge.IsEnabled || (repeatCharge.DateStop.Year > 1880 && repeatCharge.DateStop.AddMonths(3) < dateRun)) continue; //This repeating charge is too old to possibly create a new charge. Not precise but greatly reduces calls to DB.
                //We will filter by more stringently on the DateStop later on.
                Patient pat = null;
                List<DateTime> listBillingDates; //This list will have 1 or 2 dates where a repeating charge might be added
                if (PrefC.GetBool(PrefName.BillingUseBillingCycleDay))
                {
                    var family = listFamilies.First(x => x.IsInFamily(repeatCharge.PatNum));
                    pat = family.GetPatient(repeatCharge.PatNum);
                    listBillingDates = GetBillingDatesHelper(repeatCharge, dateRun, pat.BillingCycleDay);
                }
                else
                {
                    listBillingDates = GetBillingDatesHelper(repeatCharge, dateRun);
                }

                var codeNum = ProcedureCodes.GetCodeNum(repeatCharge.ProcCode);
                //Remove billing dates if there is a procedure from this repeat charge in that month and year
                for (var i = listBillingDates.Count - 1; i >= 0; i--)
                {
                    //iterate backwards to remove elements
                    var billingDate = listBillingDates[i];
                    for (var j = listExistingProcs.Count - 1; j >= 0; j--)
                    {
                        //iterate backwards to remove elements
                        var proc = listExistingProcs[j];
                        if ((proc.RepeatChargeNum == repeatCharge.RepeatChargeNum //Check the procedure's FK first
                             && IsRepeatDateHelper(repeatCharge, billingDate, proc.ProcDate, pat))
                            //Use the old logic without matching FKs only if the procedure was added before updating to 16.1
                            //Match patnum, codenum, fee, year, and month (IsRepeatDateHelper uses special logic to determine correct month)
                            || ((proc.ProcDate < startedUsingFKs || startedUsingFKs.Year < 1880)
                                && proc.PatNum == repeatCharge.PatNum
                                && proc.CodeNum == codeNum
                                && IsRepeatDateHelper(repeatCharge, billingDate, proc.ProcDate, pat)
                                && CompareDouble.IsEqual(proc.ProcFee, repeatCharge.ChargeAmt)))
                        {
                            //This is a match to an existing procedure.
                            listBillingDates.RemoveAt(i); //Removing so that a procedure will not get added on this date.
                            listExistingProcs.RemoveAt(j); //Removing so that another repeat charge of the same code, date, and amount will be added.
                            break; //Go to the next billing date
                        }
                    }
                }

                var orthoCaseProcedureLinker = listOrthoCaseProcedureLinkers.First(x => x.PatNum == repeatCharge.PatNum);
                //If any billing dates have not been filtered out, add a repeating charge on those dates
                foreach (var billingDate in listBillingDates)
                {
                    Procedure procAdded = null;
                    OrthoProcLink orthoProcLink = null;
                    try
                    {
                        procAdded = AddProcForRepeatCharge(repeatCharge, billingDate, dateRun, orthoCaseProcedureLinker);
                    }
                    catch (ODException ex)
                    {
                        if (!listInvalidProcCodes.Contains(repeatCharge.ProcCode)) listInvalidProcCodes.Add(repeatCharge.ProcCode);
                        continue;
                    }

                    orthoProcLink = orthoCaseProcedureLinker.LinkProcedureToActiveOrthoCaseIfNeeded(procAdded);
                    if (procAdded.ProcNum == 0)
                    {
                        //error we actually don't want to add this procedure
                        didEncounterAvaTaxError = true;
                        continue;
                    }

                    var listClaimsAdded = new List<Claim>();
                    InsPlan insPlan = null; //Used to determine which insurance plan to check for NoBillIns override.
                    var listPlansForPat = listAllPatPlans.FindAll(x => x.PatNum == repeatCharge.PatNum);
                    if (listPlansForPat.Count == 1 && PatPlans.GetOrdinal(PriSecMed.Medical, listPlansForPat, listAllInsPlans, listAllInsSubs) > 0)
                    {
                        //if there's exactly one medical plan
                        var insSub = InsSubs.GetSub(PatPlans.GetInsSubNum(listPlansForPat, PatPlans.GetOrdinal(PriSecMed.Medical, listPlansForPat, listAllInsPlans, listAllInsSubs)), listAllInsSubs);
                        insPlan = InsPlans.GetPlan(insSub.PlanNum, listAllInsPlans);
                    }
                    else
                    {
                        //Otherwise use the primary insurance.
                        var insSub = InsSubs.GetSub(PatPlans.GetInsSubNum(listPlansForPat, PatPlans.GetOrdinal(PriSecMed.Primary, listPlansForPat, listAllInsPlans, listAllInsSubs)), listAllInsSubs);
                        insPlan = InsPlans.GetPlan(insSub.PlanNum, listAllInsPlans);
                    }

                    if (repeatCharge.CreatesClaim && !InsPlanPreferences.NoBillIns(ProcedureCodes.GetProcCode(repeatCharge.ProcCode), insPlan))
                    {
                        //Filter our lists of that contain information about all repeating charge PatNums down to lists containing only 
                        //information about the current repeatCharge.PatNum.
                        var family = listFamilies.First(x => x.IsInFamily(repeatCharge.PatNum));
                        var listFamilyPatNums = family.GetPatNums();
                        var listInsSubs = listAllInsSubs.FindAll(x => listFamilyPatNums.Contains(x.Subscriber));
                        var listInsPlans = listAllInsPlans.FindAll(x => listInsSubs.Select(x => x.PlanNum).Contains(x.PlanNum));
                        listClaimsAdded = AddClaimsHelper(repeatCharge, procAdded, orthoCaseProcedureLinker, orthoProcLink, listPlansForPat, listInsSubs, listInsPlans);
                    }

                    AllocateUnearned(repeatCharge, procAdded, billingDate);
                    result.ProceduresAddedCount++;
                    result.ClaimsAddedCount += listClaimsAdded.Count;
                    if (!listAddedPatNums.Contains(procAdded.PatNum)) listAddedPatNums.Add(procAdded.PatNum);
                }
            }

            if (listInvalidProcCodes.Count > 0) result.ErrorMsg.AppendLine(Lans.g("RepeatCharges", "One or more procedures were skipped due to the procedures being in a hidden category:") + $" {string.Join(", ", listInvalidProcCodes)}");
            if (doComputeAging && listAddedPatNums.Count > 0)
            {
                var listGuarantors = Patients.GetGuarantorsForPatNums(listAddedPatNums);
                var dtNow = MiscData.GetNowDateTime();
                //will only use the famaging table if more than 1 guarantor
                if (listGuarantors.Count > 1)
                {
                    //if this will utilize the famaging table we need to check and set the pref to block others from starting aging
                    Prefs.RefreshCache();
                    if (!PrefC.IsAgingAllowedToStart())
                    {
                        //pref has been set by another process, don't run aging and notify user
                        result.ErrorMsg.AppendLine(Lans.g("RepeatCharges", "Aging failed to run for patients who had repeat charges added to their account. This is due to "
                                                                           + "the currently running aging calculations which began on") + " " + PrefC.GetDateT(PrefName.AgingBeginDateTime) + ".  " + Lans.g("RepeatCharges", "If you "
                                                                                                                                                                                                                              + "believe the current aging process has finished, a user with SecurityAdmin permission can manually clear the date and time by going "
                                                                                                                                                                                                                              + "to Setup | Preferences | Account - General and pressing the 'Clear' button.  You will need to run aging manually once the current aging process has "
                                                                                                                                                                                                                              + "finished or date and time is cleared."));
                    }
                    else
                    {
                        Prefs.UpdateString(PrefName.AgingBeginDateTime, SOut.DateT(dtNow, false)); //get lock on pref to block others
                        Signalods.SetInvalid(InvalidType.Prefs); //signal a cache refresh so other computers will have the updated pref as quickly as possible
                        try
                        {
                            Ledgers.ComputeAging(listGuarantors, dateRun);
                        }
                        finally
                        {
                            Prefs.UpdateString(PrefName.AgingBeginDateTime, ""); //clear lock on pref whether aging was successful or not
                            Signalods.SetInvalid(InvalidType.Prefs);
                        }
                    }
                }
                else
                {
                    //only 1 guar so not using the famaging table, just run aging as usual
                    Ledgers.ComputeAging(listGuarantors, dateRun);
                }
            }
        }
        catch (Exception ex)
        {
            result.ErrorMsg.AppendLine(MiscUtils.GetExceptionText(ex));
        }
        finally
        {
            Prefs.UpdateString(PrefName.RepeatingChargesBeginDateTime, "");
            //Even if failure, we want to update so OpenDentalService doesn't launch Repeating Charges again today.
            Prefs.UpdateDateT(PrefName.RepeatingChargesLastDateTime, dateRun);
        }

        return result;
    }

    /// <summary>
    ///     Do not call this until after determining if the repeate charge might generate a claim.  This function checks
    ///     current insurance and
    ///     may not add claims if no insurance is found.
    /// </summary>
    private static List<Claim> AddClaimsHelper(RepeatCharge repeateCharge, Procedure proc, OrthoCaseProcedureLinker orthoCaseProcedureLinker, OrthoProcLink orthoProcLink, List<PatPlan> patPlanList, List<InsSub> subList,
        List<InsPlan> insPlanList)
    {
        var benefitList = Benefits.Refresh(patPlanList, subList);
        var retVal = new List<Claim>();
        Claim claimCur;
        var pat = Patients.GetPat(proc.PatNum);
        if (patPlanList.Count == 0) //no current insurance, do not create a claim
            return retVal;
        //create the claimprocs
        Procedures.ComputeEstimates(proc, proc.PatNum, new List<ClaimProc>(), true, insPlanList, patPlanList, benefitList, pat.Age, subList
            , orthoProcLink, orthoCaseProcedureLinker.ActiveOrthoCase, orthoCaseProcedureLinker.OrthoSchedule
            , orthoCaseProcedureLinker.ListOrthoProcLinks);
        //get claimprocs for this proc, may be more than one
        var claimProcList = ClaimProcs.GetForProc(ClaimProcs.Refresh(proc.PatNum), proc.ProcNum);
        var claimType = "P";
        if (patPlanList.Count == 1 && PatPlans.GetOrdinal(PriSecMed.Medical, patPlanList, insPlanList, subList) > 0) //if there's exactly one medical plan
            claimType = "Med";
        claimCur = Claims.CreateClaimForRepeatCharge(claimType, patPlanList, insPlanList, claimProcList, proc, subList, pat);
        claimProcList = ClaimProcs.Refresh(proc.PatNum);
        if (claimCur.ClaimNum == 0) return retVal;
        retVal.Add(claimCur);
        Claims.CalculateAndUpdate(new List<Procedure> {proc}, insPlanList, claimCur, patPlanList, benefitList, pat, subList);
        if (PatPlans.GetOrdinal(PriSecMed.Secondary, patPlanList, insPlanList, subList) > 0 //if there exists a secondary plan
            && !CultureInfo.CurrentCulture.Name.EndsWith("CA")) //and not canada (don't create secondary claim for canada)
        {
            claimCur = Claims.CreateClaimForRepeatCharge("S", patPlanList, insPlanList, claimProcList, proc, subList, pat);
            if (claimCur.ClaimNum == 0) return retVal;
            retVal.Add(claimCur);
            ClaimProcs.Refresh(proc.PatNum);
            claimCur.ClaimStatus = "H";
            Claims.CalculateAndUpdate(new List<Procedure> {proc}, insPlanList, claimCur, patPlanList, benefitList, pat, subList);
        }

        return retVal;
    }

    ///<summary>Returns 1 or 2 dates to be billed given the date range. Only filtering based on date range has been performed.</summary>
    public static List<DateTime> GetBillingDatesHelper(RepeatCharge repeatCharge, DateTime dateRun, int billingCycleDay = 0)
    {
        var retVal = new List<DateTime>();
        var monthIncrement = 1; //monthly
        if (repeatCharge.Frequency == EnumRepeatChargeFrequency.Quarterly)
            monthIncrement = 4; //quarterly
        else if (repeatCharge.Frequency == EnumRepeatChargeFrequency.Annually) monthIncrement = 12; //annually
        //Create a list of dates for the repeating charge passed in where every date starts on the 1st of the month.
        var dateStart = new DateTime(repeatCharge.DateStart.Year, repeatCharge.DateStart.Month, 1);
        //Stop adding dates to the list once we have reached the 1st of next month.
        //This will allow for the current month to be considered which is necessary when the BillingUseBillingCycleDay preference is on.
        var dateStop = new DateTime(dateRun.Year, dateRun.Month, 1).AddMonths(1);
        while (dateStart < dateStop)
        {
            retVal.Add(dateStart);
            dateStart = dateStart.AddMonths(monthIncrement);
        }

        //Take the last 3 dates from the list for backdating purposes.
        retVal = UIHelper.TakeLast(retVal, 3).ToList();
        if (retVal.IsNullOrEmpty()) return retVal;
        if (!PrefC.GetBool(PrefName.BillingUseBillingCycleDay)) billingCycleDay = repeatCharge.DateStart.Day;
        //This loop fixes the day of the month while taking a billing cycle day that falls past the end of the month.
        //This is necessary for repeating charges that have a DateStart (or a billing cycle day) after the 28th.
        for (var i = 0; i < retVal.Count; i++)
        {
            var daysInMonth = DateTime.DaysInMonth(retVal[i].Year, retVal[i].Month);
            var billingDay = Math.Min(daysInMonth, billingCycleDay);
            retVal[i] = new DateTime(retVal[i].Year, retVal[i].Month, billingDay); //This re-adds the billing date with the proper day of month.
        }

        //Remove billing dates that are calulated before repeat charge started.
        retVal.RemoveAll(x => x < repeatCharge.DateStart);
        //Remove billing dates older than one month and 20 days ago.
        retVal.RemoveAll(x => x < dateRun.AddMonths(-1).AddDays(-20));
        //Remove any dates after today
        retVal.RemoveAll(x => x > dateRun);
        //Remove billing dates past the end of the dateStop
        var monthAdd = 0;
        //To account for a partial month, add a charge after the repeat charge stop date in certain circumstances (for each of these scenarios, the 
        //billingCycleDay will be 11):
        //--Scenario #1: The start day is before the stop day which is before the billing day. Ex: Start: 12/08, Stop 12/09
        //--Scenario #2: The start day is after the billing day which is after the stop day. Ex: Start: 11/25 Stop 12/01
        //--Scenario #3: The start day is before the stop day but before the billing day. Ex: Start: 11/25, Stop 11/27
        //--Scenario #4: The start day is the same as the stop day but after the billing day. Ex: Start: 10/13, Stop 11/13
        //--Scenario #5: The start day is the same as the stop day but before the billing day. Ex: Start: 11/10, Stop 12/10
        //Each of these repeat charges will post a charge on 12/11 even though it is after the stop date.
        if (repeatCharge.Frequency == EnumRepeatChargeFrequency.Monthly)
        {
            if (PrefC.GetBool(PrefName.BillingUseBillingCycleDay))
            {
                if (repeatCharge.DateStart.Day < billingCycleDay)
                {
                    if ((repeatCharge.DateStop.Day < billingCycleDay && repeatCharge.DateStart.Day < repeatCharge.DateStop.Day) //Scenario #1
                        || repeatCharge.DateStart.Day == repeatCharge.DateStop.Day) //Scenario #5
                        monthAdd = 1;
                }
                else if (repeatCharge.DateStart.Day > billingCycleDay)
                {
                    if (repeatCharge.DateStart.Day <= repeatCharge.DateStop.Day //Scenario #3 and #4
                        || repeatCharge.DateStop.Day < billingCycleDay) //Scenario #2
                        monthAdd = 1;
                }
            }

            if (repeatCharge.DateStop.Year > 1880) retVal.RemoveAll(x => x > repeatCharge.DateStop.AddMonths(monthAdd));
        }

        retVal.Sort(); //Order by oldest first
        return retVal;
    }

    /// <summary>
    ///     Will throw exception if the repeatCharge.ProcCode is in a hidden category and not a Z-code.I nserts a procedure for
    ///     the repeat charge.
    ///     Set isNewCropInitial to true when adding repeat charge for the first time from FormNewCrop. Possibly will allocate
    ///     prepayments to the procedure.
    /// </summary>
    public static Procedure AddProcForRepeatCharge(RepeatCharge repeatCharge, DateTime billingDate, DateTime dateNow
        , OrthoCaseProcedureLinker orthoCaseProcedureLinker, bool isNewCropInitial = false, bool isNewCropFutureDated = false)
    {
        var procedure = new Procedure();
        var procCode = ProcedureCodes.GetProcCode(repeatCharge.ProcCode);
        var pat = Patients.GetPat(repeatCharge.PatNum);
        if (ProcedureCodes.AreAnyProcCodesHidden(procCode.CodeNum) && !procCode.ProcCode.ToLower().StartsWith("z")) //Procedure is in a hidden category and not a Z-code.
            throw new ODException($"Cannot add procedure for the current repeat charge because procedure is in a hidden category: {procCode.ProcCode}");
        procedure.CodeNum = procCode.CodeNum;
        procedure.ClinicNum = pat.ClinicNum;
        procedure.DateEntryC = dateNow;
        procedure.PatNum = repeatCharge.PatNum;
        procedure.ProcDate = billingDate;
        procedure.DateTP = billingDate;
        procedure.ProcFee = repeatCharge.ChargeAmt;
        procedure.ProcStatus = ProcStat.C;
        if (procCode.ProvNumDefault == 0)
            procedure.ProvNum = pat.PriProv;
        else
            procedure.ProvNum = procCode.ProvNumDefault;
        procedure.MedicalCode = ProcedureCodes.GetProcCode(procedure.CodeNum).MedicalCode;
        procedure.BaseUnits = ProcedureCodes.GetProcCode(procedure.CodeNum).BaseUnits;
        Procedures.SetDiagnosticCodesToDefault(procedure, procCode);
        if (isNewCropInitial || isNewCropFutureDated)
            procedure.RepeatChargeNum = 0;
        else
            procedure.RepeatChargeNum = repeatCharge.RepeatChargeNum;
        procedure.PlaceService = Clinics.GetPlaceService(procedure.ClinicNum);
        //Check if the repeating charge has been flagged to copy it's note into the billing note of the procedure.
        if (repeatCharge.CopyNoteToProc)
        {
            procedure.BillingNote = repeatCharge.Note;
            if (repeatCharge.ErxAccountId != "")
            {
                procedure.BillingNote =
                    "NPI=" + repeatCharge.Npi + "  " + "ErxAccountId=" + repeatCharge.ErxAccountId;
                if (!string.IsNullOrEmpty(repeatCharge.ProviderName)) //Provider name would be empty if older and no longer updated from eRx.
                    procedure.BillingNote += "\r\nProviderName=" + repeatCharge.ProviderName;
                if (isNewCropInitial) procedure.BillingNote += "\r\nNew provider charge, previous month.";
                if (isNewCropFutureDated)
                    procedure.BillingNote += "\r\nNew provider charge, current month.";
                else if (!string.IsNullOrEmpty(repeatCharge.Note)) procedure.BillingNote += "\r\n" + repeatCharge.Note;
            }
        }

        if (!PrefC.GetBool(PrefName.EasyHidePublicHealth)) procedure.SiteNum = pat.SiteNum;
        try
        {
            //No recall synch needed because dental offices don't use this feature
            Procedures.Insert(procedure, isRepeatCharge: true, skipDiscountPlanAdjustment: orthoCaseProcedureLinker.ShouldProcedureLinkToOrthoCase(procedure, repeatCharge.ProcCode));
        }
        catch (Exception e)
        {
        }

        return procedure;
    }

    /// <summary>
    ///     If there are unearned paysplits and the repeat charge is set to allocate unearned, creates a payments and allocates
    ///     unearned
    ///     paysplits to the new payment.
    /// </summary>
    public static void AllocateUnearned(RepeatCharge repeatCharge, Procedure procedure, DateTime billingDate)
    {
        if (!repeatCharge.UsePrepay) return;
        var fam = Patients.GetFamily(repeatCharge.PatNum);
        var listUnearnedSplits = PaySplits.GetUnearnedForPats(fam.GetPatNums());
        if (!string.IsNullOrEmpty(repeatCharge.UnearnedTypes))
        {
            //This repeat charge is limited to certain unearned types. If repeatCharge.UnearnedTypes is empty, it is for all unearned types.
            var listDefNumsUnearnedTypeCur = repeatCharge.UnearnedTypes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => SIn.Long(x, false)).ToList();
            listUnearnedSplits.RemoveAll(x => !listDefNumsUnearnedTypeCur.Contains(x.UnearnedType));
        }

        if (listUnearnedSplits.Count == 0)
            //The family does not have any unallocated unearned pay splits that match the repeat charge's unearned types.
            return;
        var amountUnearned = listUnearnedSplits.Sum(x => x.SplitAmt);
        if (CompareDouble.IsLessThanOrEqualToZero(amountUnearned)) return;
        var listAccountEntries = PaymentEdit.CreateAccountEntries(new List<Procedure> {procedure});
        amountUnearned = Math.Min((double) listAccountEntries.Sum(x => x.AmountEnd), amountUnearned);
        var listPaySplits = PaymentEdit.AllocateUnearned(0, amountUnearned, listAccountEntries, fam, true);
        if (listPaySplits == null || listPaySplits.Count == 0) return;
        var payCur = new Payment();
        payCur.ClinicNum = procedure.ClinicNum;
        payCur.DateEntry = billingDate;
        payCur.IsSplit = true;
        payCur.PatNum = repeatCharge.PatNum;
        payCur.PayDate = billingDate;
        payCur.PayType = 0; //Income transfer (will always be income transfer)
        payCur.PayAmt = 0; //Income transfer payment
        var noteText = "";
        for (var i = 0; i < listPaySplits.Count; i++)
        {
            listPaySplits[i].DateEntry = billingDate;
            listPaySplits[i].DatePay = billingDate;
            if (listPaySplits[i].SplitAmt > 0)
            {
                if (noteText != "") noteText += ", ";
                noteText += listPaySplits[i].SplitAmt.ToString("c");
            }
        }

        payCur.PayNote = "Allocated " + noteText + " prepayments to repeating charge.";
        Payments.Insert(payCur, listPaySplits);
    }

    ///<summary>Returns true if the existing procedure was for the possibleBillingDate.</summary>
    private static bool IsRepeatDateHelper(RepeatCharge repeatCharge, DateTime possibleBillingDate, DateTime existingProcedureDate, Patient pat)
    {
        if (PrefC.GetBool(PrefName.BillingUseBillingCycleDay))
        {
            pat = pat ?? Patients.GetPat(repeatCharge.PatNum);
            if (pat.BillingCycleDay != existingProcedureDate.Day
                && possibleBillingDate.AddMonths(-1).Month == existingProcedureDate.Month
                && possibleBillingDate.AddMonths(-1).Year == existingProcedureDate.Year
                && existingProcedureDate >= repeatCharge.DateStart)
            {
                var dateDiff = new DateSpan(possibleBillingDate, existingProcedureDate);
                if (dateDiff.MonthsDiff > 0)
                    //return false if the last charge has been longer than a month. 
                    return false;
                //This is needed in case the patient's billing day changed after procedures had been added for a repeat charge.
                return true;
            }

            //Only match month and year to be equal
            return possibleBillingDate.Month == existingProcedureDate.Month && possibleBillingDate.Year == existingProcedureDate.Year;
        }

        if (possibleBillingDate.Month != existingProcedureDate.Month || possibleBillingDate.Year != existingProcedureDate.Year) return false;
        //Iterate through dates using new logic that takes repeatCharge.DateStart.AddMonths(n) to calculate dates
        var possibleDateNew = repeatCharge.DateStart;
        var dateNewMonths = 0;
        //Iterate through dates using old logic that starts with repeatCharge.DateStart and adds one month at a time to calculate dates
        var possibleDateOld = repeatCharge.DateStart;
        do
        {
            if (existingProcedureDate == possibleDateNew || existingProcedureDate == possibleDateOld) return true;
            dateNewMonths++;
            possibleDateNew = repeatCharge.DateStart.AddMonths(dateNewMonths);
            possibleDateOld = possibleDateOld.AddMonths(1);
        } while (possibleDateNew <= existingProcedureDate);

        return false;
    }

    public static List<RepeatCharge> FilterRepeatingCharges(List<RepeatCharge> masterList, string procCode)
    {
        return masterList.FindAll(x => x.ProcCode == procCode && (x.DateStop == DateTime.MinValue || x.DateStop > DateTime.Now) && x.DateStart <= DateTime.Now).ToList();
    }
}

public class RepeatChargeResult
{
    public int ClaimsAddedCount;

    /// <summary>
    ///     Used to return an error message, e.g. enterprise aging blocked due to currently running calculations, so this
    ///     message tells the user
    ///     to run aging afterward.
    /// </summary>
    public StringBuilder ErrorMsg = new();

    public int ProceduresAddedCount;
}