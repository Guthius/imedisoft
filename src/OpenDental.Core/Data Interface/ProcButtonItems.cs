using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProcButtonItems
{
    ///<summary>Must have already checked procCode for nonduplicate.</summary>
    public static long Insert(ProcButtonItem item)
    {
        return ProcButtonItemCrud.Insert(item);
    }

    
    public static void Update(ProcButtonItem item)
    {
        ProcButtonItemCrud.Update(item);
    }

    
    public static void Delete(ProcButtonItem item)
    {
        var command = "DELETE FROM procbuttonitem WHERE ProcButtonItemNum = '" + SOut.Long(item.ProcButtonItemNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>Sorted by Item Order.</summary>
    public static List<long> GetCodeNumListForButton(long procButtonNum)
    {
        return GetWhere(x => x.ProcButtonNum == procButtonNum && x.CodeNum > 0)
            .OrderBy(x => x.ItemOrder)
            .Select(x => x.CodeNum)
            .ToList();
    }

    ///<summary>Sorted by Item Order.</summary>
    public static List<long> GetAutoListForButton(long procButtonNum)
    {
        return GetWhere(x => x.ProcButtonNum == procButtonNum && x.AutoCodeNum > 0)
            .OrderBy(x => x.ItemOrder)
            .Select(x => x.AutoCodeNum)
            .ToList();
    }

    
    public static void DeleteAllForButton(long procButtonNum)
    {
        var command = "DELETE from procbuttonitem WHERE procbuttonnum = '" + SOut.Long(procButtonNum) + "'";
        Db.NonQ(command);
    }

    #region CachePattern

    private class ProcButtonItemCache : CacheListAbs<ProcButtonItem>
    {
        protected override List<ProcButtonItem> GetCacheFromDb()
        {
            var command = "SELECT * FROM procbuttonitem ORDER BY ItemOrder";
            return ProcButtonItemCrud.SelectMany(command);
        }

        protected override List<ProcButtonItem> TableToList(DataTable dataTable)
        {
            return ProcButtonItemCrud.TableToList(dataTable);
        }

        protected override ProcButtonItem Copy(ProcButtonItem item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProcButtonItem> items)
        {
            return ProcButtonItemCrud.ListToTable(items, "ProcButtonItem");
        }

        protected override void FillCacheIfNeeded()
        {
            ProcButtonItems.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProcButtonItemCache _procButtonItemCache = new();

    public static List<ProcButtonItem> GetDeepCopy(bool isShort = false)
    {
        return _procButtonItemCache.GetDeepCopy(isShort);
    }

    private static List<ProcButtonItem> GetWhere(Predicate<ProcButtonItem> match, bool isShort = false)
    {
        return _procButtonItemCache.GetWhere(match, isShort);
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
        _procButtonItemCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _procButtonItemCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _procButtonItemCache.ClearCache();
    }

    #endregion
}