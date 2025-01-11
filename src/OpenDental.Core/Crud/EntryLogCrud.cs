#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EntryLogCrud
{
    public static EntryLog SelectOne(long entryLogNum)
    {
        var command = "SELECT * FROM entrylog "
                      + "WHERE EntryLogNum = " + SOut.Long(entryLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EntryLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EntryLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EntryLog> TableToList(DataTable table)
    {
        var retVal = new List<EntryLog>();
        EntryLog entryLog;
        foreach (DataRow row in table.Rows)
        {
            entryLog = new EntryLog();
            entryLog.EntryLogNum = SIn.Long(row["EntryLogNum"].ToString());
            entryLog.UserNum = SIn.Long(row["UserNum"].ToString());
            entryLog.FKeyType = (EntryLogFKeyType) SIn.Int(row["FKeyType"].ToString());
            entryLog.FKey = SIn.Long(row["FKey"].ToString());
            entryLog.LogSource = (LogSources) SIn.Int(row["LogSource"].ToString());
            entryLog.EntryDateTime = SIn.DateTime(row["EntryDateTime"].ToString());
            retVal.Add(entryLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EntryLog> listEntryLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EntryLog";
        var table = new DataTable(tableName);
        table.Columns.Add("EntryLogNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("FKeyType");
        table.Columns.Add("FKey");
        table.Columns.Add("LogSource");
        table.Columns.Add("EntryDateTime");
        foreach (var entryLog in listEntryLogs)
            table.Rows.Add(SOut.Long(entryLog.EntryLogNum), SOut.Long(entryLog.UserNum), SOut.Int((int) entryLog.FKeyType), SOut.Long(entryLog.FKey), SOut.Int((int) entryLog.LogSource), SOut.DateT(entryLog.EntryDateTime, false));
        return table;
    }

    public static long Insert(EntryLog entryLog)
    {
        return Insert(entryLog, false);
    }

    public static long Insert(EntryLog entryLog, bool useExistingPK)
    {
        var command = "INSERT INTO entrylog (";

        command += "UserNum,FKeyType,FKey,LogSource,EntryDateTime) VALUES(";

        command +=
            SOut.Long(entryLog.UserNum) + ","
                                        + SOut.Int((int) entryLog.FKeyType) + ","
                                        + SOut.Long(entryLog.FKey) + ","
                                        + SOut.Int((int) entryLog.LogSource) + ","
                                        + DbHelper.Now() + ")";
        {
            entryLog.EntryLogNum = Db.NonQ(command, true, "EntryLogNum", "entryLog");
        }
        return entryLog.EntryLogNum;
    }

    public static void InsertMany(List<EntryLog> listEntryLogs)
    {
        InsertMany(listEntryLogs, false);
    }

    public static void InsertMany(List<EntryLog> listEntryLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEntryLogs.Count)
        {
            var entryLog = listEntryLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO entrylog (");
                if (useExistingPK) sbCommands.Append("EntryLogNum,");
                sbCommands.Append("UserNum,FKeyType,FKey,LogSource,EntryDateTime) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(entryLog.EntryLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(entryLog.UserNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) entryLog.FKeyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(entryLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) entryLog.LogSource));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
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
                if (index == listEntryLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(EntryLog entryLog)
    {
        return InsertNoCache(entryLog, false);
    }

    public static long InsertNoCache(EntryLog entryLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO entrylog (";
        if (isRandomKeys || useExistingPK) command += "EntryLogNum,";
        command += "UserNum,FKeyType,FKey,LogSource,EntryDateTime) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(entryLog.EntryLogNum) + ",";
        command +=
            SOut.Long(entryLog.UserNum) + ","
                                        + SOut.Int((int) entryLog.FKeyType) + ","
                                        + SOut.Long(entryLog.FKey) + ","
                                        + SOut.Int((int) entryLog.LogSource) + ","
                                        + DbHelper.Now() + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            entryLog.EntryLogNum = Db.NonQ(command, true, "EntryLogNum", "entryLog");
        return entryLog.EntryLogNum;
    }

    public static void Update(EntryLog entryLog)
    {
        var command = "UPDATE entrylog SET "
                      + "UserNum      =  " + SOut.Long(entryLog.UserNum) + ", "
                      + "FKeyType     =  " + SOut.Int((int) entryLog.FKeyType) + ", "
                      + "FKey         =  " + SOut.Long(entryLog.FKey) + ", "
                      + "LogSource    =  " + SOut.Int((int) entryLog.LogSource) + " "
                      //EntryDateTime not allowed to change
                      + "WHERE EntryLogNum = " + SOut.Long(entryLog.EntryLogNum);
        Db.NonQ(command);
    }

    public static bool Update(EntryLog entryLog, EntryLog oldEntryLog)
    {
        var command = "";
        if (entryLog.UserNum != oldEntryLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(entryLog.UserNum) + "";
        }

        if (entryLog.FKeyType != oldEntryLog.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = " + SOut.Int((int) entryLog.FKeyType) + "";
        }

        if (entryLog.FKey != oldEntryLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(entryLog.FKey) + "";
        }

        if (entryLog.LogSource != oldEntryLog.LogSource)
        {
            if (command != "") command += ",";
            command += "LogSource = " + SOut.Int((int) entryLog.LogSource) + "";
        }

        //EntryDateTime not allowed to change
        if (command == "") return false;
        command = "UPDATE entrylog SET " + command
                                         + " WHERE EntryLogNum = " + SOut.Long(entryLog.EntryLogNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EntryLog entryLog, EntryLog oldEntryLog)
    {
        if (entryLog.UserNum != oldEntryLog.UserNum) return true;
        if (entryLog.FKeyType != oldEntryLog.FKeyType) return true;
        if (entryLog.FKey != oldEntryLog.FKey) return true;
        if (entryLog.LogSource != oldEntryLog.LogSource) return true;
        //EntryDateTime not allowed to change
        return false;
    }

    public static void Delete(long entryLogNum)
    {
        var command = "DELETE FROM entrylog "
                      + "WHERE EntryLogNum = " + SOut.Long(entryLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEntryLogNums)
    {
        if (listEntryLogNums == null || listEntryLogNums.Count == 0) return;
        var command = "DELETE FROM entrylog "
                      + "WHERE EntryLogNum IN(" + string.Join(",", listEntryLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}