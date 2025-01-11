using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AlertCategoryLinks
{
    public static List<AlertCategoryLink> GetForCategory(long alertCategoryNum)
    {
        if (alertCategoryNum == 0) return new List<AlertCategoryLink>();
        var command = "SELECT * FROM alertcategorylink WHERE AlertCategoryNum = " + SOut.Long(alertCategoryNum);
        return AlertCategoryLinkCrud.SelectMany(command);
    }
    
    public static void Insert(AlertCategoryLink alertCategoryLink)
    {
        AlertCategoryLinkCrud.Insert(alertCategoryLink);
    }
    
    public static void DeleteForCategory(long alertCategoryNum)
    {
        if (alertCategoryNum == 0) return;
        var command = "DELETE FROM alertcategorylink "
                      + "WHERE AlertCategoryNum = " + SOut.Long(alertCategoryNum);
        Db.NonQ(command);
    }

    public static void Sync(List<AlertCategoryLink> listAlertCategoryLinksNew, List<AlertCategoryLink> listAlertCategoryLinksOld)
    {
        AlertCategoryLinkCrud.Sync(listAlertCategoryLinksNew, listAlertCategoryLinksOld);
    }
    
    private class AlertCategoryLinkCache : CacheListAbs<AlertCategoryLink>
    {
        protected override List<AlertCategoryLink> GetCacheFromDb()
        {
            var command = "SELECT * FROM alertcategorylink";
            return AlertCategoryLinkCrud.SelectMany(command);
        }

        protected override List<AlertCategoryLink> TableToList(DataTable dataTable)
        {
            return AlertCategoryLinkCrud.TableToList(dataTable);
        }

        protected override AlertCategoryLink Copy(AlertCategoryLink item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<AlertCategoryLink> items)
        {
            return AlertCategoryLinkCrud.ListToTable(items, "AlertCategoryLink");
        }

        protected override void FillCacheIfNeeded()
        {
            AlertCategoryLinks.GetTableFromCache(false);
        }
    }

    private static readonly AlertCategoryLinkCache Cache = new();

    public static List<AlertCategoryLink> GetWhere(Predicate<AlertCategoryLink> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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