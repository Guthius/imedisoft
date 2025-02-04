using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormStateAbbrEdit : FormODBase
{
    private readonly StateAbbr _stateAbbr;

    public FormStateAbbrEdit(StateAbbr stateAbbr)
    {
        _stateAbbr = stateAbbr;

        InitializeComponent();
    }

    private void FormStateAbbrEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _stateAbbr.Description;
        textAbbr.Text = _stateAbbr.Abbr;

        if (PrefC.GetBool(PrefName.EnforceMedicaidIDLength))
        {
            if (_stateAbbr.MedicaidIDLength != 0)
            {
                textMedIDLength.Text = _stateAbbr.MedicaidIDLength.ToString();
            }
        }
        else
        {
            labelMedIDLength.Visible = false;
            textMedIDLength.Visible = false;
            Height -= 30;
        }
    }

    private void butDelete_Click(object sender, EventArgs e)
    {
        if (_stateAbbr.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, "Delete State Abbr?"))
        {
            return;
        }

        StateAbbrs.Delete(_stateAbbr.StateAbbrNum);
        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text == "")
        {
            ShowError("Description cannot be blank.");
            return;
        }

        if (textAbbr.Text == "")
        {
            ShowError("Abbrevation cannot be blank.");
            return;
        }

        if (textMedIDLength.Visible && !textMedIDLength.IsValid())
        {
            ShowError("Medicaid ID length is invalid.");
            return;
        }

        _stateAbbr.Description = textDescription.Text;
        _stateAbbr.Abbr = textAbbr.Text;

        if (PrefC.GetBool(PrefName.EnforceMedicaidIDLength))
        {
            _stateAbbr.MedicaidIDLength = 0;
            if (textMedIDLength.Text != "")
            {
                _stateAbbr.MedicaidIDLength = SIn.Int(textMedIDLength.Text);
            }
        }

        if (_stateAbbr.IsNew)
        {
            StateAbbrs.Insert(_stateAbbr);
        }
        else
        {
            StateAbbrs.Update(_stateAbbr);
        }

        DialogResult = DialogResult.OK;
    }
}