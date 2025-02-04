using System;
using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ApptView : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ApptViewNum;

    public string Description;
    public int ItemOrder;

    ///<summary>Number of rows per time increment.  Usually 1 or 2.  Programming note: Value updated to ApptDrawing.RowsPerIncr to track current state.</summary>
    public byte RowsPerIncr;

    ///<summary>If set to true, then the only operatories that will show will be for providers that have schedules for the day, ops with no provs assigned.</summary>
    public bool OnlyScheduledProvs;

    ///<summary>If OnlyScheduledProvs is set to true, and this time is not 0:00, then only provider schedules with start or stop time before this time will be included.</summary>
    public TimeSpan OnlySchedBeforeTime;

    ///<summary>If OnlyScheduledProvs is set to true, and this time is not 0:00, then only provider schedules with start or stop time after this time will be included.</summary>
    public TimeSpan OnlySchedAfterTime;

    ///<summary>Enum:ApptViewStackBehavior </summary>
    public ApptViewStackBehavior StackBehavUR;

    ///<summary>Enum:ApptViewStackBehavior </summary>
    public ApptViewStackBehavior StackBehavLR;

    ///<summary>FK to clinic.ClinicNum.  0=All clinics.  This appointment view will only be visible when the current clinic showing is set to this clinic.  Within the appointment edit window, this setting is used to filter the list of available operatories.  Also used in conjunction with 'OnlyScheduledProvs' (when enabled) in order to filter the visible operatories within the Appt module.</summary>
    public long ClinicNum;

    ///<summary>Time the appointment module's view will scroll to on load.</summary>
    public TimeSpan ApptTimeScrollStart;

    ///<summary>If set to true, the appointment view scrolls to the first scheduled operatory start time or the first scheduled appointment.</summary>
    public bool IsScrollStartDynamic;

    ///<summary>If set to true, the appointment view will not show appointment bubbles.</summary>
    public bool IsApptBubblesDisabled;

    ///<summary>Zero is default and old behavior.  For any larger number, appointments won't shrink past that number but they may be wider.  Instead of getting narrower, a horizontal scrollbar shows up.</summary>
    public int WidthOpMinimum;

    ///<summary>Enum:EnumWaitingRmName - Shows how patient name is displayed in the waiting room. Defaults to Last, First.</summary>
    public EnumWaitingRmName WaitingRmName;

    ///<summary>If set to true, then the only days that will show in Week view will be days that have providers scheduled or an appointment scheduled.</summary>
    public bool OnlyScheduledProvDays;

    public ApptView Copy()
    {
        return (ApptView) MemberwiseClone();
    }
}

///<summary>Vertical or Horizontal</summary>
public enum ApptViewStackBehavior
{
    Vertical,
    Horizontal
}

///<summary>How the patient's name is displayed in the waiting room in the appointment module.</summary>
public enum EnumWaitingRmName
{
    ///<summary>0</summary>
    [Description("Last, First")]
    LastFirst,

    ///<summary>1</summary>
    [Description("First, Last Initial")]
    FirstLastI,

    ///<summary>2</summary>
    [Description("First")]
    First
}