using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenDental.UI;

namespace OpenDental;

public partial class FormGridSelection : FormODBase
{
    private readonly List<GridColumn> _gridColumns;
    private readonly List<GridRow> _gridRows;

    public List<object> ListSelectedTags = [];

    public FormGridSelection(List<GridColumn> gridColumns, List<GridRow> gridRows, string title, string gridTitle, GridSelectionMode selectionMode = GridSelectionMode.OneRow)
    {
        InitializeComponent();

        _gridColumns = gridColumns;
        _gridRows = gridRows;

        Text = title;

        gridMain.Title = gridTitle;
        gridMain.SelectionMode = selectionMode;
    }

    private void FormGridSelection_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        foreach (var gridColumn in _gridColumns)
        {
            gridMain.Columns.Add(gridColumn);
        }

        foreach (var gridRow in _gridRows)
        {
            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridEras_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        ListSelectedTags = new List<object>(gridMain.SelectedTags<object>());

        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("No items are selected.  Please select an item before continuing.");
            return;
        }

        ListSelectedTags = new List<object>(gridMain.SelectedTags<object>());

        DialogResult = DialogResult.OK;
    }
}