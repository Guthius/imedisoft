using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsFilingCodes
{
    public static string GetEclaimCode(long insFilingCodeNum)
    {
        var insFilingCode = GetFirstOrDefault(x => x.InsFilingCodeNum == insFilingCodeNum);
        return insFilingCode == null ? "CI" : insFilingCode.EclaimCode;
    }

    ///<summary>Gets the InsFilingCode for the specified eclaimCode, or creates one if the eclaimCodes does not exist.</summary>
    public static InsFilingCode GetOrInsertForEclaimCode(string descript, string eclaimCode)
    {
        var itemOrderMax = 0;
        var listInsFilingCodes = GetDeepCopy();
        for (var i = 0; i < listInsFilingCodes.Count; i++)
        {
            if (listInsFilingCodes[i].ItemOrder > itemOrderMax) itemOrderMax = listInsFilingCodes[i].ItemOrder;
            if (listInsFilingCodes[i].EclaimCode != eclaimCode) continue;
            return listInsFilingCodes[i];
        }

        var insFilingCode = new InsFilingCode();
        insFilingCode.Descript = descript;
        insFilingCode.EclaimCode = eclaimCode;
        insFilingCode.ItemOrder = itemOrderMax + 1;
        Insert(insFilingCode);
        return insFilingCode;
    }

    public static List<InsFilingCode> GetAll()
    {
        var command = "SELECT * FROM insfilingcode ORDER BY ItemOrder";
        return InsFilingCodeCrud.SelectMany(command);
    }

    
    public static long Insert(InsFilingCode insFilingCode)
    {
        return InsFilingCodeCrud.Insert(insFilingCode);
    }

    
    public static void Update(InsFilingCode insFilingCode)
    {
        InsFilingCodeCrud.Update(insFilingCode);
    }

    ///<summary>Surround with try/catch</summary>
    public static void Delete(long insFilingCodeNum)
    {
        var command = "SELECT COUNT(*) FROM insplan WHERE FilingCode=" + SOut.Long(insFilingCodeNum);
        if (DataCore.GetScalar(command) != "0") throw new ApplicationException(Lans.g("InsFilingCode", "Already in use by insplans."));
        InsFilingCodeCrud.Delete(insFilingCodeNum);
    }

    #region CachePattern

    private class InsFilingCodeCache : CacheListAbs<InsFilingCode>
    {
        protected override List<InsFilingCode> GetCacheFromDb()
        {
            var command = "SELECT * FROM insfilingcode ORDER BY ItemOrder";
            return InsFilingCodeCrud.SelectMany(command);
        }

        protected override List<InsFilingCode> TableToList(DataTable dataTable)
        {
            return InsFilingCodeCrud.TableToList(dataTable);
        }

        protected override InsFilingCode Copy(InsFilingCode item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<InsFilingCode> items)
        {
            return InsFilingCodeCrud.ListToTable(items, "InsFilingCode");
        }

        protected override void FillCacheIfNeeded()
        {
            InsFilingCodes.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly InsFilingCodeCache _insFilingCodeCache = new();

    public static List<InsFilingCode> GetDeepCopy(bool isShort = false)
    {
        return _insFilingCodeCache.GetDeepCopy(isShort);
    }

    public static InsFilingCode GetFirstOrDefault(Func<InsFilingCode, bool> match, bool isShort = false)
    {
        return _insFilingCodeCache.GetFirstOrDefault(match, isShort);
    }

    public static InsFilingCode GetOne(long insFilingCodeNum)
    {
        return _insFilingCodeCache.GetFirstOrDefault(x => x.InsFilingCodeNum == insFilingCodeNum);
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
        _insFilingCodeCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _insFilingCodeCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _insFilingCodeCache.ClearCache();
    }

    #endregion
}