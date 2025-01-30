using System.Drawing;
using System.Windows.Forms;

namespace OpenDental;

public class LayoutManagerForms
{
    public static void Add(Control control, Control parent)
    {
        parent.Controls.Add(control);
    }

    public static void MoveLocation(Control control, Point location)
    {
        control.Location = location;
    }
}