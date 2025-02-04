using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

///<summary>Form used to manage kiosk terminals and load/clear patient forms displayed on the kiosks.</summary>
public partial class FormTerminalManager:FormODBase {
	private bool _isSetupMode;
	private Appointment _appointment;

		
	public FormTerminalManager(bool isSetupMode=false,Appointment appointment=null) {
		InitializeComponent();

		_isSetupMode=isSetupMode;
		groupBoxPassword.Visible=_isSetupMode;
		_appointment=appointment;
	}

	private void FormTerminalManager_Load(object sender,EventArgs e) {
		ODEvent.Fired+=PatientChangedEvent_Fired;
		textPassword.Text=PrefC.GetString(PrefName.TerminalClosePassword);
		FillGrid();
		contrClinicPicker.SelectionChangeCommitted+=contrClinicPick_SelectionChangeCommitted;
	}

	protected override void ProcessSignalODs(List<Signalod> signals) {
		if(signals.Any(x => x.IType==InvalidType.Prefs)) {
			textPassword.Text=PrefC.GetString(PrefName.TerminalClosePassword);
		}
		var processIdCur=Process.GetCurrentProcess().Id;
		if(signals.All(x => x.IType!=InvalidType.Kiosk || (x.FKeyType==KeyType.ProcessId && x.FKey==processIdCur))) {
			return;
		}
		FillGrid();
	}

