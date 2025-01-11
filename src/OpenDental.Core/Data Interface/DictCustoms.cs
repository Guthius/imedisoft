using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DictCustoms
{
    public static void Insert(DictCustom dictCustom)
    {
        DictCustomCrud.Insert(dictCustom);
    }

    public static void Update(DictCustom dictCustom)
    {
        DictCustomCrud.Update(dictCustom);
    }

    public static void Delete(long dictCustomNum)
    {
        DictCustomCrud.Delete(dictCustomNum);
    }

    #region CachePattern

    private class DictCustomCache : CacheListAbs<DictCustom>
    {
        protected override List<DictCustom> GetCacheFromDb()
        {
            var command = "SELECT * FROM dictcustom ORDER BY WordText";
            return DictCustomCrud.SelectMany(command);
        }

        protected override List<DictCustom> TableToList(DataTable dataTable)
        {
            return DictCustomCrud.TableToList(dataTable);
        }

        protected override DictCustom Copy(DictCustom item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DictCustom> items)
        {
            return DictCustomCrud.ListToTable(items, "DictCustom");
        }

        protected override void FillCacheIfNeeded()
        {
            DictCustoms.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly DictCustomCache _dictCustomCache = new();

    public static List<DictCustom> GetDeepCopy(bool isShort = false)
    {
        return _dictCustomCache.GetDeepCopy(isShort);
    }

    public static DictCustom GetFirstOrDefault(Func<DictCustom, bool> match, bool isShort = false)
    {
        return _dictCustomCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    public static void FillCacheFromTable(DataTable table)
    {
        _dictCustomCache.FillCacheFromTable(table);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _dictCustomCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _dictCustomCache.ClearCache();
    }

    #endregion
}