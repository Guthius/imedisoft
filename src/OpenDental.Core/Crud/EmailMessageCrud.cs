#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailMessageCrud
{
    public static EmailMessage SelectOne(long emailMessageNum)
    {
        var command = "SELECT * FROM emailmessage "
                      + "WHERE EmailMessageNum = " + SOut.Long(emailMessageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailMessage SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailMessage> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailMessage> TableToList(DataTable table)
    {
        var retVal = new List<EmailMessage>();
        EmailMessage emailMessage;
        foreach (DataRow row in table.Rows)
        {
            emailMessage = new EmailMessage();
            emailMessage.EmailMessageNum = SIn.Long(row["EmailMessageNum"].ToString());
            emailMessage.PatNum = SIn.Long(row["PatNum"].ToString());
            emailMessage.ToAddress = SIn.String(row["ToAddress"].ToString());
            emailMessage.FromAddress = SIn.String(row["FromAddress"].ToString());
            emailMessage.Subject = SIn.String(row["Subject"].ToString());
            emailMessage.BodyText = SIn.String(row["BodyText"].ToString());
            emailMessage.MsgDateTime = SIn.DateTime(row["MsgDateTime"].ToString());
            emailMessage.SentOrReceived = (EmailSentOrReceived) SIn.Int(row["SentOrReceived"].ToString());
            emailMessage.RecipientAddress = SIn.String(row["RecipientAddress"].ToString());
            emailMessage.RawEmailIn = SIn.String(row["RawEmailIn"].ToString());
            emailMessage.ProvNumWebMail = SIn.Long(row["ProvNumWebMail"].ToString());
            emailMessage.PatNumSubj = SIn.Long(row["PatNumSubj"].ToString());
            emailMessage.CcAddress = SIn.String(row["CcAddress"].ToString());
            emailMessage.BccAddress = SIn.String(row["BccAddress"].ToString());
            emailMessage.HideIn = (HideInFlags) SIn.Int(row["HideIn"].ToString());
            emailMessage.AptNum = SIn.Long(row["AptNum"].ToString());
            emailMessage.UserNum = SIn.Long(row["UserNum"].ToString());
            emailMessage.HtmlType = (EmailType) SIn.Int(row["HtmlType"].ToString());
            emailMessage.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            emailMessage.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            var msgType = row["MsgType"].ToString();
            if (msgType == "")
                emailMessage.MsgType = 0;
            else
                try
                {
                    emailMessage.MsgType = (EmailMessageSource) Enum.Parse(typeof(EmailMessageSource), msgType);
                }
                catch
                {
                    emailMessage.MsgType = 0;
                }

            emailMessage.FailReason = SIn.String(row["FailReason"].ToString());
            retVal.Add(emailMessage);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailMessage> listEmailMessages, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailMessage";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailMessageNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ToAddress");
        table.Columns.Add("FromAddress");
        table.Columns.Add("Subject");
        table.Columns.Add("BodyText");
        table.Columns.Add("MsgDateTime");
        table.Columns.Add("SentOrReceived");
        table.Columns.Add("RecipientAddress");
        table.Columns.Add("RawEmailIn");
        table.Columns.Add("ProvNumWebMail");
        table.Columns.Add("PatNumSubj");
        table.Columns.Add("CcAddress");
        table.Columns.Add("BccAddress");
        table.Columns.Add("HideIn");
        table.Columns.Add("AptNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("HtmlType");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("MsgType");
        table.Columns.Add("FailReason");
        foreach (var emailMessage in listEmailMessages)
            table.Rows.Add(SOut.Long(emailMessage.EmailMessageNum), SOut.Long(emailMessage.PatNum), emailMessage.ToAddress, emailMessage.FromAddress, emailMessage.Subject, emailMessage.BodyText, SOut.DateT(emailMessage.MsgDateTime, false), SOut.Int((int) emailMessage.SentOrReceived), emailMessage.RecipientAddress, emailMessage.RawEmailIn, SOut.Long(emailMessage.ProvNumWebMail), SOut.Long(emailMessage.PatNumSubj), emailMessage.CcAddress, emailMessage.BccAddress, SOut.Int((int) emailMessage.HideIn), SOut.Long(emailMessage.AptNum), SOut.Long(emailMessage.UserNum), SOut.Int((int) emailMessage.HtmlType), SOut.DateT(emailMessage.SecDateTEntry, false), SOut.DateT(emailMessage.SecDateTEdit, false), SOut.Int((int) emailMessage.MsgType), emailMessage.FailReason);
        return table;
    }

    public static long Insert(EmailMessage emailMessage)
    {
        return Insert(emailMessage, false);
    }

    public static long Insert(EmailMessage emailMessage, bool useExistingPK)
    {
        var command = "INSERT INTO emailmessage (";

        command += "PatNum,ToAddress,FromAddress,Subject,BodyText,MsgDateTime,SentOrReceived,RecipientAddress,RawEmailIn,ProvNumWebMail,PatNumSubj,CcAddress,BccAddress,HideIn,AptNum,UserNum,HtmlType,SecDateTEntry,MsgType,FailReason) VALUES(";

        command +=
            SOut.Long(emailMessage.PatNum) + ","
                                           + DbHelper.ParamChar + "paramToAddress,"
                                           + DbHelper.ParamChar + "paramFromAddress,"
                                           + DbHelper.ParamChar + "paramSubject,"
                                           + DbHelper.ParamChar + "paramBodyText,"
                                           + SOut.DateT(emailMessage.MsgDateTime) + ","
                                           + SOut.Int((int) emailMessage.SentOrReceived) + ","
                                           + "'" + SOut.String(emailMessage.RecipientAddress) + "',"
                                           + DbHelper.ParamChar + "paramRawEmailIn,"
                                           + SOut.Long(emailMessage.ProvNumWebMail) + ","
                                           + SOut.Long(emailMessage.PatNumSubj) + ","
                                           + DbHelper.ParamChar + "paramCcAddress,"
                                           + DbHelper.ParamChar + "paramBccAddress,"
                                           + SOut.Int((int) emailMessage.HideIn) + ","
                                           + SOut.Long(emailMessage.AptNum) + ","
                                           + SOut.Long(emailMessage.UserNum) + ","
                                           + SOut.Int((int) emailMessage.HtmlType) + ","
                                           + DbHelper.Now() + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + "'" + SOut.String(emailMessage.MsgType.ToString()) + "',"
                                           + "'" + SOut.String(emailMessage.FailReason) + "')";
        if (emailMessage.ToAddress == null) emailMessage.ToAddress = "";
        var paramToAddress = new OdSqlParameter("paramToAddress", OdDbType.Text, SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", OdDbType.Text, SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", OdDbType.Text, SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", OdDbType.Text, SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", OdDbType.Text, SOut.StringParam(emailMessage.BccAddress));
        {
            emailMessage.EmailMessageNum = Db.NonQ(command, true, "EmailMessageNum", "emailMessage", paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
        }
        return emailMessage.EmailMessageNum;
    }

    public static long InsertNoCache(EmailMessage emailMessage)
    {
        return InsertNoCache(emailMessage, false);
    }

    public static long InsertNoCache(EmailMessage emailMessage, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailmessage (";
        if (isRandomKeys || useExistingPK) command += "EmailMessageNum,";
        command += "PatNum,ToAddress,FromAddress,Subject,BodyText,MsgDateTime,SentOrReceived,RecipientAddress,RawEmailIn,ProvNumWebMail,PatNumSubj,CcAddress,BccAddress,HideIn,AptNum,UserNum,HtmlType,SecDateTEntry,MsgType,FailReason) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailMessage.EmailMessageNum) + ",";
        command +=
            SOut.Long(emailMessage.PatNum) + ","
                                           + DbHelper.ParamChar + "paramToAddress,"
                                           + DbHelper.ParamChar + "paramFromAddress,"
                                           + DbHelper.ParamChar + "paramSubject,"
                                           + DbHelper.ParamChar + "paramBodyText,"
                                           + SOut.DateT(emailMessage.MsgDateTime) + ","
                                           + SOut.Int((int) emailMessage.SentOrReceived) + ","
                                           + "'" + SOut.String(emailMessage.RecipientAddress) + "',"
                                           + DbHelper.ParamChar + "paramRawEmailIn,"
                                           + SOut.Long(emailMessage.ProvNumWebMail) + ","
                                           + SOut.Long(emailMessage.PatNumSubj) + ","
                                           + DbHelper.ParamChar + "paramCcAddress,"
                                           + DbHelper.ParamChar + "paramBccAddress,"
                                           + SOut.Int((int) emailMessage.HideIn) + ","
                                           + SOut.Long(emailMessage.AptNum) + ","
                                           + SOut.Long(emailMessage.UserNum) + ","
                                           + SOut.Int((int) emailMessage.HtmlType) + ","
                                           + DbHelper.Now() + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + "'" + SOut.String(emailMessage.MsgType.ToString()) + "',"
                                           + "'" + SOut.String(emailMessage.FailReason) + "')";
        if (emailMessage.ToAddress == null) emailMessage.ToAddress = "";
        var paramToAddress = new OdSqlParameter("paramToAddress", OdDbType.Text, SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", OdDbType.Text, SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", OdDbType.Text, SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", OdDbType.Text, SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", OdDbType.Text, SOut.StringParam(emailMessage.BccAddress));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
        else
            emailMessage.EmailMessageNum = Db.NonQ(command, true, "EmailMessageNum", "emailMessage", paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
        return emailMessage.EmailMessageNum;
    }

    public static void Update(EmailMessage emailMessage)
    {
        var command = "UPDATE emailmessage SET "
                      + "PatNum          =  " + SOut.Long(emailMessage.PatNum) + ", "
                      + "ToAddress       =  " + DbHelper.ParamChar + "paramToAddress, "
                      + "FromAddress     =  " + DbHelper.ParamChar + "paramFromAddress, "
                      + "Subject         =  " + DbHelper.ParamChar + "paramSubject, "
                      + "BodyText        =  " + DbHelper.ParamChar + "paramBodyText, "
                      + "MsgDateTime     =  " + SOut.DateT(emailMessage.MsgDateTime) + ", "
                      + "SentOrReceived  =  " + SOut.Int((int) emailMessage.SentOrReceived) + ", "
                      + "RecipientAddress= '" + SOut.String(emailMessage.RecipientAddress) + "', "
                      + "RawEmailIn      =  " + DbHelper.ParamChar + "paramRawEmailIn, "
                      + "ProvNumWebMail  =  " + SOut.Long(emailMessage.ProvNumWebMail) + ", "
                      + "PatNumSubj      =  " + SOut.Long(emailMessage.PatNumSubj) + ", "
                      + "CcAddress       =  " + DbHelper.ParamChar + "paramCcAddress, "
                      + "BccAddress      =  " + DbHelper.ParamChar + "paramBccAddress, "
                      + "HideIn          =  " + SOut.Int((int) emailMessage.HideIn) + ", "
                      + "AptNum          =  " + SOut.Long(emailMessage.AptNum) + ", "
                      + "UserNum         =  " + SOut.Long(emailMessage.UserNum) + ", "
                      + "HtmlType        =  " + SOut.Int((int) emailMessage.HtmlType) + ", "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "MsgType         = '" + SOut.String(emailMessage.MsgType.ToString()) + "', "
                      + "FailReason      = '" + SOut.String(emailMessage.FailReason) + "' "
                      + "WHERE EmailMessageNum = " + SOut.Long(emailMessage.EmailMessageNum);
        if (emailMessage.ToAddress == null) emailMessage.ToAddress = "";
        var paramToAddress = new OdSqlParameter("paramToAddress", OdDbType.Text, SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", OdDbType.Text, SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", OdDbType.Text, SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", OdDbType.Text, SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", OdDbType.Text, SOut.StringParam(emailMessage.BccAddress));
        Db.NonQ(command, paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
    }

    public static bool Update(EmailMessage emailMessage, EmailMessage oldEmailMessage)
    {
        var command = "";
        if (emailMessage.PatNum != oldEmailMessage.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(emailMessage.PatNum) + "";
        }

        if (emailMessage.ToAddress != oldEmailMessage.ToAddress)
        {
            if (command != "") command += ",";
            command += "ToAddress = " + DbHelper.ParamChar + "paramToAddress";
        }

        if (emailMessage.FromAddress != oldEmailMessage.FromAddress)
        {
            if (command != "") command += ",";
            command += "FromAddress = " + DbHelper.ParamChar + "paramFromAddress";
        }

        if (emailMessage.Subject != oldEmailMessage.Subject)
        {
            if (command != "") command += ",";
            command += "Subject = " + DbHelper.ParamChar + "paramSubject";
        }

        if (emailMessage.BodyText != oldEmailMessage.BodyText)
        {
            if (command != "") command += ",";
            command += "BodyText = " + DbHelper.ParamChar + "paramBodyText";
        }

        if (emailMessage.MsgDateTime != oldEmailMessage.MsgDateTime)
        {
            if (command != "") command += ",";
            command += "MsgDateTime = " + SOut.DateT(emailMessage.MsgDateTime) + "";
        }

        if (emailMessage.SentOrReceived != oldEmailMessage.SentOrReceived)
        {
            if (command != "") command += ",";
            command += "SentOrReceived = " + SOut.Int((int) emailMessage.SentOrReceived) + "";
        }

        if (emailMessage.RecipientAddress != oldEmailMessage.RecipientAddress)
        {
            if (command != "") command += ",";
            command += "RecipientAddress = '" + SOut.String(emailMessage.RecipientAddress) + "'";
        }

        if (emailMessage.RawEmailIn != oldEmailMessage.RawEmailIn)
        {
            if (command != "") command += ",";
            command += "RawEmailIn = " + DbHelper.ParamChar + "paramRawEmailIn";
        }

        if (emailMessage.ProvNumWebMail != oldEmailMessage.ProvNumWebMail)
        {
            if (command != "") command += ",";
            command += "ProvNumWebMail = " + SOut.Long(emailMessage.ProvNumWebMail) + "";
        }

        if (emailMessage.PatNumSubj != oldEmailMessage.PatNumSubj)
        {
            if (command != "") command += ",";
            command += "PatNumSubj = " + SOut.Long(emailMessage.PatNumSubj) + "";
        }

        if (emailMessage.CcAddress != oldEmailMessage.CcAddress)
        {
            if (command != "") command += ",";
            command += "CcAddress = " + DbHelper.ParamChar + "paramCcAddress";
        }

        if (emailMessage.BccAddress != oldEmailMessage.BccAddress)
        {
            if (command != "") command += ",";
            command += "BccAddress = " + DbHelper.ParamChar + "paramBccAddress";
        }

        if (emailMessage.HideIn != oldEmailMessage.HideIn)
        {
            if (command != "") command += ",";
            command += "HideIn = " + SOut.Int((int) emailMessage.HideIn) + "";
        }

        if (emailMessage.AptNum != oldEmailMessage.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(emailMessage.AptNum) + "";
        }

        if (emailMessage.UserNum != oldEmailMessage.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(emailMessage.UserNum) + "";
        }

        if (emailMessage.HtmlType != oldEmailMessage.HtmlType)
        {
            if (command != "") command += ",";
            command += "HtmlType = " + SOut.Int((int) emailMessage.HtmlType) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (emailMessage.MsgType != oldEmailMessage.MsgType)
        {
            if (command != "") command += ",";
            command += "MsgType = '" + SOut.String(emailMessage.MsgType.ToString()) + "'";
        }

        if (emailMessage.FailReason != oldEmailMessage.FailReason)
        {
            if (command != "") command += ",";
            command += "FailReason = '" + SOut.String(emailMessage.FailReason) + "'";
        }

        if (command == "") return false;
        if (emailMessage.ToAddress == null) emailMessage.ToAddress = "";
        var paramToAddress = new OdSqlParameter("paramToAddress", OdDbType.Text, SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", OdDbType.Text, SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", OdDbType.Text, SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", OdDbType.Text, SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", OdDbType.Text, SOut.StringParam(emailMessage.BccAddress));
        command = "UPDATE emailmessage SET " + command
                                             + " WHERE EmailMessageNum = " + SOut.Long(emailMessage.EmailMessageNum);
        Db.NonQ(command, paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
        return true;
    }

    public static bool UpdateComparison(EmailMessage emailMessage, EmailMessage oldEmailMessage)
    {
        if (emailMessage.PatNum != oldEmailMessage.PatNum) return true;
        if (emailMessage.ToAddress != oldEmailMessage.ToAddress) return true;
        if (emailMessage.FromAddress != oldEmailMessage.FromAddress) return true;
        if (emailMessage.Subject != oldEmailMessage.Subject) return true;
        if (emailMessage.BodyText != oldEmailMessage.BodyText) return true;
        if (emailMessage.MsgDateTime != oldEmailMessage.MsgDateTime) return true;
        if (emailMessage.SentOrReceived != oldEmailMessage.SentOrReceived) return true;
        if (emailMessage.RecipientAddress != oldEmailMessage.RecipientAddress) return true;
        if (emailMessage.RawEmailIn != oldEmailMessage.RawEmailIn) return true;
        if (emailMessage.ProvNumWebMail != oldEmailMessage.ProvNumWebMail) return true;
        if (emailMessage.PatNumSubj != oldEmailMessage.PatNumSubj) return true;
        if (emailMessage.CcAddress != oldEmailMessage.CcAddress) return true;
        if (emailMessage.BccAddress != oldEmailMessage.BccAddress) return true;
        if (emailMessage.HideIn != oldEmailMessage.HideIn) return true;
        if (emailMessage.AptNum != oldEmailMessage.AptNum) return true;
        if (emailMessage.UserNum != oldEmailMessage.UserNum) return true;
        if (emailMessage.HtmlType != oldEmailMessage.HtmlType) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (emailMessage.MsgType != oldEmailMessage.MsgType) return true;
        if (emailMessage.FailReason != oldEmailMessage.FailReason) return true;
        return false;
    }

    public static void Delete(long emailMessageNum)
    {
        var command = "DELETE FROM emailmessage "
                      + "WHERE EmailMessageNum = " + SOut.Long(emailMessageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailMessageNums)
    {
        if (listEmailMessageNums == null || listEmailMessageNums.Count == 0) return;
        var command = "DELETE FROM emailmessage "
                      + "WHERE EmailMessageNum IN(" + string.Join(",", listEmailMessageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}