using System;
using System.Collections.Generic;
using System.Data;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EFormImportRules
{
    
    public static long Insert(EFormImportRule eFormImportRule)
    {
        return EFormImportRuleCrud.Insert(eFormImportRule);
    }

    
    public static void Update(EFormImportRule eFormImportRule)
    {
        EFormImportRuleCrud.Update(eFormImportRule);
    }

    
    public static void Delete(long eFormImportRuleNum)
    {
        EFormImportRuleCrud.Delete(eFormImportRuleNum);
    }

    public static bool isAllowedSit(string fieldName, EnumEFormImportSituation enumEFormImportSituation)
    {
        if (enumEFormImportSituation != EnumEFormImportSituation.Invalid) return true;
        if (fieldName.In("Address", "Address2", "City", "State", "Zip",
                "SSN", "Birthdate", "Email",
                "HmPhone", "ICEPhone", "ins1CarrierPhone", "ins2CarrierPhone", "WirelessPhone", "WkPhone"))
            return true;
        return false;
    }

    public static bool isAllowedAction(string fieldName, EnumEFormImportAction enumEFormImportAction)
    {
        if (enumEFormImportAction != EnumEFormImportAction.Fix) return true;
        if (fieldName.In("Address", "Address2", "City", "State", "Zip",
                "HmPhone", "ICEPhone", "ins1CarrierPhone", "ins2CarrierPhone", "WirelessPhone", "WkPhone",
                "FName", "MiddleI", "LName"))
            return true;
        return false;
    }

    #region Cache Pattern

    private class EFormImportRuleCache : CacheListAbs<EFormImportRule>
    {
        protected override List<EFormImportRule> GetCacheFromDb()
        {
            var command = "SELECT * FROM eformimportrule";
            return EFormImportRuleCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EFormImportRuleCache _eFormImportRuleCache = new();

    public static void ClearCache()
    {
        _eFormImportRuleCache.ClearCache();
    }

    public static List<EFormImportRule> GetDeepCopy(bool isShort = false)
    {
        return _eFormImportRuleCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _eFormImportRuleCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<EFormImportRule> match, bool isShort = false)
    {
        return _eFormImportRuleCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<EFormImportRule> match, bool isShort = false)
    {
        return _eFormImportRuleCache.GetFindIndex(match, isShort);
    }

    public static EFormImportRule GetFirst(bool isShort = false)
    {
        return _eFormImportRuleCache.GetFirst(isShort);
    }

    public static EFormImportRule GetFirst(Func<EFormImportRule, bool> match, bool isShort = false)
    {
        return _eFormImportRuleCache.GetFirst(match, isShort);
    }

    public static EFormImportRule GetFirstOrDefault(Func<EFormImportRule, bool> match, bool isShort = false)
    {
        return _eFormImportRuleCache.GetFirstOrDefault(match, isShort);
    }

    public static EFormImportRule GetLast(bool isShort = false)
    {
        return _eFormImportRuleCache.GetLast(isShort);
    }

    public static EFormImportRule GetLastOrDefault(Func<EFormImportRule, bool> match, bool isShort = false)
    {
        return _eFormImportRuleCache.GetLastOrDefault(match, isShort);
    }

    public static List<EFormImportRule> GetWhere(Predicate<EFormImportRule> match, bool isShort = false)
    {
        return _eFormImportRuleCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _eFormImportRuleCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _eFormImportRuleCache.GetTableFromCache(doRefreshCache);
    }

    #endregion Cache Pattern


    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Methods - Get
    
    public static List<EFormImportRule> Refresh(long patNum){

        string command="SELECT * FROM eformimportrule WHERE PatNum = "+POut.Long(patNum);
        return Crud.EFormImportRuleCrud.SelectMany(command);
    }

    ///<summary>Gets one EFormImportRule from the db.</summary>
    public static EFormImportRule GetOne(long eFormImportRuleNum){

        return Crud.EFormImportRuleCrud.SelectOne(eFormImportRuleNum);
    }
    #endregion Methods - Get

    */
}