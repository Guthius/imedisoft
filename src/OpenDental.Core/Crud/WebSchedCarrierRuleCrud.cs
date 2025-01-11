#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class WebSchedCarrierRuleCrud
{
    public static WebSchedCarrierRule SelectOne(long webSchedCarrierRuleNum)
    {
        var command = "SELECT * FROM webschedcarrierrule "
                      + "WHERE WebSchedCarrierRuleNum = " + SOut.Long(webSchedCarrierRuleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static WebSchedCarrierRule SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<WebSchedCarrierRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<WebSchedCarrierRule> TableToList(DataTable table)
    {
        var retVal = new List<WebSchedCarrierRule>();
        WebSchedCarrierRule webSchedCarrierRule;
        foreach (DataRow row in table.Rows)
        {
            webSchedCarrierRule = new WebSchedCarrierRule();
            webSchedCarrierRule.WebSchedCarrierRuleNum = SIn.Long(row["WebSchedCarrierRuleNum"].ToString());
            webSchedCarrierRule.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            webSchedCarrierRule.CarrierName = SIn.String(row["CarrierName"].ToString());
            webSchedCarrierRule.DisplayName = SIn.String(row["DisplayName"].ToString());
            webSchedCarrierRule.Message = SIn.String(row["Message"].ToString());
            webSchedCarrierRule.Rule = (RuleType) SIn.Int(row["Rule"].ToString());
            retVal.Add(webSchedCarrierRule);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<WebSchedCarrierRule> listWebSchedCarrierRules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "WebSchedCarrierRule";
        var table = new DataTable(tableName);
        table.Columns.Add("WebSchedCarrierRuleNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("CarrierName");
        table.Columns.Add("DisplayName");
        table.Columns.Add("Message");
        table.Columns.Add("Rule");
        foreach (var webSchedCarrierRule in listWebSchedCarrierRules)
            table.Rows.Add(SOut.Long(webSchedCarrierRule.WebSchedCarrierRuleNum), SOut.Long(webSchedCarrierRule.ClinicNum), webSchedCarrierRule.CarrierName, webSchedCarrierRule.DisplayName, webSchedCarrierRule.Message, SOut.Int((int) webSchedCarrierRule.Rule));
        return table;
    }

    public static long Insert(WebSchedCarrierRule webSchedCarrierRule)
    {
        return Insert(webSchedCarrierRule, false);
    }

    public static long Insert(WebSchedCarrierRule webSchedCarrierRule, bool useExistingPK)
    {
        var command = "INSERT INTO webschedcarrierrule (";

        command += "ClinicNum,CarrierName,DisplayName,Message,Rule) VALUES(";

        command +=
            SOut.Long(webSchedCarrierRule.ClinicNum) + ","
                                                     + "'" + SOut.String(webSchedCarrierRule.CarrierName) + "',"
                                                     + "'" + SOut.String(webSchedCarrierRule.DisplayName) + "',"
                                                     + DbHelper.ParamChar + "paramMessage,"
                                                     + SOut.Int((int) webSchedCarrierRule.Rule) + ")";
        if (webSchedCarrierRule.Message == null) webSchedCarrierRule.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(webSchedCarrierRule.Message));
        {
            webSchedCarrierRule.WebSchedCarrierRuleNum = Db.NonQ(command, true, "WebSchedCarrierRuleNum", "webSchedCarrierRule", paramMessage);
        }
        return webSchedCarrierRule.WebSchedCarrierRuleNum;
    }

    public static void InsertMany(List<WebSchedCarrierRule> listWebSchedCarrierRules)
    {
        InsertMany(listWebSchedCarrierRules, false);
    }

    public static void InsertMany(List<WebSchedCarrierRule> listWebSchedCarrierRules, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listWebSchedCarrierRules.Count)
        {
            var webSchedCarrierRule = listWebSchedCarrierRules[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO webschedcarrierrule (");
                if (useExistingPK) sbCommands.Append("WebSchedCarrierRuleNum,");
                sbCommands.Append("ClinicNum,CarrierName,DisplayName,Message,Rule) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(webSchedCarrierRule.WebSchedCarrierRuleNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(webSchedCarrierRule.ClinicNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(webSchedCarrierRule.CarrierName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(webSchedCarrierRule.DisplayName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(webSchedCarrierRule.Message) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) webSchedCarrierRule.Rule));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listWebSchedCarrierRules.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(WebSchedCarrierRule webSchedCarrierRule)
    {
        return InsertNoCache(webSchedCarrierRule, false);
    }

    public static long InsertNoCache(WebSchedCarrierRule webSchedCarrierRule, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO webschedcarrierrule (";
        if (isRandomKeys || useExistingPK) command += "WebSchedCarrierRuleNum,";
        command += "ClinicNum,CarrierName,DisplayName,Message,Rule) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(webSchedCarrierRule.WebSchedCarrierRuleNum) + ",";
        command +=
            SOut.Long(webSchedCarrierRule.ClinicNum) + ","
                                                     + "'" + SOut.String(webSchedCarrierRule.CarrierName) + "',"
                                                     + "'" + SOut.String(webSchedCarrierRule.DisplayName) + "',"
                                                     + DbHelper.ParamChar + "paramMessage,"
                                                     + SOut.Int((int) webSchedCarrierRule.Rule) + ")";
        if (webSchedCarrierRule.Message == null) webSchedCarrierRule.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(webSchedCarrierRule.Message));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMessage);
        else
            webSchedCarrierRule.WebSchedCarrierRuleNum = Db.NonQ(command, true, "WebSchedCarrierRuleNum", "webSchedCarrierRule", paramMessage);
        return webSchedCarrierRule.WebSchedCarrierRuleNum;
    }

    public static void Update(WebSchedCarrierRule webSchedCarrierRule)
    {
        var command = "UPDATE webschedcarrierrule SET "
                      + "ClinicNum             =  " + SOut.Long(webSchedCarrierRule.ClinicNum) + ", "
                      + "CarrierName           = '" + SOut.String(webSchedCarrierRule.CarrierName) + "', "
                      + "DisplayName           = '" + SOut.String(webSchedCarrierRule.DisplayName) + "', "
                      + "Message               =  " + DbHelper.ParamChar + "paramMessage, "
                      + "Rule                  =  " + SOut.Int((int) webSchedCarrierRule.Rule) + " "
                      + "WHERE WebSchedCarrierRuleNum = " + SOut.Long(webSchedCarrierRule.WebSchedCarrierRuleNum);
        if (webSchedCarrierRule.Message == null) webSchedCarrierRule.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(webSchedCarrierRule.Message));
        Db.NonQ(command, paramMessage);
    }

    public static bool Update(WebSchedCarrierRule webSchedCarrierRule, WebSchedCarrierRule oldWebSchedCarrierRule)
    {
        var command = "";
        if (webSchedCarrierRule.ClinicNum != oldWebSchedCarrierRule.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(webSchedCarrierRule.ClinicNum) + "";
        }

        if (webSchedCarrierRule.CarrierName != oldWebSchedCarrierRule.CarrierName)
        {
            if (command != "") command += ",";
            command += "CarrierName = '" + SOut.String(webSchedCarrierRule.CarrierName) + "'";
        }

        if (webSchedCarrierRule.DisplayName != oldWebSchedCarrierRule.DisplayName)
        {
            if (command != "") command += ",";
            command += "DisplayName = '" + SOut.String(webSchedCarrierRule.DisplayName) + "'";
        }

        if (webSchedCarrierRule.Message != oldWebSchedCarrierRule.Message)
        {
            if (command != "") command += ",";
            command += "Message = " + DbHelper.ParamChar + "paramMessage";
        }

        if (webSchedCarrierRule.Rule != oldWebSchedCarrierRule.Rule)
        {
            if (command != "") command += ",";
            command += "Rule = " + SOut.Int((int) webSchedCarrierRule.Rule) + "";
        }

        if (command == "") return false;
        if (webSchedCarrierRule.Message == null) webSchedCarrierRule.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(webSchedCarrierRule.Message));
        command = "UPDATE webschedcarrierrule SET " + command
                                                    + " WHERE WebSchedCarrierRuleNum = " + SOut.Long(webSchedCarrierRule.WebSchedCarrierRuleNum);
        Db.NonQ(command, paramMessage);
        return true;
    }

    public static bool UpdateComparison(WebSchedCarrierRule webSchedCarrierRule, WebSchedCarrierRule oldWebSchedCarrierRule)
    {
        if (webSchedCarrierRule.ClinicNum != oldWebSchedCarrierRule.ClinicNum) return true;
        if (webSchedCarrierRule.CarrierName != oldWebSchedCarrierRule.CarrierName) return true;
        if (webSchedCarrierRule.DisplayName != oldWebSchedCarrierRule.DisplayName) return true;
        if (webSchedCarrierRule.Message != oldWebSchedCarrierRule.Message) return true;
        if (webSchedCarrierRule.Rule != oldWebSchedCarrierRule.Rule) return true;
        return false;
    }

    public static void Delete(long webSchedCarrierRuleNum)
    {
        var command = "DELETE FROM webschedcarrierrule "
                      + "WHERE WebSchedCarrierRuleNum = " + SOut.Long(webSchedCarrierRuleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listWebSchedCarrierRuleNums)
    {
        if (listWebSchedCarrierRuleNums == null || listWebSchedCarrierRuleNums.Count == 0) return;
        var command = "DELETE FROM webschedcarrierrule "
                      + "WHERE WebSchedCarrierRuleNum IN(" + string.Join(",", listWebSchedCarrierRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}