using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDental;

/// <summary>
/// Summary description for FormBasicTemplate.
/// </summary>
public partial class FormRepeatChargeEdit :FormODBase{
		
	public bool IsNew;
	private RepeatCharge _repeatCharge;
	private RepeatCharge _repeatChargeOld;
	private bool _isErx;
	
	public FormRepeatChargeEdit(RepeatCharge repeatCur)
	{
		InitializeComponent();

		_repeatCharge=repeatCur;
		_repeatChargeOld=repeatCur.Copy();
	}

	private void FormRepeatChargeEdit_Load(object sender,EventArgs e) {
		SetPatient();
		if(IsNew){
			using var formProcCodes=new FormProcCodes();
			formProcCodes.IsSelectionMode=true;
			formProcCodes.ShowDialog();
			if(formProcCodes.DialogResult!=DialogResult.OK){
				DialogResult=DialogResult.Cancel;
				return;
			}
			var procedureCode=ProcedureCodes.GetProcCode(formProcCodes.CodeNumSelected);
			if(procedureCode.TreatArea!=TreatmentArea.Mouth 
			   && procedureCode.TreatArea!=TreatmentArea.None){
				MsgBox.Show(this,"Procedure codes that require tooth numbers are not allowed.");
				DialogResult=DialogResult.Cancel;
				return;
			}
			_repeatCharge.ProcCode=ProcedureCodes.GetStringProcCode(formProcCodes.CodeNumSelected);
			_repeatCharge.IsEnabled=true;
			_repeatCharge.CreatesClaim=false;
		}
		textCode.Text=_repeatCharge.ProcCode;
		textDesc.Text=ProcedureCodes.GetProcCode(_repeatCharge.ProcCode).Descript;
		textChargeAmt.Text=_repeatCharge.ChargeAmt.ToString("F");
		if(_repeatCharge.DateStart.Year>1880){
			textDateStart.Text=_repeatCharge.DateStart.ToShortDateString();
		}
		if(_repeatCharge.DateStop.Year>1880){
			textDateStop.Text=_repeatCharge.DateStop.ToShortDateString();
		}
		textNote.Text=_repeatCharge.Note;
		_isErx=false;
		checkCopyNoteToProc.Checked=_repeatCharge.CopyNoteToProc;
		checkCreatesClaim.Checked=_repeatCharge.CreatesClaim;
		checkIsEnabled.Checked=_repeatCharge.IsEnabled;
		var patient=Patients.GetPat(_repeatCharge.PatNum);//pat should never be null. If it is, this will fail.
		//If this is a new repeat charge and no other active repeat charges exist, set the billing cycle day to today
		if(IsNew && !RepeatCharges.ActiveRepeatChargeExists(_repeatCharge.PatNum)) {
			textBillingDay.Text=DateTime.Today.Day.ToString();
		}
		else {
			textBillingDay.Text=patient.BillingCycleDay.ToString();
		}
		if(PrefC.GetBool(PrefName.BillingUseBillingCycleDay)) {
			labelBillingCycleDay.Visible=true;
			textBillingDay.Visible=true;
		}
		comboFrequencyTypes.Items.AddEnums<EnumRepeatChargeFrequency>();
		comboFrequencyTypes.SelectedItem=_repeatCharge.Frequency;
		checkUseUnearned.Checked=_repeatCharge.UsePrepay;
		var listDefNumsUnearnedTypeCur=(_repeatCharge.UnearnedTypes??"").Split([','],StringSplitOptions.RemoveEmptyEntries)
			.Select(x => SIn.Long(x,false)).ToList();
		var listDefs=new List<Def>();
		listDefs.AddRange(Defs.GetUnearnedDefs(isShort:true));
		comboUnearnedTypes.IncludeAll=true;
		comboUnearnedTypes.Items.AddDefs(listDefs);
		for(var i=0;i<listDefNumsUnearnedTypeCur.Count;i++) {//There isn't an easy way to set selected multiple defs, so loop through each one that was selected on the RepeatCharge and set it selected if it matches an item from the list of all defs(listUnread)
			for(var j=0;j<comboUnearnedTypes.Items.Count;j++) {
				if(listDefs[j].DefNum==listDefNumsUnearnedTypeCur[i]) {
					comboUnearnedTypes.SetSelected(j,true);
					continue;
				}
			}
		}
		if(string.IsNullOrEmpty(_repeatCharge.UnearnedTypes)) {
			//An empty value indicates 'All'
			comboUnearnedTypes.IsAllSelected=true;
		}
	}

	private void SetPatient() {
		//Set the title bar to show the patient's name much like the main screen does.
		Text+=" - "+Patients.GetLim(_repeatCharge.PatNum).GetNameLF();
		textPatNum.Text=_repeatCharge.PatNum.ToString();
	}

