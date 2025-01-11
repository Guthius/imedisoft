using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.AutoComm;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ApptReminderRules
{
    public static bool UsesApptReminders()
    {
        var enabled = PrefC.GetBool(PrefName.ApptRemindAutoEnabled);
        if (!enabled)
        {
            return false;
        }
        
        return Db.GetLong("SELECT count(*) FROM ApptReminderRule WHERE TSPrior>0") > 0;
    }


    public static List<ApptReminderRule> GetAll()
    {
        var command = "SELECT * FROM apptreminderrule ";
        return ApptReminderRuleCrud.SelectMany(command).OrderByDescending(x => new[] {1, 2, 0}.ToList().IndexOf((int) x.TypeCur)).ToList();
    }
    
    public static List<bool> Get_16_3_29_ConversionFlags()
    {
        var command = "SELECT ApptReminderRuleNum, TypeCur, TSPrior, ClinicNum FROM apptreminderrule WHERE TypeCur=0";
        var groups = DataCore.GetTable(command).Select().Select(x => new
            {
                ApptReminderRuleNum = SIn.Long(x[0].ToString()),
                TypeCur = SIn.Int(x[1].ToString()),
                TSPrior = TimeSpan.FromTicks(SIn.Long(x[2].ToString())),
                ClinicNum = SIn.Long(x[3].ToString())
            })
            //All rules grouped by clinic and whether they are same day or future day.
            .GroupBy(x => new {ClincNum = x.ClinicNum, IsSameDay = x.TSPrior.TotalDays < 1});
        return new List<bool>
        {
            //Any 1 single clinic has more than 1 same day reminder.
            groups.Any(x => x.Key.IsSameDay && x.Count() > 1),
            //Any 1 single clinic has more than 1 future day reminder.
            groups.Any(x => !x.Key.IsSameDay && x.Count() > 1)
        };
    }

    public static void SyncByClinicAndTypes(List<ApptReminderRule> listApptReminderRulesNew, long clinicNum, params ApptReminderType[] apptReminderTypesArray)
    {
        if (apptReminderTypesArray.Length == 0) return;
        var listApptReminderRulesOld = GetForClinicAndTypes(clinicNum, apptReminderTypesArray); //ClinicNum can be 0
        if (ApptReminderRuleCrud.Sync(listApptReminderRulesNew, listApptReminderRulesOld))
        {
            SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, string.Join(", ", apptReminderTypesArray.Select(x => x.GetDescription()))
                                                             + " rules changed for ClinicNum: " + clinicNum + ".");
            return;
        }
    }
    
    public static void Insert(ApptReminderRule apptReminderRule)
    {
        ApptReminderRuleCrud.Insert(apptReminderRule);
    }

    public static ApptReminderRule CreateDefaultReminderRule(ApptReminderType apptReminderTypeRule, long clinicNum = 0, bool isBeforeAppointment = true)
    {
        ApptReminderRule apptReminderRule = null;
        var canUseCalendarTag = PrefC.GetBool(PrefName.ApptConfirmAutoEnabled);
        if (true) canUseCalendarTag = ClinicPrefs.GetBool(PrefName.ApptConfirmAutoEnabled, clinicNum);
        var strAddToCalendar = "";
        if (canUseCalendarTag) strAddToCalendar = " To add this to your calendar, visit [AddToCalendar].";
        var strAddToCalendarPerAppt = "";
        if (canUseCalendarTag) strAddToCalendarPerAppt = " Add to calendar: [AddToCalendar]";
        switch (apptReminderTypeRule)
        {
            case ApptReminderType.Reminder:
                apptReminderRule = new ApptReminderRule
                {
                    ClinicNum = clinicNum, //works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
                    TypeCur = ApptReminderType.Reminder,
                    TSPrior = TimeSpan.FromHours(3),
                    TemplateSMS = "Appointment Reminder: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. [Premed]If you have questions call [ClinicPhone]."
                                  + strAddToCalendar, //default message
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
                    //SendOrder="0,1,2" //part of ctor
                };
                break;
            case ApptReminderType.ConfirmationFutureDay:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum; //works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
                apptReminderRule.TypeCur = ApptReminderType.ConfirmationFutureDay;
                apptReminderRule.TSPrior = TimeSpan.FromDays(7);
                apptReminderRule.TemplateSMS = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [OfficeName]. Reply [ConfirmCode] to confirm or call [OfficePhone]." + strAddToCalendar; //default message
                apptReminderRule.TemplateEmail = @"[NameF], 

Your appointment is scheduled for [ApptTime] on [ApptDate] at [OfficeName]. Click <a href=""[ConfirmURL]"">[ConfirmURL]</a> to confirm " +
                                                 @"or call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>." + strAddToCalendar;
                apptReminderRule.TemplateEmailSubject = "Appointment Confirmation"; //default subject
                apptReminderRule.TemplateSMSAggShared = "[Appts]\nReply [ConfirmCode] to confirm or call [OfficePhone].";
                apptReminderRule.TemplateSMSAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt;
                apptReminderRule.TemplateEmailSubjAggShared = "Appointment Confirmation";
                apptReminderRule.TemplateEmailAggShared = @"[Appts]
Click <a href=""[ConfirmURL]"">[ConfirmURL]</a> to confirm or call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.";
                apptReminderRule.TemplateEmailAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt;
                //SendOrder="0,1,2" //part of ctor
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

                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum;
                apptReminderRule.TypeCur = ApptReminderType.PatientPortalInvite;
                apptReminderRule.TSPrior = new TimeSpan(-1, 0, 0); //Send 1 hour after the appointment
                apptReminderRule.TimeSpanMultipleInvites = TimeSpan.FromDays(30);
                apptReminderRule.TemplateEmail = @"[NameF],
			
Thank you for coming in to visit [OfficeName] today. As a follow up to your appointment, we invite you to visit our <a href=""[PatientPortalURL]"">Patient Portal</a> to see your health information. " + @"
There you can view your scheduled appointments, view your treatment plan, send a message to your provider, and view your account balance. " + @"
If this is your first time using Patient Portal, use this temporary username and password to log in:

Username: [UserName]
Password: [Password]

If you have any questions, please give us a call at <a href=""tel:[OfficePhone]"">[OfficePhone]</a>, and we would be happy to answer any of your questions.";
                apptReminderRule.TemplateEmailSubject = "Patient Portal Invitation";
                apptReminderRule.TemplateEmailSubjAggShared = "Patient Portal Invitation";
                apptReminderRule.TemplateEmailAggShared = @"[NameF],
			
Thank you for coming in to visit [OfficeName] today. As a follow up to your appointment, we invite you to visit our <a href=""[PatientPortalURL]"">Patient Portal</a> to see your health information. " + @"
There you can view your scheduled appointments, view your treatment plan, send a message to your provider, and view your account balance. " + @"
Visit <a href=""[PatientPortalURL]"">Patient Portal</a> to see your health information.
If this is your first time using Patient Portal, use these temporary usernames and passwords to log in:

[Credentials]
If you have any questions, please give us a call at <a href=""tel:[OfficePhone]"">[OfficePhone]</a>, and we would be happy to answer any of your questions.";
                apptReminderRule.TemplateEmailAggPerAppt = @"[NameF]
User name: [UserName]
Password: [Password]
";
                apptReminderRule.SendOrder = "2"; //Email only
                break;
            case ApptReminderType.ScheduleThankYou:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum; //works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
                apptReminderRule.TypeCur = ApptReminderType.ScheduleThankYou;
                apptReminderRule.TSPrior = new TimeSpan(-1, 0, 0); //default to send thank you 1 hour after creating appointment.
                apptReminderRule.TemplateSMS = "[NameF], thank you for scheduling with [OfficeName] on [ApptDate] at [ApptTime]."
                                               + strAddToCalendar; //default message
                apptReminderRule.TemplateEmail = @"[NameF],

Thank you for scheduling your appointment with [OfficeName] on [ApptDate] at [ApptTime]." + strAddToCalendar
                                                                                          + @" If you have questions, call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.";
                apptReminderRule.TemplateEmailSubject = "Appointment Thank You"; //default subject
                apptReminderRule.TemplateSMSAggShared = "Thank you for scheduling these appointments: [Appts]";
                apptReminderRule.TemplateSMSAggPerAppt = "[NameF] for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt;
                apptReminderRule.TemplateEmailSubjAggShared = "Appointment Thank You";
                apptReminderRule.TemplateEmailAggShared = @"Thank you for scheduling these appointments: 
[Appts]
If you have questions, call <a href=""tel:[OfficePhone]"">[OfficePhone]</a>.";
                apptReminderRule.TemplateEmailAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]." + strAddToCalendarPerAppt;
                //SendOrder="0,1,2" //part of ctor
                apptReminderRule.DoNotSendWithin = new TimeSpan(2, 0, 0); //Do not send within 2 hours of appointment.AptDateTime.
                break;
            case ApptReminderType.NewPatientThankYou:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum; //works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
                apptReminderRule.TypeCur = ApptReminderType.NewPatientThankYou;
                apptReminderRule.TSPrior = new TimeSpan(-1, 0, 0); //default to send thank you 1 hour after creating appointment.
                apptReminderRule.TemplateSMS = @"[NameF] has an appointment coming up. Please fill out this form prior to the appointment [NewPatWebFormURL] "; //default message
                apptReminderRule.TemplateEmail = @"[NameF] has an appointment coming up. Please fill out this form prior to the appointment <a href=""[NewPatWebFormURL]"">[NewPatWebFormURL]</a> ";
                apptReminderRule.TemplateEmailSubject = "New Patient Thank You"; //default subject
                apptReminderRule.TemplateSMSAggShared = "Thank you for scheduling your appointments. \nPlease fill out these forms for each patient: [Appts]";
                apptReminderRule.TemplateSMSAggPerAppt = @"[NameF] [NewPatWebFormURL]";
                apptReminderRule.TemplateEmailSubjAggShared = "New Patient, Thank you";
                apptReminderRule.TemplateEmailAggShared = @"Thank you for scheduling your appointments. 
Please fill out these forms for each patient:
[Appts]";
                apptReminderRule.TemplateEmailAggPerAppt = @"[NameF] <a href=""[NewPatWebFormURL]"">[NewPatWebFormURL]</a>";
                //SendOrder="0,1,2" //part of ctor
                apptReminderRule.DoNotSendWithin = new TimeSpan(2, 0, 0); //Do not send within 2 hours of appointment.AptDateTime.
                break;
            case ApptReminderType.Arrival:
                apptReminderRule = new ApptReminderRule();
                apptReminderRule.ClinicNum = clinicNum; //works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
                apptReminderRule.TypeCur = ApptReminderType.Arrival;
                apptReminderRule.TSPrior = TimeSpan.FromHours(3);
                apptReminderRule.TemplateSMS = $"[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. When you arrive, please respond with " +
                                               $"{ArrivalsTagReplacer.ARRIVED_TAG}. If you have questions call [ClinicPhone]."; //default message
                apptReminderRule.TemplateEmail = ""; // N/A
                apptReminderRule.TemplateEmailSubject = ""; // N/A
                apptReminderRule.TemplateSMSAggShared = $"[Appts]\nWhen you arrive, please respond with {ArrivalsTagReplacer.ARRIVED_TAG}. If you " +
                                                        "have questions call [ClinicPhone].";
                apptReminderRule.TemplateSMSAggPerAppt = "[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].";
                apptReminderRule.TemplateEmailSubjAggShared = ""; // N/A
                apptReminderRule.TemplateEmailAggShared = ""; // N/A
                apptReminderRule.SendOrder = ((int) CommType.Text).ToString(); //SMS Only
                apptReminderRule.TemplateAutoReply = "Please remain outside the office.  You will be contacted shortly to come in for your appointment.";
                apptReminderRule.TemplateComeInMessage = "Your appointment is ready. Please come in.";
                apptReminderRule.IsAutoReplyEnabled = true;
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

    /// <summary>Returns the list of replacement tags available for the passed in ApptReminderRuleType.</summary>
    public static List<string> GetAvailableTags(ApptReminderType apptReminderType)
    {
        var listStringsReplacementTags = new List<string>();
        listStringsReplacementTags.Add("[NameF]");
        listStringsReplacementTags.Add("[NamePreferredOrFirst]");
        listStringsReplacementTags.Add("[ClinicName]");
        listStringsReplacementTags.Add("[ClinicPhone]");
        listStringsReplacementTags.Add("[OfficeName]");
        listStringsReplacementTags.Add("[ProvName]");
        listStringsReplacementTags.Add("[ProvAbbr]");
        listStringsReplacementTags.Add("[PracticeName]");
        listStringsReplacementTags.Add("[PracticePhone]");
        listStringsReplacementTags.Add("[OfficePhone]");
        if (IsForAppointment(apptReminderType))
        {
            listStringsReplacementTags.Add("[ApptTime]");
            listStringsReplacementTags.Add("[ApptTimeAskedArrive]");
            listStringsReplacementTags.Add("[ApptDate]");
        }

        switch (apptReminderType)
        {
            case ApptReminderType.Reminder:
                listStringsReplacementTags.Add("[Premed]");
                listStringsReplacementTags.Add(ApptThankYouSents.AddToCalendar);
                break;
            case ApptReminderType.ConfirmationFutureDay:
                listStringsReplacementTags.Add("[ConfirmCode]");
                listStringsReplacementTags.Add("[ConfirmURL]");
                listStringsReplacementTags.Add(ApptThankYouSents.AddToCalendar);
                break;
            case ApptReminderType.PatientPortalInvite:
                listStringsReplacementTags.Add("[UserName]");
                listStringsReplacementTags.Add("[Password]");
                listStringsReplacementTags.Add("[PatientPortalURL]");
                break;
            case ApptReminderType.ScheduleThankYou:
                listStringsReplacementTags.Add(ApptThankYouSents.AddToCalendar);
                break;
            case ApptReminderType.Arrival:
                listStringsReplacementTags.Add(ArrivalsTagReplacer.ARRIVED_TAG);
                break;
            case ApptReminderType.PayPortalMsgToPay:
                listStringsReplacementTags.Add(MsgToPayTagReplacer.MSG_TO_PAY_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.MONTHLY_CARD_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.NAME_PREF_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.PATNUM_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.CURMONTH_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.STATEMENT_URL_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.STATEMENT_SHORT_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.STATEMENT_BALANCE_TAG);
                listStringsReplacementTags.Add(MsgToPayTagReplacer.STATEMENT_INS_EST_TAG);
                break;
        }

        listStringsReplacementTags.Sort(); //alphabetical
        return listStringsReplacementTags;
    }

    /// <summary>
    ///     Returns true if the remindertype is reliant on having a reminder. This is pretty much only for PaymentPortal
    ///     Text To Pay currently.
    /// </summary>
    public static bool IsForAppointment(ApptReminderType apptReminderType)
    {
        return EnumTools.GetAttributeOrDefault<ReminderRuleTypeAttribute>(apptReminderType).IsForAppointment;
    }

    public static bool IsReminderTypeAlwaysSendBefore(ApptReminderType apptReminderType)
    {
        return apptReminderType.In(ApptReminderType.Reminder,
            ApptReminderType.ConfirmationFutureDay,
            ApptReminderType.Arrival,
            ApptReminderType.Birthday,
            ApptReminderType.ScheduleThankYou,
            ApptReminderType.NewPatientThankYou,
            ApptReminderType.WebSchedRecall);
    }

    public static bool IsReminderTypeAlwaysSendAfter(ApptReminderType apptReminderType)
    {
        return apptReminderType.In(ApptReminderType.GeneralMessage);
    }

    /// <summary>
    ///     Returns the list of replacement tags available for the Aggregate Templates for the passed in
    ///     ApptReminderRuleType.
    /// </summary>
    public static List<string> GetAvailableAggTags(ApptReminderType apptReminderType)
    {
        var listReplacementTags = GetAvailableTags(apptReminderType);
        listReplacementTags.Add("[Appts]"); //[Appts] is used for child nodes
        listReplacementTags.Sort();
        return listReplacementTags;
    }

    public static bool IsAddToCalendarTagSupported(ApptReminderType apptReminderType)
    {
        return apptReminderType.In(ApptReminderType.Reminder, ApptReminderType.ScheduleThankYou, ApptReminderType.ConfirmationFutureDay);
    }

    #region Get Methods

    ///<summary>Gets all, sorts by TSPrior Desc, Should never be more than 3 (per clinic if this is implemented for clinics.)</summary>
    public static List<ApptReminderRule> GetForTypes(params ApptReminderType[] apptReminderTypesArray)
    {
        if (apptReminderTypesArray.Length == 0) return new List<ApptReminderRule>();
        var command = "SELECT * FROM apptreminderrule WHERE TypeCur IN(" + string.Join(",", apptReminderTypesArray.Select(x => SOut.Int((int) x))) + ")";
        return ApptReminderRuleCrud.SelectMany(command);
    }

    ///<summary>Gets all from the DB for the given clinic and types.</summary>
    public static List<ApptReminderRule> GetForClinicAndTypes(long clinicNum, params ApptReminderType[] apptReminderTypesArray)
    {
        if (apptReminderTypesArray.Length == 0) return new List<ApptReminderRule>();
        var command = "SELECT * FROM apptreminderrule WHERE ClinicNum=" + SOut.Long(clinicNum)
                                                                        + " AND TypeCur IN(" + string.Join(",", apptReminderTypesArray.Select(x => SOut.Int((int) x))) + ")";
        return ApptReminderRuleCrud.SelectMany(command).ToList();
    }

    #endregion
}