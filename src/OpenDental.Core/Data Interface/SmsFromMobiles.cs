using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Newtonsoft.Json;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SmsFromMobiles
{
	/// <summary>
	///     Returns the number of messages which have not yet been read.  If there are no unread messages, then empty
	///     string is returned.  If more than 99 messages are unread, then "99" is returned.  The count limit is 99, because
	///     only 2 digits can fit in the SMS notification text.
	/// </summary>
	public static string GetSmsNotification()
    {
        var command = "SELECT COUNT(*) FROM smsfrommobile WHERE SmsStatus=" + SOut.Int((int) SmsFromStatus.ReceivedUnread);
        var smsUnreadCount = SIn.Int(Db.GetCount(command));
        if (smsUnreadCount == 0) return "";
        if (smsUnreadCount > 99) return "99";
        return smsUnreadCount.ToString();
    }

    ///<summary>Call ProcessInboundSms instead.</summary>
    public static long Insert(SmsFromMobile smsFromMobile)
    {
        return SmsFromMobileCrud.Insert(smsFromMobile);
    }

    /// <summary>
    ///     Gets all SmsFromMobile entries that have been inserted or updated since dateStart, which should be in server
    ///     time.
    /// </summary>
    public static List<SmsFromMobile> GetAllChangedSince(DateTime dateStart)
    {
        var command = "SELECT * from smsfrommobile WHERE SecDateTEdit >= " + SOut.DateT(dateStart);
        return SmsFromMobileCrud.SelectMany(command);
    }

    /// <summary>Gets all SMS incoming messages for the specified filters.</summary>
    /// <param name="dateStart">If dateStart is 01/01/0001, then no start date will be used.</param>
    /// <param name="dateEnd">If dateEnd is 01/01/0001, then no end date will be used.</param>
    /// <param name="listClinicNums">Will filter by clinic only if not empty and patNum is -1.</param>
    /// <param name="patNum">
    ///     If patNum is not -1, then only the messages for the specified patient will be returned, otherwise messages for all
    ///     patients will be returned.
    /// </param>
    /// <param name="isMessageThread">Indicates if this is a message thread.</param>
    /// <param name="phoneNumber">The phone number to search by. Should be just the digits, no formatting.</param>
    /// <param name="arrayStatuses">Messages with these statuses will be found. If none, all statuses will be returned.</param>
    public static List<SmsFromMobile> GetMessages(DateTime dateStart, DateTime dateEnd, List<long> listClinicNums, long patNum,
        bool isMessageThread, string phoneNumber, List<SmsFromStatus> listSmsFromStatuses)
    {
        var listCommandFilters = new List<string>();
        if (dateStart > DateTime.MinValue) listCommandFilters.Add(DbHelper.DtimeToDate("DateTimeReceived") + ">=" + SOut.Date(dateStart));
        if (dateEnd > DateTime.MinValue) listCommandFilters.Add(DbHelper.DtimeToDate("DateTimeReceived") + "<=" + SOut.Date(dateEnd));
        if (patNum == -1)
        {
            //Only limit clinic if not searching for a particular PatNum.
            if (listClinicNums.Count > 0) listCommandFilters.Add("ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ")");
        }
        else
        {
            listCommandFilters.Add($"PatNum = {SOut.Long(patNum)}");
        }

        if (!string.IsNullOrEmpty(phoneNumber)) listCommandFilters.Add($"MobilePhoneNumber='{SOut.String(phoneNumber)}'");
        if (!isMessageThread) //Always show unread in the grid.
            listSmsFromStatuses.Add(SmsFromStatus.ReceivedUnread);
        if (listSmsFromStatuses.Count > 0) listCommandFilters.Add("SmsStatus IN (" + string.Join(",", listSmsFromStatuses.GroupBy(x => x).Select(x => SOut.Int((int) x.Key))) + ")");
        var command = "SELECT * FROM smsfrommobile";
        if (listCommandFilters.Count > 0) command += " WHERE " + string.Join(" AND ", listCommandFilters);
        return SmsFromMobileCrud.SelectMany(command);
    }

    public static string GetSmsFromStatusDescript(SmsFromStatus smsFromStatus)
    {
        if (smsFromStatus == SmsFromStatus.ReceivedUnread) return "Unread";
        if (smsFromStatus == SmsFromStatus.ReceivedRead) return "Read";
        return "";
    }

    ///<summary>Updates only the changed fields of the SMS text message (if any).</summary>
    public static bool Update(SmsFromMobile smsFromMobile, SmsFromMobile smsFromMobileOld)
    {
        return SmsFromMobileCrud.Update(smsFromMobile, smsFromMobileOld);
    }

    ///<summary>Sets the status of the passed in list of SmsFromMobileNums to read.</summary>
    public static void MarkManyAsRead(List<long> listSmsFromMobileNums)
    {
        if (listSmsFromMobileNums.IsNullOrEmpty()) return;

        var command = "UPDATE smsfrommobile "
                      + "SET SmsStatus=" + SOut.Enum(SmsFromStatus.ReceivedRead) + " "
                      + "WHERE SmsFromMobileNum IN (" + string.Join(",", listSmsFromMobileNums) + ")";
        Db.NonQ(command);
    }
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<SmsFromMobile> Refresh(long patNum){

        string command="SELECT * FROM smsfrommobile WHERE PatNum = "+POut.Long(patNum);
        return Crud.SmsFromMobileCrud.SelectMany(command);
    }

    ///<summary>Gets one SmsFromMobile from the db.</summary>
    public static SmsFromMobile GetOne(long smsFromMobileNum){

        return Crud.SmsFromMobileCrud.SelectOne(smsFromMobileNum);
    }



    
    public static void Update(SmsFromMobile smsFromMobile){

        Crud.SmsFromMobileCrud.Update(smsFromMobile);
    }

    
    public static void Delete(long smsFromMobileNum) {

        string command= "DELETE FROM smsfrommobile WHERE SmsFromMobileNum = "+POut.Long(smsFromMobileNum);
        Db.NonQ(command);
    }
    */

    ///<summary>Structured data to be stored as json List in Signalod.MsgValue for InvalidType.SmsTextMsgReceivedUnreadCount.</summary>
    public class SmsNotification
    {
        [JsonProperty(PropertyName = "A")]
        public long ClinicNum { get; set; }

        [JsonProperty(PropertyName = "B")]
        public int Count { get; set; }

        public static string GetJsonFromList(List<SmsNotification> listSmsNotifications)
        {
            return JsonConvert.SerializeObject(listSmsNotifications);
        }

        public static List<SmsNotification> GetListFromJson(string json)
        {
            List<SmsNotification> listSmsNotifications = null;
            ODException.SwallowAnyException(() => listSmsNotifications = JsonConvert.DeserializeObject<List<SmsNotification>>(json));
            return listSmsNotifications;
        }
    }
}