	private void checkUseUnearned_CheckedChanged(object sender,EventArgs e) {
		comboUnearnedTypes.Enabled=checkUseUnearned.Checked;
	}

	private void butManual_Click(object sender,EventArgs e) {
		Prefs.RefreshCache();//Refresh the cache in case another machine has updated this pref
		if(PrefC.GetString(PrefName.RepeatingChargesBeginDateTime)!="") {
			MsgBox.Show(this,"Repeating charges already running on another workstation, you must wait for them to finish before continuing.");
			return;
		}
		if(_repeatCharge.RepeatChargeNum==0) {
			MsgBox.Show(this,"Please click 'OK' to save the repeat charge before adding a manual charge.");
			return;
		}
		double procFee;
		try {
			procFee=double.Parse(textChargeAmt.Text);
		}
		catch {
			MsgBox.Show(this,"Invalid charge amount.");
			return;
		}
		if(!Security.IsAuthorized(EnumPermType.ProcComplCreate,DateTime.Today,ProcedureCodes.GetCodeNum(textCode.Text),procFee)) {
			return;
		}
		var chargeManual=_repeatCharge.Copy();//Update the fields from the form in case the user made changes
		if(!UpdateRepeatCharge(chargeManual)) {
			return;
		}
		Procedures.SetDateFirstVisit(DateTime.Today,1,Patients.GetPat(_repeatCharge.PatNum));
		Procedure procedure;
		var orthoCaseProcedureLinker=OrthoCaseProcedureLinker.CreateOneForPatient(_repeatCharge.PatNum);
		try {
			procedure=RepeatCharges.AddProcForRepeatCharge(chargeManual,DateTime.Today,DateTime.Today,orthoCaseProcedureLinker:orthoCaseProcedureLinker);
		}
		catch(ODException ex) {
			ODMessageBox.Show(ex.Message);
			return;
		}
		if(!string.IsNullOrEmpty(chargeManual.Note)) {
			textNote.Text=chargeManual.Note;
		}
		RepeatCharges.AllocateUnearned(chargeManual,procedure,DateTime.Today);
		Recalls.Synch(_repeatCharge.PatNum);
		MsgBox.Show(this,"Procedure added.");
		Signalods.SetInvalid(InvalidType.BillingList);
	}

	private void butCalculate_Click(object sender,EventArgs e) {
		if(CompareDouble.IsZero(SIn.Double(textNumOfCharges.Text))	|| CompareDouble.IsZero(SIn.Double(textTotalAmount.Text))) {
			textChargeAmt.Text=_repeatCharge.ChargeAmt.ToString("F");
			return;
		}
		textChargeAmt.Text=(SIn.Double(textTotalAmount.Text)/SIn.Double(textNumOfCharges.Text)).ToString("F");
	}

	///<summary>This button is only visible internally and for other distributors.</summary>
	private void butMoveTo_Click(object sender,EventArgs e) {
		if(!Regex.IsMatch(textErxAccountId.Text,"^(DS;)?[0-9]+\\-[a-zA-Z0-9]{5}$")) {
			MsgBox.Show(this,"A valid ErxAccountId is required before moving this eRx repeating charge to another customer.  "
			                 +"The ErxAccountId is typically filled in automatically when running eRx billing.  You can manually enter by "
			                 +"logging into the eRx portal and clicking the Maintain Top-Level Account Kids link, "
			                 +"then locate the customer account in the list and copy the customer Account ID into the ErxAccountId of this repeating charge.");
			return;
		}
		var frmPatientSelect=new FrmPatientSelect();
		frmPatientSelect.ShowDialog();
		if(frmPatientSelect.IsDialogCancel) {
			return;
		}
		_repeatCharge.PatNum=frmPatientSelect.PatNumSelected;
		SetPatient();
		var patient=Patients.GetPat(_repeatCharge.PatNum);
		textBillingDay.Text=patient.BillingCycleDay.ToString();
	}

	private void butDelete_Click(object sender, EventArgs e) {
		var patientOld=Patients.GetPat(_repeatCharge.PatNum);
		RepeatCharges.InsertRepeatChargeChangeSecurityLogEntry(_repeatCharge,EnumPermType.RepeatChargeDelete,patientOld,isAutomated:false);
		RepeatCharges.Delete(_repeatCharge);
		DialogResult=DialogResult.OK;
	}

