using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AutoCodeCond : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutoCodeCondNum;

    ///<summary>FK to autocodeitem.AutoCodeItemNum.</summary>
    public long AutoCodeItemNum;

    public AutoCondition Cond;

    public AutoCodeCond Copy()
    {
        return (AutoCodeCond) MemberwiseClone();
    }
}