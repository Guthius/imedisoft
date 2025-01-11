#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReminderRuleCrud
{
    public static ReminderRule SelectOne(long reminderRuleNum)
    {
        var command = "SELECT * FROM reminderrule "
                      + "WHERE ReminderRuleNum = " + SOut.Long(reminderRuleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ReminderRule SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ReminderRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ReminderRule> TableToList(DataTable table)
    {
        var retVal = new List<ReminderRule>();
        ReminderRule reminderRule;
        foreach (DataRow row in table.Rows)
        {
            reminderRule = new ReminderRule();
            reminderRule.ReminderRuleNum = SIn.Long(row["ReminderRuleNum"].ToString());
            reminderRule.ReminderCriterion = (EhrCriterion) SIn.Int(row["ReminderCriterion"].ToString());
            reminderRule.CriterionFK = SIn.Long(row["CriterionFK"].ToString());
            reminderRule.CriterionValue = SIn.String(row["CriterionValue"].ToString());
            reminderRule.Message = SIn.String(row["Message"].ToString());
            retVal.Add(reminderRule);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ReminderRule> listReminderRules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ReminderRule";
        var table = new DataTable(tableName);
        table.Columns.Add("ReminderRuleNum");
        table.Columns.Add("ReminderCriterion");
        table.Columns.Add("CriterionFK");
        table.Columns.Add("CriterionValue");
        table.Columns.Add("Message");
        foreach (var reminderRule in listReminderRules)
            table.Rows.Add(SOut.Long(reminderRule.ReminderRuleNum), SOut.Int((int) reminderRule.ReminderCriterion), SOut.Long(reminderRule.CriterionFK), reminderRule.CriterionValue, reminderRule.Message);
        return table;
    }

    public static long Insert(ReminderRule reminderRule)
    {
        return Insert(reminderRule, false);
    }

    public static long Insert(ReminderRule reminderRule, bool useExistingPK)
    {
        var command = "INSERT INTO reminderrule (";

        command += "ReminderCriterion,CriterionFK,CriterionValue,Message) VALUES(";

        command +=
            SOut.Int((int) reminderRule.ReminderCriterion) + ","
                                                           + SOut.Long(reminderRule.CriterionFK) + ","
                                                           + "'" + SOut.String(reminderRule.CriterionValue) + "',"
                                                           + "'" + SOut.String(reminderRule.Message) + "')";
        {
            reminderRule.ReminderRuleNum = Db.NonQ(command, true, "ReminderRuleNum", "reminderRule");
        }
        return reminderRule.ReminderRuleNum;
    }

    public static long InsertNoCache(ReminderRule reminderRule)
    {
        return InsertNoCache(reminderRule, false);
    }

    public static long InsertNoCache(ReminderRule reminderRule, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO reminderrule (";
        if (isRandomKeys || useExistingPK) command += "ReminderRuleNum,";
        command += "ReminderCriterion,CriterionFK,CriterionValue,Message) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(reminderRule.ReminderRuleNum) + ",";
        command +=
            SOut.Int((int) reminderRule.ReminderCriterion) + ","
                                                           + SOut.Long(reminderRule.CriterionFK) + ","
                                                           + "'" + SOut.String(reminderRule.CriterionValue) + "',"
                                                           + "'" + SOut.String(reminderRule.Message) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            reminderRule.ReminderRuleNum = Db.NonQ(command, true, "ReminderRuleNum", "reminderRule");
        return reminderRule.ReminderRuleNum;
    }

    public static void Update(ReminderRule reminderRule)
    {
        var command = "UPDATE reminderrule SET "
                      + "ReminderCriterion=  " + SOut.Int((int) reminderRule.ReminderCriterion) + ", "
                      + "CriterionFK      =  " + SOut.Long(reminderRule.CriterionFK) + ", "
                      + "CriterionValue   = '" + SOut.String(reminderRule.CriterionValue) + "', "
                      + "Message          = '" + SOut.String(reminderRule.Message) + "' "
                      + "WHERE ReminderRuleNum = " + SOut.Long(reminderRule.ReminderRuleNum);
        Db.NonQ(command);
    }

    public static bool Update(ReminderRule reminderRule, ReminderRule oldReminderRule)
    {
        var command = "";
        if (reminderRule.ReminderCriterion != oldReminderRule.ReminderCriterion)
        {
            if (command != "") command += ",";
            command += "ReminderCriterion = " + SOut.Int((int) reminderRule.ReminderCriterion) + "";
        }

        if (reminderRule.CriterionFK != oldReminderRule.CriterionFK)
        {
            if (command != "") command += ",";
            command += "CriterionFK = " + SOut.Long(reminderRule.CriterionFK) + "";
        }

        if (reminderRule.CriterionValue != oldReminderRule.CriterionValue)
        {
            if (command != "") command += ",";
            command += "CriterionValue = '" + SOut.String(reminderRule.CriterionValue) + "'";
        }

        if (reminderRule.Message != oldReminderRule.Message)
        {
            if (command != "") command += ",";
            command += "Message = '" + SOut.String(reminderRule.Message) + "'";
        }

        if (command == "") return false;
        command = "UPDATE reminderrule SET " + command
                                             + " WHERE ReminderRuleNum = " + SOut.Long(reminderRule.ReminderRuleNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ReminderRule reminderRule, ReminderRule oldReminderRule)
    {
        if (reminderRule.ReminderCriterion != oldReminderRule.ReminderCriterion) return true;
        if (reminderRule.CriterionFK != oldReminderRule.CriterionFK) return true;
        if (reminderRule.CriterionValue != oldReminderRule.CriterionValue) return true;
        if (reminderRule.Message != oldReminderRule.Message) return true;
        return false;
    }

    public static void Delete(long reminderRuleNum)
    {
        var command = "DELETE FROM reminderrule "
                      + "WHERE ReminderRuleNum = " + SOut.Long(reminderRuleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReminderRuleNums)
    {
        if (listReminderRuleNums == null || listReminderRuleNums.Count == 0) return;
        var command = "DELETE FROM reminderrule "
                      + "WHERE ReminderRuleNum IN(" + string.Join(",", listReminderRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}