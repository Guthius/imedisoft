using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDental.Logic;

namespace OpenDental;

public partial class FormProcEditAll:FormODBase {
	///<summary>All procedures must be for the same patient.</summary>
	public List<Procedure> ListProcedures;
	private List<Procedure> _listProceduresOld;
	///<summary>True when any proc in ProcList has a Proc Status of C.</summary>
	private bool _hasCompletedProc;
	///<summary>True when any proc in ProcList has a Proc Status of EO or EC.</summary>
	private bool _hasExistingProc;
	///<summary>True when all procs in ProcList Can Bypass Lock Date.</summary>
	private bool _canAllBypass;
	///<summary>List of providers shown in comboProv. Excludes blank(multi) and provider abbr text if user does not have access to a clinic from ProcList.</summary>>
	private List<Provider> _listProvidersForClinic;
	///<summary>List of clinics shown in comboClincs. Excludes blank(multi) and clinic abbr text if user does not have access to a clinic from ProcList.</summary>
	private List<ClinicDto> _listClinics;

	public FormProcEditAll() {
		InitializeComponent();
	}

	private void FormProcEditAll_Load(object sender,EventArgs e) {
		_listProceduresOld= [];
		for(var i=0;i<ListProcedures.Count;i++){
			_listProceduresOld.Add(ListProcedures[i].Copy());
		}
		_hasCompletedProc=false;
		_hasExistingProc=false;
		_canAllBypass=true;
		var procDateOldest=DateTime.Today;
		var isDateLoadedWithValue=true;
		for(var i=0;i<ListProcedures.Count;i++){
			if(ListProcedures[i].ProcStatus==ProcStat.C) {
				_hasCompletedProc=true;
				if(ListProcedures[i].ProcDate < procDateOldest){
					procDateOldest=ListProcedures[i].ProcDate;
				}
			}
			else if(ListProcedures[i].ProcStatus.In(ProcStat.EO,ProcStat.EC)){
				_hasExistingProc=true;
				//Eo/Ec procs check DateEntryC for permissions.
			}
			if(ListProcedures[0].ProcDate!=ListProcedures[i].ProcDate){
				isDateLoadedWithValue=false;
			}
			if(!ProcedureCodes.CanBypassLockDate(ListProcedures[i].CodeNum,ListProcedures[i].ProcFee)) {
				_canAllBypass=false;
			}
		}
		if(!IsUserAuthorizedForProcDate(procDateOldest)) {
			butSave.Enabled=false;
			butEditAnyway.Enabled=false;
		}
		var listClaimProcs=ClaimProcs.Refresh(ListProcedures[0].PatNum);
		if(Procedures.IsAttachedToClaim(ListProcedures,listClaimProcs)){
			//StartedAttachedToClaim=true;
			//however, this doesn't stop someone from creating a claim while this window is open,
			//so this is checked at the end, too.
			textDate.Enabled=false;
			butToday.Enabled=false;
			butEditAnyway.Visible=true;
			labelClaim.Visible=true;
		}
		if(isDateLoadedWithValue){
			textDate.Text=ListProcedures[0].ProcDate.ToShortDateString();
		}
		if(true) {
			labelClinic.Visible=true;
			comboClinic.Visible=true;
			FillComboClinic();//Must be called before FillProviderCombo().
		}
		FillComboProv();
	}

	///<summary>Determines if the current user had sufficient permissions to change the ProcDate to 'date' on the procedures in ProcList.</summary>
	private bool IsUserAuthorizedForProcDate(DateTime dateCProcs) {
		if(_hasCompletedProc || _hasExistingProc){
			var dateEoEcOldest=DateTime.Today;
			if(_hasExistingProc){
				dateEoEcOldest=ListProcedures.Where(x => x.ProcStatus.In(ProcStat.EO,ProcStat.EC)).Min(x => x.DateEntryC);
			}
			if(_canAllBypass){
				if(_hasCompletedProc && !Security.IsAuthorized(EnumPermType.ProcCompleteEdit,dateCProcs,ListProcedures[0].CodeNum,0)){
					return false;
				}
				if(_hasExistingProc && !Security.IsAuthorized(EnumPermType.ProcExistingEdit,dateEoEcOldest,ListProcedures[0].CodeNum,0)){
					return false;
				}
				return true;
			}
			if(_hasCompletedProc && !Security.IsAuthorized(EnumPermType.ProcCompleteEdit,dateCProcs)){
				return false;
			}
			if(_hasExistingProc && !Security.IsAuthorized(EnumPermType.ProcExistingEdit,dateEoEcOldest)){
				return false;
			}
		}
		return true;
	}

