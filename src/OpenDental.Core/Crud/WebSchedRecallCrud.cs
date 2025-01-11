#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class WebSchedRecallCrud
{
    public static WebSchedRecall SelectOne(long webSchedRecallNum)
    {
        var command = "SELECT * FROM webschedrecall "
                      + "WHERE WebSchedRecallNum = " + SOut.Long(webSchedRecallNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static WebSchedRecall SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<WebSchedRecall> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<WebSchedRecall> TableToList(DataTable table)
    {
        var retVal = new List<WebSchedRecall>();
        WebSchedRecall webSchedRecall;
        foreach (DataRow row in table.Rows)
        {
            webSchedRecall = new WebSchedRecall();
            webSchedRecall.WebSchedRecallNum = SIn.Long(row["WebSchedRecallNum"].ToString());
            webSchedRecall.RecallNum = SIn.Long(row["RecallNum"].ToString());
            webSchedRecall.DateDue = SIn.DateTime(row["DateDue"].ToString());
            webSchedRecall.ReminderCount = SIn.Int(row["ReminderCount"].ToString());
            webSchedRecall.DateTimeSendFailed = SIn.DateTime(row["DateTimeSendFailed"].ToString());
            webSchedRecall.Source = (WebSchedRecallSource) SIn.Int(row["Source"].ToString());
            webSchedRecall.CommlogNum = SIn.Long(row["CommlogNum"].ToString());
            webSchedRecall.PatNum = SIn.Long(row["PatNum"].ToString());
            webSchedRecall.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            webSchedRecall.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            webSchedRecall.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            webSchedRecall.MessageFk = SIn.Long(row["MessageFk"].ToString());
            webSchedRecall.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            webSchedRecall.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            webSchedRecall.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            webSchedRecall.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            webSchedRecall.ShortGUID = SIn.String(row["ShortGUID"].ToString());
            retVal.Add(webSchedRecall);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<WebSchedRecall> listWebSchedRecalls, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "WebSchedRecall";
        var table = new DataTable(tableName);
        table.Columns.Add("WebSchedRecallNum");
        table.Columns.Add("RecallNum");
        table.Columns.Add("DateDue");
        table.Columns.Add("ReminderCount");
        table.Columns.Add("DateTimeSendFailed");
        table.Columns.Add("Source");
        table.Columns.Add("CommlogNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SendStatus");
        table.Columns.Add("MessageType");
        table.Columns.Add("MessageFk");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimeSent");
        table.Columns.Add("ResponseDescript");
        table.Columns.Add("ApptReminderRuleNum");
        table.Columns.Add("ShortGUID");
        foreach (var webSchedRecall in listWebSchedRecalls)
            table.Rows.Add(SOut.Long(webSchedRecall.WebSchedRecallNum), SOut.Long(webSchedRecall.RecallNum), SOut.DateT(webSchedRecall.DateDue, false), SOut.Int(webSchedRecall.ReminderCount), SOut.DateT(webSchedRecall.DateTimeSendFailed, false), SOut.Int((int) webSchedRecall.Source), SOut.Long(webSchedRecall.CommlogNum), SOut.Long(webSchedRecall.PatNum), SOut.Long(webSchedRecall.ClinicNum), SOut.Int((int) webSchedRecall.SendStatus), SOut.Int((int) webSchedRecall.MessageType), SOut.Long(webSchedRecall.MessageFk), SOut.DateT(webSchedRecall.DateTimeEntry, false), SOut.DateT(webSchedRecall.DateTimeSent, false), webSchedRecall.ResponseDescript, SOut.Long(webSchedRecall.ApptReminderRuleNum), webSchedRecall.ShortGUID);
        return table;
    }

    public static long Insert(WebSchedRecall webSchedRecall)
    {
        return Insert(webSchedRecall, false);
    }

    public static long Insert(WebSchedRecall webSchedRecall, bool useExistingPK)
    {
        var command = "INSERT INTO webschedrecall (";

        command += "RecallNum,DateDue,ReminderCount,DateTimeSendFailed,Source,CommlogNum,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ShortGUID) VALUES(";

        command +=
            SOut.Long(webSchedRecall.RecallNum) + ","
                                                + SOut.DateT(webSchedRecall.DateDue) + ","
                                                + SOut.Int(webSchedRecall.ReminderCount) + ","
                                                + SOut.DateT(webSchedRecall.DateTimeSendFailed) + ","
                                                + SOut.Int((int) webSchedRecall.Source) + ","
                                                + SOut.Long(webSchedRecall.CommlogNum) + ","
                                                + SOut.Long(webSchedRecall.PatNum) + ","
                                                + SOut.Long(webSchedRecall.ClinicNum) + ","
                                                + SOut.Int((int) webSchedRecall.SendStatus) + ","
                                                + SOut.Int((int) webSchedRecall.MessageType) + ","
                                                + SOut.Long(webSchedRecall.MessageFk) + ","
                                                + DbHelper.Now() + ","
                                                + SOut.DateT(webSchedRecall.DateTimeSent) + ","
                                                + DbHelper.ParamChar + "paramResponseDescript,"
                                                + SOut.Long(webSchedRecall.ApptReminderRuleNum) + ","
                                                + "'" + SOut.String(webSchedRecall.ShortGUID) + "')";
        if (webSchedRecall.ResponseDescript == null) webSchedRecall.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(webSchedRecall.ResponseDescript));
        {
            webSchedRecall.WebSchedRecallNum = Db.NonQ(command, true, "WebSchedRecallNum", "webSchedRecall", paramResponseDescript);
        }
        return webSchedRecall.WebSchedRecallNum;
    }

    public static void InsertMany(List<WebSchedRecall> listWebSchedRecalls)
    {
        InsertMany(listWebSchedRecalls, false);
    }

    public static void InsertMany(List<WebSchedRecall> listWebSchedRecalls, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listWebSchedRecalls.Count)
        {
            var webSchedRecall = listWebSchedRecalls[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO webschedrecall (");
                if (useExistingPK) sbCommands.Append("WebSchedRecallNum,");
                sbCommands.Append("RecallNum,DateDue,ReminderCount,DateTimeSendFailed,Source,CommlogNum,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ShortGUID) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(webSchedRecall.WebSchedRecallNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(webSchedRecall.RecallNum));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(webSchedRecall.DateDue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(webSchedRecall.ReminderCount));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(webSchedRecall.DateTimeSendFailed));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) webSchedRecall.Source));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(webSchedRecall.CommlogNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(webSchedRecall.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(webSchedRecall.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) webSchedRecall.SendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) webSchedRecall.MessageType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(webSchedRecall.MessageFk));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(webSchedRecall.DateTimeSent));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(webSchedRecall.ResponseDescript) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(webSchedRecall.ApptReminderRuleNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(webSchedRecall.ShortGUID) + "'");
            sbRow.Append(")");
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
                if (index == listWebSchedRecalls.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(WebSchedRecall webSchedRecall)
    {
        return InsertNoCache(webSchedRecall, false);
    }

    public static long InsertNoCache(WebSchedRecall webSchedRecall, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO webschedrecall (";
        if (isRandomKeys || useExistingPK) command += "WebSchedRecallNum,";
        command += "RecallNum,DateDue,ReminderCount,DateTimeSendFailed,Source,CommlogNum,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum,ShortGUID) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(webSchedRecall.WebSchedRecallNum) + ",";
        command +=
            SOut.Long(webSchedRecall.RecallNum) + ","
                                                + SOut.DateT(webSchedRecall.DateDue) + ","
                                                + SOut.Int(webSchedRecall.ReminderCount) + ","
                                                + SOut.DateT(webSchedRecall.DateTimeSendFailed) + ","
                                                + SOut.Int((int) webSchedRecall.Source) + ","
                                                + SOut.Long(webSchedRecall.CommlogNum) + ","
                                                + SOut.Long(webSchedRecall.PatNum) + ","
                                                + SOut.Long(webSchedRecall.ClinicNum) + ","
                                                + SOut.Int((int) webSchedRecall.SendStatus) + ","
                                                + SOut.Int((int) webSchedRecall.MessageType) + ","
                                                + SOut.Long(webSchedRecall.MessageFk) + ","
                                                + DbHelper.Now() + ","
                                                + SOut.DateT(webSchedRecall.DateTimeSent) + ","
                                                + DbHelper.ParamChar + "paramResponseDescript,"
                                                + SOut.Long(webSchedRecall.ApptReminderRuleNum) + ","
                                                + "'" + SOut.String(webSchedRecall.ShortGUID) + "')";
        if (webSchedRecall.ResponseDescript == null) webSchedRecall.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(webSchedRecall.ResponseDescript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramResponseDescript);
        else
            webSchedRecall.WebSchedRecallNum = Db.NonQ(command, true, "WebSchedRecallNum", "webSchedRecall", paramResponseDescript);
        return webSchedRecall.WebSchedRecallNum;
    }

    public static void Update(WebSchedRecall webSchedRecall)
    {
        var command = "UPDATE webschedrecall SET "
                      + "RecallNum          =  " + SOut.Long(webSchedRecall.RecallNum) + ", "
                      + "DateDue            =  " + SOut.DateT(webSchedRecall.DateDue) + ", "
                      + "ReminderCount      =  " + SOut.Int(webSchedRecall.ReminderCount) + ", "
                      + "DateTimeSendFailed =  " + SOut.DateT(webSchedRecall.DateTimeSendFailed) + ", "
                      + "Source             =  " + SOut.Int((int) webSchedRecall.Source) + ", "
                      + "CommlogNum         =  " + SOut.Long(webSchedRecall.CommlogNum) + ", "
                      + "PatNum             =  " + SOut.Long(webSchedRecall.PatNum) + ", "
                      + "ClinicNum          =  " + SOut.Long(webSchedRecall.ClinicNum) + ", "
                      + "SendStatus         =  " + SOut.Int((int) webSchedRecall.SendStatus) + ", "
                      + "MessageType        =  " + SOut.Int((int) webSchedRecall.MessageType) + ", "
                      + "MessageFk          =  " + SOut.Long(webSchedRecall.MessageFk) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeSent       =  " + SOut.DateT(webSchedRecall.DateTimeSent) + ", "
                      + "ResponseDescript   =  " + DbHelper.ParamChar + "paramResponseDescript, "
                      + "ApptReminderRuleNum=  " + SOut.Long(webSchedRecall.ApptReminderRuleNum) + ", "
                      + "ShortGUID          = '" + SOut.String(webSchedRecall.ShortGUID) + "' "
                      + "WHERE WebSchedRecallNum = " + SOut.Long(webSchedRecall.WebSchedRecallNum);
        if (webSchedRecall.ResponseDescript == null) webSchedRecall.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(webSchedRecall.ResponseDescript));
        Db.NonQ(command, paramResponseDescript);
    }

    public static bool Update(WebSchedRecall webSchedRecall, WebSchedRecall oldWebSchedRecall)
    {
        var command = "";
        if (webSchedRecall.RecallNum != oldWebSchedRecall.RecallNum)
        {
            if (command != "") command += ",";
            command += "RecallNum = " + SOut.Long(webSchedRecall.RecallNum) + "";
        }

        if (webSchedRecall.DateDue != oldWebSchedRecall.DateDue)
        {
            if (command != "") command += ",";
            command += "DateDue = " + SOut.DateT(webSchedRecall.DateDue) + "";
        }

        if (webSchedRecall.ReminderCount != oldWebSchedRecall.ReminderCount)
        {
            if (command != "") command += ",";
            command += "ReminderCount = " + SOut.Int(webSchedRecall.ReminderCount) + "";
        }

        if (webSchedRecall.DateTimeSendFailed != oldWebSchedRecall.DateTimeSendFailed)
        {
            if (command != "") command += ",";
            command += "DateTimeSendFailed = " + SOut.DateT(webSchedRecall.DateTimeSendFailed) + "";
        }

        if (webSchedRecall.Source != oldWebSchedRecall.Source)
        {
            if (command != "") command += ",";
            command += "Source = " + SOut.Int((int) webSchedRecall.Source) + "";
        }

        if (webSchedRecall.CommlogNum != oldWebSchedRecall.CommlogNum)
        {
            if (command != "") command += ",";
            command += "CommlogNum = " + SOut.Long(webSchedRecall.CommlogNum) + "";
        }

        if (webSchedRecall.PatNum != oldWebSchedRecall.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(webSchedRecall.PatNum) + "";
        }

        if (webSchedRecall.ClinicNum != oldWebSchedRecall.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(webSchedRecall.ClinicNum) + "";
        }

        if (webSchedRecall.SendStatus != oldWebSchedRecall.SendStatus)
        {
            if (command != "") command += ",";
            command += "SendStatus = " + SOut.Int((int) webSchedRecall.SendStatus) + "";
        }

        if (webSchedRecall.MessageType != oldWebSchedRecall.MessageType)
        {
            if (command != "") command += ",";
            command += "MessageType = " + SOut.Int((int) webSchedRecall.MessageType) + "";
        }

        if (webSchedRecall.MessageFk != oldWebSchedRecall.MessageFk)
        {
            if (command != "") command += ",";
            command += "MessageFk = " + SOut.Long(webSchedRecall.MessageFk) + "";
        }

        //DateTimeEntry not allowed to change
        if (webSchedRecall.DateTimeSent != oldWebSchedRecall.DateTimeSent)
        {
            if (command != "") command += ",";
            command += "DateTimeSent = " + SOut.DateT(webSchedRecall.DateTimeSent) + "";
        }

        if (webSchedRecall.ResponseDescript != oldWebSchedRecall.ResponseDescript)
        {
            if (command != "") command += ",";
            command += "ResponseDescript = " + DbHelper.ParamChar + "paramResponseDescript";
        }

        if (webSchedRecall.ApptReminderRuleNum != oldWebSchedRecall.ApptReminderRuleNum)
        {
            if (command != "") command += ",";
            command += "ApptReminderRuleNum = " + SOut.Long(webSchedRecall.ApptReminderRuleNum) + "";
        }

        if (webSchedRecall.ShortGUID != oldWebSchedRecall.ShortGUID)
        {
            if (command != "") command += ",";
            command += "ShortGUID = '" + SOut.String(webSchedRecall.ShortGUID) + "'";
        }

        if (command == "") return false;
        if (webSchedRecall.ResponseDescript == null) webSchedRecall.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(webSchedRecall.ResponseDescript));
        command = "UPDATE webschedrecall SET " + command
                                               + " WHERE WebSchedRecallNum = " + SOut.Long(webSchedRecall.WebSchedRecallNum);
        Db.NonQ(command, paramResponseDescript);
        return true;
    }

    public static bool UpdateComparison(WebSchedRecall webSchedRecall, WebSchedRecall oldWebSchedRecall)
    {
        if (webSchedRecall.RecallNum != oldWebSchedRecall.RecallNum) return true;
        if (webSchedRecall.DateDue != oldWebSchedRecall.DateDue) return true;
        if (webSchedRecall.ReminderCount != oldWebSchedRecall.ReminderCount) return true;
        if (webSchedRecall.DateTimeSendFailed != oldWebSchedRecall.DateTimeSendFailed) return true;
        if (webSchedRecall.Source != oldWebSchedRecall.Source) return true;
        if (webSchedRecall.CommlogNum != oldWebSchedRecall.CommlogNum) return true;
        if (webSchedRecall.PatNum != oldWebSchedRecall.PatNum) return true;
        if (webSchedRecall.ClinicNum != oldWebSchedRecall.ClinicNum) return true;
        if (webSchedRecall.SendStatus != oldWebSchedRecall.SendStatus) return true;
        if (webSchedRecall.MessageType != oldWebSchedRecall.MessageType) return true;
        if (webSchedRecall.MessageFk != oldWebSchedRecall.MessageFk) return true;
        //DateTimeEntry not allowed to change
        if (webSchedRecall.DateTimeSent != oldWebSchedRecall.DateTimeSent) return true;
        if (webSchedRecall.ResponseDescript != oldWebSchedRecall.ResponseDescript) return true;
        if (webSchedRecall.ApptReminderRuleNum != oldWebSchedRecall.ApptReminderRuleNum) return true;
        if (webSchedRecall.ShortGUID != oldWebSchedRecall.ShortGUID) return true;
        return false;
    }

    public static void Delete(long webSchedRecallNum)
    {
        var command = "DELETE FROM webschedrecall "
                      + "WHERE WebSchedRecallNum = " + SOut.Long(webSchedRecallNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listWebSchedRecallNums)
    {
        if (listWebSchedRecallNums == null || listWebSchedRecallNums.Count == 0) return;
        var command = "DELETE FROM webschedrecall "
                      + "WHERE WebSchedRecallNum IN(" + string.Join(",", listWebSchedRecallNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}