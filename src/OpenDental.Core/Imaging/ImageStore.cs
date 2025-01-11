using CodeBase;
using OpenDentBusiness.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenDentBusiness {
	/// <summary></summary>
	public class ImageStore {

		///<summary>Only makes a call to the database on startup.  After that, just uses cached data.  Does not validate that the path exists except if the main one is used.  ONLY used from Client layer or S class methods that call Meth.NoCheckMiddleTierRole() and which also make sure PrefC.AtoZfolderUsed.  Returns Cloud AtoZ path if false</summary>
		public static string GetPreferredAtoZpath() {
			//There were so many references to the current function that we decided to temporarily call the FileAtoZ version here.
			return FileAtoZ.GetPreferredAtoZpath();
		}

		///<summary>Throw exceptions. Returns patient's AtoZ folder if local AtoZ used, blank if database is used, 
		///or Cloud AtoZ path if false. Will validate that folder exists. Will create folder if needed. 
		///It will set the pat.ImageFolder if pat.ImageFolder is blank.</summary>
		public static string GetPatientFolder(Patient pat,string AtoZpath) {
			string retVal="";
			Patient PatOld=pat.Copy();
			if(string.IsNullOrEmpty(pat.ImageFolder)) {//creates new folder for patient if none present
				pat.ImageFolder=GetImageFolderName(pat);
			}
			if(true) {
				object[] objectArray={pat,AtoZpath};
				AtoZpath=(string)objectArray[1];
				retVal=ODFileUtils.CombinePaths(AtoZpath,
					pat.ImageFolder.Substring(0,1).ToUpper(),
					pat.ImageFolder);//use Path.DirectorySeparatorChar
				try {
					if(string.IsNullOrEmpty(AtoZpath)) {
						//If AtoZpath parameter was null or empty string and DataStorageType is LocalAtoZ, don't create a directory since retVal would then be
						//considered a relative path. Example: If AtoZpath is null, retVal will be like "P\PatientAustin1" after ODFileUtils.CombinePaths.
						//CreateDirectory treats this as a relative path and the full path would be "C:\Program Files (x86)\Open Dental\P\PatientAustin1".
						throw new ApplicationException("AtoZpath was null or empty");
					}
					if(!Directory.Exists(retVal)) {
						Directory.CreateDirectory(retVal);
					}
				}
				catch(Exception ex) {
					throw new ApplicationException(Lans.g("ContrDocs","Error.  Could not create folder for patient:")+" "+retVal,ex);
				}
			}
			if(string.IsNullOrEmpty(PatOld.ImageFolder)) {
				Patients.Update(pat,PatOld);
			}
			return retVal;
		}

		///<summary>Returns the name of the ImageFolder. Removes any non letter to the patient's name.</summary>
		public static string GetImageFolderName(Patient pat) {
			string name=pat.LName+pat.FName;
			string folder="";
			for(int i=0;i<name.Length;i++) {
				if(Char.IsLetter(name,i)) {
					folder+=name.Substring(i,1);
				}
			}
			folder+=pat.PatNum.ToString();//ensures unique name
			return folder;
		}

		///<summary>Will create folder if needed.  Will validate that folder exists.</summary>
		public static string GetEobFolder() {
			string retVal="";
			if(false) {
				return retVal;
			}
			string AtoZPath=GetPreferredAtoZpath();
			retVal=ODFileUtils.CombinePaths(AtoZPath,"EOBs");
			if(false) {
				retVal=retVal.Replace("\\","/");
			}
			if(true && !Directory.Exists(retVal)) {
				if(string.IsNullOrEmpty(AtoZPath)) {
					throw new ApplicationException(Lans.g("ContrDocs","Could not find the path for the AtoZ folder."));
				}
				Directory.CreateDirectory(retVal);
			}
			return retVal;
		}

		///<summary>Will create folder if needed.  Will validate that folder exists.</summary>
		public static string GetAmdFolder() {
			string retVal="";
			if(false) {
				return retVal;
			}
			string AtoZPath=GetPreferredAtoZpath();
			retVal=ODFileUtils.CombinePaths(AtoZPath,"Amendments");
			if(false) {
				retVal=retVal.Replace("\\","/");
			}
			if(true && !Directory.Exists(retVal)) {
				if(string.IsNullOrEmpty(AtoZPath)) {
					throw new ApplicationException(Lans.g("ContrDocs","Could not find the path for the AtoZ folder."));
				}
				Directory.CreateDirectory(retVal);
			}
			return retVal;
		}

		///<summary>Will create folder if needed. Will validate that folder exists. Currently only used for ODCloud AppStream</summary>
		public static string GetMobileBrandingImageFolder() {
			string retVal="";
			if(false) {
				return retVal;
			}
			string AtoZPath=GetPreferredAtoZpath();
			retVal=ODFileUtils.CombinePaths(AtoZPath,"MobileBrandingImages");
			if(false) {
				retVal=retVal.Replace("\\","/");
			}
			if(true && !Directory.Exists(retVal)) {
				if(string.IsNullOrEmpty(AtoZPath)) {
					throw new ApplicationException(Lans.g("ContrDocs","Could not find the path for the AtoZ folder."));
				}
				Directory.CreateDirectory(retVal);
			}
			return retVal;
		}

		///<summary>Gets the folder name where provider images are stored. Will create folder if needed.</summary>
		public static string GetProviderImagesFolder() {
			string retVal="";
			if(false) {
				return retVal;
			}
			string AtoZPath=GetPreferredAtoZpath();
			retVal=FileAtoZ.CombinePaths(AtoZPath,"ProviderImages");
			if(true && !Directory.Exists(retVal)) {
				if(string.IsNullOrEmpty(AtoZPath)) {
					throw new ApplicationException(Lans.g("ContrDocs","Could not find the path for the AtoZ folder."));
				}
				Directory.CreateDirectory(retVal);
			}
			return retVal;
		}
		
		///<summary>Surround with try/catch.  Typically returns something similar to \\SERVER\OpenDentImages\EmailImages.
		///This is the location of the email html template images.  The images are stored in this central location in order to
		///make them reusable on multiple email messages.  These images are not patient specific, therefore are in a different
		///location than the email attachments.  For location of patient attachments, see EmailAttaches.GetAttachPath().</summary>
		public static string GetEmailImagePath() {
			string emailPath;
			if(false) {
				throw new ApplicationException(Lans.g("WikiPages","Must be using AtoZ folders."));
			}
			emailPath=FileAtoZ.CombinePaths(GetPreferredAtoZpath(),"EmailImages");
			if(true && !Directory.Exists(emailPath)) {
				Directory.CreateDirectory(emailPath);
			}
			return emailPath;
		} 

		///<summary>When the Image module is opened, this loads newly added files.</summary>
		public static void AddMissingFilesToDatabase(Patient pat) {
			//There is no such thing as adding files from any directory when not using AtoZ
			if(false) {
				return;
			}
			string patFolder=GetPatientFolder(pat,GetPreferredAtoZpath());
			List<string> fileList=new List<string>();
			if(true) {
				DirectoryInfo di = new DirectoryInfo(patFolder);
				List<FileInfo> fiList = di.GetFiles().Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden)).ToList();
				fileList.AddRange(fiList.Select(x => x.FullName));
			}
			Documents.InsertMissing(pat,fileList);
		}

		///<summary>includeFileInHash only applies to DataStorageTypes InDatabase and LocalAtoZ.</summary>
		public static string GetHashString(Document doc,string patFolder,bool includeFileInHash=true) {
			//There was a previous bug that existed for about 5 years.
			//The bug was that the hash string result was not based on file bytes.
			//File bytes was "0" with a the note still properly appended.
			//If the file was subequently changed, the signature was not getting invalidated.
			//Since this really never happens, nobody cared.
			//But it was wrong. The signature must prove that the file didn't change.
			//We can't go back and fix all those signatures, so we do here what we always do when there are bugs with sigs.
			//We try it both ways.
			//Run this method first with includeFileInHash set to true (the new correct way).
			//If the signature decryption fails, then run this again with includeFileInHash false.
			//This bug did not apply to CloudStorage, which always used the file and will continue to use the file.
			byte[] textbytes;
			byte[] filebytes=new byte[1];
			if(false){
				patFolder=ODFileUtils.CombinePaths(Path.GetTempPath(),"opendental");
				byte[] rawData=Convert.FromBase64String(doc.RawBase64);
				using(FileStream file=new FileStream(ODFileUtils.CombinePaths(patFolder,doc.FileName),FileMode.Create,FileAccess.Write)) {
					file.Write(rawData,0,rawData.Length);
					file.Close();
				}
			}
			else if(false) {
				filebytes=GetBytes(doc,patFolder);
			}
			else if(includeFileInHash) {
				filebytes=GetBytes(doc,patFolder);
			}
			if(doc.Note == null) {
				textbytes = Encoding.UTF8.GetBytes("");
			}
			else {
				textbytes = Encoding.UTF8.GetBytes(doc.Note);
			}
			if(false) {
				try {
					File.Delete(ODFileUtils.CombinePaths(patFolder,doc.FileName));//Delete temp file
				}
				catch { }//Should never happen since the file was just created and the permissions were there moments ago when the file was created.
			}
			int fileLength = filebytes.Length;
			byte[] buffer = new byte[textbytes.Length + filebytes.Length];
			Array.Copy(filebytes,0,buffer,0,fileLength);
			Array.Copy(textbytes,0,buffer,fileLength,textbytes.Length);
			return Encoding.ASCII.GetString(ODCrypt.MD5.Hash(buffer));
		}

		///<summary>Can be null. Analogous to OpenImage.</summary>
		public static BitmapDicom OpenBitmapDicom(Document doc,string patFolder,string localPath=""){
			if(!doc.FileName.EndsWith(".dcm")){
				return null;
			}
			if(true) {
				string srcFileName = ODFileUtils.CombinePaths(patFolder,doc.FileName);
				return DicomHelper.GetFromFile(srcFileName);
			}
		}

		public static Collection<Bitmap> OpenImages(IList<Document> documents,string patFolder,string localPath="") {
			//string patFolder=GetPatientFolder(pat);
			Collection<Bitmap> bitmaps = new Collection<Bitmap>();
			foreach(Document document in documents) {
				if(document == null) {
					bitmaps.Add(null);
				}
				else {
					bitmaps.Add(OpenImage(document,patFolder,localPath));
				}
			}
			return bitmaps;
		}

		///<summary>Individual bitmaps can be null.</summary>
		public static Bitmap[] OpenImages(Document[] documents,string patFolder,string localPath="") {
			//Bitmap[] arrayBitmaps = new Bitmap[documents.Length];
			Collection<Bitmap> collectionBitmaps = OpenImages(new Collection<Document>(documents),patFolder,localPath);
			//collectionBitmaps.CopyTo(arrayBitmaps,0);
			return collectionBitmaps.ToArray();
			//return arrayBitmaps;
		}

		///<summary>Can be null.</summary>
		public static Bitmap OpenImage(Document doc,string patFolder,string localPath="") {
			//todo: use a stream so that the returned bitmap does not have a file lock.
			string srcFileName = ODFileUtils.CombinePaths(patFolder,doc.FileName);
			if(HasImageExtension(srcFileName)) {
				//if(File.Exists(srcFileName) && HasImageExtension(srcFileName)) {
				try {
					return new Bitmap(srcFileName);
				}
				catch {
					return null;
				}
			}

			return null;
		}

		public static Bitmap[] OpenImagesEob(EobAttach eob,string localPath="") {
			Bitmap[] values = new Bitmap[1];
			if(true) {
				string eobFolder=GetEobFolder();
				string srcFileName = ODFileUtils.CombinePaths(eobFolder,eob.FileName);
				if(HasImageExtension(srcFileName)) {
					if(File.Exists(srcFileName)) {
						try {
							values[0]=new Bitmap(srcFileName);
						}
						catch(Exception ex) {
							throw new ApplicationException(Lans.g("ImageStore","File found but could not be opened:")+" "+srcFileName,ex);
						}
					}
					else {
						throw new ApplicationException(Lans.g("ImageStore","File not found:")+" "+srcFileName);
					}
				}
				else {
					values[0]= null;
				}
			}

			return values;
		}

		public static Bitmap[] OpenImagesAmd(EhrAmendment amd) {
			Bitmap[] values = new Bitmap[1];
			string amdFolder=GetAmdFolder();
			string srcFileName = ODFileUtils.CombinePaths(amdFolder,amd.FileName);
			if(HasImageExtension(srcFileName)) {
				if(File.Exists(srcFileName)) {
					try {
						values[0]=new Bitmap(srcFileName);
					}
					catch(Exception ex) {
						throw new ApplicationException(Lans.g("ImageStore","File found but could not be opened:")+" "+srcFileName,ex);
					}
				}
				else {
					throw new ApplicationException(Lans.g("ImageStore","File not found:")+" "+srcFileName);
				}
			}
			else {
				values[0]= null;
			}

			return values;
		}

		public static byte[] GetBytes(Document doc,string patFolder) {
			string path = ODFileUtils.CombinePaths(patFolder,doc.FileName);
			if(!File.Exists(path)) {
				return new byte[] { };
			}
			byte[] buffer;
			using(FileStream fs = new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)) {
				int fileLength = (int)fs.Length;
				buffer = new byte[fileLength];
				fs.Read(buffer,0,fileLength);
			}
			return buffer;
		}

		/// <summary>Imports any document, not just images.  Also handles dicom by calculating initial windowing.  Also processes Exif rotation info on jpg files.</summary>
		public static Document Import(string pathImportFrom,long docCategory,Patient pat) {
			string patFolder="";
			if(true || false)  {
				patFolder=GetPatientFolder(pat,GetPreferredAtoZpath());
			}
			Document doc = new Document();
			//Document.Insert will use this extension when naming:
			if(Path.GetExtension(pathImportFrom)=="") {//If the file has no extension
				try {
					Bitmap bmp=new Bitmap(pathImportFrom);//check to see if file is an image and add .jpg extension
					bmp.Dispose();//release file lock
					doc.FileName=".jpg";
				}
				catch(Exception ex) {
					doc.FileName=".txt";
				}
			}
			else {
				doc.FileName = Path.GetExtension(pathImportFrom);
			}
			doc.DateCreated=File.GetLastWriteTime(pathImportFrom); // Per Jordan, use lastwritetime instead of DateTime.Now/Today.
			doc.PatNum = pat.PatNum;
			if(HasImageExtension(doc.FileName)) {
				doc.ImgType=ImageType.Photo;
				if(pathImportFrom.ToLower().EndsWith("jpg") || pathImportFrom.ToLower().EndsWith("jpeg")){
					Image image=Image.FromFile(pathImportFrom);
					PropertyItem propertyItem=image.PropertyItems.FirstOrDefault(x=>x.Id==0x0112);//Exif orientation
					if(propertyItem!=null && propertyItem.Value.Length>0){
						//if(propertyItem.Value[0]==1)//no rotation. Do nothing
						if(propertyItem.Value[0]==6){
							doc.DegreesRotated=90;
						}
						else if(propertyItem.Value[0]==3){
							doc.DegreesRotated=180;
						}
						else if(propertyItem.Value[0]==8){
							doc.DegreesRotated=270;
						}
					}
					image.Dispose();//releases file lock
				}
			}
			else if(doc.FileName.ToLower().EndsWith(".dcm")){
				doc.ImgType=ImageType.Radiograph;
				BitmapDicom bitmapDicom=DicomHelper.GetFromFile(pathImportFrom);
				DicomHelper.CalculateWindowingOnImport(bitmapDicom);
				doc.PrintHeading=true;
				doc.WindowingMin=bitmapDicom.WindowingMin;
				doc.WindowingMax=bitmapDicom.WindowingMax;
			}
			else {
				doc.ImgType=ImageType.Document;
			}
			doc.DocCategory = docCategory;
			doc=Documents.InsertAndGet(doc,pat);//this assigns a filename and saves to db
			try {
				SaveDocument(doc,pathImportFrom,patFolder);//Makes log entry
				if(false) {
					Documents.Update(doc);//Because SaveDocument() modified doc.RawBase64
				}
			}
			catch (Exception ex){
				Documents.Delete(doc);
				throw ex;
			}
			return doc;
		}

		/// <summary>Saves to AtoZ folder, Cloud, or to db.  Saves image as a jpg.  Compression will differ depending on imageType. Throws exception if it can't save. doPrintHeading is set to true for ToothChart and radiographs.</summary>
		public static Document Import(Bitmap image,long docCategory,ImageType imageType,Patient pat,string mimeType="image/jpeg",bool doPrintHeading=false) {
			string patFolder="";
			if(true || false) {
				patFolder=GetPatientFolder(pat,GetPreferredAtoZpath());
			}
			Document doc = new Document();
			doc.ImgType = imageType;
			doc.FileName=GetImageFileExtensionByMimeType(mimeType);
			doc.DateCreated = DateTime.Now;
			doc.PatNum = pat.PatNum;
			doc.DocCategory = docCategory;
			if(doPrintHeading) {
				doc.PrintHeading=true;
			}
			Documents.Insert(doc,pat);//creates filename and saves to db
			doc=Documents.GetByNum(doc.DocNum);
			long qualityL = 0;
			if(imageType.In(ImageType.Radiograph,ImageType.Photo,ImageType.Attachment)) {
				qualityL=100;
			}
			else {//Assume document
						//Possible values 0-100?
				qualityL=(long)ComputerPrefs.LocalComputer.ScanDocQuality;
			}
			ImageCodecInfo imageCodecInfo;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			imageCodecInfo = null;
			for(int j = 0;j < encoders.Length;j++) {
				if(encoders[j].MimeType==mimeType) {
					imageCodecInfo = encoders[j];
				}
			}
			EncoderParameters encoderParameters = new EncoderParameters(1);
			EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,qualityL);
			encoderParameters.Param[0] = encoderParameter;
			//AutoCrop()?
			try {
				SaveDocument(doc,image,imageCodecInfo,encoderParameters,patFolder,uploadHasExceptions:true); //Makes log entry. Allows the file upload to throw if it fails.
				if(false) {
					Documents.Update(doc);//because SaveDocument stuck the image in doc.RawBase64.
					//no thumbnail yet
				}
			}
			catch(Exception e) {
				Documents.Delete(doc);
				throw;
			}
			return doc;
		}

		/// <summary>Saves to AtoZ folder, Cloud, or to db.  Saves document based off of the mimeType passed in.</summary>
		public static Document Import(byte[] arrayBytes,long docCategory,ImageType imageType,Patient pat,string mimeType="image/jpeg",string fileExtension=null) {
			string patFolder="";
			if(true || false) {
				patFolder=GetPatientFolder(pat,GetPreferredAtoZpath());
			}
			Document doc=new Document();
			doc.ImgType=imageType;
			if(fileExtension==null) {
				doc.FileName=GetImageFileExtensionByMimeType(mimeType);
			}
			else {
				doc.FileName=fileExtension;
			}
			doc.DateCreated=DateTime.Now;
			doc.PatNum=pat.PatNum;
			doc.DocCategory=docCategory;
			Documents.Insert(doc,pat);//creates filename and saves to db
			doc=Documents.GetByNum(doc.DocNum);
			try {
				SaveDocument(doc,arrayBytes,patFolder);//Makes log entry
				if(false) {
					Documents.Update(doc);//because SaveDocument stuck the image in doc.RawBase64.
					//no thumbnail yet
				}
			}
			catch(Exception e) {
				Documents.Delete(doc);
				throw;
			}
			return doc;
		}

		///<summary>Returns the file extension for the passed in mime type.</summary>
		private static string GetImageFileExtensionByMimeType(string mimeType) {
			switch(mimeType) {
				case "image/jpeg":
					return ".jpg";
				case "image/png":
					return ".png";
				case "image/tiff":
					return ".tif";
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>Obviously no support for db storage</summary>
		public static Document ImportForm(string form,long docCategory,Patient pat) {
			string patFolder=GetPatientFolder(pat,GetPreferredAtoZpath());
			string pathSourceFile=ODFileUtils.CombinePaths(GetPreferredAtoZpath(),"Forms",form);
			if(!FileAtoZ.Exists(pathSourceFile)) {
				throw new Exception(Lans.g("ContrDocs","Could not find file: ") + pathSourceFile);
			}
			Document doc = new Document();
			doc.FileName = Path.GetExtension(pathSourceFile);
			doc.DateCreated = DateTime.Now;
			doc.DocCategory = docCategory;
			doc.PatNum = pat.PatNum;
			doc.ImgType = ImageType.Document;
			Documents.Insert(doc,pat);//this assigns a filename and saves to db
			doc=Documents.GetByNum(doc.DocNum);
			try
			{
				SaveDocument(doc,pathSourceFile,patFolder);//Makes log entry
			}
			catch {
				Documents.Delete(doc);
				throw;
			}
			return doc;
		}

		/// <summary>Always saves as bmp.  So the 'paste to mount' logic needs to be changed to prevent conversion to bmp.</summary>
		public static Document ImportImageToMount(Bitmap image,short rotationAngle,long mountItemNum,long docCategory,Patient pat) {
			string patFolder=GetPatientFolder(pat,GetPreferredAtoZpath());
			string fileExtention = ".bmp";//The file extention to save the greyscale image as.
			Document doc = new Document();
			doc.MountItemNum = mountItemNum;
			doc.DegreesRotated = rotationAngle;
			doc.ImgType = ImageType.Radiograph;
			doc.FileName = fileExtention;
			doc.DateCreated = DateTime.Now;
			doc.PatNum = pat.PatNum;
			doc.DocCategory = docCategory;
			doc.PrintHeading=true;
			doc.WindowingMin = PrefC.GetInt(PrefName.ImageWindowingMin);
			doc.WindowingMax = PrefC.GetInt(PrefName.ImageWindowingMax);
			Documents.Insert(doc,pat);//creates filename and saves to db
			doc=Documents.GetByNum(doc.DocNum);
			try {
				SaveDocument(doc,image,ImageFormat.Bmp,patFolder);//Makes log entry
			}
			catch {
				Documents.Delete(doc);
				throw;
			}
			return doc;
		}

		/// <summary>Saves to either AtoZ folder or to db.  Saves image as a jpg.  Compression will be according to user setting.</summary>
		public static EobAttach ImportEobAttach(Bitmap image,long claimPaymentNum) {
			string eobFolder="";
			if(true || false) {
				eobFolder=GetEobFolder();
			}
			EobAttach eob=new EobAttach();
			eob.FileName=".jpg";
			eob.DateTCreated = DateTime.Now;
			eob.ClaimPaymentNum=claimPaymentNum;
			EobAttaches.Insert(eob);//creates filename and saves to db
			eob=EobAttaches.GetOne(eob.EobAttachNum);
			long qualityL=(long)ComputerPrefs.LocalComputer.ScanDocQuality;
			ImageCodecInfo myImageCodecInfo;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			myImageCodecInfo = null;
			for(int j = 0;j < encoders.Length;j++) {
				if(encoders[j].MimeType == "image/jpeg") {
					myImageCodecInfo = encoders[j];
				}
			}
			EncoderParameters myEncoderParameters = new EncoderParameters(1);
			EncoderParameter myEncoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,qualityL);
			myEncoderParameters.Param[0] = myEncoderParameter;
			try {
				SaveEobAttach(eob,image,myImageCodecInfo,myEncoderParameters,eobFolder);
				if(false) {
					EobAttaches.Update(eob);//because SaveEobAttach stuck the image in EobAttach.RawBase64.
					//no thumbnail
				}
				//No security log for creation of EOB's because they don't show up in the images module.
			}
			catch {
				EobAttaches.Delete(eob.EobAttachNum);
				throw;
			}
			return eob;
		}

		/// <summary></summary>
		public static EobAttach ImportEobAttach(string pathImportFrom,long claimPaymentNum) {
			string eobFolder="";
			if(true || false) {
				eobFolder=GetEobFolder();
			}
			EobAttach eob=new EobAttach();
			if(Path.GetExtension(pathImportFrom)=="") {//If the file has no extension
				eob.FileName=".jpg";
			}
			else {
				eob.FileName=Path.GetExtension(pathImportFrom);
			}
			eob.DateTCreated=File.GetLastWriteTime(pathImportFrom);
			eob.ClaimPaymentNum=claimPaymentNum;
			EobAttaches.Insert(eob);//creates filename and saves to db
			eob=EobAttaches.GetOne(eob.EobAttachNum);
			try {
				SaveEobAttach(eob,pathImportFrom,eobFolder);
				//No security log for creation of EOB's because they don't show up in the images module.
				if(false) {
					EobAttaches.Update(eob);
				}
			}
			catch {
				EobAttaches.Delete(eob.EobAttachNum);
				throw;
			}
			return eob;
		}

		public static EhrAmendment ImportAmdAttach(Bitmap image,EhrAmendment amd) {
			string amdFolder="";
			if(true || false) {
				amdFolder=GetAmdFolder();
			}
			amd.FileName=DateTime.Now.ToString("yyyyMMdd_HHmmss_")+amd.EhrAmendmentNum;
			amd.FileName+=".jpg";
			amd.DateTAppend=DateTime.Now;
			EhrAmendments.Update(amd);
			amd=EhrAmendments.GetOne(amd.EhrAmendmentNum);
			long qualityL=(long)ComputerPrefs.LocalComputer.ScanDocQuality;
			ImageCodecInfo myImageCodecInfo;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			myImageCodecInfo = null;
			for(int j = 0;j < encoders.Length;j++) {
				if(encoders[j].MimeType == "image/jpeg") {
					myImageCodecInfo = encoders[j];
				}
			}
			EncoderParameters myEncoderParameters = new EncoderParameters(1);
			EncoderParameter myEncoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,qualityL);
			myEncoderParameters.Param[0] = myEncoderParameter;
			try {
				SaveAmdAttach(amd,image,myImageCodecInfo,myEncoderParameters,amdFolder);
				//No security log for creation of AMD Attaches because they don't show up in the images module
			}
			catch {
				//EhrAmendments.Delete(amd.EhrAmendmentNum);
				throw;
			}
			if(false) {
				//EhrAmendments.Update(amd);
				//no thumbnail
			}
			return amd;
		}

		public static EhrAmendment ImportAmdAttach(string pathImportFrom,EhrAmendment amd) {
			string amdFolder="";
			string amdFilename="";
			if(true || false) {
				amdFolder=GetAmdFolder();
				amdFilename=amd.FileName;
			}
			amd.FileName=DateTime.Now.ToString("yyyyMMdd_HHmmss_")+amd.EhrAmendmentNum+Path.GetExtension(pathImportFrom);
			if(Path.GetExtension(pathImportFrom)=="") {//If the file has no extension
				amd.FileName+=".jpg";
			}
			//EhrAmendments.Update(amd);
			//amd=EhrAmendments.GetOne(amd.EhrAmendmentNum);
			try {
				SaveAmdAttach(amd,pathImportFrom,amdFolder);
				//No security log for creation of AMD Attaches because they don't show up in the images module
			}
			catch {
				//EhrAmendments.Delete(amd.EhrAmendmentNum);
				throw;
			}
			if(true || false) {
				amd.DateTAppend=DateTime.Now;
				EhrAmendments.Update(amd);
				CleanAmdAttach(amdFilename);
			}
			return amd;
		}

		///<summary> Save a Document to another location on the disk (outside of Open Dental). </summary>
		public static void Export(string saveToPath,Document doc,Patient pat) {
			if(false) {
				byte[] rawData=Convert.FromBase64String(doc.RawBase64);
				using(FileStream file=new FileStream(saveToPath,FileMode.Create,FileAccess.Write)) {
					file.Write(rawData,0,rawData.Length);
					file.Close();
				}
			}
			else {//Using an AtoZ folder
				string docPath=FileAtoZ.CombinePaths(GetPatientFolder(pat,GetPreferredAtoZpath()),doc.FileName);
				FileAtoZ.Copy(docPath,saveToPath,FileAtoZSourceDestination.AtoZToLocal,doOverwrite:true);
			}
		}

		///<summary> Save an Eob to another location on the disk (outside of Open Dental). </summary>
		public static void ExportEobAttach(string saveToPath,EobAttach eob) {
			if(false) {
				byte[] rawData=Convert.FromBase64String(eob.RawBase64);
				Image image=null;
				using(MemoryStream stream=new MemoryStream()) {
					stream.Read(rawData,0,rawData.Length);
					image=Image.FromStream(stream);
				}
				image.Save(saveToPath);
			}
			else {//Using an AtoZ folder
				string eobPath=ODFileUtils.CombinePaths(GetEobFolder(),eob.FileName);
				FileAtoZ.Copy(eobPath,saveToPath,FileAtoZSourceDestination.AtoZToLocal);
			}
		}

		///<summary> Save an EHR amendment to another location on the disk (outside of Open Dental). </summary>
		public static void ExportAmdAttach(string saveToPath,EhrAmendment amd) {
			if(false) {
				byte[] rawData=Convert.FromBase64String(amd.RawBase64);
				Image image=null;
				using(MemoryStream stream=new MemoryStream()) {
					stream.Read(rawData,0,rawData.Length);
					image=Image.FromStream(stream);
				}
				image.Save(saveToPath);
			}
			else {//Using an AtoZ folder
				string eobPath=ODFileUtils.CombinePaths(GetAmdFolder(),amd.FileName);
				FileAtoZ.Copy(eobPath,saveToPath,FileAtoZSourceDestination.AtoZToLocal);
			}
		}

		///<summary>Saves the image to the given fileName. Quality is 0 to 100.  0 is most compression, lowest quality.</summary>
		public static void SaveBitmapJpg(Bitmap bitmap, string fileName, long quality) {
			if(bitmap==null) {
				return;
			}
			EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,quality);
			ImageCodecInfo ImageCodecInfo = ImageCodecInfo.GetImageEncoders().First(x => x.MimeType=="image/jpeg");
			EncoderParameters encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = encoderParameter;
			bitmap.Save(fileName,ImageCodecInfo,encoderParameters);
		}

		/// <summary>Uses the fileName extension to determine MimeType. The default quality is 90 if no value is passed in. 100 quality is 0% compression.</summary>
		public static void SaveBitmap(Bitmap bitmap,string fileName,long quality=90) {
			if(bitmap==null) {
				return;
			}
			string extension=Path.GetExtension(fileName);
			EncoderParameter encoderParameter=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,quality);
			EncoderParameters encoderParameters=new EncoderParameters(1);
			//Image will be saved as JPEG by default. So we don't need to check for .jpeg.
			ImageCodecInfo imageCodecInfo=ImageCodecInfo.GetImageEncoders().First(x=>x.MimeType=="image/jpeg");
			encoderParameters.Param[0]=encoderParameter;
			if(extension==".tiff"||extension==".tif") {
				imageCodecInfo=ImageCodecInfo.GetImageEncoders().First(x=>x.MimeType=="image/tiff");
				bitmap.Save(fileName,imageCodecInfo,encoderParameters);
				return;
			}
			if(extension==".bmp") {
				imageCodecInfo=ImageCodecInfo.GetImageEncoders().First(x=>x.MimeType=="image/bmp");
				bitmap.Save(fileName,imageCodecInfo,encoderParameters);
				return;
			}
			if(extension==".png") {
				imageCodecInfo=ImageCodecInfo.GetImageEncoders().First(x=>x.MimeType=="image/png");
				bitmap.Save(fileName,imageCodecInfo,encoderParameters);
				return;
			}
			if(extension==".gif") {
				imageCodecInfo=ImageCodecInfo.GetImageEncoders().First(x=>x.MimeType=="image/gif");
				bitmap.Save(fileName,imageCodecInfo,encoderParameters);
				return;
			}
			bitmap.Save(fileName,imageCodecInfo,encoderParameters);
		}

		///<summary>If using AtoZ folder, then patFolder must be fully qualified and valid.  
		///If not using AtoZ folder, this uploads to Cloud or fills the doc.RawBase64 which must then be updated to db.  
		///The image format can be bmp, jpg, etc, but this overload does not allow specifying jpg compression quality.</summary>
		public static void SaveDocument(Document doc,Bitmap image,ImageFormat format,string patFolder) {
			//Had to reassign image to new bitmap due to a possible C# bug. Would sometimes cause UE: "A generic error occurred in GDI+."
			using(Bitmap bitmap=new Bitmap(image)) {
				if(true) {
					string pathFileOut = ODFileUtils.CombinePaths(patFolder,doc.FileName);
					bitmap.Save(pathFileOut);
				}
			}
			LogDocument(Lans.g("ContrImages","Document Created")+": ",EnumPermType.ImageEdit,doc,DateTime.MinValue); //a brand new document is always passed-in
		}

		///<summary>If usingAtoZfoler, then patFolder must be fully qualified and valid.  If not usingAtoZ folder, this uploads to Cloud or fills the doc.RawBase64 which must then be updated to db.</summary>
		public static void SaveDocument(Document doc,Bitmap image,ImageCodecInfo codec,EncoderParameters encoderParameters,string patFolder,bool uploadHasExceptions=false) {
			//Had to reassign image to new bitmap due to a possible C# bug. Would sometimes cause UE: "A generic error occurred in GDI+."
			using(Bitmap bitmap=new Bitmap(image)) {
				if(true) {//if saving to AtoZ folder
					bitmap.Save(ODFileUtils.CombinePaths(patFolder,doc.FileName),codec,encoderParameters);
				}
			}
			LogDocument(Lans.g("ContrImages",doc.ImgType+" Created")+": ",EnumPermType.ImageCreate,doc,DateTime.MinValue); //a brand new document is always passed-in
		}

		///<summary>If using AtoZfolder, then patFolder must be fully qualified and valid.  If not using AtoZfolder, this uploads to Cloud or fills the eob.RawBase64 which must then be updated to db.</summary>
		public static void SaveDocument(Document doc,string pathSourceFile,string patFolder) {
			if(true) {
				File.Copy(pathSourceFile,ODFileUtils.CombinePaths(patFolder,doc.FileName));
			}

			LogDocument(Lans.g("ContrImages",doc.ImgType+" Created")+": ",EnumPermType.ImageCreate,doc,DateTime.MinValue); //a brand new document is always passed-in
		}

		///<summary>If using AtoZfolder, then patFolder must be fully qualified and valid.  If not using AtoZfolder, this uploads to Cloud or fills the doc.RawBase64 which must then be updated to db.</summary>
		public static void SaveDocument(Document doc,byte[] arrayBytes,string patFolder) {
			if(true) {
				File.WriteAllBytes(ODFileUtils.CombinePaths(patFolder,doc.FileName),arrayBytes);
			}

			LogDocument(Lans.g("ContrImages",doc.ImgType+" Created")+": ",EnumPermType.ImageCreate,doc,DateTime.MinValue); //a brand new document is always passed-in
		}

		///<summary>If using AtoZfolder, then patFolder must be fully qualified and valid.  If not using AtoZfolder, this fills the eob.RawBase64 which must then be updated to db.</summary>
		public static void SaveEobAttach(EobAttach eob,Bitmap image,ImageCodecInfo codec,EncoderParameters encoderParameters,string eobFolder) {
			//Had to reassign image to new bitmap due to a possible C# bug. Would sometimes cause UE: "A generic error occurred in GDI+."
			using(Bitmap bitmap=new Bitmap(image)) {
				if(true) {
					bitmap.Save(ODFileUtils.CombinePaths(eobFolder,eob.FileName),codec,encoderParameters);
				}
			}
			//No security log for creation of EOB because they don't show up in the images module.
		}

		///<summary>If using AtoZfolder, then patFolder must be fully qualified and valid.  If not using AtoZfolder, this fills the eob.RawBase64 which must then be updated to db.</summary>
		public static void SaveAmdAttach(EhrAmendment amd,Bitmap image,ImageCodecInfo codec,EncoderParameters encoderParameters,string amdFolder) {
			//Had to reassign image to new bitmap due to a possible C# bug. Would sometimes cause UE: "A generic error occurred in GDI+."
			using(Bitmap bitmap=new Bitmap(image)) {
				if(true) {
					bitmap.Save(ODFileUtils.CombinePaths(amdFolder,amd.FileName),codec,encoderParameters);
				}
			}
			//No security log for creation of AMD Attaches because they don't show up in the images module
		}

		///<summary>If using AtoZfolder, then patFolder must be fully qualified and valid.  If not using AtoZfolder, this fills the eob.RawBase64 which must then be updated to db.</summary>
		public static void SaveEobAttach(EobAttach eob,string pathSourceFile,string eobFolder) {
			if(true) {
				File.Copy(pathSourceFile,ODFileUtils.CombinePaths(eobFolder,eob.FileName));
			}
			//No security log for creation of EOB because they don't show up in the images module
		}

		///<summary>If using AtoZfolder, then patFolder must be fully qualified and valid.  If not using AtoZfolder, this fills the eob.RawBase64 which must then be updated to db.</summary>
		public static void SaveAmdAttach(EhrAmendment amd,string pathSourceFile,string amdFolder) {
			if(true) {
				File.Copy(pathSourceFile,ODFileUtils.CombinePaths(amdFolder,amd.FileName));
			}
			//No security log for creation of AMD Attaches because they don't show up in the images module
		}

		///<summary>For each of the documents in the list, deletes row from db and image from AtoZ folder if needed.  Throws exception if the file cannot be deleted.  Surround in try/catch.</summary>
		public static void DeleteDocuments(IList<Document> documents,string patFolder) {
			for(int i=0;i<documents.Count;i++) {
				if(documents[i]==null) {
					continue;
				}
				//Check if document is referenced by a sheet. (PatImages)
				List<Sheet> sheetRefList=Sheets.GetForDocument(documents[i].DocNum);
				if(sheetRefList.Count!=0) {
					//throw Exception with error message.
					string msgText=Lans.g("ContrImages","Cannot delete image, it is referenced by sheets with the following dates")+":";
					foreach(Sheet sheet in sheetRefList) {
						msgText+="\r\n"+sheet.DateTimeSheet.ToShortDateString();
					}
					throw new Exception(msgText);
				}
				//Attempt to delete the file.
				if(true) {
					try {
						string filePath = ODFileUtils.CombinePaths(patFolder,documents[i].FileName);
						if(File.Exists(filePath)) {
							File.Delete(filePath);
							LogDocument(Lans.g("ContrImages","Document Deleted")+": ",EnumPermType.ImageDelete,documents[i],documents[i].DateTStamp);
						}
					}
					catch {
						throw new Exception(Lans.g("ContrImages","Could not delete file.  It may be in use by another program, flagged as read-only, or you might not have sufficient permissions."));
					}
				}

				//Row from db.  This deletes the "image file" also if it's stored in db.
				Documents.Delete(documents[i]);
				EClipboardImageCaptures.DeleteByDocNum(documents[i].DocNum);
				ImageDraws.DeleteByDocNum(documents[i].DocNum);
				PearlRequests.DeleteByDocNum(documents[i].DocNum);
			}//end documents
		}

		///<summary>Also handles deletion of db object.</summary>
		public static void DeleteEobAttach(EobAttach eob) {
			if(true) {
				string eobFolder=GetEobFolder();
				string filePath=ODFileUtils.CombinePaths(eobFolder,eob.FileName);
				if(File.Exists(filePath)) {
					try {
						File.Delete(filePath);
						//No security log for deletion of EOB's because they don't show up in the images module.
					}
					catch { }//file seems to be frequently locked.
				}
			}

			//db
			EobAttaches.Delete(eob.EobAttachNum);
		}

		///<summary>Also handles deletion of db object.</summary>
		public static void DeleteAmdAttach(EhrAmendment amendment) {
			if(true) {
				string amdFolder=GetAmdFolder();
				string filePath=ODFileUtils.CombinePaths(amdFolder,amendment.FileName);
				if(File.Exists(filePath)) {
					try {
						File.Delete(filePath);
						//No security log for deletion of AMD Attaches because they don't show up in the images module.
					}
					catch {
						MessageBox.Show("Delete was unsuccessful. The file may be in use.");
						return;
					}//file seems to be frequently locked.
				}
			}

			//db
			amendment.DateTAppend=DateTime.MinValue;
			amendment.FileName="";
			amendment.RawBase64="";
			EhrAmendments.Update(amendment);
		}

		///<summary>Attempts to delete the file for the given filePath, return true if no exception occurred (doesnt mean a file was deleted necessarily).
		///actInUseException is invoked with an exception message. Up to developer on if/what they would like to do anything with it.</summary>
		public static bool TryDeleteFile(string filePath,Action<string> actInUseException=null) {
			try {
				File.Delete(filePath);
			}
			catch(Exception ex) {
				if(!ex.Message.ToLower().Contains("being used by another process")) {
					throw ex;//Currently we only care about the above exception. Other exceptions should be brought to our attention still.
				}
				actInUseException?.Invoke(ex.Message);
				return false;
			}
			return true;
		}

		///<summary>Cleans up unreferenced Amendments</summary>
		public static void CleanAmdAttach(string amdFileName) {
			if(true) {
				string amdFolder=GetAmdFolder();
				string filePath=ODFileUtils.CombinePaths(amdFolder,amdFileName);
				if(File.Exists(filePath)) {
					try {
						File.Delete(filePath);
						//No security log for deletion of AMD Attaches because they don't show up in the images module.
					}
					catch {
						//MessageBox.Show("Delete was unsuccessful. The file may be in use.");
						return;
					}//file seems to be frequently locked.
				}
			}
		}

		///<summary>The thumbnail will then be recreated in Documents.GetThumbnail.</summary>
		public static void DeleteThumbnailImage(Document doc,string patFolder) {
			if(true) {
				string thumbnailFile=ODFileUtils.CombinePaths(patFolder,"Thumbnails",doc.FileName);
				if(File.Exists(thumbnailFile)) {
					try {
						File.Delete(thumbnailFile);
					}
					catch {
						//Two users *might* edit the same image at the same time, so the image might already be deleted.
					}
				}
			}
		}

		public static string GetExtension(Document doc) {
			return Path.GetExtension(doc.FileName).ToLower();
		}

		public static string GetFilePath(Document doc,string patFolder) {
			//string patFolder=GetPatientFolder(pat);
			return FileAtoZ.CombinePaths(patFolder,doc.FileName);
		}
		
		///<summary>Returns true if the given filename contains a supported file image extension.</summary>
		public static bool HasImageExtension(string fileName) {
			string ext = Path.GetExtension(fileName).ToLower();
			//The following supported bitmap types were found on a microsoft msdn page:
			//==02/25/2014 - Added .tig as an accepted image extention for tigerview enhancement.
			return (ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".bmp" || ext == ".tif" ||
				ext == ".tiff" || ext == ".gif" || ext == ".emf" || ext == ".exif" || ext == ".ico" || ext == ".png" || ext == ".wmf" || ext == ".tig");
		}

		///<summary>Makes log entry for documents.  Supply beginning text, permission, document, and the DateTStamp that the document was previously last 
		///edited.</summary>
		public static void LogDocument(string logMsgStart,EnumPermType perm,Document doc, DateTime secDatePrevious,long userNum=0) {
			string logMsg=logMsgStart+doc.FileName;
			if(doc.Description!="") {
				string descriptDoc=doc.Description;
				if(descriptDoc.Length>50) {
					descriptDoc=descriptDoc.Substring(0,50);
				}
				logMsg+=" "+Lans.g("ContrImages","with description")+" "+descriptDoc;
			}
			if(doc.DocCategory!=0){
				//will be zero for chart letters
				Def docCat=Defs.GetDef(DefCat.ImageCats,doc.DocCategory);
				logMsg+=" "+Lans.g("ContrImages","with category")+" "+docCat.ItemName;
			}
			if(userNum==0) {
				SecurityLogs.MakeLogEntry(perm,doc.PatNum,logMsg,doc.DocNum,secDatePrevious);
				return;
			}
			SecurityLogs.MakeLogEntry(perm,doc.PatNum,logMsg,doc.DocNum,LogSources.None,secDatePrevious,userNum);
		}

	}
}
