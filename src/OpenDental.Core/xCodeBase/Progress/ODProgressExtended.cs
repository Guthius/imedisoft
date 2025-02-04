using System;
using System.Threading;
using System.Windows.Forms;

namespace CodeBase;

public class ODProgressExtended : IODProgressExtended
{
    private readonly Action _actionCloser;
    private readonly ProgBarStyle _progBarStyle;

    public bool IsPaused { get; private set; }

    public bool IsCanceled { get; private set; }

    public string LanThis { get; set; }

    public ODProgressExtended(Form currentForm, object tag = null, ProgBarStyle progBarStyle = ProgBarStyle.Blocks, string lanThis = "ProgressExtended", string cancelButtonText = null)
    {
        _actionCloser = ODProgress.ShowExtended(currentForm, tag,
            (_, _) => { IsCanceled = true; },
            (_, e) => { IsPaused = e.IsPaused; }, cancelButtonText);
        _progBarStyle = progBarStyle;
        LanThis = lanThis;
    }

    public void AllowResume()
    {
        ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper("", progressBarEventType: ProgBarEventType.AllowResume));
    }

    public void HideButtons()
    {
        ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper("", progressBarEventType: ProgBarEventType.HideButtons));
    }

    public void OnProgressDone()
    {
        ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper("", progressBarEventType: ProgBarEventType.Done));
    }

    public void Close()
    {
        _actionCloser?.Invoke();
    }

    public void Fire(ODEventType odEventType, object tag)
    {
        Fire(new ODEventArgs(odEventType, tag));
    }

    public void Fire(ODEventArgs e)
    {
        if (e.Tag is ProgressBarHelper {ProgressStyle: ProgBarStyle.NoneSpecified} progBarHelper)
        {
            progBarHelper.ProgressStyle = _progBarStyle;
        }

        ODEvent.Fire(ODEventType.ProgressBar, e.Tag);
    }

    public void UpdateProgressDetailed(string labelValue, string tagString, string percentVal = "", int barVal = 0, int barMax = 100, int marqSpeed = 0, string labelTop = "", bool isLeftHidden = false, bool isTopHidden = false, bool isPercentHidden = false, ProgBarStyle progStyle = ProgBarStyle.Blocks, ProgBarEventType progEvent = ProgBarEventType.ProgressBar)
    {
        ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper(labelValue, percentVal, barVal, barMax, progStyle, tagString, marqSpeed, labelTop, isLeftHidden, isTopHidden, isPercentHidden, progressBarEventType: progEvent));
    }

    public void UpdateProgress(string message)
    {
        ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper(message, progressBarEventType: ProgBarEventType.TextMsg));
    }

    public void UpdateProgress(string labelTop, string tagstring, string percentVal = "", int barVal = 0, int barMax = 100, bool isTopHidden = false, string labelValue = "")
    {
        ODEvent.Fire(ODEventType.ProgressBar, new ProgressBarHelper(labelValue, percentVal, barVal, barMax, tagString: tagstring, labelTop: labelTop, isTopHidden: isTopHidden));
    }

    public bool IsPauseOrCancel()
    {
        while (IsPaused)
        {
            AllowResume();

            Thread.Sleep(10);

            if (IsCanceled)
            {
                break;
            }
        }

        return IsCanceled;
    }
}