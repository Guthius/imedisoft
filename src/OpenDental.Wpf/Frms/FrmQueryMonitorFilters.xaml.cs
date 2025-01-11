using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;
using Newtonsoft.Json;
using Microsoft.Win32;
using CodeBase;

namespace OpenDental {
	
	public partial class FrmQueryMonitorFilters : FrmODBase {
		public List<QueryFilter> ListQueryFilters;
		private string _filterGroup="None";

		#region Constructor
		public FrmQueryMonitorFilters() {
			InitializeComponent();
			Load+=Frm_Load;
			FormClosed+=FrmQueryMonitorFilters_FormClosed;
			gridMain.CellDoubleClick+=gridMain_CellDoubleClick;
			comboGroupName.SelectionChangeCommitted+=comboGroupName_SelectionChangeCommitted;
			FormClosing+=FrmQueryMonitorFilters_FormClosing;
			PreviewKeyDown+=FrmQueryMonitorFilters_PreviewKeyDown;
		}

		private void FrmQueryMonitorFilters_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butAdd.IsAltKey(Key.A,e)){
				butAdd_Click(this,new EventArgs());
			}
			if(butImport.IsAltKey(Key.I,e)){
				butImport_Click(this,new EventArgs());
			}
			if(butExport.IsAltKey(Key.E,e)){
				butExport_Click(this,new EventArgs());
			}
		}
		#endregion

		#region Event Handlers
		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			if(QueryFilters.QueryFilterGroup!=null){
				_filterGroup=QueryFilters.QueryFilterGroup;
			}
			FillGrid();
			FillComboBox();
		}

		private void FillGrid(){
			ListQueryFilters=QueryFilters.GetAll();
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn gridColumn=new GridColumn(Lans.g("Filters","Group"),120);
			gridMain.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lans.g("Filters","Filter"),200);
			gridMain.Columns.Add(gridColumn);
			gridMain.ListGridRows.Clear();
			GridRow gridRow;
			for(int i=0;i<ListQueryFilters.Count;i++){
				if(ListQueryFilters[i].GroupName!=_filterGroup){
					continue;
				}
				gridRow=new GridRow();
				gridRow.Cells.Add(ListQueryFilters[i].GroupName);
				gridRow.Cells.Add(ListQueryFilters[i].FilterText);
				gridRow.Tag=ListQueryFilters[i];
				gridMain.ListGridRows.Add(gridRow);
			}
			gridMain.EndUpdate();
		}

		private void butAdd_Click(object sender,EventArgs e) {
			if(_filterGroup=="None"){
				MsgBox.Show(this,"Cannot add filters for the None group");
				return;
			}
			FrmQueryMonitorFilterEdit frmQueryMonitorFilterEdit=new FrmQueryMonitorFilterEdit();
			frmQueryMonitorFilterEdit.QueryFilterCur=new QueryFilter();
			frmQueryMonitorFilterEdit.QueryFilterCur.GroupName=_filterGroup;
			frmQueryMonitorFilterEdit.QueryFilterCur.IsNew=true;
			frmQueryMonitorFilterEdit.ShowDialog();
			if(frmQueryMonitorFilterEdit.IsDialogCancel){
				return;
			}
			_filterGroup=frmQueryMonitorFilterEdit.QueryFilterCur.GroupName;
			FillGrid();
			FillComboBox();
		}

		private void butImport_Click(object sender,EventArgs e) {
			try {
				_filterGroup=ImportJsonFile();
			}
			catch(Exception ex) {
				FriendlyException.Show(Lang.g(this,"Import failed."),ex);
				return;
			}
			MsgBox.Show(this,"Import successful");
			FillGrid();
			FillComboBox();
		}

		///<summary>Throws exceptions. Returns the group name of the newly imported query filters.</summary>
		private string ImportJsonFile() {
			//We considered the following strategies for import.
			//We settled on 4.
			//Scenario 1: Add all filters to the currently selected group if we import.
					//We can just make a message box for the current logic to do this or to do the current behavior.
					//This can easily be done by just ignoring the group associated with the import and only taking the filter.
			//Scenario 2: Add in each filter to the group name that's in the import. Create new group if needed.
					//This is the current behavior
			//Scenario 3: Replace all existing filters inside of each group if we import. EX: Group 1 has 3 filters already but we import 5 filters for group 1. If we replace them all we'll have 5 filters instead of 8 since we're deleting the 3 already existing ones and replacing them.
					//This can just be done by deleting all the filters for groups that we import (Or maybe just clearing the filters in general too), then doing what we're already doing here.
			//Scenario 4: Make a brand new import group with a unique name and add all the filters to that group. Ignore group name in import.
					//This is also easy to do since we just make a new group called something like "Import" then stick all the filters in there.
			OpenFileDialog openFileDialog=new OpenFileDialog();
			openFileDialog.DefaultExt = ".json";
			openFileDialog.Filter="JSON Files|*.json";
			bool success=(bool)openFileDialog.ShowDialog();
			if(!success){
				throw new ODException(Lang.g(this,"User cancelled out of the import process."));
			}
			List<QueryFilter> listQueryFiltersImport=new List<QueryFilter>();
			string fileContents=File.ReadAllText(openFileDialog.FileName);
			listQueryFiltersImport=JsonConvert.DeserializeObject<List<QueryFilter>>(fileContents);
			listQueryFiltersImport.RemoveAll(x=>x.FilterText=="");//Remove empty entries
			string groupname="New Import";
			int increment=1;//Starting at 1 so when it is incremented, it'll be at 2 for the first time it's used.
			while(true){
				if(!ListQueryFilters.Select(x=>x.GroupName).Any(x=>x==groupname)){//If the group name doesn't exist
					break;//done
				}
				increment++;
				groupname="New Import "+increment;
			}
			//Add in the filters that were imported
			for(int i=0;i<listQueryFiltersImport.Count;i++){
				listQueryFiltersImport[i].GroupName=groupname;
				QueryFilters.Insert(listQueryFiltersImport[i]);
			}
			return groupname;
		}

		private void butExport_Click(object sender,EventArgs e) {
			try {
				ExportJsonFile();
			}
			catch(Exception ex) {
				FriendlyException.Show(Lang.g(this,"Export failed."),ex);
				return;
			}
			MsgBox.Show(this,"Export successful");
		}

		private void ExportJsonFile() {
			SaveFileDialog saveFileDialog=new SaveFileDialog();
			saveFileDialog.FileName="Query filters";
			saveFileDialog.DefaultExt=".json";
			bool success=(bool)saveFileDialog.ShowDialog();
			if(!success){
				return;
			}
			List<QueryFilter> listQueryFilters=ListQueryFilters.FindAll(x=>x.GroupName==_filterGroup);
			string jsonString=JsonConvert.SerializeObject(listQueryFilters);
			File.WriteAllText(saveFileDialog.FileName,jsonString);
		}

		private void gridMain_CellDoubleClick(object sender,GridClickEventArgs e) {
			FrmQueryMonitorFilterEdit frmQueryMonitorFilterEdit=new FrmQueryMonitorFilterEdit();
			//Finds the currently selected filter. We are using tags here because the grid is a subset of the list of all filters. The grid only shows one group at a time.
			frmQueryMonitorFilterEdit.QueryFilterCur=gridMain.SelectedTag<QueryFilter>();
			frmQueryMonitorFilterEdit.ShowDialog();
			if(frmQueryMonitorFilterEdit.IsDialogCancel){
				return;
			}
			_filterGroup=frmQueryMonitorFilterEdit.QueryFilterCur.GroupName;
			FillGrid();
			FillComboBox();
		}

		private void comboGroupName_SelectionChangeCommitted(object sender,EventArgs e) {
			_filterGroup=comboGroupName.GetSelected<string>();
			FillGrid();
		}

		private void FrmQueryMonitorFilters_FormClosed(object sender,EventArgs e) {
			QueryFilters.QueryFilterGroup=_filterGroup;
		}

		private void butNewGroup_Click(object sender,EventArgs e) {
			InputBox inputBox=new InputBox(Lang.g(this,"New group name"));
			inputBox.ShowDialog();
			if(inputBox.IsDialogCancel){
				return;
			}
			string stringResult=inputBox.StringResult;
			if(stringResult==null){
				return;
			}
			if(stringResult=="None"){
				MsgBox.Show(this,"Cannot add a None group");
				return;
			}
			if(ListQueryFilters.Select(x=>x.GroupName).Any(x=>x==stringResult)){
				MsgBox.Show(this,"Group name already exists.");
				return;
			}
			//Need to insert a blank query otherwise the group doesn't show up in the combobox since it doesn't exist in the database yet.
			_filterGroup=stringResult;
			QueryFilter queryFilter=new QueryFilter();
			queryFilter.FilterText="";
			queryFilter.GroupName=_filterGroup;
			QueryFilters.Insert(queryFilter);
			FillGrid();
			FillComboBox();
		}

		private void butChangeName_Click(object sender,EventArgs e) {
			if(_filterGroup=="None"){
				MsgBox.Show(this,"Cannot change the name of the None group.");
				return;
			}
			InputBox inputBox=new InputBox(Lang.g(this,"Group name"),_filterGroup);
			inputBox.ShowDialog();
			if(inputBox.IsDialogCancel){
				return;
			}
			string stringResult=inputBox.StringResult;
			if(stringResult==null){
				return;
			}
			if(stringResult=="None"){
				MsgBox.Show(this,"Cannot rename a group to None");
				return;
			}
			if(ListQueryFilters.Select(x=>x.GroupName).Any(x=>x==stringResult)){
				MsgBox.Show(this,"Cannot rename this group to an already existing group.");
				return;
			}
			List<QueryFilter> listQueryFiltersToUpdate=ListQueryFilters.FindAll(x=>x.GroupName==_filterGroup);
			for(int i=0;i<listQueryFiltersToUpdate.Count;i++){
				listQueryFiltersToUpdate[i].GroupName=stringResult;
				QueryFilters.Update(listQueryFiltersToUpdate[i]);
			}
			_filterGroup=stringResult;
			FillGrid();
			FillComboBox();
		}

		private void FrmQueryMonitorFilters_FormClosing(object sender,System.ComponentModel.CancelEventArgs e) {
			//Refresh the list and add the current filters in the current group to it.
			ListQueryFilters=QueryFilters.GetForGroup(_filterGroup);
		}
		#endregion

		#region Methods - Private
		///<summary>Should be called after the fill grid or if listQueryFilters and _filterGroup have been updated with the latest information</summary>
		private void FillComboBox(){
			comboGroupName.Items.Clear();
			comboGroupName.Items.Add("None");
			comboGroupName.SelectedIndex=0;
			List<string> listQueryFilterStrings=ListQueryFilters.Select(x => x.GroupName).Distinct().ToList();
			for(int i=0;i<listQueryFilterStrings.Count;i++) {
				comboGroupName.Items.Add(listQueryFilterStrings[i]);
				if(_filterGroup==listQueryFilterStrings[i]) {
					comboGroupName.SetSelected(i+1);//Add one for None.
				}
			}
		}
		#endregion

	}
}