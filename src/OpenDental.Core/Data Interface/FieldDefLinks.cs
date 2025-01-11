using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FieldDefLinks
{
    public static bool Sync(List<FieldDefLink> listFieldDefLinksNew)
    {
        var command = "SELECT * FROM fielddeflink";
        var listFieldDefLinksDb = FieldDefLinkCrud.SelectMany(command);
        return FieldDefLinkCrud.Sync(listFieldDefLinksNew, listFieldDefLinksDb);
    }

    ///<summary>Deletes all fieldDefLink rows that are associated to the given fieldDefNum and fieldDefType.</summary>
    public static void DeleteForFieldDefNum(long fieldDefNum, FieldDefTypes fieldDefTypes)
    {
        if (fieldDefNum == 0) return;
        //Only delete records of the correct fieldDefType (Pat vs Appt)
        Db.NonQ("DELETE FROM fielddeflink WHERE FieldDefNum=" + SOut.Long(fieldDefNum) + " AND FieldDefType=" + SOut.Int((int) fieldDefTypes));
    }

    #region CachePattern

    private class FieldDefLinkCache : CacheListAbs<FieldDefLink>
    {
        protected override List<FieldDefLink> GetCacheFromDb()
        {
            var command = "SELECT * FROM fielddeflink";
            return FieldDefLinkCrud.SelectMany(command);
        }

        protected override List<FieldDefLink> TableToList(DataTable dataTable)
        {
            return FieldDefLinkCrud.TableToList(dataTable);
        }

        protected override FieldDefLink Copy(FieldDefLink item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<FieldDefLink> items)
        {
            return FieldDefLinkCrud.ListToTable(items, "FieldDefLink");
        }

        protected override void FillCacheIfNeeded()
        {
            FieldDefLinks.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly FieldDefLinkCache _fieldDefLinkCache = new();

    public static bool GetExists(Predicate<FieldDefLink> match, bool isShort = false)
    {
        return _fieldDefLinkCache.GetExists(match, isShort);
    }

    public static List<FieldDefLink> GetDeepCopy(bool isShort = false)
    {
        return _fieldDefLinkCache.GetDeepCopy(isShort);
    }

    public static FieldDefLink GetFirstOrDefault(Func<FieldDefLink, bool> match, bool isShort = false)
    {
        return _fieldDefLinkCache.GetFirstOrDefault(match, isShort);
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
        _fieldDefLinkCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _fieldDefLinkCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _fieldDefLinkCache.ClearCache();
    }

    #endregion
}