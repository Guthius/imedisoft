namespace CodeBase;

public interface IODProgress
{
    string LanThis { get; }

    void UpdateProgress(string message);

    void UpdateProgressDetailed(string labelValue, string percentVal = "", string tagString = "", int barVal = 0, int barMax = 100, int marqSpeed = 0, string labelTop = "", bool isLeftHidden = false, bool isTopHidden = false, bool isPercentHidden = false, ProgBarStyle progStyle = ProgBarStyle.Blocks, ProgBarEventType progEvent = ProgBarEventType.ProgressBar);
}

public class ODProgressDoNothing : IODProgress
{
    public string LanThis
    {
        get => "ODProgressDoNothing";
        set { }
    }

    public void UpdateProgress(string message)
    {
    }

    public void UpdateProgressDetailed(string labelValue, string percentVal = "", string tagString = "", int barVal = 0, int barMax = 100, int marqSpeed = 0, string labelTop = "", bool isLeftHidden = false, bool isTopHidden = false, bool isPercentHidden = false, ProgBarStyle progStyle = ProgBarStyle.Blocks, ProgBarEventType progEvent = ProgBarEventType.ProgressBar)
    {
    }
}

public enum ProgBarStyle
{
    NoneSpecified,
    Blocks,
    Marquee,
    Continuous
}

public enum ProgBarEventType
{
    ProgressBar,
    BringToFront,
    Header,
    ProgressLog,
    TextMsg,
    WarningOff,
    AllowResume,
    Done,
    HideButtons
}