	///<summary>Updates the repeatCharge with the values entered on the form.</summary>
	private bool UpdateRepeatCharge(RepeatCharge repeatCharge) {
		if(!textChargeAmt.IsValid()
		   || !textDateStart.IsValid()
		   || !textDateStop.IsValid()
		   || !textBillingDay.IsValid()) 
		{
			MsgBox.Show(this,"Please fix data entry errors first.");
			return false;
		}
		if(SIn.Double(textChargeAmt.Text)<0 && checkCreatesClaim.Checked) {//user entered a value less than zero while checkCreatesClaim is checked
			MsgBox.Show(this,"Creates Claim cannot be checked while Charge Amout is less than zero.");
			return false;
		}
		if(textDateStart.Text=="") {
			MsgBox.Show(this,"Start date cannot be left blank.");
			return false;
		}
		if(SIn.Date(textDateStart.Text)!=_repeatCharge.DateStart) {//if the user changed the date
			if(SIn.Date(textDateStart.Text)<DateTime.Today.AddDays(-3)) {//and if the date the user entered is more than three days in the past
				MsgBox.Show(this,"Start date cannot be more than three days in the past.  You should enter previous charges manually in the account.");
				return false;
			}
		}
		if(textDateStop.Text.Trim()!="" && SIn.Date(textDateStart.Text)>SIn.Date(textDateStop.Text)) {
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"The start date is after the stop date.  Continue?")) {
				return false;
			}
		}
		if(_isErx && !Regex.IsMatch(textNpi.Text,"^[0-9]{10}$")) {
			MsgBox.Show(this,"Invalid NPI.  Must be 10 digits.");
			return false;
		}
		var accountId=textErxAccountId.Text;
		if(textErxAccountId.Text.Length>2 && textErxAccountId.Text.Substring(0,3).ToLower()=="ds;") {//support for DoseSpot account Ids
			accountId=textErxAccountId.Text.Substring(3);
		}
		if(_isErx && textErxAccountId.Text!="" && !Regex.IsMatch(accountId,"^[0-9]+\\-[a-zA-Z0-9]{5}$")) {
			MsgBox.Show(this,"Invalid ErxAccountId.");
			return false;
		}
		repeatCharge.ProcCode=textCode.Text;
		repeatCharge.ChargeAmt=SIn.Double(textChargeAmt.Text);
		repeatCharge.Frequency=(EnumRepeatChargeFrequency)comboFrequencyTypes.SelectedItem;
		repeatCharge.DateStart=SIn.Date(textDateStart.Text);
		repeatCharge.DateStop=SIn.Date(textDateStop.Text);
		repeatCharge.Npi=textNpi.Text;
		repeatCharge.ErxAccountId=textErxAccountId.Text;
		repeatCharge.Note=textNote.Text;
		repeatCharge.ProviderName=textProvName.Text;
		repeatCharge.CopyNoteToProc=checkCopyNoteToProc.Checked;
		repeatCharge.IsEnabled=checkIsEnabled.Checked;
		repeatCharge.CreatesClaim=checkCreatesClaim.Checked;
		repeatCharge.UsePrepay=checkUseUnearned.Checked;
		repeatCharge.UnearnedTypes="";//If 'All' is selected. An empty database column indicates all unearned types are to be used.
		if(!comboUnearnedTypes.IsAllSelected) {
			repeatCharge.UnearnedTypes=string.Join(",",comboUnearnedTypes.GetListSelected<Def>().Select(x => x.DefNum));
		}
		return true;
	}

	private void butSave_Click(object sender, EventArgs e){
		if(!UpdateRepeatCharge(_repeatCharge)) {
			return;
		}
		var patientOldChange=Patients.GetPat(_repeatCharge.PatNum);
		var patientNewChange=patientOldChange.Copy();
		if(patientNewChange.PatStatus==PatientStatus.Deleted) {
			MsgBox.Show("Patient has been deleted by another user.");
			return;
		}
		if(PrefC.GetBool(PrefName.BillingUseBillingCycleDay) && textBillingDay.Text!="") {
			patientNewChange.BillingCycleDay=SIn.Int(textBillingDay.Text);
			Patients.Update(patientNewChange,patientOldChange);
		}
		if(IsNew) {
			if(!RepeatCharges.ActiveRepeatChargeExists(_repeatCharge.PatNum) 
			   && (textBillingDay.Text=="" || textBillingDay.Text=="0"))
			{
				patientNewChange.BillingCycleDay=SIn.Date(textDateStart.Text).Day;
				Patients.Update(patientNewChange,patientOldChange);
			}
			_repeatCharge.RepeatChargeNum=RepeatCharges.Insert(_repeatCharge);
			RepeatCharges.InsertRepeatChargeChangeSecurityLogEntry(_repeatCharge,EnumPermType.RepeatChargeCreate,patientOldChange,isAutomated:false);
		}
		else{ //not a new repeat charge
			RepeatCharges.InsertRepeatChargeChangeSecurityLogEntry(_repeatChargeOld,EnumPermType.RepeatChargeUpdate,patientOldChange,newCharge:_repeatCharge,isAutomated:false,newPat:patientNewChange);
			RepeatCharges.Update(_repeatCharge);
		}
		DialogResult=DialogResult.OK;
	}

}