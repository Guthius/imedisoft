using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormPayorTypes : FormODBase
{
    private List<PayorType> _payorTypes;

    public Patient PatientCur;

    public FormPayorTypes()
    {
        InitializeComponent();
    }

    private void FormPayorTypes_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        _payorTypes = PayorTypes.GetPatientData(PatientCur.PatNum);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date Start", 70) {TextAlign = HorizontalAlignment.Center});
        gridMain.Columns.Add(new GridColumn("Date End", 70) {TextAlign = HorizontalAlignment.Center});
        gridMain.Columns.Add(new GridColumn("SOP Code", 70));
        gridMain.Columns.Add(new GridColumn("Description", 250));
        gridMain.Columns.Add(new GridColumn("Note", 100));

        gridMain.ListGridRows.Clear();

        for (var i = 0; i < _payorTypes.Count; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(_payorTypes[i].DateStart.ToShortDateString());
            gridRow.Cells.Add(i == _payorTypes.Count - 1 ? "Current" : _payorTypes[i + 1].DateStart.ToShortDateString());
            gridRow.Cells.Add(_payorTypes[i].SopCode);
            gridRow.Cells.Add(Sops.GetDescriptionFromCode(_payorTypes[i].SopCode));
            gridRow.Cells.Add(_payorTypes[i].Note);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var payorType = _payorTypes[e.Row];

        using var formPayorTypeEdit = new FormPayorTypeEdit(payorType);

        if (formPayorTypeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var payorType = new PayorType
        {
            PatNum = PatientCur.PatNum,
            DateStart = DateTime.Today
        };

        using var formPayorTypeEdit = new FormPayorTypeEdit(payorType);

        if (formPayorTypeEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }
}