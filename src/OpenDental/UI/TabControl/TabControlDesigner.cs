using System.Drawing;
using System.Windows.Forms.Design;

namespace OpenDental.UI.Design;

public class TabControlDesigner : ParentControlDesigner
{
    public TabControlDesigner()
    {
        EnableDragDrop(true);
    }

    protected override bool GetHitTest(Point pointScreen)
    {
        var point = Control.PointToClient(pointScreen);
        var tabControl = (TabControl) Control;
        
        foreach (var rectangle in tabControl.ListRectanglesTabs)
        {
            if (rectangle.Contains(point))
            {
                return true;
            }
        }

        return false;
    }
}