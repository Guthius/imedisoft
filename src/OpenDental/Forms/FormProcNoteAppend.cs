using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;

namespace OpenDental {
	public partial class FormProcNoteAppend:FormODBase {
		public Procedure ProcedureCur;

		public FormProcNoteAppend() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormProcNoteAppend_Load(object sender,EventArgs e) {
			signatureBoxWrapper.SetAllowDigitalSig(true);
			textUser.Text=Security.CurUser.UserName;
			textNotes.Text=ProcedureCur.Note;
			if(!Userods.CanUserSignNote()) {
				signatureBoxWrapper.Enabled=false;
				labelPermAlert.Visible=true;
			}
			//there is no signature to display when this form is opened.
			//signatureBoxWrapper.FillSignature(false,"","");
			signatureBoxWrapper.BringToFront();
			//signatureBoxWrapper.ClearSignature();
		}

		private void buttonUseAutoNote_Click(object sender,EventArgs e) {
			FrmAutoNoteCompose frmAutoNoteCompose=new FrmAutoNoteCompose();
			frmAutoNoteCompose.ShowDialog();
			if(frmAutoNoteCompose.IsDialogOK) {
				textAppended.AppendText(frmAutoNoteCompose.StrCompletedNote);
			}
		}

		private string GetSignatureKey() {
			//ProcCur.Note was already assembled as it will appear in proc edit window.  We want to key on that.
			//Procs and proc groups are keyed differently
			string keyData;
			if(ProcedureCodes.GetStringProcCode(ProcedureCur.CodeNum)==ProcedureCodes.GroupProcCode) {
				keyData=ProcedureCur.ProcDate.ToShortDateString();
				keyData+=ProcedureCur.DateEntryC.ToShortDateString();
				keyData+=ProcedureCur.UserNum.ToString();//Security.CurUser.UserName;
				keyData+=ProcedureCur.Note;
				List<ProcGroupItem> listProcGroupItems=ProcGroupItems.GetForGroup(ProcedureCur.ProcNum);//Orders the list to ensure same key in all cases.
				for(int i=0;i<listProcGroupItems.Count;i++) {
					keyData+=listProcGroupItems[i].ProcGroupItemNum.ToString();
				}
			}
			else {//regular proc
				keyData=ProcedureCur.Note+ProcedureCur.UserNum.ToString();
			}
			//using MsgBoxCopyPaste msgb=new MsgBoxCopyPaste(keyData);
			//msgb.ShowDialog();
			keyData=keyData.Replace("\r\n","\n");//We need all newlines to be the same, a mix of \r\n and \n can invalidate the procedure signature.
			return keyData;
		}

		private void SaveSignature() {
			//This is not a good pattern to copy, because it's simpler than usual.  Try FormCommItem.
			string keyData=GetSignatureKey();
			ProcedureCur.Signature=signatureBoxWrapper.GetSignature(keyData);
			ProcedureCur.SigIsTopaz=signatureBoxWrapper.GetSigIsTopaz();
		}

		private void butSave_Click(object sender,EventArgs e) {
			Procedure procedure=ProcedureCur.Copy();
			ProcedureCur.UserNum=Security.CurUser.UserNum;
			ProcedureCur.Note=textNotes.Text+"\r\n"
				+DateTime.Now.ToShortDateString()+" "+DateTime.Now.ToShortTimeString()+" "+Security.CurUser.UserName+":  "
				+textAppended.Text;
			if((procedure.UserNum!=ProcedureCur.UserNum) && !procedure.Signature.IsNullOrEmpty()) {
				//Check if user is subcribed to the alertType.SignatureCleared and create an alert.
				if(AlertSubs.GetAllAlertTypesForUser(procedure.UserNum).Contains(AlertType.SignatureCleared)) {
					string procCode=ProcedureCodes.GetStringProcCode(procedure.CodeNum);
					string procedureType=Lans.g(this,"Procedure");
					if(ProcedureCodes.GetStringProcCode(ProcedureCur.CodeNum)==ProcedureCodes.GroupProcCode) {
						procedureType=Lans.g(this,"Group");
					}
					string alertDescription=Lans.g(this,"Locked")+" "+procedureType+" "+Lans.g(this,"Note Changed for PatNum:")+" "+procedure.PatNum+" "
					+Lans.g(this,"Date:")+" "+procedure.ProcDate.ToShortDateString()+" "
					+Lans.g(this,"Procedure Code:")+" "+procCode;
					AlertItems.Insert(new AlertItem {
						//Allow to alert to be deleted or marked as read.
						Actions=ActionType.MarkAsRead | ActionType.Delete,
						Description=alertDescription,
						Severity=SeverityType.Low,
						Type=AlertType.SignatureCleared,
						UserNum=procedure.UserNum,
						ClinicNum=procedure.ClinicNum,
					});
				}
			}
			try {
				SaveSignature();
			}
			catch(Exception ex) {
				MessageBox.Show(Lan.g(this,"Error saving signature.")+"\r\n"+ex.Message);
				//and continue with the rest of this method
			}
			Procedures.Update(ProcedureCur,procedure);
			DialogResult=DialogResult.OK;
		}

		private void FormProcNoteAppend_FormClosing(object sender,FormClosingEventArgs e) {
			signatureBoxWrapper?.SetTabletState(0);
		}

	}
}