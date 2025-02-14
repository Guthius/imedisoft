using System;
using System.Collections.Generic;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ImageDraws
{
    public static List<ImageDraw> RefreshForDoc(long docNum)
    {
        return ImageDrawCrud.SelectMany("SELECT * FROM imagedraw WHERE DocNum = " + docNum);
    }

    public static List<ImageDraw> RefreshForMount(long mountNum)
    {
        return ImageDrawCrud.SelectMany("SELECT * FROM imagedraw WHERE MountNum = " + mountNum);
    }

    public static void Insert(ImageDraw imageDraw)
    {
        ImageDrawCrud.Insert(imageDraw);
    }

    public static void Update(ImageDraw imageDraw)
    {
        ImageDrawCrud.Update(imageDraw);
    }

    public static void Delete(long imageDrawNum)
    {
        ImageDrawCrud.Delete(imageDrawNum);
    }

    public static void DeleteByDocNum(long docNum)
    {
        Db.NonQ("DELETE FROM imagedraw WHERE DocNum = " + docNum);
    }

    public static void DeleteByDocNumAndVendor(long docNum, EnumImageAnnotVendor enumImageAnnotVendor)
    {
        Db.NonQ("DELETE FROM imagedraw WHERE DocNum = " + docNum + " AND ImageAnnotVendor = " + SOut.Enum(enumImageAnnotVendor));
    }

    public static float CalcLengthLine(List<PointF> listPointFs)
    {
        float lengthTotal = 0;

        for (var p = 1; p < listPointFs.Count; p++)
        {
            var x1 = listPointFs[p - 1].X;
            var y1 = listPointFs[p - 1].Y;
            var x2 = listPointFs[p].X;
            var y2 = listPointFs[p].Y;
            var lengthLine = (float) Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            lengthTotal += lengthLine;
        }

        return lengthTotal;
    }

    public static float CalcBitmapScaleToFitMountItem(int bitmapWidth, int bitmapHeight, int mountItemWidth, int mountItemHeight)
    {
        if (bitmapWidth == 0 || bitmapHeight == 0)
        {
            return 1f;
        }

        float scale;
        if (IsBitmapWiderThanMountItem(bitmapWidth, bitmapHeight, mountItemWidth, mountItemHeight))
        {
            scale = (float) mountItemWidth / bitmapWidth;
        }
        else
        {
            scale = (float) mountItemHeight / bitmapHeight;
        }

        return scale;
    }

    public static Point CalcBitmapPaddingToFitMountItem(int bitmapWidth, int bitmapHeight, int mountItemWidth, int mountItemHeight, float scale)
    {
        var point = new Point();

        if (IsBitmapWiderThanMountItem(bitmapWidth, bitmapHeight, mountItemWidth, mountItemHeight))
        {
            point.Y = (int) (mountItemHeight - bitmapHeight * scale) / 2;
        }
        else
        {
            point.X = (int) (mountItemWidth - bitmapWidth * scale) / 2;
        }

        return point;
    }

    private static bool IsBitmapWiderThanMountItem(int bitmapWidth, int bitmapHeight, int mountItemWidth, int mountItemHeight)
    {
        var ratioWtoHMountItem = (float) mountItemWidth / mountItemHeight;
        var ratioWtoHBitmap = (float) bitmapWidth / bitmapHeight;

        return ratioWtoHBitmap > ratioWtoHMountItem;
    }

    public static List<PointF> ScalePointsToMountItem(List<PointF> points, float scale)
    {
        var scaledPoints = new List<PointF>();

        foreach (var point in points)
        {
            scaledPoints.Add(new PointF
            {
                X = point.X * scale,
                Y = point.Y * scale
            });
        }

        return scaledPoints;
    }

    public static List<PointF> TranslatePointsToMountItem(List<PointF> points, PointF origin)
    {
        var translatedPoints = new List<PointF>();

        foreach (var point in points)
        {
            translatedPoints.Add(new PointF
            {
                X = point.X + origin.X,
                Y = point.Y + origin.Y
            });
        }

        return translatedPoints;
    }
}