using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Ebill : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EbillNum;

    ///<summary>FK to clinic.ClinicNum</summary>
    public long ClinicNum;

    ///<summary>The account number for the e-statement client.</summary>
    public string ClientAcctNumber;

    ///<summary>The user name for this particular account.</summary>
    public string ElectUserName;

    ///<summary>The password for this particular account.</summary>
    public string ElectPassword;

    ///<summary>Enum:EbillAddress </summary>
    public EbillAddress PracticeAddress;

    ///<summary>Enum:EbillAddress </summary>
    public EbillAddress RemitAddress;

    public Ebill Copy()
    {
        return (Ebill) MemberwiseClone();
    }
}

public enum EbillAddress
{
    PracticePhysical,
    PracticeBilling,
    PracticePayTo,
    ClinicPhysical,
    ClinicBilling,
    ClinicPayTo
}