using System;
using OpenDental.UI;
using System.Linq;
using Imedisoft.Core.Data;

namespace OpenDental;

public partial class FormUpdateHistory:FormODBase {

	public FormUpdateHistory() {
		InitializeComponent();
	}

	private void FormUpdateHistory_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillGrid() {
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		var col=new GridColumn(Lan.g(this,"Version"),117);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Date"),117);
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		GridRow row=null;
		var listUpdateHistories=UpdateHistories.GetAll().OrderByDescending(x => x.DateTimeUpdated).ToList();
		for(var i = 0;i<listUpdateHistories.Count;i++) {
			row=new GridRow();
			row.Cells.Add(listUpdateHistories[i].ProgramVersion);
			row.Cells.Add(listUpdateHistories[i].DateTimeUpdated.ToString());
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

}