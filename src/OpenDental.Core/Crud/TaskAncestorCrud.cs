using System.Collections.Generic;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskAncestorCrud
{
    public static void Insert(TaskAncestor taskAncestor)
    {
        var command = "INSERT INTO taskancestor (";

        command += "TaskNum,TaskListNum) VALUES(";

        command +=
            SOut.Long(taskAncestor.TaskNum) + ","
                                            + SOut.Long(taskAncestor.TaskListNum) + ")";
        {
            taskAncestor.TaskAncestorNum = Db.NonQ(command, true, "TaskAncestorNum", "taskAncestor");
        }
    }

    public static void InsertMany(List<TaskAncestor> listTaskAncestors, bool useExistingPK = false)
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
}