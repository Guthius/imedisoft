using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class EFormDefs
{
    public static long Insert(EFormDef eFormDef)
    {
        return EFormDefCrud.Insert(eFormDef);
    }

    public static void Update(EFormDef eFormDef)
    {
        EFormDefCrud.Update(eFormDef);
    }

    public static void Delete(long eFormDefNum)
    {
        EFormDefCrud.Delete(eFormDefNum);
    }

    private class EFormDefCache : CacheListAbs<EFormDef>
    {
        protected override List<EFormDef> GetCacheFromDb()
        {
            return EFormDefCrud.SelectMany("SELECT * FROM eformdef");
        }

        protected override List<EFormDef> TableToList(DataTable dataTable)
        {
            return EFormDefCrud.TableToList(dataTable);
        }

        protected override EFormDef Copy(EFormDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EFormDef> items)
        {
            return EFormDefCrud.ListToTable(items, "EFormDef");
        }

        protected override void FillCacheIfNeeded()
        {
            EFormDefs.GetTableFromCache(false);
        }
    }

    private static readonly EFormDefCache Cache = new();

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<EFormDef> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static EFormDef GetFirstOrDefault(Func<EFormDef, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }
}