using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskHistCrud
{
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

    public static void Insert(TaskHist taskHist)
    {
        var command = "INSERT INTO taskhist (";

        command += "UserNumHist,DateTStamp,IsNoteChange,TaskNum,TaskListNum,DateTask,KeyNum,Descript,TaskStatus,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,UserNum,DateTimeFinished,PriorityDefNum,ReminderGroupId,ReminderType,ReminderFrequency,DateTimeOriginal,DescriptOverride,IsReadOnly,TriageCategory) VALUES(";

        command +=
            SOut.Long(taskHist.UserNumHist) + ","
                                            + "NOW()" + ","
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
                                            + SOut.DateTime(taskHist.DateTimeEntry) + ","
                                            + SOut.Long(taskHist.UserNum) + ","
                                            + SOut.DateTime(taskHist.DateTimeFinished) + ","
                                            + SOut.Long(taskHist.PriorityDefNum) + ","
                                            + "'" + SOut.String(taskHist.ReminderGroupId) + "',"
                                            + SOut.Int((int) taskHist.ReminderType) + ","
                                            + SOut.Int(taskHist.ReminderFrequency) + ","
                                            + SOut.DateTime(taskHist.DateTimeOriginal) + ","
                                            //SecDateTEdit can only be set by MySQL
                                            + "'" + SOut.String(taskHist.DescriptOverride) + "',"
                                            + SOut.Bool(taskHist.IsReadOnly) + ","
                                            + SOut.Long(taskHist.TriageCategory) + ")";
        if (taskHist.Descript == null) taskHist.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", SOut.StringParam(taskHist.Descript));
        {
            taskHist.TaskHistNum = Db.NonQ(command, true, "TaskHistNum", "taskHist", paramDescript);
        }
    }
}