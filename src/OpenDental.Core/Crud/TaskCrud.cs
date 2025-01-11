#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskCrud
{
    public static Task SelectOne(long taskNum)
    {
        var command = "SELECT * FROM task "
                      + "WHERE TaskNum = " + SOut.Long(taskNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Task SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Task> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Task> TableToList(DataTable table)
    {
        var retVal = new List<Task>();
        Task task;
        foreach (DataRow row in table.Rows)
        {
            task = new Task();
            task.TaskNum = SIn.Long(row["TaskNum"].ToString());
            task.TaskListNum = SIn.Long(row["TaskListNum"].ToString());
            task.DateTask = SIn.Date(row["DateTask"].ToString());
            task.KeyNum = SIn.Long(row["KeyNum"].ToString());
            task.Descript = SIn.String(row["Descript"].ToString());
            task.TaskStatus = (TaskStatusEnum) SIn.Int(row["TaskStatus"].ToString());
            task.IsRepeating = SIn.Bool(row["IsRepeating"].ToString());
            task.DateType = (TaskDateType) SIn.Int(row["DateType"].ToString());
            task.FromNum = SIn.Long(row["FromNum"].ToString());
            task.ObjectType = (TaskObjectType) SIn.Int(row["ObjectType"].ToString());
            task.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            task.UserNum = SIn.Long(row["UserNum"].ToString());
            task.DateTimeFinished = SIn.DateTime(row["DateTimeFinished"].ToString());
            task.PriorityDefNum = SIn.Long(row["PriorityDefNum"].ToString());
            task.ReminderGroupId = SIn.String(row["ReminderGroupId"].ToString());
            task.ReminderType = (TaskReminderType) SIn.Int(row["ReminderType"].ToString());
            task.ReminderFrequency = SIn.Int(row["ReminderFrequency"].ToString());
            task.DateTimeOriginal = SIn.DateTime(row["DateTimeOriginal"].ToString());
            task.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            task.DescriptOverride = SIn.String(row["DescriptOverride"].ToString());
            task.IsReadOnly = SIn.Bool(row["IsReadOnly"].ToString());
            task.TriageCategory = SIn.Long(row["TriageCategory"].ToString());
            retVal.Add(task);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Task> listTasks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Task";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskNum");
        table.Columns.Add("TaskListNum");
        table.Columns.Add("DateTask");
        table.Columns.Add("KeyNum");
        table.Columns.Add("Descript");
        table.Columns.Add("TaskStatus");
        table.Columns.Add("IsRepeating");
        table.Columns.Add("DateType");
        table.Columns.Add("FromNum");
        table.Columns.Add("ObjectType");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("UserNum");
        table.Columns.Add("DateTimeFinished");
        table.Columns.Add("PriorityDefNum");
        table.Columns.Add("ReminderGroupId");
        table.Columns.Add("ReminderType");
        table.Columns.Add("ReminderFrequency");
        table.Columns.Add("DateTimeOriginal");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("DescriptOverride");
        table.Columns.Add("IsReadOnly");
        table.Columns.Add("TriageCategory");
        foreach (var task in listTasks)
            table.Rows.Add(SOut.Long(task.TaskNum), SOut.Long(task.TaskListNum), SOut.DateT(task.DateTask, false), SOut.Long(task.KeyNum), task.Descript, SOut.Int((int) task.TaskStatus), SOut.Bool(task.IsRepeating), SOut.Int((int) task.DateType), SOut.Long(task.FromNum), SOut.Int((int) task.ObjectType), SOut.DateT(task.DateTimeEntry, false), SOut.Long(task.UserNum), SOut.DateT(task.DateTimeFinished, false), SOut.Long(task.PriorityDefNum), task.ReminderGroupId, SOut.Int((int) task.ReminderType), SOut.Int(task.ReminderFrequency), SOut.DateT(task.DateTimeOriginal, false), SOut.DateT(task.SecDateTEdit, false), task.DescriptOverride, SOut.Bool(task.IsReadOnly), SOut.Long(task.TriageCategory));
        return table;
    }

    public static long Insert(Task task)
    {
        return Insert(task, false);
    }

    public static long Insert(Task task, bool useExistingPK)
    {
        var command = "INSERT INTO task (";

        command += "TaskListNum,DateTask,KeyNum,Descript,TaskStatus,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,UserNum,DateTimeFinished,PriorityDefNum,ReminderGroupId,ReminderType,ReminderFrequency,DateTimeOriginal,DescriptOverride,IsReadOnly,TriageCategory) VALUES(";

        command +=
            SOut.Long(task.TaskListNum) + ","
                                        + SOut.Date(task.DateTask) + ","
                                        + SOut.Long(task.KeyNum) + ","
                                        + DbHelper.ParamChar + "paramDescript,"
                                        + SOut.Int((int) task.TaskStatus) + ","
                                        + SOut.Bool(task.IsRepeating) + ","
                                        + SOut.Int((int) task.DateType) + ","
                                        + SOut.Long(task.FromNum) + ","
                                        + SOut.Int((int) task.ObjectType) + ","
                                        + SOut.DateT(task.DateTimeEntry) + ","
                                        + SOut.Long(task.UserNum) + ","
                                        + SOut.DateT(task.DateTimeFinished) + ","
                                        + SOut.Long(task.PriorityDefNum) + ","
                                        + "'" + SOut.String(task.ReminderGroupId) + "',"
                                        + SOut.Int((int) task.ReminderType) + ","
                                        + SOut.Int(task.ReminderFrequency) + ","
                                        + DbHelper.Now() + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + "'" + SOut.String(task.DescriptOverride) + "',"
                                        + SOut.Bool(task.IsReadOnly) + ","
                                        + SOut.Long(task.TriageCategory) + ")";
        if (task.Descript == null) task.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(task.Descript));
        {
            task.TaskNum = Db.NonQ(command, true, "TaskNum", "task", paramDescript);
        }
        return task.TaskNum;
    }

    public static long InsertNoCache(Task task)
    {
        return InsertNoCache(task, false);
    }

    public static long InsertNoCache(Task task, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO task (";
        if (isRandomKeys || useExistingPK) command += "TaskNum,";
        command += "TaskListNum,DateTask,KeyNum,Descript,TaskStatus,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,UserNum,DateTimeFinished,PriorityDefNum,ReminderGroupId,ReminderType,ReminderFrequency,DateTimeOriginal,DescriptOverride,IsReadOnly,TriageCategory) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(task.TaskNum) + ",";
        command +=
            SOut.Long(task.TaskListNum) + ","
                                        + SOut.Date(task.DateTask) + ","
                                        + SOut.Long(task.KeyNum) + ","
                                        + DbHelper.ParamChar + "paramDescript,"
                                        + SOut.Int((int) task.TaskStatus) + ","
                                        + SOut.Bool(task.IsRepeating) + ","
                                        + SOut.Int((int) task.DateType) + ","
                                        + SOut.Long(task.FromNum) + ","
                                        + SOut.Int((int) task.ObjectType) + ","
                                        + SOut.DateT(task.DateTimeEntry) + ","
                                        + SOut.Long(task.UserNum) + ","
                                        + SOut.DateT(task.DateTimeFinished) + ","
                                        + SOut.Long(task.PriorityDefNum) + ","
                                        + "'" + SOut.String(task.ReminderGroupId) + "',"
                                        + SOut.Int((int) task.ReminderType) + ","
                                        + SOut.Int(task.ReminderFrequency) + ","
                                        + DbHelper.Now() + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + "'" + SOut.String(task.DescriptOverride) + "',"
                                        + SOut.Bool(task.IsReadOnly) + ","
                                        + SOut.Long(task.TriageCategory) + ")";
        if (task.Descript == null) task.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(task.Descript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescript);
        else
            task.TaskNum = Db.NonQ(command, true, "TaskNum", "task", paramDescript);
        return task.TaskNum;
    }

    public static void Update(Task task)
    {
        var command = "UPDATE task SET "
                      + "TaskListNum      =  " + SOut.Long(task.TaskListNum) + ", "
                      + "DateTask         =  " + SOut.Date(task.DateTask) + ", "
                      + "KeyNum           =  " + SOut.Long(task.KeyNum) + ", "
                      + "Descript         =  " + DbHelper.ParamChar + "paramDescript, "
                      + "TaskStatus       =  " + SOut.Int((int) task.TaskStatus) + ", "
                      + "IsRepeating      =  " + SOut.Bool(task.IsRepeating) + ", "
                      + "DateType         =  " + SOut.Int((int) task.DateType) + ", "
                      + "FromNum          =  " + SOut.Long(task.FromNum) + ", "
                      + "ObjectType       =  " + SOut.Int((int) task.ObjectType) + ", "
                      + "DateTimeEntry    =  " + SOut.DateT(task.DateTimeEntry) + ", "
                      + "UserNum          =  " + SOut.Long(task.UserNum) + ", "
                      + "DateTimeFinished =  " + SOut.DateT(task.DateTimeFinished) + ", "
                      + "PriorityDefNum   =  " + SOut.Long(task.PriorityDefNum) + ", "
                      + "ReminderGroupId  = '" + SOut.String(task.ReminderGroupId) + "', "
                      + "ReminderType     =  " + SOut.Int((int) task.ReminderType) + ", "
                      + "ReminderFrequency=  " + SOut.Int(task.ReminderFrequency) + ", "
                      //DateTimeOriginal not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "DescriptOverride = '" + SOut.String(task.DescriptOverride) + "', "
                      + "IsReadOnly       =  " + SOut.Bool(task.IsReadOnly) + ", "
                      + "TriageCategory   =  " + SOut.Long(task.TriageCategory) + " "
                      + "WHERE TaskNum = " + SOut.Long(task.TaskNum);
        if (task.Descript == null) task.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(task.Descript));
        Db.NonQ(command, paramDescript);
    }

    public static bool Update(Task task, Task oldTask)
    {
        var command = "";
        if (task.TaskListNum != oldTask.TaskListNum)
        {
            if (command != "") command += ",";
            command += "TaskListNum = " + SOut.Long(task.TaskListNum) + "";
        }

        if (task.DateTask.Date != oldTask.DateTask.Date)
        {
            if (command != "") command += ",";
            command += "DateTask = " + SOut.Date(task.DateTask) + "";
        }

        if (task.KeyNum != oldTask.KeyNum)
        {
            if (command != "") command += ",";
            command += "KeyNum = " + SOut.Long(task.KeyNum) + "";
        }

        if (task.Descript != oldTask.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = " + DbHelper.ParamChar + "paramDescript";
        }

        if (task.TaskStatus != oldTask.TaskStatus)
        {
            if (command != "") command += ",";
            command += "TaskStatus = " + SOut.Int((int) task.TaskStatus) + "";
        }

        if (task.IsRepeating != oldTask.IsRepeating)
        {
            if (command != "") command += ",";
            command += "IsRepeating = " + SOut.Bool(task.IsRepeating) + "";
        }

        if (task.DateType != oldTask.DateType)
        {
            if (command != "") command += ",";
            command += "DateType = " + SOut.Int((int) task.DateType) + "";
        }

        if (task.FromNum != oldTask.FromNum)
        {
            if (command != "") command += ",";
            command += "FromNum = " + SOut.Long(task.FromNum) + "";
        }

        if (task.ObjectType != oldTask.ObjectType)
        {
            if (command != "") command += ",";
            command += "ObjectType = " + SOut.Int((int) task.ObjectType) + "";
        }

        if (task.DateTimeEntry != oldTask.DateTimeEntry)
        {
            if (command != "") command += ",";
            command += "DateTimeEntry = " + SOut.DateT(task.DateTimeEntry) + "";
        }

        if (task.UserNum != oldTask.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(task.UserNum) + "";
        }

        if (task.DateTimeFinished != oldTask.DateTimeFinished)
        {
            if (command != "") command += ",";
            command += "DateTimeFinished = " + SOut.DateT(task.DateTimeFinished) + "";
        }

        if (task.PriorityDefNum != oldTask.PriorityDefNum)
        {
            if (command != "") command += ",";
            command += "PriorityDefNum = " + SOut.Long(task.PriorityDefNum) + "";
        }

        if (task.ReminderGroupId != oldTask.ReminderGroupId)
        {
            if (command != "") command += ",";
            command += "ReminderGroupId = '" + SOut.String(task.ReminderGroupId) + "'";
        }

        if (task.ReminderType != oldTask.ReminderType)
        {
            if (command != "") command += ",";
            command += "ReminderType = " + SOut.Int((int) task.ReminderType) + "";
        }

        if (task.ReminderFrequency != oldTask.ReminderFrequency)
        {
            if (command != "") command += ",";
            command += "ReminderFrequency = " + SOut.Int(task.ReminderFrequency) + "";
        }

        //DateTimeOriginal not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (task.DescriptOverride != oldTask.DescriptOverride)
        {
            if (command != "") command += ",";
            command += "DescriptOverride = '" + SOut.String(task.DescriptOverride) + "'";
        }

        if (task.IsReadOnly != oldTask.IsReadOnly)
        {
            if (command != "") command += ",";
            command += "IsReadOnly = " + SOut.Bool(task.IsReadOnly) + "";
        }

        if (task.TriageCategory != oldTask.TriageCategory)
        {
            if (command != "") command += ",";
            command += "TriageCategory = " + SOut.Long(task.TriageCategory) + "";
        }

        if (command == "") return false;
        if (task.Descript == null) task.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(task.Descript));
        command = "UPDATE task SET " + command
                                     + " WHERE TaskNum = " + SOut.Long(task.TaskNum);
        Db.NonQ(command, paramDescript);
        return true;
    }

    public static bool UpdateComparison(Task task, Task oldTask)
    {
        if (task.TaskListNum != oldTask.TaskListNum) return true;
        if (task.DateTask.Date != oldTask.DateTask.Date) return true;
        if (task.KeyNum != oldTask.KeyNum) return true;
        if (task.Descript != oldTask.Descript) return true;
        if (task.TaskStatus != oldTask.TaskStatus) return true;
        if (task.IsRepeating != oldTask.IsRepeating) return true;
        if (task.DateType != oldTask.DateType) return true;
        if (task.FromNum != oldTask.FromNum) return true;
        if (task.ObjectType != oldTask.ObjectType) return true;
        if (task.DateTimeEntry != oldTask.DateTimeEntry) return true;
        if (task.UserNum != oldTask.UserNum) return true;
        if (task.DateTimeFinished != oldTask.DateTimeFinished) return true;
        if (task.PriorityDefNum != oldTask.PriorityDefNum) return true;
        if (task.ReminderGroupId != oldTask.ReminderGroupId) return true;
        if (task.ReminderType != oldTask.ReminderType) return true;
        if (task.ReminderFrequency != oldTask.ReminderFrequency) return true;
        //DateTimeOriginal not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (task.DescriptOverride != oldTask.DescriptOverride) return true;
        if (task.IsReadOnly != oldTask.IsReadOnly) return true;
        if (task.TriageCategory != oldTask.TriageCategory) return true;
        return false;
    }

    public static void Delete(long taskNum)
    {
        ClearFkey(taskNum);
        var command = "DELETE FROM task "
                      + "WHERE TaskNum = " + SOut.Long(taskNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskNums)
    {
        if (listTaskNums == null || listTaskNums.Count == 0) return;
        ClearFkey(listTaskNums);
        var command = "DELETE FROM task "
                      + "WHERE TaskNum IN(" + string.Join(",", listTaskNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void ClearFkey(long taskNum)
    {
        if (taskNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(taskNum) + " AND PermType IN (66)";
        Db.NonQ(command);
    }

    public static void ClearFkey(List<long> listTaskNums)
    {
        if (listTaskNums == null || listTaskNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listTaskNums.FindAll(x => x != 0)) + ") AND PermType IN (66)";
        Db.NonQ(command);
    }
}