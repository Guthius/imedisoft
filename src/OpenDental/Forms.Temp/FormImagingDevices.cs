using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenDental;
using OpenDental.UI;
using OpenDentBusiness;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentalImaging;

public partial class FormImagingDevices:FormODBase {
	///<summary>Local copy of cached device list.</summary>
	private List<ImagingDevice> _listImagingDevices;
	private bool _changed;

	public FormImagingDevices() {
		InitializeComponent();
	}

	private void FormImagingDevices_Load(object sender, EventArgs e){
		if(!/* ODBuild.IsTrial() */ false
		   && !OpenDentBusiness.Help.IsEncryptedKeyValid())//always true in debug
		{
			MsgBox.Show(this,"This feature requires an active support plan.");
			Close();
			return;
		}
		textComputer.Text=Environment.MachineName;
		FillGrid();
	}

	private void FillGrid(){
		_listImagingDevices=ImagingDevices.GetDeepCopy();
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		var col=new GridColumn(Lan.g("TableImagingDevices","Description"),100);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableImagingDevices","Computer"),100);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableImagingDevices","Type"),100);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableImagingDevices","Twain Name"),200);
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		GridRow row;
		for(var i=0;i<_listImagingDevices.Count;i++){
			row=new GridRow();
			row.Cells.Add(_listImagingDevices[i].Description);
			row.Cells.Add(_listImagingDevices[i].ComputerName);
			row.Cells.Add(_listImagingDevices[i].DeviceType.ToString());
			row.Cells.Add(_listImagingDevices[i].TwainName);
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		using var formImagingDeviceEdit=new FormImagingDeviceEdit();
		formImagingDeviceEdit.ImagingDeviceCur=_listImagingDevices[e.Row];
		formImagingDeviceEdit.ShowDialog();
		if(formImagingDeviceEdit.DialogResult!=DialogResult.OK){
			return;
		}
		_changed=true;
		ImagingDevices.RefreshCache();
		FillGrid();
	}

	private void butAdd_Click(object sender, EventArgs e){
		using var formImagingDeviceEdit=new FormImagingDeviceEdit();
		formImagingDeviceEdit.ImagingDeviceCur=new ImagingDevice();
		formImagingDeviceEdit.ImagingDeviceCur.IsNew=true;
		formImagingDeviceEdit.ShowDialog();
		if(formImagingDeviceEdit.DialogResult!=DialogResult.OK){
			return;
		}
		_changed=true;
		ImagingDevices.RefreshCache();
		FillGrid();
	}

	private void butUp_Click(object sender, EventArgs e){
		var idxSelected=gridMain.GetSelectedIndex();
		if(idxSelected==-1) {
			MsgBox.Show(this,"Please select an item first.");
			return;
		}
		if(idxSelected==0) {//at top
			return;
		}
		var imagingDevice=_listImagingDevices[idxSelected];
		imagingDevice.ItemOrder--;
		ImagingDevices.Update(imagingDevice);
		var imagingDeviceAbove=_listImagingDevices[idxSelected-1];
		imagingDeviceAbove.ItemOrder++;
		ImagingDevices.Update(imagingDeviceAbove);
		ImagingDevices.RefreshCache();
		FillGrid();
		gridMain.SetSelected(idxSelected-1);
		_changed=true;
	}

	private void butDown_Click(object sender, EventArgs e){
		var idxSelected=gridMain.GetSelectedIndex();
		if(idxSelected==-1) {
			MsgBox.Show(this,"Please select an item first.");
			return;
		}
		if(idxSelected==_listImagingDevices.Count-1) {//at bottom
			return;
		}
		var imagingDevice=_listImagingDevices[idxSelected];
		imagingDevice.ItemOrder++;
		ImagingDevices.Update(imagingDevice);
		var imagingDeviceBelow=_listImagingDevices[idxSelected+1];
		imagingDeviceBelow.ItemOrder--;
		ImagingDevices.Update(imagingDeviceBelow);
		ImagingDevices.RefreshCache();
		FillGrid();
		gridMain.SetSelected(idxSelected+1);
		_changed=true;
	}

	private void FormImagingDevices_FormClosing(object sender,FormClosingEventArgs e) {
		if(_changed){
			DataValid.SetInvalid(InvalidType.ToolButsAndMounts);
		}
	}

}