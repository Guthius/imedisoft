﻿using System;
using System.Xml.Serialization;
using OpenDentBusiness.AutoComm;

namespace OpenDentBusiness;

///<summary>Not a database Table, use MsgToPaySent for the database. This class is here to make FeatureAbs happy. 1 to 1 copy.</summary>
public class MsgToPayLite : AutoCommObj
{
    public long MsgToPaySentNum;

    ///<summary>Contact information used for sending a message.</summary>
    public string Contact;

    public MsgToPaySource Source;

    ///<summary>The template that will be used when creating the message.</summary>
    public string TemplateMessage;

    ///<summary>FK to primary key of appropriate table.</summary>
    ///<summary>Subject of the message.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public string Subject;

    ///<summary>Content of the message.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public string Message;

    ///<summary>Only used for manually sent emails.</summary>
    public EmailType EmailType;

    ///<summary>Generated by OD. Timestamp when row is created.</summary>
    [XmlIgnore]
    public DateTime DateTimeEntry;

    ///<summary>DateTime the message was sent.</summary>
    [XmlIgnore]
    public DateTime DateTimeSent;

    ///<summary>Generated by OD in some cases and HQ in others. Any human readable error message generated by either HQ or EConnector. Used for debugging.</summary>
    public string ResponseDescript;

    ///<summary>FK to apptreminderrule.ApptReminderRuleNum. Allows us to look up the rules to determine how to send this apptcomm out.</summary>
    public long ApptReminderRuleNum;

    ///<summary>Generated by HQ. Identifies this AutoCommGuid in future transactions between HQ and OD.</summary>
    public string ShortGUID;

    ///<summary>Deprecated.  Use MessageFK and MessageType instead.FK to message table, ex. smstomobile.GuidMessage. Generated at HQ.  References 
    ///'Mobile' to limit schema changes, since that field already existed and is serialized for payloads sent to WebServiceMainHQ.  May not necessarily 
    ///be an identifier in the smstomobile table, ex. could be an EmailMessage.</summary>
    public string GuidMessageToMobile;

    public long StatementNum;
    public long ApptNum;

    [XmlIgnore]
    public DateTime DateTimeSendFailed;

    public MsgToPayLite()
    {
    }

    public MsgToPayLite(Patient patient)
    {
        NameF = patient.FName;
        NamePreferredOrFirst = patient.GetNameFirstOrPreferred();
        ProvNum = patient.PriProv;
        PatNum = patient.PatNum;
        ClinicNum = patient.ClinicNum;
    }

    public MsgToPayLite(MsgToPaySent sent)
    {
        MsgToPaySentNum = sent.MsgToPaySentNum;
        PrimaryKey = sent.MsgToPaySentNum;
        Contact = sent.Contact;
        Source = sent.Source;
        TemplateMessage = sent.TemplateMessage;
        Subject = sent.Subject;
        Message = sent.Message;
        DateTimeEntry = sent.DateTimeEntry;
        DateTimeSent = sent.DateTimeSent;
        ResponseDescript = sent.ResponseDescript;
        ApptReminderRuleNum = sent.ApptReminderRuleNum;
        ShortGUID = sent.ShortGUID;
        GuidMessageToMobile = sent.GuidMessageToMobile;
        DateTimeSendFailed = sent.DateTimeSendFailed;
        PatNum = sent.PatNum;
        ClinicNum = sent.ClinicNum;
        MessageFk = sent.MessageFk;
        EmailType = sent.EmailType;
        StatementNum = sent.StatementNum;
        ApptNum = sent.ApptNum;
    }

    public MsgToPaySent ToMsgToPaySent()
    {
        MsgToPaySent msgToPaySent = new MsgToPaySent
        {
            MsgToPaySentNum = PrimaryKey,
            Contact = Contact,
            Source = Source,
            TemplateMessage = TemplateMessage,
            Subject = Subject,
            Message = Message,
            DateTimeEntry = DateTimeEntry,
            DateTimeSent = DateTimeSent,
            ResponseDescript = ResponseDescript,
            ApptReminderRuleNum = ApptReminderRuleNum,
            ShortGUID = ShortGUID,
            GuidMessageToMobile = GuidMessageToMobile,
            DateTimeSendFailed = DateTimeSendFailed,
            PatNum = PatNum,
            ClinicNum = ClinicNum,
            MessageFk = MessageFk,
            EmailType = EmailType,
            StatementNum = StatementNum,
            ApptNum = ApptNum,
        };
        return msgToPaySent;
    }
}