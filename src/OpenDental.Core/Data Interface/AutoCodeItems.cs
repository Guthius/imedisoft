using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class AutoCodeItems
{
    public static void Insert(AutoCodeItem autoCodeItem)
    {
        AutoCodeItemCrud.Insert(autoCodeItem);
    }

    public static void Update(AutoCodeItem autoCodeItem)
    {
        AutoCodeItemCrud.Update(autoCodeItem);
    }

    public static void Delete(AutoCodeItem autoCodeItem)
    {
        Db.NonQ("DELETE FROM autocodeitem WHERE AutoCodeItemNum = " + autoCodeItem.AutoCodeItemNum);
    }

    public static List<AutoCodeItem> GetListForCode(long autoCodeNum)
    {
        return Cache.GetWhereFromList(x => x.AutoCodeNum == autoCodeNum);
    }

    public static long GetCodeNum(long autoCodeNum, string toothNum, string surf, bool isAdditional, int age, bool willBeMissing)
    {
        var autoCodeItemsForCode = GetListForCode(autoCodeNum);
        if (autoCodeItemsForCode.Count == 0)
        {
            return 0;
        }
        
        foreach (var autoCodeItem in autoCodeItemsForCode)
        {
            var autoCodeConds = AutoCodeConds.GetListForItem(autoCodeItem.AutoCodeItemNum);
            var areAllCondsMet = true;
            
            foreach (var autoCodeCond in autoCodeConds)
            {
                if (!AutoCodeConds.ConditionIsMet(autoCodeCond.Cond, toothNum, surf, isAdditional, willBeMissing, age))
                {
                    areAllCondsMet = false;
                }
            }

            if (areAllCondsMet)
            {
                return autoCodeItem.CodeNum;
            }
        }

        return autoCodeItemsForCode[0].CodeNum;
    }

    public static long VerifyCode(long codeNum, string toothNum, string surf, bool isAdditional, long patNum, int age)
    {
        if (!GetContainsKey(codeNum))
        {
            return codeNum;
        }
        
        if (!AutoCodes.GetContainsKey(GetOne(codeNum).AutoCodeNum))
        {
            return codeNum;
        }
        
        var autoCode = AutoCodes.GetOne(GetOne(codeNum).AutoCodeNum);
        if (autoCode.LessIntrusive)
        {
            return codeNum;
        }
        
        var willBeMissing = Procedures.WillBeMissing(toothNum, patNum);
        var autoCodeItems = GetListForCode(GetOne(codeNum).AutoCodeNum);
        
        foreach (var autoCodeItem in autoCodeItems)
        {
            var autoCodeConds = AutoCodeConds.GetListForItem(autoCodeItem.AutoCodeItemNum);
            
            var areAllCondsMet = true;
            foreach (var autoCodeCond in autoCodeConds)
            {
                if (!AutoCodeConds.ConditionIsMet(autoCodeCond.Cond, toothNum, surf, isAdditional, willBeMissing, age))
                {
                    areAllCondsMet = false;
                }
            }

            if (areAllCondsMet)
            {
                return autoCodeItem.CodeNum;
            }
        }

        return codeNum;
    }

    public static long GetRecommendedCodeNum(Procedure procedure, ProcedureCode procedureCode, Patient patient, bool isMandibular, List<ClaimProc> claimProcsForProc)
    {
        var ecommendedCodeNum = procedure.CodeNum;

        if (procedureCode.TreatArea == TreatmentArea.Mouth || procedureCode.TreatArea == TreatmentArea.None || procedureCode.TreatArea == TreatmentArea.Quad || procedureCode.TreatArea == TreatmentArea.Sextant || Procedures.IsAttachedToClaim(procedure, claimProcsForProc))
        {
            return ecommendedCodeNum;
        }
        
        switch (procedureCode.TreatArea)
        {
            case TreatmentArea.Arch when string.IsNullOrEmpty(procedure.Surf):
                return ecommendedCodeNum;
            
            case TreatmentArea.Arch:
                ecommendedCodeNum = VerifyCode(procedureCode.CodeNum, procedure.Surf == "U" ? "1" : "32", "", procedure.IsAdditional, patient.PatNum, patient.Age);
                break;
            
            case TreatmentArea.ToothRange when string.IsNullOrEmpty(procedure.ToothRange):
                return ecommendedCodeNum;
            
            case TreatmentArea.ToothRange:
                ecommendedCodeNum = VerifyCode(procedureCode.CodeNum, isMandibular ? "32" : "1", "", procedure.IsAdditional, patient.PatNum, patient.Age);
                break;
            
            default:
            {
                var claimSurf = Tooth.SurfTidyForClaims(procedure.Surf, procedure.ToothNum);
            
                ecommendedCodeNum = VerifyCode(procedureCode.CodeNum, procedure.ToothNum, claimSurf, procedure.IsAdditional, patient.PatNum, patient.Age);
                
                break;
            }
        }

        return ecommendedCodeNum;
    }

    private class AutoCodeItemCache : CacheDictNonPkAbs<AutoCodeItem, long, AutoCodeItem>
    {
        protected override List<AutoCodeItem> GetCacheFromDb()
        {
            return AutoCodeItemCrud.SelectMany("SELECT * FROM autocodeitem");
        }

        protected override List<AutoCodeItem> TableToList(DataTable dataTable)
        {
            return AutoCodeItemCrud.TableToList(dataTable);
        }

        protected override AutoCodeItem Copy(AutoCodeItem item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<long, AutoCodeItem> dict)
        {
            return AutoCodeItemCrud.ListToTable(dict.Values.ToList(), "AutoCodeItem");
        }

        protected override void FillCacheIfNeeded()
        {
            AutoCodeItems.GetTableFromCache(false);
        }

        protected override long GetDictKey(AutoCodeItem item)
        {
            return item.CodeNum;
        }

        protected override AutoCodeItem GetDictValue(AutoCodeItem item)
        {
            return item;
        }

        protected override AutoCodeItem CopyValue(AutoCodeItem autoCodeItem)
        {
            return autoCodeItem.Copy();
        }

        protected override DataTable ToDataTable(List<AutoCodeItem> items)
        {
            return AutoCodeItemCrud.ListToTable(items);
        }
    }

    private static readonly AutoCodeItemCache Cache = new();

    public static AutoCodeItem GetOne(long codeNum)
    {
        return Cache.GetOne(codeNum);
    }

    public static bool GetContainsKey(long codeNum)
    {
        return Cache.GetContainsKey(codeNum);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}