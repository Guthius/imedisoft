#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TimeCardRuleCrud
{
    public static TimeCardRule SelectOne(long timeCardRuleNum)
    {
        var command = "SELECT * FROM timecardrule "
                      + "WHERE TimeCardRuleNum = " + SOut.Long(timeCardRuleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TimeCardRule SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TimeCardRule> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TimeCardRule> TableToList(DataTable table)
    {
        var retVal = new List<TimeCardRule>();
        TimeCardRule timeCardRule;
        foreach (DataRow row in table.Rows)
        {
            timeCardRule = new TimeCardRule();
            timeCardRule.TimeCardRuleNum = SIn.Long(row["TimeCardRuleNum"].ToString());
            timeCardRule.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            timeCardRule.OverHoursPerDay = SIn.TimeSpan(row["OverHoursPerDay"].ToString());
            timeCardRule.AfterTimeOfDay = SIn.TimeSpan(row["AfterTimeOfDay"].ToString());
            timeCardRule.BeforeTimeOfDay = SIn.TimeSpan(row["BeforeTimeOfDay"].ToString());
            timeCardRule.IsOvertimeExempt = SIn.Bool(row["IsOvertimeExempt"].ToString());
            timeCardRule.MinClockInTime = SIn.TimeSpan(row["MinClockInTime"].ToString());
            timeCardRule.HasWeekendRate3 = SIn.Bool(row["HasWeekendRate3"].ToString());
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

    public static long Insert(TimeCardRule timeCardRule)
    {
        return Insert(timeCardRule, false);
    }

    public static long Insert(TimeCardRule timeCardRule, bool useExistingPK)
    {
        var command = "INSERT INTO timecardrule (";

        command += "EmployeeNum,OverHoursPerDay,AfterTimeOfDay,BeforeTimeOfDay,IsOvertimeExempt,MinClockInTime,HasWeekendRate3) VALUES(";

        command +=
            SOut.Long(timeCardRule.EmployeeNum) + ","
                                                + SOut.Time(timeCardRule.OverHoursPerDay) + ","
                                                + SOut.Time(timeCardRule.AfterTimeOfDay) + ","
                                                + SOut.Time(timeCardRule.BeforeTimeOfDay) + ","
                                                + SOut.Bool(timeCardRule.IsOvertimeExempt) + ","
                                                + SOut.Time(timeCardRule.MinClockInTime) + ","
                                                + SOut.Bool(timeCardRule.HasWeekendRate3) + ")";
        {
            timeCardRule.TimeCardRuleNum = Db.NonQ(command, true, "TimeCardRuleNum", "timeCardRule");
        }
        return timeCardRule.TimeCardRuleNum;
    }

    public static void InsertMany(List<TimeCardRule> listTimeCardRules)
    {
        InsertMany(listTimeCardRules, false);
    }

    public static void InsertMany(List<TimeCardRule> listTimeCardRules, bool useExistingPK)
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

    public static long InsertNoCache(TimeCardRule timeCardRule)
    {
        return InsertNoCache(timeCardRule, false);
    }

    public static long InsertNoCache(TimeCardRule timeCardRule, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO timecardrule (";
        if (isRandomKeys || useExistingPK) command += "TimeCardRuleNum,";
        command += "EmployeeNum,OverHoursPerDay,AfterTimeOfDay,BeforeTimeOfDay,IsOvertimeExempt,MinClockInTime,HasWeekendRate3) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(timeCardRule.TimeCardRuleNum) + ",";
        command +=
            SOut.Long(timeCardRule.EmployeeNum) + ","
                                                + SOut.Time(timeCardRule.OverHoursPerDay) + ","
                                                + SOut.Time(timeCardRule.AfterTimeOfDay) + ","
                                                + SOut.Time(timeCardRule.BeforeTimeOfDay) + ","
                                                + SOut.Bool(timeCardRule.IsOvertimeExempt) + ","
                                                + SOut.Time(timeCardRule.MinClockInTime) + ","
                                                + SOut.Bool(timeCardRule.HasWeekendRate3) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            timeCardRule.TimeCardRuleNum = Db.NonQ(command, true, "TimeCardRuleNum", "timeCardRule");
        return timeCardRule.TimeCardRuleNum;
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

    public static bool Update(TimeCardRule timeCardRule, TimeCardRule oldTimeCardRule)
    {
        var command = "";
        if (timeCardRule.EmployeeNum != oldTimeCardRule.EmployeeNum)
        {
            if (command != "") command += ",";
            command += "EmployeeNum = " + SOut.Long(timeCardRule.EmployeeNum) + "";
        }

        if (timeCardRule.OverHoursPerDay != oldTimeCardRule.OverHoursPerDay)
        {
            if (command != "") command += ",";
            command += "OverHoursPerDay = " + SOut.Time(timeCardRule.OverHoursPerDay) + "";
        }

        if (timeCardRule.AfterTimeOfDay != oldTimeCardRule.AfterTimeOfDay)
        {
            if (command != "") command += ",";
            command += "AfterTimeOfDay = " + SOut.Time(timeCardRule.AfterTimeOfDay) + "";
        }

        if (timeCardRule.BeforeTimeOfDay != oldTimeCardRule.BeforeTimeOfDay)
        {
            if (command != "") command += ",";
            command += "BeforeTimeOfDay = " + SOut.Time(timeCardRule.BeforeTimeOfDay) + "";
        }

        if (timeCardRule.IsOvertimeExempt != oldTimeCardRule.IsOvertimeExempt)
        {
            if (command != "") command += ",";
            command += "IsOvertimeExempt = " + SOut.Bool(timeCardRule.IsOvertimeExempt) + "";
        }

        if (timeCardRule.MinClockInTime != oldTimeCardRule.MinClockInTime)
        {
            if (command != "") command += ",";
            command += "MinClockInTime = " + SOut.Time(timeCardRule.MinClockInTime) + "";
        }

        if (timeCardRule.HasWeekendRate3 != oldTimeCardRule.HasWeekendRate3)
        {
            if (command != "") command += ",";
            command += "HasWeekendRate3 = " + SOut.Bool(timeCardRule.HasWeekendRate3) + "";
        }

        if (command == "") return false;
        command = "UPDATE timecardrule SET " + command
                                             + " WHERE TimeCardRuleNum = " + SOut.Long(timeCardRule.TimeCardRuleNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TimeCardRule timeCardRule, TimeCardRule oldTimeCardRule)
    {
        if (timeCardRule.EmployeeNum != oldTimeCardRule.EmployeeNum) return true;
        if (timeCardRule.OverHoursPerDay != oldTimeCardRule.OverHoursPerDay) return true;
        if (timeCardRule.AfterTimeOfDay != oldTimeCardRule.AfterTimeOfDay) return true;
        if (timeCardRule.BeforeTimeOfDay != oldTimeCardRule.BeforeTimeOfDay) return true;
        if (timeCardRule.IsOvertimeExempt != oldTimeCardRule.IsOvertimeExempt) return true;
        if (timeCardRule.MinClockInTime != oldTimeCardRule.MinClockInTime) return true;
        if (timeCardRule.HasWeekendRate3 != oldTimeCardRule.HasWeekendRate3) return true;
        return false;
    }

    public static void Delete(long timeCardRuleNum)
    {
        var command = "DELETE FROM timecardrule "
                      + "WHERE TimeCardRuleNum = " + SOut.Long(timeCardRuleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTimeCardRuleNums)
    {
        if (listTimeCardRuleNums == null || listTimeCardRuleNums.Count == 0) return;
        var command = "DELETE FROM timecardrule "
                      + "WHERE TimeCardRuleNum IN(" + string.Join(",", listTimeCardRuleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}