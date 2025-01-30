using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ProviderIdents
{
    public static void Update(ProviderIdent providerIdent)
    {
        ProviderIdentCrud.Update(providerIdent);
    }

    public static void Insert(ProviderIdent providerIdent)
    {
        ProviderIdentCrud.Insert(providerIdent);
    }

    public static void Delete(ProviderIdent providerIdent)
    {
        Db.NonQ("DELETE FROM providerident WHERE ProviderIdentNum = " + providerIdent.ProviderIdentNum);
    }

    public static ProviderIdent[] GetForProv(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum).ToArray();
    }

    public static ProviderIdent[] GetForPayor(long provNum, string payorId)
    {
        return GetWhere(x => x.ProvNum == provNum && x.PayorID == payorId).ToArray();
    }

    public static void DeleteAllForProv(long provNum)
    {
        Db.NonQ("DELETE from providerident WHERE provnum = " + provNum);
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
            ProviderIdents.GetTableFromCache(false);
        }
    }

    private static readonly ProviderIdentCache Cache = new();

    public static List<ProviderIdent> GetWhere(Predicate<ProviderIdent> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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
}