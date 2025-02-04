using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ReferralClinicLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ReferralClinicLinkNum;

    ///<summary>FK to referral.ReferralNum.</summary>
    public long ReferralNum;

    ///<summary>FK to clinic.ClinicNum.</summary>
    public long ClinicNum;
}