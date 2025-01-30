using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;

namespace OpenDentBusiness;

public class SmsFromMobiles
{
    public static List<SmsFromMobile> GetMessages(DateTime dateStart, DateTime dateEnd, List<long> listClinicNums, long patNum, bool isMessageThread, string phoneNumber, List<SmsFromStatus> listSmsFromStatuses)
    {
        var listCommandFilters = new List<string>();
        if (dateStart > DateTime.MinValue) listCommandFilters.Add("DATE(DateTimeReceived)>=" + SOut.Date(dateStart));
        if (dateEnd > DateTime.MinValue) listCommandFilters.Add("DATE(DateTimeReceived)<=" + SOut.Date(dateEnd));
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

    public static void Update(SmsFromMobile smsFromMobile, SmsFromMobile smsFromMobileOld)
    {
        SmsFromMobileCrud.Update(smsFromMobile, smsFromMobileOld);
    }

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