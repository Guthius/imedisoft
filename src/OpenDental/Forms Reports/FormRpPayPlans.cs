using System;
using System.Drawing;
using System.Windows.Forms;
using OpenDental.ReportingComplex;
using OpenDentBusiness;
using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Features.Providers.Dtos;

namespace OpenDental;

/// <summary>
/// Summary description for FormRpApptWithPhones.
/// </summary>
public partial class FormRpPayPlans:FormODBase {
	private List<ClinicDto> _listClinics;
	//private int pagesPrinted;
	private ErrorProvider errorProvider1=new ErrorProvider();
	//private DataTable BirthdayTable;
	//private int patientsPrinted;
	//private PrintDocument pd;
	//private OpenDental.UI.PrintPreview printPreview;
	private List<ProviderDto> _listProviders;

		
	public FormRpPayPlans()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormRpPayPlans_Load(object sender, System.EventArgs e){
		dateStart.Value=DateTime.Today;
		dateEnd.Value=DateTime.Today;
		checkHideCompletePlans.Checked=true;
		_listProviders=Providers.GetListReports();
		listProv.Items.AddList(_listProviders,x => x.Description);
		listProv.SetAll(true);
		checkAllProv.Checked=true;
		_listClinics=Clinics.GetForUserod(Security.CurUser);
		if(!Security.CurUser.ClinicIsRestricted) {
			listClin.Items.Add(Lan.g(this,"Unassigned"));
			listClin.SetSelected(0);
		}
		for(var i=0;i<_listClinics.Count;i++) {
			listClin.Items.Add(_listClinics[i].Abbr);
			if(Clinics.ClinicNum==0) {
				listClin.SetSelected(listClin.Items.Count-1);
				checkAllClin.Checked=true;
			}
			if(_listClinics[i].Id==Clinics.ClinicNum) {
				listClin.SelectedIndices.Clear();
				listClin.SetSelected(listClin.Items.Count-1);
			}
		}
	}

	private void checkAllProv_Click(object sender,EventArgs e) {
		if(checkAllProv.Checked) {
			listProv.SetAll(true);
		}
		else {
			listProv.ClearSelected();
		}
	}

	private void listProv_Click(object sender,EventArgs e) {
		if(listProv.SelectedIndices.Count>0) {
			checkAllProv.Checked=false;
		}
	}

	private void checkAllClin_Click(object sender,EventArgs e) {
		if(checkAllClin.Checked) {
			listClin.SetAll(true);
		}
		else {
			listClin.ClearSelected();
		}
	}

	private void checkHasDateRange_Click(object sender,EventArgs e) {
		if(checkHasDateRange.Checked) {
			dateStart.Enabled=true;
			dateEnd.Enabled=true;
		}
		else {
			dateStart.Enabled=false;
			dateEnd.Enabled=false;
		}
	}

	private void listClin_Click(object sender,EventArgs e) {
		if(listClin.SelectedIndices.Count>0) {
			checkAllClin.Checked=false;
		}
	}

