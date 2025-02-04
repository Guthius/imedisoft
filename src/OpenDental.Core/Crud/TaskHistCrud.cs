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
        foreach (DataRow row in table.Rows)
        {
            var taskHist = new TaskHist
            {
                TaskHistNum = SIn.Long(row["TaskHistNum"].ToString()),
                UserNumHist = SIn.Long(row["UserNumHist"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                IsNoteChange = SIn.Bool(row["IsNoteChange"].ToString()),
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