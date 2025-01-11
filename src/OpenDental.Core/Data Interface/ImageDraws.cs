using System;
using System.Collections.Generic;
using System.Drawing;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ImageDraws
{
    #region Methods - Get

    ///<summary>All drawings for a single document.</summary>
    public static List<ImageDraw> RefreshForDoc(long docNum)
    {
        var command = "SELECT * FROM imagedraw WHERE DocNum = " + SOut.Long(docNum);
        return ImageDrawCrud.SelectMany(command);
    }

    ///<summary>All drawings for a single mount.</summary>
    public static List<ImageDraw> RefreshForMount(long mountNum)
    {
        var command = "SELECT * FROM imagedraw WHERE MountNum = " + SOut.Long(mountNum);
        return ImageDrawCrud.SelectMany(command);
    }

    /*
    ///<summary>Gets one ImageDraw from the db.</summary>
    public static ImageDraw GetOne(long imageDrawNum){

        return Crud.ImageDrawCrud.SelectOne(imageDrawNum);
    }*/

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(ImageDraw imageDraw)
    {
        return ImageDrawCrud.Insert(imageDraw);
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

    #endregion Methods - Modify

    #region Methods - Helper Logic

    /*
    
    public static double ImageScale(MountItem mountItem,Bitmap bitmap,Document document) {
        double zoomOrig;
        if(document.CropW==0) {
            float ratioCropWtoH=(float)mountItem.Width/(float)mountItem.Height;
            bool isWide=false;
            if((double)bitmap.Width/bitmap.Height > ratioCropWtoH) {
                isWide=true;
            }
            if(isWide) {
                zoomOrig=(double)mountItem.Width/(double)bitmap.Width;
            }
            else {
                zoomOrig=(double)mountItem.Height/(double)bitmap.Height;
            }
        }
        else {
            zoomOrig=(double)mountItem.Width/(double)document.CropW;
        }
        zoomOrig=Math.Round(zoomOrig,3);
        return zoomOrig;
    }*/

    ///<summary>Calculates in pixels, which separately get converted according to scale.</summary>
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

    /// <summary>
    ///     Calculates the scale required to fit the bitmap into the mount item while maintaining original aspect ratio.
    ///     Used for scaling generated ImageDraws to align with the mounted bitmap.
    /// </summary>
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

    /// <summary>
    ///     Calculates the padding required to fit the bitmap into the mount item while maintaining original aspect ratio.
    ///     Used for translating generated ImageDraws to align with the mounted bitmap.
    /// </summary>
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

    ///<summary>Compares aspect ratio of mount item to bitmap.</summary>
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

    /// <summary>
    ///     For text + line and contour boxes, Pearl uses pixels as coordinates relative to the original image. If the
    ///     image was in a mount with different pixel size, we need to scale the pixels accordingly.
    /// </summary>
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

    ///<summary>Translates a list of points to a position. Used to place points over the correct location in a mount.</summary>
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

    #endregion
}