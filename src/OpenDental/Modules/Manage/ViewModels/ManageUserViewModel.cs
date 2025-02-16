using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDentBusiness;

namespace OpenDental.Modules.Manage.ViewModels;

public sealed partial class ManageUserViewModel : ObservableObject
{
    [RelayCommand]
    private void SendClaims()
    {
    }

    [RelayCommand]
    private void OpenInsurancePayments()
    {
    }

    [RelayCommand]
    private void OpenBilling()
    {
    }

    [RelayCommand]
    private void OpenDeposits()
    {
        if (!Security.IsAuthorized(EnumPermType.DepositSlips, DateTime.Today))
        {
            return;
        }

        using var formDeposits = new FormDeposits();

        formDeposits.ShowDialog();
    }

    [RelayCommand]
    private void OpenTasks()
    {
        var formTasks = new FormTasks();

        formTasks.Show();
    }

    [RelayCommand]
    private void OpenBackup()
    {
    }

    [RelayCommand]
    private void OpenAccounting()
    {
    }

    [RelayCommand]
    private void OpenEmails()
    {
    }

    [RelayCommand]
    private void OpenEras()
    {
    }

    [RelayCommand]
    private void ImportInsurancePlans()
    {
    }
}