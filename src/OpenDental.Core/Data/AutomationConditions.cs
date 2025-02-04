using System.Collections.Generic;
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
}