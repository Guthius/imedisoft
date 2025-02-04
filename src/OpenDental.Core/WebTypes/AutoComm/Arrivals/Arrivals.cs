using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness.AutoComm;

public class Arrivals
{
    private IEnumerable<ApptReminderSent> _listArrivalsSent = new List<ApptReminderSent>();
    private IEnumerable<ApptReminderRule> _listApptReminderRules = new List<ApptReminderRule>();
    private readonly TagReplacer _tagReplacer;

    private Arrivals()
    {
        _tagReplacer = new ArrivalsTagReplacer();
    }

    public static Arrivals LoadArrivals()
    {
        return new Arrivals();
    }

    public static Arrivals LoadArrivals(List<long> listClinicNums, List<long> listApptNums)
    {
        var arrivals = LoadArrivals();

        var listSignedUpClinics = listClinicNums.Where(x => ClinicPrefs.GetBool(PrefName.ApptConfirmAutoSignedUp, x)).ToList();
        //Only do the work of looking up ApptReminderSents and ApptReminderRules if the appropriate eServices are enabled and in use.
        if (!listApptNums.IsNullOrEmpty() && listSignedUpClinics.Any() && PrefC.GetBool(PrefName.ApptArrivalAutoEnabled))
        {
            arrivals._listApptReminderRules = GetApptReminderRules(listSignedUpClinics);
            if (arrivals._listApptReminderRules.Any())
            {
                var listApptReminderRuleNums = arrivals._listApptReminderRules.Select(x => x.ApptReminderRuleNum).ToList();
                //This clinic has at least one Reminder Rule with an arrival/come-in template defined.  We now know we need ApptReminderSent data.
                arrivals._listArrivalsSent = ApptReminderSents.GetForApt(listApptNums.ToArray())
                    //Arrivals only, not eReminders.
                    .Where(x => listApptReminderRuleNums.Contains(x.ApptReminderRuleNum)).ToList();
            }
        }

        return arrivals;
    }

    private static List<ApptReminderRule> GetApptReminderRules(List<long> listClinicNums)
    {
        var listRules = ApptReminderRules.GetForTypes(ApptReminderType.Arrival);
        var listRulesDefault = listRules.Where(x => x.ClinicNum == 0).ToList();
        //Make sure a rule is included for clinics using defaults.
        foreach (var clinicNum in listClinicNums)
        {
            var clinicPref = ClinicPrefs.GetPref(PrefName.ApptArrivalUseDefaults, clinicNum);
            if (clinicPref != null && SIn.Bool(clinicPref.ValueString))
            {
                listRules.AddRange(listRulesDefault.Select(x => x.CopyWithClinicNum(clinicNum)));
            }
        }

        return listRules.Where(x =>
            x.IsEnabled //Rule must be enabled
            && listClinicNums.Contains(x.ClinicNum) //For these clinics
            && (
                (x.IsAutoReplyEnabled && !string.IsNullOrWhiteSpace(x.TemplateAutoReply)) //AutoReply is enabled and has a template
                || !string.IsNullOrWhiteSpace(x.TemplateComeInMessage) //or ComeIn has a template.
            )
        ).ToList();
    }

    public static void ProcessArrival(SmsFromMobile sms)
    {
        if (sms.MsgTotal != 1 || sms.MsgText.ToLower().Trim() != ArrivalsTagReplacer.ArrivedCode.ToLower().Trim())
        {
            //Not an "Arrived" sms.
            return;
        }

        //It's possible a dependent and guarantor both have appointments on the same day, but have different wireless phone numbers.
        //We don't want the dependent to mark the guarantor appointment as 'Arrived' as well.
        var listPatients = Patients.GetFamily(sms.PatNum).ListPats.ToList();
        listPatients.RemoveAll(x => PhoneNumbers.RemoveNonDigitsAndTrimStart(x.WirelessPhone) != PhoneNumbers.RemoveNonDigitsAndTrimStart(sms.MobilePhoneNumber));
        var arrayPatNums = listPatients.Select(x => x.PatNum).ToArray();
        var listAppointments = Appointments.GetAppointmentsForPat(arrayPatNums);
        ProcessArrivalAsync(sms.PatNum, sms.ClinicNum, sms.MobilePhoneNumber, listAppointments, true);
    }

    public static void ProcessArrival(long patNumForResponse, long clinicNum, List<Appointment> listAppts)
    {
        ProcessArrivalAsync(patNumForResponse, clinicNum, null, listAppts, false);
    }

