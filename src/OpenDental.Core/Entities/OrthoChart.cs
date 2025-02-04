using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoChart : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoChartNum;

    ///<summary>Deprecated, FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>Deprecated, use orthochartrow table instead.  Date of service.</summary>
    public DateTime DateService;

    ///<summary>Keyed to displayfield.Description.</summary>
    public string FieldName;

    ///<summary>Stores the text that the user entered or picked.</summary>
    public string FieldValue;

    ///<summary>Deprecated, use orthochartrow table instead. FK to userod.UserNum.  The user that created or last edited an ortho chart field.</summary>
    public long UserNum;

    ///<summary>Deprecated, use orthochartrow table instead. FK to provider.ProvNum.  Can be 0.</summary>
    public long ProvNum;

    ///<summary>FK to orthochartrow.OrthoChartRowNum.</summary>
    public long OrthoChartRowNum;
}