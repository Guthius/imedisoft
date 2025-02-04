using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormJournalEntryEdit : FormODBase
{
    private readonly JournalEntry _journalEntry;
    private Account _accountPicked;

    public FormJournalEntryEdit(JournalEntry journalEntry)
    {
        _journalEntry = journalEntry;

        InitializeComponent();
    }

    private void FormJournalEntryEdit_Load(object sender, EventArgs e)
    {
        _accountPicked = Accounts.GetAccount(_journalEntry.AccountNum);

        FillAccount();

        if (_journalEntry.DebitAmt > 0)
        {
            textDebit.Text = _journalEntry.DebitAmt.ToString("n");
        }

        if (_journalEntry.CreditAmt > 0)
        {
            textCredit.Text = _journalEntry.CreditAmt.ToString("n");
        }

        textMemo.Text = _journalEntry.Memo;
        textCheckNumber.Text = _journalEntry.CheckNumber;

        if (_journalEntry.ReconcileNum == 0)
        {
            labelReconcile.Visible = false;
            textReconcile.Visible = false;
            return;
        }

        textReconcile.Text = Reconciles.GetOne(_journalEntry.ReconcileNum).DateReconcile.ToShortDateString();
        textDebit.ReadOnly = true;
        textCredit.ReadOnly = true;

        butDelete.Enabled = false;
        butChange.Enabled = false;
    }

    private void FormJournalEntryEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.Cancel)
        {
            return;
        }

        if (_journalEntry.JournalEntryNum == 0)
        {
            DialogResult = DialogResult.Abort;
        }
    }

    private void FillAccount()
    {
        if (_accountPicked is null)
        {
            textAccount.Text = "";
            butChange.Text = "Pick";
            labelDebit.Text = "Debit";
            labelCredit.Text = "Credit";
            return;
        }

        textAccount.Text = _accountPicked.Description;

        butChange.Text = "Change";

        if (Accounts.DebitIsPos(_accountPicked.AcctType))
        {
            labelDebit.Text = "Debit(+)";
            labelCredit.Text = "Credit(-)";
            return;
        }

        labelDebit.Text = "Debit(-)";
        labelCredit.Text = "Credit(+)";
    }

    private void ButtonChange_Click(object sender, EventArgs e)
    {
        using var formAccountPick = new FormAccountPick();

        if (formAccountPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _accountPicked = formAccountPick.SelectedAccount;

        FillAccount();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_journalEntry.JournalEntryNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            
            return;
        }

        DialogResult = DialogResult.Abort;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!double.TryParse(textDebit.Text, out var debit) ||
            !double.TryParse(textCredit.Text, out var credit))
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        if (debit < 0 || credit < 0)
        {
            ShowError("Both amounts not allowed to be less than 0.");
            return;
        }

        if (debit == 0 && credit == 0)
        {
            ShowError("One amount must be filled in.");
            return;
        }

        if (debit > 0 && credit > 0)
        {
            ShowError("Only one amount can be filled in.");
            return;
        }

        if (_accountPicked is null || _accountPicked.AccountNum == 0)
        {
            ShowError("Please select an account.");
            return;
        }

        _journalEntry.AccountNum = _accountPicked.AccountNum;
        _journalEntry.DebitAmt = debit;
        _journalEntry.CreditAmt = credit;
        _journalEntry.Memo = textMemo.Text;
        _journalEntry.CheckNumber = textCheckNumber.Text;

        DialogResult = DialogResult.OK;
    }
}