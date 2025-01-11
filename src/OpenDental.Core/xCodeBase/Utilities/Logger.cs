using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodeBase;

public partial class Logger
{
    public enum Severity
    {
        NONE = 0, //Must be first.
        DEBUG = 1,
        INFO = 2,
        WARNING = 3,
        ERROR = 4,
        FATAL_ERROR = 5,
    }

    private const int LogRollByteCount = 1048576;

    private readonly string _logFile = "";
        
#if(DEBUG)
    public Severity level = Severity.DEBUG;
#else
		public Severity level = Severity.NONE;
#endif

    public const int MaxFileSizeKb = 1000;
    private static Dictionary<string /*sub-directory, can be empty string (not null though)*/, object[] /*{StreamWriter, Create DateTime} the file currently linked to this sub-directory*/> _files = new();
    private static readonly object Lock = new();
    private static ODThread _threadLoggerCleanup;
        
    private static bool _canUseMyDocsDir;

    public const string DatetimeFormat = "MM/dd/yy HH:mm:ss:fff";

    public static readonly string LoggerDirOverride = string.Empty;

    public static void LogToPath(string log, LogPath path, LogPhase logPhase, string optionalDesc = "")
    {
        if (DoVerboseLogging == null || !DoVerboseLogging())
        {
            return;
        }

        var logWrite = GetCallingMethod() + " " + log;
        switch (logPhase)
        {
            case LogPhase.Unspecified:
                break;
            case LogPhase.Start:
                logWrite += " start";
                break;
            case LogPhase.End:
                logWrite += " end";
                break;
        }

        if (optionalDesc != "")
        {
            logWrite += " ... " + optionalDesc;
        }

        LogVerbose(logWrite, path + "\\" + Process.GetCurrentProcess().Id);
    }

    public static void LogAction(string log, LogPath logPath, Action action, string optionalDesc = "")
    {
        LogToPath(log, logPath, LogPhase.Start, optionalDesc);
        action();
        LogToPath(log, logPath, LogPhase.End);
    }

