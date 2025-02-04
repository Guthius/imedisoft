using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDentBusiness.Pearl;
using Color = System.Drawing.Color;

namespace Imedisoft.Core.Crud;

public class ImageDrawCrud
{
    public static List<ImageDraw> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ImageDraw> TableToList(DataTable table)
    {
        var retVal = new List<ImageDraw>();
        foreach (DataRow row in table.Rows)
        {
            var imageDraw = new ImageDraw
            {
                ImageDrawNum = SIn.Long(row["ImageDrawNum"].ToString()),
                DocNum = SIn.Long(row["DocNum"].ToString()),
                MountNum = SIn.Long(row["MountNum"].ToString()),
                ColorDraw = Color.FromArgb(SIn.Int(row["ColorDraw"].ToString())),
                ColorBack = Color.FromArgb(SIn.Int(row["ColorBack"].ToString())),
                DrawingSegment = SIn.String(row["DrawingSegment"].ToString()),
                DrawText = SIn.String(row["DrawText"].ToString()),
                FontSize = SIn.Float(row["FontSize"].ToString()),
                DrawType = (ImageDrawType) SIn.Int(row["DrawType"].ToString()),
                ImageAnnotVendor = (EnumImageAnnotVendor) SIn.Int(row["ImageAnnotVendor"].ToString()),
                Details = SIn.String(row["Details"].ToString()),
                PearlLayer = (EnumCategoryOD) SIn.Int(row["PearlLayer"].ToString()),
                BetterDiagLayer = (EnumCategoryBetterDiag) SIn.Int(row["BetterDiagLayer"].ToString())
            };
            retVal.Add(imageDraw);
        }

        return retVal;
    }

    public static void Insert(ImageDraw imageDraw)
    {
        var command = "INSERT INTO imagedraw (";

        command += "DocNum,MountNum,ColorDraw,ColorBack,DrawingSegment,DrawText,FontSize,DrawType,ImageAnnotVendor,Details,PearlLayer,BetterDiagLayer) VALUES(";

        command +=
            SOut.Long(imageDraw.DocNum) + ","
                                        + SOut.Long(imageDraw.MountNum) + ","
                                        + SOut.Int(imageDraw.ColorDraw.ToArgb()) + ","
                                        + SOut.Int(imageDraw.ColorBack.ToArgb()) + ","
                                        + DbHelper.ParamChar + "paramDrawingSegment,"
                                        + "'" + SOut.String(imageDraw.DrawText) + "',"
                                        + SOut.Float(imageDraw.FontSize) + ","
                                        + SOut.Int((int) imageDraw.DrawType) + ","
                                        + SOut.Int((int) imageDraw.ImageAnnotVendor) + ","
                                        + DbHelper.ParamChar + "paramDetails,"
                                        + SOut.Int((int) imageDraw.PearlLayer) + ","
                                        + SOut.Int((int) imageDraw.BetterDiagLayer) + ")";
        if (imageDraw.DrawingSegment == null) imageDraw.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", SOut.StringParam(imageDraw.DrawingSegment));
        if (imageDraw.Details == null) imageDraw.Details = "";
        var paramDetails = new OdSqlParameter("paramDetails", SOut.StringParam(imageDraw.Details));
        {
            imageDraw.ImageDrawNum = Db.NonQ(command, true, "ImageDrawNum", "imageDraw", paramDrawingSegment, paramDetails);
        }
    }

    public static void Update(ImageDraw imageDraw)
    {
        var command = "UPDATE imagedraw SET "
                      + "DocNum          =  " + SOut.Long(imageDraw.DocNum) + ", "
                      + "MountNum        =  " + SOut.Long(imageDraw.MountNum) + ", "
                      + "ColorDraw       =  " + SOut.Int(imageDraw.ColorDraw.ToArgb()) + ", "
                      + "ColorBack       =  " + SOut.Int(imageDraw.ColorBack.ToArgb()) + ", "
                      + "DrawingSegment  =  " + DbHelper.ParamChar + "paramDrawingSegment, "
                      + "DrawText        = '" + SOut.String(imageDraw.DrawText) + "', "
                      + "FontSize        =  " + SOut.Float(imageDraw.FontSize) + ", "
                      + "DrawType        =  " + SOut.Int((int) imageDraw.DrawType) + ", "
                      + "ImageAnnotVendor=  " + SOut.Int((int) imageDraw.ImageAnnotVendor) + ", "
                      + "Details         =  " + DbHelper.ParamChar + "paramDetails, "
                      + "PearlLayer      =  " + SOut.Int((int) imageDraw.PearlLayer) + ", "
                      + "BetterDiagLayer =  " + SOut.Int((int) imageDraw.BetterDiagLayer) + " "
                      + "WHERE ImageDrawNum = " + SOut.Long(imageDraw.ImageDrawNum);
        if (imageDraw.DrawingSegment == null) imageDraw.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", SOut.StringParam(imageDraw.DrawingSegment));
        if (imageDraw.Details == null) imageDraw.Details = "";
        var paramDetails = new OdSqlParameter("paramDetails", SOut.StringParam(imageDraw.Details));
        Db.NonQ(command, paramDrawingSegment, paramDetails);
    }

    public static void Delete(long imageDrawNum)
    {
        var command = "DELETE FROM imagedraw "
                      + "WHERE ImageDrawNum = " + SOut.Long(imageDrawNum);
        Db.NonQ(command);
    }
}