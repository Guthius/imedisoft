using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcButtonItems
{
    public static void Insert(ProcButtonItem item)
    {
        ProcButtonItemCrud.Insert(item);
    }

    public static List<long> GetCodeNumListForButton(long procButtonNum)
    {
        return GetWhere(x => x.ProcButtonNum == procButtonNum && x.CodeNum > 0)
            .OrderBy(x => x.ItemOrder)
            .Select(x => x.CodeNum)
            .ToList();
    }

    public static List<long> GetAutoListForButton(long procButtonNum)
    {
        return GetWhere(x => x.ProcButtonNum == procButtonNum && x.AutoCodeNum > 0)
            .OrderBy(x => x.ItemOrder)
            .Select(x => x.AutoCodeNum)
            .ToList();
    }

    public static void DeleteAllForButton(long procButtonNum)
    {
        Db.NonQ("DELETE from procbuttonitem WHERE procbuttonnum = " + procButtonNum);
    }

    private class ProcButtonItemCache : CacheListAbs<ProcButtonItem>
    {
        protected override List<ProcButtonItem> GetCacheFromDb()
        {
            return ProcButtonItemCrud.SelectMany("SELECT * FROM procbuttonitem ORDER BY ItemOrder");
        }

        protected override List<ProcButtonItem> TableToList(DataTable dataTable)
        {
            return ProcButtonItemCrud.TableToList(dataTable);
        }

        protected override ProcButtonItem Copy(ProcButtonItem item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProcButtonItem> items)
        {
            return ProcButtonItemCrud.ListToTable(items, "ProcButtonItem");
        }

        protected override void FillCacheIfNeeded()
        {
            ProcButtonItems.GetTableFromCache(false);
        }
    }

    private static readonly ProcButtonItemCache Cache = new();

    public static List<ProcButtonItem> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    private static List<ProcButtonItem> GetWhere(Predicate<ProcButtonItem> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}