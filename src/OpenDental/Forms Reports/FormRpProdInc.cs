using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.ReportingComplex;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormRpProdInc : FormODBase {
	private DateTime dateFrom;
	private DateTime dateTo;
	///<summary>Can be set externally when automating.</summary>
	public string DailyMonthlyAnnual;
	///<summary>If set externally, then this sets the date on startup.</summary>
	public DateTime DateStart;
	///<summary>If set externally, then this sets the date on startup.</summary>
	public DateTime DateEnd;
	private List<ClinicDto> _listClinics;
	///<summary>Includes hidden and hidden on reports providers.
	///This is used instead of Providers.GetListReports() because we need the full list of providers when running All Providers
	///This is also so we can show provider specific information in the report.
	///Includes providers that share the same name as the provider currently logged if user has the ReportProdIncAllProviders permission.</summary>
	private List<ProviderDto> _listProviders;
	///<summary>Includes hidden providers, excludes hidden on reports providers.</summary>
	///This list directly resembles all providers that are showing within the providers list box that is showing to the user.</summary>
	private List<ProviderDto> _listFilteredProviders;

		
	public FormRpProdInc(){
		InitializeComponent();
	}

	private void FormProduction_Load(object sender, System.EventArgs e) {
		_listProviders=Providers.GetWhere(x => !x.IsDeleted);
		_listProviders.Insert(0,Providers.GetUnearnedProv());
		_listFilteredProviders= [];
		textToday.Text=DateTime.Today.ToShortDateString();
		if(!Security.IsAuthorized(EnumPermType.ReportProdIncAllProviders,true)) {
			var prov=Providers.GetFirstOrDefault(x => x.Id==Security.CurUser.ProvNum);
			if(prov!=null) {
				_listProviders=_listProviders.FindAll(x => x.FirstName == prov.FirstName && x.LastName == prov.LastName);
			}
			checkAllProv.Checked=false;
			checkAllProv.Enabled=false;
		}
		//Fill the short list of providers, ignoring those marked "hidden on reports"
		for(var i=0;i<_listProviders.Count;i++){
			if(_listProviders[i].IsHiddenFromReports) {
				continue;
			}
			listProv.Items.Add(_listProviders[i].Description);
			_listFilteredProviders.Add(_listProviders[i]);
		}
		//If the user is not allowed to run the report for all providers, default the selection to the first in the list box.
		if(checkAllProv.Enabled==false && listProv.Items.Count > 0) {
			listProv.SetSelected(0);
		}
		//If the user cannot run this report for any other provider, every single provider available in the list will be the provider logged in.
		if(!Security.IsAuthorized(EnumPermType.ReportProdIncAllProviders,true)) {
			listProv.SetAll(true);
		}
		checkClinicInfo.Checked=PrefC.GetBool(PrefName.ReportPandIhasClinicInfo);
		checkClinicBreakdown.Checked=PrefC.GetBool(PrefName.ReportPandIhasClinicBreakdown);
		_listClinics=Clinics.GetForUserod(Security.CurUser);
		if(!Security.CurUser.ClinicIsRestricted) {
			listClin.Items.Add(Lan.g(this,"Unassigned"));
			listClin.SetSelected(0);
		}
		for(var i=0;i<_listClinics.Count;i++) { //adds visible clinics
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
		switch(DailyMonthlyAnnual){
			case "Daily":
				radioDaily.Checked=true;
				break;
			case "Monthly":
				radioMonthly.Checked=true;
				break;
			case "Annual":
				radioAnnual.Checked=true;
				break;
		}
		SetDates();
		switch(PrefC.GetInt(PrefName.ReportsPPOwriteoffDefaultToProcDate)) {
			case 0: radioWriteoffPay.Checked=true; break;
			case 1: radioWriteoffProc.Checked=true; break;
			case 2: radioWriteoffBoth.Checked=true; break;
			default: radioWriteoffBoth.Checked=true; break;
		}
		if(DateStart.Year>1880){
			textDateFrom.Text=DateStart.ToShortDateString();
			textDateTo.Text=DateEnd.ToShortDateString();
			switch(DailyMonthlyAnnual) {
				case "Daily":
					RunDaily();
					break;
				case "Monthly":
					RunMonthly();
					break;
				case "Annual":
					RunAnnual();
					break;
			}
			Close();
		}
	}

	private void checkAllProv_Click(object sender,EventArgs e) {
		if(checkAllProv.Checked) {
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

	private void listClin_Click(object sender,EventArgs e) {
		if(listClin.SelectedIndices.Count>0) {
			checkAllClin.Checked=false;
		}
	}

	private void radioDaily_Click(object sender, System.EventArgs e) {
		SetDates();
	}

	private void radioMonthly_Click(object sender, System.EventArgs e) {
		SetDates();
	}

	private void radioAnnual_Click(object sender, System.EventArgs e) {
		SetDates();
	}

	private void radioProvider_Click(object sender,EventArgs e) {
		SetDates();
	}

	private void radioProviderPayroll_Click(object sender,EventArgs e) {
		SetDates();
	}

	private void SetDates(){
		if(radioDaily.Checked) {
			if(true) {
				checkClinicInfo.Visible=true;
				if(checkClinicInfo.Checked) {
					checkClinicBreakdown.Visible=true;
				}
				else {
					//Clinic info not checked so hide the clinic breakdown
					checkClinicBreakdown.Checked=false;
					checkClinicBreakdown.Visible=false;
				}
			}
			textDateFrom.Text=DateTime.Today.ToShortDateString();
			textDateTo.Text=DateTime.Today.ToShortDateString();
			butThis.Text=Lan.g(this,"Today");
		}
		else if(radioProvider.Checked) {
			if(true) {
				checkClinicInfo.Visible=false;
				checkClinicBreakdown.Visible=true;
			}
			textDateFrom.Text=DateTime.Today.ToShortDateString();
			textDateTo.Text=DateTime.Today.ToShortDateString();
			butThis.Text=Lan.g(this,"Today");
		}
		else if(radioMonthly.Checked) {
			if(true) {
				checkClinicInfo.Visible=false;
				checkClinicBreakdown.Visible=true;
			}
			textDateFrom.Text=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).ToShortDateString();
			textDateTo.Text=new DateTime(DateTime.Today.Year,DateTime.Today.Month
				,DateTime.DaysInMonth(DateTime.Today.Year,DateTime.Today.Month)).ToShortDateString();
			butThis.Text=Lan.g(this,"This Month");
		}
		else{//annual
			if(true) {
				checkClinicInfo.Visible=false;
				checkClinicBreakdown.Visible=true;
			}
			textDateFrom.Text=new DateTime(DateTime.Today.Year,1,1).ToShortDateString();
			textDateTo.Text=new DateTime(DateTime.Today.Year,12,31).ToShortDateString();
			butThis.Text=Lan.g(this,"This Year");
		}
	}

	private void butThis_Click(object sender, System.EventArgs e) {
		SetDates();
	}

	private void butLeft_Click(object sender, System.EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		dateFrom=SIn.Date(textDateFrom.Text);
		if(dateFrom.Year < 1880) {
			MsgBox.Show(this,"Please fix the From date first.");
			return;
		}
		dateTo=SIn.Date(textDateTo.Text);
		if(radioDaily.Checked || radioProvider.Checked) {
			textDateFrom.Text=dateFrom.AddDays(-1).ToShortDateString();
			textDateTo.Text=dateTo.AddDays(-1).ToShortDateString();
		}
		else if(radioMonthly.Checked){
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
		else{//annual
			textDateFrom.Text=dateFrom.AddYears(-1).ToShortDateString();
			textDateTo.Text=dateTo.AddYears(-1).ToShortDateString();
		}
	}

	private void butRight_Click(object sender, System.EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		dateFrom=SIn.Date(textDateFrom.Text);
		dateTo=SIn.Date(textDateTo.Text);
		if(radioDaily.Checked || radioProvider.Checked) {
			textDateFrom.Text=dateFrom.AddDays(1).ToShortDateString();
			textDateTo.Text=dateTo.AddDays(1).ToShortDateString();
		}
		else if(radioMonthly.Checked){
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
		else{//annual
			textDateFrom.Text=dateFrom.AddYears(1).ToShortDateString();
			textDateTo.Text=dateTo.AddYears(1).ToShortDateString();
		}
	}
    
	private void checkClinicInfo_CheckedChanged(object sender,EventArgs e) {
		if(true) {
			if(checkClinicInfo.Checked) {
				checkClinicBreakdown.Visible=true;
			}
			else {
				checkClinicBreakdown.Checked=false;
				checkClinicBreakdown.Visible=false;
			}
		}
	}

	///<summary>Gets the list of clinics to use in the report. Filters out clinics depending on selection and security permissions.</summary>
	private List<ClinicDto> GetClinicsForReport() {
		var listClinics=new List<ClinicDto>();
		if(true) {
			for(var i=0;i<listClin.SelectedIndices.Count;i++) {
				if(Security.CurUser.ClinicIsRestricted) {
					listClinics.Add(_listClinics[listClin.SelectedIndices[i]]);//we know that the list is a 1:1 to _listClinics
				}
				else {
					if(listClin.SelectedIndices[i]==0) {
						listClinics.Add(new ClinicDto {
							Id=0,
							Abbr=Lan.g(this,"Unassigned")
						});//Will have ClinicNum of 0 for our "Unassigned" needs.
					}
					else {
						listClinics.Add(_listClinics[listClin.SelectedIndices[i]-1]);//Minus 1 from the selected index
					}
				}
			}
			if(checkAllClin.Checked) {//All Clinics selected; add all visible or hidden unrestricted clinics to the list
				listClinics.AddRange(Clinics.GetAllForUserod(Security.CurUser).Where(x => !listClinics.Select(y => y.Id).Contains(x.Id)));
			}
		}
		return listClinics;
	}

	private void RunDaily() {
		//The old daily prod and inc report (prior to report complex) had portait mode for non-clinic users and landscape for clinic users.
		var isLandscape=false;
		if((true && checkClinicInfo.Checked) || radioWriteoffBoth.Checked) {
			isLandscape=true;
		}
		var report=new ReportComplex(true,isLandscape);
		if(checkAllProv.Checked) {
			listProv.SetAll(true);
		}
		else if(listProv.SelectedIndices.Count==0){
			MsgBox.Show(this,"All providers are hidden on reports.");
			return;
		}
		if(checkAllClin.Checked) {
			listClin.SetAll(true);
		}
		dateFrom=SIn.Date(textDateFrom.Text);
		dateTo=SIn.Date(textDateTo.Text);
		var listProvs=new List<ProviderDto>();
		if(checkAllProv.Checked) {
			listProvs=_listProviders;
		}
		else {
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				listProvs.Add(_listFilteredProviders[listProv.SelectedIndices[i]]);
			}
		}
		var listClinics=GetClinicsForReport();
		//true if the all clinics checkbox is checked and the selected clinics contains every ClinicNum, including the 'Unassigned' ClinicNum of 0,
		//all hidden clinics, and the user cannot be restricted.  'All' clinics means all in the list, which may not be all clinics.
		var listSelectedClinicNums=listClinics.Select(x => x.Id).ToList();
		var hasAllClinics=checkAllClin.Checked && listSelectedClinicNums.Contains(0)
		                                       && Clinics.GetDeepCopy().Select(x => x.Id).All(x => listSelectedClinicNums.Contains(x));
		var dataSetDailyProd=RpProdInc.GetDailyData(dateFrom,dateTo,listProvs,listClinics,checkAllProv.Checked,hasAllClinics
			,checkClinicBreakdown.Checked,checkClinicInfo.Checked,checkUnearned.Checked,GetWriteoffType());
		var tableDailyProd=dataSetDailyProd.Tables["DailyProd"];//Includes multiple clinics that will get separated out later.
		var dataSetDailyProdSplitByClinic=new DataSet();
		if(true && checkClinicInfo.Checked) {
			//Split up each clinic into its own table and add that to the data set split up by clinics.
			var lastClinic="";
			var dtClinic=tableDailyProd.Clone();//Clones the structure, not the data.
			for(var i=0;i<tableDailyProd.Rows.Count;i++) {
				var currentClinic=tableDailyProd.Rows[i]["Clinic"].ToString();
				if(currentClinic=="") {
					currentClinic="Unassigned"; //not actually displayed to the user, so no translation.
				}
				if(lastClinic=="") {
					lastClinic=currentClinic;
				}
				//Check if we have successfully added all rows for the current clinic and add the datatable to the dataset if there is information present.
				if(lastClinic!=currentClinic && dtClinic.Rows.Count>0) {
					var dtClinicTemp=dtClinic.Copy();
					dtClinicTemp.TableName="Clinic"+i;//The name of the table does not matter but has to be unique in a DataSet.
					dataSetDailyProdSplitByClinic.Tables.Add(dtClinicTemp);
					dtClinic.Rows.Clear();//Clear out the data to start collecting the information for the next clinic.
					lastClinic=currentClinic;
				}
				dtClinic.Rows.Add(tableDailyProd.Rows[i].ItemArray);
				//If this is the last row, add dtClinic to the dataset.
				if(i==tableDailyProd.Rows.Count-1) {
					var dtClinicTemp=dtClinic.Copy();
					//Added 1 to guarantee unique tablename.
					dtClinicTemp.TableName="Clinic"+(i+1);//The name of the table does not matter but has to be unique in a DataSet. 
					dataSetDailyProdSplitByClinic.Tables.Add(dtClinicTemp);
				}
			}
		}
		report.ReportName="DailyP&I";
		report.AddTitle("Title",Lan.g(this,"Daily Production and Income"));
		report.AddSubTitle("PracName",PrefC.GetString(PrefName.PracticeTitle));
		var dateRangeStr=dateFrom.ToShortDateString()+" - "+dateTo.ToShortDateString();
		if(dateFrom.Date==dateTo.Date) {
			dateRangeStr=dateFrom.ToShortDateString();//Do not show a date range for the same day...
		}
		report.AddSubTitle("Date",dateRangeStr);
		if(checkAllProv.Checked) {
			report.AddSubTitle("Providers",Lan.g(this,"All Providers"));
		}
		else {
			var str="";
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				if(i>0) {
					str+=", ";
				}
				str+=_listFilteredProviders[listProv.SelectedIndices[i]].Abbr;
			}
			report.AddSubTitle("Providers",str);
		}
		if(true) {
			if(checkAllClin.Checked) {
				report.AddSubTitle("Clinics",Lan.g(this,"All Clinics (includes hidden)"));
			}
			else {
				var clinNames="";
				for(var i=0;i<listClin.SelectedIndices.Count;i++) {
					if(i>0) {
						clinNames+=", ";
					}
					if(Security.CurUser.ClinicIsRestricted) {
						clinNames+=_listClinics[listClin.SelectedIndices[i]].Abbr;
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							clinNames+=Lan.g(this,"Unassigned");
						}
						else {
							clinNames+=_listClinics[listClin.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index
						}
					}
				}
				report.AddSubTitle("Clinics",clinNames);
			}
		}
		//setup query
		QueryObject query=null;
		//Default Column Widths for Landscape (either clinic info showing or insurance writeoff est info showing) -----------------
		var dateWidth=75;
		var patientNameWidth=130;
		var descriptionWidth=220;
		var provWidth=65;
		var clinicWidth=130;
		var prodWidth=75;
		var adjustWidth=75;
		var writeoffWidth=75;
		var writeoffEstWidth=75;
		var writeoffAdjWidth=75;
		var ptIncWidth=75;
		var insIncWidth=75;
		//This has every column showing and will not fit with the normal landscape column widths.
		if(radioWriteoffBoth.Checked && checkClinicInfo.Checked) {
			clinicWidth=100;
			prodWidth=65;
			adjustWidth=65;
			writeoffWidth=65;
			writeoffEstWidth=65;
			writeoffAdjWidth=65;
			ptIncWidth=65;
			insIncWidth=65;
		}
		if(!report.IsLandscape) {
			//Trim some fat off for non-clinic users because this report shows in portait mode (default widths are for landscape).
			dateWidth=68;
			patientNameWidth=120;
			descriptionWidth=180;
			provWidth=55;
			prodWidth=75;
			adjustWidth=70;
			writeoffWidth=70;
			ptIncWidth=75;
			insIncWidth=75;
		}
		var font=new Font("Tahoma",8);
		query=report.AddQuery(tableDailyProd,Lan.g(this,"Date")+": "+DateTime.Today.ToShortDateString(),"ClinicSplit",SplitByKind.Value,1,true);
		query.AddColumn("Date",dateWidth,FieldValueType.String,font);
		query.AddColumn("Patient Name",patientNameWidth,FieldValueType.String,font);
		query.AddColumn("Description",descriptionWidth,FieldValueType.String,font);
		query.AddColumn("Prov",provWidth,FieldValueType.String,font);
		if(true && checkClinicInfo.Checked) {//Not no clinics
			query.AddColumn("Clinic",clinicWidth,FieldValueType.String,font);
		}
		query.AddColumn("Production",prodWidth,FieldValueType.Number,font);
		query.AddColumn("Adjust",adjustWidth,FieldValueType.Number,font);
		if(radioWriteoffBoth.Checked) {
			query.AddColumn("Write-off Est",writeoffEstWidth,FieldValueType.Number,font);
			query.AddColumn("Write-off Adj",writeoffAdjWidth,FieldValueType.Number,font);
		}
		else {
			query.AddColumn("Write-off",writeoffWidth,FieldValueType.Number,font);
		}
		query.AddColumn("Pt Income",ptIncWidth,FieldValueType.Number,font);
		query.AddColumn("Ins Income",insIncWidth,FieldValueType.Number,font);
		//If more than one clinic selected, we want to add a table to the end of the report that totals all the clinics together.
		//When only one clinic is showing , the "Summary" at the end of every daily report will suffice. (total prod and total income lines).
		if(true && listClinics.Count > 1 && checkClinicInfo.Checked) {
			var tableClinicTotals=GetClinicTotals(dataSetDailyProdSplitByClinic);
			query=report.AddQuery(tableClinicTotals,"Clinic Totals","",SplitByKind.None,2,true);
			query.AddColumn("Clinic",410,FieldValueType.String,font);
			query.AddColumn("Production",75,FieldValueType.Number,font);
			query.AddColumn("Adjust",75,FieldValueType.Number,font);
			if(radioWriteoffBoth.Checked) {
				query.AddColumn("Write-off Est",writeoffEstWidth,FieldValueType.Number,font);
				query.AddColumn("Write-off Adj",writeoffAdjWidth,FieldValueType.Number,font);
			}
			else {
				query.AddColumn("Write-off",75,FieldValueType.Number,font);
			}
			query.AddColumn("Pt Income",75,FieldValueType.Number,font);
			query.AddColumn("Ins Income",75,FieldValueType.Number,font);
		}
		//Calculate the total production and total income and add them to the bottom of the report:
		double totalProduction=0;
		double totalIncome=0;
		for(var i=0;i<tableDailyProd.Rows.Count;i++) {
			//Total production is (Production + Adjustments - Writeoffs)
			totalProduction+=SIn.Double(tableDailyProd.Rows[i]["Production"].ToString());
			totalProduction+=SIn.Double(tableDailyProd.Rows[i]["Adjust"].ToString());
			if(radioWriteoffBoth.Checked) {
				totalProduction+=SIn.Double(tableDailyProd.Rows[i]["Writeoff Est"].ToString());
				totalProduction+=SIn.Double(tableDailyProd.Rows[i]["Writeoff Adj"].ToString());
			}
			else {
				totalProduction+=SIn.Double(tableDailyProd.Rows[i]["Writeoff"].ToString());
			}
			//Total income is (Pt Income + Ins Income)
			totalIncome+=SIn.Double(tableDailyProd.Rows[i]["Pt Income"].ToString());
			totalIncome+=SIn.Double(tableDailyProd.Rows[i]["Ins Income"].ToString());
		}
		//Add the Total Production and Total Income to the bottom of the report if there were any rows present.
		if(tableDailyProd.Rows.Count > 0) {
			//Use a custom table and add it like it is a "query" to the report because using a group summary would be more complicated due
			//to the need to add and subtract from multiple columns at the same time.
			var tableTotals=new DataTable("TotalProdAndInc");
			tableTotals.Columns.Add("Summary");
			string prodLabel;
			if(radioWriteoffBoth.Checked) {
				prodLabel="Total Production (Production + Adjustments - Write-off Ests - Write-off Adjs)";
			}
			else {
				prodLabel="Total Production (Production + Adjustments - Write-offs)";
			}
			tableTotals.Rows.Add(Lan.g(this,prodLabel)+" "+totalProduction.ToString("c"));
			tableTotals.Rows.Add(Lan.g(this,"Total Income (Pt Income + Ins Income):")+" "+totalIncome.ToString("c"));
			//Add tableTotals to the report.
			//No column name and no header because we want to display this table to NOT look like a table.
			query=report.AddQuery(tableTotals,"","",SplitByKind.None,2,false);
			query.AddColumn("",785,FieldValueType.String,new Font("Tahoma",8,FontStyle.Bold));
		}
		report.AddPageNum();
		// execute query
		if(!report.SubmitQueries()) {
			return;
		}
		// display report
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		//DialogResult=DialogResult.OK;//Allow running multiple reports.
	}

	private PPOWriteoffDateCalc GetWriteoffType() {
		if(radioWriteoffPay.Checked) {
			return PPOWriteoffDateCalc.InsPayDate;
		}
		else if(radioWriteoffProc.Checked){
			return PPOWriteoffDateCalc.ProcDate;
		}
		else {//radioWriteoffClaim.Checked is checked
			return PPOWriteoffDateCalc.ClaimPayDate;
		}
	} 

	private DataTable GetClinicTotals(DataSet dataSetDailyProdSplitByClinic) {
		var tableClinicTotals=new DataTable("ClinicTotals");
		tableClinicTotals.Columns.Add(new DataColumn("Clinic"));
		tableClinicTotals.Columns.Add(new DataColumn("Production"));
		tableClinicTotals.Columns.Add(new DataColumn("Adjust"));
		if(radioWriteoffBoth.Checked) {
			tableClinicTotals.Columns.Add(new DataColumn("Write-off Est"));
			tableClinicTotals.Columns.Add(new DataColumn("Write-off Adj"));
		}
		else {
			tableClinicTotals.Columns.Add(new DataColumn("Write-off"));
		}
		tableClinicTotals.Columns.Add(new DataColumn("Pt Income"));
		tableClinicTotals.Columns.Add(new DataColumn("Ins Income"));
		for(var i=0;i<dataSetDailyProdSplitByClinic.Tables.Count;i++) {
			var clinicDesc="";
			if(dataSetDailyProdSplitByClinic.Tables[i].Rows.Count > 0) {
				clinicDesc=dataSetDailyProdSplitByClinic.Tables[i].Rows[0]["Clinic"].ToString();//Take description of first row.
			}
			clinicDesc=clinicDesc=="" ? Lan.g(this,"Unassigned") : clinicDesc;
			//Calculate the total production and total income for this clinic.
			double production=0;
			double adjust=0;
			double writeoffest=0;
			double writeoff=0;
			double writeoffadj=0;
			double ptIncome=0;
			double insIncome=0;
			for(var j=0;j<dataSetDailyProdSplitByClinic.Tables[i].Rows.Count;j++) {
				production+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Production"].ToString());
				adjust+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Adjust"].ToString());
				if(radioWriteoffBoth.Checked) {
					writeoffest+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Writeoff Est"].ToString());
					writeoffadj+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Writeoff Adj"].ToString());
				}
				else {
					writeoff+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Writeoff"].ToString());
				}
				ptIncome+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Pt Income"].ToString());
				insIncome+=SIn.Double(dataSetDailyProdSplitByClinic.Tables[i].Rows[j]["Ins Income"].ToString());
			}
			if(radioWriteoffBoth.Checked) {
				tableClinicTotals.Rows.Add(clinicDesc,production,adjust,writeoffest,writeoffadj,ptIncome,insIncome);
			}
			else {
				tableClinicTotals.Rows.Add(clinicDesc,production,adjust,writeoff,ptIncome,insIncome);
			}
		}
		return tableClinicTotals;
	}

	private void RunMonthly(){
		//If adding the unearned column, need more space. Set report to landscape.
		var report=new ReportComplex(true,radioWriteoffBoth.Checked || checkUnearned.Checked);
		report.PrintMargins=new Margins(45,0,50,50);
		if(checkAllProv.Checked) {
			listProv.SetAll(true);
		}
		else if(listProv.SelectedIndices.Count==0){
			MsgBox.Show(this,"All providers are hidden on reports.");
			return;
		}
		if(checkAllClin.Checked) {
			listClin.SetAll(true);
		}
		dateFrom=SIn.Date(textDateFrom.Text);
		dateTo=SIn.Date(textDateTo.Text);
		var listProvs=new List<ProviderDto>();
		if(checkAllProv.Checked) {
			listProvs=_listProviders;
		}
		else {
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				listProvs.Add(_listFilteredProviders[listProv.SelectedIndices[i]]);
			}
		}
		var listClinics=GetClinicsForReport();
		//true if the all clinics checkbox is checked and the selected clinics contains every ClinicNum, including the 'Unassigned' ClinicNum of 0,
		//all hidden clinics, and the user cannot be restricted.  'All' clinics means all in the list, which may not be all clinics.
		var listSelectedClinicNums=listClinics.Select(x => x.Id).ToList();
		var hasAllClinics=checkAllClin.Checked && listSelectedClinicNums.Contains(0)
		                                       && Clinics.GetDeepCopy().Select(x => x.Id).All(x => listSelectedClinicNums.Contains(x));
		var ds=RpProdInc.GetMonthlyData(dateFrom,dateTo,listProvs,listClinics,radioWriteoffPay.Checked,checkAllProv.Checked,hasAllClinics
			,radioWriteoffBoth.Checked,checkUnearned.Checked);
		var dt=ds.Tables["Total"];
		var dtClinic=new DataTable();
		if(true) {
			dtClinic=ds.Tables["Clinic"];
		}
		report.ReportName="MonthlyP&I";
		report.AddTitle("Title",Lan.g(this,"Monthly Production and Income"));
		report.AddSubTitle("PracName",PrefC.GetString(PrefName.PracticeTitle));
		report.AddSubTitle("Date",dateFrom.ToShortDateString()+" - "+dateTo.ToShortDateString());
		if(checkAllProv.Checked) {
			report.AddSubTitle("Providers",Lan.g(this,"All Providers"));
		}
		else {
			var str="";
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				if(i>0) {
					str+=", ";
				}
				str+=_listFilteredProviders[listProv.SelectedIndices[i]].Abbr;
			}
			report.AddSubTitle("Providers",str);
		}
		if(true) {
			if(checkAllClin.Checked) {
				report.AddSubTitle("Clinics",Lan.g(this,"All Clinics (includes hidden)"));
			}
			else {
				var clinNames="";
				for(var i=0;i<listClin.SelectedIndices.Count;i++) {
					if(i>0) {
						clinNames+=", ";
					}
					if(Security.CurUser.ClinicIsRestricted) {
						clinNames+=_listClinics[listClin.SelectedIndices[i]].Abbr;
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							clinNames+=Lan.g(this,"Unassigned");
						}
						else {
							clinNames+=_listClinics[listClin.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index
						}
					}
				}
				report.AddSubTitle("Clinics",clinNames);
			}
		}
		//setup query
		QueryObject query;
		if(true && checkClinicBreakdown.Checked) {
			query=report.AddQuery(dtClinic,"","Clinic",SplitByKind.Value,1,true);
		}
		else {
			query=report.AddQuery(dt,"","",SplitByKind.None,1,true);
		}
		// add columns to report
		var font=new Font("Tahoma",8,FontStyle.Regular);
		var datewidth=70;
		var day=35;
		var productionwidth=90;
		var schedwidth=85;
		var adjwidth=85;
		var writeoffestwidth=95;
		var writeoffwidth=80;
		var writeoffadjwidth=70;
		var totprodwidth=90;
		var ptincomewidth=85;
		var unearnedPtIncomeWidth=80;
		var insincomewidth=85;
		var totincomewidth=90;
		var summaryOffSetY=30;
		var summaryIncomeOffSetY=4;
		query.AddColumn("Date",datewidth,FieldValueType.String,font);
		query.AddColumn("Day",day,FieldValueType.String,font);
		query.AddColumn("Production",productionwidth,FieldValueType.Number,font);
		query.AddColumn("Sched",schedwidth,FieldValueType.Number,font);
		query.AddColumn("Adj",adjwidth,FieldValueType.Number,font);
		if(radioWriteoffBoth.Checked) {
			query.AddColumn("Write-off Est",writeoffestwidth,FieldValueType.Number,font);
			query.AddColumn("Write-off Adj",writeoffadjwidth,FieldValueType.Number,font);
		}
		else {
			query.AddColumn("Write-off",writeoffwidth,FieldValueType.Number,font);
		}
		query.AddColumn("Tot Prod",totprodwidth,FieldValueType.Number,font);
		query.AddColumn("Pt Income",ptincomewidth,FieldValueType.Number,font);
		if(checkUnearned.Checked) {
			query.AddColumn("Unearned Pt Income",unearnedPtIncomeWidth,FieldValueType.Number,font);
		}
		query.AddColumn("Ins Income",insincomewidth,FieldValueType.Number,font);
		query.AddColumn("Tot Income",totincomewidth,FieldValueType.Number,font);
		if(true && listClin.SelectedIndices.Count>1 && checkClinicBreakdown.Checked) {
			//If more than one clinic selected, we want to add a table to the end of the report that totals all the clinics together.
			query=report.AddQuery(dt,"Totals","",SplitByKind.None,2,true);
			query.AddColumn("Date",datewidth,FieldValueType.String,font);
			query.AddColumn("Day",day,FieldValueType.String,font);
			query.AddColumn("Production",productionwidth,FieldValueType.Number,font);
			query.AddColumn("Sched",schedwidth,FieldValueType.Number,font);
			query.AddColumn("Adj",adjwidth,FieldValueType.Number,font);
			if(radioWriteoffBoth.Checked) {
				query.AddColumn("Write-off Est",writeoffestwidth,FieldValueType.Number,font);
				query.AddColumn("Write-off Adj",writeoffadjwidth,FieldValueType.Number,font);
			}
			else {
				query.AddColumn("Write-off",writeoffwidth,FieldValueType.Number,font);
			}
			query.AddColumn("Tot Prod",totprodwidth,FieldValueType.Number,font);
			query.AddColumn("Pt Income",ptincomewidth,FieldValueType.Number,font);
			if(checkUnearned.Checked) {
				query.AddColumn("Unearned Pt Income",unearnedPtIncomeWidth,FieldValueType.Number,font);
			}
			query.AddColumn("Ins Income",insincomewidth,FieldValueType.Number,font);
			query.AddColumn("Tot Income",totincomewidth,FieldValueType.Number,font);
			//Column used to align the summary fields.
			var columnNameAlign =radioWriteoffBoth.Checked ? "Tot Prod": "Write-off";
			if(radioWriteoffBoth.Checked) {
				query.AddGroupSummaryField("Total Production (Production + Scheduled + Adjustments - Write-off Ests - Write-off Adjs): ",
					columnNameAlign,"Tot Prod",SummaryOperation.Sum, [2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			else {
				query.AddGroupSummaryField("Total Production (Production + Scheduled + Adjustments - Write-offs): ",columnNameAlign,
					"Tot Prod",SummaryOperation.Sum, [2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			if(checkUnearned.Checked) {//if unearned check, add summaries.
				query.AddGroupSummaryField("Total Pt Income (Pt Income + Unearned Pt Income): ",columnNameAlign,"Total Pt Income",
					SummaryOperation.Sum, [2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
			}
			query.AddGroupSummaryField("Total Income (Total Pt Income + Ins Income): ",columnNameAlign,"Total Income",SummaryOperation.Sum,
				[2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
		}
		else {
			var columnNameAlign=radioWriteoffBoth.Checked ? "Tot Prod" : "Write-off";//column used to align the summary fields
			if(radioWriteoffBoth.Checked) {
				query.AddGroupSummaryField("Total Production (Production + Scheduled + Adjustments - Write-off Ests - Write-off Adjs): ",
					columnNameAlign,"Tot Prod",SummaryOperation.Sum, [1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			else {
				query.AddGroupSummaryField("Total Production (Production + Scheduled + Adjustments - Write-offs): ",columnNameAlign,
					"Tot Prod",SummaryOperation.Sum, [1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			if(checkUnearned.Checked) {//if unearned check, add summaries.
				query.AddGroupSummaryField("Total Pt Income (Pt Income + Unearned Pt Income): ",columnNameAlign,"Total Pt Income",
					SummaryOperation.Sum, [1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
			}
			query.AddGroupSummaryField("Total Income (Total Pt Income + Ins Income): ",columnNameAlign,"Total Income",SummaryOperation.Sum,
				[1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
		}
		report.AddPageNum();
		// execute query
		if(!report.SubmitQueries()) {
			return;
		}
		// display report
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		//DialogResult=DialogResult.OK;//Allow running multiple reports.
	}

	private void RunAnnual(){
		//If adding the unearned column, need more space. Set report to landscape.
		var report=new ReportComplex(true,radioWriteoffBoth.Checked || checkUnearned.Checked);
		if(checkAllProv.Checked) {
			listProv.SetAll(true);
		}
		else if(listProv.SelectedIndices.Count==0){
			MsgBox.Show(this,"All providers are hidden on reports.");
			return;
		}
		if(checkAllClin.Checked) {
			listClin.SetAll(true);
		}
		dateFrom=SIn.Date(textDateFrom.Text);
		dateTo=SIn.Date(textDateTo.Text);
		var listProvs=new List<ProviderDto>();
		if(checkAllProv.Checked) {
			listProvs=_listProviders;
		}
		else {
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				listProvs.Add(_listFilteredProviders[listProv.SelectedIndices[i]]);
			}
		}
		var listClinics=GetClinicsForReport();
		//true if the all clinics checkbox is checked and the selected clinics contains every ClinicNum, including the 'Unassigned' ClinicNum of 0,
		//all hidden clinics, and the user cannot be restricted.  'All' clinics means all in the list, which may not be all clinics.
		var listSelectedClinicNums=listClinics.Select(x => x.Id).ToList();
		var hasAllClinics=checkAllClin.Checked && listSelectedClinicNums.Contains(0)
		                                       && Clinics.GetDeepCopy().Select(x => x.Id).All(x => listSelectedClinicNums.Contains(x));
		var ds=RpProdInc.GetAnnualData(dateFrom,dateTo,listProvs,listClinics,radioWriteoffPay.Checked,checkAllProv.Checked,hasAllClinics
			,radioWriteoffBoth.Checked,checkUnearned.Checked);
		var dt=ds.Tables["Total"];
		var dtClinic=new DataTable();
		if(true) {
			dtClinic=ds.Tables["Clinic"];
		}
		report.ReportName="AnnualP&I";
		report.AddTitle("Title",Lan.g(this,"Annual Production and Income"));
		report.AddSubTitle("PracName",PrefC.GetString(PrefName.PracticeTitle));
		report.AddSubTitle("Date",dateFrom.ToShortDateString()+" - "+dateTo.ToShortDateString());
		if(checkAllProv.Checked) {
			report.AddSubTitle("Providers",Lan.g(this,"All Providers"));
		}
		else {
			var str="";
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				if(i>0) {
					str+=", ";
				}
				str+=_listFilteredProviders[listProv.SelectedIndices[i]].Abbr;
			}
			report.AddSubTitle("Providers",str);
		}
		if(true) {
			if(checkAllClin.Checked) {
				report.AddSubTitle("Clinics",Lan.g(this,"All Clinics (includes hidden)"));
			}
			else {
				var clinNames="";
				for(var i=0;i<listClin.SelectedIndices.Count;i++) {
					if(i>0) {
						clinNames+=", ";
					}
					if(Security.CurUser.ClinicIsRestricted) {
						clinNames+=_listClinics[listClin.SelectedIndices[i]].Abbr;
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							clinNames+=Lan.g(this,"Unassigned");
						}
						else {
							clinNames+=_listClinics[listClin.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index
						}
					}
				}
				report.AddSubTitle("Clinics",clinNames);
			}
		}
		//setup query
		QueryObject query;
		if(true && checkClinicBreakdown.Checked) {
			query=report.AddQuery(dtClinic,"","Clinic",SplitByKind.Value,1,true);
		}
		else {
			query=report.AddQuery(dt,"","",SplitByKind.None,1,true);
		}
		// add columns to report
		var datewidth=65;//65px width allows room for a 3 letter month abbreviation and 4 digit year.
		var productionwidth=110;//110px width allows the total row to fit values up to 999,999,999.99 and down to -99,999,999.99
		var adjwidth=110;//110px width allows the total row to fit values up to 999,999,999.99 and down to -99,999,999.99
		var writeoffestwidth=115;//110px width allows the total row to fit values up to 999,999,999.99 and down to -99,999,999.99
		var writeoffwidth=110;//110px width allows the total row to fit values up to 999,999,999.99 and down to -99,999,999.99
		var writeoffadjwidth=80;//80px width allows the total row to fit values up to 999,999.99 and down to -99,999.99
		var totprodwidth=110;//110px width allows the total row to fit values up to 999,999,999.99 and down to -99,999,999.99
		var ptincomewidth=100;//100px width allows the total row to fit values up to 99,999,999.99 and down to -9,999,999.99
		var unearnedPtIncomeWidth=80;//80px width allows the total row to fit values up to 999,999.99 and down to -99,999.99
		var insincomewidth=100;//100px width allows the total row to fit values up to 99,999,999.99 and down to -9,999,999.99
		var totincomewidth=110;//110px width allows the total row to fit values up to 999,999,999.99 and down to -99,999,999.99
		var summaryOffSetY=30;
		var summaryIncomeOffSetY=4;
		query.AddColumn("Month",datewidth,FieldValueType.String);
		query.AddColumn("Production",productionwidth,FieldValueType.Number);
		query.AddColumn("Adjustments",adjwidth,FieldValueType.Number);
		if(radioWriteoffBoth.Checked) {
			query.AddColumn("Write-off Est",writeoffestwidth,FieldValueType.Number);
			query.AddColumn("Write-off Adj",writeoffadjwidth,FieldValueType.Number);
		}
		else {
			query.AddColumn("Write-off",writeoffwidth,FieldValueType.Number);
		}
		query.AddColumn("Tot Prod",totprodwidth,FieldValueType.Number);
		query.AddColumn("Pt Income",ptincomewidth,FieldValueType.Number);
		if(checkUnearned.Checked) {
			query.AddColumn("Unearned Pt Income",unearnedPtIncomeWidth,FieldValueType.Number);
		}
		query.AddColumn("Ins Income",insincomewidth,FieldValueType.Number);
		query.AddColumn("Total Income",totincomewidth,FieldValueType.Number);
		if(true && listClin.SelectedIndices.Count>1 && checkClinicBreakdown.Checked) {
			//If more than one clinic selected, we want to add a table to the end of the report that totals all the clinics together.
			query=report.AddQuery(dt,"Totals","",SplitByKind.None,2,true);
			query.AddColumn("Month",datewidth,FieldValueType.String);
			query.AddColumn("Production",productionwidth,FieldValueType.Number);
			query.AddColumn("Adjustments",adjwidth,FieldValueType.Number);
			if(radioWriteoffBoth.Checked) {
				query.AddColumn("Write-off Est",writeoffestwidth,FieldValueType.Number);
				query.AddColumn("Write-off Adj",writeoffadjwidth,FieldValueType.Number);
			}				
			else {
				query.AddColumn("Write-off",writeoffwidth,FieldValueType.Number);
			}
			query.AddColumn("Tot Prod",totprodwidth,FieldValueType.Number);
			query.AddColumn("Pt Income",ptincomewidth,FieldValueType.Number);
			if(checkUnearned.Checked) {
				query.AddColumn("Unearned Pt Income",unearnedPtIncomeWidth,FieldValueType.Number);
			}
			query.AddColumn("Ins Income",insincomewidth,FieldValueType.Number);
			query.AddColumn("Total Income",totincomewidth,FieldValueType.Number);
			if(radioWriteoffBoth.Checked) {
				query.AddGroupSummaryField("Total Production (Production + Adjustments - Write-off Ests - Write-off Adjs): ",
					"Tot Prod","Tot Prod",SummaryOperation.Sum, [2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			else {
				query.AddGroupSummaryField("Total Production (Production + Adjustments - Write-offs): ","Tot Prod",
					"Tot Prod",SummaryOperation.Sum, [2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			if(checkUnearned.Checked) {//if unearned check, add summaries.
				query.AddGroupSummaryField("Total Pt Income (Pt Income + Unearned Pt Income): ","Tot Prod","Total Pt Income",
					SummaryOperation.Sum, [2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
			}
			query.AddGroupSummaryField("Total Income (Total Pt Income + Ins Income): ","Tot Prod","Total Income",SummaryOperation.Sum,
				[2],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
		}
		else {
			if(radioWriteoffBoth.Checked) {
				query.AddGroupSummaryField("Total Production (Production + Adjustments - Write-off Ests - Write-off Adjs): ",
					"Tot Prod","Tot Prod",SummaryOperation.Sum, [1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			else {
				query.AddGroupSummaryField("Total Production (Production + Adjustments - Write-offs): ","Tot Prod",
					"Tot Prod",SummaryOperation.Sum, [1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryOffSetY);
			}
			if(checkUnearned.Checked) {//if unearned check, add summaries.
				query.AddGroupSummaryField("Total Pt Income (Pt Income + Unearned Pt Income): ","Tot Prod","Total Pt Income",
					SummaryOperation.Sum, [1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
			}
			query.AddGroupSummaryField("Total Income (Total Pt Income + Ins Income): ","Tot Prod","Total Income",SummaryOperation.Sum,
				[1],Color.Black,new Font("Tahoma",9,FontStyle.Bold),75,summaryIncomeOffSetY);
		}
		report.AddPageNum();
		// execute query
		if(!report.SubmitQueries()) {
			return;
		}
		// display report
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		//DialogResult=DialogResult.OK;//Allow running multiple reports.
	}

	private void RunProvider() {
		//If adding the unearned column, need more space. Set report to landscape.
		var report=new ReportComplex(true,radioWriteoffBoth.Checked || checkUnearned.Checked);
		dateFrom=SIn.Date(textDateFrom.Text);
		dateTo=SIn.Date(textDateTo.Text);
		if(checkAllProv.Checked) {
			listProv.SetAll(true);
		}
		else if(listProv.SelectedIndices.Count==0){
			MsgBox.Show(this,"All providers are hidden on reports.");
			return;
		}
		if(checkAllClin.Checked) {
			listClin.SetAll(true);
		}
		var listProvs=new List<ProviderDto>();
		if(checkAllProv.Checked) {
			listProvs=_listProviders;
		}
		else {
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				listProvs.Add(_listFilteredProviders[listProv.SelectedIndices[i]]);
			}
		}
		var listClinics=GetClinicsForReport();
		var hasAllClinics=(!Security.CurUser.ClinicIsRestricted && checkAllClin.Checked);
		var ds=RpProdInc.GetProviderDataForClinics(dateFrom,dateTo,listProvs,listClinics,checkAllProv.Checked,hasAllClinics
			,checkUnearned.Checked,GetWriteoffType());
		report.ReportName="Provider P&I";
		report.AddTitle("Title",Lan.g(this,"Provider Production and Income"));
		report.AddSubTitle("PracName",PrefC.GetString(PrefName.PracticeTitle));
		report.AddSubTitle("Date",dateFrom.ToShortDateString()+" - "+dateTo.ToShortDateString());
		if(checkAllProv.Checked) {
			report.AddSubTitle("Providers",Lan.g(this,"All Providers"));
		}
		else {
			var str="";
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				if(i>0) {
					str+=", ";
				}
				str+=_listFilteredProviders[listProv.SelectedIndices[i]].Abbr;
			}
			report.AddSubTitle("Providers",str);
		}
		if(true) {
			if(checkAllClin.Checked) {
				report.AddSubTitle("Clinics",Lan.g(this,"All Clinics (includes hidden)"));
			}
			else {
				var clinNames="";
				for(var i=0;i<listClin.SelectedIndices.Count;i++) {
					if(i>0) {
						clinNames+=", ";
					}
					if(Security.CurUser.ClinicIsRestricted) {
						clinNames+=_listClinics[listClin.SelectedIndices[i]].Abbr;
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							clinNames+=Lan.g(this,"Unassigned");
						}
						else {
							clinNames+=_listClinics[listClin.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index
						}
					}
				}
				report.AddSubTitle("Clinics",clinNames);
			}
		}
		//setup query
		QueryObject query;
		var dtClinic=new DataTable();
		if(true) {
			dtClinic=ds.Tables["Clinic"].Copy();
		}
		var dt=ds.Tables["Total"].Copy();
		if(true && checkClinicBreakdown.Checked) {
			query=report.AddQuery(dtClinic,"","Clinic",SplitByKind.Value,1,true);
		}
		else {
			query=report.AddQuery(dt,"","",SplitByKind.None,1,true);
		}
		// add columns to report
		var provwidth=110;
		var productionwidth=90;
		var adjwidth=90;
		var writeoffestwidth=90;
		var writeoffwidth=90;
		var writeoffadjwidth=90;
		var totprodwidth=90;
		var ptincomewidth=90;
		var unearnedPtIncomeWidth=90;
		var insincomewidth=90;
		var totincomewidth=90;
		query.AddColumn("Provider",provwidth,FieldValueType.String);
		query.AddColumn("Production",productionwidth,FieldValueType.Number);
		query.AddColumn("Adjustments",adjwidth,FieldValueType.Number);
		if(radioWriteoffBoth.Checked) {
			query.AddColumn("Write-off Est",writeoffestwidth,FieldValueType.Number);
			query.AddColumn("Write-off Adj",writeoffadjwidth,FieldValueType.Number);
		}
		else {
			query.AddColumn("Write-off",writeoffwidth,FieldValueType.Number);
		}
		query.AddColumn("Tot Prod",totprodwidth,FieldValueType.Number);
		query.AddColumn("Pt Income",ptincomewidth,FieldValueType.Number);
		if(checkUnearned.Checked) {
			query.AddColumn("Unearned Pt Income",unearnedPtIncomeWidth,FieldValueType.Number);
		}
		query.AddColumn("Ins Income",insincomewidth,FieldValueType.Number);
		query.AddColumn("Total Income",totincomewidth,FieldValueType.Number);
		if(true && listClin.SelectedIndices.Count>1 && checkClinicBreakdown.Checked) {
			//If more than one clinic selected, we want to add a table to the end of the report that totals all the clinics together.
			query=report.AddQuery(dt,"Totals","",SplitByKind.None,2,true);
			query.AddColumn("Provider",provwidth,FieldValueType.String);
			query.AddColumn("Production",productionwidth,FieldValueType.Number);
			query.AddColumn("Adjustments",adjwidth,FieldValueType.Number);
			if(radioWriteoffBoth.Checked) {
				query.AddColumn("Write-off Est",writeoffestwidth,FieldValueType.Number);
				query.AddColumn("Write-off Adj",writeoffadjwidth,FieldValueType.Number);
			}
			else {
				query.AddColumn("Write-off",writeoffwidth,FieldValueType.Number);
			}
			query.AddColumn("Tot Prod",totprodwidth,FieldValueType.Number);
			query.AddColumn("Pt Income",ptincomewidth,FieldValueType.Number);
			if(checkUnearned.Checked) {
				query.AddColumn("Unearned Pt Income",unearnedPtIncomeWidth,FieldValueType.Number);
			}
			query.AddColumn("Ins Income",insincomewidth,FieldValueType.Number);
			query.AddColumn("Total Income",totincomewidth,FieldValueType.Number);
		}
		report.AddPageNum();
		// execute query
		if(!report.SubmitQueries()) {//Does not actually submit queries because we use datatables in the central management tool.
			return;
		}
		// display the report
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		//DialogResult=DialogResult.OK;//Allow running multiple reports.
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			MsgBox.Show(this,"Please fix data entry errors first.");
			return;
		}
		if(!checkAllProv.Checked && listProv.SelectedIndices.Count==0){
			MsgBox.Show(this,"At least one provider must be selected.");
			return;
		}
		if(true) {
			if(!checkAllClin.Checked && listClin.SelectedIndices.Count==0) {
				MsgBox.Show(this,"At least one clinic must be selected.");
				return;
			}
		}
		dateFrom=SIn.Date(textDateFrom.Text);
		dateTo=SIn.Date(textDateTo.Text);
		if(dateTo<dateFrom) {
			MsgBox.Show(this,"To date cannot be before From date.");
			return;
		}
		if(radioDaily.Checked){
			RunDaily();
		}
		else if(radioMonthly.Checked){
			RunMonthly();
		}
		else if(radioAnnual.Checked) {
			RunAnnual();
		}
		else {//Provider
			RunProvider();
		}
		//DialogResult=DialogResult.OK;//Stay here so that a series of similar reports can be run
	}

}