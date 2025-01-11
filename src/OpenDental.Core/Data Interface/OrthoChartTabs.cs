using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoChartTabs
{
	/// <summary>
	///     Inserts, updates, or deletes the passed in list against the stale list listOld.  Returns true if db changes
	///     were made.
	/// </summary>
	public static bool Sync(List<OrthoChartTab> listOrthoChartTabsNew, List<OrthoChartTab> listOrthoChartTabsOld)
    {
        return OrthoChartTabCrud.Sync(listOrthoChartTabsNew, listOrthoChartTabsOld);
    }

    #region CachePattern

    private class OrthoChartTabCache : CacheListAbs<OrthoChartTab>
    {
        protected override List<OrthoChartTab> GetCacheFromDb()
        {
            var command = "SELECT * FROM orthocharttab ORDER BY ItemOrder";
            return OrthoChartTabCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly OrthoChartTabCache _orthoChartTabCache = new();

    public static List<OrthoChartTab> GetDeepCopy(bool isShort = false)
    {
        return _orthoChartTabCache.GetDeepCopy(isShort);
    }

    public static OrthoChartTab GetFirst(bool isShort = false)
    {
        return _orthoChartTabCache.GetFirst(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _orthoChartTabCache.GetCount(isShort);
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
        _orthoChartTabCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _orthoChartTabCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _orthoChartTabCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<OrthoChartTab> Refresh(long patNum){

        string command="SELECT * FROM orthocharttab WHERE PatNum = "+POut.Long(patNum);
        return Crud.OrthoChartTabCrud.SelectMany(command);
    }

    ///<summary>Gets one OrthoChartTab from the db.</summary>
    public static OrthoChartTab GetOne(long orthoChartTabNum){

        return Crud.OrthoChartTabCrud.SelectOne(orthoChartTabNum);
    }

    
    public static long Insert(OrthoChartTab orthoChartTab){

        return Crud.OrthoChartTabCrud.Insert(orthoChartTab);
    }

    
    public static void Update(OrthoChartTab orthoChartTab){

        Crud.OrthoChartTabCrud.Update(orthoChartTab);
    }

    
    public static void Delete(long orthoChartTabNum) {

        Crud.OrthoChartTabCrud.Delete(orthoChartTabNum);
    }




    */
}