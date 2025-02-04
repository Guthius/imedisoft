using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormLabTurnaroundEdit : FormODBase
{
    private readonly LabTurnaround _labTurnaround;

    public FormLabTurnaroundEdit(LabTurnaround labTurnaround)
    {
        _labTurnaround = labTurnaround;

        InitializeComponent();
    }

    private void FormLabTurnaroundEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _labTurnaround.Description;

        if (_labTurnaround.DaysPublished > 0)
        {
            textDaysPublished.Text = _labTurnaround.DaysPublished.ToString();
        }

        textDaysActual.Text = _labTurnaround.DaysActual.ToString();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(textDaysActual.Text, out var daysActual) ||
            !int.TryParse(textDaysPublished.Text, out var daysPublished))
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        if (daysActual == 0)
        {
            ShowError("Actual Days cannot be zero.");
            return;
        }

        if (textDescription.Text == "")
        {
            ShowError("Please enter a description.");
            return;
        }

        _labTurnaround.Description = textDescription.Text;
        _labTurnaround.DaysPublished = daysPublished;
        _labTurnaround.DaysActual = daysActual;

        DialogResult = DialogResult.OK;
    }
}