#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskAttachmentCrud
{
    public static TaskAttachment SelectOne(long taskAttachmentNum)
    {
        var command = "SELECT * FROM taskattachment "
                      + "WHERE TaskAttachmentNum = " + SOut.Long(taskAttachmentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        TaskAttachment taskAttachment;
        foreach (DataRow row in table.Rows)
        {
            taskAttachment = new TaskAttachment();
            taskAttachment.TaskAttachmentNum = SIn.Long(row["TaskAttachmentNum"].ToString());
            taskAttachment.TaskNum = SIn.Long(row["TaskNum"].ToString());
            taskAttachment.DocNum = SIn.Long(row["DocNum"].ToString());
            taskAttachment.TextValue = SIn.String(row["TextValue"].ToString());
            taskAttachment.Description = SIn.String(row["Description"].ToString());
            retVal.Add(taskAttachment);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskAttachment> listTaskAttachments, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskAttachment";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskAttachmentNum");
        table.Columns.Add("TaskNum");
        table.Columns.Add("DocNum");
        table.Columns.Add("TextValue");
        table.Columns.Add("Description");
        foreach (var taskAttachment in listTaskAttachments)
            table.Rows.Add(SOut.Long(taskAttachment.TaskAttachmentNum), SOut.Long(taskAttachment.TaskNum), SOut.Long(taskAttachment.DocNum), taskAttachment.TextValue, taskAttachment.Description);
        return table;
    }

    public static long Insert(TaskAttachment taskAttachment)
    {
        return Insert(taskAttachment, false);
    }

    public static long Insert(TaskAttachment taskAttachment, bool useExistingPK)
    {
        var command = "INSERT INTO taskattachment (";

        command += "TaskNum,DocNum,TextValue,Description) VALUES(";

        command +=
            SOut.Long(taskAttachment.TaskNum) + ","
                                              + SOut.Long(taskAttachment.DocNum) + ","
                                              + DbHelper.ParamChar + "paramTextValue,"
                                              + "'" + SOut.String(taskAttachment.Description) + "')";
        if (taskAttachment.TextValue == null) taskAttachment.TextValue = "";
        var paramTextValue = new OdSqlParameter("paramTextValue", OdDbType.Text, SOut.StringNote(taskAttachment.TextValue));
        {
            taskAttachment.TaskAttachmentNum = Db.NonQ(command, true, "TaskAttachmentNum", "taskAttachment", paramTextValue);
        }
        return taskAttachment.TaskAttachmentNum;
    }

    public static long InsertNoCache(TaskAttachment taskAttachment)
    {
        return InsertNoCache(taskAttachment, false);
    }

    public static long InsertNoCache(TaskAttachment taskAttachment, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO taskattachment (";
        if (isRandomKeys || useExistingPK) command += "TaskAttachmentNum,";
        command += "TaskNum,DocNum,TextValue,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskAttachment.TaskAttachmentNum) + ",";
        command +=
            SOut.Long(taskAttachment.TaskNum) + ","
                                              + SOut.Long(taskAttachment.DocNum) + ","
                                              + DbHelper.ParamChar + "paramTextValue,"
                                              + "'" + SOut.String(taskAttachment.Description) + "')";
        if (taskAttachment.TextValue == null) taskAttachment.TextValue = "";
        var paramTextValue = new OdSqlParameter("paramTextValue", OdDbType.Text, SOut.StringNote(taskAttachment.TextValue));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramTextValue);
        else
            taskAttachment.TaskAttachmentNum = Db.NonQ(command, true, "TaskAttachmentNum", "taskAttachment", paramTextValue);
        return taskAttachment.TaskAttachmentNum;
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
        var paramTextValue = new OdSqlParameter("paramTextValue", OdDbType.Text, SOut.StringNote(taskAttachment.TextValue));
        Db.NonQ(command, paramTextValue);
    }

    public static bool Update(TaskAttachment taskAttachment, TaskAttachment oldTaskAttachment)
    {
        var command = "";
        if (taskAttachment.TaskNum != oldTaskAttachment.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskAttachment.TaskNum) + "";
        }

        if (taskAttachment.DocNum != oldTaskAttachment.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(taskAttachment.DocNum) + "";
        }

        if (taskAttachment.TextValue != oldTaskAttachment.TextValue)
        {
            if (command != "") command += ",";
            command += "TextValue = " + DbHelper.ParamChar + "paramTextValue";
        }

        if (taskAttachment.Description != oldTaskAttachment.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(taskAttachment.Description) + "'";
        }

        if (command == "") return false;
        if (taskAttachment.TextValue == null) taskAttachment.TextValue = "";
        var paramTextValue = new OdSqlParameter("paramTextValue", OdDbType.Text, SOut.StringNote(taskAttachment.TextValue));
        command = "UPDATE taskattachment SET " + command
                                               + " WHERE TaskAttachmentNum = " + SOut.Long(taskAttachment.TaskAttachmentNum);
        Db.NonQ(command, paramTextValue);
        return true;
    }

    public static bool UpdateComparison(TaskAttachment taskAttachment, TaskAttachment oldTaskAttachment)
    {
        if (taskAttachment.TaskNum != oldTaskAttachment.TaskNum) return true;
        if (taskAttachment.DocNum != oldTaskAttachment.DocNum) return true;
        if (taskAttachment.TextValue != oldTaskAttachment.TextValue) return true;
        if (taskAttachment.Description != oldTaskAttachment.Description) return true;
        return false;
    }

    public static void Delete(long taskAttachmentNum)
    {
        var command = "DELETE FROM taskattachment "
                      + "WHERE TaskAttachmentNum = " + SOut.Long(taskAttachmentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskAttachmentNums)
    {
        if (listTaskAttachmentNums == null || listTaskAttachmentNums.Count == 0) return;
        var command = "DELETE FROM taskattachment "
                      + "WHERE TaskAttachmentNum IN(" + string.Join(",", listTaskAttachmentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}