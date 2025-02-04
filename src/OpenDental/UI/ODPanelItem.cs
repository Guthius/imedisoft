using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenDental.UI;

public class ODPanelItem
{
    public string Text;
    public ODPanelItemType ItemType;
    public int YPos;
    public int ItemOrder;
    public readonly List<object> Tags = [];
    public int ItemWidth;

    public Point Location = new();

    public static int SortYX(ODPanelItem p1, ODPanelItem p2)
    {
        if (p1.YPos != p2.YPos)
        {
            return p1.YPos.CompareTo(p2.YPos);
        }

        return p1.ItemOrder != p2.ItemOrder ? p1.ItemOrder.CompareTo(p2.ItemOrder) : string.Compare(p1.Text, p2.Text, StringComparison.Ordinal);
    }
}

public enum ODPanelItemType
{
    Button,
    Label
}