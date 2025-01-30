using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental;

public partial class FormRpEraAutoProcessed:FormODBase {
	///<summary>A list of all X835Status values.</summary>
	List<X835Status> _listX835Statuses;
	///<summary>The data for the ERA grid's currently selected row.</summary>
	private EraRowData _eraRowDataSelected;
	///<summary>The data for the Claim grid's currently selected row.</summary>
	private ClaimRowData _claimRowDataSelected;

		
	public FormRpEraAutoProcessed() {
		InitializeComponent();
	}

		
	private void FormRpEraAutoProcessed_Load(object sender,EventArgs e) {
		base.SetFilterControlsAndAction(() => RefreshGridEras(),dateRangePicker,textCarrier,textCheckTrace,comboClinics,listAutoProcessStatuses,checkShowAcknowledged);
		SetFiltersAndDefaults();
		BuildContextMenuGridEras();
		BuildContextMenuGridClaims();
		FillGridEras();
		FillGridClaims(null);//Just to init columns for the Claims and Claim Adjustment Procedures grids.
	}

	///<summary>Populate filters and set default selections.</summary>
	private void SetFiltersAndDefaults() {
		//We always get X835s for all X835Statuses, so we fill this list once on load.
		_listX835Statuses=Enum.GetValues(typeof(X835Status)).Cast<X835Status>().ToList();
		if(true) {
			comboClinics.IsAllSelected=true;//Defaults to 'All' so that 835s with missing clinic will show.
		}
		if(!PrefC.GetBool(PrefName.EraShowStatusAndClinic)) {
			comboClinics.Visible=false;
		}
		dateRangePicker.SetDateTimeFrom(DateTime.Today.AddDays(-7));
		dateRangePicker.SetDateTimeTo(DateTime.Today);
		var listX835AutoProcessedValues=Enum.GetValues(typeof(X835AutoProcessed)).Cast<X835AutoProcessed>().ToList();
		for(var i=0;i<listX835AutoProcessedValues.Count;i++) {
			if(listX835AutoProcessedValues[i]==X835AutoProcessed.None) {
				continue;
			}
			listAutoProcessStatuses.Items.Add(Lan.g(this,listX835AutoProcessedValues[i].GetDescription()),listX835AutoProcessedValues[i]);
		}
		listAutoProcessStatuses.SetAll(true);
	}

	///<summary>Builds the context menu for the ERAs grid.</summary>
	private void BuildContextMenuGridEras() {
		var listMenuItems=new List<MenuItem>();
		listMenuItems.Add(new MenuItem(Lan.g(this,"Acknowledge"),new EventHandler(MenuItemAcknowledge_Click)));
		var contextMenu=new ContextMenu(listMenuItems.ToArray());
		gridEras.ContextMenu=contextMenu;
	}

	///<summary>Builds the context menu for the Claims grid.</summary>
	private void BuildContextMenuGridClaims() {
		var listMenuItems=new List<MenuItem>();
		listMenuItems.Add(new MenuItem(Lan.g(this,"Go to Account"),new EventHandler(MenuItemGoToAccount_Click)));
		listMenuItems.Add(new MenuItem(Lan.g(this,"Process Unsent Secondary Claims"),new EventHandler(MenuItemProcessSecondaryClaims_Click)));
		var contextMenu=new ContextMenu(listMenuItems.ToArray());
		gridClaims.ContextMenu=contextMenu;
	}

