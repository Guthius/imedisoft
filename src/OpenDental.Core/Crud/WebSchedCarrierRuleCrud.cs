using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class WebSchedCarrierRuleCrud
{
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
        var paramMessage = new OdSqlParameter("paramMessage", SOut.StringParam(webSchedCarrierRule.Message));
        Db.NonQ(command, paramMessage);
    }

    public static void DeleteMany(List<long> listWebSchedCarrierRuleNums)
    {
        if (listWebSchedCarrierRuleNums == null || listWebSchedCarrierRuleNums.Count == 0) return;
        var command = "DELETE FROM webschedcarrierrule "
                      + "WHERE WebSchedCarrierRuleNum IN(" + string.Join(",", listWebSchedCarrierRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}