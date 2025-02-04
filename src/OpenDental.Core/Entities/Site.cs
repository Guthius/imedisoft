using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Site : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SiteNum;

    public string Description;
    public string Note;
    public string Address;
    public string Address2;
    public string City;
    public string State;
    public string Zip;

    ///<summary>FK to provider.ProvNum.  Default provider for the site.</summary>
    public long ProvNum;

    ///<summary>Enum:PlaceOfService Describes where the site is located.</summary>
    public PlaceOfService PlaceService;

    public Site Copy()
    {
        return (Site) MemberwiseClone();
    }
}