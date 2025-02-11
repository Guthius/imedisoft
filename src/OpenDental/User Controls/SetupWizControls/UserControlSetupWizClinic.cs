using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Core.Features.Providers;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizClinic : SetupWizControl
{
    private int _blink;

    public UserControlSetupWizClinic()
    {
        InitializeComponent();
    }

    private void UserControlSetupWizClinic_Load(object sender, EventArgs e)
    {
        FillGrid();

        if (Clinics.GetCount(true) != 0)
        {
            return;
        }

        MsgBox.Show("FormSetupWizard", "You have no valid clinics. Please click the Add button to add a clinic.");

        timer1.Start();
    }

    private void FillGrid()
    {
        var needsAttnCol = OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
        gridMain.BeginUpdate();
        gridMain.Columns.Clear();
        GridColumn col;
        col = new GridColumn("Clinic", 110);
        gridMain.Columns.Add(col);
        col = new GridColumn("Abbrev", 70);
        gridMain.Columns.Add(col);
        col = new GridColumn("Phone", 100);
        gridMain.Columns.Add(col);
        col = new GridColumn("Address", 120);
        gridMain.Columns.Add(col);
        col = new GridColumn("City", 90);
        gridMain.Columns.Add(col);
        col = new GridColumn("State", 50);
        gridMain.Columns.Add(col);
        col = new GridColumn("ZIP", 80);
        gridMain.Columns.Add(col);
        col = new GridColumn("Default Prov", 75);
        gridMain.Columns.Add(col);
        col = new GridColumn("IsHidden", 55, HorizontalAlignment.Center);
        gridMain.Columns.Add(col);
        gridMain.ListGridRows.Clear();
        var IsAllComplete = true;
        var listClins = Clinics.GetDeepCopy();
        if (listClins.Count == 0)
        {
            IsAllComplete = false;
        }

        foreach (var clinCur in listClins)
        {
            var row = new GridRow();

            row.Cells.Add(clinCur.Description);
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.Description))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(clinCur.Abbr);
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.Abbr))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(TelephoneNumbers.FormatNumbersExactTen(clinCur.PhoneNumber));
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.PhoneNumber))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(clinCur.AddressLine1);
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.AddressLine1))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(clinCur.City);
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.City))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(clinCur.State);
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.State))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(clinCur.Zip);
            if (!clinCur.IsHidden && string.IsNullOrEmpty(clinCur.Zip))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                IsAllComplete = false;
            }

            row.Cells.Add(Providers.GetAbbr(clinCur.DefaultProviderId ?? 0));
            row.Cells.Add(clinCur.IsHidden ? "X" : "");
            row.Tag = clinCur;
            gridMain.ListGridRows.Add(row);
        }

        gridMain.EndUpdate();

        IsDone = IsAllComplete;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (_blink > 5)
        {
            pictureAdd.Visible = true;
            timer1.Stop();
            return;
        }

        pictureAdd.Visible = !pictureAdd.Visible;

        _blink++;
    }

    private void butAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ClinicEdit))
        {
            return;
        }

        using var formClinicEdit = new FormClinicEdit(new ClinicDto());

        if (formClinicEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Clinics.Insert(formClinicEdit.ClinicCur);
        DataValid.SetInvalid(InvalidType.Providers);

        FillGrid();
    }

    private void gridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ClinicEdit))
        {
            return;
        }

        var clinCur = (ClinicDto) gridMain.ListGridRows[e.Row].Tag;

        using var formClinicEdit = new FormClinicEdit(clinCur);

        if (formClinicEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        DataValid.SetInvalid(InvalidType.Providers);

        FillGrid();
    }

    private void butAdvanced_Click(object sender, EventArgs e)
    {
        using var formClinics = new FormClinics();

        formClinics.ShowDialog();

        FillGrid();
    }
}