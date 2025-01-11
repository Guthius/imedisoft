#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskHistCrud
{
    public static TaskHist SelectOne(long taskHistNum)
    {
        var command = "SELECT * FROM taskhist "
                      + "WHERE TaskHistNum = " + SOut.Long(taskHistNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskHist SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskHist> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskHist> TableToList(DataTable table)
    {
        var retVal = new List<TaskHist>();
        TaskHist taskHist;
        foreach (DataRow row in table.Rows)
        {
            taskHist = new TaskHist();
            taskHist.TaskHistNum = SIn.Long(row["TaskHistNum"].ToString());
            taskHist.UserNumHist = SIn.Long(row["UserNumHist"].ToString());
            taskHist.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            taskHist.IsNoteChange = SIn.Bool(row["IsNoteChange"].ToString());
            taskHist.TaskNum = SIn.Long(row["TaskNum"].ToString());
            taskHist.TaskListNum = SIn.Long(row["TaskListNum"].ToString());
            taskHist.DateTask = SIn.Date(row["DateTask"].ToString());
            taskHist.KeyNum = SIn.Long(row["KeyNum"].ToString());
            taskHist.Descript = SIn.String(row["Descript"].ToString());
            taskHist.TaskStatus = (TaskStatusEnum) SIn.Int(row["TaskStatus"].ToString());
            taskHist.IsRepeating = SIn.Bool(row["IsRepeating"].ToString());
            taskHist.DateType = (TaskDateType) SIn.Int(row["DateType"].ToString());
            taskHist.FromNum = SIn.Long(row["FromNum"].ToString());
            taskHist.ObjectType = (TaskObjectType) SIn.Int(row["ObjectType"].ToString());
            taskHist.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            taskHist.UserNum = SIn.Long(row["UserNum"].ToString());
            taskHist.DateTimeFinished = SIn.DateTime(row["DateTimeFinished"].ToString());
            taskHist.PriorityDefNum = SIn.Long(row["PriorityDefNum"].ToString());
            taskHist.ReminderGroupId = SIn.String(row["ReminderGroupId"].ToString());
            taskHist.ReminderType = (TaskReminderType) SIn.Int(row["ReminderType"].ToString());
            taskHist.ReminderFrequency = SIn.Int(row["ReminderFrequency"].ToString());
            taskHist.DateTimeOriginal = SIn.DateTime(row["DateTimeOriginal"].ToString());
            taskHist.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            taskHist.DescriptOverride = SIn.String(row["DescriptOverride"].ToString());
            taskHist.IsReadOnly = SIn.Bool(row["IsReadOnly"].ToString());
            taskHist.TriageCategory = SIn.Long(row["TriageCategory"].ToString());
            retVal.Add(taskHist);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskHist> listTaskHists, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskHist";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskHistNum");
        table.Columns.Add("UserNumHist");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("IsNoteChange");
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
        foreach (var taskHist in listTaskHists)
            table.Rows.Add(SOut.Long(taskHist.TaskHistNum), SOut.Long(taskHist.UserNumHist), SOut.DateT(taskHist.DateTStamp, false), SOut.Bool(taskHist.IsNoteChange), SOut.Long(taskHist.TaskNum), SOut.Long(taskHist.TaskListNum), SOut.DateT(taskHist.DateTask, false), SOut.Long(taskHist.KeyNum), taskHist.Descript, SOut.Int((int) taskHist.TaskStatus), SOut.Bool(taskHist.IsRepeating), SOut.Int((int) taskHist.DateType), SOut.Long(taskHist.FromNum), SOut.Int((int) taskHist.ObjectType), SOut.DateT(taskHist.DateTimeEntry, false), SOut.Long(taskHist.UserNum), SOut.DateT(taskHist.DateTimeFinished, false), SOut.Long(taskHist.PriorityDefNum), taskHist.ReminderGroupId, SOut.Int((int) taskHist.ReminderType), SOut.Int(taskHist.ReminderFrequency), SOut.DateT(taskHist.DateTimeOriginal, false), SOut.DateT(taskHist.SecDateTEdit, false), taskHist.DescriptOverride, SOut.Bool(taskHist.IsReadOnly), SOut.Long(taskHist.TriageCategory));
        return table;
    }

    public static long Insert(TaskHist taskHist)
    {
        return Insert(taskHist, false);
    }

    public static long Insert(TaskHist taskHist, bool useExistingPK)
    {
        var command = "INSERT INTO taskhist (";

        command += "UserNumHist,DateTStamp,IsNoteChange,TaskNum,TaskListNum,DateTask,KeyNum,Descript,TaskStatus,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,UserNum,DateTimeFinished,PriorityDefNum,ReminderGroupId,ReminderType,ReminderFrequency,DateTimeOriginal,DescriptOverride,IsReadOnly,TriageCategory) VALUES(";

        command +=
            SOut.Long(taskHist.UserNumHist) + ","
                                            + DbHelper.Now() + ","
                                            + SOut.Bool(taskHist.IsNoteChange) + ","
                                            + SOut.Long(taskHist.TaskNum) + ","
                                            + SOut.Long(taskHist.TaskListNum) + ","
                                            + SOut.Date(taskHist.DateTask) + ","
                                            + SOut.Long(taskHist.KeyNum) + ","
                                            + DbHelper.ParamChar + "paramDescript,"
                                            + SOut.Int((int) taskHist.TaskStatus) + ","
                                            + SOut.Bool(taskHist.IsRepeating) + ","
                                            + SOut.Int((int) taskHist.DateType) + ","
                                            + SOut.Long(taskHist.FromNum) + ","
                                            + SOut.Int((int) taskHist.ObjectType) + ","
                                            + SOut.DateT(taskHist.DateTimeEntry) + ","
                                            + SOut.Long(taskHist.UserNum) + ","
                                            + SOut.DateT(taskHist.DateTimeFinished) + ","
                                            + SOut.Long(taskHist.PriorityDefNum) + ","
                                            + "'" + SOut.String(taskHist.ReminderGroupId) + "',"
                                            + SOut.Int((int) taskHist.ReminderType) + ","
                                            + SOut.Int(taskHist.ReminderFrequency) + ","
                                            + SOut.DateT(taskHist.DateTimeOriginal) + ","
                                            //SecDateTEdit can only be set by MySQL
                                            + "'" + SOut.String(taskHist.DescriptOverride) + "',"
                                            + SOut.Bool(taskHist.IsReadOnly) + ","
                                            + SOut.Long(taskHist.TriageCategory) + ")";
        if (taskHist.Descript == null) taskHist.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(taskHist.Descript));
        {
            taskHist.TaskHistNum = Db.NonQ(command, true, "TaskHistNum", "taskHist", paramDescript);
        }
        return taskHist.TaskHistNum;
    }

    public static long InsertNoCache(TaskHist taskHist)
    {
        return InsertNoCache(taskHist, false);
    }

    public static long InsertNoCache(TaskHist taskHist, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO taskhist (";
        if (isRandomKeys || useExistingPK) command += "TaskHistNum,";
        command += "UserNumHist,DateTStamp,IsNoteChange,TaskNum,TaskListNum,DateTask,KeyNum,Descript,TaskStatus,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,UserNum,DateTimeFinished,PriorityDefNum,ReminderGroupId,ReminderType,ReminderFrequency,DateTimeOriginal,DescriptOverride,IsReadOnly,TriageCategory) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskHist.TaskHistNum) + ",";
        command +=
            SOut.Long(taskHist.UserNumHist) + ","
                                            + DbHelper.Now() + ","
                                            + SOut.Bool(taskHist.IsNoteChange) + ","
                                            + SOut.Long(taskHist.TaskNum) + ","
                                            + SOut.Long(taskHist.TaskListNum) + ","
                                            + SOut.Date(taskHist.DateTask) + ","
                                            + SOut.Long(taskHist.KeyNum) + ","
                                            + DbHelper.ParamChar + "paramDescript,"
                                            + SOut.Int((int) taskHist.TaskStatus) + ","
                                            + SOut.Bool(taskHist.IsRepeating) + ","
                                            + SOut.Int((int) taskHist.DateType) + ","
                                            + SOut.Long(taskHist.FromNum) + ","
                                            + SOut.Int((int) taskHist.ObjectType) + ","
                                            + SOut.DateT(taskHist.DateTimeEntry) + ","
                                            + SOut.Long(taskHist.UserNum) + ","
                                            + SOut.DateT(taskHist.DateTimeFinished) + ","
                                            + SOut.Long(taskHist.PriorityDefNum) + ","
                                            + "'" + SOut.String(taskHist.ReminderGroupId) + "',"
                                            + SOut.Int((int) taskHist.ReminderType) + ","
                                            + SOut.Int(taskHist.ReminderFrequency) + ","
                                            + SOut.DateT(taskHist.DateTimeOriginal) + ","
                                            //SecDateTEdit can only be set by MySQL
                                            + "'" + SOut.String(taskHist.DescriptOverride) + "',"
                                            + SOut.Bool(taskHist.IsReadOnly) + ","
                                            + SOut.Long(taskHist.TriageCategory) + ")";
        if (taskHist.Descript == null) taskHist.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(taskHist.Descript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescript);
        else
            taskHist.TaskHistNum = Db.NonQ(command, true, "TaskHistNum", "taskHist", paramDescript);
        return taskHist.TaskHistNum;
    }

    public static void Update(TaskHist taskHist)
    {
        var command = "UPDATE taskhist SET "
                      + "UserNumHist      =  " + SOut.Long(taskHist.UserNumHist) + ", "
                      //DateTStamp not allowed to change
                      + "IsNoteChange     =  " + SOut.Bool(taskHist.IsNoteChange) + ", "
                      + "TaskNum          =  " + SOut.Long(taskHist.TaskNum) + ", "
                      + "TaskListNum      =  " + SOut.Long(taskHist.TaskListNum) + ", "
                      + "DateTask         =  " + SOut.Date(taskHist.DateTask) + ", "
                      + "KeyNum           =  " + SOut.Long(taskHist.KeyNum) + ", "
                      + "Descript         =  " + DbHelper.ParamChar + "paramDescript, "
                      + "TaskStatus       =  " + SOut.Int((int) taskHist.TaskStatus) + ", "
                      + "IsRepeating      =  " + SOut.Bool(taskHist.IsRepeating) + ", "
                      + "DateType         =  " + SOut.Int((int) taskHist.DateType) + ", "
                      + "FromNum          =  " + SOut.Long(taskHist.FromNum) + ", "
                      + "ObjectType       =  " + SOut.Int((int) taskHist.ObjectType) + ", "
                      + "DateTimeEntry    =  " + SOut.DateT(taskHist.DateTimeEntry) + ", "
                      + "UserNum          =  " + SOut.Long(taskHist.UserNum) + ", "
                      + "DateTimeFinished =  " + SOut.DateT(taskHist.DateTimeFinished) + ", "
                      + "PriorityDefNum   =  " + SOut.Long(taskHist.PriorityDefNum) + ", "
                      + "ReminderGroupId  = '" + SOut.String(taskHist.ReminderGroupId) + "', "
                      + "ReminderType     =  " + SOut.Int((int) taskHist.ReminderType) + ", "
                      + "ReminderFrequency=  " + SOut.Int(taskHist.ReminderFrequency) + ", "
                      + "DateTimeOriginal =  " + SOut.DateT(taskHist.DateTimeOriginal) + ", "
                      //SecDateTEdit can only be set by MySQL
                      + "DescriptOverride = '" + SOut.String(taskHist.DescriptOverride) + "', "
                      + "IsReadOnly       =  " + SOut.Bool(taskHist.IsReadOnly) + ", "
                      + "TriageCategory   =  " + SOut.Long(taskHist.TriageCategory) + " "
                      + "WHERE TaskHistNum = " + SOut.Long(taskHist.TaskHistNum);
        if (taskHist.Descript == null) taskHist.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(taskHist.Descript));
        Db.NonQ(command, paramDescript);
    }

    public static bool Update(TaskHist taskHist, TaskHist oldTaskHist)
    {
        var command = "";
        if (taskHist.UserNumHist != oldTaskHist.UserNumHist)
        {
            if (command != "") command += ",";
            command += "UserNumHist = " + SOut.Long(taskHist.UserNumHist) + "";
        }

        //DateTStamp not allowed to change
        if (taskHist.IsNoteChange != oldTaskHist.IsNoteChange)
        {
            if (command != "") command += ",";
            command += "IsNoteChange = " + SOut.Bool(taskHist.IsNoteChange) + "";
        }

        if (taskHist.TaskNum != oldTaskHist.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskHist.TaskNum) + "";
        }

        if (taskHist.TaskListNum != oldTaskHist.TaskListNum)
        {
            if (command != "") command += ",";
            command += "TaskListNum = " + SOut.Long(taskHist.TaskListNum) + "";
        }

        if (taskHist.DateTask.Date != oldTaskHist.DateTask.Date)
        {
            if (command != "") command += ",";
            command += "DateTask = " + SOut.Date(taskHist.DateTask) + "";
        }

        if (taskHist.KeyNum != oldTaskHist.KeyNum)
        {
            if (command != "") command += ",";
            command += "KeyNum = " + SOut.Long(taskHist.KeyNum) + "";
        }

        if (taskHist.Descript != oldTaskHist.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = " + DbHelper.ParamChar + "paramDescript";
        }

        if (taskHist.TaskStatus != oldTaskHist.TaskStatus)
        {
            if (command != "") command += ",";
            command += "TaskStatus = " + SOut.Int((int) taskHist.TaskStatus) + "";
        }

        if (taskHist.IsRepeating != oldTaskHist.IsRepeating)
        {
            if (command != "") command += ",";
            command += "IsRepeating = " + SOut.Bool(taskHist.IsRepeating) + "";
        }

        if (taskHist.DateType != oldTaskHist.DateType)
        {
            if (command != "") command += ",";
            command += "DateType = " + SOut.Int((int) taskHist.DateType) + "";
        }

        if (taskHist.FromNum != oldTaskHist.FromNum)
        {
            if (command != "") command += ",";
            command += "FromNum = " + SOut.Long(taskHist.FromNum) + "";
        }

        if (taskHist.ObjectType != oldTaskHist.ObjectType)
        {
            if (command != "") command += ",";
            command += "ObjectType = " + SOut.Int((int) taskHist.ObjectType) + "";
        }

        if (taskHist.DateTimeEntry != oldTaskHist.DateTimeEntry)
        {
            if (command != "") command += ",";
            command += "DateTimeEntry = " + SOut.DateT(taskHist.DateTimeEntry) + "";
        }

        if (taskHist.UserNum != oldTaskHist.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(taskHist.UserNum) + "";
        }

        if (taskHist.DateTimeFinished != oldTaskHist.DateTimeFinished)
        {
            if (command != "") command += ",";
            command += "DateTimeFinished = " + SOut.DateT(taskHist.DateTimeFinished) + "";
        }

        if (taskHist.PriorityDefNum != oldTaskHist.PriorityDefNum)
        {
            if (command != "") command += ",";
            command += "PriorityDefNum = " + SOut.Long(taskHist.PriorityDefNum) + "";
        }

        if (taskHist.ReminderGroupId != oldTaskHist.ReminderGroupId)
        {
            if (command != "") command += ",";
            command += "ReminderGroupId = '" + SOut.String(taskHist.ReminderGroupId) + "'";
        }

        if (taskHist.ReminderType != oldTaskHist.ReminderType)
        {
            if (command != "") command += ",";
            command += "ReminderType = " + SOut.Int((int) taskHist.ReminderType) + "";
        }

        if (taskHist.ReminderFrequency != oldTaskHist.ReminderFrequency)
        {
            if (command != "") command += ",";
            command += "ReminderFrequency = " + SOut.Int(taskHist.ReminderFrequency) + "";
        }

        if (taskHist.DateTimeOriginal != oldTaskHist.DateTimeOriginal)
        {
            if (command != "") command += ",";
            command += "DateTimeOriginal = " + SOut.DateT(taskHist.DateTimeOriginal) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (taskHist.DescriptOverride != oldTaskHist.DescriptOverride)
        {
            if (command != "") command += ",";
            command += "DescriptOverride = '" + SOut.String(taskHist.DescriptOverride) + "'";
        }

        if (taskHist.IsReadOnly != oldTaskHist.IsReadOnly)
        {
            if (command != "") command += ",";
            command += "IsReadOnly = " + SOut.Bool(taskHist.IsReadOnly) + "";
        }

        if (taskHist.TriageCategory != oldTaskHist.TriageCategory)
        {
            if (command != "") command += ",";
            command += "TriageCategory = " + SOut.Long(taskHist.TriageCategory) + "";
        }

        if (command == "") return false;
        if (taskHist.Descript == null) taskHist.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(taskHist.Descript));
        command = "UPDATE taskhist SET " + command
                                         + " WHERE TaskHistNum = " + SOut.Long(taskHist.TaskHistNum);
        Db.NonQ(command, paramDescript);
        return true;
    }

    public static bool UpdateComparison(TaskHist taskHist, TaskHist oldTaskHist)
    {
        if (taskHist.UserNumHist != oldTaskHist.UserNumHist) return true;
        //DateTStamp not allowed to change
        if (taskHist.IsNoteChange != oldTaskHist.IsNoteChange) return true;
        if (taskHist.TaskNum != oldTaskHist.TaskNum) return true;
        if (taskHist.TaskListNum != oldTaskHist.TaskListNum) return true;
        if (taskHist.DateTask.Date != oldTaskHist.DateTask.Date) return true;
        if (taskHist.KeyNum != oldTaskHist.KeyNum) return true;
        if (taskHist.Descript != oldTaskHist.Descript) return true;
        if (taskHist.TaskStatus != oldTaskHist.TaskStatus) return true;
        if (taskHist.IsRepeating != oldTaskHist.IsRepeating) return true;
        if (taskHist.DateType != oldTaskHist.DateType) return true;
        if (taskHist.FromNum != oldTaskHist.FromNum) return true;
        if (taskHist.ObjectType != oldTaskHist.ObjectType) return true;
        if (taskHist.DateTimeEntry != oldTaskHist.DateTimeEntry) return true;
        if (taskHist.UserNum != oldTaskHist.UserNum) return true;
        if (taskHist.DateTimeFinished != oldTaskHist.DateTimeFinished) return true;
        if (taskHist.PriorityDefNum != oldTaskHist.PriorityDefNum) return true;
        if (taskHist.ReminderGroupId != oldTaskHist.ReminderGroupId) return true;
        if (taskHist.ReminderType != oldTaskHist.ReminderType) return true;
        if (taskHist.ReminderFrequency != oldTaskHist.ReminderFrequency) return true;
        if (taskHist.DateTimeOriginal != oldTaskHist.DateTimeOriginal) return true;
        //SecDateTEdit can only be set by MySQL
        if (taskHist.DescriptOverride != oldTaskHist.DescriptOverride) return true;
        if (taskHist.IsReadOnly != oldTaskHist.IsReadOnly) return true;
        if (taskHist.TriageCategory != oldTaskHist.TriageCategory) return true;
        return false;
    }

    public static void Delete(long taskHistNum)
    {
        var command = "DELETE FROM taskhist "
                      + "WHERE TaskHistNum = " + SOut.Long(taskHistNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskHistNums)
    {
        if (listTaskHistNums == null || listTaskHistNums.Count == 0) return;
        var command = "DELETE FROM taskhist "
                      + "WHERE TaskHistNum IN(" + string.Join(",", listTaskHistNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}