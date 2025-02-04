using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenDental.Core.ViewModels;

public abstract class DialogViewModel<TResult> : ObservableObject
{
    public Action<TResult> CloseAction { get; set; }

    protected void Close(TResult result)
    {
        CloseAction?.Invoke(result);
    }
}

public abstract class DialogViewModel : DialogViewModel<bool?>;