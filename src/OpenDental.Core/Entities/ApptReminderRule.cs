using System;
using System.ComponentModel;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ApptReminderRule : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ApptReminderRuleNum;

    ///<summary>Enum:ApptReminderType </summary>
    public ApptReminderType TypeCur;

    ///<summary>Time before appointment that this confirmation should be sent.</summary>
    public TimeSpan TSPrior;

    ///<summary>Comma Delimited List of comm types. Enum values of ApptComm.CommType. 0=pref,1=sms,2=email; Like the deprecated pref "ApptReminderSendOrder"</summary>
    public string SendOrder = "0,1,2";

    ///<summary>Set to True if both an email AND a text should be sent.</summary>
    public bool IsSendAll;

    ///<summary>If using SMS, this template will be used to generate the body of the text message.</summary>
    public string TemplateSMS = "";

    ///<summary>If using email, this template will be used to generate the subject of the email.</summary>
    public string TemplateEmailSubject = "";

    ///<summary>If using email, this template will be used to generate the body of the email.</summary>
    public string TemplateEmail = "";

    ///<summary>FK to clinic.ClinicNum.  Allows reminder rules to be configured on a per clinic basis. If ClinicNum==0 then it is the practice/HQ/default settings.</summary>
    public long ClinicNum;

    ///<summary>Used when aggregating multiple appointments together into a single message.</summary>
    public string TemplateSMSAggShared = "";

    ///<summary>Used when aggregating multiple appointments together into a single message.</summary>
    public string TemplateSMSAggPerAppt = "";

    ///<summary>Used when aggregating multiple appointments together into a single message.</summary>
    public string TemplateEmailSubjAggShared = "";

    ///<summary>Used when aggregating multiple appointments together into a single message.</summary>
    public string TemplateEmailAggShared = "";

    ///<summary>Used when aggregating multiple appointments together into a single message.</summary>
    public string TemplateEmailAggPerAppt = "";

    ///<summary>The time before the appointment in which this reminder should NOT be sent. E.g., if this value is 2 days, and an appt is created one
    ///day in the future, a reminder will not be sent.</summary>
    public TimeSpan DoNotSendWithin;

    ///<summary>Enables/Disables the ApptReminderRule.</summary>
    public bool IsEnabled = true;

    ///<summary>Used when auto replying single eConfirmations.</summary>
    public string TemplateAutoReply = "";

    ///<summary>Used when auto replying multiple patient eConfirmations.</summary>
    public string TemplateAutoReplyAgg = "";

    ///<summary>Used when auto replying to appointment confirmations that failed.</summary>
    public string TemplateFailureAutoReply;

    ///<summary>Enables/Disables eConfirmation auto replies. Only for when the patient responds positively via text.</summary>
    public bool IsAutoReplyEnabled = false;

    ///<summary>When set, matched by text against the patient's language. Typically eng (English), fra (French), spa (Spanish), or similar.  
    /// If it's a custom language, then it might look like Tahitian. 
    /// Empty string implies that this rule uses the default language of the practice.</summary>
    public string Language = "";

    ///<summary>Enum. The Type of email for the template.</summary>
    public EmailType EmailTemplateType;

    ///<summary>Enum:EmailType The type of email for the aggregated template. </summary>
    public EmailType AggEmailTemplateType;

    ///<summary>Used when inviting patient to come into office.</summary>
    
    public string TemplateComeInMessage;

    ///<summary>Boolean false by default. Controls if birthday messages will get sent to a minor for their birthday.</summary>
    public bool IsSendForMinorsBirthday;

    ///<summary>FK to emailhostingtemplate.EmailHostingTemplateNum. If used, rules fields will be based from the template.</summary>
    public long EmailHostingTemplateNum;

    ///<summary>When IsSendForMinorsBirthday is true, this is the age that defines what a minor is.</summary>
    public int MinorAge;

    ///<summary>Enum:SendMultipleInvites . Whether we are able to send multiple invites.</summary>
    public SendMultipleInvites SendMultipleInvites;

    ///<summary>Used in conjunction with CanSendMultipleInvites. We will not send an invite if a patient has visited Patient Portal within this timespan.</summary>
    public TimeSpan TimeSpanMultipleInvites;

    public bool IsValidDuration
    {
        get
        {
            if (TypeCur == ApptReminderType.Birthday)
            {
                return true;
            }

            if (!IsEnabled)
            {
                return false;
            }

            if (TypeCur == ApptReminderType.ConfirmationFutureDay)
            {
                return TSPrior.Days >= 1;
            }

            return true;
        }
    }

    public bool IsSameDay => IsValidDuration && TSPrior.Days == 0;

    public bool IsFutureDay => IsValidDuration && TSPrior.TotalDays >= 1;

    public bool IsPastDay => IsValidDuration && TSPrior.TotalDays <= -1;

    public bool IsAfterApptScheduled => IsValidDuration && TypeCur is ApptReminderType.ScheduleThankYou or ApptReminderType.NewPatientThankYou;

    public int NumDaysInFuture
    {
        get
        {
            if (!IsFutureDay)
            {
                return 0;
            }
            
            return (int) Math.Ceiling(TSPrior.TotalDays);
        }
    }
    
    public int NumDaysInPast
    {
        get
        {
            if (!IsPastDay)
            {
                return 0;
            }

            return (int) Math.Ceiling(Math.Abs(TSPrior.TotalDays));
        }
    }

    public ApptReminderRule Copy()
    {
        return (ApptReminderRule) MemberwiseClone();
    }

    public ApptReminderRule CopyWithClinicNum(long clinicNum)
    {
        var retVal = Copy();
        retVal.ClinicNum = clinicNum;
        return retVal;
    }
}

