using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ZipCode : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ZipCodeNum;

    ///<summary>The actual zipcode.</summary>
    public string ZipCodeDigits;
    
    public string City;
    public string State;

    ///<summary>If true, then it will show in the dropdown list in the patient edit window.</summary>
    public bool IsFrequent;

    public ZipCode Copy()
    {
        return (ZipCode) MemberwiseClone();
    }
}