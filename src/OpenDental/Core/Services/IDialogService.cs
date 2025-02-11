using OpenDental.Core.ViewModels;

namespace OpenDental.Core.Services;

public interface IDialogService
{
    TResult Show<TResult, TViewModel>(TViewModel viewModel) where TViewModel : DialogViewModel<TResult>;
    bool? Show<TViewModel>(TViewModel viewModel) where TViewModel : DialogViewModel<bool?>;
}