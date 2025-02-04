using System;
using System.Windows.Forms;

namespace OpenDental.Forms;

public partial class FormCalendar : FormODBase
{
    public DateTime SelectedDate { get; set; }
    public DateTime MinDate { get; set; }

    public FormCalendar()
    {
        InitializeComponent();
    }

    private void FormDatePicker_Load(object sender, EventArgs e)
    {
        monthCalendar.SetDate(SelectedDate);
        monthCalendar.MinDate = MinDate;
    }

    private void MonthCalendar_DateChanged(object sender, DateRangeEventArgs e)
    {
        SelectedDate = monthCalendar.SelectionStart;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }
}