using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace CodeBase;

public class ODThread
{
    private readonly Thread _thread;
    private readonly AutoResetEvent _waitEvent = new(false);
    private AutoResetEvent _waitEventAsyncQuitComplete;
    private readonly WorkerDelegate _worker;
    private ExceptionDelegate _exceptionHandler;
    private WorkerDelegate _exitHandler;
    private readonly WorkerDelegate _setupHandler = null;
    private static readonly WorkerDelegate OnInitialize = null;
    private static readonly List<ODThread> ListOdThreads = [];
    private static readonly object LockObj = new();
    private bool _isAutoCleanup;
    private static Action<Exception, Thread> _actionUnhandledException;

    public int TimeIntervalMs;
    private bool _wasAbortAttempted;
    public object Tag;
    public readonly object[] Parameters;
    public string GroupName = "default";

    public bool HasQuit { get; private set; }

    public string Name
    {
        get => _thread.Name;
        set => _thread.Name = value;
    }

    public ODThread(WorkerDelegate worker) : this(worker, null)
    {
    }

    public ODThread(WorkerDelegate worker, params object[] parameters) : this(0, worker, parameters)
    {
    }

    public ODThread(int timeIntervalMs, WorkerDelegate worker, params object[] parameters)
    {
        lock (LockObj)
        {
            ListOdThreads.Add(this);
        }

        _thread = new Thread(Run);
        TimeIntervalMs = timeIntervalMs;
        _worker += worker;
        Parameters = parameters;
        OnInitialize?.Invoke(this);
    }

    public override string ToString()
    {
        return Name;
    }

    public void SetApartmentState(ApartmentState aptState)
    {
        _thread.SetApartmentState(aptState);
    }

    public void Start(bool isAutoCleanup = true)
    {
        _isAutoCleanup = isAutoCleanup;
        if (_thread.IsAlive)
        {
            return; //The thread is already running.
        }

        if (HasQuit)
        {
            return; //The thread has finished.
        }

        _thread.Start();
    }

    public void Wakeup()
    {
        _waitEvent.Set();
    }

    public void Wait(int waitTimeMs)
    {
        _waitEvent.WaitOne(waitTimeMs);
    }

    private void Run()
    {
        try
        {
            SetupRunTeardown();
        }
        finally
        {
            if (_isAutoCleanup)
            {
                lock (LockObj)
                {
                    ListOdThreads.Remove(this);
                }
            }
        }
    }

    private void SetupRunTeardown()
    {
        try
        {
            _setupHandler?.Invoke(this);
        }
        catch (Exception e)
        {
            if (!WorkerExceptionHandler(e))
            {
                return;
            }
        }

        while (!HasQuit)
        {
            try
            {
                _worker(this);
            }
            catch (Exception e)
            {
                if (!WorkerExceptionHandler(e))
                {
                    return;
                }
            }

            switch (TimeIntervalMs)
            {
                case > 0:
                {
                    if (!HasQuit)
                    {
                        Wait(TimeIntervalMs);
                    }

                    break;
                }
                
                case <= 0:
                    HasQuit = true;
                    break;
            }
        }

        try
        {
            _exitHandler?.Invoke(this);
        }
        catch (Exception e)
        {
            WorkerExceptionHandler(e);
        }
        finally
        {
            ODException.SwallowAnyException(() => { _waitEventAsyncQuitComplete?.Set(); });
        }
    }

    private bool WorkerExceptionHandler(Exception e)
    {
        if (_wasAbortAttempted || e is ThreadAbortException)
        {
            //We know that a join failed by exceeding the allotted timeout.
            HasQuit = true;
            return false;
        }

        if (_exceptionHandler != null)
        {
            try
            {
                _exceptionHandler(e);
            }
            catch (Exception ex)
            {
                HandleUnhandledExceptionOrThrow(ex);

                return false;
            }
        }
        else
        {
            HandleUnhandledExceptionOrThrow(e);

            return false;
        }

        return true;
    }

    private void HandleUnhandledExceptionOrThrow(Exception e)
    {
        HasQuit = true;
        if (_actionUnhandledException != null)
        {
            _actionUnhandledException(e, _thread);
            return;
        }

        MiscUtils.PreserveExceptionInfoAndThrow(e);
    }

    public void Join(int timeoutMs)
    {
        if (_thread.ThreadState == ThreadState.Unstarted)
        {
            return;
        }

        var hasJoined = _thread.Join(timeoutMs);
        if (hasJoined)
        {
            return;
        }

        _wasAbortAttempted = true;
        _thread.Abort();
    }

    public static void AddGroupNameExitHandler(string groupName, EventHandler onExit)
    {
        new ODThread(_ =>
        {
            JoinThreadsByGroupName(Timeout.Infinite, groupName);
            onExit(groupName, EventArgs.Empty);
        }).Start();
    }

    public static void JoinThreadsByGroupName(int timeoutMs, string groupName, bool doRemoveThreads = false)
    {
        var listOdThreadsForGroup = GetThreadsByGroupName(groupName);
        foreach (var t in listOdThreadsForGroup)
        {
            t.Join(timeoutMs);
        }

        if (!doRemoveThreads)
        {
            return;
        }

        foreach (var thread in listOdThreadsForGroup)
        {
            thread.QuitAsync();
        }
    }

