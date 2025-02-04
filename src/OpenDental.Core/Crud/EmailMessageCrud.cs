using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<EmailMessage> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailMessage> TableToList(DataTable table)
    {
        var retVal = new List<EmailMessage>();
        foreach (DataRow row in table.Rows)
        {
            var emailMessage = new EmailMessage
            {
                EmailMessageNum = SIn.Long(row["EmailMessageNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ToAddress = SIn.String(row["ToAddress"].ToString()),
                FromAddress = SIn.String(row["FromAddress"].ToString()),
                Subject = SIn.String(row["Subject"].ToString()),
                BodyText = SIn.String(row["BodyText"].ToString()),
                MsgDateTime = SIn.DateTime(row["MsgDateTime"].ToString()),
                SentOrReceived = (EmailSentOrReceived) SIn.Int(row["SentOrReceived"].ToString()),
                RecipientAddress = SIn.String(row["RecipientAddress"].ToString()),
                RawEmailIn = SIn.String(row["RawEmailIn"].ToString()),
                ProvNumWebMail = SIn.Long(row["ProvNumWebMail"].ToString()),
                PatNumSubj = SIn.Long(row["PatNumSubj"].ToString()),
                CcAddress = SIn.String(row["CcAddress"].ToString()),
                BccAddress = SIn.String(row["BccAddress"].ToString()),
                HideIn = (HideInFlags) SIn.Int(row["HideIn"].ToString()),
                AptNum = SIn.Long(row["AptNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                HtmlType = (EmailType) SIn.Int(row["HtmlType"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
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

    public static void Insert(EmailMessage emailMessage)
    {
        var command = "INSERT INTO emailmessage (";

        command += "PatNum,ToAddress,FromAddress,Subject,BodyText,MsgDateTime,SentOrReceived,RecipientAddress,RawEmailIn,ProvNumWebMail,PatNumSubj,CcAddress,BccAddress,HideIn,AptNum,UserNum,HtmlType,SecDateTEntry,MsgType,FailReason) VALUES(";

        command +=
            SOut.Long(emailMessage.PatNum) + ","
                                           + DbHelper.ParamChar + "paramToAddress,"
                                           + DbHelper.ParamChar + "paramFromAddress,"
                                           + DbHelper.ParamChar + "paramSubject,"
                                           + DbHelper.ParamChar + "paramBodyText,"
                                           + SOut.DateTime(emailMessage.MsgDateTime) + ","
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
                                           + "NOW()" + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + "'" + SOut.String(emailMessage.MsgType.ToString()) + "',"
                                           + "'" + SOut.String(emailMessage.FailReason) + "')";
        if (emailMessage.ToAddress == null) emailMessage.ToAddress = "";
        var paramToAddress = new OdSqlParameter("paramToAddress", SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", SOut.StringParam(emailMessage.BccAddress));
        {
            emailMessage.EmailMessageNum = Db.NonQ(command, true, "EmailMessageNum", "emailMessage", paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
        }
    }

    public static void Update(EmailMessage emailMessage)
    {
        var command = "UPDATE emailmessage SET "
                      + "PatNum          =  " + SOut.Long(emailMessage.PatNum) + ", "
                      + "ToAddress       =  " + DbHelper.ParamChar + "paramToAddress, "
                      + "FromAddress     =  " + DbHelper.ParamChar + "paramFromAddress, "
                      + "Subject         =  " + DbHelper.ParamChar + "paramSubject, "
                      + "BodyText        =  " + DbHelper.ParamChar + "paramBodyText, "
                      + "MsgDateTime     =  " + SOut.DateTime(emailMessage.MsgDateTime) + ", "
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
        var paramToAddress = new OdSqlParameter("paramToAddress", SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", SOut.StringParam(emailMessage.BccAddress));
        Db.NonQ(command, paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
    }

    public static void Update(EmailMessage emailMessage, EmailMessage oldEmailMessage)
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
            command += "MsgDateTime = " + SOut.DateTime(emailMessage.MsgDateTime) + "";
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

        if (command == "") return;
        if (emailMessage.ToAddress == null) emailMessage.ToAddress = "";
        var paramToAddress = new OdSqlParameter("paramToAddress", SOut.StringParam(emailMessage.ToAddress));
        if (emailMessage.FromAddress == null) emailMessage.FromAddress = "";
        var paramFromAddress = new OdSqlParameter("paramFromAddress", SOut.StringParam(emailMessage.FromAddress));
        if (emailMessage.Subject == null) emailMessage.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", SOut.StringParam(emailMessage.Subject));
        if (emailMessage.BodyText == null) emailMessage.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", SOut.StringParam(emailMessage.BodyText));
        if (emailMessage.RawEmailIn == null) emailMessage.RawEmailIn = "";
        var paramRawEmailIn = new OdSqlParameter("paramRawEmailIn", SOut.StringParam(emailMessage.RawEmailIn));
        if (emailMessage.CcAddress == null) emailMessage.CcAddress = "";
        var paramCcAddress = new OdSqlParameter("paramCcAddress", SOut.StringParam(emailMessage.CcAddress));
        if (emailMessage.BccAddress == null) emailMessage.BccAddress = "";
        var paramBccAddress = new OdSqlParameter("paramBccAddress", SOut.StringParam(emailMessage.BccAddress));
        command = "UPDATE emailmessage SET " + command
                                             + " WHERE EmailMessageNum = " + SOut.Long(emailMessage.EmailMessageNum);
        Db.NonQ(command, paramToAddress, paramFromAddress, paramSubject, paramBodyText, paramRawEmailIn, paramCcAddress, paramBccAddress);
    }
}