using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class CovCats
{
    public static void Update(CovCat covcat)
    {
        CovCatCrud.Update(covcat);
    }

    public static void Insert(CovCat covcat)
    {
        CovCatCrud.Insert(covcat);
    }

    public static void MoveUp(CovCat covcat)
    {
        var listCovCats = GetDeepCopy();

        var oldOrder = listCovCats.FindIndex(x => x.CovCatNum == covcat.CovCatNum);
        if (oldOrder is 0 or -1)
        {
            return;
        }

        SetOrder(listCovCats[oldOrder], oldOrder - 1);
        SetOrder(listCovCats[oldOrder - 1], oldOrder);
    }

    public static void MoveDown(CovCat covcat)
    {
        var listCovCats = GetDeepCopy();

        var oldOrder = listCovCats.FindIndex(x => x.CovCatNum == covcat.CovCatNum);
        if (oldOrder == listCovCats.Count - 1 || oldOrder == -1)
        {
            return;
        }

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

    public static string GetDesc(long covCatNum)
    {
        var covCat = GetLastOrDefault(x => x.CovCatNum == covCatNum);
        return covCat == null ? "" : covCat.Description;
    }

    public static int GetOrderShort(long covCatNum)
    {
        return GetFindIndex(x => x.CovCatNum == covCatNum, true);
    }

    public static CovCat GetForEbenCat(EbenefitCategory ebenefitCategory)
    {
        return GetFirstOrDefault(x => x.EbenefitCat == ebenefitCategory, true);
    }

    public static CovCat GetForDesc(string description)
    {
        return GetFirstOrDefault(x => x.Description == description, true);
    }

    public static EbenefitCategory GetEbenCat(long covCatNum)
    {
        var covCat = GetFirstOrDefault(x => x.CovCatNum == covCatNum, true);
        
        return covCat?.EbenefitCat ?? EbenefitCategory.None;
    }

    public static int CountForEbenCat(EbenefitCategory ebenefitCategory)
    {
        return GetWhere(x => x.EbenefitCat == ebenefitCategory, true).Count;
    }

    public static void SetOrdersToDefault()
    {
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

        var idx = 14;
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            idx = 16;
        }

        var covCatsShort = GetWhere(x => x.EbenefitCat == EbenefitCategory.None, true);
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            covCatsShort.RemoveAll(x => x.Description is "Implants" or "SC/RP");
        }

        foreach (var covCat in covCatsShort)
        {
            SetOrder(covCat, idx);
            idx++;
        }

        var covCats = GetWhere(x => x.EbenefitCat == EbenefitCategory.None && x.IsHidden);
        foreach (var covCat in covCats)
        {
            SetOrder(covCat, idx);
            idx++;
        }
    }

    public static void SetSpansToDefault()
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            SetSpansToDefaultCanada();
        }
        else
        {
            SetSpansToDefaultUsa();
        }
    }

    public static void SetSpansToDefaultUsa()
    {
        var covCatNum = GetForEbenCat(EbenefitCategory.General).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D0000",
            ToCode = "D7999"
        });
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D9000",
            ToCode = "D9999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Diagnostic).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D0000",
            ToCode = "D0999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.DiagnosticXRay).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D0200",
            ToCode = "D0399"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.RoutinePreventive).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D1000",
            ToCode = "D1999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Restorative).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D2000",
            ToCode = "D2699"
        });
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D2800",
            ToCode = "D2999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Endodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D3000",
            ToCode = "D3999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Periodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D4000",
            ToCode = "D4999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.OralSurgery).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D7000",
            ToCode = "D7999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Crowns).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D2700",
            ToCode = "D2799"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Prosthodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D5000",
            ToCode = "D5899"
        });
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D6200",
            ToCode = "D6899"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.MaxillofacialProsth).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D5900",
            ToCode = "D5999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Accident).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        covCatNum = GetForEbenCat(EbenefitCategory.Orthodontics).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D8000",
            ToCode = "D8999"
        });
        covCatNum = GetForEbenCat(EbenefitCategory.Adjunctive).CovCatNum;
        CovSpans.DeleteForCat(covCatNum);
        CovSpans.Insert(new CovSpan
        {
            CovCatNum = covCatNum,
            FromCode = "D9000",
            ToCode = "D9999"
        });
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

    private static void SetSpansForCovCatNum(long covCatNum, params string[] codeRanges)
    {
        CovSpans.DeleteForCat(covCatNum);
        
        foreach (var codeRange in codeRanges)
        {
            var covSpan = new CovSpan
            {
                CovCatNum = covCatNum
            };

            var dash = codeRange.IndexOf('-');
            if (dash != -1)
            {
                covSpan.FromCode = codeRange.Substring(0, dash);
                covSpan.ToCode = codeRange.Substring(dash + 1);
            }
            else
            {
                covSpan.FromCode = codeRange;
                covSpan.ToCode = codeRange;
            }

            CovSpans.Insert(covSpan);
        }
    }
    
    private static void RecreateSpansForCategory(EbenefitCategory eBenefitCategory, params string[] stringArrayCodeRanges)
    {
        SetSpansForCovCatNum(GetForEbenCat(eBenefitCategory).CovCatNum, stringArrayCodeRanges);
    }

    private static void RecreateSpansForCategoryCanada(string categoryName, params string[] stringArrayCodeRanges)
    {
        var covCat = GetForDesc(categoryName);
        if (covCat == null)
        {
            covCat = new CovCat
            {
                Description = categoryName,
                EbenefitCat = EbenefitCategory.None,
                DefaultPercent = -1,
                CovOrder = GetDeepCopy().Count,
                IsHidden = false
            };
            
            Insert(covCat);
            
            RefreshCache();
        }

        SetSpansForCovCatNum(covCat.CovCatNum, stringArrayCodeRanges);
    }
    
    public static List<string> GetValidCodesForEbenCat(EbenefitCategory ebenefitCategory)
    {
        var validStrings = new List<string>();
        var covCats = GetWhere(x => x.EbenefitCat == ebenefitCategory, true);
        
        foreach (var covCat in covCats)
        {
            var covSpans = CovSpans.GetForCat(covCat.CovCatNum);
            
            validStrings.AddRange(ProcedureCodes
                .GetWhere(x => CovSpans.IsCodeInSpans(x.ProcCode, covSpans), true)
                .Select(x => x.ProcCode));
        }

        return validStrings.Distinct().ToList();
    }
    
    public static double GetAmtUsedForCat(List<Procedure> procedures, CovCat covCat)
    {
        var procedureCodes = new List<ProcedureCode>();
        foreach (var procedure in procedures)
        {
            procedureCodes.Add(ProcedureCodes.GetProcCode(procedure.CodeNum));
        }
        
        double total = 0;
        foreach (var procedureCode in procedureCodes)
        {
            var covCatsForProc = GetCovCats(CovSpans.GetCats(procedureCode.ProcCode));
            if (covCatsForProc.Any(x => x.CovCatNum == covCat.CovCatNum))
            {
                total += procedureCode.CanadaTimeUnits;
            }
        }

        return total;
    }
    
    private class CovCatCache : CacheListAbs<CovCat>
    {
        protected override List<CovCat> GetCacheFromDb()
        {
            return CovCatCrud.SelectMany("SELECT * FROM covcat ORDER BY CovOrder");
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

    private static readonly CovCatCache Cache = new();

    public static List<CovCat> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static int GetFindIndex(Predicate<CovCat> match, bool isShort = false)
    {
        return Cache.GetFindIndex(match, isShort);
    }

    public static CovCat GetFirst(bool isShort = false)
    {
        return Cache.GetFirst(isShort);
    }

    public static CovCat GetFirstOrDefault(Func<CovCat, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static CovCat GetLastOrDefault(Func<CovCat, bool> match, bool isShort = false)
    {
        return Cache.GetLastOrDefault(match, isShort);
    }

    public static List<CovCat> GetWhere(Predicate<CovCat> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
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