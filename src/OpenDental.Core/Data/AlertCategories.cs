using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class AlertCategories
{
    public static long Insert(AlertCategory alertCategory)
    {
        return AlertCategoryCrud.Insert(alertCategory);
    }

    public static void Update(AlertCategory alertCategory)
    {
        AlertCategoryCrud.Update(alertCategory);
    }

    public static void Delete(long alertCategoryNum)
    {
        AlertCategoryCrud.Delete(alertCategoryNum);
    }

    private class AlertCategoryCache : CacheListAbs<AlertCategory>
    {
        protected override List<AlertCategory> GetCacheFromDb()
        {
            return AlertCategoryCrud.SelectMany("SELECT * FROM alertcategory");
        }

        protected override List<AlertCategory> TableToList(DataTable dataTable)
        {
            return AlertCategoryCrud.TableToList(dataTable);
        }

        protected override AlertCategory Copy(AlertCategory item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<AlertCategory> items)
        {
            return AlertCategoryCrud.ListToTable(items, "AlertCategory");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly AlertCategoryCache Cache = new();

    public static List<AlertCategory> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
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