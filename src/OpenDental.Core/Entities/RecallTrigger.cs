using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class RecallTrigger : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long RecallTriggerNum;

    ///<summary>FK to recalltype.RecallTypeNum</summary>
    public long RecallTypeNum;

    ///<summary>FK to procedurecode.CodeNum</summary>
    public long CodeNum;

    public RecallTrigger Copy()
    {
        return (RecallTrigger) MemberwiseClone();
    }
}