	private void butToday_Click(object sender,EventArgs e) {
		if(textDate.Enabled){
			textDate.Text=DateTime.Today.ToShortDateString();
		}
	}

	private void FillComboClinic() {
		//Not using ComboBoxClinicPicker because we need to add a blank "" for no selection logic when procedures have different clinics.
		comboClinic.Items.Clear();
		comboClinic.Items.Add("",null);
		comboClinic.SelectedIndex=0;//Selection is not changed if isAllProcsForSameClinic is false.
		var isAllProcsForSameClinic=ListProcedures.Select(x => x.ClinicNum).Distinct().ToList().Count==1;
		_listClinics=Clinics.GetForUserod(Security.CurUser);
		_listClinics.Insert(0,Clinics.GetPracticeAsClinicZero(Lan.g(this,"None")));
		for(var i=0;i<_listClinics.Count;i++) {//None mimics FormProcEdit
			comboClinic.Items.Add(_listClinics[i].Abbr,_listClinics[i]);
			if(isAllProcsForSameClinic && _listClinics[i].Id==ListProcedures[0].ClinicNum) {
				comboClinic.SetSelected(i+1);
			}
		}
		if(isAllProcsForSameClinic && _listClinics.All(x => x.Id != ListProcedures[0].ClinicNum)) {
			//All procedure clinics are the same but value is missing from our list.
			//We might eventaully check to see how many clincs from proc list do not exists in listClinics.
			comboClinic.SetSelectedKey<ClinicDto>(ListProcedures[0].ClinicNum,x=>x.Id,x=>Clinics.GetAbbr(x));//selectedIndex -1
		}
	}

	private void comboClinic_SelectionChangeCommitted(object sender,EventArgs e) {
		FillComboProv(true);
	}
		
	private void FillComboProv(bool tryMaintainOldSelection=false) {
		_listProvidersForClinic= [];
		if(comboClinic.GetSelected<ClinicDto>()==null) {//Dummy clinic is selected either directly or on load due to procs having multiple clinics.
			//default the list to use all providers
			//We might want to change this to instead load all providers for all ClinicNums in ProcList.
			_listProvidersForClinic=Providers.GetDeepCopy(true);
		}
		else {
			_listProvidersForClinic=Providers.GetProvsForClinic(comboClinic.GetSelected<ClinicDto>().Id);
		}
		_listProvidersForClinic=_listProvidersForClinic.Where(x => !x.IsHidden).OrderBy(x => x.ItemOrder).ToList();
		Provider providerSelection=null;
		if(tryMaintainOldSelection && comboProv.GetSelected<Provider>()!=null){//Only true on manual selection, not on load.
			providerSelection=_listProvidersForClinic.FirstOrDefault(x => x.ProvNum==comboProv.GetSelectedProvNum());
		}
		comboProv.Items.Clear();
		comboProv.Items.Add("",null);
		comboProv.Items.AddProvsFull(_listProvidersForClinic);
		comboProv.SelectedIndex=0;//default selected index to blank/original values, override if there is a different match below.
		var isAllProcsForSameProv=ListProcedures.Select(x => x.ProvNum).Distinct().ToList().Count==1;
		if(tryMaintainOldSelection && providerSelection!=null){
			comboProv.SetSelectedProvNum(providerSelection.ProvNum);//set to previous selection
		}
		else if(isAllProcsForSameProv){
			comboProv.SetSelectedProvNum(ListProcedures[0].ProvNum);//default to the proc's prov if all procs have same prov
		}
		if(isAllProcsForSameProv && !_listProvidersForClinic.Any(x => x.ProvNum==ListProcedures[0].ProvNum)) {
			//All procedure clinics are the same but value is missing from our list.
			//We might eventaully check to see how many clincs from proc list do not exists in listClinics.
			comboProv.SetSelectedProvNum(ListProcedures[0].ProvNum);//selectedIndex -1
		}
	}

