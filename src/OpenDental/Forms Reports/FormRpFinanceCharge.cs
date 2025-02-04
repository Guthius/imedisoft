using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenDental.ReportingComplex;
using OpenDentBusiness;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Features.Providers.Dtos;

namespace OpenDental;

public partial class FormRpFinanceCharge : FormODBase {
	private List<ProviderDto> _listProviders= [];
	private List<Def> _listBillingTypeDefs= [];

		
	public FormRpFinanceCharge(){
		InitializeComponent();
	}

	private void FormRpFinanceCharge_Load(object sender, System.EventArgs e) {
		textDateFrom.Text=PrefC.GetDate(PrefName.FinanceChargeLastRun).ToShortDateString();
		textDateTo.Text=PrefC.GetDate(PrefName.FinanceChargeLastRun).ToShortDateString();
		_listProviders=Providers.GetListReports();
		_listBillingTypeDefs=Defs.GetDefsForCategory(DefCat.BillingTypes,true);
		listBillingType.Items.AddList(_listBillingTypeDefs,x => x.ItemName);
		if(listBillingType.Items.Count>0) {
			listBillingType.SelectedIndex=0;
		}
		listProv.Items.AddList(_listProviders,x => x.Description,x => x.Abbr);
		if(listProv.Items.Count>0) {
			listProv.SelectedIndex=0; 
		}
		checkAllProv.Checked=true;
		checkAllBilling.Checked=true;
		listProv.Visible=false;
		listBillingType.Visible=false;
	}

	private void checkAllProv_Click(object sender,EventArgs e) {
		listProv.Visible=!checkAllProv.Checked;
	}

	private void checkAllBilling_Click(object sender,EventArgs e) {
		listBillingType.Visible=!checkAllBilling.Checked;
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			MsgBox.Show(this,"Please fix data entry errors first.");
			return;
		}
		var dateFrom=SIn.Date(textDateFrom.Text);
		var dateTo=SIn.Date(textDateTo.Text);
		if(dateTo<dateFrom) {
			MsgBox.Show(this,"To date cannot be before From date.");
			return;
		}
		var report=new ReportComplex(true,false);
		var listProvNums = new List<long>();
		if(!checkAllProv.Checked) {
			listProvNums.AddRange(listProv.SelectedIndices.Select(x => _listProviders[x].Id).ToList());
		}
		var listBillingDefNums= new List<long>();
		if(!checkAllBilling.Checked) {
			listBillingDefNums.AddRange(listBillingType.SelectedIndices.Select(x => _listBillingTypeDefs[x].DefNum).ToList());
		}
		var table=RpFinanceCharge.GetFinanceChargeTable(dateFrom,dateTo,PrefC.GetLong(PrefName.FinanceChargeAdjustmentType),listProvNums,listBillingDefNums);
		var font=new Font("Tahoma",9);
		var fontTitle=new Font("Tahoma",17,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"Finance Charge Report");
		report.AddTitle("Title",Lan.g(this,"Finance Charge Report"),fontTitle);
		report.AddSubTitle("PracticeTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		report.AddSubTitle("Date SubTitle",dateFrom.ToString("d")+" - "+dateTo.ToString("d"),fontSubTitle);
		var subtitleProvs="";
		if(listProvNums.Count>0) {
			subtitleProvs+=listProv.GetStringSelectedItems(true);
		}
		else {
			subtitleProvs=Lan.g(this,"All Providers");
		}
		report.AddSubTitle("Provider Subtitle",subtitleProvs);
		var subtBillingTypes="";
		if(listBillingDefNums.Count>0) {
			subtBillingTypes+=listBillingType.GetStringSelectedItems();
		}
		else {
			subtBillingTypes=Lan.g(this,"All Billing Types");
		}
		report.AddSubTitle("Billing Subtitle",subtBillingTypes);
		var query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
		query.AddColumn("PatNum",75);
		query.AddColumn("Patient Name",180);
		query.AddColumn("Preferred Name",130);
		query.AddColumn("Amount",100,FieldValueType.Number);
		query.GetColumnDetail("Amount").ContentAlignment=ContentAlignment.MiddleRight;
		report.AddPageNum(font);
		if(!report.SubmitQueries()) {
			return;
		}
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}

}