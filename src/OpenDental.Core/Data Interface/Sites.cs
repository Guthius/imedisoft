using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Sites
{
    ///<Summary>Gets one Site from the database.</Summary>
    public static Site CreateObject(long siteNum)
    {
        return SiteCrud.SelectOne(siteNum);
    }

    
    public static long Insert(Site site)
    {
        return SiteCrud.Insert(site);
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

    /// <summary>
    ///     Will return -1 if no match, 0 if a description of empty string was passed in, otherwise the corresponding
    ///     SiteNum.
    /// </summary>
    public static long FindMatchSiteNum(string description)
    {
        if (description == "") return 0; //Preserving old behavior...
        var site = GetFirstOrDefault(x => x.Description.ToLower() == description.ToLower());
        if (site == null) return -1;
        return site.SiteNum;
    }

    #region CachePattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SiteCache _siteCache = new();

    public static List<Site> GetDeepCopy(bool isShort = false)
    {
        return _siteCache.GetDeepCopy(isShort);
    }

    public static List<Site> GetWhere(Predicate<Site> match, bool isShort = false)
    {
        return _siteCache.GetWhere(match, isShort);
    }

    public static Site GetFirst(bool isShort = false)
    {
        return _siteCache.GetFirst(isShort);
    }

    public static Site GetFirst(Func<Site, bool> match, bool isShort = false)
    {
        return _siteCache.GetFirst(match, isShort);
    }

    public static Site GetFirstOrDefault(Func<Site, bool> match, bool isShort = false)
    {
        return _siteCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _siteCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _siteCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _siteCache.ClearCache();
    }

    #endregion
}