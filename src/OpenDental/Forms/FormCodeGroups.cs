using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCodeGroups : FormODBase
{
    private List<CodeGroup> _codeGroupsOld;
    private List<CodeGroup> _codeGroups;

    public FormCodeGroups()
    {
        InitializeComponent();
    }

    private void FormCodeGroups_Load(object sender, EventArgs e)
    {
        _codeGroupsOld = CodeGroups.GetDeepCopy();
        _codeGroups = _codeGroupsOld.Select(x => x.Copy()).ToList();

        FillGrid();
    }

    private void FillGrid()
    {
        var codeGroupSelected = gridMain.SelectedTag<CodeGroup>();

        UpdateItemOrder();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Group Name", 0) {IsWidthDynamic = true, DynamicWeight = 1});
        gridMain.Columns.Add(new GridColumn("Fixed Group", 110));
        gridMain.Columns.Add(new GridColumn("Proc Codes", 0) {IsWidthDynamic = true, DynamicWeight = 2});
        gridMain.Columns.Add(new GridColumn("Freq", 35, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Age", 35, HorizontalAlignment.Center));
        gridMain.ListGridRows.Clear();

        var index = -1;

        foreach (var codeGroup in _codeGroups)
        {
            if (!checkShowHidden.Checked && codeGroup.IsHidden && !codeGroup.ShowInAgeLimit)
            {
                continue;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(codeGroup.GroupName);

            var strCodeGroupFixed = "";
            if (codeGroup.CodeGroupFixed != EnumCodeGroupFixed.None)
            {
                strCodeGroupFixed = codeGroup.CodeGroupFixed.GetDescription();
            }

            gridRow.Cells.Add(strCodeGroupFixed);
            gridRow.Cells.Add(codeGroup.ProcCodes);
            gridRow.Cells.Add(codeGroup.IsHidden ? "" : "X");
            gridRow.Cells.Add(codeGroup.ShowInAgeLimit ? "X" : "");
            gridRow.Tag = codeGroup;

            if (codeGroup.Equals(codeGroupSelected))
            {
                index = gridMain.ListGridRows.Count;
            }

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        gridMain.SetSelected(index);
    }

    private void UpdateItemOrder()
    {
        int itemOrder;

        var codeGroupsShown = _codeGroups.FindAll(x => !x.IsHidden || x.ShowInAgeLimit);
        for (itemOrder = 0; itemOrder < codeGroupsShown.Count; itemOrder++)
        {
            codeGroupsShown[itemOrder].ItemOrder = itemOrder;
        }

        var hiddenCodeGroups = _codeGroups.FindAll(x => x.IsHidden && !x.ShowInAgeLimit);
        foreach (var codeGroup in hiddenCodeGroups)
        {
            codeGroup.ItemOrder = itemOrder;
            itemOrder++;
        }

        _codeGroups = _codeGroups.OrderBy(x => x.ItemOrder).ToList();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var codeGroupFixedsInUse = _codeGroups.Select(x => x.CodeGroupFixed).ToList();
        var codeGroup = new CodeGroup
        {
            IsNew = true
        };

        using var formCodeGroupEdit = new FormCodeGroupEdit(codeGroup, codeGroupFixedsInUse);

        if (formCodeGroupEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (!_codeGroups.IsNullOrEmpty())
        {
            codeGroup.ItemOrder = _codeGroups.Max(x => x.ItemOrder) + 1;
        }

        _codeGroups.Add(codeGroup);

        FillGrid();
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        var selectedCodeGroup = gridMain.SelectedTag<CodeGroup>();
        if (selectedCodeGroup is null)
        {
            return;
        }

        var codeGroups = gridMain.GetTags<CodeGroup>();

        var index = codeGroups.IndexOf(selectedCodeGroup);
        if (index <= 0)
        {
            return;
        }

        if (!selectedCodeGroup.IsVisible() && codeGroups[index - 1].IsVisible())
        {
            ShowError("Hidden CodeGroups cannot be ordered above visible codegroups.");
            return;
        }

        var codeGroupSwap = codeGroups[index - 1];

        (selectedCodeGroup.ItemOrder, codeGroupSwap.ItemOrder) = (codeGroupSwap.ItemOrder, selectedCodeGroup.ItemOrder);

        _codeGroups = _codeGroups.OrderBy(x => x.ItemOrder).ToList();
        FillGrid();
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        var selectedCodeGroup = gridMain.SelectedTag<CodeGroup>();
        if (selectedCodeGroup is null)
        {
            return;
        }

        var codeGroups = gridMain.GetTags<CodeGroup>();

        var index = codeGroups.IndexOf(selectedCodeGroup);
        if (index == -1 || index >= codeGroups.Count - 1)
        {
            return;
        }

        if (selectedCodeGroup.IsVisible() && !codeGroups[index + 1].IsVisible())
        {
            ShowError("Hidden CodeGroups cannot be ordered above visible codegroups.");
            return;
        }

        var codeGroupSwap = codeGroups[index + 1];

        (selectedCodeGroup.ItemOrder, codeGroupSwap.ItemOrder) = (codeGroupSwap.ItemOrder, selectedCodeGroup.ItemOrder);

        _codeGroups = _codeGroups.OrderBy(x => x.ItemOrder).ToList();

        FillGrid();
    }

    private void CheckBoxShowHidden_CheckedChanged(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var codeGroup = gridMain.SelectedTag<CodeGroup>();
        var codeGroupFixedsInUse = _codeGroups.Select(x => x.CodeGroupFixed).ToList();

        using var formCodeGroupEdit = new FormCodeGroupEdit(codeGroup, codeGroupFixedsInUse);

        if (formCodeGroupEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (CodeGroups.Sync(_codeGroups, _codeGroupsOld))
        {
            DataValid.SetInvalid(InvalidType.CodeGroups);
        }

        DialogResult = DialogResult.OK;
    }
}