	private void butMoreProvs_Click(object sender,EventArgs e) {
		var frmProviderPick=new FrmProviderPick(_listProvidersForClinic);
		frmProviderPick.ShowDialog();
		if(!frmProviderPick.IsDialogOK) {
			return;
		}
		comboProv.SetSelectedProvNum(frmProviderPick.ProvNumSelected);
	}

	private void butEditAnyway_Click(object sender,EventArgs e) {
		textDate.Enabled=true;
		butToday.Enabled=true;
	}

	private bool EntriesAreValid() {
		//Get a new recent list of claimprocs for pat to be able to validate for provider and procedure status change.
		if(textDate.Text!="" && !textDate.IsValid()) {
			//Either loaded blank or user deleted date. Either way blank will not make it to DB.
			MsgBox.Show(this,"Please fix data entry errors first.");
			return false;
		}
		var dateProc=SIn.Date(textDate.Text);
		if(textDate.Text!="" && ListProcedures.Any(x => x.ProcDate!=dateProc)){
			if(!IsUserAuthorizedForProcDate(dateProc)) {//Do not allow new ProcDate outside of date limitations.  Mimics behavior in FormProcEdit.
				return false;
			}
			Appointment appointment;
			for(var i=0;i<ListProcedures.Count;i++){//first validate for all dates.
				#region Future dating completed procedures validation.
				if(!PrefC.GetBool(PrefName.FutureTransDatesAllowed) && ListProcedures[i].ProcStatus==ProcStat.C && dateProc>DateTime.Today) {
					MsgBox.Show(this,"Completed procedures cannot have future dates.");
					return false;
				}
				#endregion
				#region Procedures attached to appointments date validation.
				if(ListProcedures[i].AptNum==0){
					continue;
				}
				appointment=Appointments.GetOneApt(ListProcedures[i].AptNum);
				if(ListProcedures[i].ProcDate.Date!=appointment.AptDateTime.Date) {
					var error=Lan.g(this,"Date does not match appointment date for a procedure dated:")+" "+ListProcedures[i].ProcDate.ToShortDateString()
					          +"\r\n"+Lan.g(this,"Continue anyway?");
					if(ODMessageBox.Show(error,"",MessageBoxButtons.YesNo)==DialogResult.No) {
						return false;
					}
					break;
				}
				#endregion
			}
		}
		var listClaimProcsForPat=ClaimProcs.RefreshForProcs(ListProcedures.Select(x => x.ProcNum).ToList());
		for(var i=0;i<ListProcedures.Count;i++) {
			if(ListProcedures[i].IsLocked) {
				MsgBox.Show(this,"Locked procedures cannot be edited.");
				return false;
			}
			#region Provider change validation.
			var listClaimProcsForProc=ClaimProcs.GetForProc(listClaimProcsForPat,ListProcedures[i].ProcNum);
			var selectedProvNum=(comboProv.GetSelected<Provider>()?.ProvNum??0);//0 if no selection made
			if(selectedProvNum!=0 && !ProcedureL.ValidateProvider(listClaimProcsForProc,selectedProvNum,ListProcedures[i].ProvNum)) {
				return false;
			}
			#endregion
		}
		return true;
	}

	private string ConstructSecurityLogForProcType(Procedure procedure,Procedure procedureOld) {
		var logTextForProc="";
		var code=ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode;
		var procDateStrOld=SOut.Date(procedureOld.ProcDate);
		var procDateStrNew=SOut.Date(procedure.ProcDate);
		logTextForProc+=SecurityLogEntryHelper(code,SecurityLogFields.ProcDate,procDateStrOld,procDateStrNew);
		var provNumStrOld=Providers.GetAbbr(procedureOld.ProvNum);
		var provNumStrNew=Providers.GetAbbr(procedure.ProvNum);
		logTextForProc+=SecurityLogEntryHelper(code,SecurityLogFields.ProvNum,provNumStrOld,provNumStrNew);
		var clinicNumStrOld=Clinics.GetAbbr(procedureOld.ClinicNum);
		var clinicNumStrNew=Clinics.GetAbbr(procedure.ClinicNum);
		logTextForProc+=SecurityLogEntryHelper(code,SecurityLogFields.ClinicNum,clinicNumStrOld,clinicNumStrNew);
		return logTextForProc;
	}

