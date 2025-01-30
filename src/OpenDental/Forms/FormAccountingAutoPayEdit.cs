using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAccountingAutoPayEdit : FormODBase
{
    private readonly AccountingAutoPay _accountingAutoPay;
    private List<long> _accountNums;
    private List<Def> _paymentTypeDefs;
    
    public FormAccountingAutoPayEdit(AccountingAutoPay accountingAutoPay)
    {
        _accountingAutoPay = accountingAutoPay;

        InitializeComponent();
    }

    private void FormAccountingAutoPayEdit_Load(object sender, EventArgs e)
    {
        if (_accountingAutoPay is null)
        {
            ShowError("Autopay cannot be null.");
            return;
        }

        _paymentTypeDefs = Defs.GetDefsForCategory(DefCat.PaymentTypes, true);
        for (var i = 0; i < _paymentTypeDefs.Count; i++)
        {
            comboPayType.Items.Add(_paymentTypeDefs[i].ItemName);

            if (_paymentTypeDefs[i].DefNum == _accountingAutoPay.PayType)
            {
                comboPayType.SelectedIndex = i;
            }
        }

        _accountingAutoPay.PickList ??= "";

        var accountNums = _accountingAutoPay.PickList.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

        _accountNums = [];
        foreach (var str in accountNums)
        {
            _accountNums.Add(SIn.Long(str));
        }

        FillList();
    }

    private void FillList()
    {
        listAccounts.Items.Clear();

        foreach (var accountNum in _accountNums)
        {
            listAccounts.Items.Add(Accounts.GetDescript(accountNum));
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        using var formAccountPick = new FormAccountPick();

        if (formAccountPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _accountNums.Add(formAccountPick.SelectedAccount.AccountNum);

        FillList();
    }

    private void ButtonRemove_Click(object sender, EventArgs e)
    {
        if (listAccounts.SelectedIndex == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        _accountNums.RemoveAt(listAccounts.SelectedIndex);

        FillList();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_accountingAutoPay.AccountingAutoPayNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        DialogResult = DialogResult.Abort;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (comboPayType.SelectedIndex == -1)
        {
            ShowError("Please select a pay type first.");
            return;
        }

        if (_accountNums.Count == 0)
        {
            ShowError("Please add at least one account to the pick list first.");
            return;
        }

        _accountingAutoPay.PayType = _paymentTypeDefs[comboPayType.SelectedIndex].DefNum;
        _accountingAutoPay.PickList = "";

        for (var i = 0; i < _accountNums.Count; i++)
        {
            if (i > 0)
            {
                _accountingAutoPay.PickList += ",";
            }

            _accountingAutoPay.PickList += _accountNums[i].ToString();
        }

        DialogResult = DialogResult.OK;
    }
}