using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using CodeBase;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Logic;
using OpenDentBusiness.WebTypes.Shared.XWeb;
using OpenDentBusiness.Eclaims;

namespace OpenDental;

public partial class ControlAccount:UserControl {
	#region Fields - Public
	///<summary>Public so this can be checked from FormOpenDental and the note can be saved.  Necessary because in some cases the leave event doesn't
	///fire, like when a user switches to a non-modal form, like big phones, and switches patients from that form.</summary>
	public bool IsFinNoteChanged;
	
	///<summary>Public so this can be checked from FormOpenDental and the note can be saved.  Necessary because in some cases the leave event doesn't
	///fire, like when a user switches to a non-modal form, like big phones, and switches patients from that form.</summary>
	public bool IsUrgFinNoteChanged;
	#endregion Fields - Public	

	#region Fields - Private
	private int _scrollValueWhenDoubleClick=-1;
	private int _scrollValueWhenDoubleClickTpUnearned=-1;
	///<summary>This holds some of the data needed for display.  It is retrieved in one call to the database.</summary>
	private DataSet _dataSetMain;
	private Def[] _arrayDefsAcctProcQuickAdd;
	private Family _family;
	private FormRpServiceDateView _formRpServiceDateView=null;
	private bool _isInitializedOnStartup;
	private List<DisplayField> _listDisplayFieldsForMainGrid;
	///<summary>List of all orthocases for the selected patient.</summary>
	private List<OrthoCase> _listOrthoCases= [];
	private PatField[] _patFieldArray;
	private List<DisplayField> _listDisplayFieldsPatInfo;
	private RepeatCharge[] _repeatChargeArray;
	///<summary>Shows a breakdown of payment split totals by each unearned type.</summary>
	private Label labelUnearnedBreakdown=new Label();
	private List<PaySplit> _listPaySplitsHidden= [];
	///<summary>This holds nearly all of the data needed for display.  It is retrieved in one call to the database.</summary>
	private AccountModules.LoadData _loadData;
	///<summary>Partially implemented lock object for an attempted bug fix.</summary>
	private object _lockDataSetMain=new object();
	private GridOD gridUnearnedBreakdown=new GridOD { TitleVisible=false,VScrollVisible=false,Visible=false };
	private GridOD gridInsEstOpenClaims=new GridOD { TitleVisible=false,VScrollVisible=false,Visible=false };
		
	private Patient _patient;
	private PatientNote _patientNote;
	///<summary>Gets updated to PatCur.PatNum that the last security log was made with so that we don't make too many security logs for this patient.  When _patNumLast no longer matches PatCur.PatNum (e.g. switched to a different patient within a module), a security log will be entered.  Gets reset (cleared and the set back to PatCur.PatNum) any time a module button is clicked which will cause another security log to be entered.</summary>
	private long _patNumLast;
	private decimal _patientPortionBalanceTotal;
	private string _famUrgFinNoteOnLoad;
	private string _famFinNoteOnLoad;
	///<summary>Used to track status of split panel visibility to avoid flicker from changing status too often.</summary>
	private bool _showGridPayPlan;
	///<summary>Used to track status of split panel visibility to avoid flicker from changing status too often.</summary>
	private bool _showGridRepeating;
	private List<Patient> _listPatientsSuperFamilyGuarantors;
	private List<Patient> _listPatientsSuperFamilyMembers;
	private bool _useSuperFam;
	private bool _superFamPrefEnabled;
	#endregion Fields - Private

	#region Constructor
		
	public ControlAccount() {
		InitializeComponent();// This call is required by the Windows.Forms Form Designer.
		Font=new("Microsoft Sans Serif", 8.25f);
		Controls.Add(gridUnearnedBreakdown);
		Controls.Add(gridInsEstOpenClaims);
		ToolBarMain.Controls.Add(textQuickProcs);
	}
	#endregion Constructor

	#region Delegates
	private delegate void ToolBarClick();
	#endregion Delegates

	#region Structs Nested
	private struct AutoOrthoPat {
		public InsPlan InsPlan_;
		public PatPlan PatPlan_;
		public string CarrierName;
		public string SubID;
		public double DefaultFee;
	}
	#endregion Structs Nested

	#region Properties
	public long GetPatNum() {
		return _patient.PatNum;
	}

	///<summary>True if 'Entire Family' is selected in the Select Patient grid.</summary>
	private bool IsFamilySelected() {
		return gridAcctPat.GetSelectedIndex()==gridAcctPat.ListGridRows.Count-1 || (_useSuperFam && SuperFamHasData());
	}

	private bool SuperFamHasData() {
		if(_listPatientsSuperFamilyMembers==null || _listPatientsSuperFamilyGuarantors==null) {
			return false;
		}
		if(_listPatientsSuperFamilyMembers.Count==0 || _listPatientsSuperFamilyGuarantors.Count==0) {
			return false;
		}
		return true;
	}
	#endregion Properties	

	#region Methods - Event Handlers Buttons
	private void but45days_Click(object sender,EventArgs e) {
		textDateStart.Text=DateTime.Today.AddDays(-45).ToShortDateString();
		textDateEnd.Text="";
		ModuleSelected(_patient.PatNum);
	}

	private void but90days_Click(object sender,EventArgs e) {
		textDateStart.Text=DateTime.Today.AddDays(-90).ToShortDateString();
		textDateEnd.Text="";
		ModuleSelected(_patient.PatNum);
	}

	private void ButAddOrthoCase_Click(object sender,EventArgs e) {
		using var formOrthoCase=new FormOrthoCase(true,_patient);
		formOrthoCase.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void butAutoOrthoDefaultMonthsTreat_Click(object sender,EventArgs e) {
		//Setting OrthoMonthsTreatOverride locks this value into place just in case it the pref changes down the road.
		_patientNote.OrthoMonthsTreatOverride=PrefC.GetByte(PrefName.OrthoDefaultMonthsTreat);
		PatientNotes.Update(_patientNote,_patient.Guarantor);
		FillAutoOrtho();
	}

	private void butAutoOrthoDefaultPlacement_Click(object sender,EventArgs e) {
		_patientNote.DateOrthoPlacementOverride=DateTime.MinValue;
		PatientNotes.Update(_patientNote,_patient.Guarantor);
		FillAutoOrtho();
	}

	private void butAutoOrthoEditMonthsTreat_Click(object sender,EventArgs e) {
		int txMonths;
		try {
			txMonths=SIn.Byte(textAutoOrthoMonthsTreat.Text);
		}
		catch {
			MsgBox.Show(this,"Please enter a number between 0 and 255.");
			return;
		}
		_patientNote.OrthoMonthsTreatOverride=txMonths;
		PatientNotes.Update(_patientNote,_patient.Guarantor);
		FillAutoOrtho();
	}

	private void butCreditCard_Click(object sender,EventArgs e) {
		using var formCreditCardManage=new FormCreditCardManage(_patient);
		formCreditCardManage.ShowDialog();
	}

	private void butDatesAll_Click(object sender,EventArgs e) {
		textDateStart.Text="";
		textDateEnd.Text="";
		ModuleSelected(_patient.PatNum);
	}

	private void butEditAutoOrthoPlacement_Click(object sender,EventArgs e) {
		DateTime dateOrthoPlacement;
		try {
			dateOrthoPlacement=SIn.Date(textDateAutoOrthoPlacement.Text);
		}
		catch {
			MsgBox.Show(this,"Invalid date.");
			return;
		}
		_patientNote.DateOrthoPlacementOverride=dateOrthoPlacement;
		PatientNotes.Update(_patientNote,_patient.Guarantor);
		FillAutoOrtho();
	}

	private void ButMakeOrthoCaseActive_Click(object sender,EventArgs e) {
		if(gridOrthoCases.SelectedGridRows.Count<1) {
			return;
		}
		var orthoCase=(OrthoCase)gridOrthoCases.SelectedGridRows[0].Tag;
		_listOrthoCases=OrthoCases.Activate(orthoCase,_patient.PatNum);
		RefreshOrthoCasesGridRows();
		var orthoProcLink=OrthoProcLinks.GetByType(orthoCase.OrthoCaseNum,OrthoProcType.Debond);
		if(orthoProcLink!=null) {//If link exists debond proc must be complete
			MsgBox.Show(this,"The activated Ortho Case has a completed debond procedure. This procedure must be detached before others can be added.");
		}
	}

	private void butRefresh_Click(object sender,EventArgs e) {
		if(_patient==null) {
			return;
		}
		ModuleSelected(_patient.PatNum);
	}

	private void butServiceDateView_Click(object sender,EventArgs e) {
		//If the window is already open and it's for the same patient, bring the window to front. Otherwise close and/or open it.
		var patNum=_patient.PatNum;
		if(IsFamilySelected()) {
			patNum=_family.Guarantor.PatNum;
		}
		if(_formRpServiceDateView!=null && (_formRpServiceDateView.PatNum!=patNum || _formRpServiceDateView.IsFamily!=IsFamilySelected())) {
			_formRpServiceDateView.Close();
			_formRpServiceDateView=null;
		}
		if(_formRpServiceDateView?.IsDisposed!=false) {
			_formRpServiceDateView=new FormRpServiceDateView(patNum,IsFamilySelected());
			_formRpServiceDateView.FormClosed+=new FormClosedEventHandler((_,_) => _formRpServiceDateView=null);
			_formRpServiceDateView.Show();
		}
		if(_formRpServiceDateView.WindowState==FormWindowState.Minimized) {
			_formRpServiceDateView.WindowState=FormWindowState.Normal;
		}
		_formRpServiceDateView.BringToFront();
	}

	private void butToday_Click(object sender,EventArgs e) {
		textDateStart.Text=DateTime.Today.ToShortDateString();
		textDateEnd.Text=DateTime.Today.ToShortDateString();
		ModuleSelected(_patient.PatNum);
	}
	#endregion Methods - Event Handlers Buttons

	#region Methods - Event Handlers CheckBoxes
	private void CheckHideInactiveOrthoCases_CheckedChanged(object sender,EventArgs e) {
		RefreshOrthoCasesGridRows();
	}

	private void checkShowCompletePayPlans_Click(object sender,EventArgs e) {
		Prefs.UpdateBool(PrefName.AccountShowCompletedPaymentPlans,checkShowCompletePayPlans.Checked);
		FillPaymentPlans();
		RefreshModuleScreen(false); //so the grids get redrawn if the payment plans grid hides/shows itself.
	}

	private void checkShowDetail_Click(object sender,EventArgs e) {
		var userOdPref=UserOdPrefs.GetFirstOrNewByUserAndFkeyType(Security.CurUser.UserNum,UserOdFkeyType.AcctProcBreakdown);
		userOdPref.ValueString=SOut.Bool(checkShowDetail.Checked);
		UserOdPrefs.Upsert(userOdPref);
		DataValid.SetInvalid(InvalidType.UserOdPrefs);
		if(_patient==null) {
			return;
		}
		ModuleSelected(_patient.PatNum);
	}

	private void checkShowFamilyComm_Click(object sender,EventArgs e) {
		FillComm();
	}

	///<summary>Uses the UserODPref to store ShowAutomatedCommlog separately from the chart module.</summary>
	private void checkShowAutoComm_Click(object sender, EventArgs e) {
		var userOdPref=UserOdPrefs.GetFirstOrNewByUserAndFkeyType(Security.CurUser.UserNum,UserOdFkeyType.ShowAutomatedCommlog);
		userOdPref.ValueString=SOut.Bool(checkShowCommAuto.Checked);
		UserOdPrefs.Upsert(userOdPref);
		DataValid.SetInvalid(InvalidType.UserOdPrefs);
		if(_patient==null) {
			return;
		}
		ModuleSelected(_patient.PatNum);
	}

	private void checkUseSuperFam_CheckedChanged(object sender,EventArgs e) {
		Cursor=Cursors.WaitCursor;
		_useSuperFam=checkUseSuperFam.Checked;
		if(gridAcctPat.SelectedTag<Patient>() is Patient patient) {
			GlobalFormOpenDental.PatientSelected(patient,false);
			ModuleSelected(_patient.PatNum,true);
		}
		Cursor=Cursors.Default;
	}

	#endregion Methods - Event Handlers CheckBoxes

	#region Methods - Event Handlers ContextMenus
	///<summary>Hides the 'Add Adjustment' and 'Refund' context menus if anything other than a procedure (for adjustment) or one single payment (for refund) is selected.</summary>
	private void contextMenuAcctGrid_Popup(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		var listIdxRowsSelected=gridAccount.SelectedIndices.ToList();
		//Add Adjustment---------------------------------------------------------------------------------------------------
		menuItemAddAdj.Enabled=true;
		//Disable the Add Adjustment menu item if any non-procedure rows are selected.
		for(var i=0;i<listIdxRowsSelected.Count;i++) {
			if(table.Rows[listIdxRowsSelected[i]]["ProcNum"].ToString()!="0") {
				continue;
			}
			menuItemAddAdj.Enabled=false;
			break;
		}
		//Refund-----------------------------------------------------------------------------------------------------------
		Payment payment=null;
		menuItemAddRefund.Enabled=true;
		menuItemAddRefundWorkNotPerformed.Enabled=true;
		for(var i=0;i<listIdxRowsSelected.Count;i++) {
			var payNum=SIn.Long(table.Rows[listIdxRowsSelected[i]]["PayNum"].ToString());
			if(payNum==0) {
				continue;//something is selected that's not a payment, move on.
			}
			payment??=Payments.GetPayment(payNum);
			if(payment.PayNum!=payNum) {
				//more than one payment was selected and they aren't the same payment.
				menuItemAddRefund.Enabled=false;
				menuItemAddRefundWorkNotPerformed.Enabled=false;
				break;
			}
		}
		//Disable the refund menu item if no payments were selected.
		if(payment==null) {
			menuItemAddRefund.Enabled=false;
			menuItemAddRefundWorkNotPerformed.Enabled=false;
		}
		//DentalXChange Attachments----------------------------------------------------------------------------------------
		//Hide DXC options outright if Canada user
		if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {//Canadian. en-CA or fr-CA
			menuItemSnipAttachment.Visible=false;
			menuItemSelectImage.Visible=false;
			menuItemPasteAttachment.Visible=false;
			menuItemAttachmentHistory.Visible=false;
		}
		menuItemSnipAttachment.Enabled=true;
		menuItemSelectImage.Enabled=true;
		menuItemPasteAttachment.Enabled=true;
		menuItemAttachmentHistory.Enabled=true;
		var clearingHouse=new Clearinghouse();
		var countClaim=listIdxRowsSelected.Count(x => table.Rows[x]["ProcCode"].ToString()=="Claim");//See AccountModules.GetAccount() Claims region
		//Must be exactly 1 claim selected.
		if(countClaim==1) {
			clearingHouse=GetClearingHouseForClaim();
		}
		else{
			menuItemSnipAttachment.Enabled=false;
			menuItemSelectImage.Enabled=false;
			menuItemPasteAttachment.Enabled=false;
			menuItemAttachmentHistory.Enabled=false;
		}
		//Are attachments allowed to be sent and is the office using ClaimConnect
		if(clearingHouse?.IsAttachmentSendAllowed!=true || clearingHouse.CommBridge!=EclaimsCommBridge.ClaimConnect) {
			menuItemSnipAttachment.Enabled=false;
			menuItemSelectImage.Enabled=false;
			menuItemPasteAttachment.Enabled=false;
			menuItemAttachmentHistory.Enabled=false;
		}
		// Edit PayPlan Charge --------------------------------------------------------------------------------------------
		menuItemEditPayPlanCharge.Visible=false;
		menuItemEditPayPlanCharge.Enabled=false;
		if(!Security.IsAuthorized(EnumPermType.PayPlanEdit,suppressMessage:true)) {
			return;
		}
		// Only one row should be selected, and it should be a PayPlanCharge.
		long payPlanChargeNum=0;
		if(listIdxRowsSelected.Count==1) {
			payPlanChargeNum=SIn.Long(table.Rows[listIdxRowsSelected[0]]["PayPlanChargeNum"].ToString());
		}
		if(payPlanChargeNum!=0) {
			menuItemEditPayPlanCharge.Visible=true;
			// Enabled if not a down payment.
			var description=table.Rows[listIdxRowsSelected[0]]["description"].ToString();
			if(!description.Contains("Down Payment")) { // Logic used to determine 'IsDownPayment' in FormPayPlanDynamic
				menuItemEditPayPlanCharge.Enabled=true;
			}
		}
		//Delete PayPlan Charge--------------------------------------------------------------------------------------------
		menuItemDeletePayPlanCharge.Visible=false;
		for(var i=0;i<listIdxRowsSelected.Count;i++) {
			payPlanChargeNum=SIn.Long(table.Rows[listIdxRowsSelected[i]]["PayPlanChargeNum"].ToString());
			if(payPlanChargeNum==0) {
				continue;
			}
			menuItemDeletePayPlanCharge.Visible=true;
		}
	}

	private void contextMenuPayment_Popup(object sender, EventArgs e) {
		menuItemIncomeTransfer.Visible=PrefC.GetBool(PrefName.ShowIncomeTransferManager);
	}

	///<summary>This gets run just prior to the contextMenuQuickCharge menu displaying to the user.</summary>
	private void contextMenuQuickProcs_Popup(object sender,EventArgs e) {
		//Dynamically fill contextMenuQuickCharge's menu items because the definitions may have changed since last time it was filled.
		_arrayDefsAcctProcQuickAdd=Defs.GetDefsForCategory(DefCat.AccountQuickCharge,true).ToArray();
		contextMenuQuickProcs.MenuItems.Clear();
		if(!PrefC.GetString(PrefName.SalesTaxProcCode).IsNullOrEmpty()) {
			contextMenuQuickProcs.MenuItems.Add(new MenuItem(Lan.g(this,"Sales Tax"),menuItemSalesTaxProc_Click));
		}
		for(var i=0;i<_arrayDefsAcctProcQuickAdd.Length;i++) {
			contextMenuQuickProcs.MenuItems.Add(new MenuItem(_arrayDefsAcctProcQuickAdd[i].ItemName,menuItemQuickProcs_Click));
		}
		if(_arrayDefsAcctProcQuickAdd.Length==0) {
			contextMenuQuickProcs.MenuItems.Add(new MenuItem(Lan.g(this,"No quick charge procedures defined. Go to Setup | Definitions to add."),(_,_) => { }));//"null" event handler.
		}
	}
	#endregion Methods - Event Handlers ContextMenus

	#region Methods - Event Handlers ContrAccount
	private void ContrAccount_Layout(object sender,LayoutEventArgs e) {
		//see LayoutPanels()
	}

	private void ContrAccount_Load(object sender,EventArgs e) {
		this.Parent.MouseWheel+=Parent_MouseWheel;
		_superFamPrefEnabled=PrefC.GetBool(PrefName.ShowFeatureSuperfamilies);
		ToggleUseSuperFamCheckboxEnabled(_superFamPrefEnabled);
	}

	private void ContrAccount_Resize(object sender,EventArgs e) {
		if(PrefC.HListIsNull()) {
			return;//helps on startup.
		}
		LayoutPanelsAndRefreshMainGrids();
	}
	#endregion Methods - Event Handlers ContrAccount

	#region Methods - Event Handlers Forms
	/// <summary>Event handler for closing FormSheetFillEdit when it is non-modal.</summary>
	private void FormSheetFillEdit_FormClosing(object sender,FormClosingEventArgs e) {
		if(((FormSheetFillEdit)sender).DialogResult==DialogResult.OK || ((FormSheetFillEdit)sender).DidChangeSheet) {
			ModuleSelected(_patient.PatNum);
		}
	}
	#endregion Methods - Event Handlers Forms

	#region Methods - Event Handlers Grids
	private void gridAccount_CellClick(object sender,ODGridClickEventArgs e) {
		var table=_dataSetMain.Tables["account"];
		//this seems to fire after a doubleclick, so this prevents error:
		if(e.Row>=table.Rows.Count) {
			return;
		}
		gridPayPlan.SetAll(false);
		for(var i=0;i<gridAccount.SelectedIndices.Length;i++) {
			if(gridAccount.SelectedIndices[i]>table.Rows.Count-1) {
				continue;//An office was getting an exception here, but we're not sure how grid and table could be out of sync and can't duplicate.
			}
			if(table.Rows[gridAccount.SelectedIndices[i]]["PayPlanNum"].ToString()=="0") {
				continue;
			}
			for(var j=0;j<gridPayPlan.ListGridRows.Count;j++) {
				if(((DataRow)gridPayPlan.ListGridRows[j].Tag)["PayPlanNum"].ToString()==table.Rows[gridAccount.SelectedIndices[i]]["PayPlanNum"].ToString()) {
					gridPayPlan.SetSelected(j,true);
				}
			}
			if(table.Rows[gridAccount.SelectedIndices[i]]["procsOnObj"].ToString()=="0") {
				continue;
			}
			for(var j=0;j<table.Rows.Count;j++) {//loop through all rows
				if(j>=gridAccount.ListGridRows.Count) {
					break;
				}
				if(table.Rows[j]["ProcNum"].ToString()==table.Rows[gridAccount.SelectedIndices[i]]["procsOnObj"].ToString()) {
					gridAccount.SetSelected(j,true);//select the pertinent procedure
					break;
				}
				if(table.Rows[j]["AdjNum"].ToString()==table.Rows[gridAccount.SelectedIndices[i]]["adjustsOnObj"].ToString()) {
					gridAccount.SetSelected(j,true);//select the pertinent adjustment
					break;
				}
			}
		}
		for(var i=0;i<gridAccount.SelectedIndices.Length;i++) {
			if(gridAccount.SelectedIndices[i]>=table.Rows.Count) {
				continue;//An office was getting an exception here, but we're not sure how grid and table could be out of sync and can't duplicate.
			}
			var dataRow=table.Rows[gridAccount.SelectedIndices[i]];
			if(dataRow["ClaimNum"].ToString()!="0") {//claims and claimpayments
				//Since we removed all selected items above, we need to reselect the claim the user just clicked on at the very least.
				//The "procsOnObj" column is going to be a comma delimited list of ProcNums associated to the corresponding claim.
				var listProcsOnClaim=dataRow["procsOnObj"].ToString().Split(',').ToList();
				//Loop through the entire table and select any rows that are related to this claim (payments) while keeping track of their related ProcNums.
				for(var j=0;j<table.Rows.Count;j++) {//loop through all rows
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(table.Rows[j]["ClaimNum"].ToString()==dataRow["ClaimNum"].ToString()) {
						gridAccount.SetSelected(j,true);//for the claim payments
						listProcsOnClaim.AddRange(table.Rows[j]["procsOnObj"].ToString().Split(','));
					}
				}
				//Other software companies allow claims to be created with no procedures attached.
				//This would cause "procsOnObj" to contain a ProcNum of '0' which the following loop would then select seemingly random rows (any w/ ProcNum=0)
				//Therefore, we need to specifically remove any entries of '0' from our procsOnClaim list before looping through it.
				listProcsOnClaim.RemoveAll(x => x=="0");
				//Loop through the table again in order to select any related procedures.
				for(var j=0;j<table.Rows.Count;j++) {
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(listProcsOnClaim.Contains(table.Rows[j]["ProcNum"].ToString())) {
						gridAccount.SetSelected(j,true);
					}
				}
			}
			else if(dataRow["PayNum"].ToString()!="0") {
				var listProcsOnPayment=dataRow["procsOnObj"].ToString().Split(',').ToList();
				var listPaymentsOnObj=dataRow["paymentsOnObj"].ToString().Split(',').ToList();
				var listAdjustsOnPayment=dataRow["adjustsOnObj"].ToString().Split(',').ToList();
				for(var j = 0;j<table.Rows.Count;j++) {//loop through all rows
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(table.Rows[j]["PayNum"].ToString()==dataRow["PayNum"].ToString()) {
						gridAccount.SetSelected(j,true);//for other splits in family view
						listProcsOnPayment.AddRange(table.Rows[j]["procsOnObj"].ToString().Split(','));
						listPaymentsOnObj.AddRange(table.Rows[j]["paymentsOnObj"].ToString().Split(','));
						listAdjustsOnPayment.AddRange(table.Rows[j]["adjustsOnObj"].ToString().Split(','));
					}
				}
				for(var j=0;j<table.Rows.Count;j++) {
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(listProcsOnPayment.Contains(table.Rows[j]["ProcNum"].ToString())) {
						gridAccount.SetSelected(j,true);
					}
					if(listPaymentsOnObj.Contains(table.Rows[j]["PayNum"].ToString())) {
						gridAccount.SetSelected(j,true);
					}
					if(listAdjustsOnPayment.Contains(table.Rows[j]["Adjnum"].ToString())) {
						gridAccount.SetSelected(j,true);
					}
				}
			}
			else if(gridAccount.SelectedIndices.Contains(e.Row) && dataRow["AdjNum"].ToString()!="0" && dataRow["procsOnObj"].ToString()!="0") {
				for(var j=0;j<table.Rows.Count;j++) {
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(table.Rows[j]["ProcNum"].ToString()==dataRow["procsOnObj"].ToString()) {
						gridAccount.SetSelected(j,true);
						break;
					}
				}
			}
			else if(dataRow["ProcNumLab"].ToString()!="0" && dataRow["ProcNumLab"].ToString()!="") {//Canadian Lab procedure, select parents and other associated labs too.
				for(var j=0;j<table.Rows.Count;j++) {
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(table.Rows[j]["ProcNum"].ToString()==dataRow["ProcNumLab"].ToString()) {
						gridAccount.SetSelected(j,true);
						continue;
					}
					if(table.Rows[j]["ProcNumLab"].ToString()==dataRow["ProcNumLab"].ToString()) {
						gridAccount.SetSelected(j,true);
						continue;
					}
				}
			}
			else if(dataRow["ProcNum"].ToString()!="0") {//Not a Canadian lab and is a procedure.
				for(var j=0;j<table.Rows.Count;j++) {
					if(j>=gridAccount.ListGridRows.Count) {
						break;
					}
					if(table.Rows[j]["ProcNumLab"].ToString()==dataRow["ProcNum"].ToString()) {
						gridAccount.SetSelected(j,true);
						continue;
					}
				}
			}
		}
	}

	private void gridAccount_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		_scrollValueWhenDoubleClick=gridAccount.ScrollValue;
		var table=_dataSetMain.Tables["account"];
		if(table.Rows[e.Row]["ProcNum"].ToString()!="0") {
			var procedure=Procedures.GetOneProc(SIn.Long(table.Rows[e.Row]["ProcNum"].ToString()),true);
			var patient=_family.GetPatient(procedure.PatNum);
			using var formProcEdit=new FormProcEdit(procedure,patient,_family);
			formProcEdit.ListClaimProcHists=_loadData.HistList;
			formProcEdit.ListClaimProcHistsLoop= [];
			formProcEdit.ShowDialog();
		}
		else if(table.Rows[e.Row]["AdjNum"].ToString()!="0") {
			var adjustment=Adjustments.GetOne(SIn.Long(table.Rows[e.Row]["AdjNum"].ToString()));
			if(adjustment==null) {
				MsgBox.Show(this,"The adjustment has been deleted.");//Don't return. Fall through to the refresh. 
			}
			else {
				using var formAdjust=new FormAdjust(_patient,adjustment);
				formAdjust.ShowDialog();
			}
		}
		else if(table.Rows[e.Row]["PayNum"].ToString()!="0") {
			var payment=Payments.GetPayment(SIn.Long(table.Rows[e.Row]["PayNum"].ToString()));
			if(payment==null) {
				ODMessageBox.Show(Lans.g("No payment exists.  Please run database maintenance method")+" "+nameof(DatabaseMaintenances.PaySplitWithInvalidPayNum));
				return;
			}
			using var formPayment=new FormPayment(_patient,_family,payment,false);
			formPayment.IsNew=false;
			formPayment.ShowDialog();
		}
		else if(table.Rows[e.Row]["ClaimNum"].ToString()!="0") {//claims and claimpayments
			if(!Security.IsAuthorized(EnumPermType.ClaimView)) {
				return;
			}
			var claim=Claims.GetClaim(SIn.Long(table.Rows[e.Row]["ClaimNum"].ToString()));
			if(claim==null) {
				MsgBox.Show(this,"The claim has been deleted.");
			}
			else {
				var patient=_family.GetPatient(claim.PatNum);
				using var formClaimEdit=new FormClaimEdit(claim,patient,_family);
				formClaimEdit.IsNew=false;
				formClaimEdit.ShowDialog();
			}
		}
		else if(table.Rows[e.Row]["StatementNum"].ToString()!="0") {
			var statement=Statements.GetStatement(SIn.Long(table.Rows[e.Row]["StatementNum"].ToString()));
			if(statement==null) {
				MsgBox.Show(this,"The statement has been deleted");//Don't return. Fall through to the refresh. 
			}
			else {
				using var formStatementOptions=new FormStatementOptions();
				formStatementOptions.StatementCur=statement;
				formStatementOptions.ShowDialog();
			}
		}
		else if(table.Rows[e.Row]["PayPlanNum"].ToString()!="0") {
			var payplan=PayPlans.GetOne(SIn.Long(table.Rows[e.Row]["PayPlanNum"].ToString()));
			if(payplan==null) {
				MsgBox.Show(this,"This pay plan has been deleted by another user.");
				ModuleSelected(_patient.PatNum,IsFamilySelected());
				return;
			}
			if(payplan.IsDynamic) {
				using var formPayPlanDynamic=new FormPayPlanDynamic(payplan);
				formPayPlanDynamic.ShowDialog();
				if(formPayPlanDynamic.PatNumGoto!=0) {
					GlobalFormOpenDental.PatientSelected(Patients.GetPat(formPayPlanDynamic.PatNumGoto),false);
					ModuleSelected(formPayPlanDynamic.PatNumGoto,false);
					return;
				}
			}
			else {//static payplan
				using var formPayPlan=new FormPayPlan(payplan);
				formPayPlan.ShowDialog();
				if(formPayPlan.PatNumGoto!=0) {
					GlobalFormOpenDental.PatientSelected(Patients.GetPat(formPayPlan.PatNumGoto),false);
					ModuleSelected(formPayPlan.PatNumGoto,false);
					return;
				}
			}
		}
		ModuleSelected(_patient.PatNum,IsFamilySelected());
	}

