using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormGuardianEdit : FormODBase
{
    private readonly Guardian _guardian;
    private readonly Family _family;
    private List<string> _guardianRelationshipNames;

    public FormGuardianEdit(Guardian guardian, Family family)
    {
        InitializeComponent();

        _guardian = guardian;
        _family = family;
    }

    private void FormGuardianEdit_Load(object sender, EventArgs e)
    {
        textPatient.Text = _family.GetNameInFamFL(_guardian.PatNumChild);
        if (_guardian.PatNumGuardian != 0)
        {
            textFamilyMember.Text = _family.GetNameInFamFL(_guardian.PatNumGuardian);
        }

        var patientChild = Patients.GetPat(_guardian.PatNumChild);
        if (_guardian.IsNew)
        {
            if (patientChild.Position == PatientPosition.Child)
            {
                checkIsGuardian.Checked = true;
            }
        }
        else
        {
            checkIsGuardian.Checked = _guardian.IsGuardian;
        }

        _guardianRelationshipNames = new List<string>(Enum.GetNames(typeof(GuardianRelationship)));
        _guardianRelationshipNames.Sort();

        for (var i = 0; i < _guardianRelationshipNames.Count; i++)
        {
            comboRelationship.Items.Add(_guardianRelationshipNames[i]);
            if (_guardianRelationshipNames[i] == _guardian.Relationship.ToString())
            {
                comboRelationship.SelectedIndex = i;
            }
        }
    }

    private void ButtonPick_Click(object sender, EventArgs e)
    {
        var frmFamilyMemberSelect = new FrmFamilyMemberSelect(_family);

        frmFamilyMemberSelect.ShowDialog();

        if (frmFamilyMemberSelect.IsDialogCancel)
        {
            return;
        }

        _guardian.PatNumGuardian = frmFamilyMemberSelect.SelectedPatNum;
        
        textFamilyMember.Text = _family.GetNameInFamFL(_guardian.PatNumGuardian);
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_guardian.IsNew)
        {
            DialogResult = DialogResult.Cancel;
        }
        else
        {
            Guardians.Delete(_guardian.GuardianNum);

            DialogResult = DialogResult.OK;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        _guardian.IsGuardian = checkIsGuardian.Checked;

        var guardianRelationshipName = comboRelationship.GetSelected<string>();
        var guardianRelationshipNamesRaw = new List<string>(Enum.GetNames(typeof(GuardianRelationship)));

        _guardian.Relationship = (GuardianRelationship) guardianRelationshipNamesRaw.IndexOf(guardianRelationshipName);

        if (_guardian.IsNew)
        {
            Guardians.Insert(_guardian);
        }
        else
        {
            Guardians.Update(_guardian);
        }

        DialogResult = DialogResult.OK;
    }
}