using CodeBase;

namespace DataConnectionBase;

public class QueryMonitorEvent
{
    public static event ODEventHandler Fired;

    public static void Fire(ODEventType odEventType, object tag)
    {
        Fired?.Invoke(new ODEventArgs(odEventType, tag));
    }
}