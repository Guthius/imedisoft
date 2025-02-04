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

    public static void InsertManyForTasks(List<Task> tasks, long userNum)
    {
        if (tasks.IsNullOrEmpty() || userNum == 0)
        {
            return;
        }

        var taskUnreads = new List<TaskUnread>();

        foreach (var task in tasks)
        {
            taskUnreads.Add(new TaskUnread
            {
                TaskNum = task.TaskNum,
                UserNum = userNum
            });

            task.IsUnread = true;
        }

        TaskUnreadCrud.InsertMany(taskUnreads);
    }

    public static void SetRead(long userNum, params Task[] tasks)
    {
        if (tasks == null || tasks.Length == 0)
        {
            return;
        }
        
        foreach (var task in tasks)
        {
            task.IsUnread = false;
        }

        var command = "DELETE FROM taskunread WHERE UserNum = " + userNum + " AND TaskNum IN (" + string.Join(",", tasks.Select(x => x.TaskNum)) + ")";
        
        Db.NonQ(command);
    }

    public static void AddUnreads(Task task, long originalUserNum)
    {
        var command = "SELECT TaskStatus,UserNum,ReminderGroupId,DateTimeEntry," + "NOW()" + " DbTime FROM task WHERE TaskNum = " + task.TaskNum;

        var dataTable = DataCore.GetTable(command);
        if (dataTable.Rows.Count == 0)
        {
            return;
        }

        var taskStatusEnum = (TaskStatusEnum) SIn.Int(dataTable.Rows[0]["TaskStatus"].ToString());
        var ownerUserNum = SIn.Long(dataTable.Rows[0]["UserNum"].ToString());
        if (taskStatusEnum == TaskStatusEnum.Done)
        {
            return;
        }

        if (ownerUserNum != originalUserNum)
        {
            SetUnread(ownerUserNum, task);
        }

        if (SIn.String(dataTable.Rows[0]["ReminderGroupId"].ToString()) != "" &&
            SIn.DateTime(dataTable.Rows[0]["DateTimeEntry"].ToString()) > SIn.DateTime(dataTable.Rows[0]["DbTime"].ToString()))
        {
            SetUnread(originalUserNum, task);
        }

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
				AND taskancestor.TaskNum = " + task.TaskNum + " ";
        command += "LEFT JOIN taskunread ON taskunread.UserNum = tasksubscription.UserNum AND taskunread.TaskNum=taskancestor.TaskNum";
        dataTable = DataCore.GetTable(command);
        var listUserNums = new List<long>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var userNum = SIn.Long(dataTable.Rows[i]["UserNum"].ToString());
            isUnread = SIn.Bool(dataTable.Rows[i]["IsUnread"].ToString());

            if (userNum == ownerUserNum || userNum == originalUserNum || listUserNums.Contains(userNum) || isUnread)
            {
                continue;
            }

            listUserNums.Add(userNum);
        }

        SetUnreadMany(listUserNums, task);
    }

    public static bool IsUnread(long userNum, Task task)
    {
        task.IsUnread = true;

        var commandText = "SELECT COUNT(*) FROM taskunread WHERE UserNum = " + userNum + " AND TaskNum = " + task.TaskNum;
        if (Db.GetCount(commandText) == "0")
        {
            task.IsUnread = false;
        }

        return task.IsUnread;
    }

    public static DataTable GetForTask(long taskNum)
    {
        var command = @"SELECT
				DISTINCT userod.UserName AS 'User',
				(CASE WHEN !ISNULL(taskunread.TaskNum) THEN 'Unread' ELSE 'Read' END) AS 'Unread' " +
                      "FROM tasksubscription " +
                      "INNER JOIN tasklist ON tasksubscription.TaskListNum=tasklist.TaskListNum " +
                      $"INNER JOIN taskancestor ON taskancestor.TaskListNum=tasklist.TaskListNum AND taskancestor.TaskNum={taskNum} " +
                      "INNER JOIN userod ON userod.UserNum=tasksubscription.UserNum " +
                      $"LEFT JOIN taskunread ON taskunread.UserNum=tasksubscription.UserNum AND taskunread.TaskNum={taskNum} " +
                      "WHERE userod.IsHidden=FALSE " +
                      "ORDER BY Unread,userod.UserName";
        return DataCore.GetTable(command);
    }

    public static void SetUnread(long userNum, Task task)
    {
        if (IsUnread(userNum, task))
        {
            return;
        }

        task.IsUnread = true;

        Insert(new TaskUnread
        {
            TaskNum = task.TaskNum,
            UserNum = userNum
        });
    }

    public static void SetUnreadMany(List<long> userNums, Task task)
    {
        var taskUnreadsToInsert = new List<TaskUnread>();

        foreach (var userNum in userNums)
        {
            taskUnreadsToInsert.Add(new TaskUnread
            {
                TaskNum = task.TaskNum,
                UserNum = userNum
            });
        }

        TaskUnreadCrud.InsertMany(taskUnreadsToInsert);

        if (userNums.Contains(Security.CurUser.UserNum))
        {
            task.IsUnread = true;
        }
    }

    public static void DeleteForTask(Task task)
    {
        Db.NonQ("DELETE FROM taskunread WHERE TaskNum = " + task.TaskNum);
    }
}