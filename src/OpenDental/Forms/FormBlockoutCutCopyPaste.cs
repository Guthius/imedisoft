using System;
using System.Windows.Forms;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormBlockoutCutCopyPaste : FormODBase
{
    private static DateTime _dateCopyStart = DateTime.MinValue;
    private static DateTime _dateCopyEnd = DateTime.MinValue;
    private static long _previousApptViewNum;

    public long ApptViewNum;

    public DateTime SelectedDate { get; set; }

    public FormBlockoutCutCopyPaste()
    {
        InitializeComponent();
    }

    private void FormBlockoutCutCopyPaste_Load(object sender, EventArgs e)
    {
        if (SelectedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            checkWeekend.Checked = true;
        }

        if (ApptViewNum != _previousApptViewNum)
        {
            _dateCopyStart = DateTime.MinValue;
            _dateCopyEnd = DateTime.MinValue;
        }

        FillClipboard();

        _previousApptViewNum = ApptViewNum;
    }

    private void ButtonClearDay_Click(object sender, EventArgs e)
    {
        var abbr = Clinics.GetAbbr(Clinics.ClinicNum);

        if (!ConfirmOk(
                "Clear all blockouts for day for clinic: " + abbr + "?\r\n" +
                "(This may include blockouts not shown in the current appointment view)"))
        {
            return;
        }

        Schedules.ClearBlockoutsForClinic(Clinics.ClinicNum, SelectedDate);
        Schedules.BlockoutLogHelper(BlockoutAction.Clear, dateTime: SelectedDate, clinicNum: Clinics.ClinicNum);

        Close();
    }

    private void FillClipboard()
    {
        if (_dateCopyStart.Year < 1880)
        {
            textClipboard.Text = "";
        }
        else if (_dateCopyStart == _dateCopyEnd)
        {
            textClipboard.Text = _dateCopyStart.ToShortDateString();
        }
        else
        {
            textClipboard.Text = _dateCopyStart.ToShortDateString() + "-" + _dateCopyEnd.ToShortDateString();
        }
    }

    private void ButtonCopyDay_Click(object sender, EventArgs e)
    {
        _dateCopyStart = SelectedDate;
        _dateCopyEnd = SelectedDate;

        Close();
    }

    private void ButtonCopyWeek_Click(object sender, EventArgs e)
    {
        _dateCopyStart = SelectedDate.DayOfWeek == DayOfWeek.Sunday ? SelectedDate.AddDays(-6) : SelectedDate.AddDays(1 - (int) SelectedDate.DayOfWeek);
        _dateCopyEnd = _dateCopyStart.AddDays(checkWeekend.Checked ? 6 : 4);

        Close();
    }

    private void ButtonPaste_Click(object sender, EventArgs e)
    {
        CopyOverBlockouts(1);
    }

    private void ButtonRepeat_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(textRepeat.Text, out var repeat))
        {
            ShowError("Please fix number box first.");

            return;
        }

        CopyOverBlockouts(repeat);
    }

    private void CopyOverBlockouts(int repeat)
    {
        if (_dateCopyStart.Year < 1880)
        {
            ShowError("Please copy a selection to the clipboard first.");
            return;
        }

        if (SelectedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            if (!checkWeekend.Checked)
            {
                ShowError("You must check 'Include Weekends' if you would like to paste into weekends.");
                return;
            }
        }

        DateTime dateStart;
        DateTime dateEnd;

        var isWeek = _dateCopyStart != _dateCopyEnd;
        if (isWeek)
        {
            dateStart = SelectedDate.DayOfWeek == DayOfWeek.Sunday ? SelectedDate.AddDays(-6) : SelectedDate.AddDays(1 - (int) SelectedDate.DayOfWeek);
            dateEnd = dateStart.AddDays((_dateCopyEnd - _dateCopyStart).Days);
        }
        else
        {
            dateStart = SelectedDate;
            dateEnd = SelectedDate;
        }

        if (dateStart == _dateCopyStart && repeat == 1)
        {
            ShowError("Not allowed to paste back onto the same date as is on the clipboard.");
            return;
        }

        Cursor = Cursors.WaitCursor;

        var errors = Schedules.CopyBlockouts(ApptViewNum, isWeek, checkWeekend.Checked, checkReplace.Checked, _dateCopyStart, _dateCopyEnd, dateStart, dateEnd, repeat);

        Cursor = Cursors.Default;

        if (!string.IsNullOrEmpty(errors))
        {
            ShowError(errors);
            return;
        }

        Close();
    }
}