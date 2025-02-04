using System;
using System.Windows.Input;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmImageFilter : FrmODBase {
		///<summary>Does not control showing drawings for Pearl or any other external source.</summary>
		public bool ShowOD;

		
		public FrmImageFilter(){
			InitializeComponent();
			Load+=FrmImageFilter_Load;
			PreviewKeyDown+=FrmImageFilter_PreviewKeyDown;
		}

		private void FrmImageFilter_Load(object sender, EventArgs e) {
			Lang.F(this);
			checkShowOD.Checked=ShowOD;
		}

		private void FrmImageFilter_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void butSave_Click(object sender, EventArgs e) {
			ShowOD=checkShowOD.Checked==true;
			IsDialogOK=true;
		}
	}
}