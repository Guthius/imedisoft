using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormProcButtonQuickEdit : FormODBase
{
    public ProcButtonQuick ProcButtonQuickCur { get; set; }
    public bool IsNew { get; set; }

    public FormProcButtonQuickEdit()
    {
        InitializeComponent();
    }

    private void FormProcButtonQuickEdit_Load(object sender, EventArgs e)
    {
        textDescript.Text = ProcButtonQuickCur.Description;
        textProcedureCode.Text = ProcButtonQuickCur.CodeValue;
        textSurfaces.Text = ProcButtonQuickCur.Surf;

        checkIsLabel.Checked = ProcButtonQuickCur.IsLabel;

        if (!Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum))
        {
            return;
        }

        labelSurfaces.Visible = false;
        textSurfaces.Visible = false;
    }

    private void CheckBoxIsLabel_CheckedChanged(object sender, EventArgs e)
    {
        textProcedureCode.Enabled = !checkIsLabel.Checked;
        textSurfaces.Enabled = !checkIsLabel.Checked;
        butPickProc.Enabled = !checkIsLabel.Checked;
    }

    private void ButtonPickProc_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes();

        formProcCodes.IsSelectionMode = true;

        if (formProcCodes.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        textProcedureCode.Text = ProcedureCodes.GetProcCode(formProcCodes.CodeNumSelected).ProcCode;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (IsNew)
        {
            ProcButtonQuickCur = null;
            DialogResult = DialogResult.Cancel;
            return;
        }

        ProcButtonQuicks.Delete(ProcButtonQuickCur.ProcButtonQuickNum);

        ProcButtonQuickCur = null;

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        ProcButtonQuickCur.Description = textDescript.Text;
        ProcButtonQuickCur.CodeValue = textProcedureCode.Text;
        ProcButtonQuickCur.Surf = textSurfaces.Text;
        ProcButtonQuickCur.IsLabel = checkIsLabel.Checked;

        if (IsNew)
        {
            ProcButtonQuicks.Insert(ProcButtonQuickCur);
        }
        else
        {
            ProcButtonQuicks.Update(ProcButtonQuickCur);
        }

        DialogResult = DialogResult.OK;
    }
}