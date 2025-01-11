using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmFeeSchedNoteEdit : FrmODBase {
		public FeeSchedNote FeeSchedNoteCur;

		
		public FrmFeeSchedNoteEdit() {
			InitializeComponent();
			Load+=Frm_Load;
			KeyDown+=FrmFeeSchedNoteEdit_KeyDown;
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			textNote.Text=FeeSchedNoteCur.Note;
			comboClinic.ListClinicNumsSelected=FeeSchedNoteCur.ListClinicNums;
			textDate.Value=FeeSchedNoteCur.DateEntry;
			if(!FeeSchedNoteCur.IsNew){
				labelNote_Copy.Visible=false;
			}
		}

		private void FrmFeeSchedNoteEdit_KeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)){
				butSave_Click(this,new EventArgs());
				return;
			}
			if(butDelete.IsAltKey(Key.D,e)){
				butDelete_Click(this,new EventArgs());
				return;
			}
		}

		private void butDelete_Click(object sender,EventArgs e) {
			if(!MsgBox.Show(MsgBoxButtons.OKCancel,"Delete?")){
				return;
			}
			if(FeeSchedNoteCur.IsNew){
				IsDialogCancel=true;
				return;
			}
			FeeSchedNotes.Delete(FeeSchedNoteCur.FeeSchedNoteNum);
			IsDialogOK=true;
		}

		private void butSave_Click(object sender, EventArgs e) {
			if(!textDate.IsValid()){
				MsgBox.Show("Please enter a valid date");
				return;
			}
			FeeSchedNoteCur.Note=textNote.Text;
			FeeSchedNoteCur.ListClinicNums=comboClinic.ListClinicNumsSelected;
			FeeSchedNoteCur.ClinicNums=FeeSchedNotes.ConvertClinicNumsToString(comboClinic.ListClinicNumsSelected);
			FeeSchedNoteCur.DateEntry=textDate.Value;
			if(FeeSchedNoteCur.IsNew){
				FeeSchedNotes.Insert(FeeSchedNoteCur);
			}
			else{
				FeeSchedNotes.Update(FeeSchedNoteCur);
			}
			IsDialogOK=true;
		}
	}
}