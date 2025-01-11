using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;
using OpenDental.UI;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDental {
	
	public partial class FormClinicEdit : FormODBase {
		public ClinicDto ClinicCur;
		//private List<Provider> _listProv;
		///<summary>True if an HL7Def is enabled with the type HL7InternalType.MedLabv2_3, otherwise false.</summary>
		private bool _isMedLabHL7DefEnabled;
		private List<Def> _listDefsRegions;
		///<summary>DefLink.FKey is ClinicNum to link to defs representing specialties for that clinic.</summary>
		public List<DefLink> ListDefLinksSpecialties;
		///<summary>List of clinics that will be used for validation. Hidden clinics and ClinicCur will not be present in this list. Some clinics in this list may not reflect what is in the cache or the database.</summary>
		private List<ClinicDto> _listClinicsForValidation;

		///<summary>Provide the clinic that will be edited. Optionally provide a list of clinics with local changes if in the middle of editing multiple clinics in memory.</summary>
		public FormClinicEdit(ClinicDto clinic,List<ClinicDto> listClinics=null) {
			ClinicCur=clinic;
			//Initialize the list of clinics for validation with all non-hidden clinics in the cache.
			_listClinicsForValidation=Clinics.GetWhere(x => x.Id!=ClinicCur.Id,isShort:true);
			//Override any cached clinics with the list of clinics that were passed in. A user could be in the middle of editing the entire list of all clinics in memory.
			if(!listClinics.IsNullOrEmpty()) {
				List<long> listClinicNums=listClinics.Select(x => x.Id).ToList();
				//Remove clinics passed in since the ones passed in can have different values.
				_listClinicsForValidation.RemoveAll(x => listClinicNums.Contains(x.Id));
				//Add the clinics passed to the class wide list. This will have all of the clinics that were passed in.
				_listClinicsForValidation.AddRange(listClinics.FindAll(x => !x.IsHidden && x.Id!=ClinicCur.Id));
			}
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormClinicEdit_Load(object sender, System.EventArgs e) {
			if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {//Canadian. en-CA or fr-CA
				label11.Text=Lang.g(this,"City, Prov, Postal");
				label20.Text=Lang.g(this,"City, Prov, Postal");
				label15.Text=Lang.g(this,"City, Prov, Postal");
			}
			checkIsMedicalOnly.Checked=ClinicCur.IsMedicalOnly;
			if(Programs.UsingEcwTightOrFullMode()) {
				checkIsMedicalOnly.Visible=false;
			}
			if(ClinicCur.Id!=0) {
				textClinicNum.Text=ClinicCur.Id.ToString();
			}
			// TODO: Remove: textExternalID.Text=ClinicCur.ExternalID.ToString();
			textDescription.Text=ClinicCur.Description;
			textClinicAbbr.Text=ClinicCur.Abbr;
			textPhone.Text=TelephoneNumbers.ReFormat(ClinicCur.PhoneNumber);
			textFax.Text=TelephoneNumbers.ReFormat(ClinicCur.FaxNumber);
			checkUseBillingAddressOnClaims.Checked=ClinicCur.UseBillingAddressOnClaims;
			checkExcludeFromInsVerifyList.Checked=ClinicCur.ExcludeFromInsuranceVerification;
			if(PrefC.GetBool(PrefName.RxHasProc)) {
				checkProcCodeRequired.Enabled=true;
				checkProcCodeRequired.Checked=(ClinicCur.Id == 0 || ClinicCur.HasProceduresOnRx);
			}
			checkHidden.Checked=ClinicCur.IsHidden;
			textAddress.Text=ClinicCur.AddressLine1;
			textAddress2.Text=ClinicCur.AddressLine2;
			textCity.Text=ClinicCur.City;
			textState.Text=ClinicCur.State;
			textZip.Text=ClinicCur.Zip;
			textBillingAddress.Text=ClinicCur.BillingAddressLine1;
			textBillingAddress2.Text=ClinicCur.BillingAddressLine2;
			textBillingCity.Text=ClinicCur.BillingCity;
			textBillingST.Text=ClinicCur.BillingState;
			textBillingZip.Text=ClinicCur.BillingZip;
			textPayToAddress.Text=ClinicCur.PayToAddressLine1;
			textPayToAddress2.Text=ClinicCur.PayToAddressLine2;
			textPayToCity.Text=ClinicCur.PayToCity;
			textPayToST.Text=ClinicCur.PayToState;
			textPayToZip.Text=ClinicCur.PayToZip;
			textBankNumber.Text=ClinicCur.BankNumber;
			textSchedRules.Text=ClinicCur.SchedulingNote;
			comboPlaceService.Items.Clear();
			comboPlaceService.Items.AddList<string>(Enum.GetNames(typeof(PlaceOfService)));
			comboPlaceService.SelectedIndex=(int)PlaceOfServiceCodes.ToEnum(ClinicCur.DefaultPlaceOfService);
			comboInsBillingProv.Items.AddProvsAbbr(Providers.GetProvsForClinic(ClinicCur.Id));
			switch (ClinicCur.BillingProviderType)
			{
				case BillingProviderType.Default:
					radioInsBillingProvDefault.Checked=true;//default=0
					break;
				
				case BillingProviderType.Treating:
					radioInsBillingProvTreat.Checked=true;//treat=-1
					break;
				
				case BillingProviderType.Specific:
					radioInsBillingProvSpecific.Checked=true;//specific=any number >0. Foreign key to ProvNum
					comboInsBillingProv.SetSelectedProvNum(ClinicCur.BillingProviderId??0);
					break;
			}
			comboDefaultProvider.Items.AddProvsAbbr(Providers.GetProvsForClinic(ClinicCur.Id));
			if(ClinicCur.DefaultProviderId.HasValue){
				comboDefaultProvider.SetSelectedProvNum(ClinicCur.DefaultProviderId.Value);
			}
			ListDefLinksSpecialties=DefLinks.GetDefLinksForClinicSpecialties(ClinicCur.Id);
			FillSpecialty();
			comboRegion.Items.Clear();
			comboRegion.Items.Add(Lan.g(this,"None"));
			comboRegion.SelectedIndex=0;
			_listDefsRegions=Defs.GetDefsForCategory(DefCat.Regions,true);
			for(int i=0;i<_listDefsRegions.Count;i++) {
				comboRegion.Items.Add(_listDefsRegions[i].ItemName);
				if(_listDefsRegions[i].DefNum==ClinicCur.RegionId) {
					comboRegion.SelectedIndex=i+1;
				}
			}
			//Pre-select billing type if there is a clinicpref associated to the chosen clinic, otherwise "Use Global Preference".
			comboDefaultBillingType.Items.AddDefNone(Lan.g(this,"Use Global Preference"));
			comboDefaultBillingType.Items.AddDefs(Defs.GetDefsForCategory(DefCat.BillingTypes,true));
			comboDefaultBillingType.SetSelectedDefNum(ClinicPrefs.GetLong(PrefName.PracticeDefaultBillType,ClinicCur.Id));
			comboInsAutoReceiveNoAssign.Items.Add("Off");
			comboInsAutoReceiveNoAssign.Items.Add("On");
			comboInsAutoReceiveNoAssign.Items.Add("Use Global Preference");
			comboInsAutoReceiveNoAssign.SetSelected(2);//Default to use global preference.
			ClinicPref clinicPrefInsAutoReceiveNoAssign=ClinicPrefs.GetPref(PrefName.InsAutoReceiveNoAssign,ClinicCur.Id);
			if(clinicPrefInsAutoReceiveNoAssign!=null) {
				comboInsAutoReceiveNoAssign.SetSelected(PIn.Int(clinicPrefInsAutoReceiveNoAssign.ValueString));
			}
			//"Always Assign Benefits to the Patient" checkbox is an override. If the clinic has this pref value, this means it is checked.
			ClinicPref clinicPrefAlwaysAssignBenToPatient=ClinicPrefs.GetPref(PrefName.InsDefaultAssignBen,ClinicCur.Id);
			if(clinicPrefAlwaysAssignBenToPatient!=null) {
				checkAlwaysAssignBenToPatient.Checked=true;
			}
			EmailAddress emailAddress=EmailAddresses.GetOne(ClinicCur.EmailAddressId??0);
			if(emailAddress!=null) {
				textEmail.Text=emailAddress.GetFrom();
				butEmailNone.Enabled=true;
			}
			textClinicEmailAliasOverride.Text=ClinicCur.EmailAliasOverride;
			_isMedLabHL7DefEnabled=HL7Defs.IsExistingHL7Enabled(0,true);
			if(_isMedLabHL7DefEnabled) {
				textMedLabAcctNum.Visible=true;
				labelMedLabAcctNum.Visible=true;
				textMedLabAcctNum.Text=ClinicCur.MedLabAccountNumber;
			}
		}

		private void butPickInsBillingProv_Click(object sender,EventArgs e) {
			FrmProviderPick frmProviderPick=new FrmProviderPick(comboInsBillingProv.Items.GetAll<Provider>());
			frmProviderPick.ProvNumSelected=comboInsBillingProv.GetSelectedProvNum();
			frmProviderPick.ShowDialog();
			if(!frmProviderPick.IsDialogOK) {
				return;
			}
			comboInsBillingProv.SetSelectedProvNum(frmProviderPick.ProvNumSelected);
		}

		private void butPickDefaultProv_Click(object sender,EventArgs e) {
			FrmProviderPick frmProviderPick=new FrmProviderPick(comboDefaultProvider.Items.GetAll<Provider>());
			frmProviderPick.ProvNumSelected=comboDefaultProvider.GetSelectedProvNum();//this is 0 if selectedIndex -1
			frmProviderPick.ShowDialog();
			if(!frmProviderPick.IsDialogOK) {
				return;
			}
			if(frmProviderPick.ProvNumSelected>0){
				comboDefaultProvider.SetSelectedProvNum(frmProviderPick.ProvNumSelected);
			}
		}

		private void butNone_Click(object sender,EventArgs e) {
			comboDefaultProvider.SetSelectedProvNum(0);
		}

		private void FillSpecialty() {
			List<Def> listDefs=Defs.GetDefsForCategory(DefCat.ClinicSpecialty);
			gridSpecialty.BeginUpdate();
			gridSpecialty.Columns.Clear();
			gridSpecialty.Columns.Add(new GridColumn(Lan.g(gridSpecialty.TranslationName,"Specialty"),100));
			gridSpecialty.ListGridRows.Clear();
			GridRow row;
			string specialtyDescript;
			for(int i = 0;i<ListDefLinksSpecialties.Count;i++) {
				row=new GridRow();
				specialtyDescript="";
				Def def = listDefs.FirstOrDefault(x => x.DefNum==ListDefLinksSpecialties[i].DefNum);
				if(def!=null) {
					specialtyDescript=def.ItemName+(def.IsHidden ? (" ("+Lan.g(this,"hidden")+")") : "");
				}
				row.Cells.Add(specialtyDescript);
				row.Tag=ListDefLinksSpecialties[i];
				gridSpecialty.ListGridRows.Add(row);
			}
			gridSpecialty.EndUpdate();
		}
		
		private void butAdd_Click(object sender,EventArgs e) {
			using FormDefinitionPicker formDefinitionPicker=new FormDefinitionPicker(DefCat.ClinicSpecialty);
			formDefinitionPicker.HasShowHiddenOption=false;
			formDefinitionPicker.IsMultiSelectionMode=true;
			formDefinitionPicker.ShowDialog();
			if(formDefinitionPicker.DialogResult==DialogResult.OK) {
				for(int i=0;i<formDefinitionPicker.ListDefsSelected.Count;i++) {
					if(ListDefLinksSpecialties.Any(x => x.DefNum==formDefinitionPicker.ListDefsSelected[i].DefNum)) {
						continue;//Definition already added to this clinic. 
					}
					DefLink defLink=new DefLink();
					defLink.DefNum=formDefinitionPicker.ListDefsSelected[i].DefNum;
					defLink.FKey=ClinicCur.Id;//could be 0 if IsNew
					defLink.LinkType=DefLinkType.ClinicSpecialty;
					ListDefLinksSpecialties.Add(defLink);
				}
				FillSpecialty();
			}
		}

		private void butRemove_Click(object sender,EventArgs e) {
			if(gridSpecialty.SelectedIndices.Length==0) {
				MessageBox.Show(Lan.g(this,"Please select a specialty first."));
				return;
			}
			gridSpecialty.SelectedIndices
				.Select(x => (DefLink)gridSpecialty.ListGridRows[x].Tag).ToList()
				.ForEach(x => ListDefLinksSpecialties.Remove(x));
			FillSpecialty();
		}

		private void butEmail_Click(object sender,EventArgs e) {
			using FormEmailAddresses formEmailAddresses=new FormEmailAddresses();
			formEmailAddresses.IsSelectionMode=true;
			formEmailAddresses.ShowDialog();
			if(formEmailAddresses.DialogResult!=DialogResult.OK) {
				return;
			}
			ClinicCur.EmailAddressId=formEmailAddresses.EmailAddressNum;
			textEmail.Text=EmailAddresses.GetOne(formEmailAddresses.EmailAddressNum).GetFrom();
			butEmailNone.Enabled=true;
		}

		private void buttDetachEmail_Click(object sender,EventArgs e) {
			ClinicCur.EmailAddressId=null;
			textEmail.Text="";
			butEmailNone.Enabled=false;
		}

		private void butSave_Click(object sender, System.EventArgs e) {
			#region Validation 
			if(textDescription.Text==""){
				MsgBox.Show(this,"Description cannot be blank.");
				return;
			}
			if(textClinicAbbr.Text==""){
				MsgBox.Show(this,"Abbreviation cannot be blank.");
				return;
			}
			if(radioInsBillingProvSpecific.Checked && comboInsBillingProv.SelectedIndex==-1){ 
				MsgBox.Show(this,"You must select a provider.");
				return;
			}
			//Check to see if abbr already exists. The check will only happen for non-hidden clinics.
			if(!checkHidden.Checked && _listClinicsForValidation.Any(x => x.Abbr.ToLower()==textClinicAbbr.Text.ToLower().Trim())) {
				MsgBox.Show(this,"Abbreviation already exists.");
				return;
			}
			string phone=textPhone.Text;
			if(Application.CurrentCulture.Name=="en-US"){
				phone=phone.Replace("(","");
				phone=phone.Replace(")","");
				phone=phone.Replace(" ","");
				phone=phone.Replace("-","");
				if(phone.Length!=0 && phone.Length!=10){
					MsgBox.Show(this,"Invalid phone");
					return;
				}
			}
			string fax=textFax.Text;
			if(Application.CurrentCulture.Name=="en-US") {
				fax=fax.Replace("(","");
				fax=fax.Replace(")","");
				fax=fax.Replace(" ","");
				fax=fax.Replace("-","");
				if(fax.Length!=0 && fax.Length!=10) {
					MsgBox.Show(this,"Invalid fax");
					return;
				}
			}
			if(_isMedLabHL7DefEnabled //MedLab HL7 def is enabled, so textMedLabAcctNum is visible
				&& !string.IsNullOrWhiteSpace(textMedLabAcctNum.Text) //MedLabAcctNum has been entered
				&& Clinics.GetWhere(x => x.Id!=ClinicCur.Id)
						.Any(x => x.MedLabAccountNumber==textMedLabAcctNum.Text.Trim())) //this account num is already in use by another Clinic
			{
				MsgBox.Show(this,"The MedLab Account Number entered is already in use by another clinic.");
				return;
			}
			if(checkHidden.Checked) {
				//ensure that there are no users who have only this clinic assigned to them.
				List<Userod> listUserodsRestricted = Userods.GetUsersOnlyThisClinic(ClinicCur.Id);
				if(listUserodsRestricted.Count > 0) {
					MessageBox.Show(Lan.g(this,"You may not hide this clinic as the following users are restricted to only this clinic") + ": "
						+ string.Join(", ",listUserodsRestricted.Select(x => x.UserName)));
					return;
				}
			}
			long externalID=0;
			if(textExternalID.Text != "") {
				try {
					externalID=long.Parse(textExternalID.Text);
				}
				catch {
					MsgBox.Show(this,"Please fix data entry errors first."+"\r\n"+", The External ID must be a number. No letters or symbols allowed.");
					return;
				}
			}
			if(!Regex.IsMatch(textClinicEmailAliasOverride.Text,"^[A-Za-z0-9.@]*$")) {
				MsgBox.Show(this,"The Email Alias Override can only contain letters, numbers, period (.), and the at sign (@).");
				return;
			}
			#endregion Validation
			#region Set Values
			ClinicCur.IsMedicalOnly=checkIsMedicalOnly.Checked;
			ClinicCur.ExcludeFromInsuranceVerification=checkExcludeFromInsVerifyList.Checked;
			ClinicCur.HasProceduresOnRx=checkProcCodeRequired.Checked;
			ClinicCur.Abbr=textClinicAbbr.Text;
			ClinicCur.Description=textDescription.Text;
			ClinicCur.PhoneNumber=phone;
			ClinicCur.FaxNumber=fax;
			ClinicCur.AddressLine1=textAddress.Text;
			ClinicCur.AddressLine2=textAddress2.Text;
			ClinicCur.City=textCity.Text;
			ClinicCur.State=textState.Text;
			ClinicCur.Zip=textZip.Text;
			ClinicCur.BillingAddressLine1=textBillingAddress.Text;
			ClinicCur.BillingAddressLine2=textBillingAddress2.Text;
			ClinicCur.BillingCity=textBillingCity.Text;
			ClinicCur.BillingState=textBillingST.Text;
			ClinicCur.BillingZip=textBillingZip.Text;
			ClinicCur.PayToAddressLine1=textPayToAddress.Text;
			ClinicCur.PayToAddressLine2=textPayToAddress2.Text;
			ClinicCur.PayToCity=textPayToCity.Text;
			ClinicCur.PayToState=textPayToST.Text;
			ClinicCur.PayToZip=textPayToZip.Text;
			ClinicCur.BankNumber=textBankNumber.Text;
			ClinicCur.DefaultPlaceOfService=comboPlaceService.SelectedIndex.ToString();
			ClinicCur.UseBillingAddressOnClaims=checkUseBillingAddressOnClaims.Checked;
			ClinicCur.IsHidden=checkHidden.Checked;
			ClinicCur.EmailAliasOverride=textClinicEmailAliasOverride.Text.Trim();
			long defNumRegion=0;
			if(comboRegion.SelectedIndex>0){
				defNumRegion=_listDefsRegions[comboRegion.SelectedIndex-1].DefNum;
			}
			ClinicCur.RegionId=defNumRegion;
			if(radioInsBillingProvDefault.Checked){//default=0
				ClinicCur.BillingProviderType = BillingProviderType.Default;
				ClinicCur.BillingProviderId = null;
			}
			else if(radioInsBillingProvTreat.Checked){//treat=-1
				ClinicCur.BillingProviderType = BillingProviderType.Treating;
				ClinicCur.BillingProviderId = null;
			}
			else{
				ClinicCur.BillingProviderType = BillingProviderType.Specific;
				ClinicCur.BillingProviderId=comboInsBillingProv.GetSelectedProvNum();
			}
			ClinicCur.DefaultProviderId=comboDefaultProvider.GetSelectedProvNum();//0 for selectedIndex -1
			if(_isMedLabHL7DefEnabled) {
				ClinicCur.MedLabAccountNumber=textMedLabAcctNum.Text.Trim();
			}
			ClinicCur.SchedulingNote=textSchedRules.Text;
			List<PrefName> listClinicPrefsToDelete=new List<PrefName>();
			if(comboDefaultBillingType.SelectedIndex>0) {//If default billing type is not set to default, update/insert clinicpref row.
				string strBillingType=POut.Long(comboDefaultBillingType.GetSelectedDefNum());
				ClinicPrefs.Upsert(PrefName.PracticeDefaultBillType,ClinicCur.Id,strBillingType);
			}
			else {//"Use Global Preference" selected, delete the pref if it exists.
				listClinicPrefsToDelete.Add(PrefName.PracticeDefaultBillType);
			}
			if(checkAlwaysAssignBenToPatient.Checked) {//If checked, we will upsert into clinicpref table. False accurately represents the pref.
				ClinicPrefs.Upsert(PrefName.InsDefaultAssignBen,ClinicCur.Id,"0");
			}
			else {//Since we aren't overriding, we want to remove the clinicpref to not confuse as boolean state.
				listClinicPrefsToDelete.Add(PrefName.InsDefaultAssignBen);
			}
			if(comboInsAutoReceiveNoAssign.SelectedIndex==2) {//2=Use Global Preference
				listClinicPrefsToDelete.Add(PrefName.InsAutoReceiveNoAssign);
			}
			else {
				string strInsAutoReceiveNoAssignClinicPref=POut.Int(comboInsAutoReceiveNoAssign.SelectedIndex);
				ClinicPrefs.Upsert(PrefName.InsAutoReceiveNoAssign,ClinicCur.Id,strInsAutoReceiveNoAssignClinicPref);
			}
			ClinicPrefs.DeletePrefs(ClinicCur.Id,listClinicPrefsToDelete);
			ClinicPrefs.RefreshCache();
			#endregion Set Values
			DialogResult=DialogResult.OK;
		}

		private void checkHidden_CheckedChanged(object sender,EventArgs e) {
			if(!checkHidden.Checked) { //Unhiding clinic
				return;
			}
			//User is trying to hide clinic
			long numPatients=Patients.GetPatNumsByClinic(ClinicCur.Id).Count;
			if(numPatients>0) { //Patients attached to this clinic
				MsgBox.Show(this,"There are "+numPatients+" patient(s) attached to this clinic. It cannot be hidden until they are removed.");
				checkHidden.Checked=false; //Return box to unchecked
			}
		}

	}
}