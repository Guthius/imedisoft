using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ERoutingDefs
{
    private class ERoutingDefCache : CacheListAbs<ERoutingDef>
    {
        protected override List<ERoutingDef> GetCacheFromDb()
        {
            return ERoutingDefCrud.SelectMany("SELECT * FROM eroutingdef");
        }

        protected override List<ERoutingDef> TableToList(DataTable dataTable)
        {
            return ERoutingDefCrud.TableToList(dataTable);
        }

        protected override ERoutingDef Copy(ERoutingDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ERoutingDef> items)
        {
            return ERoutingDefCrud.ListToTable(items, "ERoutingDef");
        }

        protected override void FillCacheIfNeeded()
        {
            ERoutingDefs.GetTableFromCache(false);
        }
    }

    private static readonly ERoutingDefCache Cache = new();

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<ERoutingDef> GetByClinic(long clinicNum)
    {
        return ERoutingDefCrud.SelectMany("SELECT * FROM eroutingdef WHERE clinicNum = " + clinicNum);
    }
    
    public static long Insert(ERoutingDef eRoutingDef)
    {
        return ERoutingDefCrud.Insert(eRoutingDef);
    }
    
    public static void Update(ERoutingDef eRoutingDef)
    {
        ERoutingDefCrud.Update(eRoutingDef);
    }
    
    public static void Delete(long eRoutingDefNum)
    {
        ERoutingDefCrud.Delete(eRoutingDefNum);
    }
}