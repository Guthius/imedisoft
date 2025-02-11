using System.Windows.Forms;

namespace OpenDental.Extensions;

internal static class ListBoxExtensions
{
    public static TItem GetSelected<TItem>(this ListBox listBox)
    {
        if (listBox.SelectedItem is TItem item)
        {
            return item;
        }

        return default;
    }
}