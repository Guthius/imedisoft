using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormTxtMsgMany : FormODBase
{
    private readonly List<PatComm> _patComms;
    private readonly long _clinicNum;
    private readonly SmsMessageSource _smsMessageSource;

    public bool DoCombineNumbers;

    public FormTxtMsgMany(List<PatComm> patComms, string textMessageText, long clinicNum, SmsMessageSource smsMessageSource)
    {
        _patComms = patComms;
        _clinicNum = clinicNum;
        _smsMessageSource = smsMessageSource;

        InitializeComponent();

        textMessage.Text = textMessageText;
    }

    private void FormTxtMsgMany_Load(object sender, EventArgs e)
    {
        FillGrid();

        SetFilterControlsAndAction(SetMessageCounts, 0, textMessage);
    }

    private void SetMessageCounts()
    {
        textCharCount.Text = textMessage.TextLength.ToString();
        textMsgCountPerPatient.Text = SmsPhones.CalculateMessagePartsNumber(textMessage.Text).ToString();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Phone Number", 120));
        gridMain.Columns.Add(new GridColumn("Patient", 200));

        gridMain.ListGridRows.Clear();

        if (DoCombineNumbers)
        {
            var wirelessPhones = _patComms.Select(x => x.WirelessPhone).Distinct().ToList();
            foreach (var wirelessPhone in wirelessPhones)
            {
                var patComms = _patComms.FindAll(x => x.WirelessPhone == wirelessPhone);

                var gridRow = new GridRow();

                gridRow.Cells.Add(wirelessPhone);
                gridRow.Cells.Add(string.Join("\r\n", patComms.Select(x => x.LName + ", " + x.FName)));
                gridRow.Tag = patComms.ToList();

                gridMain.ListGridRows.Add(gridRow);
            }

            gridMain.EndUpdate();
            return;
        }

        var patNums = _patComms.Select(x => x.PatNum).Distinct().ToList();
        foreach (var patNum in patNums)
        {
            var patComms = _patComms.FindAll(x => x.PatNum == patNum);

            var gridRow = new GridRow();

            gridRow.Cells.Add(patComms[0].WirelessPhone);
            gridRow.Cells.Add(string.Join("\r\n", patComms.Select(x => x.LName + ", " + x.FName)));
            gridRow.Tag = patComms.ToList();

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private bool SendText(PatComm patComm, long clinicNum, string message)
    {
        if (!patComm.IsSmsAnOption)
        {
            Cursor = Cursors.Default;

            ShowError("It is not OK to text patient " + patComm.FName + " " + patComm.LName + ".");

            Cursor = Cursors.WaitCursor;

            return false;
        }

        SmsToMobiles.SendSmsSingle(patComm.PatNum, patComm.SmsPhone, message, clinicNum, _smsMessageSource, true, Security.CurUser);
        return true;
    }

    private void ButtonSend_Click(object sender, EventArgs e)
    {
        if (!SmsPhones.IsIntegratedTextingEnabled())
        {
            ShowError("Integrated Texting has not been enabled.");
            return;
        }

        if (textMessage.Text == "")
        {
            ShowError("Please enter a message first.");
            return;
        }

        if (textMessage.Text.ToLower().Contains("[date]") || textMessage.Text.ToLower().Contains("[time]"))
        {
            ShowError("Please replace or remove the [Date] and [Time] tags.");
            return;
        }

        if (!Clinics.IsTextingEnabled(_clinicNum))
        {
            if (_clinicNum != 0)
            {
                ShowError("Integrated Texting has not been enabled for the following clinic:\r\n" + Clinics.GetClinic(_clinicNum).Description + ".");
                return;
            }

            ShowError("The default texting clinic has not been set.");
            return;
        }

        Cursor = Cursors.WaitCursor;

        var numberOfTextsSent = 0;

        var patComms = gridMain.ListGridRows.Select(x => x.Tag).Cast<List<PatComm>>().ToList();
        foreach (var comms in patComms)
        {
            var patComm = comms.OrderByDescending(x => x.PatNum == x.Guarantor).ThenBy(x => x.FName).First();
            var message = textMessage.Text.Replace("[NameF]", patComm.FName);

            try
            {
                if (SendText(patComm, _clinicNum, message))
                {
                    numberOfTextsSent++;
                }
            }
            catch (ODException ex)
            {
                Cursor = Cursors.Default;

                var errorMessage =
                    "There was an error sending to " + comms.First().WirelessPhone + ". " + ex.Message + " " +
                    "Do you want to continue sending messages?";

                if (!Confirm(errorMessage))
                {
                    break;
                }

                Cursor = Cursors.WaitCursor;
            }
            catch
            {
                Cursor = Cursors.Default;

                var errorMessage =
                    "There was an error sending to " + comms.First().WirelessPhone + ". " +
                    "Do you want to continue sending messages?";

                if (!Confirm(errorMessage))
                {
                    break;
                }

                Cursor = Cursors.WaitCursor;
            }
        }

        Cursor = Cursors.Default;

        ShowInfo(numberOfTextsSent + " texts sent successfully.");

        DialogResult = DialogResult.OK;

        Close();
    }
}