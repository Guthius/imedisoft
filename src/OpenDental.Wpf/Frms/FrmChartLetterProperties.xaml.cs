using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;
using CodeBase;
using Word=Microsoft.Office.Interop.Word;
using WpfControls;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmChartLetterProperties : FrmODBase {
		public Document DocumentCur;
		public Patient PatientCur;
		///<summary>If the Word doc is stored in the db, it would be a waste of time to update that field if just changing a few fields in this window, so a synch is used.</summary>
		private Document _documentOld;
		public bool IsAuditMode;

		
		public FrmChartLetterProperties() {
			InitializeComponent();
			Load+=Frm_Load;
			Shown+=Frm_Shown;
		}

		private void Frm_Shown(object sender,EventArgs e) {
			if(textFileHash.Text=="N/A") {
				MsgBox.Show("Document must be closed before opening this window in order to calculate hash.");
			}
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			_documentOld=DocumentCur.Copy();
			//if(true) {
			string patFolderName=TryGetPatientFolder();
			if(patFolderName.IsNullOrEmpty()) {
				IsDialogCancel=true;
				this.Close();
				return;
			}
			textFileName.Text=ODFileUtils.CombinePaths(patFolderName,DocumentCur.FileName);
			if(File.Exists(textFileName.Text)) {
				byte[] byteArrayFile;
				try{
					byteArrayFile=File.ReadAllBytes(textFileName.Text);
					byte[] byteArrayHash=ODCrypt.MD5.Hash(byteArrayFile);
					textFileHash.Text=ChartLetterL.ToHexString(byteArrayHash);
				}
				catch{
					//msg will show on FormShown
					textFileHash.Text="N/A";
					labelMatch.Visible=false;
				}
			}
			//}
			//We don't support these:
			//else if(false) {
			//	string patFolderName=TryGetPatientFolder();
			//	if(patFolderName.IsNullOrEmpty()) {
			//		IsDialogCancel=true;
			//		this.Close();
			//		return;
			//	}
			//	textFileName.Text=ODFileUtils.CombinePaths(patFolderName,DocumentCur.FileName,'/');
			//	butOpenFolder.Visible=false;
			//}
			//else {//storing in db
			//	labelFileName.Visible=false;
			//	textFileName.Visible=false;
			//	butOpenFolder.Visible=false;
			//	//textSize.Text=DocumentCur.RawBase64.Length.ToString("n0");
			//}
			textStoredHash.Text=DocumentCur.ChartLetterHash;
			if(textFileHash.Text==textStoredHash.Text){
				labelMatch.Text="MATCH. No external changes to file detected.";
				labelMatch.ColorText=Colors.DarkGreen;
			}
			else{
				labelMatch.Text="NO MATCH. File was changed externally.";
				labelMatch.ColorText=Colors.Firebrick;
			}
			textDate.Text=DocumentCur.DateCreated.ToShortDateString();
			textTime.Text=DocumentCur.DateCreated.ToLongTimeString();
			textDescription.Text=DocumentCur.Description;
			textUser.Text=Userods.GetUser(DocumentCur.UserNum).UserName;
			comboImageCats.Items.AddDefs(Defs.GetDefsForCategory(DefCat.ImageCats,isShort:true));
			comboImageCats.SetSelected(0);
			float scale=(float)VisualTreeHelper.GetDpi(this).DpiScaleX;
			signatureBoxWrapper.SetScaleAndZoom(scale,GetZoom());
			string keyData=textFileHash.Text;
			signatureBoxWrapper.FillSignature(DocumentCur.SigIsTopaz,keyData,DocumentCur.Signature);
			if(IsAuditMode){
				labelEditDoc.Visible=false;
				textDate.textBox.IsReadOnly=true;//hack
				textDate.ColorBack=ColorOD.Gray_Wpf(240);
				textTime.ReadOnly=true;
				textTime.ColorBack=ColorOD.Gray_Wpf(240);
				textDescription.ReadOnly=true;
				textDescription.ColorBack=ColorOD.Gray_Wpf(240);
				butEdit.Text="View Document";
				signatureBoxWrapper.IsEnabled=false;
				butDelete.IsEnabled=false;
				butSave.IsEnabled=false;
			}
		}

		///<summary>Returns patient folder name when the ImageStore was able to find or create a patient folder for the selected patient.  Sets patFolder to the corresponding folder name. Otherwise, displays an error message to the user (with additional details regarding what went wrong) and returns empty string.</summary>
		private string TryGetPatientFolder() {
			string patFolderName;
			try {
				patFolderName=ImageStore.GetPatientFolder(PatientCur,ImageStore.GetPreferredAtoZpath());
			}
			catch(Exception ex) {
				FriendlyException.Show(ex.Message,ex);
				patFolderName="";
			}
			return patFolderName;
		}

		private void butOpenFolder_Click(object sender,EventArgs e) {
			//if(true) {
			//we don't support any other DataStorage
			if(false) {
				MsgBox.Show("Not supported");
			}
			else if(false) {
				MsgBox.Show("Not supported");
			}
			else {
				System.Diagnostics.Process.Start("Explorer",Path.GetDirectoryName(textFileName.Text));
			}
		}

		private void butEdit_Click(object sender,EventArgs e) {
			if(!File.Exists(textFileName.Text)){
				MsgBox.Show("File does not exist: "+textFileName.Text);
				return;
			}
			Cursor=Cursors.Wait;
			if(IsAuditMode){
				Word._Document word_DocumentReadOnly=null;
				try{
					ChartLetterL.InitializeWordApplication();
					word_DocumentReadOnly=ChartLetterL.Word_Application.Documents.Open(textFileName.Text,ReadOnly:true,Visible:true);
					ChartLetterL.Word_Application.Visible=true;
					ChartLetterL.Word_Application.Activate();//brings to front
					word_DocumentReadOnly=null;//release our handle
				}
				catch(Exception ex) {
					Cursor=Cursors.Arrow;
					FriendlyException.Show(Lang.g(this,"Error. Is MS Word installed?"),ex);
					return;
				}
				finally{
					if(word_DocumentReadOnly != null) {
						Marshal.ReleaseComObject(word_DocumentReadOnly);
						word_DocumentReadOnly=null; 
					}
					Cursor=Cursors.Arrow;
				}
				return;
			}
			//DocumentCur will become the archive
			Document documentOld=DocumentCur.Copy();
			//everything is the same except:
			DocumentCur.ChartLetterStatus=EnumDocChartLetterStatus.AuditTrail;
			//we don't check the hash. We leave it as-is, whether matching or not.
			Documents.Update(DocumentCur,documentOld);
			//Then we make a new db row and new Word doc with datetime.Now
			//This puts PK and filenames in proper sequential order.
			Document documentNew=DocumentCur.Copy();
			documentNew.ChartLetterStatus=EnumDocChartLetterStatus.Active;
			documentNew.DateCreated=DateTime.Now;
			documentNew.UserNum=Security.CurUser.UserNum;
			documentNew.Description=textDescription.Text;//they might have changed it
			//documentNew.FileName="";//we'll set this after we create the new file
			Documents.Insert(documentNew);//gets new PK
			string fileNameNew=ODFileUtils.CleanFileName(documentNew.Description+documentNew.DocNum)+".docx";
			string fullPathNew=ODFileUtils.CombinePaths(TryGetPatientFolder(),fileNameNew);
			File.Copy(textFileName.Text,fullPathNew);
			documentOld=documentNew.Copy();
			documentNew.FileName=fileNameNew;
			Documents.Update(documentNew,documentOld);
			//Finally, launch Word
			Word._Document word_Document=null;
			try{
				ChartLetterL.InitializeWordApplication();
				word_Document=ChartLetterL.Word_Application.Documents.Open(fullPathNew);
				ChartLetterL.Word_Application.Visible=true;
				ChartLetterL.Word_Application.Activate();//brings to front
				word_Document=null;//release our handle
				ChartLetterL.AddTracker(documentNew,fullPathNew);//,alreadyCreatedOrArchived:false,_formFrame);
				//When the Word document eventually closes, event handler will notice and update hash
			}
			catch(Exception ex) {
				Cursor=Cursors.Arrow;
				FriendlyException.Show(Lang.g(this,"Error. Is MS Word installed?"),ex);
				//But archive was already created, and date of current doc was already changed. So we need to get out of this window.
				IsDialogOK=true;
				return;
			}
			finally{
				if(word_Document != null) {
					Marshal.ReleaseComObject(word_Document);
					word_Document=null; 
				}
			}
			Cursor=Cursors.Arrow;
			IsDialogOK=true;
		}

		private void butPdf_Click(object sender,EventArgs e) {
			Cursor=Cursors.Wait;
			string fullPathTemp=PrefC.GetRandomTempFile(".pdf");
			Word.Document word_Document=null;
			try{
				ChartLetterL.InitializeWordApplication();
				word_Document=ChartLetterL.Word_Application.Documents.Open(textFileName.Text);
				word_Document.SaveAs2(fullPathTemp,Word.WdSaveFormat.wdFormatPDF);
			}
			catch(Exception ex){
				Cursor=Cursors.Arrow;
				FriendlyException.Show(Lang.g(this,"Error. Is MS Word installed?"),ex);
			}
			finally{
				if(word_Document!=null){
					word_Document.Close(SaveChanges:false);
					Marshal.ReleaseComObject(word_Document);
					word_Document = null;
				}
			}
			string rawBase64="";
			if(false) {//even though we don't support this
				rawBase64=Convert.ToBase64String(File.ReadAllBytes(fullPathTemp));
			}
			Document document=new Document();
			document.DocNum=Documents.Insert(document);
			document.ImgType=ImageType.Document;
			document.DateCreated=DateTime.Now;
			document.PatNum=PatientCur.PatNum;
			document.DocCategory=comboImageCats.GetSelectedDefNum();
			document.UserNum=Security.CurUser.UserNum;
			document.Description=textDescription.Text;
			document.RawBase64=rawBase64;//blank if using AtoZfolder
			document.FileName=ODFileUtils.CleanFileName(document.Description+document.DocNum)+".pdf";
			string fileDestPath=ImageStore.GetFilePath(document,ImageStore.GetPatientFolder(PatientCur,ImageStore.GetPreferredAtoZpath()));
			File.Copy(fullPathTemp,fileDestPath);
			Documents.Update(document);
			Cursor=Cursors.Arrow;
			MsgBox.Show("Saved");
		}

		private void butDelete_Click(object sender,EventArgs e) {
			Document documentOld=DocumentCur.Copy();
			DocumentCur.ChartLetterStatus=EnumDocChartLetterStatus.Deleted;
			//we don't check the hash. We leave it as-is, whether matching or not.
			Documents.Update(DocumentCur,documentOld);
			IsDialogOK=true;
		}

		private void butSave_Click(object sender, EventArgs e) {
			if(!textDate.IsValid()) {
				MessageBox.Show(Lang.g(this,"Please fix data entry errors first."));
				return;
			}
			if(textDate.Text=="") {
				MsgBox.Show(this,"Please enter a date.");
				return;
			}
			if(textTime.Text=="") {
				MsgBox.Show(this,"Please enter a time.");
				return;
			}
			DateTime date;
			if(!DateTime.TryParse(textTime.Text,out date)) {
				MsgBox.Show(this,"Please enter a valid time.");
				return;
			}
			//We had a security bug where users could change the date to a more recent date, and then subsequently delete.
			//The code below is for that specific scenario.
			DateTime dateTimeEntered=PIn.DateTime(textDate.Text+" "+textTime.Text);
			if(dateTimeEntered>DocumentCur.DateCreated) {
				//user is trying to change the date to some date after the previously linked date
				//is the new doc date allowed?
				List<EnumPermType> listMissingPermissions=SecurityL.GetMissingPermissionsNeededToDeleteImage(DocumentCur);
				if(listMissingPermissions.Count > 0) {
					//suppress the default security message above (it's too confusing for this case) and generate our own here
					string permissionString="";
					for(int i=0;i < listMissingPermissions.Count;i++) {
						permissionString+=$"{GroupPermissions.GetDesc(listMissingPermissions[i])}";
						if(i != listMissingPermissions.Count-1) {
							permissionString+=" and ";
						}
					}
					MessageBox.Show(Lang.g(this,"Not allowed to future date this image from")+": "
						+"\r\n"+DocumentCur.DateCreated.ToString()+" to "+dateTimeEntered.ToString()
						+"\r\n\r\n"+Lang.g(this,"A user with the SecurityAdmin permission must grant you access for")
						+":\r\n"+permissionString);
					return;
				}
			}
			DocumentCur.DateCreated=dateTimeEntered;
			DocumentCur.Description=textDescription.Text;
			if(signatureBoxWrapper.GetSigChanged()){
				string keyData=textFileHash.Text;
				DocumentCur.Signature=signatureBoxWrapper.GetSignature(keyData);
				DocumentCur.SigIsTopaz=signatureBoxWrapper.GetSigIsTopaz();
			}
			if(Documents.Update(DocumentCur,_documentOld)) {
				ImageStore.LogDocument(Lang.g(this,"Document Edited")+": ",EnumPermType.ImageEdit,DocumentCur,_documentOld.DateTStamp);
			}
			IsDialogOK=true;
		}
	}
}