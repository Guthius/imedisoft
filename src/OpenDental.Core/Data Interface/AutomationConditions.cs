using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AutomationConditions
{
    public static List<AutomationCondition> GetListByAutomationNum(long automationNum)
    {
        var command = "SELECT * FROM automationcondition WHERE AutomationNum = " + SOut.Long(automationNum);
        return AutomationConditionCrud.SelectMany(command);
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
        var command = "DELETE FROM automationcondition WHERE AutomationConditionNum = " + SOut.Long(automationConditionNum);
        Db.NonQ(command);
    }

    public static void DeleteByAutomationNum(long automationNum)
    {
        var command = "DELETE FROM automationcondition WHERE AutomationNum = " + SOut.Long(automationNum);
        Db.NonQ(command);
    }
    
    private class AutomationConditionCache : CacheListAbs<AutomationCondition>
    {
        protected override List<AutomationCondition> GetCacheFromDb()
        {
            var command = "SELECT * FROM automationcondition";
            return AutomationConditionCrud.SelectMany(command);
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