	///<summary>Fill the ERAs grid. Gets data from the DB every time it is called.</summary>
	private void FillGridEras() {
		var showStatusAndClinics=PrefC.GetBool(PrefName.EraShowStatusAndClinic);
		gridEras.BeginUpdate();
		gridEras.Columns.Clear();
		gridEras.ListGridRows.Clear();
		#region Initilize columns
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","Patient Name"),250));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","Carrier Name"),250));
		if(showStatusAndClinics) {
			gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","Status"),80,HorizontalAlignment.Center));
		}
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","Date"),80,GridSortingStrategy.DateParse));
		if(showStatusAndClinics && true) {
			gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","Clinic"),80,HorizontalAlignment.Center));
		}
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","EraPaid"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","InsEst"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","InsPaid"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","InsVar"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","W/O Est"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","Writeoff"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridEras.Columns.Add(new GridColumn(Lan.g("TableEtrans835s","W/O Var"),63,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		#endregion Initilize columns
		#region Get data
		var dateFrom=dateRangePicker.GetDateTimeFrom();
		var dateTo=dateRangePicker.GetDateTimeTo(true);
		var progressOD=new ProgressWin();
		progressOD.ActionMain=() => {
			EtransL.AddMissingEtrans835s(dateFrom,dateTo);
		};
		progressOD.ShowDialog();
		if(progressOD.IsCancelled) {
			gridEras.EndUpdate();
			MsgBox.Show(this,"Cancel was clicked before the ERA table could finish loading. Click Refresh to load ERAs.");
			return;
		}
		var listClinicNumsSelected=GetSelectedClinicNums();
		var carrierName=textCarrier.Text;
		var checkTraceNum=textCheckTrace.Text;
		var listX835AutoProcessedsSelected=listAutoProcessStatuses.GetListSelected<X835AutoProcessed>();
		var doIncludeAcknowledged=checkShowAcknowledged.Checked;
		var areAllClinicsSelected=comboClinics.IsAllSelected;
		var eraData=new EraData();
		var listHx835_ShortClaimProcs=new List<Hx835_ShortClaimProc>();
		progressOD=new ProgressWin();
		progressOD.ActionMain=() => {
			eraData=EtransL.GetEraDataFiltered(showStatusAndClinics,_listX835Statuses,listClinicNumsSelected,
				amountMin:"",amountMax:"",dateFrom,dateTo,carrierName,checkTraceNum,controlId:"",includeAutomatableCarriersOnly:false,
				areAllClinicsSelected,listX835AutoProcessedsSelected,doIncludeAcknowledged);
			var listClaimNums=eraData.ListAttached.Select(x => x.ClaimNum).ToList();
			listHx835_ShortClaimProcs=Hx835_ShortClaimProc.RefreshForClaims(listClaimNums);
		};
		progressOD.ShowDialog();
		if(progressOD.IsCancelled) {
			gridEras.EndUpdate();
			MsgBox.Show(this,"Cancel was clicked before the ERA table could finish loading. Click Refresh to load ERAs.");
			return;
		}
		#endregion Get data
		#region Fill rows
		for(var i=0;i<eraData.ListEtrans.Count;i++) {
			var etrans835=eraData.ListEtrans835s[i];
			var listEtrans835Attaches=eraData.ListAttached.FindAll(x => x.EtransNum==etrans835.EtransNum);
			var listClaimNumsForRow=listEtrans835Attaches.Select(x => x.ClaimNum).ToList();
			var listHx835_ShortClaimProcsRow=listHx835_ShortClaimProcs.FindAll(x => listClaimNumsForRow.Contains(x.ClaimNum));
			var insEstSum=listHx835_ShortClaimProcsRow.Sum(x => x.InsPayEst);
			var insPaidSum=listHx835_ShortClaimProcsRow.Sum(x => x.InsPayAmt);
			var writeOffSum=listHx835_ShortClaimProcsRow.Sum(x => x.WriteOff);
			var writeOffEstSum=CalcWriteOffEstSumForRow(listHx835_ShortClaimProcsRow);
			var row=new GridRow();
			row.Cells.Add(etrans835.PatientName);
			row.Cells.Add(etrans835.PayerName);
			if(showStatusAndClinics) {
				row.Cells.Add(etrans835.Status.GetDescription());
			}
			row.Cells.Add(eraData.ListEtrans[i].DateTimeTrans.ToShortDateString());
			if(showStatusAndClinics && true) {
				var clinicAbbr=GetClinicAbbrForEraGridRow(listEtrans835Attaches,eraData.ListEtrans[i]);
				row.Cells.Add(clinicAbbr);
			}
			row.Cells.Add(etrans835.InsPaid.ToString("f"));
			row.Cells.Add(insEstSum.ToString("f"));
			row.Cells.Add(insPaidSum.ToString("f"));
			row.Cells.Add((insEstSum-insPaidSum).ToString("f"));
			row.Cells.Add(writeOffEstSum.ToString("f"));
			row.Cells.Add(writeOffSum.ToString("f"));
			row.Cells.Add((writeOffEstSum-writeOffSum).ToString("f"));
			row.Tag=new EraRowData(etrans835,eraData.ListEtrans[i]);
			gridEras.ListGridRows.Add(row);
		}
		#endregion Fill rows
		gridEras.EndUpdate();
	}

	///<summary>If no clinics are selected, we select All.</summary>
	private List<long> GetSelectedClinicNums() {
		var listClinicNums=new List<long>();
		if(true) {
			if(comboClinics.ListClinicNumsSelected.Count==0) {
				comboClinics.IsAllSelected=true;//All clinics.
			}
			listClinicNums=comboClinics.ListClinicNumsSelected;
		}
		return listClinicNums;
	}

	///<summary>Determines the string to put in the Clinic column of the ERAs grid. ERAs may have claims from multiple cliinics.</summary>
	private string GetClinicAbbrForEraGridRow(List<Etrans835Attach> listAttached,Etrans etrans) {
		var listClinicNums=listAttached.Select(x => x.ClinicNum).Distinct().ToList();
		var clinicAbbr="";
		if(listClinicNums.Count==1) {
			if(listClinicNums[0]==0) {
				clinicAbbr=Lan.g(this,"Unassigned");
			}
			else {
				clinicAbbr=Clinics.GetAbbr(listClinicNums[0]);
			}
		}
		else if(listClinicNums.Count>1) {
			clinicAbbr="("+Lan.g(this,"Multiple")+")";
		}
		return clinicAbbr;
	}

	///<summary>Sums the estimated writeoffs for the list of ClaimProcs. Uses the WriteOffEstOverride if it isn't -1.</summary>
	private double CalcWriteOffEstSumForRow(List<Hx835_ShortClaimProc> listHx835_ShortClaimProcs) {
		double sum=0;
		for(var i=0;i<listHx835_ShortClaimProcs.Count;i++) {
			if(!CompareDouble.IsEqual(listHx835_ShortClaimProcs[i].WriteOffEstOverride,-1)) {
				sum+=listHx835_ShortClaimProcs[i].WriteOffEstOverride;
			}
			else if(!CompareDouble.IsEqual(listHx835_ShortClaimProcs[i].WriteOffEst,-1)) {
				sum+=listHx835_ShortClaimProcs[i].WriteOffEst;
			}
		}
		return sum;
	}

		
	private void butRefresh_Click(object sender,EventArgs e) {
		RefreshGridEras();
	}

	private void RefreshGridEras() {
		gridEras.SetAll(false);
		FillGridClaims(null);//Passing null will clear the Claims and Claim Procedure Adjustments grids.
		FillGridEras();
	}

	///<summary>Updates Etrans835.IsApproved to true for each selected row in the ERA grid.</summary>
	private void MenuItemAcknowledge_Click(object sender,EventArgs e) {
		AcknowledgeErasSelected();
	}

	///<summary>Updates Etrans835.IsApproved to true for each selected row in the ERA grid.</summary>
	private void butAcknowledge_Click(object sender,EventArgs e) {
		AcknowledgeErasSelected();
	}

	///<summary>Sets Etrans835.IsApproved to true for all selected ERAs and updates them.
	///If checkShowAcknowledged isn't checked, acknowledged rows are removed from the grid.</summary>
	private void AcknowledgeErasSelected() {
		var listIndicesSelected=gridEras.SelectedIndices.ToList();
		if(listIndicesSelected.IsNullOrEmpty()) {
			MsgBox.Show(this,"No ERAs are selected.");
			return;
		}
		Cursor=Cursors.WaitCursor;
		gridEras.BeginUpdate();
		var wasUpdateMade=false;
		for(var i=listIndicesSelected.Count-1;i>=0;i--) {
			var index=listIndicesSelected[i];
			var eraRowData=(EraRowData)gridEras.ListGridRows[index].Tag;
			if(eraRowData.Etrans835ForRow.IsApproved) {
				continue;//Already acknowledged and checkShowAcknowledged must be checked.
			}
			var etrans835Old=eraRowData.Etrans835ForRow.Copy();
			eraRowData.Etrans835ForRow.IsApproved=true;
			Etrans835s.Update(eraRowData.Etrans835ForRow,etrans835Old);
			wasUpdateMade=true;
			if(!checkShowAcknowledged.Checked) {
				gridEras.ListGridRows.RemoveAt(index);
			}
		}
		gridEras.EndUpdate();
		FillGridClaims(null);//Passing null will clear the Claims and Claim Procedure Adjustments grids.
		Cursor=Cursors.Default;
		if(!wasUpdateMade) {
			MsgBox.Show(this,"All selected ERAs are already acknowledged.");
		}
	}

	///<summary>Opens the ERA that was double clicked.</summary>
	private void gridEras_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var eraRowDataSelected=gridEras.SelectedTag<EraRowData>();
		EtransL.ViewFormForEra(eraRowDataSelected.EtransForRow,this);
	}

	///<summary>Refreshes the Claims grid to show claims for the selected ERA. If multiple ERAs are selected, claims for the first selected row are shown.</summary>
	private void gridEras_MouseUp(object sender,MouseEventArgs e) {
		RefreshGridClaims();
	}

	///<summary>This button is only visible when the selected ERA has multiple EOBs.
	///Forces a refresh of the Claims grid even though the Selected ERA has not changed so that the user can select a new EOB.</summary>
	private void butSelectEob_Click(object sender,EventArgs e) {
		RefreshGridClaims(doForceRefresh:true);
	}

	///<summary>Only refreshes if new ERA selected is different from the old one, or if we are forcing a refresh to allow the user to select a new EOB.</summary>
	private void RefreshGridClaims(bool doForceRefresh=false) {
		var eraRowDataSelectedNew=gridEras.SelectedTag<EraRowData>();
		if(!doForceRefresh 
		   && _eraRowDataSelected!=null 
		   && eraRowDataSelectedNew!=null 
		   && _eraRowDataSelected.EtransForRow.EtransNum==eraRowDataSelectedNew.EtransForRow.EtransNum)
		{
			return;//Same ERA was selected, so we don't need to refill the claims grid.
		}
		FillGridClaims(eraRowDataSelectedNew);
	}

	///<summary>Passing null will clear the Claims Claim Procedure Adjustments grids. Gets data from the DB each time it is filled.</summary>
	private void FillGridClaims(EraRowData eraRowData) {
		Cursor=Cursors.WaitCursor;
		butSelectEob.Visible=false;
		_eraRowDataSelected=eraRowData;
		gridClaims.BeginUpdate();
		gridClaims.Columns.Clear();
		gridClaims.ListGridRows.Clear();
		#region Init columns
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","Recd"),65,HorizontalAlignment.Center));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","Patient Name"),250));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","Type"),80,HorizontalAlignment.Center));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","DateService"),80,GridSortingStrategy.DateParse));
		if(true) {
			gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","Clinic"),80,HorizontalAlignment.Center));//Consider translation category name.
		}
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","EraPaid"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","InsEst"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","InsPaid"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","InsVar"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","W/O Est"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","Writeoff"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","W/O Var"),60,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","Supp?"),65,HorizontalAlignment.Center));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","ClaimAdj?"),65,HorizontalAlignment.Center));
		gridClaims.Columns.Add(new GridColumn(Lan.g("TableEtrans835Claims","MoreClaims?"),65,HorizontalAlignment.Center));
		#endregion Init columns
		if(eraRowData==null) {
			gridClaims.EndUpdate();
			FillGridClaimProcedureAdjustments(null);
			Cursor=Cursors.Default;
			return;
		}
		#region Get ERA data 
		var messageText835=EtransMessageTexts.GetMessageText(eraRowData.EtransForRow.EtransMessageTextNum);
		var etrans=eraRowData.EtransForRow;
		var tranSetIdSelected=GetTranSetIdForEobSelected(etrans,messageText835);
		if(tranSetIdSelected==null) {//Only null if user was prompted to select an EOB and they canceled instead of choosing one.
			_eraRowDataSelected=null;
			gridClaims.EndUpdate();
			FillGridClaimProcedureAdjustments(null);
			Cursor=Cursors.Default;
			return;
		}
		var listEtrans835Attaches=Etrans835Attaches.GetForEtrans(etrans.EtransNum);
		var x835=new X835(etrans,messageText835,tranSetIdSelected,listEtrans835Attaches);
		x835.RefreshAttachesAndClaimProcsFromDb(out listEtrans835Attaches,out var listShortClaimProcs);
		var listClaimNums=x835.ListClaimsPaid.Select(x => x.ClaimNum).Where(x => x!=0).ToList();
		var listHx835_ShortClaims=Hx835_ShortClaim.GetClaimsFromClaimNums(listClaimNums);
		var listProcNumsOnClaims=listShortClaimProcs.Select(x => x.ProcNum).Where(x => x!=0).ToList();
		var listHx835_ShortClaimProcsForUnsentClaimsAll=Hx835_ShortClaimProc.GetUnsentForProcNums(listProcNumsOnClaims);
		#endregion Get ERA data
		#region Fill rows
		for(var i=0;i<x835.ListClaimsPaid.Count;i++) {
			var hx835_Claim=x835.ListClaimsPaid[i];
			var shortClaim=listHx835_ShortClaims.FirstOrDefault(x => x.ClaimNum==hx835_Claim.ClaimNum);//Will be null if no claim is attached.
			var claimStatus="";
			var isClaimPaidProcessed=false;
			if(hx835_Claim.IsProcessed(listShortClaimProcs,listEtrans835Attaches)) {
				isClaimPaidProcessed=true;
				claimStatus="X";
			}
			else if((hx835_Claim.IsAttachedToClaim && hx835_Claim.ClaimNum==0) || !hx835_Claim.IsAttachedToClaim) {
				claimStatus=Lan.g("TableETrans835Claims","No Claim Attached");
			}
			var stringDateService=hx835_Claim.DateServiceStart.ToShortDateString();
			if(hx835_Claim.DateServiceEnd>hx835_Claim.DateServiceStart) {
				stringDateService+=" to "+hx835_Claim.DateServiceEnd.ToShortDateString();
			}
			var stringClaimType="";
			var stringClinic="";
			var stringInsEst="";
			var stringInsPaid="";
			var stringInsEstVar="";
			var stringWriteoff="";
			var stringWriteoffEst="";
			var stringWriteOffVar="";
			var stringOtherUnsentClaims="";
			var listHx835_ShortClaimProcsForClaim=new List<Hx835_ShortClaimProc>();
			var listHx835_ShortClaimProcsForUnsentClaims=new List<Hx835_ShortClaimProc>();
			if(shortClaim!=null) {
				listHx835_ShortClaimProcsForClaim=listShortClaimProcs.FindAll(x => x.ClaimNum==shortClaim.ClaimNum);
				Enum.TryParse(shortClaim.ClaimType,out EnumClaimType claimType);
				stringClaimType=claimType.GetDescription();
				stringClinic=Clinics.GetAbbr(shortClaim.ClinicNum);
				if(stringClinic.IsNullOrEmpty()) {
					stringClinic=Lan.g("TableEtrans835Claims","Unassigned");
				}
				var insPaySum=listHx835_ShortClaimProcsForClaim.Sum(x => x.InsPayAmt);
				var insEstSum=listHx835_ShortClaimProcsForClaim.Sum(x => x.InsPayEst);
				var writeOffSum=listHx835_ShortClaimProcsForClaim.Sum(x => x.WriteOff);
				var writeOffEstSum=CalcWriteOffEstSumForRow(listHx835_ShortClaimProcsForClaim);
				stringInsEst=insEstSum.ToString("f");
				stringInsPaid=insPaySum.ToString("f");
				stringInsEstVar=(insEstSum-insPaySum).ToString("f");
				stringWriteoff=writeOffSum.ToString("f");
				stringWriteoffEst=writeOffEstSum.ToString("f");
				stringWriteOffVar=(writeOffEstSum-writeOffSum).ToString("f");
				listHx835_ShortClaimProcsForUnsentClaims=FilterClaimProcsForUnsentClaims(listHx835_ShortClaimProcsForUnsentClaimsAll,listHx835_ShortClaimProcsForClaim);
				if(listHx835_ShortClaimProcsForUnsentClaims.Count>0) {
					stringOtherUnsentClaims="X";
				}
			}
			var stringSupplemental="";
			if(hx835_Claim.GetIsSupplemental(listEtrans835Attaches,listShortClaimProcs)) {
				stringSupplemental="X";
			}
			var stringClaimAdjustments="";
			//If any procedures on the claim have a claim adjustment on the ERA
			if(hx835_Claim.ListProcs.SelectMany(x => x.ListProcAdjustments).Count()>0) {
				stringClaimAdjustments="X";
			}
			var row=new GridRow();
			row.Cells.Add(claimStatus);
			row.Cells.Add(hx835_Claim.PatientName.ToString());
			row.Cells.Add(stringClaimType);
			row.Cells.Add(stringDateService);
			if(true) {
				row.Cells.Add(stringClinic);
			}
			row.Cells.Add(hx835_Claim.InsPaid.ToString("f"));
			row.Cells.Add(stringInsEst);
			row.Cells.Add(stringInsPaid);
			row.Cells.Add(stringInsEstVar);
			row.Cells.Add(stringWriteoffEst);
			row.Cells.Add(stringWriteoff);
			row.Cells.Add(stringWriteOffVar);
			row.Cells.Add(stringSupplemental);
			row.Cells.Add(stringClaimAdjustments);
			row.Cells.Add(stringOtherUnsentClaims);
			row.Tag=new ClaimRowData(hx835_Claim,isClaimPaidProcessed,shortClaim,listHx835_ShortClaimProcsForUnsentClaims);
			gridClaims.ListGridRows.Add(row);
		}
		#endregion Fill rows
		gridClaims.EndUpdate();
		Cursor=Cursors.Default;
	}

	///<summary>Takes a list of Hx835_ShortClaimProcs for unsent claims that share one or more procedures with any claim on the selected ERA,
	///and the list of Hx835_shortClaimProcs for the current claim being processed on the claim grid. Returns a list of Hx835_ShortClaimProcs for unsent claims that share one
	///or more procedures with the current claim.///</summary>
	private List<Hx835_ShortClaimProc> FilterClaimProcsForUnsentClaims(List<Hx835_ShortClaimProc> listHx835_ShortClaimProcsForUnsentClaims,
		List<Hx835_ShortClaimProc> listHx835_ShortClaimProcsForClaim)
	{
		if(listHx835_ShortClaimProcsForUnsentClaims.Count==0 || listHx835_ShortClaimProcsForClaim.Count==0) {
			return [];
		}
		var listProcNumsForClaim=listHx835_ShortClaimProcsForClaim.Select(x => x.ProcNum).ToList();
		//Get all claimprocs for unsent claims. Make sure they are not for our current claim.
		return listHx835_ShortClaimProcsForUnsentClaims.FindAll(x => x.ClaimNum!=listHx835_ShortClaimProcsForClaim[0].ClaimNum && listProcNumsForClaim.Contains(x.ProcNum));
	}

	///<summary>Etrans from 14.2 or older versions can represent multiple EOBs. These should be rare, but we need to prompt the user
	///when they select one of these etrans to ask which EOB they want to populate the claims grid with.
	///If a blank string is returned, the user was prompted to select an EOB and hit cancel.</summary>
	private string GetTranSetIdForEobSelected(Etrans etrans,string messageText835) {
		var x12object=new X12object(messageText835);
		var listTranSetIds=x12object.GetTranSetIds();
		var transSetIdSelected=etrans.TranSetId835;
		if(listTranSetIds.Count>=2 && etrans.TranSetId835=="") {
			using var formEtrans835PickEob=new FormEtrans835PickEob(listTranSetIds,messageText835,etrans,doOpenEtrans835:false);
			Cursor=Cursors.Default;
			formEtrans835PickEob.ShowDialog();
			Cursor=Cursors.WaitCursor;
			transSetIdSelected=formEtrans835PickEob.TransSetIdSelected;//May be null if user closed without picking an EOB.
			butSelectEob.Visible=true;
		}
		return transSetIdSelected;
	}

	///<summary>Goes to the patients account for the selected claim.</summary>
	private void MenuItemGoToAccount_Click(object sender,EventArgs e) {
		if(gridClaims.GetSelectedIndex()==-1) {
			MsgBox.Show(this,"No claim is selected.");
			return;
		}
		var hx835_Claim=gridClaims.SelectedTag<ClaimRowData>().Hx835_Claim_;
		if(hx835_Claim==null) {
			MsgBox.Show(this,"No claim is selected.");
			return;
		}
		EtransL.GoToAccountForHx835_Claim(hx835_Claim);
	}

	///<summary>Validates that the selected claim has procedures that are on one or more secondary claims with a status of Unsent or Hold Unti Pri Recieved.
	///If so, the user is prompted to send these claims, change there status to "Waiting to send", or do nothing.</summary>
	private void MenuItemProcessSecondaryClaims_Click(object sender,EventArgs e) {
		var claimRowDataSelected=gridClaims.SelectedTag<ClaimRowData>();
		if(claimRowDataSelected==null) {
			MsgBox.Show(this,"No claim is selected.");
			return;
		}
		var listHx835_ShortClaimProcsForUnsentClaims=claimRowDataSelected.ListHx835_ShortClaimProcsForUnsentClaims;
		if(listHx835_ShortClaimProcsForUnsentClaims.IsNullOrEmpty()) {
			MsgBox.Show(this,"There are no unsent claims for the procedures on the selected claim.");
			return;
		}
		var listClaimNums=listHx835_ShortClaimProcsForUnsentClaims.Select(x => x.ClaimNum).ToList();
		var listHx835_ShortClaimsUnsent=Hx835_ShortClaim.GetClaimsFromClaimNums(listClaimNums);
		if(!listHx835_ShortClaimsUnsent.Any(x => x.ClaimType=="S" && x.ClaimStatus.In("U","H","I"))) {
			MsgBox.Show(this,"There are unsent claim(s) for one or more procedures on the selected claim, " 
			                 +"but none of them are for secondary claims with a status of 'Unsent', 'Hold Until Pri Received', or 'Hold for In Process'. "
			                 +"Please go to the account to send them.");
			return;
		}
		var hx835_ShortClaim=claimRowDataSelected.Hx835_ShortClaim_;
		//The selected claim must be recieved and processed properly before we can send secondary claims.
		if(hx835_ShortClaim.ClaimStatus!="R" || !claimRowDataSelected.IsProcessed) {//R=Received
			MsgBox.Show(this,"The selected claim must be received before secondary claims can be sent.");
			return;
		}
		var listClaimProcsForClaim=ClaimProcs.RefreshForClaim(hx835_ShortClaim.ClaimNum);
		ClaimL.PromptForPrimaryOrSecondaryClaim(listClaimProcsForClaim);
	}

	///<summary>Opens the Edit Claim window for the claim if one is attached to the Hx835_Claim for the row.</summary>
	private void gridClaims_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var hx835_Claim=gridClaims.SelectedTag<ClaimRowData>().Hx835_Claim_;
		var claim=hx835_Claim.GetClaimFromDb();
		if(claim==null) {
			MsgBox.Show(this,"The selected claim payment is not attached to a claim.");
			return;
		}
		else if(Security.IsAuthorized(EnumPermType.ClaimView)) {
			var patient=Patients.GetPat(claim.PatNum);
			var family=Patients.GetFamily(claim.PatNum);
			using var formClaimEdit=new FormClaimEdit(claim,patient,family);
			formClaimEdit.ShowDialog();//Modal, because the user could edit information in this window.
		}
	}

	///<summary>Only refreshes the Claim Procedure Adjustments grid if the new claim selected is different from the previous one.</summary>
	private void gridClaims_SelectionCommitted(object sender,EventArgs e) {
		var claimRowDataSelectedNew=gridClaims.SelectedTag<ClaimRowData>();
		if(_claimRowDataSelected!=null
		   && claimRowDataSelectedNew!=null
		   && _claimRowDataSelected.Hx835_Claim_.Era.EtransSource.EtransNum==claimRowDataSelectedNew.Hx835_Claim_.Era.EtransSource.EtransNum
		   && _claimRowDataSelected.Hx835_Claim_.ClpSegmentIndex==claimRowDataSelectedNew.Hx835_Claim_.ClpSegmentIndex)
		{
			return;//Same claim was selected, so we don't need to refill the claim procedure detail grid.
		}
		FillGridClaimProcedureAdjustments(claimRowDataSelectedNew);
	}

	///<summary>Passing null will clear the grid.</summary>
	private void FillGridClaimProcedureAdjustments(ClaimRowData claimRowData) {
		_claimRowDataSelected=claimRowData;
		gridClaimProcedureAdjustments.BeginUpdate();
		gridClaimProcedureAdjustments.Columns.Clear();
		gridClaimProcedureAdjustments.ListGridRows.Clear();
		#region Init columns
		var widthProcColumn=35;
		var widthProcCodeColumn=70;
		var widthDescriptionColumn=200;
		var widthAdjAmtColumn=65;
		var widthReasonColumn=gridClaimProcedureAdjustments.Width-10-widthProcColumn-widthProcCodeColumn-widthDescriptionColumn-widthAdjAmtColumn;
		gridClaimProcedureAdjustments.Columns.Add(new GridColumn(Lan.g("TableEtrans835ClaimProcedureAdjustments","Proc"),widthProcColumn,HorizontalAlignment.Center));
		gridClaimProcedureAdjustments.Columns.Add(new GridColumn(Lan.g("TableEtrans835ClaimProcedureAdjustments","ProcCode"),widthProcCodeColumn));
		gridClaimProcedureAdjustments.Columns.Add(new GridColumn(Lan.g("TableEtrans835ClaimProcedureAdjustments","Description"),widthDescriptionColumn));
		gridClaimProcedureAdjustments.Columns.Add(new GridColumn(Lan.g("TableEtrans835ClaimProcedureAdjustments","Reason"),widthReasonColumn));
		gridClaimProcedureAdjustments.Columns.Add(new GridColumn(Lan.g("TableEtrans835ClaimProcedureAdjustments","AdjAmt"),
			widthAdjAmtColumn,HorizontalAlignment.Right,GridSortingStrategy.AmountParse));
		#endregion Init columns
		if(claimRowData==null) {
			gridClaimProcedureAdjustments.EndUpdate();
			return;
		}
		#region Fill rows
		var procCount=1;
		for(var i=0;i<claimRowData.Hx835_Claim_.ListProcs.Count;i++) {
			var hx835_Proc=claimRowData.Hx835_Claim_.ListProcs[i];
			if(hx835_Proc.ListProcAdjustments.Count==0) {
				continue;
			}
			for(var j=0;j<hx835_Proc.ListProcAdjustments.Count;j++) {
				var hx835_Adj=hx835_Proc.ListProcAdjustments[j];
				var row=new GridRow();
				row.Cells.Add((procCount).ToString());
				row.Cells.Add(hx835_Proc.ProcCodeBilled);
				row.Cells.Add(hx835_Adj.AdjustRemarks);
				row.Cells.Add(hx835_Adj.ReasonDescript);
				row.Cells.Add(hx835_Adj.AdjAmt.ToString("f"));
				gridClaimProcedureAdjustments.ListGridRows.Add(row);
			}
			procCount++;
		}
		#endregion Fill rows
		gridClaimProcedureAdjustments.EndUpdate();
	}

	///<summary>Holds data for the rows of the ERA grid.</summary>
	private class EraRowData {
		public Etrans835 Etrans835ForRow;
		public Etrans EtransForRow;

			
		public EraRowData(Etrans835 etrans835,Etrans etrans) {
			Etrans835ForRow=etrans835;
			EtransForRow=etrans;
		}
	}

	///<summary>Holds data for the rows of the Claims grid.</summary>
	private class ClaimRowData {
		public Hx835_Claim Hx835_Claim_;
		public bool IsProcessed;
		public List<Hx835_ShortClaimProc> ListHx835_ShortClaimProcsForUnsentClaims= [];
		public Hx835_ShortClaim Hx835_ShortClaim_;

			
		public ClaimRowData(Hx835_Claim hx835_Claim,bool isProcessed,Hx835_ShortClaim hx835_ShortClaim,List<Hx835_ShortClaimProc> listHx835_ShortClaimProcsForUnsentClaims) {
			Hx835_Claim_=hx835_Claim;
			IsProcessed=isProcessed;
			Hx835_ShortClaim_=hx835_ShortClaim;
			ListHx835_ShortClaimProcsForUnsentClaims=listHx835_ShortClaimProcsForUnsentClaims;
		}
	}

	private void FormRpEraAutoProcessed_Shown(object sender,EventArgs e) {
		SecurityLogs.MakeLogEntry(EnumPermType.InsPayCreate,0,"Window 'ERA's Automatically Processed' opened.");
	}

}