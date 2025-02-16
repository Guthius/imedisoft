using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental;

public partial class FormEtrans835PickEra : FormODBase
{
    private readonly List<Etrans> _etranses;
    private readonly long _specificClaimNum;

    public FormEtrans835PickEra(List<Etrans> etranses, long specificClaimNum)
    {
        _etranses = etranses;
        _specificClaimNum = specificClaimNum;

        InitializeComponent();
    }

    private void FormEtrans835PickEra_Load(object sender, EventArgs e)
    {
        FillGridEras();
    }

    private void FillGridEras()
    {
        gridEras.BeginUpdate();

        gridEras.Columns.Clear();
        gridEras.Columns.Add(new GridColumn("Carrier", 200));
        gridEras.Columns.Add(new GridColumn("Date Rec'd", 70, HorizontalAlignment.Center));
        gridEras.Columns.Add(new GridColumn("Note", 70) {IsWidthDynamic = true});

        gridEras.ListGridRows.Clear();

        foreach (var etrans in _etranses)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(etrans.CarrierNameRaw);
            gridRow.Cells.Add(etrans.DateTimeTrans.ToShortDateString());
            gridRow.Cells.Add(etrans.Note);

            gridEras.ListGridRows.Add(gridRow);
        }

        gridEras.EndUpdate();
    }

    private void GridEras_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        EtransL.ViewFormForEra(_etranses[gridEras.SelectedIndices[0]], this, _specificClaimNum);
    }
}