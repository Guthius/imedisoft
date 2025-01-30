using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormCvxs : FormODBase
{
    public bool IsSelectionMode;
    public Cvx CvxSelected;
    private List<Cvx> _cvxs;

    public FormCvxs()
    {
        InitializeComponent();
    }

    private void FormCvxs_Load(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            butOK.Visible = false;
        }

        ActiveControl = textCode;
    }

    private void ButtonSearch_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("CVX Code", 100));
        gridMain.Columns.Add(new GridColumn("Description", 500));

        gridMain.ListGridRows.Clear();

        _cvxs = Cvxs.GetBySearchText(textCode.Text);

        foreach (var cvx in _cvxs)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(cvx.CvxCode);
            gridRow.Cells.Add(cvx.Description);
            gridRow.Tag = cvx;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!IsSelectionMode)
        {
            return;
        }

        CvxSelected = (Cvx) gridMain.ListGridRows[e.Row].Tag;
        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            MsgBox.Show(this, "Please select an item first.");
            return;
        }

        CvxSelected = (Cvx) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
        DialogResult = DialogResult.OK;
    }
}