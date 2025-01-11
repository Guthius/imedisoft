#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ErxLogCrud
{
    public static ErxLog SelectOne(long erxLogNum)
    {
        var command = "SELECT * FROM erxlog "
                      + "WHERE ErxLogNum = " + SOut.Long(erxLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ErxLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ErxLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ErxLog> TableToList(DataTable table)
    {
        var retVal = new List<ErxLog>();
        ErxLog erxLog;
        foreach (DataRow row in table.Rows)
        {
            erxLog = new ErxLog();
            erxLog.ErxLogNum = SIn.Long(row["ErxLogNum"].ToString());
            erxLog.PatNum = SIn.Long(row["PatNum"].ToString());
            erxLog.MsgText = SIn.String(row["MsgText"].ToString());
            erxLog.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            erxLog.ProvNum = SIn.Long(row["ProvNum"].ToString());
            erxLog.UserNum = SIn.Long(row["UserNum"].ToString());
            retVal.Add(erxLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ErxLog> listErxLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ErxLog";
        var table = new DataTable(tableName);
        table.Columns.Add("ErxLogNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("MsgText");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("ProvNum");
        table.Columns.Add("UserNum");
        foreach (var erxLog in listErxLogs)
            table.Rows.Add(SOut.Long(erxLog.ErxLogNum), SOut.Long(erxLog.PatNum), erxLog.MsgText, SOut.DateT(erxLog.DateTStamp, false), SOut.Long(erxLog.ProvNum), SOut.Long(erxLog.UserNum));
        return table;
    }

    public static long Insert(ErxLog erxLog)
    {
        return Insert(erxLog, false);
    }

    public static long Insert(ErxLog erxLog, bool useExistingPK)
    {
        var command = "INSERT INTO erxlog (";

        command += "PatNum,MsgText,ProvNum,UserNum) VALUES(";

        command +=
            SOut.Long(erxLog.PatNum) + ","
                                     + DbHelper.ParamChar + "paramMsgText,"
                                     //DateTStamp can only be set by MySQL
                                     + SOut.Long(erxLog.ProvNum) + ","
                                     + SOut.Long(erxLog.UserNum) + ")";
        if (erxLog.MsgText == null) erxLog.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(erxLog.MsgText));
        {
            erxLog.ErxLogNum = Db.NonQ(command, true, "ErxLogNum", "erxLog", paramMsgText);
        }
        return erxLog.ErxLogNum;
    }

    public static long InsertNoCache(ErxLog erxLog)
    {
        return InsertNoCache(erxLog, false);
    }

    public static long InsertNoCache(ErxLog erxLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO erxlog (";
        if (isRandomKeys || useExistingPK) command += "ErxLogNum,";
        command += "PatNum,MsgText,ProvNum,UserNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(erxLog.ErxLogNum) + ",";
        command +=
            SOut.Long(erxLog.PatNum) + ","
                                     + DbHelper.ParamChar + "paramMsgText,"
                                     //DateTStamp can only be set by MySQL
                                     + SOut.Long(erxLog.ProvNum) + ","
                                     + SOut.Long(erxLog.UserNum) + ")";
        if (erxLog.MsgText == null) erxLog.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(erxLog.MsgText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMsgText);
        else
            erxLog.ErxLogNum = Db.NonQ(command, true, "ErxLogNum", "erxLog", paramMsgText);
        return erxLog.ErxLogNum;
    }


    public static void Update(ErxLog erxLog)
    {
        var command = "UPDATE erxlog SET "
                      + "PatNum    =  " + SOut.Long(erxLog.PatNum) + ", "
                      + "MsgText   =  " + DbHelper.ParamChar + "paramMsgText, "
                      //DateTStamp can only be set by MySQL
                      + "ProvNum   =  " + SOut.Long(erxLog.ProvNum) + ", "
                      + "UserNum   =  " + SOut.Long(erxLog.UserNum) + " "
                      + "WHERE ErxLogNum = " + SOut.Long(erxLog.ErxLogNum);
        if (erxLog.MsgText == null) erxLog.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(erxLog.MsgText));
        Db.NonQ(command, paramMsgText);
    }


    public static bool Update(ErxLog erxLog, ErxLog oldErxLog)
    {
        var command = "";
        if (erxLog.PatNum != oldErxLog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(erxLog.PatNum) + "";
        }

        if (erxLog.MsgText != oldErxLog.MsgText)
        {
            if (command != "") command += ",";
            command += "MsgText = " + DbHelper.ParamChar + "paramMsgText";
        }

        //DateTStamp can only be set by MySQL
        if (erxLog.ProvNum != oldErxLog.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(erxLog.ProvNum) + "";
        }

        if (erxLog.UserNum != oldErxLog.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(erxLog.UserNum) + "";
        }

        if (command == "") return false;
        if (erxLog.MsgText == null) erxLog.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(erxLog.MsgText));
        command = "UPDATE erxlog SET " + command
                                       + " WHERE ErxLogNum = " + SOut.Long(erxLog.ErxLogNum);
        Db.NonQ(command, paramMsgText);
        return true;
    }


    public static bool UpdateComparison(ErxLog erxLog, ErxLog oldErxLog)
    {
        if (erxLog.PatNum != oldErxLog.PatNum) return true;
        if (erxLog.MsgText != oldErxLog.MsgText) return true;
        //DateTStamp can only be set by MySQL
        if (erxLog.ProvNum != oldErxLog.ProvNum) return true;
        if (erxLog.UserNum != oldErxLog.UserNum) return true;
        return false;
    }


    public static void Delete(long erxLogNum)
    {
        var command = "DELETE FROM erxlog "
                      + "WHERE ErxLogNum = " + SOut.Long(erxLogNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listErxLogNums)
    {
        if (listErxLogNums == null || listErxLogNums.Count == 0) return;
        var command = "DELETE FROM erxlog "
                      + "WHERE ErxLogNum IN(" + string.Join(",", listErxLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}