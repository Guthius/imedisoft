using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AlertCategoryLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AlertCategoryLinkNum;

    ///<summary>FK to AlertCategory.AlertCategoryNum.</summary>
    public long AlertCategoryNum;

    public AlertType AlertType;

    public AlertCategoryLink()
    {
    }

    public AlertCategoryLink(long alertCategoryNum, AlertType alertType)
    {
        AlertCategoryNum = alertCategoryNum;
        AlertType = alertType;
    }

    public AlertCategoryLink Copy()
    {
        return (AlertCategoryLink) MemberwiseClone();
    }
}