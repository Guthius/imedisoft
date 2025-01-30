using System;
using System.Collections.Generic;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ImageDraws
{
    public static List<ImageDraw> RefreshForDoc(long docNum)
    {
        var command = "SELECT * FROM imagedraw WHERE DocNum = " + SOut.Long(docNum);
        return ImageDrawCrud.SelectMany(command);
    }

    public static List<ImageDraw> RefreshForMount(long mountNum)
    {
        var command = "SELECT * FROM imagedraw WHERE MountNum = " + SOut.Long(mountNum);
        return ImageDrawCrud.SelectMany(command);
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
        var command = "DELETE FROM imagedraw WHERE DocNum=" + SOut.Long(docNum);
        Db.NonQ(command);
    }

    public static void DeleteByDocNumAndVendor(long docNum, EnumImageAnnotVendor enumImageAnnotVendor)
    {
        var command = "DELETE FROM imagedraw WHERE DocNum=" + SOut.Long(docNum) + " AND ImageAnnotVendor=" + SOut.Enum(enumImageAnnotVendor);
        Db.NonQ(command);
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
        var scale = 1f;
        if (bitmapWidth == 0 || bitmapHeight == 0) return scale;
        if (IsBitmapWiderThanMountItem(bitmapWidth, bitmapHeight, mountItemWidth, mountItemHeight))
            //The bitmap is wider in shape than the mount item. Scale points based on the width, as it is the limiting dimension.
            scale = (float) mountItemWidth / bitmapWidth;
        else
            //The bitmap is taller in shape than the mount item. Scale points based on the height, as it is the limiting dimension.
            scale = (float) mountItemHeight / bitmapHeight;
        return scale;
    }

    public static Point CalcBitmapPaddingToFitMountItem(int bitmapWidth, int bitmapHeight, int mountItemWidth, int mountItemHeight, float scale)
    {
        var point = new Point();
        if (IsBitmapWiderThanMountItem(bitmapWidth, bitmapHeight, mountItemWidth, mountItemHeight))
            //The bitmap is wider in shape than the mount item. Move down to be centered within mount item.
            point.Y = (int) (mountItemHeight - bitmapHeight * scale) / 2;
        else
            //The bitmap is taller in shape than the mount item. Move right to be centered within mount item..
            point.X = (int) (mountItemWidth - bitmapWidth * scale) / 2;
        return point;
    }

    private static bool IsBitmapWiderThanMountItem(int bitmapWidth, int bitmapHeight, int mountItemWidth, int mountItemHeight)
    {
        //Scale points when bitmap aspect ratio doesn't match mount item's aspect ratio. 
        var ratioWtoHMountItem = (float) mountItemWidth / mountItemHeight;
        var ratioWtoHBitmap = (float) bitmapWidth / bitmapHeight;
        if (ratioWtoHBitmap > ratioWtoHMountItem)
            //The bitmap is wider in shape than the mount item.
            return true;
        //The bitmap is taller in shape than the mount item.
        return false;
    }

    public static List<PointF> ScalePointsToMountItem(List<PointF> listPointFs, float scale)
    {
        var listPointFsScaled = new List<PointF>();
        for (var i = 0; i < listPointFs.Count; i++)
        {
            var pointFScaled = new PointF();
            pointFScaled.X = listPointFs[i].X * scale;
            pointFScaled.Y = listPointFs[i].Y * scale;
            listPointFsScaled.Add(pointFScaled);
        }

        return listPointFsScaled;
    }

    public static List<PointF> TranslatePointsToMountItem(List<PointF> listPointFs, PointF pointFMountItem)
    {
        var listPointFsTranslated = new List<PointF>();
        for (var i = 0; i < listPointFs.Count; i++)
        {
            var pointF = new PointF();
            pointF.X = listPointFs[i].X + pointFMountItem.X;
            pointF.Y = listPointFs[i].Y + pointFMountItem.Y;
            listPointFsTranslated.Add(pointF);
        }

        return listPointFsTranslated;
    }
}