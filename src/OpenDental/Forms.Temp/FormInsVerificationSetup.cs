using System;
using System.Linq;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormInsVerificationSetup:FormODBase {
	private bool _hasChanged;

	public FormInsVerificationSetup() {
		InitializeComponent();
	}

	private void FormInsVerificationSetup_Load(object sender,EventArgs e) {
		textInsBenefitEligibilityDaysStandard.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyBenefitEligibilityDays));
		textPatientEnrollmentDaysStandard.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyPatientEnrollmentDays));
		textScheduledAppointmentDaysStandard.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyAppointmentScheduledDays));
		textPastDueDaysStandard.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyDaysFromPastDueAppt));
		textInsBenefitEligibilityDaysMedicaid.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyBenefitEligibilityDaysMedicaid));
		textPatientEnrollmentDaysMedicaid.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyPatientEnrollmentDaysMedicaid));
		textScheduledAppointmentDaysMedicaid.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyAppointmentScheduledDaysMedicaid));
		textPastDueDaysMedicaid.Text=SOut.Int(PrefC.GetInt(PrefName.InsVerifyDaysFromPastDueApptMedicaid));
		checkInsVerifyUseCurrentUser.Checked=PrefC.GetBool(PrefName.InsVerifyDefaultToCurrentUser);
		checkInsVerifyExcludePatVerify.Checked=PrefC.GetBool(PrefName.InsVerifyExcludePatVerify);
		checkFutureDateBenefitYear.Checked=PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear);
		checkFutureDatePatEnrollmentYear.Checked=PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear);
		var listInsVerifyMedicaidFilingCodes=PrefC.GetString(PrefName.InsVerifyMedicaidFilingCodes).Split(",",StringSplitOptions.RemoveEmptyEntries).ToList();
		//Convert InsVerifyMedicaidFilingCodes pref into a list of longs
		var listInsVerifyMedicaidFilingCodeNums=listInsVerifyMedicaidFilingCodes.Select(x => SIn.Long(x,throwExceptions:false)).ToList();
		//Add each filing code from the DB to our listBox
		var listInsFilingCodes=InsFilingCodes.GetAll();
		listBoxInsFilingCodes.Items.AddList(listInsFilingCodes,x=>x.Descript);
		//Set the ones from the preference as selected in the listBox.
		for(var i=0;i<listBoxInsFilingCodes.Items.Count;i++) {
			var insFilingCode=(InsFilingCode)listBoxInsFilingCodes.Items.GetObjectAt(i);
			if(listInsVerifyMedicaidFilingCodeNums.Contains(insFilingCode.InsFilingCodeNum)){
				listBoxInsFilingCodes.SelectedIndices.Add(i);
			}
		}
		if(!PrefC.GetBool(PrefName.ShowFeaturePatientClone)) {
			checkExcludePatientClones.Visible=false;
		}
		else {
			checkExcludePatientClones.Checked=PrefC.GetBool(PrefName.InsVerifyExcludePatientClones);
		}
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(!textInsBenefitEligibilityDaysStandard.IsValid()) {
			MsgBox.Show(this,"The number entered for standard insurance benefit eligibility was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textPatientEnrollmentDaysStandard.IsValid()) {
			MsgBox.Show(this,"The number entered for standard patient enrollment was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textScheduledAppointmentDaysStandard.IsValid()) {
			MsgBox.Show(this,"The number entered for standard scheduled appointments was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textPastDueDaysStandard.IsValid()) {
			MsgBox.Show(this,"The number entered for standard appointment days past due was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textInsBenefitEligibilityDaysMedicaid.IsValid()) {
			MsgBox.Show(this,"The number entered for medicaid insurance benefit eligibility was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textPatientEnrollmentDaysMedicaid.IsValid()) {
			MsgBox.Show(this,"The number entered for medicaid patient enrollment was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textScheduledAppointmentDaysMedicaid.IsValid()) {
			MsgBox.Show(this,"The number entered for medicaid scheduled appointments was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		if(!textPastDueDaysMedicaid.IsValid()) {
			MsgBox.Show(this,"The number entered for medicaid appointment days past due was not a valid number.  Please enter a valid number to continue.");
			return;
		}
		var insBenefitEligibilityDaysStandard=SIn.Int(textInsBenefitEligibilityDaysStandard.Text);
		var patientEnrollmentDaysStandard=SIn.Int(textPatientEnrollmentDaysStandard.Text);
		var scheduledAppointmentDaysStandard=SIn.Int(textScheduledAppointmentDaysStandard.Text);
		var pastDueDaysStandard=SIn.Int(textPastDueDaysStandard.Text);
		var insBenefitEligibilityDaysMedicaid=SIn.Int(textInsBenefitEligibilityDaysMedicaid.Text);
		var patientEnrollmentDaysMedicaid=SIn.Int(textPatientEnrollmentDaysMedicaid.Text);
		var scheduledAppointmentDaysMedicaid=SIn.Int(textScheduledAppointmentDaysMedicaid.Text);
		var pastDueDaysMedicaid=SIn.Int(textPastDueDaysMedicaid.Text);
		var listInsFilingCodeNums=listBoxInsFilingCodes.GetListSelected<InsFilingCode>().Select(x=>x.InsFilingCodeNum).ToList();
		var insVerifyMedicaidFilingCodes=string.Join(",",listInsFilingCodeNums);
		if(Prefs.UpdateInt(PrefName.InsVerifyBenefitEligibilityDays,insBenefitEligibilityDaysStandard)
		   | Prefs.UpdateInt(PrefName.InsVerifyPatientEnrollmentDays,patientEnrollmentDaysStandard)
		   | Prefs.UpdateInt(PrefName.InsVerifyAppointmentScheduledDays,scheduledAppointmentDaysStandard)
		   | Prefs.UpdateInt(PrefName.InsVerifyDaysFromPastDueAppt,pastDueDaysStandard)
		   | Prefs.UpdateInt(PrefName.InsVerifyBenefitEligibilityDaysMedicaid,insBenefitEligibilityDaysMedicaid)
		   | Prefs.UpdateInt(PrefName.InsVerifyPatientEnrollmentDaysMedicaid,patientEnrollmentDaysMedicaid)
		   | Prefs.UpdateInt(PrefName.InsVerifyAppointmentScheduledDaysMedicaid,scheduledAppointmentDaysMedicaid)
		   | Prefs.UpdateInt(PrefName.InsVerifyDaysFromPastDueApptMedicaid,pastDueDaysMedicaid)
		   | Prefs.UpdateString(PrefName.InsVerifyMedicaidFilingCodes,insVerifyMedicaidFilingCodes)
		   | Prefs.UpdateBool(PrefName.InsVerifyExcludePatVerify,checkInsVerifyExcludePatVerify.Checked)
		   | Prefs.UpdateBool(PrefName.InsVerifyFutureDateBenefitYear,checkFutureDateBenefitYear.Checked)
		   | Prefs.UpdateBool(PrefName.InsVerifyFutureDatePatEnrollmentYear,checkFutureDatePatEnrollmentYear.Checked)
		   | Prefs.UpdateBool(PrefName.InsVerifyExcludePatientClones,checkExcludePatientClones.Checked)
		   | Prefs.UpdateBool(PrefName.InsVerifyDefaultToCurrentUser,checkInsVerifyUseCurrentUser.Checked)) 
		{
			_hasChanged=true;
		}
		DialogResult=DialogResult.OK;
	}

	private void FormInsVerificationSetup_FormClosing(object sender,FormClosingEventArgs e) {
		if(_hasChanged) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
	}

}