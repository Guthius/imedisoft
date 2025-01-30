using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormAdjMulti : FormODBase
{
    private readonly Patient _patient;
    private readonly List<long> _procNums;
    private readonly List<Adjustment> _adjustments;
    private readonly PaymentEdit.LoadData _loadData;
    private readonly Program _program;
    private readonly Patient _patientGuar;
    private List<ProcAdjs> _procAdjs = [];
    private RigorousAdjustments _rigorousAdjustment;
    private List<Def> _defsAdjPosCats;
    private List<Def> _defsAdjNegCats;
    private List<long> _excludedAdjTypeNums;

    public FormAdjMulti(Patient patient, List<long> procNums = null, List<Adjustment> adjustments = null)
    {
        InitializeComponent();

        _patient = patient;
        _procNums = procNums ?? [];
        _adjustments = adjustments ?? [];
        _loadData = PaymentEdit.GetLoadData(patient, new Payment(), true, false);
        _program = Programs.GetCur(ProgramName.Transworld);
        _patientGuar = Patients.GetGuarForPat(_patient.PatNum);
    }

    private void FormMultiAdj_Load(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.AdjustmentCreate, true) && !Security.IsAuthorized(EnumPermType.AdjustmentEditZero, true))
        {
            ODMessageBox.Show("Not authorized for\r\n" + GroupPermissions.GetDesc(EnumPermType.AdjustmentCreate) + " and " + GroupPermissions.GetDesc(EnumPermType.AdjustmentEditZero));
            DialogResult = DialogResult.Cancel;
            return;
        }

        dateAdjustment.Text = DateTime.Today.ToShortDateString();

        _rigorousAdjustment = PrefC.GetEnum<RigorousAdjustments>(PrefName.RigorousAdjustments);

        checkOnlyTsiExcludedAdjTypes.CheckedChanged -= checkOnlyTsiExcludedAdjTypes_Checked;

        var programPropertiesExcludedAdjTypes = ProgramProperties
            .GetWhere(x => x.ProgramNum == _program.ProgramNum && _program.Enabled &&
                           x.PropertyDesc is
                               ProgramProperties.PropertyDescs.TransWorld.SyncExcludePosAdjType or
                               ProgramProperties.PropertyDescs.TransWorld.SyncExcludeNegAdjType);

        var programPropertiesForClinicExcludedAdjTypes = programPropertiesExcludedAdjTypes.FindAll(x => x.ClinicNum == _patientGuar.ClinicNum);
        if (programPropertiesForClinicExcludedAdjTypes.Count == 0)
        {
            programPropertiesForClinicExcludedAdjTypes = programPropertiesExcludedAdjTypes.FindAll(x => x.ClinicNum == 0);
        }

        _excludedAdjTypeNums = programPropertiesForClinicExcludedAdjTypes.Select(x => SIn.Long(x.PropertyValue, false)).ToList();
        if (_program.Enabled && Patients.IsGuarCollections(_patientGuar.PatNum) && _excludedAdjTypeNums.Any(x => x > 0))
        {
            checkOnlyTsiExcludedAdjTypes.Checked = true;
        }
        else
        {
            checkOnlyTsiExcludedAdjTypes.Visible = false;
            checkOnlyTsiExcludedAdjTypes.Checked = false;
        }

        FillListBoxAdjTypes();

        checkOnlyTsiExcludedAdjTypes.CheckedChanged += checkOnlyTsiExcludedAdjTypes_Checked;

        FillComboProv();

        comboClinic.ClinicNumSelected = 0;

        if (_rigorousAdjustment == RigorousAdjustments.EnforceFully)
        {
            comboProv.Enabled = false;
            comboClinic.Enabled = false;
            butPickProv.Enabled = false;
        }

        if (_rigorousAdjustment == RigorousAdjustments.DontEnforce)
        {
            radioIncludeAll.Checked = true;
        }
        else
        {
            radioAllocatedOnly.Checked = true;
        }

        FillGrid();

        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            if (gridMain.ListGridRows[i].Tag.GetType() != typeof(Procedure))
            {
                continue;
            }

            if (_procNums.Contains(((Procedure) gridMain.ListGridRows[i].Tag).ProcNum))
            {
                gridMain.SetSelected(i);
            }
        }

        textAmt.Select();
    }

    private void FillComboProv()
    {
        comboProv.Items.Clear();

        List<Provider> providers =
        [
            new()
            {
                ProvNum = 0,
                Abbr = "Inherit",
                IsHidden = false,
            }
        ];

        providers.AddRange(Providers.GetDeepCopy(true));

        comboProv.Items.AddProvsAbbr(providers);

        comboProv.SelectedIndex = 0;
    }

    private void FillGrid()
    {
        _procAdjs = GetProcAdjs();

        var assignedProcAdjs = _procAdjs.FindAll(x => x.ProcedureCur != null);
        var unassignedProcAdjs = _procAdjs.FindAll(x => x.ProcedureCur == null);

        CreditCalcType creditCalcType;

        if (radioAllocatedOnly.Checked)
        {
            creditCalcType = CreditCalcType.AllocatedOnly;
        }
        else if (radioIncludeAll.Checked)
        {
            creditCalcType = CreditCalcType.IncludeAll;
        }
        else
        {
            creditCalcType = CreditCalcType.ExcludeAll;
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 70, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn {Heading = "Provider", IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn {Heading = "Clinic", IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn {Heading = "Type", IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn("Fee", 70, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Rem Before", 70, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Adj Amt", 70, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Rem After", 70, HorizontalAlignment.Right));

        gridMain.ListGridRows.Clear();

        foreach (var procAdjs in assignedProcAdjs)
        {
            if (creditCalcType is CreditCalcType.IncludeAll or CreditCalcType.AllocatedOnly && !_procNums.Contains(procAdjs.ProcNum) && procAdjs.AccountEntryAdjustments.IsNullOrEmpty() && CompareDecimal.IsZero(procAdjs.AccountEntryProc.AmountEnd))
            {
                continue;
            }

            gridMain.ListGridRows.AddRange(procAdjs.GetGridRows());
        }

        for (var i = 0; i < unassignedProcAdjs.Count; i++)
        {
            if (i == 0)
            {
                var gridRow = new GridRow();

                gridRow.Cells.Add("Unassigned");
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.ColorBackG = Color.LightYellow;

                gridMain.ListGridRows.Add(gridRow);
            }

            gridMain.ListGridRows.AddRange(unassignedProcAdjs[i].GetGridRows());
        }

        gridMain.EndUpdate();
    }

    private void FillListBoxAdjTypes()
    {
        listTypePos.Items.Clear();
        listTypeNeg.Items.Clear();

        _defsAdjPosCats = Defs.GetPositiveAdjTypes(considerPermission: true);
        _defsAdjNegCats = Defs.GetNegativeAdjTypes(considerPermission: true);

        if (checkOnlyTsiExcludedAdjTypes.Checked)
        {
            _defsAdjPosCats = _defsAdjPosCats.FindAll(x => _excludedAdjTypeNums.Contains(x.DefNum));
            _defsAdjNegCats = _defsAdjNegCats.FindAll(x => _excludedAdjTypeNums.Contains(x.DefNum));

            listTypePos.Items.AddList(_defsAdjPosCats, x => x.ItemName);
            listTypeNeg.Items.AddList(_defsAdjNegCats, x => x.ItemName);
            return;
        }

        _defsAdjPosCats = _defsAdjPosCats.FindAll(x => !_excludedAdjTypeNums.Contains(x.DefNum));
        _defsAdjNegCats = _defsAdjNegCats.FindAll(x => !_excludedAdjTypeNums.Contains(x.DefNum));

        listTypePos.Items.AddList(_defsAdjPosCats, x => x.ItemName);
        listTypeNeg.Items.AddList(_defsAdjNegCats, x => x.ItemName);
    }
    
    private bool AddAdjustments()
    {
        if (!IsValid())
        {
            return false;
        }

        var selectedProcedures = gridMain.SelectedTags<Procedure>();
        if (selectedProcedures.IsNullOrEmpty())
        {
            _adjustments.Add(GetAdjFromUi());
        }
        else
        {
            var completeProcs = selectedProcedures.FindAll(x => x.ProcStatus == ProcStat.C);
            if (completeProcs.Any())
            {
                if (!completeProcs.All(x => Security.IsAuthorized(EnumPermType.ProcCompleteAddAdj, Procedures.GetDateForPermCheck(x), suppressMessage: true)))
                {
                    ShowError("Not allowed to add adjustments to completed procedures exceeding date limitation.");
                    return false;
                }
            }

            List<Adjustment> adjustmentsWarnOrBlock = [];
            
            var adjustmentBlockOrWarn = PrefC.GetEnum<EnumAdjustmentBlockOrWarn>(PrefName.AdjustmentBlockNegativeExceedingPatPortion);
            foreach (var procedure in selectedProcedures)
            {
                var procAdjs = _procAdjs.First(x => x.ProcedureCur == procedure);
                var adjustment = GetAdjFromUi(selectedProcedure: procedure, adjustmentsRelated: procAdjs.AccountEntryAdjustments.Select(x => (Adjustment) x.Tag).ToList());
               
                if (((double) procAdjs.AccountEntryProc.AmountEnd <= 0 && adjustment.AdjAmt > 0 && listTypeNeg.SelectedIndices.Count > 0 && SIn.Decimal(textAmt.Text) > 0)
                    || adjustment.AdjNum != 0)
                {
                    continue;
                }

                var adjustmentAmtTotal = _adjustments.FindAll(x => x.ProcNum == procedure.ProcNum).Sum(x => x.AdjAmt); //Get the sum of all new adjustments.
                if (adjustmentBlockOrWarn != EnumAdjustmentBlockOrWarn.Allow && (double) procAdjs.AccountEntryProc.AmountEnd + adjustmentAmtTotal + adjustment.AdjAmt < 0 && adjustment.AdjAmt < 0)
                {
                    adjustmentsWarnOrBlock.Add(adjustment);
                    continue;
                }

                _adjustments.Add(adjustment);
            }

            if (adjustmentsWarnOrBlock.Count > 0)
            {
                switch (adjustmentBlockOrWarn)
                {
                    case EnumAdjustmentBlockOrWarn.Block:
                        ShowError("Could not create a negative adjustment exceeding the remaining amount on " + adjustmentsWarnOrBlock.Count + " procedure(s).");
                        break;
                    
                    case EnumAdjustmentBlockOrWarn.Warn:
                    {
                        if (MsgBox.Show(MsgBoxButtons.YesNo, "Remaining amount on " + adjustmentsWarnOrBlock.Count + " procedure(s) is negative. Continue?", "Overpaid Procedure Warning"))
                        {
                            _adjustments.AddRange(adjustmentsWarnOrBlock);
                        }

                        break;
                    }
                }
            }
        }

        FillGrid();

        return true;
    }

    private Adjustment GetAdjFromUi(Adjustment adjustment = null, Procedure selectedProcedure = null, List<Adjustment> adjustmentsRelated = null)
    {
        adjustmentsRelated ??= [];

        var defAdjType = GetSelectedAdjDef();

        adjustment ??= new Adjustment();
        adjustment.AdjType = defAdjType.DefNum;
        adjustment.AdjDate = SIn.Date(dateAdjustment.Text);
        adjustment.AdjNote = SIn.String(textNote.Text);

        long selectedClinicNum;
        if (comboClinic.IsUnassignedSelected)
        {
            selectedClinicNum = selectedProcedure?.ClinicNum ?? _patient.ClinicNum;
        }
        else
        {
            selectedClinicNum = comboClinic.ClinicNumSelected;
        }

        adjustment.ClinicNum = selectedClinicNum;
        adjustment.PatNum = _patient.PatNum;
        adjustment.ProcDate = selectedProcedure?.ProcDate ?? DateTime.MinValue;
        adjustment.ProcNum = selectedProcedure?.ProcNum ?? 0;

        var provNum = comboProv.GetSelectedProvNum();
        if (provNum == 0)
        {
            provNum = selectedProcedure?.ProvNum ?? _patient.PriProv;
        }

        adjustment.ProvNum = provNum;

        var adjAmtOrPerc = SIn.Double(textAmt.Text);

        ProcAdjs procAdjs = null;

        if (selectedProcedure is not null)
        {
            procAdjs = _procAdjs.First(x => x.ProcedureCur == selectedProcedure);
        }

        switch (GetAdjAmtType())
        {
            case AdjAmtType.FixedAmt:
                adjustment.AdjAmt = adjAmtOrPerc;
                break;

            case AdjAmtType.PercentOfFee:
                adjustment.AdjAmt = Math.Round(adjAmtOrPerc / 100 * (double) procAdjs.AccountEntryProc.AmountOriginal, 2);
                break;

            case AdjAmtType.PercentOfRemBal:
                var adjSum = adjustmentsRelated.Sum(x => x.AdjAmt);
                var amtRem = (double) procAdjs.AccountEntryProc.AmountEnd + adjSum;
                adjustment.AdjAmt = Math.Round(adjAmtOrPerc / 100 * amtRem, 2);
                break;
        }

        if (defAdjType.ItemValue == "-")
        {
            adjustment.AdjAmt *= -1;
        }

        return adjustment;
    }

    private AdjAmtType GetAdjAmtType()
    {
        if (radioFixedAmt.Checked)
        {
            return AdjAmtType.FixedAmt;
        }

        return radioPercentRemBal.Checked ? AdjAmtType.PercentOfRemBal : AdjAmtType.PercentOfFee;
    }

    private List<ProcAdjs> GetProcAdjs()
    {
        var constructResults = PaymentEdit.ConstructAndLinkChargeCredits(_patient.PatNum,
            listPatNums: ListTools.FromSingle(_patient.PatNum),
            isIncomeTxfr: !radioIncludeAll.Checked,
            loadData: _loadData,
            hasInsOverpay: true);

        constructResults.ListAccountEntries.AddRange(_adjustments.Select(x => new AccountEntry(x)));

        return constructResults.ListAccountEntries
            .FindAll(x => x.PatNum == _patient.PatNum &&
                          (x.GetType() == typeof(Adjustment) && _adjustments.Contains(x.Tag) ||
                           (x.GetType() == typeof(Procedure) && ((Procedure) x.Tag).ProcStatus == ProcStat.C)))
            .GroupBy(x => x.ProcNum)
            .ToDictionary(x => x.Key, x => x.ToList())
            .Select(x => new ProcAdjs(x.Value))
            .OrderBy(x => x.ProcedureCur == null)
            .ToList();
    }

    private Def GetSelectedAdjDef()
    {
        return listTypePos.GetSelected<Def>() ?? listTypeNeg.GetSelected<Def>();
    }

    private bool IsValid()
    {
        var selectedProcedures = gridMain.SelectedTags<Procedure>();
        if (selectedProcedures.IsNullOrEmpty() && (_rigorousAdjustment == RigorousAdjustments.EnforceFully || GetAdjAmtType() != AdjAmtType.FixedAmt))
        {
            ShowError("You must select a procedure to add the adjustment to.");
            return false;
        }

        if (listTypePos.SelectedIndex == -1 && listTypeNeg.SelectedIndex == -1)
        {
            ShowError("Please pick an adjustment type.");
            return false;
        }

        if (comboProv.SelectedIndex == -1)
        {
            ShowError("Please pick a provider.");
            return false;
        }

        if (comboClinic.ClinicNumSelected == -1)
        {
            ShowError("Please pick a clinic.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(dateAdjustment.Text) || !dateAdjustment.IsValid())
        {
            ShowError("Please enter a valid date.");
            return false;
        }

        if (SIn.Date(dateAdjustment.Text).Date > DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed))
        {
            ShowError("Adjustments cannot be made for future dates");
            return false;
        }

        if (string.IsNullOrWhiteSpace(textAmt.Text) || !textAmt.IsValid())
        {
            ShowError("Please enter a valid amount.");
            return false;
        }

        var orthoProcLinks = OrthoProcLinks.GetManyForProcs(selectedProcedures.Select(x => x.ProcNum).ToList());
        if (orthoProcLinks.Count == 0)
        {
            return true;
        }

        ShowError(
            "One or more of the selected procedures cannot be adjusted because it is attached to an ortho case. " +
            "Please deselect these items and try again.");

        return false;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        AddAdjustments();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        var selectedAdjustments = gridMain.SelectedTags<Adjustment>();
        if (selectedAdjustments.IsNullOrEmpty())
        {
            return;
        }

        foreach (var adjustment in selectedAdjustments)
        {
            _adjustments.Remove(adjustment);
        }

        FillGrid();
    }

    private void ButtonPickProv_Click(object sender, EventArgs e)
    {
        var providers = comboProv.Items.GetAll<Provider>().FindAll(x => x.ProvNum > 0);
        var frmProviderPick = new FrmProviderPick(providers);

        frmProviderPick.ShowDialog();

        if (!frmProviderPick.IsDialogOK)
        {
            return;
        }

        comboProv.SelectedIndex = -1;
        comboProv.SetSelectedKey<Provider>(frmProviderPick.ProvNumSelected, x => x.ProvNum);
    }

    private void butUpdate_Click(object sender, EventArgs e)
    {
        var adjustments = gridMain.SelectedTags<Adjustment>();
        if (adjustments.IsNullOrEmpty())
        {
            return;
        }

        var hasSkipped = false;
        foreach (var adjustment in adjustments)
        {
            Procedure procedure = null;

            List<Adjustment> listAdjustmentsRelated = [];

            var procAdjs = _procAdjs.Where(x => x.ProcNum > 0).FirstOrDefault(x => x.ProcNum == adjustment.ProcNum);
            if (procAdjs is null)
            {
                if (GetAdjAmtType() is AdjAmtType.PercentOfFee or AdjAmtType.PercentOfRemBal)
                {
                    continue;
                }
            }
            else
            {
                procedure = procAdjs.ProcedureCur;

                var indexAdjEntry = procAdjs.AccountEntryAdjustments.FindIndex(x => x.Tag == adjustment);
                for (var j = indexAdjEntry - 1; j >= 0; j--)
                {
                    listAdjustmentsRelated.Add((Adjustment) procAdjs.AccountEntryAdjustments[j].Tag);
                }
            }

            if (!Security.IsAuthorized(EnumPermType.AdjustmentCreate, dateAdjustment.Value, true) && adjustment.AdjAmt != 0)
            {
                hasSkipped = true;
                continue;
            }

            GetAdjFromUi(adjustment: adjustment, selectedProcedure: procedure, adjustmentsRelated: listAdjustmentsRelated);
        }

        if (hasSkipped)
        {
            ShowError("Adjustment amount has to be 0.00 due to security permission.");
        }

        FillGrid();
    }

    private void GridMain_CellClick(object sender, ODGridClickEventArgs e)
    {
        var selectedAdjustments = gridMain.SelectedTags<Adjustment>();
        if (selectedAdjustments.IsNullOrEmpty())
        {
            return;
        }

        textNote.Text = selectedAdjustments.First().AdjNote;
    }

    private void ListBoxTypeNeg_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listTypeNeg.SelectedIndex > -1)
        {
            listTypePos.SelectedIndex = -1;
        }
    }

    private void ListBoxTypePos_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listTypePos.SelectedIndex > -1)
        {
            listTypeNeg.SelectedIndex = -1;
        }
    }

    private void RadioButtonCredits_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void RadioButtonFixedAmt_CheckedChanged(object sender, EventArgs e)
    {
        labelAmount.Text = radioFixedAmt.Checked ? "Amount" : "Percent";
    }
    
    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (_adjustments.IsNullOrEmpty())
        {
            if (!AddAdjustments())
            {
                return;
            }
        }

        var adjustmentBlockOrWarn = PrefC.GetEnum<EnumAdjustmentBlockOrWarn>(PrefName.AdjustmentBlockNegativeExceedingPatPortion);
        if (adjustmentBlockOrWarn != EnumAdjustmentBlockOrWarn.Allow)
        {
            var count = 0;

            var procAdjs = GetProcAdjs().Where(x => x.AccountEntryProc != null).ToList();
            foreach (var procAdj in procAdjs)
            {
                decimal sum = 0;
                if (procAdj.AccountEntryAdjustments.All(x => x.AdjNum != 0))
                {
                    continue;
                }

                foreach (var accountEntry in procAdj.AccountEntryAdjustments)
                {
                    sum += accountEntry.AmountEnd;
                }

                if (procAdj.AccountEntryProc.AmountEnd + sum < 0)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                switch (adjustmentBlockOrWarn)
                {
                    case EnumAdjustmentBlockOrWarn.Warn when !Confirm("There are " + count + " procedure(s) with negative amounts. Proceed?"):
                        return;

                    case EnumAdjustmentBlockOrWarn.Block:
                        ShowError(count + " procedure(s) cannot have their adjustments updated due to negative remaining values. Please fix to save");
                        return;
                }
            }
        }

        if (_program.Enabled && Patients.IsGuarCollections(_patientGuar.PatNum) && _excludedAdjTypeNums.Any(x => x > 0))
        {
            var listAdjustmentsPosExcluded = _adjustments.FindAll(x => _excludedAdjTypeNums.Contains(x.AdjType));
            var listAdjustmentsNegExcluded = _adjustments.FindAll(x => _excludedAdjTypeNums.Contains(x.AdjType));

            var messageText =
                "The guarantor of this family has been sent to TSI for a past due balance and you have selected an adjustment type that will be synched with TSI. " +
                "This balance adjustment could result in a TSI charge for collection. Continue?";

            if (listAdjustmentsPosExcluded.Count > 0 || listAdjustmentsNegExcluded.Any())
            {
                messageText =
                    "The guarantor of this family has been sent to TSI for a past due balance and you have selected an adjustment type that is excluded from being synched with TSI. " +
                    "This will not reduce the balance sent for collection by TSI. Continue?";
            }

            if (!MsgBox.Show(this, MsgBoxButtons.OKCancel, messageText))
            {
                return;
            }
        }

        var loadData = _loadData.Copy();

        loadData.ConstructChargesData.ListAdjustments.AddRange(_adjustments);

        PaymentEdit.ConstructAndLinkChargeCredits(_patient.PatNum, listPatNums: [_patient.PatNum], isIncomeTxfr: !radioIncludeAll.Checked, loadData: loadData, hasInsOverpay: true);

        if (!Security.IsAuthorized(EnumPermType.AdjustmentCreate, SIn.Date(dateAdjustment.Text), true) || _adjustments.Any(x => !Security.IsAuthorized(EnumPermType.AdjustmentCreate, x.AdjDate, true)))
        {
            if (_adjustments.Any(x => !CompareDouble.IsZero(x.AdjAmt)))
            {
                ShowError("Amount has to be 0.00 due to security permission.");
                return;
            }
        }

        List<string> adjustmentAmounts = [];
        foreach (var adjustment in _adjustments)
        {
            Adjustments.Insert(adjustment);

            TsiTransLogs.CheckAndInsertLogsIfAdjTypeExcluded(adjustment);

            adjustmentAmounts.Add(adjustment.AdjAmt.ToString("c"));
        }

        if (adjustmentAmounts.Count > 0)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentCreate, _patient.PatNum,
                "Adjustment(s) created from Add Multiple Adjustments window: " + string.Join(",", adjustmentAmounts));
        }

        Signalods.SetInvalid(InvalidType.BillingList);

        DialogResult = DialogResult.OK;
    }

    private class ProcAdjs
    {
        public readonly AccountEntry AccountEntryProc;
        public readonly List<AccountEntry> AccountEntryAdjustments;

        public long ProcNum
        {
            get
            {
                long procNum = 0;

                if (AccountEntryProc != null)
                {
                    procNum = ((Procedure) AccountEntryProc.Tag).ProcNum;
                }

                return procNum;
            }
        }

        public Procedure ProcedureCur
        {
            get
            {
                Procedure procedure = null;

                if (AccountEntryProc != null)
                {
                    procedure = (Procedure) AccountEntryProc.Tag;
                }

                return procedure;
            }
        }

        public ProcAdjs(List<AccountEntry> listAccountEntries)
        {
            AccountEntryProc = listAccountEntries.FirstOrDefault(x => x.GetType() == typeof(Procedure));
            AccountEntryAdjustments = listAccountEntries.FindAll(x => x.GetType() == typeof(Adjustment));
        }

        public List<GridRow> GetGridRows()
        {
            double amtRemBefore = 0;

            List<GridRow> gridRows = [];

            if (AccountEntryProc is not null)
            {
                amtRemBefore = (double) AccountEntryProc.AmountEnd;

                var gridRow = new GridRow();

                gridRow.Cells.Add(ProcedureCur.ProcDate.ToShortDateString());
                gridRow.Cells.Add(Providers.GetAbbr(ProcedureCur.ProvNum));
                gridRow.Cells.Add(Clinics.GetAbbr(ProcedureCur.ClinicNum));
                gridRow.Cells.Add(ProcedureCodes.GetStringProcCode(ProcedureCur.CodeNum));
                gridRow.Cells.Add(new GridCell(AccountEntryProc.AmountOriginal.ToString("c")) {ColorBackG = Color.LightYellow});
                gridRow.Cells.Add(new GridCell(amtRemBefore.ToString("c")) {ColorBackG = Color.LightYellow});
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");
                gridRow.Tag = ProcedureCur;

                gridRows.Add(gridRow);
            }

            foreach (var accountEntry in AccountEntryAdjustments)
            {
                var adjustment = (Adjustment) accountEntry.Tag;
                var adjustmentAmount = Math.Round(adjustment.AdjAmt, 2);

                var gridRow = new GridRow();

                gridRow.Cells.Add(adjustment.AdjDate.ToShortDateString());
                gridRow.Cells.Add(Providers.GetAbbr(adjustment.ProvNum));
                gridRow.Cells.Add(Clinics.GetAbbr(adjustment.ClinicNum));
                gridRow.Cells.Add(Defs.GetName(DefCat.AdjTypes, adjustment.AdjType));
                gridRow.Cells.Add("");
                gridRow.Cells.Add("");

                gridRow.Cells.Add(new GridCell(adjustmentAmount.ToString("c")) {ColorBackG = Color.LightCyan});

                var gridCellRemAfter = new GridCell {ColorBackG = Color.LightCyan};
                if (AccountEntryProc != null)
                {
                    amtRemBefore += adjustmentAmount;

                    gridCellRemAfter.Text = amtRemBefore.ToString("c");
                }

                gridRow.Cells.Add(gridCellRemAfter);
                gridRow.Tag = adjustment;

                gridRows.Add(gridRow);
            }

            return gridRows;
        }
    }

    private enum AdjAmtType
    {
        FixedAmt,
        PercentOfRemBal,
        PercentOfFee,
    }

    private void checkOnlyTsiExcludedAdjTypes_Checked(object sender, EventArgs e)
    {
        FillListBoxAdjTypes();
    }
}