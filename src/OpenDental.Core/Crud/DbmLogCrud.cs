#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DbmLogCrud
{
    public static DbmLog SelectOne(long dbmLogNum)
    {
        var command = "SELECT * FROM dbmlog "
                      + "WHERE DbmLogNum = " + SOut.Long(dbmLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DbmLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DbmLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DbmLog> TableToList(DataTable table)
    {
        var retVal = new List<DbmLog>();
        DbmLog dbmLog;
        foreach (DataRow row in table.Rows)
        {
            dbmLog = new DbmLog();
            dbmLog.DbmLogNum = SIn.Long(row["DbmLogNum"].ToString());
            dbmLog.UserNum = SIn.Long(row["UserNum"].ToString());
            dbmLog.FKey = SIn.Long(row["FKey"].ToString());
            dbmLog.FKeyType = (DbmLogFKeyType) SIn.Int(row["FKeyType"].ToString());
            dbmLog.ActionType = (DbmLogActionType) SIn.Int(row["ActionType"].ToString());
            dbmLog.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            dbmLog.MethodName = SIn.String(row["MethodName"].ToString());
            dbmLog.LogText = SIn.String(row["LogText"].ToString());
            retVal.Add(dbmLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DbmLog> listDbmLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DbmLog";
        var table = new DataTable(tableName);
        table.Columns.Add("DbmLogNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("FKey");
        table.Columns.Add("FKeyType");
        table.Columns.Add("ActionType");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("MethodName");
        table.Columns.Add("LogText");
        foreach (var dbmLog in listDbmLogs)
            table.Rows.Add(SOut.Long(dbmLog.DbmLogNum), SOut.Long(dbmLog.UserNum), SOut.Long(dbmLog.FKey), SOut.Int((int) dbmLog.FKeyType), SOut.Int((int) dbmLog.ActionType), SOut.DateT(dbmLog.DateTimeEntry, false), dbmLog.MethodName, dbmLog.LogText);
        return table;
    }

    public static long Insert(DbmLog dbmLog)
    {
        return Insert(dbmLog, false);
    }

    public static long Insert(DbmLog dbmLog, bool useExistingPK)
    {
        var command = "INSERT INTO dbmlog (";

        command += "UserNum,FKey,FKeyType,ActionType,DateTimeEntry,MethodName,LogText) VALUES(";

        command +=
            SOut.Long(dbmLog.UserNum) + ","
                                      + SOut.Long(dbmLog.FKey) + ","
                                      + SOut.Int((int) dbmLog.FKeyType) + ","
                                      + SOut.Int((int) dbmLog.ActionType) + ","
                                      + DbHelper.Now() + ","
                                      + "'" + SOut.String(dbmLog.MethodName) + "',"
                                      + DbHelper.ParamChar + "paramLogText)";
        if (dbmLog.LogText == null) dbmLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(dbmLog.LogText));
        {
            dbmLog.DbmLogNum = Db.NonQ(command, true, "DbmLogNum", "dbmLog", paramLogText);
        }
        return dbmLog.DbmLogNum;
    }

    public static void InsertMany(List<DbmLog> listDbmLogs)
    {
        InsertMany(listDbmLogs, false);
    }

    public static void InsertMany(List<DbmLog> listDbmLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listDbmLogs.Count)
        {
            var dbmLog = listDbmLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO dbmlog (");
                if (useExistingPK) sbCommands.Append("DbmLogNum,");
                sbCommands.Append("UserNum,FKey,FKeyType,ActionType,DateTimeEntry,MethodName,LogText) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(dbmLog.DbmLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(dbmLog.UserNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(dbmLog.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) dbmLog.FKeyType));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) dbmLog.ActionType));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(dbmLog.MethodName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(dbmLog.LogText) + "'");
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
                if (index == listDbmLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(DbmLog dbmLog)
    {
        return InsertNoCache(dbmLog, false);
    }

    public static long InsertNoCache(DbmLog dbmLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dbmlog (";
        if (isRandomKeys || useExistingPK) command += "DbmLogNum,";
        command += "UserNum,FKey,FKeyType,ActionType,DateTimeEntry,MethodName,LogText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dbmLog.DbmLogNum) + ",";
        command +=
            SOut.Long(dbmLog.UserNum) + ","
                                      + SOut.Long(dbmLog.FKey) + ","
                                      + SOut.Int((int) dbmLog.FKeyType) + ","
                                      + SOut.Int((int) dbmLog.ActionType) + ","
                                      + DbHelper.Now() + ","
                                      + "'" + SOut.String(dbmLog.MethodName) + "',"
                                      + DbHelper.ParamChar + "paramLogText)";
        if (dbmLog.LogText == null) dbmLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(dbmLog.LogText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramLogText);
        else
            dbmLog.DbmLogNum = Db.NonQ(command, true, "DbmLogNum", "dbmLog", paramLogText);
        return dbmLog.DbmLogNum;
    }


    public static void Update(DbmLog dbmLog)
    {
        var command = "UPDATE dbmlog SET "
                      + "UserNum      =  " + SOut.Long(dbmLog.UserNum) + ", "
                      + "FKey         =  " + SOut.Long(dbmLog.FKey) + ", "
                      + "FKeyType     =  " + SOut.Int((int) dbmLog.FKeyType) + ", "
                      + "ActionType   =  " + SOut.Int((int) dbmLog.ActionType) + ", "
                      //DateTimeEntry not allowed to change
                      + "MethodName   = '" + SOut.String(dbmLog.MethodName) + "', "
                      + "LogText      =  " + DbHelper.ParamChar + "paramLogText "
                      + "WHERE DbmLogNum = " + SOut.Long(dbmLog.DbmLogNum);
        if (dbmLog.LogText == null) dbmLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(dbmLog.LogText));
        Db.NonQ(command, paramLogText);
    }


    public static bool Update(DbmLog dbmLog, DbmLog oldDbmLog)
    {
        var command = "";
        if (dbmLog.UserNum != oldDbmLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(dbmLog.UserNum) + "";
        }

        if (dbmLog.FKey != oldDbmLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(dbmLog.FKey) + "";
        }

        if (dbmLog.FKeyType != oldDbmLog.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = " + SOut.Int((int) dbmLog.FKeyType) + "";
        }

        if (dbmLog.ActionType != oldDbmLog.ActionType)
        {
            if (command != "") command += ",";
            command += "ActionType = " + SOut.Int((int) dbmLog.ActionType) + "";
        }

        //DateTimeEntry not allowed to change
        if (dbmLog.MethodName != oldDbmLog.MethodName)
        {
            if (command != "") command += ",";
            command += "MethodName = '" + SOut.String(dbmLog.MethodName) + "'";
        }

        if (dbmLog.LogText != oldDbmLog.LogText)
        {
            if (command != "") command += ",";
            command += "LogText = " + DbHelper.ParamChar + "paramLogText";
        }

        if (command == "") return false;
        if (dbmLog.LogText == null) dbmLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(dbmLog.LogText));
        command = "UPDATE dbmlog SET " + command
                                       + " WHERE DbmLogNum = " + SOut.Long(dbmLog.DbmLogNum);
        Db.NonQ(command, paramLogText);
        return true;
    }


    public static bool UpdateComparison(DbmLog dbmLog, DbmLog oldDbmLog)
    {
        if (dbmLog.UserNum != oldDbmLog.UserNum) return true;
        if (dbmLog.FKey != oldDbmLog.FKey) return true;
        if (dbmLog.FKeyType != oldDbmLog.FKeyType) return true;
        if (dbmLog.ActionType != oldDbmLog.ActionType) return true;
        //DateTimeEntry not allowed to change
        if (dbmLog.MethodName != oldDbmLog.MethodName) return true;
        if (dbmLog.LogText != oldDbmLog.LogText) return true;
        return false;
    }


    public static void Delete(long dbmLogNum)
    {
        var command = "DELETE FROM dbmlog "
                      + "WHERE DbmLogNum = " + SOut.Long(dbmLogNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listDbmLogNums)
    {
        if (listDbmLogNums == null || listDbmLogNums.Count == 0) return;
        var command = "DELETE FROM dbmlog "
                      + "WHERE DbmLogNum IN(" + string.Join(",", listDbmLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}