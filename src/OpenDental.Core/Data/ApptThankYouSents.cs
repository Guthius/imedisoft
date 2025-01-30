using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ApptThankYouSents
{
    public const string AddToCalendar = "[AddToCalendar]";

    public static List<ApptThankYouSent> GetForApt(long aptNum)
    {
        return ApptThankYouSentCrud.SelectMany("SELECT * FROM apptthankyousent WHERE ApptNum=" + aptNum);
    }

    public static void Update(ApptThankYouSent apptThankYouSent)
    {
        ApptThankYouSentCrud.Update(apptThankYouSent);
    }
}