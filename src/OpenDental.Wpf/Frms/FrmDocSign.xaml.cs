using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CodeBase;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {

	public partial class FrmDocSign:FrmODBase {
		//<summary></summary>
		//public bool IsNew;
		//private Patient PatCur;
		private Document _document;
		private Document _documentOld;
		///<summary>This keeps the noteChanged event from erasing the signature when first loading.</summary>
		private bool _isStartingUp;
		private bool _isSignatureChanged;
		private Patient _patient;
		private string _patFolderName;
		///<summary>In screen coords. LL because it's 'anchored' LL and it will grow up and right.</summary>
		public System.Drawing.Point PointLLStart;
		
		
		public FrmDocSign(Document document,Patient patient) {
			InitializeComponent();
			_document=document;
			_documentOld=document.Copy();
			_patient=patient;
			Load+=FrmDocSign_Load;
			textNote.TextChanged+=textNote_TextChanged;
			signatureBoxWrapper.SignatureChanged+=signatureBoxWrapper_SignatureChanged;
			PreviewKeyDown+=FrmDocSign_PreviewKeyDown;
		}

		
		public void FrmDocSign_Load(object sender, EventArgs e){
			_formFrame.Location=new System.Drawing.Point(PointLLStart.X,PointLLStart.Y-_formFrame.Height+4);
			Lang.F(this);
			_patFolderName=ImageStore.GetPatientFolder(_patient,ImageStore.GetPreferredAtoZpath());
			_isStartingUp=true;
			textNote.Text=_document.Note;
			signatureBoxWrapper.SignatureMode=UI.SignatureBoxWrapper.SigMode.Document;
			string keyData=ImageStore.GetHashString(_document,_patFolderName);
			signatureBoxWrapper.FillSignature(_document.SigIsTopaz,keyData,_document.Signature);
			//If sig is not showing, then try using hash without raw file data included.
			if(signatureBoxWrapper.GetNumberOfTabletPoints(_document.SigIsTopaz)==0) {
				keyData=ImageStore.GetHashString(_document,_patFolderName,includeFileInHash:false);
				signatureBoxWrapper.FillSignature(_document.SigIsTopaz,keyData,_document.Signature);
			}
			_isStartingUp=false;
		}

		private void textNote_TextChanged(object sender,EventArgs e) {
			if(!_isStartingUp//so this happens only if user changes the note
				&& !_isSignatureChanged)//and the original signature is still showing.
			{
				signatureBoxWrapper.ClearSignature();
				//this will call OnSignatureChanged to set UserNum, textUser, and SigChanged
			}
		}

		private void SaveSignature() {
			if(_isSignatureChanged) {
				string keyData=ImageStore.GetHashString(_document,_patFolderName);
				_document.Signature=signatureBoxWrapper.GetSignature(keyData);
				_document.SigIsTopaz=signatureBoxWrapper.GetSigIsTopaz();
			}
			LogSignatureChangesIfNeeded();
		}

		///<summary>Creates a security log for any changes to the note and/or signature of the document if one is needed.</summary>
		private void LogSignatureChangesIfNeeded() {
			bool hadSignatureDocumentOld=!_documentOld.Signature.IsNullOrEmpty();
			bool hasSignatureDocument=!_document.Signature.IsNullOrEmpty();
			StringBuilder stringBuilderLogText=new StringBuilder();
			EnumPermType permissionForLog=EnumPermType.None;
			if(!hadSignatureDocumentOld && hasSignatureDocument) {
				permissionForLog = EnumPermType.ImageSignatureCreate;
				stringBuilderLogText.AppendLine(Lans.g(this,"Document signed."));
			}
			else if(hadSignatureDocumentOld && !hasSignatureDocument) {
				permissionForLog = EnumPermType.SignedImageEdit;
				stringBuilderLogText.AppendLine(Lans.g(this,"Signature removed."));
			}
			else if(hadSignatureDocumentOld && hasSignatureDocument) {
				permissionForLog = EnumPermType.SignedImageEdit;
				if(_documentOld.Signature!=_document.Signature) {
					stringBuilderLogText.AppendLine(Lans.g(this,"Existing signature changed."));
				}
			}
			else {
				//document didn't and still doesn't have signature. We don't need log text for signature changes.
				//We log under the ImageSignatureCreate permission if a note is added but signature hasn't changed.
				permissionForLog = EnumPermType.ImageSignatureCreate;
			}
			bool hadNoteDocumentOld=!_documentOld.Note.IsNullOrEmpty();
			bool hasNoteDocument=!_document.Note.IsNullOrEmpty();
			if(!hadNoteDocumentOld && hasNoteDocument) {
				stringBuilderLogText.AppendLine(Lans.g(this,"Note added."));
			}
			else if(hadNoteDocumentOld && !hasNoteDocument) {
				stringBuilderLogText
					.AppendLine(Lans.g(this,"Note removed:"))
					.AppendLine(_documentOld.Note);
			}
			else if(hadNoteDocumentOld && hasNoteDocument && _documentOld.Note!=_document.Note) {
				stringBuilderLogText
					.AppendLine(Lans.g(this,"Previous note:"))
					.AppendLine(_documentOld.Note);
			}
			if(stringBuilderLogText.Length > 0) {
				SecurityLogs.MakeLogEntry(permissionForLog,_patient.PatNum,stringBuilderLogText.ToString().TrimEnd(),_document.DocNum,_document.DateTStamp);
			}
		}

		private void signatureBoxWrapper_SignatureChanged(object sender,EventArgs e) {
			_isSignatureChanged=true;
		}

		private void FrmDocSign_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		///<summary>Shows a message box and returns false if the user doesn't have the correct permission to sign the document. User must have ImageSignatureCreate to sign unsigned documents, and they must have SignedImageEdit to edit the signature of signed documents.</summary>
		private bool IsAuthorizedToSignImage() {
			//Document doesn't have signature and user is allowed to create signatures for documents that don't have them.
			if(_document.Signature.IsNullOrEmpty() && Security.IsAuthorized(EnumPermType.ImageSignatureCreate)) {
				return true;
			}
			//Document has signature and user is allowed to edit existing signatures.
			if(!_document.Signature.IsNullOrEmpty() && Security.IsAuthorized(EnumPermType.SignedImageEdit)) {
				return true;
			}
			return false;
		}

		private void butSave_Click(object sender, System.EventArgs e){
			if(!IsAuthorizedToSignImage()) {
				return;
			}
			_document.Note=textNote.Text;
			SaveSignature();
			Documents.Update(_document);
			IsDialogOK=true;
		}

	}
}