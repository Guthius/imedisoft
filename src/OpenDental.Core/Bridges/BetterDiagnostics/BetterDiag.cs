using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using CodeBase;
using OpenDentBusiness.BetterDiag;

namespace OpenDentBusiness.Bridges{
	/// <summary></summary>
	public class BetterDiag:IDisposable{
		public Patient Patient_;
		///<summary>Contains the bitmap to be sent to BetterDiagnostics.</summary>
		public Bitmap Bitmap_;
		///<summary>For a single document. In this case, the MountNum will be zero.</summary>
		public Document Document_;
		///<summary>Only used for a mount. We need this to determine the offset for each bitmap in the mount. Since we will get back drawings for an individual bitmap, we need to shift them all. We store drawings for a mount all relative to the mount origin, kind of like a big giant bitmap.</summary>
		public MountItem MountItem_;
		///<summary>Used to trigger a UI refresh when the thread is complete.</summary>
		public static event EventHandler EventRefreshDisplay;

		///<summary>Image can be given as a bitmap or a file path. Set mountItem if image is part of a mount. Returns null if filePath couldn't be converted to bitmap or image was already sent to BetterDiagnostics.</summary>
		public static void SendSingleOnThread(Patient patient,Document document,Bitmap bitmap=null,string filePath="",MountItem mountItem=null) {
			if(bitmap==null) {
				try {
					bitmap=new Bitmap(filePath);
				}
				catch {
					return;//File couldn't be converted to bitmap
				}
			}
			BetterDiag betterDiag=new BetterDiag();
			betterDiag.Patient_=patient;
			if(mountItem!=null) {
				betterDiag.Bitmap_=ImageHelper.ApplyDocumentSettingsToImage(document,bitmap,ImageSettingFlags.ALL);//Creates copy of bitmap
			}
			else {
				betterDiag.Bitmap_=(Bitmap)bitmap.Clone();//Creates copy of bitmap
			}
			betterDiag.MountItem_=mountItem;
			betterDiag.Document_=document;
			betterDiag.SendOnThread();
		}

		///<summary>Returns whether an image category should automatically upload an image to BetterDiagnostics. Returns false if the BetterDiagnostics program property containing the image category names is not found.</summary>
		public static bool DoAutoUploadForImageCategory(long defNumCategory) {
			if(Programs.GetActiveImagingAIProgram()!=ProgramName.BetterDiagnostics) {
				return false;
			}
			string categoriesStr;
			try {
				categoriesStr=ProgramProperties.GetPropVal(ProgramName.BetterDiagnostics,"Image categories for automatic upload");
			}
			catch {
				return false;
			}
			List<string> listCategories=categoriesStr.Split(',').ToList();
			string categoryName=Defs.GetName(DefCat.ImageCats,defNumCategory);
			if(listCategories.Contains(categoryName)) {
				return true;
			}
			return false;
		}

		public void Dispose() {
			Bitmap_?.Dispose();
		}

		
		public void SendOnThread() {
			ODThread oDThreadBetterDiag=new ODThread(this.SendOnThreadWorker);
			//Swallow all exceptions and allow thread to exit gracefully.
			oDThreadBetterDiag.AddExceptionHandler(new ODThread.ExceptionDelegate((Exception ex) => {
				AlertUserOfError("There was an error processing a BetterDiagnostics image.");
			}));
			oDThreadBetterDiag.AddExitHandler(new ODThread.WorkerDelegate((o) => {
				EventRefreshDisplay?.Invoke(this,new EventArgs());
				Dispose();
			}));
			oDThreadBetterDiag.Start(isAutoCleanup: true);
		}

		///<summary>This is the worker thread.</summary>
		public void SendOnThreadWorker(ODThread oDThread){
			if(Document_ is null) {
				throw new Exception("Document must be specified.");
			}
			if(Bitmap_ is null) {
				return;
			}
			BetterDiagResponse betterDiagResponse=null;
			try {
				betterDiagResponse=BetterDiagApiClient.SendOneImageToBetterDiag(Document_,Bitmap_,Patient_);
			}
			catch(Exception ex) {
				AlertUserOfError("There was an error uploading an image to BetterDiagnostics",ex.Message);
				return;
			}
			if(betterDiagResponse==null) {
				AlertUserOfError("A Better Diagnostics image failed to upload.");
				return;
			}
			ProcessResultsForOneImage(betterDiagResponse,Bitmap_,Document_,MountItem_);
		}

