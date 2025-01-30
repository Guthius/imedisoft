using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TaskNotes
{
    public static List<TaskNote> GetForTask(long taskNum)
    {
        return TaskNoteCrud.SelectMany("SELECT * FROM tasknote WHERE TaskNum = " + taskNum + " ORDER BY DateTimeNote");
    }

    public static List<TaskNote> GetForTasks(List<long> taskNums)
    {
        if (taskNums == null || taskNums.Count == 0)
        {
            return [];
        }

        return TaskNoteCrud.SelectMany("SELECT * FROM tasknote WHERE TaskNum IN (" + string.Join(",", taskNums) + ")");
    }

    public static List<TaskNote> RefreshForTasks(List<long> taskNums)
    {
        return taskNums.Count == 0 ? [] : TaskNoteCrud.SelectMany("SELECT * FROM tasknote WHERE TaskNum IN (" + string.Join(", ", taskNums) + ") ORDER BY DateTimeNote");
    }

    public static void Insert(TaskNote taskNote)
    {
        TaskNoteCrud.Insert(taskNote);
    }

    public static void Update(TaskNote taskNote)
    {
        TaskNoteCrud.Update(taskNote);
    }

    public static void Delete(long taskNoteNum)
    {
        Db.NonQ("DELETE FROM tasknote WHERE TaskNoteNum = " + taskNoteNum);
    }
}