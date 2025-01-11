using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CodeBase;
using JetBrains.Annotations;

namespace DataConnectionBase;

public class QueryMonitor
{
    public static QueryMonitor Instance { get; } = new();
    public bool IsMonitoring { get; set; }
    public bool HasStackTrace { get; set; }
    
    public void RunMonitoredQuery([InstantHandle] Action action, DbCommand dbCommand, bool hasStackTrace = false)
    {
        if (!IsMonitoring)
        {
            action();

            return;
        }

        DbQueryObj query = null;
        Stopwatch s = null;
        try
        {
            query = new DbQueryObj(dbCommand.CommandText, dbCommand.Connection.DataSource)
            {
                IsPayload = false
            };

            dbCommand.Parameters
                .OfType<DbParameter>()
                .OrderByDescending(x => x.ParameterName.Length)
                .ForEach(x => query.CommandText = query.CommandText.Replace(x.ParameterName, "'" + SOut.String(x.Value.ToString()) + "'"));

            var stackTrace = new StackTrace();

            query.MethodName = stackTrace.GetFrame(3).GetMethod().Name;

            if (HasStackTrace || hasStackTrace)
            {
                query.StackTrace = Environment.StackTrace;
            }

            QueryMonitorEvent.Fire(ODEventType.QueryMonitor, query);

            query.DateTimeStart = DateTime.Now;

            s = Stopwatch.StartNew();

            action();
        }
        finally
        {
            if (s != null)
            {
                s.Stop();

                query.DateTimeStop = query.DateTimeStart.Add(s.Elapsed);
                QueryMonitorEvent.Fire(ODEventType.QueryMonitor, query);
            }
        }
    }
}

public class QueryMonitorEvent
{
    public static event ODEventHandler Fired;

    public static void Fire(ODEventType odEventType, object tag)
    {
        Fired?.Invoke(new ODEventArgs(odEventType, tag));
    }
}