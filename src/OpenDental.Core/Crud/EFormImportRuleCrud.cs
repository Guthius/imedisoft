#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EFormImportRuleCrud
{
    public static EFormImportRule SelectOne(long eFormImportRuleNum)
    {
        var command = "SELECT * FROM eformimportrule "
                      + "WHERE EFormImportRuleNum = " + SOut.Long(eFormImportRuleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EFormImportRule SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EFormImportRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EFormImportRule> TableToList(DataTable table)
    {
        var retVal = new List<EFormImportRule>();
        EFormImportRule eFormImportRule;
        foreach (DataRow row in table.Rows)
        {
            eFormImportRule = new EFormImportRule();
            eFormImportRule.EFormImportRuleNum = SIn.Long(row["EFormImportRuleNum"].ToString());
            eFormImportRule.FieldName = SIn.String(row["FieldName"].ToString());
            eFormImportRule.Situation = (EnumEFormImportSituation) SIn.Int(row["Situation"].ToString());
            eFormImportRule.Action = (EnumEFormImportAction) SIn.Int(row["Action"].ToString());
            retVal.Add(eFormImportRule);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EFormImportRule> listEFormImportRules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EFormImportRule";
        var table = new DataTable(tableName);
        table.Columns.Add("EFormImportRuleNum");
        table.Columns.Add("FieldName");
        table.Columns.Add("Situation");
        table.Columns.Add("Action");
        foreach (var eFormImportRule in listEFormImportRules)
            table.Rows.Add(SOut.Long(eFormImportRule.EFormImportRuleNum), eFormImportRule.FieldName, SOut.Int((int) eFormImportRule.Situation), SOut.Int((int) eFormImportRule.Action));
        return table;
    }

    public static long Insert(EFormImportRule eFormImportRule)
    {
        return Insert(eFormImportRule, false);
    }

    public static long Insert(EFormImportRule eFormImportRule, bool useExistingPK)
    {
        var command = "INSERT INTO eformimportrule (";

        command += "FieldName,Situation,Action) VALUES(";

        command +=
            "'" + SOut.String(eFormImportRule.FieldName) + "',"
            + SOut.Int((int) eFormImportRule.Situation) + ","
            + SOut.Int((int) eFormImportRule.Action) + ")";
        {
            eFormImportRule.EFormImportRuleNum = Db.NonQ(command, true, "EFormImportRuleNum", "eFormImportRule");
        }
        return eFormImportRule.EFormImportRuleNum;
    }

    public static long InsertNoCache(EFormImportRule eFormImportRule)
    {
        return InsertNoCache(eFormImportRule, false);
    }

    public static long InsertNoCache(EFormImportRule eFormImportRule, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eformimportrule (";
        if (isRandomKeys || useExistingPK) command += "EFormImportRuleNum,";
        command += "FieldName,Situation,Action) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eFormImportRule.EFormImportRuleNum) + ",";
        command +=
            "'" + SOut.String(eFormImportRule.FieldName) + "',"
            + SOut.Int((int) eFormImportRule.Situation) + ","
            + SOut.Int((int) eFormImportRule.Action) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eFormImportRule.EFormImportRuleNum = Db.NonQ(command, true, "EFormImportRuleNum", "eFormImportRule");
        return eFormImportRule.EFormImportRuleNum;
    }

    public static void Update(EFormImportRule eFormImportRule)
    {
        var command = "UPDATE eformimportrule SET "
                      + "FieldName         = '" + SOut.String(eFormImportRule.FieldName) + "', "
                      + "Situation         =  " + SOut.Int((int) eFormImportRule.Situation) + ", "
                      + "Action            =  " + SOut.Int((int) eFormImportRule.Action) + " "
                      + "WHERE EFormImportRuleNum = " + SOut.Long(eFormImportRule.EFormImportRuleNum);
        Db.NonQ(command);
    }

    public static bool Update(EFormImportRule eFormImportRule, EFormImportRule oldEFormImportRule)
    {
        var command = "";
        if (eFormImportRule.FieldName != oldEFormImportRule.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(eFormImportRule.FieldName) + "'";
        }

        if (eFormImportRule.Situation != oldEFormImportRule.Situation)
        {
            if (command != "") command += ",";
            command += "Situation = " + SOut.Int((int) eFormImportRule.Situation) + "";
        }

        if (eFormImportRule.Action != oldEFormImportRule.Action)
        {
            if (command != "") command += ",";
            command += "Action = " + SOut.Int((int) eFormImportRule.Action) + "";
        }

        if (command == "") return false;
        command = "UPDATE eformimportrule SET " + command
                                                + " WHERE EFormImportRuleNum = " + SOut.Long(eFormImportRule.EFormImportRuleNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EFormImportRule eFormImportRule, EFormImportRule oldEFormImportRule)
    {
        if (eFormImportRule.FieldName != oldEFormImportRule.FieldName) return true;
        if (eFormImportRule.Situation != oldEFormImportRule.Situation) return true;
        if (eFormImportRule.Action != oldEFormImportRule.Action) return true;
        return false;
    }

    public static void Delete(long eFormImportRuleNum)
    {
        var command = "DELETE FROM eformimportrule "
                      + "WHERE EFormImportRuleNum = " + SOut.Long(eFormImportRuleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEFormImportRuleNums)
    {
        if (listEFormImportRuleNums == null || listEFormImportRuleNums.Count == 0) return;
        var command = "DELETE FROM eformimportrule "
                      + "WHERE EFormImportRuleNum IN(" + string.Join(",", listEFormImportRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}