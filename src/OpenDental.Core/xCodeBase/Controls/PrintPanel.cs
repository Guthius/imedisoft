using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBase;

public partial class PrintPanel : UserControl
{
    private Bitmap _backImage;
    
    public Graphics BackBuffer;

    public PrintPanel()
    {
        InitializeComponent();
        _backImage = new Bitmap(Width, Height);
        BackBuffer = Graphics.FromImage(_backImage);
    }

    public Point Origin
    {
        get => new((int) BackBuffer.Transform.OffsetX, (int) BackBuffer.Transform.OffsetY);
        set => BackBuffer.TranslateTransform(value.X - BackBuffer.Transform.OffsetX, value.Y - BackBuffer.Transform.OffsetY);
    }

    public void Clear()
    {
        BackBuffer.Clear(BackColor);
    }

    private void PrintPanel_SizeChanged(object sender, EventArgs e)
    {
        panelSurface.Size = Size;
    }

    private void panelSurface_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.DrawImageUnscaled(_backImage, 0, 0);
    }

    private void panelSurface_SizeChanged(object sender, EventArgs e)
    {
        Bitmap newBuffer = null;
        
        if (Width > _backImage.Width)
        {
            newBuffer = Height >= _backImage.Height ? new Bitmap(Width, Height) : new Bitmap(Width, _backImage.Height);
        }
        else if (Height > _backImage.Height)
        {
            newBuffer = new Bitmap(_backImage.Width, Height);
        }

        if (newBuffer == null)
        {
            return;
        }
        
        var graphics = Graphics.FromImage(newBuffer);
            
        graphics.DrawImageUnscaled(_backImage, new Point(0, 0));
        
        BackBuffer.Dispose();
        
        _backImage.Dispose();
        _backImage = newBuffer;
        
        BackBuffer = graphics;
    }
}