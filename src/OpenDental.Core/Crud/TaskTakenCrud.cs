#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskTakenCrud
{
    public static TaskTaken SelectOne(long taskTakenNum)
    {
        var command = "SELECT * FROM tasktaken "
                      + "WHERE TaskTakenNum = " + SOut.Long(taskTakenNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskTaken SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskTaken> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskTaken> TableToList(DataTable table)
    {
        var retVal = new List<TaskTaken>();
        TaskTaken taskTaken;
        foreach (DataRow row in table.Rows)
        {
            taskTaken = new TaskTaken();
            taskTaken.TaskTakenNum = SIn.Long(row["TaskTakenNum"].ToString());
            taskTaken.TaskNum = SIn.Long(row["TaskNum"].ToString());
            taskTaken.LogJson = SIn.String(row["LogJson"].ToString());
            retVal.Add(taskTaken);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskTaken> listTaskTakens, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskTaken";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskTakenNum");
        table.Columns.Add("TaskNum");
        table.Columns.Add("LogJson");
        foreach (var taskTaken in listTaskTakens)
            table.Rows.Add(SOut.Long(taskTaken.TaskTakenNum), SOut.Long(taskTaken.TaskNum), taskTaken.LogJson);
        return table;
    }

    public static long Insert(TaskTaken taskTaken)
    {
        return Insert(taskTaken, false);
    }

    public static long Insert(TaskTaken taskTaken, bool useExistingPK)
    {
        var command = "INSERT INTO tasktaken (";

        command += "TaskNum,LogJson) VALUES(";

        command +=
            SOut.Long(taskTaken.TaskNum) + ","
                                         + DbHelper.ParamChar + "paramLogJson)";
        if (taskTaken.LogJson == null) taskTaken.LogJson = "";
        var paramLogJson = new OdSqlParameter("paramLogJson", OdDbType.Text, SOut.StringParam(taskTaken.LogJson));
        {
            taskTaken.TaskTakenNum = Db.NonQ(command, true, "TaskTakenNum", "taskTaken", paramLogJson);
        }
        return taskTaken.TaskTakenNum;
    }

    public static long InsertNoCache(TaskTaken taskTaken)
    {
        return InsertNoCache(taskTaken, false);
    }

    public static long InsertNoCache(TaskTaken taskTaken, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO tasktaken (";
        if (isRandomKeys || useExistingPK) command += "TaskTakenNum,";
        command += "TaskNum,LogJson) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskTaken.TaskTakenNum) + ",";
        command +=
            SOut.Long(taskTaken.TaskNum) + ","
                                         + DbHelper.ParamChar + "paramLogJson)";
        if (taskTaken.LogJson == null) taskTaken.LogJson = "";
        var paramLogJson = new OdSqlParameter("paramLogJson", OdDbType.Text, SOut.StringParam(taskTaken.LogJson));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramLogJson);
        else
            taskTaken.TaskTakenNum = Db.NonQ(command, true, "TaskTakenNum", "taskTaken", paramLogJson);
        return taskTaken.TaskTakenNum;
    }

    public static void Update(TaskTaken taskTaken)
    {
        var command = "UPDATE tasktaken SET "
                      + "TaskNum     =  " + SOut.Long(taskTaken.TaskNum) + ", "
                      + "LogJson     =  " + DbHelper.ParamChar + "paramLogJson "
                      + "WHERE TaskTakenNum = " + SOut.Long(taskTaken.TaskTakenNum);
        if (taskTaken.LogJson == null) taskTaken.LogJson = "";
        var paramLogJson = new OdSqlParameter("paramLogJson", OdDbType.Text, SOut.StringParam(taskTaken.LogJson));
        Db.NonQ(command, paramLogJson);
    }

    public static bool Update(TaskTaken taskTaken, TaskTaken oldTaskTaken)
    {
        var command = "";
        if (taskTaken.TaskNum != oldTaskTaken.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskTaken.TaskNum) + "";
        }

        if (taskTaken.LogJson != oldTaskTaken.LogJson)
        {
            if (command != "") command += ",";
            command += "LogJson = " + DbHelper.ParamChar + "paramLogJson";
        }

        if (command == "") return false;
        if (taskTaken.LogJson == null) taskTaken.LogJson = "";
        var paramLogJson = new OdSqlParameter("paramLogJson", OdDbType.Text, SOut.StringParam(taskTaken.LogJson));
        command = "UPDATE tasktaken SET " + command
                                          + " WHERE TaskTakenNum = " + SOut.Long(taskTaken.TaskTakenNum);
        Db.NonQ(command, paramLogJson);
        return true;
    }

    public static bool UpdateComparison(TaskTaken taskTaken, TaskTaken oldTaskTaken)
    {
        if (taskTaken.TaskNum != oldTaskTaken.TaskNum) return true;
        if (taskTaken.LogJson != oldTaskTaken.LogJson) return true;
        return false;
    }

    public static void Delete(long taskTakenNum)
    {
        var command = "DELETE FROM tasktaken "
                      + "WHERE TaskTakenNum = " + SOut.Long(taskTakenNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskTakenNums)
    {
        if (listTaskTakenNums == null || listTaskTakenNums.Count == 0) return;
        var command = "DELETE FROM tasktaken "
                      + "WHERE TaskTakenNum IN(" + string.Join(",", listTaskTakenNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}