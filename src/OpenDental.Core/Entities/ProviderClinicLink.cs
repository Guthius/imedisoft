using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProviderClinicLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProviderClinicLinkNum;

    ///<summary>FK to provider.ProvNum</summary>
    public long ProvNum;

    ///<summary>FK to clinic.ClinicNum. An entry of -1 means the provider is associated to no clinics.</summary>
    public long ClinicNum;

    public ProviderClinicLink()
    {
    }

    public ProviderClinicLink(long clinicNum, long provNum)
    {
        ProvNum = provNum;
        ClinicNum = clinicNum;
    }


    public ProviderClinicLink Copy()
    {
        return (ProviderClinicLink) MemberwiseClone();
    }
}