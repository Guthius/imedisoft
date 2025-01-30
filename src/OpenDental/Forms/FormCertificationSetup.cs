using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCertificationSetup : FormODBase
{
    private List<Cert> _filteredCerts;

    public FormCertificationSetup()
    {
        InitializeComponent();
    }

    private void FormCertificationSetup_Load(object sender, EventArgs e)
    {
        var defs = Defs.GetDefsForCategory(DefCat.CertificationCategories, true);

        listBoxCategories.Items.AddList(defs, x => x.ItemName);

        FillGrid();
    }

    private void FillGrid()
    {
        if (listBoxCategories.SelectedIndex == -1)
        {
            return;
        }

        _filteredCerts = Certs.GetAllForCategory(listBoxCategories.GetSelected<Def>().DefNum);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Certification", 260));
        gridMain.Columns.Add(new GridColumn("WikiPage", 260));
        gridMain.Columns.Add(new GridColumn("Hidden", 66, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();

        for (var i = 0; i < _filteredCerts.Count; i++)
        {
            if (_filteredCerts[i].ItemOrder != i)
            {
                _filteredCerts[i].ItemOrder = i;

                Certs.Update(_filteredCerts[i]);
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(_filteredCerts[i].Description);
            gridRow.Cells.Add(_filteredCerts[i].WikiPageLink);
            gridRow.Cells.Add(_filteredCerts[i].IsHidden ? "X" : "");

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void ListBoxCategories_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        var index = gridMain.GetSelectedIndex();
        switch (index)
        {
            case -1:
                ShowError("Please select a category first.");
                return;

            case 0:
                return;
        }

        _filteredCerts[index].ItemOrder = index - 1;
        Certs.Update(_filteredCerts[index]);

        _filteredCerts[index - 1].ItemOrder = index;
        Certs.Update(_filteredCerts[index - 1]);

        FillGrid();

        gridMain.SetSelected(index - 1);
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        var index = gridMain.GetSelectedIndex();
        if (index == -1)
        {
            ShowError("Please select a category first.");
            return;
        }

        if (index == _filteredCerts.Count - 1)
        {
            return;
        }

        _filteredCerts[index].ItemOrder = index + 1;
        Certs.Update(_filteredCerts[index]);

        _filteredCerts[index + 1].ItemOrder = index;
        Certs.Update(_filteredCerts[index + 1]);

        FillGrid();

        gridMain.SetSelected(index + 1);
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (listBoxCategories.SelectedIndex == -1)
        {
            ShowError("Please select a category first.");
            return;
        }

        var cert = new Cert
        {
            IsNew = true,
            CertCategoryNum = listBoxCategories.GetSelected<Def>().DefNum,
            ItemOrder = _filteredCerts.Count
        };

        using var formCertificationEdit = new FormCertificationEdit(cert);

        if (formCertificationEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var cert = Certs.GetOne(_filteredCerts[e.Row].CertNum);

        using var formCertificationEdit = new FormCertificationEdit(cert);

        if (formCertificationEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }
}