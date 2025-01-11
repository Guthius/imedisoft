using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ConfirmationRequests
{
    #region Get Methods

    ///<summary>Gets all confirmation requests for the AptNums sent in.</summary>
    public static List<ConfirmationRequest> GetAllForAppts(List<long> listAptNums)
    {
        if (listAptNums.Count == 0) return new List<ConfirmationRequest>();

        var command = "SELECT * FROM confirmationrequest WHERE ApptNum IN(" + string.Join(",", listAptNums.Select(x => SOut.Long(x))) + ")";
        return ConfirmationRequestCrud.SelectMany(command);
    }

    #endregion

    ///<summary>Get all rows where RSVPStatus==PendingRsvp that match the apptReminderRule.</summary>
    public static List<ConfirmationRequest> GetPendingForRule(long apptReminderRuleNum)
    {
        var command = "SELECT * FROM confirmationrequest WHERE RSVPStatus = " + SOut.Int((int) RSVPStatusCodes.PendingRsvp)
                                                                              + " AND ApptReminderRuleNum=" + SOut.Long(apptReminderRuleNum);
        return ConfirmationRequestCrud.SelectMany(command);
    }

    public static long Insert(ConfirmationRequest confirmationRequest)
    {
        return ConfirmationRequestCrud.Insert(confirmationRequest);
    }

    public static void Update(ConfirmationRequest confirmationRequest)
    {
        ConfirmationRequestCrud.Update(confirmationRequest);
    }
}