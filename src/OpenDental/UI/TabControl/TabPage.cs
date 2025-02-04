using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenDental.UI.Design;

namespace OpenDental.UI;

[Designer(typeof(TabPageDesigner))]
public class TabPage : Panel
{
    public Rectangle rectangleTab;

    private string _text;
    
    public TabPage()
    {
        DoubleBuffered = true;
        Text = "tab";
    }

    public TabPage(string text)
    {
        DoubleBuffered = true;
        Text = text;
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        if (DesignMode)
        {
            base.OnLayout(levent);
        }
    }

    public override string ToString()
    {
        return "UI.TabPage2: " + Name;
    }

    [Category("OD")]
    [Description("Warning. This is not supported.  Instead, put an autoscroll panel inside the tab page.")]
    [DefaultValue(false)]
    public new bool AutoScroll
    {
        get => base.AutoScroll;
        set => base.AutoScroll = value;
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public Color ColorTab { get; set; } = Color.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new DockStyle Dock { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Point Location
    {
        get => base.Location;
        set => value = base.Location;
    }
    
    [Category("Appearance")]
    [DefaultValue("")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    public new string Text
    {
        get => _text;
        set
        {
            _text = value;

            if (Parent is not TabControl tabControl)
            {
                return;
            }

            tabControl.LayoutTabs();
            tabControl.Invalidate();
        }
    }
}