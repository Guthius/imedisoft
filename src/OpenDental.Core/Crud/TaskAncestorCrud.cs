#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TaskAncestorCrud
{
    public static TaskAncestor SelectOne(long taskAncestorNum)
    {
        var command = "SELECT * FROM taskancestor "
                      + "WHERE TaskAncestorNum = " + SOut.Long(taskAncestorNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TaskAncestor SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskAncestor> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskAncestor> TableToList(DataTable table)
    {
        var retVal = new List<TaskAncestor>();
        TaskAncestor taskAncestor;
        foreach (DataRow row in table.Rows)
        {
            taskAncestor = new TaskAncestor();
            taskAncestor.TaskAncestorNum = SIn.Long(row["TaskAncestorNum"].ToString());
            taskAncestor.TaskNum = SIn.Long(row["TaskNum"].ToString());
            taskAncestor.TaskListNum = SIn.Long(row["TaskListNum"].ToString());
            retVal.Add(taskAncestor);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TaskAncestor> listTaskAncestors, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TaskAncestor";
        var table = new DataTable(tableName);
        table.Columns.Add("TaskAncestorNum");
        table.Columns.Add("TaskNum");
        table.Columns.Add("TaskListNum");
        foreach (var taskAncestor in listTaskAncestors)
            table.Rows.Add(SOut.Long(taskAncestor.TaskAncestorNum), SOut.Long(taskAncestor.TaskNum), SOut.Long(taskAncestor.TaskListNum));
        return table;
    }

    public static long Insert(TaskAncestor taskAncestor)
    {
        return Insert(taskAncestor, false);
    }

    public static long Insert(TaskAncestor taskAncestor, bool useExistingPK)
    {
        var command = "INSERT INTO taskancestor (";

        command += "TaskNum,TaskListNum) VALUES(";

        command +=
            SOut.Long(taskAncestor.TaskNum) + ","
                                            + SOut.Long(taskAncestor.TaskListNum) + ")";
        {
            taskAncestor.TaskAncestorNum = Db.NonQ(command, true, "TaskAncestorNum", "taskAncestor");
        }
        return taskAncestor.TaskAncestorNum;
    }

    public static void InsertMany(List<TaskAncestor> listTaskAncestors)
    {
        InsertMany(listTaskAncestors, false);
    }

    public static void InsertMany(List<TaskAncestor> listTaskAncestors, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listTaskAncestors.Count)
        {
            var taskAncestor = listTaskAncestors[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO taskancestor (");
                if (useExistingPK) sbCommands.Append("TaskAncestorNum,");
                sbCommands.Append("TaskNum,TaskListNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(taskAncestor.TaskAncestorNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(taskAncestor.TaskNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(taskAncestor.TaskListNum));
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
                if (index == listTaskAncestors.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(TaskAncestor taskAncestor)
    {
        return InsertNoCache(taskAncestor, false);
    }

    public static long InsertNoCache(TaskAncestor taskAncestor, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO taskancestor (";
        if (isRandomKeys || useExistingPK) command += "TaskAncestorNum,";
        command += "TaskNum,TaskListNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(taskAncestor.TaskAncestorNum) + ",";
        command +=
            SOut.Long(taskAncestor.TaskNum) + ","
                                            + SOut.Long(taskAncestor.TaskListNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            taskAncestor.TaskAncestorNum = Db.NonQ(command, true, "TaskAncestorNum", "taskAncestor");
        return taskAncestor.TaskAncestorNum;
    }

    public static void Update(TaskAncestor taskAncestor)
    {
        var command = "UPDATE taskancestor SET "
                      + "TaskNum        =  " + SOut.Long(taskAncestor.TaskNum) + ", "
                      + "TaskListNum    =  " + SOut.Long(taskAncestor.TaskListNum) + " "
                      + "WHERE TaskAncestorNum = " + SOut.Long(taskAncestor.TaskAncestorNum);
        Db.NonQ(command);
    }

    public static bool Update(TaskAncestor taskAncestor, TaskAncestor oldTaskAncestor)
    {
        var command = "";
        if (taskAncestor.TaskNum != oldTaskAncestor.TaskNum)
        {
            if (command != "") command += ",";
            command += "TaskNum = " + SOut.Long(taskAncestor.TaskNum) + "";
        }

        if (taskAncestor.TaskListNum != oldTaskAncestor.TaskListNum)
        {
            if (command != "") command += ",";
            command += "TaskListNum = " + SOut.Long(taskAncestor.TaskListNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE taskancestor SET " + command
                                             + " WHERE TaskAncestorNum = " + SOut.Long(taskAncestor.TaskAncestorNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TaskAncestor taskAncestor, TaskAncestor oldTaskAncestor)
    {
        if (taskAncestor.TaskNum != oldTaskAncestor.TaskNum) return true;
        if (taskAncestor.TaskListNum != oldTaskAncestor.TaskListNum) return true;
        return false;
    }

    public static void Delete(long taskAncestorNum)
    {
        var command = "DELETE FROM taskancestor "
                      + "WHERE TaskAncestorNum = " + SOut.Long(taskAncestorNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTaskAncestorNums)
    {
        if (listTaskAncestorNums == null || listTaskAncestorNums.Count == 0) return;
        var command = "DELETE FROM taskancestor "
                      + "WHERE TaskAncestorNum IN(" + string.Join(",", listTaskAncestorNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}