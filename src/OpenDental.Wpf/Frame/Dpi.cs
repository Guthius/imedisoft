using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenDental;

public class Dpi
{
    public enum DPI_AWARENESS_CONTEXT
    {
        DPI_AWARENESS_CONTEXT_DEFAULT = 0,
        DPI_AWARENESS_CONTEXT_UNAWARE = -1,
        DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,
        DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = -5
    }

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