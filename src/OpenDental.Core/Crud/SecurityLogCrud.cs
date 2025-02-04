using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<SecurityLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SecurityLog> TableToList(DataTable table)
    {
        var retVal = new List<SecurityLog>();
        foreach (DataRow row in table.Rows)
        {
            var securityLog = new SecurityLog
            {
                SecurityLogNum = SIn.Long(row["SecurityLogNum"].ToString()),
                PermType = (EnumPermType) SIn.Int(row["PermType"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                LogDateTime = SIn.DateTime(row["LogDateTime"].ToString()),
                LogText = SIn.String(row["LogText"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                CompName = SIn.String(row["CompName"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                LogSource = (LogSources) SIn.Int(row["LogSource"].ToString()),
                DefNum = SIn.Long(row["DefNum"].ToString()),
                DefNumError = SIn.Long(row["DefNumError"].ToString()),
                DateTPrevious = SIn.DateTime(row["DateTPrevious"].ToString())
            };
            retVal.Add(securityLog);
        }

        return retVal;
    }

    public static long Insert(SecurityLog securityLog)
    {
        var command = "INSERT INTO securitylog (";

        command += "PermType,UserNum,LogDateTime,LogText,PatNum,CompName,FKey,LogSource,DefNum,DefNumError,DateTPrevious) VALUES(";

        command +=
            SOut.Int((int) securityLog.PermType) + ","
                                                 + SOut.Long(securityLog.UserNum) + ","
                                                 + "NOW()" + ","
                                                 + DbHelper.ParamChar + "paramLogText,"
                                                 + SOut.Long(securityLog.PatNum) + ","
                                                 + "'" + SOut.String(securityLog.CompName) + "',"
                                                 + SOut.Long(securityLog.FKey) + ","
                                                 + SOut.Int((int) securityLog.LogSource) + ","
                                                 + SOut.Long(securityLog.DefNum) + ","
                                                 + SOut.Long(securityLog.DefNumError) + ","
                                                 + SOut.DateTime(securityLog.DateTPrevious) + ")";
        if (securityLog.LogText == null) securityLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", SOut.StringParam(securityLog.LogText));
        {
            securityLog.SecurityLogNum = Db.NonQ(command, true, "SecurityLogNum", "securityLog", paramLogText);
        }
        return securityLog.SecurityLogNum;
    }

    public static long InsertNoCache(SecurityLog securityLog, bool useExistingPK = false)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO securitylog (";
        if (isRandomKeys || useExistingPK) command += "SecurityLogNum,";
        command += "PermType,UserNum,LogDateTime,LogText,PatNum,CompName,FKey,LogSource,DefNum,DefNumError,DateTPrevious) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(securityLog.SecurityLogNum) + ",";
        command +=
            SOut.Int((int) securityLog.PermType) + ","
                                                 + SOut.Long(securityLog.UserNum) + ","
                                                 + "NOW()" + ","
                                                 + DbHelper.ParamChar + "paramLogText,"
                                                 + SOut.Long(securityLog.PatNum) + ","
                                                 + "'" + SOut.String(securityLog.CompName) + "',"
                                                 + SOut.Long(securityLog.FKey) + ","
                                                 + SOut.Int((int) securityLog.LogSource) + ","
                                                 + SOut.Long(securityLog.DefNum) + ","
                                                 + SOut.Long(securityLog.DefNumError) + ","
                                                 + SOut.DateTime(securityLog.DateTPrevious) + ")";
        if (securityLog.LogText == null) securityLog.LogText = "";
        var paramLogText = new OdSqlParameter("paramLogText", SOut.StringParam(securityLog.LogText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramLogText);
        else
            securityLog.SecurityLogNum = Db.NonQ(command, true, "SecurityLogNum", "securityLog", paramLogText);
        return securityLog.SecurityLogNum;
    }
}