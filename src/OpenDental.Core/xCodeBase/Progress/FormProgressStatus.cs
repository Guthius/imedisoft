using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBase;

public partial class FormProgressStatus : FormProgressBase
{
    private readonly bool _hasHistory;

    private DateTime _dateTimeLastEvent;

    public FormProgressStatus(bool hasHistory = false, bool hasMinimize = true, string startingMessage = "", ProgressBarStyle progStyle = ProgressBarStyle.Marquee)
    {
        InitializeComponent();

        labelMsg.Text = startingMessage;
        progressBar.Style = progStyle;

        if (!hasMinimize)
        {
            panelMinimize.Visible = false;
        }

        ControlBox = false;

        _hasHistory = hasHistory;
        if (!_hasHistory)
        {
            return;
        }

        Height += 120;
        Width += 60;

        _dateTimeLastEvent = DateTime.MinValue;

        labelMsg.Visible = false;
        textHistoryMsg.Visible = true;
    }

    public sealed override void UpdateProgress(string status, ProgressBarHelper progHelper, bool hasProgHelper)
    {
        if (Visible && _hasHistory && !progressBar.Visible)
        {
            return;
        }

        labelMsg.Text = status;

        if (hasProgHelper)
        {
            if (progHelper.BlockMax != 0)
            {
                progressBar.Maximum = (int) progHelper.BlockMax;
            }

            if (progHelper.BlockValue != 0)
            {
                progressBar.Value = (int) progHelper.BlockValue;
            }

            progressBar.Style = progHelper.ProgressStyle switch
            {
                ProgBarStyle.Marquee => ProgressBarStyle.Marquee,
                ProgBarStyle.Blocks => ProgressBarStyle.Blocks,
                ProgBarStyle.Continuous => ProgressBarStyle.Continuous,
                _ => progressBar.Style
            };
        }

        if (!_hasHistory)
        {
            return;
        }

        if (_dateTimeLastEvent == DateTime.MinValue)
        {
            textHistoryMsg.AppendText(status.PadRight(60));
        }
        else
        {
            textHistoryMsg.AppendText(GetElapsedTime(_dateTimeLastEvent, DateTime.Now) + "\r\n" + status.PadRight(60));
        }

        _dateTimeLastEvent = DateTime.Now;
    }

    private static string GetElapsedTime(DateTime start, DateTime end)
    {
        var timeElapsed = new TimeSpan(end.Ticks - start.Ticks);
        if (timeElapsed.TotalMinutes > 2)
        {
            return "Elapsed Time: " + timeElapsed.TotalMinutes + " minutes";
        }

        if (timeElapsed.TotalSeconds > 2)
        {
            return "Elapsed Time: " + timeElapsed.TotalSeconds + " seconds";
        }

        return "Elapsed Time: " + timeElapsed.TotalMilliseconds + " milliseconds";
    }

    private void labelMinimize_Click(object sender, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;

        Owner?.Focus();
    }

    private void labelMinimize_MouseEnter(object sender, EventArgs e)
    {
        labelMinimize.BackColor = SystemColors.ControlDark;
    }

    private void labelMinimize_MouseLeave(object sender, EventArgs e)
    {
        labelMinimize.BackColor = SystemColors.ControlLight;
    }

    private void butCopyToClipboard_Click(object sender, EventArgs e)
    {
        try
        {
            ODClipboard.SetClipboard(textHistoryMsg.Text);
            MessageBox.Show("Copied");
        }
        catch (Exception)
        {
            MessageBox.Show("Could not copy contents to the clipboard.  Please try again.");
        }
    }

    private void butClose_Click(object sender, EventArgs e)
    {
        ForceClose = true;
    }
}