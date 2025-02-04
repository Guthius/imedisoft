using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class OrthoChartTabs
{
    public static void Sync(List<OrthoChartTab> listOrthoChartTabsNew, List<OrthoChartTab> listOrthoChartTabsOld)
    {
        OrthoChartTabCrud.Sync(listOrthoChartTabsNew, listOrthoChartTabsOld);
    }

    private class OrthoChartTabCache : CacheListAbs<OrthoChartTab>
    {
        protected override List<OrthoChartTab> GetCacheFromDb()
        {
            return OrthoChartTabCrud.SelectMany("SELECT * FROM orthocharttab ORDER BY ItemOrder");
        }

        protected override List<OrthoChartTab> TableToList(DataTable dataTable)
        {
            return OrthoChartTabCrud.TableToList(dataTable);
        }

        protected override OrthoChartTab Copy(OrthoChartTab item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<OrthoChartTab> items)
        {
            return OrthoChartTabCrud.ListToTable(items, "OrthoChartTab");
        }

        protected override void FillCacheIfNeeded()
        {
            OrthoChartTabs.GetTableFromCache(false);
        }

        protected override bool IsInListShort(OrthoChartTab item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly OrthoChartTabCache Cache = new();

    public static List<OrthoChartTab> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static OrthoChartTab GetFirst(bool shortList = false)
    {
        return Cache.GetFirst(shortList);
    }

    public static int GetCount(bool shortList = false)
    {
        return Cache.GetCount(shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}