using System;

namespace CodeBase;

public class Logger
{
    public const string DatetimeFormat = "MM/dd/yy HH:mm:ss:fff";

    public class LoggerEventArgs : EventArgs;
}

public enum LogLevel
{
    Error = 0,
    Information = 1,
    Verbose = 2
}