using System;

namespace DataConnectionBase;

public class DbQueryObj(string commandText, string serverName)
{
    public Guid Guid = Guid.NewGuid();
    public string CommandText = commandText;
    public DateTime DateTimeInit = DateTime.Now;
    public DateTime DateTimeStart;
    public DateTime DateTimeStop;
    public string MethodName;
    public string StackTrace;
    public readonly string ServerName = serverName;
    public bool IsPayload;
    
    public TimeSpan Elapsed
    {
        get
        {
            if (DateTimeStart == DateTime.MinValue || DateTimeStop == DateTime.MinValue)
            {
                return TimeSpan.MinValue;
            }

            return DateTimeStop - DateTimeStart;
        }
    }

    public override string ToString()
    {
        return $"# GUID: {Guid.ToString()}{(string.IsNullOrEmpty(MethodName) ? "" : $"  Method Name: {MethodName}")}\r\n" + 
               $"# DateTimeStart: {DateTimeStart:MM/dd/yyyy hh:mm:ss.fffffff tt}\r\n" + 
               $"# DateTimeStop: {(DateTimeStop == DateTime.MinValue ? "Still Running" : DateTimeStop.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt"))}\r\n" + 
               $"# Elapsed: {Elapsed:G}\r\n" + 
               $"{CommandText}";
    }
}