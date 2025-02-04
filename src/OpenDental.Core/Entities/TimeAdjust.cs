using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TimeAdjust : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TimeAdjustNum;

    ///<summary>FK to employee.EmployeeNum</summary>
    public long EmployeeNum;

    ///<summary>The date and time that this entry will show on timecard.</summary>
    public DateTime TimeEntry;

    ///<summary>The number of regular hours to adjust timecard by.  Can be + or -.</summary>
    public TimeSpan RegHours;

    ///<summary>Overtime hours. Usually +.  Automatically combined with a - adj to RegHours.  Another option is clockevent.OTimeHours.</summary>
    public TimeSpan OTimeHours;

    public string Note;

    ///<summary>Set to true if this adjustment was automatically made by the system.  When the calc weekly OT tool is run, these types of adjustments are fair game for deletion.  Other adjustments are preserved.</summary>
    public bool IsAuto;

    ///<summary>FK to clinic.ClinicNum.  The clinic the TimeAdjust was entered at.</summary>
    public long ClinicNum;

    ///<summary>FK to definition.DefNum.  Defaults to 0.  Is set to 0 for general adjustments.
    ///When not 0, points to a definition in the TimeCardAdjTypes category.</summary>
    public long PtoDefNum;

    ///<summary>PTO Hours.  The number of PTO hours applied to a specific day.  Ignored if PtoDefNum is 0.</summary>
    public TimeSpan PtoHours;

    ///<summary>Defaults to false. True when this TimeAdjust is for unpaid protected leave. Hours from unpaid protected leave adjustments contribute to hours worked, but not to payable hours.</summary>
    public bool IsUnpaidProtectedLeave;

    ///<summary>FK to userod.UserNum. The user that created this TimeAdjust.</summary>
    public long SecuUserNumEntry;

    public TimeAdjust Copy()
    {
        return (TimeAdjust) MemberwiseClone();
    }
}