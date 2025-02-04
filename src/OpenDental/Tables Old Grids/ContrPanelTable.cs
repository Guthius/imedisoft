using System.ComponentModel;
using System.Windows.Forms;

namespace OpenDental;

public class ContrPanelTable : UserControl
{
    private readonly Container _components = null;

    public ContrPanelTable()
    {
        InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components?.Dispose();
        }

        base.Dispose(disposing);
    }
    
    private void InitializeComponent()
    {
        BackColor = System.Drawing.SystemColors.Window;
        Name = "ContrPanelTable";
    }
}