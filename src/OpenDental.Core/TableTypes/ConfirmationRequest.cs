﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using CodeBase;
using OpenDentBusiness.WebTypes.AutoComm;

namespace OpenDentBusiness {
	///<summary>Requests that have been sent via EConnector to HQ. HQ will process and update status as responses become available.</summary>
	[Serializable,CrudTable(HasBatchWriteMethods=true)]
	public class ConfirmationRequest:TableBase,IAutoCommApptGuid {
		///<summary>PK. Generated by HQ.</summary>
		[CrudColumn(IsPriKey=true)]
		public long ConfirmationRequestNum;
		///<summary>Generated by OD. Typically the time of the appointment. This is the time at which HQ will consider this unconfirmed and auto terminate.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.DateT)]
		public DateTime DateTimeConfirmExpire;
		///<summary>Generated by OD. HQ and OD proper are in different timezones. This is used to tell HQ how far in advance to set the DateTimeExpire when entering the row into HQ db. This is a work-around for timezone offset.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public int SecondsFromEntryToExpire;
		///<summary>Generated by HQ. The code that the patient will text back in order to confirm the appointment. If received then it indicates a positive response.</summary>
		public string ConfirmCode;
		///<summary>Generated by OD. Timestamp when EConnector sent this confirm request to HQ. Stored in local customer timezone.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.DateT)]
		public DateTime DateTimeConfirmTransmit;
		///<summary>Generated by OD. Timestamp when HQ updates this request to indicate that it has been terminated. RSVPStatusCode will change to its final state at this time. Stored in local customer timezone.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.DateT)]
		public DateTime DateTimeRSVP;
		///<summary>Enum:RSVPStatusCodes Generated by OD in some cases and HQ in others. Indicates current status in the lifecycle of this ConfirmationRequest.</summary>
		public RSVPStatusCodes RSVPStatus;
		///<summary>FK to smsfrommobile.GuidMessage. Generated at HQ when the confirmation pending is terminated with confirmation text message.
		///Also allows SmsFromMobile to be linked to ConfirmationRequest in OD proper.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
		public string GuidMessageFromMobile;
		///<summary>Indicates whether the user has chosen to not resend the confirmation request when the AptDateTime has changed.</summary>
		public bool DoNotResend;
		///<summary>Generated by HQ. Identifies this AutoCommGuid in future transactions between HQ and OD.</summary>
		public string ShortGUID;
		///<summary>Foreign key to the appointment represented by this AutoCommAppt.</summary>
		public long ApptNum;
		///<summary>The Date and time of the original appointment. We need this in case the appointment was moved and needs another reminder sent out.</summary>
		[CrudColumn(SpecialType = CrudSpecialColType.DateT)]
		public DateTime ApptDateTime;
		///<summary>This was the TSPrior used to send this reminder. </summary>
		[XmlIgnore]
		[CrudColumn(SpecialType = CrudSpecialColType.TimeSpanLong)]
		public TimeSpan TSPrior;
		///<summary>Used only for serialization purposes. In order to xml serialize, this CANNOT be implemented from an interface.</summary>
		[XmlElement("TSPrior",typeof(long))]
		public long TSPriorXml {
			get {
				return TSPrior.Ticks;
			}
			set {
				TSPrior=TimeSpan.FromTicks(value);
			}
		}
		///<summary>A list of CalendarIcsInfo objects. Currently intended to be used by ThankYous, Reminder, and Confirmations for AddToCalendar tag replacements and
		///generating ics files.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public List<CalendarIcsInfo> ListCalIcsInfos;
		///<summary>FK to patient.PatNum for the corresponding patient.</summary>
		public long PatNum;
		///<summary>FK to clinic.ClinicNum for the corresponding appointment.</summary>
		public long ClinicNum;
		///<summary>Contact information used for sending a message.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string Contact;
		///<summary>Indicates status of message.</summary>
		public AutoCommStatus SendStatus;
		
