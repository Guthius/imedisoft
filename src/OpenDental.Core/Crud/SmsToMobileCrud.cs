#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SmsToMobileCrud
{
    public static SmsToMobile SelectOne(long smsToMobileNum)
    {
        var command = "SELECT * FROM smstomobile "
                      + "WHERE SmsToMobileNum = " + SOut.Long(smsToMobileNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SmsToMobile SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SmsToMobile> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsToMobile> TableToList(DataTable table)
    {
        var retVal = new List<SmsToMobile>();
        SmsToMobile smsToMobile;
        foreach (DataRow row in table.Rows)
        {
            smsToMobile = new SmsToMobile();
            smsToMobile.SmsToMobileNum = SIn.Long(row["SmsToMobileNum"].ToString());
            smsToMobile.PatNum = SIn.Long(row["PatNum"].ToString());
            smsToMobile.GuidMessage = SIn.String(row["GuidMessage"].ToString());
            smsToMobile.GuidBatch = SIn.String(row["GuidBatch"].ToString());
            smsToMobile.SmsPhoneNumber = SIn.String(row["SmsPhoneNumber"].ToString());
            smsToMobile.MobilePhoneNumber = SIn.String(row["MobilePhoneNumber"].ToString());
            smsToMobile.IsTimeSensitive = SIn.Bool(row["IsTimeSensitive"].ToString());
            smsToMobile.MsgType = (SmsMessageSource) SIn.Int(row["MsgType"].ToString());
            smsToMobile.MsgText = SIn.String(row["MsgText"].ToString());
            smsToMobile.SmsStatus = (SmsDeliveryStatus) SIn.Int(row["SmsStatus"].ToString());
            smsToMobile.MsgParts = SIn.Int(row["MsgParts"].ToString());
            smsToMobile.MsgChargeUSD = SIn.Float(row["MsgChargeUSD"].ToString());
            smsToMobile.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            smsToMobile.CustErrorText = SIn.String(row["CustErrorText"].ToString());
            smsToMobile.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            smsToMobile.DateTimeTerminated = SIn.DateTime(row["DateTimeTerminated"].ToString());
            smsToMobile.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            smsToMobile.MsgDiscountUSD = SIn.Float(row["MsgDiscountUSD"].ToString());
            smsToMobile.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(smsToMobile);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SmsToMobile> listSmsToMobiles, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SmsToMobile";
        var table = new DataTable(tableName);
        table.Columns.Add("SmsToMobileNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("GuidMessage");
        table.Columns.Add("GuidBatch");
        table.Columns.Add("SmsPhoneNumber");
        table.Columns.Add("MobilePhoneNumber");
        table.Columns.Add("IsTimeSensitive");
        table.Columns.Add("MsgType");
        table.Columns.Add("MsgText");
        table.Columns.Add("SmsStatus");
        table.Columns.Add("MsgParts");
        table.Columns.Add("MsgChargeUSD");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("CustErrorText");
        table.Columns.Add("DateTimeSent");
        table.Columns.Add("DateTimeTerminated");
        table.Columns.Add("IsHidden");
        table.Columns.Add("MsgDiscountUSD");
        table.Columns.Add("SecDateTEdit");
        foreach (var smsToMobile in listSmsToMobiles)
            table.Rows.Add(SOut.Long(smsToMobile.SmsToMobileNum), SOut.Long(smsToMobile.PatNum), smsToMobile.GuidMessage, smsToMobile.GuidBatch, smsToMobile.SmsPhoneNumber, smsToMobile.MobilePhoneNumber, SOut.Bool(smsToMobile.IsTimeSensitive), SOut.Int((int) smsToMobile.MsgType), smsToMobile.MsgText, SOut.Int((int) smsToMobile.SmsStatus), SOut.Int(smsToMobile.MsgParts), SOut.Float(smsToMobile.MsgChargeUSD), SOut.Long(smsToMobile.ClinicNum), smsToMobile.CustErrorText, SOut.DateT(smsToMobile.DateTimeSent, false), SOut.DateT(smsToMobile.DateTimeTerminated, false), SOut.Bool(smsToMobile.IsHidden), SOut.Float(smsToMobile.MsgDiscountUSD), SOut.DateT(smsToMobile.SecDateTEdit, false));
        return table;
    }

    public static long Insert(SmsToMobile smsToMobile)
    {
        return Insert(smsToMobile, false);
    }

    public static long Insert(SmsToMobile smsToMobile, bool useExistingPK)
    {
        var command = "INSERT INTO smstomobile (";

        command += "PatNum,GuidMessage,GuidBatch,SmsPhoneNumber,MobilePhoneNumber,IsTimeSensitive,MsgType,MsgText,SmsStatus,MsgParts,MsgChargeUSD,ClinicNum,CustErrorText,DateTimeSent,DateTimeTerminated,IsHidden,MsgDiscountUSD) VALUES(";

        command +=
            SOut.Long(smsToMobile.PatNum) + ","
                                          + "'" + SOut.String(smsToMobile.GuidMessage) + "',"
                                          + "'" + SOut.String(smsToMobile.GuidBatch) + "',"
                                          + "'" + SOut.String(smsToMobile.SmsPhoneNumber) + "',"
                                          + "'" + SOut.String(smsToMobile.MobilePhoneNumber) + "',"
                                          + SOut.Bool(smsToMobile.IsTimeSensitive) + ","
                                          + SOut.Int((int) smsToMobile.MsgType) + ","
                                          + DbHelper.ParamChar + "paramMsgText,"
                                          + SOut.Int((int) smsToMobile.SmsStatus) + ","
                                          + SOut.Int(smsToMobile.MsgParts) + ","
                                          + SOut.Float(smsToMobile.MsgChargeUSD) + ","
                                          + SOut.Long(smsToMobile.ClinicNum) + ","
                                          + "'" + SOut.String(smsToMobile.CustErrorText) + "',"
                                          + SOut.DateT(smsToMobile.DateTimeSent) + ","
                                          + SOut.DateT(smsToMobile.DateTimeTerminated) + ","
                                          + SOut.Bool(smsToMobile.IsHidden) + ","
                                          + SOut.Float(smsToMobile.MsgDiscountUSD) + ")";
        //SecDateTEdit can only be set by MySQL
        if (smsToMobile.MsgText == null) smsToMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsToMobile.MsgText));
        {
            smsToMobile.SmsToMobileNum = Db.NonQ(command, true, "SmsToMobileNum", "smsToMobile", paramMsgText);
        }
        return smsToMobile.SmsToMobileNum;
    }

    public static void InsertMany(List<SmsToMobile> listSmsToMobiles)
    {
        InsertMany(listSmsToMobiles, false);
    }

    public static void InsertMany(List<SmsToMobile> listSmsToMobiles, bool useExistingPK)
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
            sbRow.Append(SOut.DateT(smsToMobile.DateTimeSent));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(smsToMobile.DateTimeTerminated));
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

    public static long InsertNoCache(SmsToMobile smsToMobile)
    {
        return InsertNoCache(smsToMobile, false);
    }

    public static long InsertNoCache(SmsToMobile smsToMobile, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO smstomobile (";
        if (isRandomKeys || useExistingPK) command += "SmsToMobileNum,";
        command += "PatNum,GuidMessage,GuidBatch,SmsPhoneNumber,MobilePhoneNumber,IsTimeSensitive,MsgType,MsgText,SmsStatus,MsgParts,MsgChargeUSD,ClinicNum,CustErrorText,DateTimeSent,DateTimeTerminated,IsHidden,MsgDiscountUSD) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(smsToMobile.SmsToMobileNum) + ",";
        command +=
            SOut.Long(smsToMobile.PatNum) + ","
                                          + "'" + SOut.String(smsToMobile.GuidMessage) + "',"
                                          + "'" + SOut.String(smsToMobile.GuidBatch) + "',"
                                          + "'" + SOut.String(smsToMobile.SmsPhoneNumber) + "',"
                                          + "'" + SOut.String(smsToMobile.MobilePhoneNumber) + "',"
                                          + SOut.Bool(smsToMobile.IsTimeSensitive) + ","
                                          + SOut.Int((int) smsToMobile.MsgType) + ","
                                          + DbHelper.ParamChar + "paramMsgText,"
                                          + SOut.Int((int) smsToMobile.SmsStatus) + ","
                                          + SOut.Int(smsToMobile.MsgParts) + ","
                                          + SOut.Float(smsToMobile.MsgChargeUSD) + ","
                                          + SOut.Long(smsToMobile.ClinicNum) + ","
                                          + "'" + SOut.String(smsToMobile.CustErrorText) + "',"
                                          + SOut.DateT(smsToMobile.DateTimeSent) + ","
                                          + SOut.DateT(smsToMobile.DateTimeTerminated) + ","
                                          + SOut.Bool(smsToMobile.IsHidden) + ","
                                          + SOut.Float(smsToMobile.MsgDiscountUSD) + ")";
        //SecDateTEdit can only be set by MySQL
        if (smsToMobile.MsgText == null) smsToMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsToMobile.MsgText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMsgText);
        else
            smsToMobile.SmsToMobileNum = Db.NonQ(command, true, "SmsToMobileNum", "smsToMobile", paramMsgText);
        return smsToMobile.SmsToMobileNum;
    }

    public static void Update(SmsToMobile smsToMobile)
    {
        var command = "UPDATE smstomobile SET "
                      + "PatNum            =  " + SOut.Long(smsToMobile.PatNum) + ", "
                      + "GuidMessage       = '" + SOut.String(smsToMobile.GuidMessage) + "', "
                      + "GuidBatch         = '" + SOut.String(smsToMobile.GuidBatch) + "', "
                      + "SmsPhoneNumber    = '" + SOut.String(smsToMobile.SmsPhoneNumber) + "', "
                      + "MobilePhoneNumber = '" + SOut.String(smsToMobile.MobilePhoneNumber) + "', "
                      + "IsTimeSensitive   =  " + SOut.Bool(smsToMobile.IsTimeSensitive) + ", "
                      + "MsgType           =  " + SOut.Int((int) smsToMobile.MsgType) + ", "
                      + "MsgText           =  " + DbHelper.ParamChar + "paramMsgText, "
                      + "SmsStatus         =  " + SOut.Int((int) smsToMobile.SmsStatus) + ", "
                      + "MsgParts          =  " + SOut.Int(smsToMobile.MsgParts) + ", "
                      + "MsgChargeUSD      =  " + SOut.Float(smsToMobile.MsgChargeUSD) + ", "
                      + "ClinicNum         =  " + SOut.Long(smsToMobile.ClinicNum) + ", "
                      + "CustErrorText     = '" + SOut.String(smsToMobile.CustErrorText) + "', "
                      + "DateTimeSent      =  " + SOut.DateT(smsToMobile.DateTimeSent) + ", "
                      + "DateTimeTerminated=  " + SOut.DateT(smsToMobile.DateTimeTerminated) + ", "
                      + "IsHidden          =  " + SOut.Bool(smsToMobile.IsHidden) + ", "
                      + "MsgDiscountUSD    =  " + SOut.Float(smsToMobile.MsgDiscountUSD) + " "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE SmsToMobileNum = " + SOut.Long(smsToMobile.SmsToMobileNum);
        if (smsToMobile.MsgText == null) smsToMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsToMobile.MsgText));
        Db.NonQ(command, paramMsgText);
    }

    public static bool Update(SmsToMobile smsToMobile, SmsToMobile oldSmsToMobile)
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
            command += "DateTimeSent = " + SOut.DateT(smsToMobile.DateTimeSent) + "";
        }

        if (smsToMobile.DateTimeTerminated != oldSmsToMobile.DateTimeTerminated)
        {
            if (command != "") command += ",";
            command += "DateTimeTerminated = " + SOut.DateT(smsToMobile.DateTimeTerminated) + "";
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
        if (command == "") return false;
        if (smsToMobile.MsgText == null) smsToMobile.MsgText = "";
        var paramMsgText = new OdSqlParameter("paramMsgText", OdDbType.Text, SOut.StringNote(smsToMobile.MsgText));
        command = "UPDATE smstomobile SET " + command
                                            + " WHERE SmsToMobileNum = " + SOut.Long(smsToMobile.SmsToMobileNum);
        Db.NonQ(command, paramMsgText);
        return true;
    }

    public static bool UpdateComparison(SmsToMobile smsToMobile, SmsToMobile oldSmsToMobile)
    {
        if (smsToMobile.PatNum != oldSmsToMobile.PatNum) return true;
        if (smsToMobile.GuidMessage != oldSmsToMobile.GuidMessage) return true;
        if (smsToMobile.GuidBatch != oldSmsToMobile.GuidBatch) return true;
        if (smsToMobile.SmsPhoneNumber != oldSmsToMobile.SmsPhoneNumber) return true;
        if (smsToMobile.MobilePhoneNumber != oldSmsToMobile.MobilePhoneNumber) return true;
        if (smsToMobile.IsTimeSensitive != oldSmsToMobile.IsTimeSensitive) return true;
        if (smsToMobile.MsgType != oldSmsToMobile.MsgType) return true;
        if (smsToMobile.MsgText != oldSmsToMobile.MsgText) return true;
        if (smsToMobile.SmsStatus != oldSmsToMobile.SmsStatus) return true;
        if (smsToMobile.MsgParts != oldSmsToMobile.MsgParts) return true;
        if (smsToMobile.MsgChargeUSD != oldSmsToMobile.MsgChargeUSD) return true;
        if (smsToMobile.ClinicNum != oldSmsToMobile.ClinicNum) return true;
        if (smsToMobile.CustErrorText != oldSmsToMobile.CustErrorText) return true;
        if (smsToMobile.DateTimeSent != oldSmsToMobile.DateTimeSent) return true;
        if (smsToMobile.DateTimeTerminated != oldSmsToMobile.DateTimeTerminated) return true;
        if (smsToMobile.IsHidden != oldSmsToMobile.IsHidden) return true;
        if (smsToMobile.MsgDiscountUSD != oldSmsToMobile.MsgDiscountUSD) return true;
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long smsToMobileNum)
    {
        var command = "DELETE FROM smstomobile "
                      + "WHERE SmsToMobileNum = " + SOut.Long(smsToMobileNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSmsToMobileNums)
    {
        if (listSmsToMobileNums == null || listSmsToMobileNums.Count == 0) return;
        var command = "DELETE FROM smstomobile "
                      + "WHERE SmsToMobileNum IN(" + string.Join(",", listSmsToMobileNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}