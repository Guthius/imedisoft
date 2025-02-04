using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ScheduleOpCrud
{
    public static List<ScheduleOp> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ScheduleOp> TableToList(DataTable table)
    {
        var retVal = new List<ScheduleOp>();
        foreach (DataRow row in table.Rows)
        {
            var scheduleOp = new ScheduleOp
            {
                ScheduleOpNum = SIn.Long(row["ScheduleOpNum"].ToString()),
                ScheduleNum = SIn.Long(row["ScheduleNum"].ToString()),
                OperatoryNum = SIn.Long(row["OperatoryNum"].ToString())
            };
            retVal.Add(scheduleOp);
        }

        return retVal;
    }

    public static void Insert(ScheduleOp scheduleOp)
    {
        var command = "INSERT INTO scheduleop (";

        command += "ScheduleNum,OperatoryNum) VALUES(";

        command +=
            SOut.Long(scheduleOp.ScheduleNum) + ","
                                              + SOut.Long(scheduleOp.OperatoryNum) + ")";
        {
            scheduleOp.ScheduleOpNum = Db.NonQ(command, true, "ScheduleOpNum", "scheduleOp");
        }
    }

    public static void InsertMany(List<ScheduleOp> listScheduleOps, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listScheduleOps.Count)
        {
            var scheduleOp = listScheduleOps[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO scheduleop (");
                if (useExistingPK) sbCommands.Append("ScheduleOpNum,");
                sbCommands.Append("ScheduleNum,OperatoryNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(scheduleOp.ScheduleOpNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(scheduleOp.ScheduleNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(scheduleOp.OperatoryNum));
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
                if (index == listScheduleOps.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }
}