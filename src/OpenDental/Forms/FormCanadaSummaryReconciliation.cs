using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Features.Providers.Dtos;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental.Forms;

public partial class FormCanadaSummaryReconciliation : FormODBase
{
    private List<Carrier> _carriers = [];
    private List<ProviderDto> _providers;
    private List<CanadianNetwork> _canadianNetworks;

    public FormCanadaSummaryReconciliation()
    {
        InitializeComponent();
    }

    private void FormCanadaPaymentReconciliation_Load(object sender, EventArgs e)
    {
        _canadianNetworks = CanadianNetworks.GetDeepCopy();

        foreach (var canadianNetwork in _canadianNetworks)
        {
            listNetworks.Items.Add(canadianNetwork.Abbrev + " - " + canadianNetwork.Descript);
        }

        _carriers = Carriers.GetWhere(x => x.CDAnetVersion != "02" && (x.CanadianSupportedTypes & CanSupTransTypes.RequestForSummaryReconciliation_05) == CanSupTransTypes.RequestForSummaryReconciliation_05);
        foreach (var carrier in _carriers)
        {
            listCarriers.Items.Add(carrier.CarrierName);
        }

        var defaultProvNum = PrefC.GetLong(PrefName.PracticeDefaultProv);

        _providers = Providers.GetDeepCopy(true);

        for (var i = 0; i < _providers.Count; i++)
        {
            listTreatingProvider.Items.Add(_providers[i].Abbr);

            if (_providers[i].Id == defaultProvNum)
            {
                listTreatingProvider.SelectedIndex = i;
            }
        }

        textDateReconciliation.Text = DateTime.Today.ToShortDateString();
    }

    private void CheckBoxGetForAllCarriers_Click(object sender, EventArgs e)
    {
        groupCarrierOrNetwork.Enabled = !checkGetForAllCarriers.Checked;
    }

    private void ListBoxCarriers_Click(object sender, EventArgs e)
    {
        listNetworks.SelectedIndex = -1;
    }

    private void ListBoxNetwork_Click(object sender, EventArgs e)
    {
        listCarriers.SelectedIndex = -1;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!checkGetForAllCarriers.Checked)
        {
            if (listCarriers.SelectedIndex < 0 && listNetworks.SelectedIndex < 0)
            {
                ShowError("You must first choose one carrier or one network.");

                return;
            }
        }

        if (listTreatingProvider.SelectedIndex < 0)
        {
            ShowError("You must first choose a treating provider.");

            return;
        }

        DateTime reconciliationDate;
        try
        {
            reconciliationDate = DateTime.Parse(textDateReconciliation.Text).Date;
        }
        catch
        {
            ShowError("Reconciliation date invalid.");

            return;
        }

        Cursor = Cursors.WaitCursor;
        try
        {
            if (checkGetForAllCarriers.Checked)
            {
                var carrier = new Carrier
                {
                    CDAnetVersion = "04",
                    ElectID = "999999",
                    CanadianEncryptionMethod = 1
                };

                var clearinghouseHq = Canadian.GetCanadianClearinghouseHq(carrier);
                var clearinghouseClin = Clearinghouses.OverrideFields(clearinghouseHq, Clinics.ClinicNum);
                
                CanadianOutput.GetSummaryReconciliation(clearinghouseClin, carrier, null, _providers[listTreatingProvider.SelectedIndex], 
                    reconciliationDate, false, FormCCDPrint.PrintCCD);
            }
            else
            {
                if (listCarriers.SelectedIndex >= 0)
                {
                    var carrier = _carriers[listCarriers.SelectedIndex];
                    var clearinghouseHq = Canadian.GetCanadianClearinghouseHq(carrier);
                    var clearinghouseClin = Clearinghouses.OverrideFields(clearinghouseHq, Clinics.ClinicNum);

                    CanadianOutput.GetSummaryReconciliation(clearinghouseClin, carrier, null, _providers[listTreatingProvider.SelectedIndex], 
                        reconciliationDate, false, FormCCDPrint.PrintCCD);
                }
                else
                {
                    var clearinghouseHq = Canadian.GetCanadianClearinghouseHq(null);
                    var clearinghouseClin = Clearinghouses.OverrideFields(clearinghouseHq, Clinics.ClinicNum);

                    CanadianOutput.GetSummaryReconciliation(clearinghouseClin, null,
                        _canadianNetworks[listNetworks.SelectedIndex],
                        _providers[listTreatingProvider.SelectedIndex],
                        reconciliationDate, false, FormCCDPrint.PrintCCD);
                }
            }

            Cursor = Cursors.Default;

            ShowInfo("Done.");
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;

            ShowError("Request failed: " + ex.Message);
        }

        DialogResult = DialogResult.OK;
    }
}