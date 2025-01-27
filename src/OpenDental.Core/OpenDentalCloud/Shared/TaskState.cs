﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDental.Cloud.Shared
{
    public abstract class TaskState
    {
        public bool IsDone { private set; get; }
        public bool HasExceptions { get; set; }
        public Exception Error { private set; get; }

        /// <summary>
        /// This is a quick way to determine if there were complications with deleting the file.  
        /// The Error variable can provide more info.
        /// </summary>
        public bool HasFailed => IsDone && Error != null;

        ///<summary>This property allows the user with this TaskState to cancel an async task if they so choose.  
        ///This is usually wired up to a Cancel button in a progress form.</summary>
        public bool DoCancel { get; set; }

        protected readonly object Lock = new();

        protected abstract Task PerformIO();

        /// <summary>
        /// Runs PerformIO logic behind a synchronous or asynchronous task.
        /// See implemented class for PerformIO logic.
        /// </summary>
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
}