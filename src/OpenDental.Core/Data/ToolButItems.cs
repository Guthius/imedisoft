using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ToolButItems
{
    public static void Insert(ToolButItem toolButItem)
    {
        ToolButItemCrud.Insert(toolButItem);
    }

    public static void DeleteAllForProgram(long programNum)
    {
        Db.NonQ("DELETE from toolbutitem WHERE ProgramNum = " + programNum);
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
            return ToolButItemCrud.SelectMany("SELECT * from toolbutitem");
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
            GetTableFromCache(false);
        }
    }

    private static readonly ToolButItemCache Cache = new();

    public static bool GetCacheIsNull()
    {
        return Cache.ListIsNull();
    }

    public static List<ToolButItem> GetWhere(Predicate<ToolButItem> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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