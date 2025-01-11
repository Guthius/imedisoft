using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoChartTabLinks
{
	/// <summary>
	///     Inserts, updates, or deletes the passed in list against the stale list listOld.  Returns true if db changes
	///     were made.
	/// </summary>
	public static bool Sync(List<OrthoChartTabLink> listOrthoChartTabLinksNew, List<OrthoChartTabLink> listOrthoChartTabLinksOld)
    {
        return OrthoChartTabLinkCrud.Sync(listOrthoChartTabLinksNew, listOrthoChartTabLinksOld);
    }

    #region CachePattern

    private class OrthoChartTabLinkCache : CacheListAbs<OrthoChartTabLink>
    {
        protected override List<OrthoChartTabLink> GetCacheFromDb()
        {
            var command = "SELECT * FROM orthocharttablink ORDER BY ItemOrder";
            return OrthoChartTabLinkCrud.SelectMany(command);
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
            OrthoChartTabLinks.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly OrthoChartTabLinkCache _orthoChartTabLinkCache = new();

    public static bool GetExists(Predicate<OrthoChartTabLink> match, bool isShort = false)
    {
        return _orthoChartTabLinkCache.GetExists(match, isShort);
    }

    public static List<OrthoChartTabLink> GetDeepCopy(bool isShort = false)
    {
        return _orthoChartTabLinkCache.GetDeepCopy(isShort);
    }

    public static List<OrthoChartTabLink> GetWhere(Predicate<OrthoChartTabLink> match, bool isShort = false)
    {
        return _orthoChartTabLinkCache.GetWhere(match, isShort);
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
        _orthoChartTabLinkCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _orthoChartTabLinkCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _orthoChartTabLinkCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<OrthoChartTabLink> Refresh(long patNum){

        string command="SELECT * FROM orthocharttablink WHERE PatNum = "+POut.Long(patNum);
        return Crud.OrthoChartTabLinkCrud.SelectMany(command);
    }

    ///<summary>Gets one OrthoChartTabLink from the db.</summary>
    public static OrthoChartTabLink GetOne(long orthoChartTabLinkNum){

        return Crud.OrthoChartTabLinkCrud.SelectOne(orthoChartTabLinkNum);
    }

    
    public static long Insert(OrthoChartTabLink orthoChartTabLink){

        return Crud.OrthoChartTabLinkCrud.Insert(orthoChartTabLink);
    }

    
    public static void Update(OrthoChartTabLink orthoChartTabLink){

        Crud.OrthoChartTabLinkCrud.Update(orthoChartTabLink);
    }

    
    public static void Delete(long orthoChartTabLinkNum) {

        Crud.OrthoChartTabLinkCrud.Delete(orthoChartTabLinkNum);
    }




    */
}