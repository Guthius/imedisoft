using System;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormDashboardWidgets:FormODBase {
	///<summary>The dashboard that has been selected.</summary>
	public SheetDef SheetDefDashboardWidget;

	public FormDashboardWidgets(bool isCEMT=false) {
		InitializeComponent();
	}
		
	private void FormDashboard_Load(object sender,EventArgs e) {
		LayoutMenu();
		FillGrid();
	}

	private void LayoutMenu() {
		menuMain.BeginUpdate();
		menuMain.Add(new MenuItemOD("Setup",setupToolStripMenuItem_Click));
		menuMain.EndUpdate();
	}

	private void FillGrid() {
		var listSheetDefsWidgets=SheetDefs.GetCustomForType(SheetTypeEnum.PatientDashboardWidget);
		listSheetDefsWidgets=listSheetDefsWidgets.FindAll(x => Security.IsAuthorized(EnumPermType.DashboardWidget,x.SheetDefNum,true));
		var listSheetDefsSelected=gridMain.SelectedTags<SheetDef>();
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		gridMain.Columns.Add(new GridColumn("Dashboard Name",0,HorizontalAlignment.Left));
		gridMain.ListGridRows.Clear();
		for(var i=0;i<listSheetDefsWidgets.Count;i++) {
			var row=new GridRow();
			row.Cells.Add(listSheetDefsWidgets[i].Description);
			row.Tag=listSheetDefsWidgets[i];
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
		for(var i=0;i<gridMain.ListGridRows.Count;i++) {
			var sheetDefNum=((SheetDef)gridMain.ListGridRows[i].Tag).SheetDefNum;
			var listSheetDefNums=listSheetDefsSelected.Select(x => x.SheetDefNum).ToList();
			if(listSheetDefNums.Contains(sheetDefNum)) {
				gridMain.SetSelected(i,true);
			}
		}
	}
		
	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		SheetDefDashboardWidget=gridMain.SelectedTag<SheetDef>();
		DialogResult=DialogResult.OK;
	}

	private void setupToolStripMenuItem_Click(object sender,EventArgs e) {
		if(!Security.IsAuthorized(EnumPermType.Setup)) {
			return;
		}
		using var formDashboardWidgetSetup=new FormDashboardWidgetSetup();
		if(formDashboardWidgetSetup.ShowDialog()==DialogResult.OK) {
			FillGrid();
		}
	}

	private void butOK_Click(object sender,EventArgs e) {
		SheetDefDashboardWidget=gridMain.SelectedTag<SheetDef>();
		DialogResult=DialogResult.OK;
	}

}