using System;
using System.Windows.Forms;

namespace CodeBase;

public static class ODInvokeExtensions
{
    public static void Invoke(this Control control, Action action)
    {
        control.Invoke(action);
    }

    public static void InvokeIfRequired(this Control control, Action action)
    {
        if (control.InvokeRequired)
        {
            control.Invoke(action);
            return;
        }

        action();
    }

    public static void InvokeIfNotDisposed(this Control control, Action action)
    {
        if (control.Disposing || control.IsDisposed)
        {
            return;
        }

        var invokeSuccessful = false;

        try
        {
            control.Invoke(() =>
            {
                invokeSuccessful = true;
                action();
            });
        }
        catch
        {
            if (invokeSuccessful)
            {
                throw;
            }
        }
    }

    public static void BeginInvoke(this Control control, Action action)
    {
        control.BeginInvoke(action);
    }
}