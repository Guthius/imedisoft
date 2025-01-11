using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmFeeSchedNotes : FrmODBase {
		public FeeSched FeeSchedCur;
		public long ClinicNum;


		
		public FrmFeeSchedNotes() {
			InitializeComponent();
			Load+=Frm_Load;
			comboClinic.SelectionChangeCommitted+=ComboClinic_SelectionChangeCommitted;
			gridMain.CellDoubleClick+=GridMain_CellDoubleClick;
			PreviewKeyDown+=FrmFeeSchedNotes_PreviewKeyDown;
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			comboClinic.ClinicNumSelected=ClinicNum;
			if(ClinicNum==0){
				comboClinic.IsAllSelected=true;
			}
			textFeeSchedule.Text=FeeSchedCur.Description;
			FillGrid();
		}

		private void FillGrid(){
			List<FeeSchedNote> listFeeSchedNotes=new List<FeeSchedNote>();
			listFeeSchedNotes=FeeSchedNotes.GetNotesForGlobal(FeeSchedCur.FeeSchedNum);
			ClinicNum=comboClinic.ClinicNumSelected;
			//Populate all the listClinicNums
			listFeeSchedNotes=FeeSchedNotes.ConvertListStringsToClinicNums(listFeeSchedNotes);
			List<FeeSchedNote> listFeeSchedNotesFiltered=new List<FeeSchedNote>();
			for(int i=0;i<listFeeSchedNotes.Count;i++){
				if(comboClinic.IsAllSelected){//Don't do the loop if we're not filtering the list
					listFeeSchedNotesFiltered=listFeeSchedNotes;
					break;
				}
				//For each fee sched note
				//Check and see if it's list has the clinic num we have as the filter. These notes came from the database so we can't use listClinicNums
				if(listFeeSchedNotes[i].ListClinicNums.Contains(ClinicNum)){
					listFeeSchedNotesFiltered.Add(listFeeSchedNotes[i]);
				}
			}
			//Sort the dates by most recent
			listFeeSchedNotesFiltered=listFeeSchedNotesFiltered.OrderBy(x=>x.DateEntry).ToList();
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn gridColumn=new GridColumn(Lans.g("TableFeeScheduleNotes","Date"),100);
			gridMain.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lans.g("TableFeeScheduleNotes","Clinics"),300);
			gridMain.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lans.g("TableFeeScheduleNotes","Note"),330);
			gridMain.Columns.Add(gridColumn);
			gridMain.ListGridRows.Clear();
			GridRow gridRow;
			for(int i=0;i<listFeeSchedNotesFiltered.Count;i++){
				gridRow=new GridRow();
				gridRow.Cells.Add(listFeeSchedNotesFiltered[i].DateEntry.ToShortDateString());//Date
				//Find all within the list where the clinic num is within the list of clinic nums and get the abbreviation.
				List<ClinicDto> listClinicsForNote=comboClinic.ListClinics.FindAll(x=>listFeeSchedNotesFiltered[i].ListClinicNums.Contains(x.Id));
				//Get the abbreviation for those clinics
				List<string> listClinicAbbrs=listClinicsForNote.Select(x=>x.Abbr).ToList();
				//Join all the different clinic abbreviations together
				string clinicAbbrs=string.Join(",",listClinicAbbrs);
				gridRow.Cells.Add(clinicAbbrs);//Clinic
				gridRow.Cells.Add(listFeeSchedNotesFiltered[i].Note);//Note
				gridRow.Tag=listFeeSchedNotesFiltered[i];
				gridMain.ListGridRows.Add(gridRow);
			}
			gridMain.EndUpdate();
			gridMain.ScrollToEnd();
		}

		private void GridMain_CellDoubleClick(object sender,GridClickEventArgs e) {
			int index=gridMain.GetSelectedIndex();
			if(index<0){
				MsgBox.Show("Please select a grid row");
				return;
			}
			FrmFeeSchedNoteEdit frmFeeSchedNoteEdit=new FrmFeeSchedNoteEdit();
			frmFeeSchedNoteEdit.FeeSchedNoteCur=(FeeSchedNote)gridMain.ListGridRows[index].Tag;
			frmFeeSchedNoteEdit.ShowDialog();
			if(frmFeeSchedNoteEdit.IsDialogCancel){
				return;
			}
			FillGrid();
		}

		private void ComboClinic_SelectionChangeCommitted(object sender,EventArgs e) {
			FillGrid();
		}

		private void FrmFeeSchedNotes_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butAdd.IsAltKey(Key.A,e)){
				butAdd_Click(this,new EventArgs());
			}
		}

		private void butAdd_Click(object sender,EventArgs e) {
			FrmFeeSchedNoteEdit frmFeeSchedNoteEdit=new FrmFeeSchedNoteEdit();
			FeeSchedNote feeSchedNote=new FeeSchedNote();
			feeSchedNote.IsNew=true;
			feeSchedNote.FeeSchedNum=FeeSchedCur.FeeSchedNum;
			feeSchedNote.DateEntry=DateTime.Today;
			if(comboClinic.IsAllSelected){
				feeSchedNote.ListClinicNums=comboClinic.ListClinics.Select(x=>x.Id).ToList();
			}
			else{
				feeSchedNote.ListClinicNums.Add(comboClinic.ClinicNumSelected);
			}
			if(comboClinic.IsUnassignedSelected){
				feeSchedNote.ListClinicNums.Add(0);
			}
			frmFeeSchedNoteEdit.FeeSchedNoteCur=feeSchedNote;
			frmFeeSchedNoteEdit.ShowDialog();
			if(frmFeeSchedNoteEdit.IsDialogCancel){
				return;
			}
			FillGrid();
		}
	}
}