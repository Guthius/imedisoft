using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DictCustom : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DictCustomNum;

    /// <summary>No space or punctuation allowed.</summary>
    public string WordText;

    public DictCustom Copy()
    {
        return (DictCustom) MemberwiseClone();
    }
}