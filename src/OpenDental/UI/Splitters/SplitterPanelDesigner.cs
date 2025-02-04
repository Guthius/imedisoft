using System.Drawing;
using System.Windows.Forms.Design;

namespace OpenDental.UI.Design;

public class SplitterPanelDesigner : ParentControlDesigner
{
    public SplitterPanelDesigner()
    {
        EnableDragDrop(true);
    }

    protected override bool GetHitTest(Point pointScreen)
    {
        return false;
    }
}