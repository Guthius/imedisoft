using System;
using System.Diagnostics;
using System.IO;
#if !DOT_NET_CORE && !DOT_NET_STANDARD
using System.Windows.Forms;
#endif

namespace CodeBase;

public partial class Logger
{
    public static readonly Logger Openlog = new(ODFileUtils.GetProgramDirectory() + "openlog.txt");

    public Logger(string pLogFile)
    {
        _logFile = pLogFile;
    }

    public static string SeverityToString(Severity sev)
    {
        switch (sev)
        {
            case Severity.NONE:
                return "NONE";
            case Severity.DEBUG:
                return "DEBUG";
            case Severity.INFO:
                return "INFO";
            case Severity.WARNING:
                return "WARNING";
            case Severity.ERROR:
                return "ERROR";
            case Severity.FATAL_ERROR:
                return "FATAL ERROR";
            default:
                break;
        }

        return "UNKNOWN SEVERITY";
    }

    public static void UseMyDocsDirectory()
    {
        lock (Lock)
        {
            if (_canUseMyDocsDir)
            {
                return;
            }

            CloseLogger();
            _canUseMyDocsDir = true;
        }
    }

    public void LogMB(string message, Severity severity)
    {
        Log(null, "", message, true, severity);
    }

    public void LogMB(object sender, string sendingFunctionName, string message, Severity severity)
    {
        Log(sender, sendingFunctionName, message, true, severity);
    }
        
    public void Log(object sender, string sendingFunctionName, string message, bool msgBox, Severity severity)
    {
        if (severity < level)
        {
            //Only log messages with a severity matches the current level. This will even skip message boxes.
            return;
        }

        try
        {
            if (sender != null)
            {
                if (sendingFunctionName != null && sendingFunctionName.Length > 0)
                {
                    message = sender.ToString() + "." + sendingFunctionName + ": " + message;
                }
                else
                {
                    message = sender.ToString() + ": " + message;
                }
            }
            else if (sendingFunctionName != null && sendingFunctionName.Length > 0)
            {
                message = sendingFunctionName + ": " + message;
            }

            int procId = Process.GetCurrentProcess().Id;
            message = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " " + procId.ToString().PadLeft(6, '0') + " " +
                      SeverityToString(severity) + " " + message;
#if DOT_NET_CORE || DOT_NET_STANDARD
			}
			//Allow to throw.
			finally {
			}
#else
            if (msgBox)
            {
                using MsgBoxCopyPaste mbox = new MsgBoxCopyPaste(message);
                mbox.ShowDialog();
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
#endif
        //File access is always exclusive, so if we cannot access the file, we can try again for a little while
        //and hope that the other process will release the file.
        bool tryagain = true;
        int numtries = 0;
        while (tryagain && numtries < 5)
        {
            tryagain = false;
            numtries++;
            try
            {
                if (_logFile != null)
                {
                    //Ensure that the log file always exists before trying to read it.
                    if (!File.Exists(_logFile))
                    {
                        try
                        {
                            FileStream fs = File.Create(_logFile);
                            fs.Dispose();
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        //Make the log file roll into the old log file when it reaches the roll byte size.
                        StreamReader sr = new StreamReader(_logFile);
                        if (sr != null)
                        {
                            Stream st = sr.BaseStream;
                            long fileLength = st.Length;
                            if (fileLength >= LogRollByteCount)
                            {
                                try
                                {
                                    File.Copy(_logFile, _logFile + ".old.txt");
                                }
                                catch
                                {
                                }

                                try
                                {
                                    File.Delete(_logFile);
                                }
                                catch
                                {
                                }

                                fileLength = 0;
                            }

                            st.Dispose();
                            sr.Dispose();
                            if (fileLength < 1)
                            {
                                try
                                {
                                    FileStream fs = File.Create(_logFile);
                                    fs.Dispose();
                                }
                                catch
                                {
                                }
                            }
                        }
                    }

                    //Re-open the log file 
                    StreamWriter sw = new StreamWriter(_logFile, true); //Open the file exclusively.
                    if (sw != null)
                    {
                        sw.WriteLine(message);
                        sw.Flush();
                        sw.Dispose(); //Close the file to allow exclusive access by other instances of OpenDental.
                    }
                }
            }
            catch
            {
                tryagain = true;
            }
        }
    }
}