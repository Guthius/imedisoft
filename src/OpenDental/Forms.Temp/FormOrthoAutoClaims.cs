using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental;

public partial class FormOrthoAutoClaims:FormODBase {
	///<summary>OutstandingAutoClaims</summary>
	private DataTable _table;

	public FormOrthoAutoClaims() {
		InitializeComponent();
	}

	private void FormAutoOrtho_Load(object sender,EventArgs e) {
		_table=PatPlans.GetOutstandingOrtho();
		if(!Security.CurUser.ClinicIsRestricted) {
			comboClinics.IncludeAll=true;
		}
		if(comboClinics.IncludeAll && Clinics.ClinicNum==0) {
			comboClinics.IsAllSelected=true;
		}
		else {
			comboClinics.ClinicNumSelected=Clinics.ClinicNum;
		}
		FillGrid();
	}

	private void FillGrid() {
		long clinicNum = 0;
		if(!comboClinics.IsAllSelected) {
			clinicNum=comboClinics.ClinicNumSelected;
		}
		var clinicWidth=80;
		var patientWidth=180;
		var carrierWidth=220;
		if(true) {
			patientWidth=140;
			carrierWidth=200;
		}
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		var col=new GridColumn(Lan.g("TableAutoOrthoClaims","Patient"),patientWidth);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","Carrier"),carrierWidth);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","TxMonths"),70);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","Banding"),80,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","MonthsRem"),100);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","#Sent"),60);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","LastSent"),80,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAutoOrthoClaims","NextClaim"),80,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		if(true) { //clinics is turned on
			col=new GridColumn(Lan.g("TableAutoOrthoClaims","Clinic"),clinicWidth,HorizontalAlignment.Center);
			gridMain.Columns.Add(col);
		}
		gridMain.ListGridRows.Clear();
		GridRow row;
		for(var i=0;i<_table.Rows.Count;i++) {
			//need a check for if clinics is on here
			if(true //Clinics are enabled
			   && (Security.CurUser.ClinicIsRestricted || !comboClinics.IsAllSelected) 
			   && clinicNum!=SIn.Long(_table.Rows[i]["ClinicNum"].ToString()))   //currently selected clinic doesn't match the row's clinic
			{
				continue;
			}
			row=new GridRow();
			var dateLastSeen=SIn.Date(_table.Rows[i]["LastSent"].ToString());
			var dateBanding=SIn.Date(_table.Rows[i]["DateBanding"].ToString());
			var dateNextClaim=SIn.Date(_table.Rows[i]["OrthoAutoNextClaimDate"].ToString());
			var dateSpanMonthsRem=new DateSpan(SIn.Date(_table.Rows[i]["DateBanding"].ToString()).AddMonths(SIn.Int(_table.Rows[i]["MonthsTreat"].ToString())),DateTime.Today);
			row.Cells.Add(SIn.String(_table.Rows[i]["Patient"].ToString()));
			row.Cells.Add(SIn.String(_table.Rows[i]["CarrierName"].ToString()));
			row.Cells.Add(SIn.String(_table.Rows[i]["MonthsTreat"].ToString()));
			row.Cells.Add(dateBanding.Year < 1880 ? "" : dateBanding.ToShortDateString());//add blank if there is no banding
			if(dateBanding.Year < 1880) { //add blank if there is no banding
				row.Cells.Add("");
			}
			else {
				row.Cells.Add(((dateSpanMonthsRem.YearsDiff * 12) + dateSpanMonthsRem.MonthsDiff)+" "+Lan.g(this,"months")
				              +", "+dateSpanMonthsRem.DaysDiff +" "+Lan.g(this,"days"));
			}
			row.Cells.Add(SIn.String(_table.Rows[i]["NumSent"].ToString()));
			row.Cells.Add(dateLastSeen.Year < 1880 ? "" : dateLastSeen.ToShortDateString());
			row.Cells.Add(dateNextClaim.Year < 1880 ? "" : dateNextClaim.ToShortDateString());
			if(true) { //clinics is turned on
				//Use the long list of clinics so that hidden clinics can be shown for unrestricted users.
				row.Cells.Add(Clinics.GetAbbr(SIn.Long(_table.Rows[i]["ClinicNum"].ToString())));
			}
			row.Tag=_table.Rows[i];
			gridMain.ListGridRows.Add(row);

		}
		gridMain.EndUpdate();
	}

	private void butGenerateClaims_Click(object sender,EventArgs e) {
		if(gridMain.SelectedIndices.Count() < 1) {
			MsgBox.Show(this,"Please select the rows for which you would like to create procedures and claims.");
			return;
		}
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Are you sure you want to generate claims and procedures for all patients and insurance plans?")) {
			return;
		}
		var listPlanNums = new List<long>();
		var listPatPlanNums = new List<long>();
		var listInsSubNums = new List<long>();
		for(var i = 0;i < gridMain.SelectedIndices.Count();i++) {
			var row =(DataRow)gridMain.ListGridRows[gridMain.SelectedIndices[i]].Tag;
			listPlanNums.Add(SIn.Long(row["PlanNum"].ToString()));
			listPatPlanNums.Add(SIn.Long(row["PatPlanNum"].ToString()));
			listInsSubNums.Add(SIn.Long(row["InsSubNum"].ToString()));
		}
		var listInsPlansSelected=InsPlans.GetPlans(listPlanNums);
		var listPatPlansSelected=PatPlans.GetPatPlans(listPatPlanNums);
		var listInsSubsSelected=InsSubs.GetMany(listInsSubNums);
		var listRowsSucceeded=new List<DataRow>();
		var rowsFailed=0;
		var listPatientsFailed=new List<Patient>();
		var listBenefitsAll=Benefits.Refresh(listPatPlansSelected,listInsSubsSelected);
		var listHiddenProcCodes=new List<string>();
		for(var i = 0;i < gridMain.SelectedIndices.Count();i++) {
			var row =(DataRow)gridMain.ListGridRows[gridMain.SelectedIndices[i]].Tag;
			var patNum = SIn.Long(row["PatNum"].ToString());
			var patient = Patients.GetPat(patNum);
			var patientNote = PatientNotes.Refresh(patNum,patient.Guarantor);
			var codeNum = SIn.Long(row["AutoCodeNum"].ToString());
			var provNum = SIn.Long(row["ProvNum"].ToString());
			var clinicNum = SIn.Long(row["ClinicNum"].ToString());
			var insPlanNum = SIn.Long(row["PlanNum"].ToString());
			var patPlanNum = SIn.Long(row["PatPlanNum"].ToString());
			var insSubNum = SIn.Long(row["InsSubNum"].ToString());
			var monthsTreat = SIn.Int(row["MonthsTreat"].ToString());
			var dateTimeDue =  SIn.Date(row["OrthoAutoNextClaimDate"].ToString());
			var procCode=ProcedureCodes.GetProcCode(codeNum).ProcCode;
			if(!listHiddenProcCodes.Contains(procCode) && ProcedureCodes.AreAnyProcCodesHidden(codeNum)) {
				listHiddenProcCodes.Add(procCode);
			}
			//for each selected row
			//create a procedure
			//Procedures.CreateProcForPat(patNumCur,codeNumCur,"","",ProcStat.C,provNumCur);
			if(ProcedureCodes.AreAnyProcCodesHidden(codeNum)) {
				//We do not show a message here because it would be annoying in a loop.
				//Todo: show the message somehow at the end.
				//MsgBox.Show($"Cannot create auto ortho procedure because procedure is in a hidden category: {ProcedureCodes.GetProcCode(codeNum)}");
				rowsFailed++;
				continue;
			}
			if(provNum==0) {//Procedure was possibly deleted, 
				listPatientsFailed.Add(patient);
				continue;
			}
			var procedure = Procedures.CreateOrthoAutoProcsForPat(patNum,codeNum,provNum,clinicNum,dateTimeDue);
			var insPlan = InsPlans.GetPlan(insPlanNum,listInsPlansSelected);
			var patPlan = listPatPlansSelected.FirstOrDefault(x => x.PatPlanNum == patPlanNum);
			var insSub = listInsSubsSelected.FirstOrDefault(x => x.InsSubNum==insSubNum);
			var listBenefits=listBenefitsAll.FindAll(x => x.PatPlanNum==patPlan.PatPlanNum || x.PlanNum==insSub.PlanNum);
			//create a claimproc
			var listClaimProcs=new List<ClaimProc>();
			Procedures.ComputeEstimates(procedure,patNum,ref listClaimProcs,true, [insPlan],
				[patPlan],listBenefits,null,null,true,patient.Age, [insSub],isForOrtho:true);
			//make the feebilled == the insplan feebilled or patplan feebilled
			var feeBilled = patPlan.OrthoAutoFeeBilledOverride == -1 ? insPlan.OrthoAutoFeeBilled : patPlan.OrthoAutoFeeBilledOverride;
			//create a claim with that claimproc
			var claimType="";
			switch(patPlan.Ordinal) {
				case 1:
					claimType="P";
					break;
				case 2:
					claimType="S";
					break;
			}
			var dateSpanMonthsRem=new DateSpan(SIn.Date(row["DateBanding"].ToString()).AddMonths(SIn.Int(row["MonthsTreat"].ToString())),DateTime.Today);
			Claims.CreateClaimForOrthoProc(claimType,patPlan,insPlan,insSub,
				ClaimProcs.GetForProcWithOrdinal(procedure.ProcNum,patPlan.Ordinal),procedure,feeBilled,SIn.Date(row["DateBanding"].ToString()),
				SIn.Int(row["MonthsTreat"].ToString()),((dateSpanMonthsRem.YearsDiff * 12) + dateSpanMonthsRem.MonthsDiff));
			PatPlans.IncrementOrthoNextClaimDates(patPlan,insPlan,monthsTreat,patientNote);
			listRowsSucceeded.Add(row);
			SecurityLogs.MakeLogEntry(EnumPermType.ProcComplCreate,patient.PatNum
				,Lan.g(this,"Automatic ortho procedure and claim generated for")+" "+dateTimeDue.ToShortDateString());
		}
		var message=Lan.g(this,"Done.")+" "+Lan.g(this,"There were")+" "+listRowsSucceeded.Count+" "
		            +Lan.g(this,"claim(s) generated and")+" "+(rowsFailed+listPatientsFailed.Count)+" "+Lan.g(this,"failures")+".";
		if(listHiddenProcCodes.Count > 0) {
			message+="\r\n"+Lan.g(this,"Some failed because the following procedures are in a hidden category")+$": {string.Join(", ",listHiddenProcCodes)}";
		}
		if(listPatientsFailed.Count > 0) {
			message+="\r\n"+Lan.g(this,"Claims could not be made for these patients because of deleted ortho procedures")
			               +$": {string.Join(", ",listPatientsFailed.Select(x => x.FName+" "+x.LName).ToList())}";
		}
		MsgBox.Show(message);
		for(var i=0;i<listRowsSucceeded.Count;i++) {
			_table.Rows.Remove(listRowsSucceeded[i]);
		}
		FillGrid();
	}

	private void butSelectAll_Click(object sender,EventArgs e) {
		gridMain.SetAll(true);
	}

	private void comboClinics_SelectionChangeCommitted(object sender,EventArgs e) {
		FillGrid();
	}

}