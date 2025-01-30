using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32;
using System.Net;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using OpenDental.UI;
using System.Linq;
using System.DirectoryServices;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Features.Core;
using OpenDental.Forms;
using OpenDentalImaging;
using OpenDental.Thinfinity;
using OpenDental.Graph.Dashboard;
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
    private Bitmap _bitmapIcon;
    private SigButDef[] _arraySigButDefs;
    private bool _isMouseDownOnSplitter;
    private Point _pointSplitterOriginalLocation;
    private Point _pointOriginalMouse;
    private List<PopupEvent> _listPopupEvents;
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
    private FormDashboardEditTab _formDashboardEditTab;
    private List<Task> _listTasksReminders;
    private Dictionary<long, TaskList> _dictionaryAllTaskLists;
    private List<Task> _listTasksRemindersOverLimit;
    private List<long> _listTaskNumsNormal;
    private long _userNumTasks;
    private DateTime _dateReminderRefresh = DateTime.MinValue;
    private List<AlertRead> _alertItemReads = [];
    private List<AlertItem> _alertItems = [];
    private FormXWebTransactions _formXWebTransactions;
    private FormLoginFailed _formLoginFailed = null;
    private Exception _exceptionSignalsTick;
    private bool _onlyProcessHighPrioritySignals;
    private PointF _pointFPanelSplitter96dpi;
    private FormRpDPPOvercharged _formRpDPPOvercharged;
    private readonly PatientData _pd = new();

    public FormOpenDental(string[] cla)
    {
        _formOpenDentalSingleton = this;
            
        var formSplash = new FormSplash();
            
        formSplash.Show();
            
        InitializeComponent();

        new Font("Segoe", 13);
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        //toolbar		
        ToolBarMain = new ToolBarOD();
        ToolBarMain.Name = "ToolBarMain";
        ToolBarMain.Location = new Point(51, 0);
        ToolBarMain.Size = new Size(931, 25);
        ToolBarMain.Dock = DockStyle.Top;
        ToolBarMain.ImageList = imageListMain;
        ToolBarMain.ButtonClick += toolBarMain_ButtonClick;
        LayoutManagerForms.Add(ToolBarMain, this);
        //module bar
        moduleBar = new ModuleBar();
        moduleBar.Location = new Point(0, 0);
        moduleBar.Size = new Size(51, 626);
        moduleBar.Dock = DockStyle.Left;
        moduleBar.ButtonClicked += moduleBar_ButtonClicked;
        LayoutManagerForms.Add(moduleBar, this);
        menuMain.SendToBack(); //so it has top dock priority
        //MAIN MODULE CONTROLS
        splitContainer.ColorBorder = BackColor; //the border was only visible in design mode so that we could see it.
        //contrAppt
        controlAppt = new ControlAppt {Visible = false};
        controlAppt.Dock = DockStyle.Fill;
        LayoutManagerForms.Add(controlAppt, splitContainer.Panel1);
        //contrFamily
        controlFamily = new ControlFamily {Visible = false};
        controlFamily.Dock = DockStyle.Fill;
        LayoutManagerForms.Add(controlFamily, splitContainer.Panel1);
        //contrFamilyEcw
        controlFamilyEcw = new ControlFamilyEcw {Visible = false};
        controlFamily.Dock = DockStyle.Fill;
        LayoutManagerForms.Add(controlFamilyEcw, splitContainer.Panel1);
        //contrAccount
        controlAccount = new ControlAccount {Visible = false};
        controlAccount.Dock = DockStyle.Fill;
        LayoutManagerForms.Add(controlAccount, splitContainer.Panel1);
        //contrTreat
        controlTreat = new ControlTreat {Visible = false};
        controlTreat.Dock = DockStyle.Fill;
        LayoutManagerForms.Add(controlTreat, splitContainer.Panel1);
        //contrChart
        controlChart = new ControlChart();
        controlChart.Visible = false;
        controlChart.Dock = DockStyle.Fill;
        controlChart.EventImageClick += ControlChart_ImageClick;
        controlChart.Pd = _pd;
        LayoutManagerForms.Add(controlChart, splitContainer.Panel1);
        //contrImages
        //Moved down to Load because it needs a pref to decide which one to load.
        //contrManage
        controlManage = new ControlManage {Visible = false};
        controlManage.Dock = DockStyle.Fill;
        LayoutManagerForms.Add(controlManage, splitContainer.Panel1);
        userControlDashboard = new UserControlDashboard();
        //userControlDashboard.Anchor=AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        userControlDashboard.Dock = DockStyle.Fill;
        userControlDashboard.Size = new Size(splitContainer.Panel2.Width, splitContainer.Panel2.Height);
        userControlDashboard.AutoScroll = true;
        LayoutManagerForms.Add(userControlDashboard, splitContainer.Panel2);
        userControlTasks1 = new UserControlTasks {Visible = false};
        LayoutManagerForms.Add(userControlTasks1, this);
        panelSplitter.ContextMenu = menuSplitter;
        menuItemDockBottom.Checked = true;
        DataValid.EventInvalid += (_, e) => DataValid_BecameInvalid(e);
        GlobalFormOpenDental.EventLockODForMountAcquire += (_, isEnabled) => LockODForMountAcquire(isEnabled);
        GlobalFormOpenDental.EventRefreshCurrentModule += (_, isClinicRefresh) => RefreshCurrentModule(isClinicRefresh: isClinicRefresh);
        GlobalFormOpenDental.EventModuleSelected += (_, e) => GotoModule_ModuleSelected(e);
        GlobalFormOpenDental.EventPatientSelected += (_, e) => Contr_PatientSelected(e);
        GlobalFormOpenDental.SendTextMessage = toolButTxtMsg_Click;
        GlobalFormOpenDental.GoToModule = GotoModule_ModuleSelected;
        GlobalFormOpenDental.ControlMainForm = this;
        FormLauncher.EventLaunch += FormLauncherHelper.Launch;
        //Hook up the signal processing event to the timerSignal.
        _timerSignals.Tick += timerSignals_Tick;

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
        var odPassHash = "";
        var isSilentUpdate = false;
        var odPassword = "";
        var odPassObfuscated = "";
        if (odPassword == "" && odPassObfuscated != "")
        {
            CDT.Class1.Decrypt(odPassObfuscated, out odPassword);
        }

        var serverName = "";
        var databaseName = "";
        var mySqlUser = "";
        var mySqlPassword = "";
        var mySqlPassHash = "";
        var mySqlPassObfuscated = "";
        if (mySqlPassword == "" && mySqlPassObfuscated != "")
        {
            CDT.Class1.Decrypt(mySqlPassObfuscated, out mySqlPassword);
        }
        
        var clinicNumCLA = "";
        var webServiceIsEcw = YN.Unknown;

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

            if (!PrefsStartup(false))
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
            InvalidType.ToolButsAndMounts,
            InvalidType.SigMessages
        };

        RefreshLocalData(invalidTypes.ToArray());

        FillSignalButtons();

        controlManage.InitializeOnStartup();

        if (PrefC.GetBoolSilent(PrefName.ImagesModuleUsesOld2020, false))
        {
            controlImagesOld = new ControlImagesOld {Visible = false};
            controlImagesOld.Dock = DockStyle.Fill;
            LayoutManagerForms.Add(controlImagesOld, splitContainer.Panel1);
        }
        else
        {
            controlImages = new ControlImages {Visible = false};
            controlImages.Dock = DockStyle.Fill;
            controlImages.EventKeyDown += FormOpenDental_KeyDown;
            controlImages.Controls.Add(splitContainer.Panel1);
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

        _menuItemQueryMonitor.Available = true;

        if (Security.IsAuthorized(EnumPermType.ProcCodeEdit, true) && !PrefC.GetBool(PrefName.ADAdescriptionsReset))
        {
            ProcedureCodes.ResetADAdescriptionsAndAbbrs();
            Prefs.UpdateBool(PrefName.ADAdescriptionsReset, true);
        }

        BeginOdServiceStarterThread();

        formSplash.Close();

        Signalods.DateTHighPrioritySignalLastRefreshed = MiscData.GetNowDateTime();
        if (PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs) > 0)
        {
            _timerSignals.Interval = PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs) * 1000;
            _timerSignals.Start();
        }

        LogOnOpenDentalUser(odUser, odPassword, string.Empty);

        // At this point a user has successfully logged in.
        // Flag the userod cache as safe to cache data.
        Userods.SetIsCacheAllowed(true);

        if (Security.CurUser != null)
        {
            Clinics.LoadClinicNumForUser();

            RefreshMenuClinics();
        }

        BeginOdDashboardStarterThread();

        FillSignalButtons();

        IsTreatPlanSortByTooth = PrefC.GetBool(PrefName.TreatPlanSortByTooth);
        if (userControlTasks1.Visible)
        {
            userControlTasks1.InitializeOnStartup();
        }

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

        ODException.SwallowAnyException(() => { Computers.UpdateHeartBeat(ODEnvironment.MachineName, true); });

        Text = PatientL.GetMainTitle(Patients.GetPat(PatNumCur), Clinics.ClinicNum);
        Security.DateTimeLastActivity = DateTime.Now;

        ODException.SwallowAnyException(EmailMessages.CreateCertificateStoresIfNeeded);

        var patient = Patients.GetPat(PatNumCur);

        var isApptModuleSelected = moduleBar.SelectedModule == EnumModuleType.Appointments;
        
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

        if (controlFamilyEcw.Visible)
        {
            controlFamilyEcw.Update();
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
        if (controlChart.Visible)
        {
            userControlTasks1.RefreshPatTicketsIfNeeded();
        }
        else
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
        return PrefsStartup(false);
    }

    private bool PrefsStartup(bool isSilentUpdate)
    {
        try
        {
            Cache.Refresh(InvalidType.Prefs);
        }
        catch (Exception ex)
        {
            if (isSilentUpdate)
            {
                Environment.Exit(100);

                return false;
            }

            ODMessageBox.Show(ex.Message);
            return false;
        }

        try
        {
            MiscData.SetSqlMode();
        }
        catch
        {
            if (isSilentUpdate)
            {
                Environment.Exit(111);
                return false;
            }

            ODMessageBox.Show("Unable to set global sql mode.  User probably does not have enough permission.");
            return false;
        }

        var updateComputerName = PrefC.GetStringSilent(PrefName.UpdateInProgressOnComputerName);

        var clientNameUpper = ODEnvironment.MachineName.ToUpper();
        var machineNameUpper = Environment.MachineName.ToUpper();
        if (updateComputerName != "" && !updateComputerName.ToUpper().In(clientNameUpper, machineNameUpper))
        {
            if (isSilentUpdate)
            {
                Environment.Exit(120);
                return false;
            }

            using var formUpdateInProgress = new FormUpdateInProgress(updateComputerName);

            if (formUpdateInProgress.ShowDialog() != DialogResult.OK)
            {
                return false;
            }
        }

        var dbVersionOld = PrefC.GetString(PrefName.DataBaseVersion);
        if (dbVersionOld != PrefC.GetString(PrefName.DataBaseVersion) && !isSilentUpdate)
        {
            MsgBox.Show("Database update successful");
        }

        if (!FormRegistrationKey.ValidateKey(PrefC.GetString(PrefName.RegistrationKey)))
        {
            if (isSilentUpdate)
            {
                Environment.Exit(311);
                return false;
            }

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
        LanguageForeigns.RefreshCache();

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
                _menuItemPublicHealthScreening.Available = false;
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

            if (NeedsRedraw("TaskLists"))
            {
                if (PrefC.GetBool(PrefName.TaskListAlwaysShowsAtBottom))
                {
                    //Refreshing task list here may not be the best course of action.
                    //separate if statement to prevent database call if not showing task list at bottom to begin with
                    //ComputerPref computerPref = ComputerPrefs.GetForLocalComputer();
                    if (ComputerPrefs.LocalComputer.TaskKeepListHidden)
                    {
                        userControlTasks1.Visible = false;
                    }
                    else if (WindowState != FormWindowState.Minimized)
                    {
                        //task list show and window is not minimized.
                        userControlTasks1.Visible = true;
                        userControlTasks1.InitializeOnStartup();
                        if (ComputerPrefs.LocalComputer.TaskDock == 0)
                        {
                            //bottom
                            menuItemDockBottom.Checked = true;
                            menuItemDockRight.Checked = false;
                            panelSplitter.Cursor = Cursors.HSplit;
                            panelSplitter.Height = 7;
                            var splitterNewY = 540;
                            if (ComputerPrefs.LocalComputer.TaskY != 0)
                            {
                                splitterNewY = ComputerPrefs.LocalComputer.TaskY;
                                if (splitterNewY < 300)
                                {
                                    splitterNewY = 300; //keeps it from going too high
                                }

                                if (splitterNewY > ClientSize.Height - 50)
                                {
                                    splitterNewY = ClientSize.Height - panelSplitter.Height - 50; //keeps it from going off the bottom edge
                                }
                            }

                            _pointFPanelSplitter96dpi = new PointF(0, (splitterNewY));
                            panelSplitter.Location = new Point(moduleBar.Width, (int) _pointFPanelSplitter96dpi.Y);
                        }
                        else
                        {
                            menuItemDockRight.Checked = true;
                            menuItemDockBottom.Checked = false;
                            panelSplitter.Cursor = Cursors.VSplit;
                            panelSplitter.Width = 7;

                            var splitterNewX = 900;
                            if (ComputerPrefs.LocalComputer.TaskX != 0)
                            {
                                splitterNewX = ComputerPrefs.LocalComputer.TaskX;
                                if (splitterNewX < 300)
                                {
                                    splitterNewX = 300; //keeps it from going too far to the left
                                }

                                if (splitterNewX > ClientSize.Width - 60)
                                {
                                    splitterNewX = ClientSize.Width - panelSplitter.Width - 60; //keeps it from going off the right edge
                                }
                            }

                            _pointFPanelSplitter96dpi = new PointF((splitterNewX), 0);
                            panelSplitter.Location = new Point((int) _pointFPanelSplitter96dpi.X, ToolBarMain.Height);
                        }
                    }
                }
                else
                {
                    userControlTasks1.Visible = false;
                }
            }

            LayoutControls();
        }

        if (arrayITypes.Contains(InvalidType.Sheets) && userControlDashboard.IsInitialized)
        {
            LayoutControls();

            userControlDashboard.RefreshDashboard();

            ResizeDashboard();
            RefreshMenuDashboards();
        }

        if (arrayITypes.Contains(InvalidType.Security) || isAll)
        {
            RefreshMenuDashboards();
        }

        if (arrayITypes.Contains(InvalidType.SigMessages) || isAll)
        {
            FillSignalButtons();
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

        controlTreat.InitializeLocalData(); //easier to leave this here for now than to split it.
        _dictionaryChartPrefsCache.Clear();
        _dictionaryTaskListPrefsCache.Clear();
        //Chart Drawing Prefs
        _dictionaryChartPrefsCache.Add(PrefName.UseInternationalToothNumbers.ToString(), PrefC.GetInt(PrefName.UseInternationalToothNumbers));
        _dictionaryChartPrefsCache.Add("GraphicsUseHardware", ComputerPrefs.LocalComputer.GraphicsUseHardware);
        _dictionaryChartPrefsCache.Add("PreferredPixelFormatNum", ComputerPrefs.LocalComputer.PreferredPixelFormatNum);
        _dictionaryChartPrefsCache.Add("GraphicsSimple", ComputerPrefs.LocalComputer.GraphicsSimple);
        _dictionaryChartPrefsCache.Add(PrefName.ShowFeatureEhr.ToString(), PrefC.GetBool(PrefName.ShowFeatureEhr));
        _dictionaryChartPrefsCache.Add("DirectXFormat", ComputerPrefs.LocalComputer.DirectXFormat);
        _dictionaryChartPrefsCache.Add(PrefName.OrthoShowInChart.ToString(), PrefC.GetBool(PrefName.OrthoShowInChart));
        //Task list drawing prefs
        _dictionaryTaskListPrefsCache.Add("TaskDock", ComputerPrefs.LocalComputer.TaskDock);
        _dictionaryTaskListPrefsCache.Add("TaskY", ComputerPrefs.LocalComputer.TaskY);
        _dictionaryTaskListPrefsCache.Add("TaskX", ComputerPrefs.LocalComputer.TaskX);
        _dictionaryTaskListPrefsCache.Add(PrefName.TaskListAlwaysShowsAtBottom.ToString(), PrefC.GetBool(PrefName.TaskListAlwaysShowsAtBottom));
        _dictionaryTaskListPrefsCache.Add(PrefName.TasksUseRepeating.ToString(), PrefC.GetBool(PrefName.TasksUseRepeating));
        _dictionaryTaskListPrefsCache.Add(PrefName.TasksNewTrackedByUser.ToString(), PrefC.GetBool(PrefName.TasksNewTrackedByUser));
        _dictionaryTaskListPrefsCache.Add(PrefName.TasksShowOpenTickets.ToString(), PrefC.GetBool(PrefName.TasksShowOpenTickets));
        _dictionaryTaskListPrefsCache.Add("TaskKeepListHidden", ComputerPrefs.LocalComputer.TaskKeepListHidden);
        if (Security.IsAuthorized(EnumPermType.UserQueryAdmin, true))
        {
            _menuItemUserQuery.Available = true;
        }
        else
        {
            _menuItemUserQuery.Available = false;
        }

        _menuItemQueryFavorites.Available = Security.IsAuthorized(EnumPermType.UserQuery, true);
    }

    ///<summary>Compares preferences related to sections of the program that require redraws and returns true if a redraw is necessary, false otherwise.  If anything goes wrong with checking the status of any preference this method will return true.</summary>
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
                        || PrefC.GetBool(PrefName.ShowFeatureEhr) != (bool) _dictionaryChartPrefsCache["ShowFeatureEhr"]
                        || ComputerPrefs.LocalComputer.DirectXFormat != (string) _dictionaryChartPrefsCache["DirectXFormat"]
                        || PrefC.GetBool(PrefName.OrthoShowInChart) != (bool) _dictionaryChartPrefsCache["OrthoShowInChart"])
                    {
                        return true;
                    }

                    break;
                case "TaskLists":
                    if (_dictionaryTaskListPrefsCache.Count == 0
                        || ComputerPrefs.LocalComputer.TaskDock != (int) _dictionaryTaskListPrefsCache["TaskDock"] //Checking for task list redrawing
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
                //case "TreatmentPlan":
                //	//If needed implement this section
                //	break;
            } //end switch

            return false;
        }
        catch
        {
            return true; //Should never happen.  Would most likely be caused by invalid preferences within the database.
        }
    }

    ///<summary>Causes the toolbar to be laid out again.</summary>
    private void LayoutToolBar()
    {
        ToolBarMain.Buttons.Clear();
        ToolBarMain.ImageList = imageListMain;
        ODToolBarButton toolBarButton;
        toolBarButton = new ODToolBarButton(Lan.g(this, "Select Patient"), EnumIcons.PatSelect, "", "Patient")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuPatient
        };
        ToolBarMain.Buttons.Add(toolBarButton);
        //eCW tight only gets Patient Select and Popups toolbar buttons
        toolBarButton = new ODToolBarButton(Lan.g(this, "Commlog"), EnumIcons.CommLog, Lan.g(this, "New Commlog Entry"), "Commlog");
        ToolBarMain.Buttons.Add(toolBarButton);
        toolBarButton = new ODToolBarButton(Lan.g(this, "E-mail"), EnumIcons.Email, Lan.g(this, "Send E-mail"), "Email")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuEmail
        };
        ToolBarMain.Buttons.Add(toolBarButton);
        toolBarButton = new ODToolBarButton(Lan.g(this, "WebMail"), EnumIcons.WebMail, Lan.g(this, "Secure WebMail"), "WebMail")
        {
            Enabled = true //Always enabled.  If the patient does not have an email address, then the user will be blocked from the FormWebMailMessageEdit window.
        };
        ToolBarMain.Buttons.Add(toolBarButton);
        if (_toolBarButtonText == null)
        {
            //If laying out again (after modifying setup), we keep the button to preserve the current notification text.
            _toolBarButtonText = new ODToolBarButton(Lan.g(this, "Text"), EnumIcons.Text, Lan.g(this, "Send Text Message"), "Text")
            {
                Style = ODToolBarButtonStyle.DropDownButton,
                DropDownMenu = menuText,
                Enabled = Programs.IsEnabled(ProgramName.CallFire) || SmsPhones.IsIntegratedTextingEnabled()
            };
            //The Notification text has not been set since startup.  We need an accurate starting count.
            if (SmsPhones.IsIntegratedTextingEnabled())
            {
                //Init.  Will query for sms notification signal, or insert one if not found (eConnector hasn't updated this signal since we last cleared
                //old signals).
                SetSmsNotificationText();
            }
        }

        ToolBarMain.Buttons.Add(_toolBarButtonText);
        toolBarButton = new ODToolBarButton(Lan.g(this, "Letter"), -1, Lan.g(this, "Quick Letter"), "Letter")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuLetter
        };
        ToolBarMain.Buttons.Add(toolBarButton);
        toolBarButton = new ODToolBarButton(Lan.g(this, "Forms"), -1, "", "Form");
        //button.Style=ODToolBarButtonStyle.DropDownButton;
        //button.DropDownMenu=menuForm;
        ToolBarMain.Buttons.Add(toolBarButton);
        if (_toolBarButtonTask == null)
        {
            _toolBarButtonTask = new ODToolBarButton(Lan.g(this, "Tasks"), 3, Lan.g(this, "Open Tasks"), "Tasklist")
            {
                Style = ODToolBarButtonStyle.DropDownButton,
                DropDownMenu = menuTask
            };
        }

        ToolBarMain.Buttons.Add(_toolBarButtonTask);
        toolBarButton = new ODToolBarButton(Lan.g(this, "Label"), 4, Lan.g(this, "Print Label"), "Label")
        {
            Style = ODToolBarButtonStyle.DropDownButton,
            DropDownMenu = menuLabel
        };
        ToolBarMain.Buttons.Add(toolBarButton);
        ToolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this, "Popups"), -1, Lan.g(this, "Edit popups for this patient"), "Popups"));
        ProgramL.LoadToolBar(ToolBarMain, EnumToolBar.MainToolbar);
        ToolBarMain.Invalidate();
        UpdateToolbarButtons();
    }
        
    ///<summary>Enables toolbar buttons if a patient is selected, otherwise disables them.</summary>
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
            if (_toolBarButtonText != null)
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

    private void panelSplitter_MouseDown(object sender, MouseEventArgs e)
    {
        _isMouseDownOnSplitter = true;
        _pointSplitterOriginalLocation = panelSplitter.Location;
        _pointOriginalMouse = new Point(panelSplitter.Left + e.X, panelSplitter.Top + e.Y);
    }

    private void panelSplitter_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDownOnSplitter)
        {
            return;
        }

        if (menuItemDockBottom.Checked)
        {
            var splitterNewY = _pointSplitterOriginalLocation.Y + panelSplitter.Top + e.Y - _pointOriginalMouse.Y;
            if (splitterNewY < 300)
            {
                splitterNewY = 300;
            }

            if (splitterNewY > ClientSize.Height - 50)
            {
                splitterNewY = ClientSize.Height - panelSplitter.Height - 50;
            }

            _pointFPanelSplitter96dpi.Y = (splitterNewY);
        }
        else
        {
            var splitterNewX = _pointSplitterOriginalLocation.X + panelSplitter.Left + e.X - _pointOriginalMouse.X;
            if (splitterNewX < 300)
            {
                splitterNewX = 300;
            }

            if (splitterNewX > ClientSize.Width - 60)
            {
                splitterNewX = ClientSize.Width - panelSplitter.Width - 60;
            }

            _pointFPanelSplitter96dpi.X = (splitterNewX);
        }

        LayoutControls();
    }

    private void panelSplitter_MouseUp(object sender, MouseEventArgs e)
    {
        _isMouseDownOnSplitter = false;

        TaskDockSavePos();
    }

    private void menuItemDockBottom_Click(object sender, EventArgs e)
    {
        menuItemDockBottom.Checked = true;
        menuItemDockRight.Checked = false;

        panelSplitter.Cursor = Cursors.HSplit;

        TaskDockSavePos();
        LayoutControls();
    }

    private void menuItemDockRight_Click(object sender, EventArgs e)
    {
        if (IsDashboardVisible)
        {
            MsgBox.Show("Tasks cannot be docked to the right when Dashboards are in use.");
            return;
        }

        menuItemDockBottom.Checked = false;
        menuItemDockRight.Checked = true;

        panelSplitter.Cursor = Cursors.VSplit;

        TaskDockSavePos();
        LayoutControls();
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

        userControlDashboard.SuspendLayout();

        lightSignalGrid1.Location = new Point(0, 489);
        lightSignalGrid1.Width = moduleBar.Width - 1;

        var pointPosition = new Point(moduleBar.Width, ToolBarMain.Bottom);
        var width = ClientSize.Width - pointPosition.X;
        var height = ClientSize.Height - pointPosition.Y;

        if (userControlTasks1.Visible)
        {
            if (menuItemDockBottom.Checked)
            {
                if (panelSplitter.Height > 9)
                {
                    panelSplitter.Height=7;

                    _pointFPanelSplitter96dpi = new Point(0, 540);
                }

                panelSplitter.Location = new Point(pointPosition.X, (int) _pointFPanelSplitter96dpi.Y);
                panelSplitter.Width = width;

                panelSplitter.Visible = true;

                userControlTasks1.Location = new Point(pointPosition.X, panelSplitter.Bottom);
                userControlTasks1.Width = width;
                userControlTasks1.Height = ClientSize.Height - userControlTasks1.Top;

                height = ClientSize.Height - panelSplitter.Height - userControlTasks1.Height - ToolBarMain.Height - menuMain.Height;
            }
            else
            {
                if (panelSplitter.Width > 9)
                {
                    panelSplitter.Width=7;

                    _pointFPanelSplitter96dpi = new Point(900, 0);
                }

                panelSplitter.Location = new Point((int) _pointFPanelSplitter96dpi.X, pointPosition.Y);
                panelSplitter.Height = height;
                panelSplitter.Visible = true;
                userControlTasks1.Bounds = new Rectangle(panelSplitter.Right, pointPosition.Y, ClientSize.Width - panelSplitter.Right, height);

                width = ClientSize.Width - panelSplitter.Width - userControlTasks1.Width - pointPosition.X;
            }

            panelSplitter.BringToFront();
            panelSplitter.Invalidate();
            userControlTasks1.Refresh();
        }
        else
        {
            panelSplitter.Visible = false;
        }

        splitContainer.Bounds = new Rectangle(pointPosition.X, pointPosition.Y, width, height);
        if (userControlDashboard.IsInitialized && userControlDashboard.ListOpenWidgets.Count > 0)
        {
            if (splitContainer.Panel2Collapsed)
            {
                splitContainer.Panel2Collapsed = false;
            }
        }
        else
        {
            splitContainer.Panel2Collapsed = true;
        }

        ResizeDashboard();

        userControlDashboard.ResumeLayout();

        FillSignalButtons(null);
    }

    private void ResizeDashboard()
    {
        var width = userControlDashboard.WidgetWidth;
        var widthScrollBar = 0;

        if (userControlDashboard.VerticalScroll.Visible)
        {
            widthScrollBar = SystemInformation.VerticalScrollBarWidth;
        }

        splitContainer.SplitterDistance = Math.Max(splitContainer.Width - splitContainer.SplitterWidth - width - userControlDashboard.Margin.Left - widthScrollBar, 0);
        userControlDashboard.Size = new Size(splitContainer.Panel2.Width, splitContainer.Panel2.Height);
    }

    private void splitContainer_SplitterMoved(object sender, EventArgs e)
    {
        if (userControlDashboard == null || splitContainer.Panel2Collapsed)
        {
            return;
        }

        if (controlAppt.Visible)
        {
            controlAppt.LayoutControls();
        }
    }

    private void TaskDockSavePos()
    {
        if (menuItemDockBottom.Checked)
        {
            ComputerPrefs.LocalComputer.TaskY = (int) _pointFPanelSplitter96dpi.Y;
            ComputerPrefs.LocalComputer.TaskDock = 0;
        }
        else
        {
            ComputerPrefs.LocalComputer.TaskX = (int) _pointFPanelSplitter96dpi.X;
            ComputerPrefs.LocalComputer.TaskDock = 1;
        }

        ComputerPrefs.Update(ComputerPrefs.LocalComputer);
    }

    #region ToolBar

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
                    toolButWebMail_Click();
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
                    toolButTasks_Click();
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
                //if a trigger happened
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

        _listPopupEvents ??= [];

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

        //New patient selected.  Everything below here is for popups.
        //First, remove all expired popups from the event list.
        for (var i = _listPopupEvents.Count - 1; i >= 0; i--)
        {
            //go backwards
            if (_listPopupEvents[i].DateTimeDisableUntil < DateTime.Now)
            {
                //expired
                _listPopupEvents.RemoveAt(i);
            }
        }

        //Now, loop through all popups for the patient.
        var listPopups = Popups.GetForPatient(patient); //get all possible 
        for (var i = 0; i < listPopups.Count; i++)
        {
            //skip any popups that are disabled because they are on the event list
            var popupIsDisabled = false;
            for (var e = 0; e < _listPopupEvents.Count; e++)
            {
                if (listPopups[i].PopupNum == _listPopupEvents[e].PopupNum)
                {
                    popupIsDisabled = true;
                    break;
                }
            }

            if (popupIsDisabled)
            {
                continue;
            }

            //This popup is not disabled, so show it.
            //A future improvement would be to assemble all the popups that are to be shown and then show them all in one large window.
            //But for now, they will show in sequence.
            if (controlAppt.Visible)
            {
                controlAppt.MouseUpForced();
            }

            using var formPopupDisplay = new FormPopupDisplay();
            formPopupDisplay.PopupCur = listPopups[i];
            formPopupDisplay.ShowDialog();
            if (formPopupDisplay.MinutesDisabled > 0)
            {
                var popupEvent = new PopupEvent
                {
                    PopupNum = listPopups[i].PopupNum,
                    DateTimeDisableUntil = DateTime.Now + TimeSpan.FromMinutes(formPopupDisplay.MinutesDisabled),
                    DateTimeLastViewed = DateTime.Now
                };
                _listPopupEvents.Add(popupEvent);
                _listPopupEvents.Sort();
            }
        }
    }

    ///<summary>Happens when any of the modules changes the current patient or when this main form changes the patient.  The calling module should refresh itself.  The current patNum is stored here in the parent form so that when switching modules, the parent form knows which patient to call up for that module.</summary>
    private void Contr_PatientSelected(PatientSelectedEventArgs e)
    {
        PatNumCur = e.Patient_.PatNum;

        if (e.IsRefreshCurModule)
        {
            RefreshCurrentModule(e.HasForcedRefresh, e.IsApptRefreshDataPat);
        }

        userControlTasks1.RefreshPatTicketsIfNeeded();

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
        if (patient != null && patient.PatStatus.ToString() == "NonPatient" && _datePopupDelay <= DateTime.Now)
        {
            MsgBox.Show(this, "A patient with the status NonPatient is currently selected.");

            _datePopupDelay = DateTime.Now.AddMinutes(5);
        }
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

        var listRefAttachs = RefAttaches.Refresh(PatNumCur);
        var referralDescript = DisplayFields.GetForCategory(DisplayFieldCategory.PatientInformation).FirstOrDefault(x => x.InternalName == "Referrals")?.Description;
        if (string.IsNullOrWhiteSpace(referralDescript))
        {
            //either not displaying the Referral field or no description entered, default to 'Referral'
            referralDescript = Lan.g(this, "Referral");
        }

        foreach (var t in listRefAttachs)
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

    private void toolButWebMail_Click()
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
        if (((MenuItem) sender).Tag == null)
        {
            return;
        }
            
        if (((MenuItem) sender).Tag.GetType() == typeof(Referral))
        {
            var referral = (Referral) ((MenuItem) sender).Tag;
            if (referral.EMail == "")
            {
                return;
            }

            var emailMessage = new EmailMessage
            {
                PatNum = PatNumCur
            };
            var patient = Patients.GetPat(PatNumCur);
            emailMessage.ToAddress = referral.EMail; //pat.Email;
            var emailAddress = EmailAddresses.GetByClinic(patient.ClinicNum);
            emailAddress = EmailAddresses.OverrideSenderAddressClinical(emailAddress, patient.ClinicNum);
            emailMessage.FromAddress = emailAddress.GetFrom();
            emailMessage.Subject = Lan.g(this, "RE: ") + patient.GetNameFL();
            emailMessage.MsgType = EmailMessageSource.Manual;
            using var formEmailMessageEdit = new FormEmailMessageEdit(emailMessage, emailAddress);
            formEmailMessageEdit.IsNew = true;
            formEmailMessageEdit.ShowDialog();
            if (formEmailMessageEdit.DialogResult == DialogResult.OK)
            {
                RefreshCurrentModule();
            }
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
            //If there is no currently minimized commlog form, make a new persistent one
            var frmCommItem = new FrmCommItem(GetNewCommlog())
            {
                IsPersistent = true
            };
            frmCommItem.Show();
            return;
        }

        //A persistent window already is open
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
        MenuItem menuItem;
        menuItem = new MenuItem(Lan.g(this, "Merge"), menuLetter_Click);
        menuItem.Tag = "Merge";
        menuLetter.MenuItems.Add(menuItem);
        //menuItem=new MenuItem(Lan.g(this,"Stationery"),menuLetter_Click);
        //menuItem.Tag="Stationery";
        //menuLetter.MenuItems.Add(menuItem);
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

    private void toolButTasks_Click()
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
        if (_listTasksReminders == null)
        {
            return 0;
        }

        //Mimics how checkNew is set in FormTaskEdit.
        if (PrefC.GetBool(PrefName.TasksNewTrackedByUser))
        {
            //Per definition of task.IsUnread.
            return _listTasksReminders.FindAll(x => x.IsUnread && x.DateTimeEntry <= DateTime.Now).Count;
        }

        return _listTasksReminders.FindAll(x => x.TaskStatus == TaskStatusEnum.New && x.DateTimeEntry <= DateTime.Now).Count;
    }

    private void menuItemTaskNewForUser_Click(object sender, EventArgs e)
    {
        controlManage.LaunchTaskWindow(false, UserControlTasksTab.ForUser); //Set the tab to the "for [User]" tab.
    }

    private void menuItemTaskReminders_Click(object sender, EventArgs e)
    {
        controlManage.LaunchTaskWindow(false, UserControlTasksTab.Reminders); //Set the tab to the "Reminders" tab
    }

    private delegate void ToolBarMainClick(long patNum);

    private void toolButLabel_Click()
    {
        //The reason we are using a delegate and BeginInvoke() is because of a Microsoft bug that causes the Print Dialog window to not be in focus			
        //when it comes from a toolbar click.
        //https://social.msdn.microsoft.com/Forums/windows/en-US/681a50b4-4ae3-407a-a747-87fb3eb427fd/first-mouse-click-after-showdialog-hits-the-parent-form?forum=winforms
        ToolBarMainClick toolClick = LabelSingle.PrintPat;
        BeginInvoke(toolClick, PatNumCur);
    }

    private void menuLabel_Popup(object sender, EventArgs e)
    {
        menuLabel.MenuItems.Clear();
        MenuItem menuItem;
        var listSheetDefsLabel = SheetDefs.GetCustomForType(SheetTypeEnum.LabelPatient);
        if (listSheetDefsLabel.Count == 0)
        {
            menuItem = new MenuItem(Lan.g(this, "LName, FName, Address"), menuLabel_Click);
            menuItem.Tag = "PatientLFAddress";
            menuLabel.MenuItems.Add(menuItem);
            menuItem = new MenuItem(Lan.g(this, "Name, ChartNumber"), menuLabel_Click);
            menuItem.Tag = "PatientLFChartNumber";
            menuLabel.MenuItems.Add(menuItem);
            menuItem = new MenuItem(Lan.g(this, "Name, PatNum"), menuLabel_Click);
            menuItem.Tag = "PatientLFPatNum";
            menuLabel.MenuItems.Add(menuItem);
            menuItem = new MenuItem(Lan.g(this, "Radiograph"), menuLabel_Click);
            menuItem.Tag = "PatRadiograph";
            menuLabel.MenuItems.Add(menuItem);
        }
        else
        {
            for (var i = 0; i < listSheetDefsLabel.Count; i++)
            {
                menuItem = new MenuItem(listSheetDefsLabel[i].Description, menuLabel_Click);
                menuItem.Tag = listSheetDefsLabel[i];
                menuLabel.MenuItems.Add(menuItem);
            }
        }

        menuLabel.MenuItems.Add("-");
        //Carriers---------------------------------------------------------------------------------------
        var family = Patients.GetFamily(PatNumCur);
        //Received multiple bug submissions where CurPatNum==0, even though this toolbar button should not be enabled when no patient is selected.
        if (family.ListPats != null && family.ListPats.Length > 0)
        {
            var listPatPlans = PatPlans.Refresh(PatNumCur);
            var listInsSubs = InsSubs.RefreshForFam(family);
            var listInsPlans = InsPlans.RefreshForSubList(listInsSubs);
            Carrier carrier;
            InsPlan insPlan;
            InsSub insSub;
            for (var i = 0; i < listPatPlans.Count; i++)
            {
                insSub = InsSubs.GetSub(listPatPlans[i].InsSubNum, listInsSubs);
                insPlan = InsPlans.GetPlan(insSub.PlanNum, listInsPlans);
                carrier = Carriers.GetCarrier(insPlan.CarrierNum);
                menuItem = new MenuItem(carrier.CarrierName, menuLabel_Click);
                menuItem.Tag = carrier;
                menuLabel.MenuItems.Add(menuItem);
            }

            menuLabel.MenuItems.Add("-");
        }

        //Referrals---------------------------------------------------------------------------------------
        menuItem = new MenuItem(Lan.g(this, "Referrals:"));
        menuItem.Tag = null;
        menuLabel.MenuItems.Add(menuItem);
        var referralDescript = DisplayFields.GetForCategory(DisplayFieldCategory.PatientInformation)
            .FirstOrDefault(x => x.InternalName == "Referrals")?.Description;
        if (string.IsNullOrWhiteSpace(referralDescript))
        {
            //either not displaying the Referral field or no description entered, default to 'Referral'
            referralDescript = Lan.g(this, "Referral");
        }

        var listRefAttaches = RefAttaches.Refresh(PatNumCur);
        Referral referral;
        string str;
        for (var i = 0; i < listRefAttaches.Count; i++)
        {
            if (!Referrals.TryGetReferral(listRefAttaches[i].ReferralNum, out referral))
            {
                continue;
            }

            if (listRefAttaches[i].RefType == ReferralType.RefFrom)
            {
                str = Lan.g(this, "From");
            }
            else if (listRefAttaches[i].RefType == ReferralType.RefTo)
            {
                str = Lan.g(this, "To");
            }
            else
            {
                str = referralDescript;
            }

            str += " " + Referrals.GetNameFL(referral.ReferralNum);
            menuItem = new MenuItem(str, menuLabel_Click);
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
        using var formPopupsForFam = new FormPopupsForFam(_listPopupEvents);
            
        formPopupsForFam.PatientCur = Patients.GetPat(PatNumCur);
        formPopupsForFam.ShowDialog();
    }

    #endregion ToolBar

    #region SMS Text Messaging

    ///<summary>Called from the text message button and the right click context menu for an appointment. Returns true if the message was sent successfully.</summary>
    private bool toolButTxtMsg_Click(long patNum, string startingText = "")
    {
        if (patNum == 0)
        {
            using var formTxtMsgEdit = new FormTxtMsgEdit();
            formTxtMsgEdit.Message = startingText;
            formTxtMsgEdit.PatNum = 0;
            formTxtMsgEdit.ShowDialog();
            if (formTxtMsgEdit.DialogResult == DialogResult.OK)
            {
                RefreshCurrentModule();
                return true;
            }

            return false;
        }

        var patient = Patients.GetPat(patNum);
        var updateTextYN = false;
        if (patient.TxtMsgOk == YN.No)
        {
            if (MsgBox.Show(this, MsgBoxButtons.YesNo, "This patient is marked to not receive text messages. "
                                                       + "Would you like to mark this patient as okay to receive text messages?"))
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
            if (MsgBox.Show(this, MsgBoxButtons.YesNo, "This patient might not want to receive text messages. "
                                                       + "Would you like to mark this patient as okay to receive text messages?"))
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
            Patients.InsertAddressChangeSecurityLogEntry(patientOld, patient); // track change in securitylog for TxtOK Field
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
        formTxtMsgEdit2.ShowDialog();
        if (formTxtMsgEdit2.DialogResult == DialogResult.OK)
        {
            RefreshCurrentModule();
            return true;
        }

        return false;
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

    ///<summary>Sets the SMS "Text" button's Notification Text based on structured data parsed from signalSmsCount.MsgValue.  This Signalod will have been inserted into the db by the eConnector.  If signalSmsCount is not passed in, attempts to find the most recent Signalod of type SmsTextMsgReceivedUnreadCount, using it to update the notification text, or if not found, either creates and inserts the Signalod (this occurs on startup if the Signalod table does not have an entry for this signal type) or uses the currently displayed value of the Sms notification 
    ///Text and the 'increment' value to update the locally displayed notification count (occurs when this method is called between signal intervals
    ///and the eConnector has not updated the SmsTextmsgReceivedUnreadCount signal in the last signal interval).
    ///</summary>
    ///<param name="signalodSmsCount">Signalod, inserted by the eConnector, containing a list of clinicnums and the count of unread SmsFromMobiles for 
    ///each clinic.</param>
    ///<param name="doUseSignalInterval">Defaults to true.  Indicates, in the event that signalSmsCount is null, if the query to find the most 
    ///recent SmsTextMsgReceivedUnreadCount type Signalod should be run for the interval since signals were last processed, or if the entire table
    ///should be considered.</param>
    ///<param name="increment">Defaults to 0.  Increments the value displayed in the Sms notification "Text" button for unread SmsFromMobiles, but
    ///only if a Signalod of type SmsTextMsgReceivedUnreadCount was not found.  This can occur if signalSmsCount is null, doUseSignalInteral is true
    ///and the signal was not found in the the last signal interval (meaning the eConnector has not updated the SmsNotification count recently).
    ///</param>
    private void SetSmsNotificationText(Signalod signalodSmsCount = null, bool doUseSignalInterval = true, int increment = 0)
    {
        if (_toolBarButtonText == null)
        {
            return; //This button does not exist in eCW tight integration mode.
        }

        try
        {
            if (!_toolBarButtonText.Enabled)
            {
                return; //This button is disabled when neither of the Text Messaging bridges have been enabled.
            }

            List<SmsFromMobiles.SmsNotification> listSmsNotifications = null;
            if (signalodSmsCount == null)
            {
                //If we are here because the user changed clinics, then get the absolute most recent sms notification signal.
                //Otherwise, use DateTime since last signal refresh.
                var timeSignalStart = doUseSignalInterval ? Signalods.DateTRegularPrioritySignalLastRefreshed : DateTime.MinValue;
                //Get the most recent SmsTextMsgReceivedUnreadCount. Should only be one, but just in case, order desc.
                signalodSmsCount = Signalods.RefreshTimed(timeSignalStart, [InvalidType.SmsTextMsgReceivedUnreadCount])
                    .OrderByDescending(x => x.SigDateTime)
                    .FirstOrDefault();
                if (signalodSmsCount == null && timeSignalStart == DateTime.MinValue)
                {
                    //No SmsTextMsgReceivedUnreadCount signal in db.  This means the eConnector has not updated the sms notification signal in quite some 
                    //time.  Do the eConnector's job; 
                    listSmsNotifications = Signalods.UpsertSmsNotification();
                }
            }

            if (signalodSmsCount != null)
            {
                //Either the signal was passed in, or we found it when we queried.
                listSmsNotifications = SmsFromMobiles.SmsNotification.GetListFromJson(signalodSmsCount.MsgValue); //Extract notifications from signal.
                if (listSmsNotifications == null)
                {
                    return; //Something went wrong deserializing the signal.  Leave the stale notification count until eConnector updates the signal.
                }
            }

            var smsUnreadCount = 0;
            if (listSmsNotifications == null)
            {
                //listNotifications might still be null if signalSmsCount was not passed in, signal processing had already started, and we didn't find the
                //sms notification signal in the last signal interval.  We will assume the signal is stale.  We know the count has changed (based on some 
                //action) if 'increment' is non-zero, so increment according to our known changes.
                smsUnreadCount = SIn.Int(_toolBarButtonText.NotificationText) + increment;
            }
            else if (!true || Clinics.ClinicNum == 0)
            {
                //No clinics or HQ clinic is active so sum them all.
                smsUnreadCount = listSmsNotifications.Sum(x => x.Count);
            }
            else
            {
                //Only count the active clinic.
                smsUnreadCount = listSmsNotifications.Where(x => x.ClinicNum == Clinics.ClinicNum).Sum(x => x.Count);
            }

            //Default to empty so we show nothing if there aren't any notifications.
            var smsNotificationText = "";
            if (smsUnreadCount > 99)
            {
                //We only have room in the UI for a 2-digit number.
                smsNotificationText = "99";
            }
            else if (smsUnreadCount > 0)
            {
                //We have a "real" number so show it.
                smsNotificationText = smsUnreadCount.ToString();
            }

            if (_toolBarButtonText.NotificationText == smsNotificationText)
            {
                //Prevent the toolbar from being invalidated unnecessarily.
                return;
            }

            _toolBarButtonText.NotificationText = smsNotificationText;
            if (menuItemTextMessagesReceived.Text.Contains("("))
            {
                //Remove the old count from the menu item.
                menuItemTextMessagesReceived.Text = menuItemTextMessagesReceived.Text.Substring(0, menuItemTextMessagesReceived.Text.IndexOf("(") - 1);
            }

            if (smsNotificationText != "")
            {
                menuItemTextMessagesReceived.Text += " (" + smsNotificationText + ")";
            }
        }
        finally
        {
            //Always redraw the toolbar item.
            ToolBarMain.Invalidate(_toolBarButtonText.Bounds); //To cause the Text button to redraw.			
        }
    }

    #endregion SMS Text Messaging

    #region Clinics

    private void RefreshMenuClinics()
    {
        _menuItemClinicsMain.DropDown.Items.Clear();
        var listClinics = Clinics.GetForUserod(Security.CurUser);
        if (listClinics.Count < 30)
        {
            //This number of clinics will fit in a 990x735 form.
            MenuItemOD menuItem;
            if (!Security.CurUser.ClinicIsRestricted)
            {
                menuItem = new MenuItemOD(Lan.g(this, "Headquarters"), menuClinic_Click);
                menuItem.Tag = new ClinicDto(); //Having a ClinicNum of 0 will make OD act like 'Headquarters'.  This allows the user to see unassigned appt views, all operatories, etc.
                if (Clinics.ClinicNum == 0)
                {
                    menuItem.Checked = true;
                }

                _menuItemClinicsMain.Add(menuItem);
                _menuItemClinicsMain.AddSeparator();
            }

            for (var i = 0; i < listClinics.Count; i++)
            {
                menuItem = new MenuItemOD(listClinics[i].Abbr, menuClinic_Click);
                menuItem.Tag = listClinics[i];
                if (Clinics.ClinicNum == listClinics[i].Id)
                {
                    menuItem.Checked = true;
                }

                _menuItemClinicsMain.Add(menuItem);
            }
        }
        else
        {
            //too many clinics to put in a menu drop down
            _menuItemClinicsMain.Click -= menuClick_OpenPickList;
            _menuItemClinicsMain.Click += menuClick_OpenPickList;
        }

        RefreshLocalData(InvalidType.Views, //fills apptviews, sets the view, and then calls ContrAppt.ModuleSelected
            InvalidType.ToolButsAndMounts); //because program link buttons can be shown/hidden by clinic
        if (!controlAppt.Visible)
        {
            RefreshCurrentModule(); //calls ModuleSelected of the current module, don't do this if ContrAppt2 is visible since it was just done above
        }

        moduleBar.RefreshButtons();
        CheckAlerts();
    }

    private void menuClick_OpenPickList(object sender, EventArgs e)
    {
        using var formClinics = new FormClinics();
        formClinics.IsSelectionMode = true;
        formClinics.ShowDialog();
        if (formClinics.DialogResult != DialogResult.OK)
        {
            return;
        }

        if (formClinics.SelectedClinicId == 0)
        {
            //'Headquarters' was selected.
            RefreshCurrentClinic(new ClinicDto());
            return;
        }

        var clinic = Clinics.GetFirstOrDefault(x => x.Id == formClinics.SelectedClinicId);
        if (clinic != null)
        {
            //Should never be null because the clinic should always be in the list
            RefreshCurrentClinic(clinic);
        }

        CheckAlerts();
    }

    ///<summary>This is will set Clinics.ClinicNum and refresh the current module.</summary>
    private void menuClinic_Click(object sender, EventArgs e)
    {
        if (sender.GetType() != typeof(MenuItemOD) && ((MenuItemOD) sender).Tag != null)
        {
            return;
        }

        var clinic = (ClinicDto) ((MenuItemOD) sender).Tag;
        RefreshCurrentClinic(clinic);
    }

    ///<summary>This is used to set Clinics.ClinicNum and refreshes the current module.</summary>
    private void RefreshCurrentClinic(ClinicDto clinic)
    {
        var isChangingClinic = Clinics.ClinicNum != clinic.Id;
        Clinics.SetClinicNum(clinic.Id);
        Text = PatientL.GetMainTitle(Patients.GetPat(PatNumCur), Clinics.ClinicNum);
        SetSmsNotificationText(doUseSignalInterval: !isChangingClinic);
        if (PrefC.GetBool(PrefName.AppointmentClinicTimeReset))
        {
            controlAppt.ModuleSelected(DateTime.Today);
            //this actually refreshes the module, which is possibly different behavior than before the overhaul.
            //Alternatively, we might check to see of that module is selected first.
        }

        RefreshMenuClinics();
        if (isChangingClinic)
        {
            _listTaskNumsNormal = null; //Will cause task preprocessing to run again.
            _listTasksReminders = null; //Will cause task preprocessing to run again.
            UserControlTasks.ResetGlobalTaskFilterTypesToDefaultAllInstances();
            UserControlTasks.RefreshTasksForAllInstances(null); //Refresh tasks so any filter changes are applied immediately.
            //In the future this may need to be enhanced to also consider refreshing other clinic specific features
            RefreshMenuReports();
            LayoutToolBar();
            FillPatientButton(Patients.GetPat(PatNumCur)); //Need to do this also for disabling of buttons when no pat is selected.
        }
    }

    #endregion Clinics

    #region Signals

    ///<summary>This is called when any local data becomes outdated.  It's purpose is to tell the other computers to update certain local data.</summary>
    private void DataValid_BecameInvalid(ValidEventArgs e)
    {
        var suffix = Lan.g(nameof(Cache), "Refreshing Caches") + ": ";
        ODEvent.Fire(ODEventType.Cache, suffix);
        if (e.OnlyLocal)
        {
            //Currently used after doing a restore from FormBackup so that the local cache is forcefully updated.
            ODEvent.Fire(ODEventType.Cache, suffix + Lan.g(nameof(Cache), "PrefsStartup"));
            if (!PrefsStartup())
            {
                //??
                return;
            }

            ODEvent.Fire(ODEventType.Cache, suffix + Lan.g(nameof(Cache), "AllLocal"));
            RefreshLocalData(InvalidType.AllLocal); //does local computer only
            return;
        }

        if (!e.ITypes.Contains(InvalidType.Appointment) //local refresh for dates is handled within ContrAppt, not here
            && !e.ITypes.Contains(InvalidType.Task) //Tasks are not "cached" data.
            && !e.ITypes.Contains(InvalidType.TaskPopup))
        {
            RefreshLocalData(e.ITypes); //does local computer
        }

        if (e.ITypes.Contains(InvalidType.Task) || e.ITypes.Contains(InvalidType.TaskPopup))
        {
            if (controlChart?.Visible ?? false)
            {
                ODEvent.Fire(ODEventType.Cache, suffix + Lan.g(nameof(Cache), "Chart Module"));
                controlChart.ModuleSelected(PatNumCur);
            }

            return; //All task signals should already be sent. Sending more Task signals here would cause unnecessary refreshes.
        }

        ODEvent.Fire(ODEventType.Cache, suffix + Lan.g(nameof(Cache), "Inserting Signals"));
        for (var i = 0; i < e.ITypes.Length; i++)
        {
            var signalod = new Signalod
            {
                IType = e.ITypes[i]
            };
            switch (e.ITypes[i])
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

    ///<summary>Manipulates the current lightSignalGrid1 control based on the SigMessages passed in. Pass in a null list in order to simply refresh the lightSignalGrid1 control in its current state (no database call).</summary>
    private void FillSignalButtons(List<SigMessage> listSigMessages)
    {
        if (!DoFillSignalButtons())
        {
            return;
        }

        if (_arraySigButDefs == null)
        {
            _arraySigButDefs = SigButDefs.GetByComputer(ODEnvironment.MachineName);
        }

        var maxButton = _arraySigButDefs.Select(x => x.ButtonIndex).DefaultIfEmpty(-1).Max() + 1;
        var lightGridHeightOld = lightSignalGrid1.Height;
        var lightGridHeightNew = Math.Min(maxButton * 25 + 1, Height - lightSignalGrid1.Location.Y);
        if (lightGridHeightOld != lightGridHeightNew)
        {
            lightSignalGrid1.Visible = false; //"erases" light signal grid that has been drawn on FormOpenDental
            lightSignalGrid1.Height=lightGridHeightNew;
            lightSignalGrid1.Visible = true; //re-draws light signal grid to the correct size.
        }

        if (listSigMessages == null)
        {
            return; //No new SigMessages to process.
        }

        SigButDef sigButDef;
        int row;
        Color color;
        var hadErrorPainting = false;
        for (var i = 0; i < listSigMessages.Count; i++)
        {
            if (listSigMessages[i].AckDateTime.Year > 1880)
            {
                //process ack
                var buttonIndex = lightSignalGrid1.ProcessAck(listSigMessages[i].SigMessageNum);
                if (buttonIndex != -1)
                {
                    sigButDef = SigButDefs.GetByIndex(buttonIndex, _arraySigButDefs);
                    if (sigButDef != null)
                    {
                        try
                        {
                            PaintOnIcon(sigButDef.SynchIcon, Color.White);
                        }
                        catch (Exception ex)
                        {
                            hadErrorPainting = true;
                        }
                    }
                }
            }
            else
            {
                //process normal message
                row = 0;
                color = Color.White;
                var listSigElementDefs = SigElementDefs.GetDefsForSigMessage(listSigMessages[i]);
                for (var j = 0; j < listSigElementDefs.Count; j++)
                {
                    if (listSigElementDefs[j].LightRow != 0)
                    {
                        row = listSigElementDefs[j].LightRow;
                    }

                    if (listSigElementDefs[j].LightColor.ToArgb() != Color.White.ToArgb())
                    {
                        color = listSigElementDefs[j].LightColor;
                    }
                }

                if (row != 0 && color != Color.White)
                {
                    lightSignalGrid1.SetButtonActive(row - 1, color, listSigMessages[i]);
                    sigButDef = SigButDefs.GetByIndex(row - 1, _arraySigButDefs);
                    if (sigButDef != null)
                    {
                        try
                        {
                            PaintOnIcon(sigButDef.SynchIcon, color);
                        }
                        catch (Exception ex)
                        {
                            hadErrorPainting = true;
                        }
                    }
                }
            }
        }

        if (hadErrorPainting)
        {
            ODMessageBox.Show("Error painting on program icon.  Probably too many non-ack'd messages.");
        }
    }

    ///<summary>Refreshes the entire lightSignalGrid1 control to the current state according to the database. This is typically used when the program is first starting up or when a signal is processed for a change to the SigButDef cache.</summary>
    private void FillSignalButtons()
    {
        if (!DoFillSignalButtons())
        {
            return;
        }

        _arraySigButDefs = SigButDefs.GetByComputer(ODEnvironment.MachineName);
        lightSignalGrid1.SetButtons(_arraySigButDefs);
        lightSignalGrid1.Visible = _arraySigButDefs.Length > 0;
        FillSignalButtons(SigMessages.RefreshCurrentButState()); //Get the current SigMessages from the database.
    }

    private bool DoFillSignalButtons()
    {
        if (!Security.IsUserLoggedIn)
        {
            return false;
        }

        if (!lightSignalGrid1.Visible && false)
        {
            //for faster eCW loading
            return false;
        }

        return true;
    }

    ///<summary>Pass in the cellNum as 1-based.</summary>
    private void PaintOnIcon(int cellNum, Color color)
    {
        Graphics g;
        if (_bitmapIcon == null)
        {
            _bitmapIcon = new Bitmap(16, 16);
            g = Graphics.FromImage(_bitmapIcon);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, 15, 15);
            //horizontal
            g.DrawLine(Pens.Black, 0, 0, 15, 0);
            g.DrawLine(Pens.Black, 0, 5, 15, 5);
            g.DrawLine(Pens.Black, 0, 10, 15, 10);
            g.DrawLine(Pens.Black, 0, 15, 15, 15);
            //vertical
            g.DrawLine(Pens.Black, 0, 0, 0, 15);
            g.DrawLine(Pens.Black, 5, 0, 5, 15);
            g.DrawLine(Pens.Black, 10, 0, 10, 15);
            g.DrawLine(Pens.Black, 15, 0, 15, 15);
            g.Dispose();
        }

        if (cellNum == 0)
        {
            return;
        }

        g = Graphics.FromImage(_bitmapIcon);
        var x = 0;
        var y = 0;
        switch (cellNum)
        {
            case 1:
                x = 1;
                y = 1;
                break;
            case 2:
                x = 6;
                y = 1;
                break;
            case 3:
                x = 11;
                y = 1;
                break;
            case 4:
                x = 1;
                y = 6;
                break;
            case 5:
                x = 6;
                y = 6;
                break;
            case 6:
                x = 11;
                y = 6;
                break;
            case 7:
                x = 1;
                y = 11;
                break;
            case 8:
                x = 6;
                y = 11;
                break;
            case 9:
                x = 11;
                y = 11;
                break;
        }

        g.FillRectangle(new SolidBrush(color), x, y, 4, 4);
        var intPtr = _bitmapIcon.GetHicon();
        var icon = Icon.FromHandle(intPtr);
        Icon = (Icon) icon.Clone();
        DestroyIcon(intPtr);
        icon.Dispose();
        g.Dispose();
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);

    private void lightSignalGrid1_ButtonClick(object sender, ODLightSignalGridClickEventArgs e)
    {
        if (e.ActiveSignal != null)
        {
            //user trying to ack an existing light signal
            //Acknowledge all sigmessages in the database which correspond with the button that was just clicked.
            //Only acknowledge sigmessages which have a MessageDateTime prior to the last time we processed signals in the singal timer.
            //This is so that we don't accidentally acknowledge any sigmessages that we are currently unaware of.
            SigMessages.AckButton(e.ButtonIndex + 1, Signalods.DateTRegularPrioritySignalLastRefreshed);
            //Immediately update the signal button instead of waiting on our instance to process its own signals.
            e.ActiveSignal.AckDateTime = DateTime.Now;
            FillSignalButtons([e.ActiveSignal]); //Does not run query.
            return;
        }

        if (e.ButtonDef == null || e.ButtonDef.SigElementDefNumUser == 0 && e.ButtonDef.SigElementDefNumExtra == 0 && e.ButtonDef.SigElementDefNumMsg == 0)
        {
            return; //There is no signal to send.
        }

        //user trying to send a signal
        var sigMessage = new SigMessage
        {
            SigElementDefNumUser = e.ButtonDef.SigElementDefNumUser,
            SigElementDefNumExtra = e.ButtonDef.SigElementDefNumExtra,
            SigElementDefNumMsg = e.ButtonDef.SigElementDefNumMsg
        };
        var sigElementDefUser = SigElementDefs.GetElementDef(e.ButtonDef.SigElementDefNumUser);
        if (sigElementDefUser != null)
        {
            sigMessage.ToUser = sigElementDefUser.SigText;
        }

        SigMessages.Insert(sigMessage);
        FillSignalButtons([sigMessage]); //Does not run query.
        //Let the other computers in the office know to refresh this specific light.
        var signalod = new Signalod
        {
            IType = InvalidType.SigMessages,
            FKeyType = KeyType.SigMessage,
            FKey = sigMessage.SigMessageNum
        };
        Signalods.Insert(signalod);
    }

    private void timerTimeIndic_Tick(object sender, EventArgs e)
    {
        //every minute:
        if (WindowState != FormWindowState.Minimized && controlAppt.Visible)
        {
            controlAppt.TickRefresh();
        }
    }

    ///<summary>Helper method to check if we need to start or stop preprocessing signals</summary>
    private bool IsWorkStationActive()
    {
        var sigInactiveMin = PrefC.GetInt(PrefName.SignalInactiveMinutes);
        if (sigInactiveMin == 0)
        {
            return true;
        }

        var dateTimeToSignalInactive = Security.DateTimeLastActivity + TimeSpan.FromMinutes(sigInactiveMin);
        if (DateTime.Now > dateTimeToSignalInactive)
        {
            return false;
        }

        return true;
    }

    ///<summary>Usually set at 4 to 6 second intervals.</summary>
    private void timerSignals_Tick(object sender, EventArgs e)
    {
        try
        {
            SignalsTick();
        }
        catch (Exception ex)
        {
            SignalsTickExceptionHandler(ex);
        }
    }

    ///<summary>Processes signals.</summary>
    private void SignalsTick(bool isAllInvalidTypes = true)
    {
        try
        {
                
            //This checks if any forms are open that make us want to continue processing signals even if inactive. Currently only FormTerminal.
            //Then check if we're inactive and if so, pause regular signal processing and set the private shutdown signal check variable
            //this gets checked frequently, with each tick.
            if (Application.OpenForms.OfType<FormTerminal>().Count() == 0 && !IsWorkStationActive())
            {
                _onlyProcessHighPrioritySignals = true;
            }

            if (Security.CurUser == null || !Userods.GetIsCacheAllowed())
            {
                //User must be at the log in screen or resetting their password, so no need to process signals. We will need to look for shutdown signals since the last refreshed time when the user attempts to log in. 'Userods.GetIsCacheAllowed()' is used to detect edge case when a User is logged in, but is resetting their password so none of the caches are loaded yet.
                _onlyProcessHighPrioritySignals = true;
            }

            //If signal processing was paused due to inactivity or due to Security.CurUser being null (i.e. login screen visible)
            //and we are now going to process signals again, we need to set _hasSignalProcessingPaused to false
            //If a user signed in, but is resetting their password, Userods.GetIsCacheAllowed() will result in false,
            //check it here to determine if we're returning to processing all signals from that case.
            if (_onlyProcessHighPrioritySignals && IsWorkStationActive() && Security.CurUser != null && Userods.GetIsCacheAllowed())
            {
                string errorMsg;
                //When trying to start signal processing back up again, we will shut down OD if:
                //1. there is a mismatch between the current software version and the program version stored in the db (ProgramVersion pref)
                //2. the UpdateInProgressOnComputerName pref is set (regardless of whether or not the computer name matches this machine name)
                //3. the CorruptedDatabase flag is set
                if (! /* ODBuild.IsDebug() */ false && !IsDbConnectionSafe(out errorMsg))
                {
                    //Running version verses ProgramVersion preference can be different in debug.
                    _timerSignals.Stop();
                    ODMessageBox.Show(this, errorMsg);
                    ProcessKillCommand();
                    return;
                }

                _onlyProcessHighPrioritySignals = false;
            }
        }
        catch
        {
            //Currently do nothing.
        }

        #region Task Preprocessing

        if (!_onlyProcessHighPrioritySignals)
        {
            PreprocessForTasks();
        }

        #endregion Task Preprocessing

        //Signal Processing
        _timerSignals.Stop();
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
                        catch (Exception ex)
                        {
                            var message = "SignalsTick SyncCurUser: " + MiscUtils.GetExceptionText(ex);
                                
                        }
                    });
                }

                //Make a shallow copy of the list of forms that cannot be manipulated while looping through it.
                var listFormODBases = new List<FormODBase>(listODForms);
                //Broadcast to all subscribed signal processors.
                Invoke(() =>
                {
                    for (var i = 0; i < listFormODBases.Count; i++)
                    {
                        try
                        {
                            listFormODBases[i].ProcessSignals(listSignals);
                        }
                        catch (Exception ex)
                        {
                            var message = "ODForm.ProcessSignals Exception: " + MiscUtils.GetExceptionText(ex);
                                
                        }
                    }
                });
            },
            () => { },
            isAllInvalidTypes: !_onlyProcessHighPrioritySignals && isAllInvalidTypes
        );
        //Be careful about doing anything that takes a long amount of computation time after the SignalsTick.
        //The UI will appear invalid for the time it takes any methods to process.
        //STOP! 
        //If you are trying to do something in FormOpenDental that uses a signal, you should use FormOpenDental.OnProcessSignals() instead.
        //This Function is only for processing things at regular intervals IF IT DOES NOT USE SIGNALS.
            
    }

    private void PreprocessForTasks()
    {
        if (_userNumTasks != Security.CurUser.UserNum //The user has changed since the last signal tick was run (when logoff then logon),
            || _listTasksReminders == null || _listTaskNumsNormal == null) //or first time processing signals since the program started.
        {
                
            _userNumTasks = Security.CurUser.UserNum;
            var listTasksRefreshed = Tasks.GetNewTasksThisUser(Security.CurUser.UserNum, Clinics.ClinicNum); //Get all tasks pertaining to current user.
            _listTaskNumsNormal = [];
            _listTasksReminders = [];
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
                    _listTasksReminders.Add(listTasksRefreshed[i]);
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
            controlAppt.RefreshReminders(_listTasksReminders);
            _dateReminderRefresh = DateTime.Today;
                
        }
        //Check to see if a reminder task became due between the last signal interval and the current signal interval.
        else if (_listTasksReminders.FindAll(x => x.DateTimeEntry <= DateTime.Now
                                                  && x.DateTimeEntry >= DateTime.Now.AddSeconds(-PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs))).Count > 0)
        {
            var listTasksDueReminders = _listTasksReminders.FindAll(x => x.DateTimeEntry <= DateTime.Now
                                                                         && x.DateTimeEntry >= DateTime.Now.AddSeconds(-PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs)));
                
            var listSignalods = new List<Signalod>();
            for (var i = 0; i < listTasksDueReminders.Count; i++)
            {
                var signalod = new Signalod
                {
                    IType = InvalidType.TaskList,
                    FKey = listTasksDueReminders[i].TaskListNum,
                    FKeyType = KeyType.Undefined
                };
                listSignalods.Add(signalod);
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
            controlAppt.RefreshReminders(_listTasksReminders);
            _dateReminderRefresh = DateTime.Today;
        }

        RefreshTasksNotification();
    }

    ///<summary>Spawns a new thread to retrieve new signals from the DB. If isAllInvalidTypes is true, update all caches and broadcast signals to all subscribed forms.
    ///Otherwise if isAllInvalidTypes is false, only process important signals</summary>
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

            //Print signal to be processed from this machine
            var listSignalodsToPrint = listSignals.FindAll(x => x.IType == InvalidType.Print && x.FKey == Computers.GetCur().ComputerNum);
            PrintRemoteRequestL.ProcessCompPrintSignal(listSignalodsToPrint); //can handle empty list
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
            catch (Exception ex)
            {
                //If the server cannot be reached, we still need to move the signal processing forward so use local time as a fail-safe.
                dateTimeRefreshed = DateTime.Now;
                    
            }

            Signalods.DateTRegularPrioritySignalLastRefreshed = dateTimeRefreshed;
            Signalods.DateTApptSignalLastRefreshed = dateTimeRefreshed;
                
        });
        threadRefreshSignals.AddExitHandler(_ =>
        {
                
            onDone();
        });
        threadRefreshSignals.Name = "SignalsTick";
        threadRefreshSignals.Start();
    }

    ///<summary>Called when _hasSignalProcessingPaused is true and we are about to start processing signals again.  We may have missed a shutdown workstations signal, so this method will check the version, the update in progress pref, and the corrupt db pref.  Returns false if the OD
    ///instance should be restarted.  The errorMsg out variable will be set to the error message for the first failed check.</summary>
    private bool IsDbConnectionSafe(out string errorMsg)
    {
        errorMsg = "";
        Prefs.RefreshCache(); //this is a db call, but will only happen once when an inactive workstation is re-activated
        //The logic below mimics parts of PrefL.CheckProgramVersion().
        var versionStored = new Version(PrefC.GetString(PrefName.ProgramVersion));
        var versionCurrent = new Version(Application.ProductVersion);
        if (versionStored != versionCurrent)
        {
            errorMsg = Lan.g(this, "You are attempting to run version") + " " + versionCurrent.ToString(3) + ", "
                       + Lan.g(this, "but the database is using version") + " " + versionStored.ToString(3) + ".\r\n\r\n"
                       + Lan.g(this, "You will have to restart") + " " + PrefC.GetString(PrefName.SoftwareName) + " " + Lan.g(this, "to correct the version mismatch.");
            return false;
        }

        var updateComputerName = PrefC.GetString(PrefName.UpdateInProgressOnComputerName);
        if (!string.IsNullOrEmpty(updateComputerName))
        {
            errorMsg = Lan.g(this, "An update is in progress on workstation") + ": '" + updateComputerName + "'.\r\n\r\n"
                       + Lan.g(this, "You will have to restart") + " " + PrefC.GetString(PrefName.SoftwareName) + " " + Lan.g(this, "once the update has finished.");
            return false;
        }

        if (PrefC.GetBool(PrefName.CorruptedDatabase))
        {
            //only happens if the UpdateInProgressOnComputerName is blank and the CorruptedDatabase flag is set, i.e. an update has failed
            errorMsg = Lan.g(this, "Your database is corrupted because an update failed.  Please contact us.  This database is unusable and you will "
                                   + "need to restore from a backup.");
            return false;
        }

        return true;
    }

    ///<summary>Catches an exception from signal processing and sends the first one to HQ.</summary>
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

    ///<summary>Adds the alert items to the alert menu item.</summary>
    private void AddAlertsToMenu()
    {
        //At this point _listAlertItems and _listAlertReads should be user, clinic and subscription filtered.
        //If the counts match this means they have read all AlertItems. 
        //This will result in the 'Alerts' menu item to not be colored.
        var alertCount = _alertItems.Count - _alertItemReads.Count;
        if (alertCount > 99)
        {
            _menuItemAlerts.Text = Lan.g(this, "Alerts") + " (99)";
            _menuItemAlerts.ForeColor = Color.Red;
        }
        else if (alertCount == 0)
        {
            _menuItemAlerts.Text = Lan.g(this, "Alerts") + " (" + alertCount + ")";
            _menuItemAlerts.ForeColor = Color.Black;
        }
        else
        {
            _menuItemAlerts.Text = Lan.g(this, "Alerts") + " (" + alertCount + ")";
            _menuItemAlerts.ForeColor = Color.Red;
        }

            
    }

    ///<summary>This only contains UI signal processing. See Signalods.SignalsTick() for cache updates.</summary>
    protected override void ProcessSignalODs(List<Signalod> signals)
    {
        if (signals.Exists(x => x.IType == InvalidType.Programs))
        {
            RefreshMenuReports();
        }

        if (signals.Exists(x => x.IType == InvalidType.Prefs))
        {
            PrefC.InvalidateVerboseLogging();
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
            var listProvNumsVisible = controlAppt.GetListProvsVisible().Select(x => x.ProvNum).ToList();
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

        #region eClipboard/Kiosk

        if (signals.Exists(x => x.IType == InvalidType.EClipboard))
        {
            ODEvent.Fire(ODEventType.eClipboard);
        }

        #endregion

        #region Refresh

        var invalidTypesArray = Signalods.GetInvalidTypes(signals);
        if (invalidTypesArray.Length > 0)
        {
            RefreshLocalDataPostCleanup(invalidTypesArray);
        }

        #endregion Refresh

        //Sig Messages must be the last code region to run in the process signals method because it changes the application icon.

        #region Sig Messages (In the manual as "Internal Messages")

        //Check to see if any signals are sigmessages.
        var listSigMessageNums = signals.FindAll(x => x.IType == InvalidType.SigMessages && x.FKeyType == KeyType.SigMessage).Select(x => x.FKey).ToList();
        if (listSigMessageNums.Count > 0)
        {
                
            //Any SigMessage iType means we need to refresh our lights or buttons.
            var listSigMessages = SigMessages.GetSigMessages(listSigMessageNums);
            controlManage.LogMsgs(listSigMessages);
            FillSignalButtons(listSigMessages);
            //Need to add a test to this: do not play messages that are over 2 minutes old.
            BeginPlaySoundsThread(listSigMessages);
                
        }

        #endregion Sig Messages
    }

    #endregion Signals

    ///<summary>Will invoke a refresh of tasks on the only instance of FormOpenDental. listRefreshedTaskNotes and listBlockedTaskLists are only used 
    ///for Popup tasks, only used if listRefreshedTasks includes at least one popup task.</summary>
    public static void S_HandleRefreshedTasks(List<Signalod> listSignalodTasks, List<long> listEditedTaskNums, List<Task> listTasksRefreshed,
        List<TaskNote> listTaskNotesRefreshed, List<UserOdPref> listUserOdPrefsBlockedTasks)
    {
        _formOpenDentalSingleton.HandleRefreshedTasks(listSignalodTasks, listEditedTaskNums, listTasksRefreshed, listTaskNotesRefreshed, listUserOdPrefsBlockedTasks);
    }

    ///<summary>Refreshes tasks and pops up as necessary. Invoked from thread callback in OnProcessSignals(). listRefreshedTaskNotes and 
    ///listBlockedTaskLists are only used for Popup tasks, only used if listRefreshedTasks includes at least one popup task.</summary>
    private void HandleRefreshedTasks(List<Signalod> listSignalodsTasks, List<long> listEditedTaskNums, List<Task> listTasksRefreshed,
        List<TaskNote> listTaskNotesRefreshed, List<UserOdPref> listUserOdPrefsBlockedTasks)
    {
        var hasChangedReminders = UpdateTaskMetaData(listEditedTaskNums, listTasksRefreshed);
        RefreshTasksNotification();
        RefreshOpenTasksOrPopupNewTasks(listSignalodsTasks, listTasksRefreshed, listTaskNotesRefreshed, listUserOdPrefsBlockedTasks);
        //Refresh the appt module if reminders have changed, even if the appt module not visible.
        //The user will load the appt module eventually and these refreshes are the only updates the appointment module receives for reminders.
        if (hasChangedReminders)
        {
            controlAppt.RefreshReminders(_listTasksReminders);
            _dateReminderRefresh = DateTime.Today;
        }
    }

    ///<summary>Updates the class-wide meta data used for updating the task notification UI elements.
    ///Returns true if a reminder task has changed.  Otherwise; false.</summary>
    private bool UpdateTaskMetaData(List<long> listEditedTaskNums, List<Task> listTasksRefreshed)
    {
        //Check to make sure there are edited task nums passed in and that the meta data lists have been initialized by the signal processor.
        if (listEditedTaskNums == null || _listTasksReminders == null || _listTaskNumsNormal == null)
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

            var taskReminderOld = _listTasksReminders.FirstOrDefault(x => x.TaskNum == editedTaskNum);
            if (taskReminderOld != null)
            {
                //The task is a reminder which is relevant to the current user.
                hasChangedReminders = true;
                _listTasksReminders.RemoveAll(x => x.TaskNum == editedTaskNum); //Remove the old copy of the task.
                if (taskForUser != null)
                {
                    //The updated reminder task is relevant to the current user.
                    _listTasksReminders.Add(taskForUser); //Add the updated reminder task into the list (replacing the old reminder task).
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
                    _listTasksReminders.Add(taskForUser);
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

    private void GotoModule_ModuleSelected(EnumModuleType moduleType, DateTime? dateSelected = null, List<long> listPinApptNums = null,
        long selectedAptNum = 0, long claimNum = 0, long patNum = 0, long docNum = 0, bool doShowSearch = false)
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
                if (HL7Defs.IsExistingHL7Enabled())
                {
                    var hL7Def = HL7Defs.GetOneDeepEnabled();
                    if (hL7Def.ShowDemographics == HL7ShowDemographics.Hide)
                    {
                        controlFamilyEcw.Visible = true;
                        ActiveControl = controlFamilyEcw;
                        controlFamilyEcw.ModuleSelected(PatNumCur);
                    }
                    else
                    {
                        controlFamily.InitializeOnStartup();
                        controlFamily.Visible = true;
                        ActiveControl = controlFamily;
                        controlFamily.ModuleSelected(PatNumCur);
                    }
                }
                else
                {
                    controlFamily.InitializeOnStartup();
                    controlFamily.Visible = true;
                    ActiveControl = controlFamily;
                    controlFamily.ModuleSelected(PatNumCur);
                }

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
        controlFamilyEcw.Visible = false;
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

        if (controlFamilyEcw.Visible)
        {
            controlFamilyEcw.ModuleSelected(PatNumCur);
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

        userControlTasks1.RefreshPatTicketsIfNeeded();
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
                    dateSelected = DateTime.Now;
                }

                dateSelected = dateTemp;
            }
            else
            {
                dateSelected = appointment.AptDateTime;
            }

            PatNumCur = appointment.PatNum; //OnPatientSelected(apt.PatNum);
            FillPatientButton(Patients.GetPat(PatNumCur));
            GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, dateSelected: dateSelected, selectedAptNum: appointment.AptNum);
        }
    }

    private void menuItemLogOff_Click(object sender, EventArgs e)
    {
        NullUserCheck("menuItemLogOff_Click");

        if (!AreYouSurePrompt(Security.CurUser.UserNum, Lan.g(this, "Are you sure you would like to log off?")))
        {
            return;
        }

        LogOffNow(false);
    }

    private bool AreYouSurePrompt(long userNum, string message)
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

    private void menuItemPassword_Click(object sender, EventArgs e)
    {
        SecurityL.ChangePassword(false);
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
        if (!Security.IsAuthorized(EnumPermType.PrinterSetup))
        {
            return;
        }

        using var formPrinterSetup = new FormPrinterSetup();

        formPrinterSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.PrinterSetup, 0, "Printers");
    }

    private void menuItemGraphics_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.GraphicsEdit))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;

        using var formGraphics = new FormGraphics();

        formGraphics.ShowDialog();

        Cursor = Cursors.Default;

        if (formGraphics.DialogResult == DialogResult.OK)
        {
            controlChart.InitializeLocalData();

            RefreshCurrentModule();
        }
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

    private void menuItemExit_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    //Setup
    private void menuItemPreferences_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formPreferences = new FormPreferences();
        if (formPreferences.ShowDialog() == DialogResult.OK)
        {
            if (PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs) == 0)
            {
                _timerSignals.Enabled = false;
                _onlyProcessHighPrioritySignals = true;
            }
            else
            {
                _timerSignals.Interval = PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs) * 1000;
                _timerSignals.Enabled = true;
            }
        }

        FillPatientButton(Patients.GetPat(PatNumCur));
        RefreshCurrentModule(true);
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Preferences");
    }

    private void menuItemApptFieldDefs_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formApptFieldDefs = new FormApptFieldDefs();
        formApptFieldDefs.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Appointment Field Defs");
    }

    private void menuItemApptRules_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formApptRules = new FormApptRules();
        formApptRules.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Appointment Rules");
    }

    private void menuItemApptTypes_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formApptTypes = new FormApptTypes();
        formApptTypes.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Appointment Types");
    }

    private void menuItemApptViews_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formApptViews = new FormApptViews();
        formApptViews.ShowDialog();
        RefreshCurrentModule(true);
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Appointment Views");
    }

    private void menuItemAlertCategories_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SecurityAdmin))
        {
            return;
        }

        using var formAlertCategorySetup = new FormAlertCategorySetup();
        formAlertCategorySetup.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, "Alert Categories");
    }

    private void menuItemAllocations_Click(object sender, EventArgs e)
    {
        //All security is inside the window
        using var formAllocationsSetup = new FormAllocationsSetup();
        formAllocationsSetup.ShowDialog();
    }

    private void menuItemAutoCodes_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formAutoCode = new FormAutoCode();
        formAutoCode.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Auto Codes");
    }

    private void menuItemAutomation_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formAutomation = new FormAutomation();
        formAutomation.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Automation");
    }

    private void menuItemAutoNotes_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.AutoNoteQuickNoteEdit))
        {
            return;
        }

        using var formAutoNotes = new FormAutoNotes();
        formAutoNotes.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.AutoNoteQuickNoteEdit, 0, "Auto Notes Setup");
    }

    private void menuItemClaimForms_Click(object sender, EventArgs e)
    {
        if (false)
        {
            MsgBox.Show(this, "Claim Forms feature is unavailable when data path A to Z folder is disabled.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formClaimForms = new FormClaimForms();
        formClaimForms.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Claim Forms");
    }

    private void menuItemClearinghouses_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formClearinghouses = new FormClearinghouses();
        formClearinghouses.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Clearinghouses");
    }

    private void menuItemCodeGroups_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formCodeGroups = new FormCodeGroups();
        formCodeGroups.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Code Groups");
    }

    private void menuItemDiscountPlans_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formDiscountPlans = new FormDiscountPlans();
        formDiscountPlans.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Discount Plans");
    }

    private void menuItemComputers_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formComputers = new FormComputers();
        formComputers.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Computers");
    }

    private void menuItemDataPath_Click(object sender, EventArgs e)
    {
        //Security is handled from within the form.
        //Audit trail is handled within the form due to being able to access FormPath from multiple areas.
        using var formPath = new FormPath();
        formPath.ShowDialog();
        RefreshCurrentModule();
    }

    private void menuItemPayPlanTemplates_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formPayPlanTemplates = new FormPayPlanTemplates();
        formPayPlanTemplates.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Pay Plan Templates");
    }

    private void menuItemDefinitions_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        using var formDefinitions = new FormDefinitions(DefCat.AccountColors); //just the first cat.
        formDefinitions.ShowDialog();
        RefreshCurrentModule(true);
        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, "Definitions");
    }

    private void menuItemDisplayFields_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formDisplayFieldCategories = new FormDisplayFieldCategories();
        formDisplayFieldCategories.ShowDialog();
        RefreshCurrentModule(true);
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Display Fields");
    }

    private void menuItemEForms_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            //For comparison, sheets also uses Setup permission.
            //The Sheets permission is used for editing sheets, not sheetDefs.
            return;
        }

        var frmEFormDefs = new FrmEFormDefs();
        frmEFormDefs.ShowDialog();
        //Nothing to refresh
        SecurityLogs.MakeLogEntry(EnumPermType.SheetEdit, 0, "EForms");
    }

    private void menuItemEmail_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formEmailAddresses = new FormEmailAddresses();
        formEmailAddresses.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Email");
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

        //Users that are clinic restricted are not allowed to setup Fee Schedule Groups.
        if (Security.CurUser.ClinicIsRestricted)
        {
            MsgBox.Show(this, "You are restricted from accessing certain clinics.  Only user without clinic restrictions can edit Fee Schedule Groups.");
            return;
        }

        using var formFeeSchedGroups = new FormFeeSchedGroups();
        formFeeSchedGroups.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.FeeSchedEdit, 0, "Fee Schedule Groups");
    }

    private void menuItemFHIR_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;
        using var formFHIRSetup = new FormFHIRSetup();
        formFHIRSetup.ShowDialog();
        Cursor = Cursors.Default;
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "API/FHIR");
    }

    private void menuItemHIE_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formHieSetup = new FormHieSetup();
        formHieSetup.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "HIE");
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

    private void menuItemScanning_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formImagingSetup = new FormImagingSetup();
        formImagingSetup.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Imaging");
    }

    private void menuItemInsCats_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formInsCatsSetup = new FormInsCatsSetup();
        formInsCatsSetup.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Insurance Categories");
    }

    private void menuItemInsFilingCodes_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formInsFilingCodes = new FormInsFilingCodes();
        formInsFilingCodes.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Insurance Filing Codes");
    }

    private void menuItemLaboratories_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formLaboratories = new FormLaboratories();
        formLaboratories.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Laboratories");
    }

    private void menuItemMessaging_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formMessagingSetup = new FormMessagingSetup();
        formMessagingSetup.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Messaging");
    }

    private void menuItemMessagingButs_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formMessagingButSetup = new FormMessagingButSetup();
        formMessagingButSetup.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Messaging");
    }

    private void menuItemMounts_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formMountDefs = new FormMountDefs();

        formMountDefs.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Mounts");
    }

    private void menuItemImagingDevices_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formImagingDevices = new FormImagingDevices();

        formImagingDevices.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Imaging Devices");
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

        var isManualRefreshEnabledPreviously = PrefC.GetBool(PrefName.EnterpriseManualRefreshMainTaskLists);
        if (userControlTasks1.Visible && PrefC.GetBool(PrefName.EnterpriseManualRefreshMainTaskLists) != isManualRefreshEnabledPreviously)
        {
            userControlTasks1.InitializeOnStartup();
        }

        if (PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs) == 0)
        {
            _timerSignals.Enabled = false;
            _onlyProcessHighPrioritySignals = true;
        }
        else
        {
            _timerSignals.Interval = PrefC.GetInt(PrefName.ProcessSigsIntervalInSecs) * 1000;
            _timerSignals.Enabled = true;
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

    private void menuItemPayerIDs_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formElectIDs = new FormElectIDs();
        formElectIDs.IsSelectMode = false;
        formElectIDs.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Payer IDs");
    }

    private void menuItemInsBlueBook_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formInsBlueBookRules = new FormInsBlueBookRules();
        formInsBlueBookRules.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Insurance Blue Book");
    }

    private void menuItemPractice_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formPractice = new FormPractice();
        formPractice.ShowDialog();
        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Practice Info");
        if (formPractice.DialogResult != DialogResult.OK)
        {
            return;
        }

        moduleBar.RefreshButtons();
        RefreshCurrentModule();
    }

    private void menuItemProblems_Click(object sender, EventArgs e)
    {
        using var formDiseaseDefs = new FormDiseaseDefs();

        formDiseaseDefs.ShowDialog();
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
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formAsapSetup = new FormAsapSetup();

        formAsapSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "ASAP List Setup");
    }

    private void menuItemConfirmations_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formConfirmationSetup = new FormConfirmationSetup();

        formConfirmationSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Confirmation Setup");
    }

    private void menuItemInsVerify_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formInsVerificationSetup = new FormInsVerificationSetup();

        formInsVerificationSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Insurance Verification");
    }

    private void menuItemRecall_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formRecallSetup = new FormRecallSetup();

        formRecallSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Recall");
    }

    private void menuItemRecallTypes_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formRecallTypes = new FormRecallTypes();

        formRecallTypes.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Recall Types");
    }

    private void menuItemReactivation_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formReactivationSetup = new FormReactivationSetup();

        formReactivationSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Reactivation");
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

    private void menuItemRequiredFields_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formRequiredFields = new FormRequiredFields();

        formRequiredFields.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Required Fields");
    }

    private void menuItemSched_Click(object sender, EventArgs e)
    {
        using var formSchedule = new FormSchedule();

        formSchedule.ShowDialog();
    }

    private void MenuItemScheduledProcesses_Click(object sender, EventArgs e)
    {
        using var formScheduledProcesses = new FormScheduledProcesses();

        formScheduledProcesses.ShowDialog();
    }

    public static void S_MenuItemSecurity_Click(object sender, EventArgs e)
    {
        _formOpenDentalSingleton.menuItemSecurity_Click(sender, e);
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
        RefreshMenuDashboards();
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

    private void menuItemSheets_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formSheetDefs = new FormSheetDefs();

        formSheetDefs.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Sheets");
    }

    private void menuItemEasy_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ShowFeatures))
        {
            return;
        }

        using var formShowFeatures = new FormShowFeatures();
        formShowFeatures.ShowDialog();

        controlAccount.LayoutToolBar();

        RefreshCurrentModule(true);

        SecurityLogs.MakeLogEntry(EnumPermType.ShowFeatures, 0, "Show Features");
    }

    private void menuItemSpellCheck_Click(object sender, EventArgs e)
    {
        using var formSpellCheck = new FormSpellCheck();

        formSpellCheck.ShowDialog();
    }

    private void menuItemTimeCards_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formTimeCardSetup = new FormTimeCardSetup();

        formTimeCardSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Time Card Setup");
    }

    private void menuItemTask_Click(object sender, EventArgs e)
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

        if (userControlTasks1.Visible)
        {
            userControlTasks1.InitializeOnStartup();
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

    private void menuItemWebForm_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formWebFormSetup = new FormWebFormSetup();

        formWebFormSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Web Forms Setup");
    }

    private void menuItemProcCodes_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes(true);

        formProcCodes.ShowDialog();
    }

    private void menuItemAllergies_Click(object sender, EventArgs e)
    {
        using var formAllergySetup = new FormAllergySetup();

        formAllergySetup.ShowDialog();
    }

    private void menuItemClinics_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ClinicEdit))
        {
            return;
        }

        using var formClinics = new FormClinics();

        formClinics.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.ClinicEdit, 0, "Clinics");

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

    private void menuItemContacts_Click(object sender, EventArgs e)
    {
        using var formContacts = new FormContacts();

        formContacts.ShowDialog();
    }

    private void menuItemCounties_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formCounties = new FormCounties();

        formCounties.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Counties");
    }

    private void menuItemEmployees_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formEmployeeSelect = new FormEmployeeSelect();

        formEmployeeSelect.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Employees");
    }

    private void menuItemEmployers_Click(object sender, EventArgs e)
    {
        using var formEmployers = new FormEmployers();

        formEmployers.ShowDialog();
    }

    private void menuItemCarriers_Click(object sender, EventArgs e)
    {
        using var formCarriers = new FormCarriers();

        formCarriers.ShowDialog();

        RefreshCurrentModule();
    }

    private void menuItemInsPlans_Click(object sender, EventArgs e)
    {
        using var formInsPlans = new FormInsPlans();

        formInsPlans.ShowDialog();

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

    private void menuItemMedications_Click(object sender, EventArgs e)
    {
        using var formMedications = new FormMedications();

        formMedications.ShowDialog();
    }

    private void menuItemPharmacies_Click(object sender, EventArgs e)
    {
        using var formPharmacies = new FormPharmacies();

        formPharmacies.ShowDialog();
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

        using var formProviderSetup = new FormProviderSetup();

        formProviderSetup.ShowDialog();

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

    private void menuItemReportsGraphic_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.GraphicalReports))
        {
            return;
        }

        if (_formDashboardEditTab != null)
        {
            _formDashboardEditTab.BringToFront();
            return;
        }

        //on extremely large dbs, the ctor can take a few seconds to load, so show the wait cursor.
        Cursor = Cursors.WaitCursor;
        //Check if the user has permission to view all providers in production and income reports
        var hasAllProvsPermission = Security.IsAuthorized(EnumPermType.ReportProdIncAllProviders, true);
        if (!hasAllProvsPermission && Security.CurUser.ProvNum == 0)
        {
            if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "The current user must be a provider or have the 'All Providers' permission to view provider reports. Continue?"))
            {
                return;
            }
        }

        _formDashboardEditTab = new FormDashboardEditTab(Security.CurUser.ProvNum, !Security.IsAuthorized(EnumPermType.ReportProdIncAllProviders, true)) {IsEditMode = false};
        _formDashboardEditTab.FormClosed += (_, _) => { _formDashboardEditTab = null; };
        Cursor = Cursors.Default;
        _formDashboardEditTab.Show();
    }

    private void menuItemReportsUserQuery_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.UserQuery))
        {
            return;
        }

        if (Security.IsAuthorized(EnumPermType.UserQueryAdmin, true))
        {
            SecurityLogs.MakeLogEntry(EnumPermType.UserQuery, 0, Lan.g(this, "User query form accessed."));
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
            formQueryFavorites.ShowDialog();
            if (formQueryFavorites.DialogResult == DialogResult.OK)
            {
                ExecuteQueryFavorite(formQueryFavorites.UserQueryCur);
            }
        }
    }

    private void menuItemReportsFilteredClick_Click(object sender, EventArgs e)
    {
        using var formReportsFiltered = new FormReportsFiltered();
        formReportsFiltered.ShowDialog();
        if (formReportsFiltered.DialogResult == DialogResult.OK)
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
        formQueryFavorites.ShowDialog();
        if (formQueryFavorites.DialogResult == DialogResult.OK)
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

    private void menuItemReportsActivityLog_Click(object sender, EventArgs e)
    {
        using var formActivityLog = new FormActivityLog();
        formActivityLog.ShowDialog();
    }

    private void UpdateUnfinalizedPayCount(List<Signalod> listSignalods)
    {
        if (listSignalods.Count == 0)
        {
            _menuItemUnfinalizedPay.Text = Lan.g(this, "Unfinalized Payments");
            return;
        }

        var signalod = listSignalods.OrderByDescending(x => x.SigDateTime).First();
        _menuItemUnfinalizedPay.Text = Lan.g(this, "Unfinalized Payments") + ": " + signalod.MsgValue;
    }

    private void RefreshMenuReports()
    {
        _menuItemUserQuery.Available = Security.IsAuthorized(EnumPermType.UserQueryAdmin, true);
        _menuItemQueryFavorites.Available = Security.IsAuthorized(EnumPermType.UserQuery, true);
        //Find the index of the last separator which separates the static menu items from the dynamic menu items.
        var separatorIndex = -1;
        for (var i = 0; i < _menuItemReports.DropDown.Items.Count; i++)
        {
            if (_menuItemReports.DropDown.Items[i].Text == "-")
            {
                separatorIndex = i;
            }
        }

        //Remove dynamic items and separator.  Leave hard coded items.
        if (separatorIndex != -1)
        {
            for (var i = _menuItemReports.DropDown.Items.Count - 1; i >= separatorIndex; i--)
            {
                _menuItemReports.DropDown.Items.RemoveAt(i);
            }
        }

        var listToolButItems = ToolButItems.GetForToolBar(EnumToolBar.ReportsMenu);
        if (true)
        {
            listToolButItems.RemoveAll(x => ProgramProperties.GetPropForProgByDesc(x.ProgramNum, ProgramProperties.PropertyDescs.ClinicHideButton, Clinics.ClinicNum) != null);
        }

        if (listToolButItems.Count == 0)
        {
            //if there is one or more items, the add button further down will handle the layout.
            var menuStripOD = MenuStripOD.GetMenuStripOD(_menuItemReports);
            menuStripOD?.LayoutItems();

            return; //Return early to avoid adding a useless separator in the menu.
        }

        //Add separator, then dynamic items to the bottom of the menu.
        _menuItemReports.AddSeparator(); //Separator
        var newSeparatorIndex = _menuItemReports.DropDown.Items.Count - 1; //Determine the seperator's row by index
        _menuItemReports.DropDown.Items[newSeparatorIndex].Text = "-"; //Add text to assist with identification of the new separator
        listToolButItems.Sort(ToolButItem.Compare); //Alphabetical order
        for (var i = 0; i < listToolButItems.Count; i++)
        {
            var menuItem = new MenuItemOD(listToolButItems[i].ButtonText, menuReportLink_Click);
            menuItem.Tag = listToolButItems[i];
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
        //Permission already validated.
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
            case ReportNonModalSelection.WebSchedAppointments:
                var formWebSchedAppts = new FormWebSchedAppts(true, true, true, true);
                formWebSchedAppts.Show();
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
                //Both FormRpProcNotBilledIns and FormClaimsSend are non-modal.
                //If both forms are open try and update FormClaimsSend to reflect any newly created claims.
                formRpProcNotBilledIns.OnPostClaimCreation += () => controlManage.TryRefreshFormClaimSend();
                formRpProcNotBilledIns.FormClosed += (_, _) => { ODEvent.Fired -= formProcNotBilled_GoToChanged; };
                ODEvent.Fired += formProcNotBilled_GoToChanged;
                formRpProcNotBilledIns.Show(); //FormProcSend has a GoTo option and is shown as a non-modal window.
                formRpProcNotBilledIns.BringToFront();
                break;
            case ReportNonModalSelection.ODProcsOverpaid:
                var formRpProcOverpaid = new FormRpProcOverpaid();
                formRpProcOverpaid.Show();
                break;
            case ReportNonModalSelection.DPPOvercharged:
                if (_formRpDPPOvercharged == null || _formRpDPPOvercharged.IsDisposed)
                {
                    _formRpDPPOvercharged = new FormRpDPPOvercharged();
                }

                _formRpDPPOvercharged.Show();
                if (_formRpDPPOvercharged.WindowState == FormWindowState.Minimized)
                {
                    _formRpDPPOvercharged.WindowState = FormWindowState.Normal;
                }

                _formRpDPPOvercharged.BringToFront();
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

    private void UserQuery_ClickEvent(object sender, EventArgs e)
    {
        var userQuery = (UserQuery) ((MenuItem) sender).Tag;
        ExecuteQueryFavorite(userQuery, true);
    }

    private void ExecuteQueryFavorite(UserQuery userQuery, bool doRunPrompt = false)
    {
        SecurityLogs.MakeLogEntry(EnumPermType.UserQuery, 0, Lan.g(this, "User query form accessed."));
        //ReportSimpleGrid report=new ReportSimpleGrid();
        if (doRunPrompt && userQuery.IsPromptSetup && UserQueries.ParseSetStatements(userQuery.QueryText).Count > 0)
        {
            //if the user is not a query admin, they will not have the ability to edit 
            //the query before it is run, so show them the SET statement edit window.
            using var formQueryParser = new FormQueryParser(userQuery);
            formQueryParser.ShowDialog();
            if (formQueryParser.DialogResult != DialogResult.OK)
            {
                //report.Query=userQuery.QueryText;
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

    private void menuItemPrintScreen_Click(object sender, EventArgs e)
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
            return;
        }
    }

    //MiscTools
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
        //Security log entries are made from within the form.
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

    private void menuItemEditTestModeOverrides_Click(object sender, EventArgs e)
    {
        using var formEditTestModeOverrides = new FormEditTestModeOverrides();

        formEditTestModeOverrides.ShowDialog();
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

        Computers.ClearAllHeartBeats(ODEnvironment.MachineName);

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
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formFinanceCharges = new FormFinanceCharges();

        formFinanceCharges.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Run Finance Charges");
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
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formLateCharges = new FormLateCharges();

        formLateCharges.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Late Charges window");
    }

    private void menuItemOnlinePayments_Click(object sender, EventArgs e)
    {
        var formOnlinePayments = new FormOnlinePayments();

        formOnlinePayments.Show();
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

    private void menuItemScreening_Click(object sender, EventArgs e)
    {
        using var formScreenGroups = new FormScreenGroups();
        formScreenGroups.ShowDialog();
    }

    private void menuItemWebForms_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.WebFormAccess))
        {
            return;
        }

        var formWebForms = new FormWebForms();
        
        formWebForms.Show();
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

        if (userControlDashboard.Visible)
        {
            MsgBox.Show("Please restart to fix the Dashboard layout.");
        }
    }

    public static void S_TaskNumLoad(long taskNum)
    {
        var task = Tasks.GetOne(taskNum);
        if (task == null)
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
        //If Ortho Auto Claims has closed and we're in the account module then refresh patient account rows
        //Don't refresh if not in the account module, ModuleSelected is called when opening the account module
        if (controlAccount.Visible)
        {
            //ModuleSelected refreshes account row data and then calls LayoutPanelsAndRefreshMainGrids to fill rows
            controlAccount.ModuleSelected(PatNumCur);
        }
    }

    private void RefreshMenuDashboards()
    {
        var listSheetDefsDashboards = SheetDefs.GetWhere(x => x.SheetType == SheetTypeEnum.PatientDashboardWidget
                                                              && Security.IsAuthorized(EnumPermType.DashboardWidget, x.SheetDefNum, true), true);
        var isAuthorizedForSetup = Security.IsAuthorized(EnumPermType.Setup, true);
        this.InvokeIfRequired(() =>
        {
            _menuItemPatDashboards.DropDown.Items.Clear();
            if (listSheetDefsDashboards.Count > 28)
            {
                //This number of items+line+Setup will fit in a 990x735 form.
                _menuItemPatDashboards.Click -= OpenDashboardSelect; //Make sure we only subscribe once.
                _menuItemPatDashboards.Click += OpenDashboardSelect;
                return;
            }

            var listOpenDashboardsSheetDefNums = userControlDashboard.ListOpenWidgets.Select(x => x.SheetDefWidget.SheetDefNum).ToList();
            var menuItem = new MenuItemOD(Lan.g("MainMenu", "Dashboard Setup"), OpenDashboardSetup);
            if (!isAuthorizedForSetup)
            {
                menuItem.Enabled = false;
            }

            _menuItemPatDashboards.Add(menuItem);
            if (listSheetDefsDashboards.Count > 0)
            {
                _menuItemPatDashboards.AddSeparator();
            }

            for (var i = 0; i < listSheetDefsDashboards.Count; i++)
            {
                menuItem = new MenuItemOD(listSheetDefsDashboards[i].Description, DashboardMenuClick);
                menuItem.Tag = listSheetDefsDashboards[i];
                if (listOpenDashboardsSheetDefNums.Contains(listSheetDefsDashboards[i].SheetDefNum))
                {
                    //Currently open Dashboard.
                    menuItem.Checked = true;
                }

                _menuItemPatDashboards.Add(menuItem);
            }
        });
    }

    private void OpenDashboardSelect(object sender, EventArgs e)
    {
        using var formDashboardWidgets = new FormDashboardWidgets(); //Open the LaunchDashboard window.
        if (formDashboardWidgets.ShowDialog() == DialogResult.OK && formDashboardWidgets.SheetDefDashboardWidget != null)
        {
            TryLaunchPatientDashboard(formDashboardWidgets.SheetDefDashboardWidget);
        }

        RefreshMenuDashboards();
    }

    private void OpenDashboardSetup(object sender, EventArgs e)
    {
        using var formDashboardWidgetSetup = new FormDashboardWidgetSetup();
        formDashboardWidgetSetup.ShowDialog();
        RefreshMenuDashboards();
    }

    ///<summary>Opens a UserControlDashboardWidget, closing the previously selected UserControlDashboardWidget if one is already open.  If the user clicked on the menu item corresponding to the currently open Patient Dashboard, this means "Close".</summary>
    private void DashboardMenuClick(object sender, EventArgs e)
    {
        if (sender.GetType() != typeof(MenuItemOD) || ((MenuItemOD) sender).Tag == null || ((MenuItemOD) sender).Tag.GetType() != typeof(SheetDef))
        {
            return;
        }

        var sheetDefWidgetNew = (SheetDef) ((MenuItemOD) sender).Tag;
        var opened = TryLaunchPatientDashboard(sheetDefWidgetNew); //Open the newly selected Patient Dashboard.
        if (opened)
        {
            MsgBox.Show(this, "You will probably need to restart to set the layout of the new Dashboard.");
        }
        else
        {
            //closed existing.
        }
    }

    ///<summary>Opens a UserControlDashboardWidget.  The user's permissions should be validated prior to calling this method.</summary>
    private bool TryLaunchPatientDashboard(SheetDef sheetDefWidget)
    {
        if (userControlDashboard.IsInitialized)
        {
            if (userControlDashboard.ListOpenWidgets.Any(x => x.Name == SOut.Long(sheetDefWidget.SheetDefNum)))
            {
                //Clicked on the currently open Patient Dashboard.  This means "Close the Patient Dashboard".
                userControlDashboard.CloseDashboard(false); //Causes userodpref to be deleted.
                OnResizeEnd(EventArgs.Empty);
                return false;
            }

            //Changing which Patient Dashboard is being shown.  First add the new one, then close the old, and update user pref.
            //This order of operations helps avoid unnecessary UI flicker and slowness because we don't actually close the entire Dashboard control.
            var userOdPrefDashboard = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.Dashboard).FirstOrDefault();
            var listUserControlDashboardWidgets = userControlDashboard.ListOpenWidgets;
            userControlDashboard.AddWidget(sheetDefWidget);
            for (var w = 0; w < listUserControlDashboardWidgets.Count; w++)
            {
                listUserControlDashboardWidgets[w].CloseWidget();
            }

            ResizeDashboard();
            var userOdPref = userOdPrefDashboard.Clone();
            userOdPrefDashboard.Fkey = sheetDefWidget.SheetDefNum;
            if (UserOdPrefs.Update(userOdPrefDashboard, userOdPref))
            {
                //Only need to signal cache refresh on change.
                DataValid.SetInvalid(InvalidType.UserOdPrefs);
            }

            RefreshMenuDashboards();
        }
        else
        {
            var userOdPrefDashboard = new UserOdPref
            {
                //If Patient Dashboard was not open, so we need a new user pref for the current user.
                UserNum = Security.CurUser.UserNum,
                Fkey = sheetDefWidget.SheetDefNum,
                FkeyType = UserOdFkeyType.Dashboard,
                ClinicNum = Clinics.ClinicNum
            };
            if (Security.CurUser.UserNum != 0)
            {
                //If the userNum is 0 for the following command it will delete all Patient Dashboard UserOdPrefs!
                //if any Patient Dashboard UserOdPrefs already exists for this user, remove them. This could happen due to a previous concurrency bug.
                UserOdPrefs.DeleteForValueString(Security.CurUser.UserNum, UserOdFkeyType.Dashboard, "");
            }

            userOdPrefDashboard.UserOdPrefNum = UserOdPrefs.Insert(userOdPrefDashboard); //Pre-insert for PK.
            DataValid.SetInvalid(InvalidType.UserOdPrefs);
            try
            {
                InitDashboards(Security.CurUser.UserNum, userOdPrefDashboard);
            }
            catch (NotImplementedException niex)
            {
                ODMessageBox.Show(this, "Error loading Patient Dashboard:\r\n" + niex.Message + "\r\nCorrect errors in Dashboard Setup.");
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error loading Patient Dashboard: " + ex.Message, ex); //So we get bug submission.
            }
        }

        OnResizeEnd(EventArgs.Empty);
        return userControlDashboard.IsInitialized;
    }

    ///<summary>Determines if there is a user preference for which Dashboard to open on startup, and launches it if the user has permissions to launch the dashboard.</summary>
    private void InitDashboards(long userNum, UserOdPref userOdPrefDashboard = null)
    {
        var isOpenedManually = userOdPrefDashboard != null;
        userOdPrefDashboard ??= UserOdPrefs.GetByUserAndFkeyType(userNum, UserOdFkeyType.Dashboard).FirstOrDefault();
        if (userOdPrefDashboard == null)
        {
            return; //User didn't have the dashboard open the last time logged out.
        }

        if (userControlTasks1.Visible && ComputerPrefs.LocalComputer.TaskDock == 1)
        {
            //Tasks are docked right
            this.InvokeIfRequired(() =>
            {
                MsgBox.Show(this, "Dashboards are disabled when Tasks are docked to the right.");
                if (Security.CurUser.UserNum != 0)
                {
                    //If the userNum is 0 for the following command it will delete all Patient Dashboard UserOdPrefs!
                    //Stop the Patient Dashboard from attempting to open on next login.
                    UserOdPrefs.DeleteForValueString(Security.CurUser.UserNum, UserOdFkeyType.Dashboard, "");
                    DataValid.SetInvalid(InvalidType.UserOdPrefs);
                }
            });
            return;
        }

        var sheetDefDashboard = GetUserDashboard(userOdPrefDashboard);
        if (sheetDefDashboard == null)
        {
            //Couldn't find the SheetDef, no sense trying to initialize the Patient Dashboard.
            if (isOpenedManually)
            {
                //Only prompt if user attempted to open a Patient Dashboard from the menu.
                this.InvokeIfRequired(() => { MsgBox.Show(this, "Patient Dashboard could not be found."); });
            }

            return;
        }

        //Pass in SheetDef describing Dashboard layout.
        userControlDashboard.Initialize(sheetDefDashboard, () => { this.InvokeIfRequired(LayoutControls); }
            , () =>
            {
                //What to do when the user closes the dashboard.
                if (Security.CurUser.UserNum != 0)
                {
                    //If the userNum is 0 for the following command it will delete all Patient Dashboard UserOdPrefs!
                    //Stop the Patient Dashboard from attempting to open on next login.
                    UserOdPrefs.DeleteForValueString(Security.CurUser.UserNum, UserOdFkeyType.Dashboard, "");
                    DataValid.SetInvalid(InvalidType.UserOdPrefs);
                }

                RefreshMenuDashboards();
                if (controlAppt.Visible)
                {
                    //Ensure appointment view redraws.
                    controlAppt.LayoutControls();
                }
            }
        );

        RefreshMenuDashboards();
    }

    private static SheetDef GetUserDashboard(UserOdPref userOdPrefDashboard)
    {
        if (userOdPrefDashboard == null)
        {
            return null;
        }

        var sheetDefDashboardNum = userOdPrefDashboard.Fkey;
        var sheetDefDashboard = SheetDefs.GetFirstOrDefault(x => x.SheetDefNum == sheetDefDashboardNum);
        if (sheetDefDashboard == null)
        {
            //The linked Patient Dashboard for this user no longer exists.  Clean up the UserOdPref.
            if (userOdPrefDashboard.UserNum != 0)
            {
                //Defensive to ensure all Patient Dashboard userprefs are not deleted.
                UserOdPrefs.DeleteForValueString(userOdPrefDashboard.UserNum, UserOdFkeyType.Dashboard, string.Empty); //All Dashboard userodprefs for this user.
            }
            else
            {
                UserOdPrefs.Delete(userOdPrefDashboard.UserOdPrefNum); //Otherwise, be safe and only delete this one userpref.
            }

            DataValid.SetInvalid(InvalidType.UserOdPrefs);
        }
        else if (sheetDefDashboard.SheetType == SheetTypeEnum.PatientDashboard)
        {
            SheetDefs.GetFieldsAndParameters(sheetDefDashboard);
            //FieldValue corresponds to the Patient Dashboard widget SheetDef.SheetDefNum
            var firstWidgetSheetDefNum = SIn.Long(sheetDefDashboard.SheetFieldDefs.FirstOrDefault().FieldValue);
            SheetDefs.DeleteObject(sheetDefDashboard.SheetDefNum); //Delete the layout SheetDef.
            sheetDefDashboard = SheetDefs.GetFirstOrDefault(x => x.SheetDefNum == firstWidgetSheetDefNum);
            var userOdPref = userOdPrefDashboard.Clone();
            userOdPrefDashboard.Fkey = firstWidgetSheetDefNum; //May not exist.  Will get cleaned up later.
            if (UserOdPrefs.Update(userOdPrefDashboard, userOdPref))
            {
                //Only need to signal cache refresh on change.
                DataValid.SetInvalid(InvalidType.UserOdPrefs);
            }
        }

        return sheetDefDashboard;
    }

    public static bool IsDashboardVisible => !_formOpenDentalSingleton.splitContainer.Panel2Collapsed && _formOpenDentalSingleton.userControlDashboard.IsInitialized;

    private void menuItemEServices_Click(object sender, EventArgs e)
    {
        using var formEServicesSetup = new FormEServicesSetup();

        formEServicesSetup.ShowDialog();
    }

    private void ShowEServicesSetup()
    {
        if (_toolBarButtonText != null)
        {
            _toolBarButtonText.Enabled = Programs.IsEnabled(ProgramName.CallFire) || SmsPhones.IsIntegratedTextingEnabled();
        }
    }

    private void _menuItemERouting_Click(object sender, EventArgs e)
    {
        if (!ClinicPrefs.IsOdTouchAllowed(Clinics.ClinicNum))
        {
            var site = "https://www.opendental.com/site/odtouch.html";
            try
            {
                Process.Start(site);
            }
            catch
            {
                ODMessageBox.Show("Could not find " + site + "\r\nPlease set up a default web browser.");
            }

            return;
        }

        using var formERoutings = new FormERoutings();
        formERoutings.ShowDialog();
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

            case FormType.FormEServicesWebSchedRecall:
                using (var formEServicesWebSchedRecall = new FormEServicesWebSchedRecall())
                {
                    formEServicesWebSchedRecall.ShowDialog();
                }

                ShowEServicesSetup();
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

            case FormType.FormEServicesSignupPortal:
                using (var formEServicesSignup = new FormEServicesSignup())
                {
                    formEServicesSignup.ShowDialog();
                }

                ShowEServicesSetup();
                break;

            case FormType.FormEServicesWebSchedNewPat:
                using (var formEServicesWebSchedPat = new FormEServicesWebSchedPat(true))
                {
                    formEServicesWebSchedPat.ShowDialog();
                }

                ShowEServicesSetup();
                break;
            case FormType.FormEServicesEConnector:
                using (var formEServicesEConnector = new FormEServicesEConnector())
                {
                    formEServicesEConnector.ShowDialog();
                }

                ShowEServicesSetup();
                break;
            case FormType.FormApptEdit:
                var appointment = Appointments.GetOneApt(alertItem.FKey);
                var patient = Patients.GetPat(appointment.PatNum);
                GlobalFormOpenDental.PatientSelected(patient, false);
                var formApptEdit = new FormApptEdit(appointment.AptNum); //Dispose below due to local variable.
                formApptEdit.ShowDialog();
                formApptEdit.Dispose();
                break;
            case FormType.FormWebSchedAppts:
                var formWebSchedAppts = new FormWebSchedAppts(alertItem.Type == AlertType.WebSchedNewPatApptCreated,
                    alertItem.Type == AlertType.WebSchedRecallApptCreated, alertItem.Type == AlertType.WebSchedASAPApptCreated, alertItem.Type == AlertType.WebSchedExistingPatApptCreated);
                formWebSchedAppts.Show();
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

            case FormType.FormWebForms:
                if (!Security.IsAuthorized(EnumPermType.WebFormAccess))
                {
                    break;
                }

                using (var formWebForms = new FormWebForms())
                {
                    formWebForms.FormClosed += AlertFormClosingHelper;
                    formWebForms.ShowDialog();
                }

                break;

            case FormType.FormModuleSetup:
                LaunchPrerencesWithMenuItem((int) alertItem.FKey);
                break;

            case FormType.FormEServicesAutoMsging:
                using (var formEServicesAutoMsging = new FormEServicesAutoMsging())
                {
                    formEServicesAutoMsging.FormClosed += AlertFormClosingHelper;
                    formEServicesAutoMsging.ShowDialog();
                }

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

    private void AlertFormClosingHelper(object sender, FormClosedEventArgs e)
    {
        DataValid.SetInvalid(InvalidType.AlertItems);
    }

    private void AlertReadsHelper(List<long> listAlertItemNums)
    {
        listAlertItemNums.RemoveAll(x => _alertItemReads.Exists(y => y.AlertItemNum == x));
        for (var i = 0; i < listAlertItemNums.Count; i++)
        {
            AlertReads.Insert(new AlertRead(listAlertItemNums[i], Security.CurUser.UserNum));
        }
    }

    private void menuItemRemote_Click(object sender, EventArgs e)
    {
        var site = "http://www.opendental.com/contact.html";
        if (Programs.GetCur(ProgramName.BencoPracticeManagement).Enabled)
        {
            site = "https://support.benco.com/";
        }

        try
        {
            Process.Start(site);
        }
        catch (Exception)
        {
            ODMessageBox.Show("Could not find " + site + "\r\nPlease set up a default web browser.");
        }
    }

    private void menuItemHelpWindows_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("Help.chm");
        }
        catch
        {
            MsgBox.Show(this, "Could not find file.");
        }
    }

    private void menuItemHelpContents_Click(object sender, EventArgs e)
    {
        var site = "https://www.opendental.com/manual/manual.html";
        try
        {
            Process.Start(site);
        }
        catch
        {
            MsgBox.Show(this, "Could not find file.");
        }
    }

    private void menuItemHelpIndex_Click(object sender, EventArgs e)
    {
        var site = "https://www.opendental.com/site/searchsite.html";
        try
        {
            Process.Start(site);
        }
        catch
        {
            MsgBox.Show(this, "Could not find file.");
        }
    }

    private void menuItemWebinar_Click(object sender, EventArgs e)
    {
        var site = "https://opendental.com/webinars/webinars.html";
        try
        {
            Process.Start(site);
        }
        catch
        {
            MsgBox.Show(this, "Could not open page.");
        }
    }

    private void MenuItemQueryMonitor_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.QueryMonitor))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.QueryMonitor, 0, "Query Monitor opened.");

        new FormQueryMonitor().Show();
    }

    private void MenuItemSupportStatus_Click(object sender, EventArgs e)
    {
        var formSupportStatus = new FormSupportStatus();

        formSupportStatus.Show();
    }

    private void menuItemUpdate_Click(object sender, EventArgs e)
    {
        using var formUpdate = new FormUpdate();

        formUpdate.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Opened Update Window.");
    }

    private void menuItemAbout_Click(object sender, EventArgs e)
    {
        using var formAbout = new FormAbout();

        formAbout.ShowDialog();
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
                MsgBox.Show(this, "There are no users with the SecurityAdmin permission.  Call support.");
                Application.Exit();
                return;
            }

            var userNumFirstAdminNoPass = Userods.GetFirstSecurityAdminUserNumNoPasswordNoCache();
            if (userNumFirstAdminNoPass > 0)
            {
                Security.CurUser = Userods.GetUserNoCache(userNumFirstAdminNoPass);

                CheckForPasswordReset();

                SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, Lan.g(this, "User:") + " " + Security.CurUser.UserName + " " + Lan.g(this, "has logged on."));
            }
            else if (PrefC.GetBool(PrefName.DomainLoginEnabled) && !string.IsNullOrWhiteSpace(PrefC.GetString(PrefName.DomainLoginPath)))
            {
                var loginPath = PrefC.GetString(PrefName.DomainLoginPath);
                try
                {
                        
                    var directoryEntryLogin = new DirectoryEntry(loginPath);
                    var distinguishedName = directoryEntryLogin.Properties["distinguishedName"].Value.ToString();
                    var domainGuid = directoryEntryLogin.Guid.ToString();
                    var domainGuidPref = PrefC.GetString(PrefName.DomainObjectGuid);
                    if (domainGuidPref.IsNullOrEmpty())
                    {
                        //Domain login was setup before we started recording the domain's ObjectGuid. We will save it now for future use.
                        Prefs.UpdateString(PrefName.DomainObjectGuid, domainGuid);
                        domainGuidPref = domainGuid;
                    }

                    var domainUser = domainGuidPref + '\\' + Environment.UserName;

                    var directoryEntryRootDSE = new DirectoryEntry("LDAP://RootDSE");
                    var defaultNamingContext = directoryEntryRootDSE.Properties["defaultNamingContext"].Value.ToString();

                    if (!string.IsNullOrEmpty(domainUserFromCmd))
                    {
                        domainUser = domainUserFromCmd;
                    }
                    else if (!distinguishedName.ToLower().Contains(defaultNamingContext.ToLower()) || domainGuid != domainGuidPref)
                    {
                        ShowLogOn();
                        Security.IsUserLoggedIn = true;
                        return;
                    }

                    var dictDomainUserNumsAndNames = Userods.GetUsersByDomainUserNameNoCache(domainUser);
                    if (dictDomainUserNumsAndNames.Count == 0)
                    {
                        ShowLogOn();
                    }
                    else if (dictDomainUserNumsAndNames.Count > 1)
                    {
                        var inputBox = new InputBox(Lan.g(this, "Select an Open Dental user to log in with:"), dictDomainUserNumsAndNames.Select(x => x.Value).ToList());

                        inputBox.ShowDialog();

                        if (inputBox.IsDialogOK)
                        {
                            Security.CurUser = Userods.GetUserNoCache(dictDomainUserNumsAndNames.Keys.ElementAt(inputBox.SelectedIndex));

                            CheckForPasswordReset();

                            SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged on automatically via ActiveDirectory.");
                        }
                        else
                        {
                            ShowLogOn();
                        }
                    }
                    else
                    {
                        Security.CurUser = Userods.GetUserNoCache(dictDomainUserNumsAndNames.Keys.First());

                        CheckForPasswordReset();

                        SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff, 0, "User: " + Security.CurUser.UserName + " has logged on automatically via ActiveDirectory.");
                    }
                }
                catch
                {
                    ShowLogOn();

                    Security.IsUserLoggedIn = true;

                    return;
                }
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
        if (userControlTasks1.Visible)
        {
            userControlTasks1.ClearLogOff();
        }

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
        _listTasksReminders = null;
        _listTaskNumsNormal = null;
        controlAppt.RefreshReminders([]);
        RefreshTasksNotification();
        Security.IsUserLoggedIn = false;
        Text = PatientL.GetMainTitle(null, 0);
        SetTimersAndThreads(false);
        userControlDashboard.CloseDashboard(true);
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
        if (userControlTasks1.Visible)
        {
            userControlTasks1.InitializeOnStartup();
        }

        BeginOdDashboardStarterThread();
        SetTimersAndThreads(true);

        _isFormLogOnLastActive = false;

        Security.DateTimeLastActivity = DateTime.Now;
        if (moduleBar.SelectedModule == EnumModuleType.None)
        {
            MsgBox.Show(this, "You do not have permission to use any modules.");
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
            Logger.WriteLine("Shutdown signal found while logged out. Closing Open Dental.");

            ProcessKillCommand();

            return;
        }

        if (_timerSignals.Tag?.ToString() == "shutdown")
        {
            return;
        }

        _timerSignals.Enabled = false;
        _timerSignals.Tag = "shutdown";

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

        try
        {
            FormOpenDentalClosing(sender, e);
        }
        catch (Exception ex)
        {
            try
            {
                BugSubmissions.SubmitException(ex, patNumCur: PatNumCur);
            }
            catch
            {
            }
        }
    }

    private void FormOpenDentalClosing(object sender, FormClosingEventArgs e)
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
            Computers.ClearHeartBeat(ODEnvironment.MachineName);
        }
        catch
        {
            // ignored
        }

        try
        {
            var listActiveInstances = ActiveInstances.GetAllOldInstances();
            if (ActiveInstances.GetActiveInstance() != null)
            {
                listActiveInstances.Add(ActiveInstances.GetActiveInstance());
            }

            ActiveInstances.DeleteMany(listActiveInstances);
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

        var tempPath = "";
        string[] stringArrayFileNames;
        List<string> listDirectories;
        try
        {
            tempPath = PrefC.GetTempFolderPath();
            stringArrayFileNames = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories); //All files in the current directory plus all files in all subdirectories.
            listDirectories = new List<string>(Directory.GetDirectories(tempPath, "*", SearchOption.AllDirectories)); //All subdirectories.
        }
        catch
        {
            //We will only reach here if we error out of getting the temp folder path
            //If we can't get the path, then none of the stuff below matters
            return;
        }

        for (var i = 0; i < stringArrayFileNames.Length; i++)
        {
            try
            {
                //All files related to updates need to stay.  They do not contain PHI information and will not harm anything if left around.
                if (stringArrayFileNames[i].Contains("UpdateFileCopier.exe"))
                {
                    continue; //Skip any files related to updates.
                }

                //When an update is in progress, the binaries will be stored in a subfolder called UpdateFiles within the temp directory.
                if (stringArrayFileNames[i].Contains("UpdateFiles"))
                {
                    continue; //Skip any files related to updates.
                }

                //The UpdateFileCopier will create temporary backups of source and destination setup files so that it can revert if copying fails.
                if (stringArrayFileNames[i].Contains("updatefilecopier"))
                {
                    continue; //Skip any files related to updates.
                }

                File.Delete(stringArrayFileNames[i]);
            }
            catch
            {
                //Do nothing because the file could have been in use or there were not sufficient permissions.
                //This file will most likely get deleted next time a temp file is created.
            }
        }

        listDirectories.Sort();
        for (var i = listDirectories.Count - 1; i >= 0; i--)
        {
            try
            {
                if (listDirectories[i].Contains("UpdateFiles"))
                {
                    continue;
                }

                if (listDirectories[i].Contains("updatefilecopier"))
                {
                    continue;
                }

                Directory.Delete(listDirectories[i]);
            }
            catch
            {
            }
        }
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