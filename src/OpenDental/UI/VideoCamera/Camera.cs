using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace OpenDental.UI;

public class Camera : IDisposable
{
    protected readonly CameraMethods _cameraMethods;
    protected readonly object _bitmapLock = new object();
    public string Name;
    public object NativeInterface;
    public int Width = 480;
    public int Height = 640;

    public Camera(CameraMethods cameraMethods, object nativeInterface, string name)
    {
        _cameraMethods = cameraMethods;
        Name = name;
        NativeInterface = nativeInterface;
    }

    void IDisposable.Dispose()
    {
        if (NativeInterface != null)
        {
            Marshal.ReleaseComObject(NativeInterface);
            NativeInterface = null;
        }
    }

    public void Dispose()
    {
        StopCapture();
    }

    public event EventHandler<CameraEventArgs> ImageCaptured;

    public override string ToString()
    {
        return Name;
    }

    internal virtual void StartCapture()
    {
        _cameraMethods.StartCamera(this);
    }

    internal void StopCapture()
    {
        _cameraMethods.StopCamera();
    }

    public void FireImageCaptured(byte[] byteArray, Size sizeImage)
    {
        ImageCaptured?.Invoke(this, new CameraEventArgs(byteArray, sizeImage));
    }
}

public class CameraEventArgs : EventArgs
{
    public Size SizeImage;
    public byte[] ByteArray;

    public CameraEventArgs(byte[] byteArray, Size size)
    {
        ByteArray = byteArray;
        SizeImage = size;
    }
}