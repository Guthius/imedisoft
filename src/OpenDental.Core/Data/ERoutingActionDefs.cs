using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ERoutingActionDefs
{
    private class ERoutingActionDefCache : CacheListAbs<ERoutingActionDef>
    {
        protected override List<ERoutingActionDef> GetCacheFromDb()
        {
            return ERoutingActionDefCrud.SelectMany("SELECT * FROM eroutingactiondef");
        }

        protected override List<ERoutingActionDef> TableToList(DataTable dataTable)
        {
            return ERoutingActionDefCrud.TableToList(dataTable);
        }

        protected override ERoutingActionDef Copy(ERoutingActionDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ERoutingActionDef> items)
        {
            return ERoutingActionDefCrud.ListToTable(items, "ERoutingActionDef");
        }

        protected override void FillCacheIfNeeded()
        {
            ERoutingActionDefs.GetTableFromCache(false);
        }
    }

    private static readonly ERoutingActionDefCache Cache = new();

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<ERoutingActionDef> GetAllByERoutingDef(long eRoutingDefNum)
    {
        return ERoutingActionDefCrud.SelectMany($"SELECT * FROM eroutingactiondef WHERE eroutingdefnum = {eRoutingDefNum} ORDER BY ItemOrder");
    }

    public static void Insert(ERoutingActionDef eRoutingActionDef)
    {
        ERoutingActionDefCrud.Insert(eRoutingActionDef);
    }

    public static void Upsert(ERoutingActionDef eRoutingActionDef)
    {
        if (eRoutingActionDef.ERoutingActionDefNum == 0)
        {
            ERoutingActionDefCrud.Insert(eRoutingActionDef);
            return;
        }

        ERoutingActionDefCrud.Update(eRoutingActionDef);
    }

    public static void Delete(long eRoutingActionDefNum)
    {
        ERoutingActionDefCrud.Delete(eRoutingActionDefNum);
    }
}