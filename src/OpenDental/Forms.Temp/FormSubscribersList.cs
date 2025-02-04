using System;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormSubscribersList:FormODBase {
	///<summary>Ins sub for the currently selected plan.</summary>
	public InsSub InsSubCur;
	///<summary>Currently selected plan in the window.</summary>
	public InsPlan InsPlanCur;

	public FormSubscribersList() {
		InitializeComponent();
	}

	private void FormSubscribersList_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillGrid() {
		gridSubscribers.BeginUpdate();
		gridSubscribers.Columns.Clear();
		gridSubscribers.Columns.Add(new GridColumn(Lan.g(this,"Name"),200));
		gridSubscribers.ListGridRows.Clear();
		long excludeSub=-1;
		if(InsSubCur!=null){
			excludeSub=InsSubCur.InsSubNum;
		}
		var listSubs=InsSubs.GetSubscribersForPlan(InsPlanCur.PlanNum,excludeSub);
		for(var i=0;i<listSubs.Count;i++) {
			gridSubscribers.ListGridRows.Add(new GridRow(listSubs[i]));
		}
		gridSubscribers.EndUpdate();
	}

}