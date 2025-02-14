using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormTransactionEdit : FormODBase
{
    private Transaction _transaction;
    private List<JournalEntry> _journalEntries;
    private List<JournalEntry> _listJournalEntriesOld;

    ///<summary>The account where the edit originated from.  This affects how the simple version gets displayed, and the signs on debit and credit.</summary>
    private Account _accountOfOrigin;

    ///<summary>When in simple mode, this is the 'other' account, not the one of origin.  It can be null.</summary>
    private Account _accountPicked;

    ///<summary>Just used for security.</summary>
    public bool IsNew;


    public FormTransactionEdit(Transaction transaction, long accountNum)
    {
        InitializeComponent();

        _transaction = transaction;
        _accountOfOrigin = Accounts.GetAccount(accountNum);
    }

    private void FormTransactionEdit_Load(object sender, EventArgs e)
    {
        _journalEntries = JournalEntries.GetForTrans(_transaction.TransactionNum); //Count might be 0
        if (IsNew)
        {
            if (!Security.IsAuthorized(EnumPermType.AccountingCreate, DateTime.Today))
            {
                //we will check the date again when saving
                DialogResult = DialogResult.Cancel;
                return;
            }
        }
        else
        {
            if (!Security.IsAuthorized(EnumPermType.AccountingEdit, _journalEntries[0].DateDisplayed))
            {
                butSave.Enabled = false;
                butDelete.Enabled = false;
            }
        }

        _listJournalEntriesOld = [];

        foreach (var journalEntry in _journalEntries)
        {
            _listJournalEntriesOld.Add(journalEntry.Copy());
        }

        textDateTimeEntered.Text = _transaction.DateTimeEntry.ToString();
        textDateTimeEdited.Text = _transaction.SecDateTEdit.ToString();
        textUserEntered.Text = Userods.GetName(_transaction.UserNum);
        textUserEdited.Text = Userods.GetName(_transaction.SecUserNumEdit);
        textDate.Text = _journalEntries.Count > 0 ? _journalEntries[0].DateDisplayed.ToShortDateString() : DateTime.Today.ToShortDateString();

        if (_accountOfOrigin is null)
        {
            checkSimple.Checked = false;
            checkSimple.Visible = false;

            FillCompound();
        }
        else if (JournalEntries.AttachedToReconcile(_journalEntries))
        {
            labelReconcileDate.Visible = true;
            textReconcileDate.Visible = true;
            textReconcileDate.Text = JournalEntries.GetReconcileDate(_journalEntries).ToShortDateString();
            checkSimple.Checked = false;
            checkSimple.Visible = false;

            FillCompound();
        }
        else if (_journalEntries.Count > 2)
        {
            checkSimple.Checked = false;

            FillCompound();
        }
        else if (_journalEntries.Count == 2 && _journalEntries[0].Memo != _journalEntries[1].Memo)
        {
            checkSimple.Checked = false;

            FillCompound();
        }
        else
        {
            checkSimple.Checked = true;

            FillSimple();
        }

        if (_transaction.DepositNum == 0)
        {
            butAttachDep.Text = "Attach";
        }
        else
        {
            var deposit = Deposits.GetOne(_transaction.DepositNum);

            textSourceDeposit.Text = deposit.DateDeposit.ToShortDateString() + "  " + deposit.Amount.ToString("c");

            butAttachDep.Text = "Detach";
        }

        if (_transaction.PayNum == 0)
        {
            butAttachPay.Visible = false;
        }
        else
        {
            var payment = Payments.GetPayment(_transaction.PayNum);

            textSourcePay.Text = Patients.GetPat(payment.PatNum).GetNameFL() + " " + payment.PayDate.ToShortDateString() + " " + payment.PayAmt.ToString("c");
            butAttachPay.Text = "Detach";
        }

        if (_transaction.TransactionInvoiceNum == 0)
        {
            butOpenInvoice.Enabled = false;
            return;
        }

        butAttachInvoice.Text = "Detach";
        textSourceInvoice.Text = TransactionInvoices.GetName(_transaction.TransactionInvoiceNum);
        butOpenInvoice.Enabled = true;
    }

    private void FillCompound()
    {
        panelSimple.Visible = false;
        panelCompound.Visible = true;
        var isMemoSame = true;
        var memo = "";
        double debits = 0;
        double credits = 0;
        gridMain.BeginUpdate();
        gridMain.Columns.Clear();
        var col = new GridColumn(Lan.g("TableTransSplits", "Account"), 150);
        gridMain.Columns.Add(new GridColumn("Account", 150));

        var str = "Debit";
        if (_accountOfOrigin != null)
        {
            if (Accounts.DebitIsPos(_accountOfOrigin.AcctType))
            {
                str += Lan.g(this, "(+)");
            }
            else
            {
                str += Lan.g(this, "(-)");
            }
        }

        gridMain.Columns.Add(new GridColumn(str, 70, HorizontalAlignment.Right));

        str = "Credit";
        if (_accountOfOrigin != null)
        {
            if (Accounts.DebitIsPos(_accountOfOrigin.AcctType))
            {
                str += "(-)";
            }
            else
            {
                str += "(+)";
            }
        }

        gridMain.Columns.Add(new GridColumn(str, 70, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Memo", 200));

        gridMain.ListGridRows.Clear();

        for (var i = 0; i < _journalEntries.Count; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(Accounts.GetDescript(_journalEntries[i].AccountNum));
            gridRow.Cells.Add(_journalEntries[i].DebitAmt == 0 ? "" : _journalEntries[i].DebitAmt.ToString("n"));
            gridRow.Cells.Add(_journalEntries[i].CreditAmt == 0 ? "" : _journalEntries[i].CreditAmt.ToString("n"));
            gridRow.Cells.Add(_journalEntries[i].Memo);

            gridMain.ListGridRows.Add(gridRow);
            if (i == 0)
            {
                memo = _journalEntries[i].Memo;
            }
            else
            {
                if (memo != _journalEntries[i].Memo)
                {
                    isMemoSame = false;
                }
            }

            credits += _journalEntries[i].CreditAmt;
            debits += _journalEntries[i].DebitAmt;
        }

        gridMain.EndUpdate();

        checkMemoSame.Checked = isMemoSame;

        textCredit.Text = credits.ToString("n");
        textDebit.Text = debits.ToString("n");
    }

    private void FillSimple()
    {
        panelSimple.Visible = true;
        panelCompound.Visible = false;

        switch (_journalEntries.Count)
        {
            case 0:
                _accountPicked = null;
                textAccount.Text = "";
                butChange.Text = "Pick";
                textAmount.Text = "";
                textMemo.Text = "";
                _journalEntries = [];
                return;

            case 1:
            {
                double amt;

                if (Accounts.DebitIsPos(_accountOfOrigin.AcctType))
                {
                    if (_journalEntries[0].DebitAmt > 0)
                    {
                        amt = _journalEntries[0].DebitAmt;
                    }
                    else
                    {
                        amt = -_journalEntries[0].CreditAmt;
                    }
                }
                else
                {
                    if (_journalEntries[0].DebitAmt > 0)
                    {
                        amt = -_journalEntries[0].DebitAmt;
                    }
                    else
                    {
                        amt = _journalEntries[0].CreditAmt;
                    }
                }

                if (_journalEntries[0].AccountNum != _accountOfOrigin.AccountNum)
                {
                    amt = -amt;
                }

                textAmount.Text = amt.ToString("n");
                if (_journalEntries[0].AccountNum == 0)
                {
                    _accountPicked = null;
                    textAccount.Text = "";
                    butChange.Text = "Pick";
                }
                else if (_journalEntries[0].AccountNum == _accountOfOrigin.AccountNum)
                {
                    _accountPicked = null;
                    textAccount.Text = "";
                    butChange.Text = "Pick";
                }
                else
                {
                    _accountPicked = Accounts.GetAccount(_journalEntries[0].AccountNum);
                    textAccount.Text = _accountPicked.Description;
                    butChange.Text = "Change";
                }

                textMemo.Text = _journalEntries[0].Memo;
                textCheckNumber.Text = _journalEntries[0].CheckNumber;

                _journalEntries = [];
                return;
            }
        }

        JournalEntry journalEntry;
        JournalEntry journalEntryOther;

        if (_journalEntries[0].AccountNum == _accountOfOrigin.AccountNum)
        {
            journalEntry = _journalEntries[0];
            journalEntryOther = _journalEntries[1];
        }
        else
        {
            journalEntry = _journalEntries[1];
            journalEntryOther = _journalEntries[0];
        }

        if (Accounts.DebitIsPos(_accountOfOrigin.AcctType))
        {
            textAmount.Text = journalEntry.DebitAmt > 0 ? journalEntry.DebitAmt.ToString("n") : (-journalEntry.CreditAmt).ToString("n");
        }
        else
        {
            textAmount.Text = journalEntry.DebitAmt > 0 ? (-journalEntry.DebitAmt).ToString("n") : journalEntry.CreditAmt.ToString("n");
        }

        if (journalEntryOther.AccountNum == 0)
        {
            _accountPicked = null;

            textAccount.Text = "";

            butChange.Text = "Pick";
        }
        else
        {
            _accountPicked = Accounts.GetAccount(journalEntryOther.AccountNum);

            textAccount.Text = _accountPicked.Description;

            butChange.Text = "Change";
        }

        textMemo.Text = journalEntry.Memo;
        if (journalEntry.CheckNumber != "")
        {
            textCheckNumber.Text = journalEntry.CheckNumber;
        }

        if (journalEntryOther.CheckNumber != "")
        {
            textCheckNumber.Text = journalEntryOther.CheckNumber;
        }

        _journalEntries = [];
    }

    private void ButtonChange_Click(object sender, EventArgs e)
    {
        using var formAccountPick = new FormAccountPick();

        if (formAccountPick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _accountPicked = formAccountPick.SelectedAccount.Clone();

        textAccount.Text = _accountPicked.Description;

        butChange.Text = "Change";
    }

    private void checkSimple_Click(object sender, EventArgs e)
    {
        if (checkSimple.Checked)
        {
            if (_journalEntries.Count > 2)
            {
                ShowError("Not allowed to switch to simple view when there are more then two entries.");

                checkSimple.Checked = false;
                return;
            }

            if (_journalEntries.Count == 2 && _journalEntries[0].Memo != _journalEntries[1].Memo)
            {
                if (!ConfirmOk("Note might be lost. Continue?"))
                {
                    checkSimple.Checked = false;
                    return;
                }
            }

            FillSimple();
        }
        else
        {
            CreateTwoEntries();

            FillCompound();
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var journalEntry = new JournalEntry
        {
            TransactionNum = _transaction.TransactionNum
        };

        if (checkMemoSame.Checked && _journalEntries.Count > 0)
        {
            journalEntry.Memo = _journalEntries[0].Memo;
        }

        using var formJournalEntryEdit = new FormJournalEntryEdit(journalEntry);

        if (formJournalEntryEdit.ShowDialog() == DialogResult.OK)
        {
            _journalEntries.Add(journalEntry);

            if (checkMemoSame.Checked)
            {
                foreach (var otherJournalEntry in _journalEntries)
                {
                    otherJournalEntry.Memo = journalEntry.Memo;
                }
            }
        }

        FillCompound();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var journalEntry = _journalEntries[e.Row];

        using var formJournalEntryEdit = new FormJournalEntryEdit(journalEntry);

        switch (formJournalEntryEdit.ShowDialog())
        {
            case DialogResult.Abort:
                _journalEntries.RemoveAt(e.Row);
                break;

            case DialogResult.OK:
            {
                if (checkMemoSame.Checked)
                {
                    foreach (var otherJournalEntry in _journalEntries)
                    {
                        otherJournalEntry.Memo = journalEntry.Memo;
                    }
                }

                break;
            }
        }

        FillCompound();
    }

    private void butExport_Click(object sender, EventArgs e)
    {
        //DateTime reconcileDate=PIn.Date(textReconcileDate.Text);
        //List<Tuple<string,string>> listOtherDetails=new List<Tuple<string,string>>() {
        //	Tuple.Create(labelDateTimeEntered.Text,PIn.DateT(textDateTimeEntered.Text).ToString()),
        //	Tuple.Create(labelUserEntered.Text,PIn.String(textUserEntered.Text)),
        //	Tuple.Create(labelDateTimeEdited.Text,PIn.DateT(textDateTimeEdited.Text).ToString()),
        //	Tuple.Create(labelUserEdited.Text,PIn.String(textUserEdited.Text)),
        //	Tuple.Create(labelDate.Text,PIn.Date(textDate.Text).ToShortDateString()),
        //	Tuple.Create(labelReconcileDate.Text,(reconcileDate==DateTime.MinValue?"":reconcileDate.ToShortDateString()))
        //};
        //GridRow totalsRow=new GridRow("Totals",textDebit.Text,textCredit.Text,"");
        //string msg=
        gridMain.Export(gridMain.Title); //listOtherDetails:listOtherDetails,totalsRow:totalsRow);
        //if(!string.IsNullOrEmpty(msg)) {
        //	MsgBox.Show(this,msg);
        //}
    }

    private void ButtonAttachDep_Click(object sender, EventArgs e)
    {
        if (_transaction.DepositNum == 0)
        {
            using var formDeposits = new FormDeposits();

            formDeposits.IsSelectionMode = true;

            if (formDeposits.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            _transaction.DepositNum = formDeposits.SelectedDeposit.DepositNum;

            textSourceDeposit.Text = formDeposits.SelectedDeposit.DateDeposit.ToShortDateString() + "  " + formDeposits.SelectedDeposit.Amount.ToString("c");

            butAttachDep.Text = "Detach";

            return;
        }

        _transaction.DepositNum = 0;

        textSourceDeposit.Text = "";

        butAttachDep.Text = "Attach";
    }

    private void ButtonAttachPay_Click(object sender, EventArgs e)
    {
        if (_transaction.PayNum == 0)
        {
            return;
        }

        _transaction.PayNum = 0;

        textSourcePay.Text = "";

        butAttachPay.Visible = false;
    }

    private void ButtonAttachInvoice_Click(object sender, EventArgs e)
    {
        if (_transaction.TransactionInvoiceNum != 0)
        {
            if (!Confirm("Detach invoice file?"))
            {
                return;
            }

            TransactionInvoices.Delete(_transaction.TransactionInvoiceNum);

            _transaction.TransactionInvoiceNum = 0;

            Transactions.UpdateInvoiceNum(_transaction.TransactionNum, _transaction.TransactionInvoiceNum);

            textSourceInvoice.Text = "";

            butAttachInvoice.Text = "Attach";
            butOpenInvoice.Enabled = false;
            return;
        }

        using var openFileDialog = new OpenFileDialog();

        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var importFilePath = openFileDialog.FileName;
        if (!File.Exists(importFilePath))
        {
            ShowError("File does not exist or cannot be read.");
            return;
        }

        var transactionInvoice = new TransactionInvoice();
        if (PrefC.GetBool(PrefName.AccountingInvoiceAttachmentsSaveInDatabase))
        {
            transactionInvoice.FileName = Path.GetFileName(importFilePath);
            try
            {
                var bytes = File.ReadAllBytes(importFilePath);

                transactionInvoice.InvoiceData = Convert.ToBase64String(bytes);

                if (transactionInvoice.InvoiceData.Length > 16777215)
                {
                    ShowError("Invoice cannot be greater than about 12MB.");
                    return;
                }

                TransactionInvoices.Insert(transactionInvoice);
            }
            catch (Exception ex)
            {
                ShowException(ex, ex.Message);

                return;
            }
        }
        else
        {
            transactionInvoice.FileName = Path.GetFileName(importFilePath);
            transactionInvoice.FilePath = importFilePath;
            TransactionInvoices.Insert(transactionInvoice);
        }

        _transaction.TransactionInvoiceNum = transactionInvoice.TransactionInvoiceNum;

        Transactions.UpdateInvoiceNum(_transaction.TransactionNum, transactionInvoice.TransactionInvoiceNum);

        butAttachInvoice.Text = "Detach";

        textSourceInvoice.Text = transactionInvoice.FileName;

        butOpenInvoice.Enabled = true;
    }

    private void butOpenInvoice_Click(object sender, EventArgs e)
    {
        var transactionInvoice = TransactionInvoices.GetOne(_transaction.TransactionInvoiceNum);
        
        if (string.IsNullOrEmpty(transactionInvoice.FilePath))
        {
            var fileExt = Path.GetExtension(transactionInvoice.FileName);
            var prefix = transactionInvoice.FileName.Substring(0, transactionInvoice.FileName.Length - fileExt.Length);
            var filePath = ODFileUtils.CreateRandomFile(PrefC.GetTempFolderPath(), fileExt, prefix);
            var bytes = Convert.FromBase64String(transactionInvoice.InvoiceData);
            try
            {
                ODFileUtils.WriteAllBytesThenStart(filePath, bytes, null);
            }
            catch (Exception ex)
            {
                FriendlyException.Show(ex.Message, ex);
                return;
            }

            return;
        }

        try
        {
            ODFileUtils.ProcessStart(transactionInvoice.FilePath);
        }
        catch
        {
            ShowError(transactionInvoice.FilePath + " cannot be found.");
        }
    }

    private void CreateTwoEntries()
    {
        var journalEntry = new JournalEntry
        {
            TransactionNum = _transaction.TransactionNum
        };

        if (textDate.Text == "" || !textDate.IsValid())
        {
            journalEntry.DateDisplayed = DateTime.Today;
        }
        else
        {
            journalEntry.DateDisplayed = SIn.Date(textDate.Text);
        }

        journalEntry.AccountNum = _accountOfOrigin.AccountNum;

        double amt = 0;
        if (textAmount.IsValid())
        {
            amt = SIn.Double(textAmount.Text);
        }

        switch (amt)
        {
            case > 0 when Accounts.DebitIsPos(_accountOfOrigin.AcctType):
                journalEntry.DebitAmt = amt;
                break;
            
            case > 0:
                journalEntry.CreditAmt = amt;
                break;
            
            case < 0 when Accounts.DebitIsPos(_accountOfOrigin.AcctType):
                journalEntry.CreditAmt = -amt;
                break;
            
            case < 0:
                journalEntry.DebitAmt = -amt;
                break;
        }

        if (!IsNew)
        {
            journalEntry.SecUserNumEntry = _listJournalEntriesOld[0].SecUserNumEntry;
            journalEntry.SecDateTEntry = _listJournalEntriesOld[0].SecDateTEntry;
            journalEntry.JournalEntryNum = _listJournalEntriesOld[0].JournalEntryNum;
        }

        journalEntry.Memo = textMemo.Text;
        journalEntry.CheckNumber = textCheckNumber.Text;

        _journalEntries.Add(journalEntry);

        journalEntry = new JournalEntry
        {
            TransactionNum = _transaction.TransactionNum,
            DateDisplayed = _journalEntries[0].DateDisplayed,
            DebitAmt = _journalEntries[0].CreditAmt,
            CreditAmt = _journalEntries[0].DebitAmt,
            AccountNum = _accountPicked?.AccountNum ?? 0
        };

        if (!IsNew && _listJournalEntriesOld.Count > 1)
        {
            journalEntry.SecUserNumEntry = _listJournalEntriesOld[1].SecUserNumEntry;
            journalEntry.SecDateTEntry = _listJournalEntriesOld[1].SecDateTEntry;
            journalEntry.JournalEntryNum = _listJournalEntriesOld[1].JournalEntryNum;
        }

        journalEntry.Memo = textMemo.Text;
        journalEntry.CheckNumber = textCheckNumber.Text;

        _journalEntries.Add(journalEntry);
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (!ConfirmOk("Delete this entire transaction?"))
        {
            return;
        }

        var securityentry = "";
        if (!IsNew)
        {
            _journalEntries = JournalEntries.GetForTrans(_transaction.TransactionNum);

            securityentry = "Deleted: " + _journalEntries[0].DateDisplayed.ToShortDateString() + " ";

            double tot = 0;
            for (var i = 0; i < _journalEntries.Count; i++)
            {
                tot += _journalEntries[i].DebitAmt;
                if (i > 0)
                {
                    securityentry += ", ";
                }

                securityentry += Accounts.GetDescript(_journalEntries[i].AccountNum);
            }

            securityentry += ". " + tot.ToString("c");

            _journalEntries = [];
        }

        try
        {
            Transactions.Delete(_transaction);
        }
        catch (ApplicationException ex)
        {
            _journalEntries = JournalEntries.GetForTrans(_transaction.TransactionNum);

            ShowError(ex.Message);

            return;
        }

        if (!IsNew)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.AccountingEdit, 0, securityentry);
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textDate.IsValid())
        {
            ShowError("Please fix data entry errors first.");

            return;
        }

        var date = SIn.Date(textDate.Text);

        if (IsNew)
        {
            if (!Security.IsAuthorized(EnumPermType.AccountingCreate, date))
            {
                return;
            }
        }
        else
        {
            if (!Security.IsAuthorized(EnumPermType.AccountingEdit, date))
            {
                return;
            }
        }

        if (checkSimple.Checked)
        {
            if (!textAmount.IsValid())
            {
                ShowError("Please fix data entry errors first.");
                return;
            }

            if (_accountPicked == null)
            {
                ShowError("Please select an account first.");
                return;
            }

            CreateTwoEntries();
        }
        else
        {
            if (textCredit.Text != textDebit.Text)
            {
                ShowError("Debits and Credits must be equal.");
                return;
            }

            foreach (var journalEntry in _journalEntries)
            {
                if (journalEntry.AccountNum == 0)
                {
                    ShowError("Accounts must be selected for each entry first.");
                    return;
                }
            }
        }

        for (var i = 0; i < _journalEntries.Count; i++)
        {
            _journalEntries[i].DateDisplayed = date;
            
            var splits = "";
            for (var j = 0; j < _journalEntries.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if (splits != "")
                {
                    splits += "\r\n";
                }

                splits += Accounts.GetDescript(_journalEntries[j].AccountNum);
                if (_journalEntries.Count < 3)
                {
                    continue;
                }

                if (_journalEntries[j].CreditAmt > 0)
                {
                    splits += "  " + _journalEntries[j].CreditAmt.ToString("n");
                }
                else if (_journalEntries[j].DebitAmt > 0)
                {
                    splits += "  " + _journalEntries[j].DebitAmt.ToString("n");
                }
            }

            _journalEntries[i].Splits = splits;
        }

        JournalEntries.UpdateList(_listJournalEntriesOld, _journalEntries);
        
        var dateTimePrevious = _transaction.SecDateTEdit;
        
        Transactions.Update(_transaction);
        
        double total = 0;
        foreach (var journalEntry in _journalEntries)
        {
            total += journalEntry.DebitAmt;
        }

        if (IsNew)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.AccountingCreate, 0, date.ToShortDateString() + " " + _accountOfOrigin.Description + " " + total.ToString("c"), _transaction.TransactionNum, DateTime.MinValue);
            
            DialogResult = DialogResult.OK;
            return;
        }

        var logMessage = date.ToShortDateString();
        if (_accountOfOrigin != null)
        {
            logMessage += " " + _accountOfOrigin.Description;
        }

        logMessage += " " + total.ToString("c");
        
        SecurityLogs.MakeLogEntry(EnumPermType.AccountingEdit, 0, logMessage, _transaction.TransactionNum, dateTimePrevious);
        
        DialogResult = DialogResult.OK;
    }
}