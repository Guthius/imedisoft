using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CovCat : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CovCatNum;

    public string Description;

    ///<summary>Default percent for this category. -1 to skip this category and not apply a percentage.</summary>
    public int DefaultPercent;

    ///<summary>The order in which the categories are displayed.  Includes hidden categories. 0-based.</summary>
    public int CovOrder;

    public bool IsHidden;

    ///<summary>Enum:EbenefitCategory  The X12 benefit categories.  Each CovCat can link to one X12 category.  Default is 0 (unlinked).</summary>
    public EbenefitCategory EbenefitCat;

    public CovCat Copy()
    {
        return new CovCat
        {
            CovCatNum = CovCatNum,
            Description = Description,
            DefaultPercent = DefaultPercent,
            CovOrder = CovOrder,
            IsHidden = IsHidden,
            EbenefitCat = EbenefitCat
        };
    }
}