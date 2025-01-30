using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAccounting : FormODBase
{
    private DataTable _tableAccounts;

    public FormAccounting()
    {
        InitializeComponent();
    }

    private void FormAccounting_Load(object sender, EventArgs e)
    {
        LayoutMenu();
        LayoutToolBar();

        textDate.Text = DateTime.Today.ToShortDateString();

        FillGrid();
    }

    private void LayoutMenu()
    {
        var menuItemSetup = new MenuItemOD("Setup");
        menuItemSetup.Add("Open Dental", MenuItemOpenDental_Click);
        menuItemSetup.Add("QuickBooks", MenuItemQuickBooks_Click);

        var menuItemReports = new MenuItemOD("Reports");
        menuItemReports.Add("General Ledger Detail", MenuItemGeneralLedger_Click);
        menuItemReports.Add("Balance Sheet", MenuItemBalanceSheet_Click);
        menuItemReports.Add("Profit and Loss", MenuItemProfitLoss_Click);

        menuMain.BeginUpdate();
        menuMain.Add(menuItemSetup);
        menuMain.Add(new MenuItemOD("Lock", MenuItemLock_Click));
        menuMain.Add(menuItemReports);
        menuMain.EndUpdate();
    }

    private static void MenuItemOpenDental_Click(object sender, EventArgs e)
    {
        using var formAccountingSetup = new FormAccountingSetup();

        formAccountingSetup.ShowDialog();
    }

    private static void MenuItemQuickBooks_Click(object sender, EventArgs e)
    {
        using var formQuickBooksSetup = new FormQuickBooksSetup();

        formQuickBooksSetup.ShowDialog();
    }

    public void LayoutToolBar()
    {
        ToolBarMain.Buttons.Clear();
        ToolBarMain.Buttons.Add(new ODToolBarButton("Add", EnumIcons.Add, "", "Add"));
        ToolBarMain.Buttons.Add(new ODToolBarButton("Edit", 1, "Edit Selected Account", "Edit"));
        ToolBarMain.Buttons.Add(new ODToolBarButton("Export .txt", 2, "Export the Chart of Accounts as a tab delimited .txt file", "Export .txt"));
        ToolBarMain.Buttons.Add(new ODToolBarButton("Export .csv", 2, "Export the Chart of Accounts as a comma delimited .csv file", "Export .csv"));
    }

    private static void MenuItemLock_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SecurityAdmin))
        {
            return;
        }

        var frmAccountingLock = new FrmAccountingLock();

        frmAccountingLock.ShowDialog();

        if (frmAccountingLock.IsDialogOK)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, "Accounting Lock Changed");
        }
    }

    private static void MenuItemGeneralLedger_Click(object sender, EventArgs e)
    {
        using var formRpAccountingGenLedg = new FormRpAccountingGenLedg();

        formRpAccountingGenLedg.ShowDialog();
    }

    private static void MenuItemBalanceSheet_Click(object sender, EventArgs e)
    {
        using var formRpAccountingBalanceSheet = new FormRpAccountingBalanceSheet();

        formRpAccountingBalanceSheet.ShowDialog();
    }

    private static void MenuItemProfitLoss_Click(object sender, EventArgs e)
    {
        using var formRpAccountingProfitLoss = new FormRpAccountingProfitLoss();

        formRpAccountingProfitLoss.ShowDialog();
    }

    private void ToolBarMain_ButtonClick(object sender, ODToolBarButtonClickEventArgs e)
    {
        switch (e.Button.Tag.ToString())
        {
            case "Add":
                Add_Click();
                break;

            case "Edit":
                Edit_Click();
                break;

            case "Export .txt":
                Export_Click("\t");
                break;

            case "Export .csv":
                Export_Click(",");
                break;
        }
    }

    private void FillGrid()
    {
        Accounts.RefreshCache();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Type", 70));
        gridMain.Columns.Add(new GridColumn("Description", 170));
        gridMain.Columns.Add(new GridColumn("Balance", 80, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Bank Number", 100));
        gridMain.Columns.Add(new GridColumn("Inactive", 70, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();

        _tableAccounts = Accounts.GetFullList(!textDate.IsValid() ? DateTime.Today : SIn.Date(textDate.Text), checkInactive.Checked);

        for (var i = 0; i < _tableAccounts.Rows.Count; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(_tableAccounts.Rows[i]["type"].ToString());
            gridRow.Cells.Add(_tableAccounts.Rows[i]["Description"].ToString());
            gridRow.Cells.Add(_tableAccounts.Rows[i]["balance"].ToString());
            gridRow.Cells.Add(_tableAccounts.Rows[i]["BankNumber"].ToString());
            gridRow.Cells.Add(_tableAccounts.Rows[i]["inactive"].ToString());
            if (i < _tableAccounts.Rows.Count - 1 && _tableAccounts.Rows[i]["type"].ToString() != _tableAccounts.Rows[i + 1]["type"].ToString())
            {
                gridRow.ColorLborder = Color.Black;
            }

            gridRow.ColorBackG = Color.FromArgb(SIn.Int(_tableAccounts.Rows[i]["color"].ToString()));
            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void Add_Click()
    {
        var frmAccountEdit = new FrmAccountEdit(new Account
        {
            AcctType = AccountType.Asset,
            AccountColor = Color.White
        })
        {
            IsNew = true
        };

        frmAccountEdit.ShowDialog();

        FillGrid();
    }

    private void Edit_Click()
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please pick an account first.");
            return;
        }

        var accountNum = SIn.Long(_tableAccounts.Rows[gridMain.GetSelectedIndex()]["AccountNum"].ToString());
        if (accountNum == 0)
        {
            ShowError("This account is generated automatically, and cannot be edited.");
            return;
        }

        var account = Accounts.GetAccount(accountNum);

        var frmAccountEdit = new FrmAccountEdit(account);

        frmAccountEdit.ShowDialog();

        FillGrid();

        for (var i = 0; i < _tableAccounts.Rows.Count; i++)
        {
            if (_tableAccounts.Rows[i]["AccountNum"].ToString() == accountNum.ToString())
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void Export_Click(string delimiter)
    {
        if (gridMain.ListGridRows.Count == 0)
        {
            ShowInfo("Nothing to export");
            return;
        }

        saveFileDialog = new SaveFileDialog();
        saveFileDialog.AddExtension = true;
        saveFileDialog.FilterIndex = 0;
        saveFileDialog.Filter = delimiter == "\t"
            ? "Text files(*.txt)|*.txt|All files(*.*)|*.*"
            : "CSV files(*.csv)|*.csv|All files(*.*)|*.*";
        saveFileDialog.FileName = gridMain.Title;

        Cursor = Cursors.WaitCursor;

        if (!Directory.Exists(PrefC.GetString(PrefName.ExportPath)))
        {
            try
            {
                Directory.CreateDirectory(PrefC.GetString(PrefName.ExportPath));
                
                saveFileDialog.InitialDirectory = PrefC.GetString(PrefName.ExportPath);
            }
            catch
            {
                // ignored
            }
        }
        else
        {
            saveFileDialog.InitialDirectory = PrefC.GetString(PrefName.ExportPath);
        }

        Cursor = Cursors.Default;

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
            saveFileDialog.Dispose();
            return;
        }

        var filePath = saveFileDialog.FileName;

        saveFileDialog.Dispose();

        StreamWriter streamWriter;
        try
        {
            streamWriter = new StreamWriter(filePath, false);
        }
        catch
        {
            ShowError("File in use by another program.  Close and try again.");
            return;
        }

        var line = "";
        for (var i = 0; i < gridMain.Columns.Count; i++)
        {
            var columnCaption = gridMain.Columns[i].Heading;

            if (columnCaption.StartsWith("-") || columnCaption.StartsWith("="))
            {
                columnCaption = " " + columnCaption;
            }

            line += columnCaption;
            if (i < gridMain.Columns.Count - 1)
            {
                line += delimiter;
            }
        }

        try
        {
            streamWriter.WriteLine(line);
        }
        catch
        {
            ShowError("File in use by another program.  Close and try again.");
            return;
        }

        foreach (var gridRow in gridMain.ListGridRows)
        {
            line = "";

            for (var j = 0; j < gridMain.Columns.Count; j++)
            {
                var cell = gridRow.Cells[j].Text;

                cell = cell.Replace("\r", "");
                cell = cell.Replace("\n", "");
                cell = cell.Replace("\t", "");
                cell = cell.Replace("\"", "");

                if (delimiter == "," && cell.Contains(","))
                {
                    cell = '"' + cell + '"';
                }

                line += cell;
                if (j < gridMain.Columns.Count - 1)
                {
                    line += delimiter;
                }
            }

            try
            {
                streamWriter.WriteLine(line);
            }
            catch
            {
                ShowError("File in use by another program.  Close and try again.");
                return;
            }
        }

        streamWriter.Close();
        streamWriter.Dispose();

        ShowInfo("File created successfully");
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var accountNum = SIn.Long(_tableAccounts.Rows[e.Row]["AccountNum"].ToString());
        if (accountNum == 0)
        {
            ShowInfo("This account is generated automatically, and there is currently no way to view the detail.  " +
                     "It is the sum of all income minus all expenses for all previous years.");

            return;
        }

        var asofDate = !textDate.IsValid() ? DateTime.Today : SIn.Date(textDate.Text);

        var account = Accounts.GetAccount(accountNum);

        using var formJournal = new FormJournal(account);

        formJournal.DateInitialAsOf = asofDate;
        formJournal.ShowDialog();

        FillGrid();

        for (var i = 0; i < _tableAccounts.Rows.Count; i++)
        {
            if (_tableAccounts.Rows[i]["AccountNum"].ToString() == accountNum.ToString())
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void CheckBoxInactive_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonRefresh_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonToday_Click(object sender, EventArgs e)
    {
        textDate.Text = DateTime.Today.ToShortDateString();

        FillGrid();
    }
}