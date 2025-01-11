using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDental.Cloud.Shared;

public abstract class TaskState
{
    public bool IsDone { private set; get; }
    public bool HasExceptions { get; set; }
    public Exception Error { private set; get; }
    public bool HasFailed => IsDone && Error != null;
    public bool DoCancel { get; set; }

    protected readonly object Lock = new();

    protected abstract Task PerformIO();
    
    public void Execute(bool isAsync = false)
    {
        if (isAsync)
        {
            new Task(async () =>
            {
                try
                {
                    //Effectively makes this a blocking call within the context of this anonymous task.
                    await PerformIO();
                }
                catch (Exception e)
                {
                    Error = e;
                }
                finally
                {
                    IsDone = true;
                    if (HasExceptions && HasFailed)
                    {
                        throw Error;
                    }
                }
            }).Start();
        }
        else
        {
            var wait = new ManualResetEvent(false);
                
            new Task(async () =>
            {
                try
                {
                    await PerformIO();
                }
                catch (Exception e)
                {
                    Error = e;
                }
                finally
                {
                    IsDone = true;
                    wait.Set();
                }
            }).Start();
                
            if (!wait.WaitOne(-1))
            {
                // Wait infinitely. This makes it synchronous.
                throw new Exception("Action timed out.");
            }

            if (HasExceptions && HasFailed)
            {
                throw Error;
            }
        }
    }
}