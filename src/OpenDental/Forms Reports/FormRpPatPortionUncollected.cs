using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDental.ReportingComplex;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormRpPatPortionUncollected : FormODBase{
	private DataTable _table;// used to fill grid and print
	private DateTime _fromDate;
	private DateTime _toDate;
	private List<ClinicDto> _listClinics;
	private List<long> _listClinicNums;
	private bool _isAllSelected;

		
	public FormRpPatPortionUncollected(){
		InitializeComponent();
	}

	private void FormPaymentSheet_Load(object sender,System.EventArgs e) {
		odDateRangePicker.SetDateTimeFrom(DateTime.Today);
		odDateRangePicker.SetDateTimeTo(DateTime.Today);
		FillGrid();
	}

	private void FillGrid() {
		_fromDate = odDateRangePicker.GetDateTimeFrom();
		_toDate = odDateRangePicker.GetDateTimeTo();
		_listClinicNums = comboBoxClinicPicker.ListClinicNumsSelected;
		_listClinics = Clinics.GetClinics(comboBoxClinicPicker.ListClinicNumsSelected);
		_isAllSelected = comboBoxClinicPicker.IsAllSelected;
		gridOD.BeginUpdate();
		gridOD.ListGridRows.Clear();
		gridOD.Columns.Clear();
		gridOD.Title="Patient Portion Uncollected";
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Date"),90,UI.GridSortingStrategy.DateParse));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Patient Name"),150,UI.GridSortingStrategy.StringCompare));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Procedure"),80,UI.GridSortingStrategy.StringCompare));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Fee"),60,HorizontalAlignment.Right,UI.GridSortingStrategy.AmountParse));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Patient"),65,HorizontalAlignment.Right,UI.GridSortingStrategy.StringCompare));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Adjustment"),75,HorizontalAlignment.Right,UI.GridSortingStrategy.AmountParse));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Patient Paid"),90,HorizontalAlignment.Right,UI.GridSortingStrategy.AmountParse));
		gridOD.Columns.Add(new UI.GridColumn(Lan.g(this,"Uncollected"),80,HorizontalAlignment.Right,UI.GridSortingStrategy.AmountParse));
		if(odDateRangePicker.IsValid()) {
			_table = RpPatPortionUncollected.GetPatUncollected(_fromDate, _toDate, _listClinicNums);
		}
		UI.GridRow row;
		foreach(DataRow rowCur in _table.Rows) {
			row=new UI.GridRow { Tag=rowCur };
			row.Cells.Add(SIn.Date(rowCur["ProcDate"].ToString()).ToShortDateString());
			row.Cells.Add(SIn.String(rowCur["Patient"].ToString()));
			row.Cells.Add(SIn.String(rowCur["AbbrDesc"].ToString()));
			row.Cells.Add(SIn.Double(rowCur["Fee"].ToString()).ToString("c"));
			row.Cells.Add(SIn.Double(rowCur["PatPortion"].ToString()).ToString("c"));
			row.Cells.Add(SIn.Double(rowCur["Adjustment"].ToString()).ToString("c"));
			row.Cells.Add(SIn.Double(rowCur["Payment"].ToString()).ToString("c"));
			row.Cells.Add(SIn.Double(rowCur["Uncollected"].ToString()).ToString("c"));
			gridOD.ListGridRows.Add(row);
		}
		gridOD.EndUpdate();
	}

	private void PrintReport() {
		if(_table == null || _table.Rows.Count==0) {
			MsgBox.Show(this,Lan.g(this,"No rows to print."));
			return;
		}
		if(true && _listClinicNums.Count==0) {
			MsgBox.Show(this,Lan.g(this,"At least one clinic must be selected."));
			return;
		}
		var report=new ReportComplex(true,false);
		var subtitleClinics="";
		if(true) {
			if(_isAllSelected) {//all clinics selected. 
				subtitleClinics=Lan.g(this,"All Clinics (includes hidden)");
			}
			else {
				if(_listClinicNums.Contains(0)){
					subtitleClinics+=Lan.g(this,"Unassigned");
					subtitleClinics+=_listClinics.Count>0 ? ", " : "";
				}
				for(var i = 0;i<_listClinics.Count;i++) {
					if(i>0) {
						subtitleClinics+=", ";
					}
					subtitleClinics+=_listClinics[i].Abbr;
				}
			}
		}
		var font=new Font("Tahoma",9);
		var fontBold=new Font("Tahoma",9,FontStyle.Bold);
		var fontTitle=new Font("Tahoma",17,FontStyle.Bold);
		var fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
		report.ReportName=Lan.g(this,"Patient Portion Uncollected");
		report.AddTitle("Title",Lan.g(this,"Patient Portion Uncollected"),fontTitle);
		report.AddSubTitle("Practice Title",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
		report.AddSubTitle("Dates of Report",_fromDate.ToString("d")+" - "+_toDate.ToString("d"),fontSubTitle);
		if(true) {
			report.AddSubTitle("Clinics",subtitleClinics,fontSubTitle);
		}
		var query=report.AddQuery(_table,"Patient Portion Uncollected");
		query.AddColumn("PatNum",60,FieldValueType.Integer,font);
		query.AddColumn("Date",90,FieldValueType.Date,font);
		query.GetColumnDetail("Date").StringFormat="d";
		query.AddColumn("Patient Name",150,FieldValueType.String,font);
		query.AddColumn("Procedure",80,FieldValueType.String,font);
		query.AddColumn("Fee",75,FieldValueType.Number,font);
		query.AddColumn("Patient",75,FieldValueType.Number,font);
		query.AddColumn("Adjustment",75,FieldValueType.Number,font);
		query.AddColumn("Patient Paid",90,FieldValueType.Number,font);
		query.AddColumn("Uncollected",80,FieldValueType.Number,font);
		report.AddPageNum(font);
		report.AddGridLines();
		if(!report.SubmitQueries()) {
			return;
		}
		using var FormR=new FormReportComplex(report);
		FormR.ShowDialog();
		DialogResult=DialogResult.OK;
	}

	private void butRefresh_Click(object sender,EventArgs e) {
		FillGrid();
	}

	private void butPrint_Click(object sender,EventArgs e) {
		PrintReport();
	}

	private void menuItemAccount_Click(object sender, EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.AccountModule)) {
			return;
		}
		if(gridOD.SelectedIndices.Length==0) {
			MsgBox.Show(this,"Please select a patient first.");
			return;
		}
		var patNum=SIn.Long(gridOD.SelectedTag<DataRow>()["PatNum"].ToString());
		GlobalFormOpenDental.GoToModule(EnumModuleType.Account,patNum:patNum);
	}

}