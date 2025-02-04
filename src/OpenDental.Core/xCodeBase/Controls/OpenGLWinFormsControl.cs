using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace CodeBase;

public class OpenGLWinFormsControl : Control
{
    protected const bool AutoMakeCurrent = true;

    protected IntPtr DeviceContext = IntPtr.Zero, RenderContext = IntPtr.Zero;
    protected bool RenderEnabled;
    protected bool AutoSwapBuffers;

    public bool AutoFinish = false;

    public event EventHandler TaoRenderScene;

    public event EventHandler TaoSetupContext;

    public bool TaoRenderEnabled
    {
        set => RenderEnabled = value;
    }

    protected bool ContextsReady => DeviceContext != IntPtr.Zero && RenderContext != IntPtr.Zero;

    public OpenGLWinFormsControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

        SetStyle(ControlStyles.OptimizedDoubleBuffer, false);

        DoubleBuffered = false;

        Size = new Size(100, 100);
    }

    [DllImport("gdi32.dll", EntryPoint = "DescribePixelFormat", SetLastError = true), SuppressUnmanagedCodeSecurity]
    public static extern int _DescribePixelFormat(IntPtr hdc, int iPixelFormat, uint nBytes, ref Gdi.PIXELFORMATDESCRIPTOR ppfd);

    protected virtual int CreateContexts(IntPtr pDeviceContext, int preferredPixelFormatNum)
    {
        DeviceContext = pDeviceContext;
        if (DeviceContext == IntPtr.Zero)
        {
            throw new Exception("CreateContexts: Unable to create an OpenGL device context");
        }

        var selectedFormat = 0;

        var pixelFormat = new Gdi.PIXELFORMATDESCRIPTOR();

        pixelFormat.nSize = (short) Marshal.SizeOf(pixelFormat);
        pixelFormat.nVersion = 1;

        try
        {
            if (_DescribePixelFormat(DeviceContext, preferredPixelFormatNum, (uint) pixelFormat.nSize, ref pixelFormat) == 0 || !Gdi.SetPixelFormat(DeviceContext, preferredPixelFormatNum, ref pixelFormat))
            {
                throw new Exception($"Unable to set the requested pixel format ({selectedFormat})");
            }

            selectedFormat = preferredPixelFormatNum;
        }
        catch
        {
            try
            {
                var pfv = ChoosePixelFormatEx(DeviceContext);
                if (!Gdi.SetPixelFormat(DeviceContext, pfv.FormatNumber, ref pfv.Pfd))
                {
                    throw new Exception("");
                }

                pixelFormat = pfv.Pfd;
                selectedFormat = pfv.FormatNumber;
            }
            catch
            {
                pixelFormat = new Gdi.PIXELFORMATDESCRIPTOR();
                pixelFormat.nSize = (short) Marshal.SizeOf(pixelFormat);
                pixelFormat.nVersion = 1;

                selectedFormat = 0;
                do
                {
                    selectedFormat++;
                    if (selectedFormat > _DescribePixelFormat(DeviceContext, selectedFormat, (uint) pixelFormat.nSize, ref pixelFormat))
                    {
                        throw new Exception("There are no acceptable pixel formats for OpenGL graphics.");
                    }
                } while (!Gdi.SetPixelFormat(DeviceContext, selectedFormat, ref pixelFormat));
            }
        }

        AutoSwapBuffers = FormatSupportsDoubleBuffering(pixelFormat);

        FormatSupportsAcceleration(pixelFormat);

        RenderContext = Wgl.wglCreateContext(DeviceContext);

        if (RenderContext == IntPtr.Zero)
        {
            throw new Exception("CreateContexts: Unable to create an OpenGL rendering context");
        }

        MakeCurrentContext();

        return selectedFormat;
    }

    protected virtual void DisposeContext()
    {
        if (RenderContext != IntPtr.Zero)
        {
            Wgl.wglMakeCurrent(DeviceContext, RenderContext);
            Wgl.wglDeleteContext(RenderContext);

            RenderContext = IntPtr.Zero;
        }

        if (DeviceContext == IntPtr.Zero)
        {
            return;
        }

        User.ReleaseDC(Handle, DeviceContext);

        DeviceContext = IntPtr.Zero;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeContext();
        }

        base.Dispose(disposing);
    }

    public void MakeCurrentContext()
    {
        if (!Wgl.wglMakeCurrent(DeviceContext, RenderContext))
        {
            throw new Exception("MakeCurrentContext: Unable to active this control's OpenGL rendering context");
        }
    }

    protected void DrawDesignBackground(Graphics controlGraphics)
    {
        controlGraphics.Clear(Color.White);

        controlGraphics.DrawString("Tao OpenGL WinForms Control", new Font("Arial", 14.0f, FontStyle.Bold), Brushes.Black, 10.0f, 10.0f);

        var infoFont = new Font("Arial", 12.0f);

        controlGraphics.DrawString("This control is currently in design mode.", infoFont, Brushes.Black, 10.0f, 35.0f);
        controlGraphics.DrawString("You must set TaoRenderEnabled to true for OpenGL rendering.", infoFont, Brushes.Black, 10.0f, 55.0f);
    }

    public class PixelFormatValue : IComparable
    {
        public Gdi.PIXELFORMATDESCRIPTOR Pfd;
        public int FormatNumber;

        public int CompareTo(object obj)
        {
            var other = (PixelFormatValue) obj;

            if (other.Pfd.cColorBits > Pfd.cColorBits)
            {
                return 1;
            }

            if (other.Pfd.cColorBits < Pfd.cColorBits)
            {
                return -1;
            }

            if (other.Pfd.cDepthBits > Pfd.cDepthBits)
            {
                return 1;
            }

            if (other.Pfd.cDepthBits < Pfd.cDepthBits)
            {
                return -1;
            }

            if (other.FormatNumber < FormatNumber)
            {
                return 1;
            }

            if (other.FormatNumber > FormatNumber)
            {
                return -1;
            }

            return 0;
        }
    }

    public static PixelFormatValue[] PrioritizePixelFormats(Gdi.PIXELFORMATDESCRIPTOR[] unsortedFormats, bool requireDoubleBuffering, bool requireHardwareAccerleration)
    {
        var sortedFormats = new ArrayList();

        for (var i = 0; i < unsortedFormats.Length; i++)
        {
            var pfd = unsortedFormats[i];

            long bpp = pfd.cColorBits;
            long depth = pfd.cDepthBits;

            var pal = FormatUsesPalette(pfd);
            var hardware = FormatSupportsAcceleration(pfd);
            var opengl = FormatSupportsOpenGl(pfd);
            var window = FormatSupportsWindow(pfd);
            var dbuff = FormatSupportsDoubleBuffering(pfd);

            if (!opengl || !window || bpp < 8 || depth < 8 || pal || requireDoubleBuffering != dbuff || requireHardwareAccerleration != hardware)
            {
                continue;
            }


            sortedFormats.Add(new PixelFormatValue
            {
                Pfd = pfd,
                FormatNumber = i + 1
            });
        }

        sortedFormats.Sort();

        return (PixelFormatValue[]) sortedFormats.ToArray(typeof(PixelFormatValue));
    }

    public static Gdi.PIXELFORMATDESCRIPTOR[] GetPixelFormats(IntPtr hdc)
    {
        var pfd = new Gdi.PIXELFORMATDESCRIPTOR();

        pfd.nSize = (short) Marshal.SizeOf(pfd);
        pfd.nVersion = 1;

        var numFormats = _DescribePixelFormat(hdc, 1, (uint) pfd.nSize, ref pfd);
        var pixelFormats = new Gdi.PIXELFORMATDESCRIPTOR[numFormats];

        for (var i = 0; i < pixelFormats.Length; i++)
        {
            pixelFormats[i] = new Gdi.PIXELFORMATDESCRIPTOR();
            pixelFormats[i].nSize = (short) Marshal.SizeOf(pixelFormats[i]);
            pixelFormats[i].nVersion = 1;
            _DescribePixelFormat(hdc, i + 1, (uint) pixelFormats[i].nSize, ref pixelFormats[i]);
        }

        return pixelFormats;
    }

    public static bool FormatUsesPalette(Gdi.PIXELFORMATDESCRIPTOR pfd)
    {
        return pfd.iPixelType == Gdi.PFD_TYPE_COLORINDEX;
    }

    public static bool FormatSupportsAcceleration(Gdi.PIXELFORMATDESCRIPTOR pfd)
    {
        return (pfd.dwFlags & Gdi.PFD_GENERIC_FORMAT) == 0;
    }

    public static bool FormatSupportsOpenGl(Gdi.PIXELFORMATDESCRIPTOR pfd)
    {
        return (pfd.dwFlags & Gdi.PFD_SUPPORT_OPENGL) != 0;
    }

    public static bool FormatSupportsWindow(Gdi.PIXELFORMATDESCRIPTOR pfd)
    {
        return (pfd.dwFlags & Gdi.PFD_DRAW_TO_WINDOW) != 0;
    }

    public static bool FormatSupportsBitmap(Gdi.PIXELFORMATDESCRIPTOR pfd)
    {
        return (pfd.dwFlags & Gdi.PFD_DRAW_TO_BITMAP) != 0;
    }

    public static bool FormatSupportsDoubleBuffering(Gdi.PIXELFORMATDESCRIPTOR pfd)
    {
        return (pfd.dwFlags & Gdi.PFD_DOUBLEBUFFER) != 0;
    }

    public IntPtr GetHdc()
    {
        return User.GetDC(Handle);
    }

    public static PixelFormatValue ChoosePixelFormatEx(IntPtr hdc)
    {
        var saneformats = GetPixelFormats(hdc);

        var formats = PrioritizePixelFormats(saneformats, false, true);
        if (formats.Length > 0)
        {
            return formats[0];
        }

        formats = PrioritizePixelFormats(saneformats, true, true);
        if (formats.Length > 0)
        {
            return formats[0];
        }

        formats = PrioritizePixelFormats(saneformats, true, false);
        if (formats.Length > 0)
        {
            return formats[0];
        }

        formats = PrioritizePixelFormats(saneformats, false, false);

        return formats.Length > 0 ? formats[0] : new PixelFormatValue();
    }

    public int TaoInitializeContexts(int preferredPixelFormatNum)
    {
        if (Handle == IntPtr.Zero)
        {
            throw new Exception("CreateContexts: The control's window handle has not been created.");
        }

        return TaoInitializeContexts(GetHdc(), preferredPixelFormatNum);
    }

    public int TaoInitializeContexts(IntPtr pDeviceContext, int preferredPixelFormatNum)
    {
        var selectedFormat = 0;

        if (ContextsReady)
        {
            return selectedFormat;
        }

        selectedFormat = CreateContexts(pDeviceContext, preferredPixelFormatNum);

        TaoSetupContext?.Invoke(this, null);

        return selectedFormat;
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Render(e);
    }

    public void Render(PaintEventArgs e)
    {
        if (RenderEnabled)
        {
            if (AutoMakeCurrent)
            {
                if (RenderContext != Wgl.wglGetCurrentContext())
                {
                    MakeCurrentContext();
                }
            }

            TaoRenderScene?.Invoke(this, null);

            if (AutoFinish)
            {
                Gl.glFinish();
            }

            Gl.glGetError();

            if (AutoSwapBuffers)
            {
                Gdi.SwapBuffersFast(DeviceContext);
            }
        }
        else
        {
            DrawDesignBackground(e.Graphics);
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        if (!ContextsReady || !RenderEnabled)
        {
            return;
        }

        Gl.glViewport(0, 0, Width, Height);

        Invalidate();
    }
}