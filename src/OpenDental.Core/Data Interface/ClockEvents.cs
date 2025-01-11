using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ClockEvents
{
    
    public static List<ClockEvent> Refresh(long employeeNum, DateTime dateTimeFrom, DateTime dateTimeTo, bool isBreaks)
    {
        var command =
            "SELECT * FROM clockevent WHERE"
            + " EmployeeNum = '" + SOut.Long(employeeNum) + "'"
            + " AND TimeDisplayed1 >= " + SOut.Date(dateTimeFrom)
            //adding a day takes it to midnight of the specified toDate
            + " AND TimeDisplayed1 < " + SOut.Date(dateTimeTo.AddDays(1));
        if (isBreaks)
            command += " AND ClockStatus = '2'";
        else
            command += " AND (ClockStatus = '0' OR ClockStatus = '1')";

        command += " ORDER BY TimeDisplayed1";
        return ClockEventCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns clock events for the employee and date range passed in. Validates the events and throws an exception if
    ///     there are any errors.
    ///     Set isBreaks true to explicitly validate and return break events instead of clock in and out events.
    /// </summary>
    public static List<ClockEvent> GetValidList(long employeeNum, DateTime dateTimeFrom, DateTime dateTimeTo, bool isBreaks)
    {
        var listClockEvents = new List<ClockEvent>();
        //Fill list-----------------------------------------------------------------------------------------------------------------------------
        var command =
            "SELECT * FROM clockevent WHERE"
            + " EmployeeNum = " + SOut.Long(employeeNum)
            + " AND TimeDisplayed1 >= " + SOut.Date(dateTimeFrom)
            //adding a day takes it to midnight of the specified toDate
            + " AND TimeDisplayed1 < " + SOut.Date(dateTimeTo.AddDays(1));
        if (isBreaks)
            command += " AND ClockStatus = 2";
        else
            command += " AND (ClockStatus = 0 OR ClockStatus = 1)";

        command += " ORDER BY TimeDisplayed1";
        listClockEvents = ClockEventCrud.SelectMany(command);
        var stringBuilderErrors = new StringBuilder();
        //Validate Pay Period------------------------------------------------------------------------------------------------------------------
        for (var i = 0; i < listClockEvents.Count; i++)
            if (listClockEvents[i].TimeDisplayed2.Year < 1880)
            {
                stringBuilderErrors.Append("  " + listClockEvents[i].TimeDisplayed1.ToShortDateString() + " : ");
                if (isBreaks) stringBuilderErrors.AppendLine(Lans.g("ClockEvents", "the employee did not clock in from break."));

                stringBuilderErrors.AppendLine(Lans.g("ClockEvents", "the employee did not clock out."));
            }
            else if (listClockEvents[i].TimeDisplayed1.Date != listClockEvents[i].TimeDisplayed2.Date)
            {
                stringBuilderErrors.Append("  " + listClockEvents[i].TimeDisplayed1.ToShortDateString() + " : ");
                if (isBreaks) stringBuilderErrors.AppendLine(Lans.g("ClockEvents", "break spans multiple days."));

                stringBuilderErrors.AppendLine(Lans.g("ClockEvents", "entry spans multiple days."));
            }

        if (stringBuilderErrors.Length == 0) return listClockEvents;

        var message = Lans.g("ClockEvents", "Clock event errors :");
        if (isBreaks) message = Lans.g("ClockEvents", "Break event errors :");

        throw new Exception(message + "\r\n" + stringBuilderErrors);
    }

    /// <summary>
    ///     Returns all clock events (Breaks and Non-Breaks) for all employees across all clinics. Currently only used
    ///     internally for
    ///     payroll benefits report.
    /// </summary>
    public static List<ClockEvent> GetAllForPeriod(DateTime dateTimeFrom, DateTime dateTimeTo)
    {
        var command = "SELECT * FROM clockevent WHERE TimeDisplayed1 >= " + SOut.Date(dateTimeFrom) + " AND TimeDisplayed1 < " + SOut.Date(dateTimeTo.AddDays(1));
        return ClockEventCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of clock events (not breaks) within the date range for the employees.
    ///     No option for breaks because this is just used in summing for time card report; use GetTimeCardRule instead.
    /// </summary>
    public static List<ClockEvent> GetListForTimeCardManage(List<long> listEmployeeNums, long clinicNum, DateTime dateTimeFrom, DateTime dateTimeTo, bool isAll)
    {
        if (listEmployeeNums.IsNullOrEmpty()) return new List<ClockEvent>();

        var command = "SELECT * FROM clockevent WHERE"
                      + " EmployeeNum IN (" + string.Join(",", listEmployeeNums.Select(x => SOut.Long(x))) + ")"
                      + " AND TimeDisplayed1 >= " + SOut.Date(dateTimeFrom)
                      + " AND TimeDisplayed1 < " + SOut.Date(dateTimeTo.AddDays(1)); //adding a day takes it to midnight of the specified toDate
        if (!isAll) command += " AND ClinicNum = " + SOut.Long(clinicNum);

        command += " AND (ClockStatus = 0 OR ClockStatus = 1)"
                   + " ORDER BY TimeDisplayed1";
        return ClockEventCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns an error message containing a description of what is wrong with the clock events passed in if any problems
    ///     are detected.
    ///     Otherwise; returns an empty string if no problems are detected.
    /// </summary>
    public static string ValidateEvents(List<ClockEvent> listClockEvents)
    {
        if (listClockEvents.IsNullOrEmpty()) return "";

        var errorNoClockOut = Lans.g("ClockEvents", "the employee did not clock out.");
        var errorSpansMultipleDays = Lans.g("ClockEvents", "entry spans multiple days.");
        var stringBuilderErrors = new StringBuilder();
        for (var i = 0; i < listClockEvents.Count; i++)
            if (listClockEvents[i].TimeDisplayed2.Year < 1880)
                stringBuilderErrors.AppendLine("  " + listClockEvents[i].TimeDisplayed1.ToShortDateString() + " : " + errorNoClockOut);
            else if (listClockEvents[i].TimeDisplayed1.Date != listClockEvents[i].TimeDisplayed2.Date) stringBuilderErrors.AppendLine("  " + listClockEvents[i].TimeDisplayed1.ToShortDateString() + " : " + errorSpansMultipleDays);

        if (!string.IsNullOrEmpty(stringBuilderErrors.ToString())) return Lans.g("ClockEvents", "Clock event errors") + " :\r\n" + stringBuilderErrors;

        return "";
    }

    ///<summary>Gets one ClockEvent from the db.</summary>
    public static ClockEvent GetOne(long clockEventNum)
    {
        return ClockEventCrud.SelectOne(clockEventNum);
    }

    
    public static long Insert(ClockEvent clockEvent)
    {
        long clockEventNum = 0;
        clockEventNum = ClockEventCrud.Insert(clockEvent);
        if (PrefC.GetBool(PrefName.LocalTimeOverridesServerTime))
        {
            //Cannot call update since we manually have to update the TimeEntered1 because it is a DateEntry column
            var command = "UPDATE clockevent SET TimeEntered1=" + SOut.DateT(DateTime.Now) + ", TimeDisplayed1=" + SOut.DateT(DateTime.Now) + " WHERE clockEventNum=" + SOut.Long(clockEventNum);
            Db.NonQ(command);
        }

        return clockEventNum;
    }

    
    public static void Update(ClockEvent clockEvent)
    {
        ClockEventCrud.Update(clockEvent);
    }

    
    public static void Delete(long clockEventNum)
    {
        var command = "DELETE FROM clockevent WHERE ClockEventNum = " + SOut.Long(clockEventNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Gets directly from the database.  If the last event is a completed break, then it instead grabs the half-finished
    ///     clock in.
    ///     Other possibilities include half-finished clock in which truly was the last event, a finished clock in/out,
    ///     a half-finished clock out for break, or null for a new employee.
    ///     Returns null if employeeNum of 0 passed in or no clockevent was found for the corresponding employee.
    /// </summary>
    public static ClockEvent GetLastEvent(long employeeNum)
    {
        if (employeeNum == 0)
            //Every clockevent should be associated to an employee.  Do not waste time looking through the entire table.
            return null;

        var command = "SELECT * FROM clockevent WHERE EmployeeNum=" + SOut.Long(employeeNum)
                                                                    + " ORDER BY TimeDisplayed1 DESC";
        command = DbHelper.LimitOrderBy(command, 1);
        var clockEvent = ClockEventCrud.SelectOne(command);
        if (clockEvent == null || clockEvent.ClockStatus != TimeClockStatus.Break || clockEvent.TimeDisplayed2.Year <= 1880) return clockEvent;

        command = "SELECT * FROM clockevent WHERE EmployeeNum=" + SOut.Long(employeeNum) + " "
                  + "AND ClockStatus != 2 " //not a break
                  + "ORDER BY TimeDisplayed1 DESC";
        command = DbHelper.LimitOrderBy(command, 1);
        clockEvent = ClockEventCrud.SelectOne(command);
        return clockEvent;
    }

    ///<summary>Will throw an exception if already clocked in.</summary>
    public static void ClockIn(long employeeNum, bool isAtHome)
    {
        var timeSpanMinClockIn = TimeCardRules.GetWhere(x => x.EmployeeNum.In(0, employeeNum) && x.MinClockInTime != TimeSpan.Zero)
            .OrderBy(x => x.MinClockInTime).FirstOrDefault()?.MinClockInTime ?? TimeSpan.Zero;
        if (DateTime.Now.TimeOfDay < timeSpanMinClockIn) throw new Exception(Lans.g("ClockEvents", "Error. Cannot clock in until") + ": " + timeSpanMinClockIn.ToStringHmm());

        //we'll get this again, because it may have been a while and may be out of date
        var clockEvent = GetLastEvent(employeeNum);
        if (clockEvent == null)
        {
            //new employee clocking in
            clockEvent = new ClockEvent();
            clockEvent.EmployeeNum = employeeNum;
            clockEvent.ClockStatus = TimeClockStatus.Home;
            clockEvent.ClinicNum = Clinics.ClinicNum;
            clockEvent.IsWorkingHome = isAtHome;
            Insert(clockEvent); //times handled
        }
        else if (clockEvent.ClockStatus == TimeClockStatus.Break)
        {
            //only incomplete breaks will have been returned.
            //clocking back in from break
            if (PrefC.GetBool(PrefName.LocalTimeOverridesServerTime))
                clockEvent.TimeEntered2 = DateTime.Now;
            else
                clockEvent.TimeEntered2 = MiscData.GetNowDateTime();

            clockEvent.TimeDisplayed2 = clockEvent.TimeEntered2;
            Update(clockEvent);
            if (clockEvent.IsWorkingHome != isAtHome)
            {
                //If coming back from break, and switching locations between home / office
                //This Sleep will ensure that the end of the break is before the end of the clockevent, otherwise it would break calc daily functionality.
                Thread.Sleep(1000);
                ClockOut(employeeNum, TimeClockStatus.Home); //Home means not lunch or break is being counted.
                //This Sleep will ensure that New clockevent starts a second after the first clockevent ends, otherwise it causes incorrect employee status.
                Thread.Sleep(1000);
                ClockIn(employeeNum, isAtHome); //Clock in to start new clockEvent from new location.
                return; //ensure we dont make 2 security logs for clocking in by hitting the end of this method twice.
            }
        }
        else
        {
            //normal clock in/out
            if (clockEvent.TimeDisplayed2.Year < 1880)
                //already clocked in
                throw new Exception(Lans.g("ClockEvents", "Error.  Already clocked in."));

            //clocked out for home or lunch. Need to clock back in by starting a new row.
            var timeClockStatus = clockEvent.ClockStatus;
            clockEvent = new ClockEvent();
            clockEvent.EmployeeNum = employeeNum;
            clockEvent.ClockStatus = timeClockStatus;
            clockEvent.ClinicNum = Clinics.ClinicNum;
            clockEvent.IsWorkingHome = isAtHome;
            Insert(clockEvent); //times handled
        }

        var employee = Employees.GetEmp(employeeNum);
        SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, employee.FName + " " + employee.LName + " " + "clocked in from " + clockEvent.ClockStatus + ".");
    }

    ///<summary>Will throw an exception if already clocked out.</summary>
    public static void ClockOut(long employeeNum, TimeClockStatus timeClockStatus)
    {
        var clockEvent = GetLastEvent(employeeNum);
        if (clockEvent == null)
            //new employee never clocked in
            throw new Exception(Lans.g("ClockEvents", "Error.  New employee never clocked in."));

        if (clockEvent.ClockStatus == TimeClockStatus.Break)
            //only incomplete breaks will have been returned.
            throw new Exception(Lans.g("ClockEvents", "Error.  Already clocked out for break."));

        //normal clock in/out
        if (clockEvent.TimeDisplayed2.Year > 1880)
            //clocked out for home or lunch. 
            throw new Exception(Lans.g("ClockEvents", "Error.  Already clocked out."));

        //clocked in.
        if (timeClockStatus == TimeClockStatus.Break)
        {
            //clocking out on break
            //leave the half-finished event alone and start a new one
            var clinicNum = clockEvent.ClinicNum;
            var isWorkingHome = clockEvent.IsWorkingHome;
            clockEvent = new ClockEvent();
            clockEvent.EmployeeNum = employeeNum;
            clockEvent.ClockStatus = TimeClockStatus.Break;
            clockEvent.ClinicNum = clinicNum;
            clockEvent.IsWorkingHome = isWorkingHome;
            Insert(clockEvent); //times handled
        }
        else
        {
            //finish the existing event
            if (PrefC.GetBool(PrefName.LocalTimeOverridesServerTime))
                clockEvent.TimeEntered2 = DateTime.Now;
            else
                clockEvent.TimeEntered2 = MiscData.GetNowDateTime();

            clockEvent.TimeDisplayed2 = clockEvent.TimeEntered2;
            clockEvent.ClockStatus = timeClockStatus; //whatever the user selected
            Update(clockEvent);
        }

        var employee = Employees.GetEmp(employeeNum);
        SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, employee.FName + " " + employee.LName + " " + "clocked out for " + clockEvent.ClockStatus + ".");
    }

    /// <summary>
    ///     Used in the timecard to track hours worked per week when the week started in a previous time period.  This
    ///     gets all the hours of the first week before the date listed.  Also adds in any adjustments for that week.
    /// </summary>
    public static TimeSpan GetWeekTotal(long employeeNum, DateTime date)
    {
        var timeSpan = new TimeSpan(0);
        var timeCardOvertimeFirstDayOfWeek = (DayOfWeek) PrefC.GetInt(PrefName.TimeCardOvertimeFirstDayOfWeek);
        //If the first day of the pay period is the starting date for the overtime, then there is no need to retrieve any times from the previous pay period.
        if (date.DayOfWeek == timeCardOvertimeFirstDayOfWeek) return timeSpan;

        //We only want to go back to the most recent "FirstDayOfWeek" week day.
        //The old code would simply go back in time 6 days which would cause problems.
        //The main problem was that hours from previous weeks were being counted towards other pay periods (sometimes).
        //E.g. pay period A = 01/28/2014 - 02/10/2014  pay period B = 02/11/2014 - 02/24/2014
        //The preference TimeCardOvertimeFirstDayOfWeek is set to Tuesday.
        //Employee worked 8 hours on Monday 02/10/2014 (falls within pay period A)
        //The first weekly total in pay period B will include hours worked from Monday 02/10/2014 (pay period A) because Tuesday > Monday (old logic).
        //However, the weekly total should NOT include Monday 02/10/2014 in pay period B's first weekly total because it is in not part of the current "week" based on TimeCardOvertimeFirstDayOfWeek. 
        //Therefore, we need to find out the date of the most recent TimeCardOvertimeFirstDayOfWeek.
        var dateMostRecentFirstDayOfWeek = date; //Start with the current date.
        //Loop backward through the week days until the TimeCardOvertimeFirstDayOfWeek is hit.
        for (var i = 1; i < 7; i++)
            //1 based because we already know that TimeCardOvertimeFirstDayOfWeek is not set to today so no need to check it.
            if (dateMostRecentFirstDayOfWeek.AddDays(-i).DayOfWeek == timeCardOvertimeFirstDayOfWeek)
            {
                dateMostRecentFirstDayOfWeek = dateMostRecentFirstDayOfWeek.AddDays(-i);
                break;
            }

        //mostRecentFirstDayOfWeekDate=date.AddDays(-6);
        var listClockEvents = Refresh(employeeNum, dateMostRecentFirstDayOfWeek, date.AddDays(-1), false);
        //eg, if this is Thursday, then we are getting last Friday through this Wed.
        for (var i = 0; i < listClockEvents.Count; i++)
        {
            //This is the old logic of trying to figure out if "events" fall within the most recent week.
            //The new way is correctly getting only the "events" for the most recent week which negates the need for any filtering.
            //if(events[i].TimeDisplayed1.DayOfWeek > date.DayOfWeek){//eg, Friday > Thursday, so ignore
            //	continue;
            //}
            //This scenario happens if the user clocks in and their system clock changes to previous date/time then they clock out.
            //This specific scenario has happened to PatNum 31287
            //This same PatNum also had an employee with multiple clock in events in the same minute (10+) with only one of the events having a clock out.
            //This check would fix that issue as well. 
            //If someone intentionally backdates a clock out event to get negative time, they can use an adjustment instead.
            if (listClockEvents[i].TimeDisplayed2 < listClockEvents[i].TimeDisplayed1) continue;

            timeSpan += listClockEvents[i].TimeDisplayed2 - listClockEvents[i].TimeDisplayed1;
            if (listClockEvents[i].AdjustIsOverridden)
                timeSpan += listClockEvents[i].Adjust;
            else
                timeSpan += listClockEvents[i].AdjustAuto; //typically zero

            //ot
            if (listClockEvents[i].OTimeHours != TimeSpan.FromHours(-1))
                //overridden
                timeSpan -= listClockEvents[i].OTimeHours;
            else
                timeSpan -= listClockEvents[i].OTimeAuto; //typically zero
        }

        //now, adjustments
        var listTimeAdjusts = TimeAdjusts.Refresh(employeeNum, dateMostRecentFirstDayOfWeek, date.AddDays(-1));
        for (var i = 0; i < listTimeAdjusts.Count; i++)
        {
            //This is the old logic of trying to figure out if adjustments fall within the most recent week.  
            //The new way is correctly getting only the "TimeAdjustList" for the most recent week which negates the need for any filtering.
            //if(TimeAdjustList[i].TimeEntry.DayOfWeek > date.DayOfWeek) {//eg, Friday > Thursday, so ignore
            //	continue;
            //}
            if (listTimeAdjusts[i].IsUnpaidProtectedLeave) continue;

            timeSpan += listTimeAdjusts[i].RegHours;
        }

        return timeSpan;
    }

    /// <summary>
    ///     -hh:mm or -hh.mm.ss or -hh.mm, depending on the pref.TimeCardsUseDecimalInsteadOfColon and
    ///     pref.TimeCardShowSeconds.  Blank if zero.
    /// </summary>
    public static string Format(TimeSpan timeSpan)
    {
        if (PrefC.GetBool(PrefName.TimeCardsUseDecimalInsteadOfColon))
        {
            if (timeSpan == TimeSpan.Zero) return "";

            return timeSpan.TotalHours.ToString("n");
        }

        if (PrefC.GetBool(PrefName.TimeCardShowSeconds))
            //Colon format with seconds
            return timeSpan.ToStringHmmss();

        //Colon format without seconds
        return timeSpan.ToStringHmm(); //blank if zero
    }

    /// <summary>
    ///     Avoids some funky behavior from TimeSpan.Parse(). Surround in try/catch.
    ///     Valid formats:
    ///     hh:mm
    ///     hh:mm:ss
    ///     hh:mm:ss.fff
    ///     TimeSpan.Parse("23:00:00") returns 23 hours.
    ///     TimeSpan.Parse("25:00:00") returns 25 days.
    ///     In this method, '25:00:00' is treated as 25 hours.
    ///     Throws exceptions
    /// </summary>
    public static TimeSpan ParseHours(string timeString)
    {
        var listParts = timeString.TrimStart('-').Split(':').ToList();
        if (listParts.Count > 3)
            //User input more than hours i.e. 00:00:00:00 this only accepts hours.
            throw new Exception("Invalid format");

        if (listParts.Any(x => string.IsNullOrEmpty(x)))
            //Blank or contains a blank segment
            throw new Exception("Invalid format");

        var isNegative = timeString.StartsWith("-");
        var timeSpan = TimeSpan.Zero;
        //Hours
        if (listParts.Count >= 1)
        {
            double hours = 0;
            try
            {
                hours = double.Parse(listParts[0]);
            }
            catch
            {
                throw new Exception("Invalid format");
            }

            timeSpan += TimeSpan.FromHours(hours);
        }

        //Minutes
        if (listParts.Count >= 2)
        {
            var minutes = 0;
            if (minutes >= 60 || minutes < 0) throw new Exception("Invalid format");

            try
            {
                minutes = int.Parse(listParts[1]);
            }
            catch
            {
                throw new Exception("Invalid format");
            }

            timeSpan += TimeSpan.FromMinutes(minutes);
        }

        //Seconds
        if (listParts.Count == 3)
        {
            double seconds = 0;
            if (seconds >= 60 && seconds < 0) throw new Exception("Invalid format");

            try
            {
                seconds = double.Parse(listParts[2]);
            }
            catch
            {
                throw new Exception("Invalid format");
            }

            timeSpan += TimeSpan.FromSeconds(seconds);
        }

        if (isNegative) timeSpan = -timeSpan;

        return timeSpan;
    }

    /// <summary>
    ///     Returns time card information for employees that have events during the pay period passed in.
    ///     Setting a clinicNum will only consider clock events and time adjusts from that clinic.
    ///     Set isAll true to return time card information for all employees regardless of having worked during the pay period.
    /// </summary>
    public static List<EmployeeTimeCard> GetTimeCardManage(PayPeriod payPeriod, long clinicNum, bool isAll)
    {
        var listEmployeeTimeCards = new List<EmployeeTimeCard>();
        var listTimeAdjustsPayPeriodNote = TimeAdjusts.GetNotesForPayPeriod(payPeriod.DateStart);
        var listEmployees = Employees.GetForTimeCard(); //Gets all non-hidden employees
        var listEmployeeNums = listEmployees.Select(x => x.EmployeeNum).ToList();
        //Get all of the clock events and time adjusts for every employee for the pay period.
        var listClockEvents = GetListForTimeCardManage(listEmployeeNums, clinicNum, payPeriod.DateStart, payPeriod.DateStop, isAll);
        var listTimeAdjusts = TimeAdjusts.GetListForTimeCardManage(listEmployeeNums, clinicNum, payPeriod.DateStart, payPeriod.DateStop, isAll);
        //Collect time card information from the previous pay period if it ended with an 'incomplete week'.
        //Incomplete weeks have hours worked at rates in the previous pay period that may need to be offset with overtime rates since they were paid at 'straight time' rates.
        //E.g. If 50 hours was worked in last 'incomplete week' of the previous pay period then 50 hours of 'straight time' was paid.
        //It is important that we negate the 'straight time' worked above 40 hours since those hours should be paid using overtime rates.
        //Meaning, 10 hours of straight time was paid, it needs to be negated and then replaced with 10 hours of overtime rates.
        var listClockEventsIncomplete = new List<ClockEvent>();
        var listTimeAdjustsIncomplete = new List<TimeAdjust>();
        var dateTimeStartIncomplete = TimeCardL.GetStartOfWeek(payPeriod.DateStart);
        var hasIncompleteWeek = dateTimeStartIncomplete != payPeriod.DateStart;
        if (hasIncompleteWeek)
        {
            //'Incomplete weeks' will always 'stop' on an imaginary date which will be the day before the start of the current pay period.
            //This is because we need to keep track of all time card events that happened within the previous pay period separately.
            //We need to use the values to help calculate overtime for the first week in this pay period and then to know how much 'straight time' to offset.
            var dateTimeStopIncomplete = payPeriod.DateStart.AddDays(-1);
            //Get all of the clock events and time adjusts for every employee for the 'incomplete week' portion of the previous pay period.
            listClockEventsIncomplete = GetListForTimeCardManage(listEmployeeNums, clinicNum, dateTimeStartIncomplete, dateTimeStopIncomplete, isAll);
            listTimeAdjustsIncomplete = TimeAdjusts.GetListForTimeCardManage(listEmployeeNums, clinicNum, dateTimeStartIncomplete, dateTimeStopIncomplete, isAll);
        }

        //Loop through every single non-hidden employee and create an EmployeeTimeCard object for them even if they don't have any time card events this pay period.
        for (var i = 0; i < listEmployees.Count; i++)
        {
            var employeeTimeCard = new EmployeeTimeCard();
            employeeTimeCard.Employee = listEmployees[i];
            listEmployeeTimeCards.Add(employeeTimeCard);
            var listClockEventsEmployee = listClockEvents.FindAll(x => x.EmployeeNum == listEmployees[i].EmployeeNum);
            var listTimeAdjustsEmployee = listTimeAdjusts.FindAll(x => x.EmployeeNum == listEmployees[i].EmployeeNum);
            var listClockEventsEmployeeIncomplete = listClockEventsIncomplete.FindAll(x => x.EmployeeNum == listEmployees[i].EmployeeNum);
            var listTimeAdjustsEmployeeIncomplete = listTimeAdjustsIncomplete.FindAll(x => x.EmployeeNum == listEmployees[i].EmployeeNum);

            #region Protected Leave and PTO

            double unpaidProtectedLeaveHours = 0;
            double ptoHours = 0;
            for (var j = 0; j < listTimeAdjustsEmployee.Count; j++)
            {
                if (listTimeAdjustsEmployee[j].IsUnpaidProtectedLeave) unpaidProtectedLeaveHours += listTimeAdjustsEmployee[j].RegHours.TotalHours;

                if (listTimeAdjustsEmployee[j].PtoDefNum != 0) ptoHours += listTimeAdjustsEmployee[j].PtoHours.TotalHours;
            }

            //We do not consider unprotected leave as hours worked.
            //We do NOT remove PTO hours since those are calculated at rate 1.
            listTimeAdjustsEmployee.RemoveAll(x => x.IsUnpaidProtectedLeave);

            #endregion

            //Set the time card note to the note of the first clock event or to a validation error message if there is one.

            #region Validate ClockEvents

            var listClockEventsValidate = new List<ClockEvent>(listClockEventsEmployeeIncomplete);
            listClockEventsValidate.AddRange(listClockEventsEmployee);
            var clockEventErrors = ValidateEvents(listClockEventsValidate);
            if (clockEventErrors != "")
            {
                employeeTimeCard.Note = clockEventErrors;
                employeeTimeCard.HasError = true;
                continue;
            }

            #endregion Validate ClockEvents

            employeeTimeCard.ListTimeCardWeeks = TimeCardL.GetTimeCardWeeks(listClockEventsEmployee, listTimeAdjustsEmployee);
            employeeTimeCard.ListTimeCardWeeksIncomplete = TimeCardL.GetTimeCardWeeks(listClockEventsEmployeeIncomplete, listTimeAdjustsEmployeeIncomplete);

            #region Validate Incomplete Week

            if (employeeTimeCard.ListTimeCardWeeksIncomplete.Count > 1)
            {
                //There is a critical bug with the logic above; There should only be a singular 'incomplete week' at the end of the previous pay period.
                employeeTimeCard.Note = Lans.g("ClockEvents", "Too many 'incomplete weeks' were returned when calculating overtime.");
                employeeTimeCard.HasError = true;
                continue;
            }

            #endregion

            if (listClockEventsEmployee.Count > 0)
                //Grab the note from the first clock event entry on the pay period.
                employeeTimeCard.Note = listClockEventsEmployee[0].Note;

            //Inject all time card events from the 'incomplete week' into the first week of the current pay period.
            //Any 'straight time' from the previous pay period that should have been paid at overtime rates needs to be handled here.
            //This needs to happen regardless of the existence of clock events or adjustments for the current pay period.
            //Create a 'fake' first week for the current pay period if one doesn't exist.
            if (employeeTimeCard.ListTimeCardWeeksIncomplete.Count > 0)
            {
                //Any incomplete week from the end of the previous pay period that has overtime needs to be taken care of within the first week of this pay period.
                //Find the TimeCardWeek for the first week within this pay period keeping in mind that it might not even exist (e.g. no time card events).
                var weekOfYear = TimeCardL.GetWeekOfYear(payPeriod.DateStart);
                var timeCardWeek = employeeTimeCard.ListTimeCardWeeks.Find(x => x.WeekOfYear == weekOfYear);
                //If there is no such time card week object, create one.
                if (timeCardWeek == null)
                {
                    timeCardWeek = new TimeCardWeek(weekOfYear);
                    employeeTimeCard.ListTimeCardWeeks.Insert(0, timeCardWeek);
                }

                //Inject the 'incomplete week' time card events into the first week. These events will be offset as necessary later in this method.
                timeCardWeek.ListTimeCardObjects.AddRange(employeeTimeCard.ListTimeCardWeeksIncomplete[0].ListTimeCardObjects);
            }

            double totalHours = 0;
            double totalRate1Hours = 0;
            double totalRate1OTHours = 0;
            double totalRate2Hours = 0;
            double totalRate2OTHours = 0;
            double totalRate3Hours = 0;
            double totalRate3OTHours = 0;
            //Overtime rates need to be calculated weekly.
            //Loop through each week and process overtime rates based on the hours worked during each week.
            for (var j = 0; j < employeeTimeCard.ListTimeCardWeeks.Count; j++)
            {
                //Calculate overtime for the completed week.
                var listTimeCardObjects = employeeTimeCard.ListTimeCardWeeks[j].ListTimeCardObjects;
                var straightTimeHoursWeekly = listTimeCardObjects.Sum(x => x.GetTimeSpanStraightTime().TotalHours);
                var overtimeHoursWeekly = listTimeCardObjects.Sum(x => x.GetTimeSpanOvertime().TotalHours);
                var totalHoursWeekly = listTimeCardObjects.Sum(x => x.GetTimeSpanTotal().TotalHours);
                var rate2HoursWeekly = listTimeCardObjects.Sum(x => x.GetTimeSpanRate2().TotalHours);
                var rate3HoursWeekly = listTimeCardObjects.Sum(x => x.GetTimeSpanRate3().TotalHours);
                //Calculate the ratio of hours worked this week at each rate.
                double rate1Ratio = 0;
                double rate2Ratio = 0;
                double rate3Ratio = 0;
                //Overtime hours for each rate is based on the ratio of hours of each rate against the total OT hours worked.
                //Example for 40 hour week + 10 OT hours = 50 total hours
                //
                //					Hours Worked	HoursWorked/TotalHours		BaseHoursAtRate			OTHoursAtRate
                //	Rate1				30						30 / 50	= 0.60				0.60 * 40 = 24			0.60 * 10 =  6
                //	Rate2				 5						 5 / 50 = 0.10				0.10 * 40 =  4			0.10 * 10 =  1
                //	Rate3				15						15 / 50 = 0.30				0.30 * 40 = 12			0.30 * 10 =  3
                //																											Total = 40			    Total = 10
                if (totalHoursWeekly != 0)
                {
                    rate2Ratio = rate2HoursWeekly / totalHoursWeekly;
                    rate3Ratio = rate3HoursWeekly / totalHoursWeekly;
                    rate1Ratio = 1 - rate2Ratio - rate3Ratio; //"self correcting math" guaranteed to total correctly.
                }

                //Regular time at Rate1, Rate2, and Rate3
                var rate1Hours = rate1Ratio * straightTimeHoursWeekly;
                var rate2Hours = rate2Ratio * straightTimeHoursWeekly;
                var rate3Hours = straightTimeHoursWeekly - rate1Hours - rate2Hours; //"self correcting math" guaranteed to total correctly.
                //OT hours
                var rate1OTHours = rate1Ratio * overtimeHoursWeekly;
                var rate2OTHours = rate2Ratio * overtimeHoursWeekly;
                var rate3OTHours = totalHoursWeekly - rate1Hours - rate2Hours - rate3Hours - rate1OTHours - rate2OTHours; //"self correcting math" guaranteed to total correctly.

                #region Incomplete Week

                //Offset the 'straight time' that should be paid at overtime rates from the 'incomplete week' of the previous pay period.
                //Only do this once for the very first week of the current pay period (j==0).
                if (j == 0 && employeeTimeCard.ListTimeCardWeeksIncomplete.Count > 0)
                {
                    var listTimeCardObjectsIncomplete = employeeTimeCard.ListTimeCardWeeksIncomplete[0].ListTimeCardObjects;
                    var incompleteTotalHours = listTimeCardObjectsIncomplete.Sum(x => x.GetTimeSpanTotal().TotalHours);
                    var incompleteRate2Hours = listTimeCardObjectsIncomplete.Sum(x => x.GetTimeSpanRate2().TotalHours);
                    var incompleteRate3Hours = listTimeCardObjectsIncomplete.Sum(x => x.GetTimeSpanRate3().TotalHours);
                    var incompleteRate1Hours = incompleteTotalHours - incompleteRate2Hours - incompleteRate3Hours;
                    //Blindly negate the 'straight time' that was paid in the previous pay period from each corresponding rate totals.
                    totalHoursWeekly -= incompleteTotalHours;
                    rate1Hours -= incompleteRate1Hours;
                    rate2Hours -= incompleteRate2Hours;
                    rate3Hours -= incompleteRate3Hours;
                }

                #endregion Incomplete Week

                //Add the weekly calculated hours to the totals for the pay period.
                totalHours += totalHoursWeekly;
                totalRate1Hours += rate1Hours;
                totalRate1OTHours += rate1OTHours;
                totalRate2Hours += rate2Hours;
                totalRate2OTHours += rate2OTHours;
                totalRate3Hours += rate3Hours;
                totalRate3OTHours += rate3OTHours;
            }

            employeeTimeCard.TotalHours = TimeSpan.FromHours(totalHours);
            employeeTimeCard.Rate1Hours = TimeSpan.FromHours(totalRate1Hours);
            employeeTimeCard.Rate1OTHours = TimeSpan.FromHours(totalRate1OTHours);
            employeeTimeCard.Rate2Hours = TimeSpan.FromHours(totalRate2Hours);
            employeeTimeCard.Rate2OTHours = TimeSpan.FromHours(totalRate2OTHours);
            employeeTimeCard.Rate3Hours = TimeSpan.FromHours(totalRate3Hours);
            employeeTimeCard.Rate3OTHours = TimeSpan.FromHours(totalRate3OTHours);
            employeeTimeCard.PTOHours = TimeSpan.FromHours(ptoHours);
            employeeTimeCard.ProtectedLeaveHours = TimeSpan.FromHours(unpaidProtectedLeaveHours);
            //Trump any clock event note with a pay period note to preserve old behavior.
            var timeAdjustPayPeriodNote = listTimeAdjustsPayPeriodNote.Find(x => x.EmployeeNum == listEmployees[i].EmployeeNum);
            if (timeAdjustPayPeriodNote != null && !string.IsNullOrEmpty(timeAdjustPayPeriodNote.Note)) employeeTimeCard.Note = timeAdjustPayPeriodNote.Note;
        }

        //Loop backwards through the employee time cards and remove ones that do not need to be displayed.
        for (var i = listEmployeeTimeCards.Count - 1; i >= 0; i--)
        {
            if (isAll) break; //Show them all.

            //Do not remove time cards with errors to display.
            if (listEmployeeTimeCards[i].HasError) continue;

            //Do not remove time cards with time card objects within the current pay period.
            if (listEmployeeTimeCards[i].ListTimeCardWeeks.Any(x => x.ListTimeCardObjects.Count > 0)) continue;

            //Do not remove time cards where the last incomplete week of the previous pay period has overtime to correct.
            if (listEmployeeTimeCards[i].ListTimeCardWeeksIncomplete.Count > 0) continue;

            //Remove this time card from the return payload.
            listEmployeeTimeCards.RemoveAt(i);
        }

        return listEmployeeTimeCards;
    }

    ///<summary>Returns all clock events, of all statuses, for a given employee between the date range (inclusive).</summary>
    public static List<ClockEvent> GetSimpleList(long employeeNum, DateTime dateTimeStart, DateTime dateTimeStop)
    {
        //Fill list-----------------------------------------------------------------------------------------------------------------------------
        var command =
            "SELECT * FROM clockevent WHERE"
            + " EmployeeNum = '" + SOut.Long(employeeNum) + "'"
            + " AND TimeDisplayed1 >= " + SOut.Date(dateTimeStart)
            + " AND TimeDisplayed1 < " + SOut.Date(dateTimeStop.AddDays(1)) //adding a day takes it to midnight of the specified toDate
            + " ORDER BY TimeDisplayed1";
        return ClockEventCrud.SelectMany(command);
    }
}

public class EmployeeTimeCard
{
    public Employee Employee;
    public bool HasError;
    public List<TimeCardWeek> ListTimeCardWeeks = new();
    public List<TimeCardWeek> ListTimeCardWeeksIncomplete = new();
    public string Note = "";
    public TimeSpan ProtectedLeaveHours;
    public TimeSpan PTOHours;
    public TimeSpan Rate1Hours;
    public TimeSpan Rate1OTHours;
    public TimeSpan Rate2Hours;
    public TimeSpan Rate2OTHours;
    public TimeSpan Rate3Hours;
    public TimeSpan Rate3OTHours;
    public TimeSpan TotalHours;

    public string GetExportString()
    {
        var listExportValues = new List<string>();
        listExportValues.Add(Tidy(Employee.PayrollID));
        listExportValues.Add(Tidy(Employee.EmployeeNum.ToString()));
        listExportValues.Add(Tidy(Employee.FName));
        listExportValues.Add(Tidy(Employee.LName));
        listExportValues.Add(GetTimeFomatted(TotalHours));
        listExportValues.Add(GetTimeFomatted(Rate1Hours));
        listExportValues.Add(GetTimeFomatted(Rate1OTHours));
        listExportValues.Add(GetTimeFomatted(Rate2Hours));
        listExportValues.Add(GetTimeFomatted(Rate2OTHours));
        listExportValues.Add(GetTimeFomatted(Rate3Hours));
        listExportValues.Add(GetTimeFomatted(Rate3OTHours));
        listExportValues.Add(GetTimeFomatted(PTOHours));
        listExportValues.Add(GetTimeFomatted(ProtectedLeaveHours));
        listExportValues.Add(Tidy(Note));
        return string.Join("\t", listExportValues);
    }

    private string GetTimeFomatted(TimeSpan timeSpan)
    {
        if (PrefC.GetBool(PrefName.TimeCardsUseDecimalInsteadOfColon)) return timeSpan.TotalHours.ToString("n");

        if (PrefC.GetBool(PrefName.TimeCardShowSeconds))
            //Colon format with seconds
            return timeSpan.ToStringHmmss();

        //Colon format without seconds
        return timeSpan.ToStringHmm();
    }

    private string Tidy(string text)
    {
        return text.Replace("\t", "").Replace("\r\n", ";  ");
    }
}