using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SmsToMobileCrud
{
    public static List<SmsToMobile> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsToMobile> TableToList(DataTable table)
    {
        var retVal = new List<SmsToMobile>();
        foreach (DataRow row in table.Rows)
        {
            var smsToMobile = new SmsToMobile
            {
                SmsToMobileNum = SIn.Long(row["SmsToMobileNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                GuidMessage = SIn.String(row["GuidMessage"].ToString()),
                GuidBatch = SIn.String(row["GuidBatch"].ToString()),
                SmsPhoneNumber = SIn.String(row["SmsPhoneNumber"].ToString()),
                MobilePhoneNumber = SIn.String(row["MobilePhoneNumber"].ToString()),
                IsTimeSensitive = SIn.Bool(row["IsTimeSensitive"].ToString()),
                MsgType = (SmsMessageSource) SIn.Int(row["MsgType"].ToString()),
                MsgText = SIn.String(row["MsgText"].ToString()),
                SmsStatus = (SmsDeliveryStatus) SIn.Int(row["SmsStatus"].ToString()),
                MsgParts = SIn.Int(row["MsgParts"].ToString()),
                MsgChargeUSD = SIn.Float(row["MsgChargeUSD"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                CustErrorText = SIn.String(row["CustErrorText"].ToString()),
                DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString()),
                DateTimeTerminated = SIn.DateTime(row["DateTimeTerminated"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                MsgDiscountUSD = SIn.Float(row["MsgDiscountUSD"].ToString())
            };
            retVal.Add(smsToMobile);
        }

        return retVal;
    }

    public static void InsertMany(List<SmsToMobile> listSmsToMobiles, bool useExistingPK = false)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSmsToMobiles.Count)
        {
            var smsToMobile = listSmsToMobiles[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO smstomobile (");
                if (useExistingPK) sbCommands.Append("SmsToMobileNum,");
                sbCommands.Append("PatNum,GuidMessage,GuidBatch,SmsPhoneNumber,MobilePhoneNumber,IsTimeSensitive,MsgType,MsgText,SmsStatus,MsgParts,MsgChargeUSD,ClinicNum,CustErrorText,DateTimeSent,DateTimeTerminated,IsHidden,MsgDiscountUSD) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(smsToMobile.SmsToMobileNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(smsToMobile.PatNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(smsToMobile.GuidMessage) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(smsToMobile.GuidBatch) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(smsToMobile.SmsPhoneNumber) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(smsToMobile.MobilePhoneNumber) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(smsToMobile.IsTimeSensitive));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) smsToMobile.MsgType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(smsToMobile.MsgText) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) smsToMobile.SmsStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(smsToMobile.MsgParts));
            sbRow.Append(",");
            sbRow.Append(SOut.Float(smsToMobile.MsgChargeUSD));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(smsToMobile.ClinicNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(smsToMobile.CustErrorText) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.DateTime(smsToMobile.DateTimeSent));
            sbRow.Append(",");
            sbRow.Append(SOut.DateTime(smsToMobile.DateTimeTerminated));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(smsToMobile.IsHidden));
            sbRow.Append(",");
            sbRow.Append(SOut.Float(smsToMobile.MsgDiscountUSD));
            sbRow.Append(")");
            //SecDateTEdit can only be set by MySQL
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listSmsToMobiles.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static void Update(SmsToMobile smsToMobile, SmsToMobile oldSmsToMobile)
    {
        var command = "";
        if (smsToMobile.PatNum != oldSmsToMobile.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(smsToMobile.PatNum) + "";
        }

        if (smsToMobile.GuidMessage != oldSmsToMobile.GuidMessage)
        {
            if (command != "") command += ",";
            command += "GuidMessage = '" + SOut.String(smsToMobile.GuidMessage) + "'";
        }

        if (smsToMobile.GuidBatch != oldSmsToMobile.GuidBatch)
        {
            if (command != "") command += ",";
            command += "GuidBatch = '" + SOut.String(smsToMobile.GuidBatch) + "'";
        }

        if (smsToMobile.SmsPhoneNumber != oldSmsToMobile.SmsPhoneNumber)
        {
            if (command != "") command += ",";
            command += "SmsPhoneNumber = '" + SOut.String(smsToMobile.SmsPhoneNumber) + "'";
        }

        if (smsToMobile.MobilePhoneNumber != oldSmsToMobile.MobilePhoneNumber)
        {
            if (command != "") command += ",";
            command += "MobilePhoneNumber = '" + SOut.String(smsToMobile.MobilePhoneNumber) + "'";
        }

        if (smsToMobile.IsTimeSensitive != oldSmsToMobile.IsTimeSensitive)
        {
            if (command != "") command += ",";
            command += "IsTimeSensitive = " + SOut.Bool(smsToMobile.IsTimeSensitive) + "";
        }

        if (smsToMobile.MsgType != oldSmsToMobile.MsgType)
        {
            if (command != "") command += ",";
            command += "MsgType = " + SOut.Int((int) smsToMobile.MsgType) + "";
        }

        if (smsToMobile.MsgText != oldSmsToMobile.MsgText)
        {
            if (command != "") command += ",";
            command += "MsgText = " + DbHelper.ParamChar + "paramMsgText";
        }

        if (smsToMobile.SmsStatus != oldSmsToMobile.SmsStatus)
        {
            if (command != "") command += ",";
            command += "SmsStatus = " + SOut.Int((int) smsToMobile.SmsStatus) + "";
        }

        if (smsToMobile.MsgParts != oldSmsToMobile.MsgParts)
        {
            if (command != "") command += ",";
            command += "MsgParts = " + SOut.Int(smsToMobile.MsgParts) + "";
        }

        if (smsToMobile.MsgChargeUSD != oldSmsToMobile.MsgChargeUSD)
        {
            if (command != "") command += ",";
            command += "MsgChargeUSD = " + SOut.Float(smsToMobile.MsgChargeUSD) + "";
        }

        if (smsToMobile.ClinicNum != oldSmsToMobile.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(smsToMobile.ClinicNum) + "";
        }

        if (smsToMobile.CustErrorText != oldSmsToMobile.CustErrorText)
        {
            if (command != "") command += ",";
            command += "CustErrorText = '" + SOut.String(smsToMobile.CustErrorText) + "'";
        }

        if (smsToMobile.DateTimeSent != oldSmsToMobile.DateTimeSent)
        {
            if (command != "") command += ",";
            command += "DateTimeSent = " + SOut.DateTime(smsToMobile.DateTimeSent) + "";
        }

        if (smsToMobile.DateTimeTerminated != oldSmsToMobile.DateTimeTerminated)
        {
            if (command != "") command += ",";
            command += "DateTimeTerminated = " + SOut.DateTime(smsToMobile.DateTimeTerminated) + "";
        }

        if (smsToMobile.IsHidden != oldSmsToMobile.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(smsToMobile.IsHidden) + "";
        }

        if (smsToMobile.MsgDiscountUSD != oldSmsToMobile.MsgDiscountUSD)
        {
            if (command != "") command += ",";
            command += "MsgDiscountUSD = " + SOut.Float(smsToMobile.MsgDiscountUSD) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return;
        if (smsToMobile.MsgText == null) smsToMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", SOut.StringNote(smsToMobile.MsgText));
        command = "UPDATE smstomobile SET " + command
                                            + " WHERE SmsToMobileNum = " + SOut.Long(smsToMobile.SmsToMobileNum);
        Db.NonQ(command, paramMsgText);
    }
}