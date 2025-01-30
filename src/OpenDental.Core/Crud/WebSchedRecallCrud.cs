using System.Collections.Generic;
using System.Data;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class WebSchedRecallCrud
{
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
            sbRow.Append(SOut.DateTime(webSchedRecall.DateDue));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(webSchedRecall.ReminderCount));
            sbRow.Append(",");
            sbRow.Append(SOut.DateTime(webSchedRecall.DateTimeSendFailed));
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
            sbRow.Append("NOW()");
            sbRow.Append(",");
            sbRow.Append(SOut.DateTime(webSchedRecall.DateTimeSent));
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
}