using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCertifications : FormODBase
{
    private bool _isHeadingPrinted;
    private List<Cert> _certs;
    private List<Def> _certificationCategoryDefs;
    private List<Employee> _employees;
    private int _pageNumber;

    public FormCertifications()
    {
        InitializeComponent();
    }

    private void FormCertifications_Load(object sender, EventArgs e)
    {
        _employees = Employees.GetDeepCopy();

        var employee = _employees.Find(x => x.FName == " Escalate As Needed");

        _employees.Remove(employee);

        var employees = _employees.FindAll(x => x.IsHidden == false);

        listBoxEmployee.Items.AddList(employees, x => x.FName + " " + x.LName);

        comboSupervisor.Items.Add("Any", new Employee());

        var employeeSupers = new List<Employee>();
        foreach (var emp in _employees)
        {
            if (emp.ReportsTo == 0)
            {
                continue;
            }

            if (employeeSupers.Any(x => x.EmployeeNum == emp.ReportsTo))
            {
                continue;
            }

            var supervisor = Employees.GetEmp(emp.ReportsTo);
            if (supervisor != null)
            {
                employeeSupers.Add(supervisor);
            }
        }

        employeeSupers = employeeSupers.OrderBy(x => x.FName).ToList();

        comboSupervisor.Items.AddList(employeeSupers, x => x.FName);
        comboSupervisor.SetSelected(0);

        _certificationCategoryDefs = Defs.GetDefsForCategory(DefCat.CertificationCategories, true);

        listBoxCategories.Items.Add("All");
        listBoxCategories.Items.AddList(_certificationCategoryDefs, x => x.ItemName);
        listBoxCategories.SetSelected(0);

        listBoxCategories2.Items.Add("All");
        listBoxCategories2.Items.AddList(_certificationCategoryDefs, x => x.ItemName);
        listBoxCategories2.SetSelected(0);

        _certs = Certs.GetAll(true);

        var certs = GetCertsForCategories();

        listBoxCertification.Items.AddList(certs, x => x.Description);

        labelCertification.Visible = false;
        labelCertification.Location = new Point(labelCategories.Location.X, labelCategories.Location.Y);

        listBoxCertification.Visible = false;
        listBoxCertification.Location = new Point(listBoxCategories.Location.X, listBoxCategories.Location.Y);

        labelCategories2.Visible = false;
        labelCategories2.Location = new Point(labelEmployee.Location.X, labelEmployee.Location.Y);

        listBoxCategories2.Visible = false;
        listBoxCategories2.Location = new Point(listBoxEmployee.Location.X, listBoxEmployee.Location.Y);

        checkSortDateCertComplete.Visible = false;
        checkSortDateCertComplete.Location = new Point(checkIncomplete.Location.X, checkIncomplete.Location.Y);

        FillGrid();
    }

    private void FillGrid()
    {
        if (radioCategory.Checked)
        {
            if (checkSortDate.Checked && !checkIncomplete.Checked)
            {
                FillGridByCategoryCertCompletionDate();
            }
            else
            {
                FillGridByCategoryItemOrder();
            }
        }
        else
        {
            FillGridByCertComplete();
        }
    }

    private void FillGridByCategoryCertCompletionDate()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Category", 80));
        gridMain.Columns.Add(new GridColumn("Certification", 175));
        gridMain.Columns.Add(new GridColumn("Wiki Page", 175));
        gridMain.Columns.Add(new GridColumn("Date", 65));
        gridMain.Columns.Add(new GridColumn("Note", 207));

        gridMain.ListGridRows.Clear();

        var lastCategoryName = "";
        var certEmployees = new List<CertEmployee>();

        if (listBoxEmployee.SelectedIndex > -1)
        {
            certEmployees = CertEmployees.GetAllForEmployee(listBoxEmployee.GetSelected<Employee>().EmployeeNum).OrderBy(x => x.DateCompleted).ToList();
        }

        var certsByDateCompleted = _certs
            .FindAll(x => !x.IsHidden && _certificationCategoryDefs.Any(y => y.DefNum == x.CertCategoryNum))
            .OrderByDescending(SortCert)
            .ThenBy(x => _certificationCategoryDefs.Find(y => y.DefNum == x.CertCategoryNum).ItemOrder)
            .ThenBy(x => x.ItemOrder).ToList();

        foreach (var cert in certsByDateCompleted)
        {
            var certEmployee = certEmployees.Find(x => x.CertNum == cert.CertNum);
            var def = _certificationCategoryDefs.Find(x => x.DefNum == cert.CertCategoryNum);

            if (!listBoxCategories.SelectedIndices.Contains(0))
            {
                if (!listBoxCategories.GetListSelected<Def>().Contains(def))
                {
                    continue;
                }
            }

            var gridRow = new GridRow();

            var categoryName = _certificationCategoryDefs.FirstOrDefault(x => x.DefNum == cert.CertCategoryNum)?.ItemName;
            if (lastCategoryName == categoryName)
            {
                gridRow.Cells.Add("");
            }
            else
            {
                gridRow.Cells.Add(categoryName);

                lastCategoryName = categoryName;
            }

            gridRow.Cells.Add(cert.Description);
            gridRow.Cells.Add(cert.WikiPageLink);

            if (certEmployee is null)
            {
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
            }
            else
            {
                gridRow.Cells.Add(certEmployee.DateCompleted.ToShortDateString());
                gridRow.Cells.Add(certEmployee.Note);
            }

            gridRow.Tag = cert;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();

        return;

        DateTime SortCert(Cert x)
        {
            return certEmployees.Find(y => y.CertNum == x.CertNum) == null ? DateTime.MinValue : certEmployees.Find(y => y.CertNum == x.CertNum).DateCompleted;
        }
    }

    private void FillGridByCategoryItemOrder()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Category", 80));
        gridMain.Columns.Add(new GridColumn("Certification", 175));
        gridMain.Columns.Add(new GridColumn("Wiki Page", 175));
        gridMain.Columns.Add(new GridColumn("Date", 65));
        gridMain.Columns.Add(new GridColumn("Note", 207));

        gridMain.ListGridRows.Clear();

        var lastCategoryName = "";

        var certEmployees = new List<CertEmployee>();
        if (listBoxEmployee.SelectedIndex > -1)
        {
            certEmployees = CertEmployees
                .GetAllForEmployee(listBoxEmployee.GetSelected<Employee>().EmployeeNum)
                .OrderBy(x => x.DateCompleted)
                .ToList();
        }

        foreach (var def in _certificationCategoryDefs)
        {
            var certsForCategory = _certs
                .FindAll(x => x.CertCategoryNum == def.DefNum && !x.IsHidden)
                .OrderBy(x => x.ItemOrder)
                .ToList();

            foreach (var cert in certsForCategory)
            {
                var certEmployeeCur = certEmployees.Find(x => x.CertNum == cert.CertNum);
                if (checkIncomplete.Checked && certEmployeeCur != null)
                {
                    continue;
                }

                if (!listBoxCategories.SelectedIndices.Contains(0))
                {
                    if (!listBoxCategories.GetListSelected<Def>().Contains(def))
                    {
                        continue;
                    }
                }

                var gridRow = new GridRow();

                var categoryName = def.ItemName;
                if (lastCategoryName == categoryName)
                {
                    gridRow.Cells.Add("");
                }
                else
                {
                    gridRow.Cells.Add(categoryName);

                    lastCategoryName = categoryName;
                }

                gridRow.Cells.Add(cert.Description);
                gridRow.Cells.Add(cert.WikiPageLink);
                if (certEmployeeCur == null)
                {
                    gridRow.Cells.Add("");
                    gridRow.Cells.Add("");
                }
                else
                {
                    gridRow.Cells.Add(certEmployeeCur.DateCompleted.ToShortDateString());
                    gridRow.Cells.Add(certEmployeeCur.Note);
                }

                gridRow.Tag = cert;

                gridMain.ListGridRows.Add(gridRow);
            }
        }

        gridMain.EndUpdate();
    }

    private void FillGridByCertComplete()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn(Lan.g("FormCertifications", "Certification"), 175));
        gridMain.Columns.Add(new GridColumn(Lan.g("FormCertifications", "Wiki Page"), 175));
        gridMain.Columns.Add(new GridColumn(Lan.g("FormCertifications", "Employee"), 120));
        gridMain.Columns.Add(new GridColumn(Lan.g("FormCertifications", "Date"), 65));
        gridMain.Columns.Add(new GridColumn(Lan.g("FormCertifications", "Note"), 167));

        gridMain.ListGridRows.Clear();

        var lastCertName = "";
        var certSelected = listBoxCertification.GetSelected<Cert>();
        if (certSelected == null)
        {
        }
        else
        {
            var certEmployees = CertEmployees.GetAllForCert(certSelected.CertNum);

            certEmployees = checkSortDateCertComplete.Checked
                ? certEmployees
                    .OrderByDescending(x => x.DateCompleted).ToList()
                : certEmployees
                    .OrderBy(x => _employees.Find(y => y.EmployeeNum == x.EmployeeNum).FName)
                    .ToList();

            foreach (var certEmployee in certEmployees)
            {
                var employee = _employees.Find(x => x.EmployeeNum == certEmployee.EmployeeNum);

                var gridRow = new GridRow();

                if (certSelected.IsHidden)
                {
                    continue;
                }

                if (employee.IsHidden)
                {
                    continue;
                }

                if (lastCertName == certSelected.Description)
                {
                    gridRow.Cells.Add("");
                    gridRow.Cells.Add("");
                }
                else
                {
                    gridRow.Cells.Add(certSelected.Description);
                    gridRow.Cells.Add(certSelected.WikiPageLink);

                    lastCertName = certSelected.Description;
                }

                gridRow.Cells.Add(employee.FName + " " + employee.LName);
                gridRow.Cells.Add(certEmployee.DateCompleted.ToShortDateString());
                gridRow.Cells.Add(certEmployee.Note);

                gridMain.ListGridRows.Add(gridRow);
            }
        }

        gridMain.EndUpdate();
    }

    private List<Cert> GetCertsForCategories()
    {
        var selectedDefs = listBoxCategories2.SelectedIndices.Contains(0) ? [.._certificationCategoryDefs] : listBoxCategories2.GetListSelected<Def>();

        return _certs
            .FindAll(x => !x.IsHidden && selectedDefs.Any(y => y.DefNum == x.CertCategoryNum))
            .OrderBy(x => selectedDefs.Find(y => y.DefNum == x.CertCategoryNum).ItemOrder)
            .ThenBy(x => x.ItemOrder)
            .ToList();
    }

    private void ComboBoxSupervisor_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (comboSupervisor.SelectedIndex == 0)
        {
            listBoxEmployee.Items.Clear();

            var employees = _employees.FindAll(x => x.IsHidden == false);

            listBoxEmployee.Items.AddList(employees, x => x.FName + " " + x.LName);

            FillGrid();

            return;
        }

        var selectedEmployee = comboSupervisor.GetSelected<Employee>();
        if (selectedEmployee is null)
        {
            return;
        }

        listBoxEmployee.Items.Clear();

        foreach (var employee in _employees)
        {
            if (employee.ReportsTo != selectedEmployee.EmployeeNum)
            {
                continue;
            }

            if (employee.IsHidden)
            {
                continue;
            }

            listBoxEmployee.Items.Add(employee.FName + " " + employee.LName, employee);
        }
    }

    private void ButtonSetup_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.CertificationSetup))
        {
            return;
        }

        using var formCertificationSetup = new FormCertificationSetup();

        formCertificationSetup.ShowDialog();

        _certs = Certs.GetAll(true);

        ListBoxCategories2_SelectionChangeCommitted(this, e);

        FillGrid();
    }

    private void TextBoxEmployeeSearch_KeyUp(object sender, KeyEventArgs e)
    {
        comboSupervisor.SetSelected(0);

        var empNameSearch = SIn.String(textEmpSearch.Text).ToLower();

        var filteredEmployees = _employees
            .Where(x => x.FName.ToLower().StartsWith(empNameSearch))
            .Where(x => x.IsHidden == false)
            .ToList();

        listBoxEmployee.Items.Clear();
        listBoxEmployee.Items.AddList(filteredEmployees, x => x.FName + " " + x.LName);

        if (listBoxEmployee.Items.Count != 1)
        {
            return;
        }

        listBoxEmployee.SelectedIndex = 0;
        ListBoxEmployee_SelectionChangeCommitted(this, e);
    }

    private void RadioButtonCategory_Click(object sender, EventArgs e)
    {
        labelEmpSearch.Visible = true;
        textEmpSearch.Visible = true;
        labelCertification.Visible = false;
        listBoxCertification.Visible = false;
        labelCategories2.Visible = false;
        listBoxCategories2.Visible = false;
        labelCategories.Visible = true;
        listBoxCategories.Visible = true;
        labelEmployee.Visible = true;
        listBoxEmployee.Visible = true;
        labelReportsTo.Visible = true;
        comboSupervisor.Visible = true;
        checkIncomplete.Visible = true;
        checkSortDate.Visible = true;
        checkSortDateCertComplete.Visible = false;

        FillGrid();
    }

    private void RadioButtonCertification_Click(object sender, EventArgs e)
    {
        labelEmpSearch.Visible = false;
        textEmpSearch.Visible = false;
        labelCertification.Visible = true;
        listBoxCertification.Visible = true;
        labelCategories2.Visible = true;
        listBoxCategories2.Visible = true;
        labelCategories.Visible = false;
        listBoxCategories.Visible = false;
        labelEmployee.Visible = false;
        listBoxEmployee.Visible = false;
        labelReportsTo.Visible = false;
        comboSupervisor.Visible = false;
        checkIncomplete.Visible = false;
        checkSortDate.Visible = false;
        checkSortDateCertComplete.Visible = true;

        FillGrid();
    }

    private void ListBoxEmployee_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ListBoxCategories_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ListBoxCategories2_SelectionChangeCommitted(object sender, EventArgs e)
    {
        var filteredCerts = GetCertsForCategories();

        listBoxCertification.Items.Clear();
        listBoxCertification.Items.AddList(filteredCerts, x => x.Description);

        if (listBoxCertification.Items.Count > 0)
        {
            listBoxCertification.SelectedIndex = 0;
        }

        FillGrid();
    }

    private void ListBoxCertification_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void CheckBoxIncomplete_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void CheckBoxSortDate_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void CheckBoxSortDateCertComplete_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (radioCertification.Checked)
        {
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.CertificationEmployee))
        {
            return;
        }

        if (listBoxEmployee.SelectedIndex == -1)
        {
            ShowError("Please select an Employee first.");
            return;
        }

        var employee = listBoxEmployee.GetSelected<Employee>();

        var cert = Certs.GetOne(((Cert) gridMain.ListGridRows[e.Row].Tag).CertNum);
        var certEmployee = CertEmployees.GetOne(cert.CertNum, employee.EmployeeNum);

        certEmployee ??= new CertEmployee
        {
            IsNew = true
        };

        using var formCertEmployee = new FormCertEmployee(employee, cert, certEmployee);

        if (formCertEmployee.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonPrint_Click(object sender, EventArgs e)
    {
        _pageNumber = 0;
        _isHeadingPrinted = false;

        PrinterL.TryPrintOrDebugRpPreview(PrintPage, "Certifications printed");
    }

    private void PrintPage(object sender, PrintPageEventArgs e)
    {
        var fontHeading = new Font("Arial", 13, FontStyle.Bold);

        var y = e.MarginBounds.Top;
        var cx = e.MarginBounds.X + e.MarginBounds.Width / 2;

        if (!_isHeadingPrinted)
        {
            const string header = "Certifications Completed";

            e.Graphics.DrawString(header, fontHeading, Brushes.Black, cx - e.Graphics.MeasureString(header, fontHeading).Width / 2, y);
            y += 25;

            _isHeadingPrinted = true;
        }

        y = gridMain.PrintPage(e.Graphics, _pageNumber, e.MarginBounds, y);

        _pageNumber++;

        e.HasMorePages = y == -1;
    }
}