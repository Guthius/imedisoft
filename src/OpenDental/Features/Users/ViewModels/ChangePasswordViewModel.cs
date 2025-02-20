using CommunityToolkit.Mvvm.Input;
using OpenDental.Core.ViewModels;

namespace OpenDental.Features.Users.ViewModels;

public sealed partial class ChangePasswordViewModel : DialogViewModel<bool?>
{
    [RelayCommand]
    private void Save()
    {
        Close(false);
    }
}