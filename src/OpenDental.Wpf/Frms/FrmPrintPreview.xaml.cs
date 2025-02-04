using System;
using System.Windows.Documents;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmPrintPreview : FrmODBase {
		//Yes, this was a quick and dirty window. Works fine, but we really need to build a custom one.
		//Buttons should have labels.
		public FixedDocument FixedDocumentCur;

		
		public FrmPrintPreview(){
			InitializeComponent();
			Load+=FrmPrintPreview_Load;
		}

		private void FrmPrintPreview_Load(object sender, EventArgs e) {
			documentViewer.Document=FixedDocumentCur;
			StartMaximized=true;
		}
	}
}