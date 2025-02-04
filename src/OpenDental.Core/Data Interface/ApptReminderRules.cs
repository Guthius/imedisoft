using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness.AutoComm;

namespace OpenDentBusiness;

public class ApptReminderRules
{
    public static List<ApptReminderRule> GetAll()
    {
        return ApptReminderRuleCrud
            .SelectMany("SELECT * FROM apptreminderrule")
            .OrderByDescending(x => new[] {1, 2, 0}.ToList().IndexOf((int) x.TypeCur))
            .ToList();
    }

    public static void SyncByClinicAndTypes(List<ApptReminderRule> apptReminderRulesNew, long clinicNum, params ApptReminderType[] apptReminderTypes)
    {
        if (apptReminderTypes.Length == 0)
        {
            return;
        }

        var apptReminderRulesOld = GetForClinicAndTypes(clinicNum, apptReminderTypes);
        if (!ApptReminderRuleCrud.Sync(apptReminderRulesNew, apptReminderRulesOld))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, string.Join(", ", apptReminderTypes.Select(x => x.GetDescription())) + " rules changed for ClinicNum: " + clinicNum + ".");
    }

    public static ApptReminderRule CreateDefaultReminderRule(ApptReminderType apptReminderTypeRule, long clinicNum = 0, bool isBeforeAppointment = true)
    {
        ApptReminderRule apptReminderRule = null;

        var canUseCalendarTag = ClinicPrefs.GetBool(PrefName.ApptConfirmAutoEnabled, clinicNum);

        var strAddToCalendar = "";
        if (canUseCalendarTag)
        {
            strAddToCalendar = " To add this to your calendar, visit [AddToCalendar].";
        }

        var strAddToCalendarPerAppt = "";
        if (canUseCalendarTag)
        {
            strAddToCalendarPerAppt = " Add to calendar: [AddToCalendar]";
        }

        switch (apptReminderTypeRule)
        {
            case ApptReminderType.Reminder:
                apptReminderRule = new ApptReminderRule
                {
                    ClinicNum = clinicNum,
                    TypeCur = ApptReminderType.Reminder,
                    TSPrior = TimeSpan.FromHours(3),
                    TemplateSMS = "Appointment Reminder: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. [Premed]If you have questions call [ClinicPhone]." + strAddToCalendar,
                    TemplateEmail = @"[NameF],

Your appointment is scheduled for [ApptTime] on [ApptDate] at [OfficeName]." + strAddToCalendar
                                                                             + @" [Premed]If you have questions, call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.",
                    TemplateEmailSubject = "Appointment Reminder", //default subject
                    TemplateSMSAggShared = "Appointment Reminder:\n[Appts]\n [Premed]If you have questions call [ClinicPhone].",
                    TemplateSMSAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt,
                    TemplateEmailSubjAggShared = "Appointment Reminder",
                    TemplateEmailAggShared = @"[Appts]
[Premed]
If you have questions, call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.",
                    TemplateEmailAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt
                };
                break;
            case ApptReminderType.ConfirmationFutureDay:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum;
                apptReminderRule.TypeCur = ApptReminderType.ConfirmationFutureDay;
                apptReminderRule.TSPrior = TimeSpan.FromDays(7);
                apptReminderRule.TemplateSMS = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [OfficeName]. Reply [ConfirmCode] to confirm or call [OfficePhone]." + strAddToCalendar; //default message
                apptReminderRule.TemplateEmail = @"[NameF], 

Your appointment is scheduled for [ApptTime] on [ApptDate] at [OfficeName]. Click <a href=""[ConfirmURL]"">[ConfirmURL]</a> to confirm " +
                                                 @"or call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>." + strAddToCalendar;
                apptReminderRule.TemplateEmailSubject = "Appointment Confirmation";
                apptReminderRule.TemplateSMSAggShared = "[Appts]\nReply [ConfirmCode] to confirm or call [OfficePhone].";
                apptReminderRule.TemplateSMSAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt;
                apptReminderRule.TemplateEmailSubjAggShared = "Appointment Confirmation";
                apptReminderRule.TemplateEmailAggShared = @"[Appts]
Click <a href=""[ConfirmURL]"">[ConfirmURL]</a> to confirm or call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.";
                apptReminderRule.TemplateEmailAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt;
                apptReminderRule.DoNotSendWithin = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(10));
                apptReminderRule.TemplateAutoReply = "Thank you for confirming your appointment with [OfficeName].  We look forward to seeing you." + strAddToCalendar;
                apptReminderRule.TemplateAutoReplyAgg = "Thank you for confirming your appointments with [OfficeName].  We look forward to seeing you" + strAddToCalendarPerAppt;
                apptReminderRule.TemplateFailureAutoReply = "There was an error confirming your appointment with [OfficeName]. Please call [OfficePhone] to confirm.";
                apptReminderRule.IsAutoReplyEnabled = true;
                break;
            case ApptReminderType.PatientPortalInvite:
                if (isBeforeAppointment)
                {
                    apptReminderRule = new ApptReminderRule();
                    apptReminderRule.ClinicNum = clinicNum;
                    apptReminderRule.TypeCur = ApptReminderType.PatientPortalInvite;
                    apptReminderRule.TSPrior = TimeSpan.FromDays(7);
                    apptReminderRule.TimeSpanMultipleInvites = TimeSpan.FromDays(30);
                    apptReminderRule.TemplateEmail = @"[NameF],
			
In preparation for your upcoming dental appointment at [OfficeName], we invite you to visit our <a href=""[PatientPortalURL]"">Patient Portal</a> to see your health information. " + @"
There you can view your scheduled appointments, view your treatment plan, send a message to your provider, and view your account balance. " + @"
If this is your first time using Patient Portal, use the temporary username and password below to log in:

Username: [UserName]
Password: [Password]

If you have any questions, please give us a call at <a href=""tel:[OfficePhone]"">[OfficePhone]</a>, and we would be happy to answer any of your questions.";
                    apptReminderRule.TemplateEmailSubject = "Patient Portal Invitation";
                    apptReminderRule.TemplateEmailSubjAggShared = "Patient Portal Invitation";
                    apptReminderRule.TemplateEmailAggShared = @"[NameF],
			
In preparation for your upcoming dental appointments at [OfficeName], we invite you to visit our <a href=""[PatientPortalURL]"">Patient Portal</a> to see your health information. " + @"
There you can view your scheduled appointments, view your treatment plan, send a message to your provider, and view your account balance. " + @"
If this is your first time using Patient Portal, use these temporary usernames and passwords below to log in:

[Credentials]
If you have any questions, please give us a call at <a href=""tel:[OfficePhone]"">[OfficePhone]</a>, and we would be happy to answer any of your questions.";
                    apptReminderRule.TemplateEmailAggPerAppt = @"[NameF]
User name: [UserName]
Password: [Password]
";
                    apptReminderRule.SendOrder = "2"; //Email only
                    break;
                } //Same day

                apptReminderRule = new ApptReminderRule
                {
                    ClinicNum = clinicNum,
                    TypeCur = ApptReminderType.PatientPortalInvite,
                    TSPrior = new TimeSpan(-1, 0, 0),
                    TimeSpanMultipleInvites = TimeSpan.FromDays(30),
                    TemplateEmail =
                        """
                        [NameF],

                        Thank you for coming in to visit [OfficeName] today. As a follow up to your appointment, we invite you to visit our <a href="[PatientPortalURL]">Patient Portal</a> to see your health information. 

                        There you can view your scheduled appointments, view your treatment plan, send a message to your provider, and view your account balance. 

                        If this is your first time using Patient Portal, use this temporary username and password to log in:

                        Username: [UserName]
                        Password: [Password]

                        If you have any questions, please give us a call at <a href="tel:[OfficePhone]">[OfficePhone]</a>, and we would be happy to answer any of your questions.
                        """,
                    TemplateEmailSubject = "Patient Portal Invitation",
                    TemplateEmailSubjAggShared = "Patient Portal Invitation",
                    TemplateEmailAggShared =
                        """
                        [NameF],

                        Thank you for coming in to visit [OfficeName] today. As a follow up to your appointment, we invite you to visit our <a href="[PatientPortalURL]">Patient Portal</a> to see your health information. 

                        There you can view your scheduled appointments, view your treatment plan, send a message to your provider, and view your account balance. 

                        Visit <a href="[PatientPortalURL]">Patient Portal</a> to see your health information.
                        If this is your first time using Patient Portal, use these temporary usernames and passwords to log in:

                        [Credentials]
                        If you have any questions, please give us a call at <a href="tel:[OfficePhone]">[OfficePhone]</a>, and we would be happy to answer any of your questions.
                        """,
                    TemplateEmailAggPerAppt =
                        """
                        [NameF]
                        User name: [UserName]
                        Password: [Password]

                        """,
                    SendOrder = "2"
                };
                break;

            case ApptReminderType.ScheduleThankYou:
                apptReminderRule = new ApptReminderRule
                {
                    ClinicNum = clinicNum,
                    TypeCur = ApptReminderType.ScheduleThankYou,
                    TSPrior = new TimeSpan(-1, 0, 0),
                    TemplateSMS = "[NameF], thank you for scheduling with [OfficeName] on [ApptDate] at [ApptTime]." + strAddToCalendar,
                    TemplateEmail =
                        $"""
                         [NameF],

                         Thank you for scheduling your appointment with [OfficeName] on [ApptDate] at [ApptTime].
                         {strAddToCalendar} If you have questions, call <a href="tel:[OfficePhone]">[OfficePhone]</a>.
                         """,
                    TemplateEmailSubject = "Appointment Thank You",
                    TemplateSMSAggShared = "Thank you for scheduling these appointments: [Appts]",
                    TemplateSMSAggPerAppt = "[NameF] for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt,
                    TemplateEmailSubjAggShared = "Appointment Thank You",
                    TemplateEmailAggShared =
                        """
                        Thank you for scheduling these appointments: 
                        [Appts]
                        If you have questions, call <a href="tel:[OfficePhone]">[OfficePhone]</a>.
                        """,
                    TemplateEmailAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt,
                    DoNotSendWithin = new TimeSpan(2, 0, 0)
                };
                break;

            case ApptReminderType.NewPatientThankYou:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum;
                apptReminderRule.TypeCur = ApptReminderType.NewPatientThankYou;
                apptReminderRule.TSPrior = new TimeSpan(-1, 0, 0);
                apptReminderRule.TemplateSMS = @"[NameF] has an appointment coming up. Please fill out this form prior to the appointment [NewPatWebFormURL] ";
                apptReminderRule.TemplateEmail = @"[NameF] has an appointment coming up. Please fill out this form prior to the appointment <a href=""[NewPatWebFormURL]"">[NewPatWebFormURL]</a> ";
                apptReminderRule.TemplateEmailSubject = "New Patient Thank You";
                apptReminderRule.TemplateSMSAggShared = "Thank you for scheduling your appointments. \nPlease fill out these forms for each patient: [Appts]";
                apptReminderRule.TemplateSMSAggPerAppt = @"[NameF] [NewPatWebFormURL]";
                apptReminderRule.TemplateEmailSubjAggShared = "New Patient, Thank you";
                apptReminderRule.TemplateEmailAggShared =
                    """
                    Thank you for scheduling your appointments. 
                    Please fill out these forms for each patient:
                    [Appts]
                    """;
                apptReminderRule.TemplateEmailAggPerAppt = "[NameF] <a href=\"[NewPatWebFormURL]\">[NewPatWebFormURL]</a>";
                apptReminderRule.DoNotSendWithin = new TimeSpan(2, 0, 0);
                break;

            case ApptReminderType.Arrival:
                apptReminderRule = new ApptReminderRule
                {
                    ClinicNum = clinicNum,
                    TypeCur = ApptReminderType.Arrival,
                    TSPrior = TimeSpan.FromHours(3),
                    TemplateSMS = $"[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. When you arrive, please respond with {ArrivalsTagReplacer.ArrivedTag}. If you have questions call [ClinicPhone].",
                    TemplateEmail = "",
                    TemplateEmailSubject = "",
                    TemplateSMSAggShared = $"[Appts]\nWhen you arrive, please respond with {ArrivalsTagReplacer.ArrivedTag}. If you have questions call [ClinicPhone].",
                    TemplateSMSAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
                    TemplateEmailSubjAggShared = "",
                    TemplateEmailAggShared = "",
                    SendOrder = ((int) CommType.Text).ToString(),
                    TemplateAutoReply = "Please remain outside the office.  You will be contacted shortly to come in for your appointment.",
                    TemplateComeInMessage = "Your appointment is ready. Please come in.",
                    IsAutoReplyEnabled = true
                };
                break;

            case ApptReminderType.Birthday:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum;
                apptReminderRule.TypeCur = ApptReminderType.Birthday;
                apptReminderRule.TSPrior = TimeSpan.FromDays(0);
                //No text gets stored in the templates fields. Text comes from Email Hosting Template.
                apptReminderRule.IsAutoReplyEnabled = false;
                apptReminderRule.EmailHostingTemplateNum = 0; //set to 0 for now because this should only ever get called during testing.
                apptReminderRule.SendOrder = ((int) CommType.Email).ToString(); //Email Only
                break;
            case ApptReminderType.GeneralMessage:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum;
                apptReminderRule.TypeCur = ApptReminderType.GeneralMessage;
                apptReminderRule.TSPrior = TimeSpan.FromHours(-1); //Send 1 hour after the appointment
                apptReminderRule.SendOrder = "0,1,2"; //part of ctor
                apptReminderRule.TemplateSMS = "Thank you for your visit to [OfficeName]. If you have questions call [OfficePhone]."; //default message
                apptReminderRule.TemplateSMSAggShared = "Thank you for your visit to [OfficeName]. If you have questions call [OfficePhone].";
                apptReminderRule.TemplateSMSAggPerAppt = "";
                apptReminderRule.TemplateEmailSubject = "Thank You For Your Visit"; //default subject
                apptReminderRule.TemplateEmail = @"Thank you for your visit to [OfficeName]. We look forward to seeing you again. If you have any questions, please call us at <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.";
                apptReminderRule.TemplateEmailSubjAggShared = "Thank You For Your Visit";
                apptReminderRule.TemplateEmailAggShared = @"Thank you for your visit to [OfficeName]. We look forward to seeing you again. If you have any questions, please call us at <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.";
                apptReminderRule.TemplateEmailAggPerAppt = "";
                apptReminderRule.IsAutoReplyEnabled = false;
                break;
        }

        if (PrefC.GetBool(PrefName.EmailDisclaimerIsOn))
        {
            apptReminderRule.TemplateEmail += "\r\n\r\n\r\n[EmailDisclaimer]";
            apptReminderRule.TemplateEmailAggShared += "\r\n\r\n\r\n[EmailDisclaimer]";
        }

        return apptReminderRule;
    }

    public static List<string> GetAvailableTags(ApptReminderType apptReminderType)
    {
        var replacementTags = new List<string>
        {
            "[NameF]",
            "[NamePreferredOrFirst]",
            "[ClinicName]",
            "[ClinicPhone]",
            "[OfficeName]",
            "[ProvName]",
            "[ProvAbbr]",
            "[PracticeName]",
            "[PracticePhone]",
            "[OfficePhone]"
        };

        if (IsForAppointment(apptReminderType))
        {
            replacementTags.Add("[ApptTime]");
            replacementTags.Add("[ApptTimeAskedArrive]");
            replacementTags.Add("[ApptDate]");
        }

        switch (apptReminderType)
        {
            case ApptReminderType.Reminder:
                replacementTags.Add("[Premed]");
                replacementTags.Add(ApptThankYouSents.AddToCalendar);
                break;

            case ApptReminderType.ConfirmationFutureDay:
                replacementTags.Add("[ConfirmCode]");
                replacementTags.Add("[ConfirmURL]");
                replacementTags.Add(ApptThankYouSents.AddToCalendar);
                break;

            case ApptReminderType.PatientPortalInvite:
                replacementTags.Add("[UserName]");
                replacementTags.Add("[Password]");
                replacementTags.Add("[PatientPortalURL]");
                break;

            case ApptReminderType.ScheduleThankYou:
                replacementTags.Add(ApptThankYouSents.AddToCalendar);
                break;

            case ApptReminderType.Arrival:
                replacementTags.Add(ArrivalsTagReplacer.ArrivedTag);
                break;

            case ApptReminderType.PayPortalMsgToPay:
                replacementTags.Add(MsgToPayTagReplacer.MsgToPayTag);
                replacementTags.Add(MsgToPayTagReplacer.MonthlyCardTag);
                replacementTags.Add(MsgToPayTagReplacer.NamePrefTag);
                replacementTags.Add(MsgToPayTagReplacer.PatnumTag);
                replacementTags.Add(MsgToPayTagReplacer.CurmonthTag);
                replacementTags.Add(MsgToPayTagReplacer.StatementUrlTag);
                replacementTags.Add(MsgToPayTagReplacer.StatementShortTag);
                replacementTags.Add(MsgToPayTagReplacer.StatementBalanceTag);
                replacementTags.Add(MsgToPayTagReplacer.StatementInsEstTag);
                break;
        }

        replacementTags.Sort();

        return replacementTags;
    }

    public static bool IsForAppointment(ApptReminderType apptReminderType)
    {
        return EnumTools.GetAttributeOrDefault<ReminderRuleTypeAttribute>(apptReminderType).IsForAppointment;
    }

    public static bool IsReminderTypeAlwaysSendBefore(ApptReminderType apptReminderType)
    {
        return apptReminderType is
            ApptReminderType.Reminder or
            ApptReminderType.ConfirmationFutureDay or
            ApptReminderType.Arrival or
            ApptReminderType.Birthday or
            ApptReminderType.ScheduleThankYou or
            ApptReminderType.NewPatientThankYou or
            ApptReminderType.WebSchedRecall;
    }

    public static List<string> GetAvailableAggTags(ApptReminderType apptReminderType)
    {
        var replacementTags = GetAvailableTags(apptReminderType);

        replacementTags.Add("[Appts]");
        replacementTags.Sort();

        return replacementTags;
    }

    public static bool IsAddToCalendarTagSupported(ApptReminderType apptReminderType)
    {
        return apptReminderType is ApptReminderType.Reminder or ApptReminderType.ScheduleThankYou or ApptReminderType.ConfirmationFutureDay;
    }

    public static List<ApptReminderRule> GetForTypes(params ApptReminderType[] apptReminderTypes)
    {
        return apptReminderTypes.Length == 0
            ? []
            : ApptReminderRuleCrud.SelectMany(
                "SELECT * FROM apptreminderrule " +
                "WHERE TypeCur IN (" + string.Join(",", apptReminderTypes.Cast<int>()) + ")");
    }

    public static List<ApptReminderRule> GetForClinicAndTypes(long clinicNum, params ApptReminderType[] apptReminderTypes)
    {
        if (apptReminderTypes.Length == 0)
        {
            return [];
        }

        return ApptReminderRuleCrud.SelectMany(
            "SELECT * FROM apptreminderrule " +
            "WHERE ClinicNum=" + clinicNum + " " +
            "AND TypeCur IN (" + string.Join(",", apptReminderTypes.Cast<int>()) + ")");
    }
}