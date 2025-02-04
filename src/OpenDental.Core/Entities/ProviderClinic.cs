using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProviderClinic : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProviderClinicNum;

    ///<summary>FK to provider.ProvNum.</summary>
    public long ProvNum;

    ///<summary>FK to clinic.ClinicNum.</summary>
    public long ClinicNum;

    ///<summary>The DEA number for this provider and clinic.  The DEA number used to be stored in provider.DEANum.</summary>
    public string DEANum;

    ///<summary>License number corresponding to the StateWhereLicensed.  Can include punctuation</summary>
    public string StateLicense;

    ///<summary>Provider medical State ID.</summary>
    public string StateRxID;

    ///<summary>The state abbreviation where the state license number in the StateLicense field is legally registered.</summary>
    public string StateWhereLicensed;

    ///<summary>The merchant number for this provider and clinic.</summary>
    public string CareCreditMerchantId; // TODO: Remove

    public ProviderClinic Copy()
    {
        return (ProviderClinic) MemberwiseClone();
    }
}