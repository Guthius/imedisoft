using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Features.Providers.Dtos;

namespace OpenDental;

public partial class FormRpNewPatients:FormODBase {
	private List<ProviderDto> _listProviders;

		
	public FormRpNewPatients() {
		InitializeComponent();
	}

	private void FormNewPatients_Load(object sender, System.EventArgs e) {
		_listProviders=Providers.GetListReports();
		textToday.Text=DateTime.Today.ToShortDateString();
		//always defaults to the current month
		textDateFrom.Text=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).ToShortDateString();
		textDateTo.Text=new DateTime(DateTime.Today.Year,DateTime.Today.Month
			,DateTime.DaysInMonth(DateTime.Today.Year,DateTime.Today.Month)).ToShortDateString();
		listProv.Items.Add(Lan.g(this,"all"));
		for(var i=0;i<_listProviders.Count;i++){
			listProv.Items.Add(_listProviders[i].Description);
		}
		listProv.SetSelected(0);
	}

	private void butThis_Click(object sender,EventArgs e) {
		textDateFrom.Text=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).ToShortDateString();
		textDateTo.Text=new DateTime(DateTime.Today.Year,DateTime.Today.Month
			,DateTime.DaysInMonth(DateTime.Today.Year,DateTime.Today.Month)).ToShortDateString();
	}

	private void butLeft_Click(object sender,EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		var dateFrom=SIn.Date(textDateFrom.Text);
		if(dateFrom.Year < 1880) {
			MsgBox.Show(this,"Please fix the From date first.");
			return;
		}
		var dateTo=SIn.Date(textDateTo.Text);
		var toLastDay=false;
		if(CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month)==dateTo.Day){
			toLastDay=true;
		}
		textDateFrom.Text=dateFrom.AddMonths(-1).ToShortDateString();
		textDateTo.Text=dateTo.AddMonths(-1).ToShortDateString();
		dateTo=SIn.Date(textDateTo.Text);
		if(toLastDay){
			textDateTo.Text=new DateTime(dateTo.Year,dateTo.Month,
					CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month))
				.ToShortDateString();
		}
	}

	private void butRight_Click(object sender,EventArgs e) {
		if(!textDateFrom.IsValid()|| !textDateTo.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		var dateFrom=SIn.Date(textDateFrom.Text);
		var dateTo=SIn.Date(textDateTo.Text);
		var toLastDay=false;
		if(CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month)==dateTo.Day){
			toLastDay=true;
		}
		textDateFrom.Text=dateFrom.AddMonths(1).ToShortDateString();
		textDateTo.Text=dateTo.AddMonths(1).ToShortDateString();
		dateTo=SIn.Date(textDateTo.Text);
		if(toLastDay){
			textDateTo.Text=new DateTime(dateTo.Year,dateTo.Month,
					CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month))
				.ToShortDateString();
		}
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		if(listProv.SelectedIndices.Count==0) {
			MsgBox.Show(this,"At least one provider must be selected.");
			return;
		}
		if(listProv.SelectedIndices[0]==0 && listProv.SelectedIndices.Count>1){
			MsgBox.Show(this,"You cannot select 'all' providers as well as specific providers.");
			return;
		}
		var dateFrom=SIn.Date(textDateFrom.Text);
		var dateTo=SIn.Date(textDateTo.Text);
		if(dateTo<dateFrom) {
			MsgBox.Show(this,"To date cannot be before From date.");
			return;
		}
		ReportComplex report;
		if(checkAddress.Checked) {
			report=new ReportComplex(true,true);
		}
		else {
			report=new ReportComplex(true,false);
		}
		var listProvNums=new List<long>();
		var listProvs=Providers.GetListReports();
		var subtitleProvs="";
		if(listProv.SelectedIndices[0]==0) {//'All' is selected
			for(var i=0;i<listProvs.Count;i++) {
				listProvNums.Add(listProvs[i].Id);
				subtitleProvs=Lan.g(this,"All Providers");
			}
		}
		else {
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				listProvNums.Add(listProvs[listProv.SelectedIndices[i]-1].Id);//Minus 1 from the selected index to account for 'All' option
				if(i>0) {
					subtitleProvs+=", ";
				}
				subtitleProvs+=listProvs[listProv.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index to account for 'All' option
			}
		}
		var table=RpNewPatients.GetNewPatients(dateFrom,dateTo,listProvNums,checkAddress.Checked,checkProd.Checked,listProv.SelectedIndices[0]==0);
		var font=new Font("Tahoma",9);
		var fontBold=new Font("Tahoma",9,FontStyle.Bold);
		var fontTitle=new Font("Tahoma",17,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"New Patients");
		report.AddTitle("Title",Lan.g(this,"New Patients"),fontTitle);
		report.AddSubTitle("Practice Title",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		report.AddSubTitle("Providers",subtitleProvs,fontSubTitle);
		report.AddSubTitle("Dates of Report",dateFrom.ToString("d")+" - "+dateTo.ToString("d"),fontSubTitle);
		var query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
		query.AddColumn(Lan.g(this,"#"),40,FieldValueType.String,font);
		query.AddColumn(Lan.g(this,"Date"),90,FieldValueType.Date,font);
		query.AddColumn(Lan.g(this,"Last Name"),120,FieldValueType.String,font);
		query.AddColumn(Lan.g(this,"First Name"),120,FieldValueType.String,font);
		query.AddColumn(Lan.g(this,"Referral"),140,FieldValueType.String,font);
		query.AddColumn(Lan.g(this,"Production Fee"),90,FieldValueType.Number,font);
		if(checkAddress.Checked){
			query.AddColumn(Lan.g(this,"Pref'd"),90,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Address"),100,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Add2"),80,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"City"),100,FieldValueType.String,font); 
			query.AddColumn(Lan.g(this,"ST"),30,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Zip"),55,FieldValueType.String,font);
		}
		report.AddPageNum(font);
		if(!report.SubmitQueries()) {
			return;
		}
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}

}