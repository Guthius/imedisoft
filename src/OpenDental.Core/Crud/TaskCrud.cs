using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var task = new Task
            {
                TaskNum = SIn.Long(row["TaskNum"].ToString()),
                TaskListNum = SIn.Long(row["TaskListNum"].ToString()),
                DateTask = SIn.Date(row["DateTask"].ToString()),
                KeyNum = SIn.Long(row["KeyNum"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                TaskStatus = (TaskStatusEnum) SIn.Int(row["TaskStatus"].ToString()),
                IsRepeating = SIn.Bool(row["IsRepeating"].ToString()),
                DateType = (TaskDateType) SIn.Int(row["DateType"].ToString()),
                FromNum = SIn.Long(row["FromNum"].ToString()),
                ObjectType = (TaskObjectType) SIn.Int(row["ObjectType"].ToString()),
                DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                DateTimeFinished = SIn.DateTime(row["DateTimeFinished"].ToString()),
                PriorityDefNum = SIn.Long(row["PriorityDefNum"].ToString()),
                ReminderGroupId = SIn.String(row["ReminderGroupId"].ToString()),
                ReminderType = (TaskReminderType) SIn.Int(row["ReminderType"].ToString()),
                ReminderFrequency = SIn.Int(row["ReminderFrequency"].ToString()),
                DateTimeOriginal = SIn.DateTime(row["DateTimeOriginal"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                DescriptOverride = SIn.String(row["DescriptOverride"].ToString()),
                IsReadOnly = SIn.Bool(row["IsReadOnly"].ToString()),
                TriageCategory = SIn.Long(row["TriageCategory"].ToString())
            };
            retVal.Add(task);
        }

        return retVal;
    }

    public static void Insert(Task task)
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
                                        + SOut.DateTime(task.DateTimeEntry) + ","
                                        + SOut.Long(task.UserNum) + ","
                                        + SOut.DateTime(task.DateTimeFinished) + ","
                                        + SOut.Long(task.PriorityDefNum) + ","
                                        + "'" + SOut.String(task.ReminderGroupId) + "',"
                                        + SOut.Int((int) task.ReminderType) + ","
                                        + SOut.Int(task.ReminderFrequency) + ","
                                        + "NOW()" + ","
                                        //SecDateTEdit can only be set by MySQL
                                        + "'" + SOut.String(task.DescriptOverride) + "',"
                                        + SOut.Bool(task.IsReadOnly) + ","
                                        + SOut.Long(task.TriageCategory) + ")";
        if (task.Descript == null) task.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", SOut.StringParam(task.Descript));
        {
            task.TaskNum = Db.NonQ(command, true, "TaskNum", "task", paramDescript);
        }
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
                      + "DateTimeEntry    =  " + SOut.DateTime(task.DateTimeEntry) + ", "
                      + "UserNum          =  " + SOut.Long(task.UserNum) + ", "
                      + "DateTimeFinished =  " + SOut.DateTime(task.DateTimeFinished) + ", "
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
        var paramDescript = new OdSqlParameter("paramDescript", SOut.StringParam(task.Descript));
        Db.NonQ(command, paramDescript);
    }

    public static void ClearFkey(long taskNum)
    {
        if (taskNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(taskNum) + " AND PermType IN (66)";
        Db.NonQ(command);
    }
}