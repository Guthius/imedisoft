using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Core.ViewModels;

namespace OpenDental.Features.Providers.ViewModels;

public sealed partial class ProviderPickViewModel : DialogViewModel<List<ProviderDto>>
{
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _showAll;

    [ObservableProperty]
    private ObservableCollection<ProviderDto> _providers = [];

    [RelayCommand]
    private void SelectNone()
    {
        Close([]);
    }

    [RelayCommand]
    private void Close()
    {
        Close([]);
    }
}