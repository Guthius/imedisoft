using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class EFormImportRules
{
    public static void Insert(EFormImportRule eFormImportRule)
    {
        EFormImportRuleCrud.Insert(eFormImportRule);
    }

    public static void Update(EFormImportRule eFormImportRule)
    {
        EFormImportRuleCrud.Update(eFormImportRule);
    }

    public static void Delete(long eFormImportRuleNum)
    {
        EFormImportRuleCrud.Delete(eFormImportRuleNum);
    }

    public static bool IsAllowedSit(string fieldName, EnumEFormImportSituation eformImportSituation)
    {
        if (eformImportSituation != EnumEFormImportSituation.Invalid)
        {
            return true;
        }
        
        return fieldName is 
            "Address" or 
            "Address2" or 
            "City" or 
            "State" or 
            "Zip" or
            "SSN" or 
            "Birthdate" or 
            "Email" or
            "HmPhone" or 
            "ICEPhone" or 
            "ins1CarrierPhone" or 
            "ins2CarrierPhone" or
            "WirelessPhone" or 
            "WkPhone";
    }

    public static bool IsAllowedAction(string fieldName, EnumEFormImportAction eformImportAction)
    {
        if (eformImportAction != EnumEFormImportAction.Fix)
        {
            return true;
        }
        
        return fieldName is 
            "Address" or 
            "Address2" or 
            "City" or 
            "State" or 
            "Zip" or 
            "HmPhone" or 
            "ICEPhone" or 
            "ins1CarrierPhone" or
            "ins2CarrierPhone" or 
            "WirelessPhone" or 
            "WkPhone" or
            "FName" or 
            "MiddleI" or 
            "LName";
    }

    private class EFormImportRuleCache : CacheListAbs<EFormImportRule>
    {
        protected override List<EFormImportRule> GetCacheFromDb()
        {
            return EFormImportRuleCrud.SelectMany("SELECT * FROM eformimportrule");
        }

        protected override List<EFormImportRule> TableToList(DataTable dataTable)
        {
            return EFormImportRuleCrud.TableToList(dataTable);
        }

        protected override EFormImportRule Copy(EFormImportRule item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EFormImportRule> items)
        {
            return EFormImportRuleCrud.ListToTable(items, "EFormImportRule");
        }

        protected override void FillCacheIfNeeded()
        {
            EFormImportRules.GetTableFromCache(false);
        }
    }

    private static readonly EFormImportRuleCache Cache = new();

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<EFormImportRule> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }
}