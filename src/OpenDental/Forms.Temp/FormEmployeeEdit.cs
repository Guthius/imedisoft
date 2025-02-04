using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormEmployeeEdit : FormODBase
{
    public bool IsNew;
    public Employee EmployeeCur;

    public FormEmployeeEdit()
    {
        InitializeComponent();
    }

    private void FormEmployeeEdit_Load(object sender, EventArgs e)
    {
        checkIsHidden.Checked = EmployeeCur.IsHidden;
        textLName.Text = EmployeeCur.LName;
        textFName.Text = EmployeeCur.FName;
        textMI.Text = EmployeeCur.MiddleI;
        textPayrollID.Text = EmployeeCur.PayrollID;
        textPhoneExt.Text = EmployeeCur.PhoneExt.ToString();
        textWirelessPhone.Text = EmployeeCur.WirelessPhone;
        textEmailWork.Text = EmployeeCur.EmailWork;
        textEmailPersonal.Text = EmployeeCur.EmailPersonal;
        checkIsFurloughed.Checked = EmployeeCur.IsFurloughed;
        checkIsWorkingHome.Checked = EmployeeCur.IsWorkingHome;

        var employees = Employees.GetDeepCopy(shortList: true);

        comboReportsTo.Items.AddNone<Employee>();
        comboReportsTo.Items.AddList(employees, x => x.FName + " " + x.LName);

        comboReportsTo.SetSelectedKey<Employee>(EmployeeCur.ReportsTo, x => x.EmployeeNum);
    }

    private void butDelete_Click(object sender, EventArgs e)
    {
        if (IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        try
        {
            Employees.Delete(EmployeeCur.EmployeeNum);
        }
        catch (ApplicationException ex)
        {
            ShowError(ex.Message);

            return;
        }

        DialogResult = DialogResult.OK;
    }

    private void butSave_Click(object sender, EventArgs e)
    {
        var employeeOld = EmployeeCur.Copy();

        EmployeeCur.IsHidden = checkIsHidden.Checked;
        EmployeeCur.LName = textLName.Text.Trim();
        EmployeeCur.FName = textFName.Text.Trim();
        EmployeeCur.MiddleI = textMI.Text;
        EmployeeCur.PayrollID = textPayrollID.Text;

        try
        {
            EmployeeCur.PhoneExt = SIn.Int(textPhoneExt.Text);
        }
        catch
        {
            EmployeeCur.PhoneExt = 0;
        }

        EmployeeCur.WirelessPhone = textWirelessPhone.Text;
        EmployeeCur.EmailWork = textEmailWork.Text;
        EmployeeCur.EmailPersonal = textEmailPersonal.Text;
        EmployeeCur.IsFurloughed = checkIsFurloughed.Checked;
        EmployeeCur.IsWorkingHome = checkIsWorkingHome.Checked;
        EmployeeCur.ReportsTo = comboReportsTo.GetSelectedKey<Employee>(x => x.EmployeeNum);

        if (IsNew)
        {
            try
            {
                Employees.Insert(EmployeeCur);
            }
            catch (ApplicationException ex)
            {
                ShowError(ex.Message);

                return;
            }

            DialogResult = DialogResult.OK;
            return;
        }

        try
        {
            Employees.UpdateChanged(EmployeeCur, employeeOld);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);

            return;
        }

        DialogResult = DialogResult.OK;
    }
}