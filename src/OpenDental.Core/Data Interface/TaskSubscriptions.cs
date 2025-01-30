using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class TaskSubscriptions
{
    public static List<TaskSubscription> GetTaskSubscriptionsForUser(long userNum)
    {
        var command = "SELECT * FROM tasksubscription WHERE UserNum=" + SOut.Long(userNum);
        return TaskSubscriptionCrud.SelectMany(command);
    }

    public static void Insert(TaskSubscription taskSubscription)
    {
        TaskSubscriptionCrud.Insert(taskSubscription);
    }

    public static bool TrySubscList(long taskListNum, long userNum)
    {
        //Get the list of directly subscribed TaskListNums.  This avoids the concurrency issue of the same user logged in via multiple WS and 
        //subscribing to the same TaskList.  Additionally, this allows the user to directly subscribe to child TaskLists of subscribed parent Tasklists
        //which was old behavior that was inadvertently removed.
        var listTaskSubscriptionNumsExisting = GetTaskSubscriptionsForUser(userNum).Select(x => x.TaskListNum).ToList();
        if (listTaskSubscriptionNumsExisting.Contains(taskListNum)) return false; //Already subscribed.
        //Get all currently subscribed unread Reminder tasks before adding new subscription.
        var listTaskNumsReminderOld = GetUnreadReminderTasks(userNum).Select(x => x.TaskNum).ToList();
        var taskSubscription = new TaskSubscription();
        taskSubscription.IsNew = true;
        taskSubscription.UserNum = userNum;
        taskSubscription.TaskListNum = taskListNum;
        Insert(taskSubscription);
        //Get newly subscribed unread Reminder tasks.
        var listTasksNewReminder = GetUnreadReminderTasks(userNum).FindAll(x => !listTaskNumsReminderOld.Contains(x.TaskNum));
        //Set any past unread Reminder tasks as read.
        TaskUnreads.SetRead(userNum, listTasksNewReminder.FindAll(x => x.DateTimeEntry < DateTime.Now).ToArray());
        //Get all future reminders in the newly subscribed Tasklist (and sub Tasklists) that the user was not previously subscribed to.
        var listTasksFutureReminders = GetNewReadReminders(listTaskSubscriptionNumsExisting, taskListNum, userNum)
            .Where(x => x.DateTimeEntry >= DateTime.Now).ToList();
        //We already know these tasks do not have any TaskUnreads (due to GetNewReadReminders->Tasks.RefreshChildren()), safe to insert TaskUnreads.
        TaskUnreads.InsertManyForTasks(listTasksFutureReminders, userNum);
        return true;
    }

    private static List<Task> GetNewReadReminders(List<long> listTaskSubscriptionNumsExisting, long taskListNum, long userNum)
    {
        var listTasksReminders = new List<Task>();
        if (listTaskSubscriptionNumsExisting.Contains(taskListNum))
            //We are only looking for Reminders that we were not already subscribed to.
            return listTasksReminders;
        var userNumInbox = TaskLists.GetMailboxUserNum(taskListNum); //Can be 0, not a user inbox.
        var listTaskLists = TaskLists.RefreshChildren(taskListNum, userNum, userNumInbox, TaskType.Reminder);
        for (var i = 0; i < listTaskLists.Count; i++) listTasksReminders.AddRange(GetNewReadReminders(listTaskSubscriptionNumsExisting, listTaskLists[i].TaskListNum, userNum));
        listTasksReminders.AddRange(Tasks.RefreshChildren(taskListNum, false, DateTime.MinValue, userNum, userNumInbox, TaskType.Reminder, false
        ).Where(x => !x.IsUnread)); //IsUnread field set accurately by Tasks.RefreshChildren(...)
        return listTasksReminders;
    }

    private static List<Task> GetUnreadReminderTasks(long userNum)
    {
        var listTasksReminders = new List<Task>();
        if (!PrefC.GetBool(PrefName.TasksUseRepeating))
        {
            //Using Reminders (Reminders not allowed if using repeating tasks)
            var listTasksRefreshed = Tasks.GetNewTasksThisUser(userNum, Clinics.ClinicNum); //Get all tasks pertaining to current user.
            for (var i = 0; i < listTasksRefreshed.Count; i++)
                if (!string.IsNullOrEmpty(listTasksRefreshed[i].ReminderGroupId) && listTasksRefreshed[i].ReminderType != TaskReminderType.NoReminder)
                    //Task is a Reminder.
                    listTasksReminders.Add(listTasksRefreshed[i]);
        }

        return listTasksReminders;
    }

    public static void UnsubscList(long taskListNum, long userNum)
    {
        //Get all future unread reminders
        var listTasksFutureUnreadReminders = Tasks.GetNewTasksThisUser(userNum, 0) //Use clinicnum=0 to get all tasks, no task clinic filtering.
            .Where(x => Tasks.IsReminderTask(x) && x.DateTimeEntry >= DateTime.Now)
            .ToList();
        var command = "DELETE FROM tasksubscription "
                      + "WHERE UserNum=" + SOut.Long(userNum)
                      + " AND TaskListNum=" + SOut.Long(taskListNum);
        Db.NonQ(command);
        var listTasksStillSubscribed = Tasks.GetNewTasksThisUser(userNum, 0) //Use clinicnum=0 to get all tasks, no task clinic filtering.
            .Where(x => Tasks.IsReminderTask(x) && x.DateTimeEntry >= DateTime.Now).ToList();
        var listTasksUnsubForUser = listTasksFutureUnreadReminders.Where(x => !listTasksStillSubscribed.Select(y => y.TaskNum).Contains(x.TaskNum)).ToList();
        //Set unsubbed reminders in the future to be read so reminders wont show in NewForUser tasklist.
        TaskUnreads.SetRead(userNum, listTasksUnsubForUser.ToArray());
    }

    public static void RemoveAllSubscribers(long taskListNum)
    {
        var command = "DELETE FROM tasksubscription "
                      + "WHERE TaskListNum=" + SOut.Long(taskListNum);
        Db.NonQ(command);
    }

    public static void UpdateTaskListSubs(long taskListNumOld, long taskListNumNew)
    {
        var command = "";
        if (taskListNumNew == 0)
            command = "DELETE FROM tasksubscription WHERE TaskListNum=" + SOut.Long(taskListNumOld);
        else
            command = "UPDATE tasksubscription SET TaskListNum=" + SOut.Long(taskListNumNew) + " WHERE TaskListNum=" + SOut.Long(taskListNumOld);
        Db.NonQ(command);
    }
}