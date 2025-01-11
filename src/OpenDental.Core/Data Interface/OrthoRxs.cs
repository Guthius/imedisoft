using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoRxs
{
    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.

    #region Cache Pattern

    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class OrthoRxCache : CacheListAbs<OrthoRx>
    {
        protected override List<OrthoRx> GetCacheFromDb()
        {
            var command = "SELECT * FROM orthorx ORDER BY ItemOrder";
            return OrthoRxCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly OrthoRxCache _orthoRxCache = new();

    public static List<OrthoRx> GetDeepCopy(bool isShort = false)
    {
        return _orthoRxCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _orthoRxCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<OrthoRx> match, bool isShort = false)
    {
        return _orthoRxCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<OrthoRx> match, bool isShort = false)
    {
        return _orthoRxCache.GetFindIndex(match, isShort);
    }

    public static OrthoRx GetFirst(bool isShort = false)
    {
        return _orthoRxCache.GetFirst(isShort);
    }

    public static OrthoRx GetFirst(Func<OrthoRx, bool> match, bool isShort = false)
    {
        return _orthoRxCache.GetFirst(match, isShort);
    }

    public static OrthoRx GetFirstOrDefault(Func<OrthoRx, bool> match, bool isShort = false)
    {
        return _orthoRxCache.GetFirstOrDefault(match, isShort);
    }

    public static OrthoRx GetLast(bool isShort = false)
    {
        return _orthoRxCache.GetLast(isShort);
    }

    public static OrthoRx GetLastOrDefault(Func<OrthoRx, bool> match, bool isShort = false)
    {
        return _orthoRxCache.GetLastOrDefault(match, isShort);
    }

    public static List<OrthoRx> GetWhere(Predicate<OrthoRx> match, bool isShort = false)
    {
        return _orthoRxCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _orthoRxCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _orthoRxCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _orthoRxCache.ClearCache();
    }

    #endregion Cache Pattern

    /*
    #region Methods - Get
    
    public static List<OrthoRx> Refresh(long patNum){

        string command="SELECT * FROM orthorx WHERE PatNum = "+POut.Long(patNum);
        return Crud.OrthoRxCrud.SelectMany(command);
    }

    ///<summary>Gets one OrthoRx from the db.</summary>
    public static OrthoRx GetOne(long orthoRxNum){

        return Crud.OrthoRxCrud.SelectOne(orthoRxNum);
    }
    #endregion Methods - Get*/

    #region Methods - Modify

    
    public static long Insert(OrthoRx orthoRx)
    {
        return OrthoRxCrud.Insert(orthoRx);
    }

    
    public static void Update(OrthoRx orthoRx)
    {
        OrthoRxCrud.Update(orthoRx);
    }

    
    public static void Delete(long orthoRxNum)
    {
        OrthoRxCrud.Delete(orthoRxNum);
    }

    #endregion Methods - Modify
}