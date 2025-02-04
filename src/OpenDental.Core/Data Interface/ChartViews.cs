using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ChartViews
{
    public static void Insert(ChartView chartView)
    {
        ChartViewCrud.Insert(chartView);
    }

    public static bool Update(ChartView chartView, ChartView chartViewOld = null)
    {
        if (chartViewOld is null)
        {
            ChartViewCrud.Update(chartView);
            return true;
        }

        return ChartViewCrud.Update(chartView, chartViewOld);
    }

    public static void Delete(long chartViewNum)
    {
        Db.NonQ("DELETE FROM chartview WHERE ChartViewNum = " + chartViewNum);
    }

    private class ChartViewCache : CacheListAbs<ChartView>
    {
        protected override List<ChartView> GetCacheFromDb()
        {
            var command = "SELECT * FROM chartview ORDER BY ItemOrder";
            return ChartViewCrud.SelectMany(command);
        }

        protected override List<ChartView> TableToList(DataTable dataTable)
        {
            return ChartViewCrud.TableToList(dataTable);
        }

        protected override ChartView Copy(ChartView item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ChartView> items)
        {
            return ChartViewCrud.ListToTable(items, "ChartView");
        }

        protected override void FillCacheIfNeeded()
        {
            ChartViews.GetTableFromCache(false);
        }
    }

    private static readonly ChartViewCache Cache = new();

    public static List<ChartView> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static ChartView GetFirst(bool isShort = false)
    {
        return Cache.GetFirst(isShort);
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