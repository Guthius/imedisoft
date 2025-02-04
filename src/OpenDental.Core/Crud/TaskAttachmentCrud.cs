using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskAttachmentCrud
{
    public static TaskAttachment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskAttachment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskAttachment> TableToList(DataTable table)
    {
        var retVal = new List<TaskAttachment>();
        foreach (DataRow row in table.Rows)
        {
            var taskAttachment = new TaskAttachment
            {
                TaskAttachmentNum = SIn.Long(row["TaskAttachmentNum"].ToString()),
                TaskNum = SIn.Long(row["TaskNum"].ToString()),
                DocNum = SIn.Long(row["DocNum"].ToString()),
                TextValue = SIn.String(row["TextValue"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(taskAttachment);
        }

        return retVal;
    }

    public static void Insert(TaskAttachment taskAttachment)
    {
        var command = "INSERT INTO taskattachment (";

        command += "TaskNum,DocNum,TextValue,Description) VALUES(";

        command +=
            SOut.Long(taskAttachment.TaskNum) + ","
                                              + SOut.Long(taskAttachment.DocNum) + ","
                                              + DbHelper.ParamChar + "paramTextValue,"
                                              + "'" + SOut.String(taskAttachment.Description) + "')";
        if (taskAttachment.TextValue == null) taskAttachment.TextValue = "";
        var paramTextValue = new OdSqlParameter("paramTextValue", SOut.StringNote(taskAttachment.TextValue));
        {
            taskAttachment.TaskAttachmentNum = Db.NonQ(command, true, "TaskAttachmentNum", "taskAttachment", paramTextValue);
        }
    }

    public static void Update(TaskAttachment taskAttachment)
    {
        var command = "UPDATE taskattachment SET "
                      + "TaskNum          =  " + SOut.Long(taskAttachment.TaskNum) + ", "
                      + "DocNum           =  " + SOut.Long(taskAttachment.DocNum) + ", "
                      + "TextValue        =  " + DbHelper.ParamChar + "paramTextValue, "
                      + "Description      = '" + SOut.String(taskAttachment.Description) + "' "
                      + "WHERE TaskAttachmentNum = " + SOut.Long(taskAttachment.TaskAttachmentNum);
        if (taskAttachment.TextValue == null) taskAttachment.TextValue = "";
        var paramTextValue = new OdSqlParameter("paramTextValue", SOut.StringNote(taskAttachment.TextValue));
        Db.NonQ(command, paramTextValue);
    }

    public static void Delete(long taskAttachmentNum)
    {
        var command = "DELETE FROM taskattachment "
                      + "WHERE TaskAttachmentNum = " + SOut.Long(taskAttachmentNum);
        Db.NonQ(command);
    }
}