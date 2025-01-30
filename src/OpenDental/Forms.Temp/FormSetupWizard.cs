using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormSetupWizard:FormODBase {
	private List<SetupWizard.SetupWizClass> _listSetupWizClassesItems;

	public FormSetupWizard() {
		InitializeComponent();
	}

	private void FormSetupWizard_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillListSetupItems() {
		_listSetupWizClassesItems=
		[
			new SetupWizard.RegKeySetup(),
			new SetupWizard.FeatureSetup(),
			new SetupWizard.ProvSetup(),
			new SetupWizard.EmployeeSetup(),
			new SetupWizard.FeeSchedSetup()
		];
		if(true) {
			_listSetupWizClassesItems.Add(new SetupWizard.ClinicSetup());
		}
		_listSetupWizClassesItems.Add(new SetupWizard.OperatorySetup());
		_listSetupWizClassesItems.Add(new SetupWizard.PrinterSetup());
		_listSetupWizClassesItems.Add(new SetupWizard.DefinitionSetup());
		//_listSetupWizItems.Add(new SetupWizard.ScheduleSetup());
		//_listSetupWizItems.Add(new SetupWizard.CarrierSetup());
		//_listSetupWizItems.Add(new SetupWizard.ClearinghouseSetup());
		//Add more here.
	}

	private void FillGrid() {
		FillListSetupItems();
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		//ODGridColumn col = new ODGridColumn("Setup Item",250);
		gridMain.Columns.Add(new GridColumn("Setup Item",250));
		//col = new ODGridColumn("Status",100,HorizontalAlignment.Center);
		gridMain.Columns.Add(new GridColumn("Status",100,HorizontalAlignment.Center));
		//col = new ODGridColumn("?",35,HorizontalAlignment.Center);
		//col.ImageList=imageList1;
		gridMain.Columns.Add(new GridColumn("?",35,HorizontalAlignment.Center) { ImageList=imageList1 });
		gridMain.ListGridRows.Clear();
		//Add the method rows to the grid.
		var listRows = ConstructGridRows();
		for(var i=0;i<listRows.Count;i++) {
			gridMain.ListGridRows.Add(listRows[i]);
		}
		gridMain.EndUpdate();
	}

	private List<GridRow> ConstructGridRows() {
		//the Tag of a Parent Row is its ODSetupCategory.
		//the Tag of a Child Row is a SetupWizClass
		var listRowsSetup = new List<GridRow>();
		var listRowsCategory = new List<GridRow>();
		var listRowsAll = new List<GridRow>();
		var statusCellNum = 0;
		for(var i=0;i<_listSetupWizClassesItems.Count;i++) {
			var row = new GridRow();
			row.Cells.Add("     "+_listSetupWizClassesItems[i].Name);
			row.Cells.Add(_listSetupWizClassesItems[i].GetStatus.GetDescription());
			statusCellNum=row.Cells.Count-1;
			row.Cells[statusCellNum].ColorBackG = SetupWizard.GetColor(_listSetupWizClassesItems[i].GetStatus);
			row.Cells.Add("0");
			//row.ColorBackG=SetupWizard.GetColor(setupItem.GetStatus);
			row.Tag=_listSetupWizClassesItems[i];
			listRowsSetup.Add(row);
		}
		//now add parent rows to the list
		for(var i=0;i<listRowsSetup.Count;i++) {
			var odSetupCategory = ((SetupWizard.SetupWizClass)listRowsSetup[i].Tag).GetCategory;
			//bool exists = false;
			////if the parent row doesn't exist..
			//foreach(ODGridRow parentRow in listCategoryRows) {
			//	if(parentRow.Tag.GetType() == typeof(ODSetupCategory)
			//		&& ((ODSetupCategory)parentRow.Tag) == catCur) {
			//		exists=true;
			//		break;
			//	}
			//}
			if(listRowsCategory.Any(x => x.Tag is ODSetupCategory && (ODSetupCategory)x.Tag == odSetupCategory)) {
				continue;
			}
			//add the parent row.
			var row = new GridRow();
			row.Cells.Add("\r\n"+odSetupCategory.GetDescription()+"\r\n");
			row.Cells.Add("");
			row.Cells.Add("");
			row.Tag=odSetupCategory;
			row.Bold=true;
			//row.ColorLborder=Color.Black;
			listRowsCategory.Add(row);
			//}
			////for all children rows, find the parent row -- set it to the proper parent row.
			//foreach(ODGridRow parentRow in listParentRows) {
			//	if(parentRow.Tag.GetType() == typeof(ODSetupCategory)
			//		&& ((ODSetupCategory)parentRow.Tag) == catCur) {
			//		rowCur.DropDownParent=parentRow;
			//		break;
			//	}
			//}
		}
		//Assign colors to parent rows.
		for (var i=0;i<listRowsCategory.Count();i++) {
			if(listRowsSetup.FindAll(x => ((SetupWizard.SetupWizClass)x.Tag).GetCategory == ((ODSetupCategory)listRowsCategory[i].Tag))
			   .All(x => ((SetupWizard.SetupWizClass)x.Tag).GetStatus == ODSetupStatus.Complete || ((SetupWizard.SetupWizClass)x.Tag).GetStatus == ODSetupStatus.Optional))
			{
				listRowsCategory[i].Cells[statusCellNum].Text="\r\n"+ODSetupStatus.Complete.GetDescription();
				listRowsCategory[i].Cells[statusCellNum].ColorBackG=SetupWizard.GetColor(ODSetupStatus.Complete);
					
			}
			else {
				listRowsCategory[i].Cells[statusCellNum].Text="\r\n"+ODSetupStatus.NeedsAttention.GetDescription();
				listRowsCategory[i].Cells[statusCellNum].ColorBackG=SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
			}
		}
		for (var i=0;i<listRowsCategory.Count();i++) {
			listRowsAll.Add(listRowsCategory[i]);
			listRowsSetup.FindAll(x => ((SetupWizard.SetupWizClass)x.Tag).GetCategory == ((ODSetupCategory)listRowsCategory[i].Tag)).DefaultIfEmpty(new GridRow()).LastOrDefault().ColorLborder=Color.Black;
			listRowsAll.AddRange(listRowsSetup.FindAll(x => ((SetupWizard.SetupWizClass)x.Tag).GetCategory == ((ODSetupCategory)listRowsCategory[i].Tag)));
		}
		return listRowsAll;
	}

	private void gridMain_CellClick(object sender,ODGridClickEventArgs e) {
		var rowClicked = gridMain.ListGridRows[e.Row];
		var colClicked = gridMain.Columns[e.Col];
		if(rowClicked.Tag.GetType() == typeof(ODSetupCategory)) {
			for(var i = 0;i < gridMain.ListGridRows.Count;i++) {
				var row = gridMain.ListGridRows[i];
				if(row.Tag is SetupWizard.SetupWizClass
				   && ((SetupWizard.SetupWizClass)row.Tag).GetCategory == (ODSetupCategory)rowClicked.Tag) {
					gridMain.SetSelected(i,true);
				}
			}
			return;
		}
		if(rowClicked.Tag.GetType().BaseType != typeof(SetupWizard.SetupWizClass)
		   || colClicked.ImageList == null) {
			return;
		}
		MsgBox.Show(this,((SetupWizard.SetupWizClass)rowClicked.Tag).GetDescript);
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		//Show a "Congatulations, you've already finished this!" section for finished sections.
		var rowClicked = gridMain.ListGridRows[e.Row];
		var listSetupWizClasses = new List<SetupWizard.SetupWizClass>();
		if(rowClicked.Tag.GetType().BaseType != typeof(SetupWizard.SetupWizClass)) { //category clicked
			for (var i=0;i<_listSetupWizClassesItems.Count();i++) {
				if(_listSetupWizClassesItems[i].GetCategory != (ODSetupCategory)rowClicked.Tag) {
					continue;
				}
				var setupIntroCat = new SetupWizard.SetupIntro(_listSetupWizClassesItems[i].Name, _listSetupWizClassesItems[i].GetDescript);
				var setupCompleteCat = new SetupWizard.SetupComplete(_listSetupWizClassesItems[i].Name);
				listSetupWizClasses.Add(setupIntroCat);
				listSetupWizClasses.Add(_listSetupWizClassesItems[i]);
				listSetupWizClasses.Add(setupCompleteCat);
			}
			RemoveUnauthorizedWizClasses(listSetupWizClasses);
			if(listSetupWizClasses.Count == 0) {
				return;
			}
			using var formSetupWizardProgressCat=new FormSetupWizardProgress(listSetupWizClasses,true);
			formSetupWizardProgressCat.ShowDialog();
			FillGrid();
			return;
		}
		//single row clicked
		var setupWizClass = (SetupWizard.SetupWizClass)rowClicked.Tag;
		var setupIntro = new SetupWizard.SetupIntro(setupWizClass.Name,setupWizClass.GetDescript);
		var setupComplete = new SetupWizard.SetupComplete(setupWizClass.Name);
		listSetupWizClasses.Add(setupIntro);
		listSetupWizClasses.Add(setupWizClass);
		listSetupWizClasses.Add(setupComplete);
		RemoveUnauthorizedWizClasses(listSetupWizClasses);
		if(listSetupWizClasses.Count == 0) {
			return;
		}
		using var formSetupWizardProgress=new FormSetupWizardProgress(listSetupWizClasses,false);
		formSetupWizardProgress.ShowDialog();
		FillGrid();
	}

	private void butAll_Click(object sender,EventArgs e) {
		var listSetupWizClasses = new List<OpenDental.SetupWizard.SetupWizClass>();
		for(var i=0;i<_listSetupWizClassesItems.Count();i++) {
			var setupIntro = new SetupWizard.SetupIntro(_listSetupWizClassesItems[i].Name,_listSetupWizClassesItems[i].GetDescript);
			var setupComplete = new SetupWizard.SetupComplete(_listSetupWizClassesItems[i].Name);
			listSetupWizClasses.Add(setupIntro);
			listSetupWizClasses.Add(_listSetupWizClassesItems[i]);
			listSetupWizClasses.Add(setupComplete);
		}
		RemoveUnauthorizedWizClasses(listSetupWizClasses);
		using var formSetupWizardProgress = new FormSetupWizardProgress(listSetupWizClasses,true);
		formSetupWizardProgress.ShowDialog();
		FillGrid();
	}

	private void butSelected_Click(object sender,EventArgs e) {
		var listSetupWizClasses = new List<SetupWizard.SetupWizClass>();
		for(var i=0;i<gridMain.SelectedIndices.Count();i++) {
			var gridRowSelected = gridMain.ListGridRows[gridMain.SelectedIndices[i]];
			if(gridRowSelected.Tag.GetType().BaseType != typeof(OpenDental.SetupWizard.SetupWizClass)) {
				continue;
			}
			var setupWizClass = (SetupWizard.SetupWizClass)gridRowSelected.Tag;
			var setupIntro = new SetupWizard.SetupIntro(setupWizClass.Name,setupWizClass.GetDescript);
			var setupComplete = new SetupWizard.SetupComplete(setupWizClass.Name);
			listSetupWizClasses.Add(setupIntro);
			listSetupWizClasses.Add(setupWizClass);
			listSetupWizClasses.Add(setupComplete);
		}
		RemoveUnauthorizedWizClasses(listSetupWizClasses);
		if(listSetupWizClasses.Count == 0) {
			return;
		}
		using var formSetupWizardProgress = new FormSetupWizardProgress(listSetupWizClasses,false);
		formSetupWizardProgress.ShowDialog();
	}

	public void RemoveUnauthorizedWizClasses(List<SetupWizard.SetupWizClass> listSetupClasses) {
		//Check permissions for each SetUpWizClass and remove from list if permissions are missing
		var message = Lans.g("Security","Not authorized for")+"\r\n";
		var didRemove = false;
		if(!Security.IsAuthorized(EnumPermType.ShowFeatures,true) && listSetupClasses.Any(x => x.Name =="Basic Features")) {
			listSetupClasses.RemoveAll(x => x.Name == "Basic Features");
			message += "\r\n" + GroupPermissions.GetDesc(EnumPermType.ShowFeatures);
			didRemove=true;
		}
		if(!Security.IsAuthorized(EnumPermType.ProviderEdit,true) && listSetupClasses.Any(x => x.Name =="Providers")) {
			listSetupClasses.RemoveAll(x => x.Name == "Providers");
			message += "\r\n" + GroupPermissions.GetDesc(EnumPermType.ProviderEdit);
			didRemove=true;
		}
		if(!Security.IsAuthorized(EnumPermType.ClinicEdit,true) && listSetupClasses.Any(x => x.Name =="Clinics")) {
			listSetupClasses.RemoveAll(x => x.Name == "Clinics");
			message += "\r\n" + GroupPermissions.GetDesc(EnumPermType.ClinicEdit);
			didRemove=true;
		}
		if(!Security.IsAuthorized(EnumPermType.PrinterSetup,true) && listSetupClasses.Any(x => x.Name =="Printer/Scanner")) {
			listSetupClasses.RemoveAll(x => x.Name == "Printer/Scanner");
			message += "\r\n" + GroupPermissions.GetDesc(EnumPermType.PrinterSetup);
			didRemove=true;
		}
		if(didRemove) {
			ODMessageBox.Show(message);
		}
	}

}