namespace CodeBase;

public class ODEvent
{
    public static event ODEventHandler Fired;

    public static void Fire(ODEventType odEventType, object tag = null)
    {
        Fired?.Invoke(new ODEventArgs(odEventType, tag));
    }
}

public class ODEventArgs(ODEventType eventType, object tag)
{
    public readonly object Tag = tag;
    public readonly ODEventType EventType = eventType;
}

public delegate void ODEventHandler(ODEventArgs e);

public enum ODEventType
{
    AppointmentEdited,
    Billing,
    Cache,
    CommItemSave,
    EmailSave,
    FeeSched,
    FormClaimSend_GoTo,
    FormProcNotBilled_GoTo,
    HideUnusedFeeSchedules,
    ModuleSelected,
    Patient,
    ProgressBar,
    QueryMonitor,
    RecallSync,
    ReportComplex,
    SendToPinboard,
    Shutdown,
    Userod,
    WikiSave,
}