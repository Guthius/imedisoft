using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCertEmployee : FormODBase
{
    private readonly Employee _employee;
    private readonly Cert _cert;
    private readonly CertEmployee _certEmployee;

    public FormCertEmployee(Employee employee, Cert cert, CertEmployee certEmployee)
    {
        _employee = employee;
        _cert = cert;
        _certEmployee = certEmployee;

        InitializeComponent();
    }

    private void FormCertEmployee_Load(object sender, EventArgs e)
    {
        textEmployee.Text = _employee.FName + " " + _employee.LName;
        textCertification.Text = _cert.Description;
        textCertCategories.Text = Defs.GetDef(DefCat.CertificationCategories, _cert.CertCategoryNum).ItemName;

        if (_certEmployee.IsNew)
        {
            return;
        }

        textDateCompleted.Text = _certEmployee.DateCompleted.ToShortDateString();
        textNote.Text = _certEmployee.Note;
    }

    private void ButtonToday_Click(object sender, EventArgs e)
    {
        textDateCompleted.Text = DateTime.Today.ToShortDateString();

        ActiveControl = textNote;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_certEmployee.IsNew)
        {
            DialogResult = DialogResult.Cancel;

            return;
        }

        if (!ConfirmOk("Delete Certification Completion?"))
        {
            return;
        }

        CertEmployees.Delete(_certEmployee.CertEmployeeNum);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!DateTime.TryParse(textDateCompleted.Text, out var dateCompleted))
        {
            ShowError("Please enter a valid date.");
            return;
        }

        if (dateCompleted > DateTime.Today)
        {
            ShowError("Date can not be greater than today.");
            return;
        }

        _certEmployee.DateCompleted = dateCompleted;
        _certEmployee.Note = SIn.String(textNote.Text);
        _certEmployee.UserNum = Security.CurUser.UserNum;

        if (_certEmployee.IsNew)
        {
            _certEmployee.CertNum = _cert.CertNum;
            _certEmployee.EmployeeNum = _employee.EmployeeNum;

            CertEmployees.Insert(_certEmployee);
        }
        else
        {
            CertEmployees.Update(_certEmployee);
        }

        DialogResult = DialogResult.OK;
    }
}