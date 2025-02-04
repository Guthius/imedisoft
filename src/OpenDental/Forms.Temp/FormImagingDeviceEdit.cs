using System;
using System.Text;
using System.Windows.Forms;
using OpenDental;
using CodeBase;
using ImagingDeviceManager;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentalImaging;

public partial class FormImagingDeviceEdit:FormODBase {
	public ImagingDevice ImagingDeviceCur;

	public FormImagingDeviceEdit() {
		InitializeComponent();
	}

	private void FormImagingDeviceEdit_Load(object sender, EventArgs e){
		textDescription.Text=ImagingDeviceCur.Description;
		textComputerName.Text=ImagingDeviceCur.ComputerName;
		if(ImagingDeviceCur.DeviceType.In(EnumImgDeviceType.TwainRadiograph,EnumImgDeviceType.XDR)){
			radioTwain.Checked=true;
		}
		if(ImagingDeviceCur.DeviceType==EnumImgDeviceType.TwainMulti){
			radioTwainMulti.Checked=true;
		}
		comboTwainName.Text=ImagingDeviceCur.TwainName;
		checkShowTwainUI.Checked=ImagingDeviceCur.ShowTwainUI;
	}

	private void butThis_Click(object sender,EventArgs e) {
		textComputerName.Text=Environment.MachineName;
	}

	private void comboTwainName_DropDown(object sender,EventArgs e) {
		try {
			Twain.ActivateEZTwain();
		}
		catch {
			Cursor=Cursors.Default;
			MsgBox.Show(this,"EzTwain4.dll not found.  Please run the setup file in your images folder.");
			return;
		}
		comboTwainName.Items.Clear();
		if(!EZTwain.GetSourceList()) {
			return;
		}
		var stringBuilder=new StringBuilder();
		stringBuilder.EnsureCapacity(64);
		while(EZTwain.GetNextSourceName(stringBuilder)) {
			comboTwainName.Items.Add(stringBuilder.ToString());
			stringBuilder.EnsureCapacity(64);
		}
	}

	private void butDelete_Click(object sender,EventArgs e) {
		if(ImagingDeviceCur.IsNew){
			DialogResult=DialogResult.Cancel;
			return;
		}
		if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete?")){
			return;
		}
		ImagingDevices.Delete(ImagingDeviceCur.ImagingDeviceNum);
		DialogResult=DialogResult.OK;
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(textDescription.Text==""){
			MsgBox.Show(this,"Please enter a description.");
			return;
		}
		ImagingDeviceCur.Description=textDescription.Text;
		ImagingDeviceCur.ComputerName=textComputerName.Text;
		ImagingDeviceCur.DeviceType=EnumImgDeviceType.TwainRadiograph;
		if(radioTwainMulti.Checked){
			ImagingDeviceCur.DeviceType=EnumImgDeviceType.TwainMulti;
		}
		ImagingDeviceCur.TwainName=comboTwainName.Text;
		ImagingDeviceCur.ShowTwainUI=checkShowTwainUI.Checked;
		if(ImagingDeviceCur.IsNew){
			ImagingDevices.Insert(ImagingDeviceCur);
		}
		else{
			ImagingDevices.Update(ImagingDeviceCur);
		}
		DialogResult=DialogResult.OK;
	}

}