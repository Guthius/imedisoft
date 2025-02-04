using System;
using System.Collections.Generic;
using System.Drawing;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace OpenDental.Chart;

public class ToothChartData
{
    private Size _sizeControl = SizeOriginalDrawing;

    public readonly ToothGraphicCollection ListToothGraphics = new();
    public Color ColorBackground = Color.FromArgb(150, 145, 152);
    public Color ColorText = Color.White;
    public Color ColorTextHighlight = Color.Red;
    public Color ColorBackHighlight = Color.White;
    public List<ToothInitial> DrawingSegmentList = [];
    public SizeF SizeOriginalProjection = new(130f, 97.34f);
    public float ScaleMmToPix;
    public bool IsWide;
    public Rectangle RectTarget;
    public static Size SizeOriginalDrawing = new(410, 307);
    public CursorTool CursorTool = CursorTool.Pointer;
    public Color ColorDrawing = Color.Black;
    public List<PointF> PointList = [];
    public float PixelScaleRatio;
    public ToothNumberingNomenclature ToothNumberingNomenclature;
    public bool PerioMode;
    public readonly List<PerioMeasure> ListPerioMeasure = [];
    public Color ColorBleeding;
    public Color ColorSuppuration;
    public Color ColorFurcations;
    public Color ColorFurcationsRed;
    public Color ColorGingivalMargin;
    public Color ColorCal;
    public Color ColorMgj;
    public Color ColorProbing;
    public Color ColorProbingRed;
    public int RedLimitProbing;
    public int RedLimitFurcations;

    public void CleanupDirectX()
    {
        for (var i = 0; i < ListToothGraphics.Count; i++)
        {
            ListToothGraphics[i].CleanupDirectX();
        }
    }

    public void PrepareForDirectX(Device device)
    {
        for (var i = 0; i < ListToothGraphics.Count; i++)
        {
            ListToothGraphics[i].PrepareForDirectX(device);
        }
    }

    public Size SizeControl
    {
        get => _sizeControl;
        set
        {
            _sizeControl = value;

            if (SizeOriginalProjection.Width / _sizeControl.Width < SizeOriginalProjection.Height / _sizeControl.Height)
            {
                IsWide = true;
                ScaleMmToPix = _sizeControl.Height / SizeOriginalProjection.Height;
                RectTarget.Height = _sizeControl.Height;
                RectTarget.Y = 0;
                RectTarget.Width = (int) ((float) SizeOriginalDrawing.Width / SizeOriginalDrawing.Height * RectTarget.Height);
                RectTarget.X = (_sizeControl.Width - RectTarget.Width) / 2;
            }
            else
            {
                IsWide = false;
                ScaleMmToPix = _sizeControl.Width / SizeOriginalProjection.Width;
                RectTarget.Width = _sizeControl.Width;
                RectTarget.X = 0;
                RectTarget.Height = (int) ((float) SizeOriginalDrawing.Height / SizeOriginalDrawing.Width * RectTarget.Width);
                RectTarget.Y = (_sizeControl.Height - RectTarget.Height) / 2;
            }

            PixelScaleRatio = RectTarget.Width / (float) SizeOriginalDrawing.Width;
        }
    }

    public List<string> SelectedTeeth { get; } = [];

    public bool HasSelectedTeethChanged(List<string> listSelectedTeethOld)
    {
        foreach (var tooth in listSelectedTeethOld)
        {
            if (!SelectedTeeth.Contains(tooth))
            {
                return true;
            }
        }

        foreach (var tooth in SelectedTeeth)
        {
            if (!listSelectedTeethOld.Contains(tooth))
            {
                return true;
            }
        }

        return false;
    }

