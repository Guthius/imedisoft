using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.Forms;

namespace OpenDental;

public partial class FormDefEditWebSchedApptTypes:FormODBase {
	private Def _def;
	///<summary>Every Web Sched reason is required to be associated to one (and only one) appointment type.
	///This is where the length and procedures of the appointment are retrieved.</summary>
	private AppointmentType _appointmentType;
	///<summary>The blockout types that this WSNPA reason is restricted to.  Can be empty.</summary>
	private List<Def> _listDefsRestrictToBlockoutTypes;
	///<summary>Set within class only.</summary>
	public bool IsDeleted;

	public FormDefEditWebSchedApptTypes(Def defCur,string title) {
		InitializeComponent();

		this.Text=title;
		_def=defCur;
		checkHidden.Checked=_def.IsHidden;
		textName.Text=_def.ItemName;
		//Look for an associated appointment type.
		var listDefLinksAppointmentType=DefLinks.GetDefLinksByType(DefLinkType.AppointmentType);
		var defLink=listDefLinksAppointmentType.FirstOrDefault(x => x.DefNum==_def.DefNum);
		if(defLink!=null) {
			_appointmentType=AppointmentTypes.GetFirstOrDefault(x => x.AppointmentTypeNum==defLink.FKey);
		}
		var listDefLinksBlockoutType=DefLinks.GetDefLinksByType(DefLinkType.BlockoutType,_def.DefNum);
		var listDefLinkFKeys=listDefLinksBlockoutType.Select(x => x.FKey).ToList();
		_listDefsRestrictToBlockoutTypes=Defs.GetDefs(DefCat.BlockoutTypes,listDefLinkFKeys);
		FillApptTypeValue();
		FillBlockoutTypeValues();
	}

	private void FillApptTypeValue() {
		textApptType.Clear();
		if(_appointmentType!=null) {
			textApptType.Text=_appointmentType.AppointmentTypeName;
		}
	}

	private void FillBlockoutTypeValues() {
		textRestrictToBlockouts.Clear();
		textRestrictToBlockouts.Text=string.Join(",",_listDefsRestrictToBlockoutTypes.Select(x => x.ItemName));
	}

	private void butSelect_Click(object sender,EventArgs e) {
		using var formApptTypes=new FormApptTypes();
		formApptTypes.IsSelectionMode=true;
		formApptTypes.SelectedAppointmentType=_appointmentType;
		if(formApptTypes.ShowDialog()!=DialogResult.OK) {
			return;
		}
		_appointmentType=formApptTypes.SelectedAppointmentType;
		FillApptTypeValue();
	}

	private void butColor_Click(object sender,EventArgs e) {
		colorDialog1.Color=butColor.BackColor;
		colorDialog1.ShowDialog();
		butColor.BackColor=colorDialog1.Color;
	}

	private void butSelectBlockouts_Click(object sender,EventArgs e) {
		using var formDefinitionPicker=new FormDefinitionPicker(DefCat.BlockoutTypes,_listDefsRestrictToBlockoutTypes);
		formDefinitionPicker.IsMultiSelectionMode=true;
		formDefinitionPicker.ShowDialog();
		if(formDefinitionPicker.DialogResult!=DialogResult.OK) {
			return;
		}
		_listDefsRestrictToBlockoutTypes=GenericTools.DeepCopy<List<Def>,List<Def>>(formDefinitionPicker.ListDefsSelected);
		FillBlockoutTypeValues();
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(string.IsNullOrEmpty(textName.Text.Trim())) {
			MsgBox.Show(this,"Reason required.");
			return;
		}
		if(_appointmentType==null) {
			MsgBox.Show(this,"Appointment Type required.");
			return;
		}
		_def.ItemName=SIn.String(textName.Text);
		if(_def.IsNew) {
			DefL.Insert(_def);
		}
		else {
			DefL.Update(_def);
		}
		DefLinks.SetFKeyForDef(_def.DefNum,_appointmentType.AppointmentTypeNum,DefLinkType.AppointmentType);
		DefLinks.DeleteAllForDef(_def.DefNum,DefLinkType.BlockoutType);//Remove all blockouts before inserting the new set
		var listDefNums=_listDefsRestrictToBlockoutTypes.Select(x => x.DefNum).ToList();
		DefLinks.InsertDefLinksForFKeys(_def.DefNum,listDefNums,DefLinkType.BlockoutType);
		DialogResult=DialogResult.OK;
	}

	private void butDelete_Click(object sender,EventArgs e) {
		var msgText="Are you sure you want to delete this definition? References to it will be deleted as well.";
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,msgText)){
			return;
		}
		try {
			Defs.Delete(_def);				
		}
		catch(ApplicationException ex) {
			ODMessageBox.Show(ex.Message);
		}
		//Web Sched New appointment type defs can be associated to multiple types of deflinks.  Clean them up.
		DefLinks.DeleteAllForDef(_def.DefNum);
		IsDeleted=true;
		DialogResult=DialogResult.OK;
	}

}