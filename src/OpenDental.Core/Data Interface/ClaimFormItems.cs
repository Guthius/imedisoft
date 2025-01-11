using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ClaimFormItems
{
    public static long Insert(ClaimFormItem claimFormItem)
    {
        return ClaimFormItemCrud.Insert(claimFormItem);
    }

    public static void Update(ClaimFormItem claimFormItem)
    {
        ClaimFormItemCrud.Update(claimFormItem);
    }

    public static void DeleteAllForClaimForm(long claimFormNum)
    {
        var command = "DELETE FROM claimformitem WHERE ClaimFormNum = '" + SOut.Long(claimFormNum) + "'";
        Db.NonQ(command);
    }

    public static List<ClaimFormItem> GetListForForm(long claimFormNum)
    {
        return GetWhere(x => x.ClaimFormNum == claimFormNum);
    }

    #region Cache Pattern

    private class ClaimFormItemCache : CacheListAbs<ClaimFormItem>
    {
        protected override List<ClaimFormItem> GetCacheFromDb()
        {
            var command = "SELECT * FROM claimformitem ORDER BY ImageFileName DESC";
            return ClaimFormItemCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ClaimFormItemCache _claimFormItemCache = new();

    public static List<ClaimFormItem> GetWhere(Predicate<ClaimFormItem> match, bool isShort = false)
    {
        return _claimFormItemCache.GetWhere(match, isShort);
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
        _claimFormItemCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _claimFormItemCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _claimFormItemCache.ClearCache();
    }

    #endregion Cache Pattern
}