namespace CodeBase;

public interface IODProgressExtended : IODProgress
{
    bool IsPaused { get; }
    bool IsCanceled { get; }

    void AllowResume();

    void HideButtons();

    void UpdateProgress(string labelTop, string tagstring, string percentVal = "", int barVal = 0, int barMax = 100, bool isTopHidden = false, string labelValue = "");

    bool IsPauseOrCancel();

    void OnProgressDone();
}

public class ODProgressExtendedNull : ODProgressDoNothing, IODProgressExtended
{
    public bool IsCanceled => false;
    public bool IsPaused => false;

    public void HideButtons()
    {
    }

    public void AllowResume()
    {
    }

    public bool IsPauseOrCancel()
    {
        return false;
    }

    public void UpdateProgress(string labelTop, string tagstring, string percentVal = "", int barVal = 0, int barMax = 100, bool isTopHidden = false, string labelValue = "")
    {
    }

    public void OnProgressDone()
    {
    }
}