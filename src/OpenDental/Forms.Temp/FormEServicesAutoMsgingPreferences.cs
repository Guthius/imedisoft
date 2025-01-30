using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness;
using OpenDentBusiness.WebTypes.WebForms;

namespace OpenDental;

public partial class FormEServicesAutoMsgingPreferences:FormODBase {
	private List<ClinicPref> _listClinicPrefs= [];
	private string _webSheetIdDefaults;
	private bool _under18SendToGuarantorDefault;

	public FormEServicesAutoMsgingPreferences() {
		InitializeComponent();
	}

	private void FormEServicesAutoMsgingPreferences_Load(object sender,EventArgs e) {
		var allowEdit=Security.IsAuthorized(EnumPermType.EServicesSetup,suppressMessage:true);
		_webSheetIdDefaults=PrefC.GetString(PrefName.ApptNewPatientThankYouWebSheetDefID);
		_under18SendToGuarantorDefault=PrefC.GetBool(PrefName.AutoCommUnder18SendToGuarantor);
		LoadWebFormPrefs();
		var listPrefNames=new List<PrefName> { PrefName.ApptNewPatientThankYouWebSheetDefID, PrefName.AutoMsgingUseDefaultPref, PrefName.AutoCommUnder18SendToGuarantor };
		_listClinicPrefs=ClinicPrefs.GetWhere(x => listPrefNames.Contains(x.PrefName));
		SetClinicComboBox();
		LoadDefaultPreferences();
		checkUseDefaultPrefs.Enabled=allowEdit;
		butSave.Enabled=allowEdit;
		EnablePreferenceControls(allowEdit);
	}

	private void SetClinicComboBox() {
		var hqDescription="Practice";
		if(true) {
			hqDescription="Defaults";
		}
		comboClinic.HqDescription=hqDescription;
		comboClinic.ForceShowUnassigned=true;
		comboClinic.IncludeAll=false;
		comboClinic.ClinicNumSelected=0;
		checkUseDefaultPrefs.Visible=false;
	}

	private void EnablePreferenceControls(bool allowEdit) {
		checkSendToGuarantorForMinors.Enabled=allowEdit;
		groupNewPat.Enabled=allowEdit;
	}

	private ClinicDto GetSelectedClinic() {
		return comboClinic.GetSelectedClinic();
	}

	private void comboClinic_SelectionChangeCommitted(object sender,EventArgs e) {
		var isDefaultClinic=GetSelectedClinic().Id==0;
		checkUseDefaultPrefs.Visible=!isDefaultClinic;
		checkUseDefaultPrefs.Checked=!isDefaultClinic && SetUseDefaultCheckbox(PrefName.AutoMsgingUseDefaultPref);
		if(isDefaultClinic || checkUseDefaultPrefs.Checked) {
			LoadDefaultPreferences();
		}
		else {
			LoadClinicPreferences();
		}
		var allowEdit=Security.IsAuthorized(EnumPermType.EServicesSetup,suppressMessage:true) && (isDefaultClinic || !checkUseDefaultPrefs.Checked);
		EnablePreferenceControls(allowEdit);
	}

	private string ValidateChanges() {
		var stringBuilderError=new StringBuilder();
		var listWebSheetDefIDs=_webSheetIdDefaults.Split(',').ToList();
		if(listWebSheetDefIDs.Any(x => x=="0") && listWebSheetDefIDs.Any(x => x!="0")) {
			stringBuilderError.AppendLine("Default Web Forms preference cannot contain 'None' when other forms are selected.");
		}
		for(var i=0;i<_listClinicPrefs.Count;i++) {
			if(_listClinicPrefs[i].ClinicNum==0) {
				continue;
			}
			listWebSheetDefIDs=_listClinicPrefs[i].ValueString.Split(',').ToList();
			if(listWebSheetDefIDs.Any(x => x=="0") && listWebSheetDefIDs.Any(x => x!="0")) {
				stringBuilderError.AppendLine(Clinics.GetAbbr(_listClinicPrefs[i].ClinicNum)+" Web Forms preference cannot contain 'None' when other forms are selected.");
			}
		}
		var error="";
		if(stringBuilderError.Length>0) {
			error="Please fix the following errors:\r\n"+stringBuilderError;
		}
		return error;
	}

