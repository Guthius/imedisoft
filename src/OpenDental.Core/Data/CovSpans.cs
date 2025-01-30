using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class CovSpans
{
    public static void Update(CovSpan covSpan)
    {
        Validate(covSpan);
        
        CovSpanCrud.Update(covSpan);
    }
    
    public static void Insert(CovSpan covSpan)
    {
        Validate(covSpan);
        
        CovSpanCrud.Insert(covSpan);
    }
    
    private static void Validate(CovSpan covSpan)
    {
        if (covSpan.FromCode == "" || covSpan.ToCode == "")
        {
            throw new ApplicationException(Lans.g("FormInsSpanEdit", "Codes not allowed to be blank."));
        }
        
        if (string.CompareOrdinal(covSpan.ToCode, covSpan.FromCode) < 0)
        {
            throw new ApplicationException(Lans.g("FormInsSpanEdit",
                "From Code must be less than To Code.  " +
                "Remember that the comparison is alphabetical, not numeric.  " +
                "For instance, 100 would come before 2, but after 02."));
        }
    }
    
    public static void Delete(CovSpan covSpan)
    {
        Db.NonQ("DELETE FROM covspan WHERE CovSpanNum = " + covSpan.CovSpanNum);
    }
    
    public static void DeleteForCat(long covCatNum)
    {
        Db.NonQ("DELETE FROM covspan WHERE CovCatNum = " + covCatNum);
    }
    
    public static List<long> GetCats(string myCode)
    {
        return GetWhere(x => 
            string.CompareOrdinal(myCode, x.FromCode) >= 0 && 
            string.CompareOrdinal(myCode, x.ToCode) <= 0)
            .Select(x => x.CovCatNum)
            .ToList();
    }
    
    public static List<CovSpan> GetForCat(long covCatNum)
    {
        return GetWhere(x => x.CovCatNum == covCatNum);
    }
    
    public static bool IsCodeInSpans(string strProcCode, List<CovSpan> listCovSpans)
    {
        foreach (var t in listCovSpans)
        {
            if (string.CompareOrdinal(strProcCode, t.FromCode) >= 0 && string.CompareOrdinal(strProcCode, t.ToCode) <= 0)
            {
                return true;
            }
        }

        return false;
    }
    
    private class CovSpanCache : CacheListAbs<CovSpan>
    {
        protected override List<CovSpan> GetCacheFromDb()
        {
            return CovSpanCrud.SelectMany("SELECT * FROM covspan ORDER BY FromCode");
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

    private static readonly CovSpanCache Cache = new();

    public static List<CovSpan> GetWhere(Predicate<CovSpan> match, bool isShort = false)
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