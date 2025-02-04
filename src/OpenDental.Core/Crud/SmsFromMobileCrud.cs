using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SmsFromMobileCrud
{
    public static List<SmsFromMobile> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsFromMobile> TableToList(DataTable table)
    {
        var retVal = new List<SmsFromMobile>();
        foreach (DataRow row in table.Rows)
        {
            var smsFromMobile = new SmsFromMobile
            {
                SmsFromMobileNum = SIn.Long(row["SmsFromMobileNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                CommlogNum = SIn.Long(row["CommlogNum"].ToString()),
                MsgText = SIn.String(row["MsgText"].ToString()),
                DateTimeReceived = SIn.DateTime(row["DateTimeReceived"].ToString()),
                SmsPhoneNumber = SIn.String(row["SmsPhoneNumber"].ToString()),
                MobilePhoneNumber = SIn.String(row["MobilePhoneNumber"].ToString()),
                MsgPart = SIn.Int(row["MsgPart"].ToString()),
                MsgTotal = SIn.Int(row["MsgTotal"].ToString()),
                MsgRefID = SIn.String(row["MsgRefID"].ToString()),
                SmsStatus = (SmsFromStatus) SIn.Int(row["SmsStatus"].ToString()),
                Flags = SIn.String(row["Flags"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                MatchCount = SIn.Int(row["MatchCount"].ToString()),
                GuidMessage = SIn.String(row["GuidMessage"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(smsFromMobile);
        }

        return retVal;
    }

    public static void Update(SmsFromMobile smsFromMobile, SmsFromMobile oldSmsFromMobile)
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
            command += "DateTimeReceived = " + SOut.DateTime(smsFromMobile.DateTimeReceived) + "";
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
        if (command == "") return;
        if (smsFromMobile.MsgText == null) smsFromMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", SOut.StringNote(smsFromMobile.MsgText));
        command = "UPDATE smsfrommobile SET " + command
                                              + " WHERE SmsFromMobileNum = " + SOut.Long(smsFromMobile.SmsFromMobileNum);
        Db.NonQ(command, paramMsgText);
    }
}