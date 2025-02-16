using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormEtrans835s : FormODBase
{
    private readonly List<X835Status> _x835Statuses = [];
    private DateTime _dateFrom = DateTime.MaxValue;
    private DateTime _dateTo = DateTime.MaxValue;

    public FormEtrans835s()
    {
        InitializeComponent();
    }

    private void FormEtrans835s_Load(object sender, EventArgs e)
    {
        SetFilterControlsAndAction(FilterAndFillGrid,
            textRangeMin,
            textRangeMax,
            textControlId,
            textCarrier,
            textCheckTrace,
            comboClinics,
            checkShowFinalizedOnly,
            checkAutomatableCarriersOnly,
            dateRangePicker);

        dateRangePicker.SetDateTimeFrom(DateTime.Today.AddDays(-7));
        dateRangePicker.SetDateTimeTo(DateTime.Today);

        comboClinics.IsAllSelected = true;

        if (PrefC.GetBool(PrefName.EraShowStatusAndClinic))
        {
            checkShowFinalizedOnly.Visible = false;
        }
        else
        {
            labelStatus.Visible = false;
            listStatus.Visible = false;
            checkAutomatableCarriersOnly.Visible = false;
            comboClinics.Visible = false;
        }

        for (var i = 0; i < Enum.GetValues(typeof(X835Status)).Length; i++)
        {
            var x835Status = (X835Status) i;
            if (x835Status is X835Status.None or X835Status.FinalizedSomeDetached or X835Status.FinalizedAllDetached)
            {
                continue;
            }

            listStatus.Items.Add(x835Status.GetDescription());

            _x835Statuses.Add(x835Status);

            var isSelected = x835Status != X835Status.Finalized;

            listStatus.SetSelected(listStatus.Items.Count - 1, isSelected);
        }
    }

    private void FormEtrans835s_Shown(object sender, EventArgs e)
    {
        if (!PrefC.GetBool(PrefName.EraRefreshOnLoad))
        {
            return;
        }

        FilterAndFillGrid();

        SecurityLogs.MakeLogEntry(EnumPermType.InsPayCreate, 0, "Window 'Electronic EOBs - ERA 835s' opened.");
    }

    private void FillGrid(List<long> listSelectedClinicNums, string carrierName, string checkTraceNum, string amountMin, string amountMax, string controlId, bool doShowAutomatableCarriersOnly)
    {
        Cursor = Cursors.WaitCursor;

        labelControlId.Visible = PrefC.GetBool(PrefName.EraShowControlIdFilter);
        textControlId.Visible = PrefC.GetBool(PrefName.EraShowControlIdFilter);

        var showStatusAndClinics = PrefC.GetBool(PrefName.EraShowStatusAndClinic);

        _dateFrom = dateRangePicker.GetDateTimeFrom();
        _dateTo = dateRangePicker.GetDateTimeTo(isDefaultMaxDateT: true);

        var progress = new ProgressWin
        {
            ActionMain = () => { EtransL.AddMissingEtrans835s(_dateFrom, _dateTo); }
        };

        progress.ShowDialog();
        progress = new ProgressWin
        {
            ActionMain = () =>
            {
                if (comboClinics.ListClinicNumsSelected.Count == 0)
                {
                    comboClinics.IsAllSelected = true;
                }

                var x835Statuses = new List<X835Status>();
                if (showStatusAndClinics)
                {
                    foreach (var index in listStatus.SelectedIndices)
                    {
                        x835Statuses.Add(_x835Statuses[index]);
                    }

                    if (x835Statuses.Contains(X835Status.Finalized))
                    {
                        x835Statuses.Add(X835Status.FinalizedAllDetached);
                        x835Statuses.Add(X835Status.FinalizedSomeDetached);
                    }
                }
                else if (checkShowFinalizedOnly.Checked)
                {
                    x835Statuses = new List<X835Status>([X835Status.Finalized, X835Status.FinalizedAllDetached, X835Status.FinalizedSomeDetached]);
                }
                else
                {
                    x835Statuses = new List<X835Status>([X835Status.NotFinalized, X835Status.Partial, X835Status.Unprocessed]);
                }

                var eraDataFiltered = EtransL.GetEraDataFiltered(showStatusAndClinics, x835Statuses, listSelectedClinicNums, amountMin, amountMax, _dateFrom, _dateTo, carrierName, checkTraceNum, controlId, doShowAutomatableCarriersOnly, comboClinics.IsAllSelected);

                gridMain.Invoke(gridMain.BeginUpdate);

                gridMain.Columns.Clear();
                gridMain.Columns.Add(new GridColumn("Patient Name", 250));
                gridMain.Columns.Add(new GridColumn("Carrier Name", 190));

                if (showStatusAndClinics)
                {
                    gridMain.Columns.Add(new GridColumn("Status", 80));
                }

                gridMain.Columns.Add(new GridColumn("Date", 80, GridSortingStrategy.DateParse));
                gridMain.Columns.Add(new GridColumn("Amount", 80, GridSortingStrategy.AmountParse));
                if (showStatusAndClinics)
                {
                    gridMain.Columns.Add(new GridColumn("Clinic", 70));
                }

                gridMain.Columns.Add(new GridColumn("Code", 37, HorizontalAlignment.Center));

                if (PrefC.GetBool(PrefName.EraShowControlIdFilter))
                {
                    gridMain.Columns.Add(new GridColumn("ControlID", 70) {IsWidthDynamic = true});
                }

                gridMain.Columns.Add(new GridColumn("Note", 250) {IsWidthDynamic = true, DynamicWeight = 2});
                gridMain.ListGridRows.Clear();

                for (var i = 0; i < eraDataFiltered.ListEtrans.Count; i++)
                {
                    ODEvent.Fire(ODEventType.ProgressBar, "Filling grid rows " + (i + 1) + "/" + eraDataFiltered.ListEtrans.Count);

                    var etrans835 = eraDataFiltered.ListEtrans835s[i];

                    var gridRow = new GridRow();

                    gridRow.Cells.Add(etrans835.PatientName);
                    gridRow.Cells.Add(etrans835.PayerName);

                    if (showStatusAndClinics)
                    {
                        gridRow.Cells.Add(etrans835.Status.GetDescription());
                    }

                    gridRow.Cells.Add(SOut.Date(eraDataFiltered.ListEtrans[i].DateTimeTrans));
                    gridRow.Cells.Add(SOut.Double(etrans835.InsPaid));

                    if (showStatusAndClinics)
                    {
                        var clinicNums = eraDataFiltered.ListAttached
                            .Where(x => x.EtransNum == eraDataFiltered.ListEtrans[i].EtransNum)
                            .Select(x => x.ClinicNum).Distinct()
                            .ToList();

                        var clinicAbbr = clinicNums.Count switch
                        {
                            1 => clinicNums[0] == 0 ? "Unassigned" : Clinics.GetAbbr(clinicNums[0]),
                            > 1 => "(Multiple)",
                            _ => ""
                        };

                        gridRow.Cells.Add(clinicAbbr);
                    }

                    gridRow.Cells.Add(etrans835.PaymentMethodCode);

                    if (PrefC.GetBool(PrefName.EraShowControlIdFilter))
                    {
                        gridRow.Cells.Add(etrans835.ControlId);
                    }

                    gridRow.Cells.Add(eraDataFiltered.ListEtrans[i].Note);
                    gridRow.Tag = eraDataFiltered.ListEtrans[i];

                    gridMain.ListGridRows.Add(gridRow);
                }

                gridMain.Invoke(gridMain.EndUpdate);
            }
        };

        progress.ShowDialog();

        Cursor = Cursors.Default;
    }

    private void FilterAndFillGrid()
    {
        var clinicNums = comboClinics.ListClinicNumsSelected;

        FillGrid(
            listSelectedClinicNums: clinicNums,
            carrierName: textCarrier.Text,
            checkTraceNum: textCheckTrace.Text,
            amountMin: textRangeMin.Text,
            amountMax: textRangeMax.Text,
            controlId: textControlId.Text,
            doShowAutomatableCarriersOnly: checkAutomatableCarriersOnly.Checked);
    }

    private void ButtonRefresh_Click(object sender, EventArgs e)
    {
        FilterAndFillGrid();
    }

    private void GridMain_DoubleClick(object sender, EventArgs e)
    {
        var selectedIndex = gridMain.GetSelectedIndex();
        if (selectedIndex == -1)
        {
            return;
        }

        Cursor = Cursors.WaitCursor;

        var etrans = (Etrans) gridMain.ListGridRows[selectedIndex].Tag;

        etrans = Etranss.GetEtrans(etrans.EtransNum);

        if (etrans is null)
        {
            Cursor = Cursors.Default;

            ShowError("ERA could not be found, it was most likely deleted.");

            FilterAndFillGrid();

            return;
        }

        EtransL.ViewFormForEra(etrans, this);

        Cursor = Cursors.Default;
    }

    private void ListBoxStatus_MouseUp(object sender, MouseEventArgs e)
    {
        FilterAndFillGrid();
    }

    private void ButtonAutoProcessedEras_Click(object sender, EventArgs e)
    {
        var displayReportEraAutoProcessed = DisplayReports.GetByInternalName(DisplayReports.ReportNames.EraAutoProcessed);
        if (displayReportEraAutoProcessed is null)
        {
            ShowError("The " + DisplayReports.ReportNames.EraAutoProcessed + " report could not be found.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.Reports, displayReportEraAutoProcessed.DisplayReportNum, suppressMessage: false))
        {
            return;
        }

        var formRpEraAutoProcessed = new FormRpEraAutoProcessed();

        formRpEraAutoProcessed.Show();
    }
}