#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskUnreadCrud
{
    public static TaskUnread SelectOne(long taskUnreadNum)
    {
        var command = "SELECT * FROM taskunread "
                      + "WHERE TaskUnreadNum = " + SOut.Long(taskUnreadNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskUnread SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskUnread> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskUnread> TableToList(DataTable table)
    {
        var retVal = new List<TaskUnread>();
        TaskUnread taskUnread;
        foreach (DataRow row in table.Rows)
        {
            taskUnread = new TaskUnread();
            taskUnread.TaskUnreadNum = SIn.Long(row["TaskUnreadNum"].ToString());
            taskUnread.TaskNum = SIn.Long(row["TaskNum"].ToString());
            taskUnread.UserNum = SIn.Long(row["UserNum"].ToString());
            retVal.Add(taskUnread);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskUnread> listTaskUnreads, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskUnread";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskUnreadNum");
        table.Columns.Add("TaskNum");
        table.Columns.Add("UserNum");
        foreach (var taskUnread in listTaskUnreads)
            table.Rows.Add(SOut.Long(taskUnread.TaskUnreadNum), SOut.Long(taskUnread.TaskNum), SOut.Long(taskUnread.UserNum));
        return table;
    }

    public static long Insert(TaskUnread taskUnread)
    {
        return Insert(taskUnread, false);
    }

    public static long Insert(TaskUnread taskUnread, bool useExistingPK)
    {
        var command = "INSERT INTO taskunread (";

        command += "TaskNum,UserNum) VALUES(";

        command +=
            SOut.Long(taskUnread.TaskNum) + ","
                                          + SOut.Long(taskUnread.UserNum) + ")";
        {
            taskUnread.TaskUnreadNum = Db.NonQ(command, true, "TaskUnreadNum", "taskUnread");
        }
        return taskUnread.TaskUnreadNum;
    }

    public static void InsertMany(List<TaskUnread> listTaskUnreads)
    {
        InsertMany(listTaskUnreads, false);
    }

    public static void InsertMany(List<TaskUnread> listTaskUnreads, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listTaskUnreads.Count)
        {
            var taskUnread = listTaskUnreads[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO taskunread (");
                if (useExistingPK) sbCommands.Append("TaskUnreadNum,");
                sbCommands.Append("TaskNum,UserNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(taskUnread.TaskUnreadNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(taskUnread.TaskNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(taskUnread.UserNum));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listTaskUnreads.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(TaskUnread taskUnread)
    {
        return InsertNoCache(taskUnread, false);
    }

    public static long InsertNoCache(TaskUnread taskUnread, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO taskunread (";
        if (isRandomKeys || useExistingPK) command += "TaskUnreadNum,";
        command += "TaskNum,UserNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskUnread.TaskUnreadNum) + ",";
        command +=
            SOut.Long(taskUnread.TaskNum) + ","
                                          + SOut.Long(taskUnread.UserNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            taskUnread.TaskUnreadNum = Db.NonQ(command, true, "TaskUnreadNum", "taskUnread");
        return taskUnread.TaskUnreadNum;
    }

    public static void Update(TaskUnread taskUnread)
    {
        var command = "UPDATE taskunread SET "
                      + "TaskNum      =  " + SOut.Long(taskUnread.TaskNum) + ", "
                      + "UserNum      =  " + SOut.Long(taskUnread.UserNum) + " "
                      + "WHERE TaskUnreadNum = " + SOut.Long(taskUnread.TaskUnreadNum);
        Db.NonQ(command);
    }

    public static bool Update(TaskUnread taskUnread, TaskUnread oldTaskUnread)
    {
        var command = "";
        if (taskUnread.TaskNum != oldTaskUnread.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskUnread.TaskNum) + "";
        }

        if (taskUnread.UserNum != oldTaskUnread.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(taskUnread.UserNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE taskunread SET " + command
                                           + " WHERE TaskUnreadNum = " + SOut.Long(taskUnread.TaskUnreadNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TaskUnread taskUnread, TaskUnread oldTaskUnread)
    {
        if (taskUnread.TaskNum != oldTaskUnread.TaskNum) return true;
        if (taskUnread.UserNum != oldTaskUnread.UserNum) return true;
        return false;
    }

    public static void Delete(long taskUnreadNum)
    {
        var command = "DELETE FROM taskunread "
                      + "WHERE TaskUnreadNum = " + SOut.Long(taskUnreadNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskUnreadNums)
    {
        if (listTaskUnreadNums == null || listTaskUnreadNums.Count == 0) return;
        var command = "DELETE FROM taskunread "
                      + "WHERE TaskUnreadNum IN(" + string.Join(",", listTaskUnreadNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}