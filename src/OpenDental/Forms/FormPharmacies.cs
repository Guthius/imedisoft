using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.Bridges;
using OpenDental.UI;
using OpenDentBusiness;
using Pharmacy = Imedisoft.Core.Entities.Pharmacy;

namespace OpenDental.Forms;

public partial class FormPharmacies : FormODBase
{
    private List<Pharmacy> _pharmacies;
    private bool _changed;

    public bool IsSelectionMode { get; set; }
    public long SelectedPharmacyNum { get; set; }

    public FormPharmacies()
    {
        InitializeComponent();
    }

    private void FormPharmacies_Load(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            butOK.Visible = false;
            butNone.Visible = false;
        }

        FillGrid();

        if (SelectedPharmacyNum == 0)
        {
            return;
        }

        for (var i = 0; i < _pharmacies.Count; i++)
        {
            if (_pharmacies[i].PharmacyNum != SelectedPharmacyNum)
            {
                continue;
            }

            gridMain.SetSelected(i);
            break;
        }
    }

    private void FillGrid()
    {
        Pharmacies.RefreshCache();

        _pharmacies = Pharmacies.GetDeepCopy();

        var pharmClinics = PharmClinics.GetPharmClinicsForPharmacies(_pharmacies.Select(x => x.PharmacyNum).ToList());

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Store Name", 130));
        gridMain.Columns.Add(new GridColumn("Phone", 90));
        gridMain.Columns.Add(new GridColumn("Fax", 90));
        gridMain.Columns.Add(new GridColumn("Address", 120));
        gridMain.Columns.Add(new GridColumn("City", 90));
        gridMain.Columns.Add(new GridColumn("Clinics", 115));
        gridMain.Columns.Add(new GridColumn("Note", 100));

        gridMain.ListGridRows.Clear();

        foreach (var pharmacy in _pharmacies)
        {
            var clinicNums = pharmClinics.FindAll(x => x.PharmacyNum == pharmacy.PharmacyNum).Select(y => y.ClinicNum).ToList();
            var clinics = Clinics.GetClinics(clinicNums);

            var address = pharmacy.Address;
            if (pharmacy.Address2 != "")
            {
                address += "\r\n" + pharmacy.Address2;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(pharmacy.StoreName);
            gridRow.Cells.Add(pharmacy.Phone);

            if (Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled)
            {
                gridRow.Cells[gridRow.Cells.Count - 1].ColorText = Color.Blue;
                gridRow.Cells[gridRow.Cells.Count - 1].Underline = YN.Yes;
            }

            gridRow.Cells.Add(pharmacy.Fax);
            gridRow.Cells.Add(address);
            gridRow.Cells.Add(pharmacy.City);
            gridRow.Cells.Add(string.Join(",", clinics.Select(x => x.Abbr)));
            gridRow.Cells.Add(pharmacy.Note);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var pharmacy = new Pharmacy
        {
            IsNew = true
        };

        using var formPharmacyEdit = new FormPharmacyEdit(pharmacy);

        formPharmacyEdit.ShowDialog();

        FillGrid();

        _changed = true;
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (IsSelectionMode)
        {
            SelectedPharmacyNum = _pharmacies[e.Row].PharmacyNum;
            
            DialogResult = DialogResult.OK;
            return;
        }

        using var formPharmacyEdit = new FormPharmacyEdit(_pharmacies[e.Row]);
        
        formPharmacyEdit.ShowDialog();
        
        FillGrid();
        
        _changed = true;
    }

    private void GridMain_CellClick(object sender, ODGridClickEventArgs e)
    {
        var gridCell = gridMain.ListGridRows[e.Row].Cells[e.Col];
        
        if (gridCell.ColorText == Color.Blue && gridCell.Underline == YN.Yes && Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled)
        {
            DentalTek.PlaceCall(gridCell.Text);
        }
    }

    private void ButtonNone_Click(object sender, EventArgs e)
    {
        SelectedPharmacyNum = 0;

        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SelectedPharmacyNum = gridMain.GetSelectedIndex() == -1 ? 0 : _pharmacies[gridMain.GetSelectedIndex()].PharmacyNum;

        DialogResult = DialogResult.OK;
    }

    private void FormPharmacies_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Pharmacies);
        }
    }
}