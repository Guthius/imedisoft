#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HL7MsgCrud
{
    public static HL7Msg SelectOne(long hL7MsgNum)
    {
        var command = "SELECT * FROM hl7msg "
                      + "WHERE HL7MsgNum = " + SOut.Long(hL7MsgNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HL7Msg SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HL7Msg> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HL7Msg> TableToList(DataTable table)
    {
        var retVal = new List<HL7Msg>();
        HL7Msg hL7Msg;
        foreach (DataRow row in table.Rows)
        {
            hL7Msg = new HL7Msg();
            hL7Msg.HL7MsgNum = SIn.Long(row["HL7MsgNum"].ToString());
            hL7Msg.HL7Status = (HL7MessageStatus) SIn.Int(row["HL7Status"].ToString());
            hL7Msg.MsgText = SIn.String(row["MsgText"].ToString());
            hL7Msg.AptNum = SIn.Long(row["AptNum"].ToString());
            hL7Msg.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            hL7Msg.PatNum = SIn.Long(row["PatNum"].ToString());
            hL7Msg.Note = SIn.String(row["Note"].ToString());
            retVal.Add(hL7Msg);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HL7Msg> listHL7Msgs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HL7Msg";
        var table = new DataTable(tableName);
        table.Columns.Add("HL7MsgNum");
        table.Columns.Add("HL7Status");
        table.Columns.Add("MsgText");
        table.Columns.Add("AptNum");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("PatNum");
        table.Columns.Add("Note");
        foreach (var hL7Msg in listHL7Msgs)
            table.Rows.Add(SOut.Long(hL7Msg.HL7MsgNum), SOut.Int((int) hL7Msg.HL7Status), hL7Msg.MsgText, SOut.Long(hL7Msg.AptNum), SOut.DateT(hL7Msg.DateTStamp, false), SOut.Long(hL7Msg.PatNum), hL7Msg.Note);
        return table;
    }

    public static long Insert(HL7Msg hL7Msg)
    {
        return Insert(hL7Msg, false);
    }

    public static long Insert(HL7Msg hL7Msg, bool useExistingPK)
    {
        var command = "INSERT INTO hl7msg (";

        command += "HL7Status,MsgText,AptNum,PatNum,Note) VALUES(";

        command +=
            SOut.Int((int) hL7Msg.HL7Status) + ","
                                             + DbHelper.ParamChar + "paramMsgText,"
                                             + SOut.Long(hL7Msg.AptNum) + ","
                                             //DateTStamp can only be set by MySQL
                                             + SOut.Long(hL7Msg.PatNum) + ","
                                             + DbHelper.ParamChar + "paramNote)";
        if (hL7Msg.MsgText == null) hL7Msg.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(hL7Msg.MsgText));
        if (hL7Msg.Note == null) hL7Msg.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Msg.Note));
        {
            hL7Msg.HL7MsgNum = Db.NonQ(command, true, "HL7MsgNum", "hL7Msg", paramMsgText, paramNote);
        }
        return hL7Msg.HL7MsgNum;
    }

    public static long InsertNoCache(HL7Msg hL7Msg)
    {
        return InsertNoCache(hL7Msg, false);
    }

    public static long InsertNoCache(HL7Msg hL7Msg, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hl7msg (";
        if (isRandomKeys || useExistingPK) command += "HL7MsgNum,";
        command += "HL7Status,MsgText,AptNum,PatNum,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hL7Msg.HL7MsgNum) + ",";
        command +=
            SOut.Int((int) hL7Msg.HL7Status) + ","
                                             + DbHelper.ParamChar + "paramMsgText,"
                                             + SOut.Long(hL7Msg.AptNum) + ","
                                             //DateTStamp can only be set by MySQL
                                             + SOut.Long(hL7Msg.PatNum) + ","
                                             + DbHelper.ParamChar + "paramNote)";
        if (hL7Msg.MsgText == null) hL7Msg.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(hL7Msg.MsgText));
        if (hL7Msg.Note == null) hL7Msg.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Msg.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMsgText, paramNote);
        else
            hL7Msg.HL7MsgNum = Db.NonQ(command, true, "HL7MsgNum", "hL7Msg", paramMsgText, paramNote);
        return hL7Msg.HL7MsgNum;
    }

    public static void Update(HL7Msg hL7Msg)
    {
        var command = "UPDATE hl7msg SET "
                      + "HL7Status =  " + SOut.Int((int) hL7Msg.HL7Status) + ", "
                      + "MsgText   =  " + DbHelper.ParamChar + "paramMsgText, "
                      + "AptNum    =  " + SOut.Long(hL7Msg.AptNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "PatNum    =  " + SOut.Long(hL7Msg.PatNum) + ", "
                      + "Note      =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE HL7MsgNum = " + SOut.Long(hL7Msg.HL7MsgNum);
        if (hL7Msg.MsgText == null) hL7Msg.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(hL7Msg.MsgText));
        if (hL7Msg.Note == null) hL7Msg.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Msg.Note));
        Db.NonQ(command, paramMsgText, paramNote);
    }

    public static bool Update(HL7Msg hL7Msg, HL7Msg oldHL7Msg)
    {
        var command = "";
        if (hL7Msg.HL7Status != oldHL7Msg.HL7Status)
        {
            if (command != "") command += ",";
            command += "HL7Status = " + SOut.Int((int) hL7Msg.HL7Status) + "";
        }

        if (hL7Msg.MsgText != oldHL7Msg.MsgText)
        {
            if (command != "") command += ",";
            command += "MsgText = " + DbHelper.ParamChar + "paramMsgText";
        }

        if (hL7Msg.AptNum != oldHL7Msg.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(hL7Msg.AptNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (hL7Msg.PatNum != oldHL7Msg.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(hL7Msg.PatNum) + "";
        }

        if (hL7Msg.Note != oldHL7Msg.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (hL7Msg.MsgText == null) hL7Msg.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringParam(hL7Msg.MsgText));
        if (hL7Msg.Note == null) hL7Msg.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7Msg.Note));
        command = "UPDATE hl7msg SET " + command
                                       + " WHERE HL7MsgNum = " + SOut.Long(hL7Msg.HL7MsgNum);
        Db.NonQ(command, paramMsgText, paramNote);
        return true;
    }

    public static bool UpdateComparison(HL7Msg hL7Msg, HL7Msg oldHL7Msg)
    {
        if (hL7Msg.HL7Status != oldHL7Msg.HL7Status) return true;
        if (hL7Msg.MsgText != oldHL7Msg.MsgText) return true;
        if (hL7Msg.AptNum != oldHL7Msg.AptNum) return true;
        //DateTStamp can only be set by MySQL
        if (hL7Msg.PatNum != oldHL7Msg.PatNum) return true;
        if (hL7Msg.Note != oldHL7Msg.Note) return true;
        return false;
    }

    public static void Delete(long hL7MsgNum)
    {
        var command = "DELETE FROM hl7msg "
                      + "WHERE HL7MsgNum = " + SOut.Long(hL7MsgNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHL7MsgNums)
    {
        if (listHL7MsgNums == null || listHL7MsgNums.Count == 0) return;
        var command = "DELETE FROM hl7msg "
                      + "WHERE HL7MsgNum IN(" + string.Join(",", listHL7MsgNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}