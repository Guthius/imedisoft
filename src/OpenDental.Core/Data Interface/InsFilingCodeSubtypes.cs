using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsFilingCodeSubtypes
{
    ///<Summary>Gets one InsFilingCodeSubtype from the database.</Summary>
    public static InsFilingCodeSubtype GetOne(long insFilingCodeSubtypeNum)
    {
        return InsFilingCodeSubtypeCrud.SelectOne(insFilingCodeSubtypeNum);
    }

    
    public static long Insert(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        return InsFilingCodeSubtypeCrud.Insert(insFilingCodeSubtype);
    }

    
    public static void Update(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        InsFilingCodeSubtypeCrud.Update(insFilingCodeSubtype);
    }

    ///<summary>Surround with try/catch</summary>
    public static void Delete(long insFilingCodeSubtypeNum)
    {
        var command = "SELECT COUNT(*) FROM insplan WHERE FilingCodeSubtype=" + SOut.Long(insFilingCodeSubtypeNum);
        if (DataCore.GetScalar(command) != "0") throw new ApplicationException(Lans.g("InsFilingCodeSubtype", "Already in use by insplans."));
        InsFilingCodeSubtypeCrud.Delete(insFilingCodeSubtypeNum);
    }

    public static List<InsFilingCodeSubtype> GetForInsFilingCode(long insFilingCodeNum)
    {
        return GetWhere(x => x.InsFilingCodeNum == insFilingCodeNum);
    }

    public static void DeleteForInsFilingCode(long insFilingCodeNum)
    {
        var command = "DELETE FROM insfilingcodesubtype " +
                      "WHERE InsFilingCodeNum=" + SOut.Long(insFilingCodeNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class InsFilingCodeSubtypeCache : CacheListAbs<InsFilingCodeSubtype>
    {
        protected override List<InsFilingCodeSubtype> GetCacheFromDb()
        {
            var command = "SELECT * FROM insfilingcodesubtype ORDER BY Descript";
            return InsFilingCodeSubtypeCrud.SelectMany(command);
        }

        protected override List<InsFilingCodeSubtype> TableToList(DataTable dataTable)
        {
            return InsFilingCodeSubtypeCrud.TableToList(dataTable);
        }

        protected override InsFilingCodeSubtype Copy(InsFilingCodeSubtype item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<InsFilingCodeSubtype> items)
        {
            return InsFilingCodeSubtypeCrud.ListToTable(items, "InsFilingCodeSubtype");
        }

        protected override void FillCacheIfNeeded()
        {
            InsFilingCodeSubtypes.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly InsFilingCodeSubtypeCache _InsFilingCodeSubtypeCache = new();

    public static List<InsFilingCodeSubtype> GetWhere(Predicate<InsFilingCodeSubtype> match, bool isShort = false)
    {
        return _InsFilingCodeSubtypeCache.GetWhere(match, isShort);
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
        _InsFilingCodeSubtypeCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _InsFilingCodeSubtypeCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _InsFilingCodeSubtypeCache.ClearCache();
    }

    #endregion
}