using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ScheduledProcessCrud
{
    public static List<ScheduledProcess> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ScheduledProcess> TableToList(DataTable table)
    {
        var retVal = new List<ScheduledProcess>();
        foreach (DataRow row in table.Rows)
        {
            var scheduledProcess = new ScheduledProcess
            {
                ScheduledProcessNum = SIn.Long(row["ScheduledProcessNum"].ToString())
            };
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

    public static void Insert(ScheduledProcess scheduledProcess)
    {
        var command = "INSERT INTO scheduledprocess (";

        command += "ScheduledAction,TimeToRun,FrequencyToRun,LastRanDateTime) VALUES(";

        command +=
            "'" + SOut.String(scheduledProcess.ScheduledAction.ToString()) + "',"
            + SOut.DateTime(scheduledProcess.TimeToRun) + ","
            + "'" + SOut.String(scheduledProcess.FrequencyToRun.ToString()) + "',"
            + SOut.DateTime(scheduledProcess.LastRanDateTime) + ")";

        scheduledProcess.ScheduledProcessNum = Db.NonQ(command, true, "ScheduledProcessNum", "scheduledProcess");
    }

    public static void Update(ScheduledProcess scheduledProcess, ScheduledProcess oldScheduledProcess)
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
            command += "TimeToRun = " + SOut.DateTime(scheduledProcess.TimeToRun) + "";
        }

        if (scheduledProcess.FrequencyToRun != oldScheduledProcess.FrequencyToRun)
        {
            if (command != "") command += ",";
            command += "FrequencyToRun = '" + SOut.String(scheduledProcess.FrequencyToRun.ToString()) + "'";
        }

        if (scheduledProcess.LastRanDateTime != oldScheduledProcess.LastRanDateTime)
        {
            if (command != "") command += ",";
            command += "LastRanDateTime = " + SOut.DateTime(scheduledProcess.LastRanDateTime) + "";
        }

        if (command == "") return;
        command = "UPDATE scheduledprocess SET " + command
                                                 + " WHERE ScheduledProcessNum = " + SOut.Long(scheduledProcess.ScheduledProcessNum);
        Db.NonQ(command);
    }

    public static void Delete(long scheduledProcessNum)
    {
        var command = "DELETE FROM scheduledprocess "
                      + "WHERE ScheduledProcessNum = " + SOut.Long(scheduledProcessNum);
        Db.NonQ(command);
    }
}