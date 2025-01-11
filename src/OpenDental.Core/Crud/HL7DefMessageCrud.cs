#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HL7DefMessageCrud
{
    public static HL7DefMessage SelectOne(long hL7DefMessageNum)
    {
        var command = "SELECT * FROM hl7defmessage "
                      + "WHERE HL7DefMessageNum = " + SOut.Long(hL7DefMessageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HL7DefMessage SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        return Insert(hL7DefMessage, false);
    }

    public static long Insert(HL7DefMessage hL7DefMessage, bool useExistingPK)
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
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefMessage.Note));
        {
            hL7DefMessage.HL7DefMessageNum = Db.NonQ(command, true, "HL7DefMessageNum", "hL7DefMessage", paramNote);
        }
        return hL7DefMessage.HL7DefMessageNum;
    }

    public static long InsertNoCache(HL7DefMessage hL7DefMessage)
    {
        return InsertNoCache(hL7DefMessage, false);
    }

    public static long InsertNoCache(HL7DefMessage hL7DefMessage, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hl7defmessage (";
        if (isRandomKeys || useExistingPK) command += "HL7DefMessageNum,";
        command += "HL7DefNum,MessageType,EventType,InOrOut,ItemOrder,Note,MessageStructure) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hL7DefMessage.HL7DefMessageNum) + ",";
        command +=
            SOut.Long(hL7DefMessage.HL7DefNum) + ","
                                               + "'" + SOut.String(hL7DefMessage.MessageType.ToString()) + "',"
                                               + "'" + SOut.String(hL7DefMessage.EventType.ToString()) + "',"
                                               + SOut.Int((int) hL7DefMessage.InOrOut) + ","
                                               + SOut.Int(hL7DefMessage.ItemOrder) + ","
                                               + DbHelper.ParamChar + "paramNote,"
                                               + "'" + SOut.String(hL7DefMessage.MessageStructure.ToString()) + "')";
        if (hL7DefMessage.Note == null) hL7DefMessage.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefMessage.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            hL7DefMessage.HL7DefMessageNum = Db.NonQ(command, true, "HL7DefMessageNum", "hL7DefMessage", paramNote);
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
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefMessage.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(HL7DefMessage hL7DefMessage, HL7DefMessage oldHL7DefMessage)
    {
        var command = "";
        if (hL7DefMessage.HL7DefNum != oldHL7DefMessage.HL7DefNum)
        {
            if (command != "") command += ",";
            command += "HL7DefNum = " + SOut.Long(hL7DefMessage.HL7DefNum) + "";
        }

        if (hL7DefMessage.MessageType != oldHL7DefMessage.MessageType)
        {
            if (command != "") command += ",";
            command += "MessageType = '" + SOut.String(hL7DefMessage.MessageType.ToString()) + "'";
        }

        if (hL7DefMessage.EventType != oldHL7DefMessage.EventType)
        {
            if (command != "") command += ",";
            command += "EventType = '" + SOut.String(hL7DefMessage.EventType.ToString()) + "'";
        }

        if (hL7DefMessage.InOrOut != oldHL7DefMessage.InOrOut)
        {
            if (command != "") command += ",";
            command += "InOrOut = " + SOut.Int((int) hL7DefMessage.InOrOut) + "";
        }

        if (hL7DefMessage.ItemOrder != oldHL7DefMessage.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(hL7DefMessage.ItemOrder) + "";
        }

        if (hL7DefMessage.Note != oldHL7DefMessage.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (hL7DefMessage.MessageStructure != oldHL7DefMessage.MessageStructure)
        {
            if (command != "") command += ",";
            command += "MessageStructure = '" + SOut.String(hL7DefMessage.MessageStructure.ToString()) + "'";
        }

        if (command == "") return false;
        if (hL7DefMessage.Note == null) hL7DefMessage.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefMessage.Note));
        command = "UPDATE hl7defmessage SET " + command
                                              + " WHERE HL7DefMessageNum = " + SOut.Long(hL7DefMessage.HL7DefMessageNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(HL7DefMessage hL7DefMessage, HL7DefMessage oldHL7DefMessage)
    {
        if (hL7DefMessage.HL7DefNum != oldHL7DefMessage.HL7DefNum) return true;
        if (hL7DefMessage.MessageType != oldHL7DefMessage.MessageType) return true;
        if (hL7DefMessage.EventType != oldHL7DefMessage.EventType) return true;
        if (hL7DefMessage.InOrOut != oldHL7DefMessage.InOrOut) return true;
        if (hL7DefMessage.ItemOrder != oldHL7DefMessage.ItemOrder) return true;
        if (hL7DefMessage.Note != oldHL7DefMessage.Note) return true;
        if (hL7DefMessage.MessageStructure != oldHL7DefMessage.MessageStructure) return true;
        return false;
    }

    public static void Delete(long hL7DefMessageNum)
    {
        var command = "DELETE FROM hl7defmessage "
                      + "WHERE HL7DefMessageNum = " + SOut.Long(hL7DefMessageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHL7DefMessageNums)
    {
        if (listHL7DefMessageNums == null || listHL7DefMessageNums.Count == 0) return;
        var command = "DELETE FROM hl7defmessage "
                      + "WHERE HL7DefMessageNum IN(" + string.Join(",", listHL7DefMessageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}