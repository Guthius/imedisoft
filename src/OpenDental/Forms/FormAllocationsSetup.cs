using System;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAllocationsSetup : FormODBase
{
    private bool _changed;
    private YN _prePayAllowedForTpProcs;

    public FormAllocationsSetup()
    {
        InitializeComponent();
    }

    private void FormAllocationsSetup_Load(object sender, EventArgs e)
    {
        if (Security.IsAuthorized(EnumPermType.Setup))
        {
            labelPermission.Visible = false;
        }
        else
        {
            butOK.Enabled = false;
        }

        var rigorousAccounting = (RigorousAccounting) PrefC.GetInt(PrefName.RigorousAccounting);
        switch (rigorousAccounting)
        {
            case RigorousAccounting.EnforceFully:
                radioPayEnforce.Checked = true;
                break;
            case RigorousAccounting.AutoSplitOnly:
                radioPayAuto.Checked = true;
                break;
            case RigorousAccounting.DontEnforce:
                radioPayDont.Checked = true;
                break;
        }

        var rigorousAdjustments = (RigorousAdjustments) PrefC.GetInt(PrefName.RigorousAdjustments);
        switch (rigorousAdjustments)
        {
            case RigorousAdjustments.EnforceFully:
                radioAdjustEnforce.Checked = true;
                break;
            case RigorousAdjustments.LinkOnly:
                radioAdjustLink.Checked = true;
                break;
            case RigorousAdjustments.DontEnforce:
                radioAdjustDont.Checked = true;
                break;
        }

        checkHidePaysplits.Checked = PrefC.GetBool(PrefName.PaymentWindowDefaultHideSplits);
        checkShowIncomeTransferManager.Checked = PrefC.GetBool(PrefName.ShowIncomeTransferManager);
        checkClaimPayByTotalSplitsAuto.Checked = PrefC.GetBool(PrefName.ClaimPayByTotalSplitsAuto);
        checkAdjustmentsOffset.Checked = PrefC.GetBool(PrefName.AdjustmentsOffsetEachOther);

        _prePayAllowedForTpProcs = PrefC.GetEnum<YN>(PrefName.PrePayAllowedForTpProcs);

        var autoTransferOnClaimReceive = PrefC.GetEnum<YN>(PrefName.IncomeTransfersMadeUponClaimReceived);

        checkIncomeTransfersMadeUponClaimReceived.CheckState = autoTransferOnClaimReceive switch
        {
            YN.Unknown => CheckState.Indeterminate,
            YN.Yes => CheckState.Checked,
            YN.No => CheckState.Unchecked,
            _ => checkIncomeTransfersMadeUponClaimReceived.CheckState
        };

        SetIncomeTransfersMadeUponClaimReceivedDesc();

        checkAllowPrePayToTpProcs.Checked = PrefC.GetYn(PrefName.PrePayAllowedForTpProcs);
        checkIsRefundable.Checked = PrefC.GetBool(PrefName.TpPrePayIsNonRefundable);
        checkIsRefundable.Visible = checkAllowPrePayToTpProcs.Checked; //pref will be unchecked if parent gets turned off.
        labelRefundable.Visible = checkAllowPrePayToTpProcs.Checked;
        comboTpUnearnedType.Items.AddDefs(Defs.GetDefsForCategory(DefCat.PaySplitUnearnedType, true));
        comboTpUnearnedType.SetSelectedDefNum(PrefC.GetLong(PrefName.TpUnearnedType));
    }

    private void SetIncomeTransfersMadeUponClaimReceivedDesc()
    {
        labelIncomeTransfersMadeUponClaimReceivedDesc.Text = checkIncomeTransfersMadeUponClaimReceived.CheckState switch
        {
            CheckState.Checked => "Automatically transfer patient overpayment when necessary.",
            CheckState.Unchecked => "Never make transfers automatically.",
            CheckState.Indeterminate => "Only transfer patient overpayment if Paysplits - Rigorous is enabled.",
            _ => labelIncomeTransfersMadeUponClaimReceivedDesc.Text
        };
    }

    private void ButtonLineItem_Click(object sender, EventArgs e)
    {
        radioPayEnforce.Checked = true;
        radioAdjustEnforce.Checked = true;
        checkAllowPrePayToTpProcs.Checked = false;
        checkIsRefundable.Checked = false;
        checkIsRefundable.Visible = false;
        labelRefundable.Visible = false;
        checkHidePaysplits.Checked = false;
        checkShowIncomeTransferManager.Checked = true;
        checkClaimPayByTotalSplitsAuto.Checked = true;
        checkAdjustmentsOffset.Checked = true;
    }

    private void ButtonDefault_Click(object sender, EventArgs e)
    {
        radioPayAuto.Checked = true;
        radioAdjustLink.Checked = true;
        checkAllowPrePayToTpProcs.Checked = false;
        checkIsRefundable.Checked = false;
        checkIsRefundable.Visible = false;
        labelRefundable.Visible = false;
        checkHidePaysplits.Checked = false;
        checkShowIncomeTransferManager.Checked = true;
        checkClaimPayByTotalSplitsAuto.Checked = true;
        checkAdjustmentsOffset.Checked = true;
    }

    private void ButtonSimple_Click(object sender, EventArgs e)
    {
        radioPayDont.Checked = true;
        radioAdjustDont.Checked = true;
        checkAllowPrePayToTpProcs.Checked = false;
        checkIsRefundable.Checked = false;
        checkIsRefundable.Visible = false;
        labelRefundable.Visible = false;
        checkHidePaysplits.Checked = false;
        checkShowIncomeTransferManager.Checked = false;
        checkClaimPayByTotalSplitsAuto.Checked = true;
        checkAdjustmentsOffset.Checked = true;
    }

    private void CheckBoxAllowPrePayToTpProcs_Click(object sender, EventArgs e)
    {
        if (checkAllowPrePayToTpProcs.Checked)
        {
            checkIsRefundable.Visible = true;
            checkIsRefundable.Checked = PrefC.GetBool(PrefName.TpPrePayIsNonRefundable);
            labelRefundable.Visible = true;
            _prePayAllowedForTpProcs = YN.Yes;
        }
        else
        {
            checkIsRefundable.Visible = false;
            checkIsRefundable.Checked = false;
            labelRefundable.Visible = false;
            _prePayAllowedForTpProcs = YN.No;
        }
    }

    private void CheckBoxAutoIncomeTransfer_CheckedStateChanged(object sender, EventArgs e)
    {
        SetIncomeTransfersMadeUponClaimReceivedDesc();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var rigorousAccounting = RigorousAccounting.EnforceFully;
        if (radioPayAuto.Checked)
        {
            rigorousAccounting = RigorousAccounting.AutoSplitOnly;
        }

        if (radioPayDont.Checked)
        {
            rigorousAccounting = RigorousAccounting.DontEnforce;
        }

        var prefRigorousAccounting = PrefC.GetInt(PrefName.RigorousAccounting);
        if (Prefs.UpdateInt(PrefName.RigorousAccounting, (int) rigorousAccounting))
        {
            _changed = true;
            SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0,
                "Rigorous accounting changed from " +
                ((RigorousAccounting) prefRigorousAccounting).GetDescription() + " to "
                + rigorousAccounting.GetDescription() + ".");
        }

        var rigorousAdjustments = RigorousAdjustments.EnforceFully;
        if (radioAdjustLink.Checked)
        {
            rigorousAdjustments = RigorousAdjustments.LinkOnly;
        }

        if (radioAdjustDont.Checked)
        {
            rigorousAdjustments = RigorousAdjustments.DontEnforce;
        }

        var prefRigorousAdjustments = PrefC.GetInt(PrefName.RigorousAdjustments);
        if (Prefs.UpdateInt(PrefName.RigorousAdjustments, (int) rigorousAdjustments))
        {
            _changed = true;
            SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0,
                "Rigorous adjustments changed from " +
                ((RigorousAdjustments) prefRigorousAdjustments).GetDescription() + " to "
                + rigorousAdjustments.GetDescription() + ".");
        }

        _changed |= Prefs.UpdateBool(PrefName.PaymentWindowDefaultHideSplits, checkHidePaysplits.Checked);
        _changed |= Prefs.UpdateBool(PrefName.ShowIncomeTransferManager, checkShowIncomeTransferManager.Checked);
        _changed |= Prefs.UpdateBool(PrefName.ClaimPayByTotalSplitsAuto, checkClaimPayByTotalSplitsAuto.Checked);
        _changed |= Prefs.UpdateYN(PrefName.PrePayAllowedForTpProcs, _prePayAllowedForTpProcs);
        _changed |= Prefs.UpdateYN(PrefName.IncomeTransfersMadeUponClaimReceived, checkIncomeTransfersMadeUponClaimReceived.CheckState);
        _changed |= Prefs.UpdateLong(PrefName.TpUnearnedType, comboTpUnearnedType.GetSelectedDefNum());
        _changed |= Prefs.UpdateBool(PrefName.TpPrePayIsNonRefundable, checkIsRefundable.Checked);
        _changed |= Prefs.UpdateBool(PrefName.AdjustmentsOffsetEachOther, checkAdjustmentsOffset.Checked);
        
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Prefs);
            
            SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Auto Codes");
        }

        DialogResult = DialogResult.OK;
    }
}