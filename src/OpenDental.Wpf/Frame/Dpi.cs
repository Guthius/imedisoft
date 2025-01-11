using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenDental
{
/*
Some general notes about how dpi works:
This is a starting point, although they gloss over many details and don't really give any good strategies:
https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8#configuring-your-windows-forms-app-for-high-dpi-support
OD is dpi aware by default, specifically DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2.
Users can turn this off in one of two ways:
1. Right click exe, Properties, Compatibility, Change high DPI settings, Override, System.
2. Add NoDpi.txt file.
When not dpi aware, the following will all use 96 dpi values:
Form.DesktopBounds, Form.SetDesktopBounds, and Screen.WorkingArea.
This means they will appear proportionally smaller than the real dimensions, which generally works fine.
But these must all be tested in both dpi-aware and non-aware because of lots of little issues.
150% on a 4K monitor is a good testing environment for both.

WPF works in DIPs.
If directly setting a Window.Left or Top, you must convert to windows scale.
Full examples are in WindowComboPicker, ToolTip, and WindowImageFloatWindows.
But I recommend this:
        double scale=VisualTreeHelper.GetDpi(this).DpiScaleX;

Do not use SetDesktopBounds to move a Form.
It will erroneously use the upper left of the working area as 0,0.
If a taskbar is docked left, this will shift your Form to the right by that amount on both screens.
At least on Windows 10.
Instead, use Form.Bounds=...
or Form.Location.
*/
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
        private static extern DPI_AWARENESS_CONTEXT SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT dpiContext);

        [DllImport("User32.dll")]
        private static extern bool IsValidDpiAwarenessContext(DPI_AWARENESS_CONTEXT dpiContext);

        ///<summary>This is used to set any Form to be unaware of Dpi.  This will cause Windows to handle dpi the old way, by scaling a bitmap of the form.  This will cause everything to get a little blurry, but it will at least all work correctly.  This buys us time to fix any custom drawing.  Use this just before creating a form (FormExample formExample=new FormExample()).  Then, right after that line, call Dpi.SetAware.</summary>
        public static void SetUnaware()
        {
            try
            {
                SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE);
                //We could instead set to DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED, but testing showed that it essentially looked identical.
            }
            catch
            {
                //requires Win10
            }
        }

        public static bool SupportsHighDpi()
        {
            try
            {
                if (IsValidDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                {
                    return true;
                }
            }
            catch
            {
                return false; 
            }

            return false;
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
}