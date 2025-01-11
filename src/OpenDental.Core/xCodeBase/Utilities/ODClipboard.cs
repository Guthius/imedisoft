using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBase;

public class ODClipboard
{
    public static bool Clear()
    {
        try
        {
            Clipboard.Clear();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void SetClipboard(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Clipboard.SetText(text);
    }

    public static string GetText()
    {
        return Clipboard.GetText();
    }
    
    public static Bitmap GetImage()
    {
        if (!Clipboard.ContainsImage())
        {
            return null;
        }

        IDataObject iDataObject;
        try
        {
            iDataObject = Clipboard.GetDataObject();
        }
        catch
        {
            return null;
        }

        if (iDataObject is null)
        {
            return null;
        }

        Bitmap bitmapPaste = null;
        if (iDataObject.GetDataPresent(DataFormats.Bitmap))
        {
            bitmapPaste = (Bitmap) iDataObject.GetData(DataFormats.Bitmap);
        }

        return bitmapPaste;
    }

    public static string[] GetFileDropList()
    {
        return (string[]) Clipboard.GetDataObject()?.GetData(DataFormats.FileDrop);
    }
}