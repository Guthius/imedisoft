using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Collections.Generic;
using System.Data;
using OpenDental.UI;
using CodeBase;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.Logic;

namespace OpenDental;

public partial class FormRpProcNote:FormODBase {
	private bool _headingPrinted;
	private int _pagesPrinted;
	private int _headingPrintH;
	//Used to track which window the calling form is currently displaying from, this way we know if we want to minimize this window or let it stay in view.
	private System.Windows.Forms.Screen _screenParent;

		
	public FormRpProcNote(Control controlParentForm) {
		InitializeComponent();

		_screenParent=System.Windows.Forms.Screen.FromHandle(controlParentForm.Handle);
	}

	private void FormRpProcNote_Load(object sender,System.EventArgs e) {
		checkNoNotes.Checked=PrefC.GetBool(PrefName.ReportsIncompleteProcsNoNotes);
		checkUnsignedNote.Checked=PrefC.GetBool(PrefName.ReportsIncompleteProcsUnsigned);
		var listProviders=Providers.GetListProvidersForClinic(comboClinics.ClinicNumSelected);
		FillProvs(listProviders);
		dateRangePicker.SetDateTimeFrom(DateTime.Today);
		dateRangePicker.SetDateTimeTo(DateTime.Today);
		FillGrid();
	}

	private void FillProvs(List<ProviderDto> listProviders) {
		comboProvs.Items.Clear();
		comboProvs.IncludeAll=true;
		comboProvs.Items.AddProvsFull(listProviders);
		comboProvs.IsAllSelected=true;
	}

