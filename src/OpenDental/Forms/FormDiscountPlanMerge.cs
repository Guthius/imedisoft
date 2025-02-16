using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDiscountPlanMerge : FormODBase
{
    private DiscountPlan _discountPlanInto;
    private DiscountPlan _discountPlanFrom;

    public FormDiscountPlanMerge()
    {
        InitializeComponent();
    }

    private void ButtonChangePlanInto_Click(object sender, EventArgs e)
    {
        using var formDiscountPlans = new FormDiscountPlans();
        
        formDiscountPlans.IsSelectionMode = true;
        
        if (formDiscountPlans.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _discountPlanInto = formDiscountPlans.SelectedDiscountPlan;
        
        textDescriptionInto.Text = _discountPlanInto.Description;
        textFeeSchedInto.Text = FeeScheds.GetDescription(_discountPlanInto.FeeSchedNum);
        textAdjTypeInto.Text = Defs.GetName(DefCat.AdjTypes, _discountPlanInto.DefNum);
        
        UpdateButtonState();
    }

    private void ButtonChangePlanFrom_Click(object sender, EventArgs e)
    {
        using var formDiscountPlans = new FormDiscountPlans();
        
        formDiscountPlans.IsSelectionMode = true;
        
        if (formDiscountPlans.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _discountPlanFrom = formDiscountPlans.SelectedDiscountPlan;
        
        textDescriptionFrom.Text = _discountPlanFrom.Description;
        textFeeSchedFrom.Text = FeeScheds.GetDescription(_discountPlanFrom.FeeSchedNum);
        textAdjTypeFrom.Text = Defs.GetName(DefCat.AdjTypes, _discountPlanFrom.DefNum);
        
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        butMerge.Enabled = _discountPlanInto is not null && _discountPlanFrom is not null;
    }

    private void ButtonMerge_Click(object sender, EventArgs e)
    {
        if (_discountPlanFrom.DiscountPlanNum == _discountPlanInto.DiscountPlanNum)
        {
            ShowError("You must select two different Discount Plans to merge.");
            return;
        }

        if (!Confirm("Merge the Discount Plan at the bottom into the Discount Plan shown at the top?"))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;
        
        DiscountPlans.MergeTwoPlans(_discountPlanInto, _discountPlanFrom);
        
        Cursor = Cursors.Default;
        
        SecurityLogs.MakeLogEntry(EnumPermType.DiscountPlanMerge, 0, $"{_discountPlanFrom.Description} merged into {_discountPlanInto.Description}");
        
        ShowInfo("Plans merged successfully.");
        
        _discountPlanFrom = null;
        
        textDescriptionFrom.Text = "";
        textFeeSchedFrom.Text = "";
        textAdjTypeFrom.Text = "";
        
        UpdateButtonState();
    }
}