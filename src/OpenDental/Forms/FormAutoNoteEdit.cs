using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutoNoteEdit : FormODBase
{
    private readonly AutoNote _autoNote;
    private int _selectionStart;
    private List<AutoNoteControl> _autoNoteControls;

    public FormAutoNoteEdit(AutoNote autoNote)
    {
        _autoNote = autoNote;

        InitializeComponent();
    }

    private void FormAutoNoteEdit_Load(object sender, EventArgs e)
    {
        if (Security.IsAuthorized(EnumPermType.AutoNoteQuickNoteEdit, true))
        {
            gridMain.CellDoubleClick += GridMain_CellDoubleClick;
        }
        else
        {
            butAdd.Enabled = false;
            butDelete.Enabled = false;
            butSave.Enabled = false;

            textMain.ReadOnly = true;
            textMain.BackColor = SystemColors.Window;
            textBoxAutoNoteName.ReadOnly = true;
            textBoxAutoNoteName.BackColor = SystemColors.Window;
        }

        textBoxAutoNoteName.Text = _autoNote.AutoNoteName;
        textMain.Text = _autoNote.MainText;

        FillGrid();
    }

    private void FillGrid()
    {
        AutoNoteControls.RefreshCache();

        _autoNoteControls = AutoNoteControls.GetDeepCopy();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("", 100));
        gridMain.ListGridRows.Clear();

        foreach (var autoNoteControl in _autoNoteControls)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(autoNoteControl.Descript);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formAutoNoteControlEdit = new FormAutoNoteControlEdit(_autoNoteControls[e.Row]);

        if (formAutoNoteControlEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var autoNoteControl = new AutoNoteControl
        {
            ControlType = "Text"
        };

        using var formAutoNoteControlEdit = new FormAutoNoteControlEdit(autoNoteControl);

        if (formAutoNoteControlEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonInsert_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select a prompt first.");
            return;
        }

        var description = _autoNoteControls[gridMain.GetSelectedIndex()].Descript;
        if (_selectionStart < textMain.Text.Length - 1)
        {
            textMain.Text = textMain.Text.Substring(0, _selectionStart) + "[Prompt:\"" + description + "\"]" + textMain.Text.Substring(_selectionStart);
        }
        else
        {
            textMain.Text += "[Prompt:\"" + description + "\"]";
        }

        textMain.Select(_selectionStart + description.Length + 11, 0);
        textMain.Focus();
    }

    private void TextBoxMain_Leave(object sender, EventArgs e)
    {
        _selectionStart = textMain.SelectionStart;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        var sheetFieldDefs = SheetFieldDefs.GetWhere(x => x.FieldType == SheetFieldType.InputField && x.FieldValue.Contains("AutoNoteNum:" + _autoNote.AutoNoteNum));
        if (sheetFieldDefs.Count > 0)
        {
            if (!ConfirmOk("There are sheet field definitions associated with this autonote. Delete this autonote and the associated fields?"))
            {
                return;
            }

            foreach (var sheetFieldDef in sheetFieldDefs)
            {
                SheetFieldDefs.Delete(sheetFieldDef.SheetFieldDefNum);
            }
        }
        else if (!ConfirmOk("Delete this autonote?"))
        {
            return;
        }

        if (_autoNote.AutoNoteNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        AutoNotes.Delete(_autoNote.AutoNoteNum);

        DataValid.SetInvalid(InvalidType.AutoNotes);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        _autoNote.AutoNoteName = textBoxAutoNoteName.Text;
        _autoNote.MainText = textMain.Text;

        if (_autoNote.AutoNoteNum == 0)
        {
            AutoNotes.Insert(_autoNote);
        }
        else
        {
            AutoNotes.Update(_autoNote);
        }

        DataValid.SetInvalid(InvalidType.AutoNotes);

        DialogResult = DialogResult.OK;
    }
}