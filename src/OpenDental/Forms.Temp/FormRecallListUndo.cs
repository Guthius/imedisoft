using System;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormRecallListUndo:FormODBase {
	public FormRecallListUndo() {
		InitializeComponent();
	}

	private void FormRecallListUndo_Load(object sender,EventArgs e) {
		textDate.Text=DateTime.Today.ToShortDateString();
	}

	private void textDate_TextChanged(object sender,EventArgs e) {
		if(textDate.IsValid()) {
			var count=Commlogs.GetRecallUndoCount(SIn.Date(textDate.Text));
			labelCount.Text=count.ToString();
			return;
		}
		labelCount.Text="";
			
	}

	private void butOK_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.SecurityAdmin)) {
			return;
		}
		if(!textDate.IsValid()) {
			MsgBox.Show(this,"Invalid date");
			return;
		}
		var date=SIn.Date(textDate.Text);
		if(date < DateTime.Today.AddDays(-7)){
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Date is from more than one week ago.  Continue anyway?")){
				return;
			}
		}
		if(ODMessageBox.Show("Delete all "+labelCount.Text+" commlog entries?","",MessageBoxButtons.OKCancel)!=DialogResult.OK) {
			return;
		}
		Commlogs.RecallUndo(date);
		SecurityLogs.MakeLogEntry(EnumPermType.CommlogEdit,0,"Recall list undo tool ran");
		MsgBox.Show(this,"Done");
		DialogResult=DialogResult.OK;
	}

}