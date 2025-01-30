using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAdjustSelect : FormODBase
{
    private readonly double _amtPaySplitDisplay;
    private readonly List<AccountEntry> _accountEntriesAdjPat = [];

    public Adjustment SelectedAdjustment { get; set; }

    public FormAdjustSelect(double amtPaySplit, PaySplit paySplit, List<PaySplit> paySplits, List<Adjustment> adjustmentsPat, List<PaySplit> listPaySplitsAdj)
    {
        InitializeComponent();

        _amtPaySplitDisplay = amtPaySplit * -1;

        foreach (var adjustment in adjustmentsPat)
        {
            if (adjustment.ProcNum != 0)
            {
                continue;
            }

            _accountEntriesAdjPat.Add(new AccountEntry(adjustment));
        }

        foreach (var accountEntry in _accountEntriesAdjPat)
        {
            accountEntry.AmountAvailable -= (decimal) Adjustments.GetAmtAllocated(accountEntry.PriKey, paySplit.PayNum, listPaySplitsAdj.FindAll(x => x.AdjNum == accountEntry.PriKey));
            accountEntry.AmountAvailable -= (decimal) Adjustments.GetAmtAllocated(accountEntry.PriKey, 0, paySplits.FindAll(x => x.AdjNum == accountEntry.PriKey && x != paySplit));
        }
    }

    private void FormAdjustSelect_Load(object sender, EventArgs e)
    {
        labelCurSplitAmt.Text = _amtPaySplitDisplay.ToString("C");

        FillGrid();
    }

    private void FillGrid()
    {
        gridAdjusts.BeginUpdate();

        gridAdjusts.ListGridRows.Clear();

        gridAdjusts.Columns.Clear();
        gridAdjusts.Columns.Add(new GridColumn("Date", 70, HorizontalAlignment.Center));
        gridAdjusts.Columns.Add(new GridColumn("Prov", 60) {IsWidthDynamic = true});
        gridAdjusts.Columns.Add(new GridColumn("Clinic", 60) {IsWidthDynamic = true});
        gridAdjusts.Columns.Add(new GridColumn("Amt Orig", 60, HorizontalAlignment.Right));
        gridAdjusts.Columns.Add(new GridColumn("Amt Avail", 60, HorizontalAlignment.Right));

        foreach (var accountEntry in _accountEntriesAdjPat)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(((Adjustment) accountEntry.Tag).AdjDate.ToShortDateString());
            gridRow.Cells.Add(Providers.GetAbbr(((Adjustment) accountEntry.Tag).ProvNum));
            gridRow.Cells.Add(Clinics.GetAbbr(((Adjustment) accountEntry.Tag).ClinicNum));
            gridRow.Cells.Add(accountEntry.AmountOriginal.ToString("F"));
            gridRow.Cells.Add(accountEntry.AmountAvailable.ToString("F"));
            gridRow.Tag = accountEntry;

            gridAdjusts.ListGridRows.Add(gridRow);
        }

        gridAdjusts.EndUpdate();
    }

    private void GridAdjusts_CellClick(object sender, ODGridClickEventArgs e)
    {
        var accountEntry = gridAdjusts.SelectedTag<AccountEntry>();
        var amtUsedDisplay = (accountEntry.AmountOriginal - accountEntry.AmountAvailable) * -1;

        labelAmtOriginal.Text = accountEntry.AmountOriginal.ToString("C");
        labelAmtUsed.Text = amtUsedDisplay.ToString("C");
        labelAmtAvail.Text = accountEntry.AmountAvailable.ToString("C");
        labelAmtEnd.Text = ((double) accountEntry.AmountAvailable + _amtPaySplitDisplay).ToString("C");
    }

    private void GridAdjusts_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedAdjustment = (Adjustment) gridAdjusts.SelectedTag<AccountEntry>().Tag;
        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridAdjusts.SelectedIndices.Length < 1)
        {
            ShowError("Please select an adjustment first or close this window.");
            return;
        }

        SelectedAdjustment = (Adjustment) gridAdjusts.SelectedTag<AccountEntry>().Tag;

        DialogResult = DialogResult.OK;
    }
}