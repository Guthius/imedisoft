using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using PdfSharp.Drawing;
using Brush = System.Drawing.Brush;
using FontStyle = System.Drawing.FontStyle;
using HorizontalAlignment = System.Windows.Forms.HorizontalAlignment;
using Pen = System.Drawing.Pen;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Windows.Size;

namespace OpenDentBusiness;

public class GraphicsHelper
{
    public static void DrawLine(Graphics g, Pen pen, float x1, float y1, float x2, float y2)
    {
        if (pen.Width is > 1.5f or 1)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
            return;
        }

        var vector = new Vector(x2 - x1, y2 - y1); //connect the dots
        var vector12oclock = new Vector(0, 1);
        var angle = Vector.AngleBetween(vector12oclock, vector);
        var pointCenter = new PointF(x1 + (x2 - x1) / 2f, y1 + (y2 - y1) / 2f);
        var graphicsState = g.Save();
        g.TranslateTransform(pointCenter.X, pointCenter.Y);
        g.RotateTransform((float) angle);
        using (var brush = new SolidBrush(pen.Color))
        {
            g.FillRectangle(brush, -pen.Width / 2, -(float) vector.Length / 2, pen.Width, (float) vector.Length);
        }

        g.Restore(graphicsState);
        //the result of the above is unexpectedly and stunningly an exactly perfect match for GDI+ line drawing.
    }

    public static void DrawRoundedRectangle(Graphics g, Pen pen, RectangleF rectangleF, float radiusCorner)
    {
        var graphicsState = g.Save();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        //top,right
        g.DrawLine(pen, rectangleF.Left + radiusCorner, rectangleF.Top, rectangleF.Right - radiusCorner, rectangleF.Top);
        g.DrawArc(pen, rectangleF.Right - radiusCorner * 2, rectangleF.Top, radiusCorner * 2, radiusCorner * 2, 270, 90);
        //right,bottom
        g.DrawLine(pen, rectangleF.Right, rectangleF.Top + radiusCorner, rectangleF.Right, rectangleF.Bottom - radiusCorner);
        g.DrawArc(pen, rectangleF.Right - radiusCorner * 2, rectangleF.Bottom - radiusCorner * 2, radiusCorner * 2, radiusCorner * 2, 0, 90);
        //bottom,left
        g.DrawLine(pen, rectangleF.Right - radiusCorner, rectangleF.Bottom, rectangleF.Left + radiusCorner, rectangleF.Bottom);
        g.DrawArc(pen, rectangleF.Left, rectangleF.Bottom - radiusCorner * 2, radiusCorner * 2, radiusCorner * 2, 90, 90);
        //left,top
        g.DrawLine(pen, rectangleF.Left, rectangleF.Bottom - radiusCorner, rectangleF.Left, rectangleF.Top + radiusCorner);
        g.DrawArc(pen, rectangleF.Left, rectangleF.Top, radiusCorner * 2, radiusCorner * 2, 180, 90);
        g.Restore(graphicsState);
    }

    public static RectangleF DrawString(Graphics g, string str, Font font, Brush brush, Rectangle rectangle, HorizontalAlignment align)
    {
        if (str.Trim() == "")
        {
            return rectangle; //Nothing to draw.
        }

        //This method seems to only be used for printing.
        //Screen is at 96 dpi (DIPs actually) and printer is at 100 dpi (device units actually)
        //Font follows dpi, so fonts always draw 4% bigger on printers.
        //We will increase the text width and height by 4% to try to match font scaling.
        //But this still doesn't make everything perfect.
        //To be perfect, we would need to adjust positions of all elements by 4%.
        //That would be a massive undertaking, affecting the drawing logic at all levels.
        //Not practical right now, and only a tiny benefit for a huge cost.
        //We can't do g.ScaleTransform() because that would also scale the font size, so it wouldn't fix the wrap.
        //Shrinking the font size by 4% wouldn't work because fonts scale incrementally instead of smoothly.
        //The remaining imperfection will only be noticeable when a tall section of text spills down too close to the next element.
        //We also made some tweaks to MeasureStringH, further down on this page, to give large fields 4% more vertical space.
        var rectangleActual = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width * 1.04f, rectangle.Height * 1.04f);
        var stringFormat = new StringFormat();
        //The overload for DrawString that takes a StringFormat will cause the tabs '\t' to be ignored.
        //In order for the tabs to not get ignored, we have to tell StringFormat how many pixels each tab should be.
        //50.0f is the closest to our Fill Sheet Edit preview.
        stringFormat.SetTabStops(0.0f, new float[] {50.0f});
        stringFormat.Alignment = StringAlignment.Near;
        if (align == HorizontalAlignment.Center)
        {
            stringFormat.Alignment = StringAlignment.Center;
        }

        if (align == HorizontalAlignment.Right)
        {
            stringFormat.Alignment = StringAlignment.Far;
        }

        g.DrawString(str, font, brush, rectangleActual, stringFormat);
        stringFormat?.Dispose();
        return rectangleActual;
    }

    public static RectangleF DrawStringX(XGraphics xg, string str, XFont xfont, XBrush xbrush, RectangleF rectangleF, HorizontalAlignment horizontalAlignment)
    {
        if (str.Trim() == "")
        {
            return rectangleF; //Nothing to draw.
        }

        var bitmap = new Bitmap(100, 100); //only used for measurements.
        bitmap.SetResolution(96, 96);
        var g = Graphics.FromImage(bitmap);
        //There are two coordinate systems here: pixels (used by us) and points (used by PdfSharp)..
        //PDFSharp is not capable of wrapping, so we need to do that manually.
        //Our incoming rectangleF is already in pixels.
        //It does not need to be adjusted by 4% like for the printer.
        //We need to do all our measurement using Graphics, not XGraphics.
        var fontstyle = FontStyle.Regular;
        if (xfont.Style == XFontStyle.Bold)
        {
            fontstyle = FontStyle.Bold;
        }

        var font = new Font(xfont.Name, (float) xfont.Size, fontstyle);
        var sizeLayout = new SizeF(rectangleF.Width, font.Height);
        var stringFormat = new StringFormat();
        stringFormat.SetTabStops(0.0f, new float[] {50.0f}); //helps with measurement further down.
        stringFormat.Trimming = StringTrimming.Word;
        var pixelsPerLine = font.GetHeight();
        var xStringFormat = new XStringFormat(); //or maybe XStringFormats.Default
        xStringFormat.Alignment = XStringAlignment.Near;
        if (horizontalAlignment == HorizontalAlignment.Center)
        {
            xStringFormat.Alignment = XStringAlignment.Center;
        }

        if (horizontalAlignment == HorizontalAlignment.Right)
        {
            xStringFormat.Alignment = XStringAlignment.Far;
        }

        float lineIdx = 0;
        int chars;
        //this loop adds chars each time, which is one line's worth of text.
        for (var i = 0; i < str.Length; i += chars)
        {
            //Example. We have drawn 1 line and we are getting ready to draw the second line.
            //For this example, with text wrap, this text will only be two lines high, so this is also the last line.
            //lineIdx=1. if(rect.Y+(heightRow*2)>rect.Bottom) then no room so kick out.
            //Skips checking the first line.
            if (lineIdx != 0 && rectangleF.Y + pixelsPerLine * (lineIdx + 1) > rectangleF.Bottom)
            {
                //Check if rectangleF is tall enough to show next line.
                break;
            }

            //pixels:
            //TextRenderer.MeasureText(str.Substring(i),font, //no overload for measuring line by line
            //sizeLayout is a rectangle one line high, so we are measuring how much will fit in one line.
            //_lines variable below is thrown away.
            g.MeasureString(str.Substring(i), font, sizeLayout, stringFormat, out chars, out var _lines);
            //Newline characters \r\n, \r, and \n will not be recognized in Unicode PDF and will create rectangles on the screen, so since g.MeasureString has
            //already calculated the next new line that will appear on the screen, we can remove the unneeded newline characters from the current substring.
            var substring = str.Substring(i, chars);
            substring = substring.Replace("\r\n", "");
            substring = substring.Replace("\r", "");
            substring = substring.Replace("\n", "");
            substring = substring.Replace("\t", "    ");
            //use points here:
            double x = PixelsToPoints(rectangleF.X);
            if (horizontalAlignment == HorizontalAlignment.Right)
            {
                x = PixelsToPoints(rectangleF.Right);
            }

            if (horizontalAlignment == HorizontalAlignment.Center)
            {
                x = PixelsToPoints(rectangleF.X + rectangleF.Width / 2f);
            }

            double y = PixelsToPoints(rectangleF.Y + pixelsPerLine * lineIdx);
            xg.DrawString(substring, xfont, xbrush, x, y, xStringFormat);
            lineIdx += 1;
        }

        g.Dispose();
        stringFormat?.Dispose();
        font?.Dispose();
        //xStringFormat?.Dispose();//Does not exist
        return rectangleF;
    }

    public static GraphicsPath GetRoundedPath(RectangleF rectangleF, float radiusCorner)
    {
        //There is a similar copy in ContrApptPanel.
        var graphicsPath = new GraphicsPath();
        graphicsPath.AddLine(rectangleF.Left + radiusCorner, rectangleF.Top, rectangleF.Right - radiusCorner, rectangleF.Top); //top
        graphicsPath.AddArc(rectangleF.Right - radiusCorner * 2, rectangleF.Top, radiusCorner * 2, radiusCorner * 2, 270, 90); //UR
        graphicsPath.AddLine(rectangleF.Right, rectangleF.Top + radiusCorner, rectangleF.Right, rectangleF.Bottom - radiusCorner); //right
        graphicsPath.AddArc(rectangleF.Right - radiusCorner * 2, rectangleF.Bottom - radiusCorner * 2, radiusCorner * 2, radiusCorner * 2, 0, 90); //LR
        graphicsPath.AddLine(rectangleF.Right - radiusCorner, rectangleF.Bottom, rectangleF.Left + radiusCorner, rectangleF.Bottom); //bottom
        graphicsPath.AddArc(rectangleF.Left, rectangleF.Bottom - radiusCorner * 2, radiusCorner * 2, radiusCorner * 2, 90, 90); //LL
        graphicsPath.AddLine(rectangleF.Left, rectangleF.Bottom - radiusCorner, rectangleF.Left, rectangleF.Top + radiusCorner); //left
        graphicsPath.AddArc(rectangleF.Left, rectangleF.Top, radiusCorner * 2, radiusCorner * 2, 180, 90); //UL
        return graphicsPath;
    }

    public static GraphicsPath GetRoundedPathPartial(RectangleF rectangleF, float radiusCorner, bool roundUL = false, bool roundUR = false, bool roundLL = false, bool roundLR = false)
    {
        var graphicsPath = new GraphicsPath();
        float radiusUR = 0;
        if (roundUR)
        {
            radiusUR = radiusCorner;
        }

        float radiusUL = 0;
        if (roundUL)
        {
            radiusUL = radiusCorner;
        }

        float radiusLR = 0;
        if (roundLR)
        {
            radiusLR = radiusCorner;
        }

        float radiusLL = 0;
        if (roundLL)
        {
            radiusLL = radiusCorner;
        }

        graphicsPath.AddLine(rectangleF.Left + radiusUL, rectangleF.Top, rectangleF.Right - radiusUR, rectangleF.Top); //top
        if (roundUR)
        {
            graphicsPath.AddArc(rectangleF.Right - radiusUR * 2, rectangleF.Top, radiusUR * 2, radiusUR * 2, 270, 90); //UR
        }

        graphicsPath.AddLine(rectangleF.Right, rectangleF.Top + radiusUR, rectangleF.Right, rectangleF.Bottom - radiusLR); //right
        if (roundLR)
        {
            graphicsPath.AddArc(rectangleF.Right - radiusLR * 2, rectangleF.Bottom - radiusLR * 2, radiusLR * 2, radiusLR * 2, 0, 90); //LR
        }

        graphicsPath.AddLine(rectangleF.Right - radiusLR, rectangleF.Bottom, rectangleF.Left + radiusLL, rectangleF.Bottom); //bottom
        if (roundLL)
        {
            graphicsPath.AddArc(rectangleF.Left, rectangleF.Bottom - radiusLL * 2, radiusLL * 2, radiusLL * 2, 90, 90); //LL
        }

        graphicsPath.AddLine(rectangleF.Left, rectangleF.Bottom - radiusLL, rectangleF.Left, rectangleF.Top + radiusUL); //left
        if (roundUL)
        {
            graphicsPath.AddArc(rectangleF.Left, rectangleF.Top, radiusUL * 2, radiusUL * 2, 180, 90); //UL
        }

        return graphicsPath;
    }

    public static Path GetRoundedPathWpf(Rect rect, double radiusCorner, bool roundUL = false, bool roundUR = false, bool roundLL = false, bool roundLR = false)
    {
        var path = new Path();
        double radiusUR = 0;
        if (roundUR)
        {
            radiusUR = radiusCorner;
        }

        double radiusUL = 0;
        if (roundUL)
        {
            radiusUL = radiusCorner;
        }

        double radiusLR = 0;
        if (roundLR)
        {
            radiusLR = radiusCorner;
        }

        double radiusLL = 0;
        if (roundLL)
        {
            radiusLL = radiusCorner;
        }

        var pathGeometry = new PathGeometry();
        path.Data = pathGeometry;
        var pathFigure = new PathFigure();
        pathGeometry.Figures.Add(pathFigure);
        pathFigure.StartPoint = new System.Windows.Point(rect.Left + radiusUL, rect.Top); //left top, beginning of straight portion
        var lineSegmentTop = new LineSegment(new System.Windows.Point(rect.Right - radiusUR, rect.Top), isStroked: true);
        pathFigure.Segments.Add(lineSegmentTop);
        if (roundUR)
        {
            var arcSegmentUR = new ArcSegment(new System.Windows.Point(rect.Right, rect.Top + radiusUR), new Size(radiusUR, radiusUR),
                rotationAngle: 0, isLargeArc: false, sweepDirection: SweepDirection.Clockwise, isStroked: true);
            pathFigure.Segments.Add(arcSegmentUR);
            //path.AddArc(rect.Right-radiusUR*2,rect.Top,radiusUR*2,radiusUR*2,270,90);//UR
        }

        var lineSegmentRight = new LineSegment(new System.Windows.Point(rect.Right, rect.Bottom - radiusLR), isStroked: true);
        pathFigure.Segments.Add(lineSegmentRight);
        //path.AddLine(rect.Right,rect.Top+radiusUR,rect.Right,rect.Bottom-radiusLR);//right
        if (roundLR)
        {
            var arcSegmentLR = new ArcSegment(new System.Windows.Point(rect.Right - radiusLR, rect.Bottom), new Size(radiusLR, radiusLR),
                rotationAngle: 0, isLargeArc: false, sweepDirection: SweepDirection.Clockwise, isStroked: true);
            pathFigure.Segments.Add(arcSegmentLR);
            //path.AddArc(rect.Right-radiusLR*2,rect.Bottom-radiusLR*2,radiusLR*2,radiusLR*2,0,90);//LR
        }

        var lineSegmentBottom = new LineSegment(new System.Windows.Point(rect.Left + radiusLL, rect.Bottom), isStroked: true);
        pathFigure.Segments.Add(lineSegmentBottom);
        //path.AddLine(rect.Right-radiusLR,rect.Bottom,rect.Left+radiusLL,rect.Bottom);//bottom
        if (roundLL)
        {
            var arcSegmentLL = new ArcSegment(new System.Windows.Point(rect.Left, rect.Bottom - radiusLL), new Size(radiusLL, radiusLL),
                rotationAngle: 0, isLargeArc: false, sweepDirection: SweepDirection.Clockwise, isStroked: true);
            pathFigure.Segments.Add(arcSegmentLL);
            //path.AddArc(rect.Left,rect.Bottom-radiusLL*2,radiusLL*2,radiusLL*2,90,90);//LL
        }

        var lineSegmentLeft = new LineSegment(new System.Windows.Point(rect.Left, rect.Top + radiusUL), isStroked: true);
        pathFigure.Segments.Add(lineSegmentLeft);
        //path.AddLine(rect.Left,rect.Bottom-radiusLL,rect.Left,rect.Top+radiusUL);//left
        if (roundUL)
        {
            var arcSegmentUL = new ArcSegment(new System.Windows.Point(rect.Left + radiusUL, rect.Top), new Size(radiusUL, radiusUL),
                rotationAngle: 0, isLargeArc: false, sweepDirection: SweepDirection.Clockwise, isStroked: true);
            pathFigure.Segments.Add(arcSegmentUL);
            //path.AddArc(rect.Left,rect.Top,radiusUL*2,radiusUL*2,180,90);//UL
        }

        return path;
    }

    public static int GetTextLineCount(RichTextBox richTextbox)
    {
        if (richTextbox.Text.Length == 0)
        {
            return 0;
        }

        return richTextbox.GetLineFromCharIndex(richTextbox.Text.Length - 1) + 1; //GetLineFromCharIndex() returns a zero-based index.
    }

    public static HeightAndChars MeasureStringH(string text, Font font, int widthAvail, int heightAvail, HorizontalAlignment horizontalAlignment)
    {
        //DrawString (further up in this file) has 4% error which becomes a problem for very tall fields when printing.
        //Layout appears 4% bigger on the screen than when printing, but fonts appear bigger when printing.
        //GDI cannot handle printing to screen at 96 dpi and printer at 100 dpi. See notes over in that method.
        //Only solution is to change this measurement method.
        //In this new algorithm below, we make our local available size smaller so that charactersFitted will be fewer.
        //But we want the height returned to be not adjusted.
        //This can result in a little bit of extra white space below large fields  and the last line can wrap in the middle of the line.
        //This is also used to calculate growth behavior on layout.
        var bitmap = new Bitmap(100, 100); //only used for measurements.
        bitmap.SetResolution(96, 96);
        var g = Graphics.FromImage(bitmap);
        var stringFormat = new StringFormat();
        stringFormat.Trimming = StringTrimming.Word;
        stringFormat.Alignment = StringAlignment.Near;
        if (horizontalAlignment == HorizontalAlignment.Center)
        {
            stringFormat.Alignment = StringAlignment.Center;
        }
        else if (horizontalAlignment == HorizontalAlignment.Right)
        {
            stringFormat.Alignment = StringAlignment.Far;
        }

        //These three lines are just to get charactersFitted
        var sizeFSmall = new SizeF(widthAvail * 0.96f, heightAvail * 0.96f);
        sizeFSmall.Height -= sizeFSmall.Height % font.GetHeight(96);
        //example:29%14=1. Subtracting 1 makes it integer multiple height.
        //g.MeasureString() calculates how many characters fit based upon an exact font height.
        //In order to stay consistent with g.MeasureString(), we must use GetHeight() instead of font.Height because font.Height rounds up and GetHieght() doesn't.
        //We use 96 dpi for GetHeight() in order to match the 96 dpi used above.
        //This is all necessary because g.MeasureString() overestimates the number of characters that fit when the height allows for a non-integer number of lines.
        //e.g. If the sizeFSmall.Height was tall enough for 7.1 lines, g.MeasureString would try to fit 8 when we really would want just 7 since that's how many full
        //lines we could actually fit.
        //figure out how many lines of text will fit on the current page
        g.MeasureString(text, font, sizeFSmall, stringFormat, out var charactersFitted, out var linesFilled); //don't care about linesFilled
        //These two lines are just for height
        var sizeF = new SizeF(widthAvail, heightAvail);
        var sizeFFit = g.MeasureString(text, font, sizeF, stringFormat, out var charactersFitted2, out var linesFilled2);
        bitmap.Dispose();
        g.Dispose();
        var heightAndChars = new HeightAndChars();
        heightAndChars.Chars = charactersFitted;
        heightAndChars.Height = (int) Math.Floor(sizeFFit.Height); //round down because you can't use that fraction of a pixel.
        return heightAndChars;
    }

    public static float PixelsToPoints(float pixels)
    {
        var inches = pixels / 100d; //100 ppi
        var xunit = XUnit.FromInch(inches);
        return (float) xunit.Point;
    }
}

public class RichTextLineInfo
{
    public int FirstCharIndex = 0;
}

public class HeightAndChars
{
    public int Height;
    public int Chars;
}