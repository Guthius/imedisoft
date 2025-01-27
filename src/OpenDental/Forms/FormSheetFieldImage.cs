using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;

namespace OpenDental {
	public partial class FormSheetFieldImage:FormODBase {
		///<summary>This is the object we are editing.</summary>
		public SheetFieldDef SheetFieldDefCur;
		///<summary>We need access to a few other fields of the sheetDef.</summary>
		public SheetDef SheetDefCur;
		///<summary>Ignored. Available for mobile but all fields are relevant</summary>
		public bool IsEditMobile;
		public bool IsReadOnly;

		public FormSheetFieldImage() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormSheetFieldImage_Load(object sender,EventArgs e) {
			textYPos.MaxVal=SheetDefCur.HeightTotal-1;//The maximum y-value of the sheet field must be within the page vertically.
			if(IsReadOnly){
				butSave.Enabled=false;
				butDelete.Enabled=false;
			}
			FillCombo();
			comboFieldName.Text=SheetFieldDefCur.FieldName;
			FillImage();
			textXPos.Text=SheetFieldDefCur.XPos.ToString();
			textYPos.Text=SheetFieldDefCur.YPos.ToString();
			textWidth.Text=SheetFieldDefCur.Width.ToString();
			textHeight.Text=SheetFieldDefCur.Height.ToString();
		}

		private void FillCombo(){
			if(true) {
				comboFieldName.Items.Clear();
				string[] files=null;
				if(true) {
					files=Directory.GetFiles(SheetUtil.GetImagePath());
				}
				for(int i=0;i<files.Length;i++) {
					//remove some common offending file types (non image files)
					if(files[i].EndsWith("db")
						||files[i].EndsWith("doc")
						||files[i].EndsWith("pdf"))
					{
						continue;
					}
					comboFieldName.Items.Add(Path.GetFileName(files[i]));
				}
				//comboFieldName.Items.Add("Patient Info.gif");
			}
		}

		private void butImport_Click(object sender,EventArgs e) {
			string importFilePath;
			if(!false && false) {
				importFilePath=ODCloudClient.ImportFileForCloud();
				if(importFilePath.IsNullOrEmpty()){
					return;
				}
			}
			else {
				using OpenFileDialog dialogOpenFile=new OpenFileDialog();
				dialogOpenFile.Multiselect=false;
				if(dialogOpenFile.ShowDialog()!=DialogResult.OK){
					return;
				}
				if(!File.Exists(dialogOpenFile.FileName)){
					MsgBox.Show(this,"File does not exist.");
					return;
				}
				importFilePath=dialogOpenFile.FileName;
			}
			if(!ImageHelper.HasImageExtension(importFilePath)){
				MsgBox.Show(this,"Only allowed to import an image.");
				return;
			}
			string newName=importFilePath;
			if(true) {
				newName=ODFileUtils.CombinePaths(SheetUtil.GetImagePath(),Path.GetFileName(importFilePath));
				if(File.Exists(newName)) {
					MsgBox.Show(this,"A file of that name already exists in SheetImages.  Please rename the file before importing.");
					return;
				}
				File.Copy(importFilePath,newName);
			}

			FillCombo();
			for(int i=0;i<comboFieldName.Items.Count;i++){
				if(comboFieldName.Items[i].ToString()==Path.GetFileName(newName)){
					comboFieldName.SelectedIndex=i;
					comboFieldName.Text=Path.GetFileName(newName);
					FillImage();
					ShrinkToFit();
				}
			}
		}

		private void comboFieldName_TextUpdate(object sender,EventArgs e) {
			FillImage();
			ShrinkToFit();
		}

		private void comboFieldName_SelectionChangeCommitted(object sender,EventArgs e) {
			comboFieldName.Text=comboFieldName.SelectedItem.ToString();
			FillImage();
			ShrinkToFit();
		}

		private void FillImage(){
			if(comboFieldName.Text=="") {
				return;
			}
			textFullPath.Text=ODFileUtils.CombinePaths(SheetUtil.GetImagePath(),comboFieldName.Text);
			if(File.Exists(textFullPath.Text)){
				GC.Collect();
				try {
					pictureBox.Image=Image.FromFile(textFullPath.Text);
				}
				catch {
					pictureBox.Image=null;
					MsgBox.Show(this,"Invalid image type.");
				}
			}
			else if(comboFieldName.Text=="Patient Info.gif") {//Interal image
				pictureBox.Image=OpenDentBusiness.Properties.Resources.Patient_Info;
				textFullPath.Text="Patient Info.gif (internal)";
			}
			else
			{
				pictureBox.Image=null;
			}

			if(pictureBox.Image==null) {
				textWidth2.Text="";
				textHeight2.Text="";
			}
			else {
				textWidth2.Text=pictureBox.Image.Width.ToString();
				textHeight2.Text=pictureBox.Image.Height.ToString();
			}
		}

