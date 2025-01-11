using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        AutomationCondition automationCondition;
        foreach (DataRow row in table.Rows)
        {
            automationCondition = new AutomationCondition();
            automationCondition.AutomationConditionNum = SIn.Long(row["AutomationConditionNum"].ToString());
            automationCondition.AutomationNum = SIn.Long(row["AutomationNum"].ToString());
            automationCondition.CompareField = (AutoCondField) SIn.Int(row["CompareField"].ToString());
            automationCondition.Comparison = (AutoCondComparison) SIn.Int(row["Comparison"].ToString());
            automationCondition.CompareString = SIn.String(row["CompareString"].ToString());
            retVal.Add(automationCondition);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AutomationCondition> listAutomationConditions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AutomationCondition";
        var table = new DataTable(tableName);
        table.Columns.Add("AutomationConditionNum");
        table.Columns.Add("AutomationNum");
        table.Columns.Add("CompareField");
        table.Columns.Add("Comparison");
        table.Columns.Add("CompareString");
        foreach (var automationCondition in listAutomationConditions)
            table.Rows.Add(SOut.Long(automationCondition.AutomationConditionNum), SOut.Long(automationCondition.AutomationNum), SOut.Int((int) automationCondition.CompareField), SOut.Int((int) automationCondition.Comparison), automationCondition.CompareString);
        return table;
    }

    public static long Insert(AutomationCondition automationCondition)
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
        return automationCondition.AutomationConditionNum;
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