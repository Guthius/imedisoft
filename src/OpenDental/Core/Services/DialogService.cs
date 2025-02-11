using System;
using System.Collections.Generic;
using System.Windows;
using OpenDental.Core.ViewModels;

namespace OpenDental.Core.Services;

internal sealed class DialogService : IDialogService
{
    private static readonly Dictionary<RuntimeTypeHandle, Type> Views = [];
    private static readonly Stack<Window> Windows = [];

    public TResult Show<TResult, TViewModel>(TViewModel viewModel) where TViewModel : DialogViewModel<TResult>
    {
        TResult result = default;

        var window = CreateWindowFor<TViewModel, TResult>();
        if (window is null)
        {
            return result;
        }

        window.DataContext = viewModel;
        if (Windows.Count > 0)
        {
            window.Owner = Windows.Peek();
        }

        viewModel.CloseAction = dialogResult =>
        {
            result = dialogResult;

            window.Close();
        };

        try
        {
            Windows.Push(window);

            window.ShowDialog();
        }
        finally
        {
            Windows.Pop();
        }

        return result;
    }

    public bool? Show<TViewModel>(TViewModel viewModel) where TViewModel : DialogViewModel<bool?>
    {
        return Show<bool?, TViewModel>(viewModel);
    }

    private static Window CreateWindowFor<TViewModel, TResult>() where TViewModel : DialogViewModel<TResult>
    {
        var viewModelType = typeof(TViewModel);
        if (Views.TryGetValue(viewModelType.TypeHandle, out var viewType))
        {
            return (Window) Activator.CreateInstance(viewType);
        }

        var typeName = typeof(TViewModel).FullName;
        if (typeName is null)
        {
            return null;
        }

        typeName = typeName.Replace("ViewModels", "Views");
        typeName = typeName.Replace("ViewModel", "View");

        viewType = Type.GetType(typeName);
        if (viewType is null)
        {
            return null;
        }

        if (!typeof(Window).IsAssignableFrom(viewType))
        {
            throw new InvalidOperationException($"View type {viewType.FullName} is not a Window!");
        }

        Views[viewModelType.TypeHandle] = viewType;

        return (Window) Activator.CreateInstance(viewType);
    }
}