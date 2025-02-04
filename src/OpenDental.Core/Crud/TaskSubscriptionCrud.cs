using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskSubscriptionCrud
{
    public static List<TaskSubscription> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskSubscription> TableToList(DataTable table)
    {
        var retVal = new List<TaskSubscription>();
        foreach (DataRow row in table.Rows)
        {
            var taskSubscription = new TaskSubscription
            {
                TaskSubscriptionNum = SIn.Long(row["TaskSubscriptionNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                TaskListNum = SIn.Long(row["TaskListNum"].ToString()),
                TaskNum = SIn.Long(row["TaskNum"].ToString())
            };
            retVal.Add(taskSubscription);
        }

        return retVal;
    }

    public static void Insert(TaskSubscription taskSubscription)
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
    }
}