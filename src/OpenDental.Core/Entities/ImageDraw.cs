using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using DataConnectionBase;
using OpenDentBusiness;
using OpenDentBusiness.Bridges;
using OpenDentBusiness.Pearl;
using PdfSharp.Drawing;
using Color = System.Drawing.Color;

namespace Imedisoft.Core.Entities;

public class ImageDraw : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ImageDrawNum;

    ///<summary>FK to document.DocNum</summary>
    public long DocNum;

    ///<summary>FK to mount.MountNum</summary>
    public long MountNum;

    ///<summary>For text, this is the foreground color. For lines, this is the color, and ColorBack is not used. For polygons, this is the fill color. No transparency component.</summary>
    public Color ColorDraw;

    ///<summary>Background color for text. Can be Transparent (0,255,255,255)=16777215.</summary>
    public Color ColorBack;

    ///<summary>Point data for a drawing segment.  The format would look similar to this: 45.2,68.1;48,70;49,72;0,0;55,88;etc.  It's simply a sequence of points, separated by semicolons.  Only positive floats are used, rounded to one decimal place.  0,0 is the upper left of the image or mount.  Cropping is ignored.  If the pen is picked up, it becomes a new segment, so a new row in the database.  Or, if this is DrawType.ScaleValue, then this field stores scale, decimal places, and units, separated by spaces.  Example: "123.4 0 mm". The first two are required; units is optional.</summary>
    public string DrawingSegment;

    ///<summary>The location of the text in pixels is incorporated into this string.  Example: 25,123;This shows.  Carriage returns etc are not supported.  ColorDraw and FontSize are also used.  Unlike tooth initial, this does not support floats.</summary>
    public string DrawText;

    ///<summary>This could vary significantly based on the size of the image.  It's always relative to orginal image or mount pixels. Always 0 for Pearl.</summary>
    public float FontSize;

    ///<summary>Enum:ImageDrawType</summary>
    public ImageDrawType DrawType;

    ///<summary>Enum:EnumImageAnnotVendor 0: Open Dental drawings and text, 1:Pearl AI annotations.</summary>
    public EnumImageAnnotVendor ImageAnnotVendor;

    ///<summary>Extra space for any text. Currently only used for Pearl annotation categories, relationship properties, and relationship values, which are all stored as a single chunk of user readable text drawn straight to screen when hovering.</summary>
    public string Details;

    /// <summary>Enum:Pearl.EnumCategoryOD This is how we hide and show layers for Pearl objects in the Imaging module.</summary>
    public EnumCategoryOD PearlLayer;

    /// <summary>Enum:EnumCategoryBetterDiag This is how we hide and show layers for BetterDiagnostics objects in the Imaging module.</summary>
    public EnumCategoryBetterDiag BetterDiagLayer;

    public ImageDraw Copy()
    {
        return (ImageDraw) MemberwiseClone();
    }

    public float GetFontSize()
    {
        var fontSize = FontSize;
        var programName = Programs.GetActiveImagingAIProgram();
        if (programName == ProgramName.Pearl)
        {
            var strFontHeight = ProgramProperties.GetPropVal(ProgramName.Pearl, Pearl.PEARL_FONT_SIZE_PROPERTY);
            try
            {
                fontSize = float.Parse(strFontHeight);
            }
            catch
            {
            }
        }
        else if (programName == ProgramName.BetterDiagnostics)
        {
            var strFontSize = ProgramProperties.GetPropVal(ProgramName.BetterDiagnostics, "Text font size");
            try
            {
                fontSize = float.Parse(strFontSize);
            }
            catch
            {
                fontSize = 18; //Default to 18 if property is invalid.
            }
        }

        return fontSize switch
        {
            < 1 => 1,
            > 100 => 100,
            _ => fontSize
        };
    }

    public string GetTextString()
    {
        if (!DrawText.Contains(";"))
        {
            return "";
        }

        var strArray = DrawText.Split(';');
        return strArray[1];
    }

    public Point GetTextPoint()
    {
        if (!DrawText.Contains(";"))
        {
            return new Point();
        }

        var strArray = DrawText.Split(';');
        if (!Regex.IsMatch(strArray[0], @"^\d+\.?\d*,\d+\.?\d*$"))
        {
            //"#.#,#.#"
            return new Point();
        }

        var xy = strArray[0].Split(',');
        var x = float.Parse(xy[0]);
        var y = float.Parse(xy[1]);
        return new Point((int) x, (int) y);
    }
    
    public XPoint GetTextPointPDF()
    {
        var point = GetTextPoint();
        return new XPoint(GraphicsHelper.PixelsToPoints(point.X), GraphicsHelper.PixelsToPoints(point.Y));
    }

    public void SetLocAndText(Point point, string text)
    {
        DrawText = point.X + "," + point.Y + ";" + text;
    }

    public void SetLoc(Point point)
    {
        var str = GetTextString();
        DrawText = point.X + "," + point.Y + ";" + str;
    }

    public void SetDrawingSegment(List<PointF> listPointFs)
    {
        DrawingSegment = "";
        for (var i = 0; i < listPointFs.Count; i++)
        {
            if (i > 0)
            {
                DrawingSegment += ";";
            }

            DrawingSegment += listPointFs[i].X.ToString("0.#") //tenth, if present
                              + ","
                              + listPointFs[i].Y.ToString("0.#");
        }
    }

    public List<PointF> GetPoints()
    {
        var stringArray = DrawingSegment.Split(';');
        var listPointFs = new List<PointF>();
        for (var i = 0; i < stringArray.Length; i++)
        {
            var stringArray2 = stringArray[i].Split(',');
            if (stringArray2.Length != 2)
            {
                return [];
            }

            float x = 0;
            try
            {
                x = Convert.ToSingle(stringArray2[0]);
            }
            catch
            {
            }

            float y = 0;
            try
            {
                y = Convert.ToSingle(stringArray2[1]);
            }
            catch
            {
            }

            var pointF = new PointF(x, y);
            listPointFs.Add(pointF);
        }

        return listPointFs;
    }

    /// <summary>Gets the points from DrawingSegment and translates them from pixel coordinates to point coordinates for PDFs.</summary>
    public List<XPoint> GetPointsForPDF()
    {
        var listPoints = GetPoints();
        var listXPoints = new List<XPoint>();
        for (var i = 0; i < listPoints.Count; i++)
        {
            var xpoint = new XPoint(GraphicsHelper.PixelsToPoints(listPoints[i].X), GraphicsHelper.PixelsToPoints(listPoints[i].Y));
            listXPoints.Add(xpoint);
        }

        return listXPoints;
    }

    public void SetScale(float scale, int decimals, string units)
    {
        DrawingSegment = scale + " " + decimals;
        if (!string.IsNullOrEmpty(units))
        {
            DrawingSegment += " " + units;
        }
    }

    public float GetScale()
    {
        if (DrawingSegment is null)
        {
            return 0;
        }

        var stringArray = DrawingSegment.Split(' ');
        if (stringArray.Length > 0)
        {
            return SIn.Float(stringArray[0]);
        }

        return 0;
    }

    public int GetDecimals()
    {
        if (DrawingSegment is null)
        {
            return 0;
        }

        var stringArray = DrawingSegment.Split(' ');
        if (stringArray.Length > 1)
        {
            return SIn.Int(stringArray[1]);
        }

        return 0;
    }

    public string GetScaleUnits()
    {
        if (DrawingSegment is null)
        {
            return "";
        }

        var stringArray = DrawingSegment.Split(' ');
        if (stringArray.Length == 3)
        {
            return stringArray[2];
        }

        return "";
    }
}

