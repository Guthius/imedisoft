#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoChartLogCrud
{
    public static OrthoChartLog SelectOne(long orthoChartLogNum)
    {
        var command = "SELECT * FROM orthochartlog "
                      + "WHERE OrthoChartLogNum = " + SOut.Long(orthoChartLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoChartLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoChartLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChartLog> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChartLog>();
        OrthoChartLog orthoChartLog;
        foreach (DataRow row in table.Rows)
        {
            orthoChartLog = new OrthoChartLog();
            orthoChartLog.OrthoChartLogNum = SIn.Long(row["OrthoChartLogNum"].ToString());
            orthoChartLog.PatNum = SIn.Long(row["PatNum"].ToString());
            orthoChartLog.ComputerName = SIn.String(row["ComputerName"].ToString());
            orthoChartLog.DateTimeLog = SIn.DateTime(row["DateTimeLog"].ToString());
            orthoChartLog.DateTimeService = SIn.DateTime(row["DateTimeService"].ToString());
            orthoChartLog.UserNum = SIn.Long(row["UserNum"].ToString());
            orthoChartLog.ProvNum = SIn.Long(row["ProvNum"].ToString());
            orthoChartLog.OrthoChartRowNum = SIn.Long(row["OrthoChartRowNum"].ToString());
            orthoChartLog.LogData = SIn.String(row["LogData"].ToString());
            retVal.Add(orthoChartLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoChartLog> listOrthoChartLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoChartLog";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoChartLogNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ComputerName");
        table.Columns.Add("DateTimeLog");
        table.Columns.Add("DateTimeService");
        table.Columns.Add("UserNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("OrthoChartRowNum");
        table.Columns.Add("LogData");
        foreach (var orthoChartLog in listOrthoChartLogs)
            table.Rows.Add(SOut.Long(orthoChartLog.OrthoChartLogNum), SOut.Long(orthoChartLog.PatNum), orthoChartLog.ComputerName, SOut.DateT(orthoChartLog.DateTimeLog, false), SOut.DateT(orthoChartLog.DateTimeService, false), SOut.Long(orthoChartLog.UserNum), SOut.Long(orthoChartLog.ProvNum), SOut.Long(orthoChartLog.OrthoChartRowNum), orthoChartLog.LogData);
        return table;
    }

    public static long Insert(OrthoChartLog orthoChartLog)
    {
        return Insert(orthoChartLog, false);
    }

    public static long Insert(OrthoChartLog orthoChartLog, bool useExistingPK)
    {
        var command = "INSERT INTO orthochartlog (";

        command += "PatNum,ComputerName,DateTimeLog,DateTimeService,UserNum,ProvNum,OrthoChartRowNum,LogData) VALUES(";

        command +=
            SOut.Long(orthoChartLog.PatNum) + ","
                                            + "'" + SOut.String(orthoChartLog.ComputerName) + "',"
                                            + SOut.DateT(orthoChartLog.DateTimeLog) + ","
                                            + SOut.DateT(orthoChartLog.DateTimeService) + ","
                                            + SOut.Long(orthoChartLog.UserNum) + ","
                                            + SOut.Long(orthoChartLog.ProvNum) + ","
                                            + SOut.Long(orthoChartLog.OrthoChartRowNum) + ","
                                            + DbHelper.ParamChar + "paramLogData)";
        if (orthoChartLog.LogData == null) orthoChartLog.LogData = "";
        var paramLogData = new OdSqlParameter("paramLogData", OdDbType.Text, SOut.StringParam(orthoChartLog.LogData));
        {
            orthoChartLog.OrthoChartLogNum = Db.NonQ(command, true, "OrthoChartLogNum", "orthoChartLog", paramLogData);
        }
        return orthoChartLog.OrthoChartLogNum;
    }

    public static long InsertNoCache(OrthoChartLog orthoChartLog)
    {
        return InsertNoCache(orthoChartLog, false);
    }

    public static long InsertNoCache(OrthoChartLog orthoChartLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthochartlog (";
        if (isRandomKeys || useExistingPK) command += "OrthoChartLogNum,";
        command += "PatNum,ComputerName,DateTimeLog,DateTimeService,UserNum,ProvNum,OrthoChartRowNum,LogData) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoChartLog.OrthoChartLogNum) + ",";
        command +=
            SOut.Long(orthoChartLog.PatNum) + ","
                                            + "'" + SOut.String(orthoChartLog.ComputerName) + "',"
                                            + SOut.DateT(orthoChartLog.DateTimeLog) + ","
                                            + SOut.DateT(orthoChartLog.DateTimeService) + ","
                                            + SOut.Long(orthoChartLog.UserNum) + ","
                                            + SOut.Long(orthoChartLog.ProvNum) + ","
                                            + SOut.Long(orthoChartLog.OrthoChartRowNum) + ","
                                            + DbHelper.ParamChar + "paramLogData)";
        if (orthoChartLog.LogData == null) orthoChartLog.LogData = "";
        var paramLogData = new OdSqlParameter("paramLogData", OdDbType.Text, SOut.StringParam(orthoChartLog.LogData));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramLogData);
        else
            orthoChartLog.OrthoChartLogNum = Db.NonQ(command, true, "OrthoChartLogNum", "orthoChartLog", paramLogData);
        return orthoChartLog.OrthoChartLogNum;
    }

    public static void Update(OrthoChartLog orthoChartLog)
    {
        var command = "UPDATE orthochartlog SET "
                      + "PatNum          =  " + SOut.Long(orthoChartLog.PatNum) + ", "
                      + "ComputerName    = '" + SOut.String(orthoChartLog.ComputerName) + "', "
                      + "DateTimeLog     =  " + SOut.DateT(orthoChartLog.DateTimeLog) + ", "
                      + "DateTimeService =  " + SOut.DateT(orthoChartLog.DateTimeService) + ", "
                      + "UserNum         =  " + SOut.Long(orthoChartLog.UserNum) + ", "
                      + "ProvNum         =  " + SOut.Long(orthoChartLog.ProvNum) + ", "
                      + "OrthoChartRowNum=  " + SOut.Long(orthoChartLog.OrthoChartRowNum) + ", "
                      + "LogData         =  " + DbHelper.ParamChar + "paramLogData "
                      + "WHERE OrthoChartLogNum = " + SOut.Long(orthoChartLog.OrthoChartLogNum);
        if (orthoChartLog.LogData == null) orthoChartLog.LogData = "";
        var paramLogData = new OdSqlParameter("paramLogData", OdDbType.Text, SOut.StringParam(orthoChartLog.LogData));
        Db.NonQ(command, paramLogData);
    }

    public static bool Update(OrthoChartLog orthoChartLog, OrthoChartLog oldOrthoChartLog)
    {
        var command = "";
        if (orthoChartLog.PatNum != oldOrthoChartLog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(orthoChartLog.PatNum) + "";
        }

        if (orthoChartLog.ComputerName != oldOrthoChartLog.ComputerName)
        {
            if (command != "") command += ",";
            command += "ComputerName = '" + SOut.String(orthoChartLog.ComputerName) + "'";
        }

        if (orthoChartLog.DateTimeLog != oldOrthoChartLog.DateTimeLog)
        {
            if (command != "") command += ",";
            command += "DateTimeLog = " + SOut.DateT(orthoChartLog.DateTimeLog) + "";
        }

        if (orthoChartLog.DateTimeService != oldOrthoChartLog.DateTimeService)
        {
            if (command != "") command += ",";
            command += "DateTimeService = " + SOut.DateT(orthoChartLog.DateTimeService) + "";
        }

        if (orthoChartLog.UserNum != oldOrthoChartLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(orthoChartLog.UserNum) + "";
        }

        if (orthoChartLog.ProvNum != oldOrthoChartLog.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(orthoChartLog.ProvNum) + "";
        }

        if (orthoChartLog.OrthoChartRowNum != oldOrthoChartLog.OrthoChartRowNum)
        {
            if (command != "") command += ",";
            command += "OrthoChartRowNum = " + SOut.Long(orthoChartLog.OrthoChartRowNum) + "";
        }

        if (orthoChartLog.LogData != oldOrthoChartLog.LogData)
        {
            if (command != "") command += ",";
            command += "LogData = " + DbHelper.ParamChar + "paramLogData";
        }

        if (command == "") return false;
        if (orthoChartLog.LogData == null) orthoChartLog.LogData = "";
        var paramLogData = new OdSqlParameter("paramLogData", OdDbType.Text, SOut.StringParam(orthoChartLog.LogData));
        command = "UPDATE orthochartlog SET " + command
                                              + " WHERE OrthoChartLogNum = " + SOut.Long(orthoChartLog.OrthoChartLogNum);
        Db.NonQ(command, paramLogData);
        return true;
    }

    public static bool UpdateComparison(OrthoChartLog orthoChartLog, OrthoChartLog oldOrthoChartLog)
    {
        if (orthoChartLog.PatNum != oldOrthoChartLog.PatNum) return true;
        if (orthoChartLog.ComputerName != oldOrthoChartLog.ComputerName) return true;
        if (orthoChartLog.DateTimeLog != oldOrthoChartLog.DateTimeLog) return true;
        if (orthoChartLog.DateTimeService != oldOrthoChartLog.DateTimeService) return true;
        if (orthoChartLog.UserNum != oldOrthoChartLog.UserNum) return true;
        if (orthoChartLog.ProvNum != oldOrthoChartLog.ProvNum) return true;
        if (orthoChartLog.OrthoChartRowNum != oldOrthoChartLog.OrthoChartRowNum) return true;
        if (orthoChartLog.LogData != oldOrthoChartLog.LogData) return true;
        return false;
    }

    public static void Delete(long orthoChartLogNum)
    {
        var command = "DELETE FROM orthochartlog "
                      + "WHERE OrthoChartLogNum = " + SOut.Long(orthoChartLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoChartLogNums)
    {
        if (listOrthoChartLogNums == null || listOrthoChartLogNums.Count == 0) return;
        var command = "DELETE FROM orthochartlog "
                      + "WHERE OrthoChartLogNum IN(" + string.Join(",", listOrthoChartLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}