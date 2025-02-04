using System;
using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public static class TaskSubscriptions
{
    public static List<TaskSubscription> GetTaskSubscriptionsForUser(long userNum)
    {
        return TaskSubscriptionCrud.SelectMany("SELECT * FROM tasksubscription WHERE UserNum = " + userNum);
    }

    public static void Insert(TaskSubscription taskSubscription)
    {
        TaskSubscriptionCrud.Insert(taskSubscription);
    }

    public static bool TrySubscList(long taskListNum, long userNum)
    {
        var taskSubscriptionNumsExisting = GetTaskSubscriptionsForUser(userNum).Select(x => x.TaskListNum).ToList();
        if (taskSubscriptionNumsExisting.Contains(taskListNum))
        {
            return false;
        }

        var taskNumsReminderOld = GetUnreadReminderTasks(userNum).Select(x => x.TaskNum).ToList();

        Insert(new TaskSubscription
        {
            IsNew = true,
            UserNum = userNum,
            TaskListNum = taskListNum
        });

        var tasksNewReminder = GetUnreadReminderTasks(userNum).FindAll(x => !taskNumsReminderOld.Contains(x.TaskNum));

        TaskUnreads.SetRead(userNum, tasksNewReminder.FindAll(x => x.DateTimeEntry < DateTime.Now).ToArray());

        var tasksFutureReminders = GetNewReadReminders(taskSubscriptionNumsExisting, taskListNum, userNum)
            .Where(x => x.DateTimeEntry >= DateTime.Now)
            .ToList();

        TaskUnreads.InsertManyForTasks(tasksFutureReminders, userNum);

        return true;
    }

    private static List<Task> GetNewReadReminders(List<long> taskSubscriptionNumsExisting, long taskListNum, long userNum)
    {
        var tasksReminders = new List<Task>();
        if (taskSubscriptionNumsExisting.Contains(taskListNum))
        {
            return tasksReminders;
        }

        var inboxUserNum = TaskLists.GetMailboxUserNum(taskListNum);

        var taskLists = TaskLists.RefreshChildren(taskListNum, userNum, inboxUserNum, TaskType.Reminder);
        foreach (var taskList in taskLists)
        {
            tasksReminders.AddRange(GetNewReadReminders(taskSubscriptionNumsExisting, taskList.TaskListNum, userNum));
        }

        tasksReminders.AddRange(Tasks
            .RefreshChildren(taskListNum, false, DateTime.MinValue, userNum, inboxUserNum, TaskType.Reminder, false)
            .Where(x => !x.IsUnread));

        return tasksReminders;
    }


    private static List<Task> GetUnreadReminderTasks(long userNum)
    {
        if (PrefC.GetBool(PrefName.TasksUseRepeating))
        {
            return [];
        }

        var tasksReminders = new List<Task>();
        var tasksRefreshed = Tasks.GetNewTasksThisUser(userNum, Clinics.ClinicNum);

        foreach (var task in tasksRefreshed)
        {
            if (!string.IsNullOrEmpty(task.ReminderGroupId) && task.ReminderType != TaskReminderType.NoReminder)
            {
                tasksReminders.Add(task);
            }
        }

        return tasksReminders;
    }

    public static void UnsubscList(long taskListNum, long userNum)
    {
        var tasksFutureUnreadReminders = Tasks
            .GetNewTasksThisUser(userNum, 0)
            .Where(x => Tasks.IsReminderTask(x) && x.DateTimeEntry >= DateTime.Now)
            .ToList();

        Db.NonQ("DELETE FROM tasksubscription WHERE UserNum = " + userNum + " AND TaskListNum = " + taskListNum);

        var tasksStillSubscribed = Tasks
            .GetNewTasksThisUser(userNum, 0)
            .Where(x => Tasks.IsReminderTask(x) && x.DateTimeEntry >= DateTime.Now)
            .ToList();

        var tasksUnsubForUser = tasksFutureUnreadReminders.Where(x => !tasksStillSubscribed.Select(y => y.TaskNum).Contains(x.TaskNum)).ToList();

        TaskUnreads.SetRead(userNum, tasksUnsubForUser.ToArray());
    }

    public static void RemoveAllSubscribers(long taskListNum)
    {
        Db.NonQ("DELETE FROM tasksubscription WHERE TaskListNum = " + taskListNum);
    }

    public static void UpdateTaskListSubs(long taskListNumOld, long taskListNumNew)
    {
        string commandText;
        if (taskListNumNew == 0)
        {
            commandText = "DELETE FROM tasksubscription WHERE TaskListNum = " + taskListNumOld;
        }
        else
        {
            commandText = "UPDATE tasksubscription SET TaskListNum = " + taskListNumNew + " WHERE TaskListNum = " + taskListNumOld;
        }

        Db.NonQ(commandText);
    }
}