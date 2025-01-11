using System;
using System.Runtime.InteropServices;

namespace CodeBase;

public static class WindowsApiWrapper
{
    public const int EC_LEFTMARGIN = 0x0001;

    public enum EM_Rich
    {
        EM_LINESCROLL = 0x00B6,
        EM_SETMARGINS = 0x00D3,
        EM_POSFROMCHAR = 0x00D6
    }

    public enum WinMessagesOther
    {
        WM_PAINT = 0x000F,
        WM_CHAR = 0x0102,
    }
    
    [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);

    [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wparam, int lparam);
}