public enum ImageDrawType
{
    ///<summary>0 - Location and string, combined</summary>
    Text,

    ///<summary>1 - A series of straight lines, stored the same as a pen drawing.</summary>
    Line,

    ///<summary>2 - One continuous segment of a drawing.</summary>
    Pen,

    ///<summary>3 - Stores a float, decimals, and units in the drawing segement. Only one of this type is allowed per image or mount.</summary>
    ScaleValue,

    ///<summary>4 - A series of connected points forming the outline of a closed polygon. Stored same as pen drawing. Polygons only have a fill color, not any outline color.</summary>
    Polygon
}

public enum EnumImageAnnotVendor
{
    ///<summary>0 - Open Dental drawings and text.</summary>
    OpenDental,

    ///<summary>1 - Pearl AI annotations.</summary>
    Pearl,

    ///<summary>2 - Better Diagnostics AI annotations.</summary>
    BetterDiagnostics
}

///<summary>Better Diagnostics enum for AI annotation categories. This is used to set the ImageDraw's BetterDiagLayer when processing results.</summary>
public enum EnumCategoryBetterDiag
{
    ///<summary>0 - None.</summary>
    None = 0,

    ///<summary>1 - Dentin.</summary>
    Dentin,

    ///<summary>2 - Enamel.</summary>
    Enamel,

    ///<summary>3 - Pulp.</summary>
    Pulp,

    ///<summary>4 - Restoration.</summary>
    Restoration,

    ///<summary>5 - Crown.</summary>
    Crown,

    ///<summary>6 - Cavity.</summary>
    Cavity,

    ///<summary>7 - Bone Loss.</summary>
    BoneLoss,

    ///<summary>8 - Infection.</summary>
    Infection,

    ///<summary>9 - Bone Level.</summary>
    BoneLevel,

    ///<summary>10 - Iac. Inferior alveolar canal.</summary>
    Iac,

    ///<summary>11 - Nasal floor.</summary>
    NasalFloor,

    ///<summary>12 - Normal Tmj.</summary>
    NormalTmj,

    ///<summary>13 - Sinus.</summary>
    Sinus
}