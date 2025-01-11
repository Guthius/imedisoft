using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class AsapComms
{
    ///<summary>The mode by which these AsapComms will be sent out.</summary>
    public enum SendMode
    {
        TextAndEmail,
        Text,
        Email,
        PreferredContact
    }

    ///<summary>Do not send a text to a patient if it is less than this many minutes before the start of time slot.</summary>
    public const int TextMinMinutesBefore = 30;

    #region Get Methods

    /// <summary>Gets a list of all AsapComms matching the passed in parameters.</summary>
    /// <param name="listSQLWheres">To get all AsapComms, don't include this parameter.</param>
    private static List<AsapComm> GetMany(List<SQLWhere> listSQLWheres = null)
    {
        var command = "SELECT * FROM asapcomm ";
        if (listSQLWheres != null && listSQLWheres.Count > 0) command += "WHERE " + string.Join(" AND ", listSQLWheres);

        return AsapCommCrud.SelectMany(command);
    }

    ///<summary>Gets a list of all AsapComms for the given patients.</summary>
    public static List<AsapComm> GetForPats(List<long> listPatNums)
    {
        var listSQLWheres = new List<SQLWhere>();
        var sqlWhere = SQLWhere.CreateIn(nameof(AsapComm.PatNum), listPatNums);
        listSQLWheres.Add(sqlWhere);
        return GetMany(listSQLWheres);
    }

    /// <summary>
    ///     Gets a list of AsapComms (along with a few more fields) for use in the Web Sched History window. To view for all
    ///     patients or clinics,
    ///     pass in null for those parameters.
    /// </summary>
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
            listAsapCommHists[i].SMSMessageText = SIn.String(table.Rows[i]["SMSMessageText"].ToString());
        }

        return listAsapCommHists;
    }

    #endregion

    #region Insert

    
    public static long Insert(AsapComm asapComm)
    {
        return AsapCommCrud.Insert(asapComm);
    }

    
    public static void InsertMany(List<AsapComm> listAsapComms)
    {
        AsapCommCrud.InsertMany(listAsapComms);
    }

    ///<summary>Inserts these AsapComms into the database. Also creates a block on the schedule recording this communication.</summary>
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

    #endregion

    #region Misc Methods

    ///<summary>Replaces the template with the passed in arguments.</summary>
    public static string ReplacesTemplateTags(string template, long clinicNum = -1, DateTime dateTime = new(), string nameF = null,
        string asapUrl = null, bool isHtmlEmail = false)
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

    ///<summary>Creates a list of AsapComms for sending.</summary>
    public static AsapListSender CreateSendList(List<Appointment> listAppointments, List<Recall> listRecalls, List<PatComm> listPatComms, SendMode sendMode,
        string templateText, string templateEmail, string emailSubject, DateTime dateTSlotStart, DateTime dateTStartSend, long clinicNum, bool isRawHtml)
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

    #endregion

    #region Helper classes

    [Serializable]
    public class AsapCommHist
    {
        public AsapComm AsapComm;
        public DateTime DateTimeSlotEnd;
        public DateTime DateTimeSlotStart;
        public string EmailMessageText;
        public string PatientName;
        public string SMSMessageText;
    }


    ///<summary>Helper class used to create a list of AsapComms to send.</summary>
    public class AsapListSender
    {
        private const string _lanThis = "FormWebSchedASAPSend";
        private readonly DateTime _dateTimeSlotStart;

        ///<summary>Key: PatNum, Value: All AsapComms for the patient.</summary>
        private readonly List<AsapComm> _listAsapComms;

        private readonly List<PatComm> _listPatComms;

        ///<summary>A breakdown of who is and isn't receiving what.</summary>
        private readonly List<PatientDetail> _listPatientDetails;

        private readonly int _maxTextsPerDay;

        private readonly SendMode _sendMode;

        /// <summary>
        ///     The date time all texts need to be sent by. Based on PrefName.AutomaticCommunicationTimeEnd. May be today or
        ///     tomorrow.
        /// </summary>
        public DateTime DateTimeTextSendEnd;

        ///<summary>True if it is currently outside the automatic send window.</summary>
        public bool IsOutsideSendWindow;

        ///<summary>The AsapComms to be sent.</summary>
        public List<AsapComm> ListAsapComms;

        /// <summary>Initialize the sender helper for the given PatComms and appointments.</summary>
        /// <param name="clinicNum">The clinic that is doing the sending.</param>
        /// <param name="dateTimeSlotStart">The date time of the time slot for which this list is being sent.</param>
        /// <param name="dateTimeStartSend">
        ///     The date time when the list should be sent out. This time will be adjusted if
        ///     necessary.
        /// </param>
        internal AsapListSender(SendMode sendMode, List<PatComm> listPatComms, long clinicNum, DateTime dateTimeSlotStart,
            DateTime dateTimeStartSend)
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

        ///<summary>The number of texts that are going to be sent.</summary>
        public int CountTextsToSend { get; internal set; }

        ///<summary>The number of emails that are going to be sent.</summary>
        public int CountEmailsToSend { get; internal set; }

        ///<summary>The time when texts will start to be sent.</summary>
        public DateTime DateTimeStartSendText { get; }

        ///<summary>The time when the emails will be sent.</summary>
        public DateTime DateTimeSendEmail { get; }

        ///<summary>The number of minutes that will elapse between texts being sent out.</summary>
        public int MinutesBetweenTexts { get; private set; }

        public List<PatientDetail> GetListPatientDetails()
        {
            return _listPatientDetails;
        }

        ///<summary>Sets the number of minutes between texts.</summary>
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

        /// <summary>
        ///     Returns true if the patient should be sent a text. If false, the reason why the patient can't receive a text is
        ///     added to the details dictionary.
        /// </summary>
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
                    patientDetail.AppendNote(Lans.g(_lanThis, "Not sending text because this patient has requested to not be texted or emailed about this "
                                                              + text_type + "."));
                    return false;
                }

                var countTextsSent = listAsapCommsPat.Count(x => (x.SmsSendStatus == AutoCommStatus.SendNotAttempted && x.DateTimeSmsScheduled.Date == DateTimeStartSendText.Date)
                                                                 || (x.SmsSendStatus == AutoCommStatus.SendSuccessful && x.DateTimeSmsSent.Date == DateTimeStartSendText.Date));
                if (countTextsSent >= _maxTextsPerDay)
                {
                    patientDetail.AppendNote(Lans.g(_lanThis, "Not sending text because this patient has received") + " " + _maxTextsPerDay + " "
                                             + Lans.g(_lanThis, "texts today."));
                    return false;
                }
            }

            var isWithin30Minutes = GetNextTextSendTime() < _dateTimeSlotStart && (_dateTimeSlotStart - GetNextTextSendTime()).TotalMinutes < TextMinMinutesBefore;
            var isAfterSlot = GetNextTextSendTime() > _dateTimeSlotStart;
            if (isWithin30Minutes)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, "Not sending text because the text would be sent less than") + " " + TextMinMinutesBefore + " "
                                         + Lans.g(_lanThis, "minutes before the time slot."));
                return false;
            }

            if (isAfterSlot)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, "Not sending text because the text would be sent after the time slot."));
                return false;
            }

            if (_sendMode == SendMode.Email) return false;

            if (_sendMode == SendMode.PreferredContact && patComm.PreferContactMethod != ContactMethod.TextMessage)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, "Not sending text because this patient's preferred contact method is not text message."));
                return false;
            }

            if (!patComm.IsSmsAnOption)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, patComm.GetReasonCantText(CommOptOutType.WebSchedASAP)));
                return false;
            }

            patientDetail.IsSendingText = true;
            return true;
        }

        /// <summary>
        ///     Returns true if the patient should be sent an email. If false, the reason why the patient can't receive an email is
        ///     added to the details dictionary.
        /// </summary>
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
                    patientDetail.AppendNote(Lans.g(_lanThis, "Not sending email because this patient has requested to not be texted or emailed about this "
                                                              + email_type + "."));
                    return false;
                }

            var isWithin30Minutes = DateTimeSendEmail < _dateTimeSlotStart && (_dateTimeSlotStart - DateTimeSendEmail).TotalMinutes < TextMinMinutesBefore;
            var isAfterSlot = DateTimeSendEmail > _dateTimeSlotStart;
            if (isWithin30Minutes)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, "Not sending email because the email would be sent less than") + " " + TextMinMinutesBefore + " "
                                         + Lans.g(_lanThis, "minutes before the time slot."));
                return false;
            }

            if (isAfterSlot)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, "Not sending email because the email would be sent after the time slot."));
                return false;
            }

            if (_sendMode == SendMode.Text) return false;

            if (_sendMode == SendMode.PreferredContact && patComm.PreferContactMethod != ContactMethod.Email)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, "Not sending email because this patient's preferred contact method is not email."));
                return false;
            }

            if (!patComm.IsEmailAnOption)
            {
                patientDetail.AppendNote(Lans.g(_lanThis, patComm.GetReasonCantEmail(CommOptOutType.WebSchedASAP)));
                return false;
            }

            patientDetail.IsSendingEmail = true;
            return true;
        }

        ///<summary>Returns the time when the next text should be sent out.</summary>
        internal DateTime GetNextTextSendTime()
        {
            var dateTimeSend = DateTimeStartSendText.AddMinutes(MinutesBetweenTexts * CountTextsToSend);
            if (PrefC.DoRestrictAutoSendWindow && dateTimeSend > DateTimeTextSendEnd) dateTimeSend = DateTimeTextSendEnd;

            return dateTimeSend;
        }

        ///<summary>Copies the notes from the ListPatientDetails to the actual list of actual AsapComms.</summary>
        internal void CopyNotes()
        {
            for (var i = 0; i < ListAsapComms.Count(); i++)
            {
                var patientDetail = _listPatientDetails.Find(x => x.PatNum == ListAsapComms[i].PatNum);
                if (patientDetail == null) continue;

                ListAsapComms[i].Note += patientDetail.Note;
            }
        }

        ///<summary>An object used to hold details about specific patients.</summary>
        public class PatientDetail
        {
            public bool IsSendingEmail;
            public bool IsSendingText;
            public string Note = "";
            public string PatName;
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

    ///<summary>This class is used to check if appointments can fit in a given time slot.</summary>
    public class ApptAvailabilityChecker
    {
        ///<summary>Appointments that have been previously gotten from the database.</summary>
        private readonly List<Appointment> _listAppointments;

        ///<summary>The list of appointment dates and operatories that have been gotten from the database.</summary>
        private readonly List<DateTOpNum> _listDateTOpNums;

        public ApptAvailabilityChecker()
        {
            _listAppointments = new List<Appointment>();
            _listDateTOpNums = new List<DateTOpNum>();
        }

        /// <summary>This constructor will store the appointments for the passed in dates and operatories.</summary>
        /// <param name="listDateOps">DateTime is the AptDate, long is the OperatoryNum.</param>
        public ApptAvailabilityChecker(List<DateTOpNum> listDateTOpNums)
        {
            _listDateTOpNums = listDateTOpNums;
            _listAppointments = Appointments.GetApptsForDatesOps(listDateTOpNums);
        }

        ///<summary>Returns true if the recall will fit in the time slot and there are no other appointments in the slot.</summary>
        public bool IsApptSlotAvailable(Recall recall, long opNum, DateTime dateTimeSlotStart, DateTime dateTimeSlotEnd)
        {
            var minutes = RecallTypes.GetTimePattern(recall.RecallTypeNum).Length * PrefC.GetInt(PrefName.AppointmentTimeIncrement);
            return IsApptSlotAvailable(minutes, opNum, dateTimeSlotStart, dateTimeSlotEnd);
        }

        ///<summary>Returns true if the appointment will fit in the time slot and there are no other appointments in the slot.</summary>
        public bool IsApptSlotAvailable(Appointment appointment, long opNum, DateTime dateTimeSlotStart, DateTime dateTimeSlotEnd)
        {
            return IsApptSlotAvailable(appointment.Length, opNum, dateTimeSlotStart, dateTimeSlotEnd);
        }

        /// <summary>
        ///     Returns true if the time length requested will fit in the time slot and there are no other appointments in the
        ///     slot.
        /// </summary>
        public bool IsApptSlotAvailable(int minutes, long opNum, DateTime dateTimeSlotStart, DateTime dateTimeSlotEnd)
        {
            if (!_listDateTOpNums.Any(x => x.DateTAppt == dateTimeSlotStart.Date && x.OpNum == opNum))
            {
                var dateTOpNum = new DateTOpNum();
                dateTOpNum.DateTAppt = dateTimeSlotStart;
                dateTOpNum.OpNum = opNum;
                var listDateTOpNums = new List<DateTOpNum> {dateTOpNum};
                _listAppointments.AddRange(Appointments.GetApptsForDatesOps(listDateTOpNums));
                _listDateTOpNums.Add(dateTOpNum);
            }

            var dateTimeSlotEndNew = ODMathLib.Min(dateTimeSlotStart.AddMinutes(minutes), dateTimeSlotEnd);
            if (_listAppointments.FindAll(x => x.Op == opNum)
                .Any(x => MiscUtils.DoSlotsOverlap(x.AptDateTime, x.AptDateTime.AddMinutes(x.Length), dateTimeSlotStart, dateTimeSlotEndNew)))
                return false;

            return true;
        }
    }

    public class DateTOpNum
    {
        public DateTime DateTAppt;
        public long OpNum;
    }

    #endregion
}