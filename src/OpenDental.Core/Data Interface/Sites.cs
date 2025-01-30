using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Sites
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
        //validate that not already in use.
        var command = "SELECT LName,FName FROM patient WHERE SiteNum=" + SOut.Long(siteNum);
        var table = DataCore.GetTable(command);
        //int count=PIn.PInt(Db.GetCount(command));
        var pats = "";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0) pats += ", ";
            pats += table.Rows[i]["FName"] + " " + table.Rows[i]["LName"];
        }

        if (table.Rows.Count > 0) throw new ApplicationException(Lans.g("Sites", "Site is already in use by patient(s). Not allowed to delete. ") + pats);
        SiteCrud.Delete(siteNum);
    }

    public static string GetDescription(long siteNum)
    {
        var site = GetFirstOrDefault(x => x.SiteNum == siteNum);
        if (site == null) return "";
        return site.Description;
    }

    public static List<Site> GetListFiltered(string snippet)
    {
        return GetWhere(x => x.Description.ToLower().Contains(snippet.ToLower()));
    }

    public static long FindMatchSiteNum(string description)
    {
        if (description == "") return 0; //Preserving old behavior...
        var site = GetFirstOrDefault(x => x.Description.ToLower() == description.ToLower());
        if (site == null) return -1;
        return site.SiteNum;
    }

    private class SiteCache : CacheListAbs<Site>
    {
        protected override List<Site> GetCacheFromDb()
        {
            var command = "SELECT * FROM site ORDER BY Description";
            return SiteCrud.SelectMany(command);
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
            Sites.GetTableFromCache(false);
        }
    }

    private static readonly SiteCache Cache = new();

    public static List<Site> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<Site> GetWhere(Predicate<Site> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static Site GetFirstOrDefault(Func<Site, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
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