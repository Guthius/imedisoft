using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;
using GridRow = OpenDental.UI.GridRow;

namespace OpenDental.Forms;

public partial class FormAccountingSetup : FormODBase
{
    private List<long> _depositAccountNums;
    private long _selectedDepositAccountNum;
    private long _selectedPayAccountNum;
    private List<AccountingAutoPay> _accountingAutoPays;

    public FormAccountingSetup()
    {
        InitializeComponent();
    }

    private void FormAccountingSetup_Load(object sender, EventArgs e)
    {
        var accountingDepositAccounts = PrefC.GetString(PrefName.AccountingDepositAccounts);
        var accountingDepositAccountNums = accountingDepositAccounts.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

        _depositAccountNums = [];

        foreach (var str in accountingDepositAccountNums)
        {
            _depositAccountNums.Add(SIn.Long(str));
        }

        FillDepList();

        _selectedDepositAccountNum = PrefC.GetLong(PrefName.AccountingIncomeAccount);

        textAccountInc.Text = Accounts.GetDescript(_selectedDepositAccountNum);

        _accountingAutoPays = AccountingAutoPays.GetDeepCopy();

        FillPayGrid();

        _selectedPayAccountNum = PrefC.GetLong(PrefName.AccountingCashIncomeAccount);

        textAccountCashInc.Text = Accounts.GetDescript(_selectedPayAccountNum);
    }

    private void FillDepList()
    {
        listAccountsDep.Items.Clear();

        foreach (var accountNum in _depositAccountNums)
        {
            listAccountsDep.Items.Add(Accounts.GetDescript(accountNum));
        }
    }

    private void FillPayGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Payment Type", 200));
        gridMain.Columns.Add(new GridColumn("Pick List", 250));

        gridMain.ListGridRows.Clear();

        foreach (var accountingAutoPay in _accountingAutoPays)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(Defs.GetName(DefCat.PaymentTypes, accountingAutoPay.PayType));
            gridRow.Cells.Add(AccountingAutoPays.GetPickListDescription(accountingAutoPay));

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        using var formAccountPick = new FormAccountPick();

        if (formAccountPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _depositAccountNums.Add(formAccountPick.SelectedAccount.AccountNum);

        FillDepList();
    }

    private void ButtonRemove_Click(object sender, EventArgs e)
    {
        if (listAccountsDep.SelectedIndex == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        _depositAccountNums.RemoveAt(listAccountsDep.SelectedIndex);

        FillDepList();
    }

    private void ButtonChange_Click(object sender, EventArgs e)
    {
        using var formAccountPick = new FormAccountPick();

        if (formAccountPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _selectedDepositAccountNum = formAccountPick.SelectedAccount.AccountNum;

        textAccountInc.Text = Accounts.GetDescript(_selectedDepositAccountNum);
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var accountingAutoPay = _accountingAutoPays[e.Row];

        using var formAccountingAutoPayEdit = new FormAccountingAutoPayEdit(accountingAutoPay);

        if (formAccountingAutoPayEdit.ShowDialog() == DialogResult.Abort)
        {
            _accountingAutoPays.Remove(accountingAutoPay);
        }

        FillPayGrid();
    }

    private void ButtonAddPay_Click(object sender, EventArgs e)
    {
        var accountingAutoPay = new AccountingAutoPay();

        using var formAccountingAutoPayEdit = new FormAccountingAutoPayEdit(accountingAutoPay);

        if (formAccountingAutoPayEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _accountingAutoPays.Add(accountingAutoPay);

        FillPayGrid();
    }

    private void ButtonChangeCash_Click(object sender, EventArgs e)
    {
        using var formAccountPick = new FormAccountPick();

        if (formAccountPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _selectedPayAccountNum = formAccountPick.SelectedAccount.AccountNum;

        textAccountCashInc.Text = Accounts.GetDescript(_selectedPayAccountNum);
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var accountingDepositAccounts = "";
        for (var i = 0; i < _depositAccountNums.Count; i++)
        {
            if (i > 0)
            {
                accountingDepositAccounts += ",";
            }

            accountingDepositAccounts += _depositAccountNums[i].ToString();
        }

        if (Prefs.UpdateString(PrefName.AccountingDepositAccounts, accountingDepositAccounts))
        {
            DataValid.SetInvalid(InvalidType.Prefs);
        }

        if (Prefs.UpdateLong(PrefName.AccountingIncomeAccount, _selectedDepositAccountNum))
        {
            DataValid.SetInvalid(InvalidType.Prefs);
        }

        AccountingAutoPays.SaveList(_accountingAutoPays);

        DataValid.SetInvalid(InvalidType.AccountingAutoPays);
        if (Prefs.UpdateLong(PrefName.AccountingCashIncomeAccount, _selectedPayAccountNum))
        {
            DataValid.SetInvalid(InvalidType.Prefs);
        }

        DialogResult = DialogResult.OK;
    }
}