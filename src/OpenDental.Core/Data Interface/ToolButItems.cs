using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ToolButItems
{
    public static void Insert(ToolButItem toolButItem)
    {
        ToolButItemCrud.Insert(toolButItem);
    }

    public static void DeleteAllForProgram(long programNum)
    {
        var command = "DELETE from toolbutitem WHERE ProgramNum = '"
                      + SOut.Long(programNum) + "'";
        Db.NonQ(command);
    }

    public static List<ToolButItem> GetForProgram(long programNum)
    {
        return GetWhere(x => x.ProgramNum == programNum);
    }

    public static List<ToolButItem> GetForToolBar(EnumToolBar toolBarsAvail)
    {
        return GetWhere(x => x.ToolBar == toolBarsAvail && (Programs.IsEnabled(x.ProgramNum) || ProgramProperties.IsAdvertisingBridge(x.ProgramNum)));
    }
    
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

    private static readonly ToolButItemCache Cache = new();

    public static bool GetCacheIsNull()
    {
        return Cache.ListIsNull();
    }

    public static List<ToolButItem> GetWhere(Predicate<ToolButItem> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}