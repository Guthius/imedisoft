using System;
using System.Collections.Generic;
using System.Text;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TaskHists
{
    ///<summary>Gets one TaskHist from the db.</summary>
    public static TaskHist GetOne(long taskHistNum)
    {
        return TaskHistCrud.SelectOne(taskHistNum);
    }

    public static string GetChangesDescription(TaskHist taskHist, TaskHist taskHistNext)
    {
        if (taskHist.Descript.StartsWith("This task was cut from task list ") || taskHist.Descript.StartsWith("This task was copied from task ")) return taskHist.Descript;
        if (taskHist.DateTimeEntry == DateTime.MinValue) return Lans.g("TaskHists", "New task.");
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("");
        if (taskHistNext.TaskListNum != taskHist.TaskListNum)
        {
            var descOne = Lans.g("TaskHists", "(DELETED)");
            var descTwo = Lans.g("TaskHists", "(DELETED)");
            var taskList = TaskLists.GetOne(taskHist.TaskListNum);
            if (taskList != null) descOne = taskList.Descript;
            taskList = TaskLists.GetOne(taskHistNext.TaskListNum);
            if (taskList != null) descTwo = taskList.Descript;
            stringBuilder.Append(Lans.g("TaskHists", "Task list changed from") + " " + descOne + " " + Lans.g("TaskHists", "to") + " " + descTwo + ".\r\n");
        }

        if (taskHistNext.ObjectType != taskHist.ObjectType)
            stringBuilder.Append(Lans.g("TaskHists", "Task attachment changed from") + " "
                                                                                     + taskHist.ObjectType + " " + Lans.g("TaskHists", "to") + " " + taskHistNext.ObjectType + ".\r\n");
        if (taskHistNext.KeyNum != taskHist.KeyNum) stringBuilder.Append(Lans.g("TaskHists", "Task account attachment changed.") + "\r\n");
        if (taskHistNext.Descript != taskHist.Descript && !taskHistNext.Descript.StartsWith("This task was cut from task list ")
                                                       && !taskHistNext.Descript.StartsWith("This task was copied from task "))
            //We change the description of a task when it is cut/copied. 
            //This prevents the history grid from showing a description changed when it wasn't changed by the user.
            stringBuilder.Append(Lans.g("TaskHists", "Task description changed.") + "\r\n");
        if (taskHistNext.TaskStatus != taskHist.TaskStatus)
            stringBuilder.Append(Lans.g("TaskHists", "Task status changed from") + " "
                                                                                 + taskHist.TaskStatus + " " + Lans.g("TaskHists", "to") + " " + taskHistNext.TaskStatus + ".\r\n");
        if (taskHistNext.DateTimeEntry != taskHist.DateTimeEntry)
            stringBuilder.Append(Lans.g("TaskHists", "Task date added changed from") + " "
                                                                                     + taskHist.DateTimeEntry
                                                                                     + " " + Lans.g("TaskHists", "to") + " "
                                                                                     + taskHistNext.DateTimeEntry + ".\r\n");
        if (taskHistNext.UserNum != taskHist.UserNum)
            stringBuilder.Append(Lans.g("TaskHists", "Task author changed from ") + GetUserName(taskHist.UserNum) + " "
                                 + Lans.g("TaskHists", "to") + " " + GetUserName(taskHistNext.UserNum) + ".\r\n");
        if (taskHistNext.DateTimeFinished != taskHist.DateTimeFinished)
            stringBuilder.Append(Lans.g("TaskHists", "Task date finished changed from") + " "
                                                                                        + taskHist.DateTimeFinished
                                                                                        + " " + Lans.g("TaskHists", "to") + " "
                                                                                        + taskHistNext.DateTimeFinished + ".\r\n");
        if (taskHistNext.PriorityDefNum != taskHist.PriorityDefNum)
            stringBuilder.Append(Lans.g("TaskHists", "Task priority changed from") + " "
                                                                                   + Defs.GetDef(DefCat.TaskPriorities, taskHist.PriorityDefNum).ItemName
                                                                                   + " " + Lans.g("TaskHists", "to") + " "
                                                                                   + Defs.GetDef(DefCat.TaskPriorities, taskHistNext.PriorityDefNum).ItemName + ".\r\n");
        if (taskHist.IsNoteChange) //Using taskOld because the notes changed from the old one to the new one.
            stringBuilder.Append(Lans.g("TaskHists", "Task notes changed."));
        if (taskHist.IsReadOnly != taskHistNext.IsReadOnly)
            stringBuilder.Append(Lans.g("TaskHists", "Task IsReadOnly changed from") + " "
                                                                                     + taskHist.IsReadOnly
                                                                                     + " " + Lans.g("TaskHists", "to") + " "
                                                                                     + taskHistNext.IsReadOnly + ".\r\n");
        return stringBuilder.ToString();
    }

    ///<summary>Retrieves a user's name based on the passed userNum, otherwise return INVALID.</summary>
    public static string GetUserName(long userNum)
    {
        var userod = Userods.GetUser(userNum);
        if (userod == null) return $"INVALID ({userNum})";
        return userod.UserName;
    }

    
    public static long Insert(TaskHist taskHist)
    {
        return TaskHistCrud.Insert(taskHist);
    }

    /// <summary>
    ///     Updates TaskHist references when an old task is cut (not copied) and pasted somewhere so history is
    ///     continuous.
    /// </summary>
    public static void UpdateTaskNums(Task taskOld, Task taskNew)
    {
        var command = "UPDATE taskhist SET TaskNum=" + SOut.Long(taskNew.TaskNum) + " WHERE TaskNum=" + SOut.Long(taskOld.TaskNum);
        Db.NonQ(command);
    }

    ///<summary>Gets a list of task histories for a given taskNum.</summary>
    public static List<TaskHist> GetArchivesForTask(long taskNum)
    {
        var command = "SELECT * FROM taskhist WHERE TaskNum=" + SOut.Long(taskNum) + " ORDER BY DateTStamp";
        return TaskHistCrud.SelectMany(command);
    }

    ///<summary>Deletes all TaskHists before the given cutoff date. Returns the number of entries deleted.</summary>
    public static long DeleteBeforeDate(DateTime dateCutoff)
    {
        var command = $"DELETE FROM taskhist WHERE DateTStamp <= {SOut.DateT(dateCutoff)} ";
        return Db.NonQ(command);
    }

    
    public static void Delete(long taskHistNum)
    {
        TaskHistCrud.Delete(taskHistNum);
    }
}