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
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormSheetFieldEditPatImage:FormODBase {
	///<summary>This is the object we are editing.</summary>
	public SheetField SheetFieldCur;
	public Sheet SheetCur;
	///<summary>The Y value to limit placement of PatImage to, should be set by caller.</summary>
	public int BottomYLimit;

	public FormSheetFieldEditPatImage() {
		InitializeComponent();
	}

	private void FormSheetFieldPatImage_Load(object sender,EventArgs e) {
		if(BottomYLimit==0) {
			BottomYLimit=SheetCur.Height;
		}
		FillFields();
		textXPos.Text=SheetFieldCur.XPos.ToString();
		textYPos.Text=SheetFieldCur.YPos.ToString();
		textWidth.Text=SheetFieldCur.Width.ToString();
		textHeight.Text=SheetFieldCur.Height.ToString();
	}

	private void FillFields(){
		textFieldValueDoc.Text="";
		textFieldValueMount.Text="";
		if(SheetFieldCur.FieldValue.StartsWith("MountNum:")){
			var mountNum=SIn.Long(SheetFieldCur.FieldValue.Substring(9));
			var mount=Mounts.GetByNum(mountNum);
			textFieldValueMount.Text=mount.DateCreated.ToShortDateString()+" "+mount.Description;
		}
		else if(SheetFieldCur.FieldValue!=""){
			var docNum=SIn.Long(SheetFieldCur.FieldValue);
			var document=Documents.GetByNum(docNum);
			textFieldValueDoc.Text=document.DateCreated.ToShortDateString()+" "+document.Description;
		}
	}

	private void butChange_Click(object sender, EventArgs e){
		//MsgBox.Show(sheetField.FieldValue);
		using var formImagePickerPatient=new FormImagePickerPatient();
		var patient=Patients.GetPat(SheetCur.PatNum);
		formImagePickerPatient.PatientCur=patient;
		if(SheetFieldCur.FieldValue.StartsWith("MountNum:")){
			var mountNum=SIn.Long(SheetFieldCur.FieldValue.Substring(9));
			formImagePickerPatient.MountNumSelected=mountNum;
		}
		else if(SheetFieldCur.FieldValue!=""){
			var docNum=SIn.Long(SheetFieldCur.FieldValue);
			formImagePickerPatient.DocNumSelected=docNum;
		}
		formImagePickerPatient.ShowDialog();
		if(formImagePickerPatient.DialogResult!=DialogResult.OK){
			return;
		}
		if(formImagePickerPatient.DocNumSelected>0){
			var docNumSelected=formImagePickerPatient.DocNumSelected;
			SheetFieldCur.FieldValue=docNumSelected.ToString();
			SheetFieldCur.FieldName=Documents.GetByNum(docNumSelected).DocCategory.ToString();//Returns new document if docnum is not found, so no need to check for null
		}
		if(formImagePickerPatient.MountNumSelected>0){
			var mountNumSelected=formImagePickerPatient.MountNumSelected;
			SheetFieldCur.FieldValue="MountNum:"+mountNumSelected;
			SheetFieldCur.FieldName=Mounts.GetByNum(mountNumSelected).DocCategory.ToString();//Returns new mount if mountnum is not found so no need to check for null
		}
		FillFields();
	}

	private void butDelete_Click(object sender,EventArgs e) {
		if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete?")){
			return;
		}
		SheetFieldCur=null;
		DialogResult=DialogResult.OK;
	}

	private void butSave_Click(object sender,EventArgs e) {
		//The maximum y-value of the sheet field must be within the sheet vertically.
		textYPos.MaxVal=BottomYLimit-SIn.Int(textHeight.Text);
		if(!textXPos.IsValid()
		   || !textYPos.IsValid()
		   || !textWidth.IsValid()
		   || !textHeight.IsValid())
		{
			MsgBox.Show(this,"Please fix data entry errors first.");
			return;
		}
		SheetFieldCur.XPos=SIn.Int(textXPos.Text);
		SheetFieldCur.YPos=SIn.Int(textYPos.Text);
		SheetFieldCur.Width=SIn.Int(textWidth.Text);
		SheetFieldCur.Height=SIn.Int(textHeight.Text);
		//don't save to database here.
		SheetFieldCur.IsNew=false;
		DialogResult=DialogResult.OK;
	}

}