using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormSheetFieldLine:FormODBase {
	///<summary>This is the object we are editing.</summary>
	public SheetFieldDef SheetFieldDefCur;
	///<summary>We need access to a few other fields of the sheetDef.</summary>
	public SheetDef SheetDefCur;
	///<summary>Ignored. Available for mobile but all fields are relevant</summary>
	public bool IsEditMobile;
	public bool IsReadOnly;

	public FormSheetFieldLine() {
		InitializeComponent();
	}

	private void FormSheetFieldLine_Load(object sender,EventArgs e) {
		textYPos.MaxVal=SheetDefCur.HeightTotal-1;//The maximum y-value of the sheet field must be within the page vertically.
		if(IsReadOnly){
			butSave.Enabled=false;
			butDelete.Enabled=false;
		}
		if(SheetDefCur.SheetType!=SheetTypeEnum.Statement) {
			checkPmtOpt.Visible=false;
		}
		textXPos.Text=SheetFieldDefCur.XPos.ToString();
		textYPos.Text=SheetFieldDefCur.YPos.ToString();
		textWidth.Text=SheetFieldDefCur.Width.ToString();
		textHeight.Text=SheetFieldDefCur.Height.ToString();
		checkPmtOpt.Checked=SheetFieldDefCur.IsPaymentOption;
		butColor.BackColor=SheetFieldDefCur.ItemColor;
	}

	private void butColor_Click(object sender,EventArgs e) {
		using var colorDialog1=new ColorDialog();
		colorDialog1.Color=butColor.BackColor;
		colorDialog1.ShowDialog();
		butColor.BackColor=colorDialog1.Color;
	}

	private void butDelete_Click(object sender,EventArgs e) {
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
		SheetFieldDefCur.XPos=SIn.Int(textXPos.Text);
		SheetFieldDefCur.YPos=SIn.Int(textYPos.Text);
		SheetFieldDefCur.Width=SIn.Int(textWidth.Text);
		SheetFieldDefCur.Height=SIn.Int(textHeight.Text);
		SheetFieldDefCur.IsPaymentOption=checkPmtOpt.Checked;
		SheetFieldDefCur.ItemColor=butColor.BackColor;
		//don't save to database here.
		SheetFieldDefCur.IsNew=false;
		DialogResult=DialogResult.OK;
	}

}