	private void SaveToDb() {
		var changedPrefs=false;
		changedPrefs|=Prefs.UpdateString(PrefName.ApptNewPatientThankYouWebSheetDefID,_webSheetIdDefaults);
		changedPrefs|=Prefs.UpdateBool(PrefName.AutoCommUnder18SendToGuarantor,checkSendToGuarantorForMinors.Checked);
		var changedClinicPrefs=changedPrefs;
		for(var i=0;i<_listClinicPrefs.Count;i++) {
			changedClinicPrefs|=ClinicPrefs.Upsert(_listClinicPrefs[i].PrefName,_listClinicPrefs[i].ClinicNum,_listClinicPrefs[i].ValueString);
		}
		if(changedPrefs) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		if(changedClinicPrefs) {
			DataValid.SetInvalid(InvalidType.ClinicPrefs);
		}
	}

	private bool SetUseDefaultCheckbox(PrefName prefName) {
		var clinicPref=_listClinicPrefs.FirstOrDefault(x => x.PrefName==prefName && x.ClinicNum==GetSelectedClinic().Id);
		return clinicPref!=null && SIn.Bool(clinicPref.ValueString);
	}

	private void LoadWebFormPrefs() {
		listBoxWebForms.Items.Clear();
		var listSheetDefs=new List<WebForms_SheetDef>();
		listBoxWebForms.Items.Add("None",new WebForms_SheetDef { WebSheetDefID=0 });
		if(!WebForms_SheetDefs.TryDownloadSheetDefs(out listSheetDefs)) {
			MsgBox.Show(this,"Failed to download sheet definitions.");
			return;
		}
		listBoxWebForms.Items.AddList(listSheetDefs,(x)=>x.Description);
	}

	private void LoadDefaultPreferences() {
		var listWebSheetDefIds=_webSheetIdDefaults.Split([','],StringSplitOptions.RemoveEmptyEntries).Select(x => SIn.Long(x)).ToList();
		if(listWebSheetDefIds.IsNullOrEmpty()) {
			listBoxWebForms.SetSelected(0);
			return;
		}
		listBoxWebForms.SelectedIndices.Clear();
		var listSelectedIndicies=new List<int>();
		for(var i=0;i<listBoxWebForms.Items.Count;i++) {
			if(listWebSheetDefIds.Contains(((WebForms_SheetDef)listBoxWebForms.Items.GetObjectAt(i)).WebSheetDefID)) {
				listSelectedIndicies.Add(i);
			}
		}
		listBoxWebForms.SelectedIndices=listSelectedIndicies;
		checkSendToGuarantorForMinors.Checked=_under18SendToGuarantorDefault;
	}

	private void LoadClinicPreferences() {
		var clinic=GetSelectedClinic();
		var clinicPref=_listClinicPrefs.FirstOrDefault(x => x.PrefName==PrefName.ApptNewPatientThankYouWebSheetDefID && x.ClinicNum==clinic.Id);
		var listWebSheetDefIds=new List<long>();
		if(clinicPref!=null) {
			listWebSheetDefIds=clinicPref.ValueString.Split([','],StringSplitOptions.RemoveEmptyEntries).Select(x => SIn.Long(x,false)).ToList();
			//If non-zero entries exist, remove all 0 entries.
			if(listWebSheetDefIds.Any(x => x==0)) {
				listWebSheetDefIds=listWebSheetDefIds.FindAll(x => x!=0);
			}
		}
		else {
			clinicPref=new ClinicPref(clinic.Id,PrefName.ApptNewPatientThankYouWebSheetDefID,"0");
			_listClinicPrefs.Add(clinicPref);
			listWebSheetDefIds.Add(0);
		}
		var listSelectedIndicies=new List<int>();
		for(var i=0;i<listBoxWebForms.Items.Count;i++) {
			if(listWebSheetDefIds.Contains(((WebForms_SheetDef)listBoxWebForms.Items.GetObjectAt(i)).WebSheetDefID)) {
				listSelectedIndicies.Add(i);
			}
		}
		listBoxWebForms.SelectedIndices=listSelectedIndicies;
		clinicPref=_listClinicPrefs.FirstOrDefault(x => x.PrefName==PrefName.AutoCommUnder18SendToGuarantor && x.ClinicNum==clinic.Id);
		var doSendToGuarantor=false;
		if(clinicPref!=null) {
			doSendToGuarantor=SIn.Bool(clinicPref.ValueString);
		}
		checkSendToGuarantorForMinors.Checked=doSendToGuarantor;
	}

