using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.Bridges;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAccountPick : FormODBase
{
    public Account SelectedAccount { get; set; }
    public bool IsQuickBooks { get; set; }
    public List<string> SelectedQuickBooksAccounts { get; set; }

    public FormAccountPick()
    {
        InitializeComponent();
    }

    private void FormAccountPick_Load(object sender, EventArgs e)
    {
        if (IsQuickBooks)
        {
            SelectedQuickBooksAccounts = [];

            checkInactive.Visible = false;

            FillGridQuickBooks();

            gridMain.SelectionMode = GridSelectionMode.MultiExtended;
        }
        else
        {
            FillGrid();
        }
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Type", 70));
        gridMain.Columns.Add(new GridColumn("Description", 170));
        gridMain.Columns.Add(new GridColumn("Balance", 65, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Bank Number", 100));
        gridMain.Columns.Add(new GridColumn("Inactive", 70));

        gridMain.ListGridRows.Clear();

        var accounts = Accounts.GetDeepCopy();
        if (!checkInactive.Checked)
        {
            accounts = accounts.FindAll(x => !x.Inactive);
        }

        for (var i = 0; i < accounts.Count; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(accounts[i].AcctType.ToString());
            gridRow.Cells.Add(accounts[i].Description);
            gridRow.Cells.Add(accounts[i].AcctType == AccountType.Asset ? Accounts.GetBalance(accounts[i].AccountNum, accounts[i].AcctType).ToString("n") : "");
            gridRow.Cells.Add(accounts[i].BankNumber);
            gridRow.Cells.Add(accounts[i].Inactive ? "X" : "");

            if (i < accounts.Count - 1 && accounts[i].AcctType != accounts[i + 1].AcctType)
            {
                gridRow.ColorLborder = Color.Black;
            }

            gridRow.Tag = accounts[i].Clone();
            gridRow.ColorBackG = accounts[i].AccountColor;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void FillGridQuickBooks()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 200));

        gridMain.ListGridRows.Clear();

        Cursor.Current = Cursors.WaitCursor;

        var accounts = new List<string>();
        try
        {
            accounts = QuickBooks.GetListOfAccounts();
        }
        catch (Exception e)
        {
            ODMessageBox.Show(e.Message);
        }

        Cursor.Current = Cursors.Default;

        foreach (var account in accounts)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(account);
            gridRow.Tag = account;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (IsQuickBooks)
        {
            SelectedQuickBooksAccounts.Add((string) gridMain.ListGridRows[e.Row].Tag);
        }
        else
        {
            SelectedAccount = ((Account) gridMain.ListGridRows[e.Row].Tag).Clone();
        }

        DialogResult = DialogResult.OK;
    }

    private void CheckBoxInactive_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an account first.");
            return;
        }

        if (IsQuickBooks)
        {
            foreach (var index in gridMain.SelectedIndices)
            {
                SelectedQuickBooksAccounts.Add((string) gridMain.ListGridRows[index].Tag);
            }
        }
        else
        {
            SelectedAccount = ((Account) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag).Clone();
        }

        DialogResult = DialogResult.OK;
    }
}