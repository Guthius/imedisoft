using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProviderIdent : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProviderIdentNum;

    ///<summary>FK to provider.ProvNum.  An ID only applies to one provider.</summary>
    public long ProvNum;

    ///<summary>FK to carrier.ElectID  aka Electronic ID. An ID only applies to one insurance carrier.</summary>
    public string PayorID;

    public ProviderSupplementalID SuppIDType;

    ///<summary>The number assigned by the ins carrier.</summary>
    public string IDNumber;

    public ProviderIdent Copy()
    {
        return (ProviderIdent) MemberwiseClone();
    }
}