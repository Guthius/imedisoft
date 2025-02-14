using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class Sops
{
    public static void Insert(Sop sop)
    {
        SopCrud.Insert(sop);
    }

    public static void Update(Sop sop)
    {
        SopCrud.Update(sop);
    }

    public static string GetDescriptionFromCode(string sopCode)
    {
        var sop = GetFirstOrDefault(x => x.SopCode == sopCode);

        return sop == null ? "" : sop.Description;
    }

    private class SopCache : CacheListAbs<Sop>
    {
        protected override List<Sop> GetCacheFromDb()
        {
            return SopCrud.SelectMany("SELECT * FROM sop");
        }

        protected override List<Sop> TableToList(DataTable dataTable)
        {
            return SopCrud.TableToList(dataTable);
        }

        protected override Sop Copy(Sop item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Sop> items)
        {
            return SopCrud.ListToTable(items, "Sop");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly SopCache Cache = new();

    public static List<Sop> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static Sop GetFirstOrDefault(Func<Sop, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
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