	private void checkUseDefaultPrefs_Click(object sender,EventArgs e) {
		var clinic=GetSelectedClinic();
		if(clinic.Id==0) {
			return;//Clinic 0, we don't need to do anything here.
		}
		var clinicPref=_listClinicPrefs.FirstOrDefault(x => x.ClinicNum==clinic.Id && x.PrefName==PrefName.AutoMsgingUseDefaultPref);
		if(clinicPref==null) {
			clinicPref=new ClinicPref(clinic.Id,PrefName.AutoMsgingUseDefaultPref,valueBool:false);
			_listClinicPrefs.Add(clinicPref);
		}
		//TURNING DEFAULTS OFF
		if(!checkUseDefaultPrefs.Checked && SIn.Bool(clinicPref.ValueString)) {//Default switched off
			clinicPref.ValueString=SOut.Bool(false);
			LoadClinicPreferences();
		}
		//TURNING DEFAULTS ON
		else if(checkUseDefaultPrefs.Checked && !SIn.Bool(clinicPref.ValueString)) {//Default switched on
			clinicPref.ValueString=SOut.Bool(true);
			LoadDefaultPreferences();
		}
		var allowEdit=Security.IsAuthorized(EnumPermType.EServicesSetup,suppressMessage:true) && !checkUseDefaultPrefs.Checked;
		EnablePreferenceControls(allowEdit);
	}

	private void checkSendToGuarantorForMinors_Click(object sender,EventArgs e) {
		var clinic=GetSelectedClinic();
		if(!true || clinic.Id==0) {
			_under18SendToGuarantorDefault=checkSendToGuarantorForMinors.Checked;
			return;
		}
		var clinicPref=_listClinicPrefs.FirstOrDefault(x => x.ClinicNum==clinic.Id && x.PrefName==PrefName.AutoCommUnder18SendToGuarantor);
		if(clinicPref==null) {
			clinicPref=new ClinicPref(clinic.Id,PrefName.AutoCommUnder18SendToGuarantor,valueBool:checkSendToGuarantorForMinors.Checked);
			_listClinicPrefs.Add(clinicPref);
			return;
		}
		clinicPref.ValueString=SOut.Bool(checkSendToGuarantorForMinors.Checked);
	}

	private void listBoxWebForm_SelectionChangeCommitted(object sender,EventArgs e) {
		var clinic=GetSelectedClinic();
		var webSheetDefIDs=string.Join(",",listBoxWebForms.GetListSelected<WebForms_SheetDef>().Select(x => x.WebSheetDefID));
		if(!true || clinic.Id==0) {
			_webSheetIdDefaults=webSheetDefIDs;
			return;
		}
		var clinicPref=_listClinicPrefs.FirstOrDefault(x => x.ClinicNum==clinic.Id && x.PrefName==PrefName.ApptNewPatientThankYouWebSheetDefID);
		if(clinicPref==null) {
			clinicPref=new ClinicPref(clinic.Id,PrefName.ApptNewPatientThankYouWebSheetDefID,valueString:SOut.String(webSheetDefIDs));
			_listClinicPrefs.Add(clinicPref);
			return;
		}
		clinicPref.ValueString=SOut.String(webSheetDefIDs);
	}

	private void butSave_Click(object sender,EventArgs e) {
		var error=ValidateChanges();
		if(!string.IsNullOrWhiteSpace(error)) {
			ODMessageBox.Show(Lan.g(this,error));
			return;
		}
		SaveToDb();
		DialogResult=DialogResult.OK;
	}

}