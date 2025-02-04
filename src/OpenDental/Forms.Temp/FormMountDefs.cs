using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

/// <summary></summary>
public partial class FormMountDefs : FormODBase {
	private bool _isChanged;
	private List<MountDef> _listMountDefs;

		
	public FormMountDefs()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormMountDefs_Load(object sender, System.EventArgs e) {
		FillList();
	}

	private void FillList(){
		MountDefs.RefreshCache();
		listBoxMain.Items.Clear();
		_listMountDefs=MountDefs.GetDeepCopy();
		for(var i=0;i<_listMountDefs.Count;i++){
			if(_listMountDefs[i].ItemOrder!=i){
				_listMountDefs[i].ItemOrder=i;
				MountDefs.Update(_listMountDefs[i]);
				_isChanged=true;
			}
			listBoxMain.Items.Add(_listMountDefs[i].Description);
		}
	}

	private void butAdd_Click(object sender, System.EventArgs e) {
		var mountDef=new MountDef();
		mountDef.IsNew=true;
		mountDef.Description="Mount";
		mountDef.Width=600;
		mountDef.Height=400;
		if(_listMountDefs.Count>0){
			mountDef.ItemOrder=_listMountDefs.Count;
		}
		MountDefs.Insert(mountDef);//Insert mount here instead of inside edit window so that we have an object to add items to
		using var formMountDefEdit=new FormMountDefEdit();
		formMountDefEdit.MountDefCur=mountDef;
		formMountDefEdit.ShowDialog();
		FillList();
		_isChanged=true;
	}

	private void listMain_DoubleClick(object sender, System.EventArgs e) {
		if(listBoxMain.SelectedIndex==-1){
			return;
		}
		using var formMountDefEdit=new FormMountDefEdit();
		formMountDefEdit.MountDefCur=_listMountDefs[listBoxMain.SelectedIndex];
		formMountDefEdit.ShowDialog();
		FillList();
		_isChanged=true;
	}

	private void butUp_Click(object sender,EventArgs e) {
		var selectedIdx=listBoxMain.SelectedIndex;
		if(selectedIdx==-1) {
			return;
		}
		if(selectedIdx==0) {//at top
			return;
		}
		var mountDef=_listMountDefs[selectedIdx];
		mountDef.ItemOrder--;
		MountDefs.Update(mountDef);
		var mountDefAbove=_listMountDefs[selectedIdx-1];
		mountDefAbove.ItemOrder++;
		MountDefs.Update(mountDefAbove);
		FillList();
		listBoxMain.SelectedIndex=selectedIdx-1;
		_isChanged=true;
	}

	private void butDown_Click(object sender,EventArgs e) {
		var selectedIdx=listBoxMain.SelectedIndex;
		if(selectedIdx==-1) {
			return;
		}
		if(selectedIdx==_listMountDefs.Count-1) {//at bottom
			return;
		}
		var mountDef=_listMountDefs[selectedIdx];
		mountDef.ItemOrder++;
		MountDefs.Update(mountDef);
		var mountDefBelow=_listMountDefs[selectedIdx+1];
		mountDefBelow.ItemOrder--;
		MountDefs.Update(mountDefBelow);
		FillList();
		listBoxMain.SelectedIndex=selectedIdx+1;
		_isChanged=true;
	}

	private void FormMounts_FormClosing(object sender,FormClosingEventArgs e) {
		if(_isChanged) {
			DataValid.SetInvalid(InvalidType.ToolButsAndMounts);
		}
	}

}