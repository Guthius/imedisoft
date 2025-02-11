using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental.Forms;

public partial class FormCanadaOutstandingTransactions : FormODBase
{
    private List<Carrier> _carriers = [];
    private List<ProviderDto> _providers;

    public FormCanadaOutstandingTransactions()
    {
        InitializeComponent();
    }

    private void FormCanadaOutstandingTransactions_Load(object sender, EventArgs e)
    {
        _carriers = Carriers.GetWhere(x => (x.CanadianSupportedTypes & CanSupTransTypes.RequestForOutstandingTrans_04) == CanSupTransTypes.RequestForOutstandingTrans_04);
        _providers = Providers.GetDeepCopy(true);

        foreach (var carrier in _carriers)
        {
            listCarriers.Items.Add(carrier.CarrierName);
        }

        foreach (var provider in _providers)
        {
            if (!provider.IsCdaNet || provider.NationalProviderId == "" || provider.CanadianOfficeNumber == "")
            {
                continue;
            }

            if (!listOfficeNumbers.Items.Contains(provider.CanadianOfficeNumber))
            {
                listOfficeNumbers.Items.Add(provider.CanadianOfficeNumber);
            }
        }

        if (listOfficeNumbers.Items.Count >= 1)
        {
            return;
        }

        ShowError("At least one unhidden provider must have a CDA Number and an Office Number set before running a Request for Outstanding Transactions.");

        Close();
    }

    private void RadioButtonVersion4Itrans_Click(object sender, EventArgs e)
    {
        radioVersion4Itrans.Checked = true;
        radioVersion4ToCarrier.Checked = false;
        groupCarrier.Enabled = false;
    }

    private void RadioButtonVersion4ToCarrier_Click(object sender, EventArgs e)
    {
        radioVersion4Itrans.Checked = false;
        radioVersion4ToCarrier.Checked = true;
        groupCarrier.Enabled = true;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (radioVersion4ToCarrier.Checked)
        {
            if (listCarriers.SelectedIndex < 0)
            {
                ShowError("You must first select a carrier to use.");
                return;
            }
        }

        if (listOfficeNumbers.SelectedIndex < 0)
        {
            ShowError("You must first select an Office Number to use.");
            return;
        }

        Cursor = Cursors.WaitCursor;

        ProviderDto selectedProvider = null;

        foreach (var provider in _providers)
        {
            if (provider.CanadianOfficeNumber != listOfficeNumbers.SelectedItem.ToString() || provider.NationalProviderId == "" || !provider.IsCdaNet)
            {
                continue;
            }

            selectedProvider = provider;
            break;
        }

        const string formatVersion = "04";

        Carrier carrier = null;
        if (radioVersion4ToCarrier.Checked)
        {
            carrier = _carriers[listCarriers.SelectedIndex];
        }

        try
        {
            CanadianOutput.GetOutstandingForDefault(selectedProvider, formatVersion, carrier, FormClaimPrint.PrintCdaClaimForm, FormCCDPrint.PrintCCD);

            Cursor = Cursors.Default;

            ShowInfo("Done.");
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;

            ShowException(ex, "Request failed.");
        }

        DialogResult = DialogResult.OK;
    }
}