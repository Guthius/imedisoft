#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Pearl;
using Color = System.Drawing.Color;

#endregion

namespace OpenDentBusiness.Crud;

public class ImageDrawCrud
{
    public static ImageDraw SelectOne(long imageDrawNum)
    {
        var command = "SELECT * FROM imagedraw "
                      + "WHERE ImageDrawNum = " + SOut.Long(imageDrawNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ImageDraw SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ImageDraw> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ImageDraw> TableToList(DataTable table)
    {
        var retVal = new List<ImageDraw>();
        ImageDraw imageDraw;
        foreach (DataRow row in table.Rows)
        {
            imageDraw = new ImageDraw();
            imageDraw.ImageDrawNum = SIn.Long(row["ImageDrawNum"].ToString());
            imageDraw.DocNum = SIn.Long(row["DocNum"].ToString());
            imageDraw.MountNum = SIn.Long(row["MountNum"].ToString());
            imageDraw.ColorDraw = Color.FromArgb(SIn.Int(row["ColorDraw"].ToString()));
            imageDraw.ColorBack = Color.FromArgb(SIn.Int(row["ColorBack"].ToString()));
            imageDraw.DrawingSegment = SIn.String(row["DrawingSegment"].ToString());
            imageDraw.DrawText = SIn.String(row["DrawText"].ToString());
            imageDraw.FontSize = SIn.Float(row["FontSize"].ToString());
            imageDraw.DrawType = (ImageDrawType) SIn.Int(row["DrawType"].ToString());
            imageDraw.ImageAnnotVendor = (EnumImageAnnotVendor) SIn.Int(row["ImageAnnotVendor"].ToString());
            imageDraw.Details = SIn.String(row["Details"].ToString());
            imageDraw.PearlLayer = (EnumCategoryOD) SIn.Int(row["PearlLayer"].ToString());
            imageDraw.BetterDiagLayer = (EnumCategoryBetterDiag) SIn.Int(row["BetterDiagLayer"].ToString());
            retVal.Add(imageDraw);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ImageDraw> listImageDraws, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ImageDraw";
        var table = new DataTable(tableName);
        table.Columns.Add("ImageDrawNum");
        table.Columns.Add("DocNum");
        table.Columns.Add("MountNum");
        table.Columns.Add("ColorDraw");
        table.Columns.Add("ColorBack");
        table.Columns.Add("DrawingSegment");
        table.Columns.Add("DrawText");
        table.Columns.Add("FontSize");
        table.Columns.Add("DrawType");
        table.Columns.Add("ImageAnnotVendor");
        table.Columns.Add("Details");
        table.Columns.Add("PearlLayer");
        table.Columns.Add("BetterDiagLayer");
        foreach (var imageDraw in listImageDraws)
            table.Rows.Add(SOut.Long(imageDraw.ImageDrawNum), SOut.Long(imageDraw.DocNum), SOut.Long(imageDraw.MountNum), SOut.Int(imageDraw.ColorDraw.ToArgb()), SOut.Int(imageDraw.ColorBack.ToArgb()), imageDraw.DrawingSegment, imageDraw.DrawText, SOut.Float(imageDraw.FontSize), SOut.Int((int) imageDraw.DrawType), SOut.Int((int) imageDraw.ImageAnnotVendor), imageDraw.Details, SOut.Int((int) imageDraw.PearlLayer), SOut.Int((int) imageDraw.BetterDiagLayer));
        return table;
    }

    public static long Insert(ImageDraw imageDraw)
    {
        return Insert(imageDraw, false);
    }

    public static long Insert(ImageDraw imageDraw, bool useExistingPK)
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
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(imageDraw.DrawingSegment));
        if (imageDraw.Details == null) imageDraw.Details = "";
        var paramDetails = new OdSqlParameter("paramDetails", OdDbType.Text, SOut.StringParam(imageDraw.Details));
        {
            imageDraw.ImageDrawNum = Db.NonQ(command, true, "ImageDrawNum", "imageDraw", paramDrawingSegment, paramDetails);
        }
        return imageDraw.ImageDrawNum;
    }

    public static long InsertNoCache(ImageDraw imageDraw)
    {
        return InsertNoCache(imageDraw, false);
    }

    public static long InsertNoCache(ImageDraw imageDraw, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO imagedraw (";
        if (isRandomKeys || useExistingPK) command += "ImageDrawNum,";
        command += "DocNum,MountNum,ColorDraw,ColorBack,DrawingSegment,DrawText,FontSize,DrawType,ImageAnnotVendor,Details,PearlLayer,BetterDiagLayer) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(imageDraw.ImageDrawNum) + ",";
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
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(imageDraw.DrawingSegment));
        if (imageDraw.Details == null) imageDraw.Details = "";
        var paramDetails = new OdSqlParameter("paramDetails", OdDbType.Text, SOut.StringParam(imageDraw.Details));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDrawingSegment, paramDetails);
        else
            imageDraw.ImageDrawNum = Db.NonQ(command, true, "ImageDrawNum", "imageDraw", paramDrawingSegment, paramDetails);
        return imageDraw.ImageDrawNum;
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
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(imageDraw.DrawingSegment));
        if (imageDraw.Details == null) imageDraw.Details = "";
        var paramDetails = new OdSqlParameter("paramDetails", OdDbType.Text, SOut.StringParam(imageDraw.Details));
        Db.NonQ(command, paramDrawingSegment, paramDetails);
    }

    public static bool Update(ImageDraw imageDraw, ImageDraw oldImageDraw)
    {
        var command = "";
        if (imageDraw.DocNum != oldImageDraw.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(imageDraw.DocNum) + "";
        }

        if (imageDraw.MountNum != oldImageDraw.MountNum)
        {
            if (command != "") command += ",";
            command += "MountNum = " + SOut.Long(imageDraw.MountNum) + "";
        }

        if (imageDraw.ColorDraw != oldImageDraw.ColorDraw)
        {
            if (command != "") command += ",";
            command += "ColorDraw = " + SOut.Int(imageDraw.ColorDraw.ToArgb()) + "";
        }

        if (imageDraw.ColorBack != oldImageDraw.ColorBack)
        {
            if (command != "") command += ",";
            command += "ColorBack = " + SOut.Int(imageDraw.ColorBack.ToArgb()) + "";
        }

        if (imageDraw.DrawingSegment != oldImageDraw.DrawingSegment)
        {
            if (command != "") command += ",";
            command += "DrawingSegment = " + DbHelper.ParamChar + "paramDrawingSegment";
        }

        if (imageDraw.DrawText != oldImageDraw.DrawText)
        {
            if (command != "") command += ",";
            command += "DrawText = '" + SOut.String(imageDraw.DrawText) + "'";
        }

        if (imageDraw.FontSize != oldImageDraw.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(imageDraw.FontSize) + "";
        }

        if (imageDraw.DrawType != oldImageDraw.DrawType)
        {
            if (command != "") command += ",";
            command += "DrawType = " + SOut.Int((int) imageDraw.DrawType) + "";
        }

        if (imageDraw.ImageAnnotVendor != oldImageDraw.ImageAnnotVendor)
        {
            if (command != "") command += ",";
            command += "ImageAnnotVendor = " + SOut.Int((int) imageDraw.ImageAnnotVendor) + "";
        }

        if (imageDraw.Details != oldImageDraw.Details)
        {
            if (command != "") command += ",";
            command += "Details = " + DbHelper.ParamChar + "paramDetails";
        }

        if (imageDraw.PearlLayer != oldImageDraw.PearlLayer)
        {
            if (command != "") command += ",";
            command += "PearlLayer = " + SOut.Int((int) imageDraw.PearlLayer) + "";
        }

        if (imageDraw.BetterDiagLayer != oldImageDraw.BetterDiagLayer)
        {
            if (command != "") command += ",";
            command += "BetterDiagLayer = " + SOut.Int((int) imageDraw.BetterDiagLayer) + "";
        }

        if (command == "") return false;
        if (imageDraw.DrawingSegment == null) imageDraw.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(imageDraw.DrawingSegment));
        if (imageDraw.Details == null) imageDraw.Details = "";
        var paramDetails = new OdSqlParameter("paramDetails", OdDbType.Text, SOut.StringParam(imageDraw.Details));
        command = "UPDATE imagedraw SET " + command
                                          + " WHERE ImageDrawNum = " + SOut.Long(imageDraw.ImageDrawNum);
        Db.NonQ(command, paramDrawingSegment, paramDetails);
        return true;
    }

    public static bool UpdateComparison(ImageDraw imageDraw, ImageDraw oldImageDraw)
    {
        if (imageDraw.DocNum != oldImageDraw.DocNum) return true;
        if (imageDraw.MountNum != oldImageDraw.MountNum) return true;
        if (imageDraw.ColorDraw != oldImageDraw.ColorDraw) return true;
        if (imageDraw.ColorBack != oldImageDraw.ColorBack) return true;
        if (imageDraw.DrawingSegment != oldImageDraw.DrawingSegment) return true;
        if (imageDraw.DrawText != oldImageDraw.DrawText) return true;
        if (imageDraw.FontSize != oldImageDraw.FontSize) return true;
        if (imageDraw.DrawType != oldImageDraw.DrawType) return true;
        if (imageDraw.ImageAnnotVendor != oldImageDraw.ImageAnnotVendor) return true;
        if (imageDraw.Details != oldImageDraw.Details) return true;
        if (imageDraw.PearlLayer != oldImageDraw.PearlLayer) return true;
        if (imageDraw.BetterDiagLayer != oldImageDraw.BetterDiagLayer) return true;
        return false;
    }

    public static void Delete(long imageDrawNum)
    {
        var command = "DELETE FROM imagedraw "
                      + "WHERE ImageDrawNum = " + SOut.Long(imageDrawNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listImageDrawNums)
    {
        if (listImageDrawNums == null || listImageDrawNums.Count == 0) return;
        var command = "DELETE FROM imagedraw "
                      + "WHERE ImageDrawNum IN(" + string.Join(",", listImageDrawNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}