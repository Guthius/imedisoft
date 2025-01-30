using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace OpenDental.UI;

public enum EnumIcons
{
    None,
    Account32,
    Acquire,
    Add,
    Appt32,
    ArrowLeft,
    ArrowRight,
    BreakAptX,
    Chart32,
    Chart32G,
    Chart32W,
    ChartMed32,
    CommLog,
    Complete,
    DeleteX,
    Email,
    Family32,
    ImageSelectorDoc,
    ImageSelectorFile,
    ImageSelectorFolder,
    ImageSelectorFolderWeb,
    ImageSelectorMount,
    ImageSelectorPhoto,
    ImageSelectorXray,
    Imaging32,
    Manage32,
    PatAdd,
    PatDelete,
    Patient,
    PatMoveFam,
    PatSelect,
    PatSetGuarantor,
    Probe,
    Recall,
    Text,
    TreatPlan32,
    TreatPlanMed32,
    Video,
    WebMail
}
    
public class IconLibrary
{
    public static void Draw(Graphics g, EnumIcons icon, Rectangle rectangle)
    {
        if (icon == EnumIcons.None)
        {
            return;
        }
        
        var base64 = IconSelector.GetBase64(icon);
        if (base64 == "")
        {
            return;
        }

        var byteArray = Convert.FromBase64String(base64);
        var memoryStream = new MemoryStream(byteArray);
        var bitmap = (Bitmap) Image.FromStream(memoryStream);
        memoryStream.Dispose();
        if (icon != EnumIcons.Chart32G && icon != EnumIcons.Chart32W)
        {
            g.DrawImage(bitmap, rectangle);
            bitmap.Dispose();
            return;
        }
            
        var graphicsState = g.Save();
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(bitmap, rectangle);
        bitmap.Dispose();
        if (icon == EnumIcons.Chart32G)
        {
            using var pen = new Pen(Color.FromArgb(240, 240, 240));
            g.DrawRectangle(pen, rectangle.X + 0.75f, rectangle.Y + 0.75f, rectangle.Width - 1.25f, rectangle.Height - 1.25f);
        }

        g.Restore(graphicsState);
    }

    public static void DrawDisabled(Graphics g, EnumIcons icon, Rectangle rectangle)
    {
        if (icon == EnumIcons.None)
        {
            return;
        }

        var byteArray = Convert.FromBase64String(IconSelector.GetBase64(icon));
        var memoryStream = new MemoryStream(byteArray);
        var bitmap = (Bitmap) Image.FromStream(memoryStream);
        memoryStream.Dispose();
        var bitmapDisabled = new Bitmap(bitmap.Width, bitmap.Height);
        var gfx = Graphics.FromImage(bitmapDisabled);
        ControlPaint.DrawImageDisabled(gfx, bitmap, 0, 0, ColorOD.Control);
        g.DrawImage(bitmapDisabled, rectangle);
        gfx.Dispose();
        bitmapDisabled.Dispose();
        bitmap.Dispose();
    }
}