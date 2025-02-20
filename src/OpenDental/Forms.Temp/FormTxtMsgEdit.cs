using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.CallFireService;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormTxtMsgEdit : FormODBase
{
    public long PatNum { get; set; }
    public string WirelessPhone { get; set; }
    public string Message { get; set; }
    public YN YNTxtMsgOk { get; set; }

    public FormTxtMsgEdit()
    {
        InitializeComponent();
    }

    private void FormTxtMsgEdit_Load(object sender, EventArgs e)
    {
        textWirelessPhone.Text = WirelessPhone;
        textMessage.Text = Message;

        SetMessageCounts();

        if (PatNum == 0)
        {
            radioPatient.Checked = false;
            radioOther.Checked = true;
            butPatFind.Enabled = false;
            textPatient.Enabled = false;
            textWirelessPhone.ReadOnly = false;
            textPatient.Text = "";
            textWirelessPhone.Text = "";
            SetFilterControlsAndAction(SetMessageCounts, 0, textMessage);
            return;
        }

        radioPatient.Checked = true;
        radioOther.Checked = false;
        butPatFind.Enabled = true;
        textPatient.Enabled = true;
        textWirelessPhone.ReadOnly = true;
        textPatient.Text = Patients.GetPat(PatNum).GetNameLF();
        textWirelessPhone.Text = WirelessPhone;
        SetFilterControlsAndAction(SetMessageCounts, 0, textMessage);
    }

    private void SetMessageCounts()
    {
        textCharCount.Text = textMessage.TextLength.ToString();
        textMsgCount.Text = SmsPhones.CalculateMessagePartsNumber(textMessage.Text).ToString();
    }

    public bool SendText(long patNum, string wirelessPhone, string message, YN yNTxtMsgOk, long clinicNum, SmsMessageSource smsMessageSource, bool canIncreaseLimit = false)
    {
        if (wirelessPhone == "")
        {
            MsgBox.Show(this, "Please enter a phone number.");
            return false;
        }

        if (SmsPhones.IsIntegratedTextingEnabled())
        {
            if (!Clinics.IsTextingEnabled(clinicNum))
            {
                if (clinicNum != 0)
                {
                    ShowError("Integrated Texting has not been enabled for the following clinic:\r\n" + Clinics.GetClinic(clinicNum).Description + ".");
                    return false;
                }

                ShowError("The default texting clinic has not been set.");
                return false;
            }
        }
        else if (!Programs.IsEnabled(ProgramName.CallFire))
        {
            ShowError("CallFire Program Link must be enabled.");
            return false;
        }

        if (patNum != 0 && yNTxtMsgOk == YN.Unknown && PrefC.GetBool(PrefName.TextMsgOkStatusTreatAsNo) || patNum != 0 && yNTxtMsgOk == YN.No)
        {
            ShowError("It is not OK to text this patient.");
            return false;
        }

        if (SmsPhones.IsIntegratedTextingEnabled())
        {
            try
            {
                SmsToMobiles.SendSmsSingle(patNum, wirelessPhone, message, clinicNum, smsMessageSource, userod: Security.CurUser); //Can pass in 0 as PatNum if no patient selected.
                return true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return false;
            }
        }

        if (message.Length <= 160)
        {
            return SendCallFire(patNum, wirelessPhone, message);
        }

        ShowError("Text length must be less than 160 characters.");
        return false;
    }

    private bool SendCallFire(long patNum, string wirelessPhone, string message)
    {
        var key = ProgramProperties.GetPropVal(ProgramName.CallFire, "Key From CallFire");
        var msg = wirelessPhone + "," + message.Replace(",", "");

        try
        {
            var smsService = new SMSService();

            smsService.sendSMSCampaign(key, [msg], "Open Dental");
        }
        catch (Exception ex)
        {
            ShowError("Error sending text message.\r\n\r\n" + ex.Message);
            return false;
        }

        if (patNum == 0)
        {
            return true;
        }

        var commlog = new Commlog
        {
            CommDateTime = DateTime.Now,
            DateTStamp = DateTime.Now,
            CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.TEXT),
            Mode_ = CommItemMode.Text,
            Note = msg,
            PatNum = patNum,
            SentOrReceived = CommSentOrReceived.Sent,
            UserNum = Security.CurUser.UserNum,
            DateTimeEnd = DateTime.Now
        };

        Commlogs.Insert(commlog);

        SecurityLogs.MakeLogEntry(EnumPermType.CommlogEdit, commlog.PatNum, "Insert Text Message");

        return true;
    }

    private void ButtonPatFind_Click(object sender, EventArgs e)
    {
        var frmPatientSelect = new FrmPatientSelect();

        frmPatientSelect.ShowDialog();

        if (frmPatientSelect.IsDialogCancel)
        {
            return;
        }

        var patient = Patients.GetPat(frmPatientSelect.PatNumSelected);
        PatNum = patient.PatNum;
        YNTxtMsgOk = patient.TxtMsgOk;
        textPatient.Text = patient.GetNameLF();
        textWirelessPhone.Text = patient.WirelessPhone;
    }

    private void RadioButtonOther_Click(object sender, EventArgs e)
    {
        if (!textWirelessPhone.ReadOnly)
        {
            return;
        }

        butPatFind.Enabled = false;
        textPatient.Enabled = false;
        textWirelessPhone.ReadOnly = false;
        textPatient.Text = "";
        textWirelessPhone.Text = "";
        PatNum = 0;
        YNTxtMsgOk = YN.Unknown;
    }

    private void RadioButtonPatient_Click(object sender, EventArgs e)
    {
        if (textWirelessPhone.ReadOnly)
        {
            return;
        }

        butPatFind.Enabled = true;
        textPatient.Enabled = true;
        textWirelessPhone.ReadOnly = true;
        textPatient.Text = "";
        textWirelessPhone.Text = "";
        PatNum = 0;
        YNTxtMsgOk = YN.Unknown;
    }

    private void ButtonSend_Click(object sender, EventArgs e)
    {
        if (textMessage.Text == "")
        {
            ShowError("Please enter a message first.");
            return;
        }

        if (radioOther.Checked)
        {
            if (textWirelessPhone.Text == "")
            {
                ShowError("Please enter a phone number first.");
                return;
            }

            if (!ConfirmOk(
                    "You do not have a patient selected. If you are sending a message to an existing patient " +
                    "you should choose the Patient option and use the find button. If you proceed no commlog entry " +
                    "will be created and any replies to this message will not be automatically associated with any " +
                    "patient. Continue?"))
            {
                return;
            }

            var clinicNum = Clinics.ClinicNum;
            if (clinicNum == 0)
            {
                clinicNum = PrefC.GetLong(PrefName.TextingDefaultClinicNum);
            }

            if (!SendText(0, textWirelessPhone.Text, textMessage.Text, YN.Unknown, clinicNum, SmsMessageSource.DirectSms, true))
            {
                return;
            }

            DialogResult = DialogResult.OK;
            return;
        }

        if (PatNum == 0)
        {
            ShowError("You must first select a patient with the find button, or use the Another Person option.");
            return;
        }

        if (textWirelessPhone.Text == "")
        {
            ShowError(
                "This patient has no wireless phone entered. " +
                "You must add a wireless phone number to their patient account first before you can send a text message.");

            return;
        }

        if (!SendText(PatNum, textWirelessPhone.Text, textMessage.Text, YNTxtMsgOk, SmsPhones.GetClinicNumForTexting(PatNum), SmsMessageSource.DirectSms, true))
        {
            return;
        }

        DialogResult = DialogResult.OK;
    }
}