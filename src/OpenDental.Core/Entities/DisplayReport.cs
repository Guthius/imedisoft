using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DisplayReport : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DisplayReportNum;

    public string InternalName;
    public int ItemOrder;
    public string Description;
    public DisplayReportCategory Category;
    public bool IsHidden;

    ///<summary>When true and IsHidden is false, will show this report in a pop out sub menu.</summary>
    public bool IsVisibleInSubMenu;

    public DisplayReport Copy()
    {
        return (DisplayReport) MemberwiseClone();
    }
}

public enum DisplayReportCategory
{
    ProdInc,
    Daily,
    Monthly,
    Lists,
    PublicHealth,
    ArizonaPrimaryCare
}