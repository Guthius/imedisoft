using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Main_Modules;

public partial class ControlManage : UserControl
{
    public FormAccounting FormAccounting;

    private Employee _employee;
    private FormBilling _formBilling;
    private FormClaimsSend _formClaimsSend;
    private FormEmailInbox _formEmailInbox;
    private FormEtrans834Import _formEtrans834Import;
    private long _patNum;

    public ControlManage()
    {
        InitializeComponent();
    }

    private void ButtonAccounting_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Accounting))
        {
            return;
        }

        if (FormAccounting is null || FormAccounting.IsDisposed)
        {
            FormAccounting = new FormAccounting();
        }

        FormAccounting.Show();
        if (FormAccounting.WindowState == FormWindowState.Minimized)
        {
            FormAccounting.WindowState = FormWindowState.Normal;
        }

        FormAccounting.BringToFront();
    }

    private void ButtonBackup_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Backup))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Backup, 0, "FormBackup was accessed");

        using var formBackup = new FormBackup();

        if (formBackup.ShowDialog() == DialogResult.Cancel)
        {
            return;
        }

        GlobalFormOpenDental.PatientSelected(new Patient(), false);

        DataValid.SetInvalid();

        ModuleSelected(_patNum);
    }

    private void ButtonBilling_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Billing))
        {
            return;
        }

        var unsentStatementsExist = Statements.UnsentStatementsExist();
        if (unsentStatementsExist)
        {
            if (Statements.UnsentClinicStatementsExist(Clinics.ClinicNum))
            {
                ShowBilling([Clinics.ClinicNum]);
            }
            else
            {
                ShowBillingOptions(Clinics.ClinicNum);
            }

            SecurityLogs.MakeLogEntry(EnumPermType.Billing, 0, "");
            return;
        }

        ShowBillingOptions(Clinics.ClinicNum);

        SecurityLogs.MakeLogEntry(EnumPermType.Billing, 0, "");
    }

    private void ButtonClaimPay_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPayCreate, true) && !Security.IsAuthorized(EnumPermType.InsPayEdit, true))
        {
            ODMessageBox.Show(
                "Not authorized.\r\n" +
                "A user with the SecurityAdmin permission must grant you access for:\r\n" +
                "Insurance Payment Create or Insurance Payment Edit");
            
            return;
        }

        var formClaimPayList = new FormClaimPayList();

        formClaimPayList.Show();
    }

    private void butDeposit_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DepositSlips, DateTime.Today))
        {
            return;
        }

        using var formDeposits = new FormDeposits();

        formDeposits.ShowDialog();
    }

    private void butEmailInbox_Click(object sender, EventArgs e)
    {
        if (_formEmailInbox == null || _formEmailInbox.IsDisposed)
        {
            _formEmailInbox = null;
            _formEmailInbox = new FormEmailInbox();
            _formEmailInbox.Show();
            return;
        }

        if (_formEmailInbox.WindowState == FormWindowState.Minimized)
        {
            _formEmailInbox.WindowState = FormWindowState.Maximized;
        }

        _formEmailInbox.BringToFront();
    }

    private void butEras_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPayCreate))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;

        var formEtrans835s = new FormEtrans835s();

        formEtrans835s.Show();

        Cursor = Cursors.Default;
    }

    private void butImportInsPlans_Click(object sender, EventArgs e)
    {
        if (_formEtrans834Import is {FormEtrans834PreviewCur.IsDisposed: false})
        {
            _formEtrans834Import.FormEtrans834PreviewCur.Show();
            _formEtrans834Import.FormEtrans834PreviewCur.BringToFront();
            return;
        }

        if (_formEtrans834Import == null || _formEtrans834Import.IsDisposed)
        {
            _formEtrans834Import = new FormEtrans834Import();
        }

        _formEtrans834Import.Show();
        _formEtrans834Import.BringToFront();
    }

    private void butSendClaims_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ClaimSend))
        {
            return;
        }

        if (_formClaimsSend is {IsDisposed: false})
        {
            _formClaimsSend.Focus();

            return;
        }

        Cursor = Cursors.WaitCursor;

        _formClaimsSend = new FormClaimsSend();
        _formClaimsSend.FormClosed += (_, _) => { ODEvent.Fired -= formClaimsSend_GoToChanged; };

        ODEvent.Fired += formClaimsSend_GoToChanged;

        _formClaimsSend.Show();
        _formClaimsSend.BringToFront();

        Cursor = Cursors.Default;
    }

    private void butTasks_Click(object sender, EventArgs e)
    {
        LaunchTaskWindow();
    }

    private static void formClaimsSend_GoToChanged(ODEventArgs e)
    {
        if (e.EventType != ODEventType.FormClaimSend_GoTo)
        {
            return;
        }

        var claimSendQueueItem = (ClaimSendQueueItem) e.Tag;
        var patient = Patients.GetPat(claimSendQueueItem.PatNum);
        
        GlobalFormOpenDental.PatientSelected(patient, false);
        GlobalFormOpenDental.GoToModule(EnumModuleType.Account, claimNum: claimSendQueueItem.ClaimNum);
    }

    public void InitializeOnStartup()
    {
    }

    public void LaunchTaskWindow(UserControlTasksTab tab = UserControlTasksTab.Invalid)
    {
        var formTasks = new FormTasks();

        formTasks.Show();

        if (tab != UserControlTasksTab.Invalid)
        {
            formTasks.SetUserControlTasksTab(tab);
        }
    }

    public void ModuleSelected(long patNum)
    {
        _patNum = patNum;
        RefreshModuleData();
        RefreshModuleScreen();
    }

    public void TryRefreshFormClaimSend()
    {
        if (_formClaimsSend != null && !FormODBase.IsDisposedOrClosed(_formClaimsSend))
        {
            _formClaimsSend.RefreshClaimsGrid();
        }
    }

    private void RefreshModuleData()
    {
        if (PrefC.GetBool(PrefName.LocalTimeOverridesServerTime))
        {
            new TimeSpan(0);
        }
        else
        {
        }

        Employees.RefreshCache();
    }

    private void RefreshModuleScreen()
    {
        butImportInsPlans.Visible = true;

        if (PrefC.GetBool(PrefName.EasyHidePublicHealth))
        {
            butImportInsPlans.Visible = false;
        }
    }

    private void ShowBilling(List<long> listClinicNums, bool isHistStartMinDate = false, bool showBillTransSinceZero = false, bool isAllSelected = false, List<StatementMode> listStatementModesForSms = null)
    {
        if (_formBilling is {IsDisposed: false})
        {
            _formBilling.Close();
        }

        _formBilling = new FormBilling();
        _formBilling.ClinicNumsSelectedInitial = listClinicNums;
        _formBilling.IsAllSelected = isAllSelected;
        _formBilling.IsHistoryStartMinDate = isHistStartMinDate;
        _formBilling.ShowBillTransSinceZero = showBillTransSinceZero;
        _formBilling.ListStatementModesForSms = listStatementModesForSms ?? EnumTools.ConvertListOfIntsToListOfEnums<StatementMode>(PrefC.GetString(PrefName.BillingDefaultsModesToText));
        _formBilling.Show();
        _formBilling.BringToFront();
    }


    private void ShowBillingOptions(long clinicNum)
    {
        using var formBillingOptions = new FormBillingOptions();

        formBillingOptions.ListClinicNumsSelected = [clinicNum];

        if (formBillingOptions.ShowDialog() == DialogResult.OK)
        {
            ShowBilling(formBillingOptions.ListClinicNumsSelected, formBillingOptions.IsHistoryStartMinDate, formBillingOptions.ShowBillTransSinceZero, formBillingOptions.IsAllSelected, formBillingOptions.ListStatementModesForSMS);
        }
    }
}