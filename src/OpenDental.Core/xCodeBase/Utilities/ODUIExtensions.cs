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
        var list = source.ToList();
        
        return list.Skip(Math.Max(0, list.Count - count));
    }

    public static IEnumerable<Control> GetAllControls(Control control)
    {
        var controls = control.Controls.OfType<Control>().ToList();
        
        return controls.SelectMany(GetAllControls).Concat(controls);
    }
}