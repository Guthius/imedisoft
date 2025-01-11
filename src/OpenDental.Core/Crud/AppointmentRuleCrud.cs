using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AppointmentRuleCrud
{
    public static List<AppointmentRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AppointmentRule> TableToList(DataTable table)
    {
        var retVal = new List<AppointmentRule>();
        AppointmentRule appointmentRule;
        foreach (DataRow row in table.Rows)
        {
            appointmentRule = new AppointmentRule();
            appointmentRule.AppointmentRuleNum = SIn.Long(row["AppointmentRuleNum"].ToString());
            appointmentRule.RuleDesc = SIn.String(row["RuleDesc"].ToString());
            appointmentRule.CodeStart = SIn.String(row["CodeStart"].ToString());
            appointmentRule.CodeEnd = SIn.String(row["CodeEnd"].ToString());
            appointmentRule.IsEnabled = SIn.Bool(row["IsEnabled"].ToString());
            retVal.Add(appointmentRule);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AppointmentRule> listAppointmentRules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AppointmentRule";
        var table = new DataTable(tableName);
        table.Columns.Add("AppointmentRuleNum");
        table.Columns.Add("RuleDesc");
        table.Columns.Add("CodeStart");
        table.Columns.Add("CodeEnd");
        table.Columns.Add("IsEnabled");
        foreach (var appointmentRule in listAppointmentRules)
            table.Rows.Add(SOut.Long(appointmentRule.AppointmentRuleNum), appointmentRule.RuleDesc, appointmentRule.CodeStart, appointmentRule.CodeEnd, SOut.Bool(appointmentRule.IsEnabled));
        return table;
    }

    public static long Insert(AppointmentRule appointmentRule)
    {
        var command = "INSERT INTO appointmentrule (";

        command += "RuleDesc,CodeStart,CodeEnd,IsEnabled) VALUES(";

        command +=
            "'" + SOut.String(appointmentRule.RuleDesc) + "',"
            + "'" + SOut.String(appointmentRule.CodeStart) + "',"
            + "'" + SOut.String(appointmentRule.CodeEnd) + "',"
            + SOut.Bool(appointmentRule.IsEnabled) + ")";
        {
            appointmentRule.AppointmentRuleNum = Db.NonQ(command, true, "AppointmentRuleNum", "appointmentRule");
        }
        return appointmentRule.AppointmentRuleNum;
    }

    public static void Update(AppointmentRule appointmentRule)
    {
        var command = "UPDATE appointmentrule SET "
                      + "RuleDesc          = '" + SOut.String(appointmentRule.RuleDesc) + "', "
                      + "CodeStart         = '" + SOut.String(appointmentRule.CodeStart) + "', "
                      + "CodeEnd           = '" + SOut.String(appointmentRule.CodeEnd) + "', "
                      + "IsEnabled         =  " + SOut.Bool(appointmentRule.IsEnabled) + " "
                      + "WHERE AppointmentRuleNum = " + SOut.Long(appointmentRule.AppointmentRuleNum);
        Db.NonQ(command);
    }
}