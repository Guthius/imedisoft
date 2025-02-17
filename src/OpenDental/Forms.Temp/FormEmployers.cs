using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormEmployers : FormODBase
{
    private readonly List<Employer> _employers = [];

    public FormEmployers()
    {
        InitializeComponent();
    }

    private void FormEmployers_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        Employers.RefreshCache();

        _employers.Clear();

        var employers = Employers.GetListDeep();
        foreach (var employer in employers)
        {
            _employers.Add(employer);
        }

        _employers.Sort(CompareEmployers);

        listEmp.Items.Clear();

        foreach (var employer in _employers)
        {
            listEmp.Items.Add(employer.EmpName);
        }
    }

    private static int CompareEmployers(Employer employer1, Employer employer2)
    {
        return string.Compare(employer1.EmpName, employer2.EmpName, StringComparison.Ordinal);
    }

    private void ListBoxEmp_DoubleClick(object sender, EventArgs e)
    {
        if (listEmp.SelectedIndices.Count == 0)
        {
            return;
        }

        var frmEmployerEdit = new FrmEmployerEdit
        {
            EmployerCur = _employers[listEmp.SelectedIndices[0]]
        };

        frmEmployerEdit.ShowDialog();

        if (!frmEmployerEdit.IsDialogOK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var frmEmployerEdit = new FrmEmployerEdit
        {
            EmployerCur = new Employer(),
            IsNew = true
        };

        frmEmployerEdit.ShowDialog();

        FillGrid();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (listEmp.SelectedIndices.Count != 1)
        {
            ShowError("Please select one item first.");

            return;
        }

        var dependentNames = Employers.DependentPatients(_employers[listEmp.SelectedIndices[0]]);
        if (dependentNames != "")
        {
            ShowError(
                "Not allowed to delete this employer because it it attached to the following patients. " +
                "You should combine employers instead.\r\n\r\n" +
                dependentNames);

            return;
        }

        dependentNames = Employers.DependentInsPlans(_employers[listEmp.SelectedIndices[0]]);
        if (dependentNames != "")
        {
            ShowError(
                "Not allowed to delete this employer because it is attached to the following insurance plans. " +
                "You should combine employers instead.\r\n\r\n" +
                dependentNames);

            return;
        }

        if (!ConfirmOk("Delete Employer?"))
        {
            return;
        }

        Employers.Delete(_employers[listEmp.SelectedIndices[0]]);

        FillGrid();
    }

    private void ButtonEdit_Click(object sender, EventArgs e)
    {
        if (listEmp.SelectedIndices.Count != 1)
        {
            ShowError("Please select one item first.");

            return;
        }

        var frmEmployerEdit = new FrmEmployerEdit
        {
            EmployerCur = _employers[listEmp.SelectedIndices[0]]
        };

        frmEmployerEdit.ShowDialog();

        if (!frmEmployerEdit.IsDialogOK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonCombine_Click(object sender, EventArgs e)
    {
        if (listEmp.SelectedIndices.Count < 2)
        {
            ShowError("Please select multiple items first while holding down the control key.");
            return;
        }

        if (!ConfirmOk("Combine all these employers into a single employer? This will affect all patients using these employers."))
        {
            return;
        }

        var employerNums = new List<long>();
        foreach (var index in listEmp.SelectedIndices)
        {
            employerNums.Add(_employers[index].EmployerNum);
        }

        Employers.Combine(employerNums);
        
        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        DataValid.SetInvalid(InvalidType.Employers);

        DialogResult = DialogResult.OK;
    }
}