using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormASAP:FormODBase {
	///<summary>When texting patients on this list, this will be when the beginning of the slot is.</summary>
	public DateTime DateTimeChosen;
	private List<Appointment> _listAppointmentsASAP;
	private bool _doPrintHeading;
	private int _pagesPrinted;
	///<summary>A list of patient names and patNums. Gets refilled every time FillGridAppts() is invoked.</summary>
	private List<Patients.PatientName> _listPatientNames= [];
	private List<ProviderDto> _listProviders;
	private List<Site> _listSites;
	///<summary>Holds the maximum appt length which is then used to filter out appts with larger lengths from the Appointment ASAP List.
	///Also, appts still showing after considering this filter will be reordered so that the ones with lengths closest to this filter are first in the grid.</summary>
	private int _maxApptLengthFilter=0;
	
	public FormASAP(long opNum=0) {
		InitializeComponent();// Required for Windows Form Designer support
	}

	private void FormASAP_Load(object sender,System.EventArgs e) {
		Cursor=Cursors.WaitCursor;
		LayoutMenu();
		SetFilterControlsAndAction(() => {
			if(tabControl.SelectedIndex==0) {
				FillGridAppts();
			}
			else {
				FillGridRecalls();
			}
		},comboProv,comboSite,comboClinic,comboAptStatus,comboNumberReminders,checkGroupFamilies);
		_listProviders=Providers.GetDeepCopy(true);
		comboProv.IncludeAll=true;
		comboProv.Items.AddProvsFull(_listProviders);
		comboProv.IsAllSelected=true;
		if(PrefC.GetBool(PrefName.EnterpriseApptList)){
			comboClinic.IncludeAll=false;
		}
		if(PrefC.GetBool(PrefName.EasyHidePublicHealth)){
			comboSite.Visible=false;
			labelSite.Visible=false;
		}
		else{
			_listSites=Sites.GetDeepCopy();
			comboSite.IncludeAll=true;
			comboSite.SelectedIndex=0;
			comboSite.Items.AddList(_listSites,x => x.Description);
			comboSite.IsAllSelected=true;
		}
		splitContainer.Panel2Collapsed=true;
		comboAptStatus.Items.Add(Lan.g(this,ApptStatus.Scheduled.ToString()),ApptStatus.Scheduled);
		comboAptStatus.Items.Add(Lan.g(this,ApptStatus.Planned.ToString()),ApptStatus.Planned);
		comboAptStatus.Items.Add(Lan.g(this,ApptStatus.UnschedList.ToString()),ApptStatus.UnschedList);
		comboAptStatus.Items.Add(Lan.g(this,ApptStatus.Broken.ToString()),ApptStatus.Broken);
		comboShowHygiene.Items.Add(Lan.g(this,FromASAPShowAppointment.All.GetDescription()),FromASAPShowAppointment.All);
		comboShowHygiene.Items.Add(Lan.g(this,FromASAPShowAppointment.NonHygiene.GetDescription()),FromASAPShowAppointment.NonHygiene);
		comboShowHygiene.Items.Add(Lan.g(this,FromASAPShowAppointment.Hygiene.GetDescription()),FromASAPShowAppointment.Hygiene);
		checkGroupFamilies.Checked=PrefC.GetBool(PrefName.RecallGroupByFamily);
		comboNumberReminders.Items.Add(Lan.g(this,"All"));
		comboNumberReminders.Items.Add("0");
		comboNumberReminders.Items.Add("1");
		comboNumberReminders.Items.Add("2");
		comboNumberReminders.Items.Add("3");
		comboNumberReminders.Items.Add("4");
		comboNumberReminders.Items.Add("5");
		comboNumberReminders.Items.Add("6+");
		comboNumberReminders.SelectedIndex=0;
		var daysPast=PrefC.GetInt(PrefName.RecallDaysPast);
		var daysFuture=PrefC.GetInt(PrefName.RecallDaysFuture);
		if(daysPast==-1){
			textDateStart.Text="";
		}
		else{
			textDateStart.Text=DateTime.Today.AddDays(-daysPast).ToShortDateString();
		}
		if(daysFuture==-1) {
			textDateEnd.Text="";
		}
		else {
			textDateEnd.Text=DateTime.Today.AddDays(daysFuture).ToShortDateString();
		}
		Cursor=Cursors.Default;
	}

	private void LayoutMenu() {
		menuMain.BeginUpdate();
		menuMain.Add(new MenuItemOD("Settings",menuItemSettings_Click));
		menuMain.EndUpdate();
	}

	///<summary>There is a bug in ODProgress.cs that forces windows that use a progress bar on load to go behind other applications. 
	///This is a temporary workaround until we decide how to address the issue.</summary>
	private void FormASAP_Shown(object sender,EventArgs e) {
		FillGridAppts();
	}

	private void tabControl_SelectedIndexChanged(object sender,EventArgs e) {
		switch(tabControl.SelectedIndex) {
			case 0:
			default:
				FillGridAppts();
				break;
			case 1:
				FillGridRecalls();
				break;
		}
	}

	#region Appointments Tab

	private void FillGridAppts(){
		long siteNum=0;
		if(!PrefC.GetBool(PrefName.EasyHidePublicHealth)) {
			siteNum=comboSite.GetSelectedKey<Site>(x => x.SiteNum);
		}
		if(!SmsPhones.IsIntegratedTextingEnabled()) {
			butText.Enabled=false;
		}
		var listAppStatuses=comboAptStatus.GetListSelected<ApptStatus>();
		long clinicNum=-1;
		if(true) {
			clinicNum=comboClinic.ClinicNumSelected;
		}
		var progressOD=new ProgressWin();
		progressOD.ActionMain=() => { 
			_listAppointmentsASAP=Appointments.RefreshASAP(comboProv.GetSelectedProvNum(),siteNum,clinicNum,listAppStatuses,codeRangeFilter.StartRange,
				codeRangeFilter.EndRange);
		};
		progressOD.ShowDialog();
		var scrollVal=gridAppts.ScrollValue;
		var listAptNumsSelected=gridAppts.SelectedTags<Appointment>().Select(x => x.AptNum).ToList();
		if(_maxApptLengthFilter>0) {
			_listAppointmentsASAP.RemoveAll(x => x.Length>_maxApptLengthFilter);
			_listAppointmentsASAP=_listAppointmentsASAP.OrderByDescending(x => x.Length).ThenBy(x => x.AptDateTime).ToList();
		}
		gridAppts.BeginUpdate();
		gridAppts.Columns.Clear();
		var col=new GridColumn(Lan.g("TableASAP","Patient"),140);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Date"),65,HorizontalAlignment.Center,GridSortingStrategy.DateParse);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Unsched Status"),110);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Apt Status"),75);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Prov"),50);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Procedures"),150);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Length"),60,GridSortingStrategy.AmountParse);
		gridAppts.Columns.Add(col);
		col=new GridColumn(Lan.g("TableASAP","Notes"),160);
		gridAppts.Columns.Add(col);
		gridAppts.ListGridRows.Clear();
		GridRow row;
		var listPatNums=_listPatientNames.Select(x=>x.PatNum).ToList();
		if(_listAppointmentsASAP.Any(x => !listPatNums.Contains(x.PatNum))) {
			_listPatientNames=Patients.GetPatientNameList(_listAppointmentsASAP.Select(x => x.PatNum).ToList());
		}
		var fromASAPShowAppointment=comboShowHygiene.GetSelected<FromASAPShowAppointment>();
		for(var i=0;i<_listAppointmentsASAP.Count;i++) {
			if(fromASAPShowAppointment==FromASAPShowAppointment.Hygiene && !_listAppointmentsASAP[i].IsHygiene) {
				continue;
			}
			if(fromASAPShowAppointment==FromASAPShowAppointment.NonHygiene && _listAppointmentsASAP[i].IsHygiene) {
				continue;
			}
			row=new GridRow();
			var patientName=_listPatientNames.FirstOrDefault(x=>x.PatNum==_listAppointmentsASAP[i].PatNum);
			if(patientName.Name is null) {
				patientName.Name=Lan.g(this,"UNKNOWN");
			}
			row.Cells.Add(patientName.Name);
			if(_listAppointmentsASAP[i].AptDateTime.Year < 1880){//shouldn't be possible.
				row.Cells.Add("");
			}
			else{
				row.Cells.Add(_listAppointmentsASAP[i].AptDateTime.ToShortDateString());
			}
			row.Cells.Add(Defs.GetName(DefCat.RecallUnschedStatus,_listAppointmentsASAP[i].UnschedStatus));
			row.Cells.Add(_listAppointmentsASAP[i].AptStatus.ToString());
			if(_listAppointmentsASAP[i].IsHygiene) {
				row.Cells.Add(Providers.GetAbbr(_listAppointmentsASAP[i].ProvHyg));
			}
			else {
				row.Cells.Add(Providers.GetAbbr(_listAppointmentsASAP[i].ProvNum));
			}
			row.Cells.Add(_listAppointmentsASAP[i].ProcDescript);
			row.Cells.Add(_listAppointmentsASAP[i].Length.ToString());
			row.Cells.Add(_listAppointmentsASAP[i].Note);
			row.Tag=_listAppointmentsASAP[i];
			gridAppts.ListGridRows.Add(row);
		}
		gridAppts.EndUpdate();
		gridAppts.ScrollValue=scrollVal;
		for(var i=0;i<gridAppts.ListGridRows.Count;i++) {
			if(listAptNumsSelected.Contains(((Appointment)gridAppts.ListGridRows[i].Tag).AptNum)) {
				gridAppts.SetSelected(i,true);
			}
		}
	}

	private void gridAppts_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var currentSelection=e.Row;
		var currentScroll=gridAppts.ScrollValue;
		var patient=Patients.GetPat(((Appointment)gridAppts.ListGridRows[e.Row].Tag).PatNum); //check against grid
		GlobalFormOpenDental.PatientSelected(patient,isRefreshCurModule: true);
		using var formApptEdit=new FormApptEdit(((Appointment)gridAppts.ListGridRows[e.Row].Tag).AptNum);
		formApptEdit.PinIsVisible=true;
		formApptEdit.ShowDialog();
		if(formApptEdit.DialogResult!=DialogResult.OK){
			return;
		}
		if(formApptEdit.PinClicked) {
			SendPinboard_Click(); //Whatever they double clicked on will still be selected, just fire the event to send it to the pinboard.
			DialogResult=DialogResult.OK;
		}
		else {
			FillGridAppts();
			gridAppts.SetSelected(currentSelection,true);
			gridAppts.ScrollValue=currentScroll;
		}
	}

	private void RemoveAppts_Click() {
		if(!Security.IsAuthorized(EnumPermType.AppointmentEdit)) {
			return;
		}
		if(gridAppts.SelectedIndices.Length>0 && !MsgBox.Show(this,MsgBoxButtons.OKCancel,"Change priority to normal for all selected appointments?")) {
			return;
		}
		for(var i=0;i<gridAppts.SelectedIndices.Length;i++) {
			var appointment=(Appointment)gridAppts.SelectedGridRows[i].Tag;
			var datePrevious=appointment.DateTStamp;
			Appointments.SetPriority(appointment,ApptPriority.Normal);
			SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit,appointment.PatNum,"Appointment priority set from ASAP to normal from ASAP list.",appointment.AptNum,
				datePrevious);
		}
		FillGridAppts();
	}

	private void SendPinboard_Click() {
		if(gridAppts.SelectedIndices.Length==0) {
			MsgBox.Show(this,"Please select an appointment first.");
			return;
		}
		var listAptNums=new List<long>();
		var listAppointmentsSelected=gridAppts.SelectedIndices.Select(x => (Appointment)gridAppts.ListGridRows[x].Tag).ToList();
		var listPatients=Patients.GetMultPats(listAppointmentsSelected.Select(x => x.PatNum).ToList()).ToList();
		var patsArchivedOrDeceased=0;
		for(var i=0;i<listAppointmentsSelected.Count;i++) {
			var patient=listPatients.FirstOrDefault(x => x.PatNum==listAppointmentsSelected[i].PatNum);
			if(patient!=null && patient.PatStatus.In(PatientStatus.Archived,PatientStatus.Deceased)) {
				patsArchivedOrDeceased++;
				continue;
			}
			if(listAppointmentsSelected[i].AptStatus==ApptStatus.Planned) {
				if(Procedures.GetProcsForSingle(listAppointmentsSelected[i].AptNum,isPlanned: true).Count(x => x.ProcStatus==ProcStat.C) > 0) {
					MsgBox.Show(this,"Not allowed to send a planned appointment to the pinboard if completed procedures are attached. Edit the planned "
					                 +"appointment first.");
					return;
				}
				var apppointmentNext=Appointments.GetScheduledPlannedApt(listAppointmentsSelected[i].AptNum);
				if(apppointmentNext!=null) {
					if(apppointmentNext.AptStatus==ApptStatus.Complete) {
						//Warn the user they are moving a completed appointment.
						if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"You are about to move an already completed appointment.  Continue?")) {
							return;
						}
					}
					else if(apppointmentNext.AptStatus==ApptStatus.Scheduled) {
						//Warn the user they are moving an already scheduled appointment.
						if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"You are about to move an appointment already on the schedule.  Continue?")) {
							return;
						}
					}
				}
			}
			listAptNums.Add(listAppointmentsSelected[i].AptNum);
		}
		var listUserMsgs=new List<string>();
		if(patsArchivedOrDeceased > 0) {
			listUserMsgs.Add(Lan.g(this,"Appointments skipped because patient status is archived or deceased:")+" "+patsArchivedOrDeceased+".");
		}
		if(listAptNums.Count==0) {
			listUserMsgs.Add(Lan.g(this,"There are no appointments to send to the pinboard."));
		}
		if(listUserMsgs.Count>0) {
			ODMessageBox.Show(string.Join("\r\n",listUserMsgs));
			if(listAptNums.Count==0) {
				return;
			}
		}
		GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, listPinApptNums:listAptNums,dateSelected:DateTime.Today);//Pins all appointments to the pinboard that were in listAptNums.
	}
	#endregion Appointments Tab

	#region Recalls Tab

	private void FillGridRecalls() {
		DateTime dateFrom;
		DateTime dateTo;
		if(textDateStart.Text==""){
			dateFrom=DateTime.MinValue;
		}
		else{
			dateFrom=SIn.Date(textDateStart.Text);
		}
		if(textDateEnd.Text=="") {
			dateTo=DateTime.MaxValue;
		}
		else {
			dateTo=SIn.Date(textDateEnd.Text);
		}
		long siteNum=0;
		if(!PrefC.GetBool(PrefName.EasyHidePublicHealth)) {
			siteNum=comboSite.GetSelectedKey<Site>(x => x.SiteNum);
		}
		long clinicNum=-1;
		if(true) {
			clinicNum=comboClinic.ClinicNumSelected;
		}
		var recallListShowNumberReminders=(RecallListShowNumberReminders)comboNumberReminders.SelectedIndex;
		DataTable tableRecalls=null;
		List<Recall> listRecalls=null;
		var maxReminders=PrefC.GetLong(PrefName.RecallMaxNumberReminders);
		var progressOD=new ProgressWin();
		progressOD.ActionMain=() => { 
			tableRecalls=Recalls.GetRecallList(dateFrom,dateTo,checkGroupFamilies.Checked,comboProv.GetSelectedProvNum(),clinicNum,
				siteNum,RecallListSort.DueDate,recallListShowNumberReminders,maxReminders,isAsap: true,codeRangeFilter.StartRange,codeRangeFilter.EndRange);
			listRecalls=Recalls.GetMultRecalls(tableRecalls.Rows.OfType<DataRow>().Select(x => SIn.Long(x["RecallNum"].ToString())).ToList());
		};
		progressOD.ShowDialog();
		if(progressOD.IsCancelled){
			return;
		}
		var listRecallNumsSelected=gridRecalls.SelectedTags<Recall>().Select(x => x.RecallNum).ToList();
		var hasGridBeenFilledBefore=(gridRecalls.Columns.Count > 0);
		gridRecalls.BeginUpdate();
		gridRecalls.Columns.Clear();
		GridColumn col;
		var listDisplayFields=DisplayFields.GetForCategory(DisplayFieldCategory.RecallList);
		for(var i=0;i<listDisplayFields.Count;i++) {
			if(listDisplayFields[i].Description=="") {
				col=new GridColumn(listDisplayFields[i].InternalName,listDisplayFields[i].ColumnWidth);
			}
			else {
				col=new GridColumn(listDisplayFields[i].Description,listDisplayFields[i].ColumnWidth);
			}
			col.Tag=listDisplayFields[i].InternalName;
			gridRecalls.Columns.Add(col);
		}
		gridRecalls.ListGridRows.Clear();
		GridRow row;
		for(var i=0;i<tableRecalls.Rows.Count;i++){
			row=new GridRow();
			for(var f=0;f<listDisplayFields.Count;f++) {
				switch(listDisplayFields[f].InternalName){
					case "Due Date":
						row.Cells.Add(tableRecalls.Rows[i]["dueDate"].ToString());
						break;
					case "Patient":
						row.Cells.Add(tableRecalls.Rows[i]["patientName"].ToString());
						break;
					case "Age":
						row.Cells.Add(tableRecalls.Rows[i]["age"].ToString());
						break;
					case "Type":
						row.Cells.Add(tableRecalls.Rows[i]["recallType"].ToString());
						break;
					case "Interval":
						row.Cells.Add(tableRecalls.Rows[i]["recallInterval"].ToString());
						break;
					case "#Remind":
						row.Cells.Add(tableRecalls.Rows[i]["numberOfReminders"].ToString());
						break;
					case "LastRemind":
						row.Cells.Add(tableRecalls.Rows[i]["dateLastReminder"].ToString());
						break;
					case "Contact":
						row.Cells.Add(tableRecalls.Rows[i]["contactMethod"].ToString());
						break;
					case "Status":
						row.Cells.Add(tableRecalls.Rows[i]["status"].ToString());
						break;
					case "Note":
						row.Cells.Add(tableRecalls.Rows[i]["Note"].ToString());
						break;
					case "BillingType":
						row.Cells.Add(tableRecalls.Rows[i]["billingType"].ToString());
						break;
					case "WebSched":
						row.Cells.Add(tableRecalls.Rows[i]["webSchedSendDesc"].ToString());
						break;
				}
			}
			row.Tag=listRecalls.FirstOrDefault(x => x.RecallNum==SIn.Long(tableRecalls.Rows[i]["RecallNum"].ToString()));
			var patientName=new Patients.PatientName
			{
				PatNum = SIn.Long(tableRecalls.Rows[i]["PatNum"].ToString()),
				Name = SIn.String(tableRecalls.Rows[i]["patientName"].ToString())
			};
			if(_listPatientNames.Count==0 || !_listPatientNames.Any(x => x.PatNum==patientName.PatNum)) {
				_listPatientNames.Add(patientName);
			}
			gridRecalls.ListGridRows.Add(row);
		}
		gridRecalls.EndUpdate();
		for(var i=0;i<gridRecalls.ListGridRows.Count;i++) {
			if(listRecallNumsSelected.Contains(((Recall)gridRecalls.ListGridRows[i].Tag).RecallNum)) {
				gridRecalls.SetSelected(i,true);
			}
		}
	}

	private void gridRecalls_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var recall=(Recall)gridRecalls.ListGridRows[e.Row].Tag;
		var patient=Patients.GetPat(recall.PatNum);
		GlobalFormOpenDental.PatientSelected(patient,isRefreshCurModule: true);
		using var formRecallEdit=new FormRecallEdit();
		formRecallEdit.RecallCur=recall;
		formRecallEdit.ShowDialog();
		if(formRecallEdit.DialogResult!=DialogResult.OK) {
			return;
		}
		FillGridRecalls();
	}

	private void RemoveRecalls_Click() {
		if(gridRecalls.SelectedIndices.Length==0 
		   || !MsgBox.Show(this,MsgBoxButtons.OKCancel,"Change priority to normal for all selected recalls?")) 
		{
			return;
		}
		var listRecallsSelected=gridRecalls.SelectedIndices.Select(x => (Recall)gridRecalls.ListGridRows[x].Tag).ToList();
		for(var i=0;i<listRecallsSelected.Count;i++) {
			listRecallsSelected[i].Priority=RecallPriority.Normal;
			Recalls.Update(listRecallsSelected[i]);
			SecurityLogs.MakeLogEntry(EnumPermType.RecallEdit,listRecallsSelected[i].PatNum,"Recall priority set to Normal from the ASAP List.");
		}
		FillGridRecalls();
	}

	private void MakeAppointment_Click() {
		if(!Security.IsAuthorized(EnumPermType.AppointmentCreate) || gridRecalls.SelectedIndices.Length==0) {
			return;
		}
		var listRecallsSelected=gridRecalls.SelectedIndices.Select(x => (Recall)gridRecalls.ListGridRows[x].Tag).ToList();
		for(var i=0;i<listRecallsSelected.Count;i++) {
			var patient=Patients.GetPat(listRecallsSelected[i].PatNum);
			var patientMerged=AppointmentL.GetPatientMergePrompt(patient.PatNum);
			if(patientMerged!=null) {
				patient=patientMerged;
			}
			if(patient.PatStatus.In(PatientStatus.Archived,PatientStatus.Deceased)) {
				MsgBox.Show(this,"Appointments cannot be scheduled for "+patient.PatStatus.ToString().ToLower()+" patients.");
				return;
			}
			var family=Patients.GetFamily(patient.PatNum);
			var listInsSubs=InsSubs.RefreshForFam(family);
			var listInsPlans=InsPlans.RefreshForSubList(listInsSubs);
			var appointment=AppointmentL.CreateRecallApt(patient,listRecallsSelected[i].RecallNum);
			GlobalFormOpenDental.GoToModule(EnumModuleType.Appointments, listPinApptNums: [appointment.AptNum], patNum:patient.PatNum, dateSelected:DateTime.Today);
		}
		FillGridRecalls();
	}
	#endregion Recalls Tab
		
	private void gridMenuRight_click(object sender,System.EventArgs e) {
		var toolStripMenuItem=(ToolStripMenuItem)sender;
		if(tabControl.SelectedIndex==0) {
			if(gridAppts.SelectedIndices.Length<=0) {
				return;
			}
			switch(menuApptsRightClick.Items.IndexOf(toolStripMenuItem)) {
				case 0:
					SelectPatient_Click(((Appointment)gridAppts.SelectedGridRows.Last().Tag).PatNum);
					break;
				case 1:
					if(gridAppts.SelectedIndices.Length==0) {
						MsgBox.Show(this,"Please select an appointment first.");
						return;
					}
					SeeChart_Click(((Appointment)gridAppts.SelectedGridRows.Last().Tag).PatNum);
					break;
				case 2:
					SendPinboard_Click();
					break;
				case 3:
					RemoveAppts_Click();
					break;
			}
		}
		else {//Recall tab selected
			if(gridRecalls.SelectedIndices.Length<=0) {
				return;
			}
			var recall=gridRecalls.SelectedIndices.Select(x => (Recall)gridRecalls.ListGridRows[x].Tag).ToList().FirstOrDefault();
			switch(menuRecallsRightClick.Items.IndexOf(toolStripMenuItem)) {
				case 0:
					SelectPatient_Click(recall.PatNum);
					break;
				case 1:
					if(gridRecalls.SelectedIndices.Length==0) {
						MsgBox.Show(this,"Please select a recall first first.");
						return;
					}
					SeeChart_Click(recall.PatNum);
					break;
				case 2:
					MakeAppointment_Click();
					break;
				case 3:
					RemoveRecalls_Click();
					break;
			}
		}
	}

	private void SelectPatient_Click(long patnum) {
		var patient=Patients.GetPat(patnum);//If multiple selected, just take the last one to remain consistent with SendPinboard_Click.
		GlobalFormOpenDental.PatientSelected(patient,isRefreshCurModule: true);
	}

	///<summary>If multiple patients are selected in the list, it will use the last patient to show the chart for.</summary>
	private void SeeChart_Click(long patnum) {
		var patient=Patients.GetPat(patnum); //If multiple selected, just use the last one.
		GlobalFormOpenDental.PatientSelected(patient,isRefreshCurModule: false); //Selects the patient in OpenDental.
		GlobalFormOpenDental.GoToModule(EnumModuleType.Chart,patNum:patient.PatNum);
	}

	private void butText_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.TextMessageSend)) {
			return;
		}
		var clinic=Clinics.GetClinic(Clinics.ClinicNum)??Clinics.GetDefaultForTexting()??Clinics.GetPracticeAsClinicZero();
		List<PatComm> listPatCommsToSend;
		if(tabControl.SelectedIndex==0) {//Appt Tab selected
			Func<int,long> getPatNumFromGridRow=(rowIdx) =>	{
				return ((Appointment)gridAppts.ListGridRows[rowIdx].Tag).PatNum;
			};
			listPatCommsToSend=GetPatCommsForTexting(clinic,gridAppts,_listAppointmentsASAP.Select(x => x.PatNum).ToList(),getPatNumFromGridRow);
		}
		else {//Recall tab selected
			var listRecalls=new List<Recall>();
			for(var i=0;i<gridRecalls.ListGridRows.Count;i++) {
				listRecalls.Add((Recall)gridRecalls.ListGridRows[i].Tag);
			}
			Func<int,long> getPatNumFromGridRow=(rowIdx) =>	{
				return ((Recall)gridRecalls.ListGridRows[rowIdx].Tag).PatNum;
			};
			listPatCommsToSend=GetPatCommsForTexting(clinic,gridRecalls,listRecalls.Select(x => x.PatNum).ToList(),getPatNumFromGridRow);
		}
		if(listPatCommsToSend.Count==0) {
			return;
		}
		var textTemplate=GetTextMessageText(clinic);
		using var formTxtMsgMany=new FormTxtMsgMany(listPatCommsToSend,textTemplate,clinic.Id,SmsMessageSource.AsapManual);
		formTxtMsgMany.ShowDialog();
	}

	private List<PatComm> GetPatCommsForTexting(ClinicDto clinic,GridOD grid,List<long> listPatNumsInGrid,Func<int,long> getPatNumFromGridRow) {
		var listPatComms=Patients.GetPatComms(listPatNumsInGrid,clinic,false);
		var listPatCommsToSend=new List<PatComm>();
		if(grid.SelectedIndices.Length==0) {//None selected. Select all that can be texted.
			for(var i=0;i<listPatNumsInGrid.Count;i++) {
				var patCommForAppt=listPatComms.FirstOrDefault(x => x.PatNum==listPatNumsInGrid[i]);
				if(patCommForAppt==null || !patCommForAppt.IsSmsAnOption) {
					continue;
				}
				grid.SetSelected(i,true);
			}
			if(grid.SelectedIndices.Length==0) {
				MsgBox.Show(this,"None of the patients in the list are able to receive text messages.");
				return listPatCommsToSend;
			}
		}
		//deselect the ones that are not okay to send texts to.
		var listPatsSkipped=new List<string>();
		var numRowsSelected=grid.SelectedIndices.Length;
		for(var i=grid.SelectedIndices.Length-1;i>=0;i--) {//Backwards since we are deselecting rows.
			var patCommForAppt=listPatComms.FirstOrDefault(x => x.PatNum==getPatNumFromGridRow(grid.SelectedIndices[i]));
			if(patCommForAppt!=null && patCommForAppt.IsSmsAnOption) {
				listPatCommsToSend.Add(patCommForAppt);
				continue;
			}
			//Cannot send text message to this patient
			if(patCommForAppt==null) {//Shouldn't happen
				listPatsSkipped.Add(Lan.g(this,"Unknown patient")+": "+Lan.g(this,"Cannot find contact info"));
			}
			else {
				listPatsSkipped.Add(patCommForAppt.FName+" "+patCommForAppt.LName+": "+Lan.g(this,patCommForAppt.GetReasonCantText()));
			}
			grid.SetSelected(grid.SelectedIndices[i],false);
		}
		if(listPatsSkipped.Count > 0) {
			ODMessageBox.Show(listPatsSkipped.Count+" "+Lan.g(this,"of the")+" "+numRowsSelected+" "
			                  +Lan.g(this,"selected patients cannot receive text messages and have been deselected:")+"\r\n"+string.Join("\r\n",listPatsSkipped));
		}
		return listPatCommsToSend;
	}		

	///<summary>Gets the template for this clinic and fills in the tags.</summary>
	private string GetTextMessageText(ClinicDto curClinic) {
		string textTemplate;
		var clinicPref=ClinicPrefs.GetPref(PrefName.ASAPTextTemplate,Clinics.ClinicNum);
		if(clinicPref==null) {
			textTemplate=PrefC.GetString(PrefName.ASAPTextTemplate);
		}
		else {
			textTemplate=clinicPref.ValueString;
		}
		textTemplate=textTemplate.Replace("[OfficeName]",Clinics.GetOfficeName(curClinic));
		textTemplate=textTemplate.Replace("[OfficePhone]",TelephoneNumbers.ReFormat(Clinics.GetOfficePhone(curClinic)));
		if(DateTimeChosen.Year > 1880) {//The user clicked on the appt schedule to get here.
			textTemplate=textTemplate.Replace("[Date]",DateTimeChosen.ToString(PrefC.PatientCommunicationDateFormat));
			textTemplate=textTemplate.Replace("[Time]",DateTimeChosen.ToString(PrefC.PatientCommunicationTimeFormat));
			return textTemplate;
		}
		var listInputBoxParams=new List<InputBoxParam>();
		if(textTemplate.Contains("[Date]")) {
			var inputBoxParam=new InputBoxParam();
			inputBoxParam.InputBoxType_=InputBoxType.ValidDate;
			inputBoxParam.LabelText="Enter date for available time";
			listInputBoxParams.Add(inputBoxParam);
		}
		if(textTemplate.Contains("[Time]")) {
			var inputBoxParam=new InputBoxParam();
			inputBoxParam.InputBoxType_=InputBoxType.ValidTime;
			inputBoxParam.LabelText="Enter time for available time";
			listInputBoxParams.Add(inputBoxParam);
		}
		if(listInputBoxParams.IsNullOrEmpty()){
			return textTemplate;
		}
		var inputBox=new InputBox(listInputBoxParams);
		inputBox.ShowDialog();
		if(inputBox.IsDialogOK) {
			if(textTemplate.Contains("[Date]") && inputBox.DateResult.Year > 1880) {
				textTemplate=textTemplate.Replace("[Date]",inputBox.DateResult.ToString(PrefC.PatientCommunicationDateFormat));
			}
			if(textTemplate.Contains("[Time]") && inputBox.TimeSpanResult!=TimeSpan.Zero){//A time was entered.
				//convert TimeSpan obj to DateTime obj so that PatientCommunicationTimeFormat pref can set custom format
				var dateTime=DateTime.MinValue+inputBox.TimeSpanResult;
				textTemplate=textTemplate.Replace("[Time]",dateTime.ToString(PrefC.PatientCommunicationTimeFormat));
			}
		}
		return textTemplate;
	}

	private void butRefresh_Click(object sender,EventArgs e) {
		FillGridAppts();
		FillGridRecalls();
	}

	private void menuItemSettings_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.Setup)) {
			return;
		}
		using var formAsapSetup=new FormAsapSetup();
		formAsapSetup.ShowDialog();
		SecurityLogs.MakeLogEntry(EnumPermType.Setup,0,"ASAP List Setup");
	}

	private void butPrint_Click(object sender,EventArgs e) {
		_pagesPrinted=0;	
		_doPrintHeading=false;
		PrinterL.TryPrintOrDebugRpPreview(pd_PrintPage,Lan.g(this,"ASAP list printed"),PrintoutOrientation.Landscape);
	}

	private void pd_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
		var bounds=e.MarginBounds;
		//new Rectangle(50,40,800,1035);//Some printers can handle up to 1042
		var g=e.Graphics;
		string text;
		using var headingFont=new Font("Arial",13,FontStyle.Bold);
		using var subHeadingFont=new Font("Arial",10,FontStyle.Bold);
		var y=bounds.Top;
		var center=bounds.X+bounds.Width/2;
		#region printHeading
		var headingPrintH=0;
		if(!_doPrintHeading) {
			if(tabControl.SelectedIndex==0) {
				text=Lan.g(this,"ASAP Appointment List");
			}
			else {
				text=Lan.g(this,"ASAP Recall List");
			}
			g.DrawString(text,headingFont,Brushes.Black,center-g.MeasureString(text,headingFont).Width/2,y);
			//yPos+=(int)g.MeasureString(text,headingFont).Height;
			//text=textDateFrom.Text+" "+Lan.g(this,"to")+" "+textDateTo.Text;
			//g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			y+=25;
			_doPrintHeading=true;
			headingPrintH=y;
		}
		#endregion
		if(tabControl.SelectedIndex==0) {
			y=gridAppts.PrintPage(g,_pagesPrinted,bounds,headingPrintH);
		}
		else {
			y=gridRecalls.PrintPage(g,_pagesPrinted,bounds,headingPrintH);
		}
		_pagesPrinted++;
		if(y==-1) {
			e.HasMorePages=true;
		}
		else {
			e.HasMorePages=false;
		}
	}

	private void comboShowHygiene_SelectionChangeCommitted(object sender,EventArgs e) {
		FillGridAppts();
	}
	
}