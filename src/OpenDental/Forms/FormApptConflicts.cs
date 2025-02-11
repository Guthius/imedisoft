using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptConflicts : FormODBase
{
    private readonly List<Appointment> _appointments;
    private List<Patient> _patients;
    private bool _hasHeadingPrinted;
    private int _pagesPrinted;

    public FormApptConflicts(List<Appointment> appointments)
    {
        InitializeComponent();

        _appointments = appointments.Select(x => x.Copy()).ToList();
    }

    private void FormApptConflicts_Load(object sender, EventArgs e)
    {
        gridConflicts.ContextMenu = contextRightClick;

        FillGrid();
    }

    private void FillGrid()
    {
        Cursor = Cursors.WaitCursor;

        _patients = Patients.GetLimForPats(_appointments.Select(x => x.PatNum).Distinct().ToList());

        gridConflicts.BeginUpdate();

        gridConflicts.Columns.Clear();
        gridConflicts.Columns.Add(new GridColumn("Patient", 140));
        gridConflicts.Columns.Add(new GridColumn("Date", 120));
        gridConflicts.Columns.Add(new GridColumn("Op", 110));
        gridConflicts.Columns.Add(new GridColumn("Prov", 50));
        gridConflicts.Columns.Add(new GridColumn("Procedures", 150));
        gridConflicts.Columns.Add(new GridColumn("Notes", 200));

        gridConflicts.ListGridRows.Clear();

        foreach (var appointment in _appointments)
        {
            var patient = _patients.First(x => x.PatNum == appointment.PatNum);

            var gridRow = new GridRow();

            gridRow.Cells.Add(patient.GetNameLF());

            if (appointment.AptDateTime.Year < 1880)
            {
                gridRow.Cells.Add("");
            }
            else
            {
                gridRow.Cells.Add(appointment.AptDateTime.ToShortDateString() + "  " + appointment.AptDateTime.ToShortTimeString());
            }

            gridRow.Cells.Add(Operatories.GetAbbrev(appointment.Op));
            gridRow.Cells.Add(appointment.IsHygiene ? Providers.GetAbbr(appointment.ProvHyg) : Providers.GetAbbr(appointment.ProvNum));
            gridRow.Cells.Add(appointment.ProcDescript);
            gridRow.Cells.Add(appointment.Note);
            gridRow.Tag = appointment;

            gridConflicts.ListGridRows.Add(gridRow);
        }

        gridConflicts.EndUpdate();

        Cursor = Cursors.Default;
    }

    private void GridConflicts_DoubleClick(object sender, ODGridClickEventArgs e)
    {
        var currentSelection = e.Row;
        var currentScroll = gridConflicts.ScrollValue;

        var appointment = (Appointment) gridConflicts.ListGridRows[e.Row].Tag;
        var selectedPatNum = appointment.PatNum;
        var patient = _patients.First(x => x.PatNum == selectedPatNum);

        GlobalFormOpenDental.PatientSelected(patient, true);

        using var formApptEdit = new FormApptEdit(appointment.AptNum);

        formApptEdit.PinIsVisible = true;

        if (formApptEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (formApptEdit.PinClicked)
        {
            SendPinboard_Click();
        }

        gridConflicts.SetSelected(currentSelection);
        gridConflicts.ScrollValue = currentScroll;
    }

    private void MenuItemPin_Click(object sender, EventArgs e)
    {
        SendPinboard_Click();
    }

    private void SendPinboard_Click()
    {
        if (gridConflicts.SelectedIndices.Length == 0)
        {
            ShowError("Please select an appointment first.");
            return;
        }

        var selectedAptNums = gridConflicts.SelectedIndices
            .Select(index => ((Appointment) gridConflicts.ListGridRows[index].Tag).AptNum)
            .ToList();

        _appointments.RemoveAll(x => selectedAptNums.Contains(x.AptNum));

        FillGrid();

        GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, listPinApptNums: selectedAptNums, dateSelected: DateTime.Today); //Pins all appointments to the pinboard that were in listAptSelected.
    }

    private void MenuItemSelectPatient_Click(object sender, EventArgs e)
    {
        SelectPatient_Click();
    }

    private void SelectPatient_Click()
    {
        if (gridConflicts.SelectedIndices.Length == 0)
        {
            ShowError("Please select an appointment first.");
            return;
        }

        var patient = _patients.First(x => x.PatNum == _appointments[gridConflicts.SelectedIndices[gridConflicts.SelectedIndices.Length - 1]].PatNum);

        GlobalFormOpenDental.PatientSelected(patient, true);
    }

    private void ButtonPrint_Click(object sender, EventArgs e)
    {
        _pagesPrinted = 0;
        _hasHeadingPrinted = false;

        PrinterL.TryPrintOrDebugRpPreview(PrintPage, "Operatory Merge - conflict appointment List printed.");
    }

    private void PrintPage(object sender, PrintPageEventArgs e)
    {
        using var fontHeading = new Font("Arial", 13, FontStyle.Bold);
        using var fontSubHeading = new Font("Arial", 10, FontStyle.Bold);

        var y = e.MarginBounds.Top;
        var cx = e.MarginBounds.X + e.MarginBounds.Width / 2;

        var headingPrintH = 0;
        if (!_hasHeadingPrinted)
        {
            const string header = "Operatory Merge - Conflict Appointment List";

            e.Graphics.DrawString(header, fontHeading, Brushes.Black, cx - e.Graphics.MeasureString(header, fontHeading).Width / 2, y);
            y += 25;

            _hasHeadingPrinted = true;
            headingPrintH = y;
        }

        y = gridConflicts.PrintPage(e.Graphics, _pagesPrinted, e.MarginBounds, headingPrintH);

        _pagesPrinted++;

        e.HasMorePages = y == -1;
    }
}