using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDental.UI;

namespace OpenDental{
	/// <summary></summary>
	public partial class FormNotePick:FormODBase {
		
		public string[] StringArrayNotes;
		///<summary>Upon closing with OK, this will be the final note to save.</summary>
		public string NoteSelected;
		public bool UseTrojanImportDescription;

		
		public FormNotePick() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
			//_stringArrayNotes=new string[stringArrayNotes.Length];
			//stringArrayNotes.CopyTo(_stringArrayNotes,0);
			if(UseTrojanImportDescription) {
				label1.Text=Lan.g(this,"Multiple versions of the note exist.  Please pick or edit one version to retain. You can also pick multiple rows to combine notes.");
				label2.Text=Lan.g(this,"This is the final note that will be used.");
			}
		}

		private void FormNotePick_Load(object sender,EventArgs e) {
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn col=new GridColumn("",630);
			gridMain.Columns.Add(col);
			gridMain.ListGridRows.Clear();
			GridRow row;
			for(int i=0;i<StringArrayNotes.Length;i++) {
				row=new GridRow();
				row.Cells.Add(StringArrayNotes[i]);
				gridMain.ListGridRows.Add(row);
			}
			gridMain.EndUpdate();
			if(StringArrayNotes.Length>0){
				gridMain.SetSelected(0,true);
				textNote.Text=StringArrayNotes[0];
			}
		}

		private void gridMain_CellClick(object sender,ODGridClickEventArgs e) {
			textNote.Text="";
			for(int i=0;i<gridMain.SelectedIndices.Length;i++){
				textNote.Text+=StringArrayNotes[gridMain.SelectedIndices[i]];
			}
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			NoteSelected=StringArrayNotes[e.Row];
			DialogResult=DialogResult.OK;
		}

		private void butOK_Click(object sender, System.EventArgs e) {
			NoteSelected=textNote.Text;
			DialogResult=DialogResult.OK;
		}

	}
}