    private static void ProcessArrivalAsync(long patNumForResponse, long clinicNum, string mobilePhoneNumber, List<Appointment> listAppts, bool doAlert)
    {
        if (patNumForResponse <= 0)
        {
            //Invalid patNum.
            return;
        }

        //Run Arrival Processing on a thread, because we do not want this action, which includes a web call, to slow down the UI, as everything is
        //happening behind the scenes anyway.
        var arrivalThread = new ODThread(o =>
        {
            var listTodayAppts = listAppts
                .Where(x => x.AptStatus.In(ApptStatus.Scheduled))
                .Where(x => x.ClinicNum == clinicNum && x.AptDateTime.Date == DateTime_.Today).ToList();
            var arrival = LoadArrivals(ListTools.FromSingle(clinicNum), listTodayAppts.Select(x => x.AptNum).ToList());
            arrival.ProcessArrival(patNumForResponse, clinicNum, mobilePhoneNumber, listTodayAppts, doAlert);
        });
        arrivalThread.Name = nameof(ProcessArrival) + $"_PatNum{patNumForResponse}";
        arrivalThread.GroupName = nameof(ProcessArrival);
        arrivalThread.Start();
    }

    private void ProcessArrival(long patNum, long clinicNum, string mobilePhoneNumber, List<Appointment> listAppts, bool doAlert)
    {
        var listApptsToday = listAppts.Where(x => x.ClinicNum == clinicNum && x.AptDateTime.Date == DateTime_.Today).OrderBy(x => x.AptDateTime).ToList();
        if (listApptsToday.Count == 0)
        {
            return;
        }

        var listApptsAutomationEnabled = listApptsToday
            //Check if (given clinic exists and has automation enabled) or (HQ "clinic" and Arrivals are enabled)
            .Where(x => Clinics.GetClinic(x.ClinicNum)?.IsConfirmEnabled ?? (x.ClinicNum == 0 && PrefC.GetBool(PrefName.ApptArrivalAutoEnabled)))
            .ToList();
        if (listApptsAutomationEnabled.Count == 0)
        {
            return;
        }

        var listApptResponses = GetApptResponses(listApptsAutomationEnabled);
        MarkArrived(listApptResponses, doAlert); //All appoinments should be marked Arrived.
        listApptResponses.RemoveAll(x => string.IsNullOrWhiteSpace(x.Response)); //In case the office does not want to send Arrival Response SMS.
        if (!listApptResponses.IsNullOrEmpty())
        {
            //There is a configured Arrival Response for this patient, send sms.
            var message = AppendEClipboardTokens(listApptResponses);
            TrySendArrivalResponseSms(patNum, mobilePhoneNumber, clinicNum, message);
        }
    }

    private string AppendEClipboardTokens(List<ApptResponse> listApptResponses)
    {
        if (listApptResponses.IsNullOrEmpty())
        {
            return "";
        }

        var apptResponse = listApptResponses.First();
        var message = apptResponse.Response;
        return message;
    }

    private void MarkArrived(IEnumerable<ApptResponse> listAppts, bool doAlert)
    {
        var arrivedTrigger = PrefC.GetLong(PrefName.AppointmentTimeArrivedTrigger);
        var listArriving = listAppts.Where(x => x.Appointment.Confirmed != arrivedTrigger).ToList();
        for (var i = 0; i < listArriving.Count(); i++)
        {
            //This update will trigger eClipboard to generate the appropriate check-in sheets if the appointment is not already marked as arrived.
            //If the clinic is setup for eClipboard checking, the appropriate token needs to be included in the Arrival Response sms.
            var arrivedStatusNew = Appointments.GetApptConfirmationStatus(listArriving[i].Appointment.AptNum);
            if (arrivedStatusNew != arrivedTrigger)
            {
                //If the confirmation status has not already been updated manually, do it here and create a log.
                Appointments.SetConfirmed(listArriving[i].Appointment, arrivedTrigger);
                SecurityLogs.MakeLogEntry(EnumPermType.ApptConfirmStatusEdit, listArriving[i].Appointment.PatNum, "Appointment confirmation status changed from "
                                                                                                                  + Defs.GetName(DefCat.ApptConfirmed, listArriving[i].Appointment.Confirmed) + " to " + Defs.GetName(DefCat.ApptConfirmed, arrivedTrigger)
                                                                                                                  + " due to an Arrival text.", listArriving[i].Appointment.AptNum, LogSources.AutoConfirmations, listArriving[i].Appointment.DateTStamp);
            }
        }

        if (doAlert)
        {
            CreateArrivalAlert(listArriving);
        }
    }

    private void CreateArrivalAlert(List<ApptResponse> listArriving)
    {
        foreach (var appt in listArriving)
        {
            var alert = new AlertItem()
            {
                ClinicNum = appt.Appointment.ClinicNum,
                Description = appt.PatComm.GetFirstOrPreferred()
                              + " " + Lans.g("arrived at") + " " + DateTime.Now.ToString(PrefC.PatientCommunicationTimeFormat)
                              + " " + DateTime.Now.ToString(PrefC.PatientCommunicationDateFormat)
                              + " " + Lans.g("for appointment at") + " " + appt.Appointment.AptDateTime.ToString(PrefC.PatientCommunicationTimeFormat)
                              + " " + appt.Appointment.AptDateTime.ToString(PrefC.PatientCommunicationDateFormat),
                Type = AlertType.PatientArrival,
                Actions = ActionType.MarkAsRead | ActionType.Delete | ActionType.OpenForm,
                FormToOpen = FormType.FormApptEdit,
                Severity = SeverityType.Low,
                FKey = appt.Appointment.AptNum
            };
            AlertItems.Insert(alert);
        }
    }

