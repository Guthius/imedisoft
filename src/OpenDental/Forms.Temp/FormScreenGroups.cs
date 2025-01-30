using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDental.UI;
using Screen = Imedisoft.Core.Entities.Screen;

namespace OpenDental;

/// <summary></summary>
public partial class FormScreenGroups:FormODBase {
	private List<ScreenGroup> _listScreenGroups;
	private DateTime _dateToday;

		
	public FormScreenGroups()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormScreenings_Load(object sender, System.EventArgs e) {
		_dateToday=DateTime.Today;
		textDateFrom.Text=DateTime.Today.ToShortDateString();
		textDateTo.Text=DateTime.Today.ToShortDateString();
		FillGrid();
	}

	private void FillGrid() {
		_listScreenGroups=ScreenGroups.Refresh(SIn.Date(textDateFrom.Text),SIn.Date(textDateTo.Text));
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		GridColumn col;
		col=new GridColumn(Lan.g(this,"Date"),70);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Description"),140);
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		GridRow row;
		for (var i=0;i<_listScreenGroups.Count;i++) {
			row=new GridRow();
			row.Cells.Add(_listScreenGroups[i].SGDate.ToShortDateString());
			row.Cells.Add(_listScreenGroups[i].Description);
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

	private void gridMain_CellDoubleClick(object sender,UI.ODGridClickEventArgs e) {
		using var formScreenGroupEdit=new FormScreenGroupEdit(_listScreenGroups[gridMain.GetSelectedIndex()]);
		formScreenGroupEdit.ShowDialog();
		FillGrid();
	}

	private void textDateFrom_Validating(object sender, System.ComponentModel.CancelEventArgs e) {
		if(textDateFrom.Text=="") {
			return;
		}
		try {
			DateTime.Parse(textDateFrom.Text);
		}
		catch {
			ODMessageBox.Show("Date invalid");
			e.Cancel=true;
		}
	}

	private void textDateTo_Validating(object sender, System.ComponentModel.CancelEventArgs e) {
		if(textDateTo.Text=="") {
			return;
		}
		try {
			DateTime.Parse(textDateTo.Text);
		}
		catch {
			ODMessageBox.Show("Date invalid");
			e.Cancel=true;
		}
	}

	private void butRefresh_Click(object sender, System.EventArgs e) {
		FillGrid();
	}

	private void butAdd_Click(object sender, System.EventArgs e) {
		var screenGroup=new ScreenGroup();
		if(_listScreenGroups.Count!=0) {
			screenGroup=_listScreenGroups[_listScreenGroups.Count-1];//'remembers' the last entry
		}
		screenGroup.SGDate=DateTime.Today;//except date will be today
		screenGroup.IsNew=true;
		using var formScreenGroupEdit=new FormScreenGroupEdit(screenGroup);
		formScreenGroupEdit.ShowDialog();
		FillGrid();
	}

	private void butToday_Click(object sender,EventArgs e) {
		_dateToday=DateTime.Today;
		textDateFrom.Text=DateTime.Today.ToShortDateString();
		textDateTo.Text=DateTime.Today.ToShortDateString();
	}

	private void butLeft_Click(object sender,EventArgs e) {
		_dateToday=_dateToday.AddDays(-1);
		textDateFrom.Text=_dateToday.ToShortDateString();
		textDateTo.Text=_dateToday.ToShortDateString();
	}

	private void butRight_Click(object sender,EventArgs e) {
		_dateToday=_dateToday.AddDays(1);
		textDateFrom.Text=_dateToday.ToShortDateString();
		textDateTo.Text=_dateToday.ToShortDateString();
	}

	private void butDelete_Click(object sender, System.EventArgs e) {
		if(gridMain.SelectedIndices.Length!=1){
			ODMessageBox.Show("Please select one item first.");
			return;
		}
		var screenGroup=_listScreenGroups[gridMain.GetSelectedIndex()];
		var listScreens=Screens.GetScreensForGroup(screenGroup.ScreenGroupNum);
		if(listScreens.Count>0) {
			ODMessageBox.Show("Not allowed to delete a screening group with items in it.");
			return;
		}
		ScreenGroups.Delete(screenGroup);
		FillGrid();
	}

}