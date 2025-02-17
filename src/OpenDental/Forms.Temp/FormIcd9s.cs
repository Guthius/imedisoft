using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormIcd9s : FormODBase
{
    private List<ICD9> _icd9s;

    public bool IsSelectionMode { get; set; }
    public ICD9 SelectedIcd9 { get; set; }

    public FormIcd9s()
    {
        InitializeComponent();
    }

    private void FormIcd9s_Load(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            butOK.Visible = false;
        }
    }

    private void ButtonSearch_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        Cursor = Cursors.WaitCursor;

        _icd9s = Icd9s.GetByCodeOrDescription(textCode.Text);
        _icd9s.RemoveAll(x => string.IsNullOrEmpty(x.ICD9Code));

        listMain.Items.Clear();

        foreach (var icd9 in _icd9s)
        {
            listMain.Items.Add(icd9.ICD9Code + " - " + icd9.Description);
        }

        Cursor = Cursors.Default;
    }

    private void ListBoxMain_DoubleClick(object sender, EventArgs e)
    {
        if (listMain.SelectedIndex == -1)
        {
            return;
        }

        if (!IsSelectionMode)
        {
            return;
        }

        SelectedIcd9 = _icd9s[listMain.SelectedIndex];

        DialogResult = DialogResult.OK;
    }

    private void ButtonCodeImport_Click(object sender, EventArgs e)
    {
        using var formCodeSystemsImport = new FormCodeSystemsImport();

        formCodeSystemsImport.ShowDialog();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (listMain.SelectedIndex == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        SelectedIcd9 = _icd9s[listMain.SelectedIndex];

        DialogResult = DialogResult.OK;
    }
}