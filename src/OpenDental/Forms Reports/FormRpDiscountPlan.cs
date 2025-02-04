using System;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormRpDiscountPlan:FormODBase {

	public FormRpDiscountPlan() {
		InitializeComponent();
	}

	private void butOK_Click(object sender,EventArgs e) {
		var report=new ReportComplex(true,false);
		var table=RpDiscountPlan.GetTable(textDescription.Text);
		var fontMain=new Font("Tahoma",8);
		var fontTitle=new Font("Tahoma",15,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"Discount Plan List");
		report.AddTitle("Title",Lan.g(this,"Discount Plan List"),fontTitle);
		report.AddSubTitle("Practice Title",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		var query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
		query.AddColumn("Description",190,font:fontMain);
		query.AddColumn("FeeSched",145,font:fontMain);
		query.AddColumn("AdjType",145,font:fontMain);
		query.AddColumn("DateEffective",75,fieldValueType:FieldValueType.Date,font:fontMain);
		query.AddColumn("DateTerm",75,fieldValueType:FieldValueType.Date,font:fontMain);
		query.AddColumn("Patient",165,font:fontMain);
		report.AddPageNum(fontMain);
		if(!report.SubmitQueries()) {
			return;
		}
		report.AddFooterText("Total","Total: "+report.TotalRows,fontMain,10,ContentAlignment.MiddleRight);
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}

}