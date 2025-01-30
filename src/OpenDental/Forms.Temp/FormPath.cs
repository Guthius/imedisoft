using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Cloud.Storage;

namespace OpenDental;

public partial class FormPath : FormODBase {
	///<summary>If this is set to true before opening this form, then the program cannot find the AtoZ path and needs user input.</summary>
	public bool IsStartingUp;
	private string _errorMsg="";
	private bool _didVerifySwitchingFromDBStorage;
	#region Dropbox Private Variables
	private Program _program;
	private ProgramProperty _programPropertyDropboxPathAtoZ;
	private ProgramProperty _programPropertyDropboxAccessToken;
	///<summary>Set to true if the Dropbox API has been loaded already.</summary>
	private bool _hasDropboxLoaded;
	#endregion

	#region Sftp Private Variables
	///<summary>Set to true if the Sftp stuff has been loaded already.</summary>
	private bool _hasSftpLoaded;
	private ProgramProperty _programPropertySftpPathAtoZ;
	private ProgramProperty _programPropertySftpHostname;
	private ProgramProperty _programPropertySftpUsername;
	private ProgramProperty _programPropertySftpPassword;
	#endregion

	///<summary>This is the database storage type that the user has chosen (or was pulled from the database.
	///DO NOT change the value of this variable outside of SetRadioButtonChecked() or there is a chance for a stack overflow exception</summary>
	private DataStorageType _dataStorageType=DataStorageType.LocalAtoZ;

		
	public FormPath(){
		InitializeComponent();

		//We only show the tabs in the designer for development purposes.  We want to hide them for our users.
		//Because the tab control is in "flat buttons" appearance and "fixed size" style the tabs will not show even if they are one pixel tall.
		//0,0 does not work because some size is required.
		tabControlDataStorageType.TabsAreCollapsed=true;
	}

	private void FormPath_Load(object sender, System.EventArgs e){
		if(!IsStartingUp && !Security.IsAuthorized(EnumPermType.Setup)) {//Verify user has Setup permission to change paths, after user has logged in.
			butOK.Enabled=false;
		}
		textDocPath.Text=PrefC.GetString(PrefName.DocPath);
		//ComputerPref compPref=ComputerPrefs.GetForLocalComputer();
		labelServerPath.Text="Path override for this server.";
		textServerPath.Text=ReplicationServers.GetAtoZpath();
		textLocalPath.Text=OpenDentBusiness.FileIO.FileAtoZ.LocalAtoZpath;//This was set on startup.  //compPref.AtoZpath;
		textExportPath.Text=PrefC.GetString(PrefName.ExportPath);
		textLetterMergePath.Text=PrefC.GetString(PrefName.LetterMergePath);
		SetRadioButtonChecked();
		// The opt***_checked event will enable/disable the appropriate UI elements.
		checkMultiplePaths.Checked=(textDocPath.Text.LastIndexOf(';')!=-1);	
		//Also set the "multiple paths" checkbox at startup based on the current image folder list format. No need to store this info in the db.
		if(IsStartingUp) {//and failed to find path
			MsgBox.Show(this,"Could not find the path for the AtoZ folder.");
			if(Security.CurUser==null || !Security.IsAuthorized(EnumPermType.Setup)) {
				//The user is still allowed to set the "Path override for this computer", thus the user has a way to temporariliy get into OD in worst case.
				//For example, if the primary folder path is wrong or has changed, the user can set the path override for this computer to get into OD, then
				//can to to Setup | Data Paths to fix the primary path.
				DisableMostControls();
				textLocalPath.ReadOnly=false;
				butBrowseLocal.Enabled=true;
				ActiveControl=textLocalPath;//Focus on textLocalPath, since this is the only textbox the user can edit in this case.
			}
		}
		if(true) {
			radioDatabaseStorage.Visible=false;
		}
	}

