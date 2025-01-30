using System;
using System.Windows.Forms;

namespace CodeBase;

public class ODMessageBox
{
    public static DialogResult Show(string text)
    {
        return ShowHelper(form => form.MsgBoxShow(text), () => MessageBox.Show(text));
    }

    public static void Show(string text, string caption)
    {
        ShowHelper(form => form.MsgBoxShow(text, caption), () => MessageBox.Show(text, caption));
    }

    public static void Show(IWin32Window owner, string text)
    {
        ShowHelper(form => form.MsgBoxShow(text), () => MessageBox.Show(owner, text));
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
    {
        return ShowHelper(form => form.MsgBoxShow(text, caption, buttons), () => MessageBox.Show(text, caption, buttons));
    }

    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
    {
        return ShowHelper(form => form.MsgBoxShow(text, caption, buttons), () => MessageBox.Show(owner, text, caption, buttons));
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return ShowHelper(form => form.MsgBoxShow(text, caption, buttons, icon), () => MessageBox.Show(text, caption, buttons, icon));
    }

    public static void Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        ShowHelper(form => form.MsgBoxShow(text, caption, buttons, icon), () => MessageBox.Show(owner, text, caption, buttons, icon));
    }

    private static DialogResult ShowHelper(Func<FormProgressBase, DialogResult> funcShowOverProgress, Func<DialogResult> funcShow)
    {
        var formActive = Form.ActiveForm;
        var formPb = ODProgress.FormProgressActive;

        if (formPb == null || (formActive != null && formActive != formPb))
        {
            return funcShow();
        }

        var dialogResult = DialogResult.Abort;
        try
        {
            formPb.InvokeIfRequired(() => dialogResult = funcShowOverProgress(formPb));
        }
        catch (ObjectDisposedException)
        {
            dialogResult = ShowHelper(funcShowOverProgress, funcShow);
        }

        return dialogResult;
    }
}