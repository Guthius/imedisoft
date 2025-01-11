using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental{
	/// <summary></summary>
	public partial class FormProcSelect : FormODBase {
		#region Private Variables
		private long _patNum;
		private List<ClaimProc> _listClaimProcs;
		///<summary>Set to true to enable multiple procedure selection mode.</summary>
		private bool _isMultiSelect;
		private bool _doShowAdjustments;
		private bool _doShowTreatmentPlanProcs;
		#endregion

		#region Public Variables
		///<summary>If form closes with OK, this contains selected proc num.</summary>
		public List<Procedure> ListProceduresSelected=new List<Procedure>();
		///<summary>List of paysplits for the current payment.</summary>
		public List<PaySplit> ListPaySplits=new List<PaySplit>();
		public bool IsPrepayAllowedForTpProcs=PrefC.GetYN(PrefName.PrePayAllowedForTpProcs);
		public List<AccountEntry> ListAccountEntries=new List<AccountEntry>();
		#endregion

		///<summary>Displays completed procedures for the passed-in pat.</summary>
		public FormProcSelect(long patNum,bool isMultiSelect=false,bool doShowAdjustments=false,bool doShowTreatmentPlanProcs=true) {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
			_patNum=patNum;
			_isMultiSelect=isMultiSelect;
			_doShowAdjustments=doShowAdjustments;
			if(_doShowAdjustments) {
				gridMain.Title=Lan.g(gridMain.TranslationName,"Account Entries");
				this.Text=Lan.g(this,"Select Account Entries");
			}
			_doShowTreatmentPlanProcs=doShowTreatmentPlanProcs;
		}

		private void FormProcSelect_Load(object sender,System.EventArgs e) {
			if(_isMultiSelect) {
				gridMain.SelectionMode=OpenDental.UI.GridSelectionMode.MultiExtended;
			}
			if(PrefC.GetInt(PrefName.RigorousAdjustments)==(int)RigorousAdjustments.DontEnforce) {
				radioIncludeAllCredits.Checked=true;
				groupBreakdown.Visible=false;
			}
			else {
				radioOnlyAllocatedCredits.Checked=true;
			}
			FillGrid();
		}

		private void FillGrid(){
			CreditCalcType creditCalcType;
			if(radioIncludeAllCredits.Checked) {
				creditCalcType = CreditCalcType.IncludeAll;
			}
			else if(radioOnlyAllocatedCredits.Checked) {
				creditCalcType = CreditCalcType.AllocatedOnly;
			}
			else {
				creditCalcType= CreditCalcType.ExcludeAll;
			}
			bool doIncludeTreatmentPlanned=false;
			if(IsPrepayAllowedForTpProcs && _doShowTreatmentPlanProcs) {
				doIncludeTreatmentPlanned=true;
			}
			bool doIncludeExplicitCreditsOnly=true;
			if(creditCalcType==CreditCalcType.IncludeAll) {
				doIncludeExplicitCreditsOnly=false;
			}
			//Utilize the ITM logic to get the current state of affairs for the patient passed in.
			//listPatNums: Null to include all family account entries in calculations. Filter by PatNum later.
			//doIncludeExplicitCreditsOnly: Credit filter radio buttons determine explicit (true) vs. implicit (false)
			//hasOffsettingAdjustments: True so PrefName.AdjustmentsOffsetEachOther decides if we offset unattached adjustments or not.
			PaymentEdit.ConstructResults constructResults=PaymentEdit.ConstructAndLinkChargeCredits(
				_patNum,doIncludeTreatmentPlanned:doIncludeTreatmentPlanned,
				doIncludeExplicitCreditsOnly:doIncludeExplicitCreditsOnly);
			List<AccountEntry> listAccountEntries=constructResults.ListAccountEntries.FindAll(x => x.PatNum==_patNum);
			if(!_doShowTreatmentPlanProcs) {
				//GetConstructChargesData includes TP'd procs if PrePayAllowedForTpProcs is true, even when doIncludeTreatmentPlanned is false.
				//Override to include only completed procs.
				listAccountEntries=listAccountEntries.FindAll(x => x.GetType()!=typeof(Procedure) || ((Procedure)x.Tag).ProcStatus==ProcStat.C);
			}
			List<long> listProcNums=listAccountEntries.FindAll(x => x.GetType()==typeof(Procedure)).Select(x => x.ProcNum).ToList();
			_listClaimProcs=constructResults.ListClaimProcs.FindAll(x => listProcNums.Contains(x.ProcNum));
			List<Def> listDefsPosAdj=Defs.GetPositiveAdjTypes();
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn col=new GridColumn(Lan.g("TableProcSelect","Date"),70,HorizontalAlignment.Center);
			gridMain.Columns.Add(col);
			col=new GridColumn(Lan.g("TableProcSelect","Prov"),55);
			gridMain.Columns.Add(col);
			col=new GridColumn(Lan.g("TableProcSelect","Code"),55);
			gridMain.Columns.Add(col);
			if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
				col=new GridColumn(Lan.g("TableProcSelect","Description"),290);
				gridMain.Columns.Add(col);
			}
			else {
				col=new GridColumn(Lan.g("TableProcSelect","Tooth"),40);
				gridMain.Columns.Add(col);
				col=new GridColumn(Lan.g("TableProcSelect","Description"),250);
				gridMain.Columns.Add(col);
			}
			if(creditCalcType == CreditCalcType.ExcludeAll) {
				col=new GridColumn(Lan.g("TableProcSelect","Amt"),40,HorizontalAlignment.Right){ IsWidthDynamic=true };
				gridMain.Columns.Add(col);
			}
			else {
				col=new GridColumn(Lan.g("TableProcSelect","Amt Orig"),90,HorizontalAlignment.Right);
				gridMain.Columns.Add(col);
				col=new GridColumn(Lan.g("TableProcSelect","Amt End"),90,HorizontalAlignment.Right);
				gridMain.Columns.Add(col);
			}
			gridMain.ListGridRows.Clear();
			List<AccountEntry> listAccountEntriesProcedures=listAccountEntries.FindAll(x => x.GetType()==typeof(Procedure));
			GridRow row;
			for(int i=0;i<listAccountEntries.Count;i++) {
				if(!listAccountEntries[i].GetType().In(typeof(Procedure),typeof(Adjustment))) {
					continue;
				}
				if(creditCalcType!=CreditCalcType.ExcludeAll && CompareDecimal.IsLessThanOrEqualToZero(listAccountEntries[i].AmountEnd)) {
					continue;
				}
				string procCodeStr="";
				string toothNumStr="";
				string descriptionText="";
				if(listAccountEntries[i].GetType()==typeof(Adjustment)) {
					//Find any attached procedures with the same pat, prov, and clinic.
					AccountEntry accountEntryProcedure=listAccountEntriesProcedures.Find(x => x.ProcNum==listAccountEntries[i].ProcNum
						&& x.PatNum==listAccountEntries[i].PatNum
						&& x.ProvNum==listAccountEntries[i].ProvNum
						&& x.ClinicNum==listAccountEntries[i].ClinicNum
					);
					if(!_doShowAdjustments || accountEntryProcedure!=null) {
						//Do not show adjustments or do show adjustments- must not be attached to a procedure with a matching pat, prov, and clinic.
						continue;
					}
					Def defAdjType=listDefsPosAdj.FirstOrDefault(x => x.DefNum==((Adjustment)listAccountEntries[i].Tag).AdjType);
					descriptionText=defAdjType?.ItemName??Lan.g(this,"Adjustment");
				}
				else {//Procedure
					Procedure procedure=(Procedure)listAccountEntries[i].Tag;
					ProcedureCode procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum);
					procCodeStr=procedureCode.ProcCode;
					toothNumStr=procedure.ToothNum=="" ? Tooth.SurfTidyFromDbToDisplay(procedure.Surf,procedure.ToothNum) : Tooth.Display(procedure.ToothNum);
					if(procedure.ProcStatus==ProcStat.TP) {
						descriptionText="(TP) ";
					}
					descriptionText+=procedureCode.Descript;
				}
				row=new GridRow();
				row.Cells.Add(listAccountEntries[i].Date.ToShortDateString());
				row.Cells.Add(Providers.GetAbbr(listAccountEntries[i].ProvNum));
				row.Cells.Add(procCodeStr);
				if(!Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
					row.Cells.Add(toothNumStr);
				}
				row.Cells.Add(descriptionText);
				row.Cells.Add(listAccountEntries[i].AmountOriginal.ToString("f"));
				if(creditCalcType != CreditCalcType.ExcludeAll) {
					row.Cells.Add(listAccountEntries[i].AmountEnd.ToString("f"));
				}
				row.Tag=listAccountEntries[i];
				gridMain.ListGridRows.Add(row);
			}
			gridMain.EndUpdate();
			RefreshBreakdown();
		}

		private void RefreshBreakdown() {
			if(gridMain.GetSelectedIndex()==-1) {
				labelAmtOriginal.Text=(0).ToString("c");
				labelPositiveAdjs.Text=(0).ToString("c");
				labelNegativeAdjs.Text=(0).ToString("c");
				labelPayPlanCredits.Text=(0).ToString("c");
				labelPaySplits.Text=(0).ToString("c");
				labelInsEst.Text=(0).ToString("c");
				labelInsPay.Text=(0).ToString("c");
				labelWriteOff.Text=(0).ToString("c");
				labelWriteOffEst.Text=(0).ToString("c");
				labelCurrentSplits.Text=(0).ToString("c");
				labelAmtEnd.Text=(0).ToString("c");
				return;
			}
			//There could be more than one proc selected if IsMultiSelect = true.
			List<AccountEntry> listAccountEntries=gridMain.SelectedTags<AccountEntry>();
			ProcedureBreakdown procedureBreakdown=new ProcedureBreakdown(listAccountEntries,_listClaimProcs,ListPaySplits);
			labelAmtOriginal.Text=procedureBreakdown.AmtOriginal.ToString("c");
			labelPositiveAdjs.Text=procedureBreakdown.PositiveAdjs.ToString("c");
			labelNegativeAdjs.Text=procedureBreakdown.NegativeAdjs.ToString("c");
			labelPayPlanCredits.Text=(-procedureBreakdown.PayPlanCredits).ToString("c");
			labelPaySplits.Text=(-procedureBreakdown.PaySplits).ToString("c");
			labelInsEst.Text=(-procedureBreakdown.InsEst).ToString("c");
			labelInsPay.Text=(-procedureBreakdown.InsPay).ToString("c");
			labelWriteOff.Text=(-procedureBreakdown.WriteOff).ToString("c");
			labelWriteOffEst.Text=(-procedureBreakdown.WriteOffEst).ToString("c");
			labelCurrentSplits.Text=(-procedureBreakdown.CurrentSplits).ToString("c");
			labelAmtEnd.Text=procedureBreakdown.AmtEnd.ToString("c");
		}

		private void gridMain_CellClick(object sender,ODGridClickEventArgs e) {
			RefreshBreakdown();
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			SetSelectedAccountEntries();
			DialogResult=DialogResult.OK;
		}

		///<summary>Sets the public field lists with the selected grid items. ListSelectedProcs and ListAccountEntries.</summary>
		private void SetSelectedAccountEntries() {
			ListAccountEntries=new List<AccountEntry>();
			List<AccountEntry> listAccountEntries=gridMain.SelectedTags<AccountEntry>();
			for(int i=0;i<listAccountEntries.Count;i++) {
				if(listAccountEntries[i].GetType()==typeof(Procedure)) {
					Procedure procedure=(Procedure)listAccountEntries[i].Tag;
					ListProceduresSelected.Add(procedure);
					ListAccountEntries.Add(new AccountEntry(procedure));
					continue;
				}
				//Adjustment selected. Don't add to ListSelectedProcs
				ListAccountEntries.Add(listAccountEntries[i]);
			}
		}

		private void radioCreditCalc_Click(object sender,EventArgs e) {
			if(radioIncludeAllCredits.Checked) {
				groupBreakdown.Visible=false;
			}
			else {
				groupBreakdown.Visible=true;
			}
			FillGrid();
		}

		private void butOK_Click(object sender, System.EventArgs e) {
			if(gridMain.GetSelectedIndex()==-1){
				MsgBox.Show(this,"Please select an item first.");
				return;
			}
			SetSelectedAccountEntries();
			DialogResult=DialogResult.OK;
		}

		///<summary>Holds the values to fill the Breakdown grid in FormProcSelect.cs</summary>
		private class ProcedureBreakdown {
			public decimal AmtOriginal;
			public decimal PositiveAdjs;
			public decimal NegativeAdjs;
			public decimal PayPlanCredits;
			public double PaySplits;
			public double InsEst;
			public double InsPay;
			public double WriteOff;
			public double WriteOffEst;
			public double CurrentSplits;
			public decimal AmtEnd;

			public ProcedureBreakdown(List<AccountEntry> listAccountEntries,List<ClaimProc> listClaimProcsAll,List<PaySplit> listPaySplits) {
				AmtOriginal=listAccountEntries.Sum(x => x.AmountOriginal);
				PositiveAdjs=listAccountEntries.Sum(x => x.AdjustmentAmtPos);
				NegativeAdjs=listAccountEntries.Sum(x => x.AdjustmentAmtNeg);
				PayPlanCredits=listAccountEntries.SelectMany(x => x.ListPayPlanPrincipalApplieds).Sum(x => x.PrincipalApplied);
				PaySplits=listAccountEntries.SelectMany(x => x.SplitCollection).Sum(x => x.SplitAmt);
				List<long> listProcNums=listAccountEntries.Select(x => x.ProcNum).ToList();
				//Insurance estimates/payments and writeoffs/writeoff estimates
				//ClaimProcs are always treated as explicitly linked.
				List<ClaimProc> listClaimProcs=listClaimProcsAll.FindAll(x => listProcNums.Contains(x.ProcNum));
				double insEst=0;
				double insPay=0;
				double writeOff=0;
				double writeOffEst=0;
				for(int i=0;i<listClaimProcs.Count;i++) {
					if(ClaimProcs.GetEstimatedStatuses().Contains(listClaimProcs[i].Status)) {
						if(listClaimProcs[i].InsEstTotalOverride==-1) {
							insEst+=listClaimProcs[i].InsEstTotal;
						}
						else {
							insEst+=listClaimProcs[i].InsEstTotalOverride;
						}
						if(listClaimProcs[i].WriteOffEstOverride==-1) {
							writeOffEst+=listClaimProcs[i].WriteOffEst;
						}
						else {
							writeOffEst+=listClaimProcs[i].WriteOffEstOverride;
						}
					}
					else if(ClaimProcs.GetInsPaidStatuses().Contains(listClaimProcs[i].Status)) {
						insPay+=listClaimProcs[i].InsPayAmt;
						writeOff+=listClaimProcs[i].WriteOff;
					}
				}
				InsEst=insEst;
				InsPay=insPay;
				WriteOff=writeOff;
				WriteOffEst=writeOffEst;
				//The breakdown only displays when showing explicitly linked entities so make sure PatNum, ProvNum, and ClinicNum match.
				CurrentSplits=listPaySplits.FindAll(x => listAccountEntries.Any(accountEntry => accountEntry.ProcNum > 0 
						&& accountEntry.ProcNum==x.ProcNum 
						&& accountEntry.PatNum==x.PatNum 
						&& accountEntry.ProvNum==x.ProvNum 
						&& accountEntry.ClinicNum == x.ClinicNum)
					)
					.Sum(x => x.SplitAmt);
				AmtEnd=listAccountEntries.Sum(x => x.AmountEnd);
			}
		}

	}
}