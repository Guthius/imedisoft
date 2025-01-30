using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using System.Globalization;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormInsCatEdit:FormODBase {
		
	public bool IsNew;
	///<summary>A list of CovSpans just for this category</summary>
	//private CovSpan[] CovSpanList;
	private CovCat _covCat;

		
	public FormInsCatEdit(CovCat covCat) {
		InitializeComponent();
		_covCat=covCat.Copy();
			
	}

	protected override string GetHelpOverride() {
		if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
			return "FormInsCatEditCanada";
		}
		return "FormInsCatEdit";
	}

	private void FormInsCatEdit_Load(object sender,System.EventArgs e) {
		textDescription.Text=_covCat.Description;
		if(_covCat.DefaultPercent==-1)
			textPercent.Text="";
		else
			textPercent.Text=_covCat.DefaultPercent.ToString();
		textPercent.MaxVal=100;
		checkHidden.Checked=_covCat.IsHidden;
		for(var i=0;i<Enum.GetNames(typeof(EbenefitCategory)).Length;i++){
			comboCat.Items.Add(Enum.GetNames(typeof(EbenefitCategory))[i]);
			if(Enum.GetNames(typeof(EbenefitCategory))[i]==_covCat.EbenefitCat.ToString()){
				comboCat.SelectedIndex=i;
			}
		}
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(!textPercent.IsValid())
			//|| !textPriBasicPercent.IsValid()
		{
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		_covCat.Description=textDescription.Text;
		if(textPercent.Text=="") {
			_covCat.DefaultPercent=-1;
		}
		else {
			_covCat.DefaultPercent=SIn.Int(textPercent.Text);
		}
		_covCat.IsHidden=checkHidden.Checked;
		_covCat.EbenefitCat=(EbenefitCategory)comboCat.SelectedIndex;
		if(IsNew){
			CovCats.Insert(_covCat);
		}
		else{
			CovCats.Update(_covCat);
		}
		DialogResult=DialogResult.OK;
	}

}