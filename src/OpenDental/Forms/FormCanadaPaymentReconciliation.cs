using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Features.Providers.Dtos;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental.Forms;

public partial class FormCanadaPaymentReconciliation : FormODBase
{
    private List<Carrier> _carriers = [];
    private List<ProviderDto> _providers;

    public FormCanadaPaymentReconciliation()
    {
        InitializeComponent();
    }

    private void FormCanadaPaymentReconciliation_Load(object sender, EventArgs e)
    {
        _carriers = Carriers.GetWhere(x => x.CDAnetVersion != "02" && (x.CanadianSupportedTypes & CanSupTransTypes.RequestForPaymentReconciliation_06) == CanSupTransTypes.RequestForPaymentReconciliation_06);

        foreach (var carrier in _carriers)
        {
            listCarriers.Items.Add(carrier.CarrierName);
        }

        var defaultProvNum = PrefC.GetLong(PrefName.PracticeDefaultProv);

        _providers = Providers.GetDeepCopy(true);

        for (var i = 0; i < _providers.Count; i++)
        {
            if (!_providers[i].IsCdaNet)
            {
                continue;
            }

            listBillingProvider.Items.Add(_providers[i].Abbr);
            listTreatingProvider.Items.Add(_providers[i].Abbr);

            if (_providers[i].Id != defaultProvNum)
            {
                continue;
            }

            listBillingProvider.SelectedIndex = i;
            textBillingOfficeNumber.Text = _providers[i].CanadianOfficeNumber;

            listTreatingProvider.SelectedIndex = i;
            textTreatingOfficeNumber.Text = _providers[i].CanadianOfficeNumber;
        }

        textDateReconciliation.Text = DateTime.Today.ToShortDateString();
    }

    private void ListBoxBillingProvider_Click(object sender, EventArgs e)
    {
        textBillingOfficeNumber.Text = _providers[listBillingProvider.SelectedIndex].CanadianOfficeNumber;
    }

    private void ListBoxTreatingProvider_Click(object sender, EventArgs e)
    {
        textTreatingOfficeNumber.Text = _providers[listTreatingProvider.SelectedIndex].CanadianOfficeNumber;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (listCarriers.SelectedIndex < 0)
        {
            ShowError("You must first choose a carrier.");
            return;
        }

        if (listBillingProvider.SelectedIndex < 0)
        {
            ShowError("You must first choose a billing provider.");
            return;
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
            var carrier = _carriers[listCarriers.SelectedIndex];
            var clearinghouseHq = Canadian.GetCanadianClearinghouseHq(carrier);
            var clearinghouseClin = Clearinghouses.OverrideFields(clearinghouseHq, Clinics.ClinicNum);

            CanadianOutput.GetPaymentReconciliations(
                clearinghouseClin, carrier,
                _providers[listTreatingProvider.SelectedIndex],
                _providers[listBillingProvider.SelectedIndex],
                reconciliationDate, Clinics.ClinicNum, false,
                FormCCDPrint.PrintCCD);

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