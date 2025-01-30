using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Bridges;
using OpenDental.Forms;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormQuickBooksSetup:FormODBase {
	private List<string> _listDepositAccountsQB;
	private List<string> _listIncomeAccountsQB;
	///<summary>The list of currently available QuickBook classes.  Stored in a preference.</summary>
	private List<string> _listClassRefsQB;

	public FormQuickBooksSetup() {
		InitializeComponent();
	}

	private void FormQuickBooksSetup_Load(object sender,EventArgs e) {
		textCompanyFileQB.Text=PrefC.GetString(PrefName.QuickBooksCompanyFile);
		checkQuickBooksClassRefsEnabled.Checked=PrefC.GetBool(PrefName.QuickBooksClassRefsEnabled);
		var acctStr=PrefC.GetString(PrefName.QuickBooksIncomeAccount);
		var stringArrayAccts=acctStr.Split(',');
		_listIncomeAccountsQB= [];
		for(var i=0;i<stringArrayAccts.Length;i++) {
			if(stringArrayAccts[i]=="") {
				continue;
			}
			_listIncomeAccountsQB.Add(stringArrayAccts[i]);
		}
		var depStr=PrefC.GetString(PrefName.QuickBooksDepositAccounts);
		var stringArrayDeposits=depStr.Split(',');
		_listDepositAccountsQB= [];
		for(var i=0;i<stringArrayDeposits.Length;i++) {
			if(stringArrayDeposits[i]=="") {
				continue;
			}
			_listDepositAccountsQB.Add(stringArrayDeposits[i]);
		}
		var classStr=PrefC.GetString(PrefName.QuickBooksClassRefs);
		var stringArrayClass=classStr.Split(',');
		_listClassRefsQB= [];
		for(var i=0;i<stringArrayClass.Length;i++) {
			if(stringArrayClass[i]=="") {
				continue;
			}
			_listClassRefsQB.Add(stringArrayClass[i]);
		}
		FillQBLists();
	}

	private void FillQBLists() {
		listBoxDepositAccountsQB.Items.Clear();
		listBoxDepositAccountsQB.Items.AddList(_listDepositAccountsQB,x => x.ToString());
		listBoxIncomeAccountsQB.Items.Clear();
		listBoxIncomeAccountsQB.Items.AddList(_listIncomeAccountsQB,x => x.ToString());
		listBoxClassRefsQB.Items.Clear();
		listBoxClassRefsQB.Items.AddList(_listClassRefsQB,x => x.ToString());
	}

	private void butBrowseQB_Click(object sender,EventArgs e) {
		using var openFileDialog=new OpenFileDialog();
		openFileDialog.Title="QuickBooks Company File";
		openFileDialog.InitialDirectory=@"C:\";
		openFileDialog.Filter="QuickBooks|*.qbw";
		openFileDialog.RestoreDirectory=true;
		if(openFileDialog.ShowDialog()==DialogResult.OK) {
			textCompanyFileQB.Text=openFileDialog.FileName;
		}
	}

	private void butConnectQB_Click(object sender,EventArgs e) {
		if(textCompanyFileQB.Text.Trim()=="") {
			MsgBox.Show(this,"Browse to your QuickBooks company file first.");
			return;
		}
		Cursor.Current=Cursors.WaitCursor;
		var result=QuickBooks.TestConnection(textCompanyFileQB.Text);
		Cursor.Current=Cursors.Default;
		ODMessageBox.Show(result);
	}

	private void butAddDepositQB_Click(object sender,EventArgs e) {
		var listDeposits=GetAccountsQB();
		if(listDeposits==null){
			return;
		}
		_listDepositAccountsQB.AddRange(listDeposits);
		FillQBLists();
	}

	private void butRemoveDepositQB_Click(object sender,EventArgs e) {
		if(listBoxDepositAccountsQB.SelectedIndex==-1) {
			MsgBox.Show(this,"Please select an item first.");
			return;
		}
		_listDepositAccountsQB.RemoveAt(listBoxDepositAccountsQB.SelectedIndex);
		FillQBLists();
	}

	private void butAddIncomeQB_Click(object sender,EventArgs e) {
		var listIncomes=GetAccountsQB();
		if(listIncomes==null){
			return;
		}
		_listIncomeAccountsQB.AddRange(listIncomes);
		FillQBLists();
	}

	private void butRemoveIncomeQB_Click(object sender,EventArgs e) {
		if(listBoxIncomeAccountsQB.SelectedIndex==-1) {
			MsgBox.Show(this,"Please select an item first.");
			return;
		}
		_listIncomeAccountsQB.RemoveAt(listBoxIncomeAccountsQB.SelectedIndex);
		FillQBLists();
	}

	private void buttonAddClassRefQB_Click(object sender,EventArgs e) {
		if(textCompanyFileQB.Text.Trim()=="") {
			MsgBox.Show(this,"Browse to your QuickBooks company file first.");
			return;
		}
		if(Prefs.UpdateString(PrefName.QuickBooksCompanyFile,textCompanyFileQB.Text)) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		var listClasses=new List<string>();
		try {
			listClasses=QuickBooks.GetListOfClasses();
		}
		catch(Exception ex) {
			ODMessageBox.Show(ex.Message);
			return;
		}
		var inputBoxParam=new InputBoxParam();
		if(listClasses.Count>=10){
			inputBoxParam.InputBoxType_=InputBoxType.ComboMultiSelect;
		}
		else{
			inputBoxParam.InputBoxType_=InputBoxType.ListBoxMulti;
		}
		inputBoxParam.LabelText=Lan.g(this,"Choose a class"); 
		inputBoxParam.ListSelections=listClasses;
		var inputBoxFormChooseClasses=new InputBox(inputBoxParam);
		inputBoxFormChooseClasses.ShowDialog();
		if(inputBoxFormChooseClasses.IsDialogCancel) {
			return;
		}
		if(inputBoxFormChooseClasses.SelectedIndices.Count < 1) {
			MsgBox.Show(this,"You must choose a class.");
			return;
		}
		for(var i=0;i<inputBoxFormChooseClasses.SelectedIndices.Count;i++) {
			var classCur=listClasses[inputBoxFormChooseClasses.SelectedIndices[i]];
			if(_listClassRefsQB.Contains(classCur)) {
				continue;
			}
			_listClassRefsQB.Add(classCur);
		}
		FillQBLists();
	}

	private void buttonRemoveClassRefQB_Click(object sender,EventArgs e) {
		if(listBoxClassRefsQB.SelectedIndex==-1) {
			MsgBox.Show(this,"Please select an item first.");
			return;
		}
		_listClassRefsQB.RemoveAt(listBoxClassRefsQB.SelectedIndex);
		FillQBLists();
	}

	///<summary>Launches the account pick window and lets user choose several accounts.  Returns null if anything went wrong or user canceled out.</summary>
	private List<string> GetAccountsQB() {
		if(textCompanyFileQB.Text.Trim()=="") {
			MsgBox.Show(this,"Browse to your QuickBooks company file first.");
			return null;
		}
		if(Prefs.UpdateString(PrefName.QuickBooksCompanyFile,textCompanyFileQB.Text)) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		using var formAccountPick=new FormAccountPick();
		formAccountPick.IsQuickBooks=true;
		formAccountPick.ShowDialog();
		if(formAccountPick.DialogResult!=DialogResult.OK) {
			return null;
		}
		if(formAccountPick.SelectedQuickBooksAccounts!=null) {
			return formAccountPick.SelectedQuickBooksAccounts;
		}
		return null;
	}

	private void checkQuickBooksClassRefsEnabled_CheckedChanged(object sender,EventArgs e) {
		if(checkQuickBooksClassRefsEnabled.Checked) {
			labelClass.Visible=true;
			listBoxClassRefsQB.Visible=true;
			buttonAddClassRefQB.Visible=true;
			buttonRemoveClassRefQB.Visible=true;
			return;
		}
		labelClass.Visible=false;
		listBoxClassRefsQB.Visible=false;
		buttonAddClassRefQB.Visible=false;
		buttonRemoveClassRefQB.Visible=false;
	}

	private void butSave_Click(object sender,EventArgs e) {
		var depStr="";
		for(var i=0;i<listBoxDepositAccountsQB.Items.Count;i++) {
			if(i>0) {
				depStr+=",";
			}
			depStr+=listBoxDepositAccountsQB.Items.GetTextShowingAt(i);
		}
		var incomeStr="";
		for(var i=0;i<listBoxIncomeAccountsQB.Items.Count;i++) {
			if(i>0) {
				incomeStr+=",";
			}
			incomeStr+=listBoxIncomeAccountsQB.Items.GetTextShowingAt(i);
		}
		var classStr="";
		for(var i=0;i<listBoxClassRefsQB.Items.Count;i++) {
			if(i>0) {
				classStr+=",";
			}
			classStr+=listBoxClassRefsQB.Items.GetTextShowingAt(i);
		}
		if(Prefs.UpdateString(PrefName.QuickBooksCompanyFile,textCompanyFileQB.Text)
		   | Prefs.UpdateString(PrefName.QuickBooksDepositAccounts,depStr)
		   | Prefs.UpdateString(PrefName.QuickBooksIncomeAccount,incomeStr)
		   | Prefs.UpdateBool(PrefName.QuickBooksClassRefsEnabled,checkQuickBooksClassRefsEnabled.Checked)
		   | Prefs.UpdateString(PrefName.QuickBooksClassRefs,classStr)) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		DialogResult=DialogResult.OK;
	}

}