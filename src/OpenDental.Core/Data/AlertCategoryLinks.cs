using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AlertCategoryLinks
{
    public static List<AlertCategoryLink> GetForCategory(long alertCategoryNum)
    {
        return alertCategoryNum == 0 ? [] : AlertCategoryLinkCrud.SelectMany("SELECT * FROM alertcategorylink WHERE AlertCategoryNum = " + alertCategoryNum);
    }

    public static void Insert(AlertCategoryLink alertCategoryLink)
    {
        AlertCategoryLinkCrud.Insert(alertCategoryLink);
    }

    public static void DeleteForCategory(long alertCategoryNum)
    {
        if (alertCategoryNum == 0)
        {
            return;
        }

        Db.NonQ("DELETE FROM alertcategorylink WHERE AlertCategoryNum = " + alertCategoryNum);
    }

    public static void Sync(List<AlertCategoryLink> listAlertCategoryLinksNew, List<AlertCategoryLink> listAlertCategoryLinksOld)
    {
        AlertCategoryLinkCrud.Sync(listAlertCategoryLinksNew, listAlertCategoryLinksOld);
    }

    private class AlertCategoryLinkCache : CacheListAbs<AlertCategoryLink>
    {
        protected override List<AlertCategoryLink> GetCacheFromDb()
        {
            return AlertCategoryLinkCrud.SelectMany("SELECT * FROM alertcategorylink");
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