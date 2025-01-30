using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OpenDental;

public partial class SheetComboBox : Control
{
    private bool _isHovering;
    private readonly ContextMenu _contextMenu = new();

    public string SelectedOption;
    public string DefaultOption;
    public bool IsToothChart;

    [Category("Layout"), Description("Set true if this is a toothchart combo.")]
    public bool ToothChart
    {
        get => IsToothChart;
        set => IsToothChart = value;
    }

    public string[] ComboOptions { get; }

    public SheetComboBox() : this(";None|S|PS|C|F|NFE|NN")
    {
    }

    public SheetComboBox(string values)
    {
        InitializeComponent();
        
        var strs = values.Split(';');
        if (strs.Length > 1)
        {
            SelectedOption = strs[0];
            ComboOptions = strs[1].Split('|');
        }
        else
        {
            SelectedOption = "";
            ComboOptions = strs[0].Split('|');
        }

        foreach (var option in ComboOptions)
        {
            var escapedOption = option.Replace("&", "&&");
            if (escapedOption == "-")
            {
                escapedOption = "&-";
            }

            _contextMenu.MenuItems.Add(new MenuItem(escapedOption, menuItemContext_Click));
        }
    }

    private void menuItemContext_Click(object sender, EventArgs e)
    {
        if (sender.GetType() != typeof(MenuItem))
        {
            return;
        }

        SelectedOption = ComboOptions[_contextMenu.MenuItems.IndexOf((MenuItem) sender)];
    }

    private void SheetComboBox_MouseDown(object sender, MouseEventArgs e)
    {
        _contextMenu.Show(this, new Point(0, Height));
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        base.OnPaint(pe);

        var colorSurround = Color.FromArgb(245, 234, 200);

        using Brush brushHover = new SolidBrush(colorSurround);
        using var penOutline = new Pen(Color.Black);
        using var penSurround = new Pen(colorSurround);

        float sizeFont;
        if (IsToothChart)
        {
            sizeFont = 10f;
        }
        else if (Height < 11)
        {
            sizeFont = 10f;
        }
        else
        {
            sizeFont = Height - 10;
        }

        using var fontStr = new Font(FontFamily.GenericSansSerif, sizeFont);

        var stringFormat = new StringFormat();

        stringFormat.Alignment = StringAlignment.Center;
        stringFormat.LineAlignment = StringAlignment.Center;

        var g = pe.Graphics;

        g.SmoothingMode = SmoothingMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.FillRectangle(Brushes.White, 0, 0, Width, Height);

        if (_isHovering)
        {
            g.FillRectangle(brushHover, 0, 0, Width - 1, Height - 1);
            g.DrawRectangle(penSurround, 0, 0, Width - 1, Height - 1);
        }

        g.DrawRectangle(penOutline, -1, -1, Width, Height);

        var colorText = Color.Black;
        if (ToothChart)
        {
            if (SelectedOption is "buc" or "ling" or "d" or "m" or "None")
            {
                colorText = Color.LightGray;
            }

            if (SelectedOption == "None")
            {
                SelectedOption = DefaultOption;
            }
        }

        using Brush brush = new SolidBrush(colorText);

        g.DrawString(SelectedOption, fontStr, brush, new Point(Width / 2, Height / 2), stringFormat);

        stringFormat.Dispose();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isHovering)
        {
            return;
        }

        _isHovering = true;

        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        _isHovering = false;

        Invalidate();
    }

    private void SheetComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            _contextMenu.Show(this, new Point(0, Height));
        }

        Invalidate();
    }

    private void SheetComboBox_Enter(object sender, EventArgs e)
    {
        _isHovering = true;

        Invalidate();
    }

    private void SheetComboBox_Leave(object sender, EventArgs e)
    {
        _isHovering = false;

        Invalidate();
    }
}