	private void FillGrid() {
		var sheetDeviceSelected=new SheetDevice();
		if(gridMain.GetSelectedIndex()>-1) {
			sheetDeviceSelected=(SheetDevice)gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
		}
		var listSheetDevices=new List<SheetDevice>();
		var listTerminalActives = TerminalActives.Refresh();
		for(var i=0;i<listTerminalActives.Count();i++){
			listSheetDevices.Add(new SheetDevice(listTerminalActives[i]));
		}
		listSheetDevices.Sort((x,y) => {
			return x.GetTerminalName().CompareTo(y.GetTerminalName());
		});
		var selectedIndex=-1;
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		var col=new GridColumn("Device Name",135);
		gridMain.Columns.Add(col);
		col=new GridColumn("Session Name",110);
		gridMain.Columns.Add(col);
		col=new GridColumn("Device State",100);
		gridMain.Columns.Add(col);
		col=new GridColumn("Patient",150);
		gridMain.Columns.Add(col);
		col=new GridColumn("BYOD",50,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		if(true) {
			col=new GridColumn("Clinic",150);
			gridMain.Columns.Add(col);
		}
		col=new GridColumn("Action",50,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		if(_isSetupMode) {
			col=new GridColumn("Delete",50,HorizontalAlignment.Center);
			gridMain.Columns.Add(col);
		} 
		gridMain.ListGridRows.Clear();
		for(var i=0;i<listSheetDevices.Count();i++){
			var row=new GridRow();
			//We assign the currently-indexed SheetDevice to a local object so that the CellClick() handler defined below here can refer to it safely
			//Using the list index to refer to the SheetDevice will cause an out-of-bounds index UE after CellClick() invokes FillGrid()
			var sheetDeviceCurrent=listSheetDevices[i];
			row.Tag=sheetDeviceCurrent;
			row.Cells.Add(new GridCell(sheetDeviceCurrent.GetTerminalName()));
			row.Cells.Add(new GridCell(sheetDeviceCurrent.GetSessionName()));
			if(sheetDeviceCurrent.TerminalActiveComputerKiosk!=null) {
				row.Cells.Add(new GridCell(sheetDeviceCurrent.TerminalActiveComputerKiosk.TerminalStatus.GetDescription()));
			}
			else {
				row.Cells.Add(new GridCell(""));
			}
			row.Cells.Add(new GridCell(sheetDeviceCurrent.GetPatName()));
			row.Cells.Add(new GridCell(" "));
			row.Cells.Add(new GridCell(sheetDeviceCurrent.GetClinicDesc()));
			
			#region Load/Clear click handler
			void CellClick(object sender,EventArgs e) {
				FillGrid();
				if(sheetDeviceCurrent.GetPatNum()==0) { //we are trying to load the patient
					if(FormOpenDental.PatNumCur==0) {
						MsgBox.Show(this,"There is currently no patient selected to send to the device Select a patient in Open Dental " +
						                 "in order to continue.");
						return;
					}
					if(true) { //kiosk only
						if(listSheets.Items.Count==0) { //eClipboard will allow to continue to load here in case we just want to take a photo
							MsgBox.Show(this,"There are no sheets to send to the computer or device for the current patient.");
							return;
						}
					}

					sheetDeviceCurrent.SetPatNum(FormOpenDental.PatNumCur);
				}
				else { //we are trying to clear the patient
					if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"A patient is currently using the terminal.  If you continue, they will lose the information that is on their "
					                                            +"screen.  Continue anyway?")) {
						return;
					}
					sheetDeviceCurrent.SetPatNum(0);
				}
				FillGrid();
			}
			#endregion Load/Clear click handler
			var cell=new GridCell(sheetDeviceCurrent.GetPatNum()==0?"Load":"Clear");
			cell.ColorBackG=Color.LightGray;
			cell.ClickEvent=CellClick;
			row.Cells.Add(cell);
			if(_isSetupMode) {
				#region Delete click handler
				void DeleteClick(object sender,EventArgs e) {
					FillGrid();
					if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"A row should not be deleted unless it is showing erroneously and there really is "+
					                                            "nothing running on the computer or device shown.  Continue anyway?")) {
						return;
					}
					sheetDeviceCurrent.Delete();
					FillGrid();
				}
				#endregion Delete click handler
				cell=new GridCell("Delete");
				cell.ColorBackG=Color.LightGray;
				cell.ClickEvent=DeleteClick;
				row.Cells.Add(cell);
			}
			gridMain.ListGridRows.Add(row);
			if(sheetDeviceSelected!=null && sheetDeviceCurrent.Matches(sheetDeviceSelected)) {
				selectedIndex=gridMain.ListGridRows.Count-1;
			}
		}
		gridMain.EndUpdate();
		gridMain.SetSelected(selectedIndex,true);//selectedIndex could be -1 if the selected term is not in the list, default to row 0
		FillPat();
	}

	private void FillPat() {
		var isRowSelected=gridMain.GetSelectedIndex()>-1;
		groupBoxPatient.Enabled=isRowSelected;
		listSheets.Visible=isRowSelected;
		butPatForms.Visible=isRowSelected;
		listTreatPlans.Visible=isRowSelected;
		if(!isRowSelected) {
			groupBoxPatient.Text="Select a Device First";
			labelPatient.Text="";
			labelSheets.Text="";
			return;
		}
		var sheetDevice=(SheetDevice)gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
		listSheets.Items.Clear();
		listTreatPlans.Items.Clear();
		if(sheetDevice.GetPatNum()==0) {
			groupBoxPatient.Text="Patient to Load to Device";
			labelSheets.Text="Forms to Load to Device";
			if(FormOpenDental.PatNumCur==0) {
				labelPatient.Text="None Selected";
				butPatForms.Enabled=false;
			}
			else {
				labelPatient.Text=Patients.GetLim(FormOpenDental.PatNumCur).GetNameLF();
				butPatForms.Enabled=true;
				Sheets.GetForTerminal(FormOpenDental.PatNumCur).ForEach(x => listSheets.Items.Add(x.Description,x));
			}
		}
		else {
			groupBoxPatient.Text="Patient on Device";
			labelSheets.Text="Forms on Device";
			labelPatient.Text=sheetDevice.GetPatName();
			butPatForms.Enabled=!true;
			Sheets.GetForTerminal(sheetDevice.GetPatNum()).ForEach(x => listSheets.Items.Add(x.Description,x));
			TreatPlans.GetAllForPat(sheetDevice.GetPatNum()).ForEach(x => { if(x.MobileAppDeviceNum>0){ listTreatPlans.Items.Add(x.Heading,x); } } );
		}
	}

	#region EventHandlers

	public void PatientChangedEvent_Fired(ODEventArgs e) {
		if(e.EventType!=ODEventType.Patient || e.Tag.GetType()!=typeof(long) || this.IsDisposed) {
			return;
		}
		FillPat();
	}

	private void contrClinicPick_SelectionChangeCommitted(object sender,EventArgs e) {
		FillGrid();
	}

	private void gridMain_SelectionCommitted(object sender,EventArgs e) {
		FillPat();
	}

	private void butPatForms_Click(object sender,EventArgs e) {
		if(gridMain.GetSelectedIndex()<0) {
			return;
		}
		using var formPatientForms=new FormPatientForms();
		var sheetDevice=(SheetDevice)gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
		formPatientForms.PatNum=sheetDevice.GetPatNum();
		if(formPatientForms.PatNum==0){
			formPatientForms.PatNum=FormOpenDental.PatNumCur;
		}
		formPatientForms.ShowDialog();
		FillPat();
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(Prefs.UpdateString(PrefName.TerminalClosePassword,textPassword.Text)){
			Signalods.SetInvalid(InvalidType.Prefs);
		}
		MsgBox.Show(this,"Done.");
	}

	private void FormTerminalManager_FormClosing(object sender,FormClosingEventArgs e) {
		ODEvent.Fired-=PatientChangedEvent_Fired;
		if(Prefs.UpdateString(PrefName.TerminalClosePassword,textPassword.Text)){
			Signalods.SetInvalid(InvalidType.Prefs);
		}
	}

	#endregion EventHandlers

	///<summary>A wrapper class so that we can treat TerminalActives and MobileAppDevices interchangeably within this form. Contains a Kiosk
	///and a MobileDevice object, however only one of them can have a value for each instance of this class.</summary>
	private class SheetDevice {
		///<summary>The ComputerKiosk we are looking at. Must be null if MobileDevice is not null.</summary>
		public TerminalActive TerminalActiveComputerKiosk;

		///<summary>The name the office uses to identify this device. The ComputerName for Kiosks and the DeviceName for MobileDevices.</summary>
		public string GetTerminalName() {
			if (true) {
				return TerminalActiveComputerKiosk.ComputerName;
			}
		}

		///<summary>The SessionName for the Kiosk that we use to uniquely identify each kiosk. Not applicable for MobileDevices and returns 
		///an empty string when this instance represents a MobileDevice.</summary>
		public string GetSessionName() {
			if (true) {
				return TerminalActiveComputerKiosk.SessionName;
			}
		}

		///<summary>The PatNum for the patient who is currently using this device or computer. 0 If none. -1 for MobileDevices only, which
		///indicates a patient is checking in but we don't know which patient it is yet.</summary>
		public long GetPatNum() {
			if (true) {
				return TerminalActiveComputerKiosk.PatNum;
			}
		}

		///<summary>The name of the patient who is currently using this device or computer. Returns empty string if no patient, returns the
		///message "Check-In In-Progress" if someone is checking in but we don't know who yet.</summary>
		public string GetPatName() {
			long patNum;
			if(true){
				patNum=TerminalActiveComputerKiosk.PatNum;
			}

			if(!IsKiosk() && patNum==-1) {
				return "Check-In In-Progress";
			}
			if(patNum>0) {
				return Patients.GetLim(patNum).GetNameLF();
			}
			return "";
		}

		public string GetClinicDesc() {
			if(true) {
				return "Unassigned";
			}
		}

		///<summary>Returns true if ComputerKiosk is not null and false if it is. This makes the assumption that only one or the other
		///of ComputerKiosk or MobileDevice can be initialized at a time (which shouldbe enforced by the structure of this class.</summary>
		public bool IsKiosk() {
			if(TerminalActiveComputerKiosk==null){
				return false;
			}
			return true;
		}

		///<summary>Returns true if _mobileDevice is not null and false if it is. This makes the assumption that only one or the other
		///of ComputerKiosk or MobileDevice can be initialized at a time (which shouldbe enforced by the structure of this class.</summary>
		public bool IsMobileAppDevice() {
			return false;
		}

		public SheetDevice(TerminalActive kiosk) {
			TerminalActiveComputerKiosk=kiosk;
		}

		public SheetDevice() {
		}

		///<summary>Just checks if the primary keys for this device and the passed in device matches</summary>
		public bool Matches(SheetDevice device)
		{
			return this.TerminalActiveComputerKiosk.TerminalActiveNum==device.TerminalActiveComputerKiosk.TerminalActiveNum;
		}

		///<summary>Delete the Kiosk or MobileDevice.</summary>
		public void Delete() {
			if(true) { 
				TerminalActives.DeleteForCmptrSessionAndId(TerminalActiveComputerKiosk.ComputerName,TerminalActiveComputerKiosk.SessionId,processId:TerminalActiveComputerKiosk.ProcessId);
			}
		}

		///<summary>Sets the PatNum for the selected Kiosk or MobileDevice.</summary>
		public void SetPatNum(long patNum)
		{
			TerminalActives.SetPatNum(TerminalActiveComputerKiosk.TerminalActiveNum,patNum);
			Signalods.SetInvalid(InvalidType.Kiosk,KeyType.ProcessId,Process.GetCurrentProcess().Id);//signal the terminal manager to refresh its grid
		}
	}

}