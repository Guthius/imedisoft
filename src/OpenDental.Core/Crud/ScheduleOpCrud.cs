#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ScheduleOpCrud
{
    public static ScheduleOp SelectOne(long scheduleOpNum)
    {
        var command = "SELECT * FROM scheduleop "
                      + "WHERE ScheduleOpNum = " + SOut.Long(scheduleOpNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ScheduleOp SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ScheduleOp> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ScheduleOp> TableToList(DataTable table)
    {
        var retVal = new List<ScheduleOp>();
        ScheduleOp scheduleOp;
        foreach (DataRow row in table.Rows)
        {
            scheduleOp = new ScheduleOp();
            scheduleOp.ScheduleOpNum = SIn.Long(row["ScheduleOpNum"].ToString());
            scheduleOp.ScheduleNum = SIn.Long(row["ScheduleNum"].ToString());
            scheduleOp.OperatoryNum = SIn.Long(row["OperatoryNum"].ToString());
            retVal.Add(scheduleOp);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ScheduleOp> listScheduleOps, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ScheduleOp";
        var table = new DataTable(tableName);
        table.Columns.Add("ScheduleOpNum");
        table.Columns.Add("ScheduleNum");
        table.Columns.Add("OperatoryNum");
        foreach (var scheduleOp in listScheduleOps)
            table.Rows.Add(SOut.Long(scheduleOp.ScheduleOpNum), SOut.Long(scheduleOp.ScheduleNum), SOut.Long(scheduleOp.OperatoryNum));
        return table;
    }

    public static long Insert(ScheduleOp scheduleOp)
    {
        return Insert(scheduleOp, false);
    }

    public static long Insert(ScheduleOp scheduleOp, bool useExistingPK)
    {
        var command = "INSERT INTO scheduleop (";

        command += "ScheduleNum,OperatoryNum) VALUES(";

        command +=
            SOut.Long(scheduleOp.ScheduleNum) + ","
                                              + SOut.Long(scheduleOp.OperatoryNum) + ")";
        {
            scheduleOp.ScheduleOpNum = Db.NonQ(command, true, "ScheduleOpNum", "scheduleOp");
        }
        return scheduleOp.ScheduleOpNum;
    }

    public static void InsertMany(List<ScheduleOp> listScheduleOps)
    {
        InsertMany(listScheduleOps, false);
    }

    public static void InsertMany(List<ScheduleOp> listScheduleOps, bool useExistingPK)
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

    public static long InsertNoCache(ScheduleOp scheduleOp)
    {
        return InsertNoCache(scheduleOp, false);
    }

    public static long InsertNoCache(ScheduleOp scheduleOp, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO scheduleop (";
        if (isRandomKeys || useExistingPK) command += "ScheduleOpNum,";
        command += "ScheduleNum,OperatoryNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(scheduleOp.ScheduleOpNum) + ",";
        command +=
            SOut.Long(scheduleOp.ScheduleNum) + ","
                                              + SOut.Long(scheduleOp.OperatoryNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            scheduleOp.ScheduleOpNum = Db.NonQ(command, true, "ScheduleOpNum", "scheduleOp");
        return scheduleOp.ScheduleOpNum;
    }

    public static void Update(ScheduleOp scheduleOp)
    {
        var command = "UPDATE scheduleop SET "
                      + "ScheduleNum  =  " + SOut.Long(scheduleOp.ScheduleNum) + ", "
                      + "OperatoryNum =  " + SOut.Long(scheduleOp.OperatoryNum) + " "
                      + "WHERE ScheduleOpNum = " + SOut.Long(scheduleOp.ScheduleOpNum);
        Db.NonQ(command);
    }

    public static bool Update(ScheduleOp scheduleOp, ScheduleOp oldScheduleOp)
    {
        var command = "";
        if (scheduleOp.ScheduleNum != oldScheduleOp.ScheduleNum)
        {
            if (command != "") command += ",";
            command += "ScheduleNum = " + SOut.Long(scheduleOp.ScheduleNum) + "";
        }

        if (scheduleOp.OperatoryNum != oldScheduleOp.OperatoryNum)
        {
            if (command != "") command += ",";
            command += "OperatoryNum = " + SOut.Long(scheduleOp.OperatoryNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE scheduleop SET " + command
                                           + " WHERE ScheduleOpNum = " + SOut.Long(scheduleOp.ScheduleOpNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ScheduleOp scheduleOp, ScheduleOp oldScheduleOp)
    {
        if (scheduleOp.ScheduleNum != oldScheduleOp.ScheduleNum) return true;
        if (scheduleOp.OperatoryNum != oldScheduleOp.OperatoryNum) return true;
        return false;
    }

    public static void Delete(long scheduleOpNum)
    {
        var command = "DELETE FROM scheduleop "
                      + "WHERE ScheduleOpNum = " + SOut.Long(scheduleOpNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listScheduleOpNums)
    {
        if (listScheduleOpNums == null || listScheduleOpNums.Count == 0) return;
        var command = "DELETE FROM scheduleop "
                      + "WHERE ScheduleOpNum IN(" + string.Join(",", listScheduleOpNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}