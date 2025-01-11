#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MsgToPaySentCrud
{
    public static MsgToPaySent SelectOne(long msgToPaySentNum)
    {
        var command = "SELECT * FROM msgtopaysent "
                      + "WHERE MsgToPaySentNum = " + SOut.Long(msgToPaySentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MsgToPaySent SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MsgToPaySent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MsgToPaySent> TableToList(DataTable table)
    {
        var retVal = new List<MsgToPaySent>();
        MsgToPaySent msgToPaySent;
        foreach (DataRow row in table.Rows)
        {
            msgToPaySent = new MsgToPaySent();
            msgToPaySent.MsgToPaySentNum = SIn.Long(row["MsgToPaySentNum"].ToString());
            msgToPaySent.PatNum = SIn.Long(row["PatNum"].ToString());
            msgToPaySent.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            msgToPaySent.ApptNum = SIn.Long(row["ApptNum"].ToString());
            msgToPaySent.ApptDateTime = SIn.DateTime(row["ApptDateTime"].ToString());
            msgToPaySent.TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString()));
            msgToPaySent.StatementNum = SIn.Long(row["StatementNum"].ToString());
            msgToPaySent.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            msgToPaySent.Source = (MsgToPaySource) SIn.Int(row["Source"].ToString());
            msgToPaySent.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            msgToPaySent.MessageFk = SIn.Long(row["MessageFk"].ToString());
            msgToPaySent.Subject = SIn.String(row["Subject"].ToString());
            msgToPaySent.Message = SIn.String(row["Message"].ToString());
            msgToPaySent.EmailType = (EmailType) SIn.Int(row["EmailType"].ToString());
            msgToPaySent.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            msgToPaySent.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            msgToPaySent.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            msgToPaySent.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            msgToPaySent.ShortGUID = SIn.String(row["ShortGUID"].ToString());
            msgToPaySent.DateTimeSendFailed = SIn.DateTime(row["DateTimeSendFailed"].ToString());
            retVal.Add(msgToPaySent);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MsgToPaySent> listMsgToPaySents, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MsgToPaySent";
        var table = new DataTable(tableName);
        table.Columns.Add("MsgToPaySentNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ApptNum");
        table.Columns.Add("ApptDateTime");
        table.Columns.Add("TSPrior");
        table.Columns.Add("StatementNum");
        table.Columns.Add("SendStatus");
        table.Columns.Add("Source");
        table.Columns.Add("MessageType");
        table.Columns.Add("MessageFk");
        table.Columns.Add("Subject");
        table.Columns.Add("Message");
        table.Columns.Add("EmailType");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimeSent");
        table.Columns.Add("ResponseDescript");
        table.Columns.Add("ApptReminderRuleNum");
        table.Columns.Add("ShortGUID");
        table.Columns.Add("DateTimeSendFailed");
        foreach (var msgToPaySent in listMsgToPaySents)
            table.Rows.Add(SOut.Long(msgToPaySent.MsgToPaySentNum), SOut.Long(msgToPaySent.PatNum), SOut.Long(msgToPaySent.ClinicNum), SOut.Long(msgToPaySent.ApptNum), SOut.DateT(msgToPaySent.ApptDateTime, false), SOut.Long(msgToPaySent.TSPrior.Ticks), SOut.Long(msgToPaySent.StatementNum), SOut.Int((int) msgToPaySent.SendStatus), SOut.Int((int) msgToPaySent.Source), SOut.Int((int) msgToPaySent.MessageType), SOut.Long(msgToPaySent.MessageFk), msgToPaySent.Subject, msgToPaySent.Message, SOut.Int((int) msgToPaySent.EmailType), SOut.DateT(msgToPaySent.DateTimeEntry, false), SOut.DateT(msgToPaySent.DateTimeSent, false), msgToPaySent.ResponseDescript, SOut.Long(msgToPaySent.ApptReminderRuleNum), msgToPaySent.ShortGUID, SOut.DateT(msgToPaySent.DateTimeSendFailed, false));
        return table;
    }

    public static long Insert(MsgToPaySent msgToPaySent)
    {
        return Insert(msgToPaySent, false);
    }

    public static long Insert(MsgToPaySent msgToPaySent, bool useExistingPK)
    {
        var command = "INSERT INTO msgtopaysent (";

        command += "PatNum,ClinicNum,ApptNum,ApptDateTime,TSPrior,StatementNum,SendStatus,Source,MessageType,MessageFk,Subject,Message,EmailType,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ShortGUID,DateTimeSendFailed) VALUES(";

        command +=
            SOut.Long(msgToPaySent.PatNum) + ","
                                           + SOut.Long(msgToPaySent.ClinicNum) + ","
                                           + SOut.Long(msgToPaySent.ApptNum) + ","
                                           + SOut.DateT(msgToPaySent.ApptDateTime) + ","
                                           + "'" + SOut.Long(msgToPaySent.TSPrior.Ticks) + "',"
                                           + SOut.Long(msgToPaySent.StatementNum) + ","
                                           + SOut.Int((int) msgToPaySent.SendStatus) + ","
                                           + SOut.Int((int) msgToPaySent.Source) + ","
                                           + SOut.Int((int) msgToPaySent.MessageType) + ","
                                           + SOut.Long(msgToPaySent.MessageFk) + ","
                                           + DbHelper.ParamChar + "paramSubject,"
                                           + DbHelper.ParamChar + "paramMessage,"
                                           + SOut.Int((int) msgToPaySent.EmailType) + ","
                                           + DbHelper.Now() + ","
                                           + SOut.DateT(msgToPaySent.DateTimeSent) + ","
                                           + DbHelper.ParamChar + "paramResponseDescript,"
                                           + SOut.Long(msgToPaySent.ApptReminderRuleNum) + ","
                                           + "'" + SOut.String(msgToPaySent.ShortGUID) + "',"
                                           + SOut.DateT(msgToPaySent.DateTimeSendFailed) + ")";
        if (msgToPaySent.Subject == null) msgToPaySent.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(msgToPaySent.Subject));
        if (msgToPaySent.Message == null) msgToPaySent.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(msgToPaySent.Message));
        if (msgToPaySent.ResponseDescript == null) msgToPaySent.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(msgToPaySent.ResponseDescript));
        {
            msgToPaySent.MsgToPaySentNum = Db.NonQ(command, true, "MsgToPaySentNum", "msgToPaySent", paramSubject, paramMessage, paramResponseDescript);
        }
        return msgToPaySent.MsgToPaySentNum;
    }

    public static long InsertNoCache(MsgToPaySent msgToPaySent)
    {
        return InsertNoCache(msgToPaySent, false);
    }

    public static long InsertNoCache(MsgToPaySent msgToPaySent, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO msgtopaysent (";
        if (isRandomKeys || useExistingPK) command += "MsgToPaySentNum,";
        command += "PatNum,ClinicNum,ApptNum,ApptDateTime,TSPrior,StatementNum,SendStatus,Source,MessageType,MessageFk,Subject,Message,EmailType,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ShortGUID,DateTimeSendFailed) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(msgToPaySent.MsgToPaySentNum) + ",";
        command +=
            SOut.Long(msgToPaySent.PatNum) + ","
                                           + SOut.Long(msgToPaySent.ClinicNum) + ","
                                           + SOut.Long(msgToPaySent.ApptNum) + ","
                                           + SOut.DateT(msgToPaySent.ApptDateTime) + ","
                                           + "'" + SOut.Long(msgToPaySent.TSPrior.Ticks) + "',"
                                           + SOut.Long(msgToPaySent.StatementNum) + ","
                                           + SOut.Int((int) msgToPaySent.SendStatus) + ","
                                           + SOut.Int((int) msgToPaySent.Source) + ","
                                           + SOut.Int((int) msgToPaySent.MessageType) + ","
                                           + SOut.Long(msgToPaySent.MessageFk) + ","
                                           + DbHelper.ParamChar + "paramSubject,"
                                           + DbHelper.ParamChar + "paramMessage,"
                                           + SOut.Int((int) msgToPaySent.EmailType) + ","
                                           + DbHelper.Now() + ","
                                           + SOut.DateT(msgToPaySent.DateTimeSent) + ","
                                           + DbHelper.ParamChar + "paramResponseDescript,"
                                           + SOut.Long(msgToPaySent.ApptReminderRuleNum) + ","
                                           + "'" + SOut.String(msgToPaySent.ShortGUID) + "',"
                                           + SOut.DateT(msgToPaySent.DateTimeSendFailed) + ")";
        if (msgToPaySent.Subject == null) msgToPaySent.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(msgToPaySent.Subject));
        if (msgToPaySent.Message == null) msgToPaySent.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(msgToPaySent.Message));
        if (msgToPaySent.ResponseDescript == null) msgToPaySent.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(msgToPaySent.ResponseDescript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSubject, paramMessage, paramResponseDescript);
        else
            msgToPaySent.MsgToPaySentNum = Db.NonQ(command, true, "MsgToPaySentNum", "msgToPaySent", paramSubject, paramMessage, paramResponseDescript);
        return msgToPaySent.MsgToPaySentNum;
    }

    public static void Update(MsgToPaySent msgToPaySent)
    {
        var command = "UPDATE msgtopaysent SET "
                      + "PatNum             =  " + SOut.Long(msgToPaySent.PatNum) + ", "
                      + "ClinicNum          =  " + SOut.Long(msgToPaySent.ClinicNum) + ", "
                      + "ApptNum            =  " + SOut.Long(msgToPaySent.ApptNum) + ", "
                      + "ApptDateTime       =  " + SOut.DateT(msgToPaySent.ApptDateTime) + ", "
                      + "TSPrior            =  " + SOut.Long(msgToPaySent.TSPrior.Ticks) + ", "
                      + "StatementNum       =  " + SOut.Long(msgToPaySent.StatementNum) + ", "
                      + "SendStatus         =  " + SOut.Int((int) msgToPaySent.SendStatus) + ", "
                      + "Source             =  " + SOut.Int((int) msgToPaySent.Source) + ", "
                      + "MessageType        =  " + SOut.Int((int) msgToPaySent.MessageType) + ", "
                      + "MessageFk          =  " + SOut.Long(msgToPaySent.MessageFk) + ", "
                      + "Subject            =  " + DbHelper.ParamChar + "paramSubject, "
                      + "Message            =  " + DbHelper.ParamChar + "paramMessage, "
                      + "EmailType          =  " + SOut.Int((int) msgToPaySent.EmailType) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeSent       =  " + SOut.DateT(msgToPaySent.DateTimeSent) + ", "
                      + "ResponseDescript   =  " + DbHelper.ParamChar + "paramResponseDescript, "
                      + "ApptReminderRuleNum=  " + SOut.Long(msgToPaySent.ApptReminderRuleNum) + ", "
                      + "ShortGUID          = '" + SOut.String(msgToPaySent.ShortGUID) + "', "
                      + "DateTimeSendFailed =  " + SOut.DateT(msgToPaySent.DateTimeSendFailed) + " "
                      + "WHERE MsgToPaySentNum = " + SOut.Long(msgToPaySent.MsgToPaySentNum);
        if (msgToPaySent.Subject == null) msgToPaySent.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(msgToPaySent.Subject));
        if (msgToPaySent.Message == null) msgToPaySent.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(msgToPaySent.Message));
        if (msgToPaySent.ResponseDescript == null) msgToPaySent.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(msgToPaySent.ResponseDescript));
        Db.NonQ(command, paramSubject, paramMessage, paramResponseDescript);
    }

    public static bool Update(MsgToPaySent msgToPaySent, MsgToPaySent oldMsgToPaySent)
    {
        var command = "";
        if (msgToPaySent.PatNum != oldMsgToPaySent.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(msgToPaySent.PatNum) + "";
        }

        if (msgToPaySent.ClinicNum != oldMsgToPaySent.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(msgToPaySent.ClinicNum) + "";
        }

        if (msgToPaySent.ApptNum != oldMsgToPaySent.ApptNum)
        {
            if (command != "") command += ",";
            command += "ApptNum = " + SOut.Long(msgToPaySent.ApptNum) + "";
        }

        if (msgToPaySent.ApptDateTime != oldMsgToPaySent.ApptDateTime)
        {
            if (command != "") command += ",";
            command += "ApptDateTime = " + SOut.DateT(msgToPaySent.ApptDateTime) + "";
        }

        if (msgToPaySent.TSPrior != oldMsgToPaySent.TSPrior)
        {
            if (command != "") command += ",";
            command += "TSPrior = '" + SOut.Long(msgToPaySent.TSPrior.Ticks) + "'";
        }

        if (msgToPaySent.StatementNum != oldMsgToPaySent.StatementNum)
        {
            if (command != "") command += ",";
            command += "StatementNum = " + SOut.Long(msgToPaySent.StatementNum) + "";
        }

        if (msgToPaySent.SendStatus != oldMsgToPaySent.SendStatus)
        {
            if (command != "") command += ",";
            command += "SendStatus = " + SOut.Int((int) msgToPaySent.SendStatus) + "";
        }

        if (msgToPaySent.Source != oldMsgToPaySent.Source)
        {
            if (command != "") command += ",";
            command += "Source = " + SOut.Int((int) msgToPaySent.Source) + "";
        }

        if (msgToPaySent.MessageType != oldMsgToPaySent.MessageType)
        {
            if (command != "") command += ",";
            command += "MessageType = " + SOut.Int((int) msgToPaySent.MessageType) + "";
        }

        if (msgToPaySent.MessageFk != oldMsgToPaySent.MessageFk)
        {
            if (command != "") command += ",";
            command += "MessageFk = " + SOut.Long(msgToPaySent.MessageFk) + "";
        }

        if (msgToPaySent.Subject != oldMsgToPaySent.Subject)
        {
            if (command != "") command += ",";
            command += "Subject = " + DbHelper.ParamChar + "paramSubject";
        }

        if (msgToPaySent.Message != oldMsgToPaySent.Message)
        {
            if (command != "") command += ",";
            command += "Message = " + DbHelper.ParamChar + "paramMessage";
        }

        if (msgToPaySent.EmailType != oldMsgToPaySent.EmailType)
        {
            if (command != "") command += ",";
            command += "EmailType = " + SOut.Int((int) msgToPaySent.EmailType) + "";
        }

        //DateTimeEntry not allowed to change
        if (msgToPaySent.DateTimeSent != oldMsgToPaySent.DateTimeSent)
        {
            if (command != "") command += ",";
            command += "DateTimeSent = " + SOut.DateT(msgToPaySent.DateTimeSent) + "";
        }

        if (msgToPaySent.ResponseDescript != oldMsgToPaySent.ResponseDescript)
        {
            if (command != "") command += ",";
            command += "ResponseDescript = " + DbHelper.ParamChar + "paramResponseDescript";
        }

        if (msgToPaySent.ApptReminderRuleNum != oldMsgToPaySent.ApptReminderRuleNum)
        {
            if (command != "") command += ",";
            command += "ApptReminderRuleNum = " + SOut.Long(msgToPaySent.ApptReminderRuleNum) + "";
        }

        if (msgToPaySent.ShortGUID != oldMsgToPaySent.ShortGUID)
        {
            if (command != "") command += ",";
            command += "ShortGUID = '" + SOut.String(msgToPaySent.ShortGUID) + "'";
        }

        if (msgToPaySent.DateTimeSendFailed != oldMsgToPaySent.DateTimeSendFailed)
        {
            if (command != "") command += ",";
            command += "DateTimeSendFailed = " + SOut.DateT(msgToPaySent.DateTimeSendFailed) + "";
        }

        if (command == "") return false;
        if (msgToPaySent.Subject == null) msgToPaySent.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(msgToPaySent.Subject));
        if (msgToPaySent.Message == null) msgToPaySent.Message = "";
        var paramMessage = new OdSqlParameter("paramMessage", OdDbType.Text, SOut.StringParam(msgToPaySent.Message));
        if (msgToPaySent.ResponseDescript == null) msgToPaySent.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(msgToPaySent.ResponseDescript));
        command = "UPDATE msgtopaysent SET " + command
                                             + " WHERE MsgToPaySentNum = " + SOut.Long(msgToPaySent.MsgToPaySentNum);
        Db.NonQ(command, paramSubject, paramMessage, paramResponseDescript);
        return true;
    }

    public static bool UpdateComparison(MsgToPaySent msgToPaySent, MsgToPaySent oldMsgToPaySent)
    {
        if (msgToPaySent.PatNum != oldMsgToPaySent.PatNum) return true;
        if (msgToPaySent.ClinicNum != oldMsgToPaySent.ClinicNum) return true;
        if (msgToPaySent.ApptNum != oldMsgToPaySent.ApptNum) return true;
        if (msgToPaySent.ApptDateTime != oldMsgToPaySent.ApptDateTime) return true;
        if (msgToPaySent.TSPrior != oldMsgToPaySent.TSPrior) return true;
        if (msgToPaySent.StatementNum != oldMsgToPaySent.StatementNum) return true;
        if (msgToPaySent.SendStatus != oldMsgToPaySent.SendStatus) return true;
        if (msgToPaySent.Source != oldMsgToPaySent.Source) return true;
        if (msgToPaySent.MessageType != oldMsgToPaySent.MessageType) return true;
        if (msgToPaySent.MessageFk != oldMsgToPaySent.MessageFk) return true;
        if (msgToPaySent.Subject != oldMsgToPaySent.Subject) return true;
        if (msgToPaySent.Message != oldMsgToPaySent.Message) return true;
        if (msgToPaySent.EmailType != oldMsgToPaySent.EmailType) return true;
        //DateTimeEntry not allowed to change
        if (msgToPaySent.DateTimeSent != oldMsgToPaySent.DateTimeSent) return true;
        if (msgToPaySent.ResponseDescript != oldMsgToPaySent.ResponseDescript) return true;
        if (msgToPaySent.ApptReminderRuleNum != oldMsgToPaySent.ApptReminderRuleNum) return true;
        if (msgToPaySent.ShortGUID != oldMsgToPaySent.ShortGUID) return true;
        if (msgToPaySent.DateTimeSendFailed != oldMsgToPaySent.DateTimeSendFailed) return true;
        return false;
    }

    public static void Delete(long msgToPaySentNum)
    {
        var command = "DELETE FROM msgtopaysent "
                      + "WHERE MsgToPaySentNum = " + SOut.Long(msgToPaySentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMsgToPaySentNums)
    {
        if (listMsgToPaySentNums == null || listMsgToPaySentNums.Count == 0) return;
        var command = "DELETE FROM msgtopaysent "
                      + "WHERE MsgToPaySentNum IN(" + string.Join(",", listMsgToPaySentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}