	///<summary>Returns blank string if oldVal and newVal are the same.</summary>
	private string SecurityLogEntryHelper(string procCodeVal,SecurityLogFields securityLogFields,string oldVal,string newVal) {
		if(oldVal==newVal) {
			return "";
		}
		var field="";
		switch(securityLogFields) {
			case SecurityLogFields.ProcDate:
				field=Lan.g(this,"Proc Date");
				break;
			case SecurityLogFields.ProvNum:
				field=Lan.g(this,"Provider");
				break;
			case SecurityLogFields.ClinicNum:
				field=Lan.g(this,"Clinic");
				break;
		}
		return Lan.g(this,"Procedure code")+" "+procCodeVal+" "+field+" "+Lan.g(this,"changed from")+" '"+oldVal+"' "+Lan.g(this,"to")+" '"+newVal+"'\r\n";
	}

	///<summary>Runs Procedures.ComputeEstimates for given proc and listClaimProcs.
	///Does not update existing ClaimProcs or insert new ClaimProcs in the database.
	///Outs new ClaimProcs added by Procedures.ComputeEstimates, listClaimProcs will contain any added claimProcs.</summary>
	private void RecomputeEstimates(Procedure procedure,List<ClaimProc> listClaimProcs,out List<ClaimProc> listNewClaimProcs,
		OrthoProcLink orthoProcLink,OrthoCase orthoCase,OrthoSchedule orthoSchedule,List<OrthoProcLink> listOrthoProcLinksForOrthoCase)
	{ 
		var listClaimProcsOrig=new List<ClaimProc>(listClaimProcs);
		var patient=Patients.GetPat(procedure.PatNum);
		var listPatPlans=PatPlans.GetPatPlansForPat(patient.PatNum);
		var listInsSubs=InsSubs.GetMany(listPatPlans.Select(x => x.InsSubNum).ToList());
		var listInsPlans=InsPlans.GetByInsSubs(listInsSubs.Select(x => x.InsSubNum).ToList());
		var listBenefits=Benefits.Refresh(listPatPlans,listInsSubs);//Same method used in FormProcEdit.Load()->GetLoadData()
		//FormProcEdit uses GetHistList(....,procDate:DateTime.Today,...)
		var listClaimProcHists=ClaimProcs.GetHistList(patient.PatNum,listBenefits,listPatPlans,listInsPlans,DateTime.Today,listInsSubs);
		var isSaveToDb=false;
		//When isSaveToDb is true any estimates that are missing will be inserted into DB and it refreshes listClaimProcs from the Db.
		//Refreshing the list results in losing the changes to ProvNum and ProcDate.
		Procedures.ComputeEstimates(procedure,patient.PatNum,ref listClaimProcs,false,listInsPlans,listPatPlans,listBenefits,listClaimProcHists,
			[],isSaveToDb,patient.Age,listInsSubs
			,useProcDateOnProc:true,
			orthoProcLink:orthoProcLink,orthoCase:orthoCase,orthoSchedule:orthoSchedule,listOrthoProcLinksForOrthoCase:listOrthoProcLinksForOrthoCase);
		listNewClaimProcs=listClaimProcs.Except(listClaimProcsOrig).ToList();
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(!EntriesAreValid()) {
			return;
		}
		//This list can only contain a single patNum.
		var patNum=ListProcedures[0].PatNum;
		var dateProc=DateTime.MinValue;
		if(textDate.Text!="" && ListProcedures.Any(x => x.ProcDate!=SIn.Date(textDate.Text))) {
			dateProc=SIn.Date(textDate.Text);
		}
		#region Update ProcList and DB to reflect selections
		var listClaimProcsAll=ClaimProcs.RefreshForProcs(ListProcedures.Select(x => x.ProcNum).ToList());
		//Get data for any OrthoCases that may be linked to procs in ProcList
		var listOrthoCases=OrthoCases.Refresh(patNum);
		var listOrthoProcLinksAllForPat=OrthoProcLinks.GetManyByOrthoCases(listOrthoCases.Select(x=>x.OrthoCaseNum).ToList());
		var listProcNums=ListProcedures.Select(x=>x.ProcNum).ToList();
		var listOrthoProcLinks=listOrthoProcLinksAllForPat.FindAll(x=>listProcNums.Contains(x.ProcNum));
		var listOrthoSchedules=new List<OrthoSchedule>();
		if(listOrthoProcLinks.Count>0) {
			var listSchedulePlanLinksFKey=OrthoPlanLinks.GetAllForOrthoCasesByType(listOrthoCases.Select(x=>x.OrthoCaseNum).ToList(),OrthoPlanLinkType.OrthoSchedule).Select(x=>x.FKey).ToList();
			listOrthoSchedules=OrthoSchedules.GetMany(listSchedulePlanLinksFKey);
		}
		var changeFees=false;
		var provider=comboProv.GetSelected<Provider>();
		var procFeeHelper=new ProcFeeHelper(patNum);
		if(provider!=null) {
			var listProceduresNew=new List<Procedure>();
			var listProceduresOld=new List<Procedure>();
			for(var i=0;i<ListProcedures.Count;i++) {
				var procedure=ListProcedures[i].Copy();
				procedure.ProvNum=provider.ProvNum;
				//Add the procedure to both lists only if the provider number has changed.
				if(ListProcedures[i].ProvNum!=procedure.ProvNum) {
					listProceduresOld.Add(ListProcedures[i]);
					listProceduresNew.Add(procedure);
				}
			}
			//Check to see if the ProcFee will change due to the provider changing.
			var promptText="";
			procFeeHelper.FillData();
			var patient=Patients.GetPat(patNum);
			var feeSchedNum=FeeScheds.GetFeeSched(patient,procFeeHelper.ListInsPlans,procFeeHelper.ListPatPlans,procFeeHelper.ListInsSubs,provider.ProvNum);
			var clinic=comboClinic.GetSelected<ClinicDto>();
			if(clinic==null) {
				procFeeHelper.ListFees=null;
			}
			else {
				procFeeHelper.ListFees=Fees.GetListExact(feeSchedNum,clinic.Id,provider.ProvNum);
				if(procFeeHelper.ListFees.Count==0) {
					procFeeHelper.ListFees=null;
				}
			}
			var canChangeFees=Procedures.ShouldFeesChange(listProceduresNew,listProceduresOld,ref promptText,procFeeHelper);
			if(canChangeFees) {
				if(promptText=="") {//No prompt, so change fees because canFeesChange==true.
					changeFees=true;
				}
				else {//Show the prompt if not already shown, and change fees if user picks 'Yes'.
					changeFees=MsgBox.Show(MsgBoxButtons.YesNo,promptText);
				}
			}
		}
		for(var i=0;i<ListProcedures.Count;i++) {
			var hasChanged=false;
			var hasDateChanged=false;
			var listClaimProcsForProc = ClaimProcs.GetForProc(listClaimProcsAll,ListProcedures[i].ProcNum);
			var procedureOld=_listProceduresOld.Find(x => x.ProcNum==ListProcedures[i].ProcNum);//this shouldn't fail, it needs to be in this list.
			if(dateProc!=DateTime.MinValue && ListProcedures[i].ProcDate!=dateProc) {//Using value entered in the textbox.
				ListProcedures[i].ProcDate=dateProc;
				//ClaimProc.ProcDate will be "synched" in Procedures.ComputeEstimates(), but only if not attached to a claim.  Mimics FormprocEdit.
				hasDateChanged=true;
				hasChanged=true;
			}
			if(provider!=null && provider.ProvNum!=ListProcedures[i].ProvNum) {//Using selection
				ListProcedures[i].ProvNum=provider.ProvNum;
				//Mimics FormProcEdit, uses different criteria than Procedures.ComputeEstimates().
				ClaimProcs.TrySetProvFromProc(ListProcedures[i],listClaimProcsForProc);
				hasChanged=true;
			}
			if(comboClinic.GetSelected<ClinicDto>()!=null && comboClinic.GetSelected<ClinicDto>().Id!=ListProcedures[i].ClinicNum) {//Using selection
				ListProcedures[i].ClinicNum=comboClinic.GetSelected<ClinicDto>().Id;
				listClaimProcsForProc.ForEach(x => x.ClinicNum=ListProcedures[i].ClinicNum);
				hasChanged=true;
			}
			if(changeFees) {
				ListProcedures[i].ProcFee=Procedures.GetProcFee(procFeeHelper.Pat,procFeeHelper.ListPatPlans,procFeeHelper.ListInsSubs,
					procFeeHelper.ListInsPlans,ListProcedures[i],procFeeHelper.ListBenefitsPrimary,procFeeHelper.ListFees);
				hasChanged=true;
			}
			if(hasChanged) {
				Procedures.Update(ListProcedures[i],procedureOld);
				if(hasDateChanged) {
					List<ClaimProc> listClaimProcs;
					//If proc is linked to an OrthoCase, update dates for OrthoCase.
					OrthoCase orthoCase=null;
					OrthoSchedule orthoSchedule=null;
					List<OrthoProcLink> listOrthoProcLinksForOrthoCase=null;
					var orthoProcLink=listOrthoProcLinks.Find(x=>x.ProcNum==ListProcedures[i].ProcNum);
					//If proc is linked to an OrthoCase, get other OrthoCase data needed to update estimates and update dates for OrthoCase.
					if(orthoProcLink!=null) { 
						OrthoCases.UpdateDatesByLinkedProc(orthoProcLink,ListProcedures[i]);
						var orthoCaseNum=orthoProcLink.OrthoCaseNum;
						orthoCase=listOrthoCases.Find(x=>x.OrthoCaseNum==orthoCaseNum);
						orthoSchedule=listOrthoSchedules.Find(x=>x.OrthoScheduleNum==orthoCaseNum);
						//if either is null, no problem
						listOrthoProcLinksForOrthoCase=listOrthoProcLinksAllForPat.FindAll(x=>x.OrthoCaseNum==orthoCaseNum);
					}
					RecomputeEstimates(ListProcedures[i],listClaimProcsForProc,out listClaimProcs,orthoProcLink,orthoCase,orthoSchedule,listOrthoProcLinksForOrthoCase);
					ClaimProcs.InsertMany(listClaimProcs);//Only insert claimProcs that were newly added.
				}
			}
		}
		ClaimProcs.UpdateMany(listClaimProcsAll);//Does not contain new claimProcs.
		Recalls.Synch(patNum);
		#endregion
		#region Security Log Entries
		var logTextProcComplete ="";//To make security log for procs that were C (specific permission)
		var logTextProcExisting="";//To make a security log for procs that were EO,EC (specific permission) 
		var logTextProcOther="";//All other procedures (general permission)
		for(var i=0;i<ListProcedures.Count;i++) {
			var procedureOld=_listProceduresOld.FirstOrDefault(x => x.ProcNum==ListProcedures[i].ProcNum);
			if(procedureOld==null) {
				continue;
			}
			//If proc has not been changed then a blank string will be returned.
			//Only changed procs will be reflected in security log entries.
			switch(procedureOld.ProcStatus) {
				case ProcStat.C:
					logTextProcComplete+=ConstructSecurityLogForProcType(ListProcedures[i],procedureOld);
					break;
				case ProcStat.EO:
				case ProcStat.EC:
					logTextProcExisting+=ConstructSecurityLogForProcType(ListProcedures[i],procedureOld);
					break;
				default:
					logTextProcOther+=ConstructSecurityLogForProcType(ListProcedures[i],procedureOld);
					break;
			}
		}
		if(logTextProcComplete!="") {
			SecurityLogs.MakeLogEntry(EnumPermType.ProcCompleteEdit,patNum,logTextProcComplete);
		}
		if(logTextProcExisting!="") {
			SecurityLogs.MakeLogEntry(EnumPermType.ProcExistingEdit,patNum,logTextProcExisting);
		}
		if(logTextProcOther!="") {
			SecurityLogs.MakeLogEntry(EnumPermType.ProcEdit,patNum,logTextProcOther);
		}
		#endregion
		DialogResult=DialogResult.OK;
	}

	private enum SecurityLogFields {
		ProcDate,
		ProvNum,
		ClinicNum
	}

}