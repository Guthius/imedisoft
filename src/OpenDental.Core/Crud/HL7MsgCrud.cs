using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HL7MsgCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var hL7Msg = new HL7Msg
            {
                HL7MsgNum = SIn.Long(row["HL7MsgNum"].ToString()),
                HL7Status = (HL7MessageStatus) SIn.Int(row["HL7Status"].ToString()),
                MsgText = SIn.String(row["MsgText"].ToString()),
                AptNum = SIn.Long(row["AptNum"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                Note = SIn.String(row["Note"].ToString())
            };
            retVal.Add(hL7Msg);
        }

        return retVal;
    }

    public static long Insert(HL7Msg hL7Msg)
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
        var paramMsgText = new OdSqlParameter("paramMsgText", SOut.StringParam(hL7Msg.MsgText));
        if (hL7Msg.Note == null) hL7Msg.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(hL7Msg.Note));
        {
            hL7Msg.HL7MsgNum = Db.NonQ(command, true, "HL7MsgNum", "hL7Msg", paramMsgText, paramNote);
        }
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
        var paramMsgText = new OdSqlParameter("paramMsgText", SOut.StringParam(hL7Msg.MsgText));
        if (hL7Msg.Note == null) hL7Msg.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(hL7Msg.Note));
        Db.NonQ(command, paramMsgText, paramNote);
    }
}