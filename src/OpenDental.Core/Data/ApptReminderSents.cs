using System.Collections.Generic;
using CodeBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ApptReminderSents
{
    public static List<ApptReminderSent> GetForApt(params long[] apptNums)
    {
        return apptNums.IsNullOrEmpty() ? [] : ApptReminderSentCrud.SelectMany("SELECT * FROM apptremindersent WHERE ApptNum IN (" + string.Join(",", apptNums) + ") ");
    }
}