		private void butShrink_Click(object sender,EventArgs e) {
			ShrinkToFit();
		}

		private void ShrinkToFit(){
			if(pictureBox.Image==null){
				return;
			}
			if(pictureBox.Image.Width>SheetDefCur.Width || pictureBox.Image.Height>SheetDefCur.Height){//image would be too big
				float ratioImgWtoH=((float)pictureBox.Image.Width)/((float)pictureBox.Image.Height);
				float ratioSheetWtoH=((float)SheetDefCur.Width)/((float)SheetDefCur.Height);
				float ratioNew;
				int newW;
				int newH;
				if(ratioImgWtoH < ratioSheetWtoH){//image is tall and skinny
					ratioNew=((float)SheetDefCur.Height)/((float)pictureBox.Image.Height);//restrict by height
					newW=(int)(((float)pictureBox.Image.Width)*ratioNew);
					newH=(int)(((float)pictureBox.Image.Height)*ratioNew);
					textWidth.Text=newW.ToString();
					textHeight.Text=newH.ToString();
				}
				else{//image is short and fat
					ratioNew=((float)SheetDefCur.Width)/((float)pictureBox.Image.Width);//restrict by width
					newW=(int)(((float)pictureBox.Image.Width)*ratioNew);
					newH=(int)(((float)pictureBox.Image.Height)*ratioNew);
					textWidth.Text=newW.ToString();
					textHeight.Text=newH.ToString();
				}
			}
			else{
				textWidth.Text=pictureBox.Image.Width.ToString();
				textHeight.Text=pictureBox.Image.Height.ToString();
			}
		}

		private void textWidth_KeyUp(object sender,KeyEventArgs e) {
			if(!checkRatio.Checked){
				return;
			}
			if(pictureBox.Image==null){
				return;
			}
			float w;
			try{
				w=PIn.Float(textWidth.Text);
			}
			catch{
				return;
			}
			float ratioImgWtoH=((float)pictureBox.Image.Width)/((float)pictureBox.Image.Height);
			int newH=(int)(w/ratioImgWtoH);
			textHeight.Text=newH.ToString();
		}

		private void textHeight_KeyUp(object sender,KeyEventArgs e) {
			if(!checkRatio.Checked){
				return;
			}
			if(pictureBox.Image==null){
				return;
			}
			float h;
			try{
				h=PIn.Float(textHeight.Text);
			}
			catch{
				return;
			}
			float ratioImgWtoH=((float)pictureBox.Image.Width)/((float)pictureBox.Image.Height);
			int newW=(int)(h*ratioImgWtoH);
			textWidth.Text=newW.ToString();
		}

		private void butDelete_Click(object sender,EventArgs e) {
			SheetFieldDefCur.ImageField?.Dispose();
			SheetFieldDefCur=null;
			DialogResult=DialogResult.OK;
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(!textXPos.IsValid()
				|| !textYPos.IsValid()
				|| !textWidth.IsValid()
				|| !textHeight.IsValid())
			{
				MsgBox.Show(this,"Please fix data entry errors first.");
				return;
			}
			if(comboFieldName.Text==""){
				MsgBox.Show(this,"Please enter a file name first.");
				return;
			}
			if(pictureBox.Image==null) {
				if(comboFieldName.Text=="Patient Info.gif") {
					pictureBox.Image=OpenDentBusiness.Properties.Resources.Patient_Info;
				}
				else if(true) {
					GC.Collect();
					if(!File.Exists(textFullPath.Text)) {
						MsgBox.Show(this,"Image file does not exist.");
						return;
					}
					try {//catch valid files that are not valid images.
						pictureBox.Image=Image.FromFile(textFullPath.Text);
					}
					catch {
						MsgBox.Show(this,"Not a valid image type.");
						return;
					}
				}
			}
			SheetFieldDefCur.FieldName=comboFieldName.Text;
			SheetFieldDefCur.ImageField?.Dispose();//To prevent memory leaks
			//Make a copy of pictureBox.Image using the intended dimensions, to conserve memory.
			SheetFieldDefCur.XPos=PIn.Int(textXPos.Text);
			SheetFieldDefCur.YPos=PIn.Int(textYPos.Text);
			SheetFieldDefCur.Width=PIn.Int(textWidth.Text);
			SheetFieldDefCur.Height=PIn.Int(textHeight.Text);
			SheetFieldDefCur.ImageField=new Bitmap(pictureBox.Image,SheetFieldDefCur.Width,SheetFieldDefCur.Height);
			//don't save to database here.
			SheetFieldDefCur.IsNew=false;
			DialogResult=DialogResult.OK;
		}

		private void FormSheetFieldImage_FormClosing(object sender,FormClosingEventArgs e) {
			pictureBox.Image?.Dispose();
		}

	}
}