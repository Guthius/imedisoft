using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormAutoNoteControls : FormODBase
{
    private List<AutoNoteControl> _autoNoteControls;

    public FormAutoNoteControls()
    {
        InitializeComponent();
    }

    private void FormAutoNoteControls_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        AutoNoteControls.RefreshCache();

        _autoNoteControls = AutoNoteControls.GetDeepCopy();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 100));
        gridMain.Columns.Add(new GridColumn("Type", 100));
        gridMain.Columns.Add(new GridColumn("Prompt Text", 100));
        gridMain.Columns.Add(new GridColumn("Options", 100));

        gridMain.ListGridRows.Clear();

        foreach (var autoNoteControl in _autoNoteControls)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(autoNoteControl.Descript);
            gridRow.Cells.Add(autoNoteControl.ControlType);
            gridRow.Cells.Add(autoNoteControl.ControlLabel);
            gridRow.Cells.Add(autoNoteControl.ControlOptions);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void ButtonEdit_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        using var formAutoNoteControlEdit = new FormAutoNoteControlEdit(_autoNoteControls[gridMain.GetSelectedIndex()]);

        if (formAutoNoteControlEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var autoNoteControl = new AutoNoteControl();

        using var formAutoNoteControlEdit = new FormAutoNoteControlEdit(autoNoteControl);

        if (formAutoNoteControlEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        DialogResult = DialogResult.OK;
    }
}