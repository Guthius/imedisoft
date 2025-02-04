using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var apptReminderSent = new ApptReminderSent
            {
                ApptReminderRuleNum = SIn.Long(row["ApptReminderRuleNum"].ToString()),
                ApptNum = SIn.Long(row["ApptNum"].ToString()),
                TSPrior = TimeSpan.FromTicks(SIn.Long(row["TSPrior"].ToString()))
            };
            retVal.Add(apptReminderSent);
        }

        return retVal;
    }
}