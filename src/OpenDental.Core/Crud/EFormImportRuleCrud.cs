using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EFormImportRuleCrud
{
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

    public static void Insert(EFormImportRule eFormImportRule)
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

    public static void Delete(long eFormImportRuleNum)
    {
        var command = "DELETE FROM eformimportrule "
                      + "WHERE EFormImportRuleNum = " + SOut.Long(eFormImportRuleNum);
        Db.NonQ(command);
    }
}