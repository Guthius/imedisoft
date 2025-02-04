using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MigraDoc.DocumentObjectModel;
using OpenDental.Bridges;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.HL7;
using PdfSharp.Pdf;
using CodeBase;
using System.Globalization;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.Forms;
using OpenDental.Logic;

namespace OpenDental;

///<summary>_appointment.AptNum cannot be trusted fully inside of this form. This form can create new appointments without inserting them into the DB. Due to this, make sure you consider new appointments and handle accordingly. See _isInsertRequired. Edit window for appointments.  Will have a DialogResult of Cancel if the appointment was marked as new and is deleted.</summary>
public partial class FormApptEdit:FormODBase {
	#region Fields - Public
	///<summary>Procedure were attached/detached from appt and the user clicked cancel or closed the form.  Used in ApptModule to tell if we need to refresh.</summary>
	public bool HasProcsChangedAndCancel;
	///<summary>This is the way to pass a "signal" up to the parent form that OD is to close.</summary>
	public bool IsEcwCloseOD;
	///<summary>True if appt was double clicked on from the chart module gridProg.  Currently only used to trigger an appointment overlap check.</summary>
	public bool IsInChartModule;
	///<summary>True if appt was double clicked on from the ApptsOther form.  Currently only used to trigger an appointment overlap check.</summary>
	public bool IsInViewPatAppts;
	public bool IsNew;
	public bool PinClicked;
	public bool PinIsVisible;
	#endregion Fields - Public

	#region Fields - Private
	private Appointment _appointment;
	private Appointment _appointmentOld;
	///<summary>Used when FormApptBreak is required to track what the user has selected.</summary>
	private ApptBreakSelection _apptBreakSelection=ApptBreakSelection.None;
	private Family _family;
	private bool _isClickLocked;
	private bool _isDeleted;
	///<summary>eCW Tight or Full enabled and a DFT msg for this appt has already been sent.  The 'Finish &amp; Send' button will say 'Revise'</summary>
	private bool _isEcwHL7Sent=false;
	///<summary>If no aptNum was passed into this form, this boolean will be set to true to indicate that _appointment.AptNum cannot be trusted until after the insert occurs. Someday we should consider using the IsNew flag instead after we remove all of the appointment pre-insert logic.</summary>
	private bool _isInsertRequired=false;
	///<summary>Used when first loading the form to skip calling fill methods multiple times.</summary>
	private bool _isOnLoad;
	private bool _isPlanned;
	///<summary>Indicates this appointment has been opened from the Unscheduled list.</summary>
	private bool _isSchedulingUnscheduledAppt;
	///<summary>Labs for the current appointment.</summary>
	private List<LabCase> _listLabCases;
	///<summary>A list of all Adjustments that are related to the patient's current procedures</summary>
	private List<Adjustment> _listAdjustments;
	///<summary>A list of all appointments that are associated to any procedures in the Procedures on this Appointment grid.</summary>
	private List<Appointment> _listAppointments;
	///<summary>Stale deep copy of _listAppointments to use with sync.</summary>
	private List<Appointment> _listAppointmentsOld;
	private List<Benefit> _listBenefits;
	///<summary>A list of all ClaimProcs that are related to the patient's current procedures</summary>
	private List<ClaimProc> _listClaimProcs;
	private List<Def> _listDefsApptProcsQuickAdd;
	private List<InsPlan> _listInsPlans;
	private List<InsSub> _listInsSubs;
	private List<PatPlan> _listPatPlans;
	///<summary>List of all procedures that show within the Procedures on this Appointment grid.  Filled on load.  Used to double check that we update other appointments that we could steal procedures from (e.g. planned appts with tp procs).</summary>
	private List<Procedure> _listProceduresForAppointment;
	///<summary>All ProcNums attached to the appt when form opened.</summary>
	private List<long> _listProcNumsAttachedStart= [];
	///<summary>All ProcNums intended to be selected on load, but without altering any procedure properties.</summary>
	private List<long> _listProcNumsPreSelected;
	///<summary>The data necessary to load the form.</summary>
	private ApptEdit.LoadData _loadData;
	private Patient _patient;
	private ProcedureCode _procedureCodeBroken=null;
	private DataTable _tableComms;
	private DataTable _tableFields;
	///<summary>Timer prevents accidental clicks by delaying interaction with gridProc, listQuickAdd, and butAttachAll based on PrefName.FormClickDelay value. Disposed.</summary>
	private System.Windows.Forms.Timer _timerLockDelay;
	#endregion Fields - Private