		/// <summary>Inserts ImageDraws from BetterDiagnostics API response.</summary>
		public void ProcessResultsForOneImage(BetterDiagResponse betterDiagResponse,Bitmap bitmap,Document document,MountItem mountItem){
			long mountNum=0;
			Point pointMountPos=new Point(0,0);
			float scale=1f;
			if(mountItem!=null) {
				scale=ImageDraws.CalcBitmapScaleToFitMountItem(bitmap.Width,bitmap.Height,mountItem.Width,mountItem.Height);
				Point pointPadding=ImageDraws.CalcBitmapPaddingToFitMountItem(bitmap.Width,bitmap.Height,mountItem.Width,mountItem.Height,scale);
				//Add padding to line up annotations with centered image in mount item.
				pointMountPos.X=mountItem.Xpos+pointPadding.X;
				pointMountPos.Y=mountItem.Ypos+pointPadding.Y;
				mountNum=mountItem.MountNum;
			}
			string fileName=document.FileName;
			fileName=fileName.Replace(Path.GetExtension(fileName),".jpg");//BetterDiagnostics always returns filename with .jpg instead of original extension
			if(!betterDiagResponse.findings.dictImageResults.TryGetValue(fileName,out List<ImageResult> listImageResults)) {
				return;//Something went wrong.
			}
			for(int i=0;i<listImageResults.Count;i++) {
				ImageDraw imageDraw=new ImageDraw();
				imageDraw.DocNum=document.DocNum;
				imageDraw.MountNum=mountNum;
				imageDraw.ImageAnnotVendor=EnumImageAnnotVendor.BetterDiagnostics;
				imageDraw.BetterDiagLayer=GetCategoryForTagName(listImageResults[i].tag_name);
				imageDraw.ColorDraw=GetColorForCategory(imageDraw.BetterDiagLayer);
				imageDraw.DrawType=ImageDrawType.Polygon;
				List<EnumCategoryBetterDiag> listEnumCategoryBetterDiagsToothParts=GetBetterDiagToothPartsCategories();
				if(!listEnumCategoryBetterDiagsToothParts.Contains(imageDraw.BetterDiagLayer)) {
					//Not tooth part.
					imageDraw.DrawType=ImageDrawType.Line;
					imageDraw.Details=listImageResults[i].tag_name;
				}
				List<PointF> listPointFs=listImageResults[i].points.Select(p => new PointF((float)p[0],(float)p[1])).ToList();
				listPointFs=ImageDraws.ScalePointsToMountItem(listPointFs,scale);
				listPointFs=ImageDraws.TranslatePointsToMountItem(listPointFs,pointMountPos);
				imageDraw.SetDrawingSegment(listPointFs);
				ImageDraws.Insert(imageDraw);
				//Add Text ImageDraw for bone level.
				if(imageDraw.BetterDiagLayer==EnumCategoryBetterDiag.BoneLevel) {
					ImageDraw imageDrawText=imageDraw.Copy();
					imageDrawText.Details="";//Don't show hover box for measurements.
					imageDrawText.DrawType=ImageDrawType.Text;
					imageDrawText.ColorDraw=GetTextColorForCategory(imageDrawText.BetterDiagLayer);
					PointF pointFText=new PointF();
					pointFText.X=(int)listImageResults[i].points[0][0];
					pointFText.Y=(int)listImageResults[i].points[0][1];
					pointFText=ImageDraws.ScalePointsToMountItem(ListTools.FromSingle(pointFText),scale)[0];
					pointFText=ImageDraws.TranslatePointsToMountItem(ListTools.FromSingle(pointFText),pointMountPos)[0];
					Point pointText=Point.Round(pointFText);
					//Calculate bone level measurement and set as text because it is not provided by the API response.
					string drawText=GetLineMeasurementString(imageDraw,document,mountNum);
					imageDrawText.SetLocAndText(pointText,drawText);
					ImageDraws.Insert(imageDrawText);
				}
			}
		}

		///<summary>Converts tag_name from API response into its corresponding enum value.</summary>
		public static EnumCategoryBetterDiag GetCategoryForTagName(string tagName) {
			switch(tagName) {
				case "Dentin":      return EnumCategoryBetterDiag.Dentin;
				case "Enamel":      return EnumCategoryBetterDiag.Enamel;
				case "Pulp":        return EnumCategoryBetterDiag.Pulp;
				case "Restoration": return EnumCategoryBetterDiag.Restoration;
				case "Crown":       return EnumCategoryBetterDiag.Crown;
				case "Cavity":      return EnumCategoryBetterDiag.Cavity;
				case "Bone loss":   return EnumCategoryBetterDiag.BoneLoss;
				case "Infection":   return EnumCategoryBetterDiag.Infection;
				case "Bone Level":  return EnumCategoryBetterDiag.BoneLevel;
				case "Iac":         return EnumCategoryBetterDiag.Iac;
				case "Nasal Floor": return EnumCategoryBetterDiag.NasalFloor;
				case "Normal Tmj":  return EnumCategoryBetterDiag.NormalTmj;
				case "Sinus":       return EnumCategoryBetterDiag.Sinus;
				default:            return EnumCategoryBetterDiag.None;
			}
		}

