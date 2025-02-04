using System;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormReferralMerge:FormODBase {
	private long _referralNumInto;
	private long _referralNumFrom;

	public FormReferralMerge() {
		InitializeComponent();
	}

	private void butChangeReferralInto_Click(object sender,EventArgs e) {
		var frmReferralSelect=new FrmReferralSelect();
		frmReferralSelect.IsSelectionMode=true;
		frmReferralSelect.ShowDialog();
		if(frmReferralSelect.IsDialogOK) {
			var referral=frmReferralSelect.ReferralSelected;
			_referralNumInto=referral.ReferralNum;
			textReferralNameInto.Text=referral.LName+", "+referral.FName;
			textTitleInto.Text=referral.Title;
			checkIsPersonInto.Checked=!referral.NotPerson;
			checkIsDoctorInto.Checked=referral.IsDoctor;
			CheckUIState();
		}
	}

	private void butChangeReferralFrom_Click(object sender,EventArgs e) {
		var frmReferralSelect=new FrmReferralSelect();
		frmReferralSelect.IsSelectionMode=true;
		frmReferralSelect.ShowDialog();
		if(frmReferralSelect.IsDialogOK) {
			var referral=frmReferralSelect.ReferralSelected;
			_referralNumFrom=referral.ReferralNum;
			textReferralNameFrom.Text=referral.LName+", "+referral.FName;
			textTitleFrom.Text=referral.Title;
			checkIsPersonFrom.Checked=!referral.NotPerson;
			checkIsDoctorFrom.Checked=referral.IsDoctor;
			CheckUIState();
		}
	}

	private void CheckUIState() {
		butMerge.Enabled=(textReferralNameInto.Text.Trim()!="" && textReferralNameFrom.Text.Trim()!="");
	}

	private void butMerge_Click(object sender,EventArgs e) {
		if(_referralNumInto==_referralNumFrom) {
			MsgBox.Show(this,"Cannot merge the same referral.");
			return;
		}
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Are you sure?  The results are permanent and cannot be undone.")) {
			return;
		}
		var differentFields="";
		if(textReferralNameInto.Text.Trim()!=textReferralNameFrom.Text.Trim()) {
			differentFields+=Lan.g(this,"Referral Name")+"\r\n";
		}
		if(textTitleInto.Text.Trim()!=textTitleFrom.Text.Trim()) {
			differentFields+=Lan.g(this,"Title")+"\r\n";
		}
		if(checkIsPersonInto.Checked!=checkIsPersonFrom.Checked) {
			differentFields+=Lan.g(this,"Is Person")+"\r\n";
		}
		if(checkIsDoctorInto.Checked!=checkIsDoctorFrom.Checked) {
			differentFields+=Lan.g(this,"Is Doctor")+"\r\n";
		}
		var warningMsg="";
		if(differentFields!="") {
			warningMsg+=Lan.g(this,"The following referral fields do not match")+": \r\n"+differentFields;
		}
		var countPatAttach=Referrals.CountReferralAttach(_referralNumFrom);
		warningMsg+=Lan.g(this,"The selected referrals may be different")+".  "+Lan.g(this,"This change is irreversible! The referral is attached to")+" "
		            +countPatAttach+" "+Lan.g(this,"patients")+".  "+Lan.g(this,"Continue anyways?");
		if(ODMessageBox.Show(warningMsg,"",MessageBoxButtons.YesNo)==DialogResult.No) { 
			return;
		}
		if(!Referrals.MergeReferrals(_referralNumInto,_referralNumFrom)) {
			MsgBox.Show(this,"Referrals failed to merge.");
			return;
		}
		MsgBox.Show(this,"Referrals merged successfully.");
		var logText=Lan.g(this,"Referral Merge from")
		            +" "+Referrals.GetNameLF(_referralNumFrom)+" "+Lan.g(this,"to")+" "+Referrals.GetNameLF(_referralNumInto)+"\r\n"
		            +Lan.g(this,"Patients attached to this referral")+": "+countPatAttach;
		//Make log entry here not in parent form because we can merge multiple referrals at a time.
		SecurityLogs.MakeLogEntry(EnumPermType.ReferralMerge,0,logText);
		textReferralNameFrom.Text="";
		textTitleFrom.Text="";
		checkIsPersonFrom.Checked=false;
		checkIsDoctorFrom.Checked=false;
		CheckUIState();
	}

}