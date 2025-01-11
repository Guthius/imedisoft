using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CodeBase;

public class UIHelper
{
    public static void ForceBringToFront(Form form)
    {
        form.TopMost = true;
        Application.DoEvents();
        form.TopMost = false;
    }

    public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int count)
    {
        return source.Skip(Math.Max(0, source.Count() - count));
    }

    public static IEnumerable<Control> GetAllControls(Control control)
    {
        IEnumerable<Control> controls = control.Controls.OfType<Control>();
        return controls.SelectMany(GetAllControls).Concat(controls);
    }
}