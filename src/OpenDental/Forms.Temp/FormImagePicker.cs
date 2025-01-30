using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.IO;
using System.Linq;
using CodeBase;

namespace OpenDental;

///<summary>This image picker shows all files within a specified Windows folder and lets you pick one.  Used for Wiki and email templates.</summary>
public partial class FormImagePicker:FormODBase {
	///<summary>This contains the entire qualified names including path and extension.</summary>
	private List<string> _listImageNames;
	public string ImageNameSelected;
	private string _imageFolder;

	///<summary>Check that the imageFolder exists and is accessible before calling this form.</summary>
	public FormImagePicker(string imageFolder) {
		InitializeComponent();
		_imageFolder=imageFolder;
			
	}

	private void FormImagePicker_Load(object sender,EventArgs e) {
		FillGrid();
	}

	/// <summary></summary>
	private void FillGrid(string fileNameToSelect=null) {
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		var col=new GridColumn(Lan.g(this,"Image Name"),70);
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		List<string> listFileNames=null;
		try {
			listFileNames=Directory.GetFiles(_imageFolder).ToList();//All files from the wiki file path, including images and other files.
		}
		catch(Exception ex) {
			ODMessageBox.Show(this,ex.Message);
			DialogResult=DialogResult.Cancel;
			return;
		}
		_listImageNames= [];
		for(var i=0;i<listFileNames.Count;i++) {
			//If the user has entered a search keyword, then only show file names which contain the keyword.
			if(textSearch.Text!="" && !Path.GetFileName(listFileNames[i]).ToLower().Contains(textSearch.Text.ToLower())) {
				continue;
			}
			//Only add image files to the ImageNamesList, not other files such at text files.
			if(ImageHelper.HasImageExtension(listFileNames[i])) {
				_listImageNames.Add(listFileNames[i]);
			}
		}
		var index=-1;
		for(var i=0;i<_listImageNames.Count;i++) {
			var row=new GridRow();
			var fileName=Path.GetFileName(_listImageNames[i]);
			row.Cells.Add(fileName);
			if(fileNameToSelect!=null && fileName==fileNameToSelect) {
				index=i;
			}
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
		labelImageSize.Text=Lan.g(this,"Image Size")+":";
		picturePreview.Image=null;
		picturePreview.Invalidate();
		if(index>-1) {//if importing exactly one image, select it upon returning.
			gridMain.SetSelected(index);
			paintPreviewPicture();
		}
	}

	private void gridMain_CellClick(object sender,ODGridClickEventArgs e) {
		paintPreviewPicture();
	}

	private void paintPreviewPicture() {
		if(gridMain.GetSelectedIndex()==-1) {
			return;
		}
		var imagePath=_listImageNames[gridMain.GetSelectedIndex()];
		Image imageTmp=FileAtoZ.GetImage(imagePath);//Could throw an exception if someone deletes the image right after this window loads.
		ODImaging.ImageApplyOrientation(imageTmp);
		picturePreview.Image?.Dispose();
		picturePreview.Image=ODImaging.ImageScaleMaxHeightAndWidth(imageTmp,picturePreview.Height,picturePreview.Width);
		labelImageSize.Text=Lan.g(this,"Image Size")+": "+(int)imageTmp.PhysicalDimension.Width+" x "+(int)imageTmp.PhysicalDimension.Height;
		picturePreview.Invalidate();
		imageTmp?.Dispose();
	}

	private void FormWikiImages_ResizeEnd(object sender,EventArgs e) {
		paintPreviewPicture();
	}

	private void butImport_Click(object sender,EventArgs e) {
		string[] stringArrayFileNames;
		using var openFileDialog=new OpenFileDialog();
		openFileDialog.Multiselect=true;
		if(openFileDialog.ShowDialog()!=DialogResult.OK) {
			return;
		}
		stringArrayFileNames=openFileDialog.FileNames;
		Invalidate();
		for(var i=0;i<stringArrayFileNames.Length;i++) {
			//check file types?
			var destinationPath=Path.Combine(_imageFolder,Path.GetFileName(stringArrayFileNames[i]));
			if(!File.Exists(destinationPath)){
				File.Copy(stringArrayFileNames[i],destinationPath);
				continue;
			}
			//from here down, file already exists
			var inputBoxParam=new InputBoxParam();
			inputBoxParam.InputBoxType_=InputBoxType.TextBox;
			inputBoxParam.LabelText=Lan.g(this,"New file name.");
			inputBoxParam.Text=Path.GetFileName(stringArrayFileNames[i]);
			var inputBox=new InputBox(inputBoxParam);
			inputBox.ShowDialog();
			if(inputBox.IsDialogCancel) {
				continue;//cancel, next file.
			}
			var isCancel=false;
			var stringResult=inputBox.StringResult;
			while(true){
				if(!string.IsNullOrWhiteSpace(stringResult) && !File.Exists(Path.Combine(_imageFolder,stringResult))){
					break;
				}
				MsgBox.Show(this,"File name cannot be blank or in use.");
				var inputBoxRefresh=new InputBox(inputBoxParam);
				inputBoxRefresh.ShowDialog();
				stringResult=inputBoxRefresh.StringResult;
				if(inputBoxRefresh.IsDialogCancel) {
					isCancel=true;
					break;
				}
			}
			if(isCancel) {
				continue;//cancel rename, and go to next file.
			}
			destinationPath=Path.Combine(_imageFolder,stringResult);
			FileAtoZ.Copy(stringArrayFileNames[i],destinationPath);
			stringArrayFileNames[i]=stringResult;
		}
		string fileName=null;
		if(stringArrayFileNames.Length==1) {//if importing exactly one image, select it upon returning.
			fileName=Path.GetFileName(stringArrayFileNames[0]);
		}
		FillGrid(fileNameToSelect:fileName);
	}

	private void textSearch_TextChanged(object sender,EventArgs e) {
		FillGrid();
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(gridMain.GetSelectedIndex()==-1) {
			return;
		}
		ImageNameSelected=Path.GetFileName(_listImageNames[gridMain.GetSelectedIndex()]);
		DialogResult=DialogResult.OK;
	}

	private void butOK_Click(object sender,EventArgs e) {
		if(gridMain.GetSelectedIndex()==-1) {
			return;
		}
		ImageNameSelected=Path.GetFileName(_listImageNames[gridMain.GetSelectedIndex()]);
		DialogResult=DialogResult.OK;
	}

}