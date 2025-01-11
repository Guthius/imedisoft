using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Cdcrecs
{
    public static void Insert(Cdcrec cdcrec)
    {
        CdcrecCrud.Insert(cdcrec);
    }
    
    public static void Update(Cdcrec cdcrec)
    {
        CdcrecCrud.Update(cdcrec);
    }

    public static List<Cdcrec> GetAll()
    {
        return CdcrecCrud.SelectMany("SELECT * FROM cdcrec");
    }

    public static long GetCodeCount()
    {
        return SIn.Long(Db.GetCount("SELECT COUNT(*) FROM cdcrec"));
    }
}