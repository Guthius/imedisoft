using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TimeCardRuleCrud
{
    public static List<TimeCardRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TimeCardRule> TableToList(DataTable table)
    {
        var retVal = new List<TimeCardRule>();
        foreach (DataRow row in table.Rows)
        {
            var timeCardRule = new TimeCardRule
            {
                TimeCardRuleNum = SIn.Long(row["TimeCardRuleNum"].ToString()),
                EmployeeNum = SIn.Long(row["EmployeeNum"].ToString()),
                OverHoursPerDay = SIn.TimeSpan(row["OverHoursPerDay"].ToString()),
                AfterTimeOfDay = SIn.TimeSpan(row["AfterTimeOfDay"].ToString()),
                BeforeTimeOfDay = SIn.TimeSpan(row["BeforeTimeOfDay"].ToString()),
                IsOvertimeExempt = SIn.Bool(row["IsOvertimeExempt"].ToString()),
                MinClockInTime = SIn.TimeSpan(row["MinClockInTime"].ToString()),
                HasWeekendRate3 = SIn.Bool(row["HasWeekendRate3"].ToString())
            };
            retVal.Add(timeCardRule);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TimeCardRule> listTimeCardRules, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TimeCardRule";
        var table = new DataTable(tableName);
        table.Columns.Add("TimeCardRuleNum");
        table.Columns.Add("EmployeeNum");
        table.Columns.Add("OverHoursPerDay");
        table.Columns.Add("AfterTimeOfDay");
        table.Columns.Add("BeforeTimeOfDay");
        table.Columns.Add("IsOvertimeExempt");
        table.Columns.Add("MinClockInTime");
        table.Columns.Add("HasWeekendRate3");
        foreach (var timeCardRule in listTimeCardRules)
            table.Rows.Add(SOut.Long(timeCardRule.TimeCardRuleNum), SOut.Long(timeCardRule.EmployeeNum), SOut.Time(timeCardRule.OverHoursPerDay, false), SOut.Time(timeCardRule.AfterTimeOfDay, false), SOut.Time(timeCardRule.BeforeTimeOfDay, false), SOut.Bool(timeCardRule.IsOvertimeExempt), SOut.Time(timeCardRule.MinClockInTime, false), SOut.Bool(timeCardRule.HasWeekendRate3));
        return table;
    }

    public static void InsertMany(List<TimeCardRule> listTimeCardRules, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listTimeCardRules.Count)
        {
            var timeCardRule = listTimeCardRules[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO timecardrule (");
                if (useExistingPK) sbCommands.Append("TimeCardRuleNum,");
                sbCommands.Append("EmployeeNum,OverHoursPerDay,AfterTimeOfDay,BeforeTimeOfDay,IsOvertimeExempt,MinClockInTime,HasWeekendRate3) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(timeCardRule.TimeCardRuleNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(timeCardRule.EmployeeNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(timeCardRule.OverHoursPerDay));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(timeCardRule.AfterTimeOfDay));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(timeCardRule.BeforeTimeOfDay));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(timeCardRule.IsOvertimeExempt));
            sbRow.Append(",");
            sbRow.Append(SOut.Time(timeCardRule.MinClockInTime));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(timeCardRule.HasWeekendRate3));
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
                if (index == listTimeCardRules.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(TimeCardRule timeCardRule)
    {
        var command = "UPDATE timecardrule SET "
                      + "EmployeeNum     =  " + SOut.Long(timeCardRule.EmployeeNum) + ", "
                      + "OverHoursPerDay =  " + SOut.Time(timeCardRule.OverHoursPerDay) + ", "
                      + "AfterTimeOfDay  =  " + SOut.Time(timeCardRule.AfterTimeOfDay) + ", "
                      + "BeforeTimeOfDay =  " + SOut.Time(timeCardRule.BeforeTimeOfDay) + ", "
                      + "IsOvertimeExempt=  " + SOut.Bool(timeCardRule.IsOvertimeExempt) + ", "
                      + "MinClockInTime  =  " + SOut.Time(timeCardRule.MinClockInTime) + ", "
                      + "HasWeekendRate3 =  " + SOut.Bool(timeCardRule.HasWeekendRate3) + " "
                      + "WHERE TimeCardRuleNum = " + SOut.Long(timeCardRule.TimeCardRuleNum);
        Db.NonQ(command);
    }
}