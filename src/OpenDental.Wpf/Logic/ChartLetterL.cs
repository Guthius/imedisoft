using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Word=Microsoft.Office.Interop.Word;
using OpenDentBusiness;
using CodeBase;

/*
Some comments about the Microsoft.Office.Interop.Word reference:
2024-12-05-I replaced the very old one with a newer one. 
The old one had a different name: Interop.Word.
Different sources give different versions of Word that the new interop covers.
It's dated about 2021, and I think it covers 2016 through 2024, but probably also older versions of Word.
It has some advantages:
	Less likely to crash.
	Optional parameters instead of ref _objectMissing on all the unused parameters
We weren't using Interop.Microsoft.Office.Core, so I removed it.
I added the new dll to the RequiredDlls
I set Embed Interop Types to false, but it wouldn't work. So then I set it to true. 
This means that we shouldn't need the dll, but I left it in place anyway.
*/

namespace OpenDental {
	///<summary>A ChartLetter is just a special kind of document. Word doc is stored in AtoZ folder, but only visible in Chart module instead of Imaging module. There is a lot of interop with Word, and to keep things simple, we will only support a single Word doc open at one time for tracking purposes. We need to know when user closes the doc so we can update the hash for security.</summary>
	public static class ChartLetterL {
		///<summary>Best practice is to maintain single instance and reuse it. This object is thread-affine and should only be accessed from the thread it was created on (OD UI thread). Otherwise needs to be marshalled.</summary>
		public static Word.Application Word_Application;
		///<summary>The purpose of this list is to track documents that we are editing. They must have been opened from within OD. Closing them updates the database.</summary>
		private static ConcurrentBag<DocTracker> _concurrentBagDocTrackers=new ConcurrentBag<DocTracker>();
		//private static readonly object _syncLock = new object();

		public static void InitializeWordApplication() {
			//if(Word_Application==null) {//this does not reliably tell me whether Word is available, so:
			try{
				Word_Application.Visible=false;
			}
			catch{
				Word_Application=new Word.Application();
				Word_Application.Visible=false;
				//if that fails, it will be caught one level up
				//Word_Application.DocumentBeforeSave+=Word_Application_DocumentBeforeSave;
				Word_Application.DocumentBeforeClose+=WordApplication_DocumentBeforeClose;
			}
		}

		public static void AddTracker(Document document,string fullPath){//,bool alreadyCreatedOrArchived,Control control){
			DocTracker docTracker=new DocTracker();
			docTracker.DocumentCur=document;
			docTracker.FullPath=fullPath;
			//docTracker.AlreadyCreatedOrArchived=alreadyCreatedOrArchived;
			//docTracker.Control_=control;
			_concurrentBagDocTrackers.Add(docTracker);
		}

		public static void WordApplication_DocumentBeforeClose(Word.Document word_document,ref bool cancel) {
			string pathOnDisk=word_document.FullName;
			DocTracker docTracker=_concurrentBagDocTrackers.FirstOrDefault(x=>x.FullPath==pathOnDisk && !x.Deleted);
			//This event handler seems to run instead of the normal internal Word event handler.
			//So we must supply the Save Changes dialog.
			//This applies whether we are tracking or not
			if(!word_document.Saved){
				if(MsgBox.Show(MsgBoxButtons.YesNo,"Save changes before closing?")){
					//simple save
					word_document.SaveAs2();//save it with the same name
					if(docTracker!=null){
						//docTracker.AlreadyCreatedOrArchived=true;
					}
				}
			}
			if(docTracker is null){
				return;
				//this is a document that we are not tracking. Do nothing.
				//For example, this will kick out during the initial letter merge stage because we did not yet establish a Tracker.
			}
			//They might have saved repeatedly while editing.
			//But we only care about here when they close the doc.
			//Word has a lock on the file at this point.
			//We need to close the doc to unlock it
			//We can't easily use our static Word_Application because wrong thread
			Word.Application word_Application=word_document.Application;
			word_document.Close(SaveChanges:false);
			if(word_Application.Documents.Count==0){
				//because this is the default behavior if we hadn't intercepted this event.
				word_Application.Quit();
				Marshal.ReleaseComObject(word_Application);
			}
			//Marshal.ReleaseComObject(word_document);//shouldn't need this because it was passed in as a reference.
			docTracker.Deleted=true;
			byte[] byteArrayDoc=File.ReadAllBytes(pathOnDisk);
			byte[] byteArrayHash=ODCrypt.MD5.Hash(byteArrayDoc);
			string hash=ToHexString(byteArrayHash);
			if(docTracker.DocumentCur.ChartLetterHash==hash){
				//no change
				return;
			}
			Document documentOld=docTracker.DocumentCur.Copy();
			docTracker.DocumentCur.ChartLetterHash=hash;
			Documents.Update(docTracker.DocumentCur,documentOld);
			//This gets invoked on the UI thread:
			GlobalFormOpenDental.RefreshCurrentModule(isClinicRefresh:false);
		}


