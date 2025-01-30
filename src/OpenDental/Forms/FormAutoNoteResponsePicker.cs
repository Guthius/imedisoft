using System;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormAutoNoteResponsePicker : FormODBase
{
    public string AutoNoteResponseText;

    public FormAutoNoteResponsePicker()
    {
        InitializeComponent();
    }

    private void FormAutoNoteResponsePicker_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        AutoNotes.RefreshCache();

        var autoNotes = AutoNotes.GetDeepCopy();

        gridMain.BeginUpdate();
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("", 100));
        gridMain.ListGridRows.Clear();

        foreach (var autoNote in autoNotes)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(autoNote.AutoNoteName);
            gridRow.Tag = autoNote;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textResponseText.Text))
        {
            ShowError("Please enter a response text.");

            return;
        }

        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an AutoNote.");

            return;
        }

        var autoNote = gridMain.SelectedTag<AutoNote>();
        if (autoNote is null)
        {
            ShowError("Invalid AutoNote selected. Please select a new one.");

            gridMain.SetAll(false);

            return;
        }

        AutoNoteResponseText = textResponseText.Text + " : {" + autoNote.AutoNoteName + "}";

        DialogResult = DialogResult.OK;
    }
}