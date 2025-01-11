#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskListCrud
{
    public static TaskList SelectOne(long taskListNum)
    {
        var command = "SELECT * FROM tasklist "
                      + "WHERE TaskListNum = " + SOut.Long(taskListNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskList SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskList> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskList> TableToList(DataTable table)
    {
        var retVal = new List<TaskList>();
        TaskList taskList;
        foreach (DataRow row in table.Rows)
        {
            taskList = new TaskList();
            taskList.TaskListNum = SIn.Long(row["TaskListNum"].ToString());
            taskList.Descript = SIn.String(row["Descript"].ToString());
            taskList.Parent = SIn.Long(row["Parent"].ToString());
            taskList.DateTL = SIn.Date(row["DateTL"].ToString());
            taskList.IsRepeating = SIn.Bool(row["IsRepeating"].ToString());
            taskList.DateType = (TaskDateType) SIn.Int(row["DateType"].ToString());
            taskList.FromNum = SIn.Long(row["FromNum"].ToString());
            taskList.ObjectType = (TaskObjectType) SIn.Int(row["ObjectType"].ToString());
            taskList.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            taskList.GlobalTaskFilterType = (EnumTaskFilterType) SIn.Int(row["GlobalTaskFilterType"].ToString());
            taskList.TaskListStatus = (TaskListStatusEnum) SIn.Int(row["TaskListStatus"].ToString());
            retVal.Add(taskList);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskList> listTaskLists, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskList";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskListNum");
        table.Columns.Add("Descript");
        table.Columns.Add("Parent");
        table.Columns.Add("DateTL");
        table.Columns.Add("IsRepeating");
        table.Columns.Add("DateType");
        table.Columns.Add("FromNum");
        table.Columns.Add("ObjectType");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("GlobalTaskFilterType");
        table.Columns.Add("TaskListStatus");
        foreach (var taskList in listTaskLists)
            table.Rows.Add(SOut.Long(taskList.TaskListNum), taskList.Descript, SOut.Long(taskList.Parent), SOut.DateT(taskList.DateTL, false), SOut.Bool(taskList.IsRepeating), SOut.Int((int) taskList.DateType), SOut.Long(taskList.FromNum), SOut.Int((int) taskList.ObjectType), SOut.DateT(taskList.DateTimeEntry, false), SOut.Int((int) taskList.GlobalTaskFilterType), SOut.Int((int) taskList.TaskListStatus));
        return table;
    }

    public static long Insert(TaskList taskList)
    {
        return Insert(taskList, false);
    }

    public static long Insert(TaskList taskList, bool useExistingPK)
    {
        var command = "INSERT INTO tasklist (";

        command += "Descript,Parent,DateTL,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,GlobalTaskFilterType,TaskListStatus) VALUES(";

        command +=
            "'" + SOut.String(taskList.Descript) + "',"
            + SOut.Long(taskList.Parent) + ","
            + SOut.Date(taskList.DateTL) + ","
            + SOut.Bool(taskList.IsRepeating) + ","
            + SOut.Int((int) taskList.DateType) + ","
            + SOut.Long(taskList.FromNum) + ","
            + SOut.Int((int) taskList.ObjectType) + ","
            + DbHelper.Now() + ","
            + SOut.Int((int) taskList.GlobalTaskFilterType) + ","
            + SOut.Int((int) taskList.TaskListStatus) + ")";
        {
            taskList.TaskListNum = Db.NonQ(command, true, "TaskListNum", "taskList");
        }
        return taskList.TaskListNum;
    }

    public static long InsertNoCache(TaskList taskList)
    {
        return InsertNoCache(taskList, false);
    }

    public static long InsertNoCache(TaskList taskList, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO tasklist (";
        if (isRandomKeys || useExistingPK) command += "TaskListNum,";
        command += "Descript,Parent,DateTL,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,GlobalTaskFilterType,TaskListStatus) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskList.TaskListNum) + ",";
        command +=
            "'" + SOut.String(taskList.Descript) + "',"
            + SOut.Long(taskList.Parent) + ","
            + SOut.Date(taskList.DateTL) + ","
            + SOut.Bool(taskList.IsRepeating) + ","
            + SOut.Int((int) taskList.DateType) + ","
            + SOut.Long(taskList.FromNum) + ","
            + SOut.Int((int) taskList.ObjectType) + ","
            + DbHelper.Now() + ","
            + SOut.Int((int) taskList.GlobalTaskFilterType) + ","
            + SOut.Int((int) taskList.TaskListStatus) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            taskList.TaskListNum = Db.NonQ(command, true, "TaskListNum", "taskList");
        return taskList.TaskListNum;
    }

    public static void Update(TaskList taskList)
    {
        var command = "UPDATE tasklist SET "
                      + "Descript            = '" + SOut.String(taskList.Descript) + "', "
                      + "Parent              =  " + SOut.Long(taskList.Parent) + ", "
                      + "DateTL              =  " + SOut.Date(taskList.DateTL) + ", "
                      + "IsRepeating         =  " + SOut.Bool(taskList.IsRepeating) + ", "
                      + "DateType            =  " + SOut.Int((int) taskList.DateType) + ", "
                      + "FromNum             =  " + SOut.Long(taskList.FromNum) + ", "
                      + "ObjectType          =  " + SOut.Int((int) taskList.ObjectType) + ", "
                      + "DateTimeEntry       =  " + SOut.DateT(taskList.DateTimeEntry) + ", "
                      + "GlobalTaskFilterType=  " + SOut.Int((int) taskList.GlobalTaskFilterType) + ", "
                      + "TaskListStatus      =  " + SOut.Int((int) taskList.TaskListStatus) + " "
                      + "WHERE TaskListNum = " + SOut.Long(taskList.TaskListNum);
        Db.NonQ(command);
    }

    public static bool Update(TaskList taskList, TaskList oldTaskList)
    {
        var command = "";
        if (taskList.Descript != oldTaskList.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(taskList.Descript) + "'";
        }

        if (taskList.Parent != oldTaskList.Parent)
        {
            if (command != "") command += ",";
            command += "Parent = " + SOut.Long(taskList.Parent) + "";
        }

        if (taskList.DateTL.Date != oldTaskList.DateTL.Date)
        {
            if (command != "") command += ",";
            command += "DateTL = " + SOut.Date(taskList.DateTL) + "";
        }

        if (taskList.IsRepeating != oldTaskList.IsRepeating)
        {
            if (command != "") command += ",";
            command += "IsRepeating = " + SOut.Bool(taskList.IsRepeating) + "";
        }

        if (taskList.DateType != oldTaskList.DateType)
        {
            if (command != "") command += ",";
            command += "DateType = " + SOut.Int((int) taskList.DateType) + "";
        }

        if (taskList.FromNum != oldTaskList.FromNum)
        {
            if (command != "") command += ",";
            command += "FromNum = " + SOut.Long(taskList.FromNum) + "";
        }

        if (taskList.ObjectType != oldTaskList.ObjectType)
        {
            if (command != "") command += ",";
            command += "ObjectType = " + SOut.Int((int) taskList.ObjectType) + "";
        }

        if (taskList.DateTimeEntry != oldTaskList.DateTimeEntry)
        {
            if (command != "") command += ",";
            command += "DateTimeEntry = " + SOut.DateT(taskList.DateTimeEntry) + "";
        }

        if (taskList.GlobalTaskFilterType != oldTaskList.GlobalTaskFilterType)
        {
            if (command != "") command += ",";
            command += "GlobalTaskFilterType = " + SOut.Int((int) taskList.GlobalTaskFilterType) + "";
        }

        if (taskList.TaskListStatus != oldTaskList.TaskListStatus)
        {
            if (command != "") command += ",";
            command += "TaskListStatus = " + SOut.Int((int) taskList.TaskListStatus) + "";
        }

        if (command == "") return false;
        command = "UPDATE tasklist SET " + command
                                         + " WHERE TaskListNum = " + SOut.Long(taskList.TaskListNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TaskList taskList, TaskList oldTaskList)
    {
        if (taskList.Descript != oldTaskList.Descript) return true;
        if (taskList.Parent != oldTaskList.Parent) return true;
        if (taskList.DateTL.Date != oldTaskList.DateTL.Date) return true;
        if (taskList.IsRepeating != oldTaskList.IsRepeating) return true;
        if (taskList.DateType != oldTaskList.DateType) return true;
        if (taskList.FromNum != oldTaskList.FromNum) return true;
        if (taskList.ObjectType != oldTaskList.ObjectType) return true;
        if (taskList.DateTimeEntry != oldTaskList.DateTimeEntry) return true;
        if (taskList.GlobalTaskFilterType != oldTaskList.GlobalTaskFilterType) return true;
        if (taskList.TaskListStatus != oldTaskList.TaskListStatus) return true;
        return false;
    }

    public static void Delete(long taskListNum)
    {
        var command = "DELETE FROM tasklist "
                      + "WHERE TaskListNum = " + SOut.Long(taskListNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskListNums)
    {
        if (listTaskListNums == null || listTaskListNums.Count == 0) return;
        var command = "DELETE FROM tasklist "
                      + "WHERE TaskListNum IN(" + string.Join(",", listTaskListNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}