using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoChartTabLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoChartTabLinkNum;

    public int ItemOrder;

    ///<summary>FK to orthocharttab.OrthoChartTabNum.</summary>
    public long OrthoChartTabNum;

    ///<summary>FK to displayfield.DisplayFieldNum.</summary>
    public long DisplayFieldNum;

    ///<summary>Overrides the DisplayField.ColumnWidth for OrthChartTabLinks when not 0. Otherwise uses associated DisplayFieldFieldNums DisplayField.ColumnWidth value.</summary>
    public int ColumnWidthOverride;

    public OrthoChartTabLink Copy()
    {
        return (OrthoChartTabLink) MemberwiseClone();
    }
}