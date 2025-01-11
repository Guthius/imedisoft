using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;
using CodeBase;
using System.Linq;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;

namespace OpenDental{

	public partial class FormRpProcSheet : FormODBase {

		
		public FormRpProcSheet(){
			InitializeComponent();
			InitializeLayoutManager();
 			Lan.F(this);
		}

		private void FormRpProcSheet_Load(object sender, System.EventArgs e) {
			List<Provider> _listProviders=Providers.GetListReports();
			datePickerFrom.SetDateTime(DateTime.Today);
			datePickerTo.SetDateTime(DateTime.Today);
			comboProviders.IncludeAll=true;
			comboProviders.IsAllSelected=true;
			//If the user can reach this form, they either have ReportDailyAllProviders permission, or are a provider themselves.
			//Therefore if they don't have daily report permission, then they must be a provider
			if(!Security.IsAuthorized(EnumPermType.ReportDailyAllProviders,suppressMessage:true)) {
				_listProviders=_listProviders.FindAll(x => x.ProvNum==Security.CurUser.ProvNum);
				comboProviders.IncludeAll=false;
				comboProviders.IsAllSelected=false;
			}
			comboProviders.Items.AddList(_listProviders,x => x.GetLongDesc());
			if(comboProviders.Items.Count==1) {
				comboProviders.SetSelected(0);
			}
			if(Security.CurUser.ClinicIsRestricted){
				comboClinics.IncludeAll=false;
			}
			else{
				comboClinics.IsAllSelected=true;
			}
			ContextMenu contextMenu=new ContextMenu();
			MenuItem menuItem=new MenuItem("Go to Chart");
			menuItem.Click+=MenuItem_Click;
			contextMenu.MenuItems.Add(menuItem);
			gridMain.ContextMenu=contextMenu;
		}

		private void MenuItem_Click(object sender,EventArgs e) {
			long patNum=gridMain.SelectedTag<long>();
			if(patNum==0){//Somehow no grid row was selected.
				return;
			}
			GlobalFormOpenDental.GoToModule(EnumModuleType.Chart,patNum:patNum);
		}

		private void FillGrid(){
			if(!datePickerFrom.IsValid() || !datePickerTo.IsValid()){
				MsgBox.Show("Please fix data entry errors");
				return;
			}
			List<long> listProvNums=comboProviders.GetSelectedProvNums();
			if(listProvNums.IsNullOrEmpty()){
				MsgBox.Show("Please select a provider");
				return;
			}
			bool hasClinicsEnabled=true;
			if(hasClinicsEnabled && comboClinics.IsNothingSelected){
				MsgBox.Show("Please select a clinic");
				return;
			}
			DateTime dateFrom=datePickerFrom.GetDateTime();
			DateTime dateTo=datePickerTo.GetDateTime();
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			bool isClinicMedical=AnyClinicSelectedIsMedical();
			if(hasClinicsEnabled) {
				gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Clinic"),110));//Clinic
				gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Name"),160));//Name
			}
			else{
				gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Name"),270));//Name
			}
			gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Treatment Date"),100));//TDate
			if(isClinicMedical){
				gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Code"),140));//Code
			}
			else{
				gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Code"),80));//Code
				gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Tooth"),60));//Tooth
			}
			gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Area"),100));//Area
			gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Description"),145));//Description
			gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Provider"),80));//Prov
			gridMain.Columns.Add(new GridColumn(Lan.g("TableIndividualProcedures","Fee"),65));//Fee
			gridMain.ListGridRows.Clear();
			bool hasAllProvs=comboProviders.IsAllSelected;
			DataTable table=RpProcSheet.GetIndividualTable(dateFrom,dateTo,listProvNums,comboClinics.ListClinicNumsSelected,"",
					isClinicMedical,hasAllProvs,hasClinicsEnabled);
			GridRow row;
			for(int i=0;i<table.Rows.Count;i++){
				row=new GridRow();
				if(hasClinicsEnabled){
					row.Cells.Add(table.Rows[i]["Clinic"].ToString());
				}
				row.Cells.Add(table.Rows[i]["plfname"].ToString());
				row.Cells.Add(table.Rows[i]["ProcDate"].ToString());
				row.Cells.Add(table.Rows[i]["ProcCode"].ToString());
				if(!isClinicMedical){
					row.Cells.Add(table.Rows[i]["ToothNum"].ToString());
				}
				row.Cells.Add(table.Rows[i]["Area"].ToString());
				row.Cells.Add(table.Rows[i]["Descript"].ToString());
				row.Cells.Add(table.Rows[i]["Abbr"].ToString());
				row.Cells.Add(table.Rows[i]["$fee"].ToString());
				row.Tag=PIn.Long(table.Rows[i]["PatNum"].ToString());
				gridMain.ListGridRows.Add(row);
			}
			gridMain.EndUpdate();
		}
		
