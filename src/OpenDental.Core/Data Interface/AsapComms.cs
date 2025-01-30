using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class AsapComms
{
    public enum SendMode
    {
        TextAndEmail,
        Text,
        Email,
        PreferredContact
    }

    public const int TextMinMinutesBefore = 30;

    private static List<AsapComm> GetMany(List<SQLWhere> listSqlWheres = null)
    {
        var command = "SELECT * FROM asapcomm ";
        if (listSqlWheres != null && listSqlWheres.Count > 0) command += "WHERE " + string.Join(" AND ", listSqlWheres);

        return AsapCommCrud.SelectMany(command);
    }

    public static List<AsapComm> GetForPats(List<long> listPatNums)
    {
        var listSQLWheres = new List<SQLWhere>();
        var sqlWhere = SQLWhere.CreateIn(nameof(AsapComm.PatNum), listPatNums);
        listSQLWheres.Add(sqlWhere);
        return GetMany(listSQLWheres);
    }

    public static List<AsapCommHist> GetHist(DateTime dateFrom, DateTime dateTo, List<long> listPatNums = null, List<long> listClinicNums = null)
    {
        var command = @"
				SELECT asapcomm.*," + DbHelper.Concat("patient.LName", "', '", "patient.FName") + @" PatientName,COALESCE(schedule.StartTime,'00:00:00') StartTime,
				COALESCE(schedule.StopTime,'00:00:00') StopTime,COALESCE(schedule.SchedDate,'0001-01-01') SchedDate,
				COALESCE(emailmessage.BodyText,'') EmailMessageText,COALESCE(smstomobile.MsgText,'') SMSMessageText
				FROM asapcomm
				INNER JOIN patient ON patient.PatNum=asapcomm.PatNum
				LEFT JOIN schedule ON schedule.ScheduleNum=asapcomm.ScheduleNum
				LEFT JOIN emailmessage ON emailmessage.EmailMessageNum=asapcomm.EmailMessageNum
				LEFT JOIN smstomobile ON smstomobile.GuidMessage=asapcomm.GuidMessageToMobile 
				WHERE " + DbHelper.BetweenDates("asapcomm.DateTimeEntry", dateFrom, dateTo) + " ";
        if (listPatNums != null)
        {
            if (listPatNums.Count == 0) return new List<AsapCommHist>();

            command += "AND asapcomm.PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ") ";
        }

        if (listClinicNums != null)
        {
            if (listClinicNums.Count == 0) return new List<AsapCommHist>();

            command += "AND asapcomm.ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ") ";
        }

        var table = DataCore.GetTable(command);
        var listAsapCommHists = AsapCommCrud.TableToList(table).Select(x => new AsapCommHist {AsapComm = x}).ToList();
        for (var i = 0; i < listAsapCommHists.Count; i++)
        {
            listAsapCommHists[i].PatientName = SIn.String(table.Rows[i]["PatientName"].ToString());
            listAsapCommHists[i].DateTimeSlotStart = SIn.Date(table.Rows[i]["SchedDate"].ToString()).Add(SIn.TimeSpan(table.Rows[i]["StartTime"].ToString()));
            listAsapCommHists[i].DateTimeSlotEnd = SIn.Date(table.Rows[i]["SchedDate"].ToString()).Add(SIn.TimeSpan(table.Rows[i]["StopTime"].ToString()));
            listAsapCommHists[i].EmailMessageText = SIn.String(table.Rows[i]["EmailMessageText"].ToString());
            listAsapCommHists[i].SmsMessageText = SIn.String(table.Rows[i]["SMSMessageText"].ToString());
        }

        return listAsapCommHists;
    }

    public static void InsertMany(List<AsapComm> listAsapComms)
    {
        AsapCommCrud.InsertMany(listAsapComms);
    }

    public static void InsertForSending(List<AsapComm> listAsapComms, DateTime dateTSlotStart, DateTime dateTSlotEnd, long opNum)
    {
        var countTextsToBeSent = listAsapComms.Count(x => x.SmsSendStatus != AutoCommStatus.DoNotSend);
        var countEmailsToBeSent = listAsapComms.Count(x => x.EmailSendStatus != AutoCommStatus.DoNotSend);
        //Create a slot on the appointment schedule.
        var schedule = new Schedule();
        schedule.SchedDate = dateTSlotStart.Date;
        schedule.SchedType = ScheduleType.WebSchedASAP;
        schedule.StartTime = dateTSlotStart.TimeOfDay;
        if (dateTSlotEnd.Date > dateTSlotStart.Date)
            schedule.StopTime = new TimeSpan(23, 59, 59); //Last second of the day
        else
            schedule.StopTime = dateTSlotEnd.TimeOfDay;

        schedule.Ops = new List<long> {opNum};
        schedule.Note = countTextsToBeSent + " " + Lans.g("ContrAppt", "text" + (countTextsToBeSent == 1 ? "" : "s") + " to be sent") + "\r\n"
                        + countEmailsToBeSent + " " + Lans.g("ContrAppt", "email" + (countEmailsToBeSent == 1 ? "" : "s") + " to be sent");
        Schedules.Insert(schedule, false);
        for (var i = 0; i < listAsapComms.Count(); i++) listAsapComms[i].ScheduleNum = schedule.ScheduleNum;

        InsertMany(listAsapComms);
    }

    public static string ReplacesTemplateTags(string template, long clinicNum = -1, DateTime dateTime = new(), string nameF = null, string asapUrl = null, bool isHtmlEmail = false)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(template);
        //Note: RegReplace is case insensitive by default.
        if (dateTime.Year > 1880)
        {
            StringTools.RegReplace(stringBuilder, "\\[Date]", dateTime.ToString(PrefC.PatientCommunicationDateFormat));
            StringTools.RegReplace(stringBuilder, "\\[Time]", dateTime.ToString(PrefC.PatientCommunicationTimeFormat));
        }

        if (clinicNum > -1)
        {
            var clinic = Clinics.GetClinic(clinicNum);
            Clinics.ReplaceOffice(stringBuilder, clinic, isHtmlEmail, isHtmlEmail);
        }

        if (nameF != null) StringTools.RegReplace(stringBuilder, "\\[NameF]", nameF);

        if (asapUrl != null) StringTools.RegReplace(stringBuilder, "\\[AsapURL]", asapUrl);

        return stringBuilder.ToString();
    }

    public static AsapListSender CreateSendList(List<Appointment> listAppointments, List<Recall> listRecalls, List<PatComm> listPatComms, SendMode sendMode, string templateText, string templateEmail, string emailSubject, DateTime dateTSlotStart, DateTime dateTStartSend, long clinicNum, bool isRawHtml)
    {
        var asapListSender = new AsapListSender(sendMode, listPatComms, clinicNum, dateTSlotStart, dateTStartSend);
        //Order matters here. We will send messages to appointments that are unscheduled first, then scheduled appointments, then recalls. This is
        //because we would prefer to create a brand new appointment than create a hole in the schedule where another appointment was scheduled.
        //We're doing recalls last because cleanings would be lower priority than other types of dental work.
        var listAppointmentsOrdered = listAppointments.OrderBy(x => x.AptStatus != ApptStatus.UnschedList)
            .ThenBy(x => x.AptStatus != ApptStatus.Planned)
            .ThenByDescending(x => x.AptDateTime).ToList();
        for (var i = 0; i < listAppointmentsOrdered.Count(); i++)
        {
            var asapComm = new AsapComm();
            asapComm.DateTimeOrig = listAppointmentsOrdered[i].AptDateTime;
            asapComm.FKey = listAppointmentsOrdered[i].AptNum;
            asapComm.ClinicNum = clinicNum;
            asapComm.DateTimeExpire = dateTSlotStart.AddDays(7); //Give a 7 day buffer so that the link will still be active a little longer.
            switch (listAppointmentsOrdered[i].AptStatus)
            {
                case ApptStatus.Scheduled:
                    asapComm.FKeyType = AsapCommFKeyType.ScheduledAppt;
                    break;
                case ApptStatus.UnschedList:
                    asapComm.FKeyType = AsapCommFKeyType.UnscheduledAppt;
                    break;
                case ApptStatus.Broken:
                    asapComm.FKeyType = AsapCommFKeyType.Broken;
                    break;
                case ApptStatus.Planned:
                default:
                    asapComm.FKeyType = AsapCommFKeyType.PlannedAppt;
                    break;
            }

            if (asapListSender.ShouldSendText(listAppointmentsOrdered[i].PatNum, listAppointmentsOrdered[i].AptNum, asapComm.FKeyType))
            {
                //This will record in the Note why the patient can't be sent a text.
                asapComm.DateTimeSmsScheduled = asapListSender.GetNextTextSendTime();
                asapComm.SmsSendStatus = AutoCommStatus.SendNotAttempted;
                asapComm.TemplateText = templateText;
                asapListSender.CountTextsToSend++;
            }
            else
            {
                asapComm.SmsSendStatus = AutoCommStatus.DoNotSend;
            }

            if (asapListSender.ShouldSendEmail(listAppointmentsOrdered[i].PatNum, listAppointmentsOrdered[i].AptNum, asapComm.FKeyType))
            {
                //This will record in the Note why the patient can't be sent a email.
                asapComm.EmailSendStatus = AutoCommStatus.SendNotAttempted;
                asapComm.TemplateEmail = templateEmail;
                asapComm.TemplateEmailSubj = emailSubject;
                asapComm.EmailTemplateType = EmailType.Html;
                if (isRawHtml) asapComm.EmailTemplateType = EmailType.RawHtml;

                asapListSender.CountEmailsToSend++;
            }
            else
            {
                asapComm.EmailSendStatus = AutoCommStatus.DoNotSend;
            }

            asapComm.PatNum = listAppointmentsOrdered[i].PatNum;
            if (asapComm.SmsSendStatus == AutoCommStatus.DoNotSend && asapComm.EmailSendStatus == AutoCommStatus.DoNotSend)
                asapComm.ResponseStatus = AsapRSVPStatus.UnableToSend;
            else
                asapComm.ResponseStatus = AsapRSVPStatus.AwaitingTransmit;

            asapListSender.ListAsapComms.Add(asapComm);
        }

        asapListSender.CopyNotes();
        //Now do recalls
        var listRecallsOrdered = listRecalls.OrderByDescending(x => x.DateDue).ToList();
        for (var i = 0; i < listRecallsOrdered.Count(); i++)
        {
            var asapComm = new AsapComm();
            asapComm.DateTimeOrig = listRecallsOrdered[i].DateDue;
            asapComm.FKey = listRecallsOrdered[i].RecallNum;
            asapComm.FKeyType = AsapCommFKeyType.Recall;
            asapComm.ClinicNum = clinicNum;
            asapComm.DateTimeExpire = dateTSlotStart.AddDays(7); //Give a 7 day buffer so that the link will still be active a little longer.
            if (asapListSender.ShouldSendText(listRecallsOrdered[i].PatNum, listRecallsOrdered[i].RecallNum, AsapCommFKeyType.Recall))
            {
                //This will record in the Note why the patient can't be sent a text.
                asapComm.DateTimeSmsScheduled = asapListSender.GetNextTextSendTime();
                asapComm.SmsSendStatus = AutoCommStatus.SendNotAttempted;
                asapComm.TemplateText = templateText;
                asapListSender.CountTextsToSend++;
            }
            else
            {
                asapComm.SmsSendStatus = AutoCommStatus.DoNotSend;
            }

            if (asapListSender.ShouldSendEmail(listRecallsOrdered[i].PatNum, listRecallsOrdered[i].RecallNum, AsapCommFKeyType.Recall))
            {
                //This will record in the Note why the patient can't be sent a email.
                asapComm.EmailSendStatus = AutoCommStatus.SendNotAttempted;
                asapComm.TemplateEmail = templateEmail;
                asapComm.TemplateEmailSubj = emailSubject;
                asapListSender.CountEmailsToSend++;
            }
            else
            {
                asapComm.EmailSendStatus = AutoCommStatus.DoNotSend;
            }

            asapComm.PatNum = listRecallsOrdered[i].PatNum;
            if (asapComm.SmsSendStatus == AutoCommStatus.DoNotSend && asapComm.EmailSendStatus == AutoCommStatus.DoNotSend)
                asapComm.ResponseStatus = AsapRSVPStatus.UnableToSend;
            else
                asapComm.ResponseStatus = AsapRSVPStatus.AwaitingTransmit;

            asapListSender.ListAsapComms.Add(asapComm);
        }

        return asapListSender;
    }

    public class AsapCommHist
    {
        public AsapComm AsapComm;
        public DateTime DateTimeSlotEnd;
        public DateTime DateTimeSlotStart;
        public string EmailMessageText;
        public string PatientName;
        public string SmsMessageText;
    }

    public class AsapListSender
    {
        private const string LanThis = "FormWebSchedASAPSend";
        
        private readonly DateTime _dateTimeSlotStart;
        private readonly List<AsapComm> _listAsapComms;
        private readonly List<PatComm> _listPatComms;
        private readonly List<PatientDetail> _listPatientDetails;
        private readonly int _maxTextsPerDay;
        private readonly SendMode _sendMode;
        
        public DateTime DateTimeTextSendEnd;
        public readonly bool IsOutsideSendWindow;
        public readonly List<AsapComm> ListAsapComms;

        internal AsapListSender(SendMode sendMode, List<PatComm> listPatComms, long clinicNum, DateTime dateTimeSlotStart, DateTime dateTimeStartSend)
        {
            _sendMode = sendMode;
            //listPatComms is one per appointment, but this could include multiple per PatNum.
            _listPatComms = listPatComms;
            _listPatientDetails = listPatComms.Select(x => new PatientDetail(x)).Distinct().ToList();
            _listAsapComms = GetForPats(listPatComms.Select(x => x.PatNum).ToList());
            var timeSpanAutoCommStart = PrefC.GetDateT(PrefName.AutomaticCommunicationTimeStart).TimeOfDay;
            var timeSpanAutoCommEnd = PrefC.GetDateT(PrefName.AutomaticCommunicationTimeEnd).TimeOfDay;
            DateTimeSendEmail = dateTimeStartSend; //All emails will be sent immediately.
            DateTimeStartSendText = dateTimeStartSend;
            if (PrefC.DoRestrictAutoSendWindow)
            {
                //If the time to start sending is before the automatic send window, set the time to start to the beginning of the send window.
                if (DateTimeStartSendText.TimeOfDay < timeSpanAutoCommStart)
                {
                    DateTimeStartSendText = DateTimeStartSendText.Date.Add(timeSpanAutoCommStart);
                    IsOutsideSendWindow = true;
                }
                else if (DateTimeStartSendText.TimeOfDay > timeSpanAutoCommEnd)
                {
                    //If the time to start sending is after the automatic send window, set the time to start to the beginning of the send window the next day.
                    DateTimeStartSendText = DateTimeStartSendText.Date.AddDays(1).Add(timeSpanAutoCommStart);
                    IsOutsideSendWindow = true;
                }
            }

            var strMaxTextsPrefVal = ClinicPrefs.GetPrefValue(PrefName.WebSchedAsapTextLimit, clinicNum);
            _maxTextsPerDay = SIn.Int(strMaxTextsPrefVal); //The pref may be set to blank to have no limit
            if (string.IsNullOrWhiteSpace(strMaxTextsPrefVal)) _maxTextsPerDay = int.MaxValue;

            DateTimeTextSendEnd = DateTimeStartSendText.Date.Add(timeSpanAutoCommEnd);
            _dateTimeSlotStart = dateTimeSlotStart;
            SetMinutesBetweenTexts(dateTimeSlotStart);
            ListAsapComms = new List<AsapComm>();
        }

        public int CountTextsToSend { get; internal set; }
        public int CountEmailsToSend { get; internal set; }
        public DateTime DateTimeStartSendText { get; }
        public DateTime DateTimeSendEmail { get; }
        public int MinutesBetweenTexts { get; private set; }

        public List<PatientDetail> GetListPatientDetails()
        {
            return _listPatientDetails;
        }

        private void SetMinutesBetweenTexts(DateTime dateTimeSlotStart)
        {
            var hoursUntilSlotStart = (int) (dateTimeSlotStart - DateTimeStartSendText).TotalHours;
            if (hoursUntilSlotStart < 2)
                MinutesBetweenTexts = 1;
            else if (hoursUntilSlotStart.Between(2, 11))
                MinutesBetweenTexts = 2;
            else if (hoursUntilSlotStart.Between(12, 47))
                MinutesBetweenTexts = 4;
            else
                MinutesBetweenTexts = 8;
        }

        internal bool ShouldSendText(long patNum, long fkey, AsapCommFKeyType fkeyType)
        {
            var patComm = _listPatComms.Find(x => x.PatNum == patNum);
            if (patComm == null) return false;

            var patientDetail = _listPatientDetails.Find(x => x.PatNum == patNum);
            if (patientDetail == null)
            {
                patientDetail = new PatientDetail();
                patientDetail.PatNum = patNum;
                _listPatientDetails.Add(patientDetail);
            }

            patientDetail.IsSendingText = false;
            if (_sendMode == SendMode.Email) return false; //No need to note the reason.

            var listAsapCommsPat = _listAsapComms.FindAll(x => x.PatNum == patNum);
            if (listAsapCommsPat.Count > 0)
            {
                if (listAsapCommsPat.Any(x => x.FKey == fkey && x.FKeyType == fkeyType && x.ResponseStatus == AsapRSVPStatus.DeclinedStopComm))
                {
                    var text_type = fkeyType == AsapCommFKeyType.Recall ? "recall" : "appointment";
                    patientDetail.AppendNote(Lans.g(LanThis, "Not sending text because this patient has requested to not be texted or emailed about this "
                                                              + text_type + "."));
                    return false;
                }

                var countTextsSent = listAsapCommsPat.Count(x => (x.SmsSendStatus == AutoCommStatus.SendNotAttempted && x.DateTimeSmsScheduled.Date == DateTimeStartSendText.Date)
                                                                 || (x.SmsSendStatus == AutoCommStatus.SendSuccessful && x.DateTimeSmsSent.Date == DateTimeStartSendText.Date));
                if (countTextsSent >= _maxTextsPerDay)
                {
                    patientDetail.AppendNote(Lans.g(LanThis, "Not sending text because this patient has received") + " " + _maxTextsPerDay + " "
                                             + Lans.g(LanThis, "texts today."));
                    return false;
                }
            }

            var isWithin30Minutes = GetNextTextSendTime() < _dateTimeSlotStart && (_dateTimeSlotStart - GetNextTextSendTime()).TotalMinutes < TextMinMinutesBefore;
            var isAfterSlot = GetNextTextSendTime() > _dateTimeSlotStart;
            if (isWithin30Minutes)
            {
                patientDetail.AppendNote(Lans.g(LanThis, "Not sending text because the text would be sent less than") + " " + TextMinMinutesBefore + " "
                                         + Lans.g(LanThis, "minutes before the time slot."));
                return false;
            }

            if (isAfterSlot)
            {
                patientDetail.AppendNote(Lans.g(LanThis, "Not sending text because the text would be sent after the time slot."));
                return false;
            }

            if (_sendMode == SendMode.Email) return false;

            if (_sendMode == SendMode.PreferredContact && patComm.PreferContactMethod != ContactMethod.TextMessage)
            {
                patientDetail.AppendNote(Lans.g(LanThis, "Not sending text because this patient's preferred contact method is not text message."));
                return false;
            }

            if (!patComm.IsSmsAnOption)
            {
                patientDetail.AppendNote(Lans.g(LanThis, patComm.GetReasonCantText(CommOptOutType.WebSchedASAP)));
                return false;
            }

            patientDetail.IsSendingText = true;
            return true;
        }

        internal bool ShouldSendEmail(long patNum, long fkey, AsapCommFKeyType fkeyType)
        {
            var patComm = _listPatComms.Find(x => x.PatNum == patNum);
            if (patComm == null) return false;

            var patientDetail = _listPatientDetails.Find(x => x.PatNum == patNum);
            if (patientDetail == null)
            {
                patientDetail = new PatientDetail();
                patientDetail.PatNum = patNum;
                _listPatientDetails.Add(patientDetail);
            }

            patientDetail.IsSendingEmail = false;
            if (_sendMode == SendMode.Text) return false; //No need to note the reason.

            var listAsapCommsPat = _listAsapComms.FindAll(x => x.PatNum == patNum);
            if (listAsapCommsPat.Count > 0)
                if (listAsapCommsPat.Any(x => x.FKey == fkey && x.FKeyType == fkeyType && x.ResponseStatus == AsapRSVPStatus.DeclinedStopComm))
                {
                    var email_type = fkeyType == AsapCommFKeyType.Recall ? "recall" : "appointment";
                    patientDetail.AppendNote(Lans.g(LanThis, "Not sending email because this patient has requested to not be texted or emailed about this "
                                                              + email_type + "."));
                    return false;
                }

            var isWithin30Minutes = DateTimeSendEmail < _dateTimeSlotStart && (_dateTimeSlotStart - DateTimeSendEmail).TotalMinutes < TextMinMinutesBefore;
            var isAfterSlot = DateTimeSendEmail > _dateTimeSlotStart;
            if (isWithin30Minutes)
            {
                patientDetail.AppendNote(Lans.g(LanThis, "Not sending email because the email would be sent less than") + " " + TextMinMinutesBefore + " "
                                         + Lans.g(LanThis, "minutes before the time slot."));
                return false;
            }

            if (isAfterSlot)
            {
                patientDetail.AppendNote(Lans.g(LanThis, "Not sending email because the email would be sent after the time slot."));
                return false;
            }

            if (_sendMode == SendMode.Text) return false;

            if (_sendMode == SendMode.PreferredContact && patComm.PreferContactMethod != ContactMethod.Email)
            {
                patientDetail.AppendNote(Lans.g(LanThis, "Not sending email because this patient's preferred contact method is not email."));
                return false;
            }

            if (!patComm.IsEmailAnOption)
            {
                patientDetail.AppendNote(Lans.g(LanThis, patComm.GetReasonCantEmail(CommOptOutType.WebSchedASAP)));
                return false;
            }

            patientDetail.IsSendingEmail = true;
            return true;
        }

        internal DateTime GetNextTextSendTime()
        {
            var dateTimeSend = DateTimeStartSendText.AddMinutes(MinutesBetweenTexts * CountTextsToSend);
            if (PrefC.DoRestrictAutoSendWindow && dateTimeSend > DateTimeTextSendEnd) dateTimeSend = DateTimeTextSendEnd;

            return dateTimeSend;
        }

        internal void CopyNotes()
        {
            for (var i = 0; i < ListAsapComms.Count(); i++)
            {
                var patientDetail = _listPatientDetails.Find(x => x.PatNum == ListAsapComms[i].PatNum);
                if (patientDetail == null) continue;

                ListAsapComms[i].Note += patientDetail.Note;
            }
        }

        public class PatientDetail
        {
            public bool IsSendingEmail;
            public bool IsSendingText;
            public string Note = "";
            public readonly string PatName;
            public long PatNum;

            public PatientDetail()
            {
            }

            public PatientDetail(PatComm patComm)
            {
                if (patComm == null) return;

                PatName = patComm.LName + ", " + patComm.FName;
                PatNum = patComm.PatNum;
            }

            internal void AppendNote(string note)
            {
                if (Note.Contains(note)) return; //Don't include the same note twice. This could happen if the same patient is in the ASAP list twice.

                if (!string.IsNullOrEmpty(Note)) Note += "\r\n";

                Note += note;
            }
        }
    }
}