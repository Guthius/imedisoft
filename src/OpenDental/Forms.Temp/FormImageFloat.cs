using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WpfControls.UI;

namespace OpenDental;

public partial class FormImageFloat : FormODBase
{
    public ControlImageDisplay ControlImageDisplay;
    public Func<List<FormImageFloat>> FuncListFloaters;
    public Func<string> FuncDockedTitle;

    private WindowImageFloatWindows _windowImageFloatWindows;
    private bool _isButWindowPressed;
    private bool _isClickLocked;
    private bool _isHotButWindow;
    private Rectangle _rectangleButWindows;
    private Timer _timer;

    public FormImageFloat()
    {
        InitializeComponent();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public event EventHandler<EnumImageFloatWinButton> EventButClicked;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public event EventHandler<int> EventWinPicked;

    public void SetControlImageDisplay(ControlImageDisplay controlImageDisplay)
    {
        ControlImageDisplay = controlImageDisplay;
        ControlImageDisplay.EventGotODFocus -= ControlImageDisplay__EventGotODFocus;
        ControlImageDisplay.EventGotODFocus += ControlImageDisplay__EventGotODFocus;
        ControlImageDisplay.Size = ClientRectangle.Size;
        ControlImageDisplay.Dock = DockStyle.Fill;
        Controls.Add(ControlImageDisplay);
    }

    public void SimulateMouseDown(Point point, Rectangle rectangleFormBounds)
    {
        PanelBorders.Capture = true;
    }

    private void ControlImageDisplay__EventGotODFocus(object sender, EventArgs e)
    {
        var controlImageDisplay = sender as ControlImageDisplay;
        
        var form = controlImageDisplay.FindForm();
        if (form != this)
        {
            return;
        }

        Select();
    }

    private void FormImageFloat_FormClosed(object sender, FormClosedEventArgs e)
    {
        _windowImageFloatWindows?.Close();

        ControlImageDisplay.ClearPDFBrowser();
    }

    private void FormImageFloat_Load(object sender, EventArgs e)
    {
        PanelBorders.MouseDown += PanelBorders_MouseDown;
        PanelBorders.MouseLeave += PanelBorders_MouseLeave;
        PanelBorders.MouseMove += PanelBorders_MouseMove;
        PanelBorders.Paint += PanelBorders_Paint;
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);

        ControlImageDisplay.SetZoomSliderToFit();
    }

    private void PanelBorders_MouseDown(object sender, MouseEventArgs e)
    {
        if (!_rectangleButWindows.Contains(e.Location))
        {
            return;
        }

        if (_isClickLocked)
        {
            return;
        }
        
        new Point(0, 0);
        if (_isButWindowPressed)
        {
            _isButWindowPressed = false;
            return;
        }

        _isButWindowPressed = true;
        
        _windowImageFloatWindows = new WindowImageFloatWindows();
        
        var formImageFloats = FuncListFloaters();
        var listStrings = new List<string>();
        var dockedTitle = FuncDockedTitle();
        
        listStrings.Add(dockedTitle ?? "(no image docked)");

        foreach (var formImageFloat in formImageFloats)
        {
            listStrings.Add(formImageFloat.Text);
        }

        _windowImageFloatWindows.ListFloaterTitles = listStrings;
        _windowImageFloatWindows.idxParent = formImageFloats.IndexOf(this) + 1;
        _windowImageFloatWindows.EventButClicked += (_, enumImageFloatWinButton) => EventButClicked?.Invoke(this, enumImageFloatWinButton);
        _windowImageFloatWindows.EventWinPicked += (_, idx) => EventWinPicked?.Invoke(this, idx);
        _windowImageFloatWindows.Closed += _windowImageFloatWindows_Closed;
        
        var pointL = PointToScreen(new Point(_rectangleButWindows.Left, _rectangleButWindows.Bottom - 9));
        var pointR = PointToScreen(new Point(_rectangleButWindows.Right, _rectangleButWindows.Bottom - 9));
        
        var winPointL = new System.Windows.Point(pointL.X, pointL.Y);
        var winPointR = new System.Windows.Point(pointR.X, pointR.Y);
        
        _windowImageFloatWindows.PointAnchor1 = winPointL;
        _windowImageFloatWindows.PointAnchor2 = winPointR;
        _windowImageFloatWindows.Show();
        
        _isButWindowPressed = true;
        
        PanelBorders.Invalidate();
    }

    private void PanelBorders_MouseLeave(object sender, EventArgs e)
    {
        _isHotButWindow = false;
        
        PanelBorders.Invalidate();
    }

    private void PanelBorders_MouseMove(object sender, MouseEventArgs e)
    {
        _isHotButWindow = _rectangleButWindows.Contains(e.Location);

        PanelBorders.Invalidate();
    }

    private void PanelBorders_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var strWindows = Lan.g(this, "Windows");
        var widthStr = (int) g.MeasureString(strWindows, Font).Width;
        
        _rectangleButWindows = new Rectangle(
            x: RectangleButMin.Left - widthStr - 21,
            y: 0 + 5,
            width: widthStr + 13,
            height: 26);

        var colorFloatBase = Color.FromArgb(65, 94, 154);
        var colorBorder = ColorOD.Mix(colorFloatBase, Color.White, 1, 3);
        var colorBorderText = Color.Black;
        var colorButtonHot = ColorOD.Mix(colorBorder, colorBorderText, 10, 1);
        
        if (IsImageFloatSelected)
        {
            colorBorder = ColorOD.Mix(colorFloatBase, Color.White, 3, 1);
            colorBorderText = Color.White;
            colorButtonHot = ColorOD.Mix(colorBorder, colorBorderText, 4, 1);
        }

        if (_isHotButWindow)
        {
            using var solidBrushHover = new SolidBrush(colorButtonHot);
            
            g.FillRectangle(solidBrushHover, _rectangleButWindows);
        }

        if (_isButWindowPressed)
        {
            g.FillRectangle(Brushes.White, _rectangleButWindows);
            g.DrawRectangle(Pens.Gray, _rectangleButWindows);
            
            colorBorderText = Color.Black;
        }

        using var solidBrushText = new SolidBrush(colorBorderText);
        
        g.DrawString("Windows", Font, solidBrushText, new Point(_rectangleButWindows.X + 6, _rectangleButWindows.Y + 3));
    }

    private void timer_Tick(object sender, EventArgs e)
    {
        _timer.Stop();
        _isClickLocked = false;
    }

    private void _windowImageFloatWindows_Closed(object sender, EventArgs e)
    {
        _isButWindowPressed = false;
        _isClickLocked = true;
        
        _timer = new Timer();
        _timer.Interval = 300;
        _timer.Tick += timer_Tick;
        _timer.Start();
    }
}