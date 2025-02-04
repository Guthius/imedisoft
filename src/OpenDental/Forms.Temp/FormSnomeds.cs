using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental;

public partial class FormSnomeds:FormODBase {
	public bool IsSelectionMode;
	public bool IsMultiSelectMode;
	public Snomed SnomedSelected;
	public List<Snomed> ListSnomedsSelected;
	private List<Snomed> _listSnomeds;

	public FormSnomeds() {
		InitializeComponent();
	}

	private void FormSnomeds_Load(object sender,EventArgs e) {
		if(!IsSelectionMode && !IsMultiSelectMode) {
			butOK.Visible=false;
		}
		if(IsMultiSelectMode) {
			gridMain.SelectionMode=GridSelectionMode.MultiExtended;
		}
		ActiveControl = textCode;
	}
		
	private void butSearch_Click(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillGrid() {
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		GridColumn col;
		col=new GridColumn(Lan.g(this,"SNOMED CT"),125);//column width of 125 holds the longest Snomed CT code as of 8/7/15 which is 900000000000002006
		gridMain.Columns.Add(col);
		//col=new ODGridColumn("Deprecated",75,HorizontalAlignment.Center);
		//gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Description"),500);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Used By CQM's"),185);//width 185 so all of our CQM measure nums as of 8/7/15 will fit 68,69,74,75,127,138,147,155,165
		gridMain.Columns.Add(col);
		//col=new ODGridColumn("Date Of Standard",100);
		//gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		GridRow row;
		if(textCode.Text.Contains(",")) {
			_listSnomeds=Snomeds.GetByCodes(textCode.Text);
		}
		else {
			_listSnomeds=Snomeds.GetByCodeOrDescription(textCode.Text);
		}
		if(_listSnomeds.Count>=10000) {//Max number of results returned.
			MsgBox.Show(this,"Too many results. Only the first 10,000 results will be shown.");
		}
		var listGridRowsAll=new List<GridRow>();
		for(var i=0;i<_listSnomeds.Count;i++) {
			row=new GridRow();
			row.Cells.Add(_listSnomeds[i].SnomedCode);
			//row.Cells.Add("");//IsActive==NotDeprecated
			row.Cells.Add(_listSnomeds[i].Description);
			row.Cells.Add("");
			row.Tag=_listSnomeds[i];
			//row.Cells.Add("");
			listGridRowsAll.Add(row);
		}
		listGridRowsAll.Sort(SortMeasuresMet);
		for(var i=0;i<listGridRowsAll.Count;i++) {
			gridMain.ListGridRows.Add(listGridRowsAll[i]);
		}
		gridMain.EndUpdate();
	}

	///<summary>Sort function to put the codes that apply to the most number of CQM's at the top so the user can see which codes they should select.</summary>
	private int SortMeasuresMet(GridRow row1,GridRow row2) {
		//int i=(CDSPermissions.GetForUser(Security.CurUser.UserNum).ShowInfobutton?1:0);//used to accomodate infobutton column.
		//First sort by the number of measures the codes apply to in a comma delimited list
		var diff=row2.Cells[2].Text.Split([","],StringSplitOptions.RemoveEmptyEntries).Length
		         -row1.Cells[2].Text.Split([","],StringSplitOptions.RemoveEmptyEntries).Length;
		if(diff!=0) {
			return diff;
		}
		try {
			//if the codes apply to the same number of CQMs, order by the code values
			//return PIn.Long(row1.Cells[2+_showingInfobuttonShift].Text).CompareTo(PIn.Long(row2.Cells[2+_showingInfobuttonShift].Text));
			//Just string compare
			return row1.Cells[2].Text.CompareTo(row2.Cells[2].Text);
		}
		catch(Exception) {
			return 0;
		}
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(!IsSelectionMode && !IsMultiSelectMode) {
			return;
		}
		SnomedSelected=(Snomed)gridMain.ListGridRows[e.Row].Tag;
		ListSnomedsSelected=
		[
			(Snomed) gridMain.ListGridRows[e.Row].Tag
		];
		DialogResult=DialogResult.OK;
	}

	private void butOK_Click(object sender,EventArgs e) {
		//not even visible unless IsSelectionMode
		if(gridMain.GetSelectedIndex()==-1) {
			MsgBox.Show(this,"Please select an item first.");
			return;
		}
		SnomedSelected=(Snomed)gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
		ListSnomedsSelected= [];
		for(var i=0;i<gridMain.SelectedIndices.Length;i++) {
			ListSnomedsSelected.Add((Snomed)gridMain.ListGridRows[gridMain.SelectedIndices[i]].Tag);
		}
		DialogResult=DialogResult.OK;
	}

}