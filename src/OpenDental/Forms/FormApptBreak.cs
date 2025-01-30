using System;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptBreak : FormODBase
{
    private readonly Appointment _appointment;

    public ApptBreakSelection SelectedApptBreak { get; set; }
    public ProcedureCode SelectedProcedureCode { get; set; }

    public FormApptBreak(Appointment appointment)
    {
        InitializeComponent();

        _appointment = appointment;
    }

    private void FormApptBreak_Load(object sender, EventArgs e)
    {
        var brokenApptProcs = (BrokenApptProcedure) PrefC.GetInt(PrefName.BrokenApptProcedure);

        radioMissed.Enabled = brokenApptProcs is BrokenApptProcedure.Missed or BrokenApptProcedure.Both;
        radioCancelled.Enabled = brokenApptProcs is BrokenApptProcedure.Cancelled or BrokenApptProcedure.Both;

        if (!radioMissed.Enabled && radioCancelled.Enabled)
        {
            radioCancelled.Checked = true;
        }
    }

    private bool ValidateSelection()
    {
        if (radioMissed.Checked || radioCancelled.Checked)
        {
            return true;
        }

        ShowError("Please select a broken procedure type.");

        return false;
    }

    private void DisplayFormAsapForWebSched()
    {
        if (!AppointmentL.PromptTextAsapList(_appointment.ClinicNum))
        {
            return;
        }

        var schedulesDataTable = Schedules.GetPeriodSchedule(_appointment.AptDateTime, _appointment.AptDateTime, [_appointment.Op], false);
        var schedules = Schedules.ConvertTableToList(schedulesDataTable);

        DateRange dateRange;

        try
        {
            dateRange = AppointmentL.GetAsapRange(_appointment.Op, _appointment.AptDateTime, _appointment.AptNum, schedules);
        }
        catch (ODException ex)
        {
            ShowError(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            ShowException(ex, "Unexpected error occurred.");
            return;
        }

        using var formAsap = new FormASAP(_appointment.Op);

        formAsap.DateTimeChosen = _appointment.AptDateTime;
        formAsap.DateTimeSlotStart = dateRange.Start;
        formAsap.DateTimeSlotEnd = dateRange.End;

        formAsap.ShowDialog();
    }

    private void ButtonUnsched_Click(object sender, EventArgs e)
    {
        if (!ValidateSelection())
        {
            return;
        }

        if (PrefC.GetBool(PrefName.UnscheduledListNoRecalls) && Appointments.IsRecallAppointment(_appointment))
        {
            if (!Confirm("Recall appointments cannot be sent to the Unscheduled List.\r\nDelete appointment instead?"))
            {
                return;
            }

            SelectedApptBreak = ApptBreakSelection.Delete;

            DialogResult = DialogResult.Cancel;

            return;
        }

        DisplayFormAsapForWebSched();

        SelectedApptBreak = ApptBreakSelection.Unsched;
        DialogResult = DialogResult.OK;
    }

    private void ButtonPinboard_Click(object sender, EventArgs e)
    {
        if (!ValidateSelection())
        {
            return;
        }

        DisplayFormAsapForWebSched();

        SelectedApptBreak = ApptBreakSelection.Pinboard;
        DialogResult = DialogResult.OK;
    }

    private void ButtonApptBook_Click(object sender, EventArgs e)
    {
        if (!ValidateSelection())
        {
            return;
        }

        SelectedApptBreak = ApptBreakSelection.ApptBook;
        DialogResult = DialogResult.OK;
    }

    private void FormApptBreak_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK)
        {
            return;
        }

        SelectedProcedureCode = ProcedureCodes.GetProcCode("D9987");

        if (radioMissed.Checked)
        {
            SelectedProcedureCode = ProcedureCodes.GetProcCode("D9986");
        }
    }
}

public enum ApptBreakSelection
{
    None,
    Unsched,
    Pinboard,
    ApptBook,
    Delete
}