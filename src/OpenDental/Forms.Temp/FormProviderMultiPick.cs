using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormProvidersMultiPick : FormODBase
{
    private List<ProviderDto> _providers;

    public List<ProviderDto> SelectedProviders { get; set; }

    public FormProvidersMultiPick(List<ProviderDto> providers = null)
    {
        InitializeComponent();

        _providers = providers;
    }

    private void FormProvidersMultiPick_Load(object sender, EventArgs e)
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Abbrev", 90));
        gridMain.Columns.Add(new GridColumn("Last Name", 90));
        gridMain.Columns.Add(new GridColumn("First Name", 90));

        gridMain.ListGridRows.Clear();

        _providers ??= Providers.GetDeepCopy(true);

        foreach (var provider in _providers)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(provider.Abbr);
            gridRow.Cells.Add(provider.LastName);
            gridRow.Cells.Add(provider.FirstName);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();

        var selectedProviderIds = SelectedProviders.Select(x => x.Id).ToList();
        for (var i = 0; i < _providers.Count; i++)
        {
            if (selectedProviderIds.Contains(_providers[i].Id))
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void ButtonProviderDentist_Click(object sender, EventArgs e)
    {
        SelectedProviders = [];

        for (var i = 0; i < _providers.Count; i++)
        {
            if (!_providers[i].IsSecondary)
            {
                SelectedProviders.Add(_providers[i]);

                gridMain.SetSelected(i);

                continue;
            }

            gridMain.SetSelected(i, false);
        }
    }

    private void ButtonProviderHygienist_Click(object sender, EventArgs e)
    {
        SelectedProviders = [];

        for (var i = 0; i < _providers.Count; i++)
        {
            if (_providers[i].IsSecondary)
            {
                SelectedProviders.Add(_providers[i]);

                gridMain.SetSelected(i);

                continue;
            }

            gridMain.SetSelected(i, false);
        }
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SelectedProviders = [];

        foreach (var index in gridMain.SelectedIndices)
        {
            SelectedProviders.Add(_providers[index]);
        }

        DialogResult = DialogResult.OK;
    }
}