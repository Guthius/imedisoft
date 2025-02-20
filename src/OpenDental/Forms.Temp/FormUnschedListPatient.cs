using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormUnschedListPatient : FormODBase
{
    private readonly Patient _patient;
    private List<Appointment> _appointmentsForPatUnsched;

    public Appointment Appointment;

    public FormUnschedListPatient(Patient patient)
    {
        _patient = patient;

        InitializeComponent();
    }

    private void FormPatientUnschedList_Load(object sender, EventArgs e)
    {
        Text = " " + _patient.GetNameLF();

        _appointmentsForPatUnsched = Appointments.GetUnschedApptsForPat(_patient.PatNum);

        FillGrid();
    }

    private void FillGrid()
    {
        Cursor = Cursors.WaitCursor;

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 65, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("AptStatus", 90));
        gridMain.Columns.Add(new GridColumn("UnschedStatus", 110));
        gridMain.Columns.Add(new GridColumn("Prov", 80));
        gridMain.Columns.Add(new GridColumn("Procedures", 150));
        gridMain.Columns.Add(new GridColumn("Notes", 200));

        gridMain.ListGridRows.Clear();

        foreach (var appointment in _appointmentsForPatUnsched)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(appointment.AptDateTime.ToShortDateString());
            gridRow.Cells.Add(appointment.AptStatus.ToString());
            gridRow.Cells.Add(appointment.UnschedStatus.ToString());
            gridRow.Cells.Add(Providers.GetAbbr(appointment.ProvNum));
            gridRow.Cells.Add(appointment.ProcDescript);
            gridRow.Cells.Add(appointment.Note);
            gridRow.Tag = appointment;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        Cursor = Cursors.Default;
    }

    private void SetSelectedAppt()
    {
        Appointment = gridMain.SelectedTag<Appointment>();
        if (Appointment is null)
        {
            ShowError("Please select an unscheduled appointment to use.");
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SetSelectedAppt();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SetSelectedAppt();
    }
}