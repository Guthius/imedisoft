#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EServiceLogCrud
{
    public static EServiceLog SelectOne(long eServiceLogNum)
    {
        var command = "SELECT * FROM eservicelog "
                      + "WHERE EServiceLogNum = " + SOut.Long(eServiceLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EServiceLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EServiceLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EServiceLog> TableToList(DataTable table)
    {
        var retVal = new List<EServiceLog>();
        EServiceLog eServiceLog;
        foreach (DataRow row in table.Rows)
        {
            eServiceLog = new EServiceLog();
            eServiceLog.EServiceLogNum = SIn.Long(row["EServiceLogNum"].ToString());
            eServiceLog.KeyType = (FKeyType) SIn.Int(row["KeyType"].ToString());
            eServiceLog.EServiceType = (eServiceType) SIn.Int(row["EServiceType"].ToString());
            eServiceLog.EServiceAction = (eServiceAction) SIn.Int(row["EServiceAction"].ToString());
            eServiceLog.LogDateTime = SIn.DateTime(row["LogDateTime"].ToString());
            eServiceLog.PatNum = SIn.Long(row["PatNum"].ToString());
            eServiceLog.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            eServiceLog.LogGuid = SIn.String(row["LogGuid"].ToString());
            eServiceLog.FKey = SIn.Long(row["FKey"].ToString());
            eServiceLog.DateTimeUploaded = SIn.DateTime(row["DateTimeUploaded"].ToString());
            eServiceLog.Note = SIn.String(row["Note"].ToString());
            retVal.Add(eServiceLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EServiceLog> listEServiceLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EServiceLog";
        var table = new DataTable(tableName);
        table.Columns.Add("EServiceLogNum");
        table.Columns.Add("KeyType");
        table.Columns.Add("EServiceType");
        table.Columns.Add("EServiceAction");
        table.Columns.Add("LogDateTime");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("LogGuid");
        table.Columns.Add("FKey");
        table.Columns.Add("DateTimeUploaded");
        table.Columns.Add("Note");
        foreach (var eServiceLog in listEServiceLogs)
            table.Rows.Add(SOut.Long(eServiceLog.EServiceLogNum), SOut.Int((int) eServiceLog.KeyType), SOut.Int((int) eServiceLog.EServiceType), SOut.Int((int) eServiceLog.EServiceAction), SOut.DateT(eServiceLog.LogDateTime, false), SOut.Long(eServiceLog.PatNum), SOut.Long(eServiceLog.ClinicNum), eServiceLog.LogGuid, SOut.Long(eServiceLog.FKey), SOut.DateT(eServiceLog.DateTimeUploaded, false), eServiceLog.Note);
        return table;
    }

    public static long Insert(EServiceLog eServiceLog)
    {
        return Insert(eServiceLog, false);
    }

    public static long Insert(EServiceLog eServiceLog, bool useExistingPK)
    {
        var command = "INSERT INTO eservicelog (";

        command += "KeyType,EServiceType,EServiceAction,LogDateTime,PatNum,ClinicNum,LogGuid,FKey,DateTimeUploaded,Note) VALUES(";

        command +=
            SOut.Int((int) eServiceLog.KeyType) + ","
                                                + SOut.Int((int) eServiceLog.EServiceType) + ","
                                                + SOut.Int((int) eServiceLog.EServiceAction) + ","
                                                + DbHelper.Now() + ","
                                                + SOut.Long(eServiceLog.PatNum) + ","
                                                + SOut.Long(eServiceLog.ClinicNum) + ","
                                                + "'" + SOut.String(eServiceLog.LogGuid) + "',"
                                                + SOut.Long(eServiceLog.FKey) + ","
                                                + SOut.DateT(eServiceLog.DateTimeUploaded) + ","
                                                + "'" + SOut.String(eServiceLog.Note) + "')";
        {
            eServiceLog.EServiceLogNum = Db.NonQ(command, true, "EServiceLogNum", "eServiceLog");
        }
        return eServiceLog.EServiceLogNum;
    }

    public static long InsertNoCache(EServiceLog eServiceLog)
    {
        return InsertNoCache(eServiceLog, false);
    }

    public static long InsertNoCache(EServiceLog eServiceLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eservicelog (";
        if (isRandomKeys || useExistingPK) command += "EServiceLogNum,";
        command += "KeyType,EServiceType,EServiceAction,LogDateTime,PatNum,ClinicNum,LogGuid,FKey,DateTimeUploaded,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eServiceLog.EServiceLogNum) + ",";
        command +=
            SOut.Int((int) eServiceLog.KeyType) + ","
                                                + SOut.Int((int) eServiceLog.EServiceType) + ","
                                                + SOut.Int((int) eServiceLog.EServiceAction) + ","
                                                + DbHelper.Now() + ","
                                                + SOut.Long(eServiceLog.PatNum) + ","
                                                + SOut.Long(eServiceLog.ClinicNum) + ","
                                                + "'" + SOut.String(eServiceLog.LogGuid) + "',"
                                                + SOut.Long(eServiceLog.FKey) + ","
                                                + SOut.DateT(eServiceLog.DateTimeUploaded) + ","
                                                + "'" + SOut.String(eServiceLog.Note) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eServiceLog.EServiceLogNum = Db.NonQ(command, true, "EServiceLogNum", "eServiceLog");
        return eServiceLog.EServiceLogNum;
    }

    public static void Update(EServiceLog eServiceLog)
    {
        var command = "UPDATE eservicelog SET "
                      + "KeyType         =  " + SOut.Int((int) eServiceLog.KeyType) + ", "
                      + "EServiceType    =  " + SOut.Int((int) eServiceLog.EServiceType) + ", "
                      + "EServiceAction  =  " + SOut.Int((int) eServiceLog.EServiceAction) + ", "
                      //LogDateTime not allowed to change
                      + "PatNum          =  " + SOut.Long(eServiceLog.PatNum) + ", "
                      + "ClinicNum       =  " + SOut.Long(eServiceLog.ClinicNum) + ", "
                      + "LogGuid         = '" + SOut.String(eServiceLog.LogGuid) + "', "
                      + "FKey            =  " + SOut.Long(eServiceLog.FKey) + ", "
                      + "DateTimeUploaded=  " + SOut.DateT(eServiceLog.DateTimeUploaded) + ", "
                      + "Note            = '" + SOut.String(eServiceLog.Note) + "' "
                      + "WHERE EServiceLogNum = " + SOut.Long(eServiceLog.EServiceLogNum);
        Db.NonQ(command);
    }

    public static bool Update(EServiceLog eServiceLog, EServiceLog oldEServiceLog)
    {
        var command = "";
        if (eServiceLog.KeyType != oldEServiceLog.KeyType)
        {
            if (command != "") command += ",";
            command += "KeyType = " + SOut.Int((int) eServiceLog.KeyType) + "";
        }

        if (eServiceLog.EServiceType != oldEServiceLog.EServiceType)
        {
            if (command != "") command += ",";
            command += "EServiceType = " + SOut.Int((int) eServiceLog.EServiceType) + "";
        }

        if (eServiceLog.EServiceAction != oldEServiceLog.EServiceAction)
        {
            if (command != "") command += ",";
            command += "EServiceAction = " + SOut.Int((int) eServiceLog.EServiceAction) + "";
        }

        //LogDateTime not allowed to change
        if (eServiceLog.PatNum != oldEServiceLog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(eServiceLog.PatNum) + "";
        }

        if (eServiceLog.ClinicNum != oldEServiceLog.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(eServiceLog.ClinicNum) + "";
        }

        if (eServiceLog.LogGuid != oldEServiceLog.LogGuid)
        {
            if (command != "") command += ",";
            command += "LogGuid = '" + SOut.String(eServiceLog.LogGuid) + "'";
        }

        if (eServiceLog.FKey != oldEServiceLog.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(eServiceLog.FKey) + "";
        }

        if (eServiceLog.DateTimeUploaded != oldEServiceLog.DateTimeUploaded)
        {
            if (command != "") command += ",";
            command += "DateTimeUploaded = " + SOut.DateT(eServiceLog.DateTimeUploaded) + "";
        }

        if (eServiceLog.Note != oldEServiceLog.Note)
        {
            if (command != "") command += ",";
            command += "Note = '" + SOut.String(eServiceLog.Note) + "'";
        }

        if (command == "") return false;
        command = "UPDATE eservicelog SET " + command
                                            + " WHERE EServiceLogNum = " + SOut.Long(eServiceLog.EServiceLogNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EServiceLog eServiceLog, EServiceLog oldEServiceLog)
    {
        if (eServiceLog.KeyType != oldEServiceLog.KeyType) return true;
        if (eServiceLog.EServiceType != oldEServiceLog.EServiceType) return true;
        if (eServiceLog.EServiceAction != oldEServiceLog.EServiceAction) return true;
        //LogDateTime not allowed to change
        if (eServiceLog.PatNum != oldEServiceLog.PatNum) return true;
        if (eServiceLog.ClinicNum != oldEServiceLog.ClinicNum) return true;
        if (eServiceLog.LogGuid != oldEServiceLog.LogGuid) return true;
        if (eServiceLog.FKey != oldEServiceLog.FKey) return true;
        if (eServiceLog.DateTimeUploaded != oldEServiceLog.DateTimeUploaded) return true;
        if (eServiceLog.Note != oldEServiceLog.Note) return true;
        return false;
    }

    public static void Delete(long eServiceLogNum)
    {
        var command = "DELETE FROM eservicelog "
                      + "WHERE EServiceLogNum = " + SOut.Long(eServiceLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEServiceLogNums)
    {
        if (listEServiceLogNums == null || listEServiceLogNums.Count == 0) return;
        var command = "DELETE FROM eservicelog "
                      + "WHERE EServiceLogNum IN(" + string.Join(",", listEServiceLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}