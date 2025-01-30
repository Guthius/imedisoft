using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ClaimFormItems
{
    public static void Insert(ClaimFormItem claimFormItem)
    {
        ClaimFormItemCrud.Insert(claimFormItem);
    }

    public static void Update(ClaimFormItem claimFormItem)
    {
        ClaimFormItemCrud.Update(claimFormItem);
    }

    public static void DeleteAllForClaimForm(long claimFormNum)
    {
        Db.NonQ("DELETE FROM claimformitem WHERE ClaimFormNum = " + (claimFormNum));
    }

    public static List<ClaimFormItem> GetListForForm(long claimFormNum)
    {
        return GetWhere(x => x.ClaimFormNum == claimFormNum);
    }

    private class ClaimFormItemCache : CacheListAbs<ClaimFormItem>
    {
        protected override List<ClaimFormItem> GetCacheFromDb()
        {
            return ClaimFormItemCrud.SelectMany("SELECT * FROM claimformitem ORDER BY ImageFileName DESC");
        }

        protected override List<ClaimFormItem> TableToList(DataTable dataTable)
        {
            return ClaimFormItemCrud.TableToList(dataTable);
        }

        protected override ClaimFormItem Copy(ClaimFormItem item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ClaimFormItem> items)
        {
            return ClaimFormItemCrud.ListToTable(items, "ClaimFormItem");
        }

        protected override void FillCacheIfNeeded()
        {
            ClaimFormItems.GetTableFromCache(false);
        }
    }

    private static readonly ClaimFormItemCache Cache = new();

    public static List<ClaimFormItem> GetWhere(Predicate<ClaimFormItem> match, bool isShort = false)
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