    private void TrySendArrivalResponseSms(long patNum, string wirelessPhone, long clinicNum, string message)
    {
        var retVal = false;
        try
        {
            if (wirelessPhone is null)
            {
                //try to find a usable phone number for the patient.
                foreach (var pat in Patients.GetPatComms(ListTools.FromSingle(patNum), Clinics.GetClinic(clinicNum))
                             .OrderByDescending(x => x.PatNum == patNum))
                {
                    if (pat.IsSmsAnOption)
                    {
                        wirelessPhone = pat.SmsPhone; //Stripped of formatting.
                        break;
                    }
                }
            }

            if (wirelessPhone is null)
            {
                return;
            }

            SmsToMobiles.SendSmsSingle(patNum, wirelessPhone, message, clinicNum, SmsMessageSource.Arrival);
            retVal = true;
        }
        catch
        {
        }
    }

    public bool HasComeInMsg(long aptNum)
    {
        var rule = GetArrivalRule(aptNum);
        return !string.IsNullOrWhiteSpace(rule?.TemplateComeInMessage ?? "");
    }

    private ApptReminderRule GetArrivalRule(long aptNum)
    {
        var listReminderRuleNums = _listArrivalsSent.Where(x => x.ApptNum == aptNum).Select(x => x.ApptReminderRuleNum);
        var rule = _listApptReminderRules.FirstOrDefault(x => listReminderRuleNums.Contains(x.ApptReminderRuleNum));
        return rule;
    }

    public bool TryGetComeInMsg(Appointment appt, out string message)
    {
        var patComm = Patients.GetPatComms(new List<long> {appt.PatNum}, null).FirstOrDefault();
        return TryGetMsgFromTemplate(appt, patComm, (rule) => rule?.TemplateComeInMessage ?? "", out message);
    }

    private List<ApptResponse> GetApptResponses(List<Appointment> listAppts)
    {
        var listResponses = new List<ApptResponse>();
        var listConfirmStatusToSkip = PrefC.GetString(PrefName.ApptConfirmExcludeArrivalResponse)
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => SIn.Long(x))
            .ToList();
        //It is expected here that all Appointments are for the same clinic.
        var clinic = (listAppts.Any(x => x.ClinicNum == 0)) ? Clinics.GetPracticeAsClinicZero() : Clinics.GetClinic(listAppts.First().ClinicNum);
        var listPatComms = Patients.GetPatComms(listAppts.Select(x => x.PatNum).ToList(), clinic);
        //We will mark all appointments with Confirmed not in the "skip list" as arrived, and send response SMS where configured.
        foreach (var appt in listAppts.Where(x => !listConfirmStatusToSkip.Contains(x.Confirmed)))
        {
            var patComm = listPatComms.FirstOrDefault(x => x.PatNum == appt.PatNum);

            string getAutoReplyTemplate(ApptReminderRule rule)
            {
                if (rule?.IsAutoReplyEnabled ?? false)
                {
                    return rule.TemplateAutoReply;
                }

                return ""; //disabled
            }

            //Later, we will have to filter out ApptResponses that do not have a Response/message.
            TryGetMsgFromTemplate(appt, patComm, getAutoReplyTemplate, out var message);
            listResponses.Add(new ApptResponse(appt, patComm, message));
        }

        return listResponses;
    }

    private bool TryGetMsgFromTemplate(Appointment appt, PatComm patComm, Func<ApptReminderRule, string> getTemplate, out string message)
    {
        ApptReminderRule rule = null;
        var msg = "";
        try
        {
            rule = GetArrivalRule(appt.AptNum);
            var template = getTemplate(rule);
            if (!string.IsNullOrWhiteSpace(template))
            {
                var clinic = (appt.ClinicNum == 0) ? Clinics.GetPracticeAsClinicZero() : Clinics.GetClinic(appt.ClinicNum);
                msg = _tagReplacer.ReplaceTags(template, new ApptLite(appt, patComm), clinic, false);
            }
        }
        catch
        {
        }

        message = msg;
        return !string.IsNullOrWhiteSpace(message);
    }

    private class ApptResponse
    {
        public Appointment Appointment;
        public PatComm PatComm;
        public string Response;

        public ApptResponse(Appointment appt, PatComm patComm, string response)
        {
            Appointment = appt;
            PatComm = patComm;
            Response = response;
        }
    }
}