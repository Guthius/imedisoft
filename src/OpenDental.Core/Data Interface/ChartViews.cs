using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ChartViews
{
    
    public static long Insert(ChartView chartView)
    {
        return ChartViewCrud.Insert(chartView);
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
        var command = "DELETE FROM chartview WHERE ChartViewNum = " + SOut.Long(chartViewNum);
        Db.NonQ(command);
    }

    #region CachePattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ChartViewCache _chartViewCache = new();

    public static List<ChartView> GetDeepCopy(bool isShort = false)
    {
        return _chartViewCache.GetDeepCopy(isShort);
    }

    public static ChartView GetFirst(bool isShort = false)
    {
        return _chartViewCache.GetFirst(isShort);
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
        _chartViewCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _chartViewCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _chartViewCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<ChartView> Refresh(long patNum){

        string command="SELECT * FROM chartview WHERE PatNum = "+POut.Long(patNum);
        return Crud.ChartViewCrud.SelectMany(command);
    }

    ///<summary>Gets one ChartView from the db.</summary>
    public static ChartView GetOne(long chartViewNum){

        return Crud.ChartViewCrud.SelectOne(chartViewNum);
    }

    */
}