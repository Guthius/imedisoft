using OpenDental.Core.ViewModels;

namespace OpenDental.Core.Services;

internal sealed class DialogService : IDialogService
{
    public TResult Show<TViewModel, TResult>() where TViewModel : DialogViewModel<TResult>
    {
        throw new System.NotImplementedException();
    }

    public bool? Show<TViewModel>() where TViewModel : DialogViewModel<bool?>
    {
        throw new System.NotImplementedException();
    }
}