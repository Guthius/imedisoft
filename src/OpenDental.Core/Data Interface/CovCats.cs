using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CovCats
{
    #region Delete

    public static void Delete(CovCat covCat)
    {
        var command = "DELETE FROM covcat "
                      + "WHERE CovCatNum = '" + SOut.Long(covCat.CovCatNum) + "'";
        Db.NonQ(command);
    }

    #endregion

    
    public static void Update(CovCat covcat)
    {
        CovCatCrud.Update(covcat);
    }

    
    public static long Insert(CovCat covcat)
    {
        return CovCatCrud.Insert(covcat);
    }

    /// <summary>
    ///     Does not update the cache.  The cache must be manually refreshed after using this method beccause it only
    ///     updates the database.
    /// </summary>
    public static void MoveUp(CovCat covcat)
    {
        var listCovCats = GetDeepCopy();
        var oldOrder = listCovCats.FindIndex(x => x.CovCatNum == covcat.CovCatNum);
        if (oldOrder == 0 || oldOrder == -1) return;
        SetOrder(listCovCats[oldOrder], oldOrder - 1);
        SetOrder(listCovCats[oldOrder - 1], oldOrder);
    }

    /// <summary>
    ///     Does not update the cache.  The cache must be manually refreshed after using this method beccause it only
    ///     updates the database.
    /// </summary>
    public static void MoveDown(CovCat covcat)
    {
        var listCovCats = GetDeepCopy();
        var oldOrder = listCovCats.FindIndex(x => x.CovCatNum == covcat.CovCatNum);
        if (oldOrder == listCovCats.Count - 1 || oldOrder == -1) return;
        SetOrder(listCovCats[oldOrder], oldOrder + 1);
        SetOrder(listCovCats[oldOrder + 1], oldOrder);
    }

    
    private static void SetOrder(CovCat covcat, int newOrder)
    {
        covcat.CovOrder = newOrder;
        Update(covcat);
    }

    
    public static CovCat GetCovCat(long covCatNum)
    {
        return GetFirstOrDefault(x => x.CovCatNum == covCatNum);
    }

    
    public static List<CovCat> GetCovCats(List<long> listCovCatNums)
    {
        return GetWhere(x => listCovCatNums.Contains(x.CovCatNum));
    }

    
    public static double GetDefaultPercent(long covCatNum)
    {
        var covCat = GetFirstOrDefault(x => x.CovCatNum == covCatNum);
        if (covCat == null) return 0;
        return covCat.DefaultPercent;
    }

    
    public static string GetDesc(long covCatNum)
    {
        var covCat = GetLastOrDefault(x => x.CovCatNum == covCatNum);
        if (covCat == null) return "";
        return covCat.Description;
    }

    
    public static long GetCovCatNum(int orderShort)
    {
        var covCat = GetLastOrDefault(x => x.CovOrder == orderShort, true);
        if (covCat == null) return 0;
        return covCat.CovCatNum;
    }

    ///<summary>Returns -1 if not in ListShort.</summary>
    public static int GetOrderShort(long covCatNum)
    {
        return GetFindIndex(x => x.CovCatNum == covCatNum, true);
    }

    ///<summary>Returns -1 if not in the provided list.</summary>
    public static int GetOrderShort(long covCatNum, List<CovCat> listCovCats)
    {
        var retVal = -1;
        for (var i = 0; i < listCovCats.Count; i++)
            if (covCatNum == listCovCats[i].CovCatNum)
                retVal = i;

        return retVal;
    }

    ///<summary>Gets a matching benefit category from the short list.  Returns null if not found, which should be tested for.</summary>
    public static CovCat GetForEbenCat(EbenefitCategory ebenefitCategory)
    {
        return GetFirstOrDefault(x => x.EbenefitCat == ebenefitCategory, true);
    }

    public static CovCat GetForDesc(string description)
    {
        return GetFirstOrDefault(x => x.Description == description, true);
    }

    ///<summary>If none assigned, it will return None.</summary>
    public static EbenefitCategory GetEbenCat(long covCatNum)
    {
        var covCat = GetFirstOrDefault(x => x.CovCatNum == covCatNum, true);
        if (covCat == null) return EbenefitCategory.None;
        return covCat.EbenefitCat;
    }

    public static int CountForEbenCat(EbenefitCategory ebenefitCategory)
    {
        return GetWhere(x => x.EbenefitCat == ebenefitCategory, true).Count;
    }

    public static void SetOrdersToDefault()
    {
        //This can only be run if the validation checks have been run first.
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            SetOrder(GetForEbenCat(EbenefitCategory.General), 0);
            SetOrder(GetForEbenCat(EbenefitCategory.Diagnostic), 1);
            SetOrder(GetForEbenCat(EbenefitCategory.DiagnosticXRay), 2);
            SetOrder(GetForEbenCat(EbenefitCategory.RoutinePreventive), 3);
            SetOrder(GetForEbenCat(EbenefitCategory.Restorative), 4);
            SetOrder(GetForEbenCat(EbenefitCategory.Crowns), 5);
            SetOrder(GetForEbenCat(EbenefitCategory.Endodontics), 6);
            SetOrder(GetForEbenCat(EbenefitCategory.Periodontics), 7);
            SetOrder(GetForEbenCat(EbenefitCategory.Prosthodontics), 8);
            SetOrder(GetForEbenCat(EbenefitCategory.MaxillofacialProsth), 9);
            SetOrder(GetForEbenCat(EbenefitCategory.OralSurgery), 10);
            SetOrder(GetForEbenCat(EbenefitCategory.Orthodontics), 11);
            SetOrder(GetForEbenCat(EbenefitCategory.Adjunctive), 12);
            SetOrder(GetForDesc("Implants"), 13);
            SetOrder(GetForEbenCat(EbenefitCategory.Accident), 14);
            SetOrder(GetForDesc("SC/RP"), 15);
        }
        else
        {
            SetOrder(GetForEbenCat(EbenefitCategory.General), 0);
            SetOrder(GetForEbenCat(EbenefitCategory.Diagnostic), 1);
            SetOrder(GetForEbenCat(EbenefitCategory.DiagnosticXRay), 2);
            SetOrder(GetForEbenCat(EbenefitCategory.RoutinePreventive), 3);
            SetOrder(GetForEbenCat(EbenefitCategory.Restorative), 4);
            SetOrder(GetForEbenCat(EbenefitCategory.Endodontics), 5);
            SetOrder(GetForEbenCat(EbenefitCategory.Periodontics), 6);
            SetOrder(GetForEbenCat(EbenefitCategory.OralSurgery), 7);
            SetOrder(GetForEbenCat(EbenefitCategory.Crowns), 8);
            SetOrder(GetForEbenCat(EbenefitCategory.Prosthodontics), 9);
            SetOrder(GetForEbenCat(EbenefitCategory.MaxillofacialProsth), 10);
            SetOrder(GetForEbenCat(EbenefitCategory.Accident), 11);
            SetOrder(GetForEbenCat(EbenefitCategory.Orthodontics), 12);
            SetOrder(GetForEbenCat(EbenefitCategory.Adjunctive), 13);
        }

        //now set the remaining categories to come after the ebens.
        var idx = 14;
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) idx = 16;
        var listCovCatsShort = GetWhere(x => x.EbenefitCat == EbenefitCategory.None, true);
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) listCovCatsShort.RemoveAll(x => x.Description == "Implants" || x.Description == "SC/RP");
        for (var i = 0; i < listCovCatsShort.Count; i++)
        {
            SetOrder(listCovCatsShort[i], idx);
            idx++;
        }

        //finally, the hidden categories
        var listCovCats = GetWhere(x => x.EbenefitCat == EbenefitCategory.None && x.IsHidden);
        for (var i = 0; i < listCovCats.Count; i++)
        {
            SetOrder(listCovCats[i], idx);
            idx++;
        }
    }

    public static void SetSpansToDefault()
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
            SetSpansToDefaultCanada();
        else
            SetSpansToDefaultUsa();
    }

    public static void SetSpansToDefaultUsa()
    {
        //This can only be run if the validation checks have been run first.
        long covCatNum;
        CovSpan covSpan;
        covCatNum = GetForEbenCat(EbenefitCategory.General).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D0000";
        covSpan.ToCode = "D7999";
        CovSpans.Insert(covSpan);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D9000";
        covSpan.ToCode = "D9999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Diagnostic).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D0000";
        covSpan.ToCode = "D0999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.DiagnosticXRay).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D0200";
        covSpan.ToCode = "D0399";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.RoutinePreventive).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D1000";
        covSpan.ToCode = "D1999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Restorative).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D2000";
        covSpan.ToCode = "D2699";
        CovSpans.Insert(covSpan);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D2800";
        covSpan.ToCode = "D2999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Endodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D3000";
        covSpan.ToCode = "D3999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Periodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D4000";
        covSpan.ToCode = "D4999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.OralSurgery).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D7000";
        covSpan.ToCode = "D7999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Crowns).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D2700";
        covSpan.ToCode = "D2799";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Prosthodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D5000";
        covSpan.ToCode = "D5899";
        CovSpans.Insert(covSpan);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D6200";
        covSpan.ToCode = "D6899";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.MaxillofacialProsth).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D5900";
        covSpan.ToCode = "D5999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Accident).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covCatNum = GetForEbenCat(EbenefitCategory.Orthodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D8000";
        covSpan.ToCode = "D8999";
        CovSpans.Insert(covSpan);
        covCatNum = GetForEbenCat(EbenefitCategory.Adjunctive).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covSpan = new CovSpan();
        covSpan.CovCatNum = covCatNum;
        covSpan.FromCode = "D9000";
        covSpan.ToCode = "D9999";
        CovSpans.Insert(covSpan);
    }

    public static void SetSpansToDefaultCanada()
    {
        //This can only be run if the validation checks have been run first.
        RecreateSpansForCategory(EbenefitCategory.General, "00000-99999");
        RecreateSpansForCategory(EbenefitCategory.Diagnostic, "01000-09999");
        RecreateSpansForCategory(EbenefitCategory.DiagnosticXRay, "02000-02999");
        RecreateSpansForCategory(EbenefitCategory.RoutinePreventive, "10000-19999");
        RecreateSpansForCategory(EbenefitCategory.Restorative, "20000-26999", "28000-29999");
        RecreateSpansForCategory(EbenefitCategory.Crowns, "27000-27999");
        RecreateSpansForCategory(EbenefitCategory.Endodontics, "30000-39999");
        RecreateSpansForCategory(EbenefitCategory.Periodontics, "40000-49999");
        RecreateSpansForCategory(EbenefitCategory.Prosthodontics, "50000-56999", "58000-69999");
        RecreateSpansForCategory(EbenefitCategory.MaxillofacialProsth, "57000-57999");
        RecreateSpansForCategory(EbenefitCategory.OralSurgery, "70000-79999");
        RecreateSpansForCategory(EbenefitCategory.Orthodontics, "01901-01901", "80000-89999", "93330-93349");
        RecreateSpansForCategory(EbenefitCategory.Adjunctive, "90000-93329", "93350-99999");
        RecreateSpansForCategoryCanada("Implants", "79900-79999");
        RecreateSpansForCategory(EbenefitCategory.Accident);
        RecreateSpansForCategoryCanada("SC/RP", "11111-11119", "43421-43429");
    }

    private static void SetSpansForCovCatNum(long covCatNum, params string[] stringArrayCodeRanges)
    {
        CovSpans.DeleteForCat(covCatNum);
        for (var i = 0; i < stringArrayCodeRanges.Length; i++)
        {
            var codeRange = stringArrayCodeRanges[i];
            var covSpan = new CovSpan();
            covSpan.CovCatNum = covCatNum;
            if (codeRange.Contains("-"))
            {
                //Code range
                covSpan.FromCode = codeRange.Substring(0, codeRange.IndexOf("-"));
                covSpan.ToCode = codeRange.Substring(covSpan.FromCode.Length + 1);
            }
            else
            {
                //Single code
                covSpan.FromCode = codeRange;
                covSpan.ToCode = codeRange;
            }

            CovSpans.Insert(covSpan);
        }
    }

    /// <summary>
    ///     Deletes the current CovSpans for the given eBenefitCategory, then creates new code ranges from the ranges
    ///     specified in arrayCodeRanges.  The values in arrayCodeRanges can be a single code such as "D0120" or a code range
    ///     such as "D9000-D9999".
    /// </summary>
    private static void RecreateSpansForCategory(EbenefitCategory eBenefitCategory, params string[] stringArrayCodeRanges)
    {
        SetSpansForCovCatNum(GetForEbenCat(eBenefitCategory).CovCatNum, stringArrayCodeRanges);
    }

    private static void RecreateSpansForCategoryCanada(string categoryName, params string[] stringArrayCodeRanges)
    {
        var covCat = GetForDesc(categoryName);
        if (covCat == null)
        {
            covCat = new CovCat();
            covCat.Description = categoryName;
            covCat.EbenefitCat = EbenefitCategory.None;
            covCat.DefaultPercent = -1;
            covCat.CovOrder = GetDeepCopy().Count;
            covCat.IsHidden = false;
            Insert(covCat);
            RefreshCache();
        }

        SetSpansForCovCatNum(covCat.CovCatNum, stringArrayCodeRanges);
    }

    #region Get Methods

    ///<summary>Returns a distinct list of valid ProcCodes for the given eBenefitCat.</summary>
    public static List<string> GetValidCodesForEbenCat(EbenefitCategory ebenefitCategory)
    {
        var listStringsValid = new List<string>();
        var listCovCats = GetWhere(x => x.EbenefitCat == ebenefitCategory, true);
        for (var i = 0; i < listCovCats.Count; i++)
        {
            var listCovSpans = CovSpans.GetForCat(listCovCats[i].CovCatNum);
            listStringsValid.AddRange(
                ProcedureCodes.GetWhere(x => CovSpans.IsCodeInSpans(x.ProcCode, listCovSpans), true).Select(x => x.ProcCode).ToList()
            );
        }

        return listStringsValid.Distinct().ToList();
    }

    /// <summary>
    ///     Pass in list of procedures and covCat, return the sum of all CanadaTimeUnits of the procedures in that covCat
    ///     as a double.
    /// </summary>
    public static double GetAmtUsedForCat(List<Procedure> listProcedures, CovCat covCat)
    {
        var listProcedureCodes = new List<ProcedureCode>();
        for (var i = 0; i < listProcedures.Count; i++) listProcedureCodes.Add(ProcedureCodes.GetProcCode(listProcedures[i].CodeNum)); //turn list of procedures into list of procedurecodes.
        double total = 0; //CanadaTimeUnits can be decimal numbers, like 0.5.
        for (var i = 0; i < listProcedureCodes.Count; i++)
        {
            //for every procedurecode
            var listCovCatsForProc = GetCovCats(CovSpans.GetCats(listProcedureCodes[i].ProcCode));
            if (listCovCatsForProc.Any(x => x.CovCatNum == covCat.CovCatNum)) total += listProcedureCodes[i].CanadaTimeUnits; //add the Canada time units to the total.
        }

        return total;
    }

    #endregion

    #region CachePattern

    private class CovCatCache : CacheListAbs<CovCat>
    {
        protected override List<CovCat> GetCacheFromDb()
        {
            var command = "SELECT * FROM covcat ORDER BY CovOrder";
            return CovCatCrud.SelectMany(command);
        }

        protected override List<CovCat> TableToList(DataTable dataTable)
        {
            return CovCatCrud.TableToList(dataTable);
        }

        protected override CovCat Copy(CovCat item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<CovCat> items)
        {
            return CovCatCrud.ListToTable(items, "CovCat");
        }

        protected override void FillCacheIfNeeded()
        {
            CovCats.GetTableFromCache(false);
        }

        protected override bool IsInListShort(CovCat item)
        {
            return !item.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly CovCatCache _CovCatsache = new();

    public static List<CovCat> GetDeepCopy(bool isShort = false)
    {
        return _CovCatsache.GetDeepCopy(isShort);
    }

    public static int GetFindIndex(Predicate<CovCat> match, bool isShort = false)
    {
        return _CovCatsache.GetFindIndex(match, isShort);
    }

    public static CovCat GetFirst(bool isShort = false)
    {
        return _CovCatsache.GetFirst(isShort);
    }

    public static CovCat GetFirstOrDefault(Func<CovCat, bool> match, bool isShort = false)
    {
        return _CovCatsache.GetFirstOrDefault(match, isShort);
    }

    public static CovCat GetLastOrDefault(Func<CovCat, bool> match, bool isShort = false)
    {
        return _CovCatsache.GetLastOrDefault(match, isShort);
    }

    public static List<CovCat> GetWhere(Predicate<CovCat> match, bool isShort = false)
    {
        return _CovCatsache.GetWhere(match, isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _CovCatsache.GetCount(isShort);
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
        _CovCatsache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _CovCatsache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _CovCatsache.ClearCache();
    }

    #endregion
}