using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProcMultiVisit : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProcMultiVisitNum;

    ///<summary>FK to procmultivisit.ProcMultiVisitNum.  Groups procmultivisit rows.  Set to the ProcMultiVisitNum of the first row in the group.</summary>
    public long GroupProcMultiVisitNum;

    ///<summary>FK to procedurelog.ProcNum.</summary>
    public long ProcNum;

    ///<summary>Enum:ProcStat A copy of the value from procedurelog.ProcStatus, based on ProcNum.  Reduces queries and speeds up logic.</summary>
    public ProcStat ProcStatus;

    ///<summary>A pseudo-status, calculated for the entire group and based on ProcStatuses of all procedures in the group. This will be true for all rows in an In Process group.</summary>
    public bool IsInProcess;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual date of entry.</summary>
    public DateTime SecDateTEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program
    ///updates.  Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    public ProcMultiVisit Copy()
    {
        return (ProcMultiVisit) MemberwiseClone();
    }
}