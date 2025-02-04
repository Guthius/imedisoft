using System;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormReferenceEntryEdit:FormODBase {
	private CustRefEntry _custRefEntry;

	public FormReferenceEntryEdit(CustRefEntry custRefEntry) {
		InitializeComponent();

		_custRefEntry=custRefEntry;
	}

	private void FormReferenceEdit_Load(object sender,EventArgs e) {
		textCustomer.Text=CustReferences.GetCustNameFL(_custRefEntry.PatNumCust);
		textReferredTo.Text=CustReferences.GetCustNameFL(_custRefEntry.PatNumRef);
		if(_custRefEntry.DateEntry.Year>1880) {
			textDateEntry.Text=_custRefEntry.DateEntry.ToShortDateString();
		}
		textNote.Text=_custRefEntry.Note;
	}

	private void butDeleteAll_Click(object sender,EventArgs e) {
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Delete Reference Entry?")) {
			return;
		}
		CustRefEntries.Delete(_custRefEntry.CustRefEntryNum);
		DialogResult=DialogResult.OK;
	}

	private void butSave_Click(object sender,EventArgs e) {
		_custRefEntry.Note=textNote.Text;
		CustRefEntries.Update(_custRefEntry);
		DialogResult=DialogResult.OK;
	}

}