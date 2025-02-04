using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class Cdcrecs
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
}