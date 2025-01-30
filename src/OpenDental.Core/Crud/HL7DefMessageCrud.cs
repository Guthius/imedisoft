using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HL7DefMessageCrud
{
    public static List<HL7DefMessage> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HL7DefMessage> TableToList(DataTable table)
    {
        var retVal = new List<HL7DefMessage>();
        HL7DefMessage hL7DefMessage;
        foreach (DataRow row in table.Rows)
        {
            hL7DefMessage = new HL7DefMessage();
            hL7DefMessage.HL7DefMessageNum = SIn.Long(row["HL7DefMessageNum"].ToString());
            hL7DefMessage.HL7DefNum = SIn.Long(row["HL7DefNum"].ToString());
            var messageType = row["MessageType"].ToString();
            if (messageType == "")
                hL7DefMessage.MessageType = 0;
            else
                try
                {
                    hL7DefMessage.MessageType = (MessageTypeHL7) Enum.Parse(typeof(MessageTypeHL7), messageType);
                }
                catch
                {
                    hL7DefMessage.MessageType = 0;
                }

            var eventType = row["EventType"].ToString();
            if (eventType == "")
                hL7DefMessage.EventType = 0;
            else
                try
                {
                    hL7DefMessage.EventType = (EventTypeHL7) Enum.Parse(typeof(EventTypeHL7), eventType);
                }
                catch
                {
                    hL7DefMessage.EventType = 0;
                }

            hL7DefMessage.InOrOut = (InOutHL7) SIn.Int(row["InOrOut"].ToString());
            hL7DefMessage.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            hL7DefMessage.Note = SIn.String(row["Note"].ToString());
            var messageStructure = row["MessageStructure"].ToString();
            if (messageStructure == "")
                hL7DefMessage.MessageStructure = 0;
            else
                try
                {
                    hL7DefMessage.MessageStructure = (MessageStructureHL7) Enum.Parse(typeof(MessageStructureHL7), messageStructure);
                }
                catch
                {
                    hL7DefMessage.MessageStructure = 0;
                }

            retVal.Add(hL7DefMessage);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HL7DefMessage> listHL7DefMessages, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HL7DefMessage";
        var table = new DataTable(tableName);
        table.Columns.Add("HL7DefMessageNum");
        table.Columns.Add("HL7DefNum");
        table.Columns.Add("MessageType");
        table.Columns.Add("EventType");
        table.Columns.Add("InOrOut");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Note");
        table.Columns.Add("MessageStructure");
        foreach (var hL7DefMessage in listHL7DefMessages)
            table.Rows.Add(SOut.Long(hL7DefMessage.HL7DefMessageNum), SOut.Long(hL7DefMessage.HL7DefNum), SOut.Int((int) hL7DefMessage.MessageType), SOut.Int((int) hL7DefMessage.EventType), SOut.Int((int) hL7DefMessage.InOrOut), SOut.Int(hL7DefMessage.ItemOrder), hL7DefMessage.Note, SOut.Int((int) hL7DefMessage.MessageStructure));
        return table;
    }

    public static long Insert(HL7DefMessage hL7DefMessage)
    {
        var command = "INSERT INTO hl7defmessage (";

        command += "HL7DefNum,MessageType,EventType,InOrOut,ItemOrder,Note,MessageStructure) VALUES(";

        command +=
            SOut.Long(hL7DefMessage.HL7DefNum) + ","
                                               + "'" + SOut.String(hL7DefMessage.MessageType.ToString()) + "',"
                                               + "'" + SOut.String(hL7DefMessage.EventType.ToString()) + "',"
                                               + SOut.Int((int) hL7DefMessage.InOrOut) + ","
                                               + SOut.Int(hL7DefMessage.ItemOrder) + ","
                                               + DbHelper.ParamChar + "paramNote,"
                                               + "'" + SOut.String(hL7DefMessage.MessageStructure.ToString()) + "')";
        if (hL7DefMessage.Note == null) hL7DefMessage.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(hL7DefMessage.Note));
        {
            hL7DefMessage.HL7DefMessageNum = Db.NonQ(command, true, "HL7DefMessageNum", "hL7DefMessage", paramNote);
        }
        return hL7DefMessage.HL7DefMessageNum;
    }

    public static void Update(HL7DefMessage hL7DefMessage)
    {
        var command = "UPDATE hl7defmessage SET "
                      + "HL7DefNum       =  " + SOut.Long(hL7DefMessage.HL7DefNum) + ", "
                      + "MessageType     = '" + SOut.String(hL7DefMessage.MessageType.ToString()) + "', "
                      + "EventType       = '" + SOut.String(hL7DefMessage.EventType.ToString()) + "', "
                      + "InOrOut         =  " + SOut.Int((int) hL7DefMessage.InOrOut) + ", "
                      + "ItemOrder       =  " + SOut.Int(hL7DefMessage.ItemOrder) + ", "
                      + "Note            =  " + DbHelper.ParamChar + "paramNote, "
                      + "MessageStructure= '" + SOut.String(hL7DefMessage.MessageStructure.ToString()) + "' "
                      + "WHERE HL7DefMessageNum = " + SOut.Long(hL7DefMessage.HL7DefMessageNum);
        if (hL7DefMessage.Note == null) hL7DefMessage.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(hL7DefMessage.Note));
        Db.NonQ(command, paramNote);
    }
}