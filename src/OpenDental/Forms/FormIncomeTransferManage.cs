using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormIncomeTransferManage : FormODBase
{
    private readonly Family _family;
    private readonly Patient _patient;
    private PaymentEdit.ConstructResults _constructResults;

    public FormIncomeTransferManage(Family family, Patient patient)
    {
        _family = family;
        _patient = patient;

        InitializeComponent();
    }

    private void FormIncomeTransferManage_Load(object sender, EventArgs e)
    {
        if (Security.IsAuthorized(EnumPermType.PaymentCreate, DateTime.Today, true))
        {
            try
            {
                PaymentEdit.TransferClaimsPayAsTotal(_patient.PatNum, _family.GetPatNums(), "Automatic transfer of claims pay as total from income transfer.");
            }
            catch (ApplicationException ex)
            {
                ShowException(ex, ex.Message);

                return;
            }
        }

        RefreshWindow();
    }

    private void RefreshWindow()
    {
        FillTransfers();
        FillGridCharges();

        butTransfer.Enabled = IsTransferRigorousNeeded();

        var rigorousAccounting = (RigorousAccounting) PrefC.GetInt(PrefName.RigorousAccounting);

        butFIFO.Enabled = rigorousAccounting != RigorousAccounting.EnforceFully && IsTransferFifoNeeded();
    }

    private void FillTransfers()
    {
        gridTransfers.BeginUpdate();
        gridTransfers.Columns.Clear();
        gridTransfers.Columns.Add(new GridColumn("Date", 65, HorizontalAlignment.Center));
        gridTransfers.Columns.Add(new GridColumn("Clinic", 80) {IsWidthDynamic = true});
        gridTransfers.Columns.Add(new GridColumn("Paid By", 80) {IsWidthDynamic = true});
        gridTransfers.ListGridRows.Clear();

        var paymentTransfers = Payments.GetTransfers(_family.GetPatNums()).OrderBy(x => x.PayDate).ToList();

        foreach (var payment in paymentTransfers)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(payment.PayDate.ToShortDateString());
            gridRow.Cells.Add(Clinics.GetAbbr(payment.ClinicNum));
            gridRow.Cells.Add(_family.GetNameInFamFL(payment.PatNum));
            gridRow.Tag = payment;

            gridTransfers.ListGridRows.Add(gridRow);
        }

        gridTransfers.EndUpdate();
        gridTransfers.ScrollToEnd();
    }

    private void FillGridCharges(bool doRefreshData = true)
    {
        gridImbalances.BeginUpdate();
        gridImbalances.Columns.Clear();
        gridImbalances.Columns.Add(new GridColumn("Prov", 80) {IsWidthDynamic = true});
        gridImbalances.Columns.Add(new GridColumn("Patient", 80) {IsWidthDynamic = true});
        gridImbalances.Columns.Add(new GridColumn("Clinic", 80) {IsWidthDynamic = true});

        if (checkShowBreakdown.Checked)
        {
            gridImbalances.Columns.Add(new GridColumn("Type", 80) {IsWidthDynamic = true});
        }

        gridImbalances.Columns.Add(new GridColumn("Charges", 80, HorizontalAlignment.Right, GridSortingStrategy.AmountParse));
        gridImbalances.Columns.Add(new GridColumn("Credits", 80, HorizontalAlignment.Right, GridSortingStrategy.AmountParse));
        gridImbalances.Columns.Add(new GridColumn("Balance", 80, HorizontalAlignment.Right, GridSortingStrategy.AmountParse));
        gridImbalances.ListGridRows.Clear();

        if (_constructResults is null || doRefreshData)
        {
            _constructResults = PaymentEdit.ConstructAndLinkChargeCredits(_family.GetPatNums(), _patient.PatNum, [], new Payment(), [], isIncomeTxfr: true, dateAsOf: datePickerAsOf.Value);
        }

        var patNums = _constructResults.ListAccountEntries.Select(x => x.PatNum).Distinct().ToList();
        var patients = Patients.GetLimForPats(patNums);

        var accountEntryGroups = _constructResults.ListAccountEntries
            .GroupBy(accountEntry => new
            {
                accountEntry.PatNum,
                accountEntry.ProvNum,
                accountEntry.ClinicNum
            })
            .Select(grouping => grouping.ToList())
            .ToList();

        foreach (var entries in accountEntryGroups)
        {
            if (CompareDecimal.IsZero(entries.Sum(x => x.AmountEnd)))
            {
                continue;
            }

            gridImbalances.ListGridRows.AddRange(GetRowsForGroup(entries, patients));
        }

        gridImbalances.EndUpdate();
    }

    private List<GridRow> GetRowsForGroup(List<AccountEntry> accountEntries, List<Patient> listPatients)
    {
        var gridRows = new List<GridRow>();
        var accountEntryFirst = accountEntries.First();

        var patient = listPatients.Find(x => x.PatNum == accountEntryFirst.PatNum);
        var positiveEntries = accountEntries.FindAll(x => CompareDecimal.IsGreaterThanZero(x.AmountEnd));
        var negativeEntries = accountEntries.FindAll(x => CompareDecimal.IsLessThanZero(x.AmountEnd));

        var gridRow = new GridRow();

        gridRow.Cells.Add(Providers.GetAbbr(accountEntryFirst.ProvNum, includeHidden: true));
        gridRow.Cells.Add(patient.GetNameFLnoPref());
        gridRow.Cells.Add(Clinics.GetAbbr(accountEntryFirst.ClinicNum));

        if (checkShowBreakdown.Checked)
        {
            gridRow.Cells.Add("");
        }

        gridRow.Cells.Add(positiveEntries.Sum(x => x.AmountEnd).ToString("c"));
        gridRow.Cells.Add(negativeEntries.Sum(x => x.AmountEnd).ToString("c"));
        gridRow.Cells.Add(accountEntries.Sum(x => x.AmountEnd).ToString("c"));
        gridRow.Bold = checkShowBreakdown.Checked;
        gridRow.Tag = accountEntries;

        gridRows.Add(gridRow);
        if (!checkShowBreakdown.Checked)
        {
            return gridRows;
        }

        decimal runningTotal = 0;

        foreach (var accountEntry in accountEntries)
        {
            if (accountEntry.AmountEnd == 0 && accountEntry.GetType() == typeof(Adjustment) && accountEntry.ProcNum > 0)
            {
                continue;
            }

            runningTotal += accountEntry.AmountEnd;

            gridRow = new GridRow();
            gridRow.Cells.Add("");
            gridRow.Cells.Add("");
            gridRow.Cells.Add("");
            gridRow.Cells.Add(accountEntry.DescriptionForGrid);
            gridRow.Cells.Add(CompareDecimal.IsGreaterThanZero(accountEntry.AmountEnd) ? accountEntry.AmountEnd.ToString("c") : 0.ToString("c"));
            gridRow.Cells.Add(CompareDecimal.IsLessThanZero(accountEntry.AmountEnd) ? accountEntry.AmountEnd.ToString("c") : 0.ToString("c"));
            gridRow.Cells.Add(runningTotal.ToString("c"));
            gridRow.Tag = accountEntry;

            gridRows.Add(gridRow);
        }

        return gridRows;
    }

    private bool IsTransferFifoNeeded()
    {
        List<PaySplit> paySplits = null;

        ODException.SwallowAnyException(() =>
        {
            var incomeTransferData = PaymentEdit.GetIncomeTransferDataFIFO(_patient.PatNum, DateTime.Today);

            paySplits = incomeTransferData.ListSplitsCur;
        });

        return paySplits is null || paySplits.Count > 0;
    }

    private bool IsTransferRigorousNeeded()
    {
        var listAccountEntries = _constructResults.ListAccountEntries.Select(x => x.Copy()).ToList();
        if (!PaymentEdit.TryCreateIncomeTransfer(listAccountEntries, DateTime.Today, out var results))
        {
            return true;
        }

        if (results.HasInvalidSplits)
        {
            return true;
        }

        return results.ListSplitsCur.Count > 0;
    }

    private bool IsValid()
    {
        if (!Security.IsAuthorized(EnumPermType.PaymentCreate, datePickerAsOf.Value))
        {
            return false;
        }

        var logMessage = DatabaseMaintenances.ProcedurelogDeletedWithAttachedIncome();
        if (string.IsNullOrEmpty(logMessage))
        {
            return true;
        }

        var msgBoxCopyPaste = new MsgBoxCopyPaste(logMessage);

        msgBoxCopyPaste.Show();

        return false;
    }

    private void CreatePaymentAndRefresh(List<PaySplit> paySplits, bool isRigorous)
    {
        if (paySplits.IsNullOrEmpty())
        {
            return;
        }

        var payment = new Payment
        {
            PayDate = datePickerAsOf.Value,
            PatNum = _patient.PatNum,
            ClinicNum = Clinics.ClinicNum
        };

        if ((PayClinicSetting) PrefC.GetInt(PrefName.PaymentClinicSetting) == PayClinicSetting.PatientDefaultClinic)
        {
            payment.ClinicNum = _patient.ClinicNum;
        }
        else if ((PayClinicSetting) PrefC.GetInt(PrefName.PaymentClinicSetting) == PayClinicSetting.SelectedExceptHQ)
        {
            payment.ClinicNum = Clinics.ClinicNum == 0 ? _patient.ClinicNum : Clinics.ClinicNum;
        }

        payment.DateEntry = DateTime.Today; //So that it will show properly in the new window.
        payment.PaymentSource = CreditCardSource.None;
        payment.ProcessStatus = ProcessStat.OfficeProcessed;
        payment.PayAmt = 0;
        payment.PayType = 0;

        Payments.Insert(payment, paySplits);

        Signalods.SetInvalid(InvalidType.BillingList);

        var logicType = "FIFO logic";
        if (isRigorous)
        {
            logicType = "Rigorous logic";
        }

        var logMessage = Payments.GetSecuritylogEntryText(payment, payment, isNew: true) + $", from Income Transfer Manager using {logicType}.";

        SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate, payment.PatNum, logMessage);

        var errorMessage = Ledgers.ComputeAgingForPaysplitsAllocatedToDiffPats(_patient.PatNum, paySplits);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            ShowError(errorMessage);
        }

        RefreshWindow();
    }

    private void CheckBoxShowBreakdown_CheckedChanged(object sender, EventArgs e)
    {
        FillGridCharges(doRefreshData: false);
    }

    private void DatePickerTransfer_ValueChanged(object sender, EventArgs e)
    {
        RefreshWindow();
    }

    private void GridTransfers_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formPayment = new FormPayment(_patient, _family, (Payment) gridTransfers.ListGridRows[e.Row].Tag, false);

        formPayment.IsNew = false;
        formPayment.ShowDialog();

        RefreshWindow();
    }

    private void ButtonTransfer_Click(object sender, EventArgs e)
    {
        if (!IsValid())
        {
            return;
        }

        var accountEntries = _constructResults.ListAccountEntries.Select(x => x.Copy()).ToList();
        if (!PaymentEdit.TryCreateIncomeTransfer(accountEntries, datePickerAsOf.Value, out var results))
        {
            var msgBoxCopyPaste = new MsgBoxCopyPaste(results.StringBuilderErrors.ToString().TrimEnd());

            msgBoxCopyPaste.Show();

            return;
        }

        if (results.HasInvalidSplits)
        {
            ShowError(
                "One or more transfers were not created due to 'Allow prepayments to providers' being turned off.\r\n" +
                "Please create them manually.");
        }

        CreatePaymentAndRefresh(results.ListSplitsCur, true);

        if (results.StringBuilderWarnings.Length == 0)
        {
            return;
        }

        {
            var msgBoxCopyPaste = new MsgBoxCopyPaste(
                "The following warnings happened during the income transfer process.\r\n" +
                results.StringBuilderWarnings);

            msgBoxCopyPaste.Show();
        }
    }

    private void ButtonFIFO_Click(object sender, EventArgs e)
    {
        if (!IsValid())
        {
            return;
        }

        try
        {
            var incomeTransferData = PaymentEdit.GetIncomeTransferDataFIFO(_patient.PatNum, datePickerAsOf.Value, dateAsOf: datePickerAsOf.Value);

            CreatePaymentAndRefresh(incomeTransferData.ListSplitsCur, false);

            if (incomeTransferData.StringBuilderWarnings.Length == 0)
            {
                return;
            }

            var msgBoxCopyPaste = new MsgBoxCopyPaste(
                "The following warnings happened during the income transfer process.\r\n" +
                incomeTransferData.StringBuilderWarnings);

            msgBoxCopyPaste.Show();
        }
        catch (Exception ex)
        {
            ShowException(ex, "There was a problem making a FIFO transfer. Please call support.");
        }
    }
}