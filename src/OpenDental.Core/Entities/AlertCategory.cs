using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AlertCategory : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AlertCategoryNum;

    ///<summary>False by default, indicates that this is a row that can not be edited or deleted.</summary>
    public bool IsHQCategory;

    public string InternalName;

    public string Description;
    
    public AlertCategory Copy()
    {
        return (AlertCategory) MemberwiseClone();
    }
}