    public static void LogActionIfOverTimeLimit(string log, LogPath logPath, Action action, int milliseconds = 5000)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        if (stopwatch.Elapsed > TimeSpan.FromMilliseconds(milliseconds))
        {
            LogToPath(log, logPath, LogPhase.Unspecified, $"Took too long: {(int) stopwatch.Elapsed.TotalSeconds} seconds");
        }
    }

    private static string GetCallingMethod()
    {
        try
        {
            for (var i = 2; i < 4; i++)
            {
                //Start at stackframe(2) because 0,1, and possibly 2 are the parents of this method.		
                var frame = new StackFrame(i);
                var method = frame.GetMethod();
                if (!method.Name.ToLower().Contains("logtopath") && !method.Name.ToLower().Contains("logaction"))
                {
                    return method.ReflectedType.FullName + "." + method.Name;
                }
            }
        }
        catch (Exception e)
        {
        }

        //Return blank if we couldn't find a method name that wasn't ourself in the first 5 frames
        return "";
    }

    public delegate bool DoVerboseLoggingArgs();

    public static DoVerboseLoggingArgs DoVerboseLogging;

    public static void LogVerbose(string log, string subDirectory = "")
    {
        if (DoVerboseLogging == null || !DoVerboseLogging())
        {
            return;
        }

        WriteLine(log, subDirectory, daysOld: 30);
    }

    public static string GetDirectory(string subDirectory)
    {
        subDirectory = ScrubSubDirPath(subDirectory);
        var ret = "";
        bool canUseMyDocsDir;
        string loggerDirOverride;
        lock (Lock)
        {
            canUseMyDocsDir = _canUseMyDocsDir;
            loggerDirOverride = LoggerDirOverride;
        }

        //Could make this a ternary operator but it is incredibly long.
        if (canUseMyDocsDir)
        {
            //The logger file is sometimes blocked by Windows unless OD is ran as admin. This is a work around to avoid that file block by writing to MyDocuments.
            ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Logger");
        }
        else if (!string.IsNullOrEmpty(loggerDirOverride))
        {
            ret = loggerDirOverride;
        }
        else if (File.Exists(ODFileUtils.CombinePaths(AppDomain.CurrentDomain.BaseDirectory, "web.config")))
        {
            //Note this is case insensitive
            //We are using the existence of a web.config file to determine if this is a web application. While there is a more official way to do this,
            //that way requires System.Web.dll which is not available for .NET Standard.
            var fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            var drive = Path.GetPathRoot(fi.FullName);
            //For example, an application at "E:\patientviewer.com\SignupPortal" will have its logger folder at 
            //"E:\ProgramData\OpenDental\Logs\patientviewer.com\SignupPortal\Logger".
            ret = ODFileUtils.CombinePaths(new string[]
            {
                drive, "ProgramData", "OpenDental", "Logs",
                StringTools.SubstringAfter(AppDomain.CurrentDomain.BaseDirectory, drive), "Logger"
            });
            if (!Directory.Exists(ret))
            {
                Directory.CreateDirectory(ret);
            }
        }
        else
        {
            ret = ODFileUtils.CombinePaths(AppDomain.CurrentDomain.BaseDirectory, "Logger");
        }

        if (!string.IsNullOrEmpty(subDirectory))
        {
            ret = ODFileUtils.CombinePaths(ret, subDirectory);
        }

        return ret;
    }

    public static void WriteLine(string line, string subDirectory, int daysOld = 90)
    {
        WriteLine(line, subDirectory, false, true, daysOld);
    }

    public static void WriteLine(string line, string subDirectory, bool singleFileOnly, bool includeTimestamp, int daysOld = 90)
    {
        lock (Lock)
        {
            subDirectory = ScrubSubDirPath(subDirectory);
            var file = Open(subDirectory, singleFileOnly);
            if (file == null)
            {
                return;
            }

            var timeStamp = includeTimestamp ? DateTime.Now.ToString(DatetimeFormat) + "\t" : "";
            file.WriteLine(timeStamp + line);
        }
    }

    public static void WriteError(string line, string subDirectory)
    {
        WriteLine("failed - " + line, subDirectory, false, true);
    }

    public static void WriteException(Exception e, string subDirectory)
    {
        WriteError(MiscUtils.GetExceptionText(e), subDirectory);
    }

    public static void CloseLogger()
    {
        lock (Lock)
        {
            while (_files.Count >= 1)
            {
                IEnumerator enumerator = _files.Keys.GetEnumerator();
                if (enumerator == null || !enumerator.MoveNext())
                {
                    break;
                }

                CloseFile((string) enumerator.Current);
            }
        }
    }

    private static void CloseFile(string subDirectory)
    {
        try
        {
            lock (Lock)
            {
                StreamWriter file = null;
                var created = DateTime.Now;
                if (!TryGetFile(subDirectory, out file, out created))
                {
                    return;
                }

                file.Dispose();
                _files.Remove(subDirectory);
            }
        }
        catch
        {
        }
    }

    private static bool TryGetFile(string subDirectory, out StreamWriter file, out DateTime created)
    {
        file = null;
        created = DateTime.MinValue;
        lock (Lock)
        {
            object[] obj = null;
            if (!_files.TryGetValue(subDirectory, out obj))
            {
                return false;
            }

            file = (StreamWriter) obj[0];
            created = (DateTime) obj[1];
            return true;
        }
    }

    private static StreamWriter Open(string subDirectory, bool singleFileOnly)
    {
        try
        {
            lock (Lock)
            {
                StreamWriter file = null;
                var created = DateTime.MinValue;
                if (TryGetFile(subDirectory, out file, out created))
                {
                    //file has been created
                    if (singleFileOnly)
                    {
                        return file;
                    }

                    if (DateTime.Today == created.Date)
                    {
                        //it was created today
                        if (file.BaseStream.Length / 1024 <= MaxFileSizeKb)
                        {
                            //it is within the acceptable size limit
                            return file;
                        }
                    }

                    CloseFile(subDirectory);
                }

                file = new StreamWriter(GetFileName(subDirectory, DateTime.Today, singleFileOnly), true);
                file.AutoFlush = true;
                _files[subDirectory] = new object[] {file, DateTime.Now};
                return file;
            }
        }
        catch
        {
            return null;
        }
    }

    private static string ScrubSubDirPath(string dirIn)
    {
        //Remove any extra directory delimiters from the subDirectory.
        var dirOut = dirIn.TrimEnd('\\').TrimStart('\\');
        //This exhaustive list was found here. https://stackoverflow.com/a/33608950
        var replaceThese = Path.GetInvalidPathChars().Union(new char[] {':', '?', '/', '!', '*', '%', '.', ',',}).ToList().FindAll(x => dirOut.Contains(x));
        foreach (var c in replaceThese)
        {
            dirOut = dirOut.Replace(c, '-');
        }

        return dirOut;
    }

    private static string GetFileNameSingleFileOnly(string subDirectory)
    {
        var di = new DirectoryInfo(GetDirectory(subDirectory));
        var fi = new FileInfo(ODFileUtils.CombinePaths(di.FullName, subDirectory + ".txt"));
        if (!di.Exists)
        {
            di.Create();
        }

        return fi.FullName;
    }

    private static string GetFileName(string subDirectory, DateTime date, bool singleFileOnly)
    {
        if (singleFileOnly)
        {
            return GetFileNameSingleFileOnly(subDirectory);
        }

        var formattedDate = date.ToString("yy-MM-dd");
        var di = new DirectoryInfo(ODFileUtils.CombinePaths(GetDirectory(subDirectory), formattedDate));
        if (!di.Exists)
        {
            di.Create();
        }

        var fileNum = 1;
        do
        {
            var fi = new FileInfo(ODFileUtils.CombinePaths(di.FullName, formattedDate + " (" + fileNum.ToString("D3") + ").txt"));
            if (!fi.Exists)
            {
                //file doesn't exist yet
                return fi.FullName;
            }

            if (fi.Length / 1024 <= MaxFileSizeKb)
            {
                //file is small enough to use
                return fi.FullName;
            }

            if (++fileNum >= 1000)
            {
                //only create 1000 files max
                var fileInfos = new List<FileInfo>(di.GetFiles(formattedDate + "*"));
                fileInfos.Sort(SortFileByModifiedTimeDesc);
                fileInfos[0].Delete();
                return fileInfos[0].FullName;
            }
        } while (true);
    }

    private static int SortFileByModifiedTimeDesc(FileInfo x, FileInfo y)
    {
        return x.LastWriteTime.CompareTo(y.LastWriteTime);
    }

    public class LoggerEventArgs : EventArgs;
        
    public interface IWriteLine
    {
        LogLevel LogLevel { get; set; }
        void WriteLine(string data, LogLevel logLevel, string subDirectory = "");
    }
}

public class LogWriter : Logger.IWriteLine
{
    public virtual LogLevel LogLevel { get; set; }
    public string BaseDirectory;

    public LogWriter(LogLevel logLevel, string baseDirectory)
    {
        LogLevel = logLevel;
        BaseDirectory = baseDirectory;
    }

    public virtual void WriteLine(string data, LogLevel logLevel, string subDirectory = "")
    {
        if (logLevel > LogLevel)
        {
            return;
        }

        Logger.WriteLine(data, ODFileUtils.CombinePaths(BaseDirectory, subDirectory));
    }
}

public enum LogLevel
{
    Error = 0,
    Information = 1,
    Verbose = 2
}

public enum LogPhase
{
    Unspecified,
    Start,
    End,
}

public enum LogPath
{
    Signals,
    ChartModule,
    AccountModule,
    OrthoChart,
    Threads,
    Startup,
}