using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class OrthoRxs
{
    private class OrthoRxCache : CacheListAbs<OrthoRx>
    {
        protected override List<OrthoRx> GetCacheFromDb()
        {
            return OrthoRxCrud.SelectMany("SELECT * FROM orthorx ORDER BY ItemOrder");
        }

        protected override List<OrthoRx> TableToList(DataTable dataTable)
        {
            return OrthoRxCrud.TableToList(dataTable);
        }

        protected override OrthoRx Copy(OrthoRx item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<OrthoRx> items)
        {
            return OrthoRxCrud.ListToTable(items, "OrthoRx");
        }

        protected override void FillCacheIfNeeded()
        {
            OrthoRxs.GetTableFromCache(false);
        }
    }

    private static readonly OrthoRxCache Cache = new();

    public static List<OrthoRx> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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

    public static void Insert(OrthoRx orthoRx)
    {
        OrthoRxCrud.Insert(orthoRx);
    }

    public static void Update(OrthoRx orthoRx)
    {
        OrthoRxCrud.Update(orthoRx);
    }

    public static void Delete(long orthoRxNum)
    {
        OrthoRxCrud.Delete(orthoRxNum);
    }
}