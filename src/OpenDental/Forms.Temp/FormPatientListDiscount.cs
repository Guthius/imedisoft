using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormPatientListDiscount:FormODBase {
	public DiscountPlan DiscountPlanCur;
	public List<string> ListPatNames;

	public FormPatientListDiscount() {
		InitializeComponent();
	}

	private void FormPatientListDiscount_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillGrid() {
		if(ListPatNames==null) {
			ListPatNames=DiscountPlans.GetPatNamesForPlan(DiscountPlanCur.DiscountPlanNum)
				.Distinct()
				.OrderBy(x => x)
				.ToList();
		}
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		GridColumn col;
		col=new GridColumn(Lan.g(this,"Name"),100);
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		for(var i=0;i<ListPatNames.Count;i++) {
			var row=new GridRow(ListPatNames[i]);
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

}