#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PromotionLogCrud
{
    public static PromotionLog SelectOne(long promotionLogNum)
    {
        var command = "SELECT * FROM promotionlog "
                      + "WHERE PromotionLogNum = " + SOut.Long(promotionLogNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PromotionLog SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PromotionLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PromotionLog> TableToList(DataTable table)
    {
        var retVal = new List<PromotionLog>();
        PromotionLog promotionLog;
        foreach (DataRow row in table.Rows)
        {
            promotionLog = new PromotionLog();
            promotionLog.PromotionLogNum = SIn.Long(row["PromotionLogNum"].ToString());
            promotionLog.PromotionNum = SIn.Long(row["PromotionNum"].ToString());
            promotionLog.EmailHostingFK = SIn.Long(row["EmailHostingFK"].ToString());
            promotionLog.PromotionStatus = (PromotionLogStatus) SIn.Int(row["PromotionStatus"].ToString());
            promotionLog.PatNum = SIn.Long(row["PatNum"].ToString());
            promotionLog.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            promotionLog.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            promotionLog.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            promotionLog.MessageFk = SIn.Long(row["MessageFk"].ToString());
            promotionLog.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            promotionLog.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            promotionLog.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            promotionLog.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            retVal.Add(promotionLog);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PromotionLog> listPromotionLogs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PromotionLog";
        var table = new DataTable(tableName);
        table.Columns.Add("PromotionLogNum");
        table.Columns.Add("PromotionNum");
        table.Columns.Add("EmailHostingFK");
        table.Columns.Add("PromotionStatus");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SendStatus");
        table.Columns.Add("MessageType");
        table.Columns.Add("MessageFk");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimeSent");
        table.Columns.Add("ResponseDescript");
        table.Columns.Add("ApptReminderRuleNum");
        foreach (var promotionLog in listPromotionLogs)
            table.Rows.Add(SOut.Long(promotionLog.PromotionLogNum), SOut.Long(promotionLog.PromotionNum), SOut.Long(promotionLog.EmailHostingFK), SOut.Int((int) promotionLog.PromotionStatus), SOut.Long(promotionLog.PatNum), SOut.Long(promotionLog.ClinicNum), SOut.Int((int) promotionLog.SendStatus), SOut.Int((int) promotionLog.MessageType), SOut.Long(promotionLog.MessageFk), SOut.DateT(promotionLog.DateTimeEntry, false), SOut.DateT(promotionLog.DateTimeSent, false), promotionLog.ResponseDescript, SOut.Long(promotionLog.ApptReminderRuleNum));
        return table;
    }

    public static long Insert(PromotionLog promotionLog)
    {
        return Insert(promotionLog, false);
    }

    public static long Insert(PromotionLog promotionLog, bool useExistingPK)
    {
        var command = "INSERT INTO promotionlog (";

        command += "PromotionNum,EmailHostingFK,PromotionStatus,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum) VALUES(";

        command +=
            SOut.Long(promotionLog.PromotionNum) + ","
                                                 + SOut.Long(promotionLog.EmailHostingFK) + ","
                                                 + SOut.Int((int) promotionLog.PromotionStatus) + ","
                                                 + SOut.Long(promotionLog.PatNum) + ","
                                                 + SOut.Long(promotionLog.ClinicNum) + ","
                                                 + SOut.Int((int) promotionLog.SendStatus) + ","
                                                 + SOut.Int((int) promotionLog.MessageType) + ","
                                                 + SOut.Long(promotionLog.MessageFk) + ","
                                                 + DbHelper.Now() + ","
                                                 + SOut.DateT(promotionLog.DateTimeSent) + ","
                                                 + DbHelper.ParamChar + "paramResponseDescript,"
                                                 + SOut.Long(promotionLog.ApptReminderRuleNum) + ")";
        if (promotionLog.ResponseDescript == null) promotionLog.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(promotionLog.ResponseDescript));
        {
            promotionLog.PromotionLogNum = Db.NonQ(command, true, "PromotionLogNum", "promotionLog", paramResponseDescript);
        }
        return promotionLog.PromotionLogNum;
    }

    public static void InsertMany(List<PromotionLog> listPromotionLogs)
    {
        InsertMany(listPromotionLogs, false);
    }

    public static void InsertMany(List<PromotionLog> listPromotionLogs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPromotionLogs.Count)
        {
            var promotionLog = listPromotionLogs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO promotionlog (");
                if (useExistingPK) sbCommands.Append("PromotionLogNum,");
                sbCommands.Append("PromotionNum,EmailHostingFK,PromotionStatus,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(promotionLog.PromotionLogNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(promotionLog.PromotionNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(promotionLog.EmailHostingFK));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) promotionLog.PromotionStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(promotionLog.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(promotionLog.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) promotionLog.SendStatus));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) promotionLog.MessageType));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(promotionLog.MessageFk));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(promotionLog.DateTimeSent));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(promotionLog.ResponseDescript) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(promotionLog.ApptReminderRuleNum));
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
                if (index == listPromotionLogs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PromotionLog promotionLog)
    {
        return InsertNoCache(promotionLog, false);
    }

    public static long InsertNoCache(PromotionLog promotionLog, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO promotionlog (";
        if (isRandomKeys || useExistingPK) command += "PromotionLogNum,";
        command += "PromotionNum,EmailHostingFK,PromotionStatus,PatNum,ClinicNum,SendStatus,MessageType,MessageFk,DateTimeEntry,DateTimeSent,ResponseDescript,ApptReminderRuleNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(promotionLog.PromotionLogNum) + ",";
        command +=
            SOut.Long(promotionLog.PromotionNum) + ","
                                                 + SOut.Long(promotionLog.EmailHostingFK) + ","
                                                 + SOut.Int((int) promotionLog.PromotionStatus) + ","
                                                 + SOut.Long(promotionLog.PatNum) + ","
                                                 + SOut.Long(promotionLog.ClinicNum) + ","
                                                 + SOut.Int((int) promotionLog.SendStatus) + ","
                                                 + SOut.Int((int) promotionLog.MessageType) + ","
                                                 + SOut.Long(promotionLog.MessageFk) + ","
                                                 + DbHelper.Now() + ","
                                                 + SOut.DateT(promotionLog.DateTimeSent) + ","
                                                 + DbHelper.ParamChar + "paramResponseDescript,"
                                                 + SOut.Long(promotionLog.ApptReminderRuleNum) + ")";
        if (promotionLog.ResponseDescript == null) promotionLog.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(promotionLog.ResponseDescript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramResponseDescript);
        else
            promotionLog.PromotionLogNum = Db.NonQ(command, true, "PromotionLogNum", "promotionLog", paramResponseDescript);
        return promotionLog.PromotionLogNum;
    }

    public static void Update(PromotionLog promotionLog)
    {
        var command = "UPDATE promotionlog SET "
                      + "PromotionNum       =  " + SOut.Long(promotionLog.PromotionNum) + ", "
                      + "EmailHostingFK     =  " + SOut.Long(promotionLog.EmailHostingFK) + ", "
                      + "PromotionStatus    =  " + SOut.Int((int) promotionLog.PromotionStatus) + ", "
                      + "PatNum             =  " + SOut.Long(promotionLog.PatNum) + ", "
                      + "ClinicNum          =  " + SOut.Long(promotionLog.ClinicNum) + ", "
                      + "SendStatus         =  " + SOut.Int((int) promotionLog.SendStatus) + ", "
                      + "MessageType        =  " + SOut.Int((int) promotionLog.MessageType) + ", "
                      + "MessageFk          =  " + SOut.Long(promotionLog.MessageFk) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeSent       =  " + SOut.DateT(promotionLog.DateTimeSent) + ", "
                      + "ResponseDescript   =  " + DbHelper.ParamChar + "paramResponseDescript, "
                      + "ApptReminderRuleNum=  " + SOut.Long(promotionLog.ApptReminderRuleNum) + " "
                      + "WHERE PromotionLogNum = " + SOut.Long(promotionLog.PromotionLogNum);
        if (promotionLog.ResponseDescript == null) promotionLog.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(promotionLog.ResponseDescript));
        Db.NonQ(command, paramResponseDescript);
    }

    public static bool Update(PromotionLog promotionLog, PromotionLog oldPromotionLog)
    {
        var command = "";
        if (promotionLog.PromotionNum != oldPromotionLog.PromotionNum)
        {
            if (command != "") command += ",";
            command += "PromotionNum = " + SOut.Long(promotionLog.PromotionNum) + "";
        }

        if (promotionLog.EmailHostingFK != oldPromotionLog.EmailHostingFK)
        {
            if (command != "") command += ",";
            command += "EmailHostingFK = " + SOut.Long(promotionLog.EmailHostingFK) + "";
        }

        if (promotionLog.PromotionStatus != oldPromotionLog.PromotionStatus)
        {
            if (command != "") command += ",";
            command += "PromotionStatus = " + SOut.Int((int) promotionLog.PromotionStatus) + "";
        }

        if (promotionLog.PatNum != oldPromotionLog.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(promotionLog.PatNum) + "";
        }

        if (promotionLog.ClinicNum != oldPromotionLog.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(promotionLog.ClinicNum) + "";
        }

        if (promotionLog.SendStatus != oldPromotionLog.SendStatus)
        {
            if (command != "") command += ",";
            command += "SendStatus = " + SOut.Int((int) promotionLog.SendStatus) + "";
        }

        if (promotionLog.MessageType != oldPromotionLog.MessageType)
        {
            if (command != "") command += ",";
            command += "MessageType = " + SOut.Int((int) promotionLog.MessageType) + "";
        }

        if (promotionLog.MessageFk != oldPromotionLog.MessageFk)
        {
            if (command != "") command += ",";
            command += "MessageFk = " + SOut.Long(promotionLog.MessageFk) + "";
        }

        //DateTimeEntry not allowed to change
        if (promotionLog.DateTimeSent != oldPromotionLog.DateTimeSent)
        {
            if (command != "") command += ",";
            command += "DateTimeSent = " + SOut.DateT(promotionLog.DateTimeSent) + "";
        }

        if (promotionLog.ResponseDescript != oldPromotionLog.ResponseDescript)
        {
            if (command != "") command += ",";
            command += "ResponseDescript = " + DbHelper.ParamChar + "paramResponseDescript";
        }

        if (promotionLog.ApptReminderRuleNum != oldPromotionLog.ApptReminderRuleNum)
        {
            if (command != "") command += ",";
            command += "ApptReminderRuleNum = " + SOut.Long(promotionLog.ApptReminderRuleNum) + "";
        }

        if (command == "") return false;
        if (promotionLog.ResponseDescript == null) promotionLog.ResponseDescript = "";
        var paramResponseDescript = new OdSqlParameter("paramResponseDescript", OdDbType.Text, SOut.StringParam(promotionLog.ResponseDescript));
        command = "UPDATE promotionlog SET " + command
                                             + " WHERE PromotionLogNum = " + SOut.Long(promotionLog.PromotionLogNum);
        Db.NonQ(command, paramResponseDescript);
        return true;
    }

    public static bool UpdateComparison(PromotionLog promotionLog, PromotionLog oldPromotionLog)
    {
        if (promotionLog.PromotionNum != oldPromotionLog.PromotionNum) return true;
        if (promotionLog.EmailHostingFK != oldPromotionLog.EmailHostingFK) return true;
        if (promotionLog.PromotionStatus != oldPromotionLog.PromotionStatus) return true;
        if (promotionLog.PatNum != oldPromotionLog.PatNum) return true;
        if (promotionLog.ClinicNum != oldPromotionLog.ClinicNum) return true;
        if (promotionLog.SendStatus != oldPromotionLog.SendStatus) return true;
        if (promotionLog.MessageType != oldPromotionLog.MessageType) return true;
        if (promotionLog.MessageFk != oldPromotionLog.MessageFk) return true;
        //DateTimeEntry not allowed to change
        if (promotionLog.DateTimeSent != oldPromotionLog.DateTimeSent) return true;
        if (promotionLog.ResponseDescript != oldPromotionLog.ResponseDescript) return true;
        if (promotionLog.ApptReminderRuleNum != oldPromotionLog.ApptReminderRuleNum) return true;
        return false;
    }

    public static void Delete(long promotionLogNum)
    {
        var command = "DELETE FROM promotionlog "
                      + "WHERE PromotionLogNum = " + SOut.Long(promotionLogNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPromotionLogNums)
    {
        if (listPromotionLogNums == null || listPromotionLogNums.Count == 0) return;
        var command = "DELETE FROM promotionlog "
                      + "WHERE PromotionLogNum IN(" + string.Join(",", listPromotionLogNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}