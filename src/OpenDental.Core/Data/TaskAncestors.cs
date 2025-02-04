using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TaskAncestors
{
    public static void Insert(TaskAncestor taskAncestor)
    {
        TaskAncestorCrud.Insert(taskAncestor);
    }

    public static void Synch(Task task)
    {
        Db.NonQ("DELETE FROM taskancestor WHERE TaskNum = " + task.TaskNum);

        var parentNum = task.TaskListNum;

        while (true)
        {
            if (parentNum == 0)
            {
                break;
            }

            var dataTable = DataCore.GetTable("SELECT TaskListNum, Parent FROM tasklist WHERE TaskListNum = " + parentNum);
            if (dataTable.Rows.Count == 0)
            {
                break;
            }

            var taskListNum = SIn.Long(dataTable.Rows[0]["TaskListNum"].ToString());

            parentNum = SIn.Long(dataTable.Rows[0]["Parent"].ToString());

            Insert(new TaskAncestor
            {
                TaskNum = task.TaskNum,
                TaskListNum = taskListNum
            });
        }
    }

    public static void SynchManyForSameTasklist(List<Task> tasks, long taskListNum, long taskListParent)
    {
        if (tasks == null || tasks.Count < 1 || taskListNum == 0)
        {
            return;
        }

        Db.NonQ("DELETE FROM taskancestor WHERE TaskNum IN (" + string.Join(",", tasks.Select(x => x.TaskNum)) + ")");

        while (true)
        {
            var taskAncestors = new List<TaskAncestor>();
            foreach (var task in tasks)
            {
                taskAncestors.Add(new TaskAncestor
                {
                    TaskNum = task.TaskNum,
                    TaskListNum = taskListNum
                });
            }

            TaskAncestorCrud.InsertMany(taskAncestors);

            if (taskListParent == 0)
            {
                break;
            }

            var dataTable = DataCore.GetTable("SELECT TaskListNum, Parent FROM tasklist WHERE TaskListNum = " + taskListParent);
            if (dataTable.Rows.Count == 0)
            {
                break;
            }

            taskListNum = SIn.Long(dataTable.Rows[0]["TaskListNum"].ToString());
            taskListParent = SIn.Long(dataTable.Rows[0]["Parent"].ToString());
        }
    }

    public static void SynchAll()
    {
        var tasks = Tasks.RefreshAll();

        foreach (var task in tasks)
        {
            Synch(task);
        }
    }
}