		public CommType MessageType=CommType.Invalid; 
		///<summary>The template that will be used when creating the message.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string TemplateMessage;
		///<summary>FK to primary key of appropriate table.</summary>
		[XmlIgnore]
		public long MessageFk;
		///<summary>Subject of the message.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string Subject;
		///<summary>Content of the message.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string Message;
		///<summary>Generated by OD. Timestamp when row is created.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.DateTEntry)]
		[XmlIgnore]
		public DateTime DateTimeEntry;
		///<summary>DateTime the message was sent.</summary>
		[CrudColumn(SpecialType = CrudSpecialColType.DateT)]
		[XmlIgnore]
		public DateTime DateTimeSent;
		///<summary>Generated by OD in some cases and HQ in others. Any human readable error message generated by either HQ or EConnector. Used for debugging.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
		public string ResponseDescript;
		///<summary>FK to apptreminderrule.ApptReminderRuleNum. Allows us to look up the rules to determine how to send this apptcomm out.</summary>
		public long ApptReminderRuleNum;
		#region Obsolete
		//Every field in this region should no longer be used.  They have been removed from the database, but must exist in the class for backward compatibility,
		//based on the fact that this class is serialized and sent to HQ in older versions.

		///<summary>Deprecated. Generated by OD. If true then generate and send MT text message. 
		///If false then OD proper is probably panning on emailing the message/GUID so just enter into ConfirmPending and return.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool IsForSms;
		///<summary>Deprecated. Generated by OD. If true then generate and send email.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool IsForEmail;
		///<summary>Deprecated. Use Contact instead. Generated by OD. Only allowed to be empty is IsForSms==true. In that case then it is assumed that OD proper will probably be emailing confirmation and does not want a text message sent.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string PhonePat;
		///<summary>Deprecated. Generated by HQ. Indicates whether or not we were able to send the text message.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool SmsSentOk;
		///<summary>Deprecated. Generated by HQ. Indicates whether or not we were able to send the e-mail.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool EmailSentOk;
		///<summary>Deprecated. Generated by OD. Includes [ConfirmCode] replacement tag and (optionally) [URL] replacement tag. OD proper can construct this to be any length.
		///Will be converted to final MsgText and sent to patient once tags are replaced with real values.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string MsgTextToMobileTemplate;
		///<summary>Deprecated. Generated by HQ. Applied real text to tags from MsgTextTemplate.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string MsgTextToMobile;
		///<summary>Deprecated. </summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailSubjTemplate;
		///<summary>Deprecated. </summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailSubj;
		///<summary>Deprecated. Generated by OD. Includes [ConfirmCode] replacement tag and (optionally) [URL] replacement tag. OD proper can construct this to be any length.
		///Will be converted to final EmailText and emailed to patient once tags are replaced with real values.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailTextTemplate;
		///<summary>Deprecated. Generated by HQ. Applied real text to tags from EmailTextTemplate.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailText;
		///<summary>Deprecated. Generated by HQ. Identifies this ConfirmationRequest in future transactions between HQ and OD. Will be used for email confirmations only.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string ShortGuidEmail;
		///<summary>Deprecated.  Use MessageFK and MessageType instead.FK to message table, ex. smstomobile.GuidMessage. Generated at HQ.  References 
		///'Mobile' to limit schema changes, since that field already existed and is serialized for payloads sent to WebServiceMainHQ.  May not necessarily 
		///be an identifier in the smstomobile table, ex. could be an EmailMessage.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string GuidMessageToMobile;
		#endregion

		#region AutoCommSent
		string IAutoCommSent.GuidMessageToMobile {
			get {
				return GuidMessageToMobile;
			}
			set {
				GuidMessageToMobile=value;
			}
		}

		long IAutoCommSent.PatNum {
			get {
				return PatNum;
			} 
			set {
				PatNum=value;
			}
		}

		long IAutoCommSent.ClinicNum {
			get {
				return ClinicNum;
			}
			set {
				ClinicNum=value;
			}
		}

		string IAutoCommSent.Contact {
			get {
				return Contact;
			}
			set {
				Contact=value;
			}
		}

		AutoCommStatus IAutoCommSent.SendStatus {
			get {
				return SendStatus;
			}
			set {
				SendStatus=value;
			}
		}

		CommType IAutoCommSent.MessageType {
			get {
				return MessageType;
			}
			set {
				MessageType=value;
			}
		}

		string IAutoCommSent.TemplateMessage {
			get {
				return TemplateMessage;
			}
			set {
				TemplateMessage=value;
			}
		}

		long IAutoCommSent.MessageFk {
			get {
				return MessageFk;
			}
			set {
				MessageFk=value;
			}
		}

		string IAutoCommSent.Subject {
			get {
				return Subject;
			}
			set {
				Subject=value;
			}
		}

		string IAutoCommSent.Message {
			get {
				return Message;
			}
			set {
				Message=value;
			}
		}

		DateTime IAutoCommSent.DateTimeEntry {
			get {
				return DateTimeEntry;
			}
			set {
				DateTimeEntry=value;
			}
		}

		DateTime IAutoCommSent.DateTimeSent {
			get {
				return DateTimeSent;
			}
			set {
				DateTimeSent=value;
			}
		}

		string IAutoCommSent.ResponseDescript {
			get {
				return ResponseDescript;
			}
			set {
				ResponseDescript=value;
			}
		}

		long IAutoCommSent.ApptReminderRuleNum {
			get {
				return ApptReminderRuleNum;
			}
			set {
				ApptReminderRuleNum=value;
			}
		}
		#endregion AutoCommSent

		#region AutoCommAppt
		long IAutoCommAppt.ApptNum {
			get {
				return ApptNum;
			}
			set {
				ApptNum=value;
			}
		}

		DateTime IAutoCommAppt.ApptDateTime {
			get {
				return ApptDateTime;
			}
			set {
				ApptDateTime=value;
			}
		}

		TimeSpan IAutoCommAppt.TSPrior {
			get {
				return TSPrior;
			}
			set {
				TSPrior=value;
			}
		}

		List<CalendarIcsInfo> IAutoCommAppt.ListCalIcsInfos {
			get {
				return ListCalIcsInfos;
			}
			set {
				ListCalIcsInfos=value;
			}
		}

		string IAutoCommApptGuid.ShortGUID {
			get {
				return ShortGUID;
			}
			set {
				ShortGUID=value;
			}
		}
		#endregion

		public IAutoCommSent Copy() {
			return (ConfirmationRequest)MemberwiseClone();
		}

		public bool HasSubject() {
			return EnumTools.GetAttributeOrDefault<CommTypeAttribute>(MessageType).ContactMethod==ContactMethod.Email;
		}
	}

	public enum RSVPStatusCodes {
		///<summary>Entered manually by something other than EConnector. EConnector will pickup and send to HQ and change to pendingRsvp.</summary>
		[Description("Awaiting Transmit")]
		AwaitingTransmit,
		///<summary>EConnector has sent this to HQ and will remain in this status until it is either terminated or receives a response from the patient.</summary>
		[Description("Pending Rsvp")]
		PendingRsvp,
		///<summary>Patient responded with an affirmative confirmation.</summary>
		[Description("Positive Rsvp")]
		PositiveRsvp,
		///<summary>Patient responded and declined the confirmation.</summary>
		[Description("Negative Rsvp")]
		NegativeRsvp,
		///<summary>Patient responded by requesting a callback.</summary>
		[Description("Callback")]
		Callback,
		///<summary>Patient took no action by the time DateTimeExpired passed and the confirmation was terminated.</summary>
		[Description("Expired")]
		Expired,
		///<summary>HQ or EConnector was unable to create the confirmation so it was terminated prematurely.</summary>
		[Description("Failed")]
		Failed,
		///<summary> 7 - The appointment date/time was changed before the patient responded to the original confirmation.
		///OD proper will simply delete these ConfirmationRequests. HQ will move them to the terminated table and mark them ApptChanged.</summary>
		[Description("ApptChanged")]
		ApptChanged,
	}
}