    public RectangleF GetNumberRecMm(string toothId, SizeF labelSizeF)
    {
        float xPos = 0;
        float yPos = 0;

        if (ToothGraphic.IsMaxillary(toothId))
        {
            if (Tooth.IsPrimary(toothId))
            {
                yPos += 4.9f;
            }
            else
            {
                yPos += .8f;
            }
        }
        else
        {
            if (Tooth.IsPrimary(toothId))
            {
                yPos -= labelSizeF.Height + 4.9f;
            }
            else
            {
                yPos -= labelSizeF.Height + .8f;
            }
        }

        xPos += GetTransX(toothId);
        xPos -= labelSizeF.Width / 2f;

        if (ToothGraphic.IsRight(toothId))
        {
            xPos += ListToothGraphics[toothId].ShiftM;
        }
        else
        {
            xPos -= ListToothGraphics[toothId].ShiftM;
        }

        //float toMm=(float)WidthProjection/(float)widthControl;//mm/pix
        var toMm = 1f / ScaleMmToPix;
        var recMm = new RectangleF(xPos - 0f * toMm, yPos - 0f * toMm, labelSizeF.Width - 0f * toMm, labelSizeF.Height);
        return recMm;
    }

    public Rectangle GetNumberRecPix(string toothId, SizeF labelSizeF)
    {
        return ConvertRecToPix(GetNumberRecMm(toothId, labelSizeF));
    }

    public float GetTransXpix(string toothId)
    {
        var toothInt = ToothGraphic.IdToInt(toothId);
        if (toothInt == -1)
        {
            throw new ApplicationException("Invalid tooth number: " + toothId);
        }

        var xmm = ToothGraphic.GetDefaultOrthographicXpos(toothInt);
        return _sizeControl.Width / 2f + xmm * ScaleMmToPix;
    }

    public float GetTransYfacialPix(string toothId)
    {
        if (ToothGraphic.IsMaxillary(toothId))
        {
            return _sizeControl.Height / 2f - 101f * PixelScaleRatio;
        }

        return _sizeControl.Height / 2f + 101f * PixelScaleRatio;
    }

    public float GetTransYocclusalPix(string toothId)
    {
        if (ToothGraphic.IsMaxillary(toothId))
        {
            return _sizeControl.Height / 2f - 48f * PixelScaleRatio;
        }

        return _sizeControl.Height / 2f + 48f * PixelScaleRatio;
    }

    public static float GetTransX(string toothId)
    {
        var toothInt = ToothGraphic.IdToInt(toothId);
        if (toothInt == -1)
        {
            throw new ApplicationException("Invalid tooth number: " + toothId);
        }

        return ToothGraphic.GetDefaultOrthographicXpos(toothInt);
    }

    public static float GetTransYfacial(string toothId)
    {
        const float basic = 29f;

        switch (toothId)
        {
            case "6" or "11":
            case "7" or "10":
                return basic + 1f;

            case "8" or "9":
                return basic + 2f;

            case "22" or "27":
            case "23" or "24" or "25" or "26":
                return -basic - 2f;

            default:
            {
                if (ToothGraphic.IsMaxillary(toothId))
                {
                    return basic;
                }

                break;
            }
        }

        return -basic;
    }

    public static float GetTransYocclusal(string toothId)
    {
        if (ToothGraphic.IsMaxillary(toothId))
        {
            return 13f;
        }

        return -13f;
    }

    public PointF PointPixToMm(PointF pixPoint)
    {
        var toMmRatio = 1f / ScaleMmToPix;

        var mmX = (pixPoint.X - RectTarget.X) * toMmRatio - SizeOriginalProjection.Width / 2f;
        var mmY = SizeOriginalProjection.Height / 2f - (pixPoint.Y - RectTarget.Y) * toMmRatio;

        return new PointF(mmX, mmY);
    }

    public PointF PointDrawingPixToMm(PointF pixPoint)
    {
        var toMmRatio = 1f / ScaleMmToPix;

        var mmX = pixPoint.X * PixelScaleRatio * toMmRatio - SizeOriginalProjection.Width / 2f;
        var mmY = SizeOriginalProjection.Height / 2f - pixPoint.Y * PixelScaleRatio * toMmRatio;

        return new PointF(mmX, mmY);
    }

