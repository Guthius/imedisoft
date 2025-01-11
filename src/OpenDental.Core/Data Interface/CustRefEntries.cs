using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CustRefEntries
{
    
    public static long Insert(CustRefEntry custRefEntry)
    {
        return CustRefEntryCrud.Insert(custRefEntry);
    }

    
    public static void Update(CustRefEntry custRefEntry)
    {
        CustRefEntryCrud.Update(custRefEntry);
    }

    
    public static void Delete(long custRefEntryNum)
    {
        var command = "DELETE FROM custrefentry WHERE CustRefEntryNum = " + SOut.Long(custRefEntryNum);
        Db.NonQ(command);
    }

    ///<summary>Gets all the entries for the customer.</summary>
    public static List<CustRefEntry> GetEntryListForCustomer(long patNumCust)
    {
        var command = "SELECT * FROM custrefentry WHERE PatNumCust=" + SOut.Long(patNumCust) + " OR PatNumRef=" + SOut.Long(patNumCust);
        return CustRefEntryCrud.SelectMany(command);
    }

    ///<summary>Gets all the entries for the reference.</summary>
    public static List<CustRefEntry> GetEntryListForReference(long patNumRef)
    {
        var command = "SELECT * FROM custrefentry WHERE PatNumRef=" + SOut.Long(patNumRef);
        return CustRefEntryCrud.SelectMany(command);
    }
}