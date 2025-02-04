using System;
using System.Windows;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmParagraph : FrmODBase {
		public int TextIndent;
		public int LeftMargin;
		public TextAlignment TextAlignment_;

		
		public FrmParagraph() {
			InitializeComponent();
			Load+=Frm_Load;
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			textVIntIndent.Value=TextIndent;
			textVIntLeftMargin.Value=LeftMargin;
			listBoxAlignment.SelectedIndex=(int)TextAlignment_;
		}

		private void butHanging_Click(object sender,EventArgs e) {
			textVIntIndent.Value=-25;
			textVIntLeftMargin.Value=25;
		}

		private void butSave_Click(object sender, EventArgs e) {
			if(!textVIntIndent.IsValid()){
				MsgBox.Show("Please fix errors first.");
				return;
			}
			TextIndent=textVIntIndent.Value;
			LeftMargin=textVIntLeftMargin.Value;
			TextAlignment_=(TextAlignment)listBoxAlignment.SelectedIndex;
			IsDialogOK=true;
		}
	}
}