#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskNoteCrud
{
    public static TaskNote SelectOne(long taskNoteNum)
    {
        var command = "SELECT * FROM tasknote "
                      + "WHERE TaskNoteNum = " + SOut.Long(taskNoteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskNote> TableToList(DataTable table)
    {
        var retVal = new List<TaskNote>();
        TaskNote taskNote;
        foreach (DataRow row in table.Rows)
        {
            taskNote = new TaskNote();
            taskNote.TaskNoteNum = SIn.Long(row["TaskNoteNum"].ToString());
            taskNote.TaskNum = SIn.Long(row["TaskNum"].ToString());
            taskNote.UserNum = SIn.Long(row["UserNum"].ToString());
            taskNote.DateTimeNote = SIn.DateTime(row["DateTimeNote"].ToString());
            taskNote.Note = SIn.String(row["Note"].ToString());
            retVal.Add(taskNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskNote> listTaskNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskNote";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskNoteNum");
        table.Columns.Add("TaskNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("DateTimeNote");
        table.Columns.Add("Note");
        foreach (var taskNote in listTaskNotes)
            table.Rows.Add(SOut.Long(taskNote.TaskNoteNum), SOut.Long(taskNote.TaskNum), SOut.Long(taskNote.UserNum), SOut.DateT(taskNote.DateTimeNote, false), taskNote.Note);
        return table;
    }

    public static long Insert(TaskNote taskNote)
    {
        return Insert(taskNote, false);
    }

    public static long Insert(TaskNote taskNote, bool useExistingPK)
    {
        var command = "INSERT INTO tasknote (";

        command += "TaskNum,UserNum,DateTimeNote,Note) VALUES(";

        command +=
            SOut.Long(taskNote.TaskNum) + ","
                                        + SOut.Long(taskNote.UserNum) + ","
                                        + DbHelper.Now() + ","
                                        + DbHelper.ParamChar + "paramNote)";
        if (taskNote.Note == null) taskNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(taskNote.Note));
        {
            taskNote.TaskNoteNum = Db.NonQ(command, true, "TaskNoteNum", "taskNote", paramNote);
        }
        return taskNote.TaskNoteNum;
    }

    public static long InsertNoCache(TaskNote taskNote)
    {
        return InsertNoCache(taskNote, false);
    }

    public static long InsertNoCache(TaskNote taskNote, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO tasknote (";
        if (isRandomKeys || useExistingPK) command += "TaskNoteNum,";
        command += "TaskNum,UserNum,DateTimeNote,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskNote.TaskNoteNum) + ",";
        command +=
            SOut.Long(taskNote.TaskNum) + ","
                                        + SOut.Long(taskNote.UserNum) + ","
                                        + DbHelper.Now() + ","
                                        + DbHelper.ParamChar + "paramNote)";
        if (taskNote.Note == null) taskNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(taskNote.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            taskNote.TaskNoteNum = Db.NonQ(command, true, "TaskNoteNum", "taskNote", paramNote);
        return taskNote.TaskNoteNum;
    }

    public static void Update(TaskNote taskNote)
    {
        var command = "UPDATE tasknote SET "
                      + "TaskNum     =  " + SOut.Long(taskNote.TaskNum) + ", "
                      + "UserNum     =  " + SOut.Long(taskNote.UserNum) + ", "
                      + "DateTimeNote=  " + SOut.DateT(taskNote.DateTimeNote) + ", "
                      + "Note        =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE TaskNoteNum = " + SOut.Long(taskNote.TaskNoteNum);
        if (taskNote.Note == null) taskNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(taskNote.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(TaskNote taskNote, TaskNote oldTaskNote)
    {
        var command = "";
        if (taskNote.TaskNum != oldTaskNote.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskNote.TaskNum) + "";
        }

        if (taskNote.UserNum != oldTaskNote.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(taskNote.UserNum) + "";
        }

        if (taskNote.DateTimeNote != oldTaskNote.DateTimeNote)
        {
            if (command != "") command += ",";
            command += "DateTimeNote = " + SOut.DateT(taskNote.DateTimeNote) + "";
        }

        if (taskNote.Note != oldTaskNote.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (taskNote.Note == null) taskNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(taskNote.Note));
        command = "UPDATE tasknote SET " + command
                                         + " WHERE TaskNoteNum = " + SOut.Long(taskNote.TaskNoteNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(TaskNote taskNote, TaskNote oldTaskNote)
    {
        if (taskNote.TaskNum != oldTaskNote.TaskNum) return true;
        if (taskNote.UserNum != oldTaskNote.UserNum) return true;
        if (taskNote.DateTimeNote != oldTaskNote.DateTimeNote) return true;
        if (taskNote.Note != oldTaskNote.Note) return true;
        return false;
    }

    public static void Delete(long taskNoteNum)
    {
        var command = "DELETE FROM tasknote "
                      + "WHERE TaskNoteNum = " + SOut.Long(taskNoteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskNoteNums)
    {
        if (listTaskNoteNums == null || listTaskNoteNums.Count == 0) return;
        var command = "DELETE FROM tasknote "
                      + "WHERE TaskNoteNum IN(" + string.Join(",", listTaskNoteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}