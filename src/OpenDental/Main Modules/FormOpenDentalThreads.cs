using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using MySqlConnector;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormOpenDental
{
    private readonly List<ODThread> _runOnce = [];
    private readonly List<AutoResetEvent> _exitWaitHandles = [];

    private void SetTimersAndThreads(bool doStart)
    {
        SetTimers(doStart);
        SetThreads(doStart);
    }

    private void SetTimers(bool doStart)
    {
        if (doStart)
        {
            timerTimeIndic.Start();
        }
        else
        {
            timerTimeIndic.Stop();
        }
    }

    private void SetThreads(bool doStart)
    {
        lock (_exitWaitHandles)
        {
            if (!_exitWaitHandles.IsNullOrEmpty())
            {
                var startTime = DateTime.Now;
                var waitTime = TimeSpan.FromSeconds(30);

                foreach (var resetEvent in _exitWaitHandles)
                {
                    var timeLeft = (int) waitTime.Subtract(DateTime.Now.Subtract(startTime)).TotalMilliseconds;

                    resetEvent.WaitOne(Math.Max(timeLeft, 1));
                }

                _exitWaitHandles.Clear();
            }

            if (doStart)
            {
                BeginClaimReportThread();
                BeginCanadianItransCarrierThread();
                BeginLogOffThread();
                BeginUpdateFormTextThread();
                BeginComputerHeartbeatThread();
                CheckAlerts(runOnThread: true);
                return;
            }

            Enum.GetValues(typeof(FormODThreadNames)).Cast<FormODThreadNames>().ForEach(threadName =>
            {
                switch (threadName)
                {
                    case FormODThreadNames.EhrCodeList:
                    case FormODThreadNames.ODServiceStarter:
                    case FormODThreadNames.ComputerHeartbeat:
                        break;

                    case FormODThreadNames.CanadianItransCarrier:
                    case FormODThreadNames.ClaimReport:
                    case FormODThreadNames.EServiceMonitoring:
                    case FormODThreadNames.LogOff:
                    case FormODThreadNames.ODServiceMonitor:
                    case FormODThreadNames.Podium:
                    case FormODThreadNames.UpdateFormText:
                    case FormODThreadNames.WebSync:
                    case FormODThreadNames.TimeSync:
                    case FormODThreadNames.CheckAlerts:
                    case FormODThreadNames.ApiEvents:
                    default:
                        _exitWaitHandles.AddRange(ODThread.QuitAsyncThreadsByGroupName(threadName.GetDescription()));
                        break;
                }
            });
        }
    }

    private bool IsThreadAlreadyRunning(FormODThreadNames threadName)
    {
        if (_runOnce.Any(x => x.GroupName == threadName.GetDescription()))
        {
            return true;
        }

        var threads = ODThread.GetThreadsByGroupName(threadName.GetDescription());
        
        return !threads.IsNullOrEmpty();
    }

    private void BeginCanadianItransCarrierThread()
    {
        if (IsThreadAlreadyRunning(FormODThreadNames.CanadianItransCarrier))
        {
            return;
        }

        if (!CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            return;
        }

        var thread = new ODThread((int) TimeSpan.FromHours(1).TotalMilliseconds, _ => { ItransNCpl.TryCarrierUpdate(); });

        thread.AddExceptionHandler(_ => { });
        thread.GroupName = FormODThreadNames.CanadianItransCarrier.GetDescription();
        thread.Name = FormODThreadNames.CanadianItransCarrier.GetDescription();
        thread.Start();
    }

    private void CheckAlerts(bool runOnThread = false)
    {
        if (runOnThread && IsThreadAlreadyRunning(FormODThreadNames.CheckAlerts))
        {
            return;
        }

        var getAlerts = new ODThread.WorkerDelegate(_ =>
        {
            var expiresAt = Security.DateTimeLastActivity.AddMinutes(PrefC.GetInt(PrefName.AlertInactiveMinutes));
            if (PrefC.GetInt(PrefName.AlertInactiveMinutes) != 0 && DateTime.Now > expiresAt)
            {
                return;
            }

            var clinicNum = Clinics.ClinicNum;
            var userNum = Security.CurUser.UserNum;

            var alertsItemsForUser = AlertItems.GetAlertsItemsForUser(userNum, clinicNum);
            var alertItemsUnique = new List<AlertItem>();

            foreach (var alertItemForUser in alertsItemsForUser)
            {
                var addItem = true;

                foreach (var alertItem in alertItemsUnique)
                {
                    if (!AlertItems.AreDuplicates(alertItemForUser, alertItem))
                    {
                        continue;
                    }

                    addItem = false;
                    break;
                }

                if (addItem)
                {
                    alertItemsUnique.Add(alertItemForUser);
                }
            }

            foreach (var alertItem in alertItemsUnique)
            {
                alertItem.TagOD = alertsItemsForUser
                    .Where(x => AlertItems.AreDuplicates(alertItem, x))
                    .Select(x => x.AlertItemNum)
                    .ToList();
            }

            var alertItems = alertItemsUnique.FindAll(x => x.Type != AlertType.ClinicsChangedInternal);
            var alertItemReads = AlertReads.RefreshForAlertNums(userNum, alertItems.Select(x => x.AlertItemNum).ToList());

            this.InvokeIfRequired(() =>
            {
                _alertItems = alertItems;
                _alertItemReads = alertItemReads;

                AddAlertsToMenu();
            });
        });

        if (!runOnThread)
        {
            getAlerts(null);
            return;
        }

        var checkAlertsIntervalMs = (int) TimeSpan.FromSeconds(PrefC.GetInt(PrefName.AlertCheckFrequencySeconds)).TotalMilliseconds;
        if (checkAlertsIntervalMs == 0)
        {
            return;
        }

        var thread = new ODThread(checkAlertsIntervalMs, getAlerts);

        thread.AddExceptionHandler(_ => { });
        thread.GroupName = FormODThreadNames.CheckAlerts.GetDescription();
        thread.Name = FormODThreadNames.CheckAlerts.GetDescription();
        thread.Start();
    }

    private void BeginClaimReportThread()
    {
        if (IsThreadAlreadyRunning(FormODThreadNames.ClaimReport))
        {
            return;
        }

        if (PrefC.GetBool(PrefName.ClaimReportReceivedByService))
        {
            return;
        }

        var claimReportRetrieveIntervalMs = (int) TimeSpan.FromMinutes(PrefC.GetInt(PrefName.ClaimReportReceiveInterval)).TotalMilliseconds;
        var thread = new ODThread(claimReportRetrieveIntervalMs, _ =>
        {
            var claimReportComputer = PrefC.GetString(PrefName.ClaimReportComputerName);
            if (claimReportComputer == "" || claimReportComputer != Dns.GetHostName())
            {
                return;
            }

            Clearinghouses.RetrieveReportsAutomatic(false);
        });

        thread.AddExceptionHandler(_ => { });
        thread.GroupName = FormODThreadNames.ClaimReport.GetDescription();
        thread.Name = FormODThreadNames.ClaimReport.GetDescription();
        thread.Start();
    }

    private void BeginComputerHeartbeatThread()
    {
        if (IsThreadAlreadyRunning(FormODThreadNames.ComputerHeartbeat))
        {
            return;
        }

        var thread = new ODThread(180000, _ =>
        {
            ODException.SwallowAnyException(() =>
            {
                Computers.UpdateHeartBeat(Environment.MachineName, false);

                if (Security.CurUser != null)
                {
                    ActiveInstances.Upsert(Security.CurUser.UserNum, Computers.GetCur().ComputerNum, Process.GetCurrentProcess().Id);
                }
            });
        });

        thread.AddExceptionHandler(_ => { });
        thread.GroupName = FormODThreadNames.ComputerHeartbeat.GetDescription();
        thread.Name = FormODThreadNames.ComputerHeartbeat.GetDescription();
        thread.Start();
    }
    
    private void BeginLogOffThread()
    {
        if (IsThreadAlreadyRunning(FormODThreadNames.LogOff))
        {
            return;
        }

        var thread = new ODThread((int) TimeSpan.FromSeconds(15).TotalMilliseconds, _ => LogOffWorker())
        {
            GroupName = FormODThreadNames.LogOff.GetDescription(),
            Name = FormODThreadNames.LogOff.GetDescription()
        };
        
        thread.Start();
    }

    private void LogOffWorker()
    {
        var logOffTimerMins = 0;
        try
        {
            logOffTimerMins = PrefC.LogOffTimer;
        }
        catch (MySqlException)
        {
        }

        if (logOffTimerMins == 0)
        {
            return;
        }

        if (InvokeRequired)
        {
            Invoke(LogOffWorker);
            return;
        }

        for (var f = Application.OpenForms.Count - 1; f >= 0; f--)
        {
            Form openForm;
            try
            {
                openForm = Application.OpenForms[f];
            }
            catch
            {
                continue;
            }

            if (new List<string> {"FormTerminal", "FormProgress", "ProgressOD", "FormProgressAuto"}.Contains(openForm.Name))
            {
                return;
            }
        }

        var formActive = ActiveForm;

        if (formActive == null)
        {
            _formRecentlyOpenForLogoff = null;
        }
        else if (formActive == this)
        {
            _formRecentlyOpenForLogoff = null;
            _isFormLogOnLastActive = false;
        }
        else
        {
            if (formActive == _formRecentlyOpenForLogoff)
            {
            }
            else
            {
                _formRecentlyOpenForLogoff = formActive;

                Security.DateTimeLastActivity = DateTime.Now;

                _isFormLogOnLastActive = formActive.GetType() == typeof(FormLogOn);

                return;
            }
        }

        var dtDeadline = Security.DateTimeLastActivity + TimeSpan.FromMinutes(logOffTimerMins);

        if (DateTime.Now < dtDeadline)
        {
            return;
        }

        if (Security.CurUser == null)
        {
            return;
        }

        if (_isFormLogOnLastActive)
        {
            return;
        }

        var frmLogOffWarning = new FrmLogoffWarning();

        frmLogOffWarning.ShowDialog();

        if (!frmLogOffWarning.IsDialogOK)
        {
            Security.DateTimeLastActivity = DateTime.Now;

            return;
        }

        _isFormLogOnLastActive = true;

        var thread = new ODThread(_ => { ODException.SwallowAnyException(() => { LogOffNow(true); }); });

        thread.Start();
    }
    
    private void BeginShutdownThread()
    {
        if (IsThreadAlreadyRunning(FormODThreadNames.Shutdown))
        {
            return;
        }

        var thread = new ODThread(_ =>
        {
            Thread.Sleep(15000);

            CloseOpenForms(true);

            Invoke(() => Application.Exit());
        })
        {
            GroupName = FormODThreadNames.Shutdown.GetDescription(),
            Name = FormODThreadNames.Shutdown.GetDescription()
        };

        thread.Start();
    }

    private void BeginTasksThread(List<Signalod> signals, List<long> editedTaskNums)
    {
        var thread = new ODThread(_ =>
        {
            List<TaskNote> taskNotes = null;
            List<UserOdPref> blockedTaskLists = null;
                
            var userNum = Security.CurUser?.UserNum ?? 0;
                
            var tasks = Tasks.GetNewTasksThisUser(userNum, Clinics.ClinicNum, editedTaskNums);
            if (tasks.Count > 0)
            {
                taskNotes = TaskNotes.GetForTasks(tasks.Select(x => x.TaskNum).ToList());
                blockedTaskLists = UserOdPrefs.GetByUserAndFkeyType(userNum, UserOdFkeyType.TaskListBlock);
            }

            Invoke(() => HandleRefreshedTasks(signals, editedTaskNums, tasks, taskNotes, blockedTaskLists));
        });

        thread.AddExceptionHandler(_ => { });
        thread.GroupName = FormODThreadNames.Tasks.GetDescription();
        thread.Name = FormODThreadNames.Tasks.GetDescription();
        thread.Start();
    }

    private void BeginUpdateFormTextThread()
    {
        if (IsThreadAlreadyRunning(FormODThreadNames.UpdateFormText))
        {
            return;
        }

        var thread = new ODThread((int) TimeSpan.FromSeconds(1).TotalMilliseconds, _ => { Invoke(() => { Text = PatientL.GetMainTitleSamePat(); }); });

        thread.AddExceptionHandler(_ => { });
        thread.GroupName = FormODThreadNames.UpdateFormText.GetDescription();
        thread.Name = FormODThreadNames.UpdateFormText.GetDescription();
        thread.Start();
    }
    
    public enum FormODThreadNames
    {
        CanadianItransCarrier = 1,
        CheckAlerts = 2,
        ClaimReport = 3,
        ComputerHeartbeat = 4,
        EhrCodeList = 7,
        EServiceMonitoring = 9,
        LogOff = 12,
        ODServiceMonitor = 15,
        ODServiceStarter = 16,
        PlaySounds = 18,
        Podium = 19,
        Shutdown = 22,
        Tasks = 23,
        TimeSync = 24,
        UpdateFormText = 25,
        WebSync = 27,
        Dashboard = 28,
        ApiEvents = 31
    }
}