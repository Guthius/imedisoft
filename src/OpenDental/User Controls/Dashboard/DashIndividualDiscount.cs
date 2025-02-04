using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

///<summary>Doesn't look like it's actually possible to use from dashboard.</summary>
public partial class DashIndividualDiscount : UserControl, IDashWidgetField
{
    ///<summary>The subscriber for the discount plan. Can be an newly instantiated sub with no valid values if no discount plan.</summary>
    private DiscountPlanSub _discountPlanSub;

    ///<summary>The discount plan for the patient. Can be an newly instantiated plan with no valid values if the patient does not have a discount plan.</summary>
    private DiscountPlan _discountPlan;

    ///<summary>The total of all discount plan adjustments for the current date range segment for DateTime.Now.</summary>
    private double _discountAmtUsed;

    public DashIndividualDiscount()
    {
        InitializeComponent();
    }

    public void RefreshDiscountPlan(Patient pat, DiscountPlanSub discountPlanSub, DiscountPlan discountPlan)
    {
        _discountPlanSub = discountPlanSub;
        _discountPlan = discountPlan;
        _discountAmtUsed = 0;
        groupBoxIndDiscount.Text = Lan.g(this, "Discount Plan");
        label11.Text = Lan.g(this, "Annual Max");
        textMaxAdj.Text = "";
        label12.Text = Lan.g(this, "Adj Used");
        textUsedAdj.Text = "";
        label18.Text = Lan.g(this, "Adj Remaining");
        textRemainingAdj.Text = "";
        if (pat == null || _discountPlanSub == null || _discountPlan == null)
        {
            return;
        }

        var dateEffective = DiscountPlanSubs.GetAnnualMaxDateEffective(_discountPlanSub.DateEffective);
        var dateTerm = DiscountPlanSubs.GetAnnualMaxDateTerm(_discountPlanSub.DateTerm);
        if (DateTime.Now < dateEffective || DateTime.Now > dateTerm)
        {
            return;
        }

        var dateEffectiveFinal = DiscountPlanSubs.GetDateEffectiveForAnnualDateRangeSegment(DateTime.Now, dateEffective, dateTerm);
        var dateTermFinal = DiscountPlanSubs.GetDateTermForAnnualDateRangeSegment(DateTime.Now, dateEffective, dateTerm);
        _discountAmtUsed = Adjustments.GetTotForPatByType(_discountPlanSub.PatNum, _discountPlan.DefNum, dateEffectiveFinal, dateTermFinal);
        if (_discountPlan.AnnualMax != -1)
        {
            var adjMax = _discountPlan.AnnualMax;
            textMaxAdj.Text = adjMax.ToString("F");
            var adjRem = adjMax - _discountAmtUsed;
            textRemainingAdj.Text = adjRem.ToString("F");
        }

        textUsedAdj.Text = _discountAmtUsed.ToString("F");
    }
}