public enum ApptReminderType
{
    ///<summary>-1 - Used to define an Undefined ApptReminderType.</summary>
    Undefined = -1,

    ///<summary>0 - Used to define the rules for when reminders should be sent out.</summary>
    Reminder = 0,

    ///<summary>1 - Defines rules for when confirmations should be sent out.</summary>
    [Description("Confirmation")]
    ConfirmationFutureDay = 1,

    ///<summary>3 - Send emails to patients with their credentials to the Patient Portal.</summary>
    [Description("Patient Portal Invites")]
    PatientPortalInvite = 3,

    ///<summary>4 - Defines rules for when Schedule Verify ("Thank You"s) should be sent out.</summary>
    [Description("Automated Thank-You")]
    ScheduleThankYou = 4,

    ///<summary>5 - Defines rules for when Arrival instructions should be sent out.</summary>
    [Description("Arrivals")]
    Arrival = 5,

    ///<summary>6 - Birthday. Defines rule for sending out automated birthday emails.</summary>
    [Description("Birthday")]
    Birthday = 6,

    ///<summary>7 - General Message. Defines rules for sending out automated messages after an appointment is set complete.</summary>
    [Description("General Message")]
    GeneralMessage = 7,

    ///<summary>8 - WebSchedRecall. (Note, not yet used in db, but could be if we ever want clinic specific templates) Defines rules for sending out automated messages for Recalls.</summary>
    [Description("Web Sched Recall")]
    WebSchedRecall = 8,

    ///<summary>9 - NewPatientThankYou, Thank you for New Patient which is able to send with a new patient web form URL.</summary>
    [Description("New Patient Thank-You")]
    NewPatientThankYou = 9,

    ///<summary>10 - PaymentPortal Msg-To-Pay, used to send msg-to-pay messages to patients. Currently not an AutoComm feature but doing this now so functionality can be easier added in the future.</summary>
    [ReminderRuleType(IsForAppointment = false)]
    [Description("Payment Portal Msg-To-Pay")]
    PayPortalMsgToPay = 10,
}

public class ReminderRuleTypeAttribute : Attribute
{
    public bool IsForAppointment = true;
}

[Flags]
public enum ShortCodeTypeFlag;

public class ShortCodeAttribute : Attribute
{
    public PrefName[] EServicePrefNames { get; } = [PrefName.NotApplicable];

