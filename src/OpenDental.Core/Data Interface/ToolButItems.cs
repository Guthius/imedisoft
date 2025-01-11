using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ToolButItems
{
    
    public static long Insert(ToolButItem toolButItem)
    {
        return ToolButItemCrud.Insert(toolButItem);
    }

    ///<summary>This in not currently being used.</summary>
    public static void Update(ToolButItem toolButItem)
    {
        ToolButItemCrud.Update(toolButItem);
    }

    ///<summary>This is not currently being used.</summary>
    public static void Delete(ToolButItem tootlButItem)
    {
        var command = "DELETE from toolbutitem WHERE ToolButItemNum = '"
                      + SOut.Long(tootlButItem.ToolButItemNum) + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Deletes all ToolButItems for the Programs.Cur.  This is used regularly when saving a Program link because of
    ///     the way the user interface works.
    /// </summary>
    public static void DeleteAllForProgram(long programNum)
    {
        var command = "DELETE from toolbutitem WHERE ProgramNum = '"
                      + SOut.Long(programNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>Fills ForProgram with toolbutitems attached to the Programs.Cur</summary>
    public static List<ToolButItem> GetForProgram(long programNum)
    {
        return GetWhere(x => x.ProgramNum == programNum);
    }

    ///<summary>Returns a list of toolbutitems for the specified toolbar. Used when laying out toolbars.</summary>
    public static List<ToolButItem> GetForToolBar(EnumToolBar toolBarsAvail)
    {
        return GetWhere(x => x.ToolBar == toolBarsAvail && (Programs.IsEnabled(x.ProgramNum) || ProgramProperties.IsAdvertisingBridge(x.ProgramNum)));
    }

    #region CachePattern

    private class ToolButItemCache : CacheListAbs<ToolButItem>
    {
        protected override List<ToolButItem> GetCacheFromDb()
        {
            var command = "SELECT * from toolbutitem";
            return ToolButItemCrud.SelectMany(command);
        }

        protected override List<ToolButItem> TableToList(DataTable dataTable)
        {
            return ToolButItemCrud.TableToList(dataTable);
        }

        protected override ToolButItem Copy(ToolButItem item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ToolButItem> items)
        {
            return ToolButItemCrud.ListToTable(items, "ToolButItem");
        }

        protected override void FillCacheIfNeeded()
        {
            ToolButItems.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ToolButItemCache _ToolButItemCache = new();

    public static bool GetCacheIsNull()
    {
        return _ToolButItemCache.ListIsNull();
    }

    public static List<ToolButItem> GetWhere(Predicate<ToolButItem> match, bool isShort = false)
    {
        return _ToolButItemCache.GetWhere(match, isShort);
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
        _ToolButItemCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return _ToolButItemCache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        _ToolButItemCache.ClearCache();
    }

    #endregion
}