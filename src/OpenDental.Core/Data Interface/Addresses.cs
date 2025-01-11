using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Addresses
{
    public static void Insert(Address address)
    {
        AddressCrud.Insert(address);
    }
    
    public static void Update(Address address)
    {
        AddressCrud.Update(address);
    }
    
    public static void Delete(long addressNum)
    {
        AddressCrud.Delete(addressNum);
    }
}