    public int[] PrefIsEnabledValues { get; } = [];

    public Type EnabledValueType { get; set; }

    public bool IsServiceEnabled(long clinicNum)
    {
        return Clinics.IsTextingEnabled(clinicNum) && EServicePrefNames.Any(x => IsPrefEnabled(x, clinicNum, PrefIsEnabledValues, EnabledValueType));
    }
    
    private static bool IsPrefEnabled(PrefName prefName, long clinicNum, int[] intArrayEnabledValues, Type typeEnabledValue)
    {
        if (prefName == PrefName.NotApplicable)
        {
            return true;
        }

        if (typeEnabledValue is null || intArrayEnabledValues.IsNullOrEmpty())
        {
            //No override type/values defined, so we can consider the preference to be a boolean.
            return ClinicPrefs.GetBool(prefName, clinicNum);
        }

        var arrayObjectsEnumValues = Enum.GetValues(typeEnabledValue);
        var stringPrefValue = ClinicPrefs.GetPrefValue(prefName, clinicNum);
        int? nullableIntPref = null;
        for (var i = 0; i < arrayObjectsEnumValues.Length; i++)
        {
            var objectEnumValue = arrayObjectsEnumValues.GetValue(i);
            if (stringPrefValue == ((int) objectEnumValue).ToString() || stringPrefValue == objectEnumValue.ToString())
            {
                nullableIntPref = (int) objectEnumValue;
                break;
            }
        }

        if (nullableIntPref is null)
        {
            return false;
        }

        return nullableIntPref.Value.In(intArrayEnabledValues);
    }
}

public enum CommType
{
    ///<summary>-1 - Do not use.</summary>
    Invalid = -1,

    ///<summary>0 - Use text OR email based on patient preference.</summary>
    Preferred = 0,

    ///<summary>1 - Attempt to send text message, if successful do not send via email. (Unless, a SendAll bool is used, which usually negates the need for this enumeration.)</summary>
    [CommType(ContactMethod = ContactMethod.TextMessage, Flag = CommTypeFlag.Text)]
    Text = 1,

    ///<summary>2 - Attempt to send email message, if successful do not send via text. (Unless, a SendAll bool is used, which usually negates the need for this enumeration.)</summary>
    [CommType(ContactMethod = ContactMethod.Email, Flag = CommTypeFlag.Email)]
    Email = 2,

    ///<summary>3 - Attempt to send secure email message. </summary>
    [CommType(ContactMethod = ContactMethod.Email, Flag = CommTypeFlag.SecureEmail)]
    [Description("Secure Email")]
    SecureEmail = 3,
}

///<summary>Copy of CommType but as Flags instead. For preference WebSchedManualSendTriggered.</summary>
[Flags]
public enum CommTypeFlag
{
    ///<summary>0 - Use text OR email based on patient preference.</summary>
    None = 0,

    ///<summary>1 - Attempt to send text message, if successful do not send via email. (Unless, a SendAll bool is used, which usually negates the need for this enumeration.)</summary>
    Text = 1,

    ///<summary>2 - Attempt to send email message, if successful do not send via text. (Unless, a SendAll bool is used, which usually negates the need for this enumeration.)</summary>
    Email = 2,

    ///<summary>4 - Attempt to send secure email message. </summary>
    SecureEmail = 4,
}

public class CommTypeAttribute : Attribute
{
    public ContactMethod ContactMethod;
    public CommTypeFlag Flag = CommTypeFlag.None;
}

///<summary>The different modes in which we will send a patient portal invite for this rule.</summary>
public enum SendMultipleInvites
{
    ///<summary>0 - Send a patient portal invite if the patient has not visited patient portal before.</summary>
    UntilPatientVisitsPortal,

    ///<summary>1 - Send a patient portal invite every appointment.</summary>
    EveryAppointment,

    ///<summary>2 - Send a patient portal invite if the patient hasn't visited patient portal within TimeSpanMultipleInvites pref.</summary>
    NoVisitInTimespan,
}