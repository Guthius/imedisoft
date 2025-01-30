using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class WebSchedRecalls
{
    public static List<WebSchedRecall> GetAllUnsent(List<CommType> commTypes = null, List<long> clinicNums = null)
    {
        commTypes ??= Enum.GetValues(typeof(CommType)).Cast<CommType>().ToList().FindAll(x => x != CommType.Invalid);
        
        var command = 
            $"""
             SELECT * FROM webschedrecall 
             WHERE DateTimeSent < '1880-01-01' 
             AND MessageType IN ({string.Join(",", commTypes.Cast<int>())}) 
             AND SendStatus={(int) AutoCommStatus.SendNotAttempted} 
             """;
        
        if (clinicNums is {Count: > 0})
        {
            command += "AND ClinicNum IN(" + string.Join(",", clinicNums) + ")";
        }

        return WebSchedRecallCrud.SelectMany(command);
    }

    public static List<string> InsertForRecallNums(List<long> recallNums, bool isGroupFamily, RecallListSort recallListSort, WebSchedRecallSource webSchedRecallSource, CommType commType, DateTime dateToday)
    {
        var errors = new List<string>();
        
        if (recallNums == null || recallNums.Count < 1)
        {
            errors.Add("No Recalls to schedule for " + commType.GetDescription());
            return errors;
        }
        
        var dataTable = Recalls.GetAddrTableForWebSched(recallNums, isGroupFamily, recallListSort, ListTools.FromSingle(commType));
        var webSchedRecalls = new List<WebSchedRecall>();
        
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var dataRow = dataTable.Rows[i];
            var recallNum = SIn.Long(dataRow["RecallNum"].ToString());
            
            var dateDue = SIn.Date(dataRow["dateDue"].ToString());
            if (dateDue.Year < 1880)
            {
                dateDue = dateToday.Date;
            }

            webSchedRecalls.Add(new WebSchedRecall
            {
                RecallNum = recallNum,
                ClinicNum = SIn.Long(dataRow["ClinicNum"].ToString()),
                PatNum = SIn.Long(dataRow["PatNum"].ToString()),
                ReminderCount = SIn.Int(dataRow["numberOfReminders"].ToString()),
                DateDue = dateDue,
                DateTimeSent = DateTime.MinValue,
                Source = webSchedRecallSource,
                SendStatus = AutoCommStatus.SendNotAttempted,
                MessageType = commType
            });
        }

        InsertMany(webSchedRecalls);
        
        return errors;
    }

    public static void InsertMany(List<WebSchedRecall> webSchedRecalls)
    {
        if (webSchedRecalls.IsNullOrEmpty())
        {
            return;
        }

        WebSchedRecallCrud.InsertMany(webSchedRecalls);
    }

    public static bool TemplatesHaveUrlTags()
    {
        var prefNames = new List<PrefName>
        {
            PrefName.WebSchedSubject,
            PrefName.WebSchedMessage,
            PrefName.WebSchedMessageText,
            PrefName.WebSchedAggregatedEmailBody,
            PrefName.WebSchedAggregatedEmailSubject,
            PrefName.WebSchedAggregatedTextMessage,
            PrefName.WebSchedSubject2,
            PrefName.WebSchedMessage2,
            PrefName.WebSchedMessageText2,
            PrefName.WebSchedSubject3,
            PrefName.WebSchedMessage3,
            PrefName.WebSchedMessageText3
        };
        
        foreach (var prefName in prefNames)
        {
            if (HasUrlTag(PrefC.GetString(prefName)))
            {
                return true;
            }
        }
        
        var languages = PrefC.GetString(PrefName.LanguagesUsedByPatients).Split(',').ToList();
        var prefStrings = prefNames.Select(x => x.ToString()).ToList();
        var languagePats = LanguagePats.GetListPrefTranslationsFromDb(prefStrings, languages);
        
        foreach (var languagePat in languagePats)
        {
            if (HasUrlTag(languagePat.Translation))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasUrlTag(string template)
    {
        return Regex.IsMatch(template, @"\[URL]|\[FamilyListURLs]", RegexOptions.IgnoreCase);
    }
}