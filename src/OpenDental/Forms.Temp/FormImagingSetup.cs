using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using CodeBase;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental;

public partial class FormImagingSetup:FormODBase {
	private bool _showScanDocSelectSourceOld;
	//private ComputerPref computerPrefs;

		
	public FormImagingSetup(){
		InitializeComponent();
	}

	private void FormImagingSetup_Load(object sender, System.EventArgs e) {
		comboType.Items.Add("B");
		comboType.Items.Add("D");
		_showScanDocSelectSourceOld=ComputerPrefs.LocalComputer.ScanDocSelectSource;
		checkScanDocSelectSource.Checked=_showScanDocSelectSourceOld;
		if(ComputerPrefs.LocalComputer.ScanDocShowOptions) {
			radioScanDocShowOptions.Checked=true;
			radioScanDocUseOptionsBelow.Checked=false;
			groupScanningOptions.Enabled=false;
		}
		else {
			radioScanDocShowOptions.Checked=false;
			radioScanDocUseOptionsBelow.Checked=true;
			groupScanningOptions.Enabled=true;
		}
		checkScanDocDuplex.Checked=ComputerPrefs.LocalComputer.ScanDocDuplex;
		checkScanDocGrayscale.Checked=ComputerPrefs.LocalComputer.ScanDocGrayscale;
		textScanDocResolution.Text=ComputerPrefs.LocalComputer.ScanDocResolution.ToString();
		textScanDocQuality.Text=ComputerPrefs.LocalComputer.ScanDocQuality.ToString();
		//textScanDocQuality.Text=PrefC.GetLong(PrefName.ScannerCompression).ToString();
		slider.MinVal=PrefC.GetInt(PrefName.ImageWindowingMin);
		slider.MaxVal=PrefC.GetInt(PrefName.ImageWindowingMax);
		var programSuni=Programs.GetFirstOrDefault(x => x.ProgDesc=="Suni");
		if(programSuni!=null && programSuni.Enabled){
			var exposureLevelVal=ComputerPrefs.LocalComputer.SensorExposure;
			if(exposureLevelVal<(int)upDownExposure.Minimum || exposureLevelVal>(int)upDownExposure.Maximum){
				exposureLevelVal=(int)upDownExposure.Minimum;//Play it safe with the default exposure.
			}
			upDownExposure.Value=exposureLevelVal;
			upDownPort.Value=ComputerPrefs.LocalComputer.SensorPort;
			comboType.Text=ComputerPrefs.LocalComputer.SensorType;
			checkBinned.Checked=ComputerPrefs.LocalComputer.SensorBinned;
		}
		else{
			groupBoxSuni.Visible=false;
		}
		//checkScanDocShowOptions.Checked=PrefC.GetBool(PrefName.ScannerSuppressDialog);
		//textScanDocResolution.Text=PrefC.GetString(PrefName.ScannerResolution);
		if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
			labelPanoBW.Visible=false;
		}
	}

	private void radioScanDocShowOptions_CheckedChanged(object sender,EventArgs e) {
		groupScanningOptions.Enabled=true;
		if(radioScanDocShowOptions.Checked) {
			groupScanningOptions.Enabled=false;
		}
	}

	private void butSetScanner_Click(object sender,EventArgs e) {
		try {
			ImagingDeviceManager.Twain.ActivateEZTwain();
		}
		catch {
			MsgBox.Show(this,"EzTwain4.dll not found.  Please run the setup file in your images folder.");
			return;
		}
		ImagingDeviceManager.EZTwain.SelectImageSource(this.Handle);
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(!textScanDocQuality.IsValid() || !textScanDocResolution.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		if(textScanDocQuality.Text=="100"
		   || (radioScanDocUseOptionsBelow.Checked && SIn.Int(textScanDocResolution.Text)>300)) 
		{
			if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"With the provided settings the file created may be extremely large.  Would you like to continue?")) {
				return;
			}
		}
		ComputerPrefs.LocalComputer.ScanDocSelectSource=checkScanDocSelectSource.Checked;
		ComputerPrefs.LocalComputer.ScanDocShowOptions=radioScanDocShowOptions.Checked;
		ComputerPrefs.LocalComputer.ScanDocDuplex=checkScanDocDuplex.Checked;
		ComputerPrefs.LocalComputer.ScanDocGrayscale=checkScanDocGrayscale.Checked;
		ComputerPrefs.LocalComputer.ScanDocResolution=SIn.Int(textScanDocResolution.Text);
		ComputerPrefs.LocalComputer.ScanDocQuality=SIn.Byte(textScanDocQuality.Text);
		//Prefs.UpdateLong(PrefName.ScannerCompression,PIn.Long(textScanDocQuality.Text));
		Prefs.UpdateLong(PrefName.ImageWindowingMin,slider.MinVal);
		Prefs.UpdateLong(PrefName.ImageWindowingMax,slider.MaxVal);
		//Prefs.UpdateBool(PrefName.ScannerSuppressDialog,checkScanDocShowOptions.Checked);
		//Prefs.UpdateLong(PrefName.ScannerResolution,PIn.Long(textScanDocResolution.Text));
		if(groupBoxSuni.Visible){
			ComputerPrefs.LocalComputer.SensorExposure=(int)upDownExposure.Value;
			ComputerPrefs.LocalComputer.SensorPort=(int)upDownPort.Value;
			ComputerPrefs.LocalComputer.SensorType=comboType.Text;
			ComputerPrefs.LocalComputer.SensorBinned=checkBinned.Checked;
		}
		ComputerPrefs.Update(ComputerPrefs.LocalComputer);
		DataValid.SetInvalid(InvalidType.Prefs);
		if(_showScanDocSelectSourceOld!=checkScanDocSelectSource.Checked) {
			SecurityLogs.MakeLogEntry(EnumPermType.Setup,0
				,Lan.g(this,"Show Select Scanner Window option changed from")+" "
				                                                             +(_showScanDocSelectSourceOld?Lan.g(this,"true"):Lan.g(this,"false"))+" "
				                                                             +Lan.g(this,"to")+" "+(checkScanDocSelectSource.Checked?Lan.g(this,"true"):Lan.g(this,"false")));
		}
		DialogResult=DialogResult.OK;
	}

}