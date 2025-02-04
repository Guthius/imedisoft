using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class PatFieldPickItems
{
    private class PatFieldPickItemCache : CacheListAbs<PatFieldPickItem>
    {
        protected override List<PatFieldPickItem> GetCacheFromDb()
        {
            return PatFieldPickItemCrud.SelectMany("SELECT * FROM patfieldpickitem");
        }

        protected override List<PatFieldPickItem> TableToList(DataTable dataTable)
        {
            return PatFieldPickItemCrud.TableToList(dataTable);
        }

        protected override PatFieldPickItem Copy(PatFieldPickItem item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<PatFieldPickItem> items)
        {
            return PatFieldPickItemCrud.ListToTable(items, "PatFieldPickItem");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly PatFieldPickItemCache Cache = new();

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<PatFieldPickItem> GetWhere(Predicate<PatFieldPickItem> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void Insert(PatFieldPickItem patFieldPickItem)
    {
        PatFieldPickItemCrud.Insert(patFieldPickItem);
    }

    public static void Update(PatFieldPickItem patFieldPickItem)
    {
        PatFieldPickItemCrud.Update(patFieldPickItem);
    }

    public static void Delete(long patFieldPickItemNum)
    {
        PatFieldPickItemCrud.Delete(patFieldPickItemNum);
    }
}