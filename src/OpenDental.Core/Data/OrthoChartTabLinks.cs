using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class OrthoChartTabLinks
{
    public static void Sync(List<OrthoChartTabLink> listOrthoChartTabLinksNew, List<OrthoChartTabLink> listOrthoChartTabLinksOld)
    {
        OrthoChartTabLinkCrud.Sync(listOrthoChartTabLinksNew, listOrthoChartTabLinksOld);
    }

    private class OrthoChartTabLinkCache : CacheListAbs<OrthoChartTabLink>
    {
        protected override List<OrthoChartTabLink> GetCacheFromDb()
        {
            return OrthoChartTabLinkCrud.SelectMany("SELECT * FROM orthocharttablink ORDER BY ItemOrder");
        }

        protected override List<OrthoChartTabLink> TableToList(DataTable dataTable)
        {
            return OrthoChartTabLinkCrud.TableToList(dataTable);
        }

        protected override OrthoChartTabLink Copy(OrthoChartTabLink item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<OrthoChartTabLink> items)
        {
            return OrthoChartTabLinkCrud.ListToTable(items, "OrthoChartTabLink");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly OrthoChartTabLinkCache Cache = new();

    public static bool GetExists(Predicate<OrthoChartTabLink> predicate, bool shortList = false)
    {
        return Cache.GetExists(predicate, shortList);
    }

    public static List<OrthoChartTabLink> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<OrthoChartTabLink> GetWhere(Predicate<OrthoChartTabLink> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
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