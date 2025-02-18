using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormPharmacyEdit : FormODBase
{
    private readonly Pharmacy _pharmacy;

    public FormPharmacyEdit(Pharmacy pharmacy)
    {
        _pharmacy = pharmacy;

        InitializeComponent();
    }

    private void FormPharmacyEdit_Load(object sender, EventArgs e)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            label11.Text = "City, Province, Postal Code";
        }

        textStoreName.Text = _pharmacy.StoreName;
        textPhone.Text = _pharmacy.Phone;
        textFax.Text = _pharmacy.Fax;
        textAddress.Text = _pharmacy.Address;
        textAddress2.Text = _pharmacy.Address2;
        textCity.Text = _pharmacy.City;
        textState.Text = _pharmacy.State;
        textZip.Text = _pharmacy.Zip;
        textNote.Text = _pharmacy.Note;

        var pharmClinics = PharmClinics.GetPharmClinicsForPharmacy(_pharmacy.PharmacyNum);

        comboClinic.ListClinicNumsSelected = pharmClinics.Select(x => x.ClinicNum).ToList();
        comboClinic.Tag = pharmClinics.Where(x => comboClinic.ListClinicNumsSelected.Contains(x.ClinicNum)).ToList();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_pharmacy.PharmacyNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!ConfirmOk("Delete this Pharmacy?"))
        {
            return;
        }

        try
        {
            Pharmacies.DeleteObject(_pharmacy.PharmacyNum);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);

            return;
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textStoreName.Text == "")
        {
            ShowError("Store name cannot be blank.");
            return;
        }

        if (CultureInfo.CurrentCulture.Name == "en-US")
        {
            if (textPhone.Text != "" && TelephoneNumbers.FormatNumbersExactTen(textPhone.Text) == "")
            {
                ShowError("Phone number must be in a 10-digit format.");
                return;
            }

            if (textFax.Text != "" && TelephoneNumbers.FormatNumbersExactTen(textFax.Text) == "")
            {
                ShowError("Fax number must be in a 10-digit format.");
                return;
            }
        }

        _pharmacy.StoreName = textStoreName.Text;
        _pharmacy.PharmID = "";
        _pharmacy.Phone = textPhone.Text;
        _pharmacy.Fax = textFax.Text;
        _pharmacy.Address = textAddress.Text;
        _pharmacy.Address2 = textAddress2.Text;
        _pharmacy.City = textCity.Text;
        _pharmacy.State = textState.Text;
        _pharmacy.Zip = textZip.Text;
        _pharmacy.Note = textNote.Text;

        if (_pharmacy.PharmacyNum == 0)
        {
            Pharmacies.Insert(_pharmacy);
        }
        else
        {
            Pharmacies.Update(_pharmacy);
        }

        var pharmClinics = (List<PharmClinic>) comboClinic.Tag;
        var pharmClinicsNew = new List<PharmClinic>();

        foreach (var clinicNum in comboClinic.ListClinicNumsSelected)
        {
            pharmClinicsNew.Add(pharmClinics.Any(x => x.ClinicNum == clinicNum)
                ? pharmClinics.First(x => x.ClinicNum == clinicNum)
                : new PharmClinic(_pharmacy.PharmacyNum, clinicNum));
        }

        PharmClinics.Sync(pharmClinicsNew, pharmClinics);

        DialogResult = DialogResult.OK;
    }
}