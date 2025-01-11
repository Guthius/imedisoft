using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

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