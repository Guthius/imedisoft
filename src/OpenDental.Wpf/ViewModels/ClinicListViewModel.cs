using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness;

namespace Imedisoft.ViewModels;

internal sealed partial class ClinicListViewModel : ObservableObject
{
    [ObservableProperty]
    private List<ClinicDto> _clinics;
    
    public ClinicListViewModel()
    {
        _clinics = Imedisoft.Core.Features.Clinics.Clinics.GetAllForUserod(Security.CurUser);
    }

    [RelayCommand]
    private void MovePatients()
    {
    }

    [RelayCommand]
    private void SelectAll()
    {
    }

    [RelayCommand]
    private void SelectNone()
    {
    }

    [RelayCommand]
    private void Add()
    {
    }

    [RelayCommand]
    private void Save()
    {
        
    }
}