using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SupplyNeededs
{
    ///<summary>Gets all SupplyNeededs.</summary>
    public static List<SupplyNeeded> CreateObjects()
    {
        var command = "SELECT * FROM supplyneeded ORDER BY DateAdded";
        return SupplyNeededCrud.SelectMany(command);
    }

    
    public static long Insert(SupplyNeeded supplyNeeded)
    {
        return SupplyNeededCrud.Insert(supplyNeeded);
    }

    
    public static void Update(SupplyNeeded supplyNeeded)
    {
        SupplyNeededCrud.Update(supplyNeeded);
    }

    
    public static void DeleteObject(SupplyNeeded supplyNeeded)
    {
        SupplyNeededCrud.Delete(supplyNeeded.SupplyNeededNum);
    }
}