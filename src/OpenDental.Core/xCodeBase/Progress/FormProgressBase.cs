using System;
using System.Windows.Forms;

namespace CodeBase;

public class FormProgressBase : Form
{
    public bool ForceClose;

    private bool _hasClosed;

    public FormProgressBase()
    {
        FormClosed += FormProgressStatus_FormClosed;
        Shown += FormProgressStatus_Shown;
    }

    private void FormProgressStatus_Shown(object sender, EventArgs e)
    {
        ODEvent.Fired += ODEvent_Fired;

        var threadForceCloseMonitor = new ODThread(100, o =>
        {
            if (_hasClosed)
            {
                o.QuitAsync();
                return;
            }

            if (!ForceClose)
            {
                return;
            }

            this.InvokeIfRequired(() =>
            {
                DialogResult = DialogResult.OK;
                ODException.SwallowAnyException(Close);
            });

            o.QuitAsync();
        });

        threadForceCloseMonitor.AddExceptionHandler(_ => { });
        threadForceCloseMonitor.Name = "FormProgressStatusMonitor_" + DateTime.Now.Ticks;
        threadForceCloseMonitor.Start();
    }

    public DialogResult MsgBoxShow(string text, string caption = "")
    {
        return MessageBox.Show(this, text, caption);
    }

    public DialogResult MsgBoxShow(string text, string caption, MessageBoxButtons buttons)
    {
        return MessageBox.Show(this, text, caption, buttons);
    }

    public DialogResult MsgBoxShow(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return MessageBox.Show(this, text, caption, buttons, icon);
    }

    public void ODEvent_Fired(ODEventArgs e)
    {
        try
        {
            if (InvokeRequired)
            {
                Invoke((Action) delegate() { ODEvent_Fired(e); });
                return;
            }

            if (e.Tag == null)
            {
                return;
            }

            var progHelper = new ProgressBarHelper("");
            var hasProgHelper = false;

            string status;
            if (e.Tag is string s)
            {
                status = s;
            }
            else if (e.Tag.GetType() == typeof(ProgressBarHelper))
            {
                progHelper = (ProgressBarHelper) e.Tag;
                status = progHelper.LabelValue;
                hasProgHelper = true;
            }
            else
            {
                return;
            }

            UpdateProgress(status, progHelper, hasProgHelper);
        }
        catch
        {
            // ignored
        }
    }

    public virtual void UpdateProgress(string status, ProgressBarHelper progHelper, bool hasProgHelper)
    {
        throw new NotImplementedException();
    }

    private void FormProgressStatus_FormClosed(object sender, FormClosedEventArgs e)
    {
        _hasClosed = true;

        ODEvent.Fired -= ODEvent_Fired;
    }
}