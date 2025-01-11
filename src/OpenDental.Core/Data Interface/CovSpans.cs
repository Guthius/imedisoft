using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CovSpans
{
    
    public static void Update(CovSpan covSpan)
    {
        Validate(covSpan);
        CovSpanCrud.Update(covSpan);
    }

    
    public static long Insert(CovSpan covSpan)
    {
        Validate(covSpan);
        return CovSpanCrud.Insert(covSpan);
    }

    
    private static void Validate(CovSpan covSpan)
    {
        if (covSpan.FromCode == "" || covSpan.ToCode == "") throw new ApplicationException(Lans.g("FormInsSpanEdit", "Codes not allowed to be blank."));
        if (string.Compare(covSpan.ToCode, covSpan.FromCode) < 0) throw new ApplicationException(Lans.g("FormInsSpanEdit", "From Code must be less than To Code.  Remember that the comparison is alphabetical, not numeric.  For instance, 100 would come before 2, but after 02."));
    }

    
    public static void Delete(CovSpan covSpan)
    {
        var command = "DELETE FROM covspan"
                      + " WHERE CovSpanNum = '" + SOut.Long(covSpan.CovSpanNum) + "'";
        Db.NonQ(command);
    }

    
    public static void DeleteForCat(long covCatNum)
    {
        var command = "DELETE FROM covspan WHERE CovCatNum = " + SOut.Long(covCatNum);
        Db.NonQ(command);
    }

    
    public static long GetCat(string myCode)
    {
        var covSpan = GetLastOrDefault(x => string.Compare(myCode, x.FromCode) >= 0
                                            && string.Compare(myCode, x.ToCode) <= 0);
        if (covSpan == null) return 0;
        return covSpan.CovCatNum;
    }

    public static List<long> GetCats(string myCode)
    {
        var listCovCatNums = GetWhere(x => string.Compare(myCode, x.FromCode) >= 0
                                           && string.Compare(myCode, x.ToCode) <= 0).Select(x => x.CovCatNum).ToList();
        return listCovCatNums;
    }

    
    public static List<CovSpan> GetForCat(long covCatNum)
    {
        return GetWhere(x => x.CovCatNum == covCatNum);
    }

    ///<summary>If the supplied code falls within any of the supplied spans, then returns true.</summary>
    public static bool IsCodeInSpans(string strProcCode, List<CovSpan> listCovSpans)
    {
        for (var i = 0; i < listCovSpans.Count; i++)
            if (string.Compare(strProcCode, listCovSpans[i].FromCode) >= 0
                && string.Compare(strProcCode, listCovSpans[i].ToCode) <= 0)
                return true;

        return false;
    }

    #region CachePattern

    private class CovSpanCache : CacheListAbs<CovSpan>
    {
        protected override List<CovSpan> GetCacheFromDb()
        {
            var command = "SELECT * FROM covspan ORDER BY FromCode";
            return CovSpanCrud.SelectMany(command);
        }

        protected override List<CovSpan> TableToList(DataTable dataTable)
        {
            return CovSpanCrud.TableToList(dataTable);
        }

        protected override CovSpan Copy(CovSpan item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<CovSpan> items)
        {
            return CovSpanCrud.ListToTable(items, "CovSpan");
        }

        protected override void FillCacheIfNeeded()
        {
            CovSpans.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly CovSpanCache _covSpanCache = new();

    public static List<CovSpan> GetWhere(Predicate<CovSpan> match, bool isShort = false)
    {
        return _covSpanCache.GetWhere(match, isShort);
    }

    public static List<CovSpan> GetDeepCopy(bool isShort = false)
    {
        return _covSpanCache.GetDeepCopy(isShort);
    }

    public static CovSpan GetLastOrDefault(Func<CovSpan, bool> match, bool isShort = false)
    {
        return _covSpanCache.GetLastOrDefault(match, isShort);
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
        _covSpanCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _covSpanCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _covSpanCache.ClearCache();
    }

    #endregion
}