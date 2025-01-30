using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormAdjust : FormODBase
{
    private readonly Patient _patient;
    private readonly Adjustment _adjustment;
    private bool _checkZeroAmount;
    private List<Def> _listDefsAdjPosCats;
    private List<Def> _listDefsAdjNegCats;
    private decimal _adjRemAmt;
    private bool _isEditAnyway;
    private List<PaySplit> _paySplitsForAdjustment;
    private bool _isNegativeAdjustment;
    private List<long> _tsiExcludedAdjDefNums;
    private readonly Program _program;
    private readonly Patient _patientGuar;
    private readonly bool _isTsiAdj;

    public bool IsNew;

    public FormAdjust(Patient patient, Adjustment adjustment, bool isTsiAdj = false)
    {
        _patient = patient;
        _adjustment = adjustment;
        _program = Programs.GetCur(ProgramName.Transworld);
        _patientGuar = Patients.GetGuarForPat(_adjustment.PatNum);
        _isTsiAdj = isTsiAdj;
        
        InitializeComponent();
    }

    private void FormAdjust_Load(object sender, EventArgs e)
    {
        if (IsNew)
        {
            if (!Security.IsAuthorized(EnumPermType.AdjustmentCreate, DateTime.Now, true))
            {
                if (!Security.IsAuthorized(EnumPermType.AdjustmentEditZero, true))
                {
                    ShowError("Not authorized for\r\n" + GroupPermissions.GetDesc(EnumPermType.AdjustmentCreate));

                    DialogResult = DialogResult.Cancel;

                    return;
                }

                _checkZeroAmount = true;
            }
        }
        else
        {
            var def = Defs.GetDef(DefCat.AdjTypes, _adjustment.AdjType);

            if (!GroupPermissions.HasPermissionForAdjType(EnumPermType.AdjustmentEdit, def, _adjustment.AdjDate, suppressMessage: false))
            {
                butOK.Enabled = false;
                butDelete.Enabled = GroupPermissions.HasPermissionForAdjType(EnumPermType.AdjustmentEditZero, def) && _adjustment.AdjAmt == 0 && _adjustment.DateEntry.Date == MiscData.GetNowDateTime().Date;
            }

            var isAttachedToPayPlan = PayPlanLinks.GetForFKeyAndLinkType(_adjustment.AdjNum, PayPlanLinkType.Adjustment).Count > 0;

            _paySplitsForAdjustment = PaySplits.GetForAdjustments([_adjustment.AdjNum]);

            if (_paySplitsForAdjustment.Count > 0 || isAttachedToPayPlan)
            {
                butAttachProc.Enabled = false;
                butDetachProc.Enabled = false;
                labelProcDisabled.Visible = true;
            }

            if (Defs.GetValue(DefCat.AdjTypes, _adjustment.AdjType) == "dp")
            {
                labelAdditions.Text = "Discount Plan: " + Defs.GetName(DefCat.AdjTypes, _adjustment.AdjType);
                labelSubtractions.Visible = false;
                listTypePos.Visible = false;
                listTypeNeg.Visible = false;
            }
        }

        textDateEntry.Text = _adjustment.DateEntry.ToShortDateString();
        textAdjDate.Text = _adjustment.AdjDate.ToShortDateString();

        if (_adjustment.ProcDate.Year > 1880)
        {
            textProcDate.Text = _adjustment.ProcDate.ToShortDateString();
        }

        if (_adjustment.ProcNum != 0)
        {
            textProcDate.ReadOnly = true;
            butDetachProc.Enabled = true;
        }

        if (Defs.GetValue(DefCat.AdjTypes, _adjustment.AdjType) == "+")
        {
            textAmount.Text = _adjustment.AdjAmt.ToString("F");
        }
        else if (Defs.GetValue(DefCat.AdjTypes, _adjustment.AdjType) == "-")
        {
            textAmount.Text = (-_adjustment.AdjAmt).ToString("F");
        }
        else if (Defs.GetValue(DefCat.AdjTypes, _adjustment.AdjType) == "dp")
        {
            textAmount.Text = (-_adjustment.AdjAmt).ToString("F");
        }

        comboClinic.ClinicNumSelected = _adjustment.ClinicNum;
        comboProv.SetSelectedProvNum(_adjustment.ProvNum);

        FillComboProv();

        if (_adjustment.ProcNum != 0 && PrefC.GetInt(PrefName.RigorousAdjustments) == (int) RigorousAdjustments.EnforceFully)
        {
            comboProv.Enabled = false;
            butPickProv.Enabled = false;
            comboClinic.Enabled = false;

            if (Security.IsAuthorized(EnumPermType.Setup, true))
            {
                labelEditAnyway.Visible = true;
                butEditAnyway.Visible = true;
            }
        }

        checkOnlyTsiExcludedAdjTypes.CheckedChanged -= CheckBoxOnlyTsiExcludedAdjTypes_Checked;

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

        _tsiExcludedAdjDefNums = programPropertiesForClinicExcludedAdjTypes.Select(x => SIn.Long(x.PropertyValue, false)).ToList();
        if (_program.Enabled && Patients.IsGuarCollections(_patientGuar.PatNum) && _tsiExcludedAdjDefNums.Any(x => x > 0))
        {
            checkOnlyTsiExcludedAdjTypes.Checked = true;
        }
        else
        {
            checkOnlyTsiExcludedAdjTypes.Visible = false;
            checkOnlyTsiExcludedAdjTypes.Checked = false;
        }

        FillListBoxAdjTypes();

        checkOnlyTsiExcludedAdjTypes.CheckedChanged += CheckBoxOnlyTsiExcludedAdjTypes_Checked;

        FillProcedure();

        textNote.Text = _adjustment.AdjNote;
    }

    private void ListBoxTypePos_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listTypePos.SelectedIndex <= -1)
        {
            return;
        }

        listTypeNeg.SelectedIndex = -1;

        FillProcedure();
    }

    private void ListBoxTypeNeg_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listTypeNeg.SelectedIndex <= -1)
        {
            return;
        }

        listTypePos.SelectedIndex = -1;

        FillProcedure();
    }

    private void TextBoxAmount_Validating(object sender, CancelEventArgs e)
    {
        FillProcedure();
    }

    private void ButtonPickProv_Click(object sender, EventArgs e)
    {
        var frmProviderPick = new FrmProviderPick(comboProv.Items.GetAll<Provider>())
        {
            ProvNumSelected = comboProv.GetSelectedProvNum()
        };

        frmProviderPick.ShowDialog();

        if (!frmProviderPick.IsDialogOK)
        {
            return;
        }

        comboProv.SetSelectedProvNum(frmProviderPick.ProvNumSelected);
    }

    private void ComboBoxClinic_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillComboProv();
    }

    private void FillComboProv()
    {
        var provNum = comboProv.GetSelectedProvNum();

        comboProv.Items.Clear();
        comboProv.Items.AddProvsAbbr(Providers.GetProvsForClinic(comboClinic.ClinicNumSelected));
        comboProv.SetSelectedProvNum(provNum);
    }

    private void FillProcedure()
    {
        if (_adjustment.ProcNum == 0)
        {
            textProcDate2.Text = "";
            textProcProv.Text = "";
            textProcTooth.Text = "";
            textProcDescription.Text = "";
            textProcFee.Text = "";
            textProcWriteoff.Text = "";
            textProcInsPaid.Text = "";
            textProcInsEst.Text = "";
            textProcAdj.Text = "";
            textProcPatPaid.Text = "";
            textProcAdjCur.Text = "";
            labelProcRemain.Text = "";
            _adjRemAmt = 0;
            return;
        }

        var procedure = Procedures.GetOneProc(_adjustment.ProcNum, false);
        var claimProcs = ClaimProcs.Refresh(procedure.PatNum);
        var adjustments = Adjustments.Refresh(procedure.PatNum).Where(x => x.ProcNum == procedure.ProcNum && x.AdjNum != _adjustment.AdjNum).ToList();

        textProcDate.Text = procedure.ProcDate.ToShortDateString();
        textProcDate2.Text = procedure.ProcDate.ToShortDateString();
        textProcProv.Text = Providers.GetAbbr(procedure.ProvNum);
        textProcTooth.Text = Tooth.Display(procedure.ToothNum);
        textProcDescription.Text = ProcedureCodes.GetProcCode(procedure.CodeNum).Descript;

        var procWo = -ClaimProcs.ProcWriteoff(claimProcs, procedure.ProcNum);
        var procInsPaid = -ClaimProcs.ProcInsPay(claimProcs, procedure.ProcNum);
        var procInsEst = -ClaimProcs.ProcEstNotReceived(claimProcs, procedure.ProcNum);
        var procAdj = adjustments.Sum(x => x.AdjAmt);
        var procPatPaid = -PaySplits.GetTotForProc(procedure);

        textProcFee.Text = procedure.ProcFeeTotal.ToString("F");
        textProcWriteoff.Text = procWo == 0 ? "" : procWo.ToString("F");
        textProcInsPaid.Text = procInsPaid == 0 ? "" : procInsPaid.ToString("F");
        textProcInsEst.Text = procInsEst == 0 ? "" : procInsEst.ToString("F");
        textProcAdj.Text = procAdj == 0 ? "" : procAdj.ToString("F");
        textProcPatPaid.Text = procPatPaid == 0 ? "" : procPatPaid.ToString("F");

        var patPort = ClaimProcs.GetPatPortion(procedure, claimProcs, adjustments);
        double procAdjCur = 0;

        if (textAmount.IsValid())
        {
            if (listTypePos.SelectedIndex > -1)
            {
                procAdjCur = SIn.Double(textAmount.Text);
            }
            else if (listTypeNeg.SelectedIndex > -1 || Defs.GetValue(DefCat.AdjTypes, _adjustment.AdjType) == "dp")
            {
                procAdjCur = -SIn.Double(textAmount.Text);
            }
        }

        _isNegativeAdjustment = procAdjCur < 0;

        textProcAdjCur.Text = procAdjCur == 0 ? "" : procAdjCur.ToString("F");

        _adjRemAmt = (decimal) procAdjCur + (decimal) procPatPaid + patPort;

        labelProcRemain.Text = _adjRemAmt.ToString("c");
    }

    private void ButtonAttachProc_Click(object sender, EventArgs e)
    {
        using var formProcSelect = new FormProcSelect(_adjustment.PatNum, doShowTreatmentPlanProcs: false);

        if (formProcSelect.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (OrthoProcLinks.IsProcLinked(formProcSelect.ListProceduresSelected[0].ProcNum))
        {
            ShowError("Adjustments cannot be attached to a procedure that is linked to an ortho case.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.ProcCompleteAddAdj, Procedures.GetDateForPermCheck(formProcSelect.ListProceduresSelected[0])))
        {
            return;
        }

        if (PrefC.GetInt(PrefName.RigorousAdjustments) < 2)
        {
            comboClinic.ClinicNumSelected = formProcSelect.ListProceduresSelected[0].ClinicNum;
            comboProv.SetSelectedProvNum(formProcSelect.ListProceduresSelected[0].ProvNum);

            if (PrefC.GetInt(PrefName.RigorousAdjustments) == (int) RigorousAdjustments.EnforceFully && !_isEditAnyway)
            {
                if (Security.IsAuthorized(EnumPermType.Setup, true))
                {
                    labelEditAnyway.Visible = true;
                    butEditAnyway.Visible = true;
                }

                comboProv.Enabled = false;
                butPickProv.Enabled = false;
                comboClinic.Enabled = false;
            }
        }

        _adjustment.ProcNum = formProcSelect.ListProceduresSelected[0].ProcNum;

        FillProcedure();

        textProcDate.Text = formProcSelect.ListProceduresSelected[0].ProcDate.ToShortDateString();
        textProcDate.ReadOnly = true;
    }

    private void ButtonDetachProc_Click(object sender, EventArgs e)
    {
        textProcDate.ReadOnly = false;
        comboProv.Enabled = true;
        butPickProv.Enabled = true;
        comboClinic.Enabled = true;
        labelEditAnyway.Visible = false;
        butEditAnyway.Visible = false;

        _adjustment.ProcNum = 0;

        FillProcedure();
    }

    private void ButtonEditAnyway_Click(object sender, EventArgs e)
    {
        _isEditAnyway = true;

        comboClinic.Enabled = true;
        comboProv.Enabled = true;
        butPickProv.Enabled = true;
        labelEditAnyway.Visible = false;
        butEditAnyway.Visible = false;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textAdjDate.IsValid() || !textProcDate.IsValid() || !textAmount.IsValid())
        {
            ShowError("Please fix data entry error first.");
            return;
        }

        if (Security.IsGlobalDateLock(EnumPermType.AdjustmentEdit, textAdjDate.Value))
        {
            return;
        }

        if (textAmount.Value != 0 || !Security.IsAuthorized(EnumPermType.AdjustmentEditZero))
        {
            if (IsNew && !Security.IsAuthorized(EnumPermType.AdjustmentCreate, textAdjDate.Value, false))
            {
                return;
            }
        }

        var isDiscountPlanAdj = Defs.GetValue(DefCat.AdjTypes, _adjustment.AdjType) == "dp";
        if (SIn.Date(textAdjDate.Text).Date > DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed))
        {
            ShowError("Adjustment date can not be in the future.");
            return;
        }

        if (textAmount.Text == "")
        {
            ShowError("Please enter an amount.");
            return;
        }

        if (!isDiscountPlanAdj && listTypeNeg.SelectedIndex == -1 && listTypePos.SelectedIndex == -1)
        {
            ShowError("Please select a type first.");
            return;
        }

        if (PrefC.GetInt(PrefName.RigorousAdjustments) == 0 && _adjustment.ProcNum == 0)
        {
            ShowError("You must attach a procedure to the adjustment.");
            return;
        }

        var payPlanLink = PayPlanLinks.GetForFKeyAndLinkType(_adjustment.AdjNum, PayPlanLinkType.Adjustment).FirstOrDefault();

        var isDynamic = false;
        if (payPlanLink != null)
        {
            var payPlan = PayPlans.GetOne(payPlanLink.PayPlanNum);
            
            isDynamic = payPlan.IsDynamic;
        }

        if (isDynamic)
        {
            var value = SIn.Double(textAmount.Text);
            if (listTypeNeg.SelectedIndex != -1)
            {
                value *= -1;
            }

            if (value < 0)
            {
                ShowError("This adjustment is attached to a payment plan and it cannot be negative.");
                return;
            }
        }

        if (_adjRemAmt < 0 && _isNegativeAdjustment)
        {
            var enumAdjustmentBlockOrWarn = PrefC.GetEnum<EnumAdjustmentBlockOrWarn>(PrefName.AdjustmentBlockNegativeExceedingPatPortion);
            switch (enumAdjustmentBlockOrWarn)
            {
                case EnumAdjustmentBlockOrWarn.Warn when !ConfirmOk("Remaining amount is negative.  Continue?"):
                    return;

                case EnumAdjustmentBlockOrWarn.Block:
                    ShowError("Cannot create a negative adjustment exceeding the remaining amount on the procedure.");
                    return;
            }
        }

        var changeAdjSplit = false;
        var paySplitsForAdjust = new List<PaySplit>();

        if (IsNew)
        {
            if (!Security.IsAuthorized(EnumPermType.AdjustmentCreate, SIn.Date(textAdjDate.Text), true))
            {
                if (!_checkZeroAmount)
                {
                    ShowError("Not authorized for\r\n" + GroupPermissions.GetDesc(EnumPermType.AdjustmentCreate));
                    return;
                }
            }
        }
        else
        {
            if (!Security.IsAuthorized(EnumPermType.AdjustmentEdit, SIn.Date(textAdjDate.Text)))
            {
                return;
            }

            if (_adjustment.ProvNum != comboProv.GetSelectedProvNum())
            {
                paySplitsForAdjust = PaySplits.GetForAdjustments([_adjustment.AdjNum]);
                foreach (var paySplit in paySplitsForAdjust)
                {
                    if (!Security.IsAuthorized(EnumPermType.PaymentEdit, Payments.GetPayment(paySplit.PayNum).PayDate))
                    {
                        return;
                    }

                    if (comboProv.GetSelectedProvNum() == paySplit.ProvNum || PrefC.GetInt(PrefName.RigorousAccounting) != (int) RigorousAdjustments.EnforceFully)
                    {
                        continue;
                    }

                    changeAdjSplit = true;
                    break;
                }

                if (changeAdjSplit && !ConfirmOk("The provider for the associated payment splits will be changed to match the provider on the adjustment."))
                {
                    return;
                }
            }
        }

        var datePreviousChange = _adjustment.SecDateTEdit;

        _adjustment.AdjDate = SIn.Date(textAdjDate.Text);
        _adjustment.ProcDate = SIn.Date(textProcDate.Text);
        _adjustment.ProvNum = comboProv.GetSelectedProvNum();
        _adjustment.ClinicNum = comboClinic.ClinicNumSelected;

        if (listTypePos.SelectedIndex != -1)
        {
            _adjustment.AdjType = _listDefsAdjPosCats[listTypePos.SelectedIndex].DefNum;
            _adjustment.AdjAmt = SIn.Double(textAmount.Text);
        }

        if (listTypeNeg.SelectedIndex != -1)
        {
            _adjustment.AdjType = _listDefsAdjNegCats[listTypeNeg.SelectedIndex].DefNum;
            _adjustment.AdjAmt = -SIn.Double(textAmount.Text);
        }

        if (isDiscountPlanAdj)
        {
            _adjustment.AdjAmt = -SIn.Double(textAmount.Text);
        }

        if (_checkZeroAmount && _adjustment.AdjAmt != 0)
        {
            ShowError("Amount has to be 0.00 due to security permission.");
            return;
        }

        if (_program.Enabled && Patients.IsGuarCollections(_patientGuar.PatNum) && _tsiExcludedAdjDefNums.Any(x => x > 0))
        {
            switch (checkOnlyTsiExcludedAdjTypes.Checked)
            {
                case true when !ConfirmOk(
                    "The guarantor of this family has been sent to TSI for a past due balance and you have selected an adjustment type that is excluded from being synched with TSI. " +
                    "This will not reduce the balance sent for collection by TSI. Continue?"):

                case false when !ConfirmOk(
                    "The guarantor of this family has been sent to TSI for a past due balance and you have selected an adjustment type that will be synched with TSI. " +
                    "This balance adjustment could result in a TSI charge for collection. Continue?"):
                    return;
            }
        }

        _adjustment.AdjNote = textNote.Text;
        if (IsNew)
        {
            try
            {
                Adjustments.Insert(_adjustment);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }

            SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentCreate, _adjustment.PatNum, _patient.GetNameLF() + ", " + _adjustment.AdjAmt.ToString("c"));
            TsiTransLogs.CheckAndInsertLogsIfAdjTypeExcluded(_adjustment, _isTsiAdj);
        }
        else
        {
            try
            {
                Adjustments.Update(_adjustment);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);

                return;
            }

            SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentEdit, _adjustment.PatNum, _patient.GetNameLF() + ", " + _adjustment.AdjAmt.ToString("c"), 0, datePreviousChange);
        }

        if (changeAdjSplit)
        {
            PaySplits.UpdateProvForAdjust(_adjustment, paySplitsForAdjust);
        }

        Signalods.SetInvalid(InvalidType.BillingList);

        DialogResult = DialogResult.OK;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (_paySplitsForAdjustment.Count > 0)
        {
            ShowError("Cannot delete adjustment while a payment split is attached.");
            return;
        }

        var isAttachedToPayPlan = PayPlanLinks.GetForFKeyAndLinkType(_adjustment.AdjNum, PayPlanLinkType.Adjustment).Count > 0;
        if (isAttachedToPayPlan)
        {
            ShowError("Cannot delete adjustment that is attached to a payment plan.");
            return;
        }

        if (_paySplitsForAdjustment.Count > 0 && !Confirm("There are payment splits associated to this adjustment. Do you want to continue deleting?"))
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.AdjustmentEdit, _adjustment.PatNum, "Delete for patient: " + _patient.GetNameLF() + ", " + _adjustment.AdjAmt.ToString("c"), 0, _adjustment.SecDateTEdit);

        Adjustments.Delete(_adjustment);

        Signalods.SetInvalid(InvalidType.BillingList);

        DialogResult = DialogResult.OK;
    }

    private void FillListBoxAdjTypes()
    {
        listTypePos.Items.Clear();
        listTypeNeg.Items.Clear();

        listTypeNeg.SelectedIndexChanged -= ListBoxTypeNeg_SelectedIndexChanged;
        listTypePos.SelectedIndexChanged -= ListBoxTypePos_SelectedIndexChanged;

        _listDefsAdjPosCats = Defs.GetPositiveAdjTypes(considerPermission: true);
        _listDefsAdjPosCats = checkOnlyTsiExcludedAdjTypes.Checked
            ? _listDefsAdjPosCats.FindAll(x => _tsiExcludedAdjDefNums.Contains(x.DefNum))
            : _listDefsAdjPosCats.FindAll(x => !_tsiExcludedAdjDefNums.Contains(x.DefNum));

        _listDefsAdjPosCats.ForEach(x => listTypePos.Items.Add(x.ItemName));

        listTypePos.SelectedIndex = _listDefsAdjPosCats.FindIndex(x => x.DefNum == _adjustment.AdjType);

        _listDefsAdjNegCats = Defs.GetNegativeAdjTypes(considerPermission: true);
        _listDefsAdjNegCats = checkOnlyTsiExcludedAdjTypes.Checked
            ? _listDefsAdjNegCats.FindAll(x => _tsiExcludedAdjDefNums.Contains(x.DefNum))
            : _listDefsAdjNegCats.FindAll(x => !_tsiExcludedAdjDefNums.Contains(x.DefNum));

        _listDefsAdjNegCats.ForEach(x => listTypeNeg.Items.Add(x.ItemName));

        listTypeNeg.SelectedIndex = _listDefsAdjNegCats.FindIndex(x => x.DefNum == _adjustment.AdjType);
        listTypeNeg.SelectedIndexChanged += ListBoxTypeNeg_SelectedIndexChanged;
        listTypePos.SelectedIndexChanged += ListBoxTypePos_SelectedIndexChanged;
    }

    private void CheckBoxOnlyTsiExcludedAdjTypes_Checked(object sender, EventArgs e)
    {
        FillListBoxAdjTypes();
    }
}