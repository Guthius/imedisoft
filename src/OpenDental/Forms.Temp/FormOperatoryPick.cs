using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using OpenDental.UI;

namespace OpenDental;

public partial class FormOperatoryPick : FormODBase
{
    private readonly List<Operatory> _operatories;

    public long SelectedOperatoryNum { get; set; }

    public FormOperatoryPick(List<Operatory> operatories)
    {
        _operatories = operatories;

        InitializeComponent();
    }

    private void FormOperatoryPick_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!SelectOperatory())
        {
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Op Name", 180));
        gridMain.Columns.Add(new GridColumn("Abbrev", 70));
        gridMain.Columns.Add(new GridColumn("IsHidden", 64, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Clinic", 85));
        gridMain.Columns.Add(new GridColumn("Provider", 70));
        gridMain.Columns.Add(new GridColumn("Hygienist", 70));
        gridMain.Columns.Add(new GridColumn("IsHygiene", 64, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("IsWebSched", 74, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("IsNewPat", 50, HorizontalAlignment.Center) {IsWidthDynamic = true});

        gridMain.ListGridRows.Clear();

        foreach (var operatory in _operatories)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(operatory.OpName);
            gridRow.Cells.Add(operatory.Abbrev);
            gridRow.Cells.Add(operatory.IsHidden ? "X" : "");
            gridRow.Cells.Add(Clinics.GetAbbr(operatory.ClinicNum));
            gridRow.Cells.Add(Providers.GetAbbr(operatory.ProvDentist));
            gridRow.Cells.Add(Providers.GetAbbr(operatory.ProvHygienist));
            gridRow.Cells.Add(operatory.IsHygiene ? "X" : "");
            gridRow.Cells.Add(operatory.IsWebSched ? "X" : "");
            gridRow.Cells.Add("");
            gridRow.Tag = operatory;

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private bool SelectOperatory()
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return false;
        }

        SelectedOperatoryNum = ((Operatory) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag).OperatoryNum;
        return true;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (!SelectOperatory())
        {
            return;
        }

        DialogResult = DialogResult.OK;
    }
}