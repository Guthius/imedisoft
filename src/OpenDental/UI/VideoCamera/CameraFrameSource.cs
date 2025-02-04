using System;
using System.Drawing;

namespace OpenDental.UI;

public class CameraFrameSource : IFrameSource
{
    private Camera _camera;

    public CameraFrameSource(Camera camera)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    public event Action<IFrameSource, byte[], Size> NewFrame;

    public bool IsCapturing { get; private set; }

    public Camera Camera
    {
        get => _camera;
        internal set
        {
            if (_camera == value)
            {
                return;
            }

            var restart = IsCapturing;
            if (IsCapturing)
            {
                StopFrameCapture();
            }

            _camera = value;
            if (restart)
            {
                StartFrameCapture();
            }
        }
    }

    public void StartFrameCapture()
    {
        if (IsCapturing || Camera is null)
        {
            return;
        }
        
        Camera.ImageCaptured += Camera_ImageCaptured;
        Camera.StartCapture();
        
        IsCapturing = true;
    }

    public void StopFrameCapture()
    {
        if (!IsCapturing)
        {
            return;
        }
        
        Camera.StopCapture();
        Camera.ImageCaptured -= Camera_ImageCaptured;
        
        IsCapturing = false;
    }

    public void ReleaseEvents()
    {
        StopFrameCapture();
        
        NewFrame = null;
    }

    private void Camera_ImageCaptured(object sender, CameraEventArgs e)
    {
        if (!IsCapturing)
        {
            return;
        }

        NewFrame?.Invoke(this, e.ByteArray, e.SizeImage);
    }
}