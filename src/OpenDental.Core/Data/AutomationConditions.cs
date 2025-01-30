using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AutomationConditions
{
    public static List<AutomationCondition> GetListByAutomationNum(long automationNum)
    {
        return AutomationConditionCrud.SelectMany("SELECT * FROM automationcondition WHERE AutomationNum = " + automationNum);
    }

    public static void Insert(AutomationCondition automationCondition)
    {
        AutomationConditionCrud.Insert(automationCondition);
    }

    public static void Update(AutomationCondition automationCondition)
    {
        AutomationConditionCrud.Update(automationCondition);
    }

    public static void Delete(long automationConditionNum)
    {
        Db.NonQ("DELETE FROM automationcondition WHERE AutomationConditionNum = " + automationConditionNum);
    }

    public static void DeleteByAutomationNum(long automationNum)
    {
        Db.NonQ("DELETE FROM automationcondition WHERE AutomationNum = " + automationNum);
    }
    
    private class AutomationConditionCache : CacheListAbs<AutomationCondition>
    {
        protected override List<AutomationCondition> GetCacheFromDb()
        {
            return AutomationConditionCrud.SelectMany("SELECT * FROM automationcondition");
        }

        protected override List<AutomationCondition> TableToList(DataTable dataTable)
        {
            return AutomationConditionCrud.TableToList(dataTable);
        }

        protected override AutomationCondition Copy(AutomationCondition item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<AutomationCondition> items)
        {
            return AutomationConditionCrud.ListToTable(items, "AutomationCondition");
        }

        protected override void FillCacheIfNeeded()
        {
            AutomationConditions.GetTableFromCache(false);
        }
    }
    
    private static readonly AutomationConditionCache Cache = new();

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool doRefreshCache)
    {
        Cache.GetTableFromCache(doRefreshCache);
    }
}