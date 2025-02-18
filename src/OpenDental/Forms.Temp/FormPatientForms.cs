using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormPatientForms : FormODBase
{
    public long PatNum;
    public long DocNum;

    private Patient _patient;

    public FormPatientForms()
    {
        InitializeComponent();

        DocNum = 0;
    }

    private void FormPatientForms_Load(object sender, EventArgs e)
    {
        _patient = Patients.GetPat(PatNum);

        Text = "Patient Forms for " + _patient.GetNameFL();

        LayoutMenu();

        FillGrid(refreshFromDb: true);
    }

    private void LayoutMenu()
    {
        var menuItemSetup = new MenuItemOD("Setup");

        menuItemSetup.Add("Sheets", MenuItemSheets_Click);
        menuItemSetup.Add("Image Categories", MenuItemImageCats_Click);
        menuItemSetup.Add("Options", MenuItemOptions_Click);

        menuMain.BeginUpdate();
        menuMain.Add(menuItemSetup);
        menuMain.EndUpdate();
    }

    private void FillGrid(bool refreshFromDb)
    {
        var dataTable = new DataTable();
        if (refreshFromDb)
        {
            dataTable = Sheets.GetPatientFormsTable(PatNum);
        }
        else
        {
            var listDataRows2 = gridMain.ListGridRows.Select(x => (DataRow) x.Tag).ToList(); //Get the list of DataRows from the grid.
            if (listDataRows2.Count > 0)
            {
                dataTable = listDataRows2.CopyToDataTable();
            }
        }

        if (radioSortByDateTime.Checked && dataTable.Rows.Count > 0)
        {
            var dataView = dataTable.DefaultView;

            dataView.Sort = "dateTime";
            dataTable = dataView.ToTable();
        }
        else if (radioSortByDescDateT.Checked && dataTable.Rows.Count > 0)
        {
            var dataView = dataTable.DefaultView;

            dataView.Sort = "description,dateTime";
            dataTable = dataView.ToTable();
        }

        long sheetNumSelected = 0;
        long eFormNumSelected = 0;
        long docNumSelected = 0;

        if (gridMain.GetSelectedIndex() != -1)
        {
            var dataRow = (DataRow) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;

            sheetNumSelected = SIn.Long(dataRow["SheetNum"].ToString());
            eFormNumSelected = SIn.Long(dataRow["EFormNum"].ToString());
            docNumSelected = SIn.Long(dataRow["DocNum"].ToString());
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Type", 60));
        gridMain.Columns.Add(new GridColumn("Date/Time", 140, GridSortingStrategy.DateParse));
        gridMain.Columns.Add(new GridColumn("Kiosk", 55, HorizontalAlignment.Center, GridSortingStrategy.AmountParse));
        gridMain.Columns.Add(new GridColumn("Description", 190));
        gridMain.Columns.Add(new GridColumn("Image Category", 120));
        gridMain.Columns.Add(new GridColumn("Updated", 65, GridSortingStrategy.DateParse));
        gridMain.ListGridRows.Clear();

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var gridRow = new GridRow();

            if (dataTable.Rows[i]["DocNum"].ToString() != "0")
            {
                gridRow.Cells.Add("Document");
            }
            else if (dataTable.Rows[i]["SheetNum"].ToString() != "0")
            {
                gridRow.Cells.Add("Sheet");
            }
            else if (dataTable.Rows[i]["EFormNum"].ToString() != "0")
            {
                gridRow.Cells.Add("eForm");
            }

            gridRow.Cells.Add(dataTable.Rows[i]["dateTime"].ToString());
            gridRow.Cells.Add(dataTable.Rows[i]["showInTerminal"].ToString());
            gridRow.Cells.Add(dataTable.Rows[i]["description"].ToString());
            gridRow.Cells.Add(dataTable.Rows[i]["imageCat"].ToString());
            gridRow.Cells.Add(dataTable.Rows[i]["DateTSheetEdited"].ToString());
            gridRow.Tag = dataTable.Rows[i];

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();

        var listDataRows = gridMain.ListGridRows.Select(x => (DataRow) x.Tag).ToList();
        if (sheetNumSelected != 0)
        {
            var idx = listDataRows.FindIndex(x => x["SheetNum"].ToString() == sheetNumSelected.ToString());
            gridMain.SetSelected(idx);
        }
        else if (eFormNumSelected != 0)
        {
            var idx = listDataRows.FindIndex(x => x["eFormNum"].ToString() == eFormNumSelected.ToString());
            gridMain.SetSelected(idx);
        }
        else if (docNumSelected != 0)
        {
            var idx = listDataRows.FindIndex(x => x["DocNum"].ToString() == docNumSelected.ToString());
            gridMain.SetSelected(idx);
        }
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var dataRow = (DataRow) gridMain.ListGridRows[e.Row].Tag;

        DocNum = SIn.Long(dataRow["DocNum"].ToString());
        if (DocNum != 0)
        {
            if (!Security.IsAuthorized(EnumPermType.ImagingModule))
            {
                return;
            }

            GlobalFormOpenDental.GoToModule(EnumModuleType.Imaging, patNum: PatNum, docNum: DocNum);
            return;
        }

        var sheetNum = SIn.Long(dataRow["SheetNum"].ToString());
        if (sheetNum != 0)
        {
            var sheet = Sheets.GetSheet(sheetNum);

            FormSheetFillEdit.ShowForm(sheet, FormSheetFillEdit_FormClosing);

            return;
        }

        var eFormNum = SIn.Long(dataRow["EFormNum"].ToString());
        if (eFormNum == 0)
        {
            return;
        }

        var eForm = EForms.GetEForm(eFormNum);

        var frmEFormFillEdit = new FrmEFormFillEdit
        {
            EFormCur = eForm
        };

        frmEFormFillEdit.ShowDialog();

        if (frmEFormFillEdit.IsDialogCancel)
        {
            return;
        }

        FillGrid(refreshFromDb: true);
    }

    private void MenuItemSheets_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        using var formSheetDefs = new FormSheetDefs();

        formSheetDefs.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "Sheets");

        FillGrid(refreshFromDb: false);
    }

    private void MenuItemImageCats_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        using var formDefinitions = new FormDefinitions(DefCat.ImageCats);

        formDefinitions.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, "Defs");

        FillGrid(refreshFromDb: true);
    }

    private void MenuItemOptions_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.Setup))
        {
            return;
        }

        var frmSheetSetup = new FrmSheetSetup();

        frmSheetSetup.ShowDialog();

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, "ShowForms");

        FillGrid(refreshFromDb: true);
    }

    private void RadioButtonSortByDateTime_Click(object sender, EventArgs e)
    {
        FillGrid(refreshFromDb: false);
    }

    private void RadioButtonSortByDescriptionDateTime_Click(object sender, EventArgs e)
    {
        FillGrid(refreshFromDb: false);
    }

    private void ButtonAddSheet_Click(object sender, EventArgs e)
    {
        var frmSheetPicker = new FrmSheetPicker
        {
            AllowMultiSelect = true,
            SheetType = SheetTypeEnum.PatientForm
        };

        frmSheetPicker.ShowDialog();

        if (!frmSheetPicker.IsDialogOK)
        {
            return;
        }

        Sheet sheet = null;
        foreach (var sheetDef in frmSheetPicker.ListSheetDefsSelected)
        {
            sheet = SheetUtil.CreateSheet(sheetDef, PatNum);

            if (SheetDefs.ContainsGrids(sheetDef, "ProcsWithFee", "ProcsNoFee"))
            {
                using var formSheetProcSelect = new FormSheetProcSelect();

                formSheetProcSelect.PatNum = PatNum;

                if (formSheetProcSelect.ShowDialog() == DialogResult.OK)
                {
                    SheetParameter.SetParameter(sheet, "ListProcNums", formSheetProcSelect.SelectedProcNums);
                }
            }

            SheetUtilL.SetApptProcParamsForSheet(sheet, sheetDef, PatNum);
            SheetParameter.SetParameter(sheet, "PatNum", PatNum);
            SheetFiller.FillFields(sheet);
            SheetUtil.CalculateHeights(sheet);

            if (!frmSheetPicker.DoKioskSend)
            {
                continue;
            }

            sheet.InternalNote = "";
            sheet.ShowInTerminal = (byte) (Sheets.GetBiggestShowInTerminal(PatNum) + 1);

            Sheets.SaveNewSheet(sheet);
            Sheets.SaveParameters(sheet);
        }

        if (frmSheetPicker.DoKioskSend)
        {
            FillGrid(refreshFromDb: true);

            Signalods.SetInvalid(InvalidType.Kiosk);
        }
        else if (sheet != null)
        {
            FormSheetFillEdit.ShowForm(sheet, FormSheetFillEdit_FormClosing);
        }
    }

    private void ButtonAddEForm_Click(object sender, EventArgs e)
    {
        var frmEFormPicker = new FrmEFormPicker();

        frmEFormPicker.ShowDialog();

        if (frmEFormPicker.IsDialogCancel)
        {
            return;
        }

        var eFormDef = frmEFormPicker.EFormDefSelected;

        var eForm = EForms.CreateEFormFromEFormDef(eFormDef, PatNum, EnumEFormStatus.None);

        eForm.DateTimeShown = DateTime.Now;

        EFormFiller.FillFields(eForm);
        EForms.TranslateFields(eForm, _patient.Language);

        var frmEFormFillEdit = new FrmEFormFillEdit
        {
            EFormCur = eForm
        };

        frmEFormFillEdit.ShowDialog();

        if (frmEFormFillEdit.IsDialogCancel)
        {
            return;
        }

        FillGrid(refreshFromDb: true);
    }

    private void ButtonTerminal_Click(object sender, EventArgs e)
    {
        var dataRows = gridMain.ListGridRows.Select(x => (DataRow) x.Tag).ToList();
        if (dataRows.All(x => x["showInTerminal"].ToString() == ""))
        {
            ShowError("No forms for this patient are set to show in the kiosk.");
            return;
        }

        if (PrefC.GetLong(PrefName.ProcessSigsIntervalInSecs) == 0)
        {
            ShowError("Cannot open kiosk unless process signal interval is set. To set it, go to Setup > Miscellaneous.");
            return;
        }

        using var formTerminal = new FormTerminal();

        formTerminal.IsSimpleMode = true;
        formTerminal.PatNum = PatNum;
        formTerminal.ShowDialog();

        FillGrid(refreshFromDb: true);
    }

    private void ButtonPreFill_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length != 1)
        {
            ShowError("Please select one completed sheet from the list above first.");
            return;
        }

        var dataRow = (DataRow) gridMain.ListGridRows[gridMain.SelectedIndices[0]].Tag;

        var sheetNum = SIn.Long(dataRow["SheetNum"].ToString());
        if (sheetNum == 0)
        {
            ShowError("Must select a sheet.");
            return;
        }

        var sheet = Sheets.GetSheet(sheetNum);
        if (sheet is null)
        {
            ShowError("The selected sheet has been deleted by another workstation.");
            return;
        }

        var originalSheetDef = SheetDefs.GetSheetDef(sheet.SheetDefNum, hasExceptions: false);
        if (originalSheetDef is null)
        {
            if (!Confirm("Sheet Def not found. Unable to pre-fill. Would you like to select the correct Sheet Def manually?"))
            {
                return;
            }

            var listSheetDefs = SheetDefs.GetCustomForType(sheet.SheetType);

            var frmSheetPicker = new FrmSheetPicker
            {
                SheetType = sheet.SheetType,
                ListSheetDefs = listSheetDefs,
                IsPreFill = true
            };

            frmSheetPicker.ShowDialog();

            if (!frmSheetPicker.IsDialogOK)
            {
                return;
            }

            originalSheetDef = frmSheetPicker.ListSheetDefsSelected.First();
            sheet.SheetDefNum = originalSheetDef.SheetDefNum;
        }

        var sheetNew = Sheets.PreFillSheetFromPreviousAndDatabase(originalSheetDef, sheet);

        sheetNew.IsNew = true;

        using var formSheetFillEdit = new FormSheetFillEdit();

        formSheetFillEdit.SheetCur = sheetNew;

        if (formSheetFillEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Copy, PatNum, "Patient form " + sheet.Description + " from " + sheet.DateTimeSheet + " copied via Pre-Fill");

        FillGrid(refreshFromDb: true);

        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            dataRow = (DataRow) gridMain.ListGridRows[i].Tag;
            if (dataRow["SheetNum"].ToString() != sheetNew.SheetNum.ToString())
            {
                continue;
            }

            gridMain.SetSelected(i);
            break;
        }
    }

    private void ButtonCopy_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length != 1)
        {
            ShowError("Please select one completed sheet from the list above first.");
            return;
        }

        var dataRow = (DataRow) gridMain.ListGridRows[gridMain.SelectedIndices[0]].Tag;

        var sheetNum = SIn.Long(dataRow["SheetNum"].ToString());
        if (sheetNum == 0)
        {
            ShowError("Must select a sheet.");
            return;
        }

        var sheet = Sheets.GetSheet(sheetNum);
        var sheetCopy = sheet.Copy();

        sheetCopy.DateTimeSheet = DateTime.Now;
        sheetCopy.SheetFields = new List<SheetField>(sheet.SheetFields);

        foreach (var sheetField in sheetCopy.SheetFields)
        {
            sheetField.IsNew = true;
            if (sheetField.FieldType == SheetFieldType.SigBox)
            {
                sheetField.FieldValue = "";
            }
        }

        sheetCopy.IsNew = true;

        using var formSheetFillEdit = new FormSheetFillEdit();

        formSheetFillEdit.SheetCur = sheetCopy;

        if (formSheetFillEdit.ShowDialog() != DialogResult.OK && !formSheetFillEdit.DidChangeSheet)
        {
            return;
        }

        FillGrid(refreshFromDb: true);
        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            dataRow = (DataRow) gridMain.ListGridRows[i].Tag;
            if (dataRow["SheetNum"].ToString() == sheetCopy.SheetNum.ToString())
            {
                gridMain.SetSelected(i);
            }
        }

        SecurityLogs.MakeLogEntry(EnumPermType.Copy, PatNum, "Patient form " + sheet.Description + " from " + sheet.DateTimeSheet + " copied");
    }

    private void ButtonImport_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length != 1)
        {
            ShowError("Please select one completed form from the list above first.");
            return;
        }

        var dataRow = (DataRow) gridMain.ListGridRows[gridMain.SelectedIndices[0]].Tag;

        var docNum = SIn.Long(dataRow["DocNum"].ToString());
        if (docNum != 0)
        {
            ShowError("PDFs cannot be imported into the database.");
            return;
        }

        var sheetNum = SIn.Long(dataRow["SheetNum"].ToString());
        var eFormNum = SIn.Long(dataRow["EFormNum"].ToString());

        Sheet sheet = null;

        if (sheetNum != 0)
        {
            sheet = Sheets.GetSheet(sheetNum);
            if (!SheetDefs.IsWebFormAllowed(sheet.SheetType))
            {
                ShowError("For now, only sheets of type 'PatientForm' and 'MedicalHistory' can be imported.");
                return;
            }
        }

        EForm eForm = null;
        if (eFormNum != 0)
        {
            eForm = EForms.GetEForm(eFormNum);
        }

        using var formSheetImport = new FormSheetImport();

        formSheetImport.SheetCur = sheet;
        formSheetImport.EFormCur = eForm;
        formSheetImport.ShowDialog();
    }

    private void FormSheetFillEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (((FormSheetFillEdit) sender).DialogResult == DialogResult.OK || ((FormSheetFillEdit) sender).DidChangeSheet)
        {
            FillGrid(refreshFromDb: true);
        }
    }
}