using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AutoCode : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutoCodeNum;

    public string Description;

    public bool IsHidden;

    /// <summary>
    /// This will be true if user no longer wants to see this autocode message when closing a procedure.
    /// This makes it less intrusive, but it can still be used in procedure buttons.
    /// </summary>
    public bool LessIntrusive;

    public AutoCode Copy()
    {
        return (AutoCode) MemberwiseClone();
    }
}