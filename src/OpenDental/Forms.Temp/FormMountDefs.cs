using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormMountDefs : FormODBase
{
    private bool _changed;
    private List<MountDef> _mountDefs;

    public FormMountDefs()
    {
        InitializeComponent();
    }

    private void FormMountDefs_Load(object sender, EventArgs e)
    {
        FillList();
    }

    private void FormMounts_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.ToolButsAndMounts);
        }
    }

    private void FillList()
    {
        MountDefs.RefreshCache();

        listBoxMain.Items.Clear();

        _mountDefs = MountDefs.GetDeepCopy();
        for (var i = 0; i < _mountDefs.Count; i++)
        {
            if (_mountDefs[i].ItemOrder != i)
            {
                _mountDefs[i].ItemOrder = i;

                MountDefs.Update(_mountDefs[i]);

                _changed = true;
            }

            listBoxMain.Items.Add(_mountDefs[i].Description);
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var mountDef = new MountDef
        {
            IsNew = true,
            Description = "Mount",
            Width = 600,
            Height = 400
        };

        if (_mountDefs.Count > 0)
        {
            mountDef.ItemOrder = _mountDefs.Count;
        }

        MountDefs.Insert(mountDef);

        using var formMountDefEdit = new FormMountDefEdit();

        formMountDefEdit.MountDefCur = mountDef;
        formMountDefEdit.ShowDialog();

        FillList();

        _changed = true;
    }

    private void ListBoxMain_DoubleClick(object sender, EventArgs e)
    {
        if (listBoxMain.SelectedIndex == -1)
        {
            return;
        }

        using var formMountDefEdit = new FormMountDefEdit();

        formMountDefEdit.MountDefCur = _mountDefs[listBoxMain.SelectedIndex];
        formMountDefEdit.ShowDialog();

        FillList();

        _changed = true;
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        var selectedIndex = listBoxMain.SelectedIndex;
        switch (selectedIndex)
        {
            case -1:
            case 0:
                return;
        }

        var mountDef = _mountDefs[selectedIndex];

        mountDef.ItemOrder--;
        MountDefs.Update(mountDef);

        var mountDefAbove = _mountDefs[selectedIndex - 1];

        mountDefAbove.ItemOrder++;
        MountDefs.Update(mountDefAbove);

        FillList();

        listBoxMain.SelectedIndex = selectedIndex - 1;

        _changed = true;
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        var selectedIndex = listBoxMain.SelectedIndex;
        if (selectedIndex == -1)
        {
            return;
        }

        if (selectedIndex == _mountDefs.Count - 1)
        {
            return;
        }

        var mountDef = _mountDefs[selectedIndex];

        mountDef.ItemOrder++;
        MountDefs.Update(mountDef);

        var mountDefBelow = _mountDefs[selectedIndex + 1];

        mountDefBelow.ItemOrder--;
        MountDefs.Update(mountDefBelow);

        FillList();

        listBoxMain.SelectedIndex = selectedIndex + 1;

        _changed = true;
    }
}