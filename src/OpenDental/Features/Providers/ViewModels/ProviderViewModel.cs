using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.Core.ViewModels;

namespace OpenDental.Features.Providers.ViewModels;

public sealed partial class ProviderViewModel : DialogViewModel
{
    [ObservableProperty] private ObservableCollection<ProviderDto> _providerDtos;
    [ObservableProperty] private ObservableCollection<ProviderSpecialtyDto> _providerSpecialtyDtos;

    [ObservableProperty] private string _abbr = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _middleName = string.Empty;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _suffix = string.Empty;
    [ObservableProperty] private string _preferredName = string.Empty;
    [ObservableProperty] private string _ssn = string.Empty;
    [ObservableProperty] private bool _usingTin;
    [ObservableProperty] private string _nationalProviderId = string.Empty;
    [ObservableProperty] private string _medicaidId = string.Empty;
    [ObservableProperty] private DateTime? _dateOfBirth;
    [ObservableProperty] private string _schedulerNote = string.Empty;
    [ObservableProperty] private long? _feeScheduleId;
    [ObservableProperty] private decimal _hourlyProductionGoal;
    [ObservableProperty] private long? _billingProviderId;
    [ObservableProperty] private string _taxonomyCode = string.Empty;
    [ObservableProperty] private string _color = string.Empty;
    [ObservableProperty] private string _outlineColor = string.Empty;
    [ObservableProperty] private bool _isCdaNet;
    [ObservableProperty] private string _canadianOfficeNumber = string.Empty;
    [ObservableProperty] private bool _isSecondary;
    [ObservableProperty] private bool _isNotPerson;
    [ObservableProperty] private bool _isSignatureOnFile;
    [ObservableProperty] private bool _isHiddenFromReports;
    [ObservableProperty] private bool _isHidden;
    [ObservableProperty] private DateTime? _terminatedOn;
}