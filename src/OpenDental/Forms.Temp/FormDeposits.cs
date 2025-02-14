using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;

namespace OpenDental;

public partial class FormDeposits : FormODBase
{
    private List<Deposit> _deposits;

    public bool IsSelectionMode { get; set; }
    public Deposit SelectedDeposit { get; set; }

    public FormDeposits()
    {
        InitializeComponent();
    }

    private void FormDeposits_Load(object sender, EventArgs e)
    {
        if (IsSelectionMode)
        {
            butAdd.Visible = false;
        }
        else
        {
            butOK.Visible = false;
        }

        FillGrid();
    }

    private void FillGrid()
    {
        _deposits = Deposits.GetForClinics(comboClinics.ListClinicNumsSelected.Count == 0 ? [Clinics.ClinicNum] : comboClinics.ListClinicNumsSelected, IsSelectionMode);

        grid.BeginUpdate();

        grid.Columns.Clear();
        grid.Columns.Add(new GridColumn("Date", 80));
        grid.Columns.Add(new GridColumn("Amount", 90, HorizontalAlignment.Right));
        grid.Columns.Add(new GridColumn("Clinic", 150));

        grid.ListGridRows.Clear();

        foreach (var deposit in _deposits)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(deposit.DateDeposit.ToShortDateString());
            gridRow.Cells.Add(deposit.Amount.ToString("F"));
            gridRow.Cells.Add(" " + deposit.ClinicAbbr);

            grid.ListGridRows.Add(gridRow);
        }

        grid.EndUpdate();
        grid.ScrollToEnd();
    }

    private void ComboBoxClinics_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void Grid_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (IsSelectionMode)
        {
            SelectedDeposit = _deposits[e.Row];
            DialogResult = DialogResult.OK;
            return;
        }

        using var formDepositEdit = new FormDepositEdit(_deposits[e.Row]);

        if (formDepositEdit.ShowDialog() == DialogResult.Cancel)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var deposit = new Deposit
        {
            DateDeposit = DateTime.Today
        };

        var clinic = Clinics.GetClinic(Clinics.ClinicNum);

        deposit.BankAccountInfo = clinic.BankNumber;

        using var formDepositEdit = new FormDepositEdit(deposit);

        formDepositEdit.IsNew = true;

        if (formDepositEdit.ShowDialog() == DialogResult.Cancel)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (grid.GetSelectedIndex() == -1)
        {
            ShowError("Please select a deposit first.");
            return;
        }

        SelectedDeposit = _deposits[grid.GetSelectedIndex()];

        DialogResult = DialogResult.OK;
    }
}