using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental.Chart;

public partial class ToothChart2D : UserControl
{
    public ToothChartData TcData;

    private bool _mouseIsDown;
    private string _hotTooth;
    private string _hotToothOld;
    private Graphics _g;
    private List<string> _listSelectedTeethOld = [];

    [Category("Action"), Description("Occurs when the mouse goes up ending a drawing segment.")]
    public event ToothChartDrawEventHandler SegmentDrawn;

    [Category("Action"), Description("Occurs when the mouse goes up committing tooth selection.")]
    public event ToothChartSelectionEventHandler ToothSelectionsChanged;

    public ToothChart2D()
    {
        InitializeComponent();
    }

    public void InitializeGraphics()
    {
        _g = CreateGraphics();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (DesignMode)
        {
            e.Graphics.DrawImage(pictBox.Image, new Rectangle(0, 0, Width, Height));
            return;
        }

        if (TcData is null)
        {
            return;
        }

        var bitmap = new Bitmap(Width, Height);
        var graphics = Graphics.FromImage(bitmap);

        graphics.Clear(TcData.ColorBackground);
        graphics.DrawImage(pictBox.Image, TcData.RectTarget);

        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
        {
            if (TcData.ListToothGraphics[t].ToothId == "implant")
            {
                continue;
            }

            DrawFacialView(TcData.ListToothGraphics[t], graphics);
            DrawOcclusalView(TcData.ListToothGraphics[t], graphics);
        }

        DrawWatches(graphics);
        DrawNumbers(graphics);
        DrawDrawingSegments(graphics);

        e.Graphics.DrawImage(bitmap, 0, 0);

        graphics.Dispose();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    private void DrawFacialView(ToothGraphic toothGraphic, Graphics g)
    {
        if (TcData is null)
        {
            return;
        }

        if (!toothGraphic.DrawBigX)
        {
            return;
        }

        var x = TcData.GetTransXpix(toothGraphic.ToothId);
        var y = TcData.GetTransYfacialPix(toothGraphic.ToothId);
        var halfw = 6f * TcData.PixelScaleRatio;
        var halfh = 29f * TcData.PixelScaleRatio;

        g.DrawLine(new Pen(toothGraphic.ColorX, 2f * TcData.PixelScaleRatio), x - halfw, y - halfh, x + halfw, y + halfh);
        g.DrawLine(new Pen(toothGraphic.ColorX, 2f * TcData.PixelScaleRatio), x + halfw, y - halfh, x - halfw, y + halfh);
    }

    private void DrawOcclusalView(ToothGraphic toothGraphic, Graphics g)
    {
        if (toothGraphic.Visible || toothGraphic is {IsCrown: true, IsImplant: true} || toothGraphic.IsPontic)
        {
            DrawToothOcclusal(toothGraphic, g);
        }
    }

    private void DrawToothOcclusal(ToothGraphic toothGraphic, Graphics g)
    {
        if (TcData is null)
        {
            return;
        }

        var outline = new Pen(Color.Gray);

        foreach (var toothGroup in toothGraphic.Groups)
        {
            if (!toothGroup.Visible)
            {
                continue;
            }

            var x = TcData.GetTransXpix(toothGraphic.ToothId);
            var y = TcData.GetTransYocclusalPix(toothGraphic.ToothId);

            var sqB = 4 * TcData.PixelScaleRatio;
            var cirB = 9.5f * TcData.PixelScaleRatio;
            var sqS = 3 * TcData.PixelScaleRatio;
            var cirS = 8f * TcData.PixelScaleRatio;

            GraphicsPath path;

            var brush = new SolidBrush(toothGroup.PaintColor);

            string dir;
            switch (toothGroup.GroupType)
            {
                case ToothGroupType.O:
                    g.FillRectangle(brush, x - sqB, y - sqB, 2f * sqB, 2f * sqB);
                    g.DrawRectangle(outline, x - sqB, y - sqB, 2f * sqB, 2f * sqB);
                    break;

                case ToothGroupType.I:
                    g.FillRectangle(brush, x - sqS, y - sqS, 2f * sqS, 2f * sqS);
                    g.DrawRectangle(outline, x - sqS, y - sqS, 2f * sqS, 2f * sqS);
                    break;

                case ToothGroupType.B:
                    path = GetPath(ToothGraphic.IsMaxillary(toothGraphic.ToothId) ? "U" : "D", x, y, sqB, cirB);
                    g.FillPath(brush, path);
                    g.DrawPath(outline, path);
                    break;

                case ToothGroupType.F:
                    path = GetPath(ToothGraphic.IsMaxillary(toothGraphic.ToothId) ? "U" : "D", x, y, sqS, cirS);
                    g.FillPath(brush, path);
                    g.DrawPath(outline, path);
                    break;

                case ToothGroupType.L:
                    dir = ToothGraphic.IsMaxillary(toothGraphic.ToothId) ? "D" : "U";
                    path = ToothGraphic.IsAnterior(toothGraphic.ToothId) ? GetPath(dir, x, y, sqS, cirS) : GetPath(dir, x, y, sqB, cirB);
                    g.FillPath(brush, path);
                    g.DrawPath(outline, path);
                    break;

                case ToothGroupType.M:
                    dir = ToothGraphic.IsRight(toothGraphic.ToothId) ? "R" : "L";
                    path = ToothGraphic.IsAnterior(toothGraphic.ToothId) ? GetPath(dir, x, y, sqS, cirS) : GetPath(dir, x, y, sqB, cirB);
                    g.FillPath(brush, path);
                    g.DrawPath(outline, path);
                    break;

                case ToothGroupType.D:
                    dir = ToothGraphic.IsRight(toothGraphic.ToothId) ? "L" : "R";
                    path = ToothGraphic.IsAnterior(toothGraphic.ToothId) ? GetPath(dir, x, y, sqS, cirS) : GetPath(dir, x, y, sqB, cirB);
                    g.FillPath(brush, path);
                    g.DrawPath(outline, path);
                    break;
            }
        }
    }

    private static GraphicsPath GetPath(string udlr, float x, float y, float sq, float cir)
    {
        var path = new GraphicsPath();
        var pt = cir * 0.7071f;

        switch (udlr)
        {
            case "U":
                path.AddLine(x - sq, y - sq, x + sq, y - sq);
                path.AddLine(x + sq, y - sq, x + pt, y - pt);
                path.AddArc(x - cir, y - cir, cir * 2f, cir * 2f, 360 - 45, -90);
                path.AddLine(x - pt, y - pt, x - sq, y - sq);
                break;

            case "D":
                path.AddLine(x + sq, y + sq, x - sq, y + sq);
                path.AddLine(x - sq, y + sq, x - pt, y + pt);
                path.AddArc(x - cir, y - cir, cir * 2f, cir * 2f, 90 + 45, -90);
                path.AddLine(x + pt, y + pt, x + sq, y + sq);
                break;

            case "L":
                path.AddLine(x - sq, y + sq, x - sq, y - sq);
                path.AddLine(x - sq, y - sq, x - pt, y - pt);
                path.AddArc(x - cir, y - cir, cir * 2f, cir * 2f, 180 + 45, -90);
                path.AddLine(x - pt, y + pt, x - sq, y + sq);
                break;

            case "R":
                path.AddLine(x + sq, y - sq, x + sq, y + sq);
                path.AddLine(x + sq, y + sq, x + pt, y + pt);
                path.AddArc(x - cir, y - cir, cir * 2f, cir * 2f, 45, -90);
                path.AddLine(x + pt, y - pt, x + sq, y - sq);
                break;
        }

        return path;
    }

    private void DrawWatches(Graphics graphics)
    {
        if (TcData is null)
        {
            return;
        }

        var watchTeeth = new Hashtable(TcData.ListToothGraphics.Count);

        for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
        {
            var toothGraphic = TcData.ListToothGraphics[t];
            if (toothGraphic.ToothId == "implant" || !toothGraphic.Watch || Tooth.IsPrimary(toothGraphic.ToothId))
            {
                continue;
            }

            watchTeeth[toothGraphic.ToothId] = toothGraphic;
        }

        for (var t = 0; t < TcData.ListToothGraphics.Count; t++)
        {
            var toothGraphic = TcData.ListToothGraphics[t];

            if (toothGraphic.ToothId == "implant" || !toothGraphic.Watch || !Tooth.IsPrimary(toothGraphic.ToothId) || !toothGraphic.Visible)
            {
                continue;
            }

            watchTeeth[Tooth.PriToPerm(toothGraphic.ToothId)] = toothGraphic;
        }

        foreach (DictionaryEntry toothGraphic in watchTeeth)
        {
            RenderToothWatch(graphics, (ToothGraphic) toothGraphic.Value);
        }
    }

    private void RenderToothWatch(Graphics g, ToothGraphic toothGraphic)
    {
        if (TcData is null)
        {
            return;
        }

        var brush = new SolidBrush(toothGraphic.ColorWatch);

        if (ToothGraphic.IsRight(toothGraphic.ToothId))
        {
            g.DrawString("W", Font, brush, ToothGraphic.IsMaxillary(toothGraphic.ToothId)
                ? new PointF(TcData.GetTransXpix(toothGraphic.ToothId) + toothGraphic.ShiftM - 6f, 0)
                : new PointF(TcData.GetTransXpix(toothGraphic.ToothId) + toothGraphic.ShiftM - 7f, Height - Font.Size - 8f));
        }
        else
        {
            g.DrawString("W", Font, brush, ToothGraphic.IsMaxillary(toothGraphic.ToothId)
                ? new PointF(TcData.GetTransXpix(toothGraphic.ToothId) - toothGraphic.ShiftM - 6f, 0)
                : new PointF(TcData.GetTransXpix(toothGraphic.ToothId) - toothGraphic.ShiftM - 7f, Height - Font.Size - 8f));
        }

        brush.Dispose();
    }

    private void DrawNumbers(Graphics g)
    {
        if (DesignMode || TcData is null)
        {
            return;
        }

        for (var i = 1; i <= 52; i++)
        {
            var toothId = Tooth.FromOrdinal(i);

            DrawNumber(toothId, TcData.SelectedTeeth.Contains(toothId), true, g);
        }
    }

    private void DrawNumber(string toothId, bool isSelected, bool isFullRedraw, Graphics graphics)
    {
        if (DesignMode)
        {
            return;
        }

        if (TcData is null)
        {
            return;
        }

        if (!Tooth.IsValidDB(toothId))
        {
            return;
        }

        if (TcData.ListToothGraphics[toothId] is null)
        {
            return;
        }

        if (isFullRedraw)
        {
            if (TcData.ListToothGraphics[toothId].HideNumber)
            {
                return;
            }

            if (Tooth.IsPrimary(toothId) && !TcData.ListToothGraphics[Tooth.PriToPerm(toothId)].ShowPrimaryLetter)
            {
                return;
            }
        }

        var displayNum = Tooth.DisplayGraphic(toothId, TcData.ToothNumberingNomenclature);
        var labelWidthMm = graphics.MeasureString(displayNum, Font).Width / TcData.ScaleMmToPix;
        var labelSizeF = new SizeF(labelWidthMm, Font.Height / TcData.ScaleMmToPix);
        var rec = TcData.GetNumberRecPix(toothId, labelSizeF);

        graphics.FillRectangle(isSelected ? new SolidBrush(TcData.ColorBackHighlight) : new SolidBrush(TcData.ColorBackground), rec);

        if (TcData.ListToothGraphics[toothId].HideNumber)
        {
        }
        else if (Tooth.IsPrimary(toothId) && !TcData.ListToothGraphics[Tooth.PriToPerm(toothId)].ShowPrimaryLetter)
        {
        }
        else if (isSelected)
        {
            graphics.DrawString(displayNum, Font, new SolidBrush(TcData.ColorTextHighlight), rec.X, rec.Y);
        }
        else
        {
            graphics.DrawString(displayNum, Font, new SolidBrush(TcData.ColorText), rec.X, rec.Y);
        }
    }

    private void DrawDrawingSegments(Graphics g)
    {
        if (TcData is null)
        {
            return;
        }

        foreach (var toothInitial in TcData.DrawingSegmentList)
        {
            using var pen = new Pen(toothInitial.ColorDraw, 2.2f * TcData.PixelScaleRatio);

            var pointStr = toothInitial.DrawingSegment.Split(';');

            List<PointF> points = [];
            foreach (var str in pointStr)
            {
                var xy = str.Split(',');
                if (!IsValidCoordinate(xy, out var x, out var y))
                {
                    continue;
                }

                x = TcData.RectTarget.X + x * TcData.PixelScaleRatio;
                y = TcData.RectTarget.Y + y * TcData.PixelScaleRatio;

                points.Add(new PointF(x, y));
            }

            if (points.Count < 2)
            {
                continue;
            }

            g.DrawLines(pen, points.ToArray());
        }
    }

    private static bool IsValidCoordinate(string[] coordinate, out float x, out float y)
    {
        x = 0;
        y = 0;

        return coordinate.Length == 2 &&
               float.TryParse(coordinate[0], out x) &&
               float.TryParse(coordinate[1], out y);
    }

    public Bitmap GetBitmap()
    {
        var bitmap = new Bitmap(Width, Height);

        using var graphics = Graphics.FromImage(bitmap);

        var paintEventArgs = new PaintEventArgs(graphics, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

        OnPaint(paintEventArgs);

        return bitmap;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        _mouseIsDown = true;

        if (TcData is null)
        {
            return;
        }

        if (TcData.ListToothGraphics.Count == 0)
        {
            return;
        }

        switch (TcData.CursorTool)
        {
            case CursorTool.Pointer:
            {
                _listSelectedTeethOld = TcData.SelectedTeeth.FindAll(x => x != null);

                var toothClicked = TcData.GetToothAtPoint(e.Location);

                SetSelected(toothClicked, !TcData.SelectedTeeth.Contains(toothClicked));
                break;
            }

            case CursorTool.Pen:
                TcData.PointList.Add(new PointF(e.X, e.Y));
                break;

            case CursorTool.Eraser:
                break;

            case CursorTool.ColorChanger:
            {
                const float radius = 2f;

                var pointMouseScaled = TcData.GetPointMouseScaled(e.X, e.Y, Size);

                foreach (var toothInitial in TcData.DrawingSegmentList)
                {
                    var points = toothInitial.DrawingSegment.Split(';');
                    foreach (var point in points)
                    {
                        var xy = point.Split(',');
                        if (!IsValidCoordinate(xy, out var x, out var y))
                        {
                            continue;
                        }

                        var dist = (float) Math.Sqrt(Math.Pow(Math.Abs(x - pointMouseScaled.X), 2) + Math.Pow(Math.Abs(y - pointMouseScaled.Y), 2));
                        if (dist > radius)
                        {
                            continue;
                        }

                        OnSegmentDrawn(toothInitial.DrawingSegment);

                        toothInitial.ColorDraw = TcData.ColorDrawing;

                        Invalidate();

                        return;
                    }
                }

                break;
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (TcData is null)
        {
            return;
        }

        if (TcData.ListToothGraphics.Count == 0)
        {
            return;
        }

        switch (TcData.CursorTool)
        {
            case CursorTool.Pointer:
            {
                _hotTooth = TcData.GetToothAtPoint(e.Location);

                if (_hotTooth == _hotToothOld)
                {
                    return;
                }

                _hotToothOld = _hotTooth;

                if (_mouseIsDown)
                {
                    SetSelected(_hotTooth, !TcData.SelectedTeeth.Contains(_hotTooth));
                }

                break;
            }

            case CursorTool.Pen when !_mouseIsDown:
                return;

            case CursorTool.Pen:
            {
                TcData.PointList.Add(new PointF(e.X, e.Y));

                _g.SmoothingMode = SmoothingMode.HighQuality;

                var pen = new Pen(TcData.ColorDrawing, 2.2f * TcData.PixelScaleRatio);
                var i = TcData.PointList.Count - 1;

                _g.DrawLine(pen, TcData.PointList[i - 1].X, TcData.PointList[i - 1].Y, TcData.PointList[i].X, TcData.PointList[i].Y);
                break;
            }

            case CursorTool.Eraser when !_mouseIsDown:
                return;

            case CursorTool.Eraser:
            {
                const float radius = 8f;

                var eraserPt = TcData.GetPointMouseScaled(e.X + 8.49f, e.Y + 8.49f, Size);
                for (var i = 0; i < TcData.DrawingSegmentList.Count; i++)
                {
                    var points = TcData.DrawingSegmentList[i].DrawingSegment.Split(';');
                    foreach (var point in points)
                    {
                        var xy = point.Split(',');
                        if (!IsValidCoordinate(xy, out var x, out var y))
                        {
                            continue;
                        }

                        var dist = (float) Math.Sqrt(Math.Pow(Math.Abs(x - eraserPt.X), 2) + Math.Pow(Math.Abs(y - eraserPt.Y), 2));
                        if (dist > radius)
                        {
                            continue;
                        }

                        OnSegmentDrawn(TcData.DrawingSegmentList[i].DrawingSegment);

                        TcData.DrawingSegmentList.RemoveAt(i);

                        Invalidate();
                        return;
                    }
                }

                break;
            }

            case CursorTool.ColorChanger:
                break;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        _mouseIsDown = false;

        if (TcData is null)
        {
            return;
        }

        switch (TcData.CursorTool)
        {
            case CursorTool.Pointer:
            {
                if (TcData.HasSelectedTeethChanged(_listSelectedTeethOld))
                {
                    OnToothSelectionsChanged();
                }

                break;
            }

            case CursorTool.Pen:
            {
                var drawingSegment = "";
                for (var i = 0; i < TcData.PointList.Count; i++)
                {
                    if (i > 0)
                    {
                        drawingSegment += ";";
                    }

                    var pointMouseScaled = TcData.GetPointMouseScaled(TcData.PointList[i].X, TcData.PointList[i].Y, Size);

                    drawingSegment += pointMouseScaled.X + "," + pointMouseScaled.Y;
                }

                OnSegmentDrawn(drawingSegment);

                TcData.PointList = [];
                break;
            }

            case CursorTool.Eraser:
            case CursorTool.ColorChanger:
                break;
        }
    }

    protected void OnSegmentDrawn(string drawingSegment)
    {
        var toothChartDrawEventArgs = new ToothChartDrawEventArgs(drawingSegment);

        SegmentDrawn?.Invoke(this, toothChartDrawEventArgs);
    }

    protected void OnToothSelectionsChanged()
    {
        ToothSelectionsChanged?.Invoke(this);
    }

    private void SetSelected(string toothId, bool setValue)
    {
        if (TcData is null)
        {
            return;
        }

        if (setValue)
        {
            TcData.SelectedTeeth.Add(toothId);

            DrawNumber(toothId, true, false, _g);
        }
        else
        {
            TcData.SelectedTeeth.Remove(toothId);

            DrawNumber(toothId, false, false, _g);
        }

        Invalidate();

        Application.DoEvents();
    }
}