	private void gridAcctPat_CellClick(object sender,ODGridClickEventArgs e) {
		Cursor=Cursors.WaitCursor;
		if(gridAcctPat.SelectedTag<Patient>() is Patient patient) {
			GlobalFormOpenDental.PatientSelected(patient,false);
			ModuleSelected(patient.PatNum,IsFamilySelected());
		}
		Cursor=Cursors.Default;
	}

	private void gridAutoOrtho_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(gridAutoOrtho.ListGridRows[e.Row].Tag==null || gridAutoOrtho.ListGridRows[e.Row].Tag.GetType()!=typeof(AutoOrthoPat)) {
			return;
		}
		var autoOrthoPat=(AutoOrthoPat)gridAutoOrtho.ListGridRows[e.Row].Tag;
		if(autoOrthoPat.InsPlan_.OrthoType!=OrthoClaimType.InitialPlusPeriodic) {
			MsgBox.Show(this,"To view this setup window, the insurance plan must be set to have an Ortho Claim Type of Initial Plus Periodic.");
			return;
		}
		using var formOrthoPat=new FormOrthoPat(autoOrthoPat.PatPlan_,autoOrthoPat.InsPlan_,autoOrthoPat.CarrierName,autoOrthoPat.SubID,autoOrthoPat.DefaultFee);
		formOrthoPat.ShowDialog();
		if(formOrthoPat.DialogResult==DialogResult.OK) {
			PatPlans.Update(autoOrthoPat.PatPlan_);
			FillAutoOrtho();
		}
	}

	private void gridComm_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var row=(int)gridComm.ListGridRows[e.Row].Tag;
		if(_dataSetMain.Tables["Commlog"].Rows[row]["CommlogNum"].ToString()!="0") {
			var commlog=Commlogs.GetOne(SIn.Long(_dataSetMain.Tables["Commlog"].Rows[row]["CommlogNum"].ToString()));
			if(commlog==null) {
				MsgBox.Show(this,"This commlog has been deleted by another user.");
				ModuleSelected(_patient.PatNum);
				return;
			}
			var frmCommItem=new FrmCommItem(commlog);
			frmCommItem.ShowDialog();
			if(frmCommItem.IsDialogOK) {
				ModuleSelected(_patient.PatNum);
			}
			return;
		}
		if(_dataSetMain.Tables["Commlog"].Rows[row]["EmailMessageNum"].ToString()!="0") {
			var emailMessage=
				EmailMessages.GetOne(SIn.Long(_dataSetMain.Tables["Commlog"].Rows[row]["EmailMessageNum"].ToString()));
			if(EmailMessages.IsSecureWebMail(emailMessage.SentOrReceived)) {
				//web mail uses special secure messaging portal
				using var formWebMailMessageEdit=new FormWebMailMessageEdit(_patient.PatNum,emailMessage);
				if(formWebMailMessageEdit.ShowDialog()==DialogResult.OK) {
					ModuleSelected(_patient.PatNum);
				}
				return;
			}
			using var formEmailMessageEdit=new FormEmailMessageEdit(emailMessage);
			formEmailMessageEdit.ShowDialog();
			if(formEmailMessageEdit.DialogResult==DialogResult.OK) {
				ModuleSelected(_patient.PatNum);
			}
			return;
		}
		if(_dataSetMain.Tables["Commlog"].Rows[row]["SheetNum"].ToString()!="0") {
			var sheet=Sheets.GetSheet(SIn.Long(_dataSetMain.Tables["Commlog"].Rows[row]["SheetNum"].ToString()));
			SheetUtilL.ShowSheet(sheet,_patient,FormSheetFillEdit_FormClosing);
		}
	}

	private void GridOrthoCases_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		using var formOrthoCase=new FormOrthoCase(false,_patient,(OrthoCase)gridOrthoCases.ListGridRows[e.Row].Tag);
		formOrthoCase.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void gridPatInfo_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(TerminalActives.PatIsInUse(_patient.PatNum)) {
			MsgBox.Show(this,"Patient is currently entering info at a reception terminal.  Please try again later.");
			return;
		}
		if(gridPatInfo.ListGridRows[e.Row].Tag is PatFieldDef) {//patfield for an existing PatFieldDef
			var patFieldDef=(PatFieldDef)gridPatInfo.ListGridRows[e.Row].Tag;
			var patField=PatFields.GetByName(patFieldDef.FieldName,_patFieldArray);
			PatFieldL.OpenPatField(patField,patFieldDef,_patient.PatNum);
			ModuleSelected(_patient.PatNum);
			return;
		}
		if(gridPatInfo.ListGridRows[e.Row].Tag is PatField) {//PatField for a PatFieldDef that no longer exists
			var patField=(PatField)gridPatInfo.ListGridRows[e.Row].Tag;
			using var formPatFieldEdit=new FormPatFieldEdit(patField);
			formPatFieldEdit.ShowDialog();
			ModuleSelected(_patient.PatNum);
			return;
		}
		if(!Security.IsAuthorized(EnumPermType.PatientEdit)) {
			return;
		}
		using var formPatientEdit=new FormPatientEdit();
		formPatientEdit.Patient=_patient;
		formPatientEdit.Family=_family;
		formPatientEdit.IsNew=false;
		formPatientEdit.ShowDialog();
		if(formPatientEdit.DialogResult==DialogResult.OK) {
			GlobalFormOpenDental.PatientSelected(_patient,false);
		}
		ModuleSelected(_patient.PatNum);
	}

	private void gridPayPlan_CellClick(object sender,ODGridClickEventArgs e) {
		var dataRow=(DataRow)gridPayPlan.ListGridRows[e.Row].Tag;
		if(dataRow["PayPlanNum"].ToString()=="0") {
			return;
		}
		var payPlan=PayPlans.GetOne(SIn.Long(dataRow["PayPlanNum"].ToString()));
		if(payPlan==null) {
			MsgBox.Show(this,"This pay plan has been deleted by another user.");
			return;
		}
		if(gridPayPlan.Columns[e.Col].Heading!="eClipboard") {
			return;
		}
	}

	private void gridPayPlan_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var dataRow=(DataRow)gridPayPlan.ListGridRows[e.Row].Tag;
		if(dataRow["PayPlanNum"].ToString()=="0") {//Installment Plan
			using var formInstallmentPlanEdit=new FormInstallmentPlanEdit();
			formInstallmentPlanEdit.InstallmentPlanCur=InstallmentPlans.GetOne(SIn.Long(dataRow["InstallmentPlanNum"].ToString()));
			formInstallmentPlanEdit.IsNew=false;
			formInstallmentPlanEdit.ShowDialog();
			ModuleSelected(_patient.PatNum);
			return;
		}
		//Payment plan
		var payPlan=PayPlans.GetOne(SIn.Long(dataRow["PayPlanNum"].ToString()));
		if(payPlan==null) {
			MsgBox.Show(this,"This pay plan has been deleted by another user.");
			ModuleSelected(_patient.PatNum,IsFamilySelected());
			return;
		}
		if(payPlan.IsDynamic) {
			using var formPayPlanDynamic=new FormPayPlanDynamic(payPlan);
			formPayPlanDynamic.ShowDialog();
			if(formPayPlanDynamic.PatNumGoto!=0) {
				GlobalFormOpenDental.PatientSelected(Patients.GetPat(formPayPlanDynamic.PatNumGoto),false);
				ModuleSelected(formPayPlanDynamic.PatNumGoto,false);
				return;
			}
			ModuleSelected(_patient.PatNum,IsFamilySelected());
			return;
		}
		using var formPayPlan=new FormPayPlan(payPlan);
		formPayPlan.ShowDialog();
		if(formPayPlan.PatNumGoto!=0) {
			GlobalFormOpenDental.PatientSelected(Patients.GetPat(formPayPlan.PatNumGoto),false);
			ModuleSelected(formPayPlan.PatNumGoto,false);
			return;
		}
		ModuleSelected(_patient.PatNum,IsFamilySelected());
	}

	private void gridRepeat_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		using var formRepeatChargeEdit=new FormRepeatChargeEdit(_repeatChargeArray[e.Row]);
		formRepeatChargeEdit.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void GridTpSplits_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		_scrollValueWhenDoubleClickTpUnearned=gridTpSplits.ScrollValue;
		var paySplit=(PaySplit)gridTpSplits.ListGridRows[e.Row].Tag;
		if(paySplit==null) {
			return;
		}
		var payment=Payments.GetPayment(paySplit.PayNum);
		if(payment==null) {
			MsgBox.Show(this,"Payment does not exist.");
			return;
		}
		using var formPayment=new FormPayment(_patient,_family,payment,false);
		formPayment.IsNew=false;
		formPayment.ShowDialog();
		ModuleSelected(_patient.PatNum,IsFamilySelected());
	}
	#endregion Methods - Event Handlers Grids

	#region Methods - Event Handlers Labels
	private void labelInsRem_Click(object sender,EventArgs e) {
		if(!CultureInfo.CurrentCulture.Name.EndsWith("CA")) {//Canadian. en-CA or fr-CA
			//Since the bonus information in FormInsRemain is currently only helpful in Canada,
			//we have decided not to show the form for other countries at this time.
			return;
		}
		if(_patient==null) {
			return;
		}
		using var formInsRemain=new FormInsRemain(_patient.PatNum);
		formInsRemain.ShowDialog();
	}

	private void labelInsRem_MouseEnter(object sender,EventArgs e) {
		groupBoxFamilyIns.Visible=true;
		groupBoxIndIns.Visible=true;
	}

	private void labelInsRem_MouseLeave(object sender,EventArgs e) {
		groupBoxFamilyIns.Visible=false;
		groupBoxIndIns.Visible=false;
	}

	private void labelDisRem_MouseEnter(object sender,EventArgs e) {
		groupBoxIndDis.Visible=true;
		groupBoxIndDis.Enabled=true;
	}

	private void labelDisRem_MouseLeave(object sender,EventArgs e) {
		groupBoxIndDis.Visible=false;
		groupBoxIndDis.Enabled=false;
	}

	private void labelUnearnedAmt_MouseEnter(object sender,EventArgs e) {
		if(Math.Abs(SIn.Decimal(labelUnearnedAmt.Text))>0) {
			gridUnearnedBreakdown.Visible=true;
			gridUnearnedBreakdown.Enabled=true;
		}
	}

	private void labelUnearnedAmt_MouseLeave(object sender,EventArgs e) {
		gridUnearnedBreakdown.Visible=false;
		gridUnearnedBreakdown.Enabled=false;
	}

	private void labelInsEstAmt_MouseEnter(object sender,EventArgs e) {
		if(Math.Abs(SIn.Decimal(labelInsEstAmt.Text))>0) {
			gridInsEstOpenClaims.Visible=true;
			gridInsEstOpenClaims.Enabled=true;
		}
	}

	private void labelInsEstAmt_MouseLeave(object sender,EventArgs e) {
		gridInsEstOpenClaims.Visible=false;
		gridInsEstOpenClaims.Enabled=false;
	}

	#endregion Methods - Event Handlers Labels

	#region Methods - Event Handlers Menus
	private void menuInsMedical_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.ClaimView)) {
			return;
		}
		if(!ClaimL.CheckClearinghouseDefaults()) {
			return;
		}
		var claimData=AccountModules.GetCreateClaimData(_patient,_family);
		long medSubNum=0;
		for(var i=0;i<claimData.ListPatPlans.Count;i++) {
			var insSub=InsSubs.GetSub(claimData.ListPatPlans[i].InsSubNum,claimData.ListInsSubs);
			if(InsPlans.GetPlan(insSub.PlanNum,claimData.ListInsPlans).IsMedical) {
				medSubNum=insSub.InsSubNum;
				break;
			}
		}
		if(medSubNum==0) {
			MsgBox.Show(this,"Patient does not have medical insurance.");
			return;
		}
		var table=_dataSetMain.Tables["account"];
		Procedure procedure;
		if(gridAccount.SelectedIndices.Length==0) {
			//autoselect procedures
			for(var i=0;i<table.Rows.Count;i++) {//loop through every line showing on screen
				if(table.Rows[i]["ProcNum"].ToString()=="0") {
					continue;//ignore non-procedures
				}
				procedure=Procedures.GetProcFromList(claimData.ListProcs,SIn.Long(table.Rows[i]["ProcNum"].ToString()));
				if(procedure.ProcFee==0) {
					continue;//ignore zero fee procedures, but user can explicitly select them
				}
				if(procedure.MedicalCode=="") {
					continue;//ignore non-medical procedures
				}
				if(Procedures.NeedsSent(procedure.ProcNum,medSubNum,claimData.ListClaimProcs) && i<gridAccount.ListGridRows.Count) {
					gridAccount.SetSelected(i,true);
				}
			}
			if(gridAccount.SelectedIndices.Length==0) {//if still none selected
				MsgBox.Show(this,"Please select procedures first.");
				return;
			}
		}
		var areAllProcedures=true;
		for(var i=0;i<gridAccount.SelectedIndices.Length;i++) {
			if(table.Rows[gridAccount.SelectedIndices[i]]["ProcNum"].ToString()=="0") {
				areAllProcedures=false;
			}
		}
		if(!areAllProcedures) {
			MsgBox.Show(this,"You can only select procedures.");
			return;
		}
		//Medical claims are slightly different so we'll just manually create the CreateClaimDataWrapper needed for creating the claim.
		var createClaimDataWrapper=new CreateClaimDataWrapper {
			Patient_=_patient,
			Family_=_family,
			ListCreateClaimItems=GetCreateClaimItemsFromUI(),
			CreateClaimData_=claimData,
		};
		//Block users for creating claims where the procedure can be associated with duplicate claim procs
		if(ClaimL.WarnUsersForDuplicateClaimProcs(createClaimDataWrapper))  {
			return;
		}
		var claim=new Claim();
		claim.ClaimStatus="W";
		claim.DateSent=DateTime.Today;
		claim.DateSentOrig=DateTime.MinValue;
		//Set ClaimCur to CreateClaim because the reference to ClaimCur gets broken when inserting.
		claim=ClaimL.CreateClaim(claim,"Med",true,createClaimDataWrapper);
		if(claim.ClaimNum==0) {
			ModuleSelected(_patient.PatNum);
			return;
		}
		//still have not saved some changes to the claim at this point
		using var formClaimEdit=new FormClaimEdit(claim,_patient,_family);
		formClaimEdit.IsNew=true;//this causes it to delete the claim if cancelling.
		//If there's unallocated amounts, we want to redistribute the money to other procedures.
		if(formClaimEdit.ShowDialog()==DialogResult.OK) {
			ClaimL.AllocateUnearnedPayment(_patient,_family,SIn.Double(labelUnearnedAmt.Text),claim);
		}
		ModuleSelected(_patient.PatNum);
	}

	private void menuInsOther_Click(object sender,EventArgs e) {
		var createClaimDataWrapper=ClaimL.GetCreateClaimDataWrapper(_patient,_family,GetCreateClaimItemsFromUI(),true,true);
		if(createClaimDataWrapper.HasError) {
			return;
		}
		var claim=new Claim();
		claim.ClaimStatus="U";
		//Set Claim to CreateClaim because the reference to Claim gets broken when inserting.
		claim=ClaimL.CreateClaim(claim,"Other",true,createClaimDataWrapper);
		if(claim.ClaimNum==0) {
			ModuleSelected(_patient.PatNum);
			return;
		}
		//still have not saved some changes to the claim at this point
		using var formClaimEdit=new FormClaimEdit(claim,_patient,_family);
		formClaimEdit.IsNew=true;//this causes it to delete the claim if cancelling.
		if(formClaimEdit.ShowDialog()==DialogResult.OK) {
			ClaimL.AllocateUnearnedPayment(_patient,_family,SIn.Double(labelUnearnedAmt.Text),claim);
		}
		ModuleSelected(_patient.PatNum);
	}

	private void menuInsPri_Click(object sender,EventArgs e) {
		var createClaimDataWrapper=ClaimL.GetCreateClaimDataWrapper(_patient,_family,GetCreateClaimItemsFromUI(),true,true);
		if(createClaimDataWrapper.HasError) {
			return;
		}
		if(PatPlans.GetOrdinal(PriSecMed.Primary,createClaimDataWrapper.CreateClaimData_.ListPatPlans,createClaimDataWrapper.CreateClaimData_.ListInsPlans
			   ,createClaimDataWrapper.CreateClaimData_.ListInsSubs)==0)
		{
			MsgBox.Show(this,"The patient does not have any dental insurance plans.");
			return;
		}
		var claim=new Claim();
		claim.ClaimStatus="W";
		claim.DateSent=DateTime.Today;
		claim.DateSentOrig=DateTime.MinValue;
		//Set Claim to CreateClaim because the reference to Claim gets broken when inserting.
		claim=ClaimL.CreateClaim(claim,"P",true,createClaimDataWrapper);
		if(claim.ClaimNum==0) {
			ModuleSelected(_patient.PatNum);
			return;
		}
		//still have not saved some changes to the claim at this point
		using var formClaimEdit=new FormClaimEdit(claim,_patient,_family);
		formClaimEdit.IsNew=true;//this causes it to delete the claim if cancelling.
		//If there's unallocated amounts, we want to redistribute the money to other procedures.
		if(formClaimEdit.ShowDialog()==DialogResult.OK) {
			ClaimL.AllocateUnearnedPayment(_patient,_family,SIn.Double(labelUnearnedAmt.Text),claim);
		}
		ModuleSelected(_patient.PatNum);
	}

	private void menuInsSec_Click(object sender,EventArgs e) {
		var createClaimDataWrapper=ClaimL.GetCreateClaimDataWrapper(_patient,_family,GetCreateClaimItemsFromUI(),true,true);
		if(createClaimDataWrapper.HasError) {
			return;
		}
		if(createClaimDataWrapper.CreateClaimData_.ListPatPlans.Count<2) {
			ODMessageBox.Show(Lan.g(this,"Patient does not have secondary insurance."));
			return;
		}
		if(PatPlans.GetOrdinal(PriSecMed.Secondary,createClaimDataWrapper.CreateClaimData_.ListPatPlans,createClaimDataWrapper.CreateClaimData_.ListInsPlans
			   ,createClaimDataWrapper.CreateClaimData_.ListInsSubs)==0)
		{
			MsgBox.Show(this,"Patient does not have secondary insurance.");
			return;
		}
		var claim=new Claim();
		claim.ClaimStatus="W";
		claim.DateSent=DateTime.Today;
		claim.DateSentOrig=DateTime.MinValue;
		//Set Claim to CreateClaim because the reference to Claim gets broken when inserting.
		claim=ClaimL.CreateClaim(claim,"S",true,createClaimDataWrapper);
		if(claim.ClaimNum==0) {
			ModuleSelected(_patient.PatNum);
			return;
		}
		using var formClaimEdit=new FormClaimEdit(claim,_patient,_family);
		formClaimEdit.IsNew=true;//this causes it to delete the claim if cancelling.
		//If there's unallocated amounts, we want to redistribute the money to other procedures.
		if(formClaimEdit.ShowDialog()==DialogResult.OK) {
			ClaimL.AllocateUnearnedPayment(_patient,_family,SIn.Double(labelUnearnedAmt.Text),claim);
		}
		ModuleSelected(_patient.PatNum);
	}

	#endregion Methods - Event Handlers Menus

	#region Methods - Event Handlers MenuItem
	private void menuItemAddAdj_Click(object sender,EventArgs e) {
		AddAdjustmentToSelectedProcsHelper();
	}

	private void menuItemAddRefund_Click(object sender,EventArgs e) {
		var menuItem=(MenuItem)sender;
		var table=_dataSetMain.Tables["account"];
		var listRowsSelected=gridAccount.SelectedIndices.ToList();
		Payment paymentExisting=null;
		//Figure out what payment was right clicked on.
		for(var i=0;i<listRowsSelected.Count;i++) {
			if(table.Rows[listRowsSelected[i]]["PayNum"].ToString()!="0") {
				var payNum=SIn.Long(table.Rows[listRowsSelected[i]]["PayNum"].ToString());
				paymentExisting=Payments.GetPayment(payNum);
				break;
			}
		}
		if(paymentExisting==null) {
			MsgBox.Show(this,"Payment is invalid.");
			return;
		}
		var listPaySplitsExisting=PaySplits.GetForPayment(paymentExisting.PayNum);
		//Negative payment
		if(listPaySplitsExisting.Any(x => x.SplitAmt<=0)) {
			MsgBox.Show(this,"Cannot refund payments that have negative splits.");
			return;
		}
		//Attached to payplan
		if(listPaySplitsExisting.Any(x => x.PayPlanNum>0)) {
			MsgBox.Show(this,"Cannot refund payments attached to payment plans.");
			return;
		}
		//Create negative adjustments to offset the production (typically procedures) from the existing payment if the user clicked on 'work not performed'.
		var listAdjustmentsAdded=new List<Adjustment>();
		if(menuItem==menuItemAddRefundWorkNotPerformed) {
			if(PrefC.GetLong(PrefName.RefundAdjustmentType)==0) {
				MsgBox.Show(this,"Refund adjustment type has not been set. Please go to Setup | Account to fix this.");
				return;
			}
			listAdjustmentsAdded=Adjustments.CreateNegativeAdjustmentsForRefund(paymentExisting);
			//Show FormAdjMulti with with procedures to refund, and generated adjustments prefilled in.
			if(listAdjustmentsAdded.Count>0) {
				using var formAdjMulti=new FormAdjMulti(_patient,adjustments:listAdjustmentsAdded);
				if(formAdjMulti.ShowDialog()!=DialogResult.OK) {
					return;
				}
			}
		}
		//Make a offsetting payment that negates each of the paysplits in the existing payment.
		var paymentRefund=Payments.MakeNegativePaymentsRefund(paymentExisting);
		//Show formPayment with the new generated paySplits
		using var formPayment=new FormPayment(_patient,_family,paymentRefund,false);
		formPayment.IsNew=true;
		if(formPayment.ShowDialog()!=DialogResult.OK) {	//if the user clicks cancel or x, undo any changes to the database.
			ODException.SwallowAnyException(() => Payments.Delete(paymentRefund));
			for(var i = 0;i<listAdjustmentsAdded.Count;i++) {
				Adjustments.Delete(listAdjustmentsAdded[i]);
			}
		}
		ModuleSelected(_patient.PatNum, IsFamilySelected());
	}

	private void menuItemAddMultAdj_Click(object sender,EventArgs e) {
		AddAdjustmentToSelectedProcsHelper(true);
	}

	private void menuItemAllocateUnearned_Click(object sender,EventArgs e) {
		toolBarButPay_Click(0,isPrePay:true,isIncomeTransfer:true);
	}

	private void menuItemEditPayPlanCharge_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		var indexSelected = gridAccount.GetSelectedIndex();
		var payPlanChargeNum=SIn.Long(table.Rows[indexSelected]["PayPlanChargeNum"].ToString());
		var listPaySplits=PaySplits.GetForPayPlanCharges([payPlanChargeNum]);
		if(listPaySplits.Count>0) {
			MsgBox.Show(Lan.g(this,"Charges with payments attached cannot be edited."));
			return;
		}
		var payPlanCharge = PayPlanCharges.GetOne(payPlanChargeNum);
		var payPlan = PayPlans.GetOne(payPlanCharge.PayPlanNum);
		using var formPayPlanChargeEdit = new FormPayPlanChargeEdit(payPlanCharge, payPlan);
		formPayPlanChargeEdit.ShowDialog();
		if(formPayPlanChargeEdit.DialogResult==DialogResult.Cancel) {
			return;
		}
		//FormPayPlanChargeEdit sets PayPlanChargeCur to null when the user clicks the delete button on the form.
		if(formPayPlanChargeEdit.PayPlanChargeCur==null) {
			PayPlanCharges.Delete(payPlanCharge);
			SecurityLogs.MakeLogEntry(EnumPermType.PayPlanChargeEdit,payPlanCharge.PatNum,"Deleted.");
		}
		else {
			if(!formPayPlanChargeEdit.ListChangeLog.IsNullOrEmpty()) {
				var log=PayPlans.GetChangeLog(formPayPlanChargeEdit.ListChangeLog);
				SecurityLogs.MakeLogEntry(EnumPermType.PayPlanChargeEdit,payPlanCharge.PatNum,log);
			}
			PayPlanCharges.Update(payPlanCharge);
		}
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemDeletePayPlanCharge_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		var listSelectedPayPlanChargeNums=new List<long>();
		var listIndices=gridAccount.SelectedIndices.ToList();
		for(var i=0;i<listIndices.Count;i++) {
			var payPlanChargeNum=SIn.Long(table.Rows[listIndices[i]]["PayPlanChargeNum"].ToString());
			if(payPlanChargeNum==0) {
				continue;
			}
			listSelectedPayPlanChargeNums.Add(payPlanChargeNum);
		}
		var listPayPlanCharges=PayPlanCharges.GetMany(listSelectedPayPlanChargeNums);
		var listPayPlanChargesNotDeleted=PayPlanCharges.DeleteDebitsWithoutPayments(listPayPlanCharges);
		if(listPayPlanChargesNotDeleted.Count > 0) {
			var msgString="Cannot delete";
			if(listPayPlanChargesNotDeleted.Exists(x=>x.Note.ToLower().Contains("down payment"))) {
				msgString+=" down payment charges, or";
			}
			msgString+=" charges with payments attached.";
			MsgBox.Show(Lans.g(msgString));
		}
		if(listPayPlanCharges.Count!=listPayPlanChargesNotDeleted.Count) {
			//at least one payplan charge was deleted.
			SecurityLogs.MakeLogEntry(EnumPermType.PayPlanChargeEdit,_patient.PatNum,"Deleted.");
		}
		ModuleSelected(_patient.PatNum);
	}

	private void MenuItemDynamicPayPlan_Click(object sender,EventArgs e) {
		PayPlanHelper(PayPlanModes.Dynamic);//when payment plan is dynamic, insurance vs. pat does not matter.
	}

	private void menuItemIncomeTransfer_Click(object sender,EventArgs e) {
		using var formIncomeTransferManage=new FormIncomeTransferManage(_family,_patient);
		formIncomeTransferManage.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}
		
	private void menuItemInvoice_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		var listDataRowsSuperFam=new List<DataRow>();
		var patientGuar=Patients.GetPat(_patient.Guarantor);
		var patientSuperHead=Patients.GetPat(_patient.SuperFamily);
		if(gridAccount.SelectedIndices.Length==0
		   && (patientSuperHead==null || !PrefC.GetBool(PrefName.ShowFeatureSuperfamilies) || !patientGuar.HasSuperBilling || !patientSuperHead.HasSuperBilling))
		{
			//autoselect procedures, adjustments, and some pay plan charges
			for(var i=0;i<table.Rows.Count;i++) {//loop through every line showing on screen
				if(table.Rows[i]["ProcNum"].ToString()=="0"
				   && table.Rows[i]["AdjNum"].ToString()=="0"
				   && table.Rows[i]["PayPlanChargeNum"].ToString()=="0")
				{
					continue;//ignore items that aren't procs, adjustments, or pay plan charges
				}
				if(SIn.Date(table.Rows[i]["date"].ToString())!=DateTime.Today) {
					continue;
				}
				if(table.Rows[i]["ProcNum"].ToString()!="0") {//if selected item is a procedure
					var procedure=Procedures.GetOneProc(SIn.Long(table.Rows[i]["ProcNum"].ToString()),false);
					if(procedure.StatementNum!=0) {//already attached so don't autoselect
						continue;
					}
					if(procedure.PatNum!=_patient.PatNum) {
						continue;
					}
				}
				else if(table.Rows[i]["PayPlanChargeNum"].ToString()!="0") {//selected item is pay plan charge
					var payPlanCharge=PayPlanCharges.GetOne(SIn.Long(table.Rows[i]["PayPlanChargeNum"].ToString()));
					if(payPlanCharge.PatNum!=_patient.PatNum) {
						continue;
					}
					if(payPlanCharge.ChargeType!=PayPlanChargeType.Debit) {
						continue;
					}
					if(payPlanCharge.StatementNum!=0) {
						continue;
					}
				}
				else {//item must be adjustment
					var adjustment=Adjustments.GetOne(SIn.Long(table.Rows[i]["AdjNum"].ToString()));
					if(adjustment.StatementNum!=0) {//already attached so don't autoselect
						continue;
					}
					if(adjustment.PatNum!=_patient.PatNum) {
						continue;
					}
				}
				if(i < gridAccount.ListGridRows.Count) {
					gridAccount.SetSelected(i,true);
				}
			}
			if(gridAccount.SelectedIndices.Length==0) {//if still none selected
				MsgBox.Show(this,"Please select procedures, adjustments or payment plan charges first.");
				return;
			}
		}
		else if(gridAccount.SelectedIndices.Length==0
		        && PrefC.GetBool(PrefName.ShowFeatureSuperfamilies) && patientGuar.HasSuperBilling && patientSuperHead.HasSuperBilling)
		{
			//No selections and superbilling is enabled for this family.  Show a window to select and attach procs to this statement for the superfamily.
			using var formInvoiceItemSelect=new FormInvoiceItemSelect(_patient.SuperFamily);
			if(formInvoiceItemSelect.ShowDialog()==DialogResult.Cancel) {
				return;
			}
			listDataRowsSuperFam=formInvoiceItemSelect.SelectedDataRows;
		}
		for(var i=0;i<gridAccount.SelectedIndices.Length;i++) {
			var dataRow=table.Rows[gridAccount.SelectedIndices[i]];
			if(dataRow["ProcNum"].ToString()=="0"
			   && dataRow["AdjNum"].ToString()=="0"
			   && dataRow["PayPlanChargeNum"].ToString()=="0") //the selected item is neither a procedure nor an adjustment
			{
				MsgBox.Show(this,"You can only select procedures, payment plan charges or adjustments.");
				gridAccount.SetAll(false);
				return;
			}
			if(dataRow["ProcNum"].ToString()!="0") {//the selected item is a proc
				var procedure=Procedures.GetOneProc(SIn.Long(dataRow["ProcNum"].ToString()),false);
				if(procedure.PatNum!=_patient.PatNum) {
					MsgBox.Show(this,"You can only select procedures, payment plan charges or adjustments for the current patient on an invoice.");
					gridAccount.SetAll(false);
					return;
				}
				if(procedure.StatementNum!=0) {
					MsgBox.Show(this,"Selected procedure(s) are already attached to an invoice.");
					gridAccount.SetAll(false);
					return;
				}
			}
			else if(dataRow["PayPlanChargeNum"].ToString()!="0") {
				var payPlanCharge=PayPlanCharges.GetOne(SIn.Long(dataRow["PayPlanChargeNum"].ToString()));
				if(payPlanCharge.PatNum!=_patient.PatNum) {
					MsgBox.Show(this,"You can only select procedures, payment plan charges or adjustments for a single patient on an invoice.");
					gridAccount.SetAll(false);
					return;
				}
				if(payPlanCharge.ChargeType!=PayPlanChargeType.Debit) {
					MsgBox.Show(this,"You can only select payment plan Charges.");
					gridAccount.SetAll(false);
					return;
				}
				if(payPlanCharge.StatementNum!=0) {
					MsgBox.Show(this,"Selected payment plan charges(s) are already attached to an invoice.");
					gridAccount.SetAll(false);
					return;
				}
			}
			else{//the selected item must be an adjustment
				var adjustment=Adjustments.GetOne(SIn.Long(dataRow["AdjNum"].ToString()));
				if(adjustment.AdjDate.Date > DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed)) {
					MsgBox.Show(this,"Adjustments cannot be made for future dates");
					return;
				}
				if(adjustment.PatNum!=_patient.PatNum) {
					MsgBox.Show(this,"You can only select procedures, payment plan charges or adjustments for a single patient on an invoice.");
					gridAccount.SetAll(false);
					return;
				}
				if(adjustment.StatementNum!=0) {
					MsgBox.Show(this,"Selected adjustment(s) are already attached to an invoice.");
					gridAccount.SetAll(false);
					return;
				}
			}
		}
		//At this point, all selected items are procedures or adjustments, and are not already attached, and are for a single patient.
		var statement=new Statement();
		statement.PatNum=_patient.PatNum;
		statement.DateSent=DateTime.Today;
		statement.IsSent=false;
		statement.Mode_=StatementMode.InPerson;
		statement.HidePayment=true;
		statement.SinglePatient=true;
		statement.Intermingled=false;
		statement.IsReceipt=false;
		statement.IsInvoice=true;
		statement.StatementType=StmtType.NotSet;
		statement.DateRangeFrom=DateTime.MinValue;
		statement.DateRangeTo=DateTime.Today;
		statement.Note=PrefC.GetString(PrefName.BillingDefaultsInvoiceNote);
		statement.NoteBold="";
		statement.IsBalValid=true;
		statement.BalTotal=patientGuar.BalTotal;//Value will be overwritten by operations in FormStatementOptions
		statement.InsEst=patientGuar.InsEst;//Value will be overwritten by operations in FormStatementOptions
		if(listDataRowsSuperFam.Count > 0) {
			statement.SuperFamily=_patient.SuperFamily;
		}
		Statements.Insert(statement);
		statement.IsNew=true;
		var listProceduresForPat=Procedures.Refresh(_patient.PatNum);
		for(var i=0;i<gridAccount.SelectedIndices.Length;i++) {
			var dataRow=table.Rows[gridAccount.SelectedIndices[i]];
			if(dataRow["ProcNum"].ToString()!="0") {//if selected item is a procedure
				var procedure=Procedures.GetProcFromList(listProceduresForPat,SIn.Long(dataRow["ProcNum"].ToString()));
				var procedureOld=procedure.Copy();
				procedure.StatementNum=statement.StatementNum;
				if(procedure.ProcStatus==ProcStat.C && procedure.ProcDate.Date > DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed)) {
					MsgBox.Show(this,"Completed procedures cannot be set for future dates.");
					return;
				}
				Procedures.Update(procedure,procedureOld);
			}
			else if(dataRow["PayPlanChargeNum"].ToString()!="0") {
				var payPlanCharge=PayPlanCharges.GetOne(SIn.Long(dataRow["PayPlanChargeNum"].ToString()));
				payPlanCharge.StatementNum=statement.StatementNum;
				PayPlanCharges.Update(payPlanCharge);
			}
			else {//selected item must be adjustment
				var adjustment=Adjustments.GetOne(SIn.Long(dataRow["AdjNum"].ToString()));
				adjustment.StatementNum=statement.StatementNum;
				Adjustments.Update(adjustment);
			}
		}
		for(var i=0;i<listDataRowsSuperFam.Count;i++) {
			if(listDataRowsSuperFam[i]["ChargeType"].ToString()!="") {//payplan
				var payPlanCharge = PayPlanCharges.GetOne(SIn.Long(listDataRowsSuperFam[i]["PriKey"].ToString()));
				payPlanCharge.StatementNum=statement.StatementNum;
				PayPlanCharges.Update(payPlanCharge);
			}
			else if(listDataRowsSuperFam[i]["AdjType"].ToString()!="") {//adjustment
				var adjustment=Adjustments.GetOne(SIn.Long(listDataRowsSuperFam[i]["PriKey"].ToString()));
				adjustment.StatementNum=statement.StatementNum;
				Adjustments.Update(adjustment);
			}
			else {
				var procedureNew = Procedures.GetOneProc(SIn.Long(listDataRowsSuperFam[i]["PriKey"].ToString()),false);
				var procedureOld=procedureNew.Copy();
				procedureNew.StatementNum=statement.StatementNum;
				if(procedureNew.ProcStatus==ProcStat.C && procedureNew.ProcDate.Date>DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed)) {
					MsgBox.Show(this,"Procedures cannot be set for future dates.");
					return;
				}
				Procedures.Update(procedureNew,procedureOld);
			}
		}
		//All printing and emailing will be done from within the form:
		using var formStatementOptions=new FormStatementOptions();
		formStatementOptions.StatementCur=statement;
		formStatementOptions.ShowDialog();
		if(formStatementOptions.DialogResult!=DialogResult.OK) {
			Statements.DeleteStatements([statement]);//detached from adjustments, procedurelogs, and paysplits as well
		}
		Signalods.SetInvalid(InvalidType.BillingList);
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemInsPayPlan_Click(object sender,EventArgs e) {
		PayPlanHelper(PayPlanModes.Insurance);
	}

	private void menuItemLimited_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		DataRow dataRow;
		#region Autoselect Today's Procedures
		if(gridAccount.SelectedIndices.Length==0) {//autoselect procedures
			for(var i=0;i<table.Rows.Count;i++) {//loop through every line showing on screen
				dataRow=table.Rows[i];
				if(dataRow["ProcNum"].ToString()=="0" //ignore items that aren't procs
				   || SIn.Date(dataRow["date"].ToString())!=DateTime.Today //autoselecting todays procs only
				   || SIn.Long(dataRow["PatNum"].ToString())!=_patient.PatNum) //only procs for the current patient
				{
					continue;
				}
				if(i < gridAccount.ListGridRows.Count) {
					gridAccount.SetSelected(i,true);
				}
			}
			if(gridAccount.SelectedIndices.Length==0) {//if still none selected
				MsgBox.Show(this,"Please select procedures, adjustments, payments, or claims first.");
				return;
			}
		}
		#endregion Autoselect Today's Procedures
		var payPlanVersions=(PayPlanVersions)PrefC.GetInt(PrefName.PayPlansVersion);
		//guaranteed to have rows selected from here down, verify they are allowed transactions
		var areStatementsSelected=gridAccount.SelectedIndices.Any(x => table.Rows[x]["StatementNum"].ToString()!="0");
		var arePayPlanCreditsSelected=false;//Default for PayPlanVersions.NoCharges.
		if(payPlanVersions==PayPlanVersions.AgeCreditsAndDebits || payPlanVersions==PayPlanVersions.AgeCreditsOnly) {
			arePayPlanCreditsSelected=gridAccount.SelectedIndices.Any(x => table.Rows[x]["PayPlanChargeNum"].ToString()!="0" && table.Rows[x]["charges"].ToString()=="");
		}
		else if(payPlanVersions==PayPlanVersions.DoNotAge) {
			arePayPlanCreditsSelected=gridAccount.SelectedIndices.Any(x => table.Rows[x]["PayPlanNum"].ToString()!="0" && table.Rows[x]["charges"].ToString()=="");
		}
		if(areStatementsSelected || arePayPlanCreditsSelected) {
			MsgBox.Show(this,"You can only select procedures, adjustments, payments, or claims.");
			gridAccount.SetAll(false);
			return;
		}
		//At this point, all selected items are procedures, adjustments, payments, or claims.
		//get all ClaimNums from claimprocs for the selected procs
		var listProcClaimNums=ClaimProcs.GetForProcs(gridAccount.SelectedIndices.Where(x => table.Rows[x]["ProcNum"].ToString()!="0")
			.Select(x => SIn.Long(table.Rows[x]["ProcNum"].ToString())).ToList()).FindAll(x => x.ClaimNum!=0).ConvertAll(x => x.ClaimNum);
		//get all ClaimNums for any selected claimpayments
		var listPayClaimNums=gridAccount.SelectedIndices
			.Where(x => table.Rows[x]["ClaimNum"].ToString()!="0" && table.Rows[x]["ClaimPaymentNum"].ToString()=="1")
			.Select(x => SIn.Long(table.Rows[x]["ClaimNum"].ToString())).ToList();
		//prevent user from selecting a claimpayment that is not associated with any of the selected procs
		if(listPayClaimNums.Any(x => !listProcClaimNums.Contains(x))) {
			MsgBox.Show(this,"You can only select claim payments for the selected procedures.");
			gridAccount.SetAll(false);
			return;
		}
		var listPatNums=gridAccount.SelectedIndices
			.Select(x => table.Rows[x]["PatNum"].ToString()).Distinct().Select(x => SIn.Long(x)).ToList();
		var listAdjNums=gridAccount.SelectedIndices
			.Where(x => table.Rows[x]["AdjNum"].ToString()!="0")
			.Select(x => SIn.Long(table.Rows[x]["AdjNum"].ToString())).ToList();
		var listPayNums=gridAccount.SelectedIndices
			.Where(x => table.Rows[x]["PayNum"].ToString()!="0")
			.Select(x => SIn.Long(table.Rows[x]["PayNum"].ToString())).ToList();
		var listProcNums=gridAccount.SelectedIndices
			.Where(x => table.Rows[x]["ProcNum"].ToString()!="0")
			.Select(x => SIn.Long(table.Rows[x]["ProcNum"].ToString())).ToList();
		var patNumStatement=_patient.Guarantor;
		if(listPatNums.Count==1) {//If only one patient is selected
			patNumStatement=_patient.PatNum; //Use the patient's info on statement instead of the guarantor's.
		}
		var statement=Statements.CreateLimitedStatement(listPatNums,patNumStatement,listPayClaimNums,listAdjNums,listPayNums,listProcNums);
		//All printing and emailing will be done from within the form:
		using var formStatementOptions=new FormStatementOptions();
		formStatementOptions.StatementCur=statement;
		formStatementOptions.ShowDialog();
		if(formStatementOptions.DialogResult!=DialogResult.OK) {
			Statements.DeleteStatements([statement]);//detached from adjustments, procedurelogs, and paysplits as well
		}
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemLimitedCustom_Click(object sender,EventArgs e) {
		DataRow dataRow;
		var table=_dataSetMain.Tables["account"];
		#region Autoselect Items
		#region Autoselect Today's Procedures
		if(gridAccount.SelectedIndices.Length==0) {//autoselect procedures
			for(var i=0;i<table.Rows.Count;i++) {//loop through every line showing on screen
				dataRow=table.Rows[i];
				if(dataRow["ProcNum"].ToString()=="0" //ignore items that aren't procs
				   || SIn.Date(dataRow["date"].ToString())!=DateTime.Today //autoselecting todays procs only
				   || SIn.Long(dataRow["PatNum"].ToString())!=_patient.PatNum) //only procs for the current patient
				{
					continue;
				}
				if(i < gridAccount.ListGridRows.Count) {
					gridAccount.SetSelected(i,true);
				}
			}
		}
		#endregion Autoselect Today's Procedures
		var listPatNums=new List<long>();
		var listProcClaimNums=new List<long>();
		var listPayClaimNums=new List<long>();
		var listProcNums=new List<long>();
		var listAdjNums=new List<long>();
		var listPayNums=new List<long>();
		if(gridAccount.SelectedIndices.Length>0) {
			var payPlanVersions=(PayPlanVersions)PrefC.GetInt(PrefName.PayPlansVersion);
			//guaranteed to have rows selected from here down, verify they are allowed transactions
			var areStatementsSelected=gridAccount.SelectedIndices.Any(x => table.Rows[x]["StatementNum"].ToString()!="0");
			var arePayPlanCreditsSelected=false;//Default for PayPlanVersions.NoCharges.
			if(payPlanVersions==PayPlanVersions.AgeCreditsAndDebits || payPlanVersions==PayPlanVersions.AgeCreditsOnly) {
				arePayPlanCreditsSelected=gridAccount.SelectedIndices.Any(x => table.Rows[x]["PayPlanChargeNum"].ToString()!="0" && table.Rows[x]["charges"].ToString()=="");
			}
			else if(payPlanVersions==PayPlanVersions.DoNotAge) {
				arePayPlanCreditsSelected=gridAccount.SelectedIndices.Any(x => table.Rows[x]["PayPlanNum"].ToString()!="0" && table.Rows[x]["charges"].ToString()=="");
			}
			if(areStatementsSelected || arePayPlanCreditsSelected) {
				MsgBox.Show(this,"You can only select procedures, adjustments, payments, or claims.");
				gridAccount.SetAll(false);
				return;
			}
			//get all ClaimNums from claimprocs for the selected procs
			listProcClaimNums=ClaimProcs.GetForProcs(gridAccount.SelectedIndices.Where(x => table.Rows[x]["ProcNum"].ToString()!="0")
				.Select(x => SIn.Long(table.Rows[x]["ProcNum"].ToString())).ToList()).FindAll(x => x.ClaimNum!=0).ConvertAll(x => x.ClaimNum);
			//get all ClaimNums for any selected claimpayments
			listPayClaimNums=gridAccount.SelectedIndices
				.Where(x => table.Rows[x]["ClaimNum"].ToString()!="0" && table.Rows[x]["ClaimPaymentNum"].ToString()=="1")
				.Select(x => SIn.Long(table.Rows[x]["ClaimNum"].ToString())).ToList();
			//prevent user from selecting a claimpayment that is not associatede with any of the selected procs
			if(listPayClaimNums.Any(x => !listProcClaimNums.Contains(x))) {
				MsgBox.Show(this,"You can only select claim payments for the selected procedures.");
				gridAccount.SetAll(false);
				return;
			}
			listPatNums=gridAccount.SelectedIndices
				.Select(x => table.Rows[x]["PatNum"].ToString()).Distinct().Select(x => SIn.Long(x)).ToList();
			listAdjNums=gridAccount.SelectedIndices
				.Where(x => table.Rows[x]["AdjNum"].ToString()!="0")
				.Select(x => SIn.Long(table.Rows[x]["AdjNum"].ToString())).ToList();
			listPayNums=gridAccount.SelectedIndices
				.Where(x => table.Rows[x]["PayNum"].ToString()!="0")
				.Select(x => SIn.Long(table.Rows[x]["PayNum"].ToString())).ToList();
			listProcNums=gridAccount.SelectedIndices
				.Where(x => table.Rows[x]["ProcNum"].ToString()!="0")
				.Select(x => SIn.Long(table.Rows[x]["ProcNum"].ToString())).ToList();
		}
		#endregion
		var isFamMember=_family.ListPats.Length>1;
		var isSuperFamMember=_patient.SuperFamily > 0 && Patients.GetPat(_patient.Guarantor).HasSuperBilling;
		var listPatientsSuperFamily = new List<Patient>();
		var listPatients=new List<Patient>(_family.ListPats);
		//GetSuperFamAccount takes in a statement which will be used to build the account table, so we must build a temporary one to pass in.
		//Will not be inserted, only used to determine behavior of GetSuperFamAccount.
		var statement=new Statement();
		//The point of this fake statement is to get all account information to populate the Limited Statement Select window.
		//Purposefully set the StatementType to NotSet so that GetAccount() acts like we are loading the Account module.
		//Setting it to LimitedStatement would be detrimental because GetAccount() would require lists of account entries to limit the DataSet it returns.
		statement.StatementType=StmtType.NotSet;
		statement.IsNew=true;
		statement.PatNum=_patient.PatNum;
		statement.DateRangeFrom=DateTime.MinValue;
		statement.DateRangeTo=DateTime.Today;
		statement.LimitedCustomFamily=EnumLimitedCustomFamily.Patient;
		if(isSuperFamMember) {
			statement.LimitedCustomFamily=EnumLimitedCustomFamily.SuperFamily;
			statement.SuperFamily=_patient.SuperFamily;
			listPatientsSuperFamily=Patients.GetBySuperFamily(statement.SuperFamily);
			//Only add the families of the super family where the guarantor allows super billing.
			listPatientsSuperFamily=listPatientsSuperFamily.GroupBy(x => x.Guarantor)
				.ToDictionary(x => x.First(y => y.PatNum==x.Key),x => x.ToList())
				.Where(x => x.Key.HasSuperBilling)
				.SelectMany(x => x.Value).ToList();
			listPatients.AddRange(listPatientsSuperFamily);
		}
		else if(isFamMember) {
			statement.LimitedCustomFamily=EnumLimitedCustomFamily.Family;
		}
		listPatients=listPatients.DistinctBy(x=>x.PatNum).ToList();
		var dataSetSuperFam=AccountModules.GetSuperFamAccount(statement, isComputeAging:false, doIncludePatLName:true, listPatients:listPatients);
		table=dataSetSuperFam.Tables["account"];
		using var formLimitedStatementSelect=new FormLimitedStatementSelect();
		formLimitedStatementSelect.TableAccount=table.Copy();
		formLimitedStatementSelect.ListPayClaimNums=listPayClaimNums?? [];
		formLimitedStatementSelect.ListAdjNums=listAdjNums?? [];
		formLimitedStatementSelect.ListPayNums=listPayNums?? [];
		formLimitedStatementSelect.ListProcNums=listProcNums?? [];
		formLimitedStatementSelect.ListPatNums=listPatNums?? [];
		formLimitedStatementSelect.PatCur=_patient;
		if(isSuperFamMember) {
			formLimitedStatementSelect.ListPatNumsSuperFamily=listPatientsSuperFamily.ConvertAll(x=>x.PatNum);
		}
		if(isFamMember) {
			formLimitedStatementSelect.ListPatNumsFamily=_family.GetPatNums();
		}
		if(formLimitedStatementSelect.ShowDialog()!=DialogResult.OK) {
			return;
		}
		listPatNums=formLimitedStatementSelect.ListPatNums;
		listPayClaimNums=formLimitedStatementSelect.ListPayClaimNums;
		listProcNums=formLimitedStatementSelect.ListProcNums;
		listAdjNums=formLimitedStatementSelect.ListAdjNums;
		listPayNums=formLimitedStatementSelect.ListPayNums;
		//At this point, all selected items are procedures, adjustments, payments, or claims.
		Statement statementLimited;
		//Determine if we have selected super fam members.
		var isSuperFamLimitedStatement=false;
		if(isSuperFamMember) {
			//If any patnums are selected that are not in the family, it must be a super family.
			if(listPatNums.Any(x => !x.In(_family.GetPatNums().ToArray())))	{
				isSuperFamLimitedStatement=true;
			}
		}
		var limitedCustomFamily=EnumLimitedCustomFamily.Family;
		//Figure out which patient deserves to be the PatNum associated to this statement. Start with assumption we will be using the guarantor.
		var patNumStatement=_patient.Guarantor;
		long superFamNum=0;
		if(isSuperFamLimitedStatement) {
			//Only set SuperFamNum if we selected superfam entries.
			limitedCustomFamily=EnumLimitedCustomFamily.SuperFamily;
			patNumStatement=_patient.SuperFamily;
			superFamNum=_patient.SuperFamily;
		}
		else if(listPatNums.Count==1) {
			if(listPatNums[0]==_patient.Guarantor) {
				//This is NOT a super family statement. Therefore, if the patient is the guarantor this is a patient statement.
				limitedCustomFamily=EnumLimitedCustomFamily.Patient;
			}
			else if(listPatNums[0]==_patient.PatNum) {
				//Use patient name on statement.
				patNumStatement=_patient.PatNum;
			}
		}
		statementLimited=Statements.CreateLimitedStatement(listPatNums,patNumStatement,listPayClaimNums,listAdjNums,listPayNums,listProcNums,superFamily:superFamNum,limitedCustomFamily:limitedCustomFamily);
		//All printing and emailing will be done from within the form:
		using var formStatementOptions=new FormStatementOptions();
		formStatementOptions.StatementCur=statementLimited;
		formStatementOptions.ShowDialog();
		if(formStatementOptions.DialogResult!=DialogResult.OK) {
			Statements.DeleteStatements([statementLimited]);//detached from adjustments, procedurelogs, and paysplits as well
		}
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemQuickProcs_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.AccountProcsQuickAdd)) {
			return;
		}
		//One of the QuickCharge menu items was clicked.
		if(sender.GetType()!=typeof(MenuItem)) {
			return;
		}
		//When SalesTaxProcCode pref string is not empty, this dropdown will have another menu item inserted at the 0th position.
		//This menu item will not have an associated entry in _arrayDefsAcctProcQuickAdd, so we have to offset the index before trying to access the array.
		var indexClicked=contextMenuQuickProcs.MenuItems.IndexOf((MenuItem)sender);
		if(!PrefC.GetString(PrefName.SalesTaxProcCode).IsNullOrEmpty()) {
			indexClicked-=1;
		}
		var defQuickCharge=_arrayDefsAcctProcQuickAdd[indexClicked];
		var stringArrayProcCodes=defQuickCharge.ItemValue.Split(',');
		if(stringArrayProcCodes.Length==0) {
			//No items entered into the definition category.  Notify the user.
			MsgBox.Show(this,"There are no Quick Charge items in Setup | Definitions.  There must be at least one in order to use the Quick Charge drop down menu.");
		}
		var listHiddenProcCodes=ProcedureCodes.GetProcCodesInHiddenCats(stringArrayProcCodes.Select(x => ProcedureCodes.GetCodeNum(x)).ToArray());
		if(listHiddenProcCodes.Count > 0) {
			ODMessageBox.Show(this,$"{Lan.g(this,"Cannot add the following procedures because they are in a hidden category")}: {string.Join(",",listHiddenProcCodes)}");
			return;
		}
		var listProcCodesAdded=new List<string>();
		var provider=Providers.GetById(_patient.PriProv);
		for(var i=0;i<stringArrayProcCodes.Length;i++) {
			if(AddProcAndValidate(stringArrayProcCodes[i],provider)) {
				listProcCodesAdded.Add(stringArrayProcCodes[i]);
			}
		}
		if(listProcCodesAdded.Count>0) {
			SecurityLogs.MakeLogEntry(EnumPermType.AccountProcsQuickAdd,_patient.PatNum
				,Lan.g(this,"The following procedures were added via the Quick Charge button from the Account module")
				 +": "+string.Join(",",listProcCodesAdded));
			ModuleSelected(_patient.PatNum);
		}
	}

	private void menuItemSalesTaxProc_Click(object sender,EventArgs e) {
		//This is considered separate from Quick Procs, and we want the permissions to show that.
		//We already changed UI to reflect this.
		if(!Security.IsAuthorized(EnumPermType.ProcComplCreate)) {
			return;
		}
		var table=_dataSetMain.Tables["account"];
		var listIndices=gridAccount.SelectedIndices.ToList();
		var listProcNumsSelected=new List<long>();
		for(var i=0;i<listIndices.Count;i++) {
			var procNumStr=table.Rows[listIndices[i]]["ProcNum"].ToString();
			var procNum=SIn.Long(procNumStr);
			if(procNum==0) {
				continue;
			}
			listProcNumsSelected.Add(procNum);
		}
		if(listProcNumsSelected.Count==0) {
			MsgBox.Show(this,"Please select procedures to tax first.");
			return;
		}
		var listProcs=Procedures.GetManyProc(listProcNumsSelected,includeNote:false);
		Procedures.CreateSalesTaxProc(_patient.PatNum,listProcs);
		SecurityLogs.MakeLogEntry(EnumPermType.ProcComplCreate,_patient.PatNum
			,Lan.g(this,"Sales tax procedure generated for ProcNums: ")+" "+string.Join(",",listProcNumsSelected));
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemReceipt_Click(object sender,EventArgs e) {
		var statement=new Statement();
		statement.PatNum=_patient.PatNum;
		statement.DateSent=DateTime.Today;
		statement.IsSent=true;
		statement.Mode_=StatementMode.InPerson;
		statement.HidePayment=true;
		statement.Intermingled=PrefC.GetBool(PrefName.IntermingleFamilyDefault);
		statement.SinglePatient=!statement.Intermingled;
		statement.IsReceipt=true;
		statement.StatementType=StmtType.NotSet;
		statement.DateRangeFrom=DateTime.Today;
		statement.DateRangeTo=DateTime.Today;
		statement.Note="";
		statement.NoteBold="";
		//BalTotal and InsEst are set using Statements.CalcBalTotalInsEst() in PrintStatement() before inserting in db
		PrintStatement(statement);
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemSalesTax_Click(object sender,EventArgs e) {
		if(gridAccount.SelectedIndices.Length==0) {
			MsgBox.Show(this,"Please select at least one procedure.");
			return;
		}
		var table=_dataSetMain.Tables["account"];
		var listProcNumsSelected=new List<long>();
		var listidxs=gridAccount.SelectedIndices.ToList();
		for(var i=0;i<listidxs.Count;i++) {
			if(table.Rows[listidxs[i]]["ProcNum"].ToString()=="0") {
				continue;
			}
			listProcNumsSelected.Add(SIn.Long(table.Rows[listidxs[i]]["ProcNum"].ToString()));
		}
		var listOrthoProcLinks=OrthoProcLinks.GetManyForProcs(listProcNumsSelected);
		if(listOrthoProcLinks.Count>0) {
			MsgBox.Show(this,"One or more of the selected procedures cannot be adjusted because it is attached to an ortho case." +
			                 " Please deselect these items and try again.");
			return;
		}
		var listProcedures=Procedures.GetManyProc(listProcNumsSelected,false);
		for(var i=0;i<listProcedures.Count;i++) {
			Adjustments.CreateAdjustmentForSalesTax(listProcedures[i],true);
		}
		Signalods.SetInvalid(InvalidType.BillingList);
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemStatementEmail_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.EmailSend)) {
			Cursor=Cursors.Default;
			return;
		}
		var statement=new Statement();
		statement.PatNum=_patient.Guarantor;
		statement.DateSent=DateTime.Today;
		statement.IsSent=true;
		statement.Mode_=StatementMode.Email;
		statement.HidePayment=false;
		statement.SinglePatient=false;
		statement.Intermingled=PrefC.GetBool(PrefName.IntermingleFamilyDefault);
		statement.IsReceipt=false;
		statement.StatementType=StmtType.NotSet;
		statement.DateRangeFrom=DateTime.MinValue;
		if(textDateStart.IsValid()) {
			if(textDateStart.Text!="") {
				statement.DateRangeFrom=SIn.Date(textDateStart.Text);
			}
		}
		statement.DateRangeTo=DateTime.Today;//Needed for payplan accuracy.  Used to be setting to new DateTime(2200,1,1);
		if(textDateEnd.IsValid()) {
			if(textDateEnd.Text!="") {
				statement.DateRangeTo=SIn.Date(textDateEnd.Text);
			}
		}
		statement.Note="";
		statement.NoteBold="";
		Patient patientGuar = null;
		if(_patient!=null) {
			patientGuar = Patients.GetPat(_patient.Guarantor);
		}
		if(patientGuar!=null) {
			statement.IsBalValid=true;
			statement.BalTotal=patientGuar.BalTotal;
			statement.InsEst=patientGuar.InsEst;
		}
		//It's pointless to give the user the window to select statement options, because they could just as easily have hit the More Options dropdown, then Email from there.
		PrintStatement(statement);
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemStatementMore_Click(object sender,EventArgs e) {
		var statement=new Statement();
		statement.PatNum=_patient.PatNum;
		statement.DateSent=DateTime.Today;
		statement.IsSent=false;
		statement.Mode_=StatementMode.InPerson;
		statement.HidePayment=false;
		statement.SinglePatient=false;
		statement.Intermingled=PrefC.GetBool(PrefName.IntermingleFamilyDefault);
		statement.IsReceipt=false;
		statement.StatementType=StmtType.NotSet;
		statement.DateRangeFrom=DateTime.MinValue;
		statement.DateRangeFrom=DateTime.MinValue;
		if(textDateStart.IsValid()) {
			if(textDateStart.Text!="") {
				statement.DateRangeFrom=SIn.Date(textDateStart.Text);
			}
		}
		statement.DateRangeTo=DateTime.Today;//Needed for payplan accuracy.//new DateTime(2200,1,1);
		if(textDateEnd.IsValid()) {
			if(textDateEnd.Text!="") {
				statement.DateRangeTo=SIn.Date(textDateEnd.Text);
			}
		}
		statement.Note="";
		statement.NoteBold="";
		Patient patientGuar=null;
		if(_patient!=null) {
			patientGuar=Patients.GetPat(_patient.Guarantor);
		}
		if(patientGuar!=null) {
			statement.IsBalValid=true;
			statement.BalTotal=patientGuar.BalTotal;
			statement.InsEst=patientGuar.InsEst;
		}
		//All printing and emailing will be done from within the form:
		using var formStatementOptions=new FormStatementOptions();
		statement.IsNew=true;
		formStatementOptions.StatementCur=statement;
		formStatementOptions.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemStatementWalkout_Click(object sender,EventArgs e) {
		var statement=new Statement();
		statement.PatNum=_patient.PatNum;
		statement.DateSent=DateTime.Today;
		statement.IsSent=true;
		statement.Mode_=StatementMode.InPerson;
		statement.HidePayment=true;
		statement.Intermingled=PrefC.GetBool(PrefName.IntermingleFamilyDefault);
		statement.SinglePatient=!statement.Intermingled;
		statement.IsReceipt=false;
		statement.StatementType=StmtType.NotSet;
		statement.DateRangeFrom=DateTime.Today;
		statement.DateRangeTo=DateTime.Today;
		statement.Note="";
		statement.NoteBold="";
		//BalTotal and InsEst are set using Statements.CalcBalTotalInsEst() in PrintStatement() before inserting in db
		PrintStatement(statement);
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemSendMessageToPay_Click(object sender,EventArgs e) {
		if(_patient==null) {
			MsgBox.Show("Please select a patient first.");
			return;
		}
		var formMessageToPayEdit=new FormMessageToPayEdit(_patient);
		formMessageToPayEdit.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void menuItemSnipAttachment_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		//Guaranteed to be exactly one claim selected (among possible other selections)
		var idxClaimSelected=gridAccount.SelectedIndices.ToList().Find(x => table.Rows[x]["ClaimNum"].ToString()!="0");
		var claimNum=SIn.Long(table.Rows[idxClaimSelected]["ClaimNum"].ToString());
		if(claimNum==0) {
			return;
		}
		var claim=Claims.GetClaim(claimNum);
		if(!ValidateRightClickDXC(claim)) {
			return;
		}
		var formClaimAttachSnipDXC=new FormClaimAttachSnipDXC();
		formClaimAttachSnipDXC.ClaimCur=claim;
		formClaimAttachSnipDXC.Patient=_patient;
		formClaimAttachSnipDXC.Show();
	}

	private void menuItemSelectImage_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		//Guaranteed to be exactly one claim selected (among possible other selections)
		var idxClaimSelected=gridAccount.SelectedIndices.ToList().Find(x => table.Rows[x]["ClaimNum"].ToString()!="0");
		var claimNum=SIn.Long(table.Rows[idxClaimSelected]["ClaimNum"].ToString());
		if(claimNum==0) {
			return;
		}
		var claim=Claims.GetClaim(claimNum);
		if(!ValidateRightClickDXC(claim)) {
			return;
		}
		using var formImagePickerDXC=new FormImagePickerDXC();
		formImagePickerDXC.PatientCur=_patient;
		formImagePickerDXC.ClaimCur=claim;
		formImagePickerDXC.ShowDialog();
	}

	private void menuItemPasteAttachment_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		//Guaranteed to be exactly one claim selected (among possible other selections)
		var idxClaimSelected=gridAccount.SelectedIndices.ToList().Find(x => table.Rows[x]["ClaimNum"].ToString()!="0");
		var claimNum=SIn.Long(table.Rows[idxClaimSelected]["ClaimNum"].ToString());
		if(claimNum==0) {
			return;
		}
		var claim=Claims.GetClaim(claimNum);
		if(!ValidateRightClickDXC(claim)) {
			return;
		}
		var formClaimAttachPasteDXC=new FormClaimAttachPasteDXC();
		formClaimAttachPasteDXC.ClaimCur=claim;
		formClaimAttachPasteDXC.PatientCur=_patient;
		formClaimAttachPasteDXC.Show();
	}

	private void menuItemAttachmentHistory_Click(object sender,EventArgs e) {
		var table=_dataSetMain.Tables["account"];
		//Guaranteed to be exactly one claim selected (among possible other selections)
		var idxClaimSelected=gridAccount.SelectedIndices.ToList().Find(x => table.Rows[x]["ClaimNum"].ToString()!="0");
		var claimNum=SIn.Long(table.Rows[idxClaimSelected]["ClaimNum"].ToString());
		if(claimNum==0) {
			return;
		}
		var claim=Claims.GetClaim(claimNum);
		if(!ValidateRightClickDXC(claim)) {//Validation now needed for sending Narratives
			return;
		}
		using var formClaimAttachHistory=new FormClaimAttachHistory();
		formClaimAttachHistory.ClaimCur=claim;
		formClaimAttachHistory.PatientCur=_patient;
		formClaimAttachHistory.ShowDialog();
	}
	#endregion Methods - Event Handlers MenuItem

	#region Methods - Event Handlers Parent
	private void Parent_MouseWheel(object sender,MouseEventArgs e) {
		if(Visible) {
			this.OnMouseWheel(e);
		}
	}
	#endregion Methods - Event Handlers Parent

	#region Methods - Event Handlers Text Fields
	private void textFinNote_Leave(object sender,EventArgs e) {
		UpdateFinNote();
	}

	private void textFinNote_TextChanged(object sender,EventArgs e) {
		IsFinNoteChanged=true;
	}

	private void textQuickCharge_CaptureChange(object sender,EventArgs e) {
		if(textQuickProcs.Visible) {
			textQuickProcs.Capture=true;
		}
	}

	private void textQuickCharge_FocusLost(object sender,EventArgs e) {
		textQuickProcs.Text="";
		textQuickProcs.Visible=false;
		textQuickProcs.Capture=false;
	}

	private void textQuickCharge_KeyDown(object sender,KeyEventArgs e) {
		//This is only the KeyDown event, user can still type if we return here.
		if(e.KeyCode!=Keys.Enter) {
			return;
		}
		textQuickProcs.Visible=false;
		textQuickProcs.Capture=false;
		e.Handled=true;//Suppress the "ding" in windows when pressing enter.
		e.SuppressKeyPress=true;//Suppress the "ding" in windows when pressing enter.
		if(textQuickProcs.Text=="") {
			return;
		}
		var quickProcText=textQuickProcs.Text;//because the text seems to disappear from textbox in menu bar when MsgBox comes up.
		var provider=Providers.GetById(_patient.PriProv);
		if(AddProcAndValidate(quickProcText,provider)) {
			SecurityLogs.MakeLogEntry(EnumPermType.AccountProcsQuickAdd,_patient.PatNum
				,Lan.g(this,"The following procedures were added via the Quick Charge button from the Account module")
				 +": "+string.Join(",",quickProcText));
			ModuleSelected(_patient.PatNum);
		}
		textQuickProcs.Text="";
	}

	private void textQuickCharge_MouseClick(object sender,MouseEventArgs e) {
		if(e.X<0 || e.X>textQuickProcs.Width ||e.Y<0 || e.Y>textQuickProcs.Height) {
			textQuickProcs.Text="";
			textQuickProcs.Visible=false;
			textQuickProcs.Capture=false;
		}
	}

	private void textUrgFinNote_Leave(object sender,EventArgs e) {
		//need to skip this if selecting another module. Handled in ModuleUnselected due to click event
		UpdateUrgFinNote();
	}

	private void textUrgFinNote_TextChanged(object sender,EventArgs e) {
		IsUrgFinNoteChanged=true;
	}
	#endregion Methods - Event Handlers Text Fields

	#region Methods - Event Handlers ToolBarMain
	private void ToolBarMain_ButtonClick(object sender,ODToolBarButtonClickEventArgs e) {
		if(e.Button.Tag.GetType()==typeof(string)) {
			if(Patients.GetPat(_patient.PatNum).PatStatus==PatientStatus.Deleted) {
				MsgBox.Show(this, "Selected patient has been deleted by another workstation.");
				return;
			}
			//standard predefined button
			switch(e.Button.Tag.ToString()) {
				//case "Patient":
				//	OnPat_Click();
				//	break;
				case "Payment":
					var isTsiPayment=TsiTransLogs.IsTransworldEnabled(_patient.ClinicNum)
					                 && Patients.IsGuarCollections(_patient.Guarantor,includeSuspended:false)
					                 && !MsgBox.Show(this,MsgBoxButtons.YesNo,"The guarantor of this family has been sent to TSI for a past due balance.  "
					                                                          +"Is the payment you are applying directly from the debtor or guarantor?\r\n\r\n"
					                                                          +"Yes - this payment is directly from the debtor/guarantor\r\n\r\n"
					                                                          +"No - this payment is from TSI");
					var listInputBoxParams=new List<InputBoxParam>();
					var inputBoxParam=new InputBoxParam();
					inputBoxParam.InputBoxType_=InputBoxType.ValidDouble;
					inputBoxParam.LabelText=Lan.g(this,"Please enter an amount: ");
					listInputBoxParams.Add(inputBoxParam);
					if(_family.ListPats.Length>1) {
						inputBoxParam=new InputBoxParam();
						inputBoxParam.SizeParam=new System.Windows.Size(120,20);
						inputBoxParam.InputBoxType_=InputBoxType.CheckBox;
						inputBoxParam.Text=Lan.g(this," - Prefer this patient");
						listInputBoxParams.Add(inputBoxParam);
					}
					var funcOkClick=new Func<string, bool>((text) => {
						if(text=="") {
							MsgBox.Show(this,"Please enter a value.");
							return false;//Should stop user from continuing to payment window.
						}
						return true;//Allow user to the payment window.
					});
					var inputBox=new InputBox(listInputBoxParams);
					inputBox.FuncOkClick=funcOkClick;
					inputBox.ShowDialog();
					if(inputBox.IsDialogCancel) {
						break;
					}
					var preferCurrentPat=false;
					if(inputBox.BoolResult) {
						preferCurrentPat=true;
					}
					toolBarButPay_Click(SIn.Double(inputBox.StringResult),preferCurrentPat:preferCurrentPat,isTsiPayment:isTsiPayment);
					break;
				case "Adjustment":
					toolBarButAdj_Click();
					break;
				case "Insurance":
					var createClaimDataWrapper=ClaimL.GetCreateClaimDataWrapper(_patient,_family,GetCreateClaimItemsFromUI(),true);
					if(createClaimDataWrapper.HasError) {
						break;
					}
					createClaimDataWrapper=ClaimL.CreateClaimFromWrapper(true,createClaimDataWrapper);
					if(!createClaimDataWrapper.HasError || createClaimDataWrapper.ShouldRefresh) {
						ModuleSelected(_patient.PatNum);
					}
					break;
				case "PayPlan":
					contextMenuPayPlan.Show(ToolBarMain,new Point(e.Button.Bounds.Location.X,e.Button.Bounds.Height));
					break;
				case "InstallPlan":
					toolBarButInstallPlan_Click();
					break;
				case "RepeatCharge":
					toolBarButRepeatCharge_Click();
					break;
				case "Statement":
					//The reason we are using a delegate and BeginInvoke() is because of a Microsoft bug that causes the Print Dialog window to not be in focus			
					//when it comes from a toolbar click.
					//https://social.msdn.microsoft.com/Forums/windows/en-US/681a50b4-4ae3-407a-a747-87fb3eb427fd/first-mouse-click-after-showdialog-hits-the-parent-form?forum=winforms
					ToolBarClick toolBarClick=toolBarButStatement_Click;
					this.BeginInvoke(toolBarClick);
					break;
				case "QuickProcs":
					toolBarButQuickProcs_Click();
					break;
			}
		}
		else if(e.Button.Tag.GetType()==typeof(Program)) {
			WpfControls.ProgramL.Execute(((Program)e.Button.Tag).ProgramNum,_patient);
		}
	}
	#endregion Methods - Event Handlers ToolBarMain

	#region Methods - Public
		
	public void InitializeOnStartup() {
		if(_isInitializedOnStartup) {
			return;
		}
		_isInitializedOnStartup=true;
		LayoutToolBar();
		textQuickProcs.AcceptsTab=true;
		textQuickProcs.KeyDown+=textQuickCharge_KeyDown;
		textQuickProcs.MouseDown+=textQuickCharge_MouseClick;
		textQuickProcs.MouseCaptureChanged+=textQuickCharge_CaptureChange;
		textQuickProcs.LostFocus+=textQuickCharge_FocusLost;
		//This just makes the patient information grid show up or not.
		_listDisplayFieldsPatInfo=DisplayFields.GetForCategory(DisplayFieldCategory.AccountPatientInformation);
		LayoutPanels();//Only place that we call this outside of LayoutPanelsAndRefreshMainGrids() since no grid data has been loaded yet
		splitContainerAccountCommLog.SplitterDistance=splitContainerParent.Panel2.Height * 3/5;//Make Account grid slightly bigger than commlog
		checkShowFamilyComm.Checked=PrefC.GetBoolSilent(PrefName.ShowAccountFamilyCommEntries,true);
		checkShowCompletePayPlans.Checked=PrefC.GetBool(PrefName.AccountShowCompletedPaymentPlans);
	}

	///<summary>Causes the toolbar to be laid out again.</summary>
	public void LayoutToolBar() {
		ToolBarMain.Buttons.Clear();
		ODToolBarButton toolBarButton;
		_butPayment=new ODToolBarButton(Lan.g(this,"Payment"),1,"","Payment");
		_butPayment.Style=ODToolBarButtonStyle.DropDownButton;
		_butPayment.DropDownMenu=contextMenuPayment;
		ToolBarMain.Buttons.Add(_butPayment);
		toolBarButton=new ODToolBarButton(Lan.g(this,"Adjustment"),2,"","Adjustment");
		toolBarButton.Style=ODToolBarButtonStyle.DropDownButton;
		toolBarButton.DropDownMenu=contextMenuAdjust;
		ToolBarMain.Buttons.Add(toolBarButton);
		toolBarButton=new ODToolBarButton(Lan.g(this,"New Claim"),3,"","Insurance");
		toolBarButton.Style=ODToolBarButtonStyle.DropDownButton;
		toolBarButton.DropDownMenu=contextMenuIns;
		ToolBarMain.Buttons.Add(toolBarButton);
		ToolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
		toolBarButton=new ODToolBarButton(Lan.g(this,"Payment Plan"),-1,"","PayPlan");
		toolBarButton.Style=ODToolBarButtonStyle.DropDownButton;
		toolBarButton.DropDownMenu=contextMenuPayPlan;
		ToolBarMain.Buttons.Add(toolBarButton);
		ToolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this,"Installment Plan"),-1,"","InstallPlan"));
		ToolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
		_butQuickProcs=new ODToolBarButton(Lan.g(this,"Add Proc"),-1,"","QuickProcs");
		_butQuickProcs.Style=ODToolBarButtonStyle.DropDownButton;
		_butQuickProcs.DropDownMenu=contextMenuQuickProcs;
		contextMenuQuickProcs.Popup+=contextMenuQuickProcs_Popup;
		ToolBarMain.Buttons.Add(_butQuickProcs);
		if(!PrefC.GetBool(PrefName.EasyHideRepeatCharges)) {
			toolBarButton=new ODToolBarButton(Lan.g(this,"Repeating Charge"),-1,"","RepeatCharge");
			toolBarButton.Style=ODToolBarButtonStyle.NormalButton;
			ToolBarMain.Buttons.Add(toolBarButton);
		}
		ToolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
		toolBarButton=new ODToolBarButton(Lan.g(this,"Statement"),4,"","Statement");
		toolBarButton.Style=ODToolBarButtonStyle.DropDownButton;
		toolBarButton.DropDownMenu=contextMenuStatement;
		ToolBarMain.Buttons.Add(toolBarButton);
		ProgramL.LoadToolBar(ToolBarMain,EnumToolBar.AccountModule);
		ToolBarMain.Invalidate();
		UpdateToolbarButtons();
	}

		
	public void ModuleSelected(long patNum) {
		ModuleSelected(patNum,false);
	}

		
	private void ModuleSelected(long patNum,bool isSelectingFamily) {
		if(_superFamPrefEnabled!=PrefC.GetBool(PrefName.ShowFeatureSuperfamilies)) {
			_superFamPrefEnabled=PrefC.GetBool(PrefName.ShowFeatureSuperfamilies);
			ToggleUseSuperFamCheckboxEnabled(_superFamPrefEnabled);
		}
		var userOdPrefProcBreakdown=UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum,UserOdFkeyType.AcctProcBreakdown).FirstOrDefault();
		var userOdPrefShowAutoCommlog=UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum,UserOdFkeyType.ShowAutomatedCommlog).FirstOrDefault();
		if(userOdPrefProcBreakdown==null) {
			checkShowDetail.Checked=true;
		}
		else {
			checkShowDetail.Checked=SIn.Bool(userOdPrefProcBreakdown.ValueString);
		}
		if(userOdPrefShowAutoCommlog==null) {
			checkShowCommAuto.Checked=true;
		}
		else {
			checkShowCommAuto.Checked=SIn.Bool(userOdPrefShowAutoCommlog.ValueString);
		}
		RefreshModuleData(patNum,isSelectingFamily);
		if(_patient!=null && _patient.PatStatus==PatientStatus.Deleted) {
			MsgBox.Show("Selected patient has been deleted by another workstation.");
			PatientL.RemoveFromMenu(_patient.PatNum);
			GlobalFormOpenDental.PatientSelected(new Patient(),false);
			RefreshModuleData(0,isSelectingFamily);
		}
		if(_patient!=null && _patient.PatStatus==PatientStatus.Archived && !Security.IsAuthorized(EnumPermType.ArchivedPatientSelect,suppressMessage:true)) {
			GlobalFormOpenDental.PatientSelected(new Patient(),false);
			RefreshModuleData(0,isSelectingFamily);
		}
		if(_patient!=null) {//Only when a patient is selected
			//This section could be improved to use objects once we switch from DataSet to lists.
			var dataTable=_loadData.DataSetMain.Tables["account"];
			var listDataRowsClaims=dataTable.Select().ToList().FindAll(x => x["ClaimNum"].ToString()!="0");
			//Get a list of procnums of procedures that do not have a status of complete. Since the logic that fills the account
			//table only selects completed procedures, any procnums not in the account table must have a different procstatus.
			var listProcNumsAll=dataTable.Select().Select(x => SIn.Long(x["ProcNum"].ToString())).ToList();
			var listProcNumsIncompleteClaims=string.Join(",",listDataRowsClaims.Select(x => x["procsOnObj"]))//All procNums on the all claims in account
				.Split(",",StringSplitOptions.RemoveEmptyEntries)
				.Distinct()
				.Select(x => SIn.Long(x))
				.ToList()
				//get procNums that are on claim but not in account
				.FindAll(y => !y.In(listProcNumsAll.ToArray()));
			//Warn the user if they have any incomplete procs attached to claims.
			if(!listProcNumsIncompleteClaims.IsNullOrEmpty()) {
				var stringBuilder=new StringBuilder(Lans.g("The following procedure(s) are incomplete and attached to claim(s). It is recommended that all procedures attached to claims be completed")+":\r\n");
				//These extra queries should not be executed very often, and they will only return a small dataset in most cases
				var listProcedures=Procedures.GetManyProc(listProcNumsIncompleteClaims,includeNote:false);
				for(var i=0;i<listProcedures.Count;i++) {
					stringBuilder.AppendLine(listProcedures[i].ProcDate.ToShortDateString()+"\t"+Procedures.GetDescription(listProcedures[i],forAccount:true));
				}
				var msgBoxCopyPaste=new MsgBoxCopyPaste(stringBuilder.ToString());
				msgBoxCopyPaste.Show();
			}
		}
		RefreshModuleScreen(isSelectingFamily);
		ODEvent.Fire(ODEventType.ModuleSelected,_loadData);
	}

	private void ToggleUseSuperFamCheckboxEnabled(bool prefEnabled) {
		if(prefEnabled) {
			checkUseSuperFam.Visible=true;
			checkUseSuperFam.Enabled=true;
		}
		else {
			checkUseSuperFam.Checked=false;
			_useSuperFam=false;
			checkUseSuperFam.Visible=false;
			checkUseSuperFam.Enabled=false;
		}
	}

	///<summary>Used when jumping to this module and directly to a claim.</summary>
	public void ModuleSelected(long patNum,long claimNum) {
		ModuleSelected(patNum);
		var table=_dataSetMain.Tables["account"];
		for(var i=0;i<table.Rows.Count;i++) {
			if(table.Rows[i]["ClaimPaymentNum"].ToString()!="0") {//claimpayment
				continue;
			}
			if(table.Rows[i]["ClaimNum"].ToString()=="0") {//not a claim or claimpayment
				continue;
			}
			var claimNumRow=SIn.Long(table.Rows[i]["ClaimNum"].ToString());
			if(claimNumRow!=claimNum) {
				continue;
			}
			if(i<gridAccount.ListGridRows.Count) {
				gridAccount.SetSelected(i,true);
			}
		}
	}

		
	public void ModuleUnselected() {
		UpdateUrgFinNote();
		UpdateFinNote();
		_family=null;
		_repeatChargeArray=null;
		_patNumLast=0;//Clear out the last pat num so that a security log gets entered that the module was "visited" or "refreshed".
	}

	/// <summary>Only for use in FormOpenDental.cs form deactivate event handler. Used to prevent a bug that would clear out the FamUrgFinNote when closing the program sometimes.
	/// There is a case where a user has a note on load, intentionally clears that note and OD is shutdown via shutdown signal or other process termination before they leave the textbox. This will result in the note that they deleted not being updated and still being present when OD is opened again as if the changes made were not committed.</summary>
	public bool canUpdateFinNote() {
		if(IsFinNoteChanged) {
			if(textFinNote.Text=="" && _famFinNoteOnLoad != textFinNote.Text) {
				return false;
			}
		}
		return true;
	}

	public void UpdateFinNote() {
		if(_family==null) {
			return;
		}

		if(IsFinNoteChanged) {
			_patientNote.FamFinancial=textFinNote.Text;
			PatientNotes.Update(_patientNote,_patient.Guarantor);
			IsFinNoteChanged=false;
		}
	}

	/// <summary>Only for use in FormOpenDental.cs form deactivate event handler. Used to prevent a bug that would clear out the FamUrgFinNote when closing the program sometimes.
	/// There is a case where a user has a note on load, intentionally clears that note and OD is shutdown via shutdown signal or other process termination before they leave the textbox. This will result in the note that they deleted not being updated and still being present when OD is opened again as if the changes made were not committed.</summary>
	public bool canUpdateUrgFinNote() {
		if(IsUrgFinNoteChanged) {
			if(textUrgFinNote.Text=="" && _famUrgFinNoteOnLoad != textUrgFinNote.Text) {
				return false;
			}
		}
		return true;
	}

	public void UpdateUrgFinNote() {
		if(_family==null) {
			return;
		}

		if(IsUrgFinNoteChanged) {
			var patientOld=_family.ListPats[0].Copy();
			_family.ListPats[0].FamFinUrgNote=textUrgFinNote.Text;
			Patients.Update(_family.ListPats[0],patientOld);
			IsUrgFinNoteChanged=false;
		}
	}
	#endregion Methods - Public

	#region Methods - Private ToolBar
	private void toolBarButAdj_Click() {
		AddAdjustmentToSelectedProcsHelper();
	}

	private void toolBarButInstallPlan_Click() {
		if(InstallmentPlans.GetOneForFam(_patient.Guarantor)!=null) {
			MsgBox.Show(this,"Family already has an installment plan.");
			return;
		}
		var installmentPlan=new InstallmentPlan();
		installmentPlan.PatNum=_patient.Guarantor;
		installmentPlan.DateAgreement=DateTime.Today;
		installmentPlan.DateFirstPayment=DateTime.Today;
		//InstallmentPlans.Insert(installPlan);
		using var formInstallmentPlanEdit=new FormInstallmentPlanEdit();
		formInstallmentPlanEdit.InstallmentPlanCur=installmentPlan;
		formInstallmentPlanEdit.IsNew=true;
		formInstallmentPlanEdit.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void toolBarButPay_Click(double payAmt,bool preferCurrentPat=false,bool isPrePay=false,bool isIncomeTransfer=false,bool isTsiPayment=false) {
		var payment=new Payment();
		payment.PayDate=DateTime.Today;
		payment.PatNum=_patient.PatNum;
		//Explicitly set ClinicNum=0, since a pat's ClinicNum will remain set if the user enabled clinics, assigned patients to clinics, and then
		//disabled clinics because we use the ClinicNum to determine which PayConnect or XCharge/XWeb credentials to use for payments.
		payment.ClinicNum=0;
		if(true) {//if clinics aren't enabled default to 0
			if((PayClinicSetting)PrefC.GetInt(PrefName.PaymentClinicSetting)==PayClinicSetting.PatientDefaultClinic) {
				payment.ClinicNum=_patient.ClinicNum;
			}
			else if((PayClinicSetting)PrefC.GetInt(PrefName.PaymentClinicSetting)==PayClinicSetting.SelectedExceptHQ) {
				payment.ClinicNum=Clinics.ClinicNum==0?_patient.ClinicNum:Clinics.ClinicNum;
			}
			else {
				payment.ClinicNum=Clinics.ClinicNum;
			}
		}
		payment.DateEntry=DateTime.Today;//So that it will show properly in the new window.
		var listDefs=Defs.GetDefsForCategory(DefCat.PaymentTypes,true);
		if(listDefs.Count>0) {
			payment.PayType=listDefs[0].DefNum;
		}
		payment.PaymentSource=CreditCardSource.None;
		payment.ProcessStatus=ProcessStat.OfficeProcessed;
		payment.PayAmt=payAmt;
		using var formPayment=new FormPayment(_patient,_family,payment,preferCurrentPat);
		formPayment.IsNew=true;
		formPayment.IsIncomeTransfer=isIncomeTransfer;
		var listAccountEntries=new List<AccountEntry>();
		if(gridAccount.SelectedIndices.Length>0) {
			var table=_dataSetMain.Tables["account"];
			var listIndicesSelected=gridAccount.SelectedIndices.ToList();
			for(var i=0;i<listIndicesSelected.Count;i++) {
				var adjNum=SIn.Long(table.Rows[listIndicesSelected[i]]["AdjNum"].ToString());
				var chargesDouble=SIn.Double(table.Rows[listIndicesSelected[i]]["chargesDouble"].ToString());
				var payPlanChargeNum=SIn.Long(table.Rows[listIndicesSelected[i]]["PayPlanChargeNum"].ToString());
				var procNum=SIn.Long(table.Rows[listIndicesSelected[i]]["ProcNum"].ToString());
				//Add each selected proc to the list
				if(procNum > 0) {
					listAccountEntries.Add(new AccountEntry(Procedures.GetOneProc(procNum,false)));
				}
				//Add selected positive pay plan debit to the list. Important to check for chargesDouble because there can be negative debits.
				if(CompareDecimal.IsGreaterThanZero(chargesDouble) && payPlanChargeNum > 0) {
					listAccountEntries.Add(new AccountEntry(PayPlanCharges.GetOne(payPlanChargeNum)));
				}
				if(adjNum > 0) {
					var adjustment=Adjustments.GetOne(adjNum);
					//Don't include negative adjustments or ones attached to procs because of the way we pay off procs.
					if(adjustment.AdjAmt>0 && adjustment.ProcNum==0) {
						listAccountEntries.Add(new AccountEntry(adjustment));
					}
				}
			}
		}
		var unearnedAmt=SIn.Double(labelUnearnedAmt.Text);
		//Don't allow the user to allocate negative unearned which is a problem that needs to be handled with a real income transfer.
		if(isPrePay && CompareDecimal.IsGreaterThanZero(unearnedAmt)) {
			if(listAccountEntries.Count<1) {
				using var formProcSelect=new FormProcSelect(_patient.PatNum,true,doShowAdjustments:true,doShowTreatmentPlanProcs:false);
				if(formProcSelect.ShowDialog()!=DialogResult.OK) {
					return;
				}
				listAccountEntries=formProcSelect.ListAccountEntries;
			}
			formPayment.UnearnedAmt=unearnedAmt;
		}
		formPayment.ListAccountEntriesPayFirst=listAccountEntries;
		if(payment.PayDate.Date>DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed) && !PrefC.GetBool(PrefName.AccountAllowFutureDebits)) {
			MsgBox.Show(this,"Payments cannot be in the future.");
			return;
		}
		payment.PayAmt=payAmt;
		Payments.Insert(payment);
		formPayment.ShowDialog();
		//If this is a payment received from Transworld, we don't want to send any new update messages to Transworld for any splits on this payment.
		//To prevent new msgs from being sent, we will insert TsiTransLogs linked to all splits with TsiTransType.None.  The ODService will update the
		//log TransAmt for any edits to this paysplit instead of sending a new msg to Transworld.
		if(!isTsiPayment) {
			ModuleSelected(_patient.PatNum);
			return;
		}
		var paymentTsi=Payments.GetPayment(payment.PayNum);
		if(paymentTsi!=null) {
			var listPaySplits=PaySplits.GetForPayment(paymentTsi.PayNum);
			if(listPaySplits.Count>0) {
				var patAging=Patients.GetAgingListFromGuarNums([_patient.Guarantor]).FirstOrDefault();
				var listTsiTransLogs=new List<TsiTransLog>();
				for(var i=0;i<listPaySplits.Count;i++) {
					var logAmt=patAging.ListTsiLogs.FindAll(x => x.FKeyType==TsiFKeyType.PaySplit && x.FKey==listPaySplits[i].SplitNum).Sum(x => x.TransAmt);
					if(CompareDouble.IsEqual(listPaySplits[i].SplitAmt,logAmt)) {
						continue;//split already linked to logs that sum to the split amount, nothing to do with this one
					}
					listTsiTransLogs.Add(new TsiTransLog {
						PatNum=patAging.PatNum,//this is the account guarantor, since these are reconciled by guars
						UserNum=Security.CurUser.UserNum,
						TransType=TsiTransType.None,
						//TransDateTime=DateTime.Now,//set on insert, not editable by user
						//DemandType=TsiDemandType.Accelerator,//only valid for placement msgs
						//ServiceCode=TsiServiceCode.Diplomatic,//only valid for placement msgs
						ClientId=patAging.ListTsiLogs.FirstOrDefault()?.ClientId??"",//can be blank, not used since this isn't really sent to Transworld
						TransAmt=-listPaySplits[i].SplitAmt-logAmt,//Ex. already logged -10; split changed to -20; -20-(-10)=-10; -10 this split + -10 already logged = -20 split amt
						AccountBalance=patAging.AmountDue-listPaySplits[i].SplitAmt-logAmt,
						FKeyType=TsiFKeyType.PaySplit,
						FKey=listPaySplits[i].SplitNum,
						RawMsgText="This was not a message sent to Transworld.  This paysplit was entered due to a payment received from Transworld.",
						ClinicNum=true?patAging.ClinicNum:0
						//,TransJson=""//only valid for placement msgs
					});
				}
				if(listTsiTransLogs.Count>0) {
					TsiTransLogs.InsertMany(listTsiTransLogs);
				}
			}
		}
		ModuleSelected(_patient.PatNum);
	}

	private void toolBarButQuickProcs_Click() {
		if(!Security.IsAuthorized(EnumPermType.AccountProcsQuickAdd)) {
			return;
		}
		//Main QuickCharge button was clicked.  Create a textbox that can be entered so users can insert manually entered proc codes.
		if(!Security.IsAuthorized(EnumPermType.ProcComplCreate,DateTime.Today,true)) {//Button doesn't show up unless they have AccountQuickCharge permission. 
			//user can still use dropdown, just not type in codes.
			contextMenuQuickProcs.Show(this,new Point(_butQuickProcs.Bounds.X,_butQuickProcs.Bounds.Y+_butQuickProcs.Bounds.Height));
			return;
		}
		textQuickProcs.SetBounds(_butQuickProcs.Bounds.X+1,_butQuickProcs.Bounds.Y+2,_butQuickProcs.Bounds.Width-17,_butQuickProcs.Bounds.Height-2);
		textQuickProcs.Visible=true;
		textQuickProcs.BringToFront();
		textQuickProcs.Focus();
		textQuickProcs.Capture=true;
	}

	private void toolBarButRepeatCharge_Click() {
		var repeatCharge=new RepeatCharge();
		repeatCharge.PatNum=_patient.PatNum;
		repeatCharge.DateStart=DateTime.Today;
		using var formRepeatChargeEdit=new FormRepeatChargeEdit(repeatCharge);
		formRepeatChargeEdit.IsNew=true;
		formRepeatChargeEdit.ShowDialog();
		ModuleSelected(_patient.PatNum);
	}

	private void toolBarButStatement_Click() {
		var dateStop=DateTime.MinValue;
		if(textDateEnd.IsValid() && textDateEnd.Text!="") {
			dateStop=SIn.Date(textDateEnd.Text);
		}
		var dateStart=DateTime.MinValue;
		if(textDateStart.IsValid() && textDateStart.Text!="") {//textDateStart has ultimate precedence. User may have intentionally set the date range for statement.
			dateStart=SIn.Date(textDateStart.Text);
		}
		var statement=Statements.GenerateStatement(_patient,dateStart,dateStop,StatementMode.InPerson);
		PrintStatement(statement);
		ModuleSelected(_patient.PatNum);
	}
	#endregion Methods - Private ToolBar

	#region Methods - Private Refresh
		
	private void RefreshModuleData(long patNum,bool isSelectingFamily) {
		UpdateUrgFinNote();
		UpdateFinNote();
		if(patNum==0) {
			_patient=null;
			_family=null;
			_dataSetMain=null;
			_listPaySplitsHidden.Clear();
			return;
		}
		var dateFrom=DateTime.MinValue;
		var dateTo=DateTime.MaxValue;
		if(textDateStart.IsValid() && textDateEnd.IsValid()) {
			if(textDateStart.Text!="") {
				dateFrom=SIn.Date(textDateStart.Text);
			}
			if(textDateEnd.Text!="") {
				dateTo=SIn.Date(textDateEnd.Text);
			}
		}
		var doMakeSecLog=false;
		if(_patNumLast!=patNum) {
			doMakeSecLog=true;
			_patNumLast=patNum;
		}
		var doGetAutoOrtho=PrefC.GetBool(PrefName.OrthoEnabled);
		Action action=()=> _loadData=AccountModules.GetAll(patNum,dateFrom,dateTo,isSelectingFamily,checkShowDetail.Checked,true,true,doMakeSecLog,doGetAutoOrtho);
		try
		{
			action();
		}
		catch(ApplicationException ex) {
			if(ex.Message=="Missing codenum") {
				MsgBox.Show(this,$"Missing codenum. Please run database maintenance method {nameof(DatabaseMaintenances.ProcedurelogCodeNumInvalid)}.");
				_patient=null;
				_dataSetMain=null;
				return;
			}
			throw;
		}
		lock(_lockDataSetMain) {
			_dataSetMain=_loadData.DataSetMain;
		}
		_family=_loadData.Fam;
		_patient=_family.GetPatient(patNum);
		_patientNote=_loadData.PatNote;
		_patFieldArray=_loadData.ArrPatFields;
		if(PrefC.GetBool(PrefName.ShowFeatureSuperfamilies)) {
			_listPatientsSuperFamilyGuarantors=_loadData.SuperFamilyGuarantors;
			_listPatientsSuperFamilyMembers=_loadData.SuperFamilyMembers;
		}
		_listPaySplitsHidden=_loadData.ListUnearnedSplits
			.FindAll(x => PaySplits.GetHiddenUnearnedDefNums().Contains(x.UnearnedType) && _family.GetPatNums().Contains(x.PatNum));//Don't show out of family pay splits for the payer.
		FillSummary();
		SetDiscountPlanOrInsurancePlanDash();
	}

	private void RefreshModuleScreen(bool isSelectingFamily) {
		UpdateToolbarButtons();
		FillPats(isSelectingFamily);
		FillMisc();
		FillAging(isSelectingFamily);
		//must be in this order.
		FillRepeatCharges();//1
		FillPaymentPlans();//2
		if(PrefC.GetBool(PrefName.OrthoEnabled)) {
			FillAutoOrtho(false);
		}
		if(OrthoCases.HasOrthoCasesEnabled()) {
			FillOrthoCasesGrid();
		}
		FillPatInfo();
		FillTpUnearned();
		LayoutPanelsAndRefreshMainGrids(true);
	}

	///<summary>Sets the visibility of the DPlan Rem or Ins Rem labels</summary>
	private void SetDiscountPlanOrInsurancePlanDash() {
		if(_loadData.DiscountPlanSub==null || _patient==null) {
			labelInsRem.Visible=true;
			labelInsRem.Enabled=true;
			labelDisRem.Visible=false;
			labelDisRem.Enabled=false;
			return;
		}
		groupBoxIndDis.RefreshDiscountPlan(_patient,_loadData.DiscountPlanSub,_loadData.DiscountPlan);
		labelDisRem.Visible=true;
		labelDisRem.Enabled=true;
		labelInsRem.Visible=false;
		labelInsRem.Enabled=false;
	}

	private void RefreshOrthoCasesGridRows() {
		gridOrthoCases.BeginUpdate();
		gridOrthoCases.ListGridRows.Clear();
		if(IsFamilySelected()) {
			gridOrthoCases.EndUpdate();
			return;
		}
		GridRow row;
		if(_patient!=null) {
			_listOrthoCases=OrthoCases.Refresh(_patient.PatNum);
		}
		var listOrthoProcLinksForPat=OrthoProcLinks.GetManyByOrthoCases(_listOrthoCases.ConvertAll(x => x.OrthoCaseNum));
		var listProceduresLinkedForPat=Procedures.GetManyProc(listOrthoProcLinksForPat.ConvertAll(x => x.ProcNum),false);
		butAddOrthoCase.Enabled=true;
		for(var i=0;i<_listOrthoCases.Count;i++) {
			//Skip the orthocase if it is inactive and we are not showing inactive orthocases
			if(checkHideInactiveOrthoCases.Checked && !_listOrthoCases[i].IsActive) {
				continue;
			}
			row=new GridRow();
			if(_listOrthoCases[i].IsActive) {
				row.Cells.Add("X");
				butAddOrthoCase.Enabled=false;//Can only have one active OrthoCase, se we deactivate the button to add a new active OrthoCase.
			}
			else {
				row.Cells.Add("");
			}
			if(_listOrthoCases[i].IsTransfer) {
				row.Cells.Add("X");
				row.Cells.Add(_listOrthoCases[i].BandingDate.ToShortDateString());
			}
			else {
				row.Cells.Add("");
				var orthoProcLinkBanding=listOrthoProcLinksForPat.Find(x=>x.ProcLinkType==OrthoProcType.Banding && x.OrthoCaseNum==_listOrthoCases[i].OrthoCaseNum);
				if(orthoProcLinkBanding is null) {
					row.Cells.Add(Lans.g("TableOrthoCases","Banding Not Scheduled"));
				}
				else {
					var procedureBanding=listProceduresLinkedForPat.Find(x=>x.ProcNum==orthoProcLinkBanding.ProcNum);
					if(procedureBanding is null) {
						row.Cells.Add(Lans.g("TableOrthoCases","Banding Not Scheduled"));
					}
					else if(procedureBanding.ProcStatus==ProcStat.C) {
						row.Cells.Add(procedureBanding.ProcDate.ToShortDateString());
					}
					else if(procedureBanding.ProcStatus==ProcStat.TP && procedureBanding.AptNum!=0) {
						row.Cells.Add(procedureBanding.ProcDate.ToShortDateString());
					}
					else {
						row.Cells.Add(Lans.g("TableOrthoCases","Banding Not Scheduled"));
					}
				}
			}
			var orthoProcLinkDebond=listOrthoProcLinksForPat.Find(x=>x.ProcLinkType==OrthoProcType.Debond && x.OrthoCaseNum==_listOrthoCases[i].OrthoCaseNum);
			if(orthoProcLinkDebond is null) {
				row.Cells.Add(Lan.g("TableOrthoCases","Debond Incomplete"));
			}
			else{
				var procedureDebond=listProceduresLinkedForPat.Find(x=>x.ProcNum==orthoProcLinkDebond.ProcNum);
				if(procedureDebond is null) {
					row.Cells.Add(Lan.g("TableOrthoCases","Debond Incomplete"));
				}
				else if(procedureDebond.ProcStatus==ProcStat.C) {
					row.Cells.Add(procedureDebond.ProcDate.ToShortDateString());
				}
				else {
					row.Cells.Add(Lan.g("TableOrthoCases","Debond Incomplete"));
				}
			}
			row.Tag=_listOrthoCases[i];
			gridOrthoCases.ListGridRows.Add(row);
		}
		gridOrthoCases.EndUpdate();
	}

	///<summary>Enables toolbar buttons if a patient is selected, otherwise disables them.</summary>
	private void UpdateToolbarButtons() {
		if(_patient==null) {
			tabControlAccount.Enabled=false;
			ToolBarMain.Buttons["Payment"].Enabled=false;
			ToolBarMain.Buttons["Adjustment"].Enabled=false;
			ToolBarMain.Buttons["Insurance"].Enabled=false;
			ToolBarMain.Buttons["PayPlan"].Enabled=false;
			ToolBarMain.Buttons["InstallPlan"].Enabled=false;
			if(ToolBarMain.Buttons["QuickProcs"]!=null) {
				ToolBarMain.Buttons["QuickProcs"].Enabled=false;
			}
			if(ToolBarMain.Buttons["RepeatCharge"]!=null) {
				ToolBarMain.Buttons["RepeatCharge"].Enabled=false;
			}
			ToolBarMain.Buttons["Statement"].Enabled=false;
			ToolBarMain.Invalidate();
			textUrgFinNote.Enabled=false;
			textFinNote.Enabled=false;
			//butComm.Enabled=false;
			tabControlShow.Enabled=false;
		}
		else {
			tabControlAccount.Enabled=true;
			ToolBarMain.Buttons["Payment"].Enabled=true;
			ToolBarMain.Buttons["Adjustment"].Enabled=true;
			ToolBarMain.Buttons["Insurance"].Enabled=true;
			ToolBarMain.Buttons["PayPlan"].Enabled=true;
			ToolBarMain.Buttons["InstallPlan"].Enabled=true;
			if(ToolBarMain.Buttons["QuickProcs"]!=null) {
				ToolBarMain.Buttons["QuickProcs"].Enabled=true;
			}
			if(ToolBarMain.Buttons["RepeatCharge"]!=null) {
				ToolBarMain.Buttons["RepeatCharge"].Enabled=true;
			}
			ToolBarMain.Buttons["Statement"].Enabled=true;
			ToolBarMain.Invalidate();
			textUrgFinNote.Enabled=true;
			textFinNote.Enabled=true;
			//butComm.Enabled=true;
			tabControlShow.Enabled=true;
		}
		ToolBarMain.Invalidate();
	}
	#endregion Methods - Private Refresh

	#region Methods - Private Fill
	private void FillAging(bool isSelectingFamily) {
		if(_patient==null) {
			textOver90.Text="";
			text61_90.Text="";
			text31_60.Text="";
			text0_30.Text="";
			labelTotalAmt.Text="";
			labelInsEstAmt.Text="";
			labelBalanceAmt.Text="";
			labelPatEstBalAmt.Text="";
			labelUnearnedAmt.Text="";
			//labelInsLeftAmt.Text="";
			return;
		}
		textOver90.Text=_family.ListPats[0].BalOver90.ToString("F");
		text61_90.Text=_family.ListPats[0].Bal_61_90.ToString("F");
		text31_60.Text=_family.ListPats[0].Bal_31_60.ToString("F");
		text0_30.Text=_family.ListPats[0].Bal_0_30.ToString("F");
		var total=(decimal)_family.ListPats[0].BalTotal;
		var listDefNumsTpUnearned=Defs.GetDefsForCategory(DefCat.PaySplitUnearnedType)
				.FindAll(x => x.ItemValue!="")
				.ConvertAll(x => x.DefNum)
			;
		labelTotalAmt.Text=total.ToString("F");
		labelInsEstAmt.Text=_family.ListPats[0].InsEst.ToString("F");
		FillInsEstBreakdown();
		labelBalanceAmt.Text=(total - (decimal)_family.ListPats[0].InsEst).ToString("F");
		labelPatEstBalAmt.Text="";
		var tableMisc=_dataSetMain.Tables["misc"];
		if(!isSelectingFamily) {
			for(var i=0;i<tableMisc.Rows.Count;i++) {
				if(tableMisc.Rows[i]["descript"].ToString()=="patInsEst") {
					var estBal=(decimal)_patient.EstBalance-SIn.Decimal(tableMisc.Rows[i]["value"].ToString());
					labelPatEstBalAmt.Text=estBal.ToString("F");
				}
			}
		}
		labelUnearnedAmt.Text="";
		for(var i=0;i<tableMisc.Rows.Count;i++) {
			if(tableMisc.Rows[i]["descript"].ToString()=="unearnedIncome") {
				//remove TP splits that do not show on account due to def being checked. 
				var listUnearnedShownOnAccount=_loadData.ListUnearnedSplits.FindAll(x => !listDefNumsTpUnearned.Contains(x.UnearnedType)
				                                                                         && _family.ListPats.Select(y => y.PatNum).Contains(x.PatNum));//We do not want to show unearned balances for paysplits to other families
				labelUnearnedAmt.Text=listUnearnedShownOnAccount.Sum(x => x.SplitAmt).ToString("F");
				if(SIn.Double(labelUnearnedAmt.Text)<=0) {
					labelUnearnedAmt.ForeColor=Color.Black;
					labelUnearnedAmt.Font=new Font(labelUnearnedAmt.Font,FontStyle.Regular);
				}
				else {
					labelUnearnedAmt.ForeColor=Color.Firebrick;
					labelUnearnedAmt.Font=new Font(labelUnearnedAmt.Font,FontStyle.Bold);
				}
				FillUnearnedBreakdown(listUnearnedShownOnAccount);
			}
		}
		//labelInsLeft.Text=Lan.g(this,"Ins Left");
		//labelInsLeftAmt.Text="";//etc. Will be same for everyone
		var fontBold=new Font(FontFamily.GenericSansSerif,11,FontStyle.Bold);
		//In the new way of doing it, they are all visible and calculated identically,
		//but the emphasis simply changes by slight renaming of labels
		//and by font size changes.
		if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
			labelTotal.Text=Lan.g(this,"Balance");
			labelTotalAmt.Font=fontBold;
			labelTotalAmt.ForeColor=Color.Firebrick;
			panelAgeLine.Visible=true;//verical line
			labelInsEst.Text=Lan.g(this,"Ins Pending");
			labelBalance.Text=Lan.g(this,"After Ins");
			labelBalanceAmt.Font=this.Font;
			labelBalanceAmt.ForeColor=Color.Black;
			return;
		}
		//this is more common
		labelTotal.Text=Lan.g(this,"Total");
		labelTotalAmt.Font=this.Font;
		labelTotalAmt.ForeColor = Color.Black;
		panelAgeLine.Visible=false;
		labelInsEst.Text=Lan.g(this,"-InsEst");
		labelBalance.Text=Lan.g(this,"=Est Bal");
		labelBalanceAmt.Font=fontBold;
		labelBalanceAmt.ForeColor=Color.Firebrick;
	}

	private void FillUnearnedBreakdown(List<PaySplit> listPaySplitsOnAccount) {
		var listUnearnedDefs=Defs.GetUnearnedDefs();
		//Display every unearned type along with the total of payment splits that are within said unearned bucket.
		gridUnearnedBreakdown.BeginUpdate();
		gridUnearnedBreakdown.Columns.Clear();
		gridUnearnedBreakdown.Columns.Add(new GridColumn(Lan.g(this,"Type"),113));
		gridUnearnedBreakdown.Columns.Add(new GridColumn(Lan.g(this,"Amount"),80,HorizontalAlignment.Right));
		gridUnearnedBreakdown.ListGridRows.Clear();
		for(var i=0;i<listUnearnedDefs.Count;i++) {
			if(!listPaySplitsOnAccount.Exists(x=>x.UnearnedType==listUnearnedDefs[i].DefNum)) {
				continue;
			}
			var row=new GridRow();
			row.Cells.Add(listUnearnedDefs[i].ItemName);
			var sumForType=listPaySplitsOnAccount.FindAll(x=>x.UnearnedType==listUnearnedDefs[i].DefNum).Sum(x=>x.SplitAmt);
			row.Cells.Add(sumForType.ToString("N2"));
			gridUnearnedBreakdown.ListGridRows.Add(row);
		}
		gridUnearnedBreakdown.EndUpdate();
		var gridWidth=gridUnearnedBreakdown.Columns.Sum(x => x.ColWidth);
		gridUnearnedBreakdown.Size=new Size(gridWidth,gridUnearnedBreakdown.ListGridRows.Sum(x => x.State.HeightTotal)+24);//+24 for header height 15 plus 3 extra pixels for line spacing + 6 for font increments that would cause a scrollbar
		gridUnearnedBreakdown.Location=new Point(groupBoxFamilyIns.Left-1,groupBoxFamilyIns.Top-1);
		gridUnearnedBreakdown.BringToFront();
	}

	private void FillInsEstBreakdown() {
		var listPatNumsForFamily=_family.ListPats.Select(x=>x.PatNum).ToList();
		var table=ClaimProcs.GetClaimProcEstimatesForPatients(listPatNumsForFamily);
		double totalInsPayEst=0;
		double totalWriteOff=0;
		gridInsEstOpenClaims.BeginUpdate();
		gridInsEstOpenClaims.Columns.Clear();
		gridInsEstOpenClaims.Columns.Add(new GridColumn(Lan.g(this,"DateService"),90));
		gridInsEstOpenClaims.Columns.Add(new GridColumn(Lan.g(this,"ProcCode"),70));
		gridInsEstOpenClaims.Columns.Add(new GridColumn(Lan.g(this,"InsEst"),60,HorizontalAlignment.Right));
		gridInsEstOpenClaims.Columns.Add(new GridColumn(Lan.g(this,"WriteOff"),60,HorizontalAlignment.Right));
		gridInsEstOpenClaims.ListGridRows.Clear();
		for(var i=0;i<table.Rows.Count;i++) {
			var row=new GridRow();
			row.Cells.Add(SIn.DateTime(table.Rows[i]["DateService"].ToString()).ToShortDateString());
			var codeNum=SIn.Long(table.Rows[i]["CodeNum"].ToString());
			var procedureCode=ProcedureCodes.GetProcCodeFromDb(codeNum);
			row.Cells.Add(procedureCode.ProcCode);
			var insPayEst=SIn.Double(table.Rows[i]["InsPayEst"].ToString());
			var writeOff=SIn.Double(table.Rows[i]["WriteOff"].ToString());
			if(insPayEst==0 && writeOff==0) {
				continue;
			}
			totalInsPayEst+=insPayEst;
			totalWriteOff+=writeOff;
			row.Cells.Add(insPayEst.ToString("c"));
			row.Cells.Add(writeOff.ToString("c"));
			gridInsEstOpenClaims.ListGridRows.Add(row);
		}
		if(gridInsEstOpenClaims.ListGridRows.Count>0) {
			gridInsEstOpenClaims.ListGridRows.Last().ColorLborder=Color.Black;
			var rowTotal=new GridRow();
			rowTotal.Cells.Add(Lan.g(this,"Total"));
			rowTotal.Cells.Add(Lan.g(this,""));
			rowTotal.Cells.Add(totalInsPayEst.ToString("c"));
			rowTotal.Cells.Add(totalWriteOff.ToString("c"));
			gridInsEstOpenClaims.ListGridRows.Add(rowTotal);
		}
		gridInsEstOpenClaims.EndUpdate();
		var gridWidth=gridInsEstOpenClaims.Columns.Sum(x => x.ColWidth);
		gridInsEstOpenClaims.Size=new Size(gridWidth,gridInsEstOpenClaims.ListGridRows.Sum(x => x.State.HeightTotal)+24);//+24 for header height 15 plus 3 extra pixels for line spacing + 6 for font increments that would cause a scrollbar
		gridInsEstOpenClaims.Location=new Point(labelInsEst.Right-gridWidth/2-labelInsEst.Width/2,panelAging.Bottom+1);
		gridInsEstOpenClaims.BringToFront();
	}

	private void FillAutoOrtho(bool doCalculateFirstDate=true) {
		if(_patient==null) {
			return;
		}
		gridAutoOrtho.BeginUpdate();
		gridAutoOrtho.Columns.Clear();
		gridAutoOrtho.Columns.Add(new GridColumn("",gridAutoOrtho.Width/2-20));//,HorizontalAlignment.Right));
		gridAutoOrtho.Columns.Add(new GridColumn("",gridAutoOrtho.Width/2+20));
		gridAutoOrtho.ListGridRows.Clear();
		var row=new GridRow();
		//Insurance Information
		//PriClaimType
		var listPatPlans=_loadData.ListPatPlans;
		if(listPatPlans.Count==0) {
			row=new GridRow();
			row.Cells.Add("");
			row.Cells.Add(Lan.g(this,"Patient has no insurance."));
			gridAutoOrtho.ListGridRows.Add(row);
		}
		else {
			var listDefs=Defs.GetDefsForCategory(DefCat.MiscColors);
			for(var i=0;i<listPatPlans.Count;i++) {
				var patPlan=listPatPlans[i];
				var insSub=InsSubs.GetSub(patPlan.InsSubNum,_loadData.ListInsSubs);
				var insPlan=InsPlans.GetPlan(insSub.PlanNum,_loadData.ListInsPlans);
				var carrierName=Carriers.GetCarrier(insPlan.CarrierNum).CarrierName;
				var subIDCur=insSub.SubscriberID;
				row=new GridRow();
				var autoOrthoPat=new AutoOrthoPat {
					InsPlan_=insPlan,
					PatPlan_=patPlan,
					CarrierName=carrierName,
					DefaultFee=insPlan.OrthoAutoFeeBilled,
					SubID=subIDCur
				};
				if(i==listPatPlans.Count-1) { //last row in the insurance info section
					row.ColorLborder=Color.Black;
				}
				row.ColorBackG=listDefs[(int)DefCatMiscColors.FamilyModuleCoverage].ItemColor; //same logic as family module insurance colors.
				switch(i) {
					case 0: //primary
						row.Cells.Add(Lan.g(this,"Primary Ins"));
						break;
					case 1: //secondary
						row.Cells.Add(Lan.g(this,"Secondary Ins"));
						break;
					case 2: //tertiary
						row.Cells.Add(Lan.g(this,"Tertiary Ins"));
						break;
					default: //other
						row.Cells.Add(Lan.g(this,"Other Ins"));
						break;
				}
				row.Cells.Add("");
				row.Bold=true;
				row.Tag=autoOrthoPat;
				gridAutoOrtho.ListGridRows.Add(row);
				//claimtype
				row=new GridRow();
				row.Cells.Add(Lan.g(this,"ClaimType"));
				if(insPlan==null) {
					row.Cells.Add("");
				}
				else {
					row.Cells.Add(insPlan.OrthoType.ToString());
				}
				row.Tag=autoOrthoPat;
				gridAutoOrtho.ListGridRows.Add(row);
				//Only show for initialPlusPeriodic claimtype.
				if(insPlan.OrthoType==OrthoClaimType.InitialPlusPeriodic) {
					//Frequency
					row=new GridRow();
					row.Cells.Add(Lan.g(this,"Frequency"));
					row.Cells.Add(insPlan.OrthoAutoProcFreq.ToString());
					row.Tag=autoOrthoPat;
					gridAutoOrtho.ListGridRows.Add(row);
					//Fee
					row=new GridRow();
					row.Cells.Add(Lan.g(this,"FeeBilled"));
					row.Cells.Add(patPlan.OrthoAutoFeeBilledOverride==-1 ? SOut.Double(insPlan.OrthoAutoFeeBilled) : SOut.Double(patPlan.OrthoAutoFeeBilledOverride));
					row.Tag=autoOrthoPat;
					gridAutoOrtho.ListGridRows.Add(row);
				}
				//Last Claim Date
				row=new GridRow();
				DateTime dateLast;
				if(!_loadData.DictDateLastOrthoClaims.TryGetValue(patPlan.PatPlanNum,out dateLast)) {
					dateLast=Claims.GetDateLastOrthoClaim(patPlan,insPlan.OrthoType);
				}
				row.Cells.Add(Lan.g(this,"LastClaim"));
				row.Cells.Add(dateLast==null || dateLast.Date == DateTime.MinValue.Date ? Lan.g(this,"None Sent") : dateLast.ToShortDateString());
				row.Tag=autoOrthoPat;
				gridAutoOrtho.ListGridRows.Add(row);
				//NextClaimDate - Only show for initialPlusPeriodic claimtype.
				if(insPlan.OrthoType==OrthoClaimType.InitialPlusPeriodic) {
					row=new GridRow();
					row.Cells.Add(Lan.g(this,"NextClaim"));
					row.Cells.Add(patPlan.OrthoAutoNextClaimDate.Date == DateTime.MinValue.Date ? Lan.g(this,"Stopped") : patPlan.OrthoAutoNextClaimDate.ToShortDateString());
					row.Tag=autoOrthoPat;
					gridAutoOrtho.ListGridRows.Add(row);
				}
			}
		}
		//Pat Ortho Info Title
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Pat Ortho Info"));
		row.Cells.Add("");
		row.ColorBackG=Color.LightCyan;
		row.Bold=true;
		row.ColorLborder=Color.Black;
		gridAutoOrtho.ListGridRows.Add(row);
		//OrthoAutoProc Freq
		if(doCalculateFirstDate) {
			_loadData.FirstOrthoProcDate=Procedures.GetFirstOrthoProcDate(_patientNote);
		}
		var dateFirstOrthoProc=_loadData.FirstOrthoProcDate;
		if(dateFirstOrthoProc==DateTime.MinValue) { //no ortho procedures charted for this patient.
			row=new GridRow();
			row.Cells.Add("");
			row.Cells.Add(Lan.g(this,"No ortho procedures charted."));
			gridAutoOrtho.ListGridRows.Add(row);
			gridAutoOrtho.EndUpdate();
			return;
		}
		var txMonthsTotal=_patientNote.OrthoMonthsTreatOverride==-1?PrefC.GetByte(PrefName.OrthoDefaultMonthsTreat):_patientNote.OrthoMonthsTreatOverride;
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Total Tx Time")); //Number of Years/Months/Days since the first ortho procedure on this account
		var dateSpan=new DateSpan(dateFirstOrthoProc,DateTime.Today);
		var txTimeInMonths=OrthoSchedules.CalculateAutoOrthoTimeInMonths(dateFirstOrthoProc,dateSpan,txMonthsTotal);
		var strDateDiff="";
		if(dateSpan.YearsDiff!=0) {
			strDateDiff+=dateSpan.YearsDiff+" "+Lan.g(this,"year"+(dateSpan.YearsDiff==1 ? "" : "s"));
		}
		if(dateSpan.MonthsDiff!=0) {
			if(strDateDiff!="") {
				strDateDiff+=", ";
			}
			strDateDiff+=dateSpan.MonthsDiff+" "+Lan.g(this,"month"+(dateSpan.MonthsDiff==1 ? "" : "s"));
		}
		if(dateSpan.DaysDiff!=0 || strDateDiff=="") {
			if(strDateDiff!="") {
				strDateDiff+=", ";
			}
			strDateDiff+=dateSpan.DaysDiff+" "+Lan.g(this,"day"+(dateSpan.DaysDiff==1 ? "" : "s"));
		}
		row.Cells.Add(strDateDiff);
		gridAutoOrtho.ListGridRows.Add(row);
		//Date Start
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Date Start")); //Date of the first ortho procedure on this account
		row.Cells.Add(dateFirstOrthoProc.ToShortDateString());
		gridAutoOrtho.ListGridRows.Add(row);
		//Tx Months Total
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Tx Months Total")); //this patient's OrthoClaimMonthsTreatment, or the practice default if 0.
		row.Cells.Add(txMonthsTotal.ToString());
		gridAutoOrtho.ListGridRows.Add(row);
		//Months in treatment
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Months in Treatment"));
		row.Cells.Add(txTimeInMonths.ToString());
		gridAutoOrtho.ListGridRows.Add(row);
		//Months Rem
		row=new GridRow();
		row.Cells.Add(Lan.g(this,"Months Rem")); //Months Total - Total Tx Time
		row.Cells.Add(Math.Max(0,txMonthsTotal-txTimeInMonths).ToString());
		gridAutoOrtho.ListGridRows.Add(row);
		gridAutoOrtho.EndUpdate();
	}

	/// <summary>Fills the commlog grid on this form.  It does not refresh the data from the database.</summary>
	private void FillComm() {
		if(_dataSetMain==null) {
			gridComm.BeginUpdate();
			gridComm.ListGridRows.Clear();
			gridComm.EndUpdate();
			return;
		}
		gridComm.BeginUpdate();
		gridComm.Columns.Clear();
		var col=new GridColumn(Lan.g("TableCommLogAccount","Date"),70);
		gridComm.Columns.Add(col);
		col=new GridColumn(Lan.g("TableCommLogAccount","Time"),42);//,HorizontalAlignment.Right);
		gridComm.Columns.Add(col);
		col=new GridColumn(Lan.g("TableCommLogAccount","Name"),80);
		gridComm.Columns.Add(col);
		col=new GridColumn(Lan.g("TableCommLogAccount","Type"),80);
		gridComm.Columns.Add(col);
		col=new GridColumn(Lan.g("TableCommLogAccount","Mode"),55);
		gridComm.Columns.Add(col);
		//col=new ODGridColumn(Lan.g("TableCommLogAccount","Sent/Recd"),75);
		//gridComm.Columns.Add(col);
		col=new GridColumn(Lan.g("TableCommLogAccount","Note"),455);
		gridComm.Columns.Add(col);
		gridComm.ListGridRows.Clear();
		GridRow row;
		bool isCommlogAutomated;
		var table=_dataSetMain.Tables["Commlog"];
		for(var i=0;i<table.Rows.Count;i++) {
			isCommlogAutomated=Commlogs.IsAutomated(table.Rows[i]["commType"].ToString(),
				SIn.Enum<CommItemSource>(table.Rows[i]["CommSource"].ToString()));
			//Skip commlog entries which are automated per user option.
			if(!this.checkShowCommAuto.Checked && isCommlogAutomated) {
				continue;
			}
			//Skip commlog entries which belong to other family members per user option.
			if(!this.checkShowFamilyComm.Checked//show family not checked
			   && !IsFamilySelected()//family not selected
			   && table.Rows[i]["PatNum"].ToString()!=_patient.PatNum.ToString()//not this patient
			   && table.Rows[i]["FormPatNum"].ToString()=="0")//not a questionnaire (FormPat)
			{
				continue;
			}
			else if(table.Rows[i]["EmailMessageNum"].ToString()!="0") {//if this is an Email
				if(((HideInFlags)SIn.Int(table.Rows[i]["EmailMessageHideIn"].ToString())).HasFlag(HideInFlags.AccountCommLog)) {
					continue;
				}
			}
			row=new GridRow();
			var argbColorValue=SIn.Int(table.Rows[i]["colorText"].ToString());//Convert to int. If blank or 0, will use default color.
			if(argbColorValue!=Color.Empty.ToArgb()) {//A color was set for this commlog type
				row.ColorText=Color.FromArgb(argbColorValue);
			}
			row.Cells.Add(table.Rows[i]["commDate"].ToString());
			row.Cells.Add(table.Rows[i]["commTime"].ToString());
			if(IsFamilySelected()) {
				row.Cells.Add(table.Rows[i]["patName"].ToString());
			}
			else {//one patient
				if(table.Rows[i]["PatNum"].ToString()==_patient.PatNum.ToString()) {//if this patient
					row.Cells.Add("");
				}
				else {//other patient
					row.Cells.Add(table.Rows[i]["patName"].ToString());
				}
			}
			row.Cells.Add(table.Rows[i]["commName"].ToString());
			row.Cells.Add(table.Rows[i]["mode"].ToString());
			//row.Cells.Add(table.Rows[i]["sentOrReceived"].ToString());
			if(isCommlogAutomated) { //If it's an automated commlog, show only the first line.
				row.Cells.Add(Commlogs.GetNoteFirstLine(table.Rows[i]["Note"].ToString()));
			}
			else {
				row.Cells.Add(table.Rows[i]["Note"].ToString());
			}
			row.Tag=i;
			gridComm.ListGridRows.Add(row);
		}
		gridComm.EndUpdate();
		gridComm.ScrollToEnd();
	}

	private void FillMain() {
		gridAccount.BeginUpdate();
		gridAccount.Columns.Clear();
		GridColumn col;
		_listDisplayFieldsForMainGrid=DisplayFields.GetForCategory(DisplayFieldCategory.AccountModule);
		HorizontalAlignment horizontalAlignment;
		for(var i=0;i<_listDisplayFieldsForMainGrid.Count;i++) {
			horizontalAlignment=HorizontalAlignment.Left;
			if(_listDisplayFieldsForMainGrid[i].InternalName=="Charges"
			   || _listDisplayFieldsForMainGrid[i].InternalName=="Credits"
			   || _listDisplayFieldsForMainGrid[i].InternalName=="Balance")
			{
				horizontalAlignment=HorizontalAlignment.Right;
			}
			if(_listDisplayFieldsForMainGrid[i].InternalName=="Signed") {
				horizontalAlignment=HorizontalAlignment.Center;
			}
			if(_listDisplayFieldsForMainGrid[i].Description=="") {
				col=new GridColumn(_listDisplayFieldsForMainGrid[i].InternalName,_listDisplayFieldsForMainGrid[i].ColumnWidth,horizontalAlignment);
			}
			else {
				col=new GridColumn(_listDisplayFieldsForMainGrid[i].Description,_listDisplayFieldsForMainGrid[i].ColumnWidth,horizontalAlignment);
			}
			gridAccount.Columns.Add(col);
		}
		if(gridAccount.Columns.Sum(x => x.ColWidth)>gridAccount.Width) {
			gridAccount.HScrollVisible=true;
		}
		else {
		}
		gridAccount.ListGridRows.Clear();
		GridRow row;
		DataTable table=null;
		if(_patient==null) {
			table=new DataTable();
		}
		else{
			table=_dataSetMain.Tables["account"];
		}
		for(var i=0;i<table.Rows.Count;i++) {
			row=new GridRow();
			for(var f=0;f<_listDisplayFieldsForMainGrid.Count;f++) {
				switch(_listDisplayFieldsForMainGrid[f].InternalName) {
					case "Date":
						row.Cells.Add(table.Rows[i]["date"].ToString());
						break;
					case "Patient":
						row.Cells.Add(table.Rows[i]["patient"].ToString());
						break;
					case "Prov":
						row.Cells.Add(table.Rows[i]["prov"].ToString());
						break;
					case "Clinic":
						row.Cells.Add(Clinics.GetAbbr(SIn.Long(table.Rows[i]["ClinicNum"].ToString())));
						break;
					case "ClinicDesc":
						row.Cells.Add(Clinics.GetDesc(SIn.Long(table.Rows[i]["ClinicNum"].ToString())));
						break;
					case "Code":
						row.Cells.Add(table.Rows[i]["ProcCode"].ToString());
						break;
					case "Tth":
						row.Cells.Add(table.Rows[i]["tth"].ToString());
						break;
					case "Description":
						row.Cells.Add(table.Rows[i]["description"].ToString());
						break;
					case "Charges":
						row.Cells.Add(table.Rows[i]["charges"].ToString());
						break;
					case "Credits":
						row.Cells.Add(table.Rows[i]["credits"].ToString());
						break;
					case "Balance":
						row.Cells.Add(table.Rows[i]["balance"].ToString());
						break;
					case "Signed":
						row.Cells.Add(table.Rows[i]["signed"].ToString());
						break;
					case "Abbr": //procedure abbreviation
						if(!string.IsNullOrEmpty(table.Rows[i]["AbbrDesc"].ToString())) {
							row.Cells.Add(table.Rows[i]["AbbrDesc"].ToString());
						}
						else {
							row.Cells.Add("");
						}
						break;
					default:
						row.Cells.Add("");
						break;
				}
			}
			row.ColorText=Color.FromArgb(SIn.Int(table.Rows[i]["colorText"].ToString()));
			if(i==table.Rows.Count-1//last row
			   || (DateTime)table.Rows[i]["DateTime"]!=(DateTime)table.Rows[i+1]["DateTime"])
			{
				row.ColorLborder=Color.Black;
			}
			gridAccount.ListGridRows.Add(row);
		}
		gridAccount.EndUpdate();
		if(_scrollValueWhenDoubleClick==-1) {
			gridAccount.ScrollToEnd();
		}
		else {
			gridAccount.ScrollValue=_scrollValueWhenDoubleClick;
			_scrollValueWhenDoubleClick=-1;
		}
	}

	private void FillMisc() {
		//textCC.Text="";
		//textCCexp.Text="";
		if(_patient==null) {
			textUrgFinNote.Text="";
			textFinNote.Text="";
		}
		else{
			textUrgFinNote.Text=_family.ListPats[0].FamFinUrgNote;
			_famUrgFinNoteOnLoad=_family.ListPats[0].FamFinUrgNote;
			textFinNote.Text=_patientNote.FamFinancial;
			_famFinNoteOnLoad=_patientNote.FamFinancial;
			if(!textFinNote.Focused) {
				textFinNote.SelectionStart=textFinNote.Text.Length;
				//This will cause a crash if the richTextBox currently has focus. We don't know why.
				//Only happens if you call this during a Leave event, and only when moving between two ODtextBoxes.
				//Tested with two ordinary richTextBoxes, and the problem does not exist.
				//We may pursue fixing the root problem some day, but this workaround will do for now.
				textFinNote.ScrollToCaret();
			}
			if(!textUrgFinNote.Focused) {
				textUrgFinNote.SelectionStart=0;
				textUrgFinNote.ScrollToCaret();
			}
		}
		IsUrgFinNoteChanged=false;
		IsFinNoteChanged=false;
		//CCChanged=false;
		textUrgFinNote.ReadOnly=false;
		textFinNote.ReadOnly=false;
	}

	private void FillOrthoCasesGrid() {
		gridOrthoCases.BeginUpdate();
		gridOrthoCases.Columns.Clear();
		var col=new GridColumn(Lan.g("TableOrthoCases","Is Active"),70,HorizontalAlignment.Center);
		gridOrthoCases.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOrthoCases","Is Transfer"),70,HorizontalAlignment.Center);
		gridOrthoCases.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOrthoCases","Start Date"),130,HorizontalAlignment.Center);
		gridOrthoCases.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOrthoCases","Completion Date"),120,HorizontalAlignment.Center) { IsWidthDynamic=true };
		gridOrthoCases.Columns.Add(col);
		gridOrthoCases.ListGridRows.Clear();
		gridOrthoCases.EndUpdate();
		RefreshOrthoCasesGridRows();
	}

	private void FillPatInfo() {
		if(_patient==null) {
			gridPatInfo.BeginUpdate();
			gridPatInfo.ListGridRows.Clear();
			gridPatInfo.Columns.Clear();
			gridPatInfo.EndUpdate();
			return;
		}
		gridPatInfo.BeginUpdate();
		gridPatInfo.Columns.Clear();
		var col=new GridColumn("",80);
		gridPatInfo.Columns.Add(col);
		col=new GridColumn("",150);
		gridPatInfo.Columns.Add(col);
		gridPatInfo.ListGridRows.Clear();
		GridRow row;
		_listDisplayFieldsPatInfo=DisplayFields.GetForCategory(DisplayFieldCategory.AccountPatientInformation);
		for(var f=0;f<_listDisplayFieldsPatInfo.Count;f++) {
			row=new GridRow();
			if(_listDisplayFieldsPatInfo[f].Description=="") {
				if(_listDisplayFieldsPatInfo[f].InternalName=="PatFields") {
					//don't add a cell
				}
				else {
					row.Cells.Add(_listDisplayFieldsPatInfo[f].InternalName);
				}
			}
			else {
				if(_listDisplayFieldsPatInfo[f].InternalName=="PatFields") {
					//don't add a cell
				}
				else {
					row.Cells.Add(_listDisplayFieldsPatInfo[f].Description);
				}
			}
			switch(_listDisplayFieldsPatInfo[f].InternalName) {
				case "Billing Type":
					row.Cells.Add(Defs.GetName(DefCat.BillingTypes,_patient.BillingType));
					break;
				case "PatFields":
					PatFieldL.AddPatFieldsToGrid(gridPatInfo,_patFieldArray.ToList(),FieldLocations.Account);
					break;
			}
			if(_listDisplayFieldsPatInfo[f].InternalName=="PatFields") {
				//don't add the row here
			}
			else {
				gridPatInfo.ListGridRows.Add(row);
			}
		}
		gridPatInfo.EndUpdate();
	}

	private void FillPats(bool isSelectingFamily) {
		if(_useSuperFam && SuperFamHasData()) {
			gridAcctPat.Title="SuperFamily";
			FillGridAcctPatAsSuperFam();
		}
		else {
			gridAcctPat.Title="Patients";
			FillGridAcctPat(isSelectingFamily);
		}
		if(_patient==null) {
			return;
		}
		ToggleUseSuperFamCheckboxEnabled(_superFamPrefEnabled && _patient.SuperFamily>0);
	}

	private void FillGridAcctPat(bool isSelectingFamily) {
		if(_patient==null) {
			gridAcctPat.BeginUpdate();
			gridAcctPat.ListGridRows.Clear();
			gridAcctPat.EndUpdate();
			return;
		}
		gridAcctPat.BeginUpdate();
		gridAcctPat.Columns.Clear();
		var col=new GridColumn(Lan.g("TableAccountPat","Patient"),105);
		gridAcctPat.Columns.Add(col);
		col=new GridColumn(Lan.g("TableAccountPat","Bal"),49,HorizontalAlignment.Right);
		gridAcctPat.Columns.Add(col);
		gridAcctPat.ListGridRows.Clear();
		GridRow row;
		var table=_dataSetMain.Tables["patient"];
		decimal balance=0;
		for(var i=0;i<table.Rows.Count;i++) {
			if(i!=table.Rows.Count-1 && PatientLinks.WasPatientMerged(SIn.Long(table.Rows[i]["PatNum"].ToString()),_loadData.ListMergeLinks)
			                         && _family.ListPats[i].PatNum!=_patient.PatNum && (decimal)table.Rows[i]["balanceDouble"]==0)
			{
				//Hide merged patients so that new things don't get added to them. If the user really wants to find this patient, they will have to use 
				//the Select Patient window.
				continue;
			}
			balance+=(decimal)table.Rows[i]["balanceDouble"];
			row=new GridRow();
			row.Cells.Add(GetPatNameFromTable(table,i));
			row.Cells.Add(table.Rows[i]["balance"].ToString());
			row.Tag=_family.ListPats.First(x=>x.PatNum==SIn.Long(table.Rows[i]["PatNum"].ToString()));
			if(i==0 || i==table.Rows.Count-1) {
				row.Bold=true;
			}
			gridAcctPat.ListGridRows.Add(row);
		}
		gridAcctPat.EndUpdate();
		var index=gridAcctPat.ListGridRows.FindIndex(x=>((Patient)x.Tag).PatNum==_patient.PatNum);
		if(isSelectingFamily) {
			index=gridAcctPat.ListGridRows.Count-1;
		}
		//If the index is greater than the number of rows, it will return and not select anything.
		gridAcctPat.SetSelected(index,true);
		ToolBarMain.Buttons["Insurance"].Enabled=!isSelectingFamily;
	}

	private void FillGridAcctPatAsSuperFam() {
		gridAcctPat.BeginUpdate();
		gridAcctPat.Columns.Clear();
		//Grid is primarily used for switching patients, so only show Name instead of SuperFamilyGridCols display fields.
		var gridColumn=new GridColumn(Lan.g("gridAcctPat","Name"),280);//Default width in DisplayFields.GetDefaultList()
		gridAcctPat.Columns.Add(gridColumn);
		gridAcctPat.ListGridRows.Clear();
		if(_patient==null) {
			gridAcctPat.EndUpdate();
			return;
		}
		_listPatientsSuperFamilyGuarantors.Sort(SortPatientListBySuperFamily);
		_listPatientsSuperFamilyMembers.Sort(SortPatientListBySuperFamily);
		//Loop through each family within the super family.
		for(var i=0;i<_listPatientsSuperFamilyGuarantors.Count;i++) {
			var gridRow=new GridRow();
			var patientGuar=_listPatientsSuperFamilyGuarantors[i];
			gridRow.Tag=patientGuar;
			var strSuperFam=GetCellTextSuperFamName(patientGuar);
			gridRow.Cells.Add(strSuperFam);
			if(i==0) {
				gridRow.Cells[i].Bold=YN.Yes;
				gridRow.Cells[i].ColorText=Color.OrangeRed;
			}
			gridAcctPat.ListGridRows.Add(gridRow);
		}
		gridAcctPat.EndUpdate();
		for(var i=0;i<gridAcctPat.ListGridRows.Count;i++) {
			if(((Patient)gridAcctPat.ListGridRows[i].Tag).PatNum==_patient.Guarantor) {
				gridAcctPat.SetSelected(i,true);
				break;
			}
		}
	}

	private void FillPaymentPlans() {
		_patientPortionBalanceTotal=0;
		_showGridPayPlan=false;
		if(_patient==null) {
			return;
		}
		var table=_dataSetMain.Tables["payplan"];
		if(table.Rows.OfType<DataRow>().Count(x => SIn.Long(x["Guarantor"].ToString())==_patient.PatNum
		                                           || SIn.Long(x["PatNum"].ToString())==_patient.PatNum)==0 && !IsFamilySelected()) //if we are looking at the entire family, show all the payplans 
		{
			return;
		}
		var listPayPlanNums=table.Select().Select(x => SIn.Long(x["PayPlanNum"].ToString())).ToList();
		var listPayPlansOvercharged=PayPlans.GetOverChargedPayPlans(listPayPlanNums);
		//do not hide payment plans that still have a balance when not on v2
		if(!checkShowCompletePayPlans.Checked) { //Hide the payment plans grid if there are no payment plans currently visible.
			var existsOpenPayPlan=false;
			for(var i=0;i<table.Rows.Count;i++) { //for every payment plan
				if(DoShowPayPlan(checkShowCompletePayPlans.Checked,SIn.Bool(table.Rows[i]["IsClosed"].ToString()),
					   SIn.Double(table.Rows[i]["balance"].ToString())))
				{
					existsOpenPayPlan=true;
					break; //break
				}
			}
			if(!existsOpenPayPlan) {
				return;//no need to do anything else.
			}
		}
		_showGridPayPlan=true;
		gridPayPlan.BeginUpdate();
		gridPayPlan.Columns.Clear();
		var col=new GridColumn(Lan.g("TablePaymentPlans","Date"),65);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Guarantor"),85);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Patient"),85);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Type"),30,HorizontalAlignment.Center);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Category"),60,HorizontalAlignment.Center);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Principal"),60,HorizontalAlignment.Right);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Total Cost"),60,HorizontalAlignment.Right);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Paid"),40,HorizontalAlignment.Right);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","PrincPaid"),60,HorizontalAlignment.Right);
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","Balance"),60,HorizontalAlignment.Right);
		gridPayPlan.Columns.Add(col);
		if(PrefC.GetBool(PrefName.PayPlanHideDueNow)) {
			col=new GridColumn("Closed",60,HorizontalAlignment.Center);
		}
		else {
			col=new GridColumn(Lan.g("TablePaymentPlans","Due Now"),60,HorizontalAlignment.Right);
		}
		gridPayPlan.Columns.Add(col);
		col=new GridColumn(Lan.g("TablePaymentPlans","eClipboard"),65,HorizontalAlignment.Center);
		gridPayPlan.Columns.Add(col);
		gridPayPlan.ListGridRows.Clear();
		GridRow row;
		GridCell cell;
		for(var i=0;i<table.Rows.Count;i++) {
			if(!DoShowPayPlan(checkShowCompletePayPlans.Checked,SIn.Bool(table.Rows[i]["IsClosed"].ToString()),
				   SIn.Double(table.Rows[i]["balance"].ToString())))
			{
				continue;//hide
			}
			row=new GridRow();
			row.Cells.Add(table.Rows[i]["date"].ToString());
			if(table.Rows[i]["InstallmentPlanNum"].ToString()!="0" && table.Rows[i]["PatNum"].ToString()!=_patient.Guarantor.ToString()) {//Installment plan and not on guar
				cell=new GridCell("Invalid Guarantor");
				cell.Bold=YN.Yes;
				cell.ColorText=Color.Red;
			}
			else {
				var payPlanNum=SIn.Long(table.Rows[i]["PayPlanNum"].ToString());
				var payPlan=PayPlans.GetOne(payPlanNum);
				cell=new GridCell("");
				//Installment Plans have no payPlanNum so we must skip this next if-statement if we do not receive one.
				if(payPlan?.PlanNum==0) {//Only test via PlanNum for Insurance PayPlans as per RPPayPlan.cs' logic.
					cell=new GridCell(table.Rows[i]["guarantor"].ToString());//If not an Insurance PayPlan set guarantor as per usual.
				}
			}
			row.Cells.Add(cell);
			row.Cells.Add(table.Rows[i]["patient"].ToString());
			row.Cells.Add(table.Rows[i]["type"].ToString());
			var planCategory=SIn.Long(table.Rows[i]["PlanCategory"].ToString());
			if(planCategory==0) {
				row.Cells.Add(Lan.g(this,"None"));
			}
			else {
				row.Cells.Add(Defs.GetDef(DefCat.PayPlanCategories,planCategory).ItemName);
			}
			row.Cells.Add(table.Rows[i]["principal"].ToString());
			row.Cells.Add(table.Rows[i]["totalCost"].ToString());
			row.Cells.Add(table.Rows[i]["paid"].ToString());
			row.Cells.Add(table.Rows[i]["princPaid"].ToString());
			row.Cells.Add(table.Rows[i]["balance"].ToString());
			if(table.Rows[i]["IsClosed"].ToString()=="1") {
				cell=new GridCell(Lan.g(this,"Closed"));
				row.ColorText=Color.Gray;
			}
			else if(PrefC.GetBool(PrefName.PayPlanHideDueNow)) {//pref can only be enabled when PayPlansVersion == 2.
				cell=new GridCell("");
			}
			else { //they aren't hiding the "Due Now" cell text.
				cell=new GridCell(table.Rows[i]["due"].ToString());
				//Only color the due now red and bold in version 1 and 3 of payplans.
				if(PrefC.GetInt(PrefName.PayPlansVersion).In((int)PayPlanVersions.DoNotAge,(int)PayPlanVersions.AgeCreditsOnly,(int)PayPlanVersions.NoCharges))
				{
					if(table.Rows[i]["type"].ToString()!="Ins") {
						cell.Bold=YN.Yes;
						cell.ColorText=Color.Red;
					}
				}
			}
			row.Cells.Add(cell);
			if(SIn.Long(table.Rows[i]["MobileAppDeviceNum"].ToString())>0) {
				row.Cells.Add("X");
			}
			else {
				row.Cells.Add("");
			}
			row.Tag=table.Rows[i];
			if(listPayPlansOvercharged.ConvertAll(x => x.PayPlanNum).Contains(SIn.Long(table.Rows[i]["PayPlanNum"].ToString()))) {
				row.ColorBackG=Color.FromArgb(255,255,128);
			}
			gridPayPlan.ListGridRows.Add(row);
			_patientPortionBalanceTotal+=Convert.ToDecimal(SIn.Double(table.Rows[i]["balance"].ToString()));
		}
		gridPayPlan.EndUpdate();
	}

		
	private void FillRepeatCharges() {
		_showGridRepeating=false;
		if(_patient==null) {
			return;
		}
		_repeatChargeArray=_loadData.ArrRepeatCharges;
		if(_repeatChargeArray.Length==0) {
			return;
		}
		if(PrefC.GetBool(PrefName.BillingUseBillingCycleDay)) {
			gridRepeat.Title=Lan.g(gridRepeat,"Repeat Charges")+" - Billing Day "+_patient.BillingCycleDay;
		}
		else {
			gridRepeat.Title=Lan.g(gridRepeat,"Repeat Charges");
		}
		_showGridRepeating=true;
		gridRepeat.BeginUpdate();
		gridRepeat.Columns.Clear();
		var col=new GridColumn(Lan.g("TableRepeatCharges","Description"),150);
		gridRepeat.Columns.Add(col);
		col=new GridColumn(Lan.g("TableRepeatCharges","Amount"),60,HorizontalAlignment.Right);
		gridRepeat.Columns.Add(col);
		col=new GridColumn(Lan.g("TableRepeatCharges","Start Date"),70,HorizontalAlignment.Center);
		gridRepeat.Columns.Add(col);
		col=new GridColumn(Lan.g("TableRepeatCharges","Stop Date"),70,HorizontalAlignment.Center);
		gridRepeat.Columns.Add(col);
		col=new GridColumn(Lan.g("TableRepeatCharges","Enabled"),55,HorizontalAlignment.Center);
		gridRepeat.Columns.Add(col);
		col=new GridColumn(Lan.g("TableRepeatCharges","Note"),355);
		gridRepeat.Columns.Add(col);
		gridRepeat.ListGridRows.Clear();
		GridRow row;
		ProcedureCode procedureCode;
		for(var i=0;i<_repeatChargeArray.Length;i++) {
			row=new GridRow();
			procedureCode=ProcedureCodes.GetProcCode(_repeatChargeArray[i].ProcCode);
			row.Cells.Add(procedureCode.Descript);
			row.Cells.Add(_repeatChargeArray[i].ChargeAmt.ToString("F"));
			if(_repeatChargeArray[i].DateStart.Year>1880) {
				row.Cells.Add(_repeatChargeArray[i].DateStart.ToShortDateString());
			}
			else {
				row.Cells.Add("");
			}
			if(_repeatChargeArray[i].DateStop.Year>1880) {
				row.Cells.Add(_repeatChargeArray[i].DateStop.ToShortDateString());
			}
			else {
				row.Cells.Add("");
			}
			if(_repeatChargeArray[i].IsEnabled) {
				row.Cells.Add("X");
			}
			else {
				row.Cells.Add("");
			}
			var note="";
			if(!string.IsNullOrEmpty(_repeatChargeArray[i].Npi)) {
				note+="NPI="+_repeatChargeArray[i].Npi+" ";
			}
			if(!string.IsNullOrEmpty(_repeatChargeArray[i].ErxAccountId)) {
				note+="ErxAccountId="+_repeatChargeArray[i].ErxAccountId+" ";
			}
			if(!string.IsNullOrEmpty(_repeatChargeArray[i].ProviderName)) {
				note+=_repeatChargeArray[i].ProviderName+" ";
			}
			note+=_repeatChargeArray[i].Note;
			row.Cells.Add(note);
			gridRepeat.ListGridRows.Add(row);
		}
		gridRepeat.EndUpdate();
	}

	private void FillSummary() {
		textFamPriMax.Text="";
		textFamPriDed.Text="";
		textFamSecMax.Text="";
		textFamSecDed.Text="";
		textPriMax.Text="";
		textPriDed.Text="";
		textPriDedRem.Text="";
		textPriUsed.Text="";
		textPriPend.Text="";
		textPriRem.Text="";
		textSecMax.Text="";
		textSecDed.Text="";
		textSecDedRem.Text="";
		textSecUsed.Text="";
		textSecPend.Text="";
		textSecRem.Text="";
		if(_patient==null) {
			return;
		}
		double maxFam=0;
		double maxInd=0;
		double ded=0;
		double dedFam=0;
		double dedRem=0;
		double remain=0;
		double pend=0;
		double used=0;
		InsPlan insPlan;
		InsSub insSub;
		var listInsSubs=_loadData.ListInsSubs;
		var listInsPlans=_loadData.ListInsPlans;
		var listPatPlans=_loadData.ListPatPlans;
		var listBenefits=_loadData.ListBenefits;
		var listClaims=_loadData.ListClaims;
		var listClaimProcHists=_loadData.HistList;
		if(listPatPlans.Count>0) {
			insSub=InsSubs.GetSub(listPatPlans[0].InsSubNum,listInsSubs);
			insPlan=InsPlans.GetPlan(insSub.PlanNum,listInsPlans);
			pend=InsPlans.GetPendingDisplay(listClaimProcHists,DateTime.Today,insPlan,listPatPlans[0].PatPlanNum,-1,_patient.PatNum,listPatPlans[0].InsSubNum,listBenefits);
			used=InsPlans.GetInsUsedDisplay(listClaimProcHists,DateTime.Today,insPlan.PlanNum,listPatPlans[0].PatPlanNum,-1,listInsPlans,listBenefits,_patient.PatNum,listPatPlans[0].InsSubNum);
			textPriPend.Text=pend.ToString("F");
			textPriUsed.Text=used.ToString("F");
			maxFam=Benefits.GetAnnualMaxDisplay(listBenefits,insPlan.PlanNum,listPatPlans[0].PatPlanNum,true);
			maxInd=Benefits.GetAnnualMaxDisplay(listBenefits,insPlan.PlanNum,listPatPlans[0].PatPlanNum,false);
			if(maxFam==-1) {
				textFamPriMax.Text="";
			}
			else {
				textFamPriMax.Text=maxFam.ToString("F");
			}
			if(maxInd==-1) {//if annual max is blank
				textPriMax.Text="";
				textPriRem.Text="";
			}
			else {
				remain=maxInd-used-pend;
				if(remain<0) {
					remain=0;
				}
				//textFamPriMax.Text=max.ToString("F");
				textPriMax.Text=maxInd.ToString("F");
				textPriRem.Text=remain.ToString("F");
			}
			//deductible:
			ded=Benefits.GetDeductGeneralDisplay(listBenefits,insPlan.PlanNum,listPatPlans[0].PatPlanNum,BenefitCoverageLevel.Individual);
			dedFam=Benefits.GetDeductGeneralDisplay(listBenefits,insPlan.PlanNum,listPatPlans[0].PatPlanNum,BenefitCoverageLevel.Family);
			if(ded!=-1) {
				textPriDed.Text=ded.ToString("F");
				dedRem=InsPlans.GetDedRemainDisplay(listClaimProcHists,DateTime.Today,insPlan.PlanNum,listPatPlans[0].PatPlanNum,-1,listInsPlans,_patient.PatNum,ded,dedFam);
				textPriDedRem.Text=dedRem.ToString("F");
			}
			if(dedFam!=-1) {
				textFamPriDed.Text=dedFam.ToString("F");
			}
		}
		if(listPatPlans.Count>1) {
			insSub=InsSubs.GetSub(listPatPlans[1].InsSubNum,listInsSubs);
			insPlan=InsPlans.GetPlan(insSub.PlanNum,listInsPlans);
			pend=InsPlans.GetPendingDisplay(listClaimProcHists,DateTime.Today,insPlan,listPatPlans[1].PatPlanNum,-1,_patient.PatNum,listPatPlans[1].InsSubNum,listBenefits);
			textSecPend.Text=pend.ToString("F");
			used=InsPlans.GetInsUsedDisplay(listClaimProcHists,DateTime.Today,insPlan.PlanNum,listPatPlans[1].PatPlanNum,-1,listInsPlans,listBenefits,_patient.PatNum,listPatPlans[1].InsSubNum);
			textSecUsed.Text=used.ToString("F");
			//max=Benefits.GetAnnualMaxDisplay(BenefitList,PlanCur.PlanNum,PatPlanList[1].PatPlanNum);
			maxFam=Benefits.GetAnnualMaxDisplay(listBenefits,insPlan.PlanNum,listPatPlans[1].PatPlanNum,true);
			maxInd=Benefits.GetAnnualMaxDisplay(listBenefits,insPlan.PlanNum,listPatPlans[1].PatPlanNum,false);
			if(maxFam==-1) {
				textFamSecMax.Text="";
			}
			else {
				textFamSecMax.Text=maxFam.ToString("F");
			}
			if(maxInd==-1) {//if annual max is blank
				textSecMax.Text="";
				textSecRem.Text="";
			}
			else {
				remain=maxInd-used-pend;
				if(remain<0) {
					remain=0;
				}
				//textFamSecMax.Text=max.ToString("F");
				textSecMax.Text=maxInd.ToString("F");
				textSecRem.Text=remain.ToString("F");
			}
			//deductible:
			ded=Benefits.GetDeductGeneralDisplay(listBenefits,insPlan.PlanNum,listPatPlans[1].PatPlanNum,BenefitCoverageLevel.Individual);
			dedFam=Benefits.GetDeductGeneralDisplay(listBenefits,insPlan.PlanNum,listPatPlans[1].PatPlanNum,BenefitCoverageLevel.Family);
			if(ded!=-1) {
				textSecDed.Text=ded.ToString("F");
				dedRem=InsPlans.GetDedRemainDisplay(listClaimProcHists,DateTime.Today,insPlan.PlanNum,listPatPlans[1].PatPlanNum,-1,listInsPlans,_patient.PatNum,ded,dedFam);
				textSecDedRem.Text=dedRem.ToString("F");
			}
			if(dedFam!=-1) {
				textFamSecDed.Text=dedFam.ToString("F");
			}
		}
	}

	///<summary>Show the splits that are flagged as being hidden. </summary>
	private void FillTpUnearned() {
		if(_patient==null) {
			return;
		}
		if(_listPaySplitsHidden.Count==0) {
			return;
		}
		var listProceduresForHiddenSplits=Procedures.GetManyProc(_listPaySplitsHidden.ConvertAll(x => x.ProcNum),includeNote:false);
		gridTpSplits.BeginUpdate();
		gridTpSplits.Columns.Clear();
		var col=new GridColumn(Lan.g("TableTpUnearned","Date"),65);
		gridTpSplits.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTpUnearned","Patient"),150);
		gridTpSplits.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTpUnearned","Provider"),70);
		gridTpSplits.Columns.Add(col);
		if(true) {
			col=new GridColumn(Lan.g("TableTpUnearned","Clinic"),60);
			gridTpSplits.Columns.Add(col);
		}
		col=new GridColumn(Lan.g("TableTpUnearned","Code"),80);
		gridTpSplits.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTpUnearned","Description"),180);
		gridTpSplits.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTpUnearned","Amount"),60,HorizontalAlignment.Right);
		gridTpSplits.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTpUnearned","Total"),60,HorizontalAlignment.Right);
		gridTpSplits.Columns.Add(col);
		gridTpSplits.ListGridRows.Clear();
		var listPatientsOtherFams=new List<Patient>();
		double tpRunningTotal=0;
		for(var i=0;i<_listPaySplitsHidden.Count;i++) {
			var row=new GridRow();
			row.Cells.Add(_listPaySplitsHidden[i].DatePay.ToShortDateString());//Date
			var patientForSplit=_loadData.Fam.ListPats.ToList().Find(x => x.PatNum==_listPaySplitsHidden[i].PatNum);
			patientForSplit??=listPatientsOtherFams.Find(x => x.PatNum==_listPaySplitsHidden[i].PatNum);
			patientForSplit??=Patients.GetPat(_listPaySplitsHidden[i].PatNum);
			row.Cells.Add(patientForSplit.LName+", "+patientForSplit.FName);//Patient
			row.Cells.Add(Providers.GetAbbr(_listPaySplitsHidden[i].ProvNum));//Provider
			if(true) {
				row.Cells.Add(Clinics.GetAbbr(_listPaySplitsHidden[i].ClinicNum));//Clinics
			}
			var codeNum=listProceduresForHiddenSplits.Find(x => x.ProcNum==_listPaySplitsHidden[i].ProcNum)?.CodeNum??0;
			var procedureCode=ProcedureCodes.GetFirstOrDefault(x => x.CodeNum==codeNum);
			var paymentType=Defs.GetName(DefCat.PaymentTypes,Payments.GetPayment(_listPaySplitsHidden[i].PayNum).PayType);
			var dynamicPayPlanPrepaymentUnearnedType=PrefC.GetLong(PrefName.DynamicPayPlanPrepaymentUnearnedType);
			if(_listPaySplitsHidden[i].UnearnedType==dynamicPayPlanPrepaymentUnearnedType && _listPaySplitsHidden[i].PayPlanNum!=0) {
				paymentType+=" (Attached to Payment Plan)";
			}
			if(procedureCode!=null) {
				row.Cells.Add(procedureCode.ProcCode);//Code
				row.Cells.Add(paymentType+", "+procedureCode.Descript);//Description
			}
			else {
				row.Cells.Add("");//Code
				row.Cells.Add(paymentType);//Description
			}
			row.Cells.Add(_listPaySplitsHidden[i].SplitAmt.ToString("F"));//Amount
			row.Cells.Add((tpRunningTotal+=_listPaySplitsHidden[i].SplitAmt).ToString("F"));//Total
			row.Tag=_listPaySplitsHidden[i];
			var color=Defs.GetDefsForCategory(DefCat.AccountColors)[3].ItemColor;
			row.ColorLborder=color;
			row.ColorText=color;
			gridTpSplits.ListGridRows.Add(row);
		}
		gridTpSplits.EndUpdate();
		if(_scrollValueWhenDoubleClickTpUnearned==-1) {
			gridTpSplits.ScrollToEnd();
			return;
		}
		gridTpSplits.ScrollValue=_scrollValueWhenDoubleClickTpUnearned;
		_scrollValueWhenDoubleClickTpUnearned=-1;
	}
	#endregion Methods - Private Fill

	#region Methods - Private Other
	///<summary>Validated the procedure code using FormProcEdit and prompts user for input if required.</summary>
	private bool AddProcAndValidate(string procString,ProviderDto provider) {
		var procedurecode=ProcedureCodes.GetProcCode(procString);
		if(procedurecode.CodeNum==0) {
			MsgBox.Show(Lan.g(this,"Invalid Procedure Code:")+" "+procString);
			return false; //Invalid ProcCode string manually entered.
		}
		if(ProcedureCodes.AreAnyProcCodesHidden(procedurecode.CodeNum)) {
			MsgBox.Show($"{Lan.g(this,"Cannot add procedure because it is in a hidden category")}: {procedurecode.ProcCode}");
			return false;
		}
		var procedure=new Procedure();
		procedure.ProcStatus=ProcStat.C;
		procedure.ClinicNum=_patient.ClinicNum;
		procedure.CodeNum=procedurecode.CodeNum;
		procedure.DateEntryC=DateTime.Now;
		procedure.DateTP=DateTime.Now;
		procedure.PatNum=_patient.PatNum;
		procedure.ProcDate=DateTime.Now;
		procedure.DateComplete=DateTime.Now;
		procedure.ToothRange="";
		procedure.PlaceService=Clinics.GetPlaceService(procedure.ClinicNum);
		if(!PrefC.GetBool(PrefName.EasyHidePublicHealth)) {
			procedure.SiteNum=_patient.SiteNum;
		}
		procedure.ProvNum=procedurecode.ProvNumDefault;//use proc default prov if set
		if(procedure.ProvNum==0) { //if none set, use primary provider.
			procedure.ProvNum=provider.Id;
		}
		var listInsSubs=InsSubs.RefreshForFam(_family);
		var listInsPlans=InsPlans.RefreshForSubList(listInsSubs);
		var listPatPlans=PatPlans.Refresh(_patient.PatNum);
		procedure.MedicalCode=procedurecode.MedicalCode;
		procedure.ProcFee=Procedures.GetProcFee(_patient,listPatPlans,listInsSubs,listInsPlans,procedure);
		procedure.UnitQty=1;
		//Find out if we are going to link the procedure to an ortho case.
		var orthoCaseProcedureLinker=OrthoCaseProcedureLinker.CreateOneForPatient(procedure.PatNum);
		procedure.ProcStatus=ProcStat.D;
		var shouldProcedureLinkToOrthoCase=orthoCaseProcedureLinker.ShouldProcedureLinkToOrthoCase(procedure,procedurecode.ProcCode);
		Procedures.Insert(procedure,skipDiscountPlanAdjustment:shouldProcedureLinkToOrthoCase);
		procedure.ProcStatus=ProcStat.C;
		Procedures.SetOrthoProcComplete(procedure,procedurecode);//does nothing if not an ortho proc
		orthoCaseProcedureLinker.LinkProcedureToActiveOrthoCaseIfNeeded(procedure,doUpdateProcedure:true);
		//launch form silently to validate code. If entry errors occur the form will be shown to user, otherwise it will close immediately.
		var listClaimProcHistsLoop=new List<ClaimProcHist>();
		using var formProcEdit=new FormProcEdit(procedure,_patient,_family,true);
		formProcEdit.ListClaimProcHists=_loadData.HistList;
		formProcEdit.ListClaimProcHistsLoop=listClaimProcHistsLoop;
		formProcEdit.IsNew=true;
		formProcEdit.ShowDialog();
		if(formProcEdit.DialogResult!=DialogResult.OK) {
			Procedures.Delete(procedure.ProcNum);
			return false;
		}
		if(procedure.ProcStatus==ProcStat.C) {
			AutomationL.Trigger(EnumAutomationTrigger.ProcedureComplete, [ProcedureCodes.GetStringProcCode(procedure.CodeNum)],_patient.PatNum);
			Procedures.AfterProcsSetComplete([procedure]);
		}
		return true;
	}

	///<summary>For one patient in HQ db with thousands of big email rows, it was taking 9 seconds to refresh. This reduces it to 1 second.  A better fix would be for the grid to be internally faster for this scenario.</summary>
	public void LayoutPanelsAndRefreshMainGrids(bool doLogFillMain=false) {
		gridAccount.ListGridRows.Clear();
		gridComm.ListGridRows.Clear();
		LayoutPanels();
		if(doLogFillMain) {//Only log this fill on the refresh method call
			FillMain();
		}
		else {
			FillMain();
		}
		FillComm();
	}

	///<summary>This used to be a layout event, but that was making it get called far too frequently.  Now, this must explicitly and intelligently be called.</summary>
	public void LayoutPanels() {
		textUrgFinNote.Font=new Font(FontFamily.GenericSansSerif,8.25f);
		//splitContainerParent
		//  Panel1: splitContainerRepChargesPP
		//    Panel1: gridRepeat
		//    Panel2: gridPayPlan
		//  Panel2: splitContainerAccountCommLog
		//    Panel1: tabControlAccount
		//      tabPagePatAccount: gridAccount
		//			tabPageAutoOrtho: gridAutoOrtho
		//      tabPageOrthoCases: gridOrthoCases
		//      tabPageHiddenSplits: gridTpSplits
		//    Panel2: 
		//      gridComm
		splitContainerParent.Location=new Point(0,63);
		splitContainerParent.Size=new Size(tabControlShow.Left-1,Height-splitContainerParent.Top-1);
		//If the two top grids are not visible, collapse the entire parent panel 1 so it does not show extra white space.
		splitContainerParent.Panel1Collapsed=!_showGridPayPlan && !_showGridRepeating;
		if(!splitContainerParent.Panel1Collapsed) {
			if(_showGridRepeating) {
				splitContainerRepChargesPP.Panel1Collapsed=false;
			}
			else {
				splitContainerRepChargesPP.Panel1Collapsed=true;
				splitContainerParent.Panel1MinSize=20;
			}
			if(_showGridPayPlan) {
				splitContainerRepChargesPP.Panel2Collapsed=false;
			}
			else{
				splitContainerRepChargesPP.Panel2Collapsed=true;
				splitContainerParent.Panel1MinSize=20;
			}
		}
		//If both visible, make sure the minimum size is set back to orignal value.
		if(_showGridPayPlan && _showGridRepeating) {
			splitContainerParent.Panel1MinSize=45;
		}
		if(gridAccount.HScrollVisible) {
			splitContainerParent.Panel2MinSize=85;//85px is the height needed for the account grid and the commlog grid.
			splitContainerAccountCommLog.Panel1MinSize=60;//60px is the height needed for the tabs, the grid title, and the horizontal scrollbar.
		}
		else{
			splitContainerParent.Panel2MinSize=85-gridAccount.HScrollHeight;
			splitContainerAccountCommLog.Panel1MinSize=60-gridAccount.HScrollHeight;
		}
		gridPatInfo.Visible=_listDisplayFieldsPatInfo?.Count!=0;
		//only show the auto ortho grid and tab control if they have the show feature enabled.
		//otherwise, hide the tabs and re-size the account grid.
		if(!PrefC.GetBool(PrefName.OrthoEnabled)) {
			tabControlAccount.TabPages.Remove(tabPageAutoOrtho);
		}
		else if(!tabControlAccount.TabPages.Contains(tabPageAutoOrtho)) {
			tabControlAccount.Controls.Add(tabPageAutoOrtho);
		}
		if(!OrthoCases.HasOrthoCasesEnabled()) {
			tabControlAccount.TabPages.Remove(tabPageOrthoCases);
		}
		else if(!tabControlAccount.TabPages.Contains(tabPageOrthoCases)) {
			tabControlAccount.Controls.Add(tabPageOrthoCases);
		}
		if(_listPaySplitsHidden.Count==0) {//might need to get updated more often than from loadData. Not sure how much we care. 
			tabControlAccount.TabPages.Remove(tabPageHiddenSplits);
		}
		else{
			if(!tabControlAccount.TabPages.Contains(tabPageHiddenSplits)) {
				tabControlAccount.Controls.Add(tabPageHiddenSplits);
			}
			var listPaySplits=gridTpSplits.GetTags<PaySplit>();
			var totPaySplitAmount=listPaySplits.Sum(x => x.SplitAmt);
			if(CompareDouble.IsZero(totPaySplitAmount)) {
				tabPageHiddenSplits.ColorTab=Color.Empty;
			}
			else{
				tabPageHiddenSplits.ColorTab=Color.LightCoral;//make the tab red if hidden splits do not total $0
			}
		}
		if(tabControlAccount.TabPages.Contains(tabPageAutoOrtho)
		   || tabControlAccount.TabPages.Contains(tabPageHiddenSplits)
		   || tabControlAccount.TabPages.Contains(tabPageOrthoCases))
		{//if any additional tabs present besides main one
			tabControlAccount.TabsAreCollapsed=false;
			return;
		}
		tabControlAccount.TabsAreCollapsed=true;
	}

	///<summary>Returns true if the payment plan should be displayed.</summary>
	private bool DoShowPayPlan(bool doShowCompletedPlans,bool isClosed,double balance) {
		if(doShowCompletedPlans) {
			return true;
		}
		//do not hide payment plans that still have a balance
		return !isClosed || !CompareDouble.IsEqual(balance,0);
	}

	private string GetCellTextSuperFamName(Patient patientGuar) {
		//Make a string that displays all the names of the family members.
		//Always start with the guarantor followed by the rest of the family.
		var stringReturn = patientGuar.GetNameLF();
		//From here down, we are adding family members to the string
		//when the family is currently selected in the superfam grid.
		for(var i = 0;i<_listPatientsSuperFamilyMembers.Count;i++) {
			if(_listPatientsSuperFamilyMembers[i].Guarantor!=patientGuar.Guarantor) {
				continue; //Not part of patientGuar's family.
			}
			if(_listPatientsSuperFamilyMembers[i].PatNum==patientGuar.PatNum) {
				continue; //Guarantor is already in the string.
			}
			if(PatientLinks.WasPatientMerged(_listPatientsSuperFamilyMembers[i].PatNum,_loadData.ListMergeLinks)) {
				continue; //Patient was merged into another patient, we only want the 'other' patient.
			}
			stringReturn+="\r\n   "+StringTools.Truncate(_listPatientsSuperFamilyMembers[i].GetNameLF(),40,true);
		}
		return stringReturn;
	}

	///<summary>Returns a list of CreateClaimItems comprised from the selected items within gridAccount.
	///If no rows are currently selected then the list returned will be comprised of all items within the "account" table in the DataSet.</summary>
	private List<CreateClaimItem> GetCreateClaimItemsFromUI() {
		//There have been reports of concurrency issues so make a deep copy of the selected indices and the table first to help alleviate the problem.
		//See task #830623 and task #1266253 for more details.
		var intArraySelectedIndices=(int[])gridAccount.SelectedIndices.Clone();
		var table=GetTableFromDataSet("account");
		var listCreateClaimItems=ClaimL.GetCreateClaimItems(table,intArraySelectedIndices);
		if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
			//We do not want to consider Canadian lab procs to be selected.  If we do, these lab procs will later cause the corresponding lab ClaimProcs to 
			//be included in the Claim's list of ClaimProcs, which will then cause the ClaimProcs for the labs to get a LineNumber, which will in turn cause
			//the EOB Importer to fail because the LineNumbers in the database's list of ClaimProcs no longer match the EOB LineNumbers.
			listCreateClaimItems.RemoveAll(x => x.ProcNumLab!=0);
		}
		return listCreateClaimItems;
	}

	private string GetPatNameFromTable(DataTable table,int index) {
		var name=table.Rows[index]["name"].ToString();
		if(PrefC.GetBool(PrefName.TitleBarShowSpecialty) && string.Compare(name,"Entire Family",true)!=0) {
			var patNum=SIn.Long(table.Rows[index]["PatNum"].ToString());
			var specialty=Patients.GetPatientSpecialtyDef(patNum)?.ItemName??"";
			name+=string.IsNullOrWhiteSpace(specialty)?"":"\r\n"+specialty;
		}
		return name;
	}

	///<summary>Returns a deep copy of the corresponding table from the main data set.
	///Utilizes a lock object that is partially implemented in an attempt to fix an error when invoking DataTable.Clone()</summary>
	private DataTable GetTableFromDataSet(string tableName) {
		DataTable table;
		lock(_lockDataSetMain) {
			table=_dataSetMain.Tables[tableName].Clone();
			for(var i=0;i<_dataSetMain.Tables[tableName].Rows.Count;i++) {
				table.ImportRow(_dataSetMain.Tables[tableName].Rows[i]);
			}
		}
		return table;
	}

	/// <summary>Saves the statement.  Attaches a pdf to it by creating a doc object.  Prints it or emails it.  </summary>
	private void PrintStatement(Statement statement) {
		Cursor=Cursors.WaitCursor;
		var dataSet=AccountModules.GetAccount(statement.PatNum,statement,doShowHiddenPaySplits:statement.IsReceipt);
		Statements.CalcBalTotalInsEst(statement,dataSet);
		Statements.Insert(statement);
		var sheetDef=SheetUtil.GetStatementSheetDef(statement);
		var sheet=SheetUtil.CreateSheet(sheetDef,statement.PatNum,statement.HidePayment);
		sheet.Parameters.Add(new SheetParameter(true,"Statement") { ParamValue=statement });
		SheetFiller.FillFields(sheet,dataSet,statement);
		SheetUtil.CalculateHeights(sheet,dataSet,statement);
		var tempPath=CodeBase.ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(),statement.PatNum+".pdf");
		SheetPrinting.CreatePdf(sheet,tempPath,statement,dataSet:dataSet);
		long category=0;
		var listDefs=Defs.GetDefsForCategory(DefCat.ImageCats,true);
		var wasStatementDeleted=false;
		for(var i=0;i<listDefs.Count;i++) {
			if(Regex.IsMatch(listDefs[i].ItemValue,"S")) {
				category=listDefs[i].DefNum;
				break;
			}
		}
		if(category==0) {
			category=listDefs[0].DefNum;//put it in the first category.
		}
		//create doc--------------------------------------------------------------------------------------
		Document document=null;
		try {
			document=ImageStore.Import(tempPath,category,Patients.GetPat(statement.PatNum));
		}
		catch {
			this.Cursor=Cursors.Default;
			MsgBox.Show(this,"Error saving document.");
			return;
		}
		document.ImgType=ImageType.Document;
		if(statement.IsInvoice) {
			document.Description=Lan.g(this,"Invoice");
		}
		else {
			if(statement.IsReceipt) {
				document.Description=Lan.g(this,"Receipt");
			}
			else {
				document.Description=Lan.g(this,"Statement");
			}
		}
		statement.DateSent=document.DateCreated;
		statement.DocNum=document.DocNum;//this signals the calling class that the pdf was created successfully.
		Statements.AttachDoc(statement.StatementNum,document);
		//if(ImageStore.UpdatePatient == null) {
		//	ImageStore.UpdatePatient = new FileStore.UpdatePatientDelegate(Patients.Update);
		//}
		var patientGuarantor=Patients.GetPat(statement.PatNum);
		var guarFolder=ImageStore.GetPatientFolder(patientGuarantor,ImageStore.GetDataFolder());
		//OpenDental.Imaging.ImageStoreBase imageStore = OpenDental.Imaging.ImageStore.GetImageStore(guar);
		if(statement.Mode_==StatementMode.Email) {
			if(!Security.IsAuthorized(EnumPermType.EmailSend)) {
				Cursor=Cursors.Default;
				return;
			}
			var emailMessage=EmailMessages.CreateEmailMessageForStatement(statement,patientGuarantor,guarFolder);
			using var formEmailMessageEdit=new FormEmailMessageEdit(emailMessage,EmailAddresses.GetByClinic(patientGuarantor.ClinicNum));
			formEmailMessageEdit.IsNew=true;
			formEmailMessageEdit.ShowDialog();
			//If user clicked delete or cancel, delete pdf and statement
			if(formEmailMessageEdit.DialogResult==DialogResult.Cancel) {
				Patient patient;
				string patFolder;
				if(statement.DocNum!=0) {
					//delete the pdf
					patient=Patients.GetPat(statement.PatNum);
					patFolder=ImageStore.GetPatientFolder(patient,ImageStore.GetDataFolder());
					var listDocuments=new List<Document>();
					listDocuments.Add(Documents.GetByNum(statement.DocNum));
					try {
						ImageStore.DeleteDocuments(listDocuments,patFolder);
					}
					catch {  //Image could not be deleted, in use.
						//This should never get hit because the file was created by this user within this method.  
						//If the doc cannot be deleted, then we will not stop them, they will have to manually delete it from the images module.
					}
				}
				//delete statement
				Statements.Delete(statement);
				wasStatementDeleted=true;
			}
		}
		else
		{
			sheet=SheetUtil.CreateSheet(sheetDef,statement.PatNum,statement.HidePayment);
			SheetFiller.FillFields(sheet,dataSet,statement);
			SheetUtil.CalculateHeights(sheet,dataSet,statement);
			SheetPrinting.Print(sheet,1,false,statement);//use GDI+ printing, which is slightly different than the pdf.
		}
		if(!wasStatementDeleted) {
			Statements.SyncStatementProdsForStatement(dataSet,statement.StatementNum,statement.DocNum);
		}
		Cursor=Cursors.Default;
	}

	private int SortPatientListBySuperFamily(Patient patient1,Patient patient2) {
		if(patient1.PatNum==patient2.PatNum) {
			return 0;
		}
		if(patient1.PatNum==patient1.SuperFamily) {//Superheads always go to the top no matter what.
			return -1;
		}
		if(patient2.PatNum==patient2.SuperFamily) {
			return 1;
		}
		//Sort Inactive patients to bottom of list.
		if(patient1.PatStatus!=patient2.PatStatus) {
			if(patient1.PatStatus==PatientStatus.Inactive) {
				return 1;
			}
			if(patient2.PatStatus==PatientStatus.Inactive) {
				return -1;
			}
		}
		//The sorting below happens when they are both active or both inactive.
		return patient1.GetNameLF().CompareTo(patient2.GetNameLF());
	}

	#endregion Methods - Private Other

	#region Methods - Helpers
	///<summary>If the user selects multiple procedures (validated) then we pass the selected procedures to FormMultiAdj. Otherwise if the user
	///selects one procedure (not validated) we maintain the previous functionality of opening FormAdjust.</summary>
	private void AddAdjustmentToSelectedProcsHelper(bool openMultiAdj=false) {
		var isTsiAdj=TsiTransLogs.IsTransworldEnabled(_patient.ClinicNum)
		             && Patients.IsGuarCollections(_patient.Guarantor)
		             && !MsgBox.Show(this,MsgBoxButtons.YesNo,"The guarantor of this family has been sent to TSI for a past due balance.  "
		                                                      +"Is this an adjustment applied by the office?\r\n\r\n"
		                                                      +"Yes - this is an adjustment applied by the office\r\n\r\n"
		                                                      +"No - this adjustment is the result of a payment received from TSI");
		var tableAcct=_dataSetMain.Tables["account"];
		var listProcNumsSelected=new List<long>();
		for(var i=0;i<gridAccount.SelectedIndices.Length;i++) {
			var procNum=SIn.Long(tableAcct.Rows[gridAccount.SelectedIndices[i]]["ProcNum"].ToString());
			if(procNum==0) {
				MsgBox.Show(this,"You can only select procedures.");
				return;
			}
			listProcNumsSelected.Add(procNum);
		}
		//If the user selected multiple procedures or clicked the Add Multi Adj button then open FormMultiAdj.
		if(listProcNumsSelected.Count>1 || openMultiAdj) {
			//Open the form with only the selected procedures
			using var formAdjMulti=new FormAdjMulti(_patient,listProcNumsSelected);
			formAdjMulti.ShowDialog();
		}
		else {
			var patient=_patient;
			var adjustment=new Adjustment();
			adjustment.DateEntry=DateTime.Today;//cannot be changed. Handled automatically
			adjustment.AdjDate=DateTime.Today;
			adjustment.ProcDate=DateTime.Today;
			adjustment.ProvNum=_patient.PriProv;
			adjustment.PatNum=_patient.PatNum;
			adjustment.ClinicNum=_patient.ClinicNum;
			if(gridAccount.SelectedGridRows.Count==1) {
				var orthoProcLink=OrthoProcLinks.GetByProcNum(SIn.Long(tableAcct.Rows[gridAccount.SelectedIndices[0]]["ProcNum"].ToString()));
				if(orthoProcLink!=null) {
					MsgBox.Show(this,"Procedures linked to ortho cases cannot be adjusted.");
					return;
				}
				var procNum=SIn.Long(tableAcct.Rows[gridAccount.SelectedIndices[0]]["ProcNum"].ToString());
				var procedure=Procedures.GetOneProc(procNum,false);
				if(!Security.IsAuthorized(EnumPermType.ProcCompleteAddAdj,Procedures.GetDateForPermCheck(procedure))) {
					return;
				}
				adjustment.ProcNum=procNum;
				if(procedure!=null) {
					adjustment.ProvNum=procedure.ProvNum;
					adjustment.ClinicNum=procedure.ClinicNum;
					adjustment.PatNum=procedure.PatNum;
					if(adjustment.PatNum!=_patient.PatNum) {
						patient=_family.GetPatient(adjustment.PatNum)??Patients.GetPat(adjustment.PatNum);
					}
				}
			}
			using var formAdjust=new FormAdjust(patient,adjustment,isTsiAdj);
			formAdjust.IsNew=true;
			formAdjust.ShowDialog();
			//Shared.ComputeBalances();
		}
		ModuleSelected(_patient.PatNum);
	}

	/// <summary>Return the clinic specific ClearningHouse based on the claim the user clicked on. Can return null.</summary>
	private Clearinghouse GetClearingHouseForClaim() {
		var table=_dataSetMain.Tables["account"];
		//Guaranteed to be exactly one claim selected (among possible other selections) when called from contextMenuAcctGrid_Popup()
		var idxClaimSelected=gridAccount.SelectedIndices.ToList().Find(x => table.Rows[x]["ClaimNum"].ToString()!="0");
		var claimNum=SIn.Long(table.Rows[idxClaimSelected]["ClaimNum"].ToString());
		var claim=Claims.GetClaim(claimNum);
		//Finding the clearing settings for the selected claim's clinic is from ClaimConnect.cs GetClearingHouseForClaim(). This method is private so the code was copied.
		var insPlan=InsPlans.GetPlan(claim.PlanNum,null);
		if(insPlan==null) {
			return null;
		}
		var carrier=Carriers.GetCarrier(insPlan.CarrierNum);
		if(carrier==null) {
			return null;
		}
		if(carrier.ElectID.Length<2) {
			return null;
		}
		//Fill clearing house with HQ fields
		var clearingHouseNum=Clearinghouses.AutomateClearinghouseHqSelection(carrier.ElectID,claim.MedType);
		var clearingHouse=Clearinghouses.GetClearinghouse(clearingHouseNum);
		//Refill clearingHouse with clinic specific fields
		return Clearinghouses.OverrideFields(clearingHouse,claim.ClinicNum);
	}

	///<summary>Returns true if XCharge or PayConnect payments are allowed to be made for the currently selected clinic.</summary>
	private static bool IsWebPaymentsEnabled() {
		return IsXWebPaymentsEnabled() || IsPayConnectPaymentsEnabled();
	}

	private static bool IsXWebPaymentsEnabled() {
		WebPaymentProperties webPaymentProperties;
		try {
			ProgramProperties.GetXWebCreds(Clinics.ClinicNum,out webPaymentProperties);
		}
		catch {
			return false;
		}
		return webPaymentProperties.IsPaymentsAllowed;
	}

	private static bool IsPayConnectPaymentsEnabled() {
		var webPaymentProperties=new PayConnect.WebPaymentProperties();
		try {
			ProgramProperties.GetPayConnectPatPortalCreds(Clinics.ClinicNum,out webPaymentProperties);
		}
		catch {
			return false;
		}
		return webPaymentProperties.IsPaymentsAllowed;
	}

	private void PayPlanHelper(PayPlanModes payPlanMode) {
		if(!Security.IsAuthorized(EnumPermType.PayPlanEdit)) {
			return;
		}
		var isTsiPayplan=TsiTransLogs.IsTransworldEnabled(_family.Guarantor.ClinicNum) && Patients.IsGuarCollections(_patient.Guarantor,false);
		var msg="";
		if(isTsiPayplan) {
			if(!Security.IsAuthorized(EnumPermType.Billing,true)) {
				msg=Lan.g(this,"The guarantor of this family has been sent to TSI for a past due balance.")+"\r\n"
				                                                                                           +Lan.g(this,"Creating a payment plan for this guarantor would cause the account to be suspended in the TSI system but you are not "
				                                                                                                       +"authorized for")+"\r\n"
				                                                                                           +GroupPermissions.GetDesc(EnumPermType.Billing);
				ODMessageBox.Show(this,msg);
				return;
			}
			var billingType=Defs.GetName(DefCat.BillingTypes,PrefC.GetLong(PrefName.TransworldPaidInFullBillingType));
			msg=Lan.g(this,"The guarantor of this family has been sent to TSI for a past due balance.")+"\r\n"
			                                                                                           +Lan.g(this,"Creating this payment plan will suspend the TSI account for a maximum of 50 days if the account is in the Accelerator or "
			                                                                                                       +"Profit Recovery stage.")+"\r\n"
			                                                                                           +Lan.g(this,"Continue creating the payment plan?")+"\r\n\r\n"
			                                                                                           +Lan.g(this,"Yes - Create the payment plan, send a suspend message to TSI, and change the guarantor's billing type to")+" "
			                                                                                           +billingType+".\r\n\r\n"
			                                                                                           +Lan.g(this,"No - Do not create the payment plan and allow TSI to continue managing the account.");
			if(!MsgBox.Show(this,MsgBoxButtons.YesNo,msg)) {
				return;
			}
		}
		var payPlan=new PayPlan();
		payPlan.IsNew=true;
		payPlan.PatNum=_patient.PatNum;
		payPlan.Guarantor=_patient.Guarantor;
		if(payPlanMode.HasFlag(PayPlanModes.Insurance)) {
			payPlan.Guarantor=0;//Insurance PayPlans have no guarantor because they are charged to insurance
		}
		payPlan.PayPlanDate=DateTime.Today;
		payPlan.CompletedAmt=0;
		long patNumGoto=0;
		if(payPlanMode.HasFlag(PayPlanModes.Dynamic)) {
			payPlan.IsDynamic=true;
			payPlan.ChargeFrequency=PayPlanFrequency.Monthly;
			payPlan.PayPlanNum=PayPlans.Insert(payPlan);
			using var formPayPlanDynamic=new FormPayPlanDynamic(payPlan);
			formPayPlanDynamic.ShowDialog();
			patNumGoto=formPayPlanDynamic.PatNumGoto;
		}
		else {
			payPlan.PayPlanNum=PayPlans.Insert(payPlan);
			using var formPayPlan=new FormPayPlan(payPlan);
			formPayPlan.TotalAmt=_patient.EstBalance;
			formPayPlan.IsNew=true;
			formPayPlan.IsInsPayPlan=true;
			formPayPlan.ShowDialog();
			patNumGoto=formPayPlan.PatNumGoto;
		}
		if(patNumGoto!=0) {
			GlobalFormOpenDental.PatientSelected(Patients.GetPat(patNumGoto),false);
			ModuleSelected(patNumGoto);//switches to other patient.
		}
		else{
			ModuleSelected(_patient.PatNum);
		}
		if(isTsiPayplan && PayPlans.GetOne(payPlan.PayPlanNum)!=null) {
			msg=TsiTransLogs.SuspendGuar(_family.Guarantor);
			if(!string.IsNullOrEmpty(msg)) {
				ODMessageBox.Show(this,msg+"\r\n"+Lan.g(this,"The account will have to be suspended manually using the A/R Manager or the TSI web portal."));
			}
		}
	}

	///<summary>Centralize validation for the DXC right click options. Validation logic taken from the FormClaimEdit.cs OpenAttachmentForm method.</summary>
	private bool ValidateRightClickDXC(Claim claim) {
		if(claim.ClaimStatus=="W") {
			var claimSendQueueItemsArray=Claims.GetQueueList(claim.ClaimNum,claim.ClinicNum,0);
			if(!claimSendQueueItemsArray[0].CanSendElect) {
				MsgBox.Show(this,"Carrier is not set to Send Claims Electronically.");
				return false;
			}
			var clearinghouseHq=ClearinghouseL.GetClearinghouseHq(claimSendQueueItemsArray[0].ClearinghouseNum);
			var clearinghouseClin=Clearinghouses.OverrideFields(clearinghouseHq,Clinics.ClinicNum);
			claimSendQueueItemsArray[0]=Eclaims.GetMissingData(clearinghouseClin,claimSendQueueItemsArray[0]);
			if(claimSendQueueItemsArray[0].MissingData!="") {
				ODMessageBox.Show("Cannot add attachments until missing data is fixed:\r\n"+claimSendQueueItemsArray[0].MissingData);
				return false;
			}
		}
		return true;
	}

	#endregion Methods - Helpers

	#region Methods - Inactive
	//private void textCC_Leave(object sender,EventArgs e) {
	//  if(FamCur==null)
	//    return;
	//  if(CCChanged) {
	//    CCSave();
	//    CCChanged=false;
	//    ModuleSelected(PatCur.PatNum);
	//  }
	//}

	//private void textCCexp_Leave(object sender,EventArgs e) {
	//  if(FamCur==null)
	//    return;
	//  if(CCChanged) {
	//    CCSave();
	//    CCChanged=false;
	//    ModuleSelected(PatCur.PatNum);
	//  }
	//}

	//private void CCSave() {
	//  string cc=textCC.Text;
	//  if(Regex.IsMatch(cc,@"^\d{4}-\d{4}-\d{4}-\d{4}$")) {
	//    PatientNoteCur.CCNumber=cc.Substring(0,4)+cc.Substring(5,4)+cc.Substring(10,4)+cc.Substring(15,4);
	//  }
	//  else{
	//    PatientNoteCur.CCNumber=cc;
	//  }
	//  string exp=textCCexp.Text;
	//  if(Regex.IsMatch(exp,@"^\d\d[/\- ]\d\d$")) {//08/07 or 08-07 or 08 07
	//    PatientNoteCur.CCExpiration=new DateTime(Convert.ToInt32("20"+exp.Substring(3,2)),Convert.ToInt32(exp.Substring(0,2)),1);
	//  }
	//  else if(Regex.IsMatch(exp,@"^\d{4}$")) {//0807
	//    PatientNoteCur.CCExpiration=new DateTime(Convert.ToInt32("20"+exp.Substring(2,2)),Convert.ToInt32(exp.Substring(0,2)),1);
	//  } 
	//  else if(exp=="") {
	//    PatientNoteCur.CCExpiration=new DateTime();//Allow the experation date to be deleted.
	//  } 
	//  else {
	//    MsgBox.Show(this,"Expiration format invalid.");
	//  }
	//  PatientNotes.Update(PatientNoteCur,PatCur.Guarantor);
	//}

	//private void FillPatientButton() {
	//	Patients.AddPatsToMenu(menuPatient,new EventHandler(menuPatient_Click),PatCur,FamCur);
	//}

	//private void textCC_TextChanged(object sender,EventArgs e) {
	//  CCChanged=true;
	//  if(Regex.IsMatch(textCC.Text,@"^\d{4}$")
	//    || Regex.IsMatch(textCC.Text,@"^\d{4}-\d{4}$")
	//    || Regex.IsMatch(textCC.Text,@"^\d{4}-\d{4}-\d{4}$")) 
	//  {
	//    textCC.Text=textCC.Text+"-";
	//    textCC.Select(textCC.Text.Length,0);
	//  }
	//}

	//private void textCCexp_TextChanged(object sender,EventArgs e) {
	//  CCChanged=true;
	//}

	/*private void butTask_Click(object sender, System.EventArgs e) {
		//FormTaskListSelect FormT=new FormTaskListSelect(TaskObjectType.Patient,PatCur.PatNum);
		//FormT.ShowDialog();
	}*/

	//private void gridProg_MouseUp(object sender,MouseEventArgs e) {
	//}
	#endregion Methods - Inactive
}