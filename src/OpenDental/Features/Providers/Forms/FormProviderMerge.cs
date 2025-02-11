using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDentBusiness;

namespace OpenDental.Features.Providers.Forms;

public partial class FormProviderMerge : FormODBase
{
    private List<ProviderDto> _activeProviders;

    public FormProviderMerge()
    {
        InitializeComponent();

        _activeProviders = Imedisoft.Core.Features.Providers.Providers.GetWhere(x => !x.IsDeleted, true);
    }

    private void butChangeProvInto_Click(object sender, EventArgs e)
    {
        var frmProviderPick = new FrmProviderPick(_activeProviders);
        
        frmProviderPick.ShowDialog();
        
        if (!frmProviderPick.IsDialogOK)
        {
            return;
        }

        var providerSelected = Imedisoft.Core.Features.Providers.Providers.GetById(frmProviderPick.ProvNumSelected);
        
        textAbbrInto.Text = providerSelected.Abbr;
        textProvNumInto.Text = providerSelected.Id.ToString();
        textNpiInto.Text = providerSelected.NationalProviderId;
        textFullNameInto.Text = providerSelected.FirstName + " " + providerSelected.LastName;
        
        CheckUIState();
    }

    private void butChangeProvFrom_Click(object sender, EventArgs e)
    {
        var frmProviderPick = new FrmProviderPick(checkDeletedProvs.Checked ? Imedisoft.Core.Features.Providers.Providers.GetDeepCopy() : _activeProviders);
        
        frmProviderPick.ShowDialog();
        
        if (!frmProviderPick.IsDialogOK)
        {
            return;
        }

        var providerSelected = Imedisoft.Core.Features.Providers.Providers.GetById(frmProviderPick.ProvNumSelected);
        
        textAbbrFrom.Text = providerSelected.Abbr;
        textProvNumFrom.Text = providerSelected.Id.ToString();
        textNpiFrom.Text = providerSelected.NationalProviderId;
        textFullNameFrom.Text = providerSelected.FirstName + " " + providerSelected.LastName;
        
        CheckUIState();
    }

    private void CheckUIState()
    {
        butMerge.Enabled = textProvNumInto.Text != "" && textProvNumFrom.Text != "";
    }

    private void ButtonMerge_Click(object sender, EventArgs e)
    {
        var differentFields = "";
        if (textProvNumFrom.Text == textProvNumInto.Text)
        {
            ShowError("You must select two different providers to merge.");
            return;
        }

        if (textNpiFrom.Text != textNpiInto.Text)
        {
            differentFields += "\r\nNPI";
        }

        if (textFullNameFrom.Text != textFullNameInto.Text)
        {
            differentFields += "\r\nFull Name";
        }

        var numPats = Imedisoft.Core.Features.Providers.Providers.CountPats(SIn.Long(textProvNumFrom.Text));
        var numClaims = Imedisoft.Core.Features.Providers.Providers.CountClaims(SIn.Long(textProvNumFrom.Text));
        
        if (!Confirm("Are you sure?  The results are permanent and cannot be undone."))
        {
            return;
        }

        var confirmPrompt = "";
        if (differentFields != "")
        {
            confirmPrompt = "The following provider fields do not match: " + differentFields + "\r\n";
        }

        confirmPrompt += 
            "This change is irreversible.  " +
            "This provider is the primary or secondary provider for " + numPats + " active patients, and the billing or treating provider for " + numClaims + " claims.  " +
            "Continue anyways?";
        
        if (!ConfirmOk(confirmPrompt))
        {
            return;
        }

        var rowsChanged = Imedisoft.Core.Features.Providers.Providers.Merge(SIn.Long(textProvNumFrom.Text), SIn.Long(textProvNumInto.Text));
        
        var logText = "Providers merged: " + textAbbrFrom.Text + " merged into " + textAbbrInto.Text + ".\r\nRows changed: " + SOut.Long(rowsChanged);
        
        SecurityLogs.MakeLogEntry(EnumPermType.ProviderMerge, 0, logText);
        
        textAbbrFrom.Clear();
        textProvNumFrom.Clear();
        textNpiFrom.Clear();
        textFullNameFrom.Clear();
        
        CheckUIState();
        
        ShowInfo("Done.");
        
        DataValid.SetInvalid(InvalidType.Providers);
        
        _activeProviders = Imedisoft.Core.Features.Providers.Providers.GetWhere(x => !x.IsDeleted, true);
    }
}