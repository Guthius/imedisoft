using CodeBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormMultiVisitGroup:FormODBase {

	/// <summary> If true, then this form removed one or more ProcMultiVisit items from the db </summary>
	public bool Changed=false;
	//public List<ProcMultiVisit> ListProcMultiVisits;
	public List<DataRow> ListDataRows;
	/// <summary> The function that builds a grid row for display, given an input DataRow </summary>
	public Func<DataRow,GridRow> FuncBuildGridRow;
	public List<GridColumn> ListGridColumns;
	public List<DisplayField> ListDisplayFieldsGrid;
	public string GridTitle;

	public FormMultiVisitGroup() {
		InitializeComponent();
	}

	private void FormMultiVisitGroup_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillGrid() {
		gridGroupedProcs.BeginUpdate();
		gridGroupedProcs.ListGridRows.Clear();
		gridGroupedProcs.Columns.Clear();
		gridGroupedProcs.Title=GridTitle;
		for(var i=0;i<ListGridColumns.Count;i++) {
			gridGroupedProcs.Columns.Add(ListGridColumns[i]);
		}
		for(var i=0;i<ListDataRows.Count;i++) {
			var gridRowFromDataRow=FuncBuildGridRow(ListDataRows[i]);
			gridGroupedProcs.ListGridRows.Add(gridRowFromDataRow);
		}
		gridGroupedProcs.EndUpdate();
		gridGroupedProcs.SetAll(true);//So the user can ungroup all when opening the form
	}

	private void gridGroupedProcs_Click(object sender,EventArgs e) {
		if(gridGroupedProcs.SelectedIndices.Length==0) {
			butUngroup.Enabled=false;
		}
		else {
			butUngroup.Enabled=true;
		}
	}

	private void butUngroup_Click(object sender,EventArgs e) {
		Changed=true;
		if(gridGroupedProcs.ListGridRows.Count-gridGroupedProcs.SelectedIndices.Length==1) {//One is not selected
			//Select all procedures in the form
			for(var i=0;i<gridGroupedProcs.ListGridRows.Count;i++) {
				gridGroupedProcs.SetSelected(i,true);
			}
		}
		//Get ProcNum of every procedure on the grid
		var longArrayProcNumsAll=new long[gridGroupedProcs.ListGridRows.Count];
		for(var i=0;i<gridGroupedProcs.ListGridRows.Count;i++) {
			var row=(DataRow)gridGroupedProcs.ListGridRows[i].Tag;
			longArrayProcNumsAll[i]=SIn.Long(row["ProcNum"].ToString());
		}
		var listProcMultiVisitsForGroup=ProcMultiVisits.GetGroupsForProcsFromDb(longArrayProcNumsAll);
		var isGroupInProcessOld=ProcMultiVisits.IsGroupInProcess(listProcMultiVisitsForGroup);
		var groupProcMultiVisitNum=listProcMultiVisitsForGroup.First().GroupProcMultiVisitNum;
		//Get the ProcNum of each selected procedure
		var longArrayProcNumsSelected=new long[gridGroupedProcs.SelectedIndices.Length];
		for(var i=0;i<gridGroupedProcs.SelectedIndices.Length;i++) {
			var row=(DataRow)gridGroupedProcs.ListGridRows[gridGroupedProcs.SelectedIndices[i]].Tag;
			longArrayProcNumsSelected[i]=SIn.Long(row["ProcNum"].ToString());
		}
		var isInvalid=false;
		//Get the ProcMultiVisit associated with each procedure
		var listProcMultiVisits=listProcMultiVisitsForGroup.FindAll(x=>longArrayProcNumsSelected.Contains(x.ProcNum));
		for(var i=0;i<longArrayProcNumsSelected.Length;i++) {
			var procMultiVisit=listProcMultiVisits.FirstOrDefault(x => x.ProcNum==longArrayProcNumsSelected[i]);
			if(procMultiVisit!=null) {//Could have been already ungrouped in another window/pc, possibly
				ProcMultiVisits.Delete(procMultiVisit.ProcMultiVisitNum);
				listProcMultiVisitsForGroup.RemoveAll(x=>x.ProcMultiVisitNum==procMultiVisit.ProcMultiVisitNum);
				isInvalid=true;
			}
			//Remove the procedure rows from this form
			ListDataRows.RemoveAll(row => SIn.Long(row["ProcNum"].ToString())==longArrayProcNumsSelected[i]);
		}
		//Check to see if the group is still in process after removals, updating the pmvs if so
		var isGroupInProcess=ProcMultiVisits.IsGroupInProcess(listProcMultiVisitsForGroup);
		if(isGroupInProcessOld!=isGroupInProcess) {
			ProcMultiVisits.UpdateInProcessForGroup(groupProcMultiVisitNum,isGroupInProcess);
		}
		//Signal that one or more ProcMultiVisit objects were removed from the DB
		if(isInvalid) {
			Signalods.SetInvalid(InvalidType.ProcMultiVisits);
		}
		ProcMultiVisits.RefreshCache();
		//Change status of claims if necessary
		var listClaimProcs=ClaimProcs.GetForProcs(longArrayProcNumsAll.ToList());
		var listClaims=Claims.GetClaimsFromClaimNums(listClaimProcs.Select(x=>x.ClaimNum).ToList());
		for(var i=0;i<listClaims.Count;i++) {
			//If a ClaimStatus is not "U", "W", "I", or "H", we should not be changing it.
			if(!listClaims[i].ClaimStatus.In("U","W","I","H")) {
				continue;
			}
			var claimOld=listClaims[i].Copy();
			var listClaimProcsForClaim=ClaimProcs.RefreshForClaim(listClaims[i].ClaimNum);
			if(listClaimProcsForClaim.Count==0) {//This is rare but still happens.  See DBM. 
				continue;
			}
			var isProcsInProcess=listClaimProcsForClaim.Exists(x=>ProcMultiVisits.IsProcInProcess(x.ProcNum));
			if(isProcsInProcess) {
				listClaims[i].ClaimStatus="I";
			}
			else {
				listClaims[i].ClaimStatus="W";
				if(listClaims[i].ClaimType!="P") { //If this claim isn't primary, we need to see if a primary one is also attached to this procedure
					var listProcNums=listClaimProcsForClaim.Select(x=>x.ProcNum).ToList();
					var listClaimProcsForProc=ClaimProcs.GetForProcs(listProcNums);
					var listClaimsForClaimProcs=Claims.GetClaimsFromClaimNums(listClaimProcsForProc.Select(x=>x.ClaimNum).Distinct().ToList());
					if(listClaimsForClaimProcs.Exists(x => x.ClaimType=="P")) { //If a primary claim is also attached
						listClaims[i].ClaimStatus="H"; //Set the status to Hold Until Pri Received instead of Waiting to Send
					}
				}
			}
			Claims.Update(listClaims[i],claimOld);
		}
		if(ListDataRows.Count==0) {
			MsgBox.Show(this,"All procedures removed from group.");
			Close();
		}
		else {
			MsgBox.Show(this,"Procedure(s) removed from group.");
			FillGrid();
		}
	}

}