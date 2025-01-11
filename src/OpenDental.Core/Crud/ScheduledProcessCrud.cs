#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ScheduledProcessCrud
{
    public static ScheduledProcess SelectOne(long scheduledProcessNum)
    {
        var command = "SELECT * FROM scheduledprocess "
                      + "WHERE ScheduledProcessNum = " + SOut.Long(scheduledProcessNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ScheduledProcess SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ScheduledProcess> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ScheduledProcess> TableToList(DataTable table)
    {
        var retVal = new List<ScheduledProcess>();
        ScheduledProcess scheduledProcess;
        foreach (DataRow row in table.Rows)
        {
            scheduledProcess = new ScheduledProcess();
            scheduledProcess.ScheduledProcessNum = SIn.Long(row["ScheduledProcessNum"].ToString());
            var scheduledAction = row["ScheduledAction"].ToString();
            if (scheduledAction == "")
                scheduledProcess.ScheduledAction = 0;
            else
                try
                {
                    scheduledProcess.ScheduledAction = (ScheduledActionEnum) Enum.Parse(typeof(ScheduledActionEnum), scheduledAction);
                }
                catch
                {
                    scheduledProcess.ScheduledAction = 0;
                }

            scheduledProcess.TimeToRun = SIn.DateTime(row["TimeToRun"].ToString());
            var frequencyToRun = row["FrequencyToRun"].ToString();
            if (frequencyToRun == "")
                scheduledProcess.FrequencyToRun = 0;
            else
                try
                {
                    scheduledProcess.FrequencyToRun = (FrequencyToRunEnum) Enum.Parse(typeof(FrequencyToRunEnum), frequencyToRun);
                }
                catch
                {
                    scheduledProcess.FrequencyToRun = 0;
                }

            scheduledProcess.LastRanDateTime = SIn.DateTime(row["LastRanDateTime"].ToString());
            retVal.Add(scheduledProcess);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ScheduledProcess> listScheduledProcesss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ScheduledProcess";
        var table = new DataTable(tableName);
        table.Columns.Add("ScheduledProcessNum");
        table.Columns.Add("ScheduledAction");
        table.Columns.Add("TimeToRun");
        table.Columns.Add("FrequencyToRun");
        table.Columns.Add("LastRanDateTime");
        foreach (var scheduledProcess in listScheduledProcesss)
            table.Rows.Add(SOut.Long(scheduledProcess.ScheduledProcessNum), SOut.Int((int) scheduledProcess.ScheduledAction), SOut.DateT(scheduledProcess.TimeToRun, false), SOut.Int((int) scheduledProcess.FrequencyToRun), SOut.DateT(scheduledProcess.LastRanDateTime, false));
        return table;
    }

    public static long Insert(ScheduledProcess scheduledProcess)
    {
        return Insert(scheduledProcess, false);
    }

    public static long Insert(ScheduledProcess scheduledProcess, bool useExistingPK)
    {
        var command = "INSERT INTO scheduledprocess (";

        command += "ScheduledAction,TimeToRun,FrequencyToRun,LastRanDateTime) VALUES(";

        command +=
            "'" + SOut.String(scheduledProcess.ScheduledAction.ToString()) + "',"
            + SOut.DateT(scheduledProcess.TimeToRun) + ","
            + "'" + SOut.String(scheduledProcess.FrequencyToRun.ToString()) + "',"
            + SOut.DateT(scheduledProcess.LastRanDateTime) + ")";

        scheduledProcess.ScheduledProcessNum = Db.NonQ(command, true, "ScheduledProcessNum", "scheduledProcess");
        return scheduledProcess.ScheduledProcessNum;
    }

    public static long InsertNoCache(ScheduledProcess scheduledProcess)
    {
        return InsertNoCache(scheduledProcess, false);
    }

    public static long InsertNoCache(ScheduledProcess scheduledProcess, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO scheduledprocess (";
        if (isRandomKeys || useExistingPK) command += "ScheduledProcessNum,";
        command += "ScheduledAction,TimeToRun,FrequencyToRun,LastRanDateTime) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(scheduledProcess.ScheduledProcessNum) + ",";
        command +=
            "'" + SOut.String(scheduledProcess.ScheduledAction.ToString()) + "',"
            + SOut.DateT(scheduledProcess.TimeToRun) + ","
            + "'" + SOut.String(scheduledProcess.FrequencyToRun.ToString()) + "',"
            + SOut.DateT(scheduledProcess.LastRanDateTime) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            scheduledProcess.ScheduledProcessNum = Db.NonQ(command, true, "ScheduledProcessNum", "scheduledProcess");
        return scheduledProcess.ScheduledProcessNum;
    }

    public static void Update(ScheduledProcess scheduledProcess)
    {
        var command = "UPDATE scheduledprocess SET "
                      + "ScheduledAction    = '" + SOut.String(scheduledProcess.ScheduledAction.ToString()) + "', "
                      + "TimeToRun          =  " + SOut.DateT(scheduledProcess.TimeToRun) + ", "
                      + "FrequencyToRun     = '" + SOut.String(scheduledProcess.FrequencyToRun.ToString()) + "', "
                      + "LastRanDateTime    =  " + SOut.DateT(scheduledProcess.LastRanDateTime) + " "
                      + "WHERE ScheduledProcessNum = " + SOut.Long(scheduledProcess.ScheduledProcessNum);
        Db.NonQ(command);
    }

    public static bool Update(ScheduledProcess scheduledProcess, ScheduledProcess oldScheduledProcess)
    {
        var command = "";
        if (scheduledProcess.ScheduledAction != oldScheduledProcess.ScheduledAction)
        {
            if (command != "") command += ",";
            command += "ScheduledAction = '" + SOut.String(scheduledProcess.ScheduledAction.ToString()) + "'";
        }

        if (scheduledProcess.TimeToRun != oldScheduledProcess.TimeToRun)
        {
            if (command != "") command += ",";
            command += "TimeToRun = " + SOut.DateT(scheduledProcess.TimeToRun) + "";
        }

        if (scheduledProcess.FrequencyToRun != oldScheduledProcess.FrequencyToRun)
        {
            if (command != "") command += ",";
            command += "FrequencyToRun = '" + SOut.String(scheduledProcess.FrequencyToRun.ToString()) + "'";
        }

        if (scheduledProcess.LastRanDateTime != oldScheduledProcess.LastRanDateTime)
        {
            if (command != "") command += ",";
            command += "LastRanDateTime = " + SOut.DateT(scheduledProcess.LastRanDateTime) + "";
        }

        if (command == "") return false;
        command = "UPDATE scheduledprocess SET " + command
                                                 + " WHERE ScheduledProcessNum = " + SOut.Long(scheduledProcess.ScheduledProcessNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ScheduledProcess scheduledProcess, ScheduledProcess oldScheduledProcess)
    {
        if (scheduledProcess.ScheduledAction != oldScheduledProcess.ScheduledAction) return true;
        if (scheduledProcess.TimeToRun != oldScheduledProcess.TimeToRun) return true;
        if (scheduledProcess.FrequencyToRun != oldScheduledProcess.FrequencyToRun) return true;
        if (scheduledProcess.LastRanDateTime != oldScheduledProcess.LastRanDateTime) return true;
        return false;
    }

    public static void Delete(long scheduledProcessNum)
    {
        var command = "DELETE FROM scheduledprocess "
                      + "WHERE ScheduledProcessNum = " + SOut.Long(scheduledProcessNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listScheduledProcessNums)
    {
        if (listScheduledProcessNums == null || listScheduledProcessNums.Count == 0) return;
        var command = "DELETE FROM scheduledprocess "
                      + "WHERE ScheduledProcessNum IN(" + string.Join(",", listScheduledProcessNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}