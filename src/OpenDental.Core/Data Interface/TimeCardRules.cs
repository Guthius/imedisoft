using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TimeCardRules
{
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<TimeCardRule> Refresh(long patNum){

        string command="SELECT * FROM timecardrule WHERE PatNum = "+POut.Long(patNum);
        return Crud.TimeCardRuleCrud.SelectMany(command);
    }

    ///<summary>Gets one TimeCardRule from the db.</summary>
    public static TimeCardRule GetOne(long timeCardRuleNum){

        return Crud.TimeCardRuleCrud.SelectOne(timeCardRuleNum);
    }*/

    
    public static long Insert(TimeCardRule timeCardRule)
    {
        return TimeCardRuleCrud.Insert(timeCardRule);
    }

    
    public static void InsertMany(List<TimeCardRule> listTimeCardRules)
    {
        TimeCardRuleCrud.InsertMany(listTimeCardRules);
    }

    
    public static void Update(TimeCardRule timeCardRule)
    {
        TimeCardRuleCrud.Update(timeCardRule);
    }

    
    public static void Delete(long timeCardRuleNum)
    {
        var command = "DELETE FROM timecardrule WHERE TimeCardRuleNum = " + SOut.Long(timeCardRuleNum);
        Db.NonQ(command);
    }

    
    public static void DeleteMany(List<long> listTimeCardRuleNums)
    {
        if (listTimeCardRuleNums == null || listTimeCardRuleNums.Count == 0) return;

        var command = "DELETE FROM timecardrule WHERE TimeCardRuleNum IN (" + string.Join(",", listTimeCardRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Validates pay period before making any adjustments.
    ///     If today falls before the stopDate passed in, stopDate will be set to today's date.
    /// </summary>
    public static string ValidatePayPeriod(Employee employee, DateTime dateStart, DateTime dateStop)
    {
        //If calculating breaks before the end date of the pay period, only calculate breaks and validate clock in and out events for days
        //before today.  Use the server time just because we are dealing with time cards.
        var dateTimeNow = MiscData.GetNowDateTime();
        var clockEventLast = ClockEvents.GetLastEvent(employee.EmployeeNum);
        var isStillWorking = clockEventLast != null && (clockEventLast.ClockStatus == TimeClockStatus.Break || clockEventLast.TimeDisplayed2.Year < 1880);
        if (dateTimeNow.Date < dateStop.Date && isStillWorking) dateStop = dateTimeNow.Date.AddDays(-1);
        var listClockEventsBreak = ClockEvents.Refresh(employee.EmployeeNum, dateStart, dateStop, true);
        var listClockEvents = ClockEvents.Refresh(employee.EmployeeNum, dateStart, dateStop, false);
        var hasError = false;
        var retVal = "Time card errors for employee : " + Employees.GetNameFL(employee) + "\r\n";
        //Validate clock events
        for (var i = 0; i < listClockEvents.Count; i++)
            if (listClockEvents[i].TimeDisplayed2.Year < 1880)
            {
                retVal += "  " + listClockEvents[i].TimeDisplayed1.ToShortDateString() + " : Employee not clocked out.\r\n";
                hasError = true;
            }
            else if (listClockEvents[i].TimeDisplayed1.Date != listClockEvents[i].TimeDisplayed2.Date)
            {
                retVal += "  " + listClockEvents[i].TimeDisplayed1.ToShortDateString() + " : Clock entry spans multiple days.\r\n";
                hasError = true;
            }

        //Validate Breaks
        for (var i = 0; i < listClockEventsBreak.Count; i++)
        {
            if (listClockEventsBreak[i].TimeDisplayed2.Year < 1880)
            {
                retVal += "  " + listClockEventsBreak[i].TimeDisplayed1.ToShortDateString() + " : Employee not clocked in from break.\r\n";
                hasError = true;
            }

            if (listClockEventsBreak[i].TimeDisplayed1.Date != listClockEventsBreak[i].TimeDisplayed2.Date)
            {
                retVal += "  " + listClockEventsBreak[i].TimeDisplayed1.ToShortDateString() + " : One break spans multiple days.\r\n";
                hasError = true;
            }

            for (var c = listClockEvents.Count - 1; c >= 0; c--)
            {
                if (listClockEvents[c].TimeDisplayed1.Date == listClockEventsBreak[i].TimeDisplayed1.Date) break;
                if (c == 0)
                {
                    //we never found a match
                    retVal += "  " + listClockEventsBreak[i].TimeDisplayed1.ToShortDateString() + " : Break found during non-working day.\r\n";
                    hasError = true;
                }
            }
        }

        if (hasError) return retVal;
        return "";
    }

    /// <summary>
    ///     Cannot have both AM/PM rules and OverHours rules defined.
    ///     We no longer block having multiple rules defined. With a better interface we can improve some of this
    ///     functionality. Per NS 09/15/2015.
    /// </summary>
    public static string ValidateOvertimeRules(List<long> listEmployeeNums = null)
    {
        var stringBuilder = new StringBuilder();
        RefreshCache();
        var listTimeCardRules = GetDeepCopy();
        if (listEmployeeNums != null && listEmployeeNums.Count > 0) listTimeCardRules = listTimeCardRules.FindAll(x => x.EmployeeNum == 0 || listEmployeeNums.Contains(x.EmployeeNum));
        //Generate error messages for "All Employees" timecard rules.
        var listTimeCardRulesAll = listTimeCardRules.FindAll(x => x.EmployeeNum == 0);
        if (listTimeCardRulesAll.Any(x => x.AfterTimeOfDay > TimeSpan.Zero || x.BeforeTimeOfDay > TimeSpan.Zero) //There exists an AM or PM rule
            && listTimeCardRulesAll.Any(x => x.OverHoursPerDay > TimeSpan.Zero)) //There also exists an Over hours rule.
        {
            stringBuilder.AppendLine("Time card errors found for \"All Employees\":");
            stringBuilder.AppendLine("  Both a time of day rule and an over hours per day rule found. Only one or the other is allowed.");
            return stringBuilder.ToString();
        }

        listEmployeeNums = listTimeCardRules.Where(x => x.EmployeeNum > 0).Select(x => x.EmployeeNum).Distinct().ToList();
        //Generate Employee specific errors
        for (var i = 0; i < listEmployeeNums.Count; i++)
        {
            var empNum = listEmployeeNums[i];
            var listTimeCardRulesEmp = listTimeCardRules.FindAll(x => x.EmployeeNum == 0 || x.EmployeeNum == empNum);
            if (listTimeCardRulesEmp.Any(x => x.AfterTimeOfDay > TimeSpan.Zero || x.BeforeTimeOfDay > TimeSpan.Zero) //There exists an AM or PM rule
                && listTimeCardRulesEmp.Any(x => x.OverHoursPerDay > TimeSpan.Zero)) //There also exists an Over hours rule.
            {
                var empName = Employees.GetNameFL(Employees.GetEmp(empNum));
                stringBuilder.AppendLine("Time card errors found for " + empName + ":");
                stringBuilder.AppendLine("  Both a time of day rule and an over hours per day rule found. Only one or the other is allowed.\r\n");
            }
        }

        return stringBuilder.ToString();
    }

    ///<summary>Clears automatic adjustment/adjustOT values and deletes automatic TimeAdjusts for period.</summary>
    public static void ClearAuto(long employeeNum, DateTime dateStart, DateTime dateStop)
    {
        var listClockEvents = ClockEvents.GetSimpleList(employeeNum, dateStart, dateStop);
        for (var i = 0; i < listClockEvents.Count; i++)
        {
            listClockEvents[i].AdjustAuto = TimeSpan.Zero;
            listClockEvents[i].OTimeAuto = TimeSpan.Zero;
            listClockEvents[i].Rate2Auto = TimeSpan.Zero;
            listClockEvents[i].Rate3Auto = TimeSpan.Zero;
            ClockEvents.Update(listClockEvents[i]);
        }

        var listTimeAdjusts = TimeAdjusts.GetSimpleListAuto(employeeNum, dateStart, dateStop);
        for (var i = 0; i < listTimeAdjusts.Count; i++)
        {
            TimeAdjusts.Delete(listTimeAdjusts[i]);
            SecurityLogs.MakeLogEntry(EnumPermType.TimeAdjustEdit, 0,
                $"Automatic Time Card Adjustments were cleared. Adjustment deleted for Employee: {Employees.GetNameFL(listTimeAdjusts[i].EmployeeNum)}.");
        }
    }

    /// <summary>
    ///     Clears all manual adjustments/Adjust OT values from clock events. Does not alter adjustments to
    ///     clockevent.TimeDisplayed1/2 nor does it delete or alter any TimeAdjusts.
    /// </summary>
    public static void ClearManual(long employeeNum, DateTime dateStart, DateTime dateStop)
    {
        var listClockEvents = ClockEvents.GetSimpleList(employeeNum, dateStart, dateStop);
        for (var i = 0; i < listClockEvents.Count; i++)
        {
            listClockEvents[i].Adjust = TimeSpan.Zero;
            listClockEvents[i].AdjustIsOverridden = false;
            listClockEvents[i].OTimeHours = TimeSpan.FromHours(-1);
            listClockEvents[i].Rate2Hours = TimeSpan.FromHours(-1);
            listClockEvents[i].Rate3Hours = TimeSpan.FromHours(-1);
            ClockEvents.Update(listClockEvents[i]);
        }
    }

    /// <summary>
    ///     Validates list and throws exceptions. Always returns a value. Creates a timecard rule based on all applicable
    ///     timecard rules for a given employee.
    /// </summary>
    public static TimeCardRule GetTimeCardRule(Employee employee)
    {
        //Validate Rules---------------------------------------------------------------------------------------------------------------
        var errors = ValidateOvertimeRules(new List<long> {employee.EmployeeNum});
        if (errors.Length > 0) throw new Exception(errors);
        //Build return value ----------------------------------------------------------------------------------------------------------
        var listTimeCardRulesEmp = GetWhere(x => x.EmployeeNum == 0 || x.EmployeeNum == employee.EmployeeNum);
        var timeCardRuleAm = listTimeCardRulesEmp.Where(x => x.BeforeTimeOfDay > TimeSpan.Zero).OrderByDescending(x => x.BeforeTimeOfDay).FirstOrDefault();
        var timeCardRulePm = listTimeCardRulesEmp.Where(x => x.AfterTimeOfDay > TimeSpan.Zero).OrderBy(x => x.AfterTimeOfDay).FirstOrDefault();
        var timeCardRuleHours = listTimeCardRulesEmp.Where(x => x.OverHoursPerDay > TimeSpan.Zero).OrderBy(x => x.OverHoursPerDay).FirstOrDefault();
        var timeCardRuleIsOvertimeExempt = listTimeCardRulesEmp.Where(x => x.IsOvertimeExempt).FirstOrDefault();
        var timeCardRuleIsWeekendRate3 = listTimeCardRulesEmp.Where(x => x.HasWeekendRate3).FirstOrDefault();
        var timeCardRule = new TimeCardRule();
        if (timeCardRuleAm != null) timeCardRule.BeforeTimeOfDay = timeCardRuleAm.BeforeTimeOfDay;
        if (timeCardRulePm != null) timeCardRule.AfterTimeOfDay = timeCardRulePm.AfterTimeOfDay;
        if (timeCardRuleHours != null) timeCardRule.OverHoursPerDay = timeCardRuleHours.OverHoursPerDay;
        if (timeCardRuleIsOvertimeExempt != null) timeCardRule.IsOvertimeExempt = timeCardRuleIsOvertimeExempt.IsOvertimeExempt;
        if (timeCardRuleIsWeekendRate3 != null) timeCardRule.HasWeekendRate3 = timeCardRuleIsWeekendRate3.HasWeekendRate3;
        return timeCardRule;
    }

    /// <summary>
    ///     Calculates daily overtime.  Daily overtime does not take into account any time adjust events.
    ///     All manually entered time adjust events are assumed to be entered correctly and should not be used in calculating
    ///     automatic totals.
    ///     Throws exceptions when encountering errors.
    /// </summary>
    public static void CalculateDailyOvertime(Employee employee, DateTime dateStart, DateTime dateStop)
    {
        #region Fill Lists, validate data sets, generate error messages.

        var listClockEvents = new List<ClockEvent>();
        var listClockEventsBreak = new List<ClockEvent>();
        var timeCardRule = new TimeCardRule();
        var errors = "";
        var clockErrors = "";
        var breakErrors = "";
        var ruleErrors = "";
        //If calculating breaks before the end date of the pay period, only calculate breaks and validate clock in and out events for days
        //before today.  Use the server time just because we are dealing with time cards.
        var dateTimeNow = MiscData.GetNowDateTime();
        var clockEventLast = ClockEvents.GetLastEvent(employee.EmployeeNum);
        var isStillWorking = clockEventLast != null && (clockEventLast.ClockStatus == TimeClockStatus.Break || clockEventLast.TimeDisplayed2.Year < 1880);
        if (dateTimeNow.Date < dateStop.Date && isStillWorking) dateStop = dateTimeNow.Date.AddDays(-1);
        //Fill lists and catch validation error messages------------------------------------------------------------------------------------------------------------
        try
        {
            listClockEvents = ClockEvents.GetValidList(employee.EmployeeNum, dateStart, dateStop, false);
        }
        catch (Exception ex)
        {
            clockErrors += ex.Message;
        }

        try
        {
            listClockEventsBreak = ClockEvents.GetValidList(employee.EmployeeNum, dateStart, dateStop, true);
        }
        catch (Exception ex)
        {
            breakErrors += ex.Message;
        }

        try
        {
            timeCardRule = GetTimeCardRule(employee);
        }
        catch (Exception ex)
        {
            ruleErrors += ex.Message;
        }

        //Validation between two or more lists above----------------------------------------------------------------------------------------------------------------
        for (var b = 0; b < listClockEventsBreak.Count; b++)
        {
            var isValidBreak = false;
            for (var c = 0; c < listClockEvents.Count; c++)
                if (TimeClockEventsOverlapHelper(listClockEventsBreak[b], listClockEvents[c]))
                {
                    if (listClockEventsBreak[b].TimeDisplayed1 > listClockEvents[c].TimeDisplayed1 //break started after work did
                        && listClockEventsBreak[b].TimeDisplayed2 < listClockEvents[c].TimeDisplayed2) //break ended before working hours
                    {
                        //valid break.
                        isValidBreak = true;
                        break;
                    }

                    //invalid break.
                    isValidBreak = false; //redundant, but harmless. Makes code more readable.
                    break;
                }

            if (isValidBreak) continue;
            breakErrors += "  " + listClockEventsBreak[b].TimeDisplayed1 + " : break found during non-working hours.\r\n"; //ToString() instead of ToShortDateString() to show time.
        }

        //Report Errors---------------------------------------------------------------------------------------------------------------------------------------------
        errors = ruleErrors + clockErrors + breakErrors;
        if (errors != "") throw new Exception(Employees.GetNameFL(employee) + " has the following errors:\r\n" + errors);
        //throw new Exception(errors);

        #endregion

        #region Fill time card rules

        //Begin calculations=========================================================================================================================================
        var timeSpanHoursWorkedTotal = new TimeSpan();
        var timeSpanOTHoursRule = new TimeSpan(24, 0, 0); //Example 10:00 for overtime rule after 10 hours per day.
        var timeSpanRate2AMRule = new TimeSpan(); //Example 06:00 for Rate2 rule before 6am.
        var timeSpanRate2PMRule = new TimeSpan(24, 0, 0); //Example 17:00 for Rate2 rule after  5pm.
        //Fill over hours rule from list-------------------------------------------------------------------------------------
        if (timeCardRule.OverHoursPerDay != TimeSpan.Zero) //OverHours Rule
            timeSpanOTHoursRule = timeCardRule.OverHoursPerDay; //at most, one non-zero OverHours rule available at this point.
        if (timeCardRule.BeforeTimeOfDay != TimeSpan.Zero) //AM Rule
            timeSpanRate2AMRule = timeCardRule.BeforeTimeOfDay; //at most, one non-zero AM rule available at this point.
        if (timeCardRule.AfterTimeOfDay != TimeSpan.Zero) //PM Rule
            timeSpanRate2PMRule = timeCardRule.AfterTimeOfDay; //at most, one non-zero PM rule available at this point.

        #endregion

        //Calculations: Regular Time, Overtime, Rate2, Rate3 time---------------------------------------------------------------------------------------------------
        var timeSpanDailyBreaksAdjustTotal = new TimeSpan(); //used to adjust the clock event
        var timeSpanDailyBreaksTotal = new TimeSpan(); //used in calculating breaks per day.
        var timeSpanDailyBreaksCalc = new TimeSpan(); //used in calculating breaks per day.
        var timeSpanDailyHoursMinusBreaksTotal = new TimeSpan(); //used in calculating breaks per day.
        var timeSpanDailyRate2Total = new TimeSpan(); //hours before and after AM/PM Rate2 rules. Adjusted for overbreaks.
        var timeSpanDailyRate3Total = new TimeSpan(); //hours worked on weekend for Rate3. Adjusted for overbreaks.
        //Note: If TimeCardsMakesAdjustmentsForOverBreaks is true, only the first 30 minutes of break per day are paid. 
        //All breaktime thereafter will be calculated as if the employee was clocked out at that time.
        for (var i = 0; i < listClockEvents.Count; i++)
        {
            #region Rate3 pay (including overbreak adjustments)--------------------------------------------------------------

            //Determine if using Rate3 rule and hours worked are on the weekend.
            //Rate3 overrides Rate2, meaning any hours worked on the weekend are earned at Rate3 even if working before Rate2 AM rule or after Rate2 PM rule.
            if (timeCardRule.HasWeekendRate3 &&
                ((listClockEvents[i].TimeDisplayed1.DayOfWeek == DayOfWeek.Saturday && listClockEvents[i].TimeDisplayed2.DayOfWeek == DayOfWeek.Saturday)
                 || (listClockEvents[i].TimeDisplayed1.DayOfWeek == DayOfWeek.Sunday && listClockEvents[i].TimeDisplayed2.DayOfWeek == DayOfWeek.Sunday)))
            {
                //All time worked on weekend qualifies for Rate3
                timeSpanDailyRate3Total = listClockEvents[i].TimeDisplayed2 - listClockEvents[i].TimeDisplayed1;
                var timeSpanRate3BreakTimeCounter = new TimeSpan();
                //Subtract overbreaks
                for (var b = 0; b < listClockEventsBreak.Count; b++)
                    //sum breaks that occur during this clock event
                    if (TimeClockEventsOverlapHelper(listClockEvents[i], listClockEventsBreak[b]))
                        timeSpanRate3BreakTimeCounter += listClockEventsBreak[b].TimeDisplayed2 - listClockEventsBreak[b].TimeDisplayed1;

                var timeSpanRate3AdjustAmount = timeSpanRate3BreakTimeCounter - TimeSpan.FromMinutes(30); //Overbreak
                if (timeSpanRate3AdjustAmount > TimeSpan.Zero) timeSpanDailyRate3Total -= timeSpanRate3AdjustAmount;
                if (timeSpanDailyRate3Total < TimeSpan.Zero)
                    //this should never happen. If it ever does, we need to know about it, because that means some math has been miscalculated.
                    throw new Exception(" - " + listClockEvents[i].TimeDisplayed1.Date.ToShortDateString() + ", " + employee.FName + " " + employee.LName + " : calculated Rate3 hours was negative.");
                listClockEvents[i].Rate3Auto = timeSpanDailyRate3Total; //should be zero or greater.
                listClockEvents[i].Rate2Auto = new TimeSpan(0); //No ClockEvent can have both Rate2 and Rate3 hours. Zero out in case modifications were made.
            }

            #endregion

            #region Rate2 pay (including overbreak adjustments)--------------------------------------------------------------

            else
            {
                if (i == 0 || listClockEvents[i].TimeDisplayed1.Date != listClockEvents[i - 1].TimeDisplayed1.Date) timeSpanDailyRate2Total = TimeSpan.Zero;
                //AM-----------------------------------
                if (listClockEvents[i].TimeDisplayed1.TimeOfDay < timeSpanRate2AMRule)
                {
                    //clocked in before Rate2 AM rule
                    timeSpanDailyRate2Total += timeSpanRate2AMRule - listClockEvents[i].TimeDisplayed1.TimeOfDay;
                    if (listClockEvents[i].TimeDisplayed2.TimeOfDay < timeSpanRate2AMRule) //clocked out before Rate2 AM rule also
                        timeSpanDailyRate2Total += listClockEvents[i].TimeDisplayed2.TimeOfDay - timeSpanRate2AMRule; //add a negative timespan
                    //Adjust Rate2 AM by overbreaks-----
                    var timeSpanAMBreakTimeCounter = new TimeSpan(); //tracks all break time for use in calculating overages.
                    var timeSpanAMBreakDuringRate2 = new TimeSpan(); //tracks only the portion of breaks that occurred during Rate2 hours.
                    for (var b = 0; b < listClockEventsBreak.Count; b++)
                    {
                        if (timeSpanAMBreakTimeCounter > TimeSpan.FromMinutes(30)) timeSpanAMBreakTimeCounter = TimeSpan.FromMinutes(30); //reset overages for next calculation.
                        if (listClockEventsBreak[b].TimeDisplayed1.Date != listClockEvents[i].TimeDisplayed1.Date) continue; //skip breaks for other days.
                        timeSpanAMBreakTimeCounter += listClockEventsBreak[b].TimeDisplayed2 - listClockEventsBreak[b].TimeDisplayed1;
                        timeSpanAMBreakDuringRate2 += CalcRate2Portion(timeSpanRate2AMRule, TimeSpan.FromHours(24), listClockEventsBreak[b]);
                        if (timeSpanAMBreakTimeCounter < TimeSpan.FromMinutes(30)) continue; //not over thirty minutes yet.
                        if (TimeClockEventsOverlapHelper(listClockEvents[i], listClockEventsBreak[b])) continue; //There must be multiple clock events for this day, and we have gone over breaks during a later clock event period
                        if (listClockEventsBreak[b].TimeDisplayed1.TimeOfDay > timeSpanRate2AMRule) continue; //this break started after the Rate2 AM rule so there is nothing left to do in this loop. break out of the entire loop.
                        if (listClockEventsBreak[b].TimeDisplayed2.TimeOfDay - (timeSpanAMBreakTimeCounter - TimeSpan.FromMinutes(30)) > timeSpanRate2AMRule) continue; //entirety of break overage occurred after Rate2 AM rule time.
                        //Make adjustments because: 30+ minutes of break, break occurred during clockEvent, break started before the AM rule.
                        var timeSpanAMAdjustAmount = TimeSpan.FromMinutes(30) - timeSpanAMBreakTimeCounter;
                        if (timeSpanAMAdjustAmount < -timeSpanAMBreakDuringRate2) timeSpanAMAdjustAmount = -timeSpanAMBreakDuringRate2; //cannot adjust off more break overage time than we have had breaks during this time.
                        timeSpanDailyRate2Total += timeSpanAMAdjustAmount; //adjust down
                        timeSpanAMBreakDuringRate2 += timeSpanAMAdjustAmount; //adjust down
                    }
                }

                //PM-------------------------------------
                if (listClockEvents[i].TimeDisplayed2.TimeOfDay > timeSpanRate2PMRule)
                {
                    //clocked out after Rate2 PM rule
                    timeSpanDailyRate2Total += listClockEvents[i].TimeDisplayed2.TimeOfDay - timeSpanRate2PMRule;
                    if (listClockEvents[i].TimeDisplayed1.TimeOfDay > timeSpanRate2PMRule) //clocked in after Rate2 PM rule also
                        timeSpanDailyRate2Total += timeSpanRate2PMRule - listClockEvents[i].TimeDisplayed1.TimeOfDay; //add a negative timespan
                    //Adjust Rate2 PM by overbreaks-----
                    var timeSpanPMBreakTimeCounter = new TimeSpan(); //tracks all break time for use in calculating overages.
                    var timeSpanPMBreakDuringRate2 = new TimeSpan(); //tracks only the portion of breaks that occurred during Rate2 hours.
                    for (var b = 0; b < listClockEventsBreak.Count; b++)
                    {
                        if (timeSpanPMBreakTimeCounter > TimeSpan.FromMinutes(30)) timeSpanPMBreakTimeCounter = TimeSpan.FromMinutes(30); //reset overages for next calculation.
                        if (listClockEventsBreak[b].TimeDisplayed1.Date != listClockEvents[i].TimeDisplayed1.Date) continue; //skip breaks for other days.
                        timeSpanPMBreakTimeCounter += listClockEventsBreak[b].TimeDisplayed2 - listClockEventsBreak[b].TimeDisplayed1;
                        timeSpanPMBreakDuringRate2 += CalcRate2Portion(TimeSpan.Zero, timeSpanRate2PMRule, listClockEventsBreak[b]);
                        if (timeSpanPMBreakTimeCounter < TimeSpan.FromMinutes(30)) continue; //not over thirty minutes yet.
                        if (!TimeClockEventsOverlapHelper(listClockEvents[i], listClockEventsBreak[b])) continue; //There must be multiple clock events for this day, and we have gone over breaks during a different clock event period
                        if (listClockEventsBreak[b].TimeDisplayed2.TimeOfDay < timeSpanRate2PMRule) continue; //entirety of break overage occurred before Rate2 PM time.
                        //Make adjustments because: 30+ minutes of break, break occurred during clockEvent, break ended after the PM rule.
                        var timeSpanPMAdjustAmount = TimeSpan.FromMinutes(30) - timeSpanPMBreakTimeCounter; //tsPMBreakTimeCounter is always > 30 at this point in time
                        if (timeSpanPMAdjustAmount < -timeSpanPMBreakDuringRate2) timeSpanPMAdjustAmount = -timeSpanPMBreakDuringRate2; //cannot adjust off more break overage time than we have had breaks during this time.
                        timeSpanDailyRate2Total += timeSpanPMAdjustAmount; //adjust down
                        timeSpanPMBreakDuringRate2 += timeSpanPMAdjustAmount; //adjust down
                    }
                }

                //Apply Rate2 to clock event-----------------------------------------------------------------------------------
                if (timeSpanDailyRate2Total < TimeSpan.Zero)
                    //this should never happen. If it ever does, we need to know about it, because that means some math has been miscalculated.
                    throw new Exception(" - " + listClockEvents[i].TimeDisplayed1.Date.ToShortDateString() + ", " + employee.FName + " " + employee.LName + " : calculated Rate2 hours was negative.");
                listClockEvents[i].Rate2Auto = timeSpanDailyRate2Total; //should be zero or greater.
                listClockEvents[i].Rate3Auto = new TimeSpan(0); //No ClockEvent can have both Rate2 and Rate3 hours. Zero out in case modifications were made.
            }

            #endregion

            #region Regular hours and OT hours calulations (including overbreak adjustments)----------------------------------------

            listClockEvents[i].OTimeAuto = TimeSpan.Zero;
            listClockEvents[i].AdjustAuto = TimeSpan.Zero;
            if (i == 0 || listClockEvents[i].TimeDisplayed1.Date != listClockEvents[i - 1].TimeDisplayed1.Date)
            {
                timeSpanHoursWorkedTotal = TimeSpan.Zero;
                timeSpanDailyBreaksAdjustTotal = TimeSpan.Zero;
                timeSpanDailyBreaksCalc = TimeSpan.Zero;
                timeSpanDailyBreaksTotal = TimeSpan.Zero;
                timeSpanDailyRate2Total = TimeSpan.Zero;
                timeSpanDailyRate3Total = TimeSpan.Zero;
            }

            timeSpanHoursWorkedTotal += listClockEvents[i].TimeDisplayed2 - listClockEvents[i].TimeDisplayed1; //Hours worked
            if (timeSpanHoursWorkedTotal > timeSpanOTHoursRule)
            {
                //if OverHoursPerDay then make AutoOTAdjustments.
                listClockEvents[i].OTimeAuto += timeSpanHoursWorkedTotal - timeSpanOTHoursRule; //++OTimeAuto
                //listClockEvent[i].AdjustAuto-=tsHoursWorkedTotal-tsOvertimeHoursRule;//--AdjustAuto
                timeSpanHoursWorkedTotal = timeSpanOTHoursRule; //subsequent clock events should be counted as overtime.
            }

            if (i == listClockEvents.Count - 1 || listClockEvents[i].TimeDisplayed1.Date != listClockEvents[i + 1].TimeDisplayed1.Date)
                //Either the last clock event in the list or last clock event for the day.
                //OVERBREAKS--------------------------------------------------------------------------------------------------------
                if (PrefC.GetBool(PrefName.TimeCardsMakesAdjustmentsForOverBreaks))
                {
                    //Apply overbreaks to this clockEvent.
                    timeSpanDailyBreaksAdjustTotal = new TimeSpan(); //used to adjust the clock event
                    timeSpanDailyBreaksTotal = new TimeSpan(); //used in calculating Daily Hours.
                    timeSpanDailyBreaksCalc = new TimeSpan(); //used in calculating breaks.
                    for (var b = 0; b < listClockEventsBreak.Count; b++)
                    {
                        //check all breaks for current day.
                        if (listClockEventsBreak[b].TimeDisplayed1.Date != listClockEvents[i].TimeDisplayed1.Date) continue; //skip breaks for other dates than current ClockEvent
                        timeSpanDailyBreaksCalc += listClockEventsBreak[b].TimeDisplayed2.TimeOfDay - listClockEventsBreak[b].TimeDisplayed1.TimeOfDay;
                        timeSpanDailyBreaksTotal += listClockEventsBreak[b].TimeDisplayed2.TimeOfDay - listClockEventsBreak[b].TimeDisplayed1.TimeOfDay;
                        timeSpanDailyHoursMinusBreaksTotal = timeSpanHoursWorkedTotal - timeSpanDailyBreaksTotal;
                        if (timeSpanDailyBreaksCalc > TimeSpan.FromMinutes(31))
                        {
                            //over 31 to avoid adjustments less than 1 minutes.
                            listClockEventsBreak[b].AdjustAuto = TimeSpan.FromMinutes(30) - timeSpanDailyBreaksCalc;
                            ClockEvents.Update(listClockEventsBreak[b]); //save adjustments to breaks.
                            timeSpanDailyBreaksAdjustTotal += listClockEventsBreak[b].AdjustAuto;
                            timeSpanDailyBreaksCalc = TimeSpan.FromMinutes(30); //reset daily breaks to 30 minutes so the next break is all adjustment.
                        } //end overBreaks>31 minutes
                        else
                        {
                            //If the adjustment is 30 minutes or less, the adjustment amount should be set to 0 
                            listClockEventsBreak[b].AdjustAuto = TimeSpan.Zero;
                            ClockEvents.Update(listClockEventsBreak[b]);
                        }
                    } //end checking all breaks for current day

                    //OverBreaks applies to overtime and then to RegularTime
                    listClockEvents[i].OTimeAuto += timeSpanDailyBreaksAdjustTotal; //tsDailyBreaksTotal<=TimeSpan.Zero
                    listClockEvents[i].AdjustAuto += timeSpanDailyBreaksAdjustTotal; //tsDailyBreaksTotal is less than or equal to zero
                    if (listClockEvents[i].OTimeAuto < TimeSpan.Zero)
                        //we have adjusted OT too far
                        //listClockEvent[i].AdjustAuto+=listClockEvent[i].OTimeAuto;
                        listClockEvents[i].OTimeAuto = TimeSpan.Zero;
                    timeSpanDailyBreaksTotal = TimeSpan.Zero; //zero out for the next day.
                    timeSpanHoursWorkedTotal = TimeSpan.Zero; //zero out for next day.
                } //end overbreaks

            #endregion

            ClockEvents.Update(listClockEvents[i]);
        } //end clockEvent loop.
    }

    private static TimeSpan CalcRate2Portion(TimeSpan timeSpanRate2AMRule, TimeSpan timeSpanRate2PMRule, ClockEvent clockEventBreak)
    {
        var timeSpan = new TimeSpan();
        //AM overlap==========================================================
        //Visual representation
        //AM Rule      :           X
        //Entire Break :o-------o  |             Stop-Start == Entire Break
        //Partial Break:      o----|---o         Rule-Start == Partial Break
        //No Break     :           |  o------o   Rule-Rule  == No break (won't actually happen in this block)
        timeSpan += TimeSpan.FromTicks(
            Math.Min(clockEventBreak.TimeDisplayed2.TimeOfDay.Ticks, timeSpanRate2AMRule.Ticks) //min of stop or rule
            - Math.Min(clockEventBreak.TimeDisplayed1.TimeOfDay.Ticks, timeSpanRate2AMRule.Ticks) //min of start or rule
        ); //equals the entire break, part of the break, or non of the break.
        //PM overlap==========================================================
        //Visual representation
        //PM Rule      :           X
        //Entire Break :o-------o  |             Rule-Rule   == No Break
        //Partial Break:      o----|---o         Stop-Rule   == Partial Break
        //No Break     :           |  o------o   Stop-Start  == Entire break
        timeSpan += TimeSpan.FromTicks(
            Math.Max(clockEventBreak.TimeDisplayed2.TimeOfDay.Ticks, timeSpanRate2PMRule.Ticks) //max of stop or rule
            - Math.Max(clockEventBreak.TimeDisplayed1.TimeOfDay.Ticks, timeSpanRate2PMRule.Ticks) //max of start or rule
        ); //equals the entire break, part of the break, or non of the break.
        return timeSpan;
    }

    /// <summary>
    ///     Returns true if two clock events overlap. Useful for determining if a break applies to a given clock event.
    ///     Does not matter which order clock events are provided.
    /// </summary>
    private static bool TimeClockEventsOverlapHelper(ClockEvent clockEvent1, ClockEvent clockEvent2)
    {
        //Visual representation
        //ClockEvent1:            o----------------o
        //ClockEvent2:o---------------o   or  o-------------------o
        if (clockEvent2.TimeDisplayed2 > clockEvent1.TimeDisplayed1
            && clockEvent2.TimeDisplayed1 < clockEvent1.TimeDisplayed2)
            return true;
        return false;
    }

    /// <summary>
    ///     Calculates weekly overtime and inserts TimeAdjusts accordingly. Throws an exception if there are any time card
    ///     events with errors.
    /// </summary>
    public static void CalculateWeeklyOvertime(Employee employee, PayPeriod payPeriod)
    {
        var timeCardRule = GetTimeCardRule(employee);
        if (timeCardRule != null && timeCardRule.IsOvertimeExempt) return;
        if (payPeriod.DateStart.Year < 1880) return; //Invalid pay period start date; Don't attempt to calculate overtime.
        //Manipulate the start date of the pay period so that the entire week is included.
        //This will cause clock events and time adjusts from a previous pay period to be included.
        //Including previous events is necessary for correctly calculating overtime, a weekly metric.
        var dateTimeStart = TimeCardL.GetStartOfWeek(payPeriod.DateStart);
        //All clock events and time adjustments made in an 'incomplete week' should always be paid 'straight time' even when there is more than 40 hours.
        //Overtime ratios are impossible to know until the week has officially completed.
        //Therefore, we have to pay 'straight time' for an 'incomplete week' this pay period and make necessary overtime adjustments in the next pay period.
        var dateTimeStop = TimeCardL.GetEndOfWeekForOvertime(payPeriod.DateStop);

        #region Get ClockEvents and TimeAdjusts

        var listClockEvents = new List<ClockEvent>();
        var listTimeAdjusts = new List<TimeAdjust>();
        var errors = "";
        try
        {
            listClockEvents = ClockEvents.GetValidList(employee.EmployeeNum, dateTimeStart, dateTimeStop, false);
        }
        catch (Exception ex)
        {
            errors += ex.Message;
        }

        try
        {
            listTimeAdjusts = TimeAdjusts.GetValidList(employee.EmployeeNum, dateTimeStart, dateTimeStop);
            //Do not consider protected leave as part of hours worked for OT calculations.
            listTimeAdjusts.RemoveAll(x => x.IsUnpaidProtectedLeave);
        }
        catch (Exception ex)
        {
            errors += ex.Message;
        }

        if (errors != "")
        {
            var message = Lans.g("TimeCardRules", "has the following errors:");
            throw new Exception(Employees.GetNameFL(employee) + " " + message + "\r\n" + errors);
        }

        #endregion Get ClockEvents and TimeAdjusts

        #region Delete Automatic TimeAdjusts

        //Delete all adjustments that were automatically made.
        var listTimeAdjustNumsAuto = listTimeAdjusts.FindAll(x => x.IsAuto).Select(x => x.TimeAdjustNum).ToList();
        if (listTimeAdjustNumsAuto.Count > 0)
        {
            TimeAdjusts.DeleteMany(listTimeAdjustNumsAuto);
            var logText = Lans.g("TimeCardRules", "Weekly overtime was calculated. Time Card Adjustments deleted for Employee:") + " " + Employees.GetNameFL(employee);
            SecurityLogs.MakeLogEntry(EnumPermType.TimeAdjustEdit, 0, logText);
            //Remove the TimeAdjusts that were just deleted from the list of all TimeAdjusts.
            listTimeAdjusts.RemoveAll(x => listTimeAdjustNumsAuto.Contains(x.TimeAdjustNum));
        }

        #endregion Delete Automatic TimeAdjusts

        var listTimeCardWeeks = TimeCardL.GetTimeCardWeeks(listClockEvents, listTimeAdjusts);
        var hasCreatedTimeAdjust = false;
        //Loop through every week and create a singular TimeAdjust entry per clinic to account for any overtime detected.
        for (var i = 0; i < listTimeCardWeeks.Count; i++)
        {
            //Skip this week if there is no overtime to consider.
            var totalHoursForWeek = listTimeCardWeeks[i].ListTimeCardObjects.Sum(x => x.GetTimeSpanTotal().TotalHours);
            if (totalHoursForWeek <= 40) continue;

            #region Validation

            //Preserve old behavior by blocking users from calculating overtime when there is a negative number of hours for a clinic.
            var listClinicNumTotalHours = listTimeCardWeeks[i].ListTimeCardObjects.GroupBy(x => x.ClinicNum)
                .Select(x => new ClinicNumTotalHours(x.Key, x.Sum(y => y.GetTimeSpanTotal().TotalHours)))
                .ToList();
            if (listClinicNumTotalHours.Any(x => x.TotalHours < 0)) throw new ApplicationException(Lans.g("TimeCardRules", "Clock events for employee total a negative number of hours for a clinic."));

            #endregion Validation

            #region Calculate Weekly Overtime

            var timeSpanWeeklyHours = TimeSpan.Zero;
            var listClinicNumTimeSpansOvertime = new List<ClinicNumTimeSpan>();
            for (var j = 0; j < listTimeCardWeeks[i].ListTimeCardObjects.Count; j++)
            {
                //Take a snapshot of the current weekly hours total so that we can figure out how many overtime hours this specific time span adds.
                var timeSpanPrevTotal = TimeSpan.FromHours(timeSpanWeeklyHours.TotalHours);
                //Blindly add the time span to the running weekly total.
                timeSpanWeeklyHours = timeSpanWeeklyHours.Add(listTimeCardWeeks[i].ListTimeCardObjects[j].GetTimeSpanTotal());
                //Check to see if this time span caused this week to enter overtime.
                if (timeSpanWeeklyHours.TotalHours > 40)
                {
                    //Calculate the amount of overtime that this time span added by utilizing the previous time span total (or 40 hours if this is the first OT time span).
                    var timeSpan = TimeSpan.FromHours(timeSpanWeeklyHours.TotalHours - Math.Max(40, timeSpanPrevTotal.TotalHours));
                    if (timeSpan > TimeSpan.Zero) listClinicNumTimeSpansOvertime.Add(new ClinicNumTimeSpan(listTimeCardWeeks[i].ListTimeCardObjects[j].ClinicNum, timeSpan));
                }
            }

            #endregion Calculate Weekly Overtime

            #region Offset Overtime

            //Offset the overtime value of TimeCardObject events from the clinic time span objects LIFO style.
            var listClinicNumTimeSpansAutoOvertime = listTimeCardWeeks[i].ListTimeCardObjects
                .Select(x => new ClinicNumTimeSpan(x.ClinicNum, x.GetTimeSpanOvertime()))
                .ToList();
            for (var j = 0; j < listClinicNumTimeSpansAutoOvertime.Count; j++)
            {
                if (listClinicNumTimeSpansAutoOvertime[j].TimeSpan <= TimeSpan.Zero) continue;
                //Loop through the overtime entries backwards and only offset overtime for entries from the same clinic.
                for (var k = listClinicNumTimeSpansOvertime.Count - 1; k >= 0; k--)
                {
                    if (listClinicNumTimeSpansAutoOvertime[j].TimeSpan <= TimeSpan.Zero) break;
                    if (listClinicNumTimeSpansOvertime[k].ClinicNum != listTimeCardWeeks[i].ListTimeCardObjects[j].ClinicNum) continue;
                    if (listClinicNumTimeSpansOvertime[k].TimeSpan <= TimeSpan.Zero) continue;
                    var ticksToSubtract = Math.Min(listClinicNumTimeSpansOvertime[k].TimeSpan.Ticks, listClinicNumTimeSpansAutoOvertime[j].TimeSpan.Ticks);
                    var timeSpanToSubtract = new TimeSpan(ticksToSubtract);
                    listClinicNumTimeSpansOvertime[k].TimeSpan = listClinicNumTimeSpansOvertime[k].TimeSpan.Subtract(timeSpanToSubtract);
                }

                //Loop through the overtime entries backwards again but this time offset overtime for all entries, ignoring clinics.
                for (var k = listClinicNumTimeSpansOvertime.Count - 1; k >= 0; k--)
                {
                    if (listClinicNumTimeSpansAutoOvertime[j].TimeSpan <= TimeSpan.Zero) break;
                    if (listClinicNumTimeSpansOvertime[k].TimeSpan <= TimeSpan.Zero) continue;
                    var ticksToSubtract = Math.Min(listClinicNumTimeSpansOvertime[k].TimeSpan.Ticks, listClinicNumTimeSpansAutoOvertime[j].TimeSpan.Ticks);
                    var timeSpanToSubtract = new TimeSpan(ticksToSubtract);
                    listClinicNumTimeSpansOvertime[k].TimeSpan = listClinicNumTimeSpansOvertime[k].TimeSpan.Subtract(timeSpanToSubtract);
                }
            }

            #endregion Offset Overtime

            #region Insert TimeAdjusts

            var listClinicNumTotalHoursOvertime = listClinicNumTimeSpansOvertime.GroupBy(x => x.ClinicNum)
                .Select(x => new ClinicNumTotalHours(x.Key, x.Sum(y => y.TimeSpan.TotalHours)))
                .ToList();
            for (var j = 0; j < listClinicNumTotalHoursOvertime.Count; j++)
            {
                if (listClinicNumTotalHoursOvertime[j].TotalHours <= 0) continue;
                var timeAdjust = new TimeAdjust();
                timeAdjust.IsAuto = true;
                timeAdjust.EmployeeNum = employee.EmployeeNum;
                timeAdjust.TimeEntry = listTimeCardWeeks[i].ListTimeCardObjects.Last().TimeEntry.Date.AddHours(20);
                timeAdjust.OTimeHours = TimeSpan.FromHours(listClinicNumTotalHoursOvertime[j].TotalHours);
                timeAdjust.RegHours = -timeAdjust.OTimeHours;
                timeAdjust.ClinicNum = listClinicNumTotalHoursOvertime[j].ClinicNum;
                timeAdjust.IsUnpaidProtectedLeave = false;
                timeAdjust.SecuUserNumEntry = Security.CurUser.UserNum;
                TimeAdjusts.Insert(timeAdjust);
                hasCreatedTimeAdjust = true;
            }

            #endregion Insert TimeAdjusts
        }

        if (hasCreatedTimeAdjust)
        {
            var logText = Lans.g("TimeCardRules", "Weekly overtime was calculated. Time Card Adjustment created for Employee:") + " " + Employees.GetNameFL(employee);
            SecurityLogs.MakeLogEntry(EnumPermType.TimeAdjustEdit, 0, logText);
        }
    }

    private class ClinicNumTotalHours
    {
        public readonly long ClinicNum;
        public readonly double TotalHours;

        public ClinicNumTotalHours(long clinicNum, double totalHours)
        {
            ClinicNum = clinicNum;
            TotalHours = totalHours;
        }
    }

    private class ClinicNumTimeSpan
    {
        public readonly long ClinicNum;
        public TimeSpan TimeSpan;

        public ClinicNumTimeSpan(long clinicNum, TimeSpan timeSpan)
        {
            ClinicNum = clinicNum;
            TimeSpan = timeSpan;
        }
    }

    #region CachePattern

    private class TimeCardRuleCache : CacheListAbs<TimeCardRule>
    {
        protected override List<TimeCardRule> GetCacheFromDb()
        {
            var command = "SELECT * FROM timecardrule";
            return TimeCardRuleCrud.SelectMany(command);
        }

        protected override List<TimeCardRule> TableToList(DataTable dataTable)
        {
            return TimeCardRuleCrud.TableToList(dataTable);
        }

        protected override TimeCardRule Copy(TimeCardRule item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<TimeCardRule> items)
        {
            return TimeCardRuleCrud.ListToTable(items, "TimeCardRule");
        }

        protected override void FillCacheIfNeeded()
        {
            TimeCardRules.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly TimeCardRuleCache _timeCardRuleCache = new();

    public static List<TimeCardRule> GetDeepCopy(bool isShort = false)
    {
        return _timeCardRuleCache.GetDeepCopy(isShort);
    }

    public static List<TimeCardRule> GetWhere(Predicate<TimeCardRule> match, bool isShort = false)
    {
        return _timeCardRuleCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _timeCardRuleCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return _timeCardRuleCache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        _timeCardRuleCache.ClearCache();
    }

    #endregion
}