using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormChartViewDateFilter : FormODBase
{
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }

    public FormChartViewDateFilter()
    {
        InitializeComponent();
    }

    private void FormChartViewDateFilter_Load(object sender, EventArgs e)
    {
        var chartViewDatesNames = Enum.GetNames(typeof(ChartViewDates));

        foreach (var name in chartViewDatesNames)
        {
            listPresetDateRanges.Items.Add(name);
        }

        FillDateTextBoxesHelper();
    }

    private void ListBoxPresetDateRanges_MouseClick(object sender, MouseEventArgs e)
    {
        var chartViewDates = (ChartViewDates) listPresetDateRanges.IndexFromPoint(e.Location);
        switch (chartViewDates)
        {
            case ChartViewDates.All:
                DateStart = DateTime.MinValue;
                DateEnd = DateTime.MinValue;
                break;

            case ChartViewDates.Today:
                DateStart = DateTime.Today;
                DateEnd = DateTime.Today;
                break;

            case ChartViewDates.Yesterday:
                DateStart = DateTime.Today.AddDays(-1);
                DateEnd = DateTime.Today.AddDays(-1);
                break;

            case ChartViewDates.ThisYear:
                DateStart = new DateTime(DateTime.Today.Year, 1, 1);
                DateEnd = new DateTime(DateTime.Today.Year, 12, 31);
                break;

            case ChartViewDates.LastYear:
                DateStart = new DateTime(DateTime.Today.Year - 1, 1, 1);
                DateEnd = new DateTime(DateTime.Today.Year - 1, 12, 31);
                break;
        }

        FillDateTextBoxesHelper();
    }

    private void FillDateTextBoxesHelper()
    {
        textDateStart.Text = DateStart.ToShortDateString();
        textDateEnd.Text = DateEnd.ToShortDateString();

        if (DateStart.Year < 1880)
        {
            textDateStart.Text = "";
        }

        if (DateEnd.Year < 1880)
        {
            textDateEnd.Text = "";
        }
    }

    private void ButtonNowStart_Click(object sender, EventArgs e)
    {
        DateStart = DateTime.Today;

        textDateStart.Text = DateStart.ToShortDateString();
    }

    private void ButtonNowEnd_Click(object sender, EventArgs e)
    {
        DateEnd = DateTime.Today;

        textDateEnd.Text = DateEnd.ToShortDateString();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (!textDateStart.IsValid() || !textDateEnd.IsValid())
        {
            ShowError("Please fix data entry errors first.");

            return;
        }

        DateStart = SIn.Date(textDateStart.Text);
        DateEnd = SIn.Date(textDateEnd.Text);

        DialogResult = DialogResult.OK;
    }
}