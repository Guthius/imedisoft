using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoChartTab : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoChartTabNum;

    public string TabName;
    public int ItemOrder;
    public bool IsHidden;

    public OrthoChartTab Copy()
    {
        return (OrthoChartTab) MemberwiseClone();
    }
}