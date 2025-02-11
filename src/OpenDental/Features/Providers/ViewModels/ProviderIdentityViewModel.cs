using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Core.ViewModels;

namespace OpenDental.Features.Providers.ViewModels;

public sealed partial class ProviderIdentityViewModel : DialogViewModel
{
    private readonly ProviderIdentityDto _providerIdentityDto;

    [ObservableProperty]
    private List<string> _types = ["BlueCross", "BlueShield", "SiteNumber", "CommercialNumber"];

    [ObservableProperty] private string _payorId = string.Empty;
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _value = string.Empty;

    public ProviderIdentityViewModel(ProviderIdentityDto providerIdentityDto)
    {
        _providerIdentityDto = providerIdentityDto;

        PayorId = _providerIdentityDto.PayorId;
        Type = _providerIdentityDto.Type;
        Value = _providerIdentityDto.Value;
    }

    [RelayCommand]
    private void Save()
    {
        _providerIdentityDto.PayorId = PayorId;
        _providerIdentityDto.Type = Type;
        _providerIdentityDto.Value = Value;

        Close(true);
    }
}