	#region Constructor
	///<summary>When aptNum is 0, make sure to set a valid patNum because a new appointment will be created/inserted on OK click.
	///Set useApptDrawingSettings to true if the user double clicked on the appointment schedule in order to make a new appointment.
	///listPreSelectedProcNums is used to preselect procs in the grid without pre-altering the procs properties, such as AptNum/PlannedAptNum</summary>
	public FormApptEdit(long aptNum,long patNum = 0,bool useApptDrawingSettings = false,Patient patient = null,
		List<long> listProcNumsPreSelected = null,DateTime? dateTNew = null,long? opNumNew = null) 
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		_isClickLocked=true;
		if(aptNum==0) {//Creating a new appointment
			_isInsertRequired=true;
			var patientCur=patient??Patients.GetPat(patNum);
			if(patientCur==null) {
				MsgBox.Show(this,"Invalid patient passed in.  Please call support or try again.");
				DialogResult=DialogResult.Cancel;
				if(!this.Modal) {
					Close();
				}
				return;
			}
			//not really needed, but makes it more obvious that these two parameters are not null
			_appointment=AppointmentL.MakeNewAppointment(patientCur,useApptDrawingSettings,dateTNew,opNumNew);
		}
		else {
			_appointment=Appointments.GetOneApt(aptNum);//We need this query to get the PatNum for the appointment.
		}
		_listProcNumsPreSelected=listProcNumsPreSelected;
		this.contrApptProvSlider.FormApptEdit_CheckTimeLocked=checkTimeLocked;
		//if(/* ODBuild.IsDebug() */ false && Environment.MachineName.ToLower()=="jordanhome") {
		//	textNote.RightClickLinks=true;
		//}
	}
	#endregion Constructor
		
	#region Methods - Event Handlers - Standard
	private void FormApptEdit_Load(object sender,System.EventArgs e) {
		if(_appointment==null) {//Can happen if appointment was deleted by another WS.
			MsgBox.Show(this,"Appointment no longer exists.");
			DialogResult=DialogResult.Cancel;
			if(!this.Modal) {
				Close();
			}
			return;
		}
		var appointmentTypeNum=_appointment.AppointmentTypeNum;
		if(PrefC.GetBool(PrefName.AppointmentTypeShowPrompt) && IsNew
		                                                     && !_appointment.AptStatus.In(ApptStatus.PtNote,ApptStatus.PtNoteCompleted)) {
			using var formApptTypes=new FormApptTypes();
			formApptTypes.IsSelectionMode=true;
			formApptTypes.IsNoneAllowed=true;
			formApptTypes.ShowDialog();
			if(formApptTypes.SelectedAppointmentType!=null) {
				appointmentTypeNum=formApptTypes.SelectedAppointmentType.AppointmentTypeNum;
			}
		}
		_isOnLoad=true;
		_timerLockDelay=new Timer();
		_timerLockDelay.Tick+=timerLockDelay_Tick;
		_timerLockDelay.Interval=Math.Max((int)(TimeSpan.FromSeconds(PrefC.GetDouble(PrefName.FormClickDelay,doUseEnUsFormat: true)).TotalMilliseconds),1);
		_timerLockDelay.Start();
		_loadData=ApptEdit.GetLoadData(_appointment);
		_listProceduresForAppointment=_loadData.ListProceduresForAppointment;
		_listAppointments=_loadData.ListAppointments;
		if(_listAppointments.Find(x => x.AptNum==_appointment.AptNum)==null) {
			_listAppointments.Add(_appointment);//Add _appointment if there are no procs attached to it.
		}
		_listAppointmentsOld=_listAppointments.Select(x => x.Copy()).ToList();
		for(var i = 0;i<_listAppointments.Count;i++) {
			if(_listAppointments[i].AptNum==_appointment.AptNum) {
				_appointment=_listAppointments[i];//Changing the variable pointer so all changes are done on the element in the list.
			}
		}
		_appointmentOld=_appointment.Copy();
		if(IsNew) {
			if(!Security.IsAuthorized(EnumPermType.AppointmentCreate)) { //Should have been checked before appointment was inserted into DB and this form was loaded.  Left here just in case.
				DialogResult=DialogResult.Cancel;
				if(!this.Modal) {
					Close();
				}
				return;
			}
		}
		else {
			//The order of the conditional matters; C# will not evaluate the second part of the conditional if it is not needed. 
			//Changing the order will cause unneeded Security MsgBoxes to pop up.
			if(_appointment.AptStatus!=ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentEdit)
			   || (_appointment.AptStatus==ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentCompleteEdit))) {//completed apts have their own perm.
				butSave.Enabled=false;
				butPin.Enabled=false;
				butTask.Enabled=false;
				gridProc.Enabled=false;
				listQuickAdd.Enabled=false;
				butAdd.Enabled=false;
				butDeleteProc.Enabled=false;
				butInsPlan1.Enabled=false;
				butInsPlan2.Enabled=false;
				butComplete.Enabled=false;
			}
			if(_appointment.AptStatus!=ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentDelete,suppressMessage: true)) {//Suppress message because it would be very annoying to users.
				butDelete.Enabled=false;
			}
			if(_appointment.AptStatus==ApptStatus.Complete && !Security.IsAuthorized(EnumPermType.AppointmentCompleteDelete,suppressMessage: true)) {//Suppress message because it would be very annoying to users.
				butDelete.Enabled=false;
			}
		}
		if(!Security.IsAuthorized(EnumPermType.ApptConfirmStatusEdit,suppressMessage: true)) {//Suppress message because it would be very annoying to users.
			comboConfirmed.Enabled=false;
		}
		else if(_isSchedulingUnscheduledAppt) {//User is authorized for Permissions.ApptConfirmStatusEdit.
			//Causes the confirmation status to be reset in the UI, mimics ContrAppt.pinBoard_MouseUp(...)
			_appointment.Confirmed=Defs.GetFirstForCategory(DefCat.ApptConfirmed,isShort: true).DefNum;
		}
		//The objects below are needed when adding procs to this appt.
		_family=_loadData.Family;
		_patient=_family.GetPatient(_appointment.PatNum);
		_listPatPlans=_loadData.ListPatPlans;
		_listBenefits=_loadData.ListBenefits;
		_listInsSubs=_loadData.ListInsSubs;
		_listInsPlans=_loadData.ListInsPlans;
		if(!PatPlans.IsPatPlanListValid(_listPatPlans,listInsSubs: _listInsSubs)) {
			_listPatPlans=PatPlans.Refresh(_appointment.PatNum);
			_listInsSubs=InsSubs.RefreshForFam(_family);
			_listInsPlans=InsPlans.RefreshForSubList(_listInsSubs);
		}
		_tableFields=_loadData.TableApppointmentFields;
		_tableComms=_loadData.TableComms;
		_listAdjustments=_loadData.ListAdjustments;
		_listClaimProcs=_loadData.ListClaimProcs;
		_listLabCases=_loadData.ListLabCases;
		if(!PinIsVisible) {
			butPin.Visible=false;
		}
		var titleText = this.Text;
		_isPlanned=false;
		if(_appointment.AptStatus==ApptStatus.Planned) {
			_isPlanned=true;
			titleText=Lan.g(this,"Edit Planned Appointment")+" - "+_patient.GetNameFL();
			labelStatus.Visible=false;
			comboStatus.Visible=false;
			butDelete.Visible=false;
			if(_listAppointments.FindAll(x => x.NextAptNum==_appointment.AptNum)//This planned appt is attached to a completed appt.
			   .Exists(x => x.AptStatus==ApptStatus.Complete)) {
				labelPlannedComplete.Visible=true;
			}
		}
		else if(_appointment.AptStatus==ApptStatus.PtNote) {
			labelApptNote.Text="Patient NOTE:";
			titleText=Lan.g(this,"Edit Patient Note")+" - "+_patient.GetNameFL()+" on "+_appointment.AptDateTime.DayOfWeek+", "+_appointment.AptDateTime;
			comboStatus.Items.Add(Lan.g("enumApptStatus","Patient Note"),ApptStatus.PtNote);
			comboStatus.Items.Add(Lan.g("enumApptStatus","Completed Pt. Note"),ApptStatus.PtNoteCompleted);
			labelQuickAdd.Visible=false;
			labelStatus.Visible=false;
			gridProc.Visible=false;
			listQuickAdd.Visible=false;
			butAdd.Visible=false;
			butDeleteProc.Visible=false;
			butAttachAll.Visible=false;
			//textNote.Width = 400;
		}
		else if(_appointment.AptStatus==ApptStatus.PtNoteCompleted) {
			labelApptNote.Text="Completed Patient NOTE:";
			titleText=Lan.g(this,"Edit Completed Patient Note")+" - "+_patient.GetNameFL()+" on "+_appointment.AptDateTime.DayOfWeek+", "+_appointment.AptDateTime;
			comboStatus.Items.Add(Lan.g("enumApptStatus","Patient Note"),ApptStatus.PtNote);
			comboStatus.Items.Add(Lan.g("enumApptStatus","Completed Pt. Note"),ApptStatus.PtNoteCompleted);
			labelQuickAdd.Visible=false;
			labelStatus.Visible=false;
			gridProc.Visible=false;
			listQuickAdd.Visible=false;
			butAdd.Visible=false;
			butDeleteProc.Visible=false;
			butAttachAll.Visible=false;
			//textNote.Width = 400;
		}
		else {
			titleText=Lan.g(this,"Edit Appointment")+" - "+_patient.GetNameFL()+" on "+_appointment.AptDateTime.DayOfWeek+", "+_appointment.AptDateTime;
			comboStatus.Items.Add(Lan.g("enumApptStatus","Scheduled"),ApptStatus.Scheduled);
			comboStatus.Items.Add(Lan.g("enumApptStatus","Complete"),ApptStatus.Complete);
			comboStatus.Items.Add(Lan.g("enumApptStatus","UnschedList"),ApptStatus.UnschedList);
			comboStatus.Items.Add(Lan.g("enumApptStatus","Broken"),ApptStatus.Broken);
		}
		SetAptCurComboStatusSelection();
		if(_appointment.Op != 0) {
			titleText+=" | "+Operatories.GetAbbrev(_appointment.Op);
		}
		this.Text = titleText;
		contrApptProvSlider.ProvBarText=_appointment.ProvBarText;
		checkASAP.Checked=_appointment.Priority==ApptPriority.ASAP;
		if(_appointment.AptStatus==ApptStatus.UnschedList) {
			if(Programs.UsingEcwTightOrFullMode()) {
				comboStatus.Enabled=true;
			}
			else if(HL7Defs.GetOneDeepEnabled()!=null && !HL7Defs.GetOneDeepEnabled().ShowAppts) {
				comboStatus.Enabled=true;
			}
			else {
				comboStatus.Enabled=false;
			}
		}
		comboUnschedStatus.Items.AddDefNone();
		comboUnschedStatus.Items.AddDefs(Defs.GetDefsForCategory(DefCat.RecallUnschedStatus,isShort: true));
		comboUnschedStatus.SetSelectedDefNum(_appointment.UnschedStatus);
		comboConfirmed.Items.AddDefs(Defs.GetDefsForCategory(DefCat.ApptConfirmed,isShort: true));
		comboConfirmed.SetSelectedDefNum(_appointment.Confirmed);
		checkTimeLocked.Checked=_appointment.TimeLocked;
		textNote.Text=_appointment.Note;
		_listDefsApptProcsQuickAdd=Defs.GetDefsForCategory(DefCat.ApptProcsQuickAdd,isShort: true);
		for(var i = 0;i<_listDefsApptProcsQuickAdd.Count;i++) {
			listQuickAdd.Items.Add(_listDefsApptProcsQuickAdd[i].ItemName);
		}
		comboClinic.ClinicNumSelected=_appointment.ClinicNum;
		FillCombosProv();
		comboProv.SetSelectedProvNum(_appointment.ProvNum);
		comboProvHyg.SetSelectedProvNum(_appointment.ProvHyg);//ok if 0
		checkIsHygiene.Checked=_appointment.IsHygiene;
		//Fill comboAssistant with employees and none option
		comboAssistant.Items.AddNone<Employee>();
		var listEmployees=Employees.GetDeepCopy(shortList: true);
		comboAssistant.Items.AddList(listEmployees,x=>x.FName);
		if(_appointment.Assistant==0) {
			comboAssistant.SetSelected(0);
		}
		else {
			var index=listEmployees.FindIndex(x => x.EmployeeNum==_appointment.Assistant);
			comboAssistant.SetSelected(index+1);//+1 to skip none
		}
		textLabCase.Text=GetLabCaseDescript();
		textTimeArrived.ContextMenu=contextMenuTimeArrived;
		textTimeSeated.ContextMenu=contextMenuTimeSeated;
		textTimeDismissed.ContextMenu=contextMenuTimeDismissed;
		if(_appointment.DateTimeAskedToArrive.TimeOfDay>TimeSpan.FromHours(0)) {
			textTimeAskedToArrive.Text=_appointment.DateTimeAskedToArrive.ToShortTimeString();
		}
		if(_appointment.DateTimeArrived.TimeOfDay>TimeSpan.FromHours(0)) {
			textTimeArrived.Text=_appointment.DateTimeArrived.ToShortTimeString();
		}
		if(_appointment.DateTimeSeated.TimeOfDay>TimeSpan.FromHours(0)) {
			textTimeSeated.Text=_appointment.DateTimeSeated.ToShortTimeString();
		}
		if(_appointment.DateTimeDismissed.TimeOfDay>TimeSpan.FromHours(0)) {
			textTimeDismissed.Text=_appointment.DateTimeDismissed.ToShortTimeString();
		}
		if(_appointment.AptStatus==ApptStatus.Complete
		   || _appointment.AptStatus==ApptStatus.Broken
		   || _appointment.AptStatus==ApptStatus.PtNote
		   || _appointment.AptStatus==ApptStatus.PtNoteCompleted) {
			textInsPlan1.Text=InsPlans.GetCarrierName(_appointment.InsPlan1,_listInsPlans);
			textInsPlan2.Text=InsPlans.GetCarrierName(_appointment.InsPlan2,_listInsPlans);
		}
		else {//Get the current ins plans for the patient.
			butInsPlan1.Enabled=false;
			butInsPlan2.Enabled=false;
			var insSub1=InsSubs.GetSub(PatPlans.GetInsSubNum(_listPatPlans,PatPlans.GetOrdinal(PriSecMed.Primary,_listPatPlans,_listInsPlans,_listInsSubs)),_listInsSubs);
			var insSub2=InsSubs.GetSub(PatPlans.GetInsSubNum(_listPatPlans,PatPlans.GetOrdinal(PriSecMed.Secondary,_listPatPlans,_listInsPlans,_listInsSubs)),_listInsSubs);
			_appointment.InsPlan1=insSub1.PlanNum;
			_appointment.InsPlan2=insSub2.PlanNum;
			textInsPlan1.Text=InsPlans.GetCarrierName(_appointment.InsPlan1,_listInsPlans);
			textInsPlan2.Text=InsPlans.GetCarrierName(_appointment.InsPlan2,_listInsPlans);
		}
		//IsNewPatient is set well before opening this form.
		checkIsNewPatient.Checked=_appointment.IsNewPatient;
		butColor.BackColor=_appointment.ColorOverride;
		contrApptProvSlider.MinPerIncr=PrefC.GetInt(PrefName.AppointmentTimeIncrement);
		butComplete.Visible=false;
		butPDF.Visible=false;
		//Hide text message button sometimes
		if(_patient.WirelessPhone=="" || (!Programs.IsEnabled(ProgramName.CallFire) && !SmsPhones.IsIntegratedTextingEnabled())) {
			butText.Enabled=false;
		}
		else {//Pat has a wireless phone number and CallFire is enabled
			butText.Enabled=true;//TxtMsgOk checking performed on button click.
		}
		//AppointmentType
		var listAppointmentTypes=AppointmentTypes.GetWhere(x => !x.IsHidden || x.AppointmentTypeNum==_appointment.AppointmentTypeNum);
		comboApptType.Items.AddNone<AppointmentType>();
		comboApptType.SelectedIndex=0;
		comboApptType.Items.AddList(listAppointmentTypes,x=>x.AppointmentTypeName);
		comboApptType.SetSelectedKey<AppointmentType>(appointmentTypeNum,x=>x.AppointmentTypeNum,x=>AppointmentTypes.GetName(x));
		HasProcsChangedAndCancel=false;
		FillProcedures();
		if(IsNew && comboApptType.SelectedIndex!=0) {
			AptTypeHelper();
		}
		//if this is a new appointment with no procedures attached, set the time pattern using the default preference
		else if(IsNew && gridProc.SelectedIndices.Length < 1) {
			_appointment.Pattern=Appointments.GetApptTimePatternForNoProcs();
		}
		contrApptProvSlider.Pattern=_appointment.Pattern;
		contrApptProvSlider.PatternSecondary=_appointment.PatternSecondary;
		FillPatient();//Must be after FillProcedures(), so that the initial amount for the appointment can be calculated.
		SetTimeSliderColors();
		var userOdPrefShowAutoCommlog=UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum,UserOdFkeyType.ShowAutomatedCommlog).FirstOrDefault();
		if(userOdPrefShowAutoCommlog==null) {
			checkShowCommAuto.Checked=true;
		}
		else {
			checkShowCommAuto.Checked=SIn.Bool(userOdPrefShowAutoCommlog.ValueString);
		}
		FillComm();
		FillFields();
		textNote.Focus();
		textNote.SelectionStart = 0;
		_isOnLoad=false;
	}

	private void checkASAP_CheckedChanged(object sender,EventArgs e) {
		if(checkASAP.Checked) {
			checkASAP.ForeColor=System.Drawing.Color.Red;
		}
		else {
			checkASAP.ForeColor=SystemColors.ControlText;
		}
	}

	private void checkIsHygiene_Click(object sender,EventArgs e) {
		SetTimeSliderColors();
	}

	///<summary>Uses the UserODPref to store ShowAutomatedCommlog separately from the chart module.</summary>
	private void checkShowCommAuto_Click(object sender,EventArgs e) {
		var userOdPrefShowAutoCommlog=UserOdPrefs.GetFirstOrNewByUserAndFkeyType(Security.CurUser.UserNum,UserOdFkeyType.ShowAutomatedCommlog);
		userOdPrefShowAutoCommlog.ValueString=SOut.Bool(checkShowCommAuto.Checked);
		UserOdPrefs.Upsert(userOdPrefShowAutoCommlog);
		DataValid.SetInvalid(InvalidType.UserOdPrefs);
		//refresh the data
		FillComm();
	}

	private void checkTimeLocked_Click(object sender,EventArgs e) {
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
	}

	///<summary>Only catches user changes, not programatic changes. For instance this does not fire when loading the form.</summary>
	private void comboApptType_SelectionChangeCommitted(object sender,EventArgs e) {
		if(!AptTypeHelper()) {
			comboApptType.SetSelectedKey<AppointmentType>(_appointment.AppointmentTypeNum,x=>x.AppointmentTypeNum,x=>AppointmentTypes.GetName(x));
			return;
		}
	}

	private void ComboClinic_SelectionChangeCommitted(object sender,EventArgs e) {
		FillCombosProv();
		FillProcedures();
	}

	private void comboConfirmed_SelectionChangeCommitted(object sender,EventArgs e) {
		if(PrefC.GetLong(PrefName.AppointmentTimeArrivedTrigger)!=0 //Using appointmentTimeArrivedTrigger preference
		   && comboConfirmed.GetSelectedDefNum()==PrefC.GetLong(PrefName.AppointmentTimeArrivedTrigger) //selected index matches pref
		   && string.IsNullOrWhiteSpace(textTimeArrived.Text))//time not already set 
		{
			textTimeArrived.Text=DateTime.Now.ToShortTimeString();
		}
		if(PrefC.GetLong(PrefName.AppointmentTimeSeatedTrigger)!=0 //Using AppointmentTimeSeatedTrigger preference
		   && comboConfirmed.GetSelectedDefNum()==PrefC.GetLong(PrefName.AppointmentTimeSeatedTrigger) //selected index matches pref
		   && string.IsNullOrWhiteSpace(textTimeSeated.Text))//time not already set 
		{
			textTimeSeated.Text=DateTime.Now.ToShortTimeString();
		}
		if(PrefC.GetLong(PrefName.AppointmentTimeDismissedTrigger)!=0 //Using AppointmentTimeDismissedTrigger preference
		   && comboConfirmed.GetSelectedDefNum()==PrefC.GetLong(PrefName.AppointmentTimeDismissedTrigger) //selected index matches pref
		   && string.IsNullOrWhiteSpace(textTimeDismissed.Text))//time not already set 
		{
			textTimeDismissed.Text=DateTime.Now.ToShortTimeString();
		}
	}

	private void comboProvHyg_SelectionChangeCommitted(object sender,EventArgs e) {
		SetTimeSliderColors();
	}

	private void comboProv_SelectionChangeCommitted(object sender,EventArgs e) {
		SetTimeSliderColors();
	}

	private void comboStatus_SelectionChangeCommitted(object sender,EventArgs e) {
		//This block of logic must happen first(The if statement).
		if(PrefC.GetBool(PrefName.BrokenApptRequiredOnMove)) {
			if(_appointmentOld.AptStatus==ApptStatus.Scheduled && GetApptStatusSelected()==ApptStatus.UnschedList) {
				var frmApptBreakRequired=new FrmApptBreakRequired();
				frmApptBreakRequired.ShowDialog();
				if(!frmApptBreakRequired.IsDialogOK) {
					return;
				}
				_apptBreakSelection=ApptBreakSelection.Unsched;
				_procedureCodeBroken=frmApptBreakRequired.ProcedureCodeBrokenSelected;
				return;
			}
		}
		if(comboStatus.GetSelected<ApptStatus>()!=ApptStatus.Broken) {
			return;
		}
		if(!AppointmentL.HasBrokenApptProcs()) {
			return;
		}
		//Patient note appointment types can't have a aptstatus of broken.
		if(_appointment.AptStatus==ApptStatus.PtNoteCompleted || _appointment.AptStatus==ApptStatus.PtNote) {
			return;
		}
		if(DoPreventCompletedApptChange(PreventChangesApptAction.Status)) {
			//Change the status back to Complete before returning.
			comboStatus.SelectedIndex=1;//Complete
			return;
		}
		using var formApptBreak=new FormApptBreak(_appointment);
		if(formApptBreak.ShowDialog()!=DialogResult.OK) {
			SetAptCurComboStatusSelection();//Sets status back to on load selection.
			if(formApptBreak.SelectedApptBreak==ApptBreakSelection.Delete) {
				//User wants to delete the appointment.
				OnDelete_Click(isSkipDeletePrompt: true);//Skip the standard "Delete Appointment?" prompt since we have already prompted in FormApptBreak.
				return;
			}
			_apptBreakSelection=ApptBreakSelection.None;
			_procedureCodeBroken=null;
			return;
		}
		_apptBreakSelection=formApptBreak.SelectedApptBreak;
		_procedureCodeBroken=formApptBreak.SelectedProcedureCode;
	}

	private void gridProc_MouseLeave(object sender,EventArgs e) {
		toolTip1.RemoveAll();
	}

	private void listQuickAdd_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
		if(_isClickLocked) {
			return;
		}
		if(comboProv.GetSelectedProvNum()==0) {
			MsgBox.Show(this,"Please select a provider.");
			return;
		}
		if(listQuickAdd.IndexFromPoint(e.Location)==-1) {
			return;
		}
		if(_appointment.AptStatus==ApptStatus.Complete) {
			//added procedures would be marked complete when form closes. We'll just stop it here.
			if(!Security.IsAuthorized(EnumPermType.ProcComplCreate,_appointment.AptDateTime,false)) {
				return;
			}
		}
		var stringArrayProcCodes=_listDefsApptProcsQuickAdd[listQuickAdd.IndexFromPoint(e.Location)].ItemValue.Split(',');
		try {
			//This is the only place where a toothnum is stored in the db in international format
			ProcedureCodes.ValidateProcedureCodeEntry(stringArrayProcCodes,doAllowToothNum: true);
		}
		catch(Exception ex) {
			ODMessageBox.Show(ex.Message);
			return;
		}
		var listProceduresAdded=ApptEdit.QuickAddProcs(_appointment,_patient,stringArrayProcCodes.ToList(),comboProv.GetSelectedProvNum(),comboProvHyg.GetSelectedProvNum(),_listInsSubs,_listInsPlans,_listPatPlans,_listBenefits);
		_listProceduresForAppointment=Procedures.GetProcsForApptEdit(_appointment);
		listQuickAdd.SelectedIndex=-1;
		FillProcedures();
		for(var i = 0;i<gridProc.ListGridRows.Count;i++) {
			//at this point, all procedures in the list should have a Primary Key.
			var procNumCur=((Procedure)gridProc.ListGridRows[i].Tag).ProcNum;
			if(listProceduresAdded.Any(x => x.ProcNum==procNumCur)) {
				gridProc.SetSelected(i,true);//Select those that were just added.
			}
		}
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
		CalcPatientFeeThisAppt();
		RefreshEstPatientPortion();
	}

	private void menuItemArrivedNow_Click(object sender,EventArgs e) {
		textTimeArrived.Text=DateTime.Now.ToShortTimeString();
	}

	private void menuItemDismissedNow_Click(object sender,EventArgs e) {
		textTimeDismissed.Text=DateTime.Now.ToShortTimeString();
	}

	private void menuItemSeatedNow_Click(object sender,EventArgs e) {
		textTimeSeated.Text=DateTime.Now.ToShortTimeString();
	}

	private void timerLockDelay_Tick(object sender,EventArgs e) {
		_isClickLocked=false;
		_timerLockDelay.Stop();
	}
	#endregion Methods - Event Handlers - Standard

	#region Methods - Event Handlers - Click - Center
	//The following 9 methods are ordered by usage in the center of FormApptEdit.
	private void butPickDentist_Click(object sender,EventArgs e) {
		var frmProviderPick=new FrmProviderPick(comboProv.Items.GetAll<ProviderDto>());
		frmProviderPick.ProvNumSelected=comboProv.GetSelectedProvNum();
		frmProviderPick.ShowDialog();
		if(!frmProviderPick.IsDialogOK) {
			return;
		}
		comboProv.SetSelectedProvNum(frmProviderPick.ProvNumSelected);
		SetTimeSliderColors();
	}

	private void butPickHyg_Click(object sender,EventArgs e) {
		var frmProviderPick=new FrmProviderPick(comboProvHyg.Items.GetAll<ProviderDto>());//none option will show.
		frmProviderPick.ProvNumSelected=comboProvHyg.GetSelectedProvNum();
		frmProviderPick.ShowDialog();
		if(!frmProviderPick.IsDialogOK) {
			return;
		}
		comboProvHyg.SetSelectedProvNum(frmProviderPick.ProvNumSelected);
		SetTimeSliderColors();
	}

	private void butColor_Click(object sender,EventArgs e) {
		var colorDialog=new ColorDialog();
		colorDialog.Color=butColor.BackColor;
		colorDialog.ShowDialog();
		butColor.BackColor=colorDialog.Color;
	}

	private void butColorClear_Click(object sender,EventArgs e) {
		butColor.BackColor=System.Drawing.Color.FromArgb(0);
	}

	private void butLab_Click(object sender,EventArgs e) {
		if(_isInsertRequired && !UpdateListAndDB(isClosing: false)) {
			return;
		}
		if(_listLabCases.IsNullOrEmpty()) {//no labcase
			//so let user pick one to add
			using var formLabCaseSelect=new FormLabCaseSelect();
			formLabCaseSelect.PatNum=_appointment.PatNum;
			formLabCaseSelect.IsPlanned=_isPlanned;
			formLabCaseSelect.IsSelectingUnattached=true;
			formLabCaseSelect.ShowDialog();
			if(formLabCaseSelect.DialogResult!=DialogResult.OK) {
				return;
			}
			if(_isPlanned) {
				LabCases.AttachToPlannedAppt(formLabCaseSelect.ListLabCaseNumsSelected,_appointment.AptNum);
			}
			else {
				LabCases.AttachToAppt(formLabCaseSelect.ListLabCaseNumsSelected,_appointment.AptNum);
			}
		}
		else {
			using var formLabCaseSelect=new FormLabCaseSelect();
			formLabCaseSelect.PatNum=_appointment.PatNum;
			formLabCaseSelect.IsPlanned=_isPlanned;
			formLabCaseSelect.IsSelectingUnattached=false;
			formLabCaseSelect.AptNum=_appointment.AptNum;
			formLabCaseSelect.ShowDialog();
		}
		_listLabCases=LabCases.GetForApt(_appointment);
		textLabCase.Text=GetLabCaseDescript();
	}

	private void butInsPlan1_Click(object sender,EventArgs e) {
		using var formInsPlanSelect=new FormInsPlanSelectFam(_appointment.PatNum);
		formInsPlanSelect.ShowNoneButton=true;
		formInsPlanSelect.ViewRelat=false;
		formInsPlanSelect.ShowDialog();
		if(formInsPlanSelect.DialogResult!=DialogResult.OK) {
			return;
		}
		if(formInsPlanSelect.InsPlanSelected==null) {
			_appointment.InsPlan1=0;
			textInsPlan1.Text="";
			return;
		}
		_appointment.InsPlan1=formInsPlanSelect.InsPlanSelected.PlanNum;
		textInsPlan1.Text=InsPlans.GetCarrierName(_appointment.InsPlan1,_listInsPlans);
	}

	private void butInsPlan2_Click(object sender,EventArgs e) {
		using var formInsPlanSelect=new FormInsPlanSelectFam(_appointment.PatNum);
		formInsPlanSelect.ShowNoneButton=true;
		formInsPlanSelect.ViewRelat=false;
		formInsPlanSelect.ShowDialog();
		if(formInsPlanSelect.DialogResult!=DialogResult.OK) {
			return;
		}
		if(formInsPlanSelect.InsPlanSelected==null) {
			_appointment.InsPlan2=0;
			textInsPlan2.Text="";
			return;
		}
		_appointment.InsPlan2=formInsPlanSelect.InsPlanSelected.PlanNum;
		textInsPlan2.Text=InsPlans.GetCarrierName(_appointment.InsPlan2,_listInsPlans);
	}

	#endregion Methods - Event Handlers - Click - Center

	#region Methods - Event Handlers - Click - Upper
	//The following 3 methods are ordered by usage in the upper right of FormApptEdit.
	private void butDeleteProc_Click(object sender,EventArgs e) {
		//This button will not be enabled if user does not have permission for AppointmentEdit.
		if(gridProc.SelectedIndices.Length==0) {
			MsgBox.Show(this,"Please select one or more procedures first.");
			return;
		}
		var listProceduresSelected=gridProc.SelectedTags<Procedure>();
		var listAppointmentsEmpty=new List<Appointment>();
		if(PrefC.GetBool(PrefName.ApptsRequireProc)) {
			var listApptNumsSelected=listProceduresSelected.Where(x => x.AptNum!=0).Select(x => x.AptNum).Distinct().ToList();
			if(listApptNumsSelected.Any(x => x!=_appointment.AptNum)) {
				listAppointmentsEmpty=Appointments.GetApptsGoingToBeEmpty(listProceduresSelected,listApptNumsSelected);
				if(listAppointmentsEmpty.Count>0) {
					MsgBox.Show("One or more selected procedures are attached to another appointment.");
					return;
				}
			}
		}
		//If this appointment is of a certain AppointmentType, check for required procedure codes that are going to be deleted.
		if(comboApptType.SelectedIndex>0) {
			var appointmentType=comboApptType.GetSelected<AppointmentType>();
			if(appointmentType.RequiredProcCodesNeeded!=EnumRequiredProcCodesNeeded.None) {
				var listProcCodesRequiredForAppointmentType=appointmentType.CodeStrRequired.Split(",",StringSplitOptions.RemoveEmptyEntries).ToList();//Includes duplicates.
				var listProceduresGrid=gridProc.GetTags<Procedure>();
				listProceduresGrid.RemoveAll(x => x.AptNum!=_appointment.AptNum && x.AptNum!=0);//Remove procs on other appts
				var listProceduresForReqCheck=listProceduresSelected.FindAll(x => x.AptNum!=_appointment.AptNum && x.AptNum!=0);
				var message=Appointments.CheckRequiredProcForApptType(listProceduresForReqCheck);
				if(message!=""){
					MsgBox.Show(message);
					return;
				}
			}
		}
		var wasPrompted=false;
		var canDeletePlannedAppts=false;
		var listPlannedApptNumsSelected=listProceduresSelected.Select(x=>x.PlannedAptNum).Distinct().ToList().FindAll(x=>x!=0);
		listAppointmentsEmpty=Appointments.GetApptsGoingToBeEmpty(listProceduresSelected,isForPlanned:true);
		if(PrefC.GetBool(PrefName.ApptsRequireProc) && listAppointmentsEmpty.Count>0) {
			for(var i=0;i<listAppointmentsEmpty.Count;i++){
				if(listAppointmentsEmpty[i].AptStatus==ApptStatus.Planned && listAppointmentsEmpty[i].AptNum!=_appointment.AptNum){//Is planned and not the appointment being edited
					if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Deleting selected procedures will leave empty planned appointment(s), resulting in deletion.\r\n"
					                                            + "Continue?")) {
						return;
					}
					wasPrompted=true;
					break;
				}
			}
			if(!Security.IsAuthorized(EnumPermType.AppointmentDelete,suppressMessage:false)){
				return;
			}
			canDeletePlannedAppts=true;
		}
		if(!wasPrompted){
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Permanently delete all selected procedure(s)?")) {//deleteMsg can only be of two strings, okay to use in MsgBox.Show()
				return;
			}
		}
		var skipped=0;
		var skippedSecurity=0;
		var skippedLinkedToOrthoCase=0;
		var skippedPreauth=0;
		var isProcDeleted=false;
		var listOrthoProcLinks=OrthoProcLinks.GetManyForProcs(gridProc.ListGridRows.Select(x => ((Procedure)x.Tag).ProcNum).ToList());
		var listSelectedProcNums=gridProc.SelectedTags<Procedure>().Select(x => x.ProcNum).ToList();
		var listClaimProcsForProc=ClaimProcs.GetForProcs(listSelectedProcNums, [ClaimProcStatus.Preauth]).FindAll(x => x.ClaimNum!=0);
		listClaimProcsForProc.DistinctBy(x => x.ClaimNum);//Removes duplicate ClaimNums.
		for(var i = 0;i<listClaimProcsForProc.Count;i++) {
			var listProcNumsForClaim=ClaimProcs.RefreshForClaim(listClaimProcsForProc[i].ClaimNum).Select(x => x.ProcNum).ToList();
			//We block you from deleting all procedures on a preauth, which is consistent with the claim edit window, the chart module, and procedure edit form.
			if(listProcNumsForClaim.Except(listSelectedProcNums).Count()==0) {
				listSelectedProcNums.RemoveAll(x => listProcNumsForClaim.Contains(x));
			}
		}
		for(var i = gridProc.SelectedIndices.Length-1;i>=0;i--) {
			var procedure=(Procedure)gridProc.ListGridRows[gridProc.SelectedIndices[i]].Tag;
			if(!Procedures.IsProcComplDeleteAuthorized(procedure)) {
				listProceduresSelected.Remove(procedure);
				skipped++;
				skippedSecurity++;
				continue;
			}
			if(!procedure.ProcStatus.In(ProcStat.C,ProcStat.EO,ProcStat.EC)
			   && !Security.IsAuthorized(EnumPermType.ProcDelete,Procedures.GetDateForPermCheck(procedure),suppressMessage: true)) {
				listProceduresSelected.Remove(procedure);
				skippedSecurity++;
				continue;
			}
			//If selected procedure.ProcNum is linked to an ortho case, you're not allowed to delete it.
			var orthoProcLink=listOrthoProcLinks.FirstOrDefault(x=>x.ProcNum==procedure.ProcNum);
			if(orthoProcLink!=null) {
				listProceduresSelected.Remove(procedure);
				skippedLinkedToOrthoCase++;
				continue;
			}
			if(!listSelectedProcNums.Contains(procedure.ProcNum)) {
				listProceduresSelected.Remove(procedure);
				skippedPreauth++;
				continue;
			}
			#region Delete Planned Apts that will be empty
			if(skipped>0 || skippedSecurity>0 || skippedLinkedToOrthoCase>0 || skippedPreauth>0) {//If any procs were not deleted. Check for empty again.
				listPlannedApptNumsSelected=listProceduresSelected.Select(x=>x.PlannedAptNum).Distinct().ToList().FindAll(x=>x!=0);
				listAppointmentsEmpty=Appointments.GetApptsGoingToBeEmpty(listProceduresSelected, listPlannedApptNumsSelected);
				listPlannedApptNumsSelected.Remove(_appointment.AptNum);//Do not delete the appointment being edited.
				if(PrefC.GetBool(PrefName.ApptsRequireProc)	&& listAppointmentsEmpty.Count>0) {
					Appointments.DeleteEmptyAppts(listPlannedApptNumsSelected, _appointment.PatNum);
				}
			}
			else{
				if(canDeletePlannedAppts){
					listPlannedApptNumsSelected.Remove(_appointment.AptNum); //Do not delete the appointment being edited.
					Appointments.DeleteEmptyAppts(listPlannedApptNumsSelected, _appointment.PatNum);
				}
			}
			#endregion
			try {
				Procedures.Delete(procedure.ProcNum);
				isProcDeleted=true;
			}
			catch(Exception ex) {
				ODMessageBox.Show(ex.Message);
				break;
			}
			if(procedure.ProcStatus.In(ProcStat.C,ProcStat.EO,ProcStat.EC)) {
				var perm=EnumPermType.ProcCompleteStatusEdit;
				if(procedure.ProcStatus.In(ProcStat.EO,ProcStat.EC)) {
					perm=EnumPermType.ProcExistingEdit;
				}
				SecurityLogs.MakeLogEntry(perm,_appointment.PatNum,ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode
				                                                   +" ("+procedure.ProcStatus+"), "+procedure.ProcFee.ToString("c")+", Deleted");
			}
			else {
				SecurityLogs.MakeLogEntry(EnumPermType.ProcDelete,_appointment.PatNum,ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode
				                                                                      +" ("+procedure.ProcStatus+"), "+procedure.ProcFee.ToString("c"));
			}
		}
		_listProceduresForAppointment=Procedures.GetProcsForApptEdit(_appointment);
		if(isProcDeleted) {
			Appointments.SetProcDescript(_appointment,_listProceduresForAppointment);//This is called in Procedures.Delete(...) but is not reflected in our local _appointment.
			//This is to fix a very rare bug where the user deletes a set of procedures and then re-attaches the same procedures before closing the window.
			//This would cause the DB to have correct ProcDesript and ProcsColored values at the time. But when the user closes the window after reselecting
			//the same proces, the Appointments.Update(old,new) will not update those fields due to them being identical.
			//This would cause the appt bubble to contain the incorrect values.
			_appointmentOld.ProcDescript=_appointment.ProcDescript;
			_appointmentOld.ProcsColored=_appointment.ProcsColored;
		}
		FillProcedures();
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
		CalcPatientFeeThisAppt();
		RefreshEstPatientPortion();
		if(skipped>0) {
			ODMessageBox.Show(Lan.g(this,"Procedures skipped due to lack of permission to edit completed procedures: ")+skipped);
		}
		if(skippedSecurity>0) {
			ODMessageBox.Show(Lan.g(this,"Procedures skipped due to lack of permission to delete procedures: ")+skippedSecurity);
		}
		if(skippedLinkedToOrthoCase>0) {
			ODMessageBox.Show(Lan.g(this,"Procedures skipped because they are linked to one or more ortho cases: ")+skippedLinkedToOrthoCase+"\r"
			                  +"Detach the procedure(s) or delete the ortho case(s) first.");
		}
		if(skippedPreauth>0) {
			ODMessageBox.Show(Lan.g(this,"Procedures skipped because you are not allowed to delete the last procedure attached to a preauthorization: ")+SOut.Int(skippedPreauth)+"\r"
			                  +"Detach the procedure(s) or delete the preauthorization first.");
		}
	}

	private void butAdd_Click(object sender,EventArgs e) {
		if(comboProv.GetSelectedProvNum()==0) {
			MsgBox.Show(this,"Please select a provider.");
			return;
		}
		using var formProcCodes=new FormProcCodes();
		formProcCodes.IsSelectionMode=true;
		formProcCodes.ShowDialog();
		if(formProcCodes.DialogResult!=DialogResult.OK) {
			return;
		}
		var procedureCode=ProcedureCodes.GetProcCode(formProcCodes.CodeNumSelected);
		var listSubstitutionLinks=SubstitutionLinks.GetAllForPlans(_listInsPlans);
		var discountPlanNum=DiscountPlanSubs.GetDiscountPlanNumForPat(_patient.PatNum); //Enforcing the discount plan date is done in Procedures.Insert
		var listFees=Fees.GetListFromObjects([procedureCode],listMedicalCodes: null,//no procs to pull medical codes from yet
			[comboProv.GetSelectedProvNum()],_patient.PriProv,_patient.SecProv,_patient.FeeSched,_listInsPlans, [_appointment.ClinicNum],
			[_appointment],listSubstitutionLinks,discountPlanNum);
		var procedure=Procedures.ConstructProcedureForAppt(formProcCodes.CodeNumSelected,_appointment,_patient,_listPatPlans,_listInsPlans,_listInsSubs,listFees);
		procedure.ProcStatus=ProcStat.D;
		Procedures.Insert(procedure);
		procedure.ProcStatus=ProcStat.TP;
		var listClaimProcs=new List<ClaimProc>();
		var listClaimProcHistsLoop=new List<ClaimProcHist>();
		var listClaimProcsRefresh=ClaimProcs.RefreshForTp(_patient.PatNum);
		var listProcedures=Procedures.GetTpForPats([_patient.PatNum]);
		for(var i = 0;i<listProcedures.Count;i++) {
			if(listProcedures[i].ProcNum==procedure.ProcNum) {
				break;
			}
			listClaimProcHistsLoop.AddRange(ClaimProcs.GetHistForProc(listClaimProcsRefresh,listProcedures[i],listProcedures[i].CodeNum));
		}
		Procedures.ComputeEstimates(procedure,_patient.PatNum,ref listClaimProcs,isInitialEntry: true,_listInsPlans,_listPatPlans,_listBenefits,
			_loadData.ListClaimProcHists,listClaimProcHistsLoop,saveToDb: true,_patient.Age,_listInsSubs,listClaimProcsAll: null,
			isClaimProcRemoveNeeded: false,useProcDateOnProc: false,listSubstitutionLinks,isForOrtho: false,listFees);
		using var formProcEdit=new FormProcEdit(procedure,_patient.Copy(),_family);
		formProcEdit.ListClaimProcHists=_loadData.ListClaimProcHists;
		formProcEdit.ListClaimProcHistsLoop=listClaimProcHistsLoop;
		formProcEdit.IsNew=true;
		formProcEdit.ShowDialog();
		if(formProcEdit.DialogResult==DialogResult.Cancel) {
			//any created claimprocs are automatically deleted from within procEdit window.
			try {
				Procedures.Delete(procedure.ProcNum);//also deletes the claimprocs
			}
			catch(Exception ex) {
				ODMessageBox.Show(ex.Message);
			}
			return;
		}
		_listProceduresForAppointment=Procedures.GetProcsForApptEdit(_appointment);
		FillProcedures();
		for(var i = 0;i<gridProc.ListGridRows.Count;i++) {
			if(procedure.ProcNum==((Procedure)gridProc.ListGridRows[i].Tag).ProcNum) {
				gridProc.SetSelected(i,true);//Select those that were just added.
			}
		}
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
		CalcPatientFeeThisAppt();
		RefreshEstPatientPortion();
	}

	private void butAttachAll_Click(object sender,EventArgs e) {
		if(_isClickLocked) {
			return;
		}
		var listProcedures=gridProc.ListGridRows.Select(x=>(Procedure)x.Tag).ToList();
		for(var i=0;i<listProcedures.Count;i++) {
			if(DisableDetachingOfCompletedProcFromCompletedAppt(listProcedures[i],_appointment,out var msg)) {
				continue;
			}
			gridProc.SetSelected(i,true);
		}
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
		CalcPatientFeeThisAppt();
	}
	#endregion Methods - Event Handlers - Click - Upper

	#region Methods - Event Handlers - ODGridClick
	private void gridComm_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var row=((DataRow)gridComm.ListGridRows[e.Row].Tag);
		var commNum=SIn.Long(row["CommlogNum"].ToString());
		var msgNum=SIn.Long(row["EmailMessageNum"].ToString());
		if (commNum>0) {
			var commlog=Commlogs.GetOne(commNum);
			if(commlog==null) {
				MsgBox.Show(this,"This commlog has been deleted by another user.");
				return;
			}
			var frmCommItem=new FrmCommItem(commlog);
			frmCommItem.ShowDialog();
		}
		else if (msgNum>0) {
			var emailMessage=EmailMessages.GetOne(msgNum);
			if (emailMessage==null) {
				MsgBox.Show(this,"This e-mail has been deleted by another user.");
				return;
			}
			using var formEmailMessageEdit=new FormEmailMessageEdit(emailMessage,isDeleteAllowed:false);
			formEmailMessageEdit.ShowDialog();
		}
		_tableComms=Appointments.GetCommTable(_appointment.PatNum.ToString(),_appointment.AptNum);
		FillComm();
	}

	private void gridPatient_CellClick(object sender,ODGridClickEventArgs e) {
		var gridCell=gridPatient.ListGridRows[e.Row].Cells[e.Col];
		//Only grid cells with phone numbers are blue and underlined.
		if(gridCell.ColorText==System.Drawing.Color.Blue && gridCell.Underline==YN.Yes && Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled) {
			DentalTek.PlaceCall(gridCell.Text);
		}
	}

	private void gridProc_CellClick(object sender,ODGridClickEventArgs e) {
		if(_isClickLocked) {
			return;
		}
		toolTip1.RemoveAll();
		//This grid has AllowSelection=false, so row don't get automatically selected when clicking.
		var procedureSelected=((Procedure)gridProc.ListGridRows[e.Row].Tag);
		if(DisableDetachingOfCompletedProcFromCompletedAppt(procedureSelected,_appointment,out var msg)) {
			toolTip1.AutoPopDelay=5000;//5000 is the maximum a tooltip can be displayed for using this method. 
			toolTip1.IsBalloon=true;//Shows the tooltip in an easier to read speech bubble like view.
			//Anything greater requires us to use a variation of Show(...) that takes in a time duration.
			toolTip1.SetToolTip(gridProc,msg);
			return;
		}
		InvertCurProcSelected(e.Row);
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
		CalcPatientFeeThisAppt();
		CalcEstPatientPortion();
	}

	private void gridProc_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(_isClickLocked) {
			return;
		}
		toolTip1.RemoveAll();//Ensure that any tooltip that was showing due to gridProc_CellClick(...) is removed since user was trying to double click.
		var procedureSelected=((Procedure)gridProc.ListGridRows[e.Row].Tag);
		//Only invert the procedure if we didn't block the original row inversion in gridProc_CellClick(...)
		if(!DisableDetachingOfCompletedProcFromCompletedAppt(procedureSelected,_appointment,out var msg)) {
			InvertCurProcSelected(e.Row);
		}
		//This will put the selection back to what is was before the single click event.
		//Get fresh copy from DB so we are not editing a stale procedure.
		//If this is to be changed, make sure that this window is registering for procedure changes via signals or by some other means.
		var procedure=Procedures.GetOneProc(((Procedure)gridProc.ListGridRows[e.Row].Tag).ProcNum,includeNote: true);
		var listClaimProcHists=new List<ClaimProcHist>();
		var listClaimProcs=ClaimProcs.RefreshForTp(_patient.PatNum);
		var listProcedures=Procedures.GetTpForPats([_patient.PatNum]);
		for(var i = 0;i<listProcedures.Count;i++) {
			if(listProcedures[i].ProcNum==procedure.ProcNum) {
				break;
			}
			listClaimProcHists.AddRange(ClaimProcs.GetHistForProc(listClaimProcs,listProcedures[i],listProcedures[i].CodeNum));
		}
		var aptNumOld=procedure.AptNum;
		using var formProcEdit=new FormProcEdit(procedure,_patient,_family);
		formProcEdit.ListClaimProcHists=_loadData.ListClaimProcHists;
		formProcEdit.ListClaimProcHistsLoop=listClaimProcHists;
		formProcEdit.ShowDialog();
		if(formProcEdit.DialogResult!=DialogResult.OK) {
			CalculatePatternFromProcs();
			//SetTimeSliderColors();
			return;
		}
		//Scenario we are trying to fix:
		//	appt was complete
		//	appt was set to scheduled via status combobox on this form
		//	and before closing form, this procedure was edited.
		//	Problem is that Procedures.Update erroneously detaches the proc from the appt.
		//Our solution is to let it get detached and then reattach below.
		var procedureOld=procedure.Copy();
		if(procedure.AptNum!=aptNumOld){
			procedure.AptNum=aptNumOld;
		}
		//this method guarantees that we are only changing AptNum, even though procedure object is stale.
		Procedures.Update(procedure,procedureOld);
		_listProceduresForAppointment=Procedures.GetProcsForApptEdit(_appointment);//We need to refresh in case the user changed the ProcCode or set the proc complete.
		//The next 3 lines are a duplicate of a section in butDeleteProc to handle deleted procedures.
		Appointments.SetProcDescript(_appointment,_listProceduresForAppointment);
		_appointmentOld.ProcDescript=_appointment.ProcDescript;
		_appointmentOld.ProcsColored=_appointment.ProcsColored;
		FillProcedures();
		CalculatePatternFromProcs();
		//SetTimeSliderColors();
		RefreshEstPatientPortion();//Need to refresh in case the user changed the ProcCode or set the proc complete.
	}

	private void gridFields_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(_isInsertRequired && !UpdateListAndDB(isClosing: false)) {
			return;
		}
		if(ApptFieldDefs.HasDuplicateFieldNames()) {//Check for duplicate field names.
			MsgBox.Show(this,"There are duplicate appointment field defs, go rename or delete the duplicates.");
			return;
		}
		var apptField=ApptFields.GetOne(SIn.Long(_tableFields.Rows[e.Row]["ApptFieldNum"].ToString()));
		if(apptField==null) {
			apptField=new ApptField();
			apptField.IsNew=true;
			apptField.AptNum=_appointment.AptNum;
			apptField.FieldName=_tableFields.Rows[e.Row]["FieldName"].ToString();
			var apptFieldDef=ApptFieldDefs.GetFieldDefByFieldName(apptField.FieldName);
			if(apptFieldDef==null) {//This could happen if the field def was deleted while the appointment window was open.
				MsgBox.Show(this,"This Appointment Field Def no longer exists.");
			}
			else {
				if(apptFieldDef.FieldType==ApptFieldType.Text) {
					var frmApptFieldEdit=new FrmApptFieldEdit(apptField);
					frmApptFieldEdit.ShowDialog();
				}
				else if(apptFieldDef.FieldType==ApptFieldType.PickList) {
					var FrmApptFieldPickEdit=new FrmApptFieldPickEdit(apptField);
					FrmApptFieldPickEdit.ShowDialog();
				}
			}
		}
		else if(ApptFieldDefs.GetFieldDefByFieldName(apptField.FieldName)!=null) {
			if(ApptFieldDefs.GetFieldDefByFieldName(apptField.FieldName).FieldType==ApptFieldType.Text) {
				var frmApptFieldEdit=new FrmApptFieldEdit(apptField);
				frmApptFieldEdit.ShowDialog();
			}
			else if(ApptFieldDefs.GetFieldDefByFieldName(apptField.FieldName).FieldType==ApptFieldType.PickList) {
				var FrmApptFieldPickEdit=new FrmApptFieldPickEdit(apptField);
				FrmApptFieldPickEdit.ShowDialog();
			}
		}
		else {//This probably won't happen because a field def should not be able to be deleted while in use.
			MsgBox.Show(this,"This Appointment Field Def no longer exists.");
		}
		_tableFields=Appointments.GetApptFields(_appointment.AptNum);
		FillFields();
	}
	#endregion Methods - Event Handlers - ODGridClick

	#region Methods - Event Handlers - Click - Right
	//The following 7 methods are ordered by usage on the right side of FormApptEdit.
	private void butAddComm_Click(object sender,EventArgs e) {
		var commlog=new Commlog();
		commlog.IsNew=true;
		commlog.PatNum=_appointment.PatNum;
		commlog.CommDateTime=DateTime.Now;
		commlog.CommType=Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
		commlog.UserNum=Security.CurUser.UserNum;
		var frmCommItem=new FrmCommItem(commlog);
		frmCommItem.DoOmitDefaults=PrefC.GetBool(PrefName.EnterpriseCommlogOmitDefaults);
		frmCommItem.ShowDialog();
		_tableComms=Appointments.GetCommTable(_appointment.PatNum.ToString(),_appointment.AptNum);
		FillComm();
	}

	private void butText_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.TextMessageSend)) {
			return;
		}
		if(!PatientL.CheckPatientTextingAllowed(_patient,this)) {
			return;
		}
		var message=PatComm.BuildConfirmMessage(ContactMethod.TextMessage,_patient,_appointment);
		using var formTxtMsgEdit=new FormTxtMsgEdit();
		formTxtMsgEdit.PatNum=_patient.PatNum;
		formTxtMsgEdit.WirelessPhone=_patient.WirelessPhone;
		formTxtMsgEdit.Message=message;
		formTxtMsgEdit.YNTxtMsgOk=_patient.TxtMsgOk;
		formTxtMsgEdit.ShowDialog();
	}

	private void butPDF_Click(object sender,EventArgs e) {
		if(_isInsertRequired) {
			MsgBox.Show(this,"Please click OK to create this appointment before taking this action.");
			return;
		}
		//this will only happen for eCW HL7 interface users.
		var listProcedures=Procedures.GetProcsForSingle(_appointment.AptNum,_appointment.AptStatus==ApptStatus.Planned);
		var duplicateProcs=ProcedureL.ProcsContainDuplicates(listProcedures);
		if(duplicateProcs!="") {
			ODMessageBox.Show(duplicateProcs);
			return;
		}
		//Send DFT to eCW containing a dummy procedure with this appointment in a .pdf file.	
		//no security
		var pdfDataStr=GenerateProceduresIntoPdf();
		if(HL7Defs.IsExistingHL7Enabled()) {
			//PDF messages do not contain FT1 segments, so proc list can be empty
			//MessageHL7 messageHL7=MessageConstructor.GenerateDFT(procs,EventTypeHL7.P03,pat,Patients.GetPat(pat.Guarantor),AptCur.AptNum,"progressnotes",pdfDataStr);
			var messageHL7=MessageConstructor.GenerateDFT([],EventTypeHL7.P03,_patient,Patients.GetPat(_patient.Guarantor),_appointment.AptNum,"progressnotes",pdfDataStr);
			if(messageHL7==null) {
				MsgBox.Show(this,"There is no DFT message type defined for the enabled HL7 definition.");
				return;
			}
			var hl7Msg=new HL7Msg();
			//hl7Msg.AptNum=_appointment.AptNum;
			hl7Msg.AptNum=0;//Prevents the appt complete button from changing to the "Revise" button prematurely.
			hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
			hl7Msg.MsgText=messageHL7.ToString();
			hl7Msg.PatNum=_patient.PatNum;
			HL7Msgs.Insert(hl7Msg);
		}
		MsgBox.Show(this,"Notes PDF sent.");
	}

	private void butComplete_Click(object sender,EventArgs e) {
		//It is OK to let the user click the OK button as long as _appointment.AptNum is NOT used prior to UpdateListAndDB().
		//if(_isInsertRequired) {
		//	MsgBox.Show(this,"Please click OK to create this appointment before taking this action.");
		//	return;
		//}
		//This is only used with eCW HL7 interface.
		var datePrevious=_appointment.DateTStamp;
		if(_isEcwHL7Sent) {
			if(!Security.IsAuthorized(EnumPermType.EcwAppointmentRevise)) {
				return;
			}
			MsgBox.Show(this,"Any changes that you make will not be sent to eCW.  You will also have to make the same changes in eCW.");
			//revise is only clickable if user has permission
			butSave.Enabled=true;
			gridProc.Enabled=true;
			listQuickAdd.Enabled=true;
			butAdd.Enabled=true;
			butDeleteProc.Enabled=true;
			return;
		}
		var listProceduresForAppts=gridProc.SelectedIndices.OfType<int>().Select(x => (Procedure)gridProc.ListGridRows[x].Tag).ToList();
		var duplicateProcs=ProcedureL.ProcsContainDuplicates(listProceduresForAppts);
		if(duplicateProcs!="") {
			ODMessageBox.Show(duplicateProcs);
			return;
		}
		if(ProgramProperties.GetPropVal(ProgramName.eClinicalWorks,"ProcNotesNoIncomplete")=="1") {
			if(listProceduresForAppts.Any(x => x.Note!=null && x.Note.Contains("\"\""))) {
				MsgBox.Show(this,"This appointment cannot be sent because there are incomplete procedure notes.");
				return;
			}
		}
		if(ProgramProperties.GetPropVal(ProgramName.eClinicalWorks,"ProcRequireSignature")=="1") {
			if(listProceduresForAppts.Any(x => !string.IsNullOrEmpty(x.Note) && string.IsNullOrEmpty(x.Signature))) {
				MsgBox.Show(this,"This appointment cannot be sent because there are unsigned procedure notes.");
				return;
			}
		}
		//user can only get this far if aptNum matches visit num previously passed in by eCW.
		if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Send attached procedures to eClinicalWorks and exit?")) {
			return;
		}
		comboStatus.SetSelectedEnum(ApptStatus.Complete);//Set the appointment status to complete. This will trigger the procedures to be completed in UpdateToDB() as well.
		if(!UpdateListAndDB()) {
			return;
		}
		listProceduresForAppts=Procedures.GetProcsForSingle(_appointment.AptNum,_appointment.AptStatus==ApptStatus.Planned);
		//Send DFT to eCW containing the attached procedures for this appointment in a .pdf file.				
		var pdfDataStr=GenerateProceduresIntoPdf();
		if(HL7Defs.IsExistingHL7Enabled()) {
			//MessageConstructor.GenerateDFT(procs,EventTypeHL7.P03,pat,Patients.GetPat(pat.Guarantor),AptCur.AptNum,"progressnotes",pdfDataStr);
			var messageHL7=MessageConstructor.GenerateDFT(listProceduresForAppts,EventTypeHL7.P03,_patient,Patients.GetPat(_patient.Guarantor),_appointment.AptNum,
				"progressnotes",pdfDataStr);
			if(messageHL7==null) {
				MsgBox.Show(this,"There is no DFT message type defined for the enabled HL7 definition.");
				return;
			}
			var hl7Msg=new HL7Msg();
			hl7Msg.AptNum=_appointment.AptNum;
			hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
			hl7Msg.MsgText=messageHL7.ToString();
			hl7Msg.PatNum=_patient.PatNum;
			var hl7ProcAttach=new HL7ProcAttach();
			hl7ProcAttach.HL7MsgNum=HL7Msgs.Insert(hl7Msg);
			for(var i = 0;i<listProceduresForAppts.Count;i++) {
				hl7ProcAttach.ProcNum=listProceduresForAppts[i].ProcNum;
				HL7ProcAttaches.Insert(hl7ProcAttach);
			}
		}
		IsEcwCloseOD=true;
		if(IsNew) {
			SecurityLogs.MakeLogEntry(EnumPermType.AppointmentCreate,_patient.PatNum,
				_appointment.AptDateTime+", "+_appointment.ProcDescript,
				_appointment.AptNum,datePrevious);
		}
		DialogResult=DialogResult.OK;
		if(!this.Modal) {
			Close();
		}
	}

	private void butAudit_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.ViewAppointmentAuditTrail)){
			return;
		}
		if(_isInsertRequired) {
			MsgBox.Show(this,"Please click OK to create this appointment before taking this action.");
			return;
		}
		var listPermissions=new List<EnumPermType>();
		listPermissions.Add(EnumPermType.AppointmentCreate);
		listPermissions.Add(EnumPermType.AppointmentEdit);
		listPermissions.Add(EnumPermType.AppointmentMove);
		listPermissions.Add(EnumPermType.AppointmentCompleteEdit);
		listPermissions.Add(EnumPermType.ApptConfirmStatusEdit);
		var frmAuditOneType=new FrmAuditOneType(_patient.PatNum,listPermissions,Lan.g(this,"Audit Trail for Appointment"),_appointment.AptNum);
		frmAuditOneType.ShowDialog();
	}

	private void butTask_Click(object sender,EventArgs e) {
		if(_isInsertRequired && !UpdateListAndDB(isClosing: false)) {
			return;
		}
		//If pref is enabled and apptnum not zero, fetch tasks with this appointment attached.
		//If there is exactly one, show it and return. If there are multiple, show a list of all of the tasks attached. If there are none, carry on adding one.
		if(_appointment.AptNum!=0 && !PrefC.GetBool(PrefName.TasksForApptAllowMultiple)){
			var listTasksToOpen=Tasks.GetMany(_appointment.AptNum);
			if(listTasksToOpen.Count==1){
				//Exactly one attached. Show it.
				using var formTaskEditExisting=new FormTaskEdit(listTasksToOpen.FirstOrDefault());
				formTaskEditExisting.ShowDialog();
				return;
			}
			else if(listTasksToOpen.Count>1){
				//Many attached. Show FormTasksForAppt.
				var formTasksForAppt=new FormTasksForAppt(_appointment.AptNum);
				formTasksForAppt.ShowDialog();
				return;
			}
			//If there are no Tasks attached, carry on adding one
		}
		using var formTaskListSelect=new FormTaskListSelect(TaskObjectType.Appointment,IsTaskNew:true);//,_appointment.AptNum);
		formTaskListSelect.Text=Lan.g(formTaskListSelect,"Add Task")+" - "+formTaskListSelect.Text;
		formTaskListSelect.ShowDialog();
		if(formTaskListSelect.DialogResult!=DialogResult.OK) {
			return;
		}
		var task=new Task();
		task.TaskListNum=-1;//don't show it in any list yet.
		Tasks.Insert(task);
		var taskOld=task.Copy();
		task.KeyNum=_appointment.AptNum;
		task.ObjectType=TaskObjectType.Appointment;
		task.TaskListNum=formTaskListSelect.ListSelectedLists[0];
		task.UserNum=Security.CurUser.UserNum;
		using var formTaskEdit=new FormTaskEdit(task,taskOld);
		formTaskEdit.IsNew=true;
		formTaskEdit.ShowDialog();
		if(formTaskEdit.DialogResult==DialogResult.OK) {
			formTaskEdit.SaveCopy(formTaskListSelect.ListSelectedLists.Skip(1).ToList());//Skip one because tasks was saved to first task list
		}
	}

	private void butPin_Click(object sender,System.EventArgs e) {
		if(_appointment.AptStatus.In(ApptStatus.UnschedList,ApptStatus.Planned)
		   && _patient.PatStatus.In(PatientStatus.Archived,PatientStatus.Deceased)) {
			MsgBox.Show(this,"Appointments cannot be scheduled for "+_patient.PatStatus.ToString().ToLower()+" patients.");
			return;
		}
		if(!UpdateListAndDB()) {
			return;
		}
		PinClicked=true;
		DialogResult=DialogResult.OK;
		if(!this.Modal) {
			Close();
		}
	}
	#endregion Methods - Event Handlers - Click - Right

	#region Methods - Public
	public Appointment GetAppointmentCur() {
		return _appointment.Copy();
	}

	public Appointment GetAppointmentOld() {
		return _appointmentOld.Copy();
	}

	///<summary>Indicates the Appointment is being opened from the unscheduled list.</summary>
	public void IsSchedulingUnscheduledAppt(bool isSchedulingUnscheduledAppt) {
		_isSchedulingUnscheduledAppt=isSchedulingUnscheduledAppt;
	}
	#endregion Methods - Public

	#region Methods - Private
	///<summary>Returns true if the appointment type was successfully changed, returns false if the user decided to cancel out of doing so.</summary>
	private bool AptTypeHelper() {
		if(comboApptType.SelectedIndex==0) {//'None' is selected so maintain grid selections.
			return true;
		}
		if(_appointment.AptStatus.In(ApptStatus.PtNote,ApptStatus.PtNoteCompleted)) {
			return true;//Patient notes can't have procedures associated to them.
		}
		var appointmentType=comboApptType.GetSelected<AppointmentType>();
		var listProcedureCodesApptType=ProcedureCodes.GetFromCommaDelimitedList(appointmentType.CodeStr);
		List<Procedure> listProceduresSelected;
		if(listProcedureCodesApptType.Count>0) {//AppointmentType is associated to procs.
			listProceduresSelected=gridProc.SelectedTags<Procedure>();
			var listProcCodeNumsToDetach=listProceduresSelected.Select(y => y.CodeNum).ToList()
				.Except(listProcedureCodesApptType.Select(x => x.CodeNum).ToList()).ToList();
			//if there are procedures that would get detached
			//and if they have the preference AppointmentTypeWarning on,
			//Display the warning
			if(listProcCodeNumsToDetach.Count>0 && PrefC.GetBool(PrefName.AppointmentTypeShowWarning)) {
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Selecting this appointment type will dissociate the current procedures from this "
				                                            +"appointment and attach the procedures defined for this appointment type.  Do you want to continue?")) {
					return false;
				}
			}
			Appointments.ApptTypeMissingProcHelper(_appointment,appointmentType,_listProceduresForAppointment,_patient,
				canUpdateApptPattern: true,_listPatPlans,_listInsSubs,_listInsPlans,_listBenefits);
			FillProcedures();
			//Since we have detached and attached all pertinent procs by this point it is safe to just use the PlannedAptNum or AptNum.
			gridProc.SetAll(false);
			for(var i = 0;i<listProcedureCodesApptType.Count;i++) {
				var listProceduresSelectedApptType = listProceduresSelected.FindAll(x => x.CodeNum==listProcedureCodesApptType[i].CodeNum);
				if(listProceduresSelectedApptType.Count>0) { // If procedures with this code were already selected, preserve those selections
					gridProc.SetSelected(listProceduresSelectedApptType.Select(x => gridProc.GetTags<Procedure>().FindIndex(y => x==y)).Where(x => x>-1).ToArray(),true);
					continue;
				}
				for(var j = 0;j<gridProc.ListGridRows.Count;j++) {
					var procedure=(Procedure)gridProc.ListGridRows[j].Tag;
					if(procedure.PlannedAptNum!=0 && procedure.PlannedAptNum!=_appointment.AptNum) {//do not select procedures attached to planned appointments
						continue;
					}
					if(procedure.CodeNum==listProcedureCodesApptType[i].CodeNum
					   //if the procedure code already exists in the grid and it's not attached to another appointment or planned appointment
					   && (_isPlanned && procedure.AptNum==0 && (procedure.PlannedAptNum==0 || procedure.PlannedAptNum==_appointment.AptNum)
					       || (!_isPlanned && (procedure.AptNum==0 || procedure.AptNum==_appointment.AptNum)))
					   //The row is not already selected. This is necessary so that Apt Types with two of the same procs will select both procs.
					   && !gridProc.SelectedIndices.Contains(j)) {
						gridProc.SetSelected(j,true); //set procedures selected in the grid.
						break;
					}
				}
			}
		}
		butColor.BackColor=appointmentType.AppointmentTypeColor;
		if(appointmentType.Pattern!=null && appointmentType.Pattern!="") {
			contrApptProvSlider.Pattern=appointmentType.Pattern;
		}
		//calculate the new time pattern.
		if(appointmentType!=null && listProcedureCodesApptType != null) {
			//Has Procs, but not time.
			if(appointmentType.Pattern=="" && listProcedureCodesApptType.Count > 0) {
				//Calculate and Fill
				CalculatePatternFromProcs(ignoreTimeLocked: true);
				_appointment.Pattern=contrApptProvSlider.Pattern;
				//SetTimeSliderColors();
			}
			//Has fixed time
			else if(appointmentType.Pattern!="") {
				_appointment.Pattern=appointmentType.Pattern;
				_appointment.TimeLocked=true;//Appointment has appt type and that appt type has a custom time pattern. That custom time pattern takes priority so we time lock the appt.
				checkTimeLocked.Checked=true;
				//SetTimeSliderColors();
			}
			//No Procs, No time.
			else {
				//do nothing to the time pattern
			}
		}
		return true;
	}

	///<summary>Calculates the estimated patient portion to insert into the grid</summary>
	private void CalcEstPatientPortion() {
		var listProceduresSelected=gridProc.SelectedTags<Procedure>();
		decimal totalEstPatientPortion=0;
		for(var i = 0;i<listProceduresSelected.Count;i++) {
			totalEstPatientPortion+=ClaimProcs.GetPatPortion(listProceduresSelected[i],_listClaimProcs,_listAdjustments);
		}
		var row=gridPatient.ListGridRows.ToList().Find(x => x.Cells[0].Text==Lans.g("FormApptEdit","Est. Patient Portion"));
		if(row==null) {
			return;//Probably some weird translation issue
		}
		row.Cells[1].Text=totalEstPatientPortion.ToString("F");
	}

	///<summary>Calculates the fee for this appointment using the highlighted procedures in the procedure list.</summary>
	private void CalcPatientFeeThisAppt() {
		double feeThisAppt=0;
		for(var i = 0;i<gridProc.SelectedIndices.Length;i++) {
			feeThisAppt+=((Procedure)(gridProc.ListGridRows[gridProc.SelectedIndices[i]].Tag)).ProcFeeTotal;
		}
		gridPatient.ListGridRows[gridPatient.ListGridRows.Count-1].Cells[1].Text=SOut.Double(feeThisAppt);
		gridPatient.Invalidate();
	}

	private void CalculatePatternFromProcs(bool ignoreTimeLocked = false) {
		var listProcedures=new List<Procedure>();
		for(var i = 0;i<gridProc.SelectedIndices.Length;i++) {
			listProcedures.Add((Procedure)gridProc.ListGridRows[gridProc.SelectedIndices[i]].Tag);
		}
		var timeLocked=checkTimeLocked.Checked;
		if(!IsNew && !OpenDentBusiness.Security.IsAuthorized(EnumPermType.AppointmentResize,suppressMessage:true)) {
			timeLocked=true;
			ignoreTimeLocked=false;
		}
		contrApptProvSlider.Pattern=Appointments.CalculatePattern(_patient,comboProv.GetSelectedProvNum(),comboProvHyg.GetSelectedProvNum(),
			listProcedures,timeLocked,ignoreTimeLocked);
		//contrApptProvSlider will automatically change the PatternSecondary length to match.
	}

	private bool CheckFrequencies() {
		var listProceduresFrequencies=new List<Procedure>();
		for(var i = 0;i<gridProc.SelectedIndices.Length;i++) {
			var procedure=((Procedure)gridProc.ListGridRows[gridProc.SelectedIndices[i]].Tag).Copy();
			if(procedure.ProcStatus==ProcStat.TP) {
				listProceduresFrequencies.Add(procedure);
			}
		}
		if(listProceduresFrequencies.Count>0) {
			var frequencyConflicts="";
			var discountPlanSub=DiscountPlanSubs.GetSubForPat(_patient.PatNum);
			if(discountPlanSub==null) {
				try {
					frequencyConflicts=Procedures.CheckFrequency(listProceduresFrequencies,_patient.PatNum,_appointment.AptDateTime);
				}
				catch(Exception e) {
					ODMessageBox.Show(Lan.g(this,"There was an error checking frequencies."
					                             +"  Disable the Insurance Frequency Checking feature or try to fix the following error:")
					                  +"\r\n"+e.Message);
					return false;
				}
				if(frequencyConflicts!="" && ODMessageBox.Show(Lan.g(this,"This appointment will cause frequency conflicts for the following procedures")
				                                               +":\r\n"+frequencyConflicts+"\r\n"+Lan.g(this,"Do you want to continue?"),"",MessageBoxButtons.YesNo)==DialogResult.No) {
					return false;
				}
			}
			else {
				try {
					frequencyConflicts=DiscountPlans.CheckDiscountFrequencyAndValidateDiscountPlanSub(listProceduresFrequencies,_patient.PatNum,_appointment.AptDateTime);
				}
				catch(Exception e) {
					ODMessageBox.Show(Lan.g(this,"There was an error checking discount frequencies.")
					                  +"\r\n"+e.Message);
					return false;
				}
				if(frequencyConflicts!="" && ODMessageBox.Show(Lan.g(this,"This appointment will cause frequency conflicts for the following procedures")
				                                               +":\r\n"+frequencyConflicts+"\r\n"+Lan.g(this,"Do you want to continue?"),"",MessageBoxButtons.YesNo)==DialogResult.No) {
					return false;
				}
			}
		}
		return true;
	}

	///<summary>Validates a given procedure and appointment if the PrefName.ApptPreventChangesToCompleted is true. 
	///If the preference is on, the Procedure is complete and the Appointment is complete - it returns true. 
	///If the preference is off, the Procedure is not complete OR the Appointment is not complete, it returns false. 
	///This method outs a msg variable that is passed up from DoPreventCompletedApptProcChange(...).</summary>
	private bool DisableDetachingOfCompletedProcFromCompletedAppt(Procedure proc,Appointment aptCur,out string msg) {
		msg="";//We will not need a message in a false case, so set it to an empty string.
		return (proc.ProcStatus==ProcStat.C//Row is assoicated to completed proc
		        && aptCur.AptStatus==ApptStatus.Complete//the proc is on a completed appt
		        && DoPreventCompletedApptProcChange(proc,out msg)//PrefName.ApptPreventChangesToCompleted is enable
			);
	}

	///<summary>Returns true if the user is not allowed to change a completed appointment.</summary>
	private bool DoPreventCompletedApptChange(PreventChangesApptAction action) {
		var listProceduresAttached=gridProc.SelectedTags<Procedure>();
		var doPreventChange=false;
		switch(action) {
			case PreventChangesApptAction.Delete:
				doPreventChange=AppointmentL.DoPreventChangesToCompletedAppt(_appointmentOld,action,listProceduresAttached);
				break;
			case PreventChangesApptAction.Status:
				doPreventChange=comboStatus.GetSelected<ApptStatus>()!=ApptStatus.Complete && //Setting the Apt status to something other than Complete
				                AppointmentL.DoPreventChangesToCompletedAppt(_appointmentOld,action,listProceduresAttached);
				break;
			default:
				throw new ApplicationException("Unsupported action");
		}
		return doPreventChange;
	}

	///<summary>An overload for DoPreventCompletedApptChange(...) that also handles Procedures. The inclusion of Proc's being blocked from
	///detachment when complete on a complete Appt necessitates a Proc be passed in to validate its status and a list of Procs be passed 
	///in to ensure that when a singular Proc remains on an Appt, the list can be checked to ensure that we do not allow its detachment 
	///from the Appt due to consumate logic.</summary>
	private bool DoPreventCompletedApptProcChange(Procedure proc,out string msg) {
		msg="";
		if(proc==null) {//Explicitly checking proc that was passed in because it is required.
			return true;//a valid procedure object is required when checking the Procedures action.
		}
		if(proc.ProcStatus==ProcStat.C) {
			var listProceduresAttached=_listProceduresForAppointment.FindAll(x => x.AptNum == _appointment.AptNum).Select(x => x.Copy()).ToList();
			return AppointmentL.DoPreventChangesToCompletedAppt(_appointmentOld,PreventChangesApptAction.Procedures,out msg,listProceduresAttached);
		}
		return false;
	}

	///<summary>Fills combo providers based on which clinic is selected and attempts to preserve provider selection if any.</summary>
	private void FillCombosProv() {
		var provNum=comboProv.GetSelectedProvNum();
		comboProv.Items.Clear();
		comboProv.Items.AddProvsAbbr(Providers.GetProvsForClinic(comboClinic.ClinicNumSelected));
		comboProv.SetSelectedProvNum(provNum);
		var provNumHyg=comboProvHyg.GetSelectedProvNum();
		comboProvHyg.Items.Clear();
		comboProvHyg.Items.AddProvNone();
		comboProvHyg.Items.AddProvsAbbr(Providers.GetProvsForClinic(comboClinic.ClinicNumSelected));
		comboProvHyg.SetSelectedProvNum(provNumHyg);
	}

	private void FillComm() {
		gridComm.BeginUpdate();
		gridComm.Columns.Clear();
		var col=new GridColumn(Lan.g("TableCommLog","DateTime"),80);
		gridComm.Columns.Add(col);
		col=new GridColumn(Lan.g("TableCommLog","Description"),80);
		gridComm.Columns.Add(col);
		gridComm.ListGridRows.Clear();
		GridRow row;
		var listDefsMiscColors=Defs.GetDefsForCategory(DefCat.MiscColors);
		var listDefsCommLogTypes=Defs.GetDefsForCategory(DefCat.CommLogTypes);
		bool isCommlogAutomated;
		for(var i=0;i<_tableComms.Rows.Count;i++) {
			var commTypeDefNum=SIn.Long(_tableComms.Rows[i]["CommType"].ToString());
			var defCur=Defs.GetDef(DefCat.CommLogTypes,commTypeDefNum,listDefsCommLogTypes);
			var commType=defCur==null?"":defCur.ItemValue;//EmailMessages are included in _tableComms and do not have a CommType set.
			isCommlogAutomated=Commlogs.IsAutomated(commType,SIn.Enum<CommItemSource>(_tableComms.Rows[i]["CommSource"].ToString()));
			if(!checkShowCommAuto.Checked && isCommlogAutomated) { //Skip automated commlogs if not checked.
				continue;
			}
			row=new GridRow();
			if(SIn.Long(_tableComms.Rows[i]["CommlogNum"].ToString())>0) {
				row.Cells.Add(SIn.Date(_tableComms.Rows[i]["commDateTime"].ToString()).ToShortDateString());
				if(isCommlogAutomated) {//If it's an automated commlog, show only the first line.
					row.Cells.Add(Commlogs.GetNoteFirstLine(_tableComms.Rows[i]["Note"].ToString()));
				}
				else {
					row.Cells.Add(_tableComms.Rows[i]["Note"].ToString());
				}
				if(_tableComms.Rows[i]["CommType"].ToString()==Commlogs.GetTypeAuto(CommItemTypeAuto.APPT).ToString()) {
					row.ColorBackG=listDefsMiscColors[(int)DefCatMiscColors.CommlogApptRelated].ItemColor;
				}
			}
			else if(SIn.Long(_tableComms.Rows[i]["EmailMessageNum"].ToString())>0) {
				if(((HideInFlags)SIn.Int(_tableComms.Rows[i]["EmailMessageHideIn"].ToString())).HasFlag(HideInFlags.ApptEdit)) {
					continue;
				}
				row.Cells.Add(SIn.Date(_tableComms.Rows[i]["commDateTime"].ToString()).ToShortDateString());
				row.Cells.Add(_tableComms.Rows[i]["Subject"].ToString());
			}
			row.Tag=_tableComms.Rows[i];
			gridComm.ListGridRows.Add(row);
		}
		gridComm.EndUpdate();
		gridComm.ScrollToEnd();
	}

	private void FillFields() {
		gridFields.BeginUpdate();
		gridFields.Columns.Clear();
		var col=new GridColumn("",100);
		gridFields.Columns.Add(col);
		col=new GridColumn("",100);
		gridFields.Columns.Add(col);
		gridFields.ListGridRows.Clear();
		GridRow row;
		for(var i = 0;i<_tableFields.Rows.Count;i++) {
			row=new GridRow();
			row.Cells.Add(_tableFields.Rows[i]["FieldName"].ToString());
			row.Cells.Add(_tableFields.Rows[i]["FieldValue"].ToString());
			gridFields.ListGridRows.Add(row);
		}
		gridFields.EndUpdate();
	}

	private void FillPatient() {
		var table=_loadData.TablePatients;
		gridPatient.BeginUpdate();
		gridPatient.Columns.Clear();
		var col=new GridColumn("",120);//Add 2 blank columns
		gridPatient.Columns.Add(col);
		col=new GridColumn("",120);
		gridPatient.Columns.Add(col);
		gridPatient.ListGridRows.Clear();
		GridRow row;
		for(var i = 1;i<table.Rows.Count;i++) {//starts with 1 to skip name
			row=new GridRow();
			row.Cells.Add(table.Rows[i]["field"].ToString());
			row.Cells.Add(table.Rows[i]["value"].ToString());
			if(table.Rows[i]["field"].ToString().EndsWith("Phone")  && Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled) {
				row.Cells[row.Cells.Count-1].ColorText=System.Drawing.Color.Blue;
				row.Cells[row.Cells.Count-1].Underline=YN.Yes;
			}
			gridPatient.ListGridRows.Add(row);
		}
		//Add a UI managed row to display the total fee for the selected procedures in this appointment.
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Fee This Appt"));
		row.Cells.Add("");//Calculated below
		gridPatient.ListGridRows.Add(row);
		CalcPatientFeeThisAppt();
		gridPatient.EndUpdate();
		gridPatient.ScrollToEnd();
	}

	private void FillProcedures() {
		//Every time the procedures available have been manipulated (associated to appt, deleted, etc) we need to refresh the list from the db.
		//This has the potential to call the database a lot (cell click via a grid) but we accept this inefficiency for the benefit of concurrency.
		//If the following call to the db is to be removed, make sure that all procedure manipulations from FormProcEdit, FormClaimProcEdit, etc.
		//handle the changes accordingly.  Changing this call to the database should not be done 'lightly'.  Heed our warning.
		var listProcedures=_listProceduresForAppointment;//Gets a full list because GetProcsForApptEdit has its optional param set true in ApptEdit.cs
		ProcedureLogic.SortProcedures(listProcedures);
		var listNumsSelected=new List<long>();
		if(_isOnLoad && !_isInsertRequired) {//First time filling the grid and not a new appointment.
			if(_listProcNumsPreSelected!=null) {
				//Allows us to preselect procs without setting proc.PlannedAptNum in AppointmentL.CreatePlannedAppt(). Otherwise, downstream attach/detach
				//logic has problems if we preselect by setting AptNum/PlannedAptNum because that logic uses _listProcNumsAttachedStart to determine if
				//these procs were already attached to this appointment.
				listNumsSelected.AddRange(_listProcNumsPreSelected.FindAll(x => listProcedures.Any(y => y.ProcNum==x)));
			}
			if(_isPlanned) {
				_listProcNumsAttachedStart=listProcedures.FindAll(x => x.PlannedAptNum==_appointment.AptNum).Select(x => x.ProcNum).ToList();
			}
			else {//regular appointment
				//set ProcNums attached to the appt when form opened for use in automation on closing.
				_listProcNumsAttachedStart=listProcedures.FindAll(x => x.AptNum==_appointment.AptNum).Select(x => x.ProcNum).ToList();
			}
			listNumsSelected.AddRange(_listProcNumsAttachedStart);
			if(Programs.UsingEcwTightOrFullMode() && !_isEcwHL7Sent) {//for eCW only and only if not in 'Revise' mode, select completed procs from _listProcedureForAppointments with ProcDate==AptDateTime
				//Attach procs to this appointment in memory only so that Cancel button still works.
				listNumsSelected.AddRange(listProcedures.Where(x => x.ProcStatus==ProcStat.C && x.ProcDate.Date==_appointment.AptDateTime.Date).Select(x => x.ProcNum));
			}
		}
		else {//Filling the grid later on.
			listNumsSelected.AddRange(gridProc.SelectedIndices.OfType<int>().Select(x => ((Procedure)gridProc.ListGridRows[x].Tag).ProcNum));
		}
		var isMedical=Clinics.IsMedicalPracticeOrClinic(comboClinic.ClinicNumSelected);
		gridProc.BeginUpdate();
		gridProc.ListGridRows.Clear();
		gridProc.Columns.Clear();
		List<DisplayField> listDisplayFieldsAppts;
		if(_appointment.AptStatus==ApptStatus.Planned) {
			listDisplayFieldsAppts=DisplayFields.GetForCategory(DisplayFieldCategory.PlannedAppointmentEdit);
		}
		else {
			listDisplayFieldsAppts=DisplayFields.GetForCategory(DisplayFieldCategory.AppointmentEdit);
		}
		for(var i=0;i<listDisplayFieldsAppts.Count;i++) {
			if(isMedical && (listDisplayFieldsAppts[i].InternalName=="Surf" || listDisplayFieldsAppts[i].InternalName=="Tth")) {
				continue;
			}
			if(listDisplayFieldsAppts[i].Description.IsNullOrEmpty()) {
				gridProc.Columns.Add(new GridColumn(listDisplayFieldsAppts[i].InternalName,listDisplayFieldsAppts[i].ColumnWidth));
			}
			else {
				gridProc.Columns.Add(new GridColumn(listDisplayFieldsAppts[i].Description,listDisplayFieldsAppts[i].ColumnWidth));
			}
		}
		if(listDisplayFieldsAppts.Sum(x => x.ColumnWidth) > gridProc.Width) {
			gridProc.HScrollVisible=true;
		}
		GridRow row;
		for(var i=0;i<listProcedures.Count;i++) {
			row=new GridRow();
			var procedureCode=ProcedureCodes.GetProcCode(listProcedures[i].CodeNum);
			for(var j=0;j<listDisplayFieldsAppts.Count;j++) {
				switch(listDisplayFieldsAppts[j].InternalName) {
					case "Stat":
						if(ProcMultiVisits.IsProcInProcess(listProcedures[i].ProcNum)) {
							row.Cells.Add(Lan.g("enumProcStat",ProcStatExt.InProcess));
						}
						else {
							row.Cells.Add(Lans.g("enumProcStat",listProcedures[i].ProcStatus.ToString()));
						}
						break;
					case "Priority":
						row.Cells.Add(Defs.GetName(DefCat.TxPriorities,listProcedures[i].Priority));
						break;
					case "Code":
						row.Cells.Add(procedureCode.ProcCode);
						break;
					case "Tth":
						if(isMedical) {
							continue;
						}
						row.Cells.Add(Tooth.Display(listProcedures[i].ToothNum));
						break;
					case "Surf":
						if(isMedical) {
							continue;
						}
						string displaySurf;
						if(ProcedureCodes.GetProcCode(listProcedures[i].CodeNum).TreatArea==TreatmentArea.Sextant) {
							displaySurf=Tooth.GetSextant(listProcedures[i].Surf,(ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers));
						}
						else {
							displaySurf=Tooth.SurfTidyFromDbToDisplay(listProcedures[i].Surf,listProcedures[i].ToothNum);
						}
						row.Cells.Add(displaySurf);
						break;
					case "Description":
						var descript="";
						if(listProcedures[i].ProcNumLab!=0) {//Proc is a Canadian Lab.
							//This descript is gotten the same way it was in Appointments.GetProcTable()
							descript="^ ^ "+descript;//Visual indicator that this lab is linked to the procedure on the row above this row.
						}
						if(_isPlanned && listProcedures[i].PlannedAptNum!=0 && listProcedures[i].PlannedAptNum!=_appointment.AptNum) {
							descript+=Lan.g(this,"(other appt) ");
						}
						else if(_isPlanned && listProcedures[i].AptNum!=0 && listProcedures[i].AptNum!=_appointment.AptNum) {
							descript+=Lan.g(this,"(scheduled appt) ");
						}
						else if(!_isPlanned && listProcedures[i].PlannedAptNum!=0 && listProcedures[i].PlannedAptNum!=_appointment.AptNum) {
							descript+=Lan.g(this,"(planned appt) ");
						}
						else if(!_isPlanned && listProcedures[i].AptNum!=0 && listProcedures[i].AptNum!=_appointment.AptNum) {
							descript+=Lan.g(this,"(other appt) ");
						}
						if(procedureCode.LaymanTerm=="") {
							descript+=procedureCode.Descript;
						}
						else {
							descript+=procedureCode.LaymanTerm;
						}
						if(listProcedures[i].ToothRange!="") {
							descript+=" #"+Tooth.DisplayRange(listProcedures[i].ToothRange);
						}
						row.Cells.Add(descript);
						break;
					case "Fee":
						row.Cells.Add(listProcedures[i].ProcFeeTotal.ToString("F"));
						break;
					case "Abbreviation":
						row.Cells.Add(procedureCode.AbbrDesc);
						break;
					case "Layman's Term":
						row.Cells.Add(procedureCode.LaymanTerm);
						break;
				}
			}
			row.Tag=listProcedures[i];
			gridProc.ListGridRows.Add(row);
		}
		gridProc.EndUpdate();
		for(var i=0;i<listProcedures.Count;i++) {
			//Proc is selected, or is a Canadian Lab, and its parent is selected 
			//Selection logic to ensure the parent and children labs are selected together, this mimicks logic in ContrAccount.cs
			//See gridAccount_CellClick(...) toward the bottom.
			if(listNumsSelected.Contains(listProcedures[i].ProcNum) || listNumsSelected.Contains(listProcedures[i].ProcNumLab)) {
				gridProc.SetSelected(i,true);
			}
		}
	}

	///<summary>Creates a new .pdf file containing all of the procedures attached to this appointment and returns the contents of the .pdf file as a base64 encoded string.</summary>
	private string GenerateProceduresIntoPdf() {
		var document=new MigraDoc.DocumentObjectModel.Document();
		document.DefaultPageSetup.PageWidth=Unit.FromInch(8.5);
		document.DefaultPageSetup.PageHeight=Unit.FromInch(11);
		document.DefaultPageSetup.TopMargin=Unit.FromInch(.5);
		document.DefaultPageSetup.LeftMargin=Unit.FromInch(.5);
		document.DefaultPageSetup.RightMargin=Unit.FromInch(.5);
		var section=document.AddSection();
		var headingFont=MigraDocHelper.CreateFont(13,isBold: true);
		var bodyFontx=MigraDocHelper.CreateFont(9,isBold: false);
		string text;
		//Heading---------------------------------------------------------------------------------------------------------------
		var paragraph=section.AddParagraph();
		var paragraphFormat=new ParagraphFormat();
		paragraphFormat.Alignment=ParagraphAlignment.Center;
		paragraphFormat.Font=MigraDocHelper.CreateFont(10,isBold: true);
		paragraph.Format=paragraphFormat;
		text=Lan.g(this,"procedures").ToUpper();
		paragraph.AddFormattedText(text,headingFont);
		paragraph.AddLineBreak();
		text=_patient.GetNameFLFormal();
		paragraph.AddFormattedText(text,headingFont);
		paragraph.AddLineBreak();
		text=DateTime.Now.ToShortDateString();
		paragraph.AddFormattedText(text,headingFont);
		paragraph.AddLineBreak();
		paragraph.AddLineBreak();
		//Procedure List--------------------------------------------------------------------------------------------------------
		var gridProg=new GridOD();
		gridProg.TranslationName="";
		this.Controls.Add(gridProg);//Only added temporarily so that printing will work. Removed at end with Dispose().
		gridProg.BeginUpdate();
		gridProg.Columns.Clear();
		GridColumn col;
		var listDisplayFields=DisplayFields.GetDefaultList(DisplayFieldCategory.None);
		for(var i=0;i<listDisplayFields.Count;i++) {
			if(listDisplayFields[i].InternalName=="User" || listDisplayFields[i].InternalName=="Signed") {
				continue;
			}
			if(listDisplayFields[i].Description=="") {
				col=new GridColumn(listDisplayFields[i].InternalName,listDisplayFields[i].ColumnWidth);
			}
			else {
				col=new GridColumn(listDisplayFields[i].Description,listDisplayFields[i].ColumnWidth);
			}
			if(listDisplayFields[i].InternalName=="Amount") {
				col.TextAlign=HorizontalAlignment.Right;
			}
			if(listDisplayFields[i].InternalName=="Proc Code") {
				col.TextAlign=HorizontalAlignment.Center;
			}
			gridProg.Columns.Add(col);
		}
		gridProg.NoteSpanStart=2;
		gridProg.NoteSpanStop=7;
		gridProg.ListGridRows.Clear();
		var listProceduresForDay=Procedures.GetProcsForPatByDate(_appointment.PatNum,_appointment.AptDateTime);
		var listDefsProgNoteColors=Defs.GetDefsForCategory(DefCat.ProgNoteColors);
		var listDefsMiscColors=Defs.GetDefsForCategory(DefCat.MiscColors);
		for(var i = 0;i<listProceduresForDay.Count;i++) {
			var procedure=listProceduresForDay[i];
			var procedureCode=ProcedureCodes.GetProcCode(procedure.CodeNum);
			var provider=Providers.GetDeepCopy().First(x => x.Id==procedure.ProvNum);
			var userod=Userods.GetUser(procedure.UserNum);
			var row=new GridRow();
			row.ColorLborder=System.Drawing.Color.Black;
			for(var f=0;f<listDisplayFields.Count;f++) {
				switch(listDisplayFields[f].InternalName) {
					case "Date":
						row.Cells.Add(procedure.ProcDate.Date.ToShortDateString());
						break;
					case "Time":
						row.Cells.Add(procedure.ProcDate.ToString("h:mm")+procedure.ProcDate.ToString("%t").ToLower());
						break;
					case "Th":
						row.Cells.Add(Tooth.Display(procedure.ToothNum));
						break;
					case "Surf":
						row.Cells.Add(procedure.Surf);
						break;
					case "Dx":
						row.Cells.Add(procedure.Dx.ToString());
						break;
					case "Description":
						row.Cells.Add((procedureCode.LaymanTerm!="") ? procedureCode.LaymanTerm : procedureCode.Descript);
						break;
					case "Stat":
						if(ProcMultiVisits.IsProcInProcess(procedure.ProcNum)) {
							row.Cells.Add(Lan.g("enumProcStat",ProcStatExt.InProcess));
						}
						else {
							row.Cells.Add(Lans.g("enumProcStat",procedure.ProcStatus.ToString()));
						}
						break;
					case "Prov":
						row.Cells.Add(StringTools.Truncate(provider.Abbr,5));
						break;
					case "Amount":
						row.Cells.Add(procedure.ProcFee.ToString("F"));
						break;
					case "Proc Code":
						if(procedureCode.ProcCode.Length>5 && procedureCode.ProcCode.StartsWith("D")) {
							row.Cells.Add(procedureCode.ProcCode.Substring(0,5));//Remove suffix from all D codes.
						}
						else {
							row.Cells.Add(procedureCode.ProcCode);
						}
						break;
				}
			}
			row.Note=procedure.Note;
			//Row text color.
			switch(procedure.ProcStatus) {
				case ProcStat.TP:
					row.ColorText=listDefsProgNoteColors[0].ItemColor;
					break;
				case ProcStat.C:
					row.ColorText=listDefsProgNoteColors[1].ItemColor;
					break;
				case ProcStat.EC:
					row.ColorText=listDefsProgNoteColors[2].ItemColor;
					break;
				case ProcStat.EO:
					row.ColorText=listDefsProgNoteColors[3].ItemColor;
					break;
				case ProcStat.R:
					row.ColorText=listDefsProgNoteColors[4].ItemColor;
					break;
				case ProcStat.D:
					row.ColorText=System.Drawing.Color.Black;
					break;
				case ProcStat.Cn:
					row.ColorText=listDefsProgNoteColors[22].ItemColor;
					break;
			}
			row.ColorBackG=System.Drawing.Color.White;
			if(procedure.ProcDate.Date==DateTime.Today) {
				row.ColorBackG=listDefsMiscColors[(int)DefCatMiscColors.ChartTodaysProcs].ItemColor;
			}
			gridProg.ListGridRows.Add(row);
		}
		gridProg.EndUpdate();
		MigraDocHelper.DrawGrid(section,gridProg);
		var pdfRenderer=new MigraDoc.Rendering.PdfDocumentRenderer(unicode: true,PdfFontEmbedding.Always);
		pdfRenderer.Document=document;
		pdfRenderer.RenderDocument();
		using var memorystream=new MemoryStream();
		pdfRenderer.PdfDocument.Save(memorystream);
		var pdfBytes=memorystream.GetBuffer();
		//#region Remove when testing is complete.
		//string tempFilePath=Path.GetTempFileName();
		//File.WriteAllBytes(tempFilePath,pdfBytes);
		//#endregion
		var pdfDataStr=Convert.ToBase64String(pdfBytes);
		return pdfDataStr;
	}

	///<summary>The currently selected ApptStatus.</summary>
	private ApptStatus GetApptStatusSelected() {
		if(_appointment.AptStatus==ApptStatus.Planned) {//Planned is not a selectable choice in the comboStatus box.
			return _appointment.AptStatus;
		}
		else if(comboStatus.SelectedIndex==-1) {
			return ApptStatus.Scheduled;
		}
		//When appointment is a patient note, comboStatus only displays 2 options; PtNote and PtNoteCompleted. See SetAptCurComboStatusSelection.
		else if(_appointment.AptStatus==ApptStatus.PtNote || _appointment.AptStatus==ApptStatus.PtNoteCompleted) {
			if(comboStatus.GetSelected<ApptStatus>()==ApptStatus.PtNote) {//Ptnote selected from comboStatus.
				return ApptStatus.PtNote;
			}
			return ApptStatus.PtNoteCompleted;
		}
		else if(comboStatus.GetSelected<ApptStatus>()==ApptStatus.Broken) {//Broken is SelectedIndex 3 but enum value 5, so when selected, we return ApptStatus.Broken.
			return ApptStatus.Broken;
		}
		else {//Scheduled, Complete, and Unscheduled are SelectedIndex 0,1,2 but enum values 1,2,3, so we index by 1 and return the appropriate ApptStatus.
			return (ApptStatus)comboStatus.SelectedIndex+1;
		}
	}

	private string GetLabCaseDescript() {
		if(_listLabCases is null) {
			return "";
		}
		var strLabCaseDescript="";
		for(var i=0;i<_listLabCases.Count;i++) {
			if(!string.IsNullOrEmpty(strLabCaseDescript)) {
				strLabCaseDescript+=Environment.NewLine;
			}
			var laboratory=Laboratories.GetOne(_listLabCases[i].LaboratoryNum);
			if(laboratory!=null) {  //Laboratory won't be set if the program closed in the middle of creating a new lab entry.
				strLabCaseDescript+=laboratory.Description;
			}
			else {
				strLabCaseDescript+=Lan.g(this,"ERROR retrieving laboratory.");
			}
			if(_listLabCases[i].DateTimeChecked.Year>1880) {//Logic from Appointments.cs lines 1098 to 1117
				strLabCaseDescript+=", "+Lan.g(this,"Quality Checked");
				return strLabCaseDescript;
			}
			if(_listLabCases[i].DateTimeRecd.Year>1880) {
				strLabCaseDescript+=", " + Lan.g(this,"Received");
				return strLabCaseDescript;
			}
			if(_listLabCases[i].DateTimeSent.Year>1880) {
				strLabCaseDescript+=", "+Lan.g(this,"Sent");
			}
			else {
				strLabCaseDescript+=", "+Lan.g(this,"Not Sent");
			}
			if(_listLabCases[i].DateTimeDue.Year>1880) {
				strLabCaseDescript+=", "+Lan.g(this,"Due: ")+_listLabCases[i].DateTimeDue.ToString("ddd")+" "
				                    +_listLabCases[i].DateTimeDue.ToShortDateString()+" "
				                    +_listLabCases[i].DateTimeDue.ToShortTimeString();
			}
		}
		return strLabCaseDescript;
	}

	///<summary>Will only invert the specified procedure in the grid, even if the procedure belongs to another appointment.</summary>
	private void InvertCurProcSelected(int index) {
		var isSelected=gridProc.SelectedIndices.Contains(index);
		var listIndicies=new List<int>();
		listIndicies.Add(index);
		if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {//Canadian. en-CA or fr-CA
			var procedureSelected=((Procedure)gridProc.ListGridRows[index].Tag);
			if(procedureSelected.ProcNumLab==0) {//Not a lab, but could be a parent to a lab.
				for(var i=0;i<gridProc.ListGridRows.Count;i++) {
					var proc=(Procedure)gridProc.ListGridRows[i].Tag;
					if(proc.ProcNumLab==procedureSelected.ProcNum) {//Is lab of selected procedure.
						listIndicies.Add(i);
					}
				}
			}
			else {//Is a lab.
				for(var i=0;i<gridProc.ListGridRows.Count;i++) {
					var procedure=(Procedure)gridProc.ListGridRows[i].Tag;
					if(procedure.ProcNum==procedureSelected.ProcNumLab) {//Parent of selected lab.
						listIndicies.Add(i);
					}
					else if(procedure.ProcNumLab==procedureSelected.ProcNumLab && !listIndicies.Contains(i)) {
						listIndicies.Add(i);
					}
				}
			}
		}
		for(var i=0;i<listIndicies.Count;i++) {
			gridProc.SetSelected(listIndicies[i],!isSelected);//Invert selection.
		}
	}

	///<summary>Deletes the appointment, creating appropriate logs and commlogs.  Pass in </summary>
	private void OnDelete_Click(bool isSkipDeletePrompt = false) {
		if(DoPreventCompletedApptChange(PreventChangesApptAction.Delete)) {
			return;
		}
		var datePrevious=_appointment.DateTStamp;
		if(_appointment.AptStatus==ApptStatus.PtNote || _appointment.AptStatus==ApptStatus.PtNoteCompleted) {
			if(!isSkipDeletePrompt && !MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete Patient Note?")) {
				return;
			}
			if(textNote.Text != "") {
				if(ODMessageBox.Show(Commlogs.GetDeleteApptCommlogMessage(textNote.Text,_appointment.AptStatus),"Question...",MessageBoxButtons.YesNo) == DialogResult.Yes) {
					var Commlog = new Commlog();
					Commlog.PatNum = _appointment.PatNum;
					Commlog.CommDateTime = DateTime.Now;
					Commlog.CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
					Commlog.Note = "Deleted Pt NOTE from schedule, saved copy: ";
					Commlog.Note += textNote.Text;
					Commlog.UserNum=Security.CurUser.UserNum;
					//there is no dialog here because it is just a simple entry
					Commlogs.Insert(Commlog);
				}
			}
		}
		else {//ordinary appointment
			if(!isSkipDeletePrompt && ODMessageBox.Show(Lan.g(this,"Delete appointment?"),"",MessageBoxButtons.OKCancel) != DialogResult.OK) {
				return;
			}
			//Only want to be able to break already scheduled appointments, this does not include new appointments in "schedule" status.
			if(_appointmentOld.AptNum!=0 && _appointment.AptStatus==ApptStatus.Scheduled && PrefC.GetBool(PrefName.BrokenApptRequiredOnMove)) {
				var frmApptBreakRequired=new FrmApptBreakRequired();
				frmApptBreakRequired.ShowDialog();
				if(!frmApptBreakRequired.IsDialogOK) {
					return;
				}
				AppointmentL.BreakApptHelper(_appointment,_patient,frmApptBreakRequired.ProcedureCodeBrokenSelected);
			}
			if(textNote.Text != "") {
				if(ODMessageBox.Show(Commlogs.GetDeleteApptCommlogMessage(textNote.Text,_appointment.AptStatus),"Question...",MessageBoxButtons.YesNo) == DialogResult.Yes) {
					var Commlog=new Commlog();
					Commlog.PatNum=_appointment.PatNum;
					Commlog.CommDateTime=DateTime.Now;
					Commlog.CommType=Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
					Commlog.Note="Deleted Appt. & saved note: ";
					if(_appointment.ProcDescript != "") {
						Commlog.Note+=_appointment.ProcDescript + ": ";
					}
					Commlog.Note+=textNote.Text;
					Commlog.UserNum=Security.CurUser.UserNum;
					//there is no dialog here because it is just a simple entry
					Commlogs.Insert(Commlog);
				}
			}
			//If there is an existing HL7 def enabled with an outbound SIU message defined, this appointment has been inserted, and there is an outbound
			//message with AptCur.AptNum, send an SIU_S17 Appt Deletion message
			if(_appointment.AptNum>0 && HL7Defs.IsExistingHL7Enabled() && HL7Msgs.MessageWasSent(_appointment.AptNum)) {
				//S17 - Appt Deletion event
				var messageHL7=MessageConstructor.GenerateSIU(_patient,_family.GetPatient(_patient.Guarantor),EventTypeHL7.S17,_appointment);
				//Will be null if there is no outbound SIU message defined, so do nothing
				if(messageHL7!=null) {
					var hl7Msg=new HL7Msg();
					hl7Msg.AptNum=_appointment.AptNum;
					hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
					hl7Msg.MsgText=messageHL7.ToString();
					hl7Msg.PatNum=_patient.PatNum;
					HL7Msgs.Insert(hl7Msg);
				}
			}
			if(_appointment.AptNum>0 && HieClinics.IsEnabled()) {//Ignore new appointment delete
				HieQueues.Insert(new HieQueue(_patient.PatNum));
			}
		}
		_listAppointments.RemoveAll(x => x.AptNum==_appointment.AptNum);
		if(_appointmentOld.AptStatus!=ApptStatus.Complete) { //seperate log entry for completed appointments
			SecurityLogs.MakeLogEntry(EnumPermType.AppointmentDelete,_patient.PatNum,
				"Delete for date/time: "+_appointment.AptDateTime,
				_appointment.AptNum,datePrevious);
		}
		else {
			SecurityLogs.MakeLogEntry(EnumPermType.AppointmentCompleteDelete,_patient.PatNum,
				"Delete for date/time: "+_appointment.AptDateTime,
				_appointment.AptNum,datePrevious);
		}
		if(IsNew) {
			DialogResult=DialogResult.Cancel;
			if(!this.Modal) {
				Close();
			}
		}
		else {
			DialogResult=DialogResult.OK;
			_isDeleted=true;
			if(!this.Modal) {
				Close();
			}
		}
	}

	///<summary>Fully refreshes the data and then calculate the estimated patient portion</summary>
	private void RefreshEstPatientPortion() {
		_listClaimProcs=ClaimProcs.RefreshForProcs(_listProceduresForAppointment.Select(x => x.ProcNum).ToList());
		_listAdjustments=Adjustments.GetForProcs(_listProceduresForAppointment.Select(x => x.ProcNum).ToList());
		CalcEstPatientPortion();
	}

	///<summary>Sets comboStatus based on _appointment.AptStatus.
	///_appointment.AptStatus is not updated with UI selection until after UpdateListAndDB(...) is called.</summary>
	private void SetAptCurComboStatusSelection() {
		if(_appointment.AptStatus==ApptStatus.Planned) {
			//Intentionally empty, comboStatus is not visable.
			return;
		}
		comboStatus.SetSelectedEnum(_appointment.AptStatus);
	}

	///<summary>This was FillTime, but all it does now is set the color.  This is still useful.  Color can change frequently.</summary>
	private void SetTimeSliderColors() {
		var colorProv=System.Drawing.Color.White;
		var colorProv2=System.Drawing.Color.White;
		if(checkIsHygiene.Checked) {
			if(comboProvHyg.GetSelectedProvNum()!=0) {
				colorProv=Providers.GetColor(comboProvHyg.GetSelectedProvNum());
			}
			if(comboProv.GetSelectedProvNum()!=0) {
				colorProv2=Providers.GetColor(comboProv.GetSelectedProvNum());
			}
		}
		else {//normal
			if(comboProv.GetSelectedProvNum()!=0) {
				colorProv=Providers.GetColor(comboProv.GetSelectedProvNum());//could be white if bad provNum
			}
			if(comboProvHyg.GetSelectedProvNum()!=0) {
				colorProv2=Providers.GetColor(comboProvHyg.GetSelectedProvNum());
			}
		}
		contrApptProvSlider.ColorProv=colorProv;
		contrApptProvSlider.ColorProv2=colorProv2;
		//contrApptProvSlider.Pattern= //already handled
	}

	///<summary>Validates and saves appointment and procedure information to DB.</summary>
	private bool UpdateListAndDB(bool isClosing = true,bool doCreateSecLog = false,bool doInsertHL7 = false) {
		var datePrevious=_appointment.DateTStamp;
		_listProceduresForAppointment=Procedures.GetProcsForApptEdit(_appointment);//We need to refresh so we can check for concurrency issues.
		FillProcedures();//This refills the tags in the grid so we can use the tags below.  Will also show concurrent changes by other users.
		if(comboApptType.SelectedIndex>0) {
			//If this appointment is of a certain AppointmentType, check for required procedure codes.
			var appointmentType=comboApptType.GetSelected<AppointmentType>();
			var requiredProcMsg = AppointmentTypes.CheckRequiredProcsAttached(appointmentType.AppointmentTypeNum, gridProc.SelectedTags<Procedure>());
			if(requiredProcMsg!=""){
				MsgBox.Show(requiredProcMsg);
				return false;
			}
		}
		#region PrefName.ApptsRequireProc and Permissions.ProcComplCreate check
		//First check that they have an procedures attached to this appointment. If the appointment is an existing appointment that did not originally
		//have any procedures attached, the prompt will not come up.
		if((IsNew || _listProcNumsAttachedStart.Count>0)
		   && PrefC.GetBool(PrefName.ApptsRequireProc)
		   && gridProc.SelectedIndices.Length==0
		   && !_appointment.AptStatus.In(ApptStatus.PtNote,ApptStatus.PtNoteCompleted)) {
			MsgBox.Show(this,"At least one procedure must be attached to the appointment.");
			return false;
		}
		if(GetApptStatusSelected()==ApptStatus.Complete
		   && gridProc.SelectedIndices.Select(x => (Procedure)gridProc.ListGridRows[x].Tag).Any(x => x.ProcStatus!=ProcStat.C)) {//Appt is complete, but a selected proc is not.
			var listProcedureSelected=gridProc.SelectedIndices.Select(x => (Procedure)gridProc.ListGridRows[x].Tag).ToList();
			listProcedureSelected.RemoveAll(x => x.ProcStatus==ProcStat.C);//only care about the procs that are not already complete (new attaching procs)
			for(var i=0;i<listProcedureSelected.Count;i++) {
				if(!Security.IsAuthorized(EnumPermType.ProcComplCreate,_appointment.AptDateTime,listProcedureSelected[i].CodeNum,listProcedureSelected[i].ProcFee)) {
					return false;
				}
			}
		}
		#endregion
		#region Check for Procs Attached to Another Appt
		var listAptNums_ToDelete = new List<long>();
		Func<List<long>,bool> funcListAptsToDelete = (listAptNumsToDelete) => {
			listAptNums_ToDelete = listAptNumsToDelete;
			var deleteApts=MsgBox.Show(this, MsgBoxButtons.YesNo,Appointments.PROMPT_ListAptsToDelete);
			if(deleteApts) 
			{ 
				return Security.IsAuthorized(EnumPermType.AppointmentDelete); //check that user has permission to delete apps.
			}
			return false;
		};
		var funcProcsConcurrentAndPlanned = () => {
			return MsgBox.Show(this, MsgBoxButtons.OKCancel,Appointments.PROMPT_PlannedProcsConcurrent);
		};
		var funcProcsConcurrentAndNotPlanned = () => {
			return MsgBox.Show(this, MsgBoxButtons.OKCancel,Appointments.PROMPT_NotPlannedProcsConcurrent);
		};
		var actionCompletedProceduresBeingMoved = () => {
			MsgBox.Show(this,Appointments.PROMPT_CompletedProceduresBeingMoved);
		};
		var listProceduresAll=Procedures.Refresh(_appointment.PatNum);
		var listProceduresInGrid = gridProc.ListGridRows.Select(x => x.Tag as Procedure).ToList();
		var listProcNumsSelected = gridProc.SelectedIndices.Select(x => (gridProc.ListGridRows[x].Tag as Procedure).ProcNum).ToList();
		var isValid=Appointments.ProcsAttachedToOtherAptsHelper(
			listProceduresInGrid, _appointment, listProcNumsSelected, _listProcNumsAttachedStart,
			funcListAptsToDelete, funcProcsConcurrentAndNotPlanned,funcProcsConcurrentAndPlanned,listProceduresAll,actionCompletedProceduresBeingMoved
		);
		if(!isValid) {
			_listProceduresForAppointment=Procedures.GetProcsForApptEdit(_appointment);//Refresh so user can see which procedures weren't added
			FillProcedures();
			return false;
		}
		#endregion Check for Procs Attached to Another Appt
		#region Validate Form Data
		//initial clinic selection based on Op, but user may also edit, so use selection.  The clinic combobox is the logical place to look
		//when being warned/blocked about specialty mismatch.  
		if(!AppointmentL.IsSpecialtyMismatchAllowed(_appointment.PatNum,comboClinic.ClinicNumSelected)) {
			return false;
		}
		if(_appointmentOld.AptStatus!=ApptStatus.UnschedList && comboStatus.GetSelected<ApptStatus>()==ApptStatus.UnschedList) {//previously not on unsched list and sending to unscheduled list
			if(PatRestrictionL.IsRestricted(_appointment.PatNum,PatRestrict.ApptSchedule,suppressMessage: true)) {
				ODMessageBox.Show(Lan.g(this,"Not allowed to send this appointment to the unscheduled list due to patient restriction")+" "
				                                                                                                                       +PatRestrictions.GetPatRestrictDesc(PatRestrict.ApptSchedule)+".");
				return false;
			}
			if(PrefC.GetBool(PrefName.UnscheduledListNoRecalls)
			   && Appointments.IsRecallAppointment(_appointment,gridProc.SelectedGridRows.Select(x => (Procedure)(x.Tag)).ToList())) {
				if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Recall appointments cannot be sent to the Unscheduled List.\r\nDelete appointment instead?")) {
					OnDelete_Click(isSkipDeletePrompt: true);//Skip the standard "Delete Appointment?" prompt since we have already prompted here. Closes form and syncs data.
				}
				return false;//Always return false since the appointment was either deleted of the user canceled.
			}
		}
		var dateTimeAskedToArrive=DateTime.MinValue;
		if((_appointmentOld.AptStatus==ApptStatus.Complete && comboStatus.GetSelected<ApptStatus>()!=ApptStatus.Complete)
		   || (_appointmentOld.AptStatus==ApptStatus.Broken && comboStatus.GetSelected<ApptStatus>()!=ApptStatus.Broken)) //Un-completing or un-breaking the appt.  We must use selectedindex due to _appointment gets updated later UpdateDB()
		{
			//If the insurance plans have changed since this appt was completed, warn the user that the historical data will be neutralized.
			var listPatPlans=PatPlans.Refresh(_patient.PatNum);
			var insSub1=InsSubs.GetSub(PatPlans.GetInsSubNum(listPatPlans,PatPlans.GetOrdinal(PriSecMed.Primary,listPatPlans,_listInsPlans,_listInsSubs)),_listInsSubs);
			var insSub2=InsSubs.GetSub(PatPlans.GetInsSubNum(listPatPlans,PatPlans.GetOrdinal(PriSecMed.Secondary,listPatPlans,_listInsPlans,_listInsSubs)),_listInsSubs);
			if(insSub1.PlanNum!=_appointment.InsPlan1 || insSub2.PlanNum!=_appointment.InsPlan2) {
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"The current insurance plans for this patient are different than the plans associated to this appointment.  They will be updated to the patient's current insurance plans.  Continue?")) {
					return false;
				}
				//Update the ins plans associated to this appointment so that they're the most accurate at this time.
				_appointment.InsPlan1=insSub1.PlanNum;
				_appointment.InsPlan2=insSub2.PlanNum;
			}
		}
		if(textTimeAskedToArrive.Text!="") {
			try {
				dateTimeAskedToArrive=_appointment.AptDateTime.Date+DateTime.Parse(textTimeAskedToArrive.Text).TimeOfDay;
			}
			catch {
				MsgBox.Show(this,"Time Asked To Arrive invalid.");
				return false;
			}
		}
		var dateTimeArrived=_appointment.AptDateTime.Date;
		if(textTimeArrived.Text!="") {
			try {
				dateTimeArrived=_appointment.AptDateTime.Date+DateTime.Parse(textTimeArrived.Text).TimeOfDay;
			}
			catch {
				MsgBox.Show(this,"Time Arrived invalid.");
				return false;
			}
		}
		var dateTimeSeated=_appointment.AptDateTime.Date;
		if(textTimeSeated.Text!="") {
			try {
				dateTimeSeated=_appointment.AptDateTime.Date+DateTime.Parse(textTimeSeated.Text).TimeOfDay;
			}
			catch {
				MsgBox.Show(this,"Time Seated invalid.");
				return false;
			}
		}
		var dateTimeDismissed=_appointment.AptDateTime.Date;
		if(textTimeDismissed.Text!="") {
			try {
				dateTimeDismissed=_appointment.AptDateTime.Date+DateTime.Parse(textTimeDismissed.Text).TimeOfDay;
			}
			catch {
				MsgBox.Show(this,"Time Dismissed invalid.");
				return false;
			}
		}
		//This change was just slightly too risky to make to 6.9, so 7.0 only
		if(!PrefC.GetBool(PrefName.ApptAllowFutureComplete)//Not allowed to set future appts complete.
		   && _appointment.AptStatus!=ApptStatus.Complete//was not originally complete
		   && _appointment.AptStatus!=ApptStatus.PtNote
		   && _appointment.AptStatus!=ApptStatus.PtNoteCompleted
		   && comboStatus.GetSelected<ApptStatus>()==ApptStatus.Complete //making it complete
		   && _appointment.AptDateTime.Date > DateTime.Today)//and future appt
		{
			MsgBox.Show(this,"Not allowed to set future appointments complete.");
			return false;
		}
		//get a list of procs selected on the appointment
		var listAttachedProcs = gridProc.SelectedIndices.Select(x=>gridProc.ListGridRows[x].Tag as Procedure).ToList();
		//check to see if any procedures on the appointment have a proc code in a hidden category
		var listHiddenProcCodes=ProcedureCodes.GetProcCodesInHiddenCats(listAttachedProcs.Select(x => x.CodeNum).ToArray());
		// if they do, and we are setting the appointment complete, block from completing appointment
		if(listHiddenProcCodes.Count > 0 && GetApptStatusSelected() == ApptStatus.Complete) {
			var message=Lan.g(this,"Cannot complete appointment because the following procedures are in a hidden category:\r\n")+" "+string.Join("\r\n",listHiddenProcCodes);
			MsgBox.Show(message);
			return false;
		}
		var hasProcsAttached=gridProc.SelectedIndices
			//Get tags on rows as procedures if possible
			.Select(x=>gridProc.ListGridRows[x].Tag as Procedure)
			//true if any row had a valid procedure as a tag
			.Any(x=>x!=null);
		if(!PrefC.GetBool(PrefName.ApptAllowEmptyComplete)
		   && _appointment.AptStatus!=ApptStatus.Complete//was not originally complete
		   && _appointment.AptStatus!=ApptStatus.PtNote
		   && _appointment.AptStatus!=ApptStatus.PtNoteCompleted
		   && comboStatus.GetSelected<ApptStatus>()==ApptStatus.Complete)//making it complete
		{
			if(!hasProcsAttached) {
				MsgBox.Show(this,"Appointments without procedures attached can not be set complete.");
				return false;
			}
		}
		if(DoPreventCompletedApptChange(PreventChangesApptAction.Status)) {
			//Not allowed to change existing completed appointment.
			//Change the status back to Complete before returning.
			comboStatus.SetSelectedEnum(ApptStatus.Complete);//Complete
			return false;
		}
		#region Security checks
		if(_appointment.AptStatus!=ApptStatus.Complete//was not originally complete
		   && GetApptStatusSelected()==ApptStatus.Complete //trying to make it complete
		   && hasProcsAttached
		   && !Security.IsAuthorized(EnumPermType.ProcComplCreate,_appointment.AptDateTime))//aren't authorized to complete procedures
		{
			return false;
		}
		if(!IsNew 
		   && _appointment.Pattern.Length!=contrApptProvSlider.Pattern.Length
		   && !Security.IsAuthorized(EnumPermType.AppointmentResize,suppressMessage:true))
		{
			var timeSpan=TimeSpan.FromMinutes(_appointment.Pattern.Length*5);
			var message=Lans.g("Security","Not authorized for")+" "+GroupPermissions.GetDesc(EnumPermType.AppointmentResize)
			            +"\r\n"+Lan.g(this,"The appointment length needs to be")+" "+timeSpan.ToString("h':'mm");
			MsgBox.Show(message);
			return false;
		}
		#endregion
		#region Provider Term Date Check
		//Prevents appointments with providers that are past their term end date from being scheduled
		var appointmentProviderCheck=_appointment.Copy();//Appt used only for the providers S class method
		appointmentProviderCheck.ProvNum=comboProv.GetSelectedProvNum();
		appointmentProviderCheck.ProvHyg=comboProvHyg.GetSelectedProvNum();
		if(GetApptStatusSelected()!=ApptStatus.UnschedList && GetApptStatusSelected()!=ApptStatus.Planned) {
			var message=Providers.CheckApptProvidersTermDates(appointmentProviderCheck);
			if(message!="") {
				ODMessageBox.Show(this,message);//translated in Providers S class method
				return false;
			}
		}
		#endregion Provider Term Date Check
		var listProcedures=gridProc.SelectedIndices.OfType<int>().Select(x => (Procedure)gridProc.ListGridRows[x].Tag).ToList();
		if(listProcedures.Count>0 && comboStatus.GetSelected<ApptStatus>()==ApptStatus.Complete && _appointment.AptDateTime.Date>DateTime.Today.Date
		   && !PrefC.GetBool(PrefName.FutureTransDatesAllowed)) {
			MsgBox.Show(this,"Not allowed to set procedures complete with future dates.");
			return false;
		}
		#endregion Validate Form Data
		//-----Point of no return-----
		#region Broken appt selections
		if(_apptBreakSelection==ApptBreakSelection.Unsched && !AppointmentL.ValidateApptUnsched(_appointment)) {
			_apptBreakSelection=ApptBreakSelection.None;//This way no additional logic runs below.
		}
		if(_apptBreakSelection==ApptBreakSelection.Pinboard && !AppointmentL.ValidateApptToPinboard(_appointment)) {
			_apptBreakSelection=ApptBreakSelection.None;//This way no additional logic runs below.
		}
		#endregion
		#region Set _appointment Fields
		_appointment.Pattern=contrApptProvSlider.Pattern;
		_appointment.PatternSecondary=contrApptProvSlider.PatternSecondary;
		//Only run appt overlap check if editing an appt not in unscheduled list and in chart module and eCW program link not enabled.
		//Also need to see if there is a generic HL7 def enabled where Open Dental is not the filler application.
		//Open Dental is the filler application if appointments, schedules, and operatories are maintained by Open Dental and messages are sent out
		//to inform another software of any changes made.  If Open Dental is an auxiliary application, appointments are created from inbound SIU
		//messages and Open Dental no longer has control over whether the appointments overlap or which operatory/provider's schedule the appointment
		//belongs to.  In this case, we do not want to check for overlapping appointments and the appointment module should be hidden.
		var hl7DefEnabled=HL7Defs.GetOneDeepEnabled();//the ShowAppts check box is hidden for MedLab HL7 interfaces, so only need to check the others
		var isAuxiliaryRole=false;
		if(hl7DefEnabled!=null && !hl7DefEnabled.ShowAppts) {//if the appts module is hidden
			//if an inbound SIU message is defined, OD is the auxiliary application which neither exerts control over nor requests changes to a schedule
			isAuxiliaryRole=hl7DefEnabled.hl7DefMessages.Any(x => x.MessageType==MessageTypeHL7.SIU && x.InOrOut==InOutHL7.Incoming);
		}
		if((IsInChartModule || IsInViewPatAppts)
		   && !Programs.UsingEcwTightOrFullMode()//if eCW Tight or Full mode, appts created from inbound SIU messages and appt module always hidden
		   && _appointment.AptStatus!=ApptStatus.UnschedList
		   && !isAuxiliaryRole)//generic HL7 def enabled, appt module hidden and an inbound SIU msg defined, appts created from msgs so no overlap check
		{
			//Adjusts _appointment.Pattern directly when necessary.
			if(ControlAppt.TryAdjustAppointmentPattern(_appointment,ControlApptPanel.GetListOpsVisible())) {
				MsgBox.Show(this,"Appointment is too long and would overlap another appointment or blockout.  Automatically shortened to fit.");
//todo? Consider changing PatternSecondary length to match Pattern length.  But there are many places in the program where this would need to be done.  Probably easier to assume they can be out of synch.
			}
		}
		_appointment.ProvBarText=contrApptProvSlider.ProvBarText;
		_appointment.Priority=ApptPriority.Normal;
		if(checkASAP.Checked) {
			_appointment.Priority=ApptPriority.ASAP;
		}
		_appointment.AptStatus=GetApptStatusSelected();
		//set procs complete was moved further down
		if(comboUnschedStatus.SelectedIndex==0) {//none
			_appointment.UnschedStatus=0;
		}
		else {
			_appointment.UnschedStatus=comboUnschedStatus.GetSelectedDefNum();
		}
		if(comboConfirmed.SelectedIndex!=-1) {
			_appointment.Confirmed=comboConfirmed.GetSelectedDefNum();
		}
		_appointment.TimeLocked=checkTimeLocked.Checked;
		_appointment.ColorOverride=butColor.BackColor;
		_appointment.Note=textNote.Text;
		_appointment.ClinicNum=comboClinic.ClinicNumSelected;
		_appointment.ProvNum=comboProv.GetSelectedProvNum();
		_appointment.ProvHyg=comboProvHyg.GetSelectedProvNum();
		_appointment.IsHygiene=checkIsHygiene.Checked;
		if(comboAssistant.SelectedIndex==0) {//none
			_appointment.Assistant=0;
		}
		else {
			_appointment.Assistant=comboAssistant.GetSelected<Employee>().EmployeeNum;
		}
		_appointment.IsNewPatient=checkIsNewPatient.Checked;
		_appointment.DateTimeAskedToArrive=dateTimeAskedToArrive;
		_appointment.DateTimeArrived=dateTimeArrived;
		_appointment.DateTimeSeated=dateTimeSeated;
		_appointment.DateTimeDismissed=dateTimeDismissed;
		//_appointment.InsPlan1 and InsPlan2 already handled 
		if(comboApptType.SelectedIndex==0) {//0 index = none.
			_appointment.AppointmentTypeNum=0;
		}
		else {
			_appointment.AppointmentTypeNum=comboApptType.GetSelectedKey<AppointmentType>(x => x.AppointmentTypeNum);
		}
		#endregion Set _appointment Fields
		#region Update ProcDescript for Appt
		//Use the current selections to set _appointment.ProcDescript.
		var listProceduresGridSelected=new List<Procedure>();
		gridProc.SelectedIndices.ToList().ForEach(x => listProceduresGridSelected.Add(_listProceduresForAppointment[x].Copy()));
		for(var i=0;i<listProceduresGridSelected.Count;i++) {
			//This allows Appointments.SetProcDescript(...) to associate all the passed in procs into _appointment.ProcDescript
			//listProcedureGridSelected is only used here and contains copies of procs.
			listProceduresGridSelected[i].AptNum=_appointment.AptNum;
			listProceduresGridSelected[i].PlannedAptNum=_appointment.AptNum;
		}
		Appointments.SetProcDescript(_appointment,listProceduresGridSelected);
		if(_appointmentOld.AptNum > 0) {
			//There is an edge case to handle: Completed appt is set back to Scheduled and procedures are set back to TP.
			//This temporarily sets AptNums on the procs to 0 which clears the appointment.ProcDescript.
			//_appointmentOld.ProcDescript must be updated to match db or synch will fail.
			var appointment=Appointments.GetOneApt(_appointmentOld.AptNum);
			if(appointment!=null) {
				_appointmentOld.ProcDescript=appointment.ProcDescript;
			}
		}
		#endregion Update ProcDescript for Appt
		#region Provider change and fee change check
		//Determines if we would like to update ProcFees when a provider changes, considers PrefName.ProcFeeUpdatePrompt.
		var updateProcFees=false;
		if(_appointment.AptStatus!=ApptStatus.Complete && (comboProv.GetSelectedProvNum()!=_appointmentOld.ProvNum || comboProvHyg.GetSelectedProvNum()!=_appointmentOld.ProvHyg)) {//Either the primary or hygienist changed.
			var listProceduresNew=gridProc.SelectedIndices.Select(x => Procedures.ChangeProcInAppointment(_appointment,((Procedure)gridProc.ListGridRows[x].Tag).Copy())).ToList();
			var listProceduresOld=gridProc.SelectedIndices.Select(x => ((Procedure)gridProc.ListGridRows[x].Tag).Copy()).ToList();
			var procFeeHelper=new ProcFeeHelper(_appointment.PatNum);
			var promptText="";
			//Indicates whether GetApptStatusSelected() is a patient note.
			var isPtNote = GetApptStatusSelected().In(ApptStatus.PtNote,ApptStatus.PtNoteCompleted);
			//PatientNote "Appointment" will never have fees.  Prompting/Updating proc fees unnecessary.
			updateProcFees=(!isPtNote && Procedures.ShouldFeesChange(listProceduresNew,listProceduresOld,ref promptText,procFeeHelper));
			if(updateProcFees && promptText!="" && !MsgBox.Show(this,MsgBoxButtons.YesNo,promptText)) {
				updateProcFees=false;
			}
		}
		var removeCompletedProcs=ProcedureL.DoRemoveCompletedProcs(_appointment,listProceduresGridSelected,checkForAllProcCompl: true);
		#endregion
		#region Save to DB
		Appointments.ApptSaveHelperResult apptSaveHelperResult;
		try {
			apptSaveHelperResult=Appointments.ApptSaveHelper(_appointment,_appointmentOld,_isInsertRequired,_listProceduresForAppointment,_listAppointments,
				gridProc.SelectedIndices.ToList(),_listProcNumsAttachedStart,_isPlanned,_listInsPlans,_listInsSubs,
				listProceduresGridSelected,IsNew,_patient,_family,updateProcFees,removeCompletedProcs,doCreateSecLog,doInsertHL7);
			_appointment=apptSaveHelperResult.AptCur;
			_listProceduresForAppointment=apptSaveHelperResult.ListProcsForAppt;
			_listAppointments=apptSaveHelperResult.ListAppts;
		}
		catch(ApplicationException ex) {
			ODMessageBox.Show(ex.Message);
			return false;
		}
		if(_isInsertRequired && _appointmentOld.AptNum==0) {
			//Update the the old AptNum since this is a new appointment.
			//This stops Appointments.Sync(...) from double insertings this new appointment.
			_appointmentOld.AptNum=_appointment.AptNum;
			_listAppointmentsOld.FirstOrDefault(x => x.AptNum==0).AptNum=_appointment.AptNum;
		}
		_isInsertRequired=false;//Now that we have inserted the new appointment, let typical appointment logic handle from here on.
		#endregion Save changes to DB
		#region Update gridProc tags
		//update tags with changes made so that anyone accessing it later has an updated copy.
		for(var i = 0;i<gridProc.SelectedIndices.Length;i++) {
			var procedure=_listProceduresForAppointment.FirstOrDefault(x => x.ProcNum==((Procedure)gridProc.ListGridRows[gridProc.SelectedIndices[i]].Tag).ProcNum);
			if(procedure==null) {
				continue;
			}
			gridProc.ListGridRows[gridProc.SelectedIndices[i]].Tag=procedure.Copy();
		}
		#endregion
		#region Automation
		if(apptSaveHelperResult.DoRunAutomation) {
			AutomationL.Trigger(EnumAutomationTrigger.ProcedureComplete,_listProceduresForAppointment.FindAll(x => x.AptNum==_appointment.AptNum)
				.Select(x => ProcedureCodes.GetStringProcCode(x.CodeNum)).ToList(),_appointment.PatNum);
		}
		if(_appointment.AptStatus==ApptStatus.Complete) {
			Procedures.AfterProcsSetComplete(listProceduresGridSelected);
			if(_appointmentOld.AptStatus!=ApptStatus.Complete) {
				AutomationL.Trigger(EnumAutomationTrigger.ApptComplete,null,_appointment.PatNum);
			}
		}
		#endregion Automation
		#region Broken Appt Logic
		//Do the appointment "break" automation for appointments that were just broken or going to the unscheduled list (sometimes).
		//If BrokenApptRequiredOnMove is on and a user selects the unsched list drop down item, the appointment 
		//ends up here with a status of UnschedList because the appointment has not been broken yet.
		if(_appointment.AptStatus==ApptStatus.Broken && _appointmentOld.AptStatus!=ApptStatus.Broken || (PrefC.GetBool(PrefName.BrokenApptRequiredOnMove)
		                                                                                                 && _appointment.AptStatus==ApptStatus.UnschedList && _appointmentOld.AptStatus==ApptStatus.Scheduled)) {
			AppointmentL.BreakApptHelper(_appointment,_patient,_procedureCodeBroken);
			if(isClosing) {
				switch(_apptBreakSelection) {//ApptBreakSelection.None by default.
					case ApptBreakSelection.Unsched:
						AppointmentL.SetApptUnschedHelper(_appointment,_patient);
						break;
					case ApptBreakSelection.Pinboard:
						AppointmentL.CopyAptToPinboardHelper(_appointment);
						break;
					case ApptBreakSelection.None://User did not makes selection
					case ApptBreakSelection.ApptBook://User made selection, no extra logic required.
						break;
				}
			}
		}
		#endregion Broken Appt Logic
		#region Cleanup Empty Apts
		Appointments.DeleteEmptyAppts(listAptNums_ToDelete, _appointment.PatNum);
		#endregion
		return true;
	}
	#endregion Methods - Private

	private void butDelete_Click(object sender,EventArgs e) {
		OnDelete_Click();
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		var datePrevious=_appointment.DateTStamp;
		if(comboProv.GetSelectedProvNum()==0) {
			MsgBox.Show(this,"Please select a provider.");
			return;
		}
		if(_appointmentOld.AptStatus!=ApptStatus.UnschedList && _appointment.AptStatus==ApptStatus.UnschedList) {
			//Extra log entry if the appt was sent to the unscheduled list
			var permissions=EnumPermType.AppointmentMove;
			if(_appointmentOld.AptStatus==ApptStatus.Complete) {
				permissions=EnumPermType.AppointmentCompleteEdit;
			}
			SecurityLogs.MakeLogEntry(permissions,_appointment.PatNum,_appointment.ProcDescript+", "+_appointment.AptDateTime
			                                                          +", Sent to Unscheduled List",_appointment.AptNum,datePrevious);
		}
		#region Validate Apt Start and End
		var minutes=contrApptProvSlider.Pattern.Length*5;
		//compare beginning of new appointment against end to see if they fall on different days
		if(_appointment.AptDateTime.Day!=_appointment.AptDateTime.AddMinutes(minutes).Day) {
			MsgBox.Show(this,"You cannot have an appointment that starts and ends on different days.");
			return;
		}
		#endregion
		if(!UpdateListAndDB(isClosing: true, doCreateSecLog: true, doInsertHL7: true)) {
			return;
		}
		if(PrefC.GetBool(PrefName.ApptsRequireProc)
		   && (_appointment.AptStatus==ApptStatus.Scheduled || _appointment.AptStatus==ApptStatus.Planned)
		   && gridProc.SelectedIndices.Length==0) {
			MsgBox.Show("At least one procedure must be attached to the appointment.");
			return;
		}
		if(IsNew) {
			//Refresh pat to check if PatStatus has changed.
			var patOld=Patients.GetPat(_patient.PatNum);
			if(patOld.PatStatus==PatientStatus.Deleted) {
				_patient.PatStatus=PatientStatus.Archived;
				if(Patients.Update(_patient,patOld)) {
					MsgBox.Show("Patient has been set to archived because they were deleted by another user while making this appointment.");
				}
			}
		}
		AppointmentL.ShowKioskManagerIfNeeded(_appointmentOld,_appointment.Confirmed);
		DialogResult=DialogResult.OK;
		if(!this.Modal) {
			Close();
		}
	}

	private void FormApptEdit_FormClosing(object sender,FormClosingEventArgs e) {
		Signalods.SetInvalid(InvalidType.BillingList);
		if(_appointment==null) {//Could not find _appointment in the Db on load.
			return;
		}
		if(PrefC.GetBool(PrefName.ApptsRequireProc)
		   && (_appointment.AptStatus==ApptStatus.Scheduled || _appointment.AptStatus==ApptStatus.Planned)
		   && gridProc.SelectedIndices.Length==0 && !_isDeleted && !IsNew) {
			MsgBox.Show("At least one procedure must be attached to the appointment.");
			e.Cancel=true; //form won't close until procedure is attached or appointment is canceled.
			return;
		}
		//Do not use pat.PatNum here.  Use _appointment.PatNum instead.  Pat will be null in the case that the user does not have the appt create permission.
		var datePrevious=_appointment.DateTStamp;
		if(DialogResult!=DialogResult.OK) {
			var requiredProcMsg=AppointmentTypes.CheckRequiredProcsAttached(_appointment.AppointmentTypeNum, gridProc.SelectedTags<Procedure>());
			if(requiredProcMsg!="" && !_isDeleted){
				MsgBox.Show(requiredProcMsg);
				e.Cancel=true; //form won't close until procedure is attached or appointment is canceled.
				return;
			}
			if(_appointment.AptStatus==ApptStatus.Complete) {
				//This is a completed appointment and we need to warn the user if they are trying to leave the window and need to detach procs first.
				for(var i=0;i<gridProc.ListGridRows.Count;i++) {
					var attached=false;
					if(_appointment.AptStatus==ApptStatus.Planned && ((Procedure)gridProc.ListGridRows[i].Tag).PlannedAptNum==_appointment.AptNum) {
						attached=true;
					}
					else if(((Procedure)gridProc.ListGridRows[i].Tag).AptNum==_appointment.AptNum) {
						attached=true;
					}
					if(((Procedure)gridProc.ListGridRows[i].Tag).ProcStatus!=ProcStat.TP || !attached) {
						continue;
					}
					if(!Security.IsAuthorized(EnumPermType.AppointmentCompleteEdit,suppressMessage: true)) {
						continue;
					}
					MsgBox.Show(this,"Detach treatment planned procedures or click OK in the appointment edit window to set them complete.");
					e.Cancel=true;
					return;
				}
			}
			if(IsNew) {
				SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit,_appointment.PatNum,
					"Create cancel for date/time: "+_appointment.AptDateTime,
					_appointment.AptNum,datePrevious);
				//If cancel was pressed we want to un-do any changes to other appointments that were done.
				_listAppointments=Appointments.GetAppointmentsForProcs(_listProceduresForAppointment);
				//Add the current appointment if it is not in this list so it can get properly deleted by the sync later.
				if(!_listAppointments.Exists(x => x.AptNum==_appointment.AptNum)) {
					_listAppointments.Add(_appointment);
				}
				//We need to add this current appointment to the list of old appointments so we run the Appointments.Delete fucntion on it
				//This will remove any procedure connections that we created while in this window.
				_listAppointmentsOld=_listAppointments.Select(x => x.Copy()).ToList();
				//Now we also have to remove the appointment that was pre-inserted and is in this list as well so it is deleted on sync.
				_listAppointments.RemoveAll(x => x.AptNum==_appointment.AptNum);
			}
			else {  //User clicked cancel (or X button) on an existing appt
				_appointment=_appointmentOld.Copy();  //We do not want to save any other changes made in this form.
				//Setting _appointment to a copy of _appointmentOld causes the _appointment reference in _listAppointments to be lost. Remove and add back in so the sync below does not make 
				//any changes to _appointment. We had an issue with changes to _appointment were happening outside of the OK_Click method.
				_listAppointments.RemoveAll(x => x.AptNum==_appointment.AptNum);
				_listAppointments.Add(_appointment);
				if(_appointment.AptStatus==ApptStatus.Scheduled && PrefC.GetBool(PrefName.InsChecksFrequency) && !CheckFrequencies()) {
					e.Cancel=true;
					return;
				}
			}
		}
		else {//DialogResult==DialogResult.OK (User clicked OK or Delete)
			//Note that Procedures.Sync is never used.  This is intentional.  In order to properly use procedure.Sync logic in this form we would
			//need to enhance ProcEdit and all its possible child forms to also not insert into DB until OK is clicked.  This would be a massive undertaking
			//and as such we just immediately push changes to DB.
			if(_appointment.AptStatus==ApptStatus.Scheduled && !_isDeleted && PrefC.GetBool(PrefName.InsChecksFrequency) && !CheckFrequencies()) {
				e.Cancel=true;
				return;
			}
			if(_appointment.AptStatus==ApptStatus.Scheduled) {
				//find all procs that are currently attached to the appt that weren't when the form opened
				var listProcureCodes = _listProceduresForAppointment.FindAll(x => x.AptNum==_appointment.AptNum && !_listProcNumsAttachedStart.Contains(x.ProcNum))
					.Select(x => ProcedureCodes.GetStringProcCode(x.CodeNum)).Distinct().ToList();//get list of string proc codes
				AutomationL.Trigger(EnumAutomationTrigger.ProcSchedule,listProcureCodes,_appointment.PatNum);
			}
		}
		if(_appointmentOld.AptStatus!=ApptStatus.Complete && _appointment.AptStatus==ApptStatus.Complete) {
			//If necessary, prompt the user to ask the patient to opt in to using Short Codes.
			FrmShortCodeOptIn.PromptIfNecessary(_patient,_appointment.ClinicNum);
		}
		//Sync detaches any attached procedures within Appointments.Delete() but doesn't create any ApptComm items.
		if(Appointments.Sync(_listAppointments,_listAppointmentsOld)) {
			ODEvent.Fire(ODEventType.AppointmentEdited,_appointment);
		}
		//Synch the recalls for this patient.  This is necessary in case the date of the appointment has change or has been deleted entirely.
		Recalls.Synch(_appointment.PatNum);
		Recalls.SynchScheduledApptFull(_appointment.PatNum);
	}
	

}