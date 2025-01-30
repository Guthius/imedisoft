using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutoCode : FormODBase
{
    private bool _changed;
    private List<AutoCode> _autoCodes = [];

    public FormAutoCode()
    {
        InitializeComponent();
    }

    private void FormAutoCode_Load(object sender, EventArgs e)
    {
        FillList();
    }

    private void FormAutoCode_Closing(object sender, CancelEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.AutoCodes);
        }

        DialogResult = DialogResult.OK;
    }

    private void FillList()
    {
        AutoCodes.RefreshCache();

        listAutoCodes.Items.Clear();

        _autoCodes = AutoCodes.GetListDeep();

        foreach (var autoCode in _autoCodes)
        {
            if (autoCode.IsHidden)
            {
                listAutoCodes.Items.Add(autoCode.Description + "(hidden)");
            }
            else
            {
                listAutoCodes.Items.Add(autoCode.Description);
            }
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var autoCode = new AutoCode();

        AutoCodes.Insert(autoCode);

        using var formAutoCodeEdit = new FormAutoCodeEdit(autoCode);

        formAutoCodeEdit.IsNew = true;

        if (formAutoCodeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillList();
    }

    private void ListBoxAutoCodes_DoubleClick(object sender, EventArgs e)
    {
        if (listAutoCodes.SelectedIndex == -1)
        {
            return;
        }

        var autoCode = _autoCodes[listAutoCodes.SelectedIndex];

        using var formAutoCodeEdit = new FormAutoCodeEdit(autoCode);

        if (formAutoCodeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillList();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (listAutoCodes.SelectedIndex < 0)
        {
            ShowError("You must first select a row");
            return;
        }

        var autoCode = _autoCodes[listAutoCodes.SelectedIndex];
        try
        {
            AutoCodes.Delete(autoCode);
        }
        catch (ApplicationException ex)
        {
            ShowError(ex.Message);
            return;
        }

        _changed = true;

        FillList();
    }
}