using System;
using System.Windows.Forms;
using DataConnectionBase;

namespace OpenDental;

public partial class FormPrepaymentEdit:FormODBase {
	public int CountCur;

	public FormPrepaymentEdit() {
		InitializeComponent();
	}

	private void FormPrepaymentEdit_Load(object sender,EventArgs e) {
		this.ActiveControl=textBox1;
		textBox1.Text=CountCur.ToString();
	}

	private void butSave_Click(object sender,EventArgs e) {
		CountCur=SIn.Int(textBox1.Text);
		DialogResult=DialogResult.OK;
	}

}