	private void butOK_Click(object sender,System.EventArgs e) {
		if(listProv.SelectedIndices.Count==0) {
			MsgBox.Show(this,"Please select at least one provider.");
			return;
		}
		if(true) {//Using clinics
			if(listClin.SelectedIndices.Count==0) {
				MsgBox.Show(this,"Please select at least one clinic.");
				return;
			}
		}
		if(dateStart.Value>dateEnd.Value) {
			MsgBox.Show(this,"Start date cannot be greater than the end date.");
			return;
		}
		var report=new ReportComplex(true,true);
		var listProvNums=new List<long>();
		var listClinicNums=new List<long>();
		for(var i=0;i<listProv.SelectedIndices.Count;i++) {
			listProvNums.Add(_listProviders[listProv.SelectedIndices[i]].Id);
		}
		if(checkAllProv.Checked) {
			for(var i=0;i<_listProviders.Count;i++) {
				listProvNums.Add(_listProviders[i].Id);
			}
		}
		if(true) {
			for(var i=0;i<listClin.SelectedIndices.Count;i++) {
				if(Security.CurUser.ClinicIsRestricted) {
					listClinicNums.Add(_listClinics[listClin.SelectedIndices[i]].Id);//we know that the list is a 1:1 to _listClinics
				}
				else {
					if(listClin.SelectedIndices[i]==0) {
						listClinicNums.Add(0);
					}
					else {
						listClinicNums.Add(_listClinics[listClin.SelectedIndices[i]-1].Id);//Minus 1 from the selected index
					}
				}
			}
			if(checkAllClin.Checked) {//All Clinics selected; add all visible or hidden unrestricted clinics to the list
				listClinicNums=listClinicNums.Union(Clinics.GetAllForUserod(Security.CurUser).Select(x => x.Id)).ToList();
			}
		}
		DisplayPayPlanType displayPayPlanType;
		if(radioInsurance.Checked) {
			displayPayPlanType=DisplayPayPlanType.Insurance;
		}
		else if(radioPatient.Checked) {
			displayPayPlanType=DisplayPayPlanType.Patient;
		}
		else {
			displayPayPlanType=DisplayPayPlanType.Both;
		}
		var isPayPlanV2=(PrefC.GetInt(PrefName.PayPlansVersion)==2);
		var ds=RpPayPlan.GetPayPlanTable(dateStart.Value,dateEnd.Value,listProvNums,listClinicNums,checkAllProv.Checked
			,displayPayPlanType,checkHideCompletePlans.Checked,checkShowFamilyBalance.Checked,checkHasDateRange.Checked,isPayPlanV2);
		var table=ds.Tables["Clinic"];
		var tableTotal=ds.Tables["Total"];
		var font=new Font("Tahoma",9);
		var fontTitle=new Font("Tahoma",17,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"PaymentPlans");
		report.AddTitle("Title",Lan.g(this,"Payment Plans"),fontTitle);
		report.AddSubTitle("PracticeTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		if(checkHasDateRange.Checked) {
			report.AddSubTitle("Date SubTitle",dateStart.Value.ToShortDateString()+" - "+dateEnd.Value.ToShortDateString(),fontSubTitle);
		}
		else{
			report.AddSubTitle("Date SubTitle",DateTime.Today.ToShortDateString(),fontSubTitle);
		}
		QueryObject query;
		query=report.AddQuery(table,"","clinicName",SplitByKind.Value,1,true);
		query.AddColumn("Provider",160,FieldValueType.String,font);
		query.AddColumn("Guarantor",160,FieldValueType.String,font);
		query.AddColumn("Ins",40,FieldValueType.String,font);
		query.GetColumnHeader("Ins").ContentAlignment=ContentAlignment.MiddleCenter;
		query.GetColumnDetail("Ins").ContentAlignment=ContentAlignment.MiddleCenter;
		query.AddColumn("Princ",100,FieldValueType.Number,font);
		query.GetColumnHeader("Princ").ContentAlignment=ContentAlignment.MiddleRight;
		query.GetColumnDetail("Princ").ContentAlignment=ContentAlignment.MiddleRight;
		query.AddColumn("Accum Int",100,FieldValueType.Number,font);
		query.GetColumnHeader("Accum Int").ContentAlignment=ContentAlignment.MiddleRight;
		query.GetColumnDetail("Accum Int").ContentAlignment=ContentAlignment.MiddleRight;
		query.AddColumn("Paid",100,FieldValueType.Number,font);
		query.GetColumnHeader("Paid").ContentAlignment=ContentAlignment.MiddleRight;
		query.GetColumnDetail("Paid").ContentAlignment=ContentAlignment.MiddleRight;
		query.AddColumn("Balance",100,FieldValueType.Number,font);
		query.GetColumnHeader("Balance").ContentAlignment=ContentAlignment.MiddleRight;
		query.GetColumnDetail("Balance").ContentAlignment=ContentAlignment.MiddleRight;
		query.AddColumn("Due Now",100,FieldValueType.Number,font);
		query.GetColumnHeader("Due Now").ContentAlignment=ContentAlignment.MiddleRight;
		query.GetColumnDetail("Due Now").ContentAlignment=ContentAlignment.MiddleRight;
		if(isPayPlanV2) {
			query.AddColumn("Bal Not Due",100,FieldValueType.Number,font);
			query.GetColumnHeader("Bal Not Due").ContentAlignment=ContentAlignment.MiddleRight;
			query.GetColumnDetail("Bal Not Due").ContentAlignment=ContentAlignment.MiddleRight;
		}
		if(checkShowFamilyBalance.Checked) {
			query.AddColumn("Fam Balance",100,FieldValueType.String,font);
			query.GetColumnHeader("Fam Balance").ContentAlignment=ContentAlignment.MiddleRight;
			query.GetColumnDetail("Fam Balance").ContentAlignment=ContentAlignment.MiddleRight;
			query.GetColumnDetail("Fam Balance").SuppressIfDuplicate=true;
		}
		if(true) {
			var queryTotals=report.AddQuery(tableTotal,"Totals");
			queryTotals.AddColumn("Clinic",360,FieldValueType.String,font);
			queryTotals.AddColumn("Princ",100,FieldValueType.Number,font);
			queryTotals.GetColumnHeader("Princ").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.GetColumnDetail("Princ").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.AddColumn("Accum Int",100,FieldValueType.Number,font);
			queryTotals.GetColumnHeader("Accum Int").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.GetColumnDetail("Accum Int").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.AddColumn("Paid",100,FieldValueType.Number,font);
			queryTotals.GetColumnHeader("Paid").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.GetColumnDetail("Paid").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.AddColumn("Balance",100,FieldValueType.Number,font);
			queryTotals.GetColumnHeader("Balance").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.GetColumnDetail("Balance").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.AddColumn("Due Now",100,FieldValueType.Number,font);
			queryTotals.GetColumnHeader("Due Now").ContentAlignment=ContentAlignment.MiddleRight;
			queryTotals.GetColumnDetail("Due Now").ContentAlignment=ContentAlignment.MiddleRight;
			if(isPayPlanV2) {
				queryTotals.AddColumn("Bal Not Due",100,FieldValueType.Number,font);
				queryTotals.GetColumnHeader("Bal Not Due").ContentAlignment=ContentAlignment.MiddleRight;
				queryTotals.GetColumnDetail("Bal Not Due").ContentAlignment=ContentAlignment.MiddleRight;
			}
			if(checkShowFamilyBalance.Checked) {
				queryTotals.AddColumn("Fam Balance",100,FieldValueType.String,font);
				queryTotals.GetColumnHeader("Fam Balance").ContentAlignment=ContentAlignment.MiddleRight;
				queryTotals.GetColumnDetail("Fam Balance").ContentAlignment=ContentAlignment.MiddleRight;
				queryTotals.GetColumnDetail("Fam Balance").SuppressIfDuplicate=true;
			}
		}
		if(!report.SubmitQueries()) {
			return;
		}
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}

}