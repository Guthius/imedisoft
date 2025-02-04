using System;
using System.Windows.Input;

namespace OpenDental {
	public partial class FrmAutoNotePromptPreview:FrmODBase {
		public string ResultText;

		public FrmAutoNotePromptPreview() {
			InitializeComponent();
			Load+=FrmAutoNotePromptPreview_Load;
			PreviewKeyDown+=FrmAutoNotePromptPreview_PreviewKeyDown;
		}

		private void FrmAutoNotePromptPreview_Load(object sender,EventArgs e) {
			Lang.F(this);
			textMain.Text=ResultText;
		}
		private void FrmAutoNotePromptPreview_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void butSave_Click(object sender,EventArgs e) {
			ResultText=textMain.Text;
			IsDialogOK=true;
		}

	}
}