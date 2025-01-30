using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAllergyDefEdit : FormODBase
{
    private readonly AllergyDef _allergyDef;

    public FormAllergyDefEdit(AllergyDef allergyDef)
    {
        _allergyDef = allergyDef;

        InitializeComponent();
    }

    private void FormAllergyDefEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _allergyDef.Description;

        if (!_allergyDef.IsNew)
        {
            checkHidden.Checked = _allergyDef.IsHidden;
        }

        for (var i = 0; i < Enum.GetNames(typeof(SnomedAllergy)).Length; i++)
        {
            comboSnomedAllergyType.Items.Add(Enum.GetNames(typeof(SnomedAllergy))[i]);
        }

        comboSnomedAllergyType.SelectedIndex = (int) _allergyDef.SnomedType;

        textMedication.Text = Medications.GetDescription(_allergyDef.MedicationNum);

        if (Security.IsAuthorized(EnumPermType.AllergyDefEdit))
        {
            return;
        }

        butSave.Enabled = false;
        butDelete.Enabled = false;
    }

    private void ButtonMedicationSelect_Click(object sender, EventArgs e)
    {
        using var formMedications = new FormMedications();

        formMedications.IsSelectionMode = true;

        if (formMedications.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _allergyDef.MedicationNum = formMedications.SelectedMedicationNum;

        textMedication.Text = Medications.GetDescription(_allergyDef.MedicationNum);
    }

    private void ButtonNone_Click(object sender, EventArgs e)
    {
        _allergyDef.MedicationNum = 0;
        textMedication.Text = "";
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text.Trim() == "")
        {
            ShowError("Description cannot be blank.");
            return;
        }

        _allergyDef.Description = textDescription.Text;
        _allergyDef.IsHidden = checkHidden.Checked;
        _allergyDef.SnomedType = (SnomedAllergy) comboSnomedAllergyType.SelectedIndex;
        _allergyDef.UniiCode = string.Empty;

        if (_allergyDef.IsNew)
        {
            AllergyDefs.Insert(_allergyDef);
        }
        else
        {
            AllergyDefs.Update(_allergyDef);
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_allergyDef.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (AllergyDefs.DefIsInUse(_allergyDef.AllergyDefNum))
        {
            ShowError("Cannot delete allergies in use.");
            return;
        }

        if (!ConfirmOk("Delete Allergy?"))
        {
            return;
        }

        AllergyDefs.Delete(_allergyDef.AllergyDefNum);

        DialogResult = DialogResult.OK;
    }
}