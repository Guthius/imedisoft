using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TaskAncestors
{
    
    public static long Insert(TaskAncestor taskAncestor)
    {
        return TaskAncestorCrud.Insert(taskAncestor);
    }

    /*
    
    public static void Update(TaskAncestor ancestor) {

        Crud.TaskAncestorCrud.Update(ancestor);
    }*/

    public static void Synch(Task task)
    {
        var command = "DELETE FROM taskancestor WHERE TaskNum=" + SOut.Long(task.TaskNum);
        Db.NonQ(command);
        long taskListNum = 0;
        var parentNum = task.TaskListNum;
        DataTable table;
        TaskAncestor taskAncestor;
        while (true)
        {
            if (parentNum == 0) break; //no parent to mark
            //get the parent
            command = "SELECT TaskListNum,Parent FROM tasklist WHERE TaskListNum=" + SOut.Long(parentNum);
            table = DataCore.GetTable(command);
            if (table.Rows.Count == 0) //in case of database inconsistency
                break;
            taskListNum = SIn.Long(table.Rows[0]["TaskListNum"].ToString());
            parentNum = SIn.Long(table.Rows[0]["Parent"].ToString());
            taskAncestor = new TaskAncestor();
            taskAncestor.TaskNum = task.TaskNum;
            taskAncestor.TaskListNum = taskListNum;
            Insert(taskAncestor);
        }
    }

    /// <summary>
    ///     This should only be used when synching ancestors for multiple tasks in the same tasklist.
    ///     Limits DELETE, SELECT and INSERT calls to DB.
    /// </summary>
    public static void SynchManyForSameTasklist(List<Task> listTasks, long taskListNum, long taskListParent)
    {
        //Return if the task list passed in is invalid or trying to manipulate ancestors associated to the trunk (main).
        if (listTasks == null || listTasks.Count < 1 || taskListNum == 0) return;

        var command = "DELETE FROM taskancestor WHERE TaskNum IN (" + string.Join(",", listTasks.Select(x => SOut.Long(x.TaskNum))) + ")";
        Db.NonQ(command);
        DataTable table;
        while (true)
        {
            var listTaskAncestors = new List<TaskAncestor>();
            for (var i = 0; i < listTasks.Count; i++)
            {
                var ancestor = new TaskAncestor();
                ancestor.TaskNum = listTasks[i].TaskNum;
                ancestor.TaskListNum = taskListNum;
                listTaskAncestors.Add(ancestor);
            }

            TaskAncestorCrud.InsertMany(listTaskAncestors);
            if (taskListParent == 0) break; //No more work needed
            //get the parent
            command = "SELECT TaskListNum,Parent FROM tasklist WHERE TaskListNum=" + SOut.Long(taskListParent);
            table = DataCore.GetTable(command);
            if (table.Rows.Count == 0) //in case of database inconsistency
                break;
            taskListNum = SIn.Long(table.Rows[0]["TaskListNum"].ToString());
            taskListParent = SIn.Long(table.Rows[0]["Parent"].ToString());
        }
    }

    ///<summary>Only run once after the upgrade to version 5.5.</summary>
    public static void SynchAll()
    {
        var listTasks = Tasks.RefreshAll();
        for (var i = 0; i < listTasks.Count; i++) Synch(listTasks[i]);
    }
}