using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TaskNotes
{
    #region Misc Methods

    ///<summary>Returns true if there are any rows that have a Note with char length greater than 65,535</summary>
    public static bool HasAnyLongNotes()
    {
        var command = "SELECT COUNT(*) FROM tasknote WHERE CHAR_LENGTH(tasknote.Note)>65535";
        return Db.GetCount(command) != "0";
    }

    #endregion

    ///<summary>A list of notes for one task, ordered by datetime.</summary>
    public static List<TaskNote> GetForTask(long taskNum)
    {
        var command = "SELECT * FROM tasknote WHERE TaskNum = " + SOut.Long(taskNum) + " ORDER BY DateTimeNote";
        return TaskNoteCrud.SelectMany(command);
    }

    ///<summary>A list of notes for many tasks.</summary>
    public static List<TaskNote> GetForTasks(List<long> listTaskNums)
    {
        if (listTaskNums == null || listTaskNums.Count == 0) return new List<TaskNote>();
        var command = "SELECT * FROM tasknote WHERE TaskNum IN (" + string.Join(",", listTaskNums) + ")";
        return TaskNoteCrud.SelectMany(command);
    }

    ///<summary>A list of notes for multiple tasks, ordered by date time.</summary>
    public static List<TaskNote> RefreshForTasks(List<long> listTaskNums)
    {
        if (listTaskNums.Count == 0) return new List<TaskNote>();
        var command = "SELECT * FROM tasknote WHERE TaskNum IN (";
        for (var i = 0; i < listTaskNums.Count; i++)
        {
            if (i > 0) command += ",";
            command += SOut.Long(listTaskNums[i]);
        }

        command += ") ORDER BY DateTimeNote";
        return TaskNoteCrud.SelectMany(command);
    }

    
    public static long Insert(TaskNote taskNote)
    {
        return TaskNoteCrud.Insert(taskNote);
    }

    
    public static void Update(TaskNote taskNote)
    {
        TaskNoteCrud.Update(taskNote);
    }

    
    public static void Delete(long taskNoteNum)
    {
        var command = "DELETE FROM tasknote WHERE TaskNoteNum = " + SOut.Long(taskNoteNum);
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one TaskNote from the db.</summary>
    public static TaskNote GetOne(long taskNoteNum){

        return Crud.TaskNoteCrud.SelectOne(taskNoteNum);
    }




    */
}