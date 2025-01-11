using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AlertCategories
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
            var command = "SELECT * FROM alertcategory";
            return AlertCategoryCrud.SelectMany(command);
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
            AlertCategories.GetTableFromCache(false);
        }
    }

    private static readonly AlertCategoryCache Cache = new();

    public static List<AlertCategory> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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