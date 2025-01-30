using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ERoutingDefLinks
{
    private class ERoutingDefLinkCache : CacheListAbs<ERoutingDefLink>
    {
        protected override List<ERoutingDefLink> GetCacheFromDb()
        {
            return ERoutingDefLinkCrud.SelectMany("SELECT * FROM eroutingdeflink");
        }

        protected override List<ERoutingDefLink> TableToList(DataTable dataTable)
        {
            return ERoutingDefLinkCrud.TableToList(dataTable);
        }

        protected override ERoutingDefLink Copy(ERoutingDefLink item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ERoutingDefLink> items)
        {
            return ERoutingDefLinkCrud.ListToTable(items, "ERoutingDefLink");
        }

        protected override void FillCacheIfNeeded()
        {
            ERoutingDefLinks.GetTableFromCache(false);
        }
    }
    
    private static readonly ERoutingDefLinkCache Cache = new();

    public static List<ERoutingDefLink> GetWhere(Predicate<ERoutingDefLink> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
    
    public static List<ERoutingDefLink> GetListERoutingTypesForERoutingDefNum(long eRoutingDefNum)
    {
        return ERoutingDefLinkCrud.SelectMany($"SELECT * FROM eroutingdeflink WHERE ERoutingDefNum = {eRoutingDefNum} GROUP BY ERoutingType");
    }

    public static void Insert(ERoutingDefLink eRoutingDefLink)
    {
        ERoutingDefLinkCrud.Insert(eRoutingDefLink);
    }

    public static void DeleteAll(long eRoutingDefNum)
    {
        Db.NonQ($"DELETE FROM eroutingdeflink WHERE ERoutingDefNum = {eRoutingDefNum}");
    }
}