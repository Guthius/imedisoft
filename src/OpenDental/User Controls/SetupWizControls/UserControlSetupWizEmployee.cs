using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizEmployee : SetupWizControl
{
    private int _blink;
    private bool _changed;
    private List<Employee> _employees;

    public UserControlSetupWizEmployee()
    {
        InitializeComponent();
            
        OnControlDone += ControlDone;
    }

    private void UserControlSetupWizEmployee_Load(object sender, EventArgs e)
    {
        FillGrid();
            
        if (_employees.Where(x => x.FName.ToLower() != "default").ToList().Count != 0)
        {
            return;
        }
            
        MsgBox.Show(this, "You have no valid employees. Please click the 'Add' button to add an employee.");
                
        timerBlink.Start();
    }

    private void FillGrid()
    {
        _employees = Employees.GetDeepCopy(true);
            
        var colorNeedsAttn = OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
            
        gridMain.BeginUpdate();
            
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Last Name", 135));
        gridMain.Columns.Add(new GridColumn("First Name", 135));
        gridMain.Columns.Add(new GridColumn("MI", 65));
        gridMain.Columns.Add(new GridColumn("Payroll ID", 105));
            
        gridMain.ListGridRows.Clear();
            
        var complete = _employees.Where(x => x.FName.ToLower() != "default").ToList().Count != 0;

        foreach (var emp in _employees)
        {
            var gridRow = new GridRow();
                
            gridRow.Cells.Add(emp.LName);
                
            if (string.IsNullOrEmpty(emp.LName) || emp.LName.ToLower() == "default")
            {
                gridRow.Cells[gridRow.Cells.Count - 1].ColorBackG = colorNeedsAttn;
                complete = false;
            }

            gridRow.Cells.Add(emp.FName);
            if (string.IsNullOrEmpty(emp.FName) || emp.FName.ToLower() == "default")
            {
                gridRow.Cells[gridRow.Cells.Count - 1].ColorBackG = colorNeedsAttn;
                complete = false;
            }

            gridRow.Cells.Add(emp.MiddleI);
            gridRow.Cells.Add(emp.PayrollID);
            gridRow.Tag = emp;
                
            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
            
        IsDone = complete;
    }

    private void timerBlink_Tick(object sender, EventArgs e)
    {
        if (_blink > 5)
        {
            pictureAdd.Visible = true;
                
            foreach (var rowCur in gridMain.ListGridRows)
            {
                rowCur.ColorBackG = OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
            }

            gridMain.Invalidate();
            timerBlink.Stop();
            return;
        }

        pictureAdd.Visible = !pictureAdd.Visible;
            
        foreach (var rowCur in gridMain.ListGridRows)
        {
            rowCur.ColorBackG = rowCur.ColorBackG == Color.White ? OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention) : Color.White;
        }

        gridMain.Invalidate();
            
        _blink++;
    }

    private void gridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var selectedEmployee = (Employee) gridMain.ListGridRows[e.Row].Tag;
        using var formEmployeeEdit = new FormEmployeeEdit();
            
        formEmployeeEdit.EmployeeCur = selectedEmployee;

        if (formEmployeeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }
            
        Employees.RefreshCache();
            
        FillGrid();
            
        _changed = true;
    }

    private void butAdd_Click(object sender, EventArgs e)
    {
        using var formEmployeeEdit = new FormEmployeeEdit();
            
        formEmployeeEdit.IsNew = true;
        formEmployeeEdit.EmployeeCur = new Employee();

        if (formEmployeeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }
            
        Employees.RefreshCache();
            
        FillGrid();
            
        _changed = true;
    }

    private void butAdvanced_Click(object sender, EventArgs e)
    {
        using var formEmployeeSelect = new FormEmployeeSelect();
            
        formEmployeeSelect.ShowDialog();
            
        FillGrid();
    }

    private void ControlDone(object sender, EventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Employees);
        }
    }
}