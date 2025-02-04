using System;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormRpInsCo : FormODBase {
	private string carrier;

		
	public FormRpInsCo(){
		InitializeComponent();
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		carrier= SIn.String(textBoxCarrier.Text);
		var report=new ReportComplex(true,false);
		var table=RpInsCo.GetInsCoTable(carrier);
		var fontMain=new Font("Tahoma",8);
		var fontTitle=new Font("Tahoma",15,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"Insurance Plan List");
		report.AddTitle("Title",Lan.g(this,"Insurance Plan List"),fontTitle);
		report.AddSubTitle("PracticeTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		var query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
		query.AddColumn("Carrier Name",230,font:fontMain);
		query.AddColumn("Subscriber Name",175,font:fontMain);
		query.AddColumn("Carrier Phone#",175,font:fontMain);
		query.AddColumn("Group Name",165,font:fontMain);
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