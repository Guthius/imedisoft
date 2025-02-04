using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CDT;
using CodeBase;
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

    private readonly List<TimeClockStatus> _listTimeClockStatusesShown = [];
    
    private Employee _employee;
    private FormBilling _formBilling;
    private FormClaimsSend _formClaimsSend;
    private FormEmailInbox _formEmailInbox;
    private FormEtrans834Import _formEtrans834Import;
    private List<Employee> _listEmployees = [];
    private long _patNum;
    private TimeSpan _timeSpanDelta;

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

        DataValid.SetInvalid(true);

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

    private void ButtonBreaks_Click(object sender, EventArgs e)
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

    private void ButtonClockIn_Click(object sender, EventArgs e)
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
                "No dates exist for this pay period. " +
                "Time clock events will not display until pay periods have been created for this date range");
        }
    }

    private void ButtonClockOut_Click(object sender, EventArgs e)
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

        butManage.Enabled = Security.IsAuthorized(EnumPermType.TimecardsEditAll, true);
        butBreaks.Visible = PrefC.GetBool(PrefName.ClockEventAllowBreak);
        butImportInsPlans.Visible = true;

        if (PrefC.GetBool(PrefName.EasyHidePublicHealth))
        {
            butImportInsPlans.Visible = false;
        }
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
}