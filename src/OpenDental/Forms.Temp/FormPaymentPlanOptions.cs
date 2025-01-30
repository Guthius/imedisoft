using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormPaymentPlanOptions:FormODBase {

	public FormPaymentPlanOptions(PaymentSchedule paymentSchedule) {
		InitializeComponent();

		if(paymentSchedule==PaymentSchedule.Weekly) {
			radioWeekly.Checked=true;
		}
		else if(paymentSchedule==PaymentSchedule.BiWeekly) {
			radioEveryOtherWeek.Checked=true;
		}
		else if(paymentSchedule==PaymentSchedule.MonthlyDayOfWeek) {
			radioOrdinalWeekday.Checked=true;
		}
		else if(paymentSchedule==PaymentSchedule.Monthly) {
			radioMonthly.Checked=true;
		}
		else {//quarterly
			radioQuarterly.Checked=true;
		}
	}

}