using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CovSpan : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CovSpanNum;

    ///<summary>FK to covcat.CovCatNum.</summary>
    public long CovCatNum;

    public string FromCode;
    public string ToCode;

    public CovSpan Copy()
    {
        return new CovSpan
        {
            CovSpanNum = CovSpanNum,
            CovCatNum = CovCatNum,
            FromCode = FromCode,
            ToCode = ToCode
        };
    }
}