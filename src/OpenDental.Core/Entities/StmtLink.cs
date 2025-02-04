using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class StmtLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long StmtLinkNum;

    ///<summary>FK to statement.StatementNum.</summary>
    public long StatementNum;

    ///<summary>Enum:StmtLinkTypes Represents what object FKey corresponds to.</summary>
    public StmtLinkTypes StmtLinkType;

    ///<summary>FK to type of PK of another object depending on StmtLinkType value. E.g. procedurelog.ProcNum, paysplit.PaySplitNum, adjustment.AdjNum, etc.</summary>
    public long FKey;
}

public enum StmtLinkTypes
{
    Proc,
    PaySplit,
    Adj,
    ClaimPay,
    PayPlanCharge,
    PatNum
}