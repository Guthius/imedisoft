using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TaskSubscriptions
{
    #region Get Methods

    /// <summary>
    ///     Returns a list of TaskSubscriptions for the TaskLists userNum is directly subscribed to. Does not include any
    ///     children/grandchildren
    ///     of the TaskLists in TaskSubscription.
    /// </summary>
    public static List<TaskSubscription> GetTaskSubscriptionsForUser(long userNum)
    {
        var command = "SELECT * FROM tasksubscription WHERE UserNum=" + SOut.Long(userNum);
        return TaskSubscriptionCrud.SelectMany(command);
    }

    #endregion

    
    public static long Insert(TaskSubscription taskSubscription)
    {
        return TaskSubscriptionCrud.Insert(taskSubscription);
    }

    /*
    
    public static void Update(TaskSubscription subsc) {

        Crud.TaskSubscriptionCrud.Update(subsc);
    }*/

    /// <summary>
    ///     Attempts to create a subscription to a TaskList with TaskListNum of subscribeToTaskListNum.
    ///     The curUserNum must be the currently logged in user.
    /// </summary>
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

    ///<summary>Gets all Read Reminders in a TaskList/Task hierarchy that the user was not already subscribed to.</summary>
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

    ///<summary>Gets all unread Reminder Tasks for curUserNum.  Mimics logic in FormOpenDental.SignalsTick.</summary>
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

    /// <summary>Returns a list of userNums for users that are subscribed to the task list a passed in task is currently in./// </summary>
    public static List<long> GetSubscribersForTask(Task task)
    {
        var command = @"
				SELECT tasksubscription.UserNum
				FROM tasksubscription
				INNER JOIN tasklist ON tasksubscription.TaskListNum=tasklist.TaskListNum 
				INNER JOIN taskancestor ON taskancestor.TaskListNum=tasklist.TaskListNum AND taskancestor.TaskNum='" + SOut.Long(task.TaskNum) + "'";
        return Db.GetListLong(command);
    }

    ///<summary>Removes a subscription to a list.</summary>
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

    ///<summary>Removes all the subscribers from a given tasklist</summary>
    public static void RemoveAllSubscribers(long taskListNum)
    {
        var command = "DELETE FROM tasksubscription "
                      + "WHERE TaskListNum=" + SOut.Long(taskListNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Moves all subscriptions from taskListOld to taskListNew. Used when cutting and pasting a tasklist. Can also be
    ///     used when deleting a tasklist to remove all subscriptions from the tasklist by sending in 0 as taskListNumNew.
    /// </summary>
    public static void UpdateTaskListSubs(long taskListNumOld, long taskListNumNew)
    {
        var command = "";
        if (taskListNumNew == 0)
            command = "DELETE FROM tasksubscription WHERE TaskListNum=" + SOut.Long(taskListNumOld);
        else
            command = "UPDATE tasksubscription SET TaskListNum=" + SOut.Long(taskListNumNew) + " WHERE TaskListNum=" + SOut.Long(taskListNumOld);
        Db.NonQ(command);
    }

    ///<summary>Deletes rows for given PK tasksubscription.TaskSubscriptionNums.</summary>
    public static void DeleteMany(List<long> listTaskSubscriptionNums)
    {
        if (listTaskSubscriptionNums.Count == 0) return;
        var command = "DELETE FROM tasksubscription WHERE TaskSubscriptionNum IN (" + string.Join(",", listTaskSubscriptionNums) + ")";
        Db.NonQ(command);
    }
}