	/// <summary>Returns true if user really wants to continue or N/A. Verifies if there is RawBase64 data currently stored in the database. It will warn users that switching away means they are no longer able to access that data.</summary>
	private bool VerifySwitchingAwayFromDBStorage() {
		if(_didVerifySwitchingFromDBStorage) {
			return true;//already verified
		}
		if(true) {
			return true;//N/A
		}
		if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"You have chosen to switch away from storing images in the database. If you continue, you will not be able to switch back and you will lose access to your existing Imaging Module data currently stored in the database. Continue anyway?"))
		{
			//user will have one more chance to cancel because they can just cancel out of the form.
			SetRadioButtonChecked();
			return false;//changed their mind
		}
		_didVerifySwitchingFromDBStorage=true;
		return true;
	}

	private void DisableMostControls() {
		radioUseFolder.Enabled=false;
		textDocPath.ReadOnly=true;
		butBrowseDoc.Enabled=false;
		checkMultiplePaths.Enabled=false;
		textServerPath.ReadOnly=true;
		butBrowseServer.Enabled=false;
		radioDatabaseStorage.Enabled=false;
		textExportPath.ReadOnly=true;
		butBrowseExport.Enabled=false;
		textLetterMergePath.ReadOnly=true;
		butBrowseLetter.Enabled=false;
		textLocalPath.ReadOnly=true;
		butBrowseLocal.Enabled=false;
	}

	private void SetRadioButtonChecked() {
		_dataStorageType=DataStorageType.LocalAtoZ;
		radioUseFolder.Checked=true;//Will only do something when SetRadioButtonChecked is called on Load
		tabControlDataStorageType.SelectedTab=tabAtoZ;
	}

	///<summary>Tries to show the file browser dialog to the user.  Returns true if the user actually selected a path from the dialog.
	///Returns false if the user cancels out.  Also, shows a warning message and returns false if an exception occurred.</summary>
	private bool ShowFileBrowserDialog() {
		//A customer is having a "Unable to retrieve root folder" unhandled exception occur when trying to show the file browser dialog.
		//Therefore, try to show the dialog and if any exception occurs simply show a message box giving some suggestions to the user.
		try {
			return (folderBrowserDialog.ShowDialog()==DialogResult.OK);
		}
		catch(Exception) {
			MsgBox.Show(this,"There was an error showing the Browse window.\r\nTry running as an Administrator or manually typing in a path.");
			return false;
		}
	}

	///<summary>Returns the given path with the local OS path separators as necessary.</summary>
	public static string FixDirSeparators(string path){
		if(Environment.OSVersion.Platform==PlatformID.Unix){
			path.Replace('\\',Path.DirectorySeparatorChar);
		}
		else{//Windows
			path.Replace('/',Path.DirectorySeparatorChar);
		}
		return path;
	}

	private void butBrowseDoc_Click(object sender,EventArgs e) {
		if(!ShowFileBrowserDialog()) {
			return;
		}
		//Ensure that the path entered has slashes matching the current OS (in case entered manually).
		var path=FixDirSeparators(folderBrowserDialog.SelectedPath);
		if(checkMultiplePaths.Checked && textDocPath.Text.Length>0) {
			var messageText=Lan.g(this,"Replace existing document paths? Click No to add path to existing document paths.");
			switch(ODMessageBox.Show(messageText,"",MessageBoxButtons.YesNoCancel)) {
				case DialogResult.Yes:
					textDocPath.Text=path;//Replace existing paths with new path.
					break;
				case DialogResult.No://Append to existing paths?
					//Do not append a path which is already present in the list.
					if(!IsPathInList(path,textDocPath.Text)) {
						textDocPath.Text=textDocPath.Text+";"+path;
					}
					break;
				default://Cancel button.
					break;
			}
		}
		else{
			textDocPath.Text=path;//Just replace existing paths with new path.
		}
	}

	private void butBrowseServer_Click(object sender,EventArgs e) {
		if(ShowFileBrowserDialog()) {
			textServerPath.Text=folderBrowserDialog.SelectedPath;
		}
	}

	private void butBrowseLocal_Click(object sender,EventArgs e) {
		if(ShowFileBrowserDialog()) {
			textLocalPath.Text=folderBrowserDialog.SelectedPath;
		}
	}

	private void butBrowseExport_Click(object sender, System.EventArgs e) {
		if(ShowFileBrowserDialog()) {
			textExportPath.Text=folderBrowserDialog.SelectedPath;
		}
	}

	private void butBrowseLetter_Click(object sender, System.EventArgs e) {
		if(ShowFileBrowserDialog()) {
			textLetterMergePath.Text=folderBrowserDialog.SelectedPath;
		}
	}

	///<summary>Returns true if the given path is part of the imagePaths list, false otherwise.</summary>
	private static bool IsPathInList(string path,string imagePaths){
		var stringArrayPaths=imagePaths.Split(';');
		for(var i=0;i<stringArrayPaths.Length;i++){
			if(stringArrayPaths[i]==path){//Case sensitive (since these could be unix paths).
				return true;
			}
		}
		return false;
	}

	private void radioUseFolder_Click(object sender,EventArgs e) {
		if(!VerifySwitchingAwayFromDBStorage()) { //they clicked cancel
			return;
		}
		labelPathSameForAll.Enabled = radioUseFolder.Checked;
		textDocPath.Enabled = radioUseFolder.Checked;
		butBrowseDoc.Enabled = radioUseFolder.Checked;
		checkMultiplePaths.Enabled = radioUseFolder.Checked;
		//even though server path might not be visible:
		labelServerPath.Enabled=radioUseFolder.Checked;
		textServerPath.Enabled=radioUseFolder.Checked;
		butBrowseServer.Enabled=radioUseFolder.Checked;
		//
		labelLocalPath.Enabled=radioUseFolder.Checked;
		textLocalPath.Enabled=radioUseFolder.Checked;
		butBrowseLocal.Enabled=radioUseFolder.Checked;
		SetRadioButtonChecked();
	}

	private void radioDatabaseStorage_Click(object sender,EventArgs e) {
		if(radioDatabaseStorage.Checked && true){//user attempting to use db to store images
			var inputbox=new InputBox("Please enter password");
			inputbox.ShowDialog();
			if(inputbox.IsDialogCancel){
				SetRadioButtonChecked();
				return;
			}
			if(inputbox.StringResult!="abracadabra"){//to keep ignorant people from clicking this box.
				SetRadioButtonChecked();
				MsgBox.Show(this,"Wrong password");
				return;
			}
		}
		SetRadioButtonChecked();
	}

	private void butSave_Click(object sender, System.EventArgs e){
		//remember that user might be using a website or a linux box to store images, therefore must allow forward slashes.
		if(radioUseFolder.Checked){
			if(textLocalPath.Text!="") {
				if(OpenDentBusiness.FileIO.FileAtoZ.GetValidPathFromString(textLocalPath.Text)==null) {
					MsgBox.Show(this,"The path override for this computer is invalid.  The folder must exist and must contain all 26 A through Z folders.");
					return;
				}
			}
			else if(textServerPath.Text!="") {
				if(OpenDentBusiness.FileIO.FileAtoZ.GetValidPathFromString(textServerPath.Text)==null) {
					MsgBox.Show(this,"The path override for this server is invalid.  The folder must exist and must contain all 26 A through Z folders.");
					return;
				}
			}
			else {
				if(OpenDentBusiness.FileIO.FileAtoZ.GetValidPathFromString(textDocPath.Text)==null) {
					MsgBox.Show(this,"The path is invalid.  The folder must exist and must contain all 26 A through Z folders.");
					return;
				}
			}				
		}
		var isChanged=false;
		isChanged|=Prefs.UpdateInt(PrefName.AtoZfolderUsed,(int)_dataStorageType);
		isChanged|=Prefs.UpdateString(PrefName.DocPath,textDocPath.Text);
		isChanged|=Prefs.UpdateString(PrefName.ExportPath,textExportPath.Text);
		isChanged|=Prefs.UpdateString(PrefName.LetterMergePath,textLetterMergePath.Text);
		if(isChanged) { 
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		if(OpenDentBusiness.FileIO.FileAtoZ.LocalAtoZpath!=textLocalPath.Text) {//if local path changed
			OpenDentBusiness.FileIO.FileAtoZ.LocalAtoZpath=textLocalPath.Text;
			ComputerPrefs.LocalComputer.AtoZpath=OpenDentBusiness.FileIO.FileAtoZ.LocalAtoZpath;
			ComputerPrefs.Update(ComputerPrefs.LocalComputer);
		}
		if(ReplicationServers.GetAtoZpath()!=textServerPath.Text) {
			var replicationServer=ReplicationServers.GetForLocalComputer();
			replicationServer.AtoZpath=textServerPath.Text;
			ReplicationServers.Update(replicationServer);
		}
		SecurityLogs.MakeLogEntry(EnumPermType.Setup,0,"Data Path");
		DialogResult=DialogResult.OK;
	}

	private void FormPath_Closing(object sender,System.ComponentModel.CancelEventArgs e) {
		folderBrowserDialog?.Dispose();
		/*
		if(DialogResult==DialogResult.OK) {
			return;
		}
		if(!IsStartingUp) {
			return;
		}
		//no need to check paths here.  If user hits cancel when starting up, it should always notify and exit.
		if(radioUseFolder.Checked
			&& ImageStore.GetValidPathFromString(textDocPath.Text)==null
			&& ImageStore.GetValidPathFromString(textLocalPath.Text)==null)
		{
			MsgBox.Show(this,"Invalid A to Z path.  Closing program.");
			Application.Exit();
		}*/
	}

}