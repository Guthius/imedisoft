using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormOrthoHardwareAdd : FormODBase
{
    private List<OrthoHardwareSpec> _orthoHardwareSpecs;

    public List<OrthoHardwareSpec> SelectedOrthoHardwareSpecs { get; set; }

    public FormOrthoHardwareAdd()
    {
        InitializeComponent();
    }

    private void FormOrthoHardwareAdd_Load(object sender, EventArgs e)
    {
        _orthoHardwareSpecs = OrthoHardwareSpecs.GetDeepCopy(shortList: true);

        FillGrid();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Type", 60));
        gridMain.Columns.Add(new GridColumn("Description", 200) {IsWidthDynamic = true});
        gridMain.Columns.Add(new GridColumn("Color", 40));

        gridMain.ListGridRows.Clear();

        foreach (var orthoHardwareSpec in _orthoHardwareSpecs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(orthoHardwareSpec.OrthoHardwareType.ToString());
            gridRow.Cells.Add(orthoHardwareSpec.Description);
            gridRow.Cells.Add(new GridCell("") {ColorBackG = orthoHardwareSpec.ItemColor});

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedOrthoHardwareSpecs =
        [
            _orthoHardwareSpecs[e.Row]
        ];

        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("Please selecte one or more items first.");
            return;
        }

        SelectedOrthoHardwareSpecs = [];

        foreach (var index in gridMain.SelectedIndices)
        {
            SelectedOrthoHardwareSpecs.Add(_orthoHardwareSpecs[index]);
        }

        DialogResult = DialogResult.OK;
    }
}