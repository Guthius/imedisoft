using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormOperatoryPick:FormODBase {

	///<summary>After this window closes, this will be the OperatoryNum of the selected operatory.</summary>
	public long OperatoryNumSelected;
	///<summary>Passed in list of operatories shown to user.</summary>
	private List<Operatory> _listOperatories;

	public FormOperatoryPick(List<Operatory> listOperatories) {
		InitializeComponent();

		_listOperatories=listOperatories.Select(x=>x.Copy()).ToList();
	}
		
	private void FormOperatoryPick_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(!SelectOperatory()) {
			return;
		}
		DialogResult=DialogResult.OK;
	}

	private void FillGrid(){
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		var opNameWidth=180;
		var clinicWidth=85;
		var col=new GridColumn(Lan.g("TableOperatories","Op Name"),opNameWidth);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOperatories","Abbrev"),70);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOperatories","IsHidden"),64,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		if(true) {
			col=new GridColumn(Lan.g("TableOperatories","Clinic"),clinicWidth);
			gridMain.Columns.Add(col);
		}
		col=new GridColumn(Lan.g("TableOperatories","Provider"),70);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOperatories","Hygienist"),70);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOperatories","IsHygiene"),64,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOperatories","IsWebSched"),74,HorizontalAlignment.Center);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g("TableOperatories","IsNewPat"),50,HorizontalAlignment.Center){ IsWidthDynamic=true };
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		GridRow row;
		var listOperatoryNumsWSNPA=Operatories.GetOpsForWebSchedNewOrExistingPatAppts().Select(x => x.OperatoryNum).ToList();
		for(var i=0;i<_listOperatories.Count;i++) {
			row=new GridRow();
			row.Cells.Add(_listOperatories[i].OpName);
			row.Cells.Add(_listOperatories[i].Abbrev);
			if(_listOperatories[i].IsHidden){
				row.Cells.Add("X");
			}
			else{
				row.Cells.Add("");
			}
			if(true) {
				row.Cells.Add(Clinics.GetAbbr(_listOperatories[i].ClinicNum));
			}
			row.Cells.Add(Providers.GetAbbr(_listOperatories[i].ProvDentist));
			row.Cells.Add(Providers.GetAbbr(_listOperatories[i].ProvHygienist));
			if(_listOperatories[i].IsHygiene){
				row.Cells.Add("X");
			}
			else{
				row.Cells.Add("");
			}
			row.Cells.Add(_listOperatories[i].IsWebSched?"X":"");
			row.Cells.Add(listOperatoryNumsWSNPA.Contains(_listOperatories[i].OperatoryNum) ? "X" : "");
			row.Tag=_listOperatories[i];
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

	///<summary>Returns true if there was an operatory selected.</summary>
	private bool SelectOperatory() {
		if(gridMain.GetSelectedIndex()==-1){
			ODMessageBox.Show(Lan.g(this,"Please select an item first."));
			return false;
		}
		OperatoryNumSelected=((Operatory)gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag).OperatoryNum;
		return true;
	}
		
	private void butOK_Click(object sender,EventArgs e) {
		if(!SelectOperatory()) {
			return;
		}
		DialogResult=DialogResult.OK;
	}

}