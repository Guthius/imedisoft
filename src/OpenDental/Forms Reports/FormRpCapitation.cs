using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDental.ReportingComplex;
using OpenDentBusiness;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental;

public partial class FormRpCapitation : FormODBase {

		
	public FormRpCapitation()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormRpCapitation_Load(object sender, System.EventArgs e) {
		var today = DateTime.Today;
		var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
		textDateStart.Text=new DateTime(today.Year,today.Month,1).ToShortDateString();
		textDateEnd.Text=endOfMonth.ToShortDateString();
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		ExecuteReport();
	}

	private void ExecuteReport(){
		DateTime dateStart;
		DateTime dateEnd;
		bool isMedOrClinic;
		if(!DateTime.TryParse(textDateStart.Text,out dateStart)) {
			MsgBox.Show(this,"Please input a valid date.");
			return;
		}
		if(!DateTime.TryParse(textDateEnd.Text,out dateEnd)) {
			MsgBox.Show(this,"Please input a valid date.");
			return;
		}
		if(string.IsNullOrWhiteSpace(textCarrier.Text)) {
			MsgBox.Show(this,"Carrier can not be blank. Please input a value for carrier.");
			return;
		}
		var report=new ReportComplex(true,true);
		if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
			isMedOrClinic=true;
		}
		else {
			isMedOrClinic=false;
		}
		var font=new Font("Tahoma",9);
		var fontTitle=new Font("Tahoma",17,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.AddTitle("Title",Lan.g(this,"Capitation Utilization"),fontTitle);
		report.AddSubTitle("PracTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		report.AddSubTitle("Date",textDateStart.Text+" - "+textDateEnd.Text,fontSubTitle);
		var table=RpCapitation.GetCapitationTable(dateStart,dateEnd,textCarrier.Text,isMedOrClinic);
		var query=report.AddQuery(table,"","",SplitByKind.None,1,true);
		query.AddColumn("Carrier",150,FieldValueType.String,font);
		query.GetColumnDetail("Carrier").SuppressIfDuplicate=true;
		query.AddColumn("Subscriber",120,FieldValueType.String,font);
		query.GetColumnDetail("Subscriber").SuppressIfDuplicate=true;
		query.AddColumn("Subsc SSN",70,FieldValueType.String,font);
		query.GetColumnDetail("Subsc SSN").SuppressIfDuplicate=true;
		query.AddColumn("Patient",120,FieldValueType.String,font);
		query.AddColumn("Pat DOB",80,FieldValueType.Date,font);
		if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
			query.AddColumn("Code",140,FieldValueType.String,font);
			query.AddColumn("Proc Description",120,FieldValueType.String,font);
			query.AddColumn("Date",80,FieldValueType.Date,font);
			query.AddColumn("UCR Fee",60,FieldValueType.Number,font);
			query.AddColumn("Co-Pay",60,FieldValueType.Number,font);
		}
		else {
			query.AddColumn("Code",50,FieldValueType.String,font);
			query.AddColumn("Proc Description",120,FieldValueType.String,font);
			query.AddColumn("Tth",30,FieldValueType.String,font);
			query.AddColumn("Surf",40,FieldValueType.String,font);
			query.AddColumn("Date",80,FieldValueType.Date,font);
			query.AddColumn("UCR Fee",70,FieldValueType.Number,font);
			query.AddColumn("Co-Pay",70,FieldValueType.Number,font);
		}
		if(!report.SubmitQueries()) {
			//DialogResult=DialogResult.Cancel;
			return;
		}
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}

}