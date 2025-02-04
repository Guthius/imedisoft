using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace OpenDental.UI.Design;

public class SplitContainerDesigner : ParentControlDesigner
{
    public SplitContainerDesigner()
    {
        EnableDragDrop(true);
    }

    protected override bool GetHitTest(Point pointScreen)
    {
        var point = Control.PointToClient(pointScreen);
        var splitContainer = (SplitContainer) Control;
        if (splitContainer.Orientation == Orientation.Vertical)
        {
            if (point.X < splitContainer.SplitterDistance)
            {
                return false;
            }

            return point.X <= splitContainer.SplitterDistance + splitContainer.SplitterWidth;
        }
        
        if (point.Y < splitContainer.SplitterDistance)
        {
            return false;
        }

        return point.Y <= splitContainer.SplitterDistance + splitContainer.SplitterWidth;
    }
}