		private void butPrint_Click(object sender,EventArgs e) {
			if(!datePickerFrom.IsValid() || !datePickerTo.IsValid()){
				MsgBox.Show("Please fix data entry errors");
				return;
			}
			List<long> listProvNums=comboProviders.GetSelectedProvNums();
			if(listProvNums.IsNullOrEmpty()){
				MsgBox.Show("Please select a provider");
				return;
			}
			bool hasClinicsEnabled=true;
			if(hasClinicsEnabled && comboClinics.IsNothingSelected){
				MsgBox.Show("Please select a clinic");
				return;
			}
			ReportComplex report=new ReportComplex(true,false);
			bool isAnyClinicMedical=AnyClinicSelectedIsMedical();//Used to determine whether or not to display 'Tooth' column
			DateTime dateFrom=datePickerFrom.GetDateTime();
			DateTime dateTo=datePickerTo.GetDateTime();
			DataTable table=new DataTable();
			bool hasAllProvs=comboProviders.IsAllSelected;
			try { 
				table=RpProcSheet.GetIndividualTable(dateFrom,dateTo,listProvNums,comboClinics.ListClinicNumsSelected,"",isAnyClinicMedical,
					hasAllProvs,hasClinicsEnabled);
			}
			catch (Exception ex) {
				string text=Lan.g(this,"Error getting report data:")+" "+ex.Message+"\r\n\r\n"+ex.StackTrace;
				using MsgBoxCopyPaste msgBox=new MsgBoxCopyPaste(text);
				msgBox.ShowDialog();
				return;
			}
			if(table.Columns.Contains("ToothNum")) {
				foreach(DataRow row in table.Rows) {
					row["ToothNum"]=Tooth.Display(row["ToothNum"].ToString());
				}
			}
			string subtitleProvs=ConstructProviderSubtitle();
			string subtitleClinics=ConstructClinicSubtitle();
			Font font=new Font("Tahoma",9);
			Font fontBold=new Font("Tahoma",9,FontStyle.Bold);
			Font fontTitle=new Font("Tahoma",17,FontStyle.Bold);
			Font fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
			report.ReportName=Lan.g(this,"Daily Procedures");
			report.AddTitle("Title",Lan.g(this,"Daily Procedures"),fontTitle);
			report.AddSubTitle("Practice Title",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
			report.AddSubTitle("Dates of Report",dateFrom+" - "+dateTo,fontSubTitle);
			report.AddSubTitle("Providers",subtitleProvs,fontSubTitle);
			if(hasClinicsEnabled) {
				report.AddSubTitle("Clinics",subtitleClinics,fontSubTitle);
			}
			QueryObject query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
			query.AddColumn(Lan.g(this,"Date"),85,FieldValueType.Date,font);
			query.GetColumnDetail(Lan.g(this,"Date")).StringFormat="d";
			query.AddColumn(Lan.g(this,"Patient Name"),145,FieldValueType.String,font);
			if(isAnyClinicMedical) {
				query.AddColumn(Lan.g(this,"Code"),140,FieldValueType.String,font);
			}
			else {
				query.AddColumn(Lan.g(this,"Code"),70,FieldValueType.String,font);
				query.AddColumn("Tooth",40,FieldValueType.String,font);
			}
			query.AddColumn("Area",60,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Description"),140,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Prov"),60,FieldValueType.String,font);
			if(hasClinicsEnabled) {
				query.AddColumn(Lan.g(this,"Clinic"),100,FieldValueType.String,font);
			}
			query.AddColumn(Lan.g(this,"Fee"),65,FieldValueType.Number,font);
			report.AddPageNum(font);
			if(!report.SubmitQueries()) {
				return;
			}
			using FormReportComplex formReportComplex=new FormReportComplex(report);
			formReportComplex.ShowDialog();
			DialogResult=DialogResult.OK;
		}

		private void butRefresh_Click(object sender,EventArgs e) {
			FillGrid();
		}

		private void datePickerFrom_CalendarSelectionChanged(object sender,EventArgs e) {
			datePickerTo.SetDateTime(datePickerFrom.GetDateTime());
		}

		///<summary>Returns 'All Providers' or comma separated string of clinics providers selected.</summary>
		private string ConstructProviderSubtitle() {
			string subtitleProvs="";
			if(comboProviders.IsAllSelected) {
				return Lan.g(this,"All Providers");
			}
			subtitleProvs=string.Join(",",comboProviders.SelectedIndices.Select(index => comboProviders.Items.GetAbbrShowingAt(index)));
			return subtitleProvs;
		}
		
		///<summary>Returns 'All Clinics' or comma separated string of clinics selected.</summary>
		private string ConstructClinicSubtitle() {
			string subtitleClinics="";
			if(!true) {
				return subtitleClinics;
			}
			return comboClinics.GetStringSelectedClinics();
		}

		private bool AnyClinicSelectedIsMedical() {
			if(!true) {
				return Clinics.IsMedicalPracticeOrClinic(0);//Check if the practice is medical
			}
			List<long> listClincNumsSelected=comboClinics.ListClinicNumsSelected;
			for(int i=0;i<listClincNumsSelected.Count;i++){
				if(Clinics.IsMedicalPracticeOrClinic(listClincNumsSelected[i])){
					return true;
				}
			}
			return false;
		}



	}
}