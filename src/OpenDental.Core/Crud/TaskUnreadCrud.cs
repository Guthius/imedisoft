using System.Collections.Generic;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskUnreadCrud
{
    public static void Insert(TaskUnread taskUnread)
    {
        var command = "INSERT INTO taskunread (";

        command += "TaskNum,UserNum) VALUES(";

        command +=
            SOut.Long(taskUnread.TaskNum) + ","
                                          + SOut.Long(taskUnread.UserNum) + ")";
        {
            taskUnread.TaskUnreadNum = Db.NonQ(command, true, "TaskUnreadNum", "taskUnread");
        }
    }

    public static void InsertMany(List<TaskUnread> listTaskUnreads, bool useExistingPK = false)
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
}