	private void FillGrid() {
		var table=GetIncompleteProcNotes();
		var includeAllNoNotes=checkAllNoNotes.Checked;
		var includeNoNotes=checkNoNotes.Checked;
		var includeUnsignedNotes=checkUnsignedNote.Checked;
		gridMain.BeginUpdate();
		//Columns
		gridMain.Columns.Clear();
		GridColumn col;
		col=new GridColumn(Lan.g(this,"Date"),80);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Patient Name"),120);
		gridMain.Columns.Add(col);
		if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
			col=new GridColumn(Lan.g(this,"Code"),150);
			gridMain.Columns.Add(col);
			col=new GridColumn(Lan.g(this,"Description"),220);
			gridMain.Columns.Add(col);
		}
		else {
			col=new GridColumn(Lan.g(this,"Code"),150);
			gridMain.Columns.Add(col);
			col=new GridColumn(Lan.g(this,"Description"),220);
			gridMain.Columns.Add(col);
			col=new GridColumn(Lan.g(this,"Tth"),30);
			gridMain.Columns.Add(col);
			col=new GridColumn(Lan.g(this,"Surf"),40);
			gridMain.Columns.Add(col);
		}
		if(includeUnsignedNotes || includeNoNotes || includeAllNoNotes) {
			col=new GridColumn(Lan.g(this,"Incomplete"),80);
			gridMain.Columns.Add(col);
		}
		if(includeNoNotes || includeAllNoNotes) {
			col=new GridColumn(Lan.g(this,"No Note"),60);
			gridMain.Columns.Add(col);
		}
		if(includeUnsignedNotes) {
			col=new GridColumn(Lan.g(this,"Unsigned"),70);
			gridMain.Columns.Add(col);
		}
		//Rows
		gridMain.ListGridRows.Clear();
		foreach(DataRow row in table.Rows) {
			var newRow=new GridRow();
			newRow.Cells.Add(SIn.Date(row["ProcDate"].ToString()).ToString("d"));
			newRow.Cells.Add(row["PatName"].ToString());
			if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
				newRow.Cells.Add(row["ProcCode"].ToString());
				newRow.Cells.Add(row["Descript"].ToString());
			}
			else {
				newRow.Cells.Add(row["ProcCode"].ToString());
				newRow.Cells.Add(row["Descript"].ToString());
				newRow.Cells.Add(row["ToothNum"].ToString());
				newRow.Cells.Add(row["Surf"].ToString());
			}
			if(includeUnsignedNotes || includeNoNotes || includeAllNoNotes) {
				newRow.Cells.Add(row["Incomplete"].ToString());
			}
			if(includeNoNotes || includeAllNoNotes) {
				newRow.Cells.Add(row["HasNoNote"].ToString());
			}
			if(includeUnsignedNotes) {
				newRow.Cells.Add(row["HasUnsignedNote"].ToString());
			}
			newRow.Tag=row["PatNum"].ToString();
			gridMain.ListGridRows.Add(newRow);
		}
		gridMain.EndUpdate();
	}

	private DataTable GetIncompleteProcNotes() {
		RpProcNote.ProcNoteGroupBy groupBy;
		if(radioPatient.Checked) {
			groupBy=RpProcNote.ProcNoteGroupBy.Patient;
		}
		else if(radioProcDate.Checked) {
			groupBy=RpProcNote.ProcNoteGroupBy.DateAndPatient;
		}
		else {
			groupBy=RpProcNote.ProcNoteGroupBy.Procedure;
		}
		return RpProcNote.GetData(GetListSelectedProvNums(),comboClinics.ListClinicNumsSelected,dateRangePicker.GetDateTimeFrom(),
			dateRangePicker.GetDateTimeTo(),checkNoNotes.Checked,checkUnsignedNote.Checked,
			(ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers),groupBy,showExcludedCodes:checkShowExcludedCodes.Checked, includeAllNoNotes:checkAllNoNotes.Checked);
	}

		
	private List<long> GetListSelectedProvNums (){
		return comboProvs.GetSelectedProvNums();
	}

	private void butRefresh_Click(object sender,EventArgs e) {
		FillGrid();
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(gridMain.SelectedIndices.Length!=1) {
			return;
		}
		if(!Security.IsAuthorized(EnumPermType.ChartModule)) {
			return;
		}
		var goToPatNum=SIn.Long(gridMain.ListGridRows[e.Row].Tag.ToString());
		var screenCurrent=System.Windows.Forms.Screen.FromHandle(this.Handle);
		if(screenCurrent.DeviceName==_screenParent.DeviceName) {//If on same screen as main OD
			this.WindowState=FormWindowState.Minimized;//Minimize
		}
		GlobalFormOpenDental.GoToModule(EnumModuleType.Chart,patNum:goToPatNum);
	}

	private void goToChartToolStripMenuItem_Click(object sender,EventArgs e) {
		if(gridMain.SelectedIndices.Length!=1) {
			MsgBox.Show(this,"Please select a patient first.");
			return;
		}
		if(!Security.IsAuthorized(EnumPermType.ChartModule)) {
			return;
		}
		var patNum=SIn.Long(gridMain.ListGridRows[gridMain.SelectedIndices[0]].Tag.ToString());
		var pat=Patients.GetPat(patNum);
		GlobalFormOpenDental.PatientSelected(pat,false);
		var screenCurrent=System.Windows.Forms.Screen.FromHandle(this.Handle);
		if(screenCurrent.DeviceName==_screenParent.DeviceName) {//If on same screen as main OD
			this.WindowState=FormWindowState.Minimized;//Minimize
		}
		GlobalFormOpenDental.GoToModule(EnumModuleType.Chart,patNum:pat.PatNum);
	}

	private void radioProc_MouseCaptureChanged(object sender,EventArgs e) {
		FillGrid();
	}

	private void radioPatient_MouseCaptureChanged(object sender,EventArgs e) {
		FillGrid();
	}

	private void radioProcDate_MouseCaptureChanged(object sender,EventArgs e) {
		FillGrid();
	}

	private void butPrint_Click(object sender,EventArgs e) {
		_pagesPrinted=0;
		_headingPrinted=false;
		PrinterL.TryPrintOrDebugRpPreview(pd_PrintPage,Lan.g(this,"Incomplete Procedure Notes report printed"),PrintoutOrientation.Landscape);
	}  
		
	private void pd_PrintPage(object sender,PrintPageEventArgs e) {
		var bounds=e.MarginBounds;
		var g=e.Graphics;
		string text;
		var headingFont=new Font("Arial",13,FontStyle.Bold);
		var subHeadingFont=new Font("Arial",10,FontStyle.Bold);
		var yPos=bounds.Top;
		var center=bounds.X+bounds.Width/2;
		#region printHeading
		if(!_headingPrinted) {
			text=Lan.g(this,"Incomplete Procedure Notes");
			g.DrawString(text,headingFont,Brushes.Black,center-g.MeasureString(text,headingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,headingFont).Height;
			text=PrefC.GetString(PrefName.PracticeTitle);
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,headingFont).Height;
			if(comboProvs.IsAllSelected){
				text=Lan.g(this,"All Providers");
			}
			else {
				text=Lan.g(this,"For Providers:")+" "+string.Join(",",GetListSelectedProvNums().Select(provNum => Providers.GetAbbr(provNum)));
			}
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,headingFont).Height;
			if(comboClinics.ListClinicNumsSelected.Count==0 || comboClinics.IsAllSelected) {//No clinics selected or 'All' selected
				text=Lan.g(this,"All Clinics");
			}
			else {
				text="For Clinics: "+comboClinics.GetStringSelectedClinics();
			}
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=20;
			_headingPrinted=true;
			_headingPrintH=yPos;
		}
		#endregion
		yPos=gridMain.PrintPage(g,_pagesPrinted,bounds,_headingPrintH);
		_pagesPrinted++;
		if(yPos==-1) {
			e.HasMorePages=true;
		}
		else {
			e.HasMorePages=false;
		}
		g.Dispose();
	}

	private void comboClinics_SelectionChangeCommitted(object sender,EventArgs e) {
		if(comboClinics.IsAllSelected) {//return all providers if "All" is selected.
			FillProvs(Providers.GetListReports());
		}
		else { //return providers associated with the selected clinics.
			var listProvs=comboClinics.ListClinicNumsSelected.SelectMany(x => Providers.GetListProvidersForClinic(x)).ToList();
			FillProvs(listProvs);
		}
	}

	private void butExport_Click(object sender,System.EventArgs e) {
		gridMain.Export("Incomplete Procedure Notes");
	}

	private void checkAllNoNotes_MouseClick(object sender, MouseEventArgs e) {
		if (checkNoNotes.Checked) {
			checkNoNotes.Checked=false;
		}
	}

	private void checkNoNotes_MouseClick(object sender, MouseEventArgs e) {
		if (checkAllNoNotes.Checked) {
			checkAllNoNotes.Checked=false;
		}
	}
}