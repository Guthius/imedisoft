using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormPaymentPlanOptions : FormODBase
{
    public FormPaymentPlanOptions(PaymentSchedule paymentSchedule)
    {
        InitializeComponent();

        switch (paymentSchedule)
        {
            case PaymentSchedule.Weekly:
                radioWeekly.Checked = true;
                break;
            
            case PaymentSchedule.BiWeekly:
                radioEveryOtherWeek.Checked = true;
                break;
            
            case PaymentSchedule.MonthlyDayOfWeek:
                radioOrdinalWeekday.Checked = true;
                break;
            
            case PaymentSchedule.Monthly:
                radioMonthly.Checked = true;
                break;
            
            default:
                radioQuarterly.Checked = true;
                break;
        }
    }
}