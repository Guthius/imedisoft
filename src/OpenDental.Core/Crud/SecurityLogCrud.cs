#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SecurityLogCrud
{
    public static SecurityLog SelectOne(long securityLogNum)
    {
        var command = "SELECT * FROM securitylog "
                      + "WHERE SecurityLogNum = " + SOut.Long(securityLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SecurityLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SecurityLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SecurityLog> TableToList(DataTable table)
    {
        var retVal = new List<SecurityLog>();
        SecurityLog securityLog;
        foreach (DataRow row in table.Rows)
        {
            securityLog = new SecurityLog();
            securityLog.SecurityLogNum = SIn.Long(row["SecurityLogNum"].ToString());
            securityLog.PermType = (EnumPermType) SIn.Int(row["PermType"].ToString());
            securityLog.UserNum = SIn.Long(row["UserNum"].ToString());
            securityLog.LogDateTime = SIn.DateTime(row["LogDateTime"].ToString());
            securityLog.LogText = SIn.String(row["LogText"].ToString());
            securityLog.PatNum = SIn.Long(row["PatNum"].ToString());
            securityLog.CompName = SIn.String(row["CompName"].ToString());
            securityLog.FKey = SIn.Long(row["FKey"].ToString());
            securityLog.LogSource = (LogSources) SIn.Int(row["LogSource"].ToString());
            securityLog.DefNum = SIn.Long(row["DefNum"].ToString());
            securityLog.DefNumError = SIn.Long(row["DefNumError"].ToString());
            securityLog.DateTPrevious = SIn.DateTime(row["DateTPrevious"].ToString());
            retVal.Add(securityLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SecurityLog> listSecurityLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SecurityLog";
        var table = new DataTable(tableName);
        table.Columns.Add("SecurityLogNum");
        table.Columns.Add("PermType");
        table.Columns.Add("UserNum");
        table.Columns.Add("LogDateTime");
        table.Columns.Add("LogText");
        table.Columns.Add("PatNum");
        table.Columns.Add("CompName");
        table.Columns.Add("FKey");
        table.Columns.Add("LogSource");
        table.Columns.Add("DefNum");
        table.Columns.Add("DefNumError");
        table.Columns.Add("DateTPrevious");
        foreach (var securityLog in listSecurityLogs)
            table.Rows.Add(SOut.Long(securityLog.SecurityLogNum), SOut.Int((int) securityLog.PermType), SOut.Long(securityLog.UserNum), SOut.DateT(securityLog.LogDateTime, false), securityLog.LogText, SOut.Long(securityLog.PatNum), securityLog.CompName, SOut.Long(securityLog.FKey), SOut.Int((int) securityLog.LogSource), SOut.Long(securityLog.DefNum), SOut.Long(securityLog.DefNumError), SOut.DateT(securityLog.DateTPrevious, false));
        return table;
    }

    public static long Insert(SecurityLog securityLog)
    {
        return Insert(securityLog, false);
    }

    public static long Insert(SecurityLog securityLog, bool useExistingPK)
    {
        var command = "INSERT INTO securitylog (";

        command += "PermType,UserNum,LogDateTime,LogText,PatNum,CompName,FKey,LogSource,DefNum,DefNumError,DateTPrevious) VALUES(";

        command +=
            SOut.Int((int) securityLog.PermType) + ","
                                                 + SOut.Long(securityLog.UserNum) + ","
                                                 + DbHelper.Now() + ","
                                                 + DbHelper.ParamChar + "paramLogText,"
                                                 + SOut.Long(securityLog.PatNum) + ","
                                                 + "'" + SOut.String(securityLog.CompName) + "',"
                                                 + SOut.Long(securityLog.FKey) + ","
                                                 + SOut.Int((int) securityLog.LogSource) + ","
                                                 + SOut.Long(securityLog.DefNum) + ","
                                                 + SOut.Long(securityLog.DefNumError) + ","
                                                 + SOut.DateT(securityLog.DateTPrevious) + ")";
        if (securityLog.LogText == null) securityLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(securityLog.LogText));
        {
            securityLog.SecurityLogNum = Db.NonQ(command, true, "SecurityLogNum", "securityLog", paramLogText);
        }
        return securityLog.SecurityLogNum;
    }

    public static long InsertNoCache(SecurityLog securityLog)
    {
        return InsertNoCache(securityLog, false);
    }

    public static long InsertNoCache(SecurityLog securityLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO securitylog (";
        if (isRandomKeys || useExistingPK) command += "SecurityLogNum,";
        command += "PermType,UserNum,LogDateTime,LogText,PatNum,CompName,FKey,LogSource,DefNum,DefNumError,DateTPrevious) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(securityLog.SecurityLogNum) + ",";
        command +=
            SOut.Int((int) securityLog.PermType) + ","
                                                 + SOut.Long(securityLog.UserNum) + ","
                                                 + DbHelper.Now() + ","
                                                 + DbHelper.ParamChar + "paramLogText,"
                                                 + SOut.Long(securityLog.PatNum) + ","
                                                 + "'" + SOut.String(securityLog.CompName) + "',"
                                                 + SOut.Long(securityLog.FKey) + ","
                                                 + SOut.Int((int) securityLog.LogSource) + ","
                                                 + SOut.Long(securityLog.DefNum) + ","
                                                 + SOut.Long(securityLog.DefNumError) + ","
                                                 + SOut.DateT(securityLog.DateTPrevious) + ")";
        if (securityLog.LogText == null) securityLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(securityLog.LogText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramLogText);
        else
            securityLog.SecurityLogNum = Db.NonQ(command, true, "SecurityLogNum", "securityLog", paramLogText);
        return securityLog.SecurityLogNum;
    }

    public static void Update(SecurityLog securityLog)
    {
        var command = "UPDATE securitylog SET "
                      + "PermType      =  " + SOut.Int((int) securityLog.PermType) + ", "
                      + "UserNum       =  " + SOut.Long(securityLog.UserNum) + ", "
                      //LogDateTime not allowed to change
                      + "LogText       =  " + DbHelper.ParamChar + "paramLogText, "
                      + "PatNum        =  " + SOut.Long(securityLog.PatNum) + ", "
                      + "CompName      = '" + SOut.String(securityLog.CompName) + "', "
                      + "FKey          =  " + SOut.Long(securityLog.FKey) + ", "
                      + "LogSource     =  " + SOut.Int((int) securityLog.LogSource) + ", "
                      + "DefNum        =  " + SOut.Long(securityLog.DefNum) + ", "
                      + "DefNumError   =  " + SOut.Long(securityLog.DefNumError) + ", "
                      + "DateTPrevious =  " + SOut.DateT(securityLog.DateTPrevious) + " "
                      + "WHERE SecurityLogNum = " + SOut.Long(securityLog.SecurityLogNum);
        if (securityLog.LogText == null) securityLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(securityLog.LogText));
        Db.NonQ(command, paramLogText);
    }

    public static bool Update(SecurityLog securityLog, SecurityLog oldSecurityLog)
    {
        var command = "";
        if (securityLog.PermType != oldSecurityLog.PermType)
        {
            if (command != "") command += ",";
            command += "PermType = " + SOut.Int((int) securityLog.PermType) + "";
        }

        if (securityLog.UserNum != oldSecurityLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(securityLog.UserNum) + "";
        }

        //LogDateTime not allowed to change
        if (securityLog.LogText != oldSecurityLog.LogText)
        {
            if (command != "") command += ",";
            command += "LogText = " + DbHelper.ParamChar + "paramLogText";
        }

        if (securityLog.PatNum != oldSecurityLog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(securityLog.PatNum) + "";
        }

        if (securityLog.CompName != oldSecurityLog.CompName)
        {
            if (command != "") command += ",";
            command += "CompName = '" + SOut.String(securityLog.CompName) + "'";
        }

        if (securityLog.FKey != oldSecurityLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(securityLog.FKey) + "";
        }

        if (securityLog.LogSource != oldSecurityLog.LogSource)
        {
            if (command != "") command += ",";
            command += "LogSource = " + SOut.Int((int) securityLog.LogSource) + "";
        }

        if (securityLog.DefNum != oldSecurityLog.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(securityLog.DefNum) + "";
        }

        if (securityLog.DefNumError != oldSecurityLog.DefNumError)
        {
            if (command != "") command += ",";
            command += "DefNumError = " + SOut.Long(securityLog.DefNumError) + "";
        }

        if (securityLog.DateTPrevious != oldSecurityLog.DateTPrevious)
        {
            if (command != "") command += ",";
            command += "DateTPrevious = " + SOut.DateT(securityLog.DateTPrevious) + "";
        }

        if (command == "") return false;
        if (securityLog.LogText == null) securityLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", OdDbType.Text, SOut.StringParam(securityLog.LogText));
        command = "UPDATE securitylog SET " + command
                                            + " WHERE SecurityLogNum = " + SOut.Long(securityLog.SecurityLogNum);
        Db.NonQ(command, paramLogText);
        return true;
    }

    public static bool UpdateComparison(SecurityLog securityLog, SecurityLog oldSecurityLog)
    {
        if (securityLog.PermType != oldSecurityLog.PermType) return true;
        if (securityLog.UserNum != oldSecurityLog.UserNum) return true;
        //LogDateTime not allowed to change
        if (securityLog.LogText != oldSecurityLog.LogText) return true;
        if (securityLog.PatNum != oldSecurityLog.PatNum) return true;
        if (securityLog.CompName != oldSecurityLog.CompName) return true;
        if (securityLog.FKey != oldSecurityLog.FKey) return true;
        if (securityLog.LogSource != oldSecurityLog.LogSource) return true;
        if (securityLog.DefNum != oldSecurityLog.DefNum) return true;
        if (securityLog.DefNumError != oldSecurityLog.DefNumError) return true;
        if (securityLog.DateTPrevious != oldSecurityLog.DateTPrevious) return true;
        return false;
    }

    public static void Delete(long securityLogNum)
    {
        var command = "DELETE FROM securitylog "
                      + "WHERE SecurityLogNum = " + SOut.Long(securityLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSecurityLogNums)
    {
        if (listSecurityLogNums == null || listSecurityLogNums.Count == 0) return;
        var command = "DELETE FROM securitylog "
                      + "WHERE SecurityLogNum IN(" + string.Join(",", listSecurityLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}