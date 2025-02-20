using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormOrthoHardwareSpecs : FormODBase
{
    private bool _changed;
    private List<OrthoHardwareSpec> _orthoHardwareSpecs;

    public FormOrthoHardwareSpecs()
    {
        InitializeComponent();
    }

    private void FormOrthoHardwareSpecs_Load(object sender, EventArgs e)
    {
        listType.Items.AddEnums<EnumOrthoHardwareType>();
        listType.SelectedIndex = 0;

        FillGrid();
    }

    private void FormOrthoHardwareSpecs_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.OrthoChartTabs);
        }
    }

    private void ListBoxType_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid(int selectedIndex = -1)
    {
        OrthoHardwareSpecs.RefreshCache();

        _orthoHardwareSpecs = OrthoHardwareSpecs.GetDeepCopy().FindAll(x => x.OrthoHardwareType == (EnumOrthoHardwareType) listType.SelectedIndex);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 200) {IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn("Color", 40, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Hidden", 50, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();

        for (var i = 0; i < _orthoHardwareSpecs.Count; i++)
        {
            if (_orthoHardwareSpecs[i].ItemOrder != i)
            {
                _orthoHardwareSpecs[i].ItemOrder = i;

                OrthoHardwareSpecs.Update(_orthoHardwareSpecs[i]);
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(_orthoHardwareSpecs[i].Description);
            gridRow.Cells.Add(new GridCell("") {ColorBackG = _orthoHardwareSpecs[i].ItemColor});
            gridRow.Cells.Add(new GridCell(_orthoHardwareSpecs[i].IsHidden ? "X" : ""));

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        gridMain.SetSelected(selectedIndex);
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formOrthoHardwareSpecEdit = new FormOrthoHardwareSpecEdit();

        formOrthoHardwareSpecEdit.OrthoHardwareSpecCur = _orthoHardwareSpecs[e.Row];

        if (formOrthoHardwareSpecEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillGrid(formOrthoHardwareSpecEdit.OrthoHardwareSpecCur.ItemOrder);
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var orthoHardwareSpec = new OrthoHardwareSpec
        {
            IsNew = true,
            ItemOrder = _orthoHardwareSpecs.Count,
            OrthoHardwareType = (EnumOrthoHardwareType) listType.SelectedIndex,
            ItemColor = Color.Silver
        };

        using var formOrthoHardwareSpecEdit = new FormOrthoHardwareSpecEdit();

        formOrthoHardwareSpecEdit.OrthoHardwareSpecCur = orthoHardwareSpec;

        if (formOrthoHardwareSpecEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillGrid(formOrthoHardwareSpecEdit.OrthoHardwareSpecCur.ItemOrder);
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        if (gridMain.GetSelectedIndex() == 0)
        {
            return;
        }

        var selectedIndex = gridMain.GetSelectedIndex();

        var selectedOrthoHardwareSpec = _orthoHardwareSpecs[selectedIndex];

        selectedOrthoHardwareSpec.ItemOrder--;

        OrthoHardwareSpecs.Update(selectedOrthoHardwareSpec);

        var orthoHardwareSpecAbove = _orthoHardwareSpecs[selectedIndex - 1];

        orthoHardwareSpecAbove.ItemOrder++;
        OrthoHardwareSpecs.Update(orthoHardwareSpecAbove);

        _changed = true;

        FillGrid(selectedOrthoHardwareSpec.ItemOrder);
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        if (gridMain.GetSelectedIndex() == gridMain.ListGridRows.Count - 1)
        {
            return;
        }

        var selectedIndex = gridMain.GetSelectedIndex();

        var selectedOrthoHardwareSpec = _orthoHardwareSpecs[selectedIndex];

        selectedOrthoHardwareSpec.ItemOrder++;

        OrthoHardwareSpecs.Update(selectedOrthoHardwareSpec);

        var orthoHardwareSpecBelow = _orthoHardwareSpecs[selectedIndex + 1];

        orthoHardwareSpecBelow.ItemOrder--;
        OrthoHardwareSpecs.Update(orthoHardwareSpecBelow);

        _changed = true;

        FillGrid(selectedOrthoHardwareSpec.ItemOrder);
    }
}