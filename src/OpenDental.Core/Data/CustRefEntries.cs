using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class CustRefEntries
{
    public static void Insert(CustRefEntry custRefEntry)
    {
        CustRefEntryCrud.Insert(custRefEntry);
    }

    public static void Update(CustRefEntry custRefEntry)
    {
        CustRefEntryCrud.Update(custRefEntry);
    }

    public static void Delete(long custRefEntryNum)
    {
        Db.NonQ("DELETE FROM custrefentry WHERE CustRefEntryNum = " + custRefEntryNum);
    }

    public static List<CustRefEntry> GetEntryListForCustomer(long patNumCust)
    {
        return CustRefEntryCrud.SelectMany("SELECT * FROM custrefentry WHERE PatNumCust=" + patNumCust + " OR PatNumRef=" + patNumCust);
    }

    public static List<CustRefEntry> GetEntryListForReference(long patNumRef)
    {
        return CustRefEntryCrud.SelectMany("SELECT * FROM custrefentry WHERE PatNumRef=" + patNumRef);
    }
}