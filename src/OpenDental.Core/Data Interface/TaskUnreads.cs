using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TaskUnreads
{
    public static void Insert(TaskUnread taskUnread)
    {
        TaskUnreadCrud.Insert(taskUnread);
    }

    public static void InsertManyForTasks(List<Task> listTasks, long userNum)
    {
        if (listTasks.IsNullOrEmpty() || userNum == 0)
            //Do not insert any TaskUnreads if none given or invalid usernum.
            return;

        var listTaskUnreads = new List<TaskUnread>();
        for (var i = 0; i < listTasks.Count; i++)
        {
            var taskUnread = new TaskUnread();
            taskUnread.TaskNum = listTasks[i].TaskNum;
            taskUnread.UserNum = userNum;
            listTaskUnreads.Add(taskUnread);
            listTasks[i].IsUnread = true;
        }

        TaskUnreadCrud.InsertMany(listTaskUnreads);
    }

    public static void SetRead(long userNum, params Task[] taskArray)
    {
        if (taskArray == null || taskArray.Length == 0) return;
        for (var i = 0; i < taskArray.Length; i++) taskArray[i].IsUnread = false;

        var command = "DELETE FROM taskunread WHERE UserNum = " + SOut.Long(userNum) + " "
                      + "AND TaskNum IN (" + string.Join(",", taskArray.Select(x => SOut.Long(x.TaskNum))) + ")";
        Db.NonQ(command);
    }

    public static void AddUnreads(Task task, long userNumOrig)
    {
        //if the task is done, don't add unreads
        var command = "SELECT TaskStatus,UserNum,ReminderGroupId,DateTimeEntry," + "NOW()" + " DbTime "
                      + "FROM task WHERE TaskNum = " + SOut.Long(task.TaskNum);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return;
        var taskStatusEnum = (TaskStatusEnum) SIn.Int(table.Rows[0]["TaskStatus"].ToString());
        var userNumOwner = SIn.Long(table.Rows[0]["UserNum"].ToString());
        if (taskStatusEnum == TaskStatusEnum.Done) return;
        //Set it unread for the original owner of the task.
        if (userNumOwner != userNumOrig) //but only if it's some other user
            SetUnread(userNumOwner, task);
        //Set it for this user if a future repeating task, so it will be new when "due".  Doing this here so we don't check every row below.
        //Only for future dates because we don't want to mark as new if it was already "due" and you added a note or something.
        if (SIn.String(table.Rows[0]["ReminderGroupId"].ToString()) != "" //Is a reminder
            && SIn.DateTime(table.Rows[0]["DateTimeEntry"].ToString()) > SIn.DateTime(table.Rows[0]["DbTime"].ToString())) //Is "due" in the future by DbTime 
            SetUnread(userNumOrig, task); //Set unread for current user only, other users dealt with below.
        //Then, for anyone subscribed
        bool isUnread;
        //task subscriptions are not cached yet, so we use a query.
        //Get a list of all subscribers to this task
        command = @"SELECT 
				tasksubscription.UserNum,
				(CASE WHEN taskunread.UserNum IS NULL THEN 0 ELSE 1 END) IsUnread
				FROM tasksubscription
				INNER JOIN tasklist ON tasksubscription.TaskListNum = tasklist.TaskListNum 
				INNER JOIN taskancestor ON taskancestor.TaskListNum = tasklist.TaskListNum 
				AND taskancestor.TaskNum = " + SOut.Long(task.TaskNum) + " ";
        command += "LEFT JOIN taskunread ON taskunread.UserNum = tasksubscription.UserNum AND taskunread.TaskNum=taskancestor.TaskNum";
        table = DataCore.GetTable(command);
        var listUserNums = new List<long>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var userNum = SIn.Long(table.Rows[i]["UserNum"].ToString());
            isUnread = SIn.Bool(table.Rows[i]["IsUnread"].ToString());
            if (userNum == userNumOwner //already set
                || userNum == userNumOrig //If the current user is subscribed to this task. User has obviously already read it.
                || listUserNums.Contains(userNum)
                || isUnread) //Unread currently exists
                continue;
            listUserNums.Add(userNum);
        }

        SetUnreadMany(listUserNums, task); //This no longer results in duplicates like it used to
    }

    public static bool IsUnread(long userNum, Task task)
    {
        task.IsUnread = true;
        var command = "SELECT COUNT(*) FROM taskunread WHERE UserNum = " + SOut.Long(userNum) + " "
                      + "AND TaskNum = " + SOut.Long(task.TaskNum);
        if (Db.GetCount(command) == "0") task.IsUnread = false;
        return task.IsUnread;
    }

    public static DataTable GetForTask(long taskNum)
    {
        var command = @"SELECT
				DISTINCT userod.UserName AS 'User',
				(CASE WHEN !ISNULL(taskunread.TaskNum) THEN 'Unread' ELSE 'Read' END) AS 'Unread' " +
                      "FROM tasksubscription " +
                      "INNER JOIN tasklist ON tasksubscription.TaskListNum=tasklist.TaskListNum " +
                      $"INNER JOIN taskancestor ON taskancestor.TaskListNum=tasklist.TaskListNum AND taskancestor.TaskNum={SOut.Long(taskNum)} " +
                      "INNER JOIN userod ON userod.UserNum=tasksubscription.UserNum " +
                      $"LEFT JOIN taskunread ON taskunread.UserNum=tasksubscription.UserNum AND taskunread.TaskNum={SOut.Long(taskNum)} " +
                      "WHERE userod.IsHidden=FALSE " + //Don't include hidden users
                      "ORDER BY Unread,userod.UserName";
        return DataCore.GetTable(command);
    }

    public static void SetUnread(long userNum, Task task)
    {
        if (IsUnread(userNum, task)) return; //Already set to unread, so nothing else to do
        var taskUnread = new TaskUnread();
        taskUnread.TaskNum = task.TaskNum;
        taskUnread.UserNum = userNum;
        task.IsUnread = true;
        Insert(taskUnread);
    }

    public static void SetUnreadMany(List<long> listUserNums, Task task)
    {
        var listTaskUnreadsToInsert = new List<TaskUnread>();
        for (var i = 0; i < listUserNums.Count; i++)
        {
            var taskUnread = new TaskUnread();
            taskUnread.TaskNum = task.TaskNum;
            taskUnread.UserNum = listUserNums[i];
            listTaskUnreadsToInsert.Add(taskUnread);
        }

        TaskUnreadCrud.InsertMany(listTaskUnreadsToInsert);
        if (listUserNums.Contains(Security.CurUser.UserNum)) //The IsUnread flag is only used for local refreshes.
            task.IsUnread = true;
    }

    public static void DeleteForTask(Task task)
    {
        var command = "DELETE FROM taskunread WHERE TaskNum = " + SOut.Long(task.TaskNum);
        Db.NonQ(command);
    }
}