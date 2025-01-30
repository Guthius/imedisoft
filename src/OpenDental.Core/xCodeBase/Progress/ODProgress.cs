using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CodeBase;

public class ODProgress
{
    private static readonly ReaderWriterLockSlim LockProgressCur = new();
    private static readonly List<FormProgressBase> ListActiveProgressForms = [];

    public static FormProgressBase FormProgressActive
    {
        get
        {
            LockProgressCur.EnterReadLock();
            try
            {
                return ListActiveProgressForms.LastOrDefault();
            }
            finally
            {
                LockProgressCur.ExitReadLock();
            }
        }
    }

    private static void AddActiveProgressWindow(FormProgressBase form)
    {
        LockProgressCur.EnterWriteLock();
        try
        {
            ListActiveProgressForms.Add(form);
        }
        finally
        {
            LockProgressCur.ExitWriteLock();
        }
    }

    private static void RemoveActiveProgressWindow(FormProgressBase formPB)
    {
        LockProgressCur.EnterWriteLock();
        try
        {
            ListActiveProgressForms.Remove(formPB);
        }
        finally
        {
            LockProgressCur.ExitWriteLock();
        }
    }

    [Obsolete("Deprecated.  Use ProgressOD instead.")]
    public static Action Show(string startingMessage = "Please Wait...", bool hasHistory = false, bool hasMinimize = false, ProgressBarStyle progStyle = ProgressBarStyle.Marquee)
    {
        return ShowProgressBase(
            () => new FormProgressStatus(hasHistory, hasMinimize, startingMessage, progStyle)
            {
                TopMost = true
            }, 
            "Thread_ODProgress_Show_" + DateTime.Now.Ticks);
    }

    public static Action ShowExtended(Form currentForm, object tag = null, ProgressCanceledHandler progCanceled = null, ProgressPausedHandler progPaused = null, string cancelButtonText = null)
    {
        var actionCloseProgressWindow = ShowProgressBase(
            () =>
            {
                var form = new FormProgressExtended(cancelButtonText: cancelButtonText);
                if (progCanceled != null)
                {
                    form.ProgressCanceled += progCanceled;
                }

                if (progPaused != null)
                {
                    form.ProgressPaused += progPaused;
                }

                if (tag != null)
                {
                    form.ODEvent_Fired(new ODEventArgs(ODEventType.ProgressBar, tag));
                }

                return form;
            }, 
            "Thread_ODProgress_ShowExtended_" + DateTime.Now.Ticks);
        
        return () =>
        {
            actionCloseProgressWindow();
            
            if (currentForm is {IsDisposed: false})
            {
                currentForm.Activate();
            }
        };
    }

    [Obsolete("Deprecated.  Use ProgressOD instead.")]
    public static void ShowAction(Action actionComputation, string startingMessage = "Please Wait...", Action<Exception> actionException = null, ProgressBarStyle progStyle = ProgressBarStyle.Marquee, bool hasMinimize = true, bool hasHistory = false)
    {
        if (actionComputation == null)
        {
            return;
        }

        var actionCloseProgressWindow = Show(startingMessage, hasHistory, hasMinimize, progStyle);

        try
        {
            actionComputation();
        }
        catch (Exception e)
        {
            if (actionException == null)
            {
                throw;
            }
            else
            {
                actionException(e);
            }
        }
        finally
        {
            actionCloseProgressWindow();
        }
    }

    public static Action ShowProgressBase(Func<FormProgressBase> funcGetNewProgress, string threadName = "Thread_ODProgress_ShowProgressBase")
    {
        FormProgressBase form = null;
        
        var manualReset = new ManualResetEvent(false);
        var odThread = new ODThread(_ =>
        {
            form = funcGetNewProgress();
            
            AddActiveProgressWindow(form);
            
            form.Shown += (_, _) => manualReset.Set();
            form.FormClosed += (_, _) => RemoveActiveProgressWindow(form);
            form.ShowDialog();
        });
        
        odThread.SetApartmentState(ApartmentState.STA);
        odThread.AddExceptionHandler(_ => { });
        odThread.Name = threadName;
        odThread.Start();
        
        manualReset.WaitOne();
        return () =>
        {
            form.ForceClose = true;
            
            odThread.Join(Timeout.Infinite);
        };
    }
}