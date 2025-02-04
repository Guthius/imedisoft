using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Reconcile : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ReconcileNum;

    ///<summary>FK to account.AccountNum</summary>
    public long AccountNum;

    ///<summary>User enters starting balance here.</summary>
    public double StartingBal;

    ///<summary>User enters ending balance here.</summary>
    public double EndingBal;

    ///<summary>The date that the reconcile was performed.</summary>
    public DateTime DateReconcile;

    ///<summary>If StartingBal + sum of entries selected = EndingBal, then user can lock.  Unlock requires special permission, which nobody will have by default.</summary>
    public bool IsLocked;
}