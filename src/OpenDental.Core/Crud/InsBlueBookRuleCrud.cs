using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsBlueBookRuleCrud
{
    public static List<InsBlueBookRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsBlueBookRule> TableToList(DataTable table)
    {
        var retVal = new List<InsBlueBookRule>();
        InsBlueBookRule insBlueBookRule;
        foreach (DataRow row in table.Rows)
        {
            insBlueBookRule = new InsBlueBookRule();
            insBlueBookRule.InsBlueBookRuleNum = SIn.Long(row["InsBlueBookRuleNum"].ToString());
            insBlueBookRule.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            insBlueBookRule.RuleType = (InsBlueBookRuleType) SIn.Int(row["RuleType"].ToString());
            insBlueBookRule.LimitValue = SIn.Int(row["LimitValue"].ToString());
            insBlueBookRule.LimitType = (InsBlueBookRuleLimitType) SIn.Int(row["LimitType"].ToString());
            retVal.Add(insBlueBookRule);
        }

        return retVal;
    }

    public static void Update(InsBlueBookRule insBlueBookRule, InsBlueBookRule oldInsBlueBookRule)
    {
        var command = "";
        if (insBlueBookRule.ItemOrder != oldInsBlueBookRule.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(insBlueBookRule.ItemOrder) + "";
        }

        if (insBlueBookRule.RuleType != oldInsBlueBookRule.RuleType)
        {
            if (command != "") command += ",";
            command += "RuleType = " + SOut.Int((int) insBlueBookRule.RuleType) + "";
        }

        if (insBlueBookRule.LimitValue != oldInsBlueBookRule.LimitValue)
        {
            if (command != "") command += ",";
            command += "LimitValue = " + SOut.Int(insBlueBookRule.LimitValue) + "";
        }

        if (insBlueBookRule.LimitType != oldInsBlueBookRule.LimitType)
        {
            if (command != "") command += ",";
            command += "LimitType = " + SOut.Int((int) insBlueBookRule.LimitType) + "";
        }

        if (command == "") return;
        command = "UPDATE insbluebookrule SET " + command
                                                + " WHERE InsBlueBookRuleNum = " + SOut.Long(insBlueBookRule.InsBlueBookRuleNum);
        Db.NonQ(command);
    }
}