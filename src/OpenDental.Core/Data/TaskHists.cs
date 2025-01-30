using System;
using System.Collections.Generic;
using System.Text;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TaskHists
{
    public static string GetChangesDescription(TaskHist taskHist, TaskHist taskHistNext)
    {
        if (taskHist.Descript.StartsWith("This task was cut from task list ") ||
            taskHist.Descript.StartsWith("This task was copied from task "))
        {
            return taskHist.Descript;
        }

        if (taskHist.DateTimeEntry == DateTime.MinValue)
        {
            return "New task.";
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append("");

        if (taskHistNext.TaskListNum != taskHist.TaskListNum)
        {
            var descOne = "(DELETED)";
            var descTwo = "(DELETED)";

            var taskList = TaskLists.GetOne(taskHist.TaskListNum);
            if (taskList != null)
            {
                descOne = taskList.Descript;
            }

            taskList = TaskLists.GetOne(taskHistNext.TaskListNum);
            if (taskList != null)
            {
                descTwo = taskList.Descript;
            }

            stringBuilder.Append("Task list changed from " + descOne + " to " + descTwo + ".\r\n");
        }

        if (taskHistNext.ObjectType != taskHist.ObjectType)
        {
            stringBuilder.Append("Task attachment changed from " + taskHist.ObjectType + " to " + taskHistNext.ObjectType + ".\r\n");
        }

        if (taskHistNext.KeyNum != taskHist.KeyNum)
        {
            stringBuilder.Append("Task account attachment changed.\r\n");
        }

        if (taskHistNext.Descript != taskHist.Descript && !taskHistNext.Descript.StartsWith("This task was cut from task list ") && !taskHistNext.Descript.StartsWith("This task was copied from task "))
        {
            stringBuilder.Append("Task description changed.\r\n");
        }

        if (taskHistNext.TaskStatus != taskHist.TaskStatus)
        {
            stringBuilder.Append("Task status changed from " + taskHist.TaskStatus + " to " + taskHistNext.TaskStatus + ".\r\n");
        }

        if (taskHistNext.DateTimeEntry != taskHist.DateTimeEntry)
        {
            stringBuilder.Append("Task date added changed from " + taskHist.DateTimeEntry + " to " + taskHistNext.DateTimeEntry + ".\r\n");
        }

        if (taskHistNext.UserNum != taskHist.UserNum)
        {
            stringBuilder.Append("Task author changed from " + GetUserName(taskHist.UserNum) + " to " + GetUserName(taskHistNext.UserNum) + ".\r\n");
        }

        if (taskHistNext.DateTimeFinished != taskHist.DateTimeFinished)
        {
            stringBuilder.Append("Task date finished changed from " + taskHist.DateTimeFinished + " to " + taskHistNext.DateTimeFinished + ".\r\n");
        }

        if (taskHistNext.PriorityDefNum != taskHist.PriorityDefNum)
        {
            stringBuilder.Append(
                "Task priority changed from " +
                Defs.GetDef(DefCat.TaskPriorities, taskHist.PriorityDefNum).ItemName + " to " +
                Defs.GetDef(DefCat.TaskPriorities, taskHistNext.PriorityDefNum).ItemName + ".\r\n");
        }

        if (taskHist.IsNoteChange)
        {
            stringBuilder.Append("Task notes changed.");
        }

        if (taskHist.IsReadOnly != taskHistNext.IsReadOnly)
        {
            stringBuilder.Append("Task IsReadOnly changed from " + taskHist.IsReadOnly + " to " + taskHistNext.IsReadOnly + ".\r\n");
        }

        return stringBuilder.ToString();
    }

    public static string GetUserName(long userNum)
    {
        var userod = Userods.GetUser(userNum);

        return userod == null ? $"INVALID ({userNum})" : userod.UserName;
    }

    public static void Insert(TaskHist taskHist)
    {
        TaskHistCrud.Insert(taskHist);
    }

    public static List<TaskHist> GetArchivesForTask(long taskNum)
    {
        return TaskHistCrud.SelectMany("SELECT * FROM taskhist WHERE TaskNum=" + taskNum + " ORDER BY DateTStamp");
    }
}