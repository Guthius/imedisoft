using System;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormRpPrescriptions : FormODBase {

		
	public FormRpPrescriptions(){
		InitializeComponent();
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		var report=new ReportComplex(true,false);
		var table=RpPrescriptions.GetPrescriptionTable(radioPatient.Checked,textBoxInput.Text);
		var font=new Font("Tahoma",9);
		var fontTitle=new Font("Tahoma",17,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"Prescriptions");
		report.AddTitle("Title",Lan.g(this,"Prescriptions"),fontTitle);
		report.AddSubTitle("PracticeTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		if(radioPatient.Checked){
			report.AddSubTitle("By Patient","By Patient");
		}
		else{
			report.AddSubTitle("By Drug","By Drug");
		}
		var query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));			
		query.AddColumn("Patient Name",120,FieldValueType.String);
		query.AddColumn("Date",95,FieldValueType.Date);
		query.AddColumn("Drug Name",100,FieldValueType.String);
		query.AddColumn("Directions",300);
		query.AddColumn("Dispense",100);
		query.AddColumn("Prov Name",100,FieldValueType.String);
		report.AddPageNum(font);
		if(!report.SubmitQueries()) {
			return;
		}
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}
}