		///<summary>Returns hardcoded color for given EnumCategoryBetterDiag.</summary>
		public static Color GetColorForCategory(EnumCategoryBetterDiag enumCategoryBetterDiag) {
			switch(enumCategoryBetterDiag) {
				case EnumCategoryBetterDiag.Dentin:      return Color.FromArgb(50,255,255,0);
				case EnumCategoryBetterDiag.Enamel:      return Color.FromArgb(50,100,100,255);
				case EnumCategoryBetterDiag.Pulp:        return Color.FromArgb(75,255,0,0);
				case EnumCategoryBetterDiag.Restoration: return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.Crown:       return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.Cavity:      return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.BoneLoss:    return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.Infection:   return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.BoneLevel:   return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.Iac:         return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.NasalFloor:  return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.NormalTmj:   return Color.FromArgb(200,255,0,0);
				case EnumCategoryBetterDiag.Sinus:       return Color.FromArgb(200,255,0,0);
				default: return Color.FromArgb(0,0,0,0);
			}
		}

		///<summary>Returns hardcoded color for given EnumCategoryBetterDiag.</summary>
		public static Color GetTextColorForCategory(EnumCategoryBetterDiag enumCategoryBetterDiag) {
			switch(enumCategoryBetterDiag) {
				case EnumCategoryBetterDiag.Restoration: return Color.FromArgb(200,208,255,22);
				case EnumCategoryBetterDiag.Crown:       return Color.FromArgb(200,208,255,22);
				case EnumCategoryBetterDiag.Cavity:      return Color.FromArgb(200,208,255,22);
				case EnumCategoryBetterDiag.BoneLoss:    return Color.FromArgb(200,208,255,22);
				case EnumCategoryBetterDiag.Infection:   return Color.FromArgb(200,208,255,22);
				case EnumCategoryBetterDiag.BoneLevel:   return Color.FromArgb(200,208,255,22);
				default: return Color.FromArgb(0,0,0,0);
			}
		}

		///<summary>Returns list of EnumCategoryBetterDiags considered to be "tooth parts".</summary>
		public static List<EnumCategoryBetterDiag> GetBetterDiagToothPartsCategories() {
			List<EnumCategoryBetterDiag> listEnumCategoryBetterDiags=new List<EnumCategoryBetterDiag>();
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.Dentin);
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.Enamel);
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.Pulp);
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.Iac);
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.NasalFloor);
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.NormalTmj);
			listEnumCategoryBetterDiags.Add(EnumCategoryBetterDiag.Sinus);
			return listEnumCategoryBetterDiags;
		}

		///<summary>Calculates length of line using ImagingDefaultScaleValue pref or MountDef scaling settings. Returns empty string if ScaleValue not set properly for image or mount, or if imageDraw is not a Line. Works similarly to ControlImageDisplay.ToolBarMeasure_Click().</summary>
		private string GetLineMeasurementString(ImageDraw imageDraw,Document document,long mountNum) {
			if(imageDraw.DrawType!=ImageDrawType.Line) {
				return "";
			}
			string drawText="";
			string scaleStr=PrefC.GetString(PrefName.ImagingDefaultScaleValue);//If single image, we can use preference
			float lengthPixels=ImageDraws.CalcLengthLine(imageDraw.GetPoints());
			float scaleValue=MountDefs.GetScale(scaleStr);
			float decimals=MountDefs.GetDecimals(scaleStr);
			string unitsStr=MountDefs.GetScaleUnits(scaleStr);
			if(mountNum!=0) {
				//Scale info for a mount is stored as an ImageDraw of type ScaleValue that contains the scale, number of decimals, and units.
				List<ImageDraw> listImageDraws=ImageDraws.RefreshForDoc(document.DocNum);
				ImageDraw imageDrawScale=listImageDraws.FirstOrDefault(x => x.DrawType==ImageDrawType.ScaleValue);
				if(imageDrawScale!=null) {
					scaleValue=MountDefs.GetScale(imageDrawScale.DrawingSegment);
					decimals=MountDefs.GetDecimals(imageDrawScale.DrawingSegment);
					unitsStr=MountDefs.GetScaleUnits(imageDrawScale.DrawingSegment);
				}
			}
			if(scaleValue==0) {
				return "";
			}
			float lengthScaled=lengthPixels/scaleValue;
			drawText=lengthScaled.ToString("f"+decimals.ToString());
			if(!unitsStr.IsNullOrEmpty()){
				drawText+=" "+unitsStr;
			}
			return drawText;
		}

		private void AlertUserOfError(string description,string details="") {
			AlertItems.CreateGenericAlert(description,details);
		}
	}
}

