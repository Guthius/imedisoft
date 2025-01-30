using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskNoteCrud
{
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

    public static void Insert(TaskNote taskNote)
    {
        var command = "INSERT INTO tasknote (";

        command += "TaskNum,UserNum,DateTimeNote,Note) VALUES(";

        command +=
            SOut.Long(taskNote.TaskNum) + ","
                                        + SOut.Long(taskNote.UserNum) + ","
                                        + "NOW()" + ","
                                        + DbHelper.ParamChar + "paramNote)";
        if (taskNote.Note == null) taskNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(taskNote.Note));
        {
            taskNote.TaskNoteNum = Db.NonQ(command, true, "TaskNoteNum", "taskNote", paramNote);
        }
    }

    public static void Update(TaskNote taskNote)
    {
        var command = "UPDATE tasknote SET "
                      + "TaskNum     =  " + SOut.Long(taskNote.TaskNum) + ", "
                      + "UserNum     =  " + SOut.Long(taskNote.UserNum) + ", "
                      + "DateTimeNote=  " + SOut.DateTime(taskNote.DateTimeNote) + ", "
                      + "Note        =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE TaskNoteNum = " + SOut.Long(taskNote.TaskNoteNum);
        if (taskNote.Note == null) taskNote.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(taskNote.Note));
        Db.NonQ(command, paramNote);
    }
}