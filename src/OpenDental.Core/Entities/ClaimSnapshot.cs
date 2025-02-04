using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClaimSnapshot : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClaimSnapshotNum;

    ///<summary>FK to procedurelog.ProcNum</summary>
    public long ProcNum;

    ///<summary>"P"=primary, "S"=secondary, "Other"=other, "Cap"=capitation. Never "PreAuth" as PreAuths will never be in this table</summary>
    public string ClaimType;

    public double Writeoff;

    ///<summary>Expected amount the insurance will pay on the procedure.</summary>
    public double InsPayEst;

    ///<summary>Procedure's ProcFee</summary>
    public double Fee;

    ///<summary>The date/time that the snapshot was created.  Not user editable.</summary>
    public DateTime DateTEntry;

    ///<summary>FK to claimproc.ClaimProcNum</summary>
    public long ClaimProcNum;

    ///<summary>Enum:ClaimSnapshotTrigger Stores the trigger to which this ClaimSnapshot was created.</summary>
    public ClaimSnapshotTrigger SnapshotTrigger;
}