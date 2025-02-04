using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

/// <summary> </summary>
public partial class FormMountainside:FormODBase {
	/// <summary>This Program link is new.</summary>
	public bool IsNew;
	public Program ProgramCur;
	//private static Thread thread;

		
	public FormMountainside() {
		components=new System.ComponentModel.Container();
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormEClinicalWorks_Load(object sender, System.EventArgs e) {
		FillForm();
	}

	private void FillForm(){
		ProgramProperties.RefreshCache();
		ProgramProperties.GetForProgram(ProgramCur.ProgramNum);
		textProgName.Text=ProgramCur.ProgName;
		textProgDesc.Text=ProgramCur.ProgDesc;
		checkEnabled.Checked=ProgramCur.Enabled;
		textHL7FolderOut.Text=PrefC.GetString(PrefName.HL7FolderOut);
	}

	private void checkEnabled_Click(object sender,EventArgs e) {
		MsgBox.Show(this,"You will need to restart Open Dental to see the effects.");
	}

	private bool SaveToDb(){
		if(textProgDesc.Text==""){
			MsgBox.Show(this,"Description may not be blank.");
			return false;
		}
		if(textHL7FolderOut.Text=="") {
			MsgBox.Show(this,"HL7 out folder may not be blank.");
			return false;
		}
		ProgramCur.ProgDesc=textProgDesc.Text;
		ProgramCur.Enabled=checkEnabled.Checked;
		Programs.Update(ProgramCur);
		Prefs.UpdateString(PrefName.HL7FolderOut,textHL7FolderOut.Text);
		DataValid.SetInvalid(InvalidType.Programs,InvalidType.Prefs);
		return true;
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(!SaveToDb()){
			return;
		}
		DialogResult=DialogResult.OK;
	}

}