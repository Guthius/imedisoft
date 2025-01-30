using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental;

public partial class FormSheetFieldAdd:FormODBase {
	public Sheet SheetCur;

	public FormSheetFieldAdd() {
		InitializeComponent();
	}

	private void FormSheetFieldAdd_Load(object sender,EventArgs e) {
		var listSheetFieldTypes=SheetDefs.GetVisibleButtons(SheetCur.SheetType);
		butPatImage.Visible=listSheetFieldTypes.Contains(SheetFieldType.PatImage);
	}

	private void butPatImage_Click(object sender,EventArgs e) {
		if(false) {
			MsgBox.Show(this,"Not allowed because not using AtoZ folder");
			return;
		}
		if(SheetCur.PatNum==0) {
			MsgBox.Show(this,"Not allowed to add a patient image to an anonymous patient.");
			return;
		}
		//Font font=new Font(SheetDefCur.FontName,SheetDefCur.FontSize);
		using var formSheetFieldEditPatImage=new FormSheetFieldEditPatImage();
		formSheetFieldEditPatImage.SheetCur=SheetCur;
		var sheetField=new SheetField();
		sheetField.IsNew=true;
		sheetField.FieldType=SheetFieldType.PatImage;
		sheetField.FieldName="";
		sheetField.FieldValue="";
		sheetField.SheetNum=SheetCur.SheetNum;
		sheetField.XPos=0;
		sheetField.YPos=0;
		sheetField.Width=100;
		sheetField.Height=100;
		formSheetFieldEditPatImage.SheetFieldCur=sheetField;
		formSheetFieldEditPatImage.ShowDialog();
		if(formSheetFieldEditPatImage.DialogResult!=DialogResult.OK  || formSheetFieldEditPatImage.SheetFieldCur==null) {//SheetFieldCur==null if it was Deleted
			return;
		}
		SheetCur.SheetFields.Add(sheetField);
		DialogResult=DialogResult.OK;
	}

}