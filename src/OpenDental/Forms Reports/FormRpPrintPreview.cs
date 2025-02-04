using System;
using System.Windows.Forms;
using CodeBase;
using System.Drawing.Printing;
using OpenDental.Logic;

namespace OpenDental;

///<summary>This form is only launched and used in Debug.</summary>
public partial class FormRpPrintPreview : FormODBase {

		
	public FormRpPrintPreview() {
		InitializeComponent();
	}
		
	public FormRpPrintPreview(PrintDocument printDoc) : this() {
		_printPreviewControl2.Document=printDoc;
	}

		
	public FormRpPrintPreview(ODprintout printout) : this() {
		if(printout.SettingsErrorCode!=PrintoutErrorCode.Success) {
			PrinterL.ShowError(printout);
			this.DialogResult=DialogResult.Cancel;
			return;
		}
		_printPreviewControl2.Document=printout.PrintDoc;
	}

	private void FormRpPrintPreview_Load(object sender, System.EventArgs e) {
		//LayoutManager.MoveLocation(butNext,new Point(this.ClientRectangle.Width-100,this.ClientRectangle.Height-30));
		//LayoutManager.MoveLocation(butPrev,new Point(this.ClientRectangle.Width-butPrev.Width-110,this.ClientRectangle.Height-30));
		_printPreviewControl2.Height=this.ClientRectangle.Height-40;
		_printPreviewControl2.Width=this.ClientRectangle.Width;
		_printPreviewControl2.Zoom=(double)_printPreviewControl2.ClientSize.Height
		                           /1100;
	}

	private void butNext_Click(object sender,System.EventArgs e) {
		_printPreviewControl2.StartPage++;
	}

	private void butPrev_Click(object sender,EventArgs e) {
		if(_printPreviewControl2.StartPage>0) {
			_printPreviewControl2.StartPage--;
		}
	}
}