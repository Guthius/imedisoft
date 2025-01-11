#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SmsFromMobileCrud
{
    public static SmsFromMobile SelectOne(long smsFromMobileNum)
    {
        var command = "SELECT * FROM smsfrommobile "
                      + "WHERE SmsFromMobileNum = " + SOut.Long(smsFromMobileNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SmsFromMobile SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SmsFromMobile> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsFromMobile> TableToList(DataTable table)
    {
        var retVal = new List<SmsFromMobile>();
        SmsFromMobile smsFromMobile;
        foreach (DataRow row in table.Rows)
        {
            smsFromMobile = new SmsFromMobile();
            smsFromMobile.SmsFromMobileNum = SIn.Long(row["SmsFromMobileNum"].ToString());
            smsFromMobile.PatNum = SIn.Long(row["PatNum"].ToString());
            smsFromMobile.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            smsFromMobile.CommlogNum = SIn.Long(row["CommlogNum"].ToString());
            smsFromMobile.MsgText = SIn.String(row["MsgText"].ToString());
            smsFromMobile.DateTimeReceived = SIn.DateTime(row["DateTimeReceived"].ToString());
            smsFromMobile.SmsPhoneNumber = SIn.String(row["SmsPhoneNumber"].ToString());
            smsFromMobile.MobilePhoneNumber = SIn.String(row["MobilePhoneNumber"].ToString());
            smsFromMobile.MsgPart = SIn.Int(row["MsgPart"].ToString());
            smsFromMobile.MsgTotal = SIn.Int(row["MsgTotal"].ToString());
            smsFromMobile.MsgRefID = SIn.String(row["MsgRefID"].ToString());
            smsFromMobile.SmsStatus = (SmsFromStatus) SIn.Int(row["SmsStatus"].ToString());
            smsFromMobile.Flags = SIn.String(row["Flags"].ToString());
            smsFromMobile.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            smsFromMobile.MatchCount = SIn.Int(row["MatchCount"].ToString());
            smsFromMobile.GuidMessage = SIn.String(row["GuidMessage"].ToString());
            smsFromMobile.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(smsFromMobile);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SmsFromMobile> listSmsFromMobiles, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SmsFromMobile";
        var table = new DataTable(tableName);
        table.Columns.Add("SmsFromMobileNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("CommlogNum");
        table.Columns.Add("MsgText");
        table.Columns.Add("DateTimeReceived");
        table.Columns.Add("SmsPhoneNumber");
        table.Columns.Add("MobilePhoneNumber");
        table.Columns.Add("MsgPart");
        table.Columns.Add("MsgTotal");
        table.Columns.Add("MsgRefID");
        table.Columns.Add("SmsStatus");
        table.Columns.Add("Flags");
        table.Columns.Add("IsHidden");
        table.Columns.Add("MatchCount");
        table.Columns.Add("GuidMessage");
        table.Columns.Add("SecDateTEdit");
        foreach (var smsFromMobile in listSmsFromMobiles)
            table.Rows.Add(SOut.Long(smsFromMobile.SmsFromMobileNum), SOut.Long(smsFromMobile.PatNum), SOut.Long(smsFromMobile.ClinicNum), SOut.Long(smsFromMobile.CommlogNum), smsFromMobile.MsgText, SOut.DateT(smsFromMobile.DateTimeReceived, false), smsFromMobile.SmsPhoneNumber, smsFromMobile.MobilePhoneNumber, SOut.Int(smsFromMobile.MsgPart), SOut.Int(smsFromMobile.MsgTotal), smsFromMobile.MsgRefID, SOut.Int((int) smsFromMobile.SmsStatus), smsFromMobile.Flags, SOut.Bool(smsFromMobile.IsHidden), SOut.Int(smsFromMobile.MatchCount), smsFromMobile.GuidMessage, SOut.DateT(smsFromMobile.SecDateTEdit, false));
        return table;
    }

    public static long Insert(SmsFromMobile smsFromMobile)
    {
        return Insert(smsFromMobile, false);
    }

    public static long Insert(SmsFromMobile smsFromMobile, bool useExistingPK)
    {
        var command = "INSERT INTO smsfrommobile (";

        command += "PatNum,ClinicNum,CommlogNum,MsgText,DateTimeReceived,SmsPhoneNumber,MobilePhoneNumber,MsgPart,MsgTotal,MsgRefID,SmsStatus,Flags,IsHidden,MatchCount,GuidMessage) VALUES(";

        command +=
            SOut.Long(smsFromMobile.PatNum) + ","
                                            + SOut.Long(smsFromMobile.ClinicNum) + ","
                                            + SOut.Long(smsFromMobile.CommlogNum) + ","
                                            + DbHelper.ParamChar + "paramMsgText,"
                                            + SOut.DateT(smsFromMobile.DateTimeReceived) + ","
                                            + "'" + SOut.String(smsFromMobile.SmsPhoneNumber) + "',"
                                            + "'" + SOut.String(smsFromMobile.MobilePhoneNumber) + "',"
                                            + SOut.Int(smsFromMobile.MsgPart) + ","
                                            + SOut.Int(smsFromMobile.MsgTotal) + ","
                                            + "'" + SOut.String(smsFromMobile.MsgRefID) + "',"
                                            + SOut.Int((int) smsFromMobile.SmsStatus) + ","
                                            + "'" + SOut.String(smsFromMobile.Flags) + "',"
                                            + SOut.Bool(smsFromMobile.IsHidden) + ","
                                            + SOut.Int(smsFromMobile.MatchCount) + ","
                                            + "'" + SOut.String(smsFromMobile.GuidMessage) + "')";
        //SecDateTEdit can only be set by MySQL
        if (smsFromMobile.MsgText == null) smsFromMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsFromMobile.MsgText));
        {
            smsFromMobile.SmsFromMobileNum = Db.NonQ(command, true, "SmsFromMobileNum", "smsFromMobile", paramMsgText);
        }
        return smsFromMobile.SmsFromMobileNum;
    }

    public static long InsertNoCache(SmsFromMobile smsFromMobile)
    {
        return InsertNoCache(smsFromMobile, false);
    }

    public static long InsertNoCache(SmsFromMobile smsFromMobile, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO smsfrommobile (";
        if (isRandomKeys || useExistingPK) command += "SmsFromMobileNum,";
        command += "PatNum,ClinicNum,CommlogNum,MsgText,DateTimeReceived,SmsPhoneNumber,MobilePhoneNumber,MsgPart,MsgTotal,MsgRefID,SmsStatus,Flags,IsHidden,MatchCount,GuidMessage) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(smsFromMobile.SmsFromMobileNum) + ",";
        command +=
            SOut.Long(smsFromMobile.PatNum) + ","
                                            + SOut.Long(smsFromMobile.ClinicNum) + ","
                                            + SOut.Long(smsFromMobile.CommlogNum) + ","
                                            + DbHelper.ParamChar + "paramMsgText,"
                                            + SOut.DateT(smsFromMobile.DateTimeReceived) + ","
                                            + "'" + SOut.String(smsFromMobile.SmsPhoneNumber) + "',"
                                            + "'" + SOut.String(smsFromMobile.MobilePhoneNumber) + "',"
                                            + SOut.Int(smsFromMobile.MsgPart) + ","
                                            + SOut.Int(smsFromMobile.MsgTotal) + ","
                                            + "'" + SOut.String(smsFromMobile.MsgRefID) + "',"
                                            + SOut.Int((int) smsFromMobile.SmsStatus) + ","
                                            + "'" + SOut.String(smsFromMobile.Flags) + "',"
                                            + SOut.Bool(smsFromMobile.IsHidden) + ","
                                            + SOut.Int(smsFromMobile.MatchCount) + ","
                                            + "'" + SOut.String(smsFromMobile.GuidMessage) + "')";
        //SecDateTEdit can only be set by MySQL
        if (smsFromMobile.MsgText == null) smsFromMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsFromMobile.MsgText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMsgText);
        else
            smsFromMobile.SmsFromMobileNum = Db.NonQ(command, true, "SmsFromMobileNum", "smsFromMobile", paramMsgText);
        return smsFromMobile.SmsFromMobileNum;
    }

    public static void Update(SmsFromMobile smsFromMobile)
    {
        var command = "UPDATE smsfrommobile SET "
                      + "PatNum           =  " + SOut.Long(smsFromMobile.PatNum) + ", "
                      + "ClinicNum        =  " + SOut.Long(smsFromMobile.ClinicNum) + ", "
                      + "CommlogNum       =  " + SOut.Long(smsFromMobile.CommlogNum) + ", "
                      + "MsgText          =  " + DbHelper.ParamChar + "paramMsgText, "
                      + "DateTimeReceived =  " + SOut.DateT(smsFromMobile.DateTimeReceived) + ", "
                      + "SmsPhoneNumber   = '" + SOut.String(smsFromMobile.SmsPhoneNumber) + "', "
                      + "MobilePhoneNumber= '" + SOut.String(smsFromMobile.MobilePhoneNumber) + "', "
                      + "MsgPart          =  " + SOut.Int(smsFromMobile.MsgPart) + ", "
                      + "MsgTotal         =  " + SOut.Int(smsFromMobile.MsgTotal) + ", "
                      + "MsgRefID         = '" + SOut.String(smsFromMobile.MsgRefID) + "', "
                      + "SmsStatus        =  " + SOut.Int((int) smsFromMobile.SmsStatus) + ", "
                      + "Flags            = '" + SOut.String(smsFromMobile.Flags) + "', "
                      + "IsHidden         =  " + SOut.Bool(smsFromMobile.IsHidden) + ", "
                      + "MatchCount       =  " + SOut.Int(smsFromMobile.MatchCount) + ", "
                      + "GuidMessage      = '" + SOut.String(smsFromMobile.GuidMessage) + "' "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE SmsFromMobileNum = " + SOut.Long(smsFromMobile.SmsFromMobileNum);
        if (smsFromMobile.MsgText == null) smsFromMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsFromMobile.MsgText));
        Db.NonQ(command, paramMsgText);
    }

    public static bool Update(SmsFromMobile smsFromMobile, SmsFromMobile oldSmsFromMobile)
    {
        var command = "";
        if (smsFromMobile.PatNum != oldSmsFromMobile.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(smsFromMobile.PatNum) + "";
        }

        if (smsFromMobile.ClinicNum != oldSmsFromMobile.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(smsFromMobile.ClinicNum) + "";
        }

        if (smsFromMobile.CommlogNum != oldSmsFromMobile.CommlogNum)
        {
            if (command != "") command += ",";
            command += "CommlogNum = " + SOut.Long(smsFromMobile.CommlogNum) + "";
        }

        if (smsFromMobile.MsgText != oldSmsFromMobile.MsgText)
        {
            if (command != "") command += ",";
            command += "MsgText = " + DbHelper.ParamChar + "paramMsgText";
        }

        if (smsFromMobile.DateTimeReceived != oldSmsFromMobile.DateTimeReceived)
        {
            if (command != "") command += ",";
            command += "DateTimeReceived = " + SOut.DateT(smsFromMobile.DateTimeReceived) + "";
        }

        if (smsFromMobile.SmsPhoneNumber != oldSmsFromMobile.SmsPhoneNumber)
        {
            if (command != "") command += ",";
            command += "SmsPhoneNumber = '" + SOut.String(smsFromMobile.SmsPhoneNumber) + "'";
        }

        if (smsFromMobile.MobilePhoneNumber != oldSmsFromMobile.MobilePhoneNumber)
        {
            if (command != "") command += ",";
            command += "MobilePhoneNumber = '" + SOut.String(smsFromMobile.MobilePhoneNumber) + "'";
        }

        if (smsFromMobile.MsgPart != oldSmsFromMobile.MsgPart)
        {
            if (command != "") command += ",";
            command += "MsgPart = " + SOut.Int(smsFromMobile.MsgPart) + "";
        }

        if (smsFromMobile.MsgTotal != oldSmsFromMobile.MsgTotal)
        {
            if (command != "") command += ",";
            command += "MsgTotal = " + SOut.Int(smsFromMobile.MsgTotal) + "";
        }

        if (smsFromMobile.MsgRefID != oldSmsFromMobile.MsgRefID)
        {
            if (command != "") command += ",";
            command += "MsgRefID = '" + SOut.String(smsFromMobile.MsgRefID) + "'";
        }

        if (smsFromMobile.SmsStatus != oldSmsFromMobile.SmsStatus)
        {
            if (command != "") command += ",";
            command += "SmsStatus = " + SOut.Int((int) smsFromMobile.SmsStatus) + "";
        }

        if (smsFromMobile.Flags != oldSmsFromMobile.Flags)
        {
            if (command != "") command += ",";
            command += "Flags = '" + SOut.String(smsFromMobile.Flags) + "'";
        }

        if (smsFromMobile.IsHidden != oldSmsFromMobile.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(smsFromMobile.IsHidden) + "";
        }

        if (smsFromMobile.MatchCount != oldSmsFromMobile.MatchCount)
        {
            if (command != "") command += ",";
            command += "MatchCount = " + SOut.Int(smsFromMobile.MatchCount) + "";
        }

        if (smsFromMobile.GuidMessage != oldSmsFromMobile.GuidMessage)
        {
            if (command != "") command += ",";
            command += "GuidMessage = '" + SOut.String(smsFromMobile.GuidMessage) + "'";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (smsFromMobile.MsgText == null) smsFromMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsFromMobile.MsgText));
        command = "UPDATE smsfrommobile SET " + command
                                              + " WHERE SmsFromMobileNum = " + SOut.Long(smsFromMobile.SmsFromMobileNum);
        Db.NonQ(command, paramMsgText);
        return true;
    }

    public static bool UpdateComparison(SmsFromMobile smsFromMobile, SmsFromMobile oldSmsFromMobile)
    {
        if (smsFromMobile.PatNum != oldSmsFromMobile.PatNum) return true;
        if (smsFromMobile.ClinicNum != oldSmsFromMobile.ClinicNum) return true;
        if (smsFromMobile.CommlogNum != oldSmsFromMobile.CommlogNum) return true;
        if (smsFromMobile.MsgText != oldSmsFromMobile.MsgText) return true;
        if (smsFromMobile.DateTimeReceived != oldSmsFromMobile.DateTimeReceived) return true;
        if (smsFromMobile.SmsPhoneNumber != oldSmsFromMobile.SmsPhoneNumber) return true;
        if (smsFromMobile.MobilePhoneNumber != oldSmsFromMobile.MobilePhoneNumber) return true;
        if (smsFromMobile.MsgPart != oldSmsFromMobile.MsgPart) return true;
        if (smsFromMobile.MsgTotal != oldSmsFromMobile.MsgTotal) return true;
        if (smsFromMobile.MsgRefID != oldSmsFromMobile.MsgRefID) return true;
        if (smsFromMobile.SmsStatus != oldSmsFromMobile.SmsStatus) return true;
        if (smsFromMobile.Flags != oldSmsFromMobile.Flags) return true;
        if (smsFromMobile.IsHidden != oldSmsFromMobile.IsHidden) return true;
        if (smsFromMobile.MatchCount != oldSmsFromMobile.MatchCount) return true;
        if (smsFromMobile.GuidMessage != oldSmsFromMobile.GuidMessage) return true;
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long smsFromMobileNum)
    {
        var command = "DELETE FROM smsfrommobile "
                      + "WHERE SmsFromMobileNum = " + SOut.Long(smsFromMobileNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSmsFromMobileNums)
    {
        if (listSmsFromMobileNums == null || listSmsFromMobileNums.Count == 0) return;
        var command = "DELETE FROM smsfrommobile "
                      + "WHERE SmsFromMobileNum IN(" + string.Join(",", listSmsFromMobileNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}