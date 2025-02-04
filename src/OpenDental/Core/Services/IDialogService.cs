using OpenDental.Core.ViewModels;

namespace OpenDental.Core.Services;

public interface IDialogService
{
    TResult Show<TViewModel, TResult>() where TViewModel : DialogViewModel<TResult>;
    bool? Show<TViewModel>() where TViewModel : DialogViewModel<bool?>;
}