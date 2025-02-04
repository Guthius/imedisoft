using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using OpenDental.UI;
using System.Text.RegularExpressions;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormProcGroup:FormODBase {
	public List<ClaimProcHist> ListClaimProcHists;
	public Procedure ProcedureGroup;
	private Procedure _procedureGroupOld;
	public List<ProcGroupItem> ListProcGroupItems;
	public List<Procedure> ListProcedures;
	private List<Procedure> ListProceduresOld;
	///<summary>This keeps the noteChanged event from erasing the signature when first loading.</summary>
	private bool _isStartingUp;
	private bool _hasSigChanged;
	private Patient _patient;
	///<summary>Users can temporarily log in on this form.  Defaults to Security.CurUser.</summary>
	private Userod _userod=Security.CurUser;
	///<summary>True if the user clicked the Change User button.</summary>
	private bool _hasUserChanged;
	///<summary>True if group note is attached to at least one completed proc.  Used for determining which permission to use.</summary>
	private bool _attachedToCompletedProc;
	private const string _autoNotePromptRegex=@"\[Prompt:""[a-zA-Z_0-9 ]+""\]";

	public FormProcGroup() {
		InitializeComponent();
	}
	
	private void FormProcGroup_Load(object sender, System.EventArgs e){
		signatureBoxWrapper.SetAllowDigitalSig(true);
		_isStartingUp=true;
		//ProcList gets set in ContrChart where this form is created.
		_patient=Patients.GetPat(ProcedureGroup.PatNum);
		Patients.GetFamily(ProcedureGroup.PatNum);
		_procedureGroupOld=ProcedureGroup.Copy();
		ListProceduresOld= [];
		for(var i=0;i<ListProcedures.Count;i++){
			ListProceduresOld.Add(ListProcedures[i].Copy());
		}
		textProcDate.Text=ProcedureGroup.ProcDate.ToShortDateString();
		textDateEntry.Text=ProcedureGroup.DateEntryC.ToShortDateString();
		textUser.Text=Userods.GetName(ProcedureGroup.UserNum);//might be blank. Will change automatically if user changes note or alters sig.
		textNotes.Text=ProcedureGroup.Note;
		if(ProcedureGroup.ProcStatus==ProcStat.EC && PrefC.GetBool(PrefName.ProcLockingIsAllowed) && !ProcedureGroup.IsLocked) {
			butLock.Visible=true;
		}
		else {
			butLock.Visible=false;
		}
		var permissions=EnumPermType.ProcCompleteEdit;
		if(ProcedureGroup.ProcStatus.In(ProcStat.EO,ProcStat.EC)) {
			permissions=EnumPermType.ProcExistingEdit;
		}
		if(Security.IsGlobalDateLock(permissions,ProcedureGroup.ProcDate)) {
			butLock.Enabled=false;
		}
		if(ProcedureGroup.IsLocked) {//Whether locking is currently allowed, this proc group may have been locked previously.
			butSave.Enabled=false;
			butDelete.Enabled=false;
			labelLocked.Visible=true;
			butAppend.Visible=true;
			textNotes.ReadOnly=true;//just for visual cue.  No way to save changes, anyway.
			textNotes.BackColor=SystemColors.Control;
			butInvalidate.Visible=true;
			butInvalidate.Location=butLock.Location;
		}
		else {
			butInvalidate.Visible=false;
			permissions=EnumPermType.ProcDelete;
			var dateForPerm=ProcedureGroup.DateEntryC;
			//because islocked overrides security:
			_attachedToCompletedProc=(ProcGroupItems.GetCountCompletedProcsForGroup(ProcedureGroup.ProcNum)!=0);
			if(_attachedToCompletedProc) {
				permissions=EnumPermType.ProcCompleteNote;
				dateForPerm=ProcedureGroup.ProcDate;
			}
			//This is mainly to make sure that the global security lock date is considered.
			if(!Security.IsAuthorized(permissions,dateForPerm)) {
				butSave.Enabled=false;
				butDelete.Enabled=false;
				textNotes.ReadOnly=true;
				textNotes.BackColor=SystemColors.Control;
				butAppend.Enabled=false;
				butChangeUser.Enabled=false;
				signatureBoxWrapper.Enabled=false;
				buttonUseAutoNote.Enabled=false;
				butLock.Enabled=false;
				butInvalidate.Enabled=false;
			}
		}
		if(ProcedureGroup.ProcStatus==ProcStat.D) {//an invalidated proc
			labelInvalid.Visible=true;
			butInvalidate.Visible=false;
			labelLocked.Visible=false;
			butAppend.Visible=false;
			butSave.Enabled=false;
			butDelete.Enabled=false;
		}
		FillProcedures();
		textNotes.Select();
		var keyData=GetSignatureKey();
		signatureBoxWrapper.FillSignature(ProcedureGroup.SigIsTopaz,keyData,ProcedureGroup.Signature);
		signatureBoxWrapper.BringToFront();
		if(!(Security.IsAuthorized(EnumPermType.GroupNoteEditSigned,true) || signatureBoxWrapper.SigIsBlank || ProcedureGroup.UserNum==Security.CurUser.UserNum)) {
			//User does not have permission and this note was signed by someone else.
			textNotes.ReadOnly=true;
			signatureBoxWrapper.Enabled=false;
			labelPermAlert.Visible=true;
			butAppend.Enabled=false;
			buttonUseAutoNote.Enabled=false;
			butChangeUser.Enabled=false;
			butDelete.Enabled=false;
		}
		else if(!Userods.CanUserSignNote()) {
			signatureBoxWrapper.Enabled=false;
			labelPermAlert.Visible=true;
			labelPermAlert.Text=Lans.g("Notes can only be signed by providers.");
		}
		PatFieldDefs.GetDeepCopy(true);
		textNotes.Select(textNotes.Text.Length,0);
		_isStartingUp=false;
		butEditAutoNote.Visible=HasAutoNotePrompt();
		//string retVal=GroupCur.Note+GroupCur.UserNum.ToString();
		//using MsgBoxCopyPaste msgb=new MsgBoxCopyPaste(retVal);
		//msgb.ShowDialog();
	}

	private void FillProcedures(){
		gridProc.BeginUpdate();
		gridProc.Columns.Clear();
		GridColumn col;
		DisplayFields.RefreshCache();//probably needs to be removed
		var listDisplayFields=DisplayFields.GetForCategory(DisplayFieldCategory.ProcedureGroupNote);
		for(var i=0;i<listDisplayFields.Count;i++) {
			if(listDisplayFields[i].Description=="") {
				col=new GridColumn(listDisplayFields[i].InternalName,listDisplayFields[i].ColumnWidth);
			}
			else {
				col=new GridColumn(listDisplayFields[i].Description,listDisplayFields[i].ColumnWidth);
			}
			if(listDisplayFields[i].InternalName=="Amount") {
				col.TextAlign=HorizontalAlignment.Right;
			}
			if(listDisplayFields[i].InternalName=="Proc Code") {
				col.TextAlign=HorizontalAlignment.Center;
			}
			gridProc.Columns.Add(col);
		}
		gridProc.ListGridRows.Clear();
		for(var i=0;i<ListProcedures.Count;i++) {
			var row=new GridRow();
			for(var f=0;f<listDisplayFields.Count;f++) {
				switch(listDisplayFields[f].InternalName) {
					case "Date":
						row.Cells.Add(ListProcedures[i].ProcDate.ToShortDateString());
						break;
					case "Th":
						row.Cells.Add(Tooth.Display(ListProcedures[i].ToothNum));
						break;
					case "Surf":
						row.Cells.Add(ListProcedures[i].Surf);
						break;
					case "Description":
						row.Cells.Add(ProcedureCodes.GetLaymanTerm(ListProcedures[i].CodeNum));
						break;
					case "Stat":
						if(ProcMultiVisits.IsProcInProcess(ListProcedures[i].ProcNum)) {
							row.Cells.Add(Lan.g("enumProcStat",ProcStatExt.InProcess));
						}
						else {
							row.Cells.Add(Lan.g("enumProcStat",ListProcedures[i].ProcStatus.ToString()));
						}
						break;
					case "Prov":
						row.Cells.Add(Providers.GetAbbr(ListProcedures[i].ProvNum));
						break;
					case "Amount":
						row.Cells.Add(ListProcedures[i].ProcFee.ToString("F"));
						break;
					case "Proc Code":
						row.Cells.Add(ProcedureCodes.GetStringProcCode(ListProcedures[i].CodeNum));
						break;
					case "Stat 2":
						row.Cells.Add("");//deprecated
						break;
					case "On Call":
						row.Cells.Add("");
						break;
					case "Effective Comm":
						row.Cells.Add("");
						break;
					case "Repair":
						row.Cells.Add("");
						break;
					case "DPCpost":
						row.Cells.Add("");//deprecated
						break;
				}
			}
			gridProc.ListGridRows.Add(row);
		}
		gridProc.EndUpdate();
	}

	private void RefreshGrids(){
		FillProcedures();
	}

	private void buttonUseAutoNote_Click(object sender,EventArgs e) {
		var frmAutoNoteCompose=new FrmAutoNoteCompose();
		frmAutoNoteCompose.ShowDialog();
		if(frmAutoNoteCompose.IsDialogOK) {
			textNotes.AppendText(frmAutoNoteCompose.StrCompletedNote);
			butEditAutoNote.Visible=HasAutoNotePrompt();
		}
	}

	private void ButEditAutoNote_Click(object sender,EventArgs e) {
		if(HasAutoNotePrompt()) {
			var frmAutoNoteCompose=new FrmAutoNoteCompose();
			frmAutoNoteCompose.StrMainTextNote=textNotes.Text;
			frmAutoNoteCompose.ShowDialog();
			if(frmAutoNoteCompose.IsDialogOK) {
				textNotes.Text=frmAutoNoteCompose.StrCompletedNote;
				butEditAutoNote.Visible=HasAutoNotePrompt();
			}
			return;
		}
		ODMessageBox.Show(Lan.g(this,"No Auto Note available to edit."));
	}

	private bool HasAutoNotePrompt() {
		return Regex.IsMatch(textNotes.Text,_autoNotePromptRegex);
	}

	private void butExamSheets_Click(object sender,EventArgs e) {
		using var formExamSheets=new FormExamSheets();
		formExamSheets.PatNum=ProcedureGroup.PatNum;
		formExamSheets.ShowDialog();
		//TODO: Print a note about Exam Sheet added.
	}

	private void textNotes_TextChanged(object sender,EventArgs e) {
		if(!_isStartingUp//so this happens only if user changes the note
		   && !_hasSigChanged)//and the original signature is still showing.
		{
			//SigChanged=true;//happens automatically through the event.
			signatureBoxWrapper.ClearSignature();
		}
	}

	private string GetSignatureKey(){
		var keyData=ProcedureGroup.ProcDate.ToShortDateString();
		keyData+=ProcedureGroup.DateEntryC.ToShortDateString();
		keyData+=ProcedureGroup.UserNum.ToString();//Security.CurUser.UserName;
		keyData+=ProcedureGroup.Note;
		ListProcGroupItems=ProcGroupItems.GetForGroup(ProcedureGroup.ProcNum);//Orders the list to ensure same key in all cases.
		for(var i=0;i<ListProcGroupItems.Count;i++){
			keyData+=ListProcGroupItems[i].ProcGroupItemNum.ToString();
		}
		keyData=keyData.Replace("\r\n","\n");//We need all newlines to be the same, a mix of \r\n and \n can invalidate the procedure signature.
		return keyData;
	}

	private void butChangeUser_Click(object sender,EventArgs e) {
		using var formLogOn=new FormLogOn(isSimpleSwitch:true);
		formLogOn.ShowDialog();
		if(formLogOn.DialogResult!=DialogResult.OK) { 
			return;	
		}
		_userod=formLogOn.UserodSimpleSwitch; //assign temp user
		var canUserSignNote=Userods.CanUserSignNote(_userod);//only show if user can sign
		signatureBoxWrapper.Enabled=canUserSignNote;
		if(!labelPermAlert.Visible && !canUserSignNote) {
			labelPermAlert.Text=Lans.g("Notes can only be signed by providers.");
			labelPermAlert.Visible=true;
		}
		signatureBoxWrapper.ClearSignature(); //clear sig
		signatureBoxWrapper.UserSig=_userod;
		textUser.Text=_userod.UserName; //update user textbox.
		_hasSigChanged=true;
		_hasUserChanged=true;
	}

	private void SaveSignature(){
		if(_hasSigChanged){
			var keyData=GetSignatureKey();
			ProcedureGroup.Signature=signatureBoxWrapper.GetSignature(keyData);
			ProcedureGroup.SigIsTopaz=signatureBoxWrapper.GetSigIsTopaz();
		}
	}

	private void signatureBoxWrapper_SignatureChanged(object sender,EventArgs e) {
		ProcedureGroup.UserNum=_userod.UserNum;
		if(!textUser.Text.IsNullOrEmpty() && textUser.Text!=_userod.UserName) {
			_hasUserChanged=true;
		}
		textUser.Text=_userod.UserName;
		_hasSigChanged=true;
	}

	///<summary>This button is only visible if 1. Pref ProcLockingIsAllowed is true, 2. Proc isn't already locked, 3. Proc status is C.</summary>
	private void butLock_Click(object sender,EventArgs e) {
		if(!EntriesAreValid()) {
			return;
		}
		ProcedureGroup.IsLocked=true;
		SaveAndClose();//saves all the other various changes that the user made
	}

	///<summary>This button is only visible when proc IsLocked.</summary>
	private void butInvalidate_Click(object sender,EventArgs e) {
		//What this will really do is "delete" the procedure.
		if(!Security.IsAuthorized(EnumPermType.ProcDelete,ProcedureGroup.DateEntryC)) {
			return;
		}
		if(!Security.IsAuthorized(EnumPermType.GroupNoteEditSigned)) {
			return;
		}
		try {
			Procedures.Delete(ProcedureGroup.ProcNum);//also deletes any claimprocs (other than ins payments of course).
		}
		catch(Exception ex) {
			ODMessageBox.Show(ex.Message);
			return;
		}
		//Log entry does not show procstatus because group notes don't technically have a status, always EC.
		SecurityLogs.MakeLogEntry(EnumPermType.ProcDelete,_patient.PatNum,Lan.g(this,"Invalidated: ")+
		                                                                  ProcedureCodes.GetStringProcCode(ProcedureGroup.CodeNum)+", "+ProcedureGroup.ProcDate.ToShortDateString());
		DialogResult=DialogResult.OK;
	}

	///<summary>This button is only visible when proc IsLocked.</summary>
	private void butAppend_Click(object sender,EventArgs e) {
		using var formProcNoteAppend=new FormProcNoteAppend();
		formProcNoteAppend.ProcedureCur=ProcedureGroup;
		formProcNoteAppend.ShowDialog();
		if(formProcNoteAppend.DialogResult!=DialogResult.OK) {
			return;
		}
		DialogResult=DialogResult.OK;//exit out of this window.  Change already saved, and OK button is disabled in this window, anyway.
	}

	private bool EntriesAreValid() {
		if(!textProcDate.IsValid()) {
			MsgBox.Show(this,"Please fix data entry errors first.");
			return false;
		}
		var hasAutoNotePrompt=Regex.IsMatch(textNotes.Text,_autoNotePromptRegex);
		//If ProcNoteSigsBlockedAutoNoteIncomplete is true, do not allow the user to save a changed signature if there are still autonote prompts.
		if(_hasSigChanged && !signatureBoxWrapper.SigIsBlank && hasAutoNotePrompt && PrefC.GetBool(PrefName.ProcNoteSigsBlockedAutoNoteIncomplete)) {
			ODMessageBox.Show(Lan.g(this,"Remaining auto note prompts must be completed to sign this note. Use Edit Auto Note to resume."));
			return false;
		}
		if(!signatureBoxWrapper.IsValid) {
			MsgBox.Show(this,"Your signature is invalid. Please sign and click OK again.");
			return false;
		}
		return true;
	}

	private void SaveAndClose() {
		ProcedureGroup.Note=textNotes.Text;
		try {
			SaveSignature();
		}
		catch(Exception ex){
			ODMessageBox.Show(Lan.g(this,"Error saving signature.")+"\r\n"+ex.Message);
		}
		Procedures.Update(ProcedureGroup,_procedureGroupOld);
		DialogResult=DialogResult.OK;
	}

	private void butDelete_Click(object sender, System.EventArgs e) {
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Delete this group note?")){
			return;
		}
		try { 
			Procedures.Delete(ProcedureGroup.ProcNum);
		}
		catch(Exception ex){
			ODMessageBox.Show(ex.Message+"\r\n"+Lan.g(this,"Please call support."));//GroupNotes should never fail deletion.
			return;
		}
		for(var i=0;i<ListProcGroupItems.Count;i++){
			ProcGroupItems.Delete(ListProcGroupItems[i].ProcGroupItemNum);
		}
		//This log entry is similar to the log entry made when right-clicking in the Chart and using the delete option,
		//except there is an extra : in the description for this log entry, so we programmers can know for sure where the entry was made from.
		if(_attachedToCompletedProc) {
			SecurityLogs.MakeLogEntry(EnumPermType.ProcCompleteStatusEdit,_patient.PatNum,
				":"+ProcedureCodes.GetStringProcCode(ProcedureGroup.CodeNum)+" ("+ProcedureGroup.ProcStatus+"), "+ProcedureGroup.ProcDate.ToShortDateString());
		}
		else {
			SecurityLogs.MakeLogEntry(EnumPermType.ProcDelete,_patient.PatNum,
				":"+ProcedureCodes.GetStringProcCode(ProcedureGroup.CodeNum)+" ("+ProcedureGroup.ProcStatus+"), "+ProcedureGroup.ProcDate.ToShortDateString());
		}
		DialogResult=DialogResult.OK;
	}		

	private void butSave_Click(object sender,System.EventArgs e) {
		if(!EntriesAreValid()){
			return;
		}
		if(_hasUserChanged && !_procedureGroupOld.Signature.IsNullOrEmpty()) {
			//Ask the user to re-sign if the user has changed, there was a signature when the window loaded, and the signature box is currently blank.
			if(signatureBoxWrapper.SigIsBlank) {
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,
					   "The signature box has not been re-signed.  Continuing will remove the previous signature from this procedure.  Exit anyway?"))
				{
					return;
				}
			}
			//Check if user is subcribed to the alertType.SignatureCleared and create an alert.
			if(AlertSubs.GetAllAlertTypesForUser(_procedureGroupOld.UserNum).Contains(AlertType.SignatureCleared)) {
				var procCode=ProcedureCodes.GetStringProcCode(_procedureGroupOld.CodeNum);
				var provider=Providers.GetAbbr(_procedureGroupOld.ProvNum);
				var alertDescription="Group Note Changed for PatNum: "+_procedureGroupOld.PatNum
				                                                      +" Provider: "+provider
				                                                      +" Date: "+_procedureGroupOld.ProcDate.ToShortDateString()
				                                                      +" Procedure Code: "+procCode;
				AlertItems.Insert(new AlertItem {
					//Allow to alert to be deleted or marked as read.
					Actions=ActionType.MarkAsRead | ActionType.Delete,
					Description=Lans.g("Procedures",alertDescription),
					Severity=SeverityType.Low,
					Type=AlertType.SignatureCleared,
					UserNum=_procedureGroupOld.UserNum,
					ClinicNum=_procedureGroupOld.ClinicNum,
				});
			}
		}
		SaveAndClose();
	}

	private void FormProcGroup_FormClosing(object sender,FormClosingEventArgs e) {
		signatureBoxWrapper?.SetTabletState(0);
		if(DialogResult==DialogResult.OK) {
			return;
		}
		if(_procedureGroupOld.Note.Replace("\r","").Trim()!=textNotes.Text.Replace("\r","").Trim()) {
			if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Note has been changed.  Unsaved changes will be lost.  Continue?")) {
				e.Cancel=true;//Prevent the form from closing.
				return;
			}
		}
		if(ProcedureGroup.IsNew) {
			Procedures.Delete(ProcedureGroup.ProcNum);
			for(var i=0;i<ListProcGroupItems.Count;i++) {
				ProcGroupItems.Delete(ListProcGroupItems[i].ProcGroupItemNum);
			}
		}
		DialogResult=DialogResult.Cancel;
	}
}