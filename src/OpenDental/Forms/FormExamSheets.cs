using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormExamSheets : FormODBase
{
    private List<Sheet> _sheets;

    public long PatNum;

    public FormExamSheets()
    {
        InitializeComponent();
    }

    private void FormExamSheets_Load(object sender, EventArgs e)
    {
        var patient = Patients.GetLim(PatNum);

        Text = "Exam Sheets for " + patient.GetNameFL();

        LayoutMenu();

        FillListExamTypes();
        FillGrid();
    }

    private void LayoutMenu()
    {
        var menuItemSetup = new MenuItemOD("Setup");

        menuItemSetup.Add("Sheets", MenuItemSheets_Click);

        menuMain.BeginUpdate();
        menuMain.Add(menuItemSetup);
        menuMain.EndUpdate();
    }

    private void FillListExamTypes()
    {
        listExamTypes.Items.Clear();

        var sheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.ExamSheet);
        var sheetDefFilter = new SheetDef
        {
            SheetDefNum = -1
        };

        listExamTypes.Items.Add("All", sheetDefFilter);

        foreach (var sheetDef in sheetDefs)
        {
            listExamTypes.Items.Add(sheetDef.Description, sheetDef);
        }

        listExamTypes.SelectedIndex = 0;
    }

    private void ListExamTypes_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        long selectedSheetNum = 0;
        if (gridMain.GetSelectedIndex() != -1)
        {
            selectedSheetNum = gridMain.SelectedTag<Sheet>().SheetNum;
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 70));
        gridMain.Columns.Add(new GridColumn("Time", 54));
        gridMain.Columns.Add(new GridColumn("Description", 210));
        gridMain.Columns.Add(new GridColumn("Type", 75) {IsWidthDynamic = true});

        gridMain.ListGridRows.Clear();

        var selectedSheetDefs = listExamTypes.GetSelected<SheetDef>();
        if (selectedSheetDefs is null)
        {
            gridMain.EndUpdate();
            panelSheetPreview.Invalidate();
            return;
        }

        _sheets = Sheets.GetExamSheetsTable(PatNum, DateTime.MinValue, DateTime.MaxValue, selectedSheetDefs.SheetDefNum); //SheetDefNum is -1 when 'All' is selected

        Sheets.SetSheetFieldsForSheets(_sheets);

        var examSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.ExamSheet);
        var indexToSelect = 0;
        for (var i = 0; i < _sheets.Count; i++)
        {
            var description = "";

            var sheetDef = examSheetDefs.FirstOrDefault(x => x.SheetDefNum == _sheets[i].SheetDefNum);
            if (sheetDef is not null)
            {
                description = sheetDef.Description;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(_sheets[i].DateTimeSheet.ToShortDateString());
            gridRow.Cells.Add(_sheets[i].DateTimeSheet.ToShortTimeString());
            gridRow.Cells.Add(_sheets[i].Description);
            gridRow.Cells.Add(description);
            gridRow.Tag = _sheets[i];

            gridMain.ListGridRows.Add(gridRow);

            if (_sheets[i].SheetNum == selectedSheetNum)
            {
                indexToSelect = i;
            }
        }

        gridMain.EndUpdate();
        gridMain.SetSelected(indexToSelect);

        panelSheetPreview.Invalidate();
    }

    private void GridMain_SelectionCommitted(object sender, EventArgs e)
    {
        panelSheetPreview.Invalidate();
    }

    private void PanelSheetPreview_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        g.Clear(Color.FromArgb(252, 253, 254));

        if (gridMain.GetSelectedIndex() == -1)
        {
            return;
        }

        var selectedSheet = gridMain.SelectedTag<Sheet>();

        var sy = (panelSheetPreview.Height - 1) / (float) selectedSheet.HeightPage;
        var sx = (panelSheetPreview.Width - 1) / (float) selectedSheet.WidthPage;

        var scale = sy;
        if (sx < sy)
        {
            scale = sx;
        }

        if (scale == 0)
        {
            return;
        }

        g.ScaleTransform(scale, scale);

        var rectangle = new Rectangle(0, 0, selectedSheet.WidthPage, selectedSheet.HeightPage);

        g.FillRectangle(Brushes.White, rectangle);

        var sheetPrintingJob = new SheetPrintingJob();

        sheetPrintingJob.DrawSheetFirstPage(g, selectedSheet);

        g.DrawRectangle(Pens.Gray, rectangle);
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var sheet = (Sheet) gridMain.ListGridRows[e.Row].Tag;

        FormSheetFillEdit.ShowForm(sheet, FormSheetFillEdit_Grid_FormClosing);
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

        FillListExamTypes();
        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var frmSheetPicker = new FrmSheetPicker
        {
            AllowMultiSelect = true,
            SheetType = SheetTypeEnum.ExamSheet
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
            SheetParameter.SetParameter(sheet, "PatNum", PatNum);
            SheetFiller.FillFields(sheet);
            SheetUtil.CalculateHeights(sheet);
        }

        FormSheetFillEdit.ShowForm(sheet, FormSheetFillEdit_Add_FormClosing);
    }

    private void FormSheetFillEdit_Grid_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (sender is not FormSheetFillEdit formSheetFillEdit)
        {
            return;
        }

        if (formSheetFillEdit.DialogResult != DialogResult.OK && !formSheetFillEdit.DidChangeSheet)
        {
            return;
        }

        FillGrid();

        panelSheetPreview.Invalidate();
    }

    private void FormSheetFillEdit_Add_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (sender is not FormSheetFillEdit formSheetFillEdit)
        {
            return;
        }

        if (formSheetFillEdit.DialogResult != DialogResult.OK && !formSheetFillEdit.DidChangeSheet)
        {
            return;
        }

        if (formSheetFillEdit.SheetCur is not null && formSheetFillEdit.SheetCur.Description != listExamTypes.GetSelected<SheetDef>().ToString())
        {
            listExamTypes.SelectedIndex = 0;
        }

        FillGrid();

        gridMain.SetAll(false);
        gridMain.SetSelected(gridMain.ListGridRows.Count - 1, setValue: true);

        panelSheetPreview.Invalidate();
    }
}