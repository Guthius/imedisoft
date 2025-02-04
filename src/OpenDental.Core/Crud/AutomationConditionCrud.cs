using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AutomationConditionCrud
{
    public static List<AutomationCondition> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutomationCondition> TableToList(DataTable table)
    {
        var retVal = new List<AutomationCondition>();
        foreach (DataRow row in table.Rows)
        {
            var automationCondition = new AutomationCondition
            {
                AutomationConditionNum = SIn.Long(row["AutomationConditionNum"].ToString()),
                AutomationNum = SIn.Long(row["AutomationNum"].ToString()),
                CompareField = (AutoCondField) SIn.Int(row["CompareField"].ToString()),
                Comparison = (AutoCondComparison) SIn.Int(row["Comparison"].ToString()),
                CompareString = SIn.String(row["CompareString"].ToString())
            };
            retVal.Add(automationCondition);
        }

        return retVal;
    }

    public static void Insert(AutomationCondition automationCondition)
    {
        var command = "INSERT INTO automationcondition (";

        command += "AutomationNum,CompareField,Comparison,CompareString) VALUES(";

        command +=
            SOut.Long(automationCondition.AutomationNum) + ","
                                                         + SOut.Int((int) automationCondition.CompareField) + ","
                                                         + SOut.Int((int) automationCondition.Comparison) + ","
                                                         + "'" + SOut.String(automationCondition.CompareString) + "')";
        {
            automationCondition.AutomationConditionNum = Db.NonQ(command, true, "AutomationConditionNum", "automationCondition");
        }
    }

    public static void Update(AutomationCondition automationCondition)
    {
        var command = "UPDATE automationcondition SET "
                      + "AutomationNum         =  " + SOut.Long(automationCondition.AutomationNum) + ", "
                      + "CompareField          =  " + SOut.Int((int) automationCondition.CompareField) + ", "
                      + "Comparison            =  " + SOut.Int((int) automationCondition.Comparison) + ", "
                      + "CompareString         = '" + SOut.String(automationCondition.CompareString) + "' "
                      + "WHERE AutomationConditionNum = " + SOut.Long(automationCondition.AutomationConditionNum);
        Db.NonQ(command);
    }
}