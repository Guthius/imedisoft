using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.Cloud.Storage;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Main_Modules;

public partial class ControlManage : UserControl
{
    public FormAccounting FormAccounting;

    private readonly ErrorProvider _errorProvider1 = new();
    private readonly List<TimeClockStatus> _listTimeClockStatusesShown = [];

    private SigElementDef[] _sigElementDefsForExtra;
    private SigElementDef[] _sigElementDefsForMessage;
    private SigElementDef[] _sigElementDefsForUser;
    private Employee _employee;
    private FormArManager _formArManager;
    private FormBilling _formBilling;
    private FormClaimsSend _formClaimsSend;
    private FormEmailInbox _formEmailInbox;
    private FormEtrans834Import _formEtrans834Import;
    private List<Employee> _listEmployees = [];
    private List<SigMessage> _sigMessages;
    private long _patNum;
    private TimeSpan _timeSpanDelta;

    public ControlManage()
    {
        InitializeComponent();

        Font = new("Microsoft Sans Serif", 8.25f);
    }

    private void butAccounting_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Accounting))
        {
            return;
        }

        if (FormAccounting == null || FormAccounting.IsDisposed)
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

    private void butBackup_Click(object sender, EventArgs e)
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

        DataValid.SetInvalid(true);

        ModuleSelected(_patNum);
    }

    private void butBilling_Click(object sender, EventArgs e)
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

    private void butBreaks_Click(object sender, EventArgs e)
    {
        if (PayPeriods.GetCount() == 0)
        {
            MsgBox.Show(this, "The adminstrator needs to setup pay periods first.");
            return;
        }

        using var formTimeCard = new FormTimeCard(_listEmployees);

        formTimeCard.EmployeeCur = _employee;
        formTimeCard.IsBreaks = true;
        formTimeCard.ShowDialog();

        ModuleSelected(_patNum);
    }

    private void butClaimPay_Click(object sender, EventArgs e)
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

    private void butClockIn_Click(object sender, EventArgs e)
    {
        var progress = new ProgressWin
        {
            ShowCancelButton = false,
            ActionMain = () =>
            {
                ClockEvents.ClockIn(_employee.EmployeeNum, isAtHome: false);
                Thread.Sleep(1000);
            },
            StartingMessage = "Processing clock event..."
        };

        try
        {
            progress.ShowDialog();
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
            return;
        }

        var employeeOld = _employee.Copy();

        _employee.ClockStatus = "Working";

        Employees.UpdateChanged(_employee, employeeOld, true);

        ModuleSelected(_patNum);

        if (!PayPeriods.HasPayPeriodForDate(DateTime.Today))
        {
            MsgBox.Show(this,
                "No dates exist for this pay period.  " +
                "Time clock events will not display until pay periods have been created for this date range");
        }
    }

    private void butClockOut_Click(object sender, EventArgs e)
    {
        if (listBoxStatus.SelectedIndex == -1)
        {
            MsgBox.Show(this, "Please select a status first.");
            return;
        }

        var progress = new ProgressWin
        {
            ShowCancelButton = false,
            ActionMain = () =>
            {
                ClockEvents.ClockOut(_employee.EmployeeNum, _listTimeClockStatusesShown[listBoxStatus.SelectedIndex]);
                Thread.Sleep(1000);
            },
            StartingMessage = "Processing clock event..."
        };

        try
        {
            progress.ShowDialog();
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
            return;
        }

        DataValid.SetInvalid(InvalidType.PhoneEmpDefaults);

        var employeeOld = _employee.Copy();

        _employee.ClockStatus = Lan.g("enumTimeClockStatus", _listTimeClockStatusesShown[listBoxStatus.SelectedIndex].GetDescription());

        Employees.UpdateChanged(_employee, employeeOld, true);

        ModuleSelected(_patNum);
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

    private void butManage_Click(object sender, EventArgs e)
    {
        using var formTimeCardManage = new FormTimeCardManage(_listEmployees);

        formTimeCardManage.ShowDialog();

        ModuleSelected(_patNum);
    }

    private void butManageAR_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Billing))
        {
            return;
        }

        if (!Programs.IsEnabled(ProgramName.Transworld))
        {
            const string url = "https://opendental.com/resources/redirects/redirecttransworldsystems.html";
            try
            {
                Process.Start(url);
            }
            catch
            {
                MsgBox.Show(this,
                    "Failed to open web browser.  " +
                    "Please make sure you have a default browser set and are connected to the internet and then try again.");
            }

            return;
        }

        if (_formArManager == null || _formArManager.IsDisposed)
        {
            while (!ValidateConnectionDetails())
            {
                const string messageText =
                    "An SFTP connection could not be made using the connection details for any clinic " +
                    "in the enabled Transworld (TSI) program link.  " +
                    "Would you like to edit the Transworld program link now?";

                if (!MsgBox.Show(this, MsgBoxButtons.YesNo, messageText))
                {
                    return;
                }

                using var formTransworldSetup = new FormTransworldSetup();
                if (formTransworldSetup.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            _formArManager = new FormArManager();
            _formArManager.FormClosed += (_, _) => { _formArManager = null; };
        }

        _formArManager.Restore();
        _formArManager.Show();
        _formArManager?.BringToFront();
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
        LaunchTaskWindow(false);
    }

    private void butTimeCard_Click(object sender, EventArgs e)
    {
        if (PayPeriods.GetCount() == 0)
        {
            MsgBox.Show(this, "The adminstrator needs to setup pay periods first.");
            return;
        }

        using var formTimeCard = new FormTimeCard(_listEmployees);
        
        formTimeCard.EmployeeCur = _employee;
        formTimeCard.ShowDialog();
        
        ModuleSelected(_patNum);
    }

    private void butViewSched_Click(object sender, EventArgs e)
    {
        var listEmployeeNumsPreSelected = gridEmp.SelectedGridRows.Select(x => ((Employee) x.Tag).EmployeeNum).ToList();
        var listProvNumsPreSelected = Userods
            .GetWhere(x => listEmployeeNumsPreSelected.Contains(x.EmployeeNum) && x.ProvNum != 0)
            .Select(x => x.ProvNum)
            .ToList();
        
        using var formSchedule = new FormSchedule(listEmployeeNumsPreSelected, listProvNumsPreSelected);
        
        formSchedule.ShowDialog();
    }

    private void butAck_Click(object sender, EventArgs e)
    {
        if (gridMessages.SelectedIndices.Length == 0)
        {
            MsgBox.Show(this, "Please select at least one item first.");
            return;
        }

        for (var i = gridMessages.SelectedIndices.Length - 1; i >= 0; i--)
        {
            var sigMessage = (SigMessage) gridMessages.ListGridRows[gridMessages.SelectedIndices[i]].Tag;
            if (sigMessage.AckDateTime.Year > 1880)
            {
                continue;
            }

            SigMessages.AckSigMessage(sigMessage);
            
            if (checkIncludeAck.Checked)
            {
                gridMessages.ListGridRows[gridMessages.SelectedIndices[i]].Cells[3].Text = sigMessage.MessageDateTime.ToShortTimeString();
                Signalods.SetInvalid(InvalidType.SigMessages, KeyType.SigMessage, sigMessage.SigMessageNum);
                continue;
            }

            try
            {
                gridMessages.ListGridRows.RemoveAt(gridMessages.SelectedIndices[i]);
            }
            catch
            {
                // ignored
            }

            Signalods.SetInvalid(InvalidType.SigMessages, KeyType.SigMessage, sigMessage.SigMessageNum);
        }

        gridMessages.SetAll(false);
    }

    private void butSend_Click(object sender, EventArgs e)
    {
        if (textMessage.Text == "")
        {
            MsgBox.Show(this, "Please type in a message first.");
            return;
        }

        var sigMessage = new SigMessage
        {
            SigText = textMessage.Text
        };
        
        if (listBoxTo.SelectedIndex != -1)
        {
            sigMessage.ToUser = _sigElementDefsForUser[listBoxTo.SelectedIndex].SigText;
            sigMessage.SigElementDefNumUser = _sigElementDefsForUser[listBoxTo.SelectedIndex].SigElementDefNum;
        }

        if (listBoxFrom.SelectedIndex != -1)
        {
            sigMessage.FromUser = _sigElementDefsForUser[listBoxFrom.SelectedIndex].SigText;
        }

        if (listBoxExtras.SelectedIndex != -1)
        {
            sigMessage.SigElementDefNumExtra = _sigElementDefsForExtra[listBoxExtras.SelectedIndex].SigElementDefNum;
        }

        SigMessages.Insert(sigMessage);
        textMessage.Text = "";
        listBoxFrom.SelectedIndex = -1;
        listBoxTo.SelectedIndex = -1;
        listBoxExtras.SelectedIndex = -1;
        listBoxMessages.SelectedIndex = -1;
        ShowSendingLabel();
        Signalods.SetInvalid(InvalidType.SigMessages, KeyType.SigMessage, sigMessage.SigMessageNum);
    }

    private void checkIncludeAck_Click(object sender, EventArgs e)
    {
        if (checkIncludeAck.Checked)
        {
            textDays.Text = "1";
            labelDays.Visible = true;
            textDays.Visible = true;
            FillMessages();
            return;
        }

        labelDays.Visible = false;
        textDays.Visible = false;
        _sigMessages = SigMessages.GetSigMessagesSinceDateTime(DateTime.Today); //since midnight this morning.
        FillMessages();
    }

    private void comboViewUser_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillMessages();
    }

    private void listMessages_Click(object sender, EventArgs e)
    {
        if (listBoxMessages.SelectedIndex == -1)
        {
            return;
        }

        var sigMessage = new SigMessage
        {
            SigText = textMessage.Text
        };
        
        if (listBoxTo.SelectedIndex != -1)
        {
            sigMessage.ToUser = _sigElementDefsForUser[listBoxTo.SelectedIndex].SigText;
            sigMessage.SigElementDefNumUser = _sigElementDefsForUser[listBoxTo.SelectedIndex].SigElementDefNum;
        }

        if (listBoxFrom.SelectedIndex != -1)
        {
            sigMessage.FromUser = _sigElementDefsForUser[listBoxFrom.SelectedIndex].SigText;
            //We do not set a SigElementDefNumUser for From.
        }

        if (listBoxExtras.SelectedIndex != -1)
        {
            sigMessage.SigElementDefNumExtra = _sigElementDefsForExtra[listBoxExtras.SelectedIndex].SigElementDefNum;
        }

        sigMessage.SigElementDefNumMsg = _sigElementDefsForMessage[listBoxMessages.SelectedIndex].SigElementDefNum;
        //need to do this all as a transaction, so need to do a writelock on the signal table first.
        //alternatively, we could just make sure not to retrieve any signals that were less the 300ms old.
        SigMessages.Insert(sigMessage);
        //reset the controls
        textMessage.Text = "";
        listBoxFrom.SelectedIndex = -1;
        listBoxTo.SelectedIndex = -1;
        listBoxExtras.SelectedIndex = -1;
        listBoxMessages.SelectedIndex = -1;
        ShowSendingLabel();
        Signalods.SetInvalid(InvalidType.SigMessages, KeyType.SigMessage, sigMessage.SigMessageNum);
    }

    private void textDays_TextChanged(object sender, EventArgs e)
    {
        if (!textDays.Visible)
        {
            _errorProvider1.SetError(textDays, "");
            return;
        }

        int numDays;
        try
        {
            numDays = int.Parse(textDays.Text);
        }
        catch
        {
            _errorProvider1.SetError(textDays, "Invalid number.  Usually 1 or 2.");
            return;
        }

        _errorProvider1.SetError(textDays, "");
        _sigMessages = SigMessages.GetSigMessagesSinceDateTime(DateTime.Today.AddDays(-numDays));
        try
        {
            FillMessages();
        }
        catch
        {
            _errorProvider1.SetError(textDays, "Invalid number.  Usually 1 or 2.");
        }
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

    private void gridEmp_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridEmp.SelectedIndices.Length != 1)
        {
            EnableTimeControlsForEmpI(-1);
            return;
        }

        var isPrefTimeCardSecurityEnabled = PrefC.GetBool(PrefName.TimecardSecurityEnabled);
        if (!isPrefTimeCardSecurityEnabled)
        {
            EnableTimeControlsForEmpI(e.Row);
            return;
        }

        if (Security.CurUser.EmployeeNum == ((Employee) gridEmp.ListGridRows[e.Row].Tag).EmployeeNum)
        {
            EnableTimeControlsForEmpI(e.Row);
            return;
        }

        if (Security.IsAuthorized(EnumPermType.TimecardsEditAll, true))
        {
            EnableTimeControlsForEmpI(e.Row);
            return;
        }

        EnableTimeControlsForEmpI(-1);
    }

    private void gridEmp_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (gridEmp.SelectedGridRows.Count > 1)
        {
            return;
        }

        if (PayPeriods.GetCount() == 0)
        {
            MsgBox.Show(this, "The adminstrator needs to setup pay periods first.");
            return;
        }

        if (!butTimeCard.Enabled)
        {
            return;
        }

        using var formTimeCard = new FormTimeCard(_listEmployees);

        formTimeCard.EmployeeCur = (Employee) gridEmp.ListGridRows[e.Row].Tag;
        formTimeCard.ShowDialog();

        ModuleSelected(_patNum);
    }

    private void textFilterName_TextChanged(object sender, EventArgs e)
    {
        FillEmps(false);
    }

    private void timerUpdateTime_Tick(object sender, EventArgs e)
    {
        if (Visible)
        {
            labelTime.Text = (DateTime.Now + _timeSpanDelta).ToLongTimeString();
        }
    }

    public void InitializeOnStartup()
    {
        RefreshFullMessages();
    }

    public void JumpToTriageTaskWindow()
    {
        LaunchTaskWindow(true);
    }

    public void LaunchTaskWindow(bool isTriage, UserControlTasksTab tab = UserControlTasksTab.Invalid)
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

    public void ModuleUnselected()
    {
    }

    public void TryRefreshFormClaimSend()
    {
        if (_formClaimsSend != null && !FormODBase.IsDisposedOrClosed(_formClaimsSend))
        {
            _formClaimsSend.RefreshClaimsGrid();
        }
    }

    public void LogMsgs(List<SigMessage> sigMessages)
    {
        foreach (var sigMessage in sigMessages)
        {
            var sigMessageUpdate = _sigMessages.FirstOrDefault(x => x.SigMessageNum == sigMessage.SigMessageNum);
            if (sigMessageUpdate is null)
            {
                _sigMessages.Add(sigMessage.Copy());
                continue;
            }

            sigMessageUpdate.AckDateTime = sigMessage.AckDateTime;
        }

        _sigMessages.Sort();

        FillMessages();
    }

    private static string ConvertClockStatus(string status)
    {
        if (!PrefC.GetBool(PrefName.ClockEventAllowBreak) && status == TimeClockStatus.Lunch.GetDescription())
        {
            status = TimeClockStatus.Break.GetDescription();
        }

        return Lans.g("enumTimeClockStatus", status);
    }

    private void FillEmps(bool selectUserEmployee)
    {
        gridEmp.BeginUpdate();

        gridEmp.Columns.Clear();
        gridEmp.Columns.Add(new GridColumn("Employee", 180));
        gridEmp.Columns.Add(new GridColumn("Status", 104));

        gridEmp.ListGridRows.Clear();

        _listEmployees = Employees.GetEmpsForClinic(Clinics.ClinicNum, false, true);

        for (var i = 0; i < _listEmployees.Count(); i++)
        {
            var isEmployeeFNameStartingWithFilterName = _listEmployees[i].FName.ToLower().StartsWith(textFilterName.Text.ToLower());
            if (textFilterName.Text != "" && !isEmployeeFNameStartingWithFilterName)
            {
                continue;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(Employees.GetName(_listEmployees[i]));
            gridRow.Cells.Add(ConvertClockStatus(_listEmployees[i].ClockStatus));
            gridRow.Tag = _listEmployees[i];

            gridEmp.ListGridRows.Add(gridRow);
        }

        gridEmp.EndUpdate();

        listBoxStatus.Items.Clear();

        _listTimeClockStatusesShown.Clear();

        var timeClockStatuses = Enum.GetValues(typeof(TimeClockStatus)).Cast<TimeClockStatus>().ToList();

        foreach (var timeClockStatus in timeClockStatuses)
        {
            var statusDescript = timeClockStatus.GetDescription();
            if (!PrefC.GetBool(PrefName.ClockEventAllowBreak))
            {
                switch (timeClockStatus)
                {
                    case TimeClockStatus.Break:
                        continue;

                    case TimeClockStatus.Lunch:
                        statusDescript = TimeClockStatus.Break.GetDescription();
                        break;
                }
            }

            _listTimeClockStatusesShown.Add(timeClockStatus);
            listBoxStatus.Items.Add(Lan.g("enumTimeClockStatus", statusDescript));
        }

        var index = -1;
        if (!selectUserEmployee)
        {
            EnableTimeControlsForEmpI(index); //No employee selected, disable time clock controls
            return;
        }

        for (var i = 0; i < gridEmp.ListGridRows.Count; i++)
        {
            var employee = (Employee) gridEmp.ListGridRows[i].Tag;
            if (employee.EmployeeNum != Security.CurUser.EmployeeNum)
            {
                continue;
            }

            index = i;
            break;
        }

        gridEmp.SetSelected(index);

        EnableTimeControlsForEmpI(index);
    }

    private void RefreshModuleData()
    {
        if (PrefC.GetBool(PrefName.LocalTimeOverridesServerTime))
        {
            _timeSpanDelta = new TimeSpan(0);
        }
        else
        {
            _timeSpanDelta = MiscData.GetNowDateTime() - DateTime.Now;
        }

        Employees.RefreshCache();
    }

    private void RefreshModuleScreen()
    {
        labelCurrentTime.Text = PrefC.GetBool(PrefName.LocalTimeOverridesServerTime) ? "Local Time" : "Server Time";

        labelTime.Text = (DateTime.Now + _timeSpanDelta).ToLongTimeString();
        textFilterName.Text = "";

        FillEmps(true);
        FillMessageDefs();

        butManage.Enabled = Security.IsAuthorized(EnumPermType.TimecardsEditAll, true);
        butBreaks.Visible = PrefC.GetBool(PrefName.ClockEventAllowBreak);
        butImportInsPlans.Visible = true;

        if (PrefC.GetBool(PrefName.EasyHidePublicHealth))
        {
            butImportInsPlans.Visible = false;
        }

        butManageAR.Visible = !ProgramProperties.IsAdvertisingDisabled(ProgramName.Transworld);
    }

    private void EnableTimeControlsForEmpI(int index)
    {
        if (index == -1)
        {
            butClockIn.Enabled = false;
            butClockOut.Enabled = false;
            butTimeCard.Enabled = false;
            butBreaks.Enabled = false;
            listBoxStatus.Enabled = false;
            return;
        }

        _employee = (Employee) gridEmp.ListGridRows[index].Tag;
        var clockEvent = ClockEvents.GetLastEvent(_employee.EmployeeNum);
        if (clockEvent == null)
        {
            //new employee.  They need to clock in.
            butClockIn.Enabled = true;
            butClockOut.Enabled = false;
            butTimeCard.Enabled = true;
            butBreaks.Enabled = true;
            listBoxStatus.SelectedIndex = _listTimeClockStatusesShown.IndexOf(TimeClockStatus.Home);
            listBoxStatus.Enabled = false;
            return;
        }

        if (clockEvent.ClockStatus == TimeClockStatus.Break)
        {
            //only incomplete breaks will have been returned.
            //clocked out for break, but not clocked back in
            butClockIn.Enabled = true;
            butClockOut.Enabled = false;
            butTimeCard.Enabled = true;
            butBreaks.Enabled = true;
            listBoxStatus.SelectedIndex = _listTimeClockStatusesShown.IndexOf(PrefC.GetBool(PrefName.ClockEventAllowBreak) ? TimeClockStatus.Break : TimeClockStatus.Lunch);
            listBoxStatus.Enabled = false;
            return;
        }

        //normal clock in/out
        if (clockEvent.TimeDisplayed2.Year < 1880)
        {
            butClockIn.Enabled = false;
            butClockOut.Enabled = true;
            butTimeCard.Enabled = true;
            butBreaks.Enabled = true;
            listBoxStatus.Enabled = true;
            return;
        }

        //clocked out for home or lunch.  Need to clock back in.
        butClockIn.Enabled = true;
        butClockOut.Enabled = false;
        butTimeCard.Enabled = true;
        butBreaks.Enabled = true;
        listBoxStatus.SelectedIndex = (int) clockEvent.ClockStatus;
        listBoxStatus.Enabled = false;
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

    private static bool ValidateConnectionDetails()
    {
        var program = Programs.GetCur(ProgramName.Transworld);

        var clinicNums = Clinics.GetAllForUserod(Security.CurUser).Select(x => x.Id).ToList();
        if (!Security.CurUser.ClinicIsRestricted)
        {
            clinicNums.Add(0);
        }

        var allProgramProperties = ProgramProperties.GetForProgram(program.ProgramNum);
        foreach (var clinicNum in clinicNums)
        {
            if (allProgramProperties.All(x => x.ClinicNum != clinicNum))
            {
                continue;
            }

            var properties = allProgramProperties.FindAll(x => x.ClinicNum == clinicNum);

            var sftpServerAddress = properties.Find(x => x.PropertyDesc == "SftpServerAddress")?.PropertyValue ?? "";
            var sftpUsername = properties.Find(x => x.PropertyDesc == "SftpUsername")?.PropertyValue ?? "";
            var sftpPassword = Class1.TryDecrypt(properties.Find(x => x.PropertyDesc == "SftpPassword")?.PropertyValue ?? "");

            if (!int.TryParse(properties.Find(x => x.PropertyDesc == "SftpServerPort")?.PropertyValue ?? "", out var sftpPort))
            {
                sftpPort = 22;
            }

            if (Sftp.IsConnectionValid(sftpServerAddress, sftpUsername, sftpPassword, sftpPort))
            {
                return true;
            }
        }

        return false;
    }

    private void FillMessageDefs()
    {
        _sigElementDefsForUser = SigElementDefs.GetSubList(SignalElementType.User);
        _sigElementDefsForExtra = SigElementDefs.GetSubList(SignalElementType.Extra);
        _sigElementDefsForMessage = SigElementDefs.GetSubList(SignalElementType.Message);

        listBoxTo.Items.Clear();
        listBoxTo.Items.AddList(_sigElementDefsForUser, x => x.SigText);

        listBoxFrom.Items.Clear();
        listBoxFrom.Items.AddList(_sigElementDefsForUser, x => x.SigText);

        listBoxExtras.Items.Clear();
        listBoxExtras.Items.AddList(_sigElementDefsForExtra, x => x.SigText);

        listBoxMessages.Items.Clear();
        listBoxMessages.Items.AddList(_sigElementDefsForMessage, x => x.SigText);

        comboBoxViewUser.Items.Clear();
        comboBoxViewUser.Items.Add("all");
        foreach (var sigElementDef in _sigElementDefsForUser)
        {
            comboBoxViewUser.Items.Add(sigElementDef.SigText);
        }

        comboBoxViewUser.SelectedIndex = 0;
    }

    private void FillMessages()
    {
        if (textDays.Visible && _errorProvider1.GetError(textDays) != "")
        {
            return;
        }

        var selectedSigMessageNums = gridMessages.SelectedTags<SigMessage>().Select(x => x.SigMessageNum).ToList();

        gridMessages.BeginUpdate();

        gridMessages.Columns.Clear();
        gridMessages.Columns.Add(new GridColumn("To", 60));
        gridMessages.Columns.Add(new GridColumn("From", 60));
        gridMessages.Columns.Add(new GridColumn("Sent", 63));
        gridMessages.Columns.Add(new GridColumn("Ack'd", 63) {TextAlign = HorizontalAlignment.Center});
        gridMessages.Columns.Add(new GridColumn("Text", 274));
        gridMessages.ListGridRows.Clear();

        foreach (var sigMessage in _sigMessages)
        {
            if (checkIncludeAck.Checked)
            {
                if (sigMessage.AckDateTime.Year > 1880 && sigMessage.AckDateTime < DateTime.Today.AddDays(1 - SIn.Long(textDays.Text)))
                {
                    continue;
                }
            }
            else
            {
                if (sigMessage.AckDateTime.Year > 1880)
                {
                    continue;
                }
            }

            if (sigMessage.ToUser != "" && comboBoxViewUser.SelectedIndex != 0 && _sigElementDefsForUser != null && _sigElementDefsForUser[comboBoxViewUser.SelectedIndex - 1].SigText != sigMessage.ToUser)
            {
                continue;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(sigMessage.ToUser);
            gridRow.Cells.Add(sigMessage.FromUser);

            if (sigMessage.MessageDateTime.Date == DateTime.Today)
            {
                gridRow.Cells.Add(sigMessage.MessageDateTime.ToShortTimeString());
            }
            else
            {
                gridRow.Cells.Add(sigMessage.MessageDateTime.ToShortDateString() + "\r\n" + sigMessage.MessageDateTime.ToShortTimeString());
            }

            if (sigMessage.AckDateTime.Year > 1880)
            {
                if (sigMessage.AckDateTime.Date == DateTime.Today)
                {
                    gridRow.Cells.Add(sigMessage.AckDateTime.ToShortTimeString());
                }
                else
                {
                    gridRow.Cells.Add(sigMessage.AckDateTime.ToShortDateString() + "\r\n" + sigMessage.AckDateTime.ToShortTimeString());
                }
            }
            else
            {
                gridRow.Cells.Add("");
            }

            var strSigText = sigMessage.SigText;
            var sigElementDefExtra = SigElementDefs.GetElementDef(sigMessage.SigElementDefNumExtra);
            if (sigElementDefExtra != null && !string.IsNullOrEmpty(sigElementDefExtra.SigText))
            {
                strSigText += (strSigText == "") ? "" : ".  ";
                strSigText += sigElementDefExtra.SigText;
            }

            var sigElementDefMsg = SigElementDefs.GetElementDef(sigMessage.SigElementDefNumMsg);
            if (sigElementDefMsg != null && !string.IsNullOrEmpty(sigElementDefMsg.SigText))
            {
                strSigText += (strSigText == "") ? "" : ".  ";
                strSigText += sigElementDefMsg.SigText;
            }

            gridRow.Cells.Add(strSigText);
            gridRow.Tag = sigMessage.Copy();
            gridMessages.ListGridRows.Add(gridRow);
        }

        gridMessages.EndUpdate();

        for (var i = 0; i < gridMessages.ListGridRows.Count; i++)
        {
            var sigMessage = (SigMessage) gridMessages.ListGridRows[i].Tag;
            if (selectedSigMessageNums.Contains(sigMessage.SigMessageNum))
            {
                gridMessages.SetSelected(i);
            }
        }
    }

    private void RefreshFullMessages()
    {
        _sigMessages = SigMessages.GetSigMessagesSinceDateTime(DateTime.Today);

        FillMessages();
    }

    private void ShowSendingLabel()
    {
        labelSending.Visible = true;

        var thread = new ODThread(_ =>
        {
            Thread.Sleep((int) TimeSpan.FromSeconds(1).TotalMilliseconds);

            ODException.SwallowAnyException(() => { Invoke(() => { labelSending.Visible = false; }); });
        });

        thread.Start();
    }
}