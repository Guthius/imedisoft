using System.Collections.Generic;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class ApptReminderSents
{
    public static List<ApptReminderSent> GetForApt(params long[] apptNums)
    {
        return apptNums.IsNullOrEmpty() ? [] : ApptReminderSentCrud.SelectMany("SELECT * FROM apptremindersent WHERE ApptNum IN (" + string.Join(",", apptNums) + ") ");
    }
}