			/*	///<summary>saveAsUI represents whether Save As UI should be displayed when saving document.</summary>
		public static void Word_Application_DocumentBeforeSave(Word.Document word_document,ref bool saveAsUI,ref bool cancel) {
			//This was all just a total dead end. I spent a week trying to make it work.
			//Without fail, Word would close 3 seconds after this method was done.
			string pathOnDisk=word_document.FullName;
			DocTracker docTracker=_concurrentBagDocTrackers.FirstOrDefault(x=>x.FullPath==pathOnDisk && !x.Deleted);
		if(docTracker is null){
				//We are not tracking this document
				return;
			}
			//MakeArchiveIfNeeded(word_document,docTracker);
		//}

		//<summary>Returns true if it made an archive</summary>
		//private static bool MakeArchiveIfNeeded(Word.Document word_document,DocTracker docTracker){
			if(docTracker.AlreadyCreatedOrArchived){
				//It's already been archived. We don't want to archive again.
				//OD ignores all saves after the first one.
				//It would be nice to update the hash with each save, but technical limitations prevent that.
				//Until the user closes the Word doc, the file is locked and can't even be read for hashing.
				//So we hash when they close.
				return;// false;
			}
			docTracker.AlreadyCreatedOrArchived=true;
			//So the code below only gets hit the first time.
			//The code below never gets hit when creating a new Word doc from template.
			//Since the file is locked, and since we need to make a copy, we will use a silent SaveAs.
			//It's the newly-created Word doc that will be attached to the newly-created OD document.
			//By making the copy be attached to a new document, this also puts the PK and filename in the correct order.
			Document documentSave=docTracker.DocumentCur.Copy();
			//everything is the same except:
			//documentSave.ChartLetterStatus=EnumDocChartLetterStatus.Active;//stays same
			documentSave.DateCreated=DateTime.Now;
			documentSave.UserNum=Security.CurUser.UserNum;
			documentSave.FileName="";//we'll set this after we create the new file
			Documents.Insert(documentSave);//gets new PK
			Document documentOld=documentSave.Copy();
			documentSave.FileName=ODFileUtils.CleanFileName(documentSave.Description+documentSave.DocNum)+".docx";
			Documents.Update(documentSave,documentOld);
			//New document is in place. Now we need to alter the old document to indicate that it's an archive.
			documentOld=docTracker.DocumentCur.Copy();
			docTracker.DocumentCur.ChartLetterStatus=EnumDocChartLetterStatus.AuditTrail;
			Documents.Update(docTracker.DocumentCur,documentOld);
			//Finally, make a copy of the Word doc, and switch to using it
			Patient patient=Patients.GetPat(documentSave.PatNum);
			string fileDestPath=ImageStore.GetFilePath(documentSave,ImageStore.GetPatientFolder(patient,ImageStore.GetPreferredAtoZpath()));
			string pathNew=pathOnDisk.Replace(".docx","2.docx");
			try{
				word_document.SaveAs2(pathNew);
				//word_document=word_document.Application.ActiveDocument;//this did not keep the doc from reverting
				//this should be silent to user.
				//This momentarily changes the current path of their doc to the new one.
				//But then something in Word changes it back to the old one.
				//We need to change it back to the new one
				//string filename=word_document.FullName;
				//Word.Application word_Application=word_document.Application;
				////todo: add the event handlers back?
				//Word_Application.Visible=true;//prevent Word from closing when the last document closes. Didn't work
				//_ignoreCloseEvent=true;
				//int countDocs=Word_Application.Documents.Count;//1
				//Word.Document word_document_dummy=Word_Application.Documents.Add();
				//countDocs=Word_Application.Documents.Count;//2
				word_document.Close();//If this is the last document, Word will close after this method exits. Hence, the above dummy.
				Marshal.ReleaseComObject(word_document); // Release the document
				word_document = null;
				//countDocs=Word_Application.Documents.Count;//1
				//Open the saved document to make it the active one
				//lock(_syncLock){//didn't work
				if(docTracker.Control_.InvokeRequired){
					docTracker.Control_.Invoke((Action)(() =>{
						Word.Document word_documentReopened = null;
						try{
							word_documentReopened=Word_Application.Documents.Open(pathNew);
						}
						finally{
							if(word_documentReopened!=null){
								Marshal.ReleaseComObject(word_documentReopened);
							}
						}
						//Word_Application.Documents.Open(pathNew);
						GC.KeepAlive(Word_Application);
					}));
				}
				else{
					Word_Application.Documents.Open(pathNew);
				}
				//word_document_dummy.Close(SaveChanges:false);
				//_ignoreCloseEvent=false;
				//Marshal.ReleaseComObject(word_document_dummy);
				//GC.KeepAlive(Word_Application);//didn't work
				//GC.KeepAlive(word_document);
				//This whole section just isn't working at all.
				//New plan: ignore intermittent saves by user.
				//Do everything when user closes the document.
				//For this, I will need a class here to track status.  I'll put it at the bottom.
			}
			catch(Exception ex){
				//not sure what to do
				MsgBox.Show("Error saving Word document to: "+pathNew);
			}
			finally{
				if(word_document!=null){
					Marshal.ReleaseComObject(word_document);
				}
			}
			//This gets invoked on the UI thread:
			//GlobalFormOpenDental.RefreshCurrentModule(isClinicRefresh:false);
			//return true;
		}*/

		public static string ToHexString(byte[] byteArray){
			string retVal=string.Join("",byteArray.Select(b => b.ToString("X2")));
			return retVal;
		}

		public static void Cleanup() {
			if(Word_Application != null) {
				//Word_Application.DocumentBeforeClose -= WordApplication_DocumentBeforeClose;
				Marshal.ReleaseComObject(Word_Application);
				Word_Application = null;
			}
		}

		public class DocTracker {
			public Document DocumentCur;
			public string FullPath;
			//<summary>This gets set to true for an initial merge. Also set true when editing an existing doc and it gets saved for the first time. This indicates that it will need to be hashed when closed.</summary>
			//public bool AlreadyCreatedOrArchived;
			///<summary>ConcurrentBag does not have a way to remove items, so we instead mark them deleted.</summary>
			public bool Deleted;
			//public Control Control_;
		}

	}
}
