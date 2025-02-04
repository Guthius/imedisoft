using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenDental;

public class Dpi
{
    [DllImport("User32.dll")]
    private static extern IntPtr MonitorFromPoint(Point point, uint dwFlags);
        
    [DllImport("Shcore.dll")]
    private static extern IntPtr GetDpiForMonitor(IntPtr hmonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

    public enum DpiType
    {
        Effective = 0,
    }

    public static int GetScreenDpi(Screen screen)
    {
        var point = new Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
        var monitor = MonitorFromPoint(point, 2);
            
        uint dpiMonitorX;
        try
        {
            GetDpiForMonitor(monitor, DpiType.Effective, out dpiMonitorX, out _);
        }
        catch
        {
            return 96;
        }

        return (int) dpiMonitorX;
    }
}