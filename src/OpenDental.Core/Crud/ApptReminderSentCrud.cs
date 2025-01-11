using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApptReminderSentCrud
{
    public static List<ApptReminderSent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptReminderSent> TableToList(DataTable table)
    {
        var retVal = new List<ApptReminderSent>();
        ApptReminderSent apptReminderSent;
        foreach (DataRow row in table.Rows)
        {
            apptReminderSent = new ApptReminderSent();
            apptReminderSent.ApptReminderSentNum = SIn.Long(row["ApptReminderSentNum"].ToString());
            apptReminderSent.PatNum = SIn.Long(row["PatNum"].ToString());
            apptReminderSent.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            apptReminderSent.SendStatus = (AutoCommStatus) SIn.Int(row["SendStatus"].ToString());
            apptReminderSent.MessageType = (CommType) SIn.Int(row["MessageType"].ToString());
            apptReminderSent.MessageFk = SIn.Long(row["MessageFk"].ToString());
            apptReminderSent.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            apptReminderSent.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            apptReminderSent.ResponseDescript = SIn.String(row["ResponseDescript"].ToString());
            apptReminderSent.ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString());
            apptReminderSent.ApptNum = SIn.Long(row["ApptNum"].ToString());
            apptReminderSent.ApptDateTime = SIn.DateTime(row["ApptDateTime"].ToString());
            apptReminderSent.TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString()));
            retVal.Add(apptReminderSent);
        }

        return retVal;
    }
}