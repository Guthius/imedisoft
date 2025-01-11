#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsBlueBookRuleCrud
{
    public static InsBlueBookRule SelectOne(long insBlueBookRuleNum)
    {
        var command = "SELECT * FROM insbluebookrule "
                      + "WHERE InsBlueBookRuleNum = " + SOut.Long(insBlueBookRuleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsBlueBookRule SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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

    public static DataTable ListToTable(List<InsBlueBookRule> listInsBlueBookRules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsBlueBookRule";
        var table = new DataTable(tableName);
        table.Columns.Add("InsBlueBookRuleNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("RuleType");
        table.Columns.Add("LimitValue");
        table.Columns.Add("LimitType");
        foreach (var insBlueBookRule in listInsBlueBookRules)
            table.Rows.Add(SOut.Long(insBlueBookRule.InsBlueBookRuleNum), SOut.Int(insBlueBookRule.ItemOrder), SOut.Int((int) insBlueBookRule.RuleType), SOut.Int(insBlueBookRule.LimitValue), SOut.Int((int) insBlueBookRule.LimitType));
        return table;
    }

    public static long Insert(InsBlueBookRule insBlueBookRule)
    {
        return Insert(insBlueBookRule, false);
    }

    public static long Insert(InsBlueBookRule insBlueBookRule, bool useExistingPK)
    {
        var command = "INSERT INTO insbluebookrule (";

        command += "ItemOrder,RuleType,LimitValue,LimitType) VALUES(";

        command +=
            SOut.Int(insBlueBookRule.ItemOrder) + ","
                                                + SOut.Int((int) insBlueBookRule.RuleType) + ","
                                                + SOut.Int(insBlueBookRule.LimitValue) + ","
                                                + SOut.Int((int) insBlueBookRule.LimitType) + ")";
        {
            insBlueBookRule.InsBlueBookRuleNum = Db.NonQ(command, true, "InsBlueBookRuleNum", "insBlueBookRule");
        }
        return insBlueBookRule.InsBlueBookRuleNum;
    }

    public static long InsertNoCache(InsBlueBookRule insBlueBookRule)
    {
        return InsertNoCache(insBlueBookRule, false);
    }

    public static long InsertNoCache(InsBlueBookRule insBlueBookRule, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insbluebookrule (";
        if (isRandomKeys || useExistingPK) command += "InsBlueBookRuleNum,";
        command += "ItemOrder,RuleType,LimitValue,LimitType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insBlueBookRule.InsBlueBookRuleNum) + ",";
        command +=
            SOut.Int(insBlueBookRule.ItemOrder) + ","
                                                + SOut.Int((int) insBlueBookRule.RuleType) + ","
                                                + SOut.Int(insBlueBookRule.LimitValue) + ","
                                                + SOut.Int((int) insBlueBookRule.LimitType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            insBlueBookRule.InsBlueBookRuleNum = Db.NonQ(command, true, "InsBlueBookRuleNum", "insBlueBookRule");
        return insBlueBookRule.InsBlueBookRuleNum;
    }

    public static void Update(InsBlueBookRule insBlueBookRule)
    {
        var command = "UPDATE insbluebookrule SET "
                      + "ItemOrder         =  " + SOut.Int(insBlueBookRule.ItemOrder) + ", "
                      + "RuleType          =  " + SOut.Int((int) insBlueBookRule.RuleType) + ", "
                      + "LimitValue        =  " + SOut.Int(insBlueBookRule.LimitValue) + ", "
                      + "LimitType         =  " + SOut.Int((int) insBlueBookRule.LimitType) + " "
                      + "WHERE InsBlueBookRuleNum = " + SOut.Long(insBlueBookRule.InsBlueBookRuleNum);
        Db.NonQ(command);
    }

    public static bool Update(InsBlueBookRule insBlueBookRule, InsBlueBookRule oldInsBlueBookRule)
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

        if (command == "") return false;
        command = "UPDATE insbluebookrule SET " + command
                                                + " WHERE InsBlueBookRuleNum = " + SOut.Long(insBlueBookRule.InsBlueBookRuleNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InsBlueBookRule insBlueBookRule, InsBlueBookRule oldInsBlueBookRule)
    {
        if (insBlueBookRule.ItemOrder != oldInsBlueBookRule.ItemOrder) return true;
        if (insBlueBookRule.RuleType != oldInsBlueBookRule.RuleType) return true;
        if (insBlueBookRule.LimitValue != oldInsBlueBookRule.LimitValue) return true;
        if (insBlueBookRule.LimitType != oldInsBlueBookRule.LimitType) return true;
        return false;
    }

    public static void Delete(long insBlueBookRuleNum)
    {
        var command = "DELETE FROM insbluebookrule "
                      + "WHERE InsBlueBookRuleNum = " + SOut.Long(insBlueBookRuleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsBlueBookRuleNums)
    {
        if (listInsBlueBookRuleNums == null || listInsBlueBookRuleNums.Count == 0) return;
        var command = "DELETE FROM insbluebookrule "
                      + "WHERE InsBlueBookRuleNum IN(" + string.Join(",", listInsBlueBookRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}