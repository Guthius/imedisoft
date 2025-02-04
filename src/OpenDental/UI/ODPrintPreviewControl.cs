using System;
using System.Windows.Forms;
using CodeBase;

namespace OpenDental.UI;

public class ODPrintPreviewControl : PrintPreviewControl
{
    private const int WM_VSCROLL = 277; //0x115
    private const int SB_PAGEUP = 2;
    private const int SB_PAGEDOWN = 3;

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (ModifierKeys.HasFlag(Keys.Control))
        {
            Zoom = Math.Min(4.0, Math.Max(0.25, Zoom + (e.Delta < 0 ? -0.25 : 0.25)));
        }
        else
        {
            MiscUtils.SendMessage(Handle, WM_VSCROLL, e.Delta < 0 ? SB_PAGEDOWN : SB_PAGEUP, 0);
        }
    }
}