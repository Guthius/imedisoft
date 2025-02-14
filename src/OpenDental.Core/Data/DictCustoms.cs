using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class DictCustoms
{
    public static void Insert(DictCustom dictCustom)
    {
        DictCustomCrud.Insert(dictCustom);
    }

    public static void Update(DictCustom dictCustom)
    {
        DictCustomCrud.Update(dictCustom);
    }

    public static void Delete(long dictCustomNum)
    {
        DictCustomCrud.Delete(dictCustomNum);
    }

    private class DictCustomCache : CacheListAbs<DictCustom>
    {
        protected override List<DictCustom> GetCacheFromDb()
        {
            return DictCustomCrud.SelectMany("SELECT * FROM dictcustom ORDER BY WordText");
        }

        protected override List<DictCustom> TableToList(DataTable dataTable)
        {
            return DictCustomCrud.TableToList(dataTable);
        }

        protected override DictCustom Copy(DictCustom item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DictCustom> items)
        {
            return DictCustomCrud.ListToTable(items, "DictCustom");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly DictCustomCache Cache = new();

    public static List<DictCustom> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static DictCustom GetFirstOrDefault(Func<DictCustom, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}