using DataConnectionBase;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClinicPref : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClinicPrefNum;

    ///<summary>FK to clinic.ClinicNum.</summary>
    public long ClinicNum;

    public PrefName PrefName;

    ///<summary>The stored value.</summary>
    public string ValueString;

    public ClinicPref()
    {
    }

    public ClinicPref(long clinicNum, PrefName prefName, bool valueBool)
    {
        ClinicNum = clinicNum;
        PrefName = prefName;
        ValueString = SOut.Bool(valueBool);
    }

    public ClinicPref(long clinicNum, PrefName prefName, string valueString)
    {
        ClinicNum = clinicNum;
        PrefName = prefName;
        ValueString = valueString;
    }

    public ClinicPref Clone()
    {
        return (ClinicPref) MemberwiseClone();
    }
}