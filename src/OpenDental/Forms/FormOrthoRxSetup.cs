using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormOrthoRxSetup : FormODBase
{
    private List<OrthoRx> _orthoRxs;
    private bool _changed;

    public FormOrthoRxSetup()
    {
        InitializeComponent();
    }

    private void FormOrthoRxSetup_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid(int selectedIndex = -1)
    {
        OrthoRxs.RefreshCache();

        _orthoRxs = OrthoRxs.GetDeepCopy();

        var orthoHardwareSpecs = OrthoHardwareSpecs.GetDeepCopy();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Hardware Spec", 110));
        gridMain.Columns.Add(new GridColumn("Description", 230) {IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn("Teeth", 200) {IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn("Color", 40));

        gridMain.ListGridRows.Clear();

        for (var i = 0; i < _orthoRxs.Count; i++)
        {
            if (_orthoRxs[i].ItemOrder != i)
            {
                _orthoRxs[i].ItemOrder = i;

                OrthoRxs.Update(_orthoRxs[i]);
            }

            var orthoHardwareSpec = orthoHardwareSpecs.Find(x => x.OrthoHardwareSpecNum == _orthoRxs[i].OrthoHardwareSpecNum);

            var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
            if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
            {
                toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
            }

            var teeth = "";
            if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
            {
                teeth = Tooth.DisplayOrthoCommas(_orthoRxs[i].ToothRange, toothNumberingNomenclature);
            }

            if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
            {
                teeth = Tooth.DisplayOrthoDash(_orthoRxs[i].ToothRange, toothNumberingNomenclature);
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(orthoHardwareSpec.Description);
            gridRow.Cells.Add(_orthoRxs[i].Description);
            gridRow.Cells.Add(teeth);
            gridRow.Cells.Add(new GridCell("") {ColorBackG = orthoHardwareSpec.ItemColor});

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        gridMain.SetSelected(selectedIndex);
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formOrthoRxEdit = new FormOrthoRxEdit();

        formOrthoRxEdit.OrthoRxCur = _orthoRxs[e.Row];

        if (formOrthoRxEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillGrid(formOrthoRxEdit.OrthoRxCur.ItemOrder);
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var orthoRx = new OrthoRx
        {
            IsNew = true,
            ItemOrder = _orthoRxs.Count
        };

        using var formOrthoRxEdit = new FormOrthoRxEdit();

        formOrthoRxEdit.OrthoRxCur = orthoRx;

        if (formOrthoRxEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillGrid(formOrthoRxEdit.OrthoRxCur.ItemOrder);
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
        var selectedOrthoRx = _orthoRxs[selectedIndex];

        selectedOrthoRx.ItemOrder--;
        OrthoRxs.Update(selectedOrthoRx);

        var orthoRxHardwareSpecAbove = _orthoRxs[selectedIndex - 1];

        orthoRxHardwareSpecAbove.ItemOrder++;
        OrthoRxs.Update(orthoRxHardwareSpecAbove);

        _changed = true;

        FillGrid(selectedOrthoRx.ItemOrder);
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
        var selectedOrthoRx = _orthoRxs[selectedIndex];

        selectedOrthoRx.ItemOrder++;
        OrthoRxs.Update(selectedOrthoRx);

        var orthoRxBelow = _orthoRxs[selectedIndex + 1];

        orthoRxBelow.ItemOrder--;
        OrthoRxs.Update(orthoRxBelow);

        _changed = true;

        FillGrid(selectedOrthoRx.ItemOrder);
    }

    private void FormOrthoHardwareSpecs_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.OrthoChartTabs);
        }
    }
}