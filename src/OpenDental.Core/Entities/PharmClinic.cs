using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PharmClinic : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PharmClinicNum;

    ///<summary>FK to pharmacy.PharmacyNum.</summary>
    public long PharmacyNum;

    ///<summary>FK to clinic.ClinicNum.</summary>
    public long ClinicNum;

    public PharmClinic()
    {
    }

    public PharmClinic(long pharmacyNum, long clinicNum)
    {
        PharmacyNum = pharmacyNum;
        ClinicNum = clinicNum;
    }
}