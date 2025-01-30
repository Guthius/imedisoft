using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

/// <summary></summary>
public partial class FormLabTurnaroundEdit : FormODBase {
	public LabTurnaround LabTurnaroundCur;

		
	public FormLabTurnaroundEdit()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormLabTurnaroundEdit_Load(object sender,EventArgs e) {
		textDescription.Text=LabTurnaroundCur.Description;
		if(LabTurnaroundCur.DaysPublished>0){
			textDaysPublished.Text=LabTurnaroundCur.DaysPublished.ToString();//otherwise, blank
		}
		textDaysActual.Text=LabTurnaroundCur.DaysActual.ToString();
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(!textDaysActual.IsValid() || !textDaysPublished.IsValid()) {
			MsgBox.Show(this,"Please fix data entry errors first.");
			return;
		}
		if(SIn.Long(textDaysActual.Text)==0){
			MsgBox.Show(this,"Actual Days cannot be zero.");
			return;
		}
		if(textDescription.Text==""){
			MsgBox.Show(this,"Please enter a description.");
			return;
		}
		LabTurnaroundCur.Description=textDescription.Text;
		LabTurnaroundCur.DaysPublished=SIn.Int(textDaysPublished.Text);
		LabTurnaroundCur.DaysActual=SIn.Int(textDaysActual.Text);
		DialogResult=DialogResult.OK;
	}

}