using System.Drawing;
using System.Windows.Forms.Design;
using System.Windows.Forms.Design.Behavior;

namespace OpenDental.UI.Design;

public class TabPageDesigner : ParentControlDesigner
{
    public TabPageDesigner()
    {
        EnableDragDrop(true);
    }

    protected override bool GetHitTest(Point pointScreen)
    {
        var point = Control.PointToClient(pointScreen);

        var tabPage = Control as TabPage;
        if (tabPage.VerticalScroll.Visible && point.X > tabPage.Width - 18)
        {
            return true;
        }

        return tabPage.HorizontalScroll.Visible && point.Y > tabPage.Height - 18;
    }

    public override SelectionRules SelectionRules => SelectionRules.Locked;

    public override GlyphCollection GetGlyphs(GlyphSelectionType selectionType)
    {
        var glyphCollectionBase = base.GetGlyphs(selectionType);
        var glyphCollection = new GlyphCollection();

        for (var i = 0; i < glyphCollectionBase.Count; i++)
        {
            if (i == 0)
            {
                continue;
            }

            glyphCollection.Add(glyphCollectionBase[i]);
        }

        return glyphCollection;
    }
}