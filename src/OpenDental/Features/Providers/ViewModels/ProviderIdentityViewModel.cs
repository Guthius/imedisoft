using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenDental.Core.ViewModels;
using OpenDental.Features.Providers.Models;

namespace OpenDental.Features.Providers.ViewModels;

public sealed partial class ProviderIdentityViewModel : DialogViewModel
{
    private readonly ProviderIdentityModel _providerIdentityModel;

    [ObservableProperty]
    private List<string> _types = ["BlueCross", "BlueShield", "SiteNumber", "CommercialNumber"];

    [ObservableProperty] private string _payorId = string.Empty;
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _value = string.Empty;

    public ProviderIdentityViewModel(ProviderIdentityModel providerIdentityModel)
    {
        _providerIdentityModel = providerIdentityModel;

        PayorId = _providerIdentityModel.PayorId;
        Type = _providerIdentityModel.Type;
        Value = _providerIdentityModel.Value;
    }

    [RelayCommand]
    private void Save()
    {
        _providerIdentityModel.PayorId = PayorId;
        _providerIdentityModel.Type = Type;
        _providerIdentityModel.Value = Value;

        Close(true);
    }
}