using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormScheduledProcessesEdit:FormODBase {
	private ScheduledProcess _scheduledProcess;
	private ScheduledProcess _scheduledProcessOld;

	public FormScheduledProcessesEdit() {
		InitializeComponent();
	}

	public FormScheduledProcessesEdit(ScheduledProcess scheduledProcess) {
		InitializeComponent();

		_scheduledProcess=scheduledProcess;
		_scheduledProcessOld=_scheduledProcess.Copy();
	}

	private void FormScheduledProcessesEdit_Load(object sender,EventArgs e) {
		comboScheduledAction.Items.AddEnums<ScheduledActionEnum>();
		if(!_scheduledProcess.IsNew){
			comboScheduledAction.SetSelected((int)_scheduledProcess.ScheduledAction);
		}
		comboFrequency.Items.AddEnums<FrequencyToRunEnum>();
		if(!_scheduledProcess.IsNew){
			comboFrequency.SetSelected((int)_scheduledProcess.FrequencyToRun);
		}
		textTimeToRun.Clear();
		if(!_scheduledProcess.IsNew) {	
			textTimeToRun.Text=_scheduledProcess.TimeToRun.ToShortTimeString();
		}
	}
		
	private void SetScheduledProcessObject() {
		_scheduledProcess.ScheduledAction=comboScheduledAction.GetSelected<ScheduledActionEnum>(); 
		_scheduledProcess.FrequencyToRun=comboFrequency.GetSelected<FrequencyToRunEnum>();
		_scheduledProcess.TimeToRun=SIn.DateTime(textTimeToRun.Text);
	}

	private bool ValidateFields() {
		if(comboScheduledAction.SelectedIndex==-1) {
			ODMessageBox.Show("A Scheduled Action must be selected.");
			return false;
		}
		if(comboFrequency.SelectedIndex==-1) {
			ODMessageBox.Show("A Frequency for the action must be selected.");
			return false;
		}
		if(textTimeToRun.Text=="" || !textTimeToRun.IsValid()) {
			ODMessageBox.Show("A valid time to run must be entered.");
			return false;
		}
		return true;
	}

	private bool IsScheduled() {
		//If editing a scheduled process and click ok without changing values, do not warn.
		if(_scheduledProcess.ScheduledProcessNum==_scheduledProcessOld.ScheduledProcessNum &&
		   _scheduledProcess.ScheduledAction==_scheduledProcessOld.ScheduledAction &&
		   _scheduledProcess.TimeToRun.TimeOfDay==_scheduledProcessOld.TimeToRun.TimeOfDay &&
		   _scheduledProcess.FrequencyToRun==_scheduledProcessOld.FrequencyToRun) 
		{
			return false;
		}
		var listScheduledProcesses=ScheduledProcesses.CheckAlreadyScheduled(_scheduledProcess.ScheduledAction,_scheduledProcess.FrequencyToRun,
			_scheduledProcess.TimeToRun);
		if(listScheduledProcesses.Count<1) {
			return false;
		}
		return true;
	}

	private void butDeleteSchedProc_Click(object sender,EventArgs e) {
		if(!_scheduledProcessOld.IsNew && MsgBox.Show(this,MsgBoxButtons.OKCancel,"This will delete the currently selected Scheduled Process. Continue?",
			   "Delete Scheduled Process."))
		{
			ScheduledProcesses.Delete(_scheduledProcessOld.ScheduledProcessNum);
		}
		DialogResult=DialogResult.OK;
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(!ValidateFields()) {
			return;
		}
		SetScheduledProcessObject();
		if(IsScheduled()) {
			ODMessageBox.Show("There is an identical Action already scheduled for that time and frequency.");
			return;
		}
		if(_scheduledProcess.IsNew) {
			ScheduledProcesses.Insert(_scheduledProcess);
		}
		else {
			ScheduledProcesses.Update(_scheduledProcess,_scheduledProcessOld);
		}
		DialogResult=DialogResult.OK;
	}

}