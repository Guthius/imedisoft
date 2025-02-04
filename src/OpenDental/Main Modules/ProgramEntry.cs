using System;
using System.Diagnostics;
using System.Net;
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
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            
            Security.CurComputerName = Environment.MachineName;
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

        var formOpenDental = new FormOpenDental();

        var actionUnhandled = new Action<Exception, string>((e, _) =>
        {
            FriendlyException.Show("Critical Error: " + e.Message, e, isUnhandledException: true);
            
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