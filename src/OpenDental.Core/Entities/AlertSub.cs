using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AlertSub : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AlertSubNum;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    ///<summary>FK to clinic.ClinicNum. Can be 0.</summary>
    public long ClinicNum;

    ///<summary>Deprecated.</summary>
    public AlertType Type;

    ///<summary>FK to alertcategory.AlertCategoryNum.</summary>
    public long AlertCategoryNum;

    public AlertSub()
    {
    }

    public AlertSub(long userNum, long clinicNum, long alertCatNum)
    {
        UserNum = userNum;
        ClinicNum = clinicNum;
        AlertCategoryNum = alertCatNum;
    }

    public AlertSub Copy()
    {
        return (AlertSub) MemberwiseClone();
    }
}