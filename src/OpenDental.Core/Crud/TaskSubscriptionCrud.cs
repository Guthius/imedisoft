#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskSubscriptionCrud
{
    public static TaskSubscription SelectOne(long taskSubscriptionNum)
    {
        var command = "SELECT * FROM tasksubscription "
                      + "WHERE TaskSubscriptionNum = " + SOut.Long(taskSubscriptionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskSubscription SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskSubscription> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskSubscription> TableToList(DataTable table)
    {
        var retVal = new List<TaskSubscription>();
        TaskSubscription taskSubscription;
        foreach (DataRow row in table.Rows)
        {
            taskSubscription = new TaskSubscription();
            taskSubscription.TaskSubscriptionNum = SIn.Long(row["TaskSubscriptionNum"].ToString());
            taskSubscription.UserNum = SIn.Long(row["UserNum"].ToString());
            taskSubscription.TaskListNum = SIn.Long(row["TaskListNum"].ToString());
            taskSubscription.TaskNum = SIn.Long(row["TaskNum"].ToString());
            retVal.Add(taskSubscription);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskSubscription> listTaskSubscriptions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskSubscription";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskSubscriptionNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("TaskListNum");
        table.Columns.Add("TaskNum");
        foreach (var taskSubscription in listTaskSubscriptions)
            table.Rows.Add(SOut.Long(taskSubscription.TaskSubscriptionNum), SOut.Long(taskSubscription.UserNum), SOut.Long(taskSubscription.TaskListNum), SOut.Long(taskSubscription.TaskNum));
        return table;
    }

    public static long Insert(TaskSubscription taskSubscription)
    {
        return Insert(taskSubscription, false);
    }

    public static long Insert(TaskSubscription taskSubscription, bool useExistingPK)
    {
        var command = "INSERT INTO tasksubscription (";

        command += "UserNum,TaskListNum,TaskNum) VALUES(";

        command +=
            SOut.Long(taskSubscription.UserNum) + ","
                                                + SOut.Long(taskSubscription.TaskListNum) + ","
                                                + SOut.Long(taskSubscription.TaskNum) + ")";
        {
            taskSubscription.TaskSubscriptionNum = Db.NonQ(command, true, "TaskSubscriptionNum", "taskSubscription");
        }
        return taskSubscription.TaskSubscriptionNum;
    }

    public static long InsertNoCache(TaskSubscription taskSubscription)
    {
        return InsertNoCache(taskSubscription, false);
    }

    public static long InsertNoCache(TaskSubscription taskSubscription, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO tasksubscription (";
        if (isRandomKeys || useExistingPK) command += "TaskSubscriptionNum,";
        command += "UserNum,TaskListNum,TaskNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskSubscription.TaskSubscriptionNum) + ",";
        command +=
            SOut.Long(taskSubscription.UserNum) + ","
                                                + SOut.Long(taskSubscription.TaskListNum) + ","
                                                + SOut.Long(taskSubscription.TaskNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            taskSubscription.TaskSubscriptionNum = Db.NonQ(command, true, "TaskSubscriptionNum", "taskSubscription");
        return taskSubscription.TaskSubscriptionNum;
    }

    public static void Update(TaskSubscription taskSubscription)
    {
        var command = "UPDATE tasksubscription SET "
                      + "UserNum            =  " + SOut.Long(taskSubscription.UserNum) + ", "
                      + "TaskListNum        =  " + SOut.Long(taskSubscription.TaskListNum) + ", "
                      + "TaskNum            =  " + SOut.Long(taskSubscription.TaskNum) + " "
                      + "WHERE TaskSubscriptionNum = " + SOut.Long(taskSubscription.TaskSubscriptionNum);
        Db.NonQ(command);
    }

    public static bool Update(TaskSubscription taskSubscription, TaskSubscription oldTaskSubscription)
    {
        var command = "";
        if (taskSubscription.UserNum != oldTaskSubscription.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(taskSubscription.UserNum) + "";
        }

        if (taskSubscription.TaskListNum != oldTaskSubscription.TaskListNum)
        {
            if (command != "") command += ",";
            command += "TaskListNum = " + SOut.Long(taskSubscription.TaskListNum) + "";
        }

        if (taskSubscription.TaskNum != oldTaskSubscription.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskSubscription.TaskNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE tasksubscription SET " + command
                                                 + " WHERE TaskSubscriptionNum = " + SOut.Long(taskSubscription.TaskSubscriptionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TaskSubscription taskSubscription, TaskSubscription oldTaskSubscription)
    {
        if (taskSubscription.UserNum != oldTaskSubscription.UserNum) return true;
        if (taskSubscription.TaskListNum != oldTaskSubscription.TaskListNum) return true;
        if (taskSubscription.TaskNum != oldTaskSubscription.TaskNum) return true;
        return false;
    }

    public static void Delete(long taskSubscriptionNum)
    {
        var command = "DELETE FROM tasksubscription "
                      + "WHERE TaskSubscriptionNum = " + SOut.Long(taskSubscriptionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskSubscriptionNums)
    {
        if (listTaskSubscriptionNums == null || listTaskSubscriptionNums.Count == 0) return;
        var command = "DELETE FROM tasksubscription "
                      + "WHERE TaskSubscriptionNum IN(" + string.Join(",", listTaskSubscriptionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}