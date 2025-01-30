using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using CodeBase;
using OpenDentBusiness;

namespace OpenDental;

internal static class ProgramEntry
{
    [STAThread]
    private static void Main(string[] args)
    {
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        try
        {
            ODInitialize.Initialize();
            Security.CurComputerName = ODEnvironment.MachineName;
        }
        catch (Exception e)
        {
            FriendlyException.Show("Critical Error: " + e.Message, e, isUnhandledException: true);
            return;
        }

        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            if (process.Id == Process.GetCurrentProcess().Id || !process.ProcessName.StartsWith("OpenDental"))
            {
                continue;
            }

            break;
        }

        var commandLineArgs = new string[args.Length];

        args.CopyTo(commandLineArgs, 0);

        var formOpenDental = new FormOpenDental(commandLineArgs);

        Exception submittedException = null;

        var actionUnhandled = new Action<Exception, string>((e, threadName) =>
        {
            var displayMsg = "";
            try
            {
                if (submittedException == null)
                {
                    submittedException = e;
                    BugSubmissions.SubmitException(e, out displayMsg, threadName, FormOpenDental.PatNumCur, formOpenDental.GetSelectedModuleName());
                }
            }
            catch
            {
                // ignored
            }

            FriendlyException.Show(displayMsg.IsNullOrEmpty() ? "Critical Error: " + e.Message : displayMsg, e, isUnhandledException: true);
            
            formOpenDental.ProcessKillCommand();
        });

        ODThread.RegisterForUnhandledExceptions(formOpenDental, actionUnhandled);

        Application.AddMessageFilter(new ODGlobalUserActiveHandler());
        Application.ThreadException += (_, e) =>
        {
            actionUnhandled(e.Exception, "ProgramEntry"); 
        };

        Application.Run(formOpenDental);
    }
}