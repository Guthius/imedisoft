using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class WebSchedRecalls
{
    public static List<WebSchedRecall> GetAllUnsent(List<CommType> listCommTypes = null, List<long> listClinicNums = null)
    {
        //Default to All except Invalid if not specified.
        listCommTypes ??= Enum.GetValues(typeof(CommType)).Cast<CommType>().ToList().FindAll(x => x != CommType.Invalid);
        //We don't want to include rows that have a status of SendFailed or SendSuccess
        var command = "SELECT * FROM webschedrecall WHERE DateTimeSent < '1880-01-01' "
                      + $"AND MessageType IN ({string.Join(",", listCommTypes.Select(x => SOut.Int((int) x)))}) "
                      + $"AND SendStatus={SOut.Int((int) AutoCommStatus.SendNotAttempted)} ";
        if (listClinicNums != null && listClinicNums.Count > 0) command += "AND ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") ";

        return WebSchedRecallCrud.SelectMany(command);
    }

    public static List<string> InsertForRecallNums(List<long> listRecallNums, bool isGroupFamily, RecallListSort recallListSort, WebSchedRecallSource webSchedRecallSource, CommType commType, DateTime dateToday)
    {
        var listErrors = new List<string>();
        if (listRecallNums == null || listRecallNums.Count < 1)
        {
            listErrors.Add(Lans.g("WebSched", "No Recalls to schedule for ") + commType.GetDescription());
            return listErrors;
        }

        //Loop through the selected patients and insert WebSchedRecalls so that the Auto Comm Web Sched thread can aggregate the recalls and send
        //messages.
        //Without filtering on messagetype for this in Recalls.GetAddrTAbleForWebSched the last entry will be blocked from getting inserted. 
        var tableAddrs = Recalls.GetAddrTableForWebSched(listRecallNums, isGroupFamily, recallListSort, ListTools.FromSingle(commType));
        var listWebSchedRecalls = new List<WebSchedRecall>();
        for (var i = 0; i < tableAddrs.Rows.Count; i++)
        {
            var dataRow = tableAddrs.Rows[i];
            var recallNum = SIn.Long(dataRow["RecallNum"].ToString());
            var dateDue = SIn.Date(dataRow["dateDue"].ToString());
            if (dateDue.Year < 1880)
                //It is common for offices to have patients with a blank recall date (they've never had a recall performed at the office).
                //Instead of showing 01/01/0001 in the email, we will simply show today's date because that is what the Web Sched time slots will start showing.
                dateDue = dateToday.Date;

            listWebSchedRecalls.Add(new WebSchedRecall
            {
                RecallNum = recallNum,
                ClinicNum = SIn.Long(dataRow["ClinicNum"].ToString()),
                PatNum = SIn.Long(dataRow["PatNum"].ToString()),
                ReminderCount = SIn.Int(dataRow["numberOfReminders"].ToString()),
                DateDue = dateDue,
                DateTimeSent = DateTime.MinValue,
                Source = webSchedRecallSource,
                SendStatus = AutoCommStatus.SendNotAttempted, //AutoCommWebSchedRecall thread will try to send a message.
                MessageType = commType
            });
        }

        InsertMany(listWebSchedRecalls);
        return listErrors;
    }
    
    public static void Insert(WebSchedRecall webSchedRecall)
    {
        WebSchedRecallCrud.Insert(webSchedRecall);
    }
    
    public static void InsertMany(List<WebSchedRecall> listWebSchedRecalls)
    {
        if (listWebSchedRecalls.IsNullOrEmpty()) return;

        WebSchedRecallCrud.InsertMany(listWebSchedRecalls);
    }

    public static bool TemplatesHaveURLTags()
    {
        var listPrefNames = new List<PrefName>();
        listPrefNames.Add(PrefName.WebSchedSubject);
        listPrefNames.Add(PrefName.WebSchedMessage);
        listPrefNames.Add(PrefName.WebSchedMessageText);
        listPrefNames.Add(PrefName.WebSchedAggregatedEmailBody);
        listPrefNames.Add(PrefName.WebSchedAggregatedEmailSubject);
        listPrefNames.Add(PrefName.WebSchedAggregatedTextMessage);
        listPrefNames.Add(PrefName.WebSchedSubject2);
        listPrefNames.Add(PrefName.WebSchedMessage2);
        listPrefNames.Add(PrefName.WebSchedMessageText2);
        listPrefNames.Add(PrefName.WebSchedSubject3);
        listPrefNames.Add(PrefName.WebSchedMessage3);
        listPrefNames.Add(PrefName.WebSchedMessageText3);
        //Check pref values first, because if translations have tags then it is likely the default preferences do too.
        for (var i = 0; i < listPrefNames.Count; i++)
            if (HasURLTag(PrefC.GetString(listPrefNames[i])))
                return true;

        //If no URL tags in pref values, then check translations. Only checking actively used languages.
        var listLanguages = PrefC.GetString(PrefName.LanguagesUsedByPatients).Split(',').ToList();
        var listPrefStrings = listPrefNames.Select(x => x.ToString()).ToList();
        var listLanguagePats = LanguagePats.GetListPrefTranslationsFromDb(listPrefStrings, listLanguages);
        for (var i = 0; i < listLanguagePats.Count; i++)
            if (HasURLTag(listLanguagePats[i].Translation))
                return true;

        return false;
    }

    public static bool HasURLTag(string template)
    {
        return Regex.IsMatch(template, @"\[URL]|\[FamilyListURLs]", RegexOptions.IgnoreCase);
    }
}