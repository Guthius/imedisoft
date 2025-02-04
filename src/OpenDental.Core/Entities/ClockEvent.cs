using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClockEvent : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClockEventNum;

    ///<summary>FK to employee.EmployeeNum</summary>
    public long EmployeeNum;

    ///<summary>The actual time that this entry was entered.  Cannot be 01-01-0001.</summary>
    public DateTime TimeEntered1;

    ///<summary>The time to display and to use in all calculations.  Cannot be 01-01-0001.</summary>
    public DateTime TimeDisplayed1;

    ///<summary>Enum:TimeClockStatus  Home, Lunch, or Break.  The status really only applies to the clock out.  Except the Break status applies to both out and in.</summary>
    public TimeClockStatus ClockStatus;

    public string Note;

    ///<summary>The user can never edit this, but the program has to be able to edit this when user clocks out.  Can be 01-01-0001 if not clocked out yet.</summary>
    public DateTime TimeEntered2;

    ///<summary>User can edit. Can be 01-01-0001 if not clocked out yet.</summary>
    public DateTime TimeDisplayed2;

    ///<summary>This is a manual override for OTimeAuto.  Typically -1 hour (-01:00:00) to indicate no override.  When used as override, allowed values are zero or positive.  This is an alternative to using a TimeAdjust row.</summary>
    public TimeSpan OTimeHours = TimeSpan.FromHours(-1);

    ///<summary>Automatically calculated OT.  Will be zero if none.</summary>
    public TimeSpan OTimeAuto;

    ///<summary>This is a manual override of AdjustAuto.  Ignored unless AdjustIsOverridden set to true.  When used as override, it's typically negative, although zero and positive are also allowed.</summary>
    public TimeSpan Adjust;

    ///<summary>Automatically calculated Adjust.  Will be zero if none.</summary>
    public TimeSpan AdjustAuto;

    ///<summary>True if AdjustAuto is overridden by Adjust.</summary>
    public bool AdjustIsOverridden;

    ///<summary>This is a manual override for Rate2Auto.  Typically -1 hour (-01:00:00) to indicate no override.  When used as override, allowed values are zero or positive.  This is the portion of the hours worked which are at Rate2, so it's not in addition to the hours worked.  Also used to calculate the Rate2 OT.</summary>
    public TimeSpan Rate2Hours = TimeSpan.FromHours(-1);

    ///<summary>Automatically calculated rate2 pay.  Will be zero if none.</summary>
    public TimeSpan Rate2Auto;

    ///<summary>FK to clinic.ClinicNum.  The clinic the ClockEvent was entered at.</summary>
    public long ClinicNum;

    ///<summary>This is a manual override for Rate3Auto.  Typically -1 hour (-01:00:00) to indicate no override.  When used as override, allowed values are zero or positive.  This is the portion of the hours worked which are at Rate3, so it's not in addition to the hours worked.  Also used to calculate the Rate3 OT.</summary>
    public TimeSpan Rate3Hours = TimeSpan.FromHours(-1);

    ///<summary>Automatically calculated Rate3 pay.  Will be zero if none.</summary>
    public TimeSpan Rate3Auto;

    ///<summary>True if the Clock Event is made by choosing "Available at Home" when clocking in. Will be false if "Available At Office" is selected instead.</summary>
    public bool IsWorkingHome;


    public ClockEvent Copy()
    {
        return (ClockEvent) MemberwiseClone();
    }
}