    public AutoResetEvent QuitAsync(bool removeThread = true)
    {
        HasQuit = true;

        _waitEventAsyncQuitComplete = new AutoResetEvent(false);

        Wakeup();

        if (!removeThread)
        {
            return _waitEventAsyncQuitComplete;
        }

        lock (LockObj)
        {
            ListOdThreads.Remove(this);
        }

        return _waitEventAsyncQuitComplete;
    }

    public void QuitSync(int timeoutMs)
    {
        HasQuit = true;

        Wakeup();
        try
        {
            Join(timeoutMs);
        }
        catch
        {
            // ignored
        }

        finally
        {
            lock (LockObj)
            {
                ListOdThreads.Remove(this);
            }
        }
    }

    public static List<AutoResetEvent> QuitAsyncThreadsByGroupName(string groupName, bool doRemoveThreads = false)
    {
        var listWaitHandles = new List<AutoResetEvent>();
        var listThreadsForGroup = GetThreadsByGroupName(groupName);
        foreach (var t in listThreadsForGroup)
        {
            listWaitHandles.Add(t.QuitAsync(doRemoveThreads));
        }

        return listWaitHandles;
    }

    public static void QuitSyncThreadsByGroupName(int timeoutMs, string groupName)
    {
        QuitAsyncThreadsByGroupName(groupName);

        JoinThreadsByGroupName(timeoutMs, groupName, true);
    }

    public static void QuitSyncAllOdThreads(int timeoutMs = 0)
    {
        QuitSyncThreadsByGroupName(timeoutMs, "");
    }

    public static List<ODThread> GetThreadsByGroupName(string groupName)
    {
        var listOdThreadsForGroup = new List<ODThread>();
        lock (LockObj)
        {
            foreach (var t in ListOdThreads)
            {
                if (groupName == "" || t.GroupName == groupName)
                {
                    listOdThreadsForGroup.Add(t);
                }
            }
        }

        return listOdThreadsForGroup;
    }

    public void AddExceptionHandler(ExceptionDelegate exceptionHandler)
    {
        _exceptionHandler += exceptionHandler;
    }

    public void AddExitHandler(WorkerDelegate exitHandler)
    {
        _exitHandler += exitHandler;
    }

    public static void RunParallel(List<Action> listActions, TimeSpan timeout, int numThreads = 0, ExceptionDelegate onException = null, bool isLegacy = false)
    {
        RunParallel(listActions, (int) timeout.TotalMilliseconds, numThreads, onException, isLegacy: isLegacy);
    }

    public static void RunParallel(List<Action> listActions, int timeoutMs = Timeout.Infinite, int numThreads = 0, ExceptionDelegate onException = null, bool doRunOnCurrentThreadIf1Processor = false, bool isLegacy = false)
    {
        var threadCount = numThreads;
        if (threadCount <= 0)
        {
            threadCount = Environment.ProcessorCount;

            if (threadCount < 8)
            {
                threadCount = 8;
            }
        }

        Exception exceptionFirst = null;

        if (threadCount == 1 && doRunOnCurrentThreadIf1Processor)
        {
            foreach (var action in listActions)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exceptionFirst ??= ex;
                }
            }
        }
        else
        {
            exceptionFirst = RunParallelImproved(listActions, timeoutMs, threadCount);
        }

        HandleException(exceptionFirst);

        return;

        void HandleException(Exception ex)
        {
            if (ex is null)
            {
                return;
            }

            if (onException is null)
            {
                MiscUtils.PreserveExceptionInfoAndThrow(ex);
            }

            onException?.Invoke(ex);
        }
    }

    private static Exception RunParallelImproved(List<Action> listActions, int timeoutMs, int threadCount)
    {
        Exception exceptionFirst = null;
        threadCount = Math.Min(threadCount, listActions.Count);
        var queueActions = new ConcurrentQueue<Action>(listActions);

        //Make a group of threads to spread out the workload.
        var locker = new object();

        //No one outside of this method cares about this group name. They have no authority over this group.
        var threadId = 1;
        var threadGroupGuid = Guid.NewGuid().ToString();
        var threadGroupName = "ODThread.ThreadPool()" + threadGroupGuid;
        var listThreads = new List<ODThread>();
        for (var i = 0; i < threadCount; i++)
        {
            var odThread = new ODThread(_ =>
            {
                while (queueActions.TryDequeue(out var action))
                {
                    action();
                }
            })
            {
                Name = threadGroupName + "-" + threadId,
                GroupName = threadGroupName
            };

            odThread.AddExceptionHandler(e =>
            {
                lock (locker)
                {
                    exceptionFirst ??= e;
                }
            });
            listThreads.Add(odThread);
            threadId++;
        }

        listThreads.ForEach(x => x.Start());

        JoinThreadsByGroupName(timeoutMs, threadGroupName, true);
        return exceptionFirst;
    }

    public delegate void WorkerDelegate(ODThread odThread);

    public delegate void ExceptionDelegate(Exception e);

    public static void RegisterForUnhandledExceptions(Control controlMainThread = null, Action<Exception, string> actionException = null)
    {
        _actionUnhandledException = (exception, thread) =>
        {
            if (controlMainThread == null)
            {
                actionException?.Invoke(exception, thread.Name);
                return;
            }

            controlMainThread.BeginInvoke(() =>
            {
                thread.Join();
                if (actionException != null)
                {
                    actionException(exception, thread.Name);
                    return;
                }

                Application.Exit();
            });
        };
    }
}