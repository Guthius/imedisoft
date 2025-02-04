using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenDental;

public partial class FormMessageReplacementTextEdit:FormODBase {
	///<summary>Set this to override the title of the form.</summary>
	public string FormTitle;
	public string TextEditorText;

	public FormMessageReplacementTextEdit(string formTitle="") {
		FormTitle=formTitle;
		InitializeComponent();
	}

	private void FormMessageReplacementTextEdit_Load(object sender,EventArgs e) {
		if(!string.IsNullOrWhiteSpace(FormTitle)) {
			this.Text=FormTitle;
		}
		textBoxEditor.Text=TextEditorText;
	}

	private void butReplacementText_Click(object sender,EventArgs e) {
		var listMessageReplaceTypes=new List<MessageReplaceType>();
		listMessageReplaceTypes.Add(MessageReplaceType.PaymentPlan);
		var frmMessageReplacements=new FrmMessageReplacements(listMessageReplaceTypes,false);
		frmMessageReplacements.ShowDialog();
		if(frmMessageReplacements.IsDialogCancel){
			return;
		}
		textBoxEditor.SelectedText=frmMessageReplacements.ReplacementTextSelected;
	}

	private void butSave_Click(object sender,EventArgs e) {
		TextEditorText=textBoxEditor.Text;
		DialogResult=DialogResult.OK;
	}

}