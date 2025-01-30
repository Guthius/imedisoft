using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CodeBase;

public class Logger
{
    public const string DatetimeFormat = "MM/dd/yy HH:mm:ss:fff";

    public static void LogToPath()
    {
    }

    public static void LogAction(Action action)
    {
        action();
    }
    
    public static void LogVerbose(string text)
    {
    }

    public static void WriteLine(string text)
    {
    }

    public static void WriteError(string text)
    {
    }

    public static void WriteException(Exception e)
    {
    }

    public class LoggerEventArgs : EventArgs;

    public interface IWriteLine
    {
        void WriteLine(string data, LogLevel logLevel);
    }
}

public class LogWriter : Logger.IWriteLine
{
    public virtual void WriteLine(string data, LogLevel logLevel)
    {
    }
}

public enum LogLevel
{
    Error = 0,
    Information = 1,
    Verbose = 2
}