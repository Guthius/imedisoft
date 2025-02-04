using System;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormPayTerminalEdit:FormODBase {
	private PayTerminal _payTerminalCur;
	private bool _isNew;

	public FormPayTerminalEdit(PayTerminal payTerminalCur=null) {
		InitializeComponent();

		if(payTerminalCur!=null) {
			_payTerminalCur=payTerminalCur;
			_isNew=false;
		}
		else {
			_payTerminalCur=new PayTerminal();
			_isNew=true;
		}
	}
	private void FormPayTerminalEdit_Load(object sender,EventArgs e) {
		if(!_isNew) {
			comboClinic.ClinicNumSelected=_payTerminalCur.ClinicNum;
			textName.Text=_payTerminalCur.Name;
			textId.Text=_payTerminalCur.TerminalID;
		}
	}

	private void butDelete_Click(object sender,EventArgs e) {
		if(_isNew) {
			DialogResult=DialogResult.Cancel;
			Close();
		}
		if(MsgBox.Show(MsgBoxButtons.YesNo,Lans.g("Are you sure you would like to delete this payment terminal?"))) {
			PayTerminals.Delete(_payTerminalCur.PayTerminalNum);
			Close();
		}
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(textId.Text.IsNullOrEmpty()) {
			MsgBox.Show(this,"Terminal ID is required.");
			return;
		}
		_payTerminalCur.Name=textName.Text;
		_payTerminalCur.TerminalID=textId.Text;
		_payTerminalCur.ClinicNum=comboClinic.ClinicNumSelected;
		if(_isNew) {
			PayTerminals.Insert(_payTerminalCur);
		}
		else {
			PayTerminals.Update(_payTerminalCur);
		}
		DialogResult=DialogResult.OK;
	}

}