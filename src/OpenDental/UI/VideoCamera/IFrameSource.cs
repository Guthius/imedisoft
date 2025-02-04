using System;
using System.Drawing;

namespace OpenDental.UI;

public interface IFrameSource
{
    event Action<IFrameSource, byte[], Size> NewFrame;

    void StartFrameCapture();
    void StopFrameCapture();
}