using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ProviderIdents
{
    public static ProviderIdent[] GetForPayor(long provNum, string payorId)
    {
        return GetWhere(x => x.ProvNum == provNum && x.PayorID == payorId).ToArray();
    }

    private class ProviderIdentCache : CacheListAbs<ProviderIdent>
    {
        protected override List<ProviderIdent> GetCacheFromDb()
        {
            return ProviderIdentCrud.SelectMany("SELECT * FROM providerident");
        }

        protected override List<ProviderIdent> TableToList(DataTable dataTable)
        {
            return ProviderIdentCrud.TableToList(dataTable);
        }

        protected override ProviderIdent Copy(ProviderIdent item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProviderIdent> items)
        {
            return ProviderIdentCrud.ListToTable(items, "ProviderIdent");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly ProviderIdentCache Cache = new();

    public static List<ProviderIdent> GetWhere(Predicate<ProviderIdent> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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