    private Rectangle ConvertRecToPix(RectangleF recMm)
    {
        var w = (int) (recMm.Width * ScaleMmToPix);
        var h = (int) (recMm.Height * ScaleMmToPix);
        var x = (int) (recMm.X * ScaleMmToPix + _sizeControl.Width / 2f);
        var y = (int) (_sizeControl.Height / 2f - recMm.Y * ScaleMmToPix - h);

        return new Rectangle(x, y, w, h);
    }

    public string GetToothAtPoint(Point point)
    {
        var closestDelta = SizeOriginalProjection.Width * 2;
        var closestTooth = "1";

        float toothPos;
        float delta;

        var xPos = PointPixToMm(point).X;
        var yPos = PointPixToMm(point).Y;

        string permId;
        bool isPriArea;
        bool priShowing;
        bool permShowing;
        bool usePri;
        string toothId;

        if (yPos > 0)
        {
            for (var i = 1; i <= 16; i++)
            {
                permId = i.ToString();
                if (i is >= 4 and <= 13 && !IsPermArea(yPos))
                {
                    isPriArea = true;
                }
                else
                {
                    isPriArea = false;
                }

                priShowing = ListToothGraphics[permId].ShowPrimaryLetter && !ListToothGraphics[Tooth.PermToPri(permId)].HideNumber;

                permShowing = true;
                if (ListToothGraphics[permId].HideNumber)
                {
                    permShowing = false;
                }

                switch (priShowing)
                {
                    case false when !permShowing:
                        continue;

                    case true:
                        usePri = !permShowing || isPriArea;
                        break;

                    default:
                        usePri = false;
                        break;
                }

                toothId = usePri ? Tooth.PermToPri(permId) : permId;
                toothPos = ToothGraphic.GetDefaultOrthographicXpos(i);

                if (ToothGraphic.IsRight(permId))
                {
                    toothPos += (int) ListToothGraphics[toothId].ShiftM;
                }
                else
                {
                    toothPos -= (int) ListToothGraphics[toothId].ShiftM;
                }

                if (xPos > toothPos)
                {
                    delta = xPos - toothPos;
                }
                else
                {
                    delta = toothPos - xPos;
                }

                if (!(delta < closestDelta))
                {
                    continue;
                }

                closestDelta = delta;
                closestTooth = toothId;
            }

            return closestTooth;
        }

        for (var i = 17; i <= 32; i++)
        {
            permId = i.ToString();
            if (i is >= 20 and <= 29 && !IsPermArea(yPos))
            {
                isPriArea = true;
            }
            else
            {
                isPriArea = false;
            }

            priShowing = ListToothGraphics[permId].ShowPrimaryLetter && !ListToothGraphics[Tooth.PermToPri(permId)].HideNumber;

            permShowing = true;
            if (ListToothGraphics[permId].HideNumber)
            {
                permShowing = false;
            }

            switch (priShowing)
            {
                case false when !permShowing:
                    continue;

                case true:
                    usePri = !permShowing || isPriArea;
                    break;

                default:
                    usePri = false;
                    break;
            }

            toothId = usePri ? Tooth.PermToPri(permId) : permId;
            toothPos = ToothGraphic.GetDefaultOrthographicXpos(i);

            if (ToothGraphic.IsRight(permId))
            {
                toothPos += (int) ListToothGraphics[toothId].ShiftM;
            }
            else
            {
                toothPos -= (int) ListToothGraphics[toothId].ShiftM;
            }

            if (xPos > toothPos)
            {
                delta = xPos - toothPos;
            }
            else
            {
                delta = toothPos - xPos;
            }

            if (delta >= closestDelta)
            {
                continue;
            }

            closestDelta = delta;
            closestTooth = toothId;
        }

        return closestTooth;
    }

    public static bool IsPermArea(float yPos)
    {
        return yPos is <= 5.1f and >= -4.4f;
    }

    public void SetSelected(string toothId, bool setValue)
    {
        if (setValue)
        {
            if (!SelectedTeeth.Contains(toothId))
            {
                SelectedTeeth.Add(toothId);
            }
        }
        else
        {
            if (SelectedTeeth.Contains(toothId))
            {
                SelectedTeeth.Remove(toothId);
            }
        }
    }

    public List<string> GetAffectedTeeth(string startingId, string endingId, float yPos)
    {
        var affectedTeeth = new List<string> {endingId};
        var startingOrdinal = Tooth.ToOrdinal(startingId);
        if (Tooth.IsPrimary(startingId))
        {
            startingOrdinal = Tooth.ToOrdinal(Tooth.PriToPerm(startingId));
        }

        var endingOrdinal = Tooth.ToOrdinal(endingId);
        if (Tooth.IsPrimary(endingId))
        {
            endingOrdinal = Tooth.ToOrdinal(Tooth.PriToPerm(endingId));
        }

        if (Math.Abs(startingOrdinal - endingOrdinal) <= 1)
        {
            return affectedTeeth;
        }

        if (Tooth.IsMaxillary(startingId) != Tooth.IsMaxillary(endingId))
        {
            return affectedTeeth;
        }

        var isInPermArea = IsPermArea(yPos);

        string permId;
        string priId;

        if (endingOrdinal < startingOrdinal)
        {
            for (var i = endingOrdinal + 1; i < startingOrdinal; i++)
            {
                permId = Tooth.FromOrdinal(i);
                priId = Tooth.PermToPri(permId);

                if (priId != "" && ListToothGraphics[permId].ShowPrimaryLetter && !isInPermArea && !ListToothGraphics[priId].HideNumber)
                {
                    affectedTeeth.Add(priId);
                }
                else if (!ListToothGraphics[permId].HideNumber)
                {
                    affectedTeeth.Add(permId);
                }
            }
        }
        else
        {
            for (var i = startingOrdinal + 1; i < endingOrdinal; i++)
            {
                permId = Tooth.FromOrdinal(i);
                priId = Tooth.PermToPri(permId);
                if (priId != "" && ListToothGraphics[permId].ShowPrimaryLetter && !isInPermArea && !ListToothGraphics[priId].HideNumber)
                {
                    affectedTeeth.Add(priId);
                }
                else if (!ListToothGraphics[permId].HideNumber)
                {
                    affectedTeeth.Add(permId);
                }
            }
        }

        return affectedTeeth;
    }

    public int GetFurcationValue(int intTooth, PerioSurf surf)
    {
        foreach (var perioMeasure in ListPerioMeasure)
        {
            if (perioMeasure.IntTooth != intTooth)
            {
                continue;
            }

            if (perioMeasure.SequenceType != PerioSequenceType.Furcation)
            {
                continue;
            }

            var meas = surf switch
            {
                PerioSurf.MB => perioMeasure.MBvalue,
                PerioSurf.B => perioMeasure.Bvalue,
                PerioSurf.DB => perioMeasure.DBvalue,
                PerioSurf.ML => perioMeasure.MLvalue,
                PerioSurf.L => perioMeasure.Lvalue,
                PerioSurf.DL => perioMeasure.DLvalue,
                _ => 0
            };

            return meas == -1 ? 0 : meas;
        }

        return 0;
    }

    public PointF GetFurcationPos(int intTooth, PerioSurf surf)
    {
        float ysign;
        if (Tooth.IsMaxillary(intTooth))
        {
            ysign = 1;
        }
        else
        {
            ysign = -1;
        }

        var xshift = GetXShiftPerioSite(intTooth, surf);

        return new PointF(xshift, ysign * 9.5f);
    }

    public LineSimple GetProbingLine(int intTooth, PerioSurf surf, out Color color)
    {
        color = ColorProbing;
        if (!ListToothGraphics[intTooth.ToString()].Visible && !ListToothGraphics[intTooth.ToString()].IsImplant)
        {
            return null;
        }

        var xshift = GetXShiftPerioSite(intTooth, surf);

        var gm = 0;
        var pd = 0;

        foreach (var perioMeasure in ListPerioMeasure)
        {
            if (perioMeasure.IntTooth != intTooth)
            {
                continue;
            }

            switch (perioMeasure.SequenceType)
            {
                case PerioSequenceType.Probing:
                    pd = surf switch
                    {
                        PerioSurf.MB => perioMeasure.MBvalue,
                        PerioSurf.B => perioMeasure.Bvalue,
                        PerioSurf.DB => perioMeasure.DBvalue,
                        PerioSurf.ML => perioMeasure.MLvalue,
                        PerioSurf.L => perioMeasure.Lvalue,
                        PerioSurf.DL => perioMeasure.DLvalue,
                        _ => pd
                    };
                    break;

                case PerioSequenceType.GingMargin:
                    gm = surf switch
                    {
                        PerioSurf.MB => PerioMeasures.AdjustGMVal(perioMeasure.MBvalue),
                        PerioSurf.B => PerioMeasures.AdjustGMVal(perioMeasure.Bvalue),
                        PerioSurf.DB => PerioMeasures.AdjustGMVal(perioMeasure.DBvalue),
                        PerioSurf.ML => PerioMeasures.AdjustGMVal(perioMeasure.MLvalue),
                        PerioSurf.L => PerioMeasures.AdjustGMVal(perioMeasure.Lvalue),
                        PerioSurf.DL => PerioMeasures.AdjustGMVal(perioMeasure.DLvalue),
                        _ => gm
                    };
                    break;
            }
        }

        if (pd is 0 or -1)
        {
            return null;
        }

        if (pd >= RedLimitProbing)
        {
            color = ColorProbingRed;
        }

        return Tooth.IsMaxillary(intTooth)
            ? new LineSimple(xshift, gm, 0, xshift, gm + pd, 0)
            : new LineSimple(xshift, -gm, 0, xshift, -(gm + pd), 0);
    }

    ///<summary>Relative to the center of the tooth. The sign is based on area of the mouth.  The magnitude is based on tooth width.  This will be used for the probing bars and horizontal lines.  Probably also for furcations.</summary>
    private float GetXShiftPerioSite(int intTooth, PerioSurf surf)
    {
        if (surf is PerioSurf.B or PerioSurf.L)
        {
            return 0;
        }

        float xdirect = 1;
        if (Tooth.IsMaxillary(intTooth))
        {
            if (surf is PerioSurf.MB or PerioSurf.ML)
            {
                if (ToothGraphic.IsRight(intTooth.ToString()))
                {
                    //UR quadrant
                    xdirect = 1;
                }
                else
                {
                    //UL
                    xdirect = -1;
                }
            }
            else if (surf is PerioSurf.DB or PerioSurf.DL)
            {
                if (ToothGraphic.IsRight(intTooth.ToString()))
                {
                    //UR quadrant
                    xdirect = -1;
                }
                else
                {
                    //UL
                    xdirect = 1;
                }
            }
        }
        else
        {
            //mand
            if (surf is PerioSurf.MB or PerioSurf.ML)
            {
                if (ToothGraphic.IsRight(intTooth.ToString()))
                {
                    //LR quadrant
                    xdirect = 1;
                }
                else
                {
                    //LL
                    xdirect = -1;
                }
            }
            else if (surf is PerioSurf.DB or PerioSurf.DL)
            {
                if (ToothGraphic.IsRight(intTooth.ToString()))
                {
                    //LR quadrant
                    xdirect = -1;
                }
                else
                {
                    //LL
                    xdirect = 1;
                }
            }
        }

        var toothW = ToothGraphic.GetWidth(intTooth);
        float magnitude;
        switch (intTooth)
        {
            default:
                magnitude = .28f; break;
            case 1:
            case 2:
            case 15:
            case 16:
                magnitude = .32f; break;
            case 17:
            case 32:
            case 18:
            case 31:
                magnitude = .35f; break;
            case 3:
            case 14:
                magnitude = .38f; break;
            case 19:
            case 30:
                magnitude = .37f; break;
        }

        if (ListToothGraphics[intTooth.ToString()].IsImplant)
        {
            return 2f * xdirect;
        }

        return magnitude * toothW * xdirect;
    }

    ///<summary>This gets the entire set of lines for one perio row for one sequence type.  The allowed types are GM, MGJ, and CAL.  Each LineSimple is a series of connected lines.  But the result could have interruptions, so we return a list, each item in the list being continuous.  There may be zero items in the list.  Each line in the list is guaranteed to have at least 2 points in it.</summary>
    public List<LineSimple> GetHorizontalLines(PerioSequenceType sequenceType, bool isMaxillary, bool isBuccal)
    {
        var retVal = new List<LineSimple>();
        var startTooth = 1;
        var stopTooth = 17; //doesn't perform a loop for 17.
        if (!isMaxillary)
        {
            startTooth = 32; //We still go Left to Right, even on mand.
            stopTooth = 16;
        }

        var line = new LineSimple();
        var t = startTooth;
        while (t != stopTooth)
        {
            if (!ListToothGraphics[t.ToString()].Visible && !ListToothGraphics[t.ToString()].IsImplant)
            {
                //stop any existing line.
                if (line.Vertices.Count == 1)
                {
                    //if there is already one point, then clear it, because a line can't have one point.
                    line.Vertices.Clear();
                }

                if (line.Vertices.Count > 1)
                {
                    //if 2 or more points in the line, then add the line to the result.
                    retVal.Add(line);
                    line = new LineSimple(); //and initialize a new line for future points.
                }

                //increment to next tooth
                if (isMaxillary)
                {
                    t++;
                }
                else
                {
                    t--;
                }

                continue;
            }

            var val1 = -1;
            var val2 = -1;
            var val3 = -1;
            var surf1 = PerioSurf.None;
            var surf2 = PerioSurf.None;
            var surf3 = PerioSurf.None;

            foreach (var measure in ListPerioMeasure)
            {
                if (measure.IntTooth != t)
                {
                    continue;
                }

                if (measure.SequenceType != sequenceType)
                {
                    continue;
                }

                PerioMeasure pmGm = null;

                if (sequenceType == PerioSequenceType.MGJ)
                {
                    foreach (var perioMeasure in ListPerioMeasure)
                    {
                        if (perioMeasure.IntTooth != t || perioMeasure.SequenceType != PerioSequenceType.GingMargin)
                        {
                            continue;
                        }

                        pmGm = perioMeasure;
                        break;
                    }
                }

                if (isBuccal)
                {
                    if (ToothGraphic.IsRight(t.ToString()))
                    {
                        val1 = measure.DBvalue;
                        val2 = measure.Bvalue;
                        val3 = measure.MBvalue;

                        if (sequenceType == PerioSequenceType.MGJ && pmGm != null)
                        {
                            if (pmGm.DBvalue != -1)
                            {
                                val1 += PerioMeasures.AdjustGMVal(pmGm.DBvalue);
                            }

                            if (pmGm.Bvalue != -1)
                            {
                                val2 += PerioMeasures.AdjustGMVal(pmGm.Bvalue);
                            }

                            if (pmGm.MBvalue != -1)
                            {
                                val3 += PerioMeasures.AdjustGMVal(pmGm.MBvalue);
                            }
                        }

                        surf1 = PerioSurf.DB;
                        surf2 = PerioSurf.B;
                        surf3 = PerioSurf.MB;
                    }
                    else
                    {
                        val1 = measure.MBvalue;
                        val2 = measure.Bvalue;
                        val3 = measure.DBvalue;

                        if (sequenceType == PerioSequenceType.MGJ && pmGm != null)
                        {
                            if (pmGm.MBvalue != -1)
                            {
                                val1 += PerioMeasures.AdjustGMVal(pmGm.MBvalue);
                            }

                            if (pmGm.Bvalue != -1)
                            {
                                val2 += PerioMeasures.AdjustGMVal(pmGm.Bvalue);
                            }

                            if (pmGm.DBvalue != -1)
                            {
                                val3 += PerioMeasures.AdjustGMVal(pmGm.DBvalue);
                            }
                        }

                        surf1 = PerioSurf.MB;
                        surf2 = PerioSurf.B;
                        surf3 = PerioSurf.DB;
                    }
                }
                else
                {
                    if (ToothGraphic.IsRight(t.ToString()))
                    {
                        val1 = measure.DLvalue;
                        val2 = measure.Lvalue;
                        val3 = measure.MLvalue;

                        if (sequenceType == PerioSequenceType.MGJ && pmGm != null)
                        {
                            if (pmGm.DLvalue != -1)
                            {
                                val1 += PerioMeasures.AdjustGMVal(pmGm.DLvalue);
                            }

                            if (pmGm.Lvalue != -1)
                            {
                                val2 += PerioMeasures.AdjustGMVal(pmGm.Lvalue);
                            }

                            if (pmGm.MLvalue != -1)
                            {
                                val3 += PerioMeasures.AdjustGMVal(pmGm.MLvalue);
                            }
                        }

                        surf1 = PerioSurf.DL;
                        surf2 = PerioSurf.L;
                        surf3 = PerioSurf.ML;
                    }
                    else
                    {
                        val1 = measure.MLvalue;
                        val2 = measure.Lvalue;
                        val3 = measure.DLvalue;

                        if (sequenceType == PerioSequenceType.MGJ && pmGm != null)
                        {
                            if (pmGm.MLvalue != -1)
                            {
                                val1 += PerioMeasures.AdjustGMVal(pmGm.MLvalue);
                            }

                            if (pmGm.Lvalue != -1)
                            {
                                val2 += PerioMeasures.AdjustGMVal(pmGm.Lvalue);
                            }

                            if (pmGm.DLvalue != -1)
                            {
                                val3 += PerioMeasures.AdjustGMVal(pmGm.DLvalue);
                            }
                        }

                        surf1 = PerioSurf.ML;
                        surf2 = PerioSurf.L;
                        surf3 = PerioSurf.DL;
                    }
                }
            }

            Vertex3 vertex;
            if (val1 == -1)
            {
                if (line.Vertices.Count == 1)
                {
                    line.Vertices.Clear();
                }

                if (line.Vertices.Count > 1)
                {
                    retVal.Add(line);

                    line = new LineSimple();
                }
            }
            else
            {
                vertex = new Vertex3
                {
                    Z = 0
                };

                if (isMaxillary)
                {
                    vertex.Y = PerioMeasures.AdjustGMVal(val1);
                }
                else
                {
                    vertex.Y = -PerioMeasures.AdjustGMVal(val1);
                }

                vertex.X = GetXShiftPerioSite(t, surf1) + ToothGraphic.GetDefaultOrthographicXpos(t);

                line.Vertices.Add(vertex);
            }

            if (val2 == -1)
            {
                if (line.Vertices.Count == 1)
                {
                    line.Vertices.Clear();
                }

                if (line.Vertices.Count > 1)
                {
                    retVal.Add(line);
                    line = new LineSimple();
                }
            }
            else
            {
                vertex = new Vertex3
                {
                    Z = 0
                };
                if (isMaxillary)
                {
                    vertex.Y = PerioMeasures.AdjustGMVal(val2);
                }
                else
                {
                    vertex.Y = -PerioMeasures.AdjustGMVal(val2);
                }

                vertex.X = GetXShiftPerioSite(t, surf2) + ToothGraphic.GetDefaultOrthographicXpos(t);
                line.Vertices.Add(vertex);
            }

            if (val3 == -1)
            {
                if (line.Vertices.Count == 1)
                {
                    line.Vertices.Clear();
                }

                if (line.Vertices.Count > 1)
                {
                    retVal.Add(line);
                    line = new LineSimple();
                }
            }
            else
            {
                vertex = new Vertex3
                {
                    Z = 0
                };
                if (isMaxillary)
                {
                    vertex.Y = PerioMeasures.AdjustGMVal(val3);
                }
                else
                {
                    vertex.Y = -PerioMeasures.AdjustGMVal(val3);
                }

                vertex.X = GetXShiftPerioSite(t, surf3) + ToothGraphic.GetDefaultOrthographicXpos(t);
                line.Vertices.Add(vertex);
            }

            //increment to next tooth
            if (isMaxillary)
            {
                t++;
            }
            else
            {
                t--;
            }
        }

        if (line.Vertices.Count > 1)
        {
            retVal.Add(line);
        }

        return retVal;
    }

    public PointF GetBleedingOrSuppuration(int intTooth, PerioSurf surf, bool isBleeding)
    {
        if (!ListToothGraphics[intTooth.ToString()].Visible && !ListToothGraphics[intTooth.ToString()].IsImplant)
        {
            return new PointF(0, 0);
        }

        var xshift = GetXShiftPerioSite(intTooth, surf);
        var yshift = -1.5f; //max
        if (!Tooth.IsMaxillary(intTooth))
        {
            yshift = 1.5f;
        }

        var siteVal = -1;
        foreach (var perioMeasure in ListPerioMeasure)
        {
            if (perioMeasure.IntTooth != intTooth)
            {
                continue;
            }

            if (perioMeasure.SequenceType != PerioSequenceType.BleedSupPlaqCalc)
            {
                continue;
            }

            siteVal = surf switch
            {
                PerioSurf.MB => perioMeasure.MBvalue,
                PerioSurf.B => perioMeasure.Bvalue,
                PerioSurf.DB => perioMeasure.DBvalue,
                PerioSurf.ML => perioMeasure.MLvalue,
                PerioSurf.L => perioMeasure.Lvalue,
                PerioSurf.DL => perioMeasure.DLvalue,
                _ => siteVal
            };

            break;
        }

        if (siteVal is -1 or 0)
        {
            return new PointF(0, 0);
        }

        if (isBleeding)
        {
            if (((BleedingFlags) siteVal & BleedingFlags.Blood) == BleedingFlags.Blood)
            {
                return new PointF(xshift - .3f, yshift);
            }

            return new PointF(0, 0);
        }

        if (((BleedingFlags) siteVal & BleedingFlags.Suppuration) == BleedingFlags.Suppuration)
        {
            return new PointF(xshift + .3f, yshift);
        }

        return new PointF(0, 0);
    }

    public Vector3[] GetDropletVertices()
    {
        const float scale = 1f;
        return
        [
            new Vector3(0, scale * .89f, 0),
            new Vector3(scale * .34f, scale * .049f, 0),
            new Vector3(scale * .21f, scale * -.35f, 0),
            new Vector3(scale * -.21f, scale * -.35f, 0),
            new Vector3(scale * -.34f, scale * .049f, 0)
        ];
    }

    public PointF GetPointMouseScaled(float mouseX, float mouseY, Size sizeChart)
    {
        var pointScaled = new PointF(mouseX, mouseY);
        if (sizeChart.Equals(SizeOriginalDrawing))
        {
            return pointScaled;
        }

        pointScaled.X = (float) Math.Round(SizeOriginalDrawing.Width * mouseX / sizeChart.Width, 1);
        pointScaled.Y = (float) Math.Round(SizeOriginalDrawing.Height * mouseY / sizeChart.Height, 1);

        return pointScaled;
    }
}