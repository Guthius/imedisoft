using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using OpenDental.UI;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Features.Core;
using OpenDental.Forms;
using OpenDental.Logic;
using OpenDental.Main_Modules;

namespace OpenDental;

public partial class FormOpenDental : FormODBase
{
    public static int ExitCode;

    private const int PopupPressureReliefLimit = 20;

    private static FormOpenDental _formOpenDentalSingleton;
    private static Dictionary<long, Dictionary<long, DateTime>> _dictionaryBlockedAutomations;
    private static bool _isTreatPlanSortByTooth;
    private static long _patNumCur;
    private static HttpListener _httpListenerApi;

    private EnumModuleType _moduleTypeLast;
    private List<PopupEvent> _popupEvents;
    private FormAlerts _formAlerts;
    private FormTerminalManager _formTerminalManager;
    private Form _formRecentlyOpenForLogoff;
    private bool _isFormLogOnLastActive;
    private FormCertifications _formCertifications;
    private FormCreditRecurringCharges _formCreditRecurringCharges;
    private long _patNumPrevious;
    private DateTime _datePopupDelay;
    private readonly Dictionary<string, object> _dictionaryChartPrefsCache = new();
    private readonly Dictionary<string, object> _dictionaryTaskListPrefsCache = new();
    private ODToolBarButton _toolBarButtonText;
    private ODToolBarButton _toolBarButtonTask;
    private FormSmsTextMessaging _formSmsTextMessaging;
    private FormUserQuery _formUserQuery;
    private List<Task> _tasksReminders;
    private Dictionary<long, TaskList> _dictionaryAllTaskLists;
    private List<Task> _listTasksRemindersOverLimit;
    private List<long> _listTaskNumsNormal;
    private long _userNumTasks;
    private DateTime _dateReminderRefresh = DateTime.MinValue;
    private List<AlertRead> _alertItemReads = [];
    private List<AlertItem> _alertItems = [];
    private FormXWebTransactions _formXWebTransactions;
    private Exception _exceptionSignalsTick;
    private bool _onlyProcessHighPrioritySignals;
    private FormRpDPPOvercharged _formRpDppOvercharged;
    private readonly PatientData _pd = new();

    public FormOpenDental()
    {
        _formOpenDentalSingleton = this;

        var formSplash = new FormSplash();

        formSplash.Show();

        InitializeComponent();

        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        
        controlAppt = new ControlAppt {Visible = false};
        controlAppt.Dock = DockStyle.Fill;
        splitContainer.Controls.Add(controlAppt);

        controlFamily = new ControlFamily {Visible = false};
        controlFamily.Dock = DockStyle.Fill;
        splitContainer.Controls.Add(controlFamily);

        controlAccount = new ControlAccount {Visible = false};
        controlAccount.Dock = DockStyle.Fill;
        splitContainer.Controls.Add(controlAccount);

        controlTreat = new ControlTreat {Visible = false};
        controlTreat.Dock = DockStyle.Fill;
        splitContainer.Controls.Add(controlTreat);

        controlChart = new ControlChart();
        controlChart.Visible = false;
        controlChart.Dock = DockStyle.Fill;
        controlChart.EventImageClick += ControlChart_ImageClick;
        controlChart.Pd = _pd;
        splitContainer.Controls.Add(controlChart);

        controlManage = new ControlManage {Visible = false};
        controlManage.Dock = DockStyle.Fill;
        splitContainer.Controls.Add(controlManage);
        
        DataValid.EventInvalid += (_, e) => DataValid_BecameInvalid(e);
        GlobalFormOpenDental.EventLockODForMountAcquire += (_, isEnabled) => LockODForMountAcquire(isEnabled);
        GlobalFormOpenDental.EventRefreshCurrentModule += (_, isClinicRefresh) => RefreshCurrentModule(isClinicRefresh: isClinicRefresh);
        GlobalFormOpenDental.EventModuleSelected += (_, e) => GotoModule_ModuleSelected(e);
        GlobalFormOpenDental.EventPatientSelected += (_, e) => Contr_PatientSelected(e);
        GlobalFormOpenDental.SendTextMessage = toolButTxtMsg_Click;
        GlobalFormOpenDental.GoToModule = GotoModule_ModuleSelected;
        GlobalFormOpenDental.ControlMainForm = this;

        FormLauncher.EventLaunch += FormLauncherHelper.Launch;

        formSplash.Close();
    }

    [Browsable(false)]
    public static long PatNumCur
    {
        get => _patNumCur;
        set
        {
            if (value == _patNumCur)
            {
                return;
            }

            _patNumCur = value;

            ODEvent.Fire(ODEventType.Patient, value);
        }
    }

    [Browsable(false)]
    public static Dictionary<long, Dictionary<long, DateTime>> DicBlockedAutomations
    {
        get
        {
            if (_dictionaryBlockedAutomations == null)
            {
                _dictionaryBlockedAutomations = new Dictionary<long, Dictionary<long, DateTime>>();
                return _dictionaryBlockedAutomations;
            }

            var automationNums = _dictionaryBlockedAutomations.Keys.ToList();
            foreach (var automationNum in automationNums)
            {
                var patNums = _dictionaryBlockedAutomations[automationNum].Keys.ToList();
                foreach (var patNum in patNums)
                {
                    if (_dictionaryBlockedAutomations[automationNum][patNum] > DateTime.Now)
                    {
                        continue;
                    }

                    _dictionaryBlockedAutomations[automationNum].Remove(patNum);
                }

                if (_dictionaryBlockedAutomations[automationNum].Count == 0)
                {
                    _dictionaryBlockedAutomations.Remove(automationNum);
                }
            }

            return _dictionaryBlockedAutomations;
        }
    }

    [Browsable(false)]
    public static bool IsTreatPlanSortByTooth
    {
        get => _isTreatPlanSortByTooth;
        set
        {
            _isTreatPlanSortByTooth = value;
            PrefC.IsTreatPlanSortByTooth = value;
        }
    }

    [Browsable(false)]
    public static List<string> S_Contr_TabProcPageTitles => _formOpenDentalSingleton.controlChart.GetListTabProcPageTitles();

    private void FormOpenDental_Load(object sender, EventArgs e)
    {
        LayoutMenu();

        var appDir = Application.StartupPath;
        if (File.Exists(Path.Combine(appDir, "NoD2D.txt")))
        {
        }

        EscClosesWindow = false;
    }

    private void FormOpenDental_Shown(object sender, EventArgs e)
    {
        FormOpenDentalShown();
    }

    public void FormOpenDentalShown()
    {
        ODException.SwallowAnyException(() =>
        {
            System.Windows.Automation.AutomationElement.FromHandle(Handle); //Just invoking this method wakes up something deep within Windows...
        });

        // Flag the userod cache as NOT allowed to cache any items for security purposes.
        Userods.SetIsCacheAllowed(false);

        AllNeutral();

        var odUser = "";
        var odPassword = "";
        var odPassObfuscated = "";
        if (odPassword == "" && odPassObfuscated != "")
        {
            CDT.Class1.Decrypt(odPassObfuscated, out odPassword);
        }

        var mySqlPassword = "";
        var mySqlPassObfuscated = "";
        if (mySqlPassword == "" && mySqlPassObfuscated != "")
        {
            CDT.Class1.Decrypt(mySqlPassObfuscated, out mySqlPassword);
        }

        var formSplash = new FormSplash();

        var selectDatabaseModel = SelectDatabaseModel.Load();
        while (true)
        {
            var selectDatabaseWindow = new SelectDatabaseWindow(selectDatabaseModel);

            if (selectDatabaseModel.HideOnStartup)
            {
                try
                {
                    DataConnection.SetDb(
                        selectDatabaseModel.Server,
                        selectDatabaseModel.Database,
                        selectDatabaseModel.UserId,
                        selectDatabaseModel.Password,
                        false,
                        string.Empty);
                }
                catch
                {
                    if (selectDatabaseWindow.ShowDialog() != true)
                    {
                        Environment.Exit(106);
                        return;
                    }
                }
            }
            else
            {
                if (selectDatabaseWindow.ShowDialog() != true)
                {
                    Environment.Exit(ExitCode);
                    return;
                }
            }

            Cursor = Cursors.WaitCursor;

            formSplash.Show(this);

            if (!PrefsStartup())
            {
                Cursor = Cursors.Default;

                formSplash.Close();

                if (ExitCode == 0)
                {
                    ExitCode = 999;
                }

                Environment.Exit(ExitCode);
                return;
            }

            break;
        }

        ODEvent.Fired += DataConnection_CredentialsFailedAfterLogin;

        RefreshLocalData(InvalidType.Prefs);

        Signalods.ClearOldSignals();

        var invalidTypes = new List<InvalidType>
        {
            InvalidType.Defs,
            InvalidType.Providers,
            InvalidType.Programs,
            InvalidType.ToolButsAndMounts
        };

        RefreshLocalData(invalidTypes.ToArray());
        
        controlManage.InitializeOnStartup();

        if (PrefC.GetBoolSilent(PrefName.ImagesModuleUsesOld2020, false))
        {
            controlImagesOld = new ControlImagesOld {Visible = false};
            controlImagesOld.Dock = DockStyle.Fill;
            splitContainer.Controls.Add(controlImagesOld);
        }
        else
        {
            controlImages = new ControlImages {Visible = false};
            controlImages.Dock = DockStyle.Fill;
            controlImages.EventKeyDown += FormOpenDental_KeyDown;
            splitContainer.Controls.Add(controlImages);
        }

        moduleBar.RefreshButtons();

        if (!File.Exists("Help.chm"))
        {
            _menuItemLocalHelpWindows.Available = false;
        }

        if (!PrefC.GetBool(PrefName.ProcLockingIsAllowed))
        {
            _menuItemProcLockTool.Available = false;
        }
        
        if (Security.IsAuthorized(EnumPermType.ProcCodeEdit, true) && !PrefC.GetBool(PrefName.ADAdescriptionsReset))
        {
            ProcedureCodes.ResetADAdescriptionsAndAbbrs();
            Prefs.UpdateBool(PrefName.ADAdescriptionsReset, true);
        }

        formSplash.Close();

        Signalods.DateTHighPrioritySignalLastRefreshed = MiscData.GetNowDateTime();

        LogOnOpenDentalUser(odUser, odPassword, string.Empty);

        // At this point a user has successfully logged in.
        // Flag the userod cache as safe to cache data.
        Userods.SetIsCacheAllowed(true);

        if (Security.CurUser != null)
        {
            Clinics.LoadClinicNumForUser();

            RefreshMenuClinics();
        }
        
        IsTreatPlanSortByTooth = PrefC.GetBool(PrefName.TreatPlanSortByTooth);

        moduleBar.SelectedIndex = Security.GetModule(0);

        if (HL7Defs.IsExistingHL7Enabled() && !HL7Defs.GetOneDeepEnabled().ShowAppts)
        {
            moduleBar.SelectedModule = EnumModuleType.Chart;
            LayoutControls();
        }

        moduleBar.Invalidate();

        LayoutToolBar();
        RefreshMenuReports();

        Cursor = Cursors.Default;
        if (moduleBar.SelectedModule == EnumModuleType.None)
        {
            ShowError("You do not have permission to use any modules.");
        }

        Bridges.Trojan.StartupCheck();
        Bridges.ICat.StartFileWatcher();
        Bridges.TigerView.StartFileWatcher();

        var backupReminderNeeded = PrefC.GetDate(PrefName.BackupReminderLastDateRun) < DateTime.Today.AddMonths(-1);
        if (backupReminderNeeded)
        {
            var frmBackupReminder = new FrmBackupReminder();

            frmBackupReminder.ShowDialog();

            if (frmBackupReminder.IsDialogOK)
            {
                Prefs.UpdateDateT(PrefName.BackupReminderLastDateRun, DateTime.Today);
            }
            else
            {
                Application.Exit();
                return;
            }
        }

        FillPatientButton(null);

        ODException.SwallowAnyException(() => { Computers.UpdateHeartBeat(Environment.MachineName, true); });

        Text = PatientL.GetMainTitle(Patients.GetPat(PatNumCur), Clinics.ClinicNum);
        Security.DateTimeLastActivity = DateTime.Now;

        ODException.SwallowAnyException(EmailMessages.CreateCertificateStoresIfNeeded);
        
        if (PrefC.GetString(PrefName.LanguageAndRegion) != CultureInfo.CurrentCulture.Name && !ComputerPrefs.LocalComputer.NoShowLanguage)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Warning, having mismatched language settings between the workstation and server may cause the program to behave in unexpected ways");
            stringBuilder.AppendLine("Database setting: " + PrefC.GetString(PrefName.LanguageAndRegion) ?? "");
            stringBuilder.AppendLine("Computer setting: " + CultureInfo.CurrentCulture.Name);
            stringBuilder.AppendLine("Would you like to view the language and region setup window?");

            if (ODMessageBox.Show(stringBuilder.ToString(), "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using var formLanguageAndRegion = new FormLanguageAndRegion();

                formLanguageAndRegion.ShowDialog();
            }
        }

        if (CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits != 2 && !ComputerPrefs.LocalComputer.NoShowDecimal)
        {
            var frmDecimalSettings = new FrmDecimalSettings();

            frmDecimalSettings.ShowDialog();
        }

        if (ComputerPrefs.LocalComputer.GraphicsSimple == DrawingMode.DirectX && ComputerPrefs.LocalComputer.DirectXFormat == "")
        {
            try
            {
                ComputerPrefs.LocalComputer.DirectXFormat = FormGraphics.GetPreferredDirectXFormat(this);
                if (ComputerPrefs.LocalComputer.DirectXFormat == "invalid")
                {
                    ComputerPrefs.LocalComputer.GraphicsSimple = DrawingMode.Simple2D;
                }

                ComputerPrefs.Update(ComputerPrefs.LocalComputer);

                controlChart.InitializeOnStartup();
            }
            catch
            {
                // ignored
            }
        }

        _menuItemReactivation.Available = PrefC.GetBool(PrefName.ShowFeatureReactivations);

        ComputerPrefs.UpdateLocalComputerOs();

        Tasks.NavTaskDelegate = S_TaskNumLoad;

        Signalods.DateTRegularPrioritySignalLastRefreshed = MiscData.GetNowDateTime();
        Signalods.DateTApptSignalLastRefreshed = Signalods.DateTRegularPrioritySignalLastRefreshed;

        SetTimersAndThreads(true);

        var listDefsMisColors = Defs.GetDefsForCategory(DefCat.MiscColors);

        SetBorderColor(DefCatMiscColors.MainBorder, listDefsMisColors[(int) DefCatMiscColors.MainBorder].ItemColor);
        SetBorderColor(DefCatMiscColors.MainBorderOutline, listDefsMisColors[(int) DefCatMiscColors.MainBorderOutline].ItemColor);
        SetBorderColor(DefCatMiscColors.MainBorderText, listDefsMisColors[(int) DefCatMiscColors.MainBorderText].ItemColor);

        LayoutControls();

        _httpListenerApi = new HttpListener();
        _httpListenerApi.Prefixes.Add("http://127.0.0.1:30222/");
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        LayoutControls();
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);

        LayoutControls();

        if (controlAppt.Visible)
        {
            controlAppt.LayoutControls();
        }

        if (controlFamily.Visible)
        {
            controlFamily.Update();
        }

        if (controlAccount.Visible)
        {
            controlAccount.LayoutPanelsAndRefreshMainGrids();
        }

        if (controlTreat.Visible)
        {
            controlTreat.Update();
        }

        if (controlChart.Visible)
        {
            controlChart.LayoutControls();
        }

        if (controlImagesOld is {Visible: true})
        {
            controlImagesOld.Update();
        }

        if (controlImages is {Visible: true})
        {
            controlImages.LayoutControls();
        }

        if (controlManage.Visible)
        {
            controlManage.Update();
        }
    }

    private void FormOpenDental_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F10)
        {
            e.SuppressKeyPress = true;
        }

        if (controlAppt.Visible && e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F12)
        {
            controlAppt.FunctionKeyPress(e.KeyCode);
            return;
        }

        if (controlChart.Visible && e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F12)
        {
            controlChart.FunctionKeyPressContrChart(e.KeyCode);
            return;
        }

        // Ctrl-Alt-R is supposed to show referral window, but it doesn't work on some computers.
        // so we're also going to use Ctrl-X to show the referral window.
        if (PatNumCur != 0 && e.Modifiers == (Keys.Alt | Keys.Control) && e.KeyCode == Keys.R || e.Modifiers == Keys.Control && e.KeyCode == Keys.X)
        {
            var frmReferralsPatient = new FrmReferralsPatient
            {
                PatNum = PatNumCur
            };

            frmReferralsPatient.ShowDialog();
        }

        if (controlImages is {Visible: true})
        {
            controlImages.ControlImages_KeyDown(e.KeyCode);
        }

        if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P)
        {
            toolButPatient_Click();
        }
    }

    private void ControlChart_ImageClick(object sender, EventArgsImageClick e)
    {
        controlImages?.LaunchFloaterFromChart(e.PatNum, e.DocNum, e.MountNum);
    }

    private void toolButPatient_Click()
    {
        var textClip = "";

        var prevPatNum = PatNumCur;
        try
        {
            textClip = System.Windows.Clipboard.GetText().Trim().ToLower();
        }
        catch
        {
            // ignored
        }

        if (Regex.IsMatch(textClip, @"^patnum:\d+$"))
        {
            var patNum = SIn.Long(textClip.Substring(7));
            if (patNum != prevPatNum)
            {
                if (TrySetPatient(patNum))
                {
                    return;
                }
            }
        }

        var frmPatientSelect = new FrmPatientSelect
        {
            CanAddPatients = true
        };

        frmPatientSelect.ShowDialog();

        if (frmPatientSelect.IsDialogOK)
        {
            TrySetPatient(frmPatientSelect.PatNumSelected);
        }
    }

    private bool TrySetPatient(long patNum)
    {
        var patient = Patients.GetPat(patNum);
        if (patient == null || patient.PatNum == 0)
        {
            //if not valid
            return false;
        }

        PatNumCur = patNum;
        if (!controlChart.Visible)
        {
            RefreshCurrentModule();
        }

        FillPatientButton(patient);
        return true;
    }

    private void LockODForMountAcquire(bool enabled)
    {
        var forms = Application.OpenForms.Cast<Form>().Where(f => !f.Modal && f.Name != nameof(FormImageFloat)).ToList();

        controlImages.Enabled = enabled;

        foreach (var form in forms)
        {
            form.Enabled = enabled;
        }
    }

    protected override string GetHelpOverride()
    {
        return moduleBar.SelectedModule switch
        {
            EnumModuleType.Appointments => nameof(ControlAppt),
            EnumModuleType.Family => nameof(ControlFamily),
            EnumModuleType.Account => nameof(ControlAccount),
            EnumModuleType.TreatPlan => nameof(ControlTreat),
            EnumModuleType.Chart => nameof(ControlChart),
            EnumModuleType.Imaging => nameof(ControlImages),
            EnumModuleType.Manage => nameof(ControlManage),
            _ => ""
        };
    }

    private bool ImagesModuleUsesOld2020()
    {
        return controlImagesOld != null;
    }

    private bool PrefsStartup()
    {
        try
        {
            Cache.Refresh(InvalidType.Prefs);
        }
        catch (Exception ex)
        { 
            ODMessageBox.Show(ex.Message);
            
            return false;
        }
        
        if (!FormRegistrationKey.ValidateKey(PrefC.GetString(PrefName.RegistrationKey)))
        {
            using var formRegistrationKey = new FormRegistrationKey();

            if (formRegistrationKey.ShowDialog() != DialogResult.OK)
            {
                Environment.Exit(ExitCode);
                return false;
            }

            Cache.Refresh(InvalidType.Prefs);
        }

        if (DateTime.Today < PrefC.GetDate(PrefName.TempFolderDateFirstCleaned).AddDays(7))
        {
            PrefC.GetTempFolderPath();
        }

        Lans.RefreshCache();

        return true;
    }

    private void RefreshLocalData(params InvalidType[] arrayITypes)
    {
        RefreshLocalData(true, arrayITypes);
    }

    private void RefreshLocalData(bool doRefreshServerCache, params InvalidType[] arrayITypes)
    {
        if (arrayITypes == null || arrayITypes.Length == 0)
        {
            return;
        }

        Cache.Refresh(doRefreshServerCache, arrayITypes);

        this.InvokeIfRequired(() => RefreshLocalDataPostCleanup(arrayITypes));
    }

    private void RefreshLocalDataPostCleanup(params InvalidType[] arrayITypes)
    {
        var isAll = arrayITypes.Contains(InvalidType.AllLocal);

        if (arrayITypes.Contains(InvalidType.Prefs) || isAll)
        {
            if (PrefC.GetBool(PrefName.EasyHidePublicHealth))
            {
                _menuItemSites.Available = false;
                _menuItemCounties.Available = false;
            }

            moduleBar.RefreshButtons();

            if (PrefC.GetBool(PrefName.EasyHideRepeatCharges))
            {
                _menuItemRepeatingCharges.Available = false;
            }

            if (!PrefC.HasOnlinePaymentEnabled(out _))
            {
                _menuItemOnlinePayments.Available = false;
                _menuItemPatPortalTransactions.Available = false;
            }

            _menuItemFeeSchedGroups.Available = PrefC.GetBool(PrefName.ShowFeeSchedGroups);
            var isLateChargeFeatureActive = PrefC.GetBool(PrefName.ShowFeatureLateCharges);
            if (isLateChargeFeatureActive)
            {
                _menuItemLateCharges.Available = true;
                _menuItemFinanceCharges.Available = false;
            }
            else
            {
                _menuItemLateCharges.Available = false;
                _menuItemFinanceCharges.Available = true;
            }

            if (NeedsRedraw("ChartModule"))
            {
                controlChart.InitializeLocalData();
            }

            LayoutControls();
        }
        
        if (arrayITypes.Contains(InvalidType.Programs) || isAll)
        {
            if (Programs.GetCur(ProgramName.PT).Enabled && Programs.IsEnabledByHq(ProgramName.PT, out _))
            {
                Bridges.PaperlessTechnology.InitializeFileWatcher();
            }
        }

        if (arrayITypes.Contains(InvalidType.Programs) || arrayITypes.Contains(InvalidType.Prefs) || isAll)
        {
            if (PrefC.GetBool(PrefName.EasyBasicModules))
            {
                moduleBar.SetVisible(EnumModuleType.TreatPlan, false);
                moduleBar.SetVisible(EnumModuleType.Imaging, false);
                moduleBar.SetVisible(EnumModuleType.Manage, false);
            }
            else
            {
                moduleBar.SetVisible(EnumModuleType.TreatPlan, true);
                moduleBar.SetVisible(EnumModuleType.Imaging, true);
                moduleBar.SetVisible(EnumModuleType.Manage, true);
            }

            if (HL7Defs.IsExistingHL7Enabled())
            {
                var hl7Def = HL7Defs.GetOneDeepEnabled();

                moduleBar.SetVisible(EnumModuleType.Appointments, hl7Def.ShowAppts);
                moduleBar.SetVisible(EnumModuleType.Account, hl7Def.ShowAccount);
            }
            else
            {
                moduleBar.SetVisible(EnumModuleType.Appointments, true);
                moduleBar.SetVisible(EnumModuleType.Account, true);
            }

            moduleBar.Invalidate();
        }

        if (arrayITypes.Contains(InvalidType.ToolButsAndMounts) || isAll)
        {
            controlAccount.LayoutToolBar();
            controlAppt.LayoutToolBar();

            if (controlChart.Visible)
            {
                controlChart.RefreshModuleScreen();
            }
            else
            {
                controlChart.LayoutToolBar();
            }

            if (ImagesModuleUsesOld2020())
            {
                controlImagesOld?.LayoutToolBar();
            }
            else
            {
                controlImages?.LayoutToolBars();
            }

            controlFamily.LayoutToolBar();

            LayoutToolBar();
        }

        if (arrayITypes.Contains(InvalidType.Views) || isAll)
        {
            controlAppt.FillViews();
        }

        controlTreat.InitializeLocalData();
        
        _dictionaryChartPrefsCache.Clear();
        _dictionaryTaskListPrefsCache.Clear();
        

        _dictionaryChartPrefsCache.Add(PrefName.UseInternationalToothNumbers.ToString(), PrefC.GetInt(PrefName.UseInternationalToothNumbers));
        _dictionaryChartPrefsCache.Add("GraphicsUseHardware", ComputerPrefs.LocalComputer.GraphicsUseHardware);
        _dictionaryChartPrefsCache.Add("PreferredPixelFormatNum", ComputerPrefs.LocalComputer.PreferredPixelFormatNum);
        _dictionaryChartPrefsCache.Add("GraphicsSimple", ComputerPrefs.LocalComputer.GraphicsSimple);
        _dictionaryChartPrefsCache.Add(PrefName.ShowFeatureEhr.ToString(), false);
        _dictionaryChartPrefsCache.Add("DirectXFormat", ComputerPrefs.LocalComputer.DirectXFormat);
        _dictionaryChartPrefsCache.Add(PrefName.OrthoShowInChart.ToString(), PrefC.GetBool(PrefName.OrthoShowInChart));

        _dictionaryTaskListPrefsCache.Add("TaskDock", ComputerPrefs.LocalComputer.TaskDock);
        _dictionaryTaskListPrefsCache.Add("TaskY", ComputerPrefs.LocalComputer.TaskY);
        _dictionaryTaskListPrefsCache.Add("TaskX", ComputerPrefs.LocalComputer.TaskX);
        _dictionaryTaskListPrefsCache.Add(PrefName.TaskListAlwaysShowsAtBottom.ToString(), PrefC.GetBool(PrefName.TaskListAlwaysShowsAtBottom));
        _dictionaryTaskListPrefsCache.Add(PrefName.TasksUseRepeating.ToString(), PrefC.GetBool(PrefName.TasksUseRepeating));
        _dictionaryTaskListPrefsCache.Add(PrefName.TasksNewTrackedByUser.ToString(), PrefC.GetBool(PrefName.TasksNewTrackedByUser));
        _dictionaryTaskListPrefsCache.Add(PrefName.TasksShowOpenTickets.ToString(), PrefC.GetBool(PrefName.TasksShowOpenTickets));
        _dictionaryTaskListPrefsCache.Add("TaskKeepListHidden", ComputerPrefs.LocalComputer.TaskKeepListHidden);
        
        _menuItemUserQuery.Available = Security.IsAuthorized(EnumPermType.UserQueryAdmin, true);
        _menuItemQueryFavorites.Available = Security.IsAuthorized(EnumPermType.UserQuery, true);
    }

    private bool NeedsRedraw(string section)
    {
        try
        {
            switch (section)
            {
                case "ChartModule":
                    if (_dictionaryChartPrefsCache.Count == 0
                        || PrefC.GetInt(PrefName.UseInternationalToothNumbers) != (int) _dictionaryChartPrefsCache["UseInternationalToothNumbers"]
                        || ComputerPrefs.LocalComputer.GraphicsUseHardware != (bool) _dictionaryChartPrefsCache["GraphicsUseHardware"]
                        || ComputerPrefs.LocalComputer.PreferredPixelFormatNum != (int) _dictionaryChartPrefsCache["PreferredPixelFormatNum"]
                        || ComputerPrefs.LocalComputer.GraphicsSimple != (DrawingMode) _dictionaryChartPrefsCache["GraphicsSimple"]
                        || ComputerPrefs.LocalComputer.DirectXFormat != (string) _dictionaryChartPrefsCache["DirectXFormat"]
                        || PrefC.GetBool(PrefName.OrthoShowInChart) != (bool) _dictionaryChartPrefsCache["OrthoShowInChart"])
                    {
                        return true;
                    }

                    break;
                case "TaskLists":
                    if (_dictionaryTaskListPrefsCache.Count == 0
                        || ComputerPrefs.LocalComputer.TaskDock != (int) _dictionaryTaskListPrefsCache["TaskDock"]
                        || ComputerPrefs.LocalComputer.TaskY != (int) _dictionaryTaskListPrefsCache["TaskY"]
                        || ComputerPrefs.LocalComputer.TaskX != (int) _dictionaryTaskListPrefsCache["TaskX"]
                        || PrefC.GetBool(PrefName.TaskListAlwaysShowsAtBottom) != (bool) _dictionaryTaskListPrefsCache["TaskListAlwaysShowsAtBottom"]
                        || PrefC.GetBool(PrefName.TasksUseRepeating) != (bool) _dictionaryTaskListPrefsCache["TasksUseRepeating"]
                        || PrefC.GetBool(PrefName.TasksNewTrackedByUser) != (bool) _dictionaryTaskListPrefsCache["TasksNewTrackedByUser"]
                        || PrefC.GetBool(PrefName.TasksShowOpenTickets) != (bool) _dictionaryTaskListPrefsCache["TasksShowOpenTickets"]
                        || ComputerPrefs.LocalComputer.TaskKeepListHidden != (bool) _dictionaryTaskListPrefsCache["TaskKeepListHidden"])
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    private void LayoutToolBar()
    {
        ToolBarMain.Buttons.Clear();
        ToolBarMain.ImageList = imageListMain;

        ToolBarMain.Buttons.Add(new ODToolBarButton("Select Patient", EnumIcons.PatSelect, "", "Patient")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuPatient
        });

        ToolBarMain.Buttons.Add(new ODToolBarButton("Commlog", EnumIcons.CommLog, "New Commlog Entry", "Commlog"));

        ToolBarMain.Buttons.Add(new ODToolBarButton("E-mail", EnumIcons.Email, "Send E-mail", "Email")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuEmail
        });

        ToolBarMain.Buttons.Add(new ODToolBarButton("WebMail", EnumIcons.WebMail, "Secure WebMail", "WebMail")
        {
            Enabled = true 
        });
        
        if (_toolBarButtonText == null)
        {
            _toolBarButtonText = new ODToolBarButton("Text", EnumIcons.Text, "Send Text Message", "Text")
            {
                Style = ODToolBarButtonStyle.DropDownButton,
                DropDownMenu = menuText,
                Enabled = Programs.IsEnabled(ProgramName.CallFire) || SmsPhones.IsIntegratedTextingEnabled()
            };
            
            if (SmsPhones.IsIntegratedTextingEnabled())
            {
                SetSmsNotificationText();
            }
        }

        ToolBarMain.Buttons.Add(_toolBarButtonText);

        ToolBarMain.Buttons.Add(new ODToolBarButton("Letter", -1, "Quick Letter", "Letter")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuLetter
        });
        
        ToolBarMain.Buttons.Add(new ODToolBarButton("Forms", -1, "", "Form"));
        
        _toolBarButtonTask ??= new ODToolBarButton("Tasks", 3, "Open Tasks", "Tasklist")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuTask
        };

        ToolBarMain.Buttons.Add(_toolBarButtonTask);

        ToolBarMain.Buttons.Add(new ODToolBarButton("Label", 4, "Print Label", "Label")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuLabel
        });
        
        ToolBarMain.Buttons.Add(new ODToolBarButton("Popups", -1, "Edit popups for this patient", "Popups"));
        
        ProgramL.LoadToolBar(ToolBarMain, EnumToolBar.MainToolbar);
        
        ToolBarMain.Invalidate();
        
        UpdateToolbarButtons();
    }

    private void UpdateToolbarButtons()
    {
        if (PatNumCur == 0)
        {
            ToolBarMain.Buttons["Email"].Enabled = false;
            ToolBarMain.Buttons["WebMail"].Enabled = false;
            ToolBarMain.Buttons["Commlog"].Enabled = false;
            ToolBarMain.Buttons["Letter"].Enabled = false;
            ToolBarMain.Buttons["Form"].Enabled = false;
            ToolBarMain.Buttons["Tasklist"].Enabled = true;
            ToolBarMain.Buttons["Label"].Enabled = false;

            ToolBarMain.Buttons["Popups"].Enabled = false;
        }
        else
        {
            ToolBarMain.Buttons["Commlog"].Enabled = true;
            ToolBarMain.Buttons["Email"].Enabled = true;
            if (_toolBarButtonText is not null)
            {
                _toolBarButtonText.Enabled = Programs.IsEnabled(ProgramName.CallFire) || SmsPhones.IsIntegratedTextingEnabled();
            }

            ToolBarMain.Buttons["WebMail"].Enabled = true;
            ToolBarMain.Buttons["Letter"].Enabled = true;
            ToolBarMain.Buttons["Form"].Enabled = true;
            ToolBarMain.Buttons["Tasklist"].Enabled = true;
            ToolBarMain.Buttons["Label"].Enabled = true;

            ToolBarMain.Buttons["Popups"].Enabled = true;
        }

        ToolBarMain.Invalidate();
    }

    private void LayoutControls()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            return;
        }

        if (Width < 200)
        {
            Width = 200;
        }

        if (moduleBar == null)
        {
            return;
        }
        
        var pointPosition = new Point(moduleBar.Width, ToolBarMain.Bottom);
        var width = ClientSize.Width - pointPosition.X;
        var height = ClientSize.Height - pointPosition.Y;
        
        splitContainer.Bounds = new Rectangle(pointPosition.X, pointPosition.Y, width, height);
    }

    private void toolBarMain_ButtonClick(object sender, ODToolBarButtonClickEventArgs e)
    {
        if (e.Button.Tag is string str)
        {
            switch (str)
            {
                case "Patient":
                    toolButPatient_Click();
                    break;
                case "Commlog":
                    toolButCommlog_Click();
                    break;
                case "Email":
                    toolButEmail_Click();
                    break;
                case "WebMail":
                    ToolButWebMail_Click();
                    break;
                case "Text":
                    if (!Security.IsAuthorized(EnumPermType.TextMessageSend))
                    {
                        return;
                    }

                    toolButTxtMsg_Click(PatNumCur);
                    break;
                case "Letter":
                    toolButLetter_Click();
                    break;
                case "Form":
                    toolButForm_Click();
                    break;
                case "Tasklist":
                    ToolButTasks_Click();
                    break;
                case "Label":
                    toolButLabel_Click();
                    break;
                case "Popups":
                    toolButPopups_Click();
                    break;
            }
        }
        else if (e.Button.Tag.GetType() == typeof(Program))
        {
            WpfControls.ProgramL.Execute(((Program) e.Button.Tag).ProgramNum, Patients.GetPat(PatNumCur));
        }
    }

    private void FillPatientButton(Patient patient)
    {
        patient ??= new Patient();

        if (patient.PatStatus == PatientStatus.Archived && !Security.IsAuthorized(EnumPermType.ArchivedPatientSelect))
        {
            PatNumCur = 0;
            patient = new Patient();
        }

        Text = PatientL.GetMainTitle(patient, Clinics.ClinicNum);

        var patChanged = PatientL.AddPatsToMenu(patient);
        if (patChanged)
        {
            if (AutomationL.Trigger(EnumAutomationTrigger.PatientOpen, null, patient.PatNum))
            {
                if (controlAppt.Visible)
                {
                    controlAppt.MouseUpForced();
                }
            }
        }

        if (ToolBarMain.Buttons == null || ToolBarMain.Buttons.Count < 2)
        {
            return;
        }

        UpdateToolbarButtons();

        ToolBarMain.Invalidate();

        _popupEvents ??= [];

        if (!patChanged)
        {
            return;
        }

        if (controlChart.Visible)
        {
            TryNonPatientPopup();
        }

        if (!ImagesModuleUsesOld2020())
        {
            controlImages.CloseFloaters();
        }

        for (var i = _popupEvents.Count - 1; i >= 0; i--)
        {
            if (_popupEvents[i].DateTimeDisableUntil < DateTime.Now)
            {
                _popupEvents.RemoveAt(i);
            }
        }
        
        var popups = Popups.GetForPatient(patient);
        foreach (var popup in popups)
        {
            var popupIsDisabled = false;
            
            foreach (var popupEvent in _popupEvents)
            {
                if (popup.PopupNum == popupEvent.PopupNum)
                {
                    popupIsDisabled = true;
                    break;
                }
            }

            if (popupIsDisabled)
            {
                continue;
            }
            
            if (controlAppt.Visible)
            {
                controlAppt.MouseUpForced();
            }

            using var formPopupDisplay = new FormPopupDisplay();
            
            formPopupDisplay.PopupCur = popup;
            formPopupDisplay.ShowDialog();

            if (formPopupDisplay.MinutesDisabled <= 0)
            {
                continue;
            }
            
            _popupEvents.Add(new PopupEvent
            {
                PopupNum = popup.PopupNum,
                DateTimeDisableUntil = DateTime.Now + TimeSpan.FromMinutes(formPopupDisplay.MinutesDisabled),
                DateTimeLastViewed = DateTime.Now
            });
            _popupEvents.Sort();
        }
    }

    private void Contr_PatientSelected(PatientSelectedEventArgs e)
    {
        PatNumCur = e.Patient_.PatNum;

        if (e.IsRefreshCurModule)
        {
            RefreshCurrentModule(e.HasForcedRefresh, e.IsApptRefreshDataPat);
        }
        
        FillPatientButton(e.Patient_);
    }

    private void TryNonPatientPopup()
    {
        if (PatNumCur != 0 && _patNumPrevious != PatNumCur)
        {
            _datePopupDelay = DateTime.Now;
            _patNumPrevious = PatNumCur;
        }

        if (!PrefC.GetBool(PrefName.ChartNonPatientWarn))
        {
            return;
        }

        var patient = Patients.GetPat(PatNumCur);
        if (patient is null || patient.PatStatus.ToString() != "NonPatient" || _datePopupDelay > DateTime.Now)
        {
            return;
        }
        
        MsgBox.Show(this, "A patient with the status NonPatient is currently selected.");

        _datePopupDelay = DateTime.Now.AddMinutes(5);
    }

    private void menuPatient_Click(object sender, EventArgs e)
    {
        var family = Patients.GetFamily(PatNumCur);

        PatNumCur = PatientL.ButtonSelect(menuPatient, sender, family);

        var patient = Patients.GetPat(PatNumCur);

        RefreshCurrentModule();
        FillPatientButton(patient);
    }

    private void menuPatient_Popup(object sender, EventArgs e)
    {
        Family family = null;
        if (PatNumCur != 0)
        {
            family = Patients.GetFamily(PatNumCur);
        }

        PatientL.AddFamilyToMenu(menuPatient, menuPatient_Click, PatNumCur, family);
    }

    private void toolButEmail_Click()
    {
        if (PatNumCur == 0)
        {
            MsgBox.Show(this, "Please select a patient to send an email.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.EmailSend))
        {
            return;
        }

        var patient = Patients.GetPat(PatNumCur);

        var emailAddress = EmailAddresses.GetNewEmailDefault(Security.CurUser.UserNum, patient.ClinicNum);

        emailAddress = EmailAddresses.OverrideSenderAddressClinical(emailAddress, patient.ClinicNum);

        var emailMessage = new EmailMessage
        {
            PatNum = PatNumCur,
            ToAddress = patient.Email,
            FromAddress = emailAddress.GetFrom(),
            MsgType = EmailMessageSource.Manual
        };

        using var formEmailMessageEdit = new FormEmailMessageEdit(emailMessage, emailAddress);

        formEmailMessageEdit.IsNew = true;

        if (formEmailMessageEdit.ShowDialog() == DialogResult.OK)
        {
            RefreshCurrentModule();
        }
    }

    private void menuEmail_Popup(object sender, EventArgs e)
    {
        menuEmail.MenuItems.Clear();

        var menuItem = new MenuItem("Referrals:");

        menuItem.Tag = null;

        menuEmail.MenuItems.Add(menuItem);

        var refAttachs = RefAttaches.Refresh(PatNumCur);
        var referralDescript = DisplayFields.GetForCategory(DisplayFieldCategory.PatientInformation).FirstOrDefault(x => x.InternalName == "Referrals")?.Description;
        if (string.IsNullOrWhiteSpace(referralDescript))
        {
            referralDescript = "Referral";
        }

        foreach (var t in refAttachs)
        {
            if (!Referrals.TryGetReferral(t.ReferralNum, out var referral))
            {
                continue;
            }

            var str = t.RefType switch
            {
                ReferralType.RefFrom => "From",
                ReferralType.RefTo => "To",
                _ => referralDescript
            };

            str += " " + Referrals.GetNameFL(referral.ReferralNum) + " <";
            if (referral.EMail == "")
            {
                str += "no email";
            }
            else
            {
                str += referral.EMail;
            }

            str += ">";
            
            menuItem = new MenuItem(str, menuEmail_Click);
            menuItem.Tag = referral;
            
            menuEmail.MenuItems.Add(menuItem);
        }
    }

    private static void ToolButWebMail_Click()
    {
        if (!Security.IsAuthorized(EnumPermType.WebMailSend))
        {
            return;
        }

        using var formWebMailMessageEdit = new FormWebMailMessageEdit(PatNumCur);

        formWebMailMessageEdit.ShowDialog();
    }

    private void menuEmail_Click(object sender, EventArgs e)
    {
        if (((MenuItem) sender).Tag is not Referral referral)
        {
            return;
        }

        if (referral.EMail == "")
        {
            return;
        }

        var emailMessage = new EmailMessage
        {
            PatNum = PatNumCur
        };

        var patient = Patients.GetPat(PatNumCur);

        emailMessage.ToAddress = referral.EMail;

        var emailAddress = EmailAddresses.GetByClinic(patient.ClinicNum);

        emailAddress = EmailAddresses.OverrideSenderAddressClinical(emailAddress, patient.ClinicNum);

        emailMessage.FromAddress = emailAddress.GetFrom();
        emailMessage.Subject = "RE: " + patient.GetNameFL();
        emailMessage.MsgType = EmailMessageSource.Manual;

        using var formEmailMessageEdit = new FormEmailMessageEdit(emailMessage, emailAddress);

        formEmailMessageEdit.IsNew = true;

        if (formEmailMessageEdit.ShowDialog() == DialogResult.OK)
        {
            RefreshCurrentModule();
        }
    }

    private void toolButCommlog_Click()
    {
        var frmCommItem = new FrmCommItem(GetNewCommlog())
        {
            DoOmitDefaults = PrefC.GetBool(PrefName.EnterpriseCommlogOmitDefaults)
        };

        frmCommItem.ShowDialog();

        if (frmCommItem.IsDialogOK)
        {
            RefreshCurrentModule();
        }
    }

    private void menuItemCommlogPersistent_Click(object sender, EventArgs e)
    {
        var listForms = Application.OpenForms.Cast<Form>().Where(x => x.Name == "FormCommItem").ToList();
        if (listForms.Count == 0)
        {
            var frmCommItem = new FrmCommItem(GetNewCommlog())
            {
                IsPersistent = true
            };

            frmCommItem.Show();
            return;
        }

        var form = listForms[0];
        var formFrame = (FormFrame) form;

        if (formFrame.WindowState == FormWindowState.Minimized)
        {
            formFrame.WindowState = FormWindowState.Normal;
        }

        formFrame.BringToFront();
    }

    private static Commlog GetNewCommlog()
    {
        return new Commlog
        {
            PatNum = PatNumCur,
            CommDateTime = DateTime.Now,
            CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.MISC),
            Mode_ = CommItemMode.Phone,
            SentOrReceived = CommSentOrReceived.Received,
            UserNum = Security.CurUser.UserNum,
            IsNew = true
        };
    }

    private void toolButLetter_Click()
    {
        var frmSheetPicker = new FrmSheetPicker
        {
            SheetType = SheetTypeEnum.PatientLetter
        };

        frmSheetPicker.ShowDialog();

        if (!frmSheetPicker.IsDialogOK)
        {
            return;
        }

        var sheetDef = frmSheetPicker.ListSheetDefsSelected[0];
        var sheet = SheetUtil.CreateSheet(sheetDef, PatNumCur);

        SheetParameter.SetParameter(sheet, "PatNum", PatNumCur);

        if (SheetDefs.ContainsGrids(sheetDef, "ProcsWithFee", "ProcsNoFee"))
        {
            using var formSheetProcSelect = new FormSheetProcSelect();
            formSheetProcSelect.PatNum = PatNumCur;
            formSheetProcSelect.ShowDialog();
            if (formSheetProcSelect.DialogResult == DialogResult.OK)
            {
                SheetParameter.SetParameter(sheet, "ListProcNums", formSheetProcSelect.ListProcNumsSelected);
            }
        }

        SheetUtilL.SetApptProcParamsForSheet(sheet, sheetDef, PatNumCur);
        SheetFiller.FillFields(sheet);
        SheetUtil.CalculateHeights(sheet);
        FormSheetFillEdit.ShowForm(sheet, FormSheetFillEdit_FormClosing);
    }

    private void menuLetter_Popup(object sender, EventArgs e)
    {
        menuLetter.MenuItems.Clear();

        var menuItem = new MenuItem("Merge", menuLetter_Click);
        menuItem.Tag = "Merge";
        menuLetter.MenuItems.Add(menuItem);

        menuLetter.MenuItems.Add("-");
        //Referrals---------------------------------------------------------------------------------------
        menuItem = new MenuItem("Referrals:");
        menuItem.Tag = null;
        menuLetter.MenuItems.Add(menuItem);
        var referralDescript = DisplayFields.GetForCategory(DisplayFieldCategory.PatientInformation).FirstOrDefault(x => x.InternalName == "Referrals")?.Description;
        if (string.IsNullOrWhiteSpace(referralDescript))
        {
            //either not displaying the Referral field or no description entered, default to 'Referral'
            referralDescript = "Referral";
        }

        var refAttaches = RefAttaches.Refresh(PatNumCur);
        foreach (var refAttach in refAttaches)
        {
            if (!Referrals.TryGetReferral(refAttach.ReferralNum, out var referral))
            {
                continue;
            }

            var str = refAttach.RefType switch
            {
                ReferralType.RefFrom => "From",
                ReferralType.RefTo => "To",
                _ => referralDescript
            };

            str += " " + Referrals.GetNameFL(referral.ReferralNum);

            menuItem = new MenuItem(str, menuLetter_Click);
            menuItem.Tag = referral;

            menuLetter.MenuItems.Add(menuItem);
        }
    }

    private void menuLetter_Click(object sender, EventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.Tag == null)
        {
            return;
        }

        var patient = Patients.GetPat(PatNumCur);
        if (menuItem.Tag is "Merge")
        {
            using var formLetterMerges = new FormLetterMerges(patient);

            formLetterMerges.ShowDialog();
        }

        if (menuItem.Tag is not Referral referral)
        {
            return;
        }

        var frmSheetPicker = new FrmSheetPicker
        {
            SheetType = SheetTypeEnum.ReferralLetter
        };

        frmSheetPicker.ShowDialog();

        if (!frmSheetPicker.IsDialogOK)
        {
            return;
        }

        var sheetDef = frmSheetPicker.ListSheetDefsSelected[0];
        var sheet = SheetUtil.CreateSheet(sheetDef, PatNumCur);

        SheetParameter.SetParameter(sheet, "PatNum", PatNumCur);
        SheetParameter.SetParameter(sheet, "ReferralNum", referral.ReferralNum);

        if (sheetDef.SheetFieldDefs.Any(x => x.FieldType == SheetFieldType.Grid && x.FieldName == "ReferralLetterProceduresCompleted" || x.FieldType == SheetFieldType.Special && x.FieldName == "toothChart"))
        {
            var procedures = Procedures.GetCompletedForDateRange(sheet.DateTimeSheet, sheet.DateTimeSheet, listPatNums: [PatNumCur], includeNote: true, includeGroupNote: true);
            if (sheetDef.SheetFieldDefs.Any(x => x.FieldType == SheetFieldType.Grid && x.FieldName == "ReferralLetterProceduresCompleted"))
            {
                SheetParameter.SetParameter(sheet, "CompletedProcs", procedures);
            }

            if (sheetDef.SheetFieldDefs.Any(x => x.FieldType == SheetFieldType.Special && x.FieldName == "toothChart"))
            {
                SheetParameter.SetParameter(sheet, "toothChartImg", SheetPrinting.GetToothChartHelper(PatNumCur, false, listProceduresFilteredOverride: procedures));
            }
        }

        if (SheetDefs.ContainsGrids(sheetDef, "ProcsWithFee", "ProcsNoFee"))
        {
            using var formSheetProcSelect = new FormSheetProcSelect();

            formSheetProcSelect.PatNum = PatNumCur;

            if (formSheetProcSelect.ShowDialog() == DialogResult.OK)
            {
                SheetParameter.SetParameter(sheet, "ListProcNums", formSheetProcSelect.ListProcNumsSelected);
            }
        }

        SheetUtilL.SetApptProcParamsForSheet(sheet, sheetDef, PatNumCur);
        SheetFiller.FillFields(sheet);
        SheetUtil.CalculateHeights(sheet);
        FormSheetFillEdit.ShowForm(sheet, FormSheetFillEdit_FormClosing);
    }

    private void FormSheetFillEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (((FormSheetFillEdit) sender).DialogResult == DialogResult.OK || ((FormSheetFillEdit) sender).DidChangeSheet)
        {
            RefreshCurrentModule();
        }
    }

    private void toolButForm_Click()
    {
        using var formPatientForms = new FormPatientForms();

        formPatientForms.PatNum = PatNumCur;
        formPatientForms.ShowDialog();

        var patient = Patients.GetPat(PatNumCur);

        RefreshCurrentModule(docNum: formPatientForms.DocNum);
        FillPatientButton(patient);
    }

    private static void ToolButTasks_Click()
    {
        using var formTaskListSelect = new FormTaskListSelect(TaskObjectType.Patient);

        formTaskListSelect.Location = new Point(50, 50);
        formTaskListSelect.Text = "Add Task - " + formTaskListSelect.Text;

        if (formTaskListSelect.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var task = new Task
        {
            TaskListNum = -1
        };

        Tasks.Insert(task);

        var taskOld = task.Copy();

        task.KeyNum = PatNumCur;
        task.ObjectType = TaskObjectType.Patient;
        task.TaskListNum = formTaskListSelect.ListSelectedLists[0];
        task.UserNum = Security.CurUser.UserNum;

        var formTaskEdit = new FormTaskEdit(task, taskOld);

        formTaskEdit.IsNew = true;
        formTaskEdit.Show();
    }

    private void menuTask_Popup(object sender, EventArgs e)
    {
        menuItemTaskNewForUser.Text = "for " + Security.CurUser.UserName;
        menuItemTaskReminders.Text = "Reminders";

        var reminderTaskNewCount = GetNewReminderTaskCount();
        if (reminderTaskNewCount > 0)
        {
            menuItemTaskReminders.Text += " (" + reminderTaskNewCount + ")";
        }

        menuItemTaskReminders.Visible = !PrefC.GetBool(PrefName.TasksUseRepeating);

        var otherTaskCount = _listTaskNumsNormal?.Count ?? 0;
        if (otherTaskCount > 0)
        {
            menuItemTaskNewForUser.Text += " (" + otherTaskCount + ")";
        }
    }

    private void RefreshTasksNotification()
    {
        if (_toolBarButtonTask == null)
        {
            return;
        }


        var otherTaskCount = _listTaskNumsNormal?.Count ?? 0;
        var totalTaskCount = GetNewReminderTaskCount() + otherTaskCount;
        var notificationText = "";
        if (totalTaskCount > 0)
        {
            notificationText = Math.Min(totalTaskCount, 99).ToString();
        }

        if (notificationText != _toolBarButtonTask.NotificationText)
        {
            _toolBarButtonTask.NotificationText = notificationText;
            ToolBarMain.Invalidate(_toolBarButtonTask.Bounds); //Cause the notification text on the Task button to update as soon as possible.
        }
    }

    private int GetNewReminderTaskCount()
    {
        if (_tasksReminders is null)
        {
            return 0;
        }

        return PrefC.GetBool(PrefName.TasksNewTrackedByUser) 
            ? _tasksReminders.FindAll(x => x.IsUnread && x.DateTimeEntry <= DateTime.Now).Count 
            : _tasksReminders.FindAll(x => x.TaskStatus == TaskStatusEnum.New && x.DateTimeEntry <= DateTime.Now).Count;
    }

    private void menuItemTaskNewForUser_Click(object sender, EventArgs e)
    {
        controlManage.LaunchTaskWindow(UserControlTasksTab.ForUser);
    }

    private void menuItemTaskReminders_Click(object sender, EventArgs e)
    {
        controlManage.LaunchTaskWindow(UserControlTasksTab.Reminders);
    }

    private delegate void ToolBarMainClick(long patNum);

    private void toolButLabel_Click()
    {
        ToolBarMainClick toolClick = LabelSingle.PrintPat;

        BeginInvoke(toolClick, PatNumCur);
    }

    private void menuLabel_Popup(object sender, EventArgs e)
    {
        menuLabel.MenuItems.Clear();
        MenuItem menuItem;

        var labelSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.LabelPatient);
        if (labelSheetDefs.Count == 0)
        {
            menuItem = new MenuItem("LName, FName, Address", menuLabel_Click);
            menuItem.Tag = "PatientLFAddress";
            menuLabel.MenuItems.Add(menuItem);
            menuItem = new MenuItem("Name, ChartNumber", menuLabel_Click);
            menuItem.Tag = "PatientLFChartNumber";
            menuLabel.MenuItems.Add(menuItem);
            menuItem = new MenuItem("Name, PatNum", menuLabel_Click);
            menuItem.Tag = "PatientLFPatNum";
            menuLabel.MenuItems.Add(menuItem);
            menuItem = new MenuItem("Radiograph", menuLabel_Click);
            menuItem.Tag = "PatRadiograph";
            menuLabel.MenuItems.Add(menuItem);
        }
        else
        {
            foreach (var sheetDef in labelSheetDefs)
            {
                menuItem = new MenuItem(sheetDef.Description, menuLabel_Click);
                menuItem.Tag = sheetDef;

                menuLabel.MenuItems.Add(menuItem);
            }
        }

        menuLabel.MenuItems.Add("-");

        // Carriers
        var family = Patients.GetFamily(PatNumCur);

        if (family.ListPats is {Length: > 0})
        {
            var patPlans = PatPlans.Refresh(PatNumCur);
            var insSubs = InsSubs.RefreshForFam(family);
            var insPlans = InsPlans.RefreshForSubList(insSubs);

            foreach (var patPlan in patPlans)
            {
                var insSub = InsSubs.GetSub(patPlan.InsSubNum, insSubs);
                var insPlan = InsPlans.GetPlan(insSub.PlanNum, insPlans);
                var carrier = Carriers.GetCarrier(insPlan.CarrierNum);
                menuItem = new MenuItem(carrier.CarrierName, menuLabel_Click);
                menuItem.Tag = carrier;
                menuLabel.MenuItems.Add(menuItem);
            }

            menuLabel.MenuItems.Add("-");
        }

        // Referrals
        menuItem = new MenuItem("Referrals:");
        menuItem.Tag = null;

        menuLabel.MenuItems.Add(menuItem);

        var referralDescript = DisplayFields.GetForCategory(DisplayFieldCategory.PatientInformation).FirstOrDefault(x => x.InternalName == "Referrals")?.Description;
        if (string.IsNullOrWhiteSpace(referralDescript))
        {
            referralDescript = "Referral";
        }

        var refAttaches = RefAttaches.Refresh(PatNumCur);
        foreach (var refAttach in refAttaches)
        {
            if (!Referrals.TryGetReferral(refAttach.ReferralNum, out var referral))
            {
                continue;
            }

            var text = refAttach.RefType switch
            {
                ReferralType.RefFrom => "From",
                ReferralType.RefTo => "To",
                _ => referralDescript
            };

            text += " " + Referrals.GetNameFL(referral.ReferralNum);

            menuItem = new MenuItem(text, menuLabel_Click);
            menuItem.Tag = referral;

            menuLabel.MenuItems.Add(menuItem);
        }
    }

    private void menuLabel_Click(object sender, EventArgs e)
    {
        if (((MenuItem) sender).Tag == null)
        {
            return;
        }

        if (((MenuItem) sender).Tag is string s)
        {
            switch (s)
            {
                case "PatientLFAddress":
                    LabelSingle.PrintPatientLFAddress(PatNumCur);
                    break;
                case "PatientLFChartNumber":
                    LabelSingle.PrintPatientLFChartNumber(PatNumCur);
                    break;
                case "PatientLFPatNum":
                    LabelSingle.PrintPatientLFPatNum(PatNumCur);
                    break;
                case "PatRadiograph":
                    LabelSingle.PrintPatRadiograph(PatNumCur);
                    break;
            }
        }
        else if (((MenuItem) sender).Tag.GetType() == typeof(SheetDef))
        {
            LabelSingle.PrintCustomPatient(PatNumCur, (SheetDef) ((MenuItem) sender).Tag);
        }
        else if (((MenuItem) sender).Tag.GetType() == typeof(Carrier))
        {
            var carrier = (Carrier) ((MenuItem) sender).Tag;
            LabelSingle.PrintCarrier(carrier.CarrierNum);
        }
        else if (((MenuItem) sender).Tag.GetType() == typeof(Referral))
        {
            var referral = (Referral) ((MenuItem) sender).Tag;
            LabelSingle.PrintReferral(referral.ReferralNum);
        }
    }

    private void toolButPopups_Click()
    {
        using var formPopupsForFam = new FormPopupsForFam(_popupEvents);

        formPopupsForFam.PatientCur = Patients.GetPat(PatNumCur);
        formPopupsForFam.ShowDialog();
    }

    private bool toolButTxtMsg_Click(long patNum, string startingText = "")
    {
        if (patNum == 0)
        {
            using var formTxtMsgEdit = new FormTxtMsgEdit();

            formTxtMsgEdit.Message = startingText;
            formTxtMsgEdit.PatNum = 0;

            if (formTxtMsgEdit.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            RefreshCurrentModule();

            return true;
        }

        var patient = Patients.GetPat(patNum);
        var updateTextYN = false;

        if (patient.TxtMsgOk == YN.No)
        {
            if (MsgBox.Show(this, MsgBoxButtons.YesNo,
                    "This patient is marked to not receive text messages. " +
                    "Would you like to mark this patient as okay to receive text messages?"))
            {
                updateTextYN = true;
            }
            else
            {
                return false;
            }
        }

        if (patient.TxtMsgOk == YN.Unknown && PrefC.GetBool(PrefName.TextMsgOkStatusTreatAsNo))
        {
            if (MsgBox.Show(this, MsgBoxButtons.YesNo,
                    "This patient might not want to receive text messages. " +
                    "Would you like to mark this patient as okay to receive text messages?"))
            {
                updateTextYN = true;
            }
            else
            {
                return false;
            }
        }

        if (updateTextYN)
        {
            var patientOld = patient.Copy();

            patient.TxtMsgOk = YN.Yes;
            Patients.Update(patient, patientOld);
            Patients.InsertAddressChangeSecurityLogEntry(patientOld, patient);
        }

        if (!Security.IsAuthorized(EnumPermType.TextMessageSend))
        {
            return false;
        }

        using var formTxtMsgEdit2 = new FormTxtMsgEdit();

        formTxtMsgEdit2.Message = startingText;
        formTxtMsgEdit2.PatNum = patNum;
        formTxtMsgEdit2.WirelessPhone = patient.WirelessPhone;
        formTxtMsgEdit2.YNTxtMsgOk = patient.TxtMsgOk;

        if (formTxtMsgEdit2.ShowDialog() != DialogResult.OK)
        {
            return false;
        }

        RefreshCurrentModule();
        return true;
    }

    private void menuItemTextMessagesReceived_Click(object sender, EventArgs e)
    {
        ShowFormTextMessagingModeless(false, true);
    }

    private void menuItemTextMessagesSent_Click(object sender, EventArgs e)
    {
        ShowFormTextMessagingModeless(true, false);
    }

    private void menuItemTextMessagesAll_Click(object sender, EventArgs e)
    {
        ShowFormTextMessagingModeless(true, true);
    }

    private void ShowFormTextMessagingModeless(bool isSent, bool isReceived)
    {
        if (!Security.IsAuthorized(EnumPermType.TextMessageView))
        {
            return;
        }

        if (_formSmsTextMessaging == null || _formSmsTextMessaging.IsDisposed)
        {
            _formSmsTextMessaging = new FormSmsTextMessaging(isSent, isReceived, x => { SetSmsNotificationText(increment: x); });
            _formSmsTextMessaging.FormClosed += (_, _) => { _formSmsTextMessaging = null; };
        }

        _formSmsTextMessaging.Show();
        _formSmsTextMessaging.BringToFront();
    }

    private void SetSmsNotificationText(Signalod smsCountSignals = null, bool doUseSignalInterval = true, int increment = 0)
    {
        if (_toolBarButtonText is null)
        {
            return;
        }

        try
        {
            if (!_toolBarButtonText.Enabled)
            {
                return;
            }

            List<SmsFromMobiles.SmsNotification> smsNotifications = null;
            if (smsCountSignals is null)
            {
                var timeSignalStart = doUseSignalInterval ? Signalods.DateTRegularPrioritySignalLastRefreshed : DateTime.MinValue;

                smsCountSignals = Signalods
                    .RefreshTimed(timeSignalStart, [InvalidType.SmsTextMsgReceivedUnreadCount])
                    .OrderByDescending(x => x.SigDateTime)
                    .FirstOrDefault();

                if (smsCountSignals == null && timeSignalStart == DateTime.MinValue)
                {
                    smsNotifications = Signalods.UpsertSmsNotification();
                }
            }

            if (smsCountSignals is not null)
            {
                smsNotifications = SmsFromMobiles.SmsNotification.GetListFromJson(smsCountSignals.MsgValue);
                if (smsNotifications is null)
                {
                    return;
                }
            }

            int smsUnreadCount;
            if (smsNotifications is null)
            {
                smsUnreadCount = SIn.Int(_toolBarButtonText.NotificationText) + increment;
            }
            else if (Clinics.ClinicNum == 0)
            {
                smsUnreadCount = smsNotifications.Sum(x => x.Count);
            }
            else
            {
                smsUnreadCount = smsNotifications.Where(x => x.ClinicNum == Clinics.ClinicNum).Sum(x => x.Count);
            }

            var smsNotificationText = smsUnreadCount switch
            {
                > 99 => "99",
                > 0 => smsUnreadCount.ToString(),
                _ => ""
            };

            if (_toolBarButtonText.NotificationText == smsNotificationText)
            {
                return;
            }

            _toolBarButtonText.NotificationText = smsNotificationText;

            var lparen = menuItemTextMessagesReceived.Text.IndexOf("(", StringComparison.Ordinal);
            if (lparen != -1)
            {
                menuItemTextMessagesReceived.Text = menuItemTextMessagesReceived.Text.Substring(0, lparen - 1);
            }

            if (smsNotificationText != "")
            {
                menuItemTextMessagesReceived.Text += " (" + smsNotificationText + ")";
            }
        }
        finally
        {
            ToolBarMain.Invalidate(_toolBarButtonText.Bounds);
        }
    }

    private void RefreshMenuClinics()
    {
        _menuItemClinicsMain.DropDown.Items.Clear();

        var clinicDtos = Clinics.GetForUserod(Security.CurUser);
        if (clinicDtos.Count < 30)
        {
            MenuItemOD menuItem;

            if (!Security.CurUser.ClinicIsRestricted)
            {
                menuItem = new MenuItemOD("Headquarters", menuClinic_Click);
                menuItem.Tag = new ClinicDto();

                if (Clinics.ClinicNum == 0)
                {
                    menuItem.Checked = true;
                }

                _menuItemClinicsMain.Add(menuItem);
                _menuItemClinicsMain.AddSeparator();
            }

            foreach (var clinicDto in clinicDtos)
            {
                menuItem = new MenuItemOD(clinicDto.Abbr, menuClinic_Click);
                menuItem.Tag = clinicDto;

                if (Clinics.ClinicNum == clinicDto.Id)
                {
                    menuItem.Checked = true;
                }

                _menuItemClinicsMain.Add(menuItem);
            }
        }
        else
        {
            _menuItemClinicsMain.Click -= menuClick_OpenPickList;
            _menuItemClinicsMain.Click += menuClick_OpenPickList;
        }

        RefreshLocalData(InvalidType.Views, InvalidType.ToolButsAndMounts);
        if (!controlAppt.Visible)
        {
            RefreshCurrentModule();
        }

        moduleBar.RefreshButtons();

        CheckAlerts();
    }

    private void menuClick_OpenPickList(object sender, EventArgs e)
    {
        using var formClinics = new FormClinics();

        formClinics.IsSelectionMode = true;

        if (formClinics.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (formClinics.SelectedClinicId == 0)
        {
            RefreshCurrentClinic(new ClinicDto());

            return;
        }

        var clinic = Clinics.GetFirstOrDefault(x => x.Id == formClinics.SelectedClinicId);
        if (clinic != null)
        {
            RefreshCurrentClinic(clinic);
        }

        CheckAlerts();
    }

    private void menuClinic_Click(object sender, EventArgs e)
    {
        if (sender.GetType() != typeof(MenuItemOD) && ((MenuItemOD) sender).Tag != null)
        {
            return;
        }

        var clinic = (ClinicDto) ((MenuItemOD) sender).Tag;

        RefreshCurrentClinic(clinic);
    }

    private void RefreshCurrentClinic(ClinicDto clinic)
    {
        var isChangingClinic = Clinics.ClinicNum != clinic.Id;

        Clinics.SetClinicNum(clinic.Id);

        Text = PatientL.GetMainTitle(Patients.GetPat(PatNumCur), Clinics.ClinicNum);

        SetSmsNotificationText(doUseSignalInterval: !isChangingClinic);

        if (PrefC.GetBool(PrefName.AppointmentClinicTimeReset))
        {
            controlAppt.ModuleSelected(DateTime.Today);
        }

        RefreshMenuClinics();

        if (isChangingClinic)
        {
            _listTaskNumsNormal = null;
            _tasksReminders = null;

            UserControlTasks.ResetGlobalTaskFilterTypesToDefaultAllInstances();
            UserControlTasks.RefreshTasksForAllInstances(null);

            RefreshMenuReports();

            LayoutToolBar();

            FillPatientButton(Patients.GetPat(PatNumCur));
        }
    }

    private void DataValid_BecameInvalid(ValidEventArgs e)
    {
        ODEvent.Fire(ODEventType.Cache, "Refreshing Caches: ");

        if (e.OnlyLocal)
        {
            ODEvent.Fire(ODEventType.Cache, "Refreshing Caches: PrefsStartup");
            if (!PrefsStartup())
            {
                return;
            }

            ODEvent.Fire(ODEventType.Cache, "Refreshing Caches: AllLocal");

            RefreshLocalData(InvalidType.AllLocal);

            return;
        }

        if (!e.ITypes.Contains(InvalidType.Appointment) && !e.ITypes.Contains(InvalidType.Task) && !e.ITypes.Contains(InvalidType.TaskPopup))
        {
            RefreshLocalData(e.ITypes);
        }

        if (e.ITypes.Contains(InvalidType.Task) || e.ITypes.Contains(InvalidType.TaskPopup))
        {
            if (controlChart?.Visible ?? false)
            {
                ODEvent.Fire(ODEventType.Cache, "Refreshing Caches: Chart Module");

                controlChart.ModuleSelected(PatNumCur);
            }

            return;
        }

        ODEvent.Fire(ODEventType.Cache, "Refreshing Caches: Inserting Signals");
        foreach (var invalidType in e.ITypes)
        {
            var signalod = new Signalod
            {
                IType = invalidType
            };

            switch (invalidType)
            {
                case InvalidType.Task:
                case InvalidType.TaskPopup:
                    signalod.FKey = e.TaskNum;
                    signalod.FKeyType = KeyType.Task;
                    break;

                case InvalidType.UserOdPrefs:
                    signalod.FKey = Security.CurUser?.UserNum ?? 0;
                    signalod.FKeyType = KeyType.UserOd;
                    break;
            }

            Signalods.Insert(signalod);
        }
    }

    private void timerTimeIndic_Tick(object sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized && controlAppt.Visible)
        {
            controlAppt.TickRefresh();
        }
    }

    private static bool IsWorkStationActive()
    {
        var sigInactiveMin = PrefC.GetInt(PrefName.SignalInactiveMinutes);
        if (sigInactiveMin == 0)
        {
            return true;
        }

        var dateTimeToSignalInactive = Security.DateTimeLastActivity + TimeSpan.FromMinutes(sigInactiveMin);

        return DateTime.Now <= dateTimeToSignalInactive;
    }
    
    private void SignalsTick(bool isAllInvalidTypes = true)
    {
        try
        {
            if (Application.OpenForms.OfType<FormTerminal>().Any() & !IsWorkStationActive())
            {
                _onlyProcessHighPrioritySignals = true;
            }

            if (Security.CurUser == null || !Userods.GetIsCacheAllowed())
            {
                _onlyProcessHighPrioritySignals = true;
            }

            if (_onlyProcessHighPrioritySignals && IsWorkStationActive() && Security.CurUser != null && Userods.GetIsCacheAllowed())
            {
                _onlyProcessHighPrioritySignals = false;
            }
        }
        catch
        {
            // ignored
        }

        if (!_onlyProcessHighPrioritySignals)
        {
            PreprocessForTasks();
        }
        
        Action<bool> actionOnShutdown = value => Invoke(() => InitiateShutdown(value));
        GetAndProcessSignals(
            actionOnShutdown,
            (listODForms, listSignals) =>
            {
                //Synchronize the thread static Security.CurUser on the main thread if the Userod cache was refreshed.
                if (listSignals.Any(x => x.IType.In(InvalidType.Security, InvalidType.AllLocal)))
                {
                    Invoke(() =>
                    {
                        try
                        {
                            Security.SyncCurUser();
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }

                var forms = new List<FormODBase>(listODForms);
                
                Invoke(() =>
                {
                    foreach (var form in forms)
                    {
                        try
                        {
                            form.ProcessSignals(listSignals);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                });
            },
            () => { },
            isAllInvalidTypes: !_onlyProcessHighPrioritySignals && isAllInvalidTypes
        );
    }

    private void PreprocessForTasks()
    {
        if (_userNumTasks != Security.CurUser.UserNum //The user has changed since the last signal tick was run (when logoff then logon),
            || _tasksReminders == null || _listTaskNumsNormal == null) //or first time processing signals since the program started.
        {
            _userNumTasks = Security.CurUser.UserNum;
            var listTasksRefreshed = Tasks.GetNewTasksThisUser(Security.CurUser.UserNum, Clinics.ClinicNum); //Get all tasks pertaining to current user.
            _listTaskNumsNormal = [];
            _tasksReminders = [];
            _listTasksRemindersOverLimit = [];
            var listUserOdPrefsBlockedTasks = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.TaskListBlock);
            if (_dictionaryAllTaskLists == null || listTasksRefreshed.Exists(x => !_dictionaryAllTaskLists.ContainsKey(x.TaskListNum)))
            {
                //Refresh dict if needed.
                _dictionaryAllTaskLists = TaskLists.GetAll().ToDictionary(x => x.TaskListNum);
            }

            for (var i = 0; i < listTasksRefreshed.Count; i++)
            {
                //Construct the initial task meta data for the current user's tasks.
                //If task's taskList is in dictionary and it's archived or has an archived ancestor, ignore it.
                if (_dictionaryAllTaskLists.ContainsKey(listTasksRefreshed[i].TaskListNum)
                    && (_dictionaryAllTaskLists[listTasksRefreshed[i].TaskListNum].TaskListStatus == TaskListStatusEnum.Archived
                        || TaskLists.IsAncestorTaskListArchived(ref _dictionaryAllTaskLists, _dictionaryAllTaskLists[listTasksRefreshed[i].TaskListNum])))
                {
                    continue;
                }

                var isTrackedByUser = PrefC.GetBool(PrefName.TasksNewTrackedByUser);
                if (string.IsNullOrEmpty(listTasksRefreshed[i].ReminderGroupId))
                {
                    //A normal task.
                    //Mimics how checkNew is set in FormTaskEdit.
                    if (isTrackedByUser && listTasksRefreshed[i].IsUnread || !isTrackedByUser && listTasksRefreshed[i].TaskStatus == TaskStatusEnum.New)
                    {
                        //See def of task.IsUnread
                        _listTaskNumsNormal.Add(listTasksRefreshed[i].TaskNum);
                    }
                }
                else if (PrefC.GetBool(PrefName.TasksUseRepeating) && listTasksRefreshed[i].DateTimeEntry <= DateTime.Now)
                {
                    if (isTrackedByUser && listTasksRefreshed[i].IsUnread)
                    {
                        _listTaskNumsNormal.Add(listTasksRefreshed[i].TaskNum);
                    }
                    else if (!isTrackedByUser && listTasksRefreshed[i].TaskStatus == TaskStatusEnum.New)
                    {
                        _listTaskNumsNormal.Add(listTasksRefreshed[i].TaskNum);
                    }
                }
                else if (!PrefC.GetBool(PrefName.TasksUseRepeating))
                {
                    //A reminder task (new or viewed).  Reminders not allowed if repeating tasks enabled.
                    _tasksReminders.Add(listTasksRefreshed[i]);
                    if (listTasksRefreshed[i].DateTimeEntry <= DateTime.Now)
                    {
                        //Do not show reminder popups for future reminders which are not due yet.
                        //Mimics how checkNew is set in FormTaskEdit.
                        if (isTrackedByUser && listTasksRefreshed[i].IsUnread || !isTrackedByUser && listTasksRefreshed[i].TaskStatus == TaskStatusEnum.New)
                        {
                            //See def of task.IsUnread
                            //NOTE: POPUPS ONLY HAPPEN IF THEY ARE MARKED AS NEW. (Also, they will continue to pop up as long as they are marked "new")
                            TaskPopupHelper(listTasksRefreshed[i], listUserOdPrefsBlockedTasks);
                        }
                    }
                }
            }

            //Refresh the appt module to show the current list of reminders, even if the appt module not visible.  This refresh is fast.
            //The user will load the appt module eventually and these refreshes are the only updates the appointment module receives for reminders.
            controlAppt.RefreshReminders(_tasksReminders);
            _dateReminderRefresh = DateTime.Today;
        }
        //Check to see if a reminder task became due between the last signal interval and the current signal interval.
        else if (_tasksReminders.FindAll(x => x.DateTimeEntry <= DateTime.Now
                                                  && x.DateTimeEntry >= DateTime.Now.AddSeconds(-PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs))).Count > 0)
        {
            var listTasksDueReminders = _tasksReminders.FindAll(x => x.DateTimeEntry <= DateTime.Now
                                                                         && x.DateTimeEntry >= DateTime.Now.AddSeconds(-PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs)));

            var listSignalods = new List<Signalod>();
            
            foreach (var t in listTasksDueReminders)
            {
                listSignalods.Add(new Signalod
                {
                    IType = InvalidType.TaskList,
                    FKey = t.TaskListNum,
                    FKeyType = KeyType.Undefined
                });
            }

            UserControlTasks.RefreshTasksForAllInstances(listSignalods);
            var listUserOdPrefsBlockedTasks = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.TaskListBlock);
            for (var i = 0; i < listTasksDueReminders.Count; i++)
            {
                TaskPopupHelper(listTasksDueReminders[i], listUserOdPrefsBlockedTasks);
            }
        }
        else if (_listTasksRemindersOverLimit.Count > 0)
        {
            //Try to display any due reminders that previously exceeded our limit of FormTaskEdit to show.
            var listUserOdPrefsBlockedTasks = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.TaskListBlock);
            for (var i = _listTasksRemindersOverLimit.Count - 1; i >= 0; i--)
            {
                //TaskPopupHelper
                TaskPopupHelper(_listTasksRemindersOverLimit[i], listUserOdPrefsBlockedTasks);
            }
        }
        else if (_dateReminderRefresh.Date < DateTime.Today)
        {
            //Refresh the appt module to show the current list of reminders, even if the appt module not visible.  This refresh is fast.
            //The user will load the appt module eventually and these refreshes are the only updates the appointment module receives for reminders.
            controlAppt.RefreshReminders(_tasksReminders);
            _dateReminderRefresh = DateTime.Today;
        }

        RefreshTasksNotification();
    }

    public static void GetAndProcessSignals(Action<bool> onShutdown, Action<List<FormODBase>, List<Signalod>> onProcess, Action onDone, bool isAllInvalidTypes)
    {
        var listSignals = new List<Signalod>();
        //Sets the list of high priority signal types that are always processed.
        var listInvalidTypesHighPriority = new List<InvalidType>();
        listInvalidTypesHighPriority.Add(InvalidType.ShutDownNow);
        listInvalidTypesHighPriority.Add(InvalidType.ActiveInstance);
        listInvalidTypesHighPriority.Add(InvalidType.Print);
        //Create thread to handle getting and processing signals.
        var threadRefreshSignals = new ODThread(_ =>
        {
            //Get new signals from DB.

            var dateTimeHighPrioritySignalRefresh = MiscData.GetNowDateTime();
            listSignals = Signalods.RefreshTimed(Signalods.DateTHighPrioritySignalLastRefreshed, listInvalidTypesHighPriority);
            Signalods.DateTHighPrioritySignalLastRefreshed = dateTimeHighPrioritySignalRefresh;
            if (isAllInvalidTypes)
            {
                //when coming back from inactive state, this could result in a very large list.
                listSignals.AddRange(Signalods.RefreshTimed(Signalods.DateTRegularPrioritySignalLastRefreshed, listInvalidTypesExclude: listInvalidTypesHighPriority));
            }


            //Only update the time stamp with signals retreived from the DB. Do NOT use listLocalSignals to set timestamp.
            if (listSignals.Count > 0)
            {
                if (isAllInvalidTypes)
                {
                    Signalods.DateTRegularPrioritySignalLastRefreshed = listSignals.Max(x => x.SigDateTime);
                }
                //There was an else statement here that got removed. 
                //If isAllInvalidTypes is false, listSignals will only contain our high priority signals.
                //There is no need to update Signalods.DateTSignalLastRefreshed until isAllInvalidTypes returns to true,
                //so we can process the entire backlog of low priority signals that accumulated during inactivity.
                //This is how this code would have behaved before we allowed signals to be accumulated and processed while inactive.

                //This happens to keep DateTSignalLastRefreshed in sync with DateTAppySignalLastRefreshed. 
                //The signals that depend on DateTApptSignalLastRefreshed are processed in 2 places, using 2 timers. This one timerSignal, and timerTimeIndic.
                //Both tick events handle InvalidType.Appointments and InvalidType.Schedule.
                //What this does mean though is those signals do get double processed, and they always have. This results in extra refreshes of the appt module.
                Signalods.DateTApptSignalLastRefreshed = Signalods.DateTRegularPrioritySignalLastRefreshed;
            }


            if (listSignals.Count == 0)
            {
                return;
            }


            //The shutdown method can be shared between inactive and active instances.
            //The only difference between how shutdowns are handled in the two cases is when a user is inactive a thread doesn't start
            //and UI is not shown to indicate a shutdown.
            if (listSignals.Exists(x => x.IType == InvalidType.ShutDownNow))
            {
                onShutdown(true);
                return;
            }

            //Shut down this machine if the last active instance matches the targeted signal.
            if (listSignals.Exists(x => x.IType == InvalidType.ActiveInstance && ActiveInstances.GetActiveInstance() != null && x.FKey == ActiveInstances.GetActiveInstance().ActiveInstanceNum))
            {
                onShutdown(false);
                return;
            }

            //Create a distinct list of undefined invalid types which are from signals invalidating entire cache classes.
            //Include the UserOdPrefs invalid type if there is a signal for the currently logged in user.
            var listInvalidTypes = listSignals.Where(x => x.FKey == 0 && x.FKeyType == KeyType.Undefined
                                                          || x.IType == InvalidType.UserOdPrefs && x.FKeyType == KeyType.UserOd && Security.CurUser != null && x.FKey == Security.CurUser.UserNum)
                .Select(x => x.IType)
                .Distinct()
                .ToList();
            //The preference cache is unique in that it is heavily used and should never be cleared out. It should be refreshed immediately instead.
            if (listInvalidTypes.Remove(InvalidType.Prefs))
            {
                Cache.Refresh(InvalidType.Prefs);
            }

            //The PhoneEmpDefaults cache is unique in that it is heavily used and should never be cleared out. It should be refreshed immediately instead.
            if (listInvalidTypes.Remove(InvalidType.PhoneEmpDefaults))
            {
                Cache.Refresh(InvalidType.PhoneEmpDefaults);
            }

            //The remaining caches should be cleared out and will be refilled when needed.
            Cache.ClearCaches(listInvalidTypes.ToArray());
            onProcess(FormsSubscribed, listSignals);
        });
        threadRefreshSignals.AddExceptionHandler(_ =>
        {
            DateTime dateTimeRefreshed;
            try
            {
                //Signal processing should always use the server's time.
                dateTimeRefreshed = MiscData.GetNowDateTime();
            }
            catch
            {
                //If the server cannot be reached, we still need to move the signal processing forward so use local time as a fail-safe.
                dateTimeRefreshed = DateTime.Now;
            }

            Signalods.DateTRegularPrioritySignalLastRefreshed = dateTimeRefreshed;
            Signalods.DateTApptSignalLastRefreshed = dateTimeRefreshed;
        });
        threadRefreshSignals.AddExitHandler(_ => { onDone(); });
        threadRefreshSignals.Name = "SignalsTick";
        threadRefreshSignals.Start();
    }

    private void SignalsTickExceptionHandler(Exception ex)
    {
        //If an exception happens during processing signals, we will not close the program because the user is not trying to do anything. We will
        //send the first exception to HQ.
        if (_exceptionSignalsTick == null)
        {
            _exceptionSignalsTick = new Exception("SignalsTick exception.", ex);
            ODException.SwallowAnyException(() => { BugSubmissions.SubmitException(_exceptionSignalsTick, patNumCur: PatNumCur, moduleName: GetSelectedModuleName()); });
        }
    }

    private void AddAlertsToMenu()
    {
        var alertCount = _alertItems.Count - _alertItemReads.Count;
        
        switch (alertCount)
        {
            case > 99:
                _menuItemAlerts.Text = "Alerts (99)";
                _menuItemAlerts.ForeColor = Color.Red;
                break;
            
            case 0:
                _menuItemAlerts.Text = "Alerts (" + alertCount + ")";
                _menuItemAlerts.ForeColor = Color.Black;
                break;
            
            default:
                _menuItemAlerts.Text = "Alerts (" + alertCount + ")";
                _menuItemAlerts.ForeColor = Color.Red;
                break;
        }
    }

    protected override void ProcessSignalODs(List<Signalod> signals)
    {
        if (signals.Exists(x => x.IType == InvalidType.Programs))
        {
            RefreshMenuReports();
        }

        #region SMS Notifications

        var signalodSmsCount = signals.OrderByDescending(x => x.SigDateTime)
            .FirstOrDefault(x => x.IType == InvalidType.SmsTextMsgReceivedUnreadCount && x.FKeyType == KeyType.SmsMsgUnreadCount);
        if (signalodSmsCount != null)
        {
            //Provide the pre-existing value here. This will act as a flag indicating that we should not resend the signal.  This would cause infinite signal loop.
            SetSmsNotificationText(signalodSmsCount);
        }

        #endregion SMS Notifications

        #region Tasks

        var listSignalodsTasks = signals.FindAll(x => x.IType == InvalidType.Task || x.IType == InvalidType.TaskPopup
                                                                                  || x.IType == InvalidType.TaskList || x.IType == InvalidType.TaskAuthor || x.IType == InvalidType.TaskPatient);
        var listEditedTaskNums = listSignalodsTasks.FindAll(x => x.FKeyType == KeyType.Task).Select(x => x.FKey).ToList();
        BeginTasksThread(listSignalodsTasks, listEditedTaskNums);

        #endregion Tasks

        #region Appointment Module

        if (controlAppt.Visible)
        {
            var listOpNumsVisible = controlAppt.GetListOpsVisible().Select(x => x.OperatoryNum).ToList();
            var listProvNumsVisible = controlAppt.GetListProvsVisible().Select(x => x.Id).ToList();
            var isRefreshAppts = Signalods.IsApptRefreshNeeded(controlAppt.GetDateSelected().Date, signals, listOpNumsVisible, listProvNumsVisible);
            var isRefreshScheds = Signalods.IsSchedRefreshNeeded(controlAppt.GetDateSelected().Date, signals, listOpNumsVisible, listProvNumsVisible);
            var isRefreshPanelButtons = Signalods.IsContrApptButtonRefreshNeeded(signals);
            if (isRefreshAppts || isRefreshScheds)
            {
                controlAppt.RefreshPeriod(isRefreshAppointments: isRefreshAppts, isRefreshSchedules: isRefreshScheds);

                ODEvent.Fire(ODEventType.AppointmentEdited, signals);
            }

            if (isRefreshPanelButtons)
            {
                controlAppt.RefreshModuleScreenButtonsRight();
            }
        }

        var signalodTP = signals.FirstOrDefault(x => x.IType == InvalidType.TPModule && x.FKeyType == KeyType.PatNum);
        if (controlTreat.Visible && signalodTP != null && signalodTP.FKey == controlTreat.PatientCur.PatNum)
        {
            RefreshCurrentModule();
        }

        var signalodPP = signals.FirstOrDefault(x => x.IType == InvalidType.AccModule && x.FKeyType == KeyType.PatNum);
        if (controlAccount.Visible && signalodPP != null && signalodPP.FKey == controlAccount.GetPatNum())
        {
            RefreshCurrentModule();
        }

        #endregion Appointment Module

        #region WPF

        GlobalFormOpenDental.ProcessSignalODs(signals);

        #endregion WPF

        #region Unfinalize Pay Menu Update

        UpdateUnfinalizedPayCount(signals.FindAll(x => x.IType == InvalidType.UnfinalizedPayMenuUpdate));

        #endregion Unfinalize Pay Menu Update
        
        #region Refresh

        var invalidTypesArray = Signalods.GetInvalidTypes(signals);
        if (invalidTypesArray.Length > 0)
        {
            RefreshLocalDataPostCleanup(invalidTypesArray);
        }

        #endregion Refresh
    }

    public static void S_HandleRefreshedTasks(List<Signalod> listSignalodTasks, List<long> listEditedTaskNums, List<Task> listTasksRefreshed, List<TaskNote> listTaskNotesRefreshed, List<UserOdPref> listUserOdPrefsBlockedTasks)
    {
        _formOpenDentalSingleton.HandleRefreshedTasks(listSignalodTasks, listEditedTaskNums, listTasksRefreshed, listTaskNotesRefreshed, listUserOdPrefsBlockedTasks);
    }

    private void HandleRefreshedTasks(List<Signalod> listSignalodsTasks, List<long> listEditedTaskNums, List<Task> listTasksRefreshed, List<TaskNote> listTaskNotesRefreshed, List<UserOdPref> listUserOdPrefsBlockedTasks)
    {
        var hasChangedReminders = UpdateTaskMetaData(listEditedTaskNums, listTasksRefreshed);
        RefreshTasksNotification();
        RefreshOpenTasksOrPopupNewTasks(listSignalodsTasks, listTasksRefreshed, listTaskNotesRefreshed, listUserOdPrefsBlockedTasks);
        //Refresh the appt module if reminders have changed, even if the appt module not visible.
        //The user will load the appt module eventually and these refreshes are the only updates the appointment module receives for reminders.
        if (hasChangedReminders)
        {
            controlAppt.RefreshReminders(_tasksReminders);
            _dateReminderRefresh = DateTime.Today;
        }
    }

    private bool UpdateTaskMetaData(List<long> listEditedTaskNums, List<Task> listTasksRefreshed)
    {
        //Check to make sure there are edited task nums passed in and that the meta data lists have been initialized by the signal processor.
        if (listEditedTaskNums == null || _tasksReminders == null || _listTaskNumsNormal == null)
        {
            return false; //Nothing to do.
        }

        var hasChangedReminders = false;
        for (var i = 0; i < listEditedTaskNums.Count; i++)
        {
            //Update the task meta data for the current user based on the query results.
            var editedTaskNum = listEditedTaskNums[i]; //The tasknum mentioned in the signal.
            var taskForUser = listTasksRefreshed?.FirstOrDefault(x => x.TaskNum == editedTaskNum);
            Task taskNewForUser = null;
            if (taskForUser != null)
            {
                var isTrackedByUser = PrefC.GetBool(PrefName.TasksNewTrackedByUser);
                //Mimics how checkNew is set in FormTaskEdit.
                if ((isTrackedByUser && taskForUser.IsUnread || !isTrackedByUser && taskForUser.TaskStatus == TaskStatusEnum.New) //See def of task.IsUnread
                    //Reminders not due yet are excluded from Tasks.RefreshUserNew().
                    && (string.IsNullOrEmpty(taskForUser.ReminderGroupId) || taskForUser.DateTimeEntry <= DateTime.Now))
                {
                    taskNewForUser = taskForUser;
                }
            }

            var taskReminderOld = _tasksReminders.FirstOrDefault(x => x.TaskNum == editedTaskNum);
            if (taskReminderOld != null)
            {
                //The task is a reminder which is relevant to the current user.
                hasChangedReminders = true;
                _tasksReminders.RemoveAll(x => x.TaskNum == editedTaskNum); //Remove the old copy of the task.
                if (taskForUser != null)
                {
                    //The updated reminder task is relevant to the current user.
                    _tasksReminders.Add(taskForUser); //Add the updated reminder task into the list (replacing the old reminder task).
                }
            }
            else if (_listTaskNumsNormal.Contains(editedTaskNum))
            {
                //The task is a normal task which is relevant to the current user.
                if (taskNewForUser == null)
                {
                    //But now the task is no longer relevant to the user.
                    _listTaskNumsNormal.Remove(editedTaskNum);
                }
            }
            else
            {
                //The edited tasknum is not currently in our meta data.
                if (taskNewForUser != null && string.IsNullOrEmpty(taskNewForUser.ReminderGroupId))
                {
                    //A new normal task has now become relevant.
                    _listTaskNumsNormal.Add(editedTaskNum);
                }
                else if (taskForUser != null && !string.IsNullOrEmpty(taskForUser.ReminderGroupId))
                {
                    //A reminder task has become relevant (new or viewed)
                    hasChangedReminders = true;
                    _tasksReminders.Add(taskForUser);
                }
            } //else
        } //for

        return hasChangedReminders;
    }

    private void RefreshOpenTasksOrPopupNewTasks(List<Signalod> listSignalodsTasks, List<Task> listTasksRefreshed, List<TaskNote> listTaskNotesRefreshed, List<UserOdPref> listUserOdPrefBlockedTasks)
    {
        if (listSignalodsTasks == null)
        {
            return;
        }

        var listSignalTasksNums = listSignalodsTasks.Select(x => x.FKey).ToList();
        var listTaskNumsOpen = new List<long>();
        for (var i = 0; i < Application.OpenForms.Count; i++)
        {
            var form = Application.OpenForms[i];
            if (!(form is FormTaskEdit))
            {
                continue;
            }

            var formTaskEdit = (FormTaskEdit) form;
            if (listSignalTasksNums.Contains(formTaskEdit.TaskCur.TaskNum))
            {
                formTaskEdit.OnTaskEdited();
                listTaskNumsOpen.Add(formTaskEdit.TaskCur.TaskNum);
            }
        }

        var listTasksPopup = new List<Task>();
        if (listTasksRefreshed != null)
        {
            for (var i = 0; i < listTasksRefreshed.Count; i++)
            {
                if (!listSignalodsTasks.Exists(x => x.FKeyType == KeyType.Task && x.IType == InvalidType.TaskPopup && x.FKey == listTasksRefreshed[i].TaskNum) || listTaskNumsOpen.Contains(listTasksRefreshed[i].TaskNum))
                {
                    continue;
                }

                if (!listTasksPopup.Contains(listTasksRefreshed[i]))
                {
                    listTasksPopup.Add(listTasksRefreshed[i]);
                }
            }
        }

        for (var i = 0; i < listTasksPopup.Count; i++)
        {
            //Reminders sent to a subscribed tasklist will pop up prior to the reminder date/time.
            TaskPopupHelper(listTasksPopup[i], listUserOdPrefBlockedTasks, listTaskNotesRefreshed?.FindAll(x => x.TaskNum == listTasksPopup[i].TaskNum));
        }

        if (listSignalodsTasks.Count > 0 || listTasksPopup.Count > 0)
        {
            UserControlTasks.RefreshTasksForAllInstances(listSignalodsTasks);
        }
    }

    private void TaskPopupHelper(Task taskPopup, List<UserOdPref> listUserOdPrefsBlockedTasks, List<TaskNote> listTaskNotes = null)
    {
        if (Application.OpenForms.OfType<FormTerminal>().Any())
        {
            return;
        }

        if (taskPopup.DateTimeEntry > DateTime.Now && taskPopup.ReminderType != TaskReminderType.NoReminder)
        {
            return;
        }

        if (taskPopup.ReminderType != TaskReminderType.NoReminder)
        {
            if (Application.OpenForms.OfType<FormTaskEdit>().ToList().Count >= PopupPressureReliefLimit)
            {
                if (!_listTasksRemindersOverLimit.Exists(x => x.TaskNum == taskPopup.TaskNum))
                {
                    _listTasksRemindersOverLimit.Add(taskPopup);
                }

                return;
            }

            _listTasksRemindersOverLimit.RemoveAll(x => x.TaskNum == taskPopup.TaskNum);
        }

        var listTaskNotes2 = (listTaskNotes ?? TaskNotes.GetForTask(taskPopup.TaskNum)).OrderBy(x => x.DateTimeNote).ToList();
        if (taskPopup.ReminderType == TaskReminderType.NoReminder)
        {
            if (listTaskNotes2.Count == 0)
            {
                if (taskPopup.UserNum == Security.CurUser.UserNum)
                {
                    return;
                }
            }
            else
            {
                if (listTaskNotes2[listTaskNotes2.Count - 1].UserNum == Security.CurUser.UserNum)
                {
                    return;
                }
            }
        }

        var listTaskListsUserSubsTrunk = TaskLists.RefreshUserTrunk(Security.CurUser.UserNum); //Get the list of directly subscribed tasklists.
        var listUserTaskListSubNums = listTaskListsUserSubsTrunk.Select(x => x.TaskListNum).ToList();

        var isUserSubscribed = listUserTaskListSubNums.Contains(taskPopup.TaskListNum); //First check if user is directly subscribed.
        if (!isUserSubscribed)
        {
            isUserSubscribed = listTaskListsUserSubsTrunk.Any(x => TaskLists.IsAncestor(x.TaskListNum, taskPopup.TaskListNum)); //Check ancestors for subscription.
        }

        if (isUserSubscribed)
        {
            //User is subscribed to this TaskList, or one of its ancestors.
            var byteArrayRawData = new byte[Properties.Resources.notify.Length];
            Properties.Resources.notify.Read(byteArrayRawData, 0, byteArrayRawData.Length);
            if (!listUserOdPrefsBlockedTasks.Any(x => x.Fkey == taskPopup.TaskListNum && SIn.Bool(x.ValueString)))
            {
                //Subscribed and Unblocked, Show it!
                SoundHelper.PlaySound(byteArrayRawData);
                var formTaskEdit = new FormTaskEdit(taskPopup);
                formTaskEdit.IsPopup = true;
                if (taskPopup.ReminderType != TaskReminderType.NoReminder)
                {
                    //If a reminder task, make an audit trail entry
                    Tasks.TaskEditCreateLog(EnumPermType.TaskReminderPopup, $"Reminder task {taskPopup.TaskNum} shown to user", taskPopup);
                }

                formTaskEdit.Show();
                formTaskEdit.BringToFront();
            }
            else
            {
                var userOdPrefTaskSound = UserOdPrefs.GetFirstOrNewByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.TaskBlockedMakeSound);
                if (!userOdPrefTaskSound.IsNew && SIn.Bool(userOdPrefTaskSound.ValueString))
                {
                    SoundHelper.PlaySound(byteArrayRawData);
                }
            }
        }
    }

    private void GotoModule_ModuleSelected(EnumModuleType moduleType, DateTime? dateSelected = null, List<long> listPinApptNums = null, long selectedAptNum = 0, long claimNum = 0, long patNum = 0, long docNum = 0, bool doShowSearch = false)
    {
        var e = new ModuleEventArgs(dateSelected ?? DateTime.MinValue, listPinApptNums ??= [], selectedAptNum, moduleType, claimNum, patNum, docNum, doShowSearch);
        GotoModule_ModuleSelected(e);
    }

    private void GotoModule_ModuleSelected(ModuleEventArgs e)
    {
        if (e.PatNum != 0)
        {
            if (e.PatNum != PatNumCur)
            {
                PatNumCur = e.PatNum;
            }

            var patient = Patients.GetPat(PatNumCur);

            FillPatientButton(patient);
        }

        UnselectActive();
        AllNeutral();

        if (e.ClaimNum > 0)
        {
            moduleBar.SelectedModule = e.ModuleType;
            controlAccount.Visible = true;
            ActiveControl = controlAccount;
            controlAccount.ModuleSelected(PatNumCur, e.ClaimNum);
        }
        else if (e.ListPinApptNums.Count != 0)
        {
            moduleBar.SelectedModule = e.ModuleType;
            controlAppt.Visible = true;
            ActiveControl = controlAppt;
            controlAppt.ModuleSelectedWithPinboard(PatNumCur, e.ListPinApptNums, e.DateSelected, e.DoShowSearch);
        }
        else if (e.SelectedAptNum != 0)
        {
            moduleBar.SelectedModule = e.ModuleType;
            controlAppt.Visible = true;
            ActiveControl = controlAppt;
            controlAppt.ModuleSelectedGoToAppt(e.SelectedAptNum, e.DateSelected);
        }
        else if (e.DocNum > 0)
        {
            if (ImagesModuleUsesOld2020())
            {
                moduleBar.SelectedModule = e.ModuleType;
                controlImagesOld.InitializeOnStartup();
                controlImagesOld.Visible = true;
                ActiveControl = controlImagesOld;
                controlImagesOld.ModuleSelected(PatNumCur, e.DocNum);
            }
            else
            {
                moduleBar.SelectedModule = e.ModuleType;
                controlImages.InitializeOnStartup();
                controlImages.Visible = true;
                ActiveControl = controlImages;
                controlImages.ModuleSelected(PatNumCur, e.DocNum);
            }
        }
        else if (e.ModuleType != EnumModuleType.None)
        {
            moduleBar.SelectedModule = e.ModuleType;
            SetModuleSelected();
        }

        moduleBar.Invalidate();
    }

    private void moduleBar_ButtonClicked(object sender, ButtonClicked_EventArgs e)
    {
        switch (moduleBar.SelectedModule)
        {
            case EnumModuleType.Appointments:
                if (!Security.IsAuthorized(EnumPermType.AppointmentsModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;

            case EnumModuleType.Family:
                if (PrefC.GetBool(PrefName.EhrEmergencyNow))
                {
                    if (Security.IsAuthorized(EnumPermType.EhrEmergencyAccess, true))
                    {
                        break;
                    }
                }

                if (!Security.IsAuthorized(EnumPermType.FamilyModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;

            case EnumModuleType.Account:
                if (!Security.IsAuthorized(EnumPermType.AccountModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;

            case EnumModuleType.TreatPlan:
                if (!Security.IsAuthorized(EnumPermType.TPModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;

            case EnumModuleType.Chart:
                if (!Security.IsAuthorized(EnumPermType.ChartModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;

            case EnumModuleType.Imaging:
                if (!Security.IsAuthorized(EnumPermType.ImagingModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;

            case EnumModuleType.Manage:
                if (!Security.IsAuthorized(EnumPermType.ManageModule))
                {
                    e.Cancel = true;
                    return;
                }

                break;
        }

        UnselectActive();
        AllNeutral();
        SetModuleSelected(true);
    }

    public string GetSelectedModuleName()
    {
        try
        {
            return moduleBar.SelectedModule.ToString();
        }
        catch
        {
            return "";
        }
    }

    private void SetModuleSelected(bool menuBarClicked = false)
    {
        switch (moduleBar.SelectedModule)
        {
            case EnumModuleType.Appointments:
                controlAppt.InitializeOnStartup();
                controlAppt.Visible = true;
                ActiveControl = controlAppt;
                controlAppt.ModuleSelected(PatNumCur);
                break;

            case EnumModuleType.Family:
                controlFamily.InitializeOnStartup();
                controlFamily.Visible = true;
                ActiveControl = controlFamily;
                controlFamily.ModuleSelected(PatNumCur);
                break;

            case EnumModuleType.Account:
                controlAccount.InitializeOnStartup();
                controlAccount.Visible = true;
                ActiveControl = controlAccount;
                controlAccount.ModuleSelected(PatNumCur);
                break;

            case EnumModuleType.TreatPlan:
                controlTreat.InitializeOnStartup();
                controlTreat.Visible = true;
                ActiveControl = controlTreat;
                if (menuBarClicked)
                {
                    controlTreat.ModuleSelected(PatNumCur, true);
                }
                else
                {
                    controlTreat.ModuleSelected(PatNumCur);
                }

                break;

            case EnumModuleType.Chart:
                controlChart.InitializeOnStartup();
                controlChart.Visible = true;
                ActiveControl = controlChart;
                if (menuBarClicked)
                {
                }
                else
                {
                    controlChart.ModuleSelected(PatNumCur, true);
                }

                TryNonPatientPopup();
                break;

            case EnumModuleType.Imaging:
                if (ImagesModuleUsesOld2020())
                {
                    controlImagesOld.InitializeOnStartup();
                    controlImagesOld.Visible = true;
                    ActiveControl = controlImagesOld;
                    controlImagesOld.ModuleSelected(PatNumCur);
                }
                else
                {
                    controlImages.InitializeOnStartup();
                    controlImages.Visible = true;
                    ActiveControl = controlImages;
                    controlImages.ModuleSelected(PatNumCur);
                }

                break;

            case EnumModuleType.Manage:
                controlManage.Visible = true;
                ActiveControl = controlManage;
                controlManage.ModuleSelected(PatNumCur);
                break;
        }
    }

    private void AllNeutral()
    {
        controlAppt.Visible = false;
        controlFamily.Visible = false;
        controlAccount.Visible = false;
        controlTreat.Visible = false;
        controlChart.Visible = false;

        if (ImagesModuleUsesOld2020())
        {
            controlImagesOld.Visible = false;
        }
        else
        {
            if (controlImages != null)
            {
                controlImages.Visible = false;
            }
        }

        controlManage.Visible = false;
    }

    private void UnselectActive(bool isLoggingOff = false)
    {
        if (controlAppt.Visible)
        {
            controlAppt.ModuleUnselected();
        }

        if (controlFamily.Visible)
        {
            controlFamily.ModuleUnselected();
        }

        if (controlAccount.Visible)
        {
            controlAccount.ModuleUnselected();
        }

        if (controlTreat.Visible)
        {
            controlTreat.ModuleUnselected();
        }

        if (controlChart.Visible)
        {
            controlChart.ModuleUnselected(isLoggingOff);
        }

        if (ImagesModuleUsesOld2020())
        {
            if (controlImagesOld.Visible)
            {
                controlImagesOld.ModuleUnselected();
            }
        }
        else
        {
            if (controlImages.Visible)
            {
                controlImages.ModuleUnselected();
            }
        }
    }

    private void RefreshCurrentModule(bool hasForceRefresh = false, bool isApptRefreshDataPat = true, bool isClinicRefresh = true, long docNum = 0)
    {
        if (controlAppt.Visible)
        {
            if (hasForceRefresh)
            {
                controlAppt.ModuleSelected(PatNumCur);
            }
            else
            {
                if (isApptRefreshDataPat)
                {
                    //don't usually skip data refresh, only if CurPatNum was set just prior to calling this method
                    controlAppt.RefreshModuleDataPatient(PatNumCur);
                }

                controlAppt.RefreshModuleScreenButtonsRight();
            }
        }

        if (controlFamily.Visible)
        {
            controlFamily.ModuleSelected(PatNumCur);
        }

        if (controlAccount.Visible)
        {
            controlAccount.ModuleSelected(PatNumCur);
        }

        if (controlTreat.Visible)
        {
            controlTreat.ModuleSelected(PatNumCur);
        }

        if (controlChart.Visible)
        {
            controlChart.ModuleSelected(PatNumCur, isClinicRefresh);
        }

        if (ImagesModuleUsesOld2020())
        {
            if (controlImagesOld.Visible)
            {
                controlImagesOld.ModuleSelected(PatNumCur, docNum);
            }
        }
        else
        {
            if (controlImages.Visible)
            {
                controlImages.ModuleSelected(PatNumCur, docNum);
            }
        }

        if (controlManage.Visible)
        {
            controlManage.ModuleSelected(PatNumCur);
        }
    }

    private void DataConnection_CredentialsFailedAfterLogin(ODEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => DataConnection_CredentialsFailedAfterLogin(e));
        }
    }

    public static void S_TaskGoTo(TaskObjectType taskObjectType, long keyNum)
    {
        _formOpenDentalSingleton.TaskGoTo(taskObjectType, keyNum);
    }

    private bool IsPatInRestrictedClinic(long patNum)
    {
        if (Security.IsAuthorized(EnumPermType.UnrestrictedSearch, suppressMessage: true))
        {
            return false;
        }

        var listUserClinicNums = Clinics.GetForUserod(Security.CurUser, !Security.CurUser.ClinicIsRestricted).Select(x => x.Id).ToList();
        var patient = Patients.GetLim(patNum);
        if (listUserClinicNums.Contains(patient.ClinicNum)
            || Appointments.GetAppointmentsForPat(patNum).Select(x => x.ClinicNum).Any(x => listUserClinicNums.Contains(x)))
        {
            return false;
        }

        return true;
    }

    private void TaskGoTo(TaskObjectType taskObjectType, long keyNum)
    {
        if (taskObjectType == TaskObjectType.None || keyNum == 0)
        {
            return;
        }

        if (taskObjectType == TaskObjectType.Patient)
        {
            if (IsPatInRestrictedClinic(keyNum))
            {
                MsgBox.Show(this, "This patient is assigned to a clinic that you are not authorized for. Contact an Administrator to grant you access or to " +
                                  "create an appointment in your clinic to avoid patient duplication.");
                return;
            }

            PatNumCur = keyNum;
            var patient = Patients.GetPat(PatNumCur);
            RefreshCurrentModule();
            FillPatientButton(patient);
        }

        if (taskObjectType == TaskObjectType.Appointment)
        {
            var appointment = Appointments.GetOneApt(keyNum);
            if (appointment == null)
            {
                MsgBox.Show(this, "Appointment has been deleted, so it's not available.");
                return;
            }

            if (IsPatInRestrictedClinic(appointment.PatNum))
            {
                MsgBox.Show(this, "This patient is assigned to a clinic that you are not authorized for. Contact an Administrator to grant you access or to " +
                                  "create an appointment in your clinic to avoid patient duplication.");
                return;
            }

            var dateSelected = DateTime.MinValue;
            if (appointment.AptStatus == ApptStatus.Planned || appointment.AptStatus == ApptStatus.UnschedList)
            {
                MsgBox.Show(this, "Cannot navigate to appointment.  Use the Other Appointments button.");

                var dateTemp = controlAppt.GetDateSelected();
                if (dateTemp == DateTime.MinValue)
                {
                    dateTemp = DateTime.Now;
                }

                dateSelected = dateTemp;
            }
            else
            {
                dateSelected = appointment.AptDateTime;
            }

            PatNumCur = appointment.PatNum;
            FillPatientButton(Patients.GetPat(PatNumCur));
            GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, dateSelected: dateSelected, selectedAptNum: appointment.AptNum);
        }
    }

    private void menuItemLogOff_Click(object sender, EventArgs e)
    {
        NullUserCheck("menuItemLogOff_Click");

        if (!AreYouSurePrompt(Security.CurUser.UserNum, "Are you sure you would like to log off?"))
        {
            return;
        }

        LogOffNow(false);
    }

    private static bool AreYouSurePrompt(long userNum, string message)
    {
        var userOdPrefLogOffMessage = UserOdPrefs.GetByUserAndFkeyType(userNum, UserOdFkeyType.SuppressLogOffMessage).FirstOrDefault();
        if (userOdPrefLogOffMessage != null)
        {
            return true;
        }

        var inputBox = new InputBox(new InputBoxParam
        {
            InputBoxType_ = InputBoxType.CheckBox,
            LabelText = message,
            Text = "Do not show me this message again.",
            PointPosition = new System.Windows.Point(0, 10)
        })
        {
            HasTimeout = true
        };

        inputBox.ShowDialog();

        if (inputBox.HasTimedOut)
        {
            return true;
        }

        if (inputBox.IsDialogCancel)
        {
            return false;
        }

        if (!inputBox.BoolResult)
        {
            return true;
        }

        UserOdPrefs.Insert(new UserOdPref
        {
            UserNum = Security.CurUser.UserNum,
            FkeyType = UserOdFkeyType.SuppressLogOffMessage
        });

        DataValid.SetInvalid(InvalidType.UserOdPrefs);

        return true;
    }

    private void menuItemUserEmailAddress_Click(object sender, EventArgs e)
    {
        var emailAddressCur = EmailAddresses.GetForUserDb(Security.CurUser.UserNum);
        if (emailAddressCur == null)
        {
            using var formEmailAddressEdit = new FormEmailAddressEdit(Security.CurUser.UserNum);

            formEmailAddressEdit.ShowDialog();
        }
        else
        {
            using var formEmailAddressEdit = new FormEmailAddressEdit(emailAddressCur);

            formEmailAddressEdit.ShowDialog();
        }
    }

    private void menuItemUserSettings_Click(object sender, EventArgs e)
    {
        var frmUserSetting = new FrmUserSetting();

        frmUserSetting.ShowDialog();
    }

    private void menuItemPrinter_Click(object sender, EventArgs e)
    {
        Open<FormPrinterSetup>(EnumPermType.PrinterSetup, "Printers");
    }

    private void menuItemGraphics_Click(object sender, EventArgs e)
    {
        var result = Open<FormGraphics>(EnumPermType.GraphicsEdit, "Graphics");

        if (result != DialogResult.OK)
        {
            return;
        }
        
        controlChart.InitializeLocalData();

        RefreshCurrentModule();
    }

    private void menuItemConfig_Click(object sender, EventArgs e)
    {
        // if(!Security.IsAuthorized(EnumPermType.ChooseDatabase)){
        // 	return;
        // }
        // SecurityLogs.MakeLogEntry(EnumPermType.ChooseDatabase,0,"");//make the entry before switching databases.
        // ChooseDatabaseInfo chooseDatabaseInfo=ChooseDatabaseInfo.GetChooseDatabaseInfoFromConfig();
        // ChooseDatabaseInfo.UpdateChooseDatabaseInfoFromCurrentConnection(chooseDatabaseInfo);
        // chooseDatabaseInfo.IsAccessedFromMainMenu=true;
        // using FormChooseDatabase formChooseDatabase=new FormChooseDatabase(chooseDatabaseInfo);
        // if(formChooseDatabase.ShowDialog()!=DialogResult.OK) {
        // 	return;
        // }
        // PatNumCur=0;
        // if(!PrefsStartup()){
        // 	return;
        // }
        // RefreshLocalData(InvalidType.AllLocal);
        // UnselectActive();//Deselect the currently Visible module.
        // AllNeutral();//Set all modules invisible.
        // //The following 2 methods mimic RefreshCurrentModule()
        // SetModuleSelected(true);//Reselect the previously selected module, UI is reset to same state as when program starts.
        // userControlTasks1.RefreshPatTicketsIfNeeded();
        // FillPatientButton(null);
    }

    private void menuItemPreferences_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formPreferences = new FormPreferences();
        
        formPreferences.ShowDialog();

        FillPatientButton(Patients.GetPat(PatNumCur));

        RefreshCurrentModule(true);

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Preferences");
    }

    private static void Open<TForm>() where TForm : Form, new()
    {
        using var form = new TForm();

        form.ShowDialog();
    }

    private static DialogResult Open<TForm>(EnumPermType permission, string message) where TForm : Form, new()
    {
        if (!Security.IsAuthorized(permission))
        {
            return DialogResult.Abort;
        }

        using var form = new TForm();

        form.ShowDialog();

        SecurityLogs.MakeLogEntry(permission, 0, message);
        
        return form.DialogResult;
    }

    private void menuItemApptViews_Click(object sender, EventArgs e)
    {
        Open<FormApptViews>(EnumPermType.Setup, "Appointment Views");

        RefreshCurrentModule(true);
    }

    private void menuItemDataPath_Click(object sender, EventArgs e)
    {
        Open<FormPath>();

        RefreshCurrentModule();
    }

    private void menuItemDefinitions_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        using var formDefinitions = new FormDefinitions(DefCat.AccountColors);

        formDefinitions.ShowDialog();

        RefreshCurrentModule(true);

        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, "Definitions");
    }

    private void menuItemDisplayFields_Click(object sender, EventArgs e)
    {
        Open<FormDisplayFieldCategories>(EnumPermType.Setup, "Display Fields");

        RefreshCurrentModule(true);
    }

    private void menuItemFeeScheds_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.FeeSchedEdit))
        {
            return;
        }

        using var formFeeScheds = new FormFeeScheds(false);

        formFeeScheds.ShowDialog();
    }

    private void menuFeeSchedGroups_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.FeeSchedEdit))
        {
            return;
        }

        if (Security.CurUser.ClinicIsRestricted)
        {
            MsgBox.Show(this, "You are restricted from accessing certain clinics. Only user without clinic restrictions can edit Fee Schedule Groups.");

            return;
        }

        using var formFeeSchedGroups = new FormFeeSchedGroups();

        formFeeSchedGroups.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.FeeSchedEdit, 0, "Fee Schedule Groups");
    }

    private void menuItemHL7_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formHL7Defs = new FormHL7Defs();
        
        formHL7Defs.PatNumCur = PatNumCur;
        formHL7Defs.ShowDialog();
        
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "HL7");
    }

    private void LaunchPrerencesWithMenuItem(int selectedTreeNode)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formPreferences = new FormPreferences();
        
        formPreferences.SelectedNode = selectedTreeNode;
        
        if (formPreferences.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        FillPatientButton(Patients.GetPat(PatNumCur));
        
        RefreshCurrentModule(true);
        
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Preferences");
    }

    private void menuItemOperatories_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formOperatories = new FormOperatories();
        
        formOperatories.ControlApptRef = controlAppt;
        formOperatories.ShowDialog();
        
        if (formOperatories.ListAppointmentsConflicting.Count > 0)
        {
            var formApptConflicts = new FormApptConflicts(formOperatories.ListAppointmentsConflicting);
            
            formApptConflicts.Show();
            formApptConflicts.BringToFront();
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Operatories");
    }

    private void menuItemPatFieldDefs_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formPatFieldDefs = new FormPatFieldDefs();
        
        formPatFieldDefs.ShowDialog();
        
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Patient Field Defs");
    }

    private void menuItemPractice_Click(object sender, EventArgs e)
    {
        var result = Open<FormPractice>(EnumPermType.Setup, "Practice Info");
        
        if (result != DialogResult.OK)
        {
            return;
        }

        moduleBar.RefreshButtons();
        
        RefreshCurrentModule();
    }

    private void menuItemProcedureButtons_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formProcButtons = new FormProcButtons();

        formProcButtons.Owner = this;
        formProcButtons.ShowDialog();

        SetModuleSelected();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Procedure Buttons");
    }

    private void menuItemLinks_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formProgramLinks = new FormProgramLinks();

        formProgramLinks.ShowDialog();

        controlChart.InitializeLocalData();

        RefreshMenuReports();

        if (PatNumCur > 0)
        {
            var patient = Patients.GetPat(PatNumCur);
            FillPatientButton(patient);
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Program Links");
    }

    private void menuItemAsapList_Click(object sender, EventArgs e)
    {
        Open<FormAsapSetup>(EnumPermType.Setup, "ASAP List Setup");
    }

    private void menuItemReports_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formReportSetup = new FormReportSetup(0, false);

        formReportSetup.ShowDialog();
    }

    private void menuItemSched_Click(object sender, EventArgs e)
    {
        using var formSchedule = new FormSchedule();

        formSchedule.ShowDialog();
    }

    private void menuItemSecurity_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SecurityAdmin))
        {
            return;
        }

        using var formSecurity = new FormSecurity();

        formSecurity.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, "Security Window");

        var clinicNumOld = Clinics.ClinicNum;
        if (Security.CurUser.ClinicIsRestricted)
        {
            Clinics.SetClinicNum(Security.CurUser.ClinicNum);
        }

        Text = PatientL.GetMainTitle(Patients.GetPat(PatNumCur), Clinics.ClinicNum);

        SetSmsNotificationText(doUseSignalInterval: clinicNumOld == Clinics.ClinicNum);

        RefreshMenuClinics();
    }

    private void menuItemSecurityAddUser_Click(object sender, EventArgs e)
    {
        var isAuthorizedAddNewUser = Security.IsAuthorized(EnumPermType.AddNewUser, true);
        var isAuthorizedSecurityAdmin = Security.IsAuthorized(EnumPermType.SecurityAdmin, true);
        if (!(isAuthorizedAddNewUser || isAuthorizedSecurityAdmin))
        {
            MsgBox.Show(this, "Not authorized to add a new user.");
            return;
        }

        if (PrefC.GetLong(PrefName.DefaultUserGroup) == 0)
        {
            if (isAuthorizedSecurityAdmin)
            {
                var msg = "Default user group is not set.  Would you like to set the default user group now?";
                if (MsgBox.Show(this, MsgBoxButtons.YesNo, msg, "Default user group"))
                {
                    using var formGlobalSecurity = new FormGlobalSecurity();

                    formGlobalSecurity.ShowDialog();
                }
            }
            else
            {
                //Using verbage similar to that found in the manual for describing how to navigate to a window in the program.
                var msg = "Default user group is not set.  A user with the SecurityAdmin permission must set a default user group.  "
                          + "To view the default user group, in the Main Menu, click Setup, Security, Security Settings, Global Security Settings.";
                MsgBox.Show(this, msg, "Default user group");
            }

            return;
        }

        using var formUserEdit = new FormUserEdit(new Userod(), true);
        formUserEdit.IsNew = true;
        formUserEdit.ShowDialog();
    }

    private void menuItemSecurityBadges_Click(object sender, EventArgs e)
    {
        //Check if user is authorized
        if (!Security.IsAuthorized(EnumPermType.BadgeIdEdit))
        {
            return;
        }

        //Allow selection of userod with a combobox
        var inputBoxParam = new InputBoxParam
        {
            InputBoxType_ = InputBoxType.ComboSelect, //Not multiselect
            LabelText = "Select a user."
        };
        var listUserodsAll = Userods.GetAll(); //Already orders by username
        var listUserods = listUserodsAll.FindAll(x => !x.IsHidden);
        inputBoxParam.ListSelections = listUserods.Select(x => x.UserName).ToList();
        inputBoxParam.SizeParam = new System.Windows.Size(width: 200, height: 20);
        var inputBox = new InputBox(inputBoxParam);
        inputBox.ShowDialog();
        if (inputBox.IsDialogCancel)
        {
            return;
        }

        var userodSelected = listUserods[inputBox.SelectedIndex];

        var frmBadgeEdit = new FrmBadgeEdit
        {
            UserodCur = userodSelected
        };

        frmBadgeEdit.ShowDialog();
    }

    private void MenuItemEasy_Click(object sender, EventArgs e)
    {
        Open<FormShowFeatures>(EnumPermType.ShowFeatures, "Show Features");
        
        controlAccount.LayoutToolBar();

        RefreshCurrentModule(true);
    }

    private void MenuItemTask_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formTaskPreferencesSetup = new FormTaskPreferences();
        
        if (formTaskPreferencesSetup.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Task");
    }

    private void menuItemQuickPasteNotes_Click(object sender, EventArgs e)
    {
        var frmQuickPaste = new FrmQuickPaste
        {
            QuickPasteType_ = EnumQuickPasteType.None
        };

        frmQuickPaste.ShowDialog();
    }

    private void menuItemProcCodes_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes(true);

        formProcCodes.ShowDialog();
    }

    private void menuItemClinics_Click(object sender, EventArgs e)
    {
        Open<FormClinics>(EnumPermType.ClinicEdit, "Clinics");
        
        if (Clinics.GetDesc(Clinics.ClinicNum) == "" && Clinics.ClinicNum != 0)
        {
            Clinics.SetClinicNum(Security.CurUser.ClinicNum);

            SetSmsNotificationText(doUseSignalInterval: true);

            Text = PatientL.GetMainTitle(Patients.GetPat(PatNumCur), Clinics.ClinicNum);
        }

        RefreshMenuClinics();

        var patient = Patients.GetPat(PatNumCur);

        Text = PatientL.GetMainTitle(patient, Clinics.ClinicNum);
    }

    private void menuItemCarriers_Click(object sender, EventArgs e)
    {
        Open<FormCarriers>();
        
        RefreshCurrentModule();
    }

    private void menuItemInsPlans_Click(object sender, EventArgs e)
    {
        Open<FormInsPlans>();

        RefreshCurrentModule(true);
    }

    private void menuItemLabCases_Click(object sender, EventArgs e)
    {
        using var formLabCases = new FormLabCases();

        formLabCases.ShowDialog();

        if (formLabCases.GoToAptNum == 0)
        {
            return;
        }

        var appointment = Appointments.GetOneApt(formLabCases.GoToAptNum);
        var patient = Patients.GetPat(appointment.PatNum);

        GlobalFormOpenDental.PatientSelected(patient, false);

        GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, dateSelected: appointment.AptDateTime, selectedAptNum: appointment.AptNum);
    }

    private void menuItemProviders_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ProviderAdd, suppressMessage: true) && !Security.IsAuthorized(EnumPermType.ProviderEdit, suppressMessage: true) && !Security.IsAuthorized(EnumPermType.ProviderAlphabetize, suppressMessage: true))
        {
            {
                MsgBox.Show(
                    "Not authorized.\r\n" +
                    "A user with the SecurityAdmin permission must grant you access for:\r\n" +
                    GroupPermissions.GetDesc(EnumPermType.ProviderAdd) + " or " +
                    GroupPermissions.GetDesc(EnumPermType.ProviderEdit) + " or " +
                    GroupPermissions.GetDesc(EnumPermType.ProviderAlphabetize));
                return;
            }
        }
        
        Open<FormProviderSetup>();

        SecurityLogs.MakeLogEntry(EnumPermType.ProviderEdit, 0, "Provider Setup", 0, SecurityLogs.LogSource, DateTime.MinValue);
    }

    private void menuItemReferrals_Click(object sender, EventArgs e)
    {
        var frmReferralSelect = new FrmReferralSelect();

        frmReferralSelect.ShowDialog();
    }

    private void menuItemSites_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        var frmSites = new FrmSites();

        frmSites.ShowDialog();

        RefreshCurrentModule();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Sites");
    }

    private void menuItemStateAbbrs_Click(object sender, EventArgs e)
    {
        using var formStateAbbrs = new FormStateAbbrs();

        formStateAbbrs.ShowDialog();

        RefreshCurrentModule();
    }

    private void menuItemZipCodes_Click(object sender, EventArgs e)
    {
        var frmZipCodes = new FrmZipCodes();

        frmZipCodes.ShowDialog();
    }

    private void menuItemReportsStandard_Click(object sender, EventArgs e)
    {
        using var formReportsMore = new FormReportsMore();
        formReportsMore.DateSelected = controlAppt.GetDateSelected();
        formReportsMore.ShowDialog();
        NonModalReportSelectionHelper(formReportsMore.ReportNonModalSelection_);
    }

    private void menuItemReportsUserQuery_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.UserQuery))
        {
            return;
        }

        if (Security.IsAuthorized(EnumPermType.UserQueryAdmin, true))
        {
            SecurityLogs.MakeLogEntry(EnumPermType.UserQuery, 0, "User query form accessed.");
            
            if (_formUserQuery == null || _formUserQuery.IsDisposed)
            {
                _formUserQuery = new FormUserQuery(null);
                _formUserQuery.FormClosed += (_, _) => { _formUserQuery = null; };
                _formUserQuery.Show();
            }

            if (_formUserQuery.WindowState == FormWindowState.Minimized)
            {
                _formUserQuery.WindowState = FormWindowState.Normal;
            }

            _formUserQuery.BringToFront();
        }
        else
        {
            using var formQueryFavorites = new FormQueryFavorites();

            if (formQueryFavorites.ShowDialog() == DialogResult.OK)
            {
                ExecuteQueryFavorite(formQueryFavorites.UserQueryCur);
            }
        }
    }

    private void menuItemReportsFilteredClick_Click(object sender, EventArgs e)
    {
        using var formReportsFiltered = new FormReportsFiltered();
        
        if (formReportsFiltered.ShowDialog() == DialogResult.OK)
        {
            StandardReport_Click(formReportsFiltered.DisplayReportCur);
        }
    }

    private void menuItemReportsQueryFavorites_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.UserQuery))
        {
            return;
        }

        using var formQueryFavorites = new FormQueryFavorites();
        
        if (formQueryFavorites.ShowDialog() == DialogResult.OK)
        {
            ExecuteQueryFavorite(formQueryFavorites.UserQueryCur);
        }
    }

    private void menuItemReportsUnfinalizedPay_Click(object sender, EventArgs e)
    {
        var displayReportUnfinalizedPay = DisplayReports.GetWhere(x => x.InternalName == DisplayReports.ReportNames.UnfinalizedInsPay).FirstOrDefault();
        if (displayReportUnfinalizedPay.IsHidden)
        {
            MsgBox.Show(this, "The administrator has hidden this report.");
            return;
        }

        if (!GroupPermissions.HasReportPermission(DisplayReports.ReportNames.UnfinalizedInsPay, Security.CurUser))
        {
            MsgBox.Show(this, "You do not have permission to run this report.");
            return;
        }

        var formRpUnfinalizedInsPay = new FormRpUnfinalizedInsPay();
        
        formRpUnfinalizedInsPay.Show();
    }

    private void UpdateUnfinalizedPayCount(List<Signalod> listSignalods)
    {
        if (listSignalods.Count == 0)
        {
            _menuItemUnfinalizedPay.Text = "Unfinalized Payments";
            return;
        }

        var signalod = listSignalods.OrderByDescending(x => x.SigDateTime).First();
        _menuItemUnfinalizedPay.Text = "Unfinalized Payments: " + signalod.MsgValue;
    }

    private void RefreshMenuReports()
    {
        _menuItemUserQuery.Available = Security.IsAuthorized(EnumPermType.UserQueryAdmin, true);
        _menuItemQueryFavorites.Available = Security.IsAuthorized(EnumPermType.UserQuery, true);
        
        var separatorIndex = -1;
        for (var i = 0; i < _menuItemReports.DropDown.Items.Count; i++)
        {
            if (_menuItemReports.DropDown.Items[i].Text == "-")
            {
                separatorIndex = i;
            }
        }

        if (separatorIndex != -1)
        {
            for (var i = _menuItemReports.DropDown.Items.Count - 1; i >= separatorIndex; i--)
            {
                _menuItemReports.DropDown.Items.RemoveAt(i);
            }
        }

        var toolButItems = ToolButItems.GetForToolBar(EnumToolBar.ReportsMenu);

        toolButItems.RemoveAll(x => ProgramProperties.GetPropForProgByDesc(x.ProgramNum, ProgramProperties.PropertyDescs.ClinicHideButton, Clinics.ClinicNum) != null);

        if (toolButItems.Count == 0)
        {
            var menuStripOD = MenuStripOD.GetMenuStripOD(_menuItemReports);
            
            menuStripOD?.LayoutItems();

            return;
        }

        _menuItemReports.AddSeparator();
        
        var newSeparatorIndex = _menuItemReports.DropDown.Items.Count - 1;
        
        _menuItemReports.DropDown.Items[newSeparatorIndex].Text = "-";
        
        toolButItems.Sort(ToolButItem.Compare);
        
        foreach (var toolButItem in toolButItems)
        {
            var menuItem = new MenuItemOD(toolButItem.ButtonText, menuReportLink_Click);
            
            menuItem.Tag = toolButItem;
            
            _menuItemReports.Add(menuItem);
        }
    }

    private void menuReportLink_Click(object sender, EventArgs e)
    {
        var menuItem = (MenuItemOD) sender;
        var toolButItem = (ToolButItem) menuItem.Tag;
        WpfControls.ProgramL.Execute(toolButItem.ProgramNum, Patients.GetPat(PatNumCur));
    }

    private void StandardReport_Click(DisplayReport displayReport)
    {
        var reportNonModalSelection = FormReportsMore.OpenReportHelper(displayReport, controlAppt.GetDateSelected(), doValidatePerm: false);
        
        NonModalReportSelectionHelper(reportNonModalSelection);
    }

    private void NonModalReportSelectionHelper(ReportNonModalSelection reportNonModalSelection)
    {
        switch (reportNonModalSelection)
        {
            case ReportNonModalSelection.TreatmentFinder:
                var formRpTreatmentFinder = new FormRpTreatmentFinder();
                formRpTreatmentFinder.Show();
                break;
            case ReportNonModalSelection.OutstandingIns:
                var formRpOutstandingIns = new FormRpOutstandingIns();
                formRpOutstandingIns.Show();
                break;
            case ReportNonModalSelection.UnfinalizedInsPay:
                var formRpUnfinalizedInsPay = new FormRpUnfinalizedInsPay();
                formRpUnfinalizedInsPay.Show();
                break;
            case ReportNonModalSelection.UnsentClaim:
                var formRpClaimNotSent = new FormRpClaimNotSent();
                formRpClaimNotSent.Show();
                break;
            case ReportNonModalSelection.CustomAging:
                var formRpCustomAging = new FormRpCustomAging();
                formRpCustomAging.Show();
                break;
            case ReportNonModalSelection.IncompleteProcNotes:
                var formRpProcNote = new FormRpProcNote(this);
                formRpProcNote.Show();
                break;
            case ReportNonModalSelection.ProcNotBilledIns:
                var formRpProcNotBilledIns = new FormRpProcNotBilledIns(this);

                formRpProcNotBilledIns.OnPostClaimCreation += () => controlManage.TryRefreshFormClaimSend();
                formRpProcNotBilledIns.FormClosed += (_, _) => { ODEvent.Fired -= formProcNotBilled_GoToChanged; };
                
                ODEvent.Fired += formProcNotBilled_GoToChanged;
                
                formRpProcNotBilledIns.Show();
                formRpProcNotBilledIns.BringToFront();
                break;
            
            case ReportNonModalSelection.ODProcsOverpaid:
                var formRpProcOverpaid = new FormRpProcOverpaid();
                formRpProcOverpaid.Show();
                break;
            
            case ReportNonModalSelection.DPPOvercharged:
                if (_formRpDppOvercharged == null || _formRpDppOvercharged.IsDisposed)
                {
                    _formRpDppOvercharged = new FormRpDPPOvercharged();
                }

                _formRpDppOvercharged.Show();
                if (_formRpDppOvercharged.WindowState == FormWindowState.Minimized)
                {
                    _formRpDppOvercharged.WindowState = FormWindowState.Normal;
                }

                _formRpDppOvercharged.BringToFront();
                break;
            case ReportNonModalSelection.PatPortionUncollected:
                var formRpPatPortionUncollected = new FormRpPatPortionUncollected();
                formRpPatPortionUncollected.Show();
                break;
            case ReportNonModalSelection.EraAutoProcessed:
                var formRpEraAutoProcessed = new FormRpEraAutoProcessed();
                formRpEraAutoProcessed.Show();
                break;
            case ReportNonModalSelection.ProceduresIndividual:
                var formRpProcSheet = new FormRpProcSheet();
                formRpProcSheet.Show();
                break;
            case ReportNonModalSelection.None:
            default:
                //Do nothing.
                break;
        }
    }

    private void formProcNotBilled_GoToChanged(ODEventArgs e)
    {
        if (e.EventType != ODEventType.FormProcNotBilled_GoTo)
        {
            return;
        }

        var patient = Patients.GetPat((long) e.Tag);
        GlobalFormOpenDental.PatientSelected(patient, false);
        GlobalFormOpenDental.GoToModule(EnumModuleType.Account, claimNum: (long) e.Tag);
    }

    private void ExecuteQueryFavorite(UserQuery userQuery, bool doRunPrompt = false)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.UserQuery, 0, "User query form accessed.");
        
        if (doRunPrompt && userQuery.IsPromptSetup && UserQueries.ParseSetStatements(userQuery.QueryText).Count > 0)
        {
            using var formQueryParser = new FormQueryParser(userQuery);
            
            if (formQueryParser.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        if (_formUserQuery != null)
        {
            _formUserQuery.textQuery.Text = userQuery.QueryText;
            _formUserQuery.textTitle.Text = userQuery.Description;
            _formUserQuery.SetQuery(userQuery.QueryText);
            _formUserQuery.SubmitQueryThreaded();
            _formUserQuery.BringToFront();
            return;
        }

        _formUserQuery = new FormUserQuery(userQuery.QueryText, true, userQuery);
        _formUserQuery.FormClosed += (_, _) => { _formUserQuery = null; };
        _formUserQuery.textQuery.Text = userQuery.QueryText;
        _formUserQuery.textTitle.Text = userQuery.Description;
        _formUserQuery.Show();
    }

    private static void MenuItemPrintScreen_Click(object sender, EventArgs e)
    {
        try
        {
            using var formPrntScrn = new FormPrntScrn();

            formPrntScrn.ShowDialog();
        }
        catch (Exception ex)
        {
            MsgBox.Show(ex.Message);
        }
    }

    private void menuItemScreenSnip_Click(object sender, EventArgs e)
    {
        if (!FormClaimAttachment.StartSnipAndSketchOrSnippingTool())
        {
            MsgBox.Show(this, "Neither the Snip & Sketch tool nor the Snipping Tool could be launched.");
        }
    }

    private void menuItemDuplicateBlockouts_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formBlockoutDuplicatesFix = new FormBlockoutDuplicatesFix();
        
        Cursor = Cursors.WaitCursor;
        
        formBlockoutDuplicatesFix.ShowDialog();
        
        Cursor = Cursors.Default;
    }

    private void menuItemCreateAtoZFolders_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        var frmAtoZFoldersCreate = new FrmAtoZFoldersCreate();

        frmAtoZFoldersCreate.ShowDialog();
    }

    private void menuItemMergeDPs_Click(object sender, EventArgs e)
    {
        using var formDiscountPlanMerge = new FormDiscountPlanMerge();

        formDiscountPlanMerge.ShowDialog();
    }

    private void menuItemMergeBillingType_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formBillingTypeMerge = new FormBillingTypeMerge();

        formBillingTypeMerge.ShowDialog();
    }

    private void menuItemMergeImageCat_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formImageCatMerge = new FormImageCatMerge();

        formImageCatMerge.ShowDialog();
    }

    private void menuItemMergePatients_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.PatientMerge))
        {
            return;
        }

        using var formPatientMerge = new FormPatientMerge();

        formPatientMerge.ShowDialog();
    }

    private void menuItemMergeReferrals_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ReferralMerge))
        {
            return;
        }

        using var formReferralMerge = new FormReferralMerge();

        formReferralMerge.ShowDialog();
    }

    private void menuItemMergeProviders_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ProviderMerge))
        {
            return;
        }

        using var formProviderMerge = new FormProviderMerge();

        formProviderMerge.ShowDialog();
    }

    private void menuItemMoveSubscribers_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPlanChangeSubsc))
        {
            return;
        }

        using var formSubscriberMove = new FormSubscriberMove();

        formSubscriberMove.ShowDialog();
    }

    private void menuPatientStatusSetter_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SecurityAdmin))
        {
            return;
        }

        using var formPatientStatusTool = new FormPatientStatusTool();

        formPatientStatusTool.ShowDialog();
    }

    private void menuItemProcLockTool_Click(object sender, EventArgs e)
    {
        using var formProcLockTool = new FormProcLockTool();

        formProcLockTool.ShowDialog();
    }

    private void menuItemSetupWizard_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SetupWizard))
        {
            return;
        }

        using var formSetupWizard = new FormSetupWizard();

        formSetupWizard.ShowDialog();
    }

    private void menuItemShutdown_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formShutdown = new FormShutdown();

        if (formShutdown.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Signalods.DateTHighPrioritySignalLastRefreshed = MiscData.GetNowDateTime().AddSeconds(5);

        Signalods.Insert(new Signalod
        {
            IType = InvalidType.ShutDownNow
        });

        Computers.ClearAllHeartBeats(Environment.MachineName);

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Shutdown all workstations.");
    }

    private void menuTelephone_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formTelephone = new FormTelephone();

        formTelephone.ShowDialog();
    }

    private void menuItemTestLatency_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formTestLatency = new FormTestLatency();

        formTestLatency.ShowDialog();
    }

    private void menuItemAuditTrail_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.AuditTrail))
        {
            return;
        }

        using var formAudit = new FormAudit();

        formAudit.CurPatNum = PatNumCur;
        
        formAudit.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.AuditTrail, 0, "Audit Trail");
    }

    private void menuItemFinanceCharge_Click(object sender, EventArgs e)
    {
        Open<FormFinanceCharges>(EnumPermType.Setup, "Run Finance Charges");
    }

    private void menuItemCCRecurring_Click(object sender, EventArgs e)
    {
        if (_formCreditRecurringCharges == null || _formCreditRecurringCharges.IsDisposed)
        {
            _formCreditRecurringCharges = new FormCreditRecurringCharges();
        }

        Cursor = Cursors.WaitCursor;

        _formCreditRecurringCharges.Show();

        Cursor = Cursors.Default;

        if (_formCreditRecurringCharges.WindowState == FormWindowState.Minimized)
        {
            _formCreditRecurringCharges.WindowState = FormWindowState.Normal;
        }

        _formCreditRecurringCharges.BringToFront();
    }

    private void menuItemCertifications_Click(object sender, EventArgs e)
    {
        if (_formCertifications == null || _formCertifications.IsDisposed)
        {
            _formCertifications = new FormCertifications();
        }

        _formCertifications.Show();
        if (_formCertifications.WindowState == FormWindowState.Minimized)
        {
            _formCertifications.WindowState = FormWindowState.Normal;
        }

        _formCertifications.BringToFront();
    }

    private void menuItemTerminal_Click(object sender, EventArgs e)
    {
        if (PrefC.GetLong(PrefName.ProcessSigsIntervalInSecs) == 0)
        {
            MsgBox.Show(this, "Cannot open terminal unless process signal interval is set. To set it, go to Setup > Miscellaneous.");
            return;
        }

        using var formTerminal = new FormTerminal();

        formTerminal.ShowDialog();

        Application.Exit();
    }

    private void menuItemTerminalManager_Click(object sender, EventArgs e)
    {
        if (_formTerminalManager == null || _formTerminalManager.IsDisposed)
        {
            _formTerminalManager = new FormTerminalManager(isSetupMode: true);
        }

        _formTerminalManager.Show();
        _formTerminalManager.BringToFront();
    }

    private void menuItemLateCharges_Click(object sender, EventArgs e)
    {
        Open<FormLateCharges>(EnumPermType.Setup, "Late Charges window");
    }

    private void menuItemOnlinePayments_Click(object sender, EventArgs e)
    {
        Open<FormOnlinePayments>();
    }

    private void menuItemRepeatingCharges_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.RepeatChargeTool))
        {
            return;
        }

        using var formRepeatChargesUpdate = new FormRepeatChargesUpdate();
        
        formRepeatChargesUpdate.ShowDialog();
    }

    private void menuItemXWebTrans_Click(object sender, EventArgs e)
    {
        if (_formXWebTransactions == null || _formXWebTransactions.IsDisposed)
        {
            _formXWebTransactions = new FormXWebTransactions();
            _formXWebTransactions.FormClosed += (_, _) => { _formXWebTransactions = null; };
            _formXWebTransactions.Show();
        }

        if (_formXWebTransactions.WindowState == FormWindowState.Minimized)
        {
            _formXWebTransactions.WindowState = FormWindowState.Normal;
        }

        _formXWebTransactions.BringToFront();
    }

    private void menuItemZoom_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Zoom))
        {
            return;
        }

        var sizeF96Old = new SizeF((Width), (Height));
        var frmZoom = new FrmZoom();
        frmZoom.ShowDialog();
        if (!frmZoom.IsDialogOK)
        {
            return;
        }

        //Don't send WndProc for WM_DPICHANGED because it didn't change.
        if (WindowState == FormWindowState.Maximized)
        {
            LayoutControls();
        }
        else
        {
            Size = new Size((int) sizeF96Old.Width, (int) sizeF96Old.Height);
            //This triggers FormODBase.OnResize event handler, which then calls LayoutManager.LayoutFormBoundsAndFonts();
        }

        if (controlImages is {Visible: true})
        {
            controlImages.ModuleSelected(PatNumCur); //to reset the font for any floaters
        }
    }

    public static void S_TaskNumLoad(long taskNum)
    {
        var task = Tasks.GetOne(taskNum);
        if (task is null)
        {
            MsgBox.Show("FormOpenDental", "Task does not exist.");
            return;
        }

        var formTaskEdit = new FormTaskEdit(task);
        
        formTaskEdit.Show();
    }

    private void menuItemAutoClosePayPlans_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        var frmPayPlansClose = new FrmPayPlansClose();
        
        frmPayPlansClose.ShowDialog();
    }

    private void menuItemOrthoAuto_Click(object sender, EventArgs e)
    {
        using var formOrthoAutoClaims = new FormOrthoAutoClaims();
        
        formOrthoAutoClaims.ShowDialog();
        
        if (controlAccount.Visible)
        {
            controlAccount.ModuleSelected(PatNumCur);
        }
    }

    private void menuItemAlerts_Click(object sender, EventArgs e)
    {
        _formAlerts = new FormAlerts(_alertItems, _alertItemReads);
        _formAlerts.ClickAlert += FormAlerts_Click;
        _formAlerts.ShowDialog();

        if (_formAlerts.SelectedAlertType != ActionType.OpenForm)
        {
            return;
        }

        _formAlerts.Dispose();

        var alertItem = _formAlerts.SelectedAlertItem;
        var listAlertItemNums = (List<long>) alertItem.TagOD;
        AlertReadsHelper(listAlertItemNums);
        CheckAlerts();

        switch (alertItem.FormToOpen)
        {
            case FormType.FormOnlinePayments:
                var formOnlinePayments = new FormOnlinePayments();
                formOnlinePayments.Show();
                formOnlinePayments.FormClosed += AlertFormClosingHelper;
                break;
            case FormType.FormRadOrderList:
                var listFormRadOrderLists = Application.OpenForms.OfType<FormRadOrderList>().ToList();
                if (listFormRadOrderLists.Count > 0)
                {
                    listFormRadOrderLists[0].RefreshRadOrdersForUser(Security.CurUser);
                    listFormRadOrderLists[0].BringToFront();
                }
                else
                {
                    var formRadOrderList = new FormRadOrderList(Security.CurUser);
                    formRadOrderList.Show();
                    formRadOrderList.FormClosed += AlertFormClosingHelper;
                }

                break;
            case FormType.FormApptEdit:
                var appointment = Appointments.GetOneApt(alertItem.FKey);
                var patient = Patients.GetPat(appointment.PatNum);
                GlobalFormOpenDental.PatientSelected(patient, false);
                var formApptEdit = new FormApptEdit(appointment.AptNum); //Dispose below due to local variable.
                formApptEdit.ShowDialog();
                formApptEdit.Dispose();
                break;
            case FormType.FormPatientEdit:
                if (!Security.IsAuthorized(EnumPermType.PatientEdit))
                {
                    return;
                }

                patient = Patients.GetPat(alertItem.FKey);
                var family = Patients.GetFamily(patient.PatNum);
                GlobalFormOpenDental.PatientSelected(patient, false);
                using (var formPatientEdit = new FormPatientEdit())
                {
                    formPatientEdit.Patient = patient;
                    formPatientEdit.Family = family;
                    formPatientEdit.ShowDialog();
                }

                break;
            case FormType.FormEmailInbox:
                var formEmailInbox = new FormEmailInbox("WebMail");
                formEmailInbox.FormClosed += AlertFormClosingHelper;
                formEmailInbox.Show();
                break;

            case FormType.FormEmailAddresses:
                using (var formEmailAddresses = new FormEmailAddresses())
                {
                    formEmailAddresses.FormClosed += AlertFormClosingHelper;
                    formEmailAddresses.ShowDialog();
                }

                break;

            case FormType.FormModuleSetup:
                LaunchPrerencesWithMenuItem((int) alertItem.FKey);
                break;
        }
    }

    public void FormAlerts_Click(object sender, EventArgs e)
    {
        var alertItem = _formAlerts.SelectedAlertItem;
        var actionType = _formAlerts.SelectedAlertType;

        var listAlertItemNums = (List<long>) alertItem.TagOD;
        if (actionType == ActionType.MarkAsRead)
        {
            AlertReadsHelper(listAlertItemNums);
            
            CheckAlerts();
            
            _formAlerts.ListAlertReads = _alertItemReads;
            _formAlerts.FillGrid();
            
            return;
        }

        if (actionType == ActionType.Delete)
        {
            AlertItems.Delete(listAlertItemNums);
            
            CheckAlerts();
            
            _formAlerts.ListAlertItems = _alertItems;
            _formAlerts.ListAlertReads = _alertItemReads;
            _formAlerts.FillGrid();
            return;
        }

        if (actionType == ActionType.ShowItemValue)
        {
            AlertReadsHelper(listAlertItemNums);
            
            CheckAlerts();
            
            _formAlerts.ListAlertReads = _alertItemReads;
            _formAlerts.FillGrid();
            
            var msgBoxCopyPaste = new MsgBoxCopyPaste($"{alertItem.Description}\r\n\r\n{alertItem.ItemValue}");
            
            msgBoxCopyPaste.Show();
        }
    }

    private static void AlertFormClosingHelper(object sender, FormClosedEventArgs e)
    {
        DataValid.SetInvalid(InvalidType.AlertItems);
    }

    private void AlertReadsHelper(List<long> alertItemNums)
    {
        alertItemNums.RemoveAll(x => _alertItemReads.Exists(y => y.AlertItemNum == x));
        
        foreach (var alertItemNum in alertItemNums)
        {
            AlertReads.Insert(new AlertRead(alertItemNum, Security.CurUser.UserNum));
        }
    }

    private void MenuItemRemote_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("https://www.opendental.com/contact.html");
        }
        catch (Exception)
        {
            ShowError(
                "Could not find https://www.opendental.com/contact.html\r\n" +
                "Please set up a default web browser.");
        }
    }

    private void MenuItemHelpWindows_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("Help.chm");
        }
        catch
        {
            ShowError("Could not find file.");
        }
    }

    private void MenuItemHelpContents_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("https://www.opendental.com/manual/manual.html");
        }
        catch
        {
            ShowError("Could not find file.");
        }
    }

    private void MenuItemHelpIndex_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("https://www.opendental.com/site/searchsite.html");
        }
        catch
        {
            ShowError("Could not find file.");
        }
    }

    private void MenuItemWebinar_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("https://opendental.com/webinars/webinars.html");
        }
        catch
        {
            ShowError("Could not open page.");
        }
    }

    private static void MenuItemQueryMonitor_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.QueryMonitor))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.QueryMonitor, 0, "Query Monitor opened.");

        new FormQueryMonitor().Show();
    }

    private void LogOnOpenDentalUser(string user, string password, string domainUserFromCmd)
    {
        if (Security.CurUser != null)
        {
            CheckForPasswordReset();

            Security.IsUserLoggedIn = true;
            SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged on.");
            return;
        }

        if (user != "" && password != "")
        {
            try
            {
                Security.CurUser = Userods.CheckUserAndPassword(user, password, false);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);

                Application.Exit();
                return;
            }

            SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged on via command line.");
        }

        if (Security.CurUser == null)
        {
            if (!Userods.HasSecurityAdminUserNoCache())
            {
                ShowError("There are no users with the SecurityAdmin permission. Call support.");
                
                Application.Exit();
                
                return;
            }

            var userNumFirstAdminNoPass = Userods.GetFirstSecurityAdminUserNumNoPasswordNoCache();
            if (userNumFirstAdminNoPass > 0)
            {
                Security.CurUser = Userods.GetUserNoCache(userNumFirstAdminNoPass);

                CheckForPasswordReset();

                SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged on.");
            }
            else
            {
                ShowLogOn();
            }
        }

        Security.IsUserLoggedIn = true;
    }

    private void ShowLogOn(bool doClearCaches = false)
    {
        Userods.SetIsCacheAllowed(false);

        using var formLogOn = new FormLogOn(doRefreshSecurityCache: false, doClearCaches: doClearCaches);

        formLogOn.ShowDialog(this);

        if (formLogOn.DialogResult != DialogResult.OK)
        {
            CloseOpenForms(isForceClose: true);
            
            Cursor = Cursors.Default;
            
            Application.Exit();
        }

        CheckForPasswordReset();

        Userods.SetIsCacheAllowed(true);

        if (formLogOn.RefreshSecurityCache)
        {
            DataValid.SetInvalid(InvalidType.Security);
        }
    }

    private void CheckForPasswordReset()
    {
        if (Security.CurUser == null)
        {
            return;
        }

        try
        {
            if (Security.CurUser.IsPasswordResetRequired)
            {
                using var formUserPassword = new FormUserPassword(false, Security.CurUser.UserName, isPasswordReset: true);

                if (formUserPassword.ShowDialog() != DialogResult.OK)
                {
                    Cursor = Cursors.Default;
                    Application.Exit();
                }

                var isPasswordStrong = formUserPassword.IsPasswordStrong;
                try
                {
                    Security.CurUser.IsPasswordResetRequired = false;
                    Userods.Update(Security.CurUser);
                    Userods.UpdatePassword(Security.CurUser, formUserPassword.PasswordContainer_, isPasswordStrong);
                    Security.PasswordTyped = formUserPassword.PasswordTyped;
                    Security.CurUser = Userods.GetUserNoCache(Security.CurUser.UserNum);
                }
                catch (Exception ex)
                {
                    ODMessageBox.Show(ex.Message);
                }
            }
        }
        finally
        {
            try
            {
                Security.CurUser.DateTLastLogin = DateTime.Now;

                Userods.Update(Security.CurUser);
            }
            catch (Exception ex)
            {
                ODMessageBox.Show(ex.Message);
            }
        }

        DataValid.SetInvalid(InvalidType.Security);
    }

    private List<Form> GetOpenForms()
    {
        if (InvokeRequired)
        {
            return (List<Form>) Invoke(new Func<List<Form>>(GetOpenForms));
        }

        var openForms = new List<Form>();
        for (var f = Application.OpenForms.Count - 1; f >= 0; f--)
        {
            var formOpen = Application.OpenForms[f];
            if (openForms.Contains(formOpen))
            {
                continue;
            }

            if (formOpen == this)
            {
                continue;
            }

            if (formOpen.Name == "FormLogOn")
            {
                continue;
            }

            openForms.Add(Application.OpenForms[f]);
        }

        return openForms;
    }

    private bool SaveWork(bool isForceClose)
    {
        if (InvokeRequired)
        {
            return (bool) Invoke(new Func<bool>(() => SaveWork(isForceClose)));
        }

        var forms = GetOpenForms();
        foreach (var form in forms)
        {
            if (form.Name != "FormWikiEdit")
            {
                continue;
            }

            if (isForceClose)
            {
                continue;
            }

            if (!ConfirmOk("You are currently editing a wiki page and it will be saved as a draft. Continue?"))
            {
                return false;
            }
        }

        ODEvent.Fire(ODEventType.Shutdown, isForceClose);

        foreach (var form in forms)
        {
            switch (form.Name)
            {
                case "FormWikiEdit":
                    ODEvent.Fire(ODEventType.WikiSave);
                    break;

                case "FormCommItem":
                    ODEvent.Fire(ODEventType.CommItemSave, "ShutdownAllWorkstations");
                    break;

                case "FormEmailMessageEdit":
                    ODEvent.Fire(ODEventType.EmailSave);
                    break;
            }
        }

        return true;
    }

    public bool CloseOpenForms(bool isForceClose, bool isSnip = false)
    {
        if (!SaveWork(isForceClose))
        {
            return false;
        }

        var formsToClose = GetOpenForms();

        controlImages?.InvokeIfRequired(() => controlImages.CloseFloaters());

        while (formsToClose.Count > 0)
        {
            var formToClose = formsToClose[0];
            if (isSnip)
            {
                if (!formToClose.Modal)
                {
                    formsToClose.Remove(formToClose);
                    continue;
                }
            }

            var hasShown = false;
            while (!hasShown)
            {
                hasShown = true;
                if (formToClose is FormODBase @base)
                {
                    hasShown = @base.HasShown;
                }
                else if (formToClose.GetType().GetProperty("HasShown") != null)
                {
                    hasShown = (bool) formToClose.GetType().GetProperty("HasShown").GetValue(formToClose);
                }

                if (!hasShown)
                {
                    Thread.Sleep(100);
                }
            }

            if (isForceClose)
            {
                var threadCloseForm = new ODThread(_ =>
                {
                    if (!IsDisposedOrClosed(formToClose))
                    {
                        formToClose.Invoke(formToClose.Close);
                    }
                })
                {
                    Name = "ForceCloseForm"
                };

                var hasError = false;

                threadCloseForm.AddExceptionHandler(ex =>
                {
                    hasError = true;
                    ODException.SwallowAnyException(() =>
                    {
                        //A FormClosing() or FormClosed() event caused an exception.  Try to submit the exception so that we are made aware.
                        BugSubmissions.SubmitException(new ODException(
                                "Form failed to close when force closing.\r\n"
                                + "FormName: " + formToClose.Name + "\r\n"
                                + "FormType: " + formToClose.GetType().FullName + "\r\n"
                                + "FormIsODForm: " + (formToClose is FormODBase ? "Yes" : "No") + "\r\n"
                                + "FormIsDiposed: " + (IsDisposedOrClosed(formToClose) ? "Yes" : "No") + "\r\n"
                                , "", ex),
                            threadCloseForm.Name);
                    });
                });

                threadCloseForm.Start();
                threadCloseForm.Join(1000);

                Invoke(Application.DoEvents);
                if (hasError || !IsDisposedOrClosed(formToClose))
                {
                    formToClose.Invoke(formToClose.Dispose);
                }

                var formsNew = GetOpenForms();
                foreach (var form in formsToClose)
                {
                    formsNew.Remove(form);
                }

                foreach (var form in formsNew)
                {
                    formsToClose.Insert(0, form);
                }
            }
            else
            {
                formToClose.InvokeIfRequired(() => formToClose.Close());

                Application.DoEvents();
                if (!IsDisposedOrClosed(formToClose))
                {
                    return false;
                }
            }

            formsToClose.Remove(formToClose);
        }

        return true;
    }

    public void LogOffNow(bool isForced)
    {
        if (!CloseOpenForms(isForced))
        {
            return;
        }

        FinishLogOff(isForced);
    }

    private void FinishLogOff(bool isForced)
    {
        if (InvokeRequired)
        {
            Invoke(() => { FinishLogOff(isForced); });
            return;
        }

        NullUserCheck("FinishLogOff");

        _moduleTypeLast = moduleBar.SelectedModule;

        moduleBar.SelectedModule = EnumModuleType.None;
        moduleBar.Invalidate();

        UnselectActive(true);
        AllNeutral();

        controlChart.UserLogOffCommited();

        if (isForced)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has auto logged off.");
        }
        else
        {
            SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged off.");
        }

        Clinics.LogOff();

        var userod = Security.CurUser;

        Security.CurUser = null;
        _tasksReminders = null;
        _listTaskNumsNormal = null;
        controlAppt.RefreshReminders([]);
        RefreshTasksNotification();
        Security.IsUserLoggedIn = false;
        Text = PatientL.GetMainTitle(null, 0);
        SetTimersAndThreads(false);
        ShowLogOn(doClearCaches: true);
        SignalsTick(isAllInvalidTypes: false);

        if (userod.UserNum == Security.CurUser.UserNum)
        {
        }
        else if (!PrefC.GetBool(PrefName.PatientMaintainedOnUserChange))
        {
            PatNumCur = 0;

            PatientL.RemoveAllFromMenu(menuPatient);
        }
        else
        {
            var listClinicsForUser = Clinics.GetAllForUserod(Security.CurUser);
            var listPatientsMenuLim = PatientL.GetPatientsLimFromMenu();
            for (var i = 0; i < listPatientsMenuLim.Count; i++)
            {
                if (!listClinicsForUser.Select(x => x.Id).Contains(listPatientsMenuLim[i].ClinicNum))
                {
                    PatientL.RemoveFromMenu(listPatientsMenuLim[i].PatNum);
                    if (PatNumCur == listPatientsMenuLim[i].PatNum)
                    {
                        PatNumCur = 0;
                    }
                }
            }
        }

        moduleBar.SelectedIndex = Security.GetModule(moduleBar.IndexOf(_moduleTypeLast));
        moduleBar.Invalidate();
        if (true)
        {
            Clinics.LoadClinicNumForUser();
            RefreshMenuClinics();
        }

        SetModuleSelected();

        var patient = Patients.GetPat(PatNumCur);

        Text = PatientL.GetMainTitle(patient, Clinics.ClinicNum);

        FillPatientButton(patient);

        SetTimersAndThreads(true);

        _isFormLogOnLastActive = false;

        Security.DateTimeLastActivity = DateTime.Now;
        if (moduleBar.SelectedModule == EnumModuleType.None)
        {
            ShowError("You do not have permission to use any modules.");
        }
    }

    private static void NullUserCheck(string methodName)
    {
        if (Security.CurUser != null)
        {
            return;
        }

        var stringBuilder = new StringBuilder("OpenForms:");
        for (var i = 0; i < Application.OpenForms.Count; i++)
        {
            stringBuilder.Append($"\r\n  {(Application.OpenForms[i] == null ? "Unknown" : Application.OpenForms[i].Name)}");
        }

        ODException.SwallowAnyException(() => BugSubmissions.SubmitException(
            new ODException(
                "Null user detected during log off.\r\n" +
                $"Method: {methodName}\r\n" +
                $"ActiveForm.Name: {(ActiveForm == null ? "Unknown" : ActiveForm.Name)}\r\n" +
                stringBuilder))
        );
    }

    private void InitiateShutdown(bool doShutdownAll = true)
    {
        if (Security.CurUser == null)
        {
            ProcessKillCommand();

            return;
        }
        
        var msg = "";
        if (!doShutdownAll)
        {
            msg = "Your instance of Open Dental ";
        }
        else if (Process.GetCurrentProcess().ProcessName == "OpenDental")
        {
            msg += "All copies of Open Dental ";
        }
        else
        {
            msg += Process.GetCurrentProcess().ProcessName + " ";
        }

        msg += "will shut down in 15 seconds.  Quickly click OK on any open windows with unsaved data.";

        var msgBoxCopyPaste = new MsgBoxCopyPaste(msg);

        msgBoxCopyPaste.Size = new Size(300, 300);
        msgBoxCopyPaste.TopMost = true;
        msgBoxCopyPaste.Show();

        BeginShutdownThread();
    }

    public void ProcessKillCommand()
    {
        CloseOpenForms(true);

        Application.Exit();
    }

    public static void S_ProcessKillCommand()
    {
        _formOpenDentalSingleton.ProcessKillCommand();
    }

    private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason != SessionSwitchReason.SessionLock)
        {
            return;
        }

        if (Security.CurUser == null || !Security.IsUserLoggedIn)
        {
            return;
        }

        if (!PrefC.GetBool(PrefName.SecurityLogOffWithWindows))
        {
            return;
        }

        LogOffNow(true);
    }

    private void FormOpenDental_Deactivate(object sender, EventArgs e)
    {
        if (controlChart.IsTreatmentNoteChanged)
        {
            controlChart.UpdateTreatmentNote();
        }

        if (controlAccount.canUpdateUrgFinNote())
        {
            controlAccount.UpdateUrgFinNote();
        }

        if (controlAccount.canUpdateFinNote())
        {
            controlAccount.UpdateFinNote();
        }

        if (controlTreat.HasNoteChanged)
        {
            controlTreat.UpdateTPNoteIfNeeded();
        }
    }

    private void FormOpenDental_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && Security.CurUser != null && Security.IsUserLoggedIn)
        {
            if (!AreYouSurePrompt(Security.CurUser.UserNum, "Are you sure you would like to close?"))
            {
                e.Cancel = true;
                return;
            }
        }

        FormOpenDentalClosing(e);
    }

    private void FormOpenDentalClosing(FormClosingEventArgs e)
    {
        if (ExitCode != 0)
        {
            Environment.Exit(ExitCode);
        }

        var listFormsToClose = GetOpenForms();
        var hadMultipleFormsOpen = listFormsToClose.Count > 0;

        if (!CloseOpenForms(false))
        {
            e.Cancel = true;
            return;
        }

        if (hadMultipleFormsOpen)
        {
            e.Cancel = true;
            BeginInvoke(() => Application.Exit());
            return;
        }

        try
        {
            Programs.ScrubExportedPatientData();
        }
        catch
        {
            // ignored
        }

        try
        {
            Computers.ClearHeartBeat(Environment.MachineName);
        }
        catch
        {
            // ignored
        }

        try
        {
            var activeInstances = ActiveInstances.GetAllOldInstances();
            if (ActiveInstances.GetActiveInstance() != null)
            {
                activeInstances.Add(ActiveInstances.GetActiveInstance());
            }

            ActiveInstances.DeleteMany(activeInstances);
        }
        catch
        {
            // ignored
        }

        ODThread.QuitSyncAllOdThreads();

        if (Security.CurUser != null)
        {
            try
            {
                SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged off.");
                Clinics.LogOff();
            }
            catch
            {
                // ignored
            }
        }

        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
    }

    private void FormOpenDental_FormClosed(object sender, FormClosedEventArgs e)
    {
        Dispose();

        Environment.Exit(0);
    }
}

public class PopupEvent : IComparable
{
    public long PopupNum;
    public DateTime DateTimeDisableUntil;
    public DateTime DateTimeLastViewed;

    public int CompareTo(object obj)
    {
        var popupEvent = (PopupEvent) obj;
        return DateTimeDisableUntil.CompareTo(popupEvent.DateTimeDisableUntil);
    }

    public override string ToString()
    {
        return PopupNum + ", " + DateTimeDisableUntil;
    }
}

public class ODGlobalUserActiveHandler : IMessageFilter
{
    private Point _pointPrevMousePos;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == 0x0100)
        {
            Security.DateTimeLastActivity = DateTime.Now;
        }
        else if (m.Msg == 0x0200 && _pointPrevMousePos != Cursor.Position)
        {
            _pointPrevMousePos = Cursor.Position;

            Security.DateTimeLastActivity = DateTime.Now;
        }

        return false;
    }
}