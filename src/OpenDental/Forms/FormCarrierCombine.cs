using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCarrierCombine : FormODBase
{
    private List<Carrier> _carriers;

    public List<long> CarrierNums { get; set; }
    public long SelectedCarrierNum { get; set; }

    public FormCarrierCombine()
    {
        InitializeComponent();
    }

    private void FormCarrierCombine_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        _carriers = Carriers.GetCarriers(CarrierNums);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Carrier Name", 160));
        gridMain.Columns.Add(new GridColumn("Phone", 90));
        gridMain.Columns.Add(new GridColumn("Address", 130));
        gridMain.Columns.Add(new GridColumn("Address2", 120));
        gridMain.Columns.Add(new GridColumn("City", 110));
        gridMain.Columns.Add(new GridColumn("ST", 60));
        gridMain.Columns.Add(new GridColumn("Zip", 90));
        gridMain.Columns.Add(new GridColumn("ElectID", 60));

        gridMain.ListGridRows.Clear();

        foreach (var carrier in _carriers)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(carrier.CarrierName);
            gridRow.Cells.Add(carrier.Phone);
            gridRow.Cells.Add(carrier.Address);
            gridRow.Cells.Add(carrier.Address2);
            gridRow.Cells.Add(carrier.City);
            gridRow.Cells.Add(carrier.State);
            gridRow.Cells.Add(carrier.Zip);
            gridRow.Cells.Add(carrier.ElectID);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedCarrierNum = _carriers[e.Row].CarrierNum;
        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            ShowError("Please select an item first.");
            return;
        }

        SelectedCarrierNum = _carriers[gridMain.SelectedIndices[0]].CarrierNum;

        DialogResult = DialogResult.OK;
    }
}