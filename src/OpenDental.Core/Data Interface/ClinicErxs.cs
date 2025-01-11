using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ClinicErxs
{
    #region Insert

    
    public static long Insert(ClinicErx clinicErx)
    {
        return ClinicErxCrud.Insert(clinicErx);
    }

    #endregion

    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.

    #region Cache Pattern

    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class ClinicErxCache : CacheListAbs<ClinicErx>
    {
        protected override List<ClinicErx> GetCacheFromDb()
        {
            var command = "SELECT * FROM clinicerx";
            return ClinicErxCrud.SelectMany(command);
        }

        protected override List<ClinicErx> TableToList(DataTable dataTable)
        {
            return ClinicErxCrud.TableToList(dataTable);
        }

        protected override ClinicErx Copy(ClinicErx item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ClinicErx> items)
        {
            return ClinicErxCrud.ListToTable(items, "ClinicErx");
        }

        protected override void FillCacheIfNeeded()
        {
            ClinicErxs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ClinicErxCache _clinicErxCache = new();

    public static List<ClinicErx> GetDeepCopy(bool isShort = false)
    {
        return _clinicErxCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _clinicErxCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<ClinicErx> match, bool isShort = false)
    {
        return _clinicErxCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<ClinicErx> match, bool isShort = false)
    {
        return _clinicErxCache.GetFindIndex(match, isShort);
    }

    public static ClinicErx GetFirst(bool isShort = false)
    {
        return _clinicErxCache.GetFirst(isShort);
    }

    public static ClinicErx GetFirst(Func<ClinicErx, bool> match, bool isShort = false)
    {
        return _clinicErxCache.GetFirst(match, isShort);
    }

    public static ClinicErx GetFirstOrDefault(Func<ClinicErx, bool> match, bool isShort = false)
    {
        return _clinicErxCache.GetFirstOrDefault(match, isShort);
    }

    public static ClinicErx GetLast(bool isShort = false)
    {
        return _clinicErxCache.GetLast(isShort);
    }

    public static ClinicErx GetLastOrDefault(Func<ClinicErx, bool> match, bool isShort = false)
    {
        return _clinicErxCache.GetLastOrDefault(match, isShort);
    }

    public static List<ClinicErx> GetWhere(Predicate<ClinicErx> match, bool isShort = false)
    {
        return _clinicErxCache.GetWhere(match, isShort);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _clinicErxCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _clinicErxCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _clinicErxCache.ClearCache();
    }

    #endregion Cache Pattern

    #region Get Methods

    ///<summary>Gets one ClinicErx from the cache.</summary>
    public static ClinicErx GetByClinicNum(long clinicNum)
    {
        return GetFirstOrDefault(x => x.ClinicNum == clinicNum);
    }

    ///<summary>Gets one ClinicErx from the cache.</summary>
    public static ClinicErx GetByClinicIdAndKey(string clinicId, string clinicKey)
    {
        return GetFirstOrDefault(x => x.ClinicId == clinicId && x.ClinicKey == clinicKey);
    }

    ///<summary>This should only be used for ODHQ.  Gets all account ids associated to the patient account.</summary>
    public static List<string> GetAccountIdsForPatNum(long patNum)
    {
        return GetDeepCopy().FindAll(x => x.PatNum == patNum && !string.IsNullOrWhiteSpace(x.AccountId))
            .Select(x => x.AccountId)
            .ToList();
    }

    ///<summary>Returns all ClinicErx in that have a PatNum that is found in the list of passed in PatNums.</summary>
    public static List<ClinicErx> GetManyByPatNums(List<long> listPatNums)
    {
        return GetDeepCopy().FindAll(x => listPatNums.Any(y => y == x.PatNum));
    }

    #endregion

    #region Update

    
    public static void Update(ClinicErx clinicErx)
    {
        ClinicErxCrud.Update(clinicErx);
    }

    
    public static bool Update(ClinicErx clinicErx, ClinicErx clinicErxOld)
    {
        return ClinicErxCrud.Update(clinicErx, clinicErxOld);
    }

    #endregion
}