using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Sites
{
    public static void Insert(Site site)
    {
        SiteCrud.Insert(site);
    }

    public static void Update(Site site)
    {
        SiteCrud.Update(site);
    }

    public static void DeleteObject(long siteNum)
    {
        var dataTable = DataCore.GetTable("SELECT LName,FName FROM patient WHERE SiteNum = " + siteNum);

        var patientNames = "";
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            if (i > 0)
            {
                patientNames += ", ";
            }

            patientNames += dataTable.Rows[i]["FName"] + " " + dataTable.Rows[i]["LName"];
        }

        if (dataTable.Rows.Count > 0)
        {
            throw new ApplicationException("Site is already in use by patient(s). Not allowed to delete. " + patientNames);
        }

        SiteCrud.Delete(siteNum);
    }

    public static string GetDescription(long siteNum)
    {
        var site = GetFirstOrDefault(x => x.SiteNum == siteNum);

        return site == null ? "" : site.Description;
    }

    public static List<Site> GetListFiltered(string snippet)
    {
        return GetWhere(x => x.Description.ToLower().Contains(snippet.ToLower()));
    }

    public static long FindMatchSiteNum(string description)
    {
        if (description == "")
        {
            return 0;
        }

        var site = GetFirstOrDefault(x => string.Equals(x.Description, description, StringComparison.CurrentCultureIgnoreCase));
        if (site == null)
        {
            return -1;
        }

        return site.SiteNum;
    }

    private class SiteCache : CacheListAbs<Site>
    {
        protected override List<Site> GetCacheFromDb()
        {
            return SiteCrud.SelectMany("SELECT * FROM site ORDER BY Description");
        }

        protected override List<Site> TableToList(DataTable dataTable)
        {
            return SiteCrud.TableToList(dataTable);
        }

        protected override Site Copy(Site item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Site> items)
        {
            return SiteCrud.ListToTable(items, "Site");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly SiteCache Cache = new();

    public static List<Site> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<Site> GetWhere(Predicate<Site> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static Site GetFirstOrDefault(Func<Site, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
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