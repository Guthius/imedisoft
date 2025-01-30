using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ConfirmationRequests
{
    public static List<ConfirmationRequest> GetAllForAppts(List<long> aptNums)
    {
        return aptNums.Count == 0
            ? []
            : ConfirmationRequestCrud.SelectMany(
                "SELECT * FROM confirmationrequest WHERE ApptNum IN(" + string.Join(",", aptNums) + ")");
    }

    public static List<ConfirmationRequest> GetPendingForRule(long apptReminderRuleNum)
    {
        return ConfirmationRequestCrud.SelectMany(
            "SELECT * FROM confirmationrequest WHERE RSVPStatus = " + (int) RSVPStatusCodes.PendingRsvp + " AND ApptReminderRuleNum=" + apptReminderRuleNum);
    }

    public static void Update(ConfirmationRequest confirmationRequest)
    {
        ConfirmationRequestCrud.Update(confirmationRequest);
    }
}