using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormLaboratories : FormODBase
{
    private List<Laboratory> _laboratories;

    public FormLaboratories()
    {
        InitializeComponent();
    }

    private void FormLaboratories_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        _laboratories = Laboratories.Refresh();

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 100));
        gridMain.Columns.Add(new GridColumn("Phone", 100));
        gridMain.Columns.Add(new GridColumn("Hidden", 50, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Notes", 200));

        gridMain.ListGridRows.Clear();

        foreach (var laboratory in _laboratories)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(laboratory.Description);
            gridRow.Cells.Add(laboratory.Phone);
            gridRow.Cells.Add(laboratory.IsHidden ? "X" : "");
            gridRow.Cells.Add(laboratory.Notes);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formLaboratoryEdit = new FormLaboratoryEdit(_laboratories[e.Row]);

        formLaboratoryEdit.ShowDialog();

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var laboratory = new Laboratory();

        using var formLaboratoryEdit = new FormLaboratoryEdit(laboratory);

        formLaboratoryEdit.ShowDialog();

        FillGrid();
    }
}