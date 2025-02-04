using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SubstitutionLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SubstitutionLinkNum;

    ///<summary>FK to insplan.PlanNum.</summary>
    public long PlanNum;

    ///<summary>FK to procedurecode.CodeNum.</summary>
    public long CodeNum;

    ///<summary>FK to procedurecode.ProcCode.</summary>
    public string SubstitutionCode;

    public SubstitutionCondition SubstOnlyIf;
}