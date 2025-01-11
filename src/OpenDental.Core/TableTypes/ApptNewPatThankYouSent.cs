﻿using CodeBase;
using OpenDentBusiness.WebTypes.AutoComm;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenDentBusiness {
	///<summary>When a reminder is sent for an appointment a record of that send is stored here. Only want to send new patient thank yous once per patient.</summary>
	[Serializable,CrudTable(HasBatchWriteMethods=true)]
	public class ApptNewPatThankYouSent:TableBase,IAutoCommApptGuid {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long ApptNewPatThankYouSentNum;
		///<summary>The Date and time of the original appointment.</summary>
		[CrudColumn(SpecialType = CrudSpecialColType.DateT)]
		public DateTime ApptSecDateTEntry;
		///<summary>Generated by OD. Timestamp when EConnector sent this ApptNewPatThankYouSent to HQ. Stored in local customer timezone.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.DateT)]
		public DateTime DateTimeNewPatThankYouTransmit;
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
		///<summary>Generated by HQ. Identifies this AutoCommGuid in future transactions between HQ and OD.</summary>
		public string ShortGUID;
		#region Obsolete
		//Every field in this region should no longer be used.  They have been removed from the database, but must exist in the class for backward compatibility,
		//based on the fact that this class is serialized and sent to HQ in older versions.

		///<summary>Deprecated.Generated by HQ. Identifies this ApptNewPatThankYouSent in future transactions between HQ and OD. Will be used for email thankyous only.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string ShortGuidEmail;
		///<summary>Deprecated.Enum:AutoCommStatus Generated by HQ. Indicates whether or not we were able to send the text message.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public AutoCommStatus SmsSentStatus;
		///<summary>Deprecated.Enum:AutoCommStatus Generated by HQ. Indicates whether or not we were able to send the e-mail.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public AutoCommStatus EmailSentStatus;
		///<summary>Deprecated.Generated by OD. If true then generate and send sms.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool IsForSms;
		/////<summary>Deprecated.Generated by OD. If true then generate and send email.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public bool IsForEmail;
		///<summary>Deprecated.Generated by OD.  Wireless phone number of the corresponding patient.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string PhonePat;
		///<summary>Deprecated.Generated by OD. OD proper can construct this to be any length. Will be converted to final MsgText and sent to patient once tags are 
		///replaced with real values.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string MsgTextToMobileTemplate;
		///<summary>Deprecated.Generated by HQ. Applied real text to tags from MsgTextTemplate.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string MsgTextToMobile;
		///<summary>Deprecated.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailSubjTemplate;
		///<summary>Deprecated.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailSubj;
		///<summary>Deprecated.Generated by OD. Includes [ConfirmCode] replacement tag and (optionally) [URL] replacement tag. OD proper can construct this to be any length.
		///Will be converted to final EmailText and emailed to patient once tags are replaced with real values.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailTextTemplate;
		///<summary>Deprecated.Generated by HQ. Applied real text to tags from EmailTextTemplate.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public string EmailText;
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

		public IAutoCommSent Copy() {
			return (ApptNewPatThankYouSent)MemberwiseClone();
		}

		public bool HasSubject() {
			return EnumTools.GetAttributeOrDefault<CommTypeAttribute>(MessageType).ContactMethod==ContactMethod.Email;
		}
		#endregion
	}
}
