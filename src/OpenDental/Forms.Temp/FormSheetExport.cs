using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormSheetExport:FormODBase {
	private List<SheetDef> _listSheetDefs;
	///<summary>Form was opened via Dashboard Setup.</summary>
	private bool _isOpenedFromDashboardSetup;

	public FormSheetExport(bool isOpenedFromDashboardSetup) {
		InitializeComponent();

		_isOpenedFromDashboardSetup=isOpenedFromDashboardSetup;
	}

	private void FormSheetExport_Load(object sender,EventArgs e) {
		FillGridCustomSheet();
	}

	private void FillGridCustomSheet() {
		SheetDefs.RefreshCache();
		SheetFieldDefs.RefreshCache();
		//If from Dashboard Setup, populate when SheetDef is a Dashboard type.
		//If from normal Sheet window, populate all Sheets except Dashboard types.
		_listSheetDefs=SheetDefs.GetDeepCopy(false).FindAll(x => SheetDefs.IsDashboardType(x)==_isOpenedFromDashboardSetup);
		gridCustomSheet.BeginUpdate();
		gridCustomSheet.Columns.Clear();
		var col=new GridColumn(Lan.g("TableSheetDef","Description"),170);
		gridCustomSheet.Columns.Add(col);
		col=new GridColumn(Lan.g("TableSheetDef","Type"),100);
		gridCustomSheet.Columns.Add(col);
		gridCustomSheet.ListGridRows.Clear();
		GridRow row;
		for(var i=0;i<_listSheetDefs.Count;i++){
			row=new GridRow();
			row.Cells.Add(_listSheetDefs[i].Description);
			row.Cells.Add(_listSheetDefs[i].SheetType.ToString());
			gridCustomSheet.ListGridRows.Add(row);
		}
		gridCustomSheet.EndUpdate();
	}

	private void butExport_Click(object sender,EventArgs e) {
		if(gridCustomSheet.GetSelectedIndex()==-1) {
			MsgBox.Show(this,"Please select a sheet from the list first.");
			return;
		}
		var sheetDef=SheetDefs.GetSheetDef(_listSheetDefs[gridCustomSheet.GetSelectedIndex()].SheetDefNum);
		var listFieldDefImages=sheetDef.SheetFieldDefs.FindAll(x => x.FieldType==SheetFieldType.Image && x.FieldName!="Patient Info.gif");
		if(!listFieldDefImages.IsNullOrEmpty()) {//Alert them of any images they need to copy if there are any.
			var sheetImagesPath="";
			ODException.SwallowAnyException(() => {
				sheetImagesPath=SheetUtil.GetImagePath();
			});
			var stringBuilder=new StringBuilder();
			stringBuilder.AppendLine(Lan.g(this,"The following images will need to be manually imported with the same file name when importing this "
			                                    +"sheet to a new environment."));
			stringBuilder.AppendLine();
			for(var i=0;i<listFieldDefImages.Count;i++){
				stringBuilder.AppendLine(ODFileUtils.CombinePaths(sheetImagesPath,listFieldDefImages[i].FieldName));
			}
			using var msgBoxCopyPaste=new MsgBoxCopyPaste(stringBuilder.ToString());
			msgBoxCopyPaste.ShowDialog();
		}
		var xmlSerializer=new XmlSerializer(typeof(SheetDef));
		var fileName="SheetDefCustom.xml";
		using var saveFileDialog=new SaveFileDialog();
		saveFileDialog.InitialDirectory=PrefC.GetString(PrefName.ExportPath);
		saveFileDialog.FileName=fileName;
		if(saveFileDialog.ShowDialog()!=DialogResult.OK) {
			return;
		}
		using TextWriter textWriter=new StreamWriter(saveFileDialog.FileName);
		xmlSerializer.Serialize(textWriter,sheetDef);
		textWriter.Close();
		MsgBox.Show(this,"Exported");
	}

}