using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class UserClinic : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long UserClinicNum;

    ///<summary>FK to userod.UserNum</summary>
    public long UserNum;

    ///<summary>FK to clinic.ClinicNum</summary>
    public long ClinicNum;

    public UserClinic()
    {
    }

    public UserClinic(long clinicNum, long userNum)
    {
        UserNum = userNum;
        ClinicNum = clinicNum;
    }

    public UserClinic Copy()
    {
        return (UserClinic) MemberwiseClone();
    }
}