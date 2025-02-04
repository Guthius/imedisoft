using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DocumentCrud
{
    public static Document SelectOne(long docNum)
    {
        var command = "SELECT * FROM document "
                      + "WHERE DocNum = " + SOut.Long(docNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Document SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Document> TableToList(DataTable table)
    {
        var retVal = new List<Document>();
        foreach (DataRow row in table.Rows)
        {
            var document = new Document
            {
                DocNum = SIn.Long(row["DocNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                DateCreated = SIn.DateTime(row["DateCreated"].ToString()),
                DocCategory = SIn.Long(row["DocCategory"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                FileName = SIn.String(row["FileName"].ToString()),
                ImgType = (ImageType) SIn.Int(row["ImgType"].ToString()),
                IsFlipped = SIn.Bool(row["IsFlipped"].ToString()),
                DegreesRotated = SIn.Float(row["DegreesRotated"].ToString()),
                ToothNumbers = SIn.String(row["ToothNumbers"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString()),
                Signature = SIn.String(row["Signature"].ToString()),
                CropX = SIn.Int(row["CropX"].ToString()),
                CropY = SIn.Int(row["CropY"].ToString()),
                CropW = SIn.Int(row["CropW"].ToString()),
                CropH = SIn.Int(row["CropH"].ToString()),
                WindowingMin = SIn.Int(row["WindowingMin"].ToString()),
                WindowingMax = SIn.Int(row["WindowingMax"].ToString()),
                MountItemNum = SIn.Long(row["MountItemNum"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                RawBase64 = SIn.String(row["RawBase64"].ToString()),
                Thumbnail = SIn.String(row["Thumbnail"].ToString()),
                ExternalGUID = SIn.String(row["ExternalGUID"].ToString())
            };
            var externalSource = row["ExternalSource"].ToString();
            if (externalSource == "")
                document.ExternalSource = 0;
            else
                try
                {
                    document.ExternalSource = (ExternalSourceType) Enum.Parse(typeof(ExternalSourceType), externalSource);
                }
                catch
                {
                    document.ExternalSource = 0;
                }

            document.ProvNum = SIn.Long(row["ProvNum"].ToString());
            document.IsCropOld = SIn.Bool(row["IsCropOld"].ToString());
            document.OcrResponseData = SIn.String(row["OcrResponseData"].ToString());
            document.ImageCaptureType = SIn.Int(row["ImageCaptureType"].ToString());
            document.PrintHeading = SIn.Bool(row["PrintHeading"].ToString());
            document.ChartLetterStatus = (EnumDocChartLetterStatus) SIn.Int(row["ChartLetterStatus"].ToString());
            document.UserNum = SIn.Long(row["UserNum"].ToString());
            document.ChartLetterHash = SIn.String(row["ChartLetterHash"].ToString());
            retVal.Add(document);
        }

        return retVal;
    }

    public static long Insert(Document document)
    {
        var command = "INSERT INTO document (";

        command += "Description,DateCreated,DocCategory,PatNum,FileName,ImgType,IsFlipped,DegreesRotated,ToothNumbers,Note,SigIsTopaz,Signature,CropX,CropY,CropW,CropH,WindowingMin,WindowingMax,MountItemNum,RawBase64,Thumbnail,ExternalGUID,ExternalSource,ProvNum,IsCropOld,OcrResponseData,ImageCaptureType,PrintHeading,ChartLetterStatus,UserNum,ChartLetterHash) VALUES(";

        command +=
            "'" + SOut.String(document.Description) + "',"
            + SOut.DateTime(document.DateCreated) + ","
            + SOut.Long(document.DocCategory) + ","
            + SOut.Long(document.PatNum) + ","
            + "'" + SOut.String(document.FileName) + "',"
            + SOut.Int((int) document.ImgType) + ","
            + SOut.Bool(document.IsFlipped) + ","
            + SOut.Float(document.DegreesRotated) + ","
            + "'" + SOut.String(document.ToothNumbers) + "',"
            + DbHelper.ParamChar + "paramNote,"
            + SOut.Bool(document.SigIsTopaz) + ","
            + DbHelper.ParamChar + "paramSignature,"
            + SOut.Int(document.CropX) + ","
            + SOut.Int(document.CropY) + ","
            + SOut.Int(document.CropW) + ","
            + SOut.Int(document.CropH) + ","
            + SOut.Int(document.WindowingMin) + ","
            + SOut.Int(document.WindowingMax) + ","
            + SOut.Long(document.MountItemNum) + ","
            //DateTStamp can only be set by MySQL
            + DbHelper.ParamChar + "paramRawBase64,"
            + DbHelper.ParamChar + "paramThumbnail,"
            + "'" + SOut.String(document.ExternalGUID) + "',"
            + "'" + SOut.String(document.ExternalSource.ToString()) + "',"
            + SOut.Long(document.ProvNum) + ","
            + SOut.Bool(document.IsCropOld) + ","
            + DbHelper.ParamChar + "paramOcrResponseData,"
            + SOut.Int((int) document.ImageCaptureType) + ","
            + SOut.Bool(document.PrintHeading) + ","
            + SOut.Int((int) document.ChartLetterStatus) + ","
            + SOut.Long(document.UserNum) + ","
            + "'" + SOut.String(document.ChartLetterHash) + "')";
        if (document.Note == null) document.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(document.Note));
        if (document.Signature == null) document.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(document.Signature));
        if (document.RawBase64 == null) document.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", SOut.StringParam(document.RawBase64));
        if (document.Thumbnail == null) document.Thumbnail = "";
        var paramThumbnail = new OdSqlParameter("paramThumbnail", SOut.StringParam(document.Thumbnail));
        if (document.OcrResponseData == null) document.OcrResponseData = "";
        var paramOcrResponseData = new OdSqlParameter("paramOcrResponseData", SOut.StringParam(document.OcrResponseData));
        {
            document.DocNum = Db.NonQ(command, true, "DocNum", "document", paramNote, paramSignature, paramRawBase64, paramThumbnail, paramOcrResponseData);
        }
        return document.DocNum;
    }

    public static void Update(Document document)
    {
        var command = "UPDATE document SET "
                      + "Description      = '" + SOut.String(document.Description) + "', "
                      + "DateCreated      =  " + SOut.DateTime(document.DateCreated) + ", "
                      + "DocCategory      =  " + SOut.Long(document.DocCategory) + ", "
                      + "PatNum           =  " + SOut.Long(document.PatNum) + ", "
                      + "FileName         = '" + SOut.String(document.FileName) + "', "
                      + "ImgType          =  " + SOut.Int((int) document.ImgType) + ", "
                      + "IsFlipped        =  " + SOut.Bool(document.IsFlipped) + ", "
                      + "DegreesRotated   =  " + SOut.Float(document.DegreesRotated) + ", "
                      + "ToothNumbers     = '" + SOut.String(document.ToothNumbers) + "', "
                      + "Note             =  " + DbHelper.ParamChar + "paramNote, "
                      + "SigIsTopaz       =  " + SOut.Bool(document.SigIsTopaz) + ", "
                      + "Signature        =  " + DbHelper.ParamChar + "paramSignature, "
                      + "CropX            =  " + SOut.Int(document.CropX) + ", "
                      + "CropY            =  " + SOut.Int(document.CropY) + ", "
                      + "CropW            =  " + SOut.Int(document.CropW) + ", "
                      + "CropH            =  " + SOut.Int(document.CropH) + ", "
                      + "WindowingMin     =  " + SOut.Int(document.WindowingMin) + ", "
                      + "WindowingMax     =  " + SOut.Int(document.WindowingMax) + ", "
                      + "MountItemNum     =  " + SOut.Long(document.MountItemNum) + ", "
                      //DateTStamp can only be set by MySQL
                      + "RawBase64        =  " + DbHelper.ParamChar + "paramRawBase64, "
                      + "Thumbnail        =  " + DbHelper.ParamChar + "paramThumbnail, "
                      + "ExternalGUID     = '" + SOut.String(document.ExternalGUID) + "', "
                      + "ExternalSource   = '" + SOut.String(document.ExternalSource.ToString()) + "', "
                      + "ProvNum          =  " + SOut.Long(document.ProvNum) + ", "
                      + "IsCropOld        =  " + SOut.Bool(document.IsCropOld) + ", "
                      + "OcrResponseData  =  " + DbHelper.ParamChar + "paramOcrResponseData, "
                      + "ImageCaptureType =  " + SOut.Int((int) document.ImageCaptureType) + ", "
                      + "PrintHeading     =  " + SOut.Bool(document.PrintHeading) + ", "
                      + "ChartLetterStatus=  " + SOut.Int((int) document.ChartLetterStatus) + ", "
                      + "UserNum          =  " + SOut.Long(document.UserNum) + ", "
                      + "ChartLetterHash  = '" + SOut.String(document.ChartLetterHash) + "' "
                      + "WHERE DocNum = " + SOut.Long(document.DocNum);
        if (document.Note == null) document.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(document.Note));
        if (document.Signature == null) document.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(document.Signature));
        if (document.RawBase64 == null) document.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", SOut.StringParam(document.RawBase64));
        if (document.Thumbnail == null) document.Thumbnail = "";
        var paramThumbnail = new OdSqlParameter("paramThumbnail", SOut.StringParam(document.Thumbnail));
        if (document.OcrResponseData == null) document.OcrResponseData = "";
        var paramOcrResponseData = new OdSqlParameter("paramOcrResponseData", SOut.StringParam(document.OcrResponseData));
        Db.NonQ(command, paramNote, paramSignature, paramRawBase64, paramThumbnail, paramOcrResponseData);
    }

    public static bool Update(Document document, Document oldDocument)
    {
        var command = "";
        if (document.Description != oldDocument.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(document.Description) + "'";
        }

        if (document.DateCreated != oldDocument.DateCreated)
        {
            if (command != "") command += ",";
            command += "DateCreated = " + SOut.DateTime(document.DateCreated) + "";
        }

        if (document.DocCategory != oldDocument.DocCategory)
        {
            if (command != "") command += ",";
            command += "DocCategory = " + SOut.Long(document.DocCategory) + "";
        }

        if (document.PatNum != oldDocument.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(document.PatNum) + "";
        }

        if (document.FileName != oldDocument.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(document.FileName) + "'";
        }

        if (document.ImgType != oldDocument.ImgType)
        {
            if (command != "") command += ",";
            command += "ImgType = " + SOut.Int((int) document.ImgType) + "";
        }

        if (document.IsFlipped != oldDocument.IsFlipped)
        {
            if (command != "") command += ",";
            command += "IsFlipped = " + SOut.Bool(document.IsFlipped) + "";
        }

        if (document.DegreesRotated != oldDocument.DegreesRotated)
        {
            if (command != "") command += ",";
            command += "DegreesRotated = " + SOut.Float(document.DegreesRotated) + "";
        }

        if (document.ToothNumbers != oldDocument.ToothNumbers)
        {
            if (command != "") command += ",";
            command += "ToothNumbers = '" + SOut.String(document.ToothNumbers) + "'";
        }

        if (document.Note != oldDocument.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (document.SigIsTopaz != oldDocument.SigIsTopaz)
        {
            if (command != "") command += ",";
            command += "SigIsTopaz = " + SOut.Bool(document.SigIsTopaz) + "";
        }

        if (document.Signature != oldDocument.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (document.CropX != oldDocument.CropX)
        {
            if (command != "") command += ",";
            command += "CropX = " + SOut.Int(document.CropX) + "";
        }

        if (document.CropY != oldDocument.CropY)
        {
            if (command != "") command += ",";
            command += "CropY = " + SOut.Int(document.CropY) + "";
        }

        if (document.CropW != oldDocument.CropW)
        {
            if (command != "") command += ",";
            command += "CropW = " + SOut.Int(document.CropW) + "";
        }

        if (document.CropH != oldDocument.CropH)
        {
            if (command != "") command += ",";
            command += "CropH = " + SOut.Int(document.CropH) + "";
        }

        if (document.WindowingMin != oldDocument.WindowingMin)
        {
            if (command != "") command += ",";
            command += "WindowingMin = " + SOut.Int(document.WindowingMin) + "";
        }

        if (document.WindowingMax != oldDocument.WindowingMax)
        {
            if (command != "") command += ",";
            command += "WindowingMax = " + SOut.Int(document.WindowingMax) + "";
        }

        if (document.MountItemNum != oldDocument.MountItemNum)
        {
            if (command != "") command += ",";
            command += "MountItemNum = " + SOut.Long(document.MountItemNum) + "";
        }

        //DateTStamp can only be set by MySQL
        if (document.RawBase64 != oldDocument.RawBase64)
        {
            if (command != "") command += ",";
            command += "RawBase64 = " + DbHelper.ParamChar + "paramRawBase64";
        }

        if (document.Thumbnail != oldDocument.Thumbnail)
        {
            if (command != "") command += ",";
            command += "Thumbnail = " + DbHelper.ParamChar + "paramThumbnail";
        }

        if (document.ExternalGUID != oldDocument.ExternalGUID)
        {
            if (command != "") command += ",";
            command += "ExternalGUID = '" + SOut.String(document.ExternalGUID) + "'";
        }

        if (document.ExternalSource != oldDocument.ExternalSource)
        {
            if (command != "") command += ",";
            command += "ExternalSource = '" + SOut.String(document.ExternalSource.ToString()) + "'";
        }

        if (document.ProvNum != oldDocument.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(document.ProvNum) + "";
        }

        if (document.IsCropOld != oldDocument.IsCropOld)
        {
            if (command != "") command += ",";
            command += "IsCropOld = " + SOut.Bool(document.IsCropOld) + "";
        }

        if (document.OcrResponseData != oldDocument.OcrResponseData)
        {
            if (command != "") command += ",";
            command += "OcrResponseData = " + DbHelper.ParamChar + "paramOcrResponseData";
        }

        if (document.ImageCaptureType != oldDocument.ImageCaptureType)
        {
            if (command != "") command += ",";
            command += "ImageCaptureType = " + SOut.Int((int) document.ImageCaptureType) + "";
        }

        if (document.PrintHeading != oldDocument.PrintHeading)
        {
            if (command != "") command += ",";
            command += "PrintHeading = " + SOut.Bool(document.PrintHeading) + "";
        }

        if (document.ChartLetterStatus != oldDocument.ChartLetterStatus)
        {
            if (command != "") command += ",";
            command += "ChartLetterStatus = " + SOut.Int((int) document.ChartLetterStatus) + "";
        }

        if (document.UserNum != oldDocument.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(document.UserNum) + "";
        }

        if (document.ChartLetterHash != oldDocument.ChartLetterHash)
        {
            if (command != "") command += ",";
            command += "ChartLetterHash = '" + SOut.String(document.ChartLetterHash) + "'";
        }

        if (command == "") return false;
        if (document.Note == null) document.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(document.Note));
        if (document.Signature == null) document.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", SOut.StringParam(document.Signature));
        if (document.RawBase64 == null) document.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", SOut.StringParam(document.RawBase64));
        if (document.Thumbnail == null) document.Thumbnail = "";
        var paramThumbnail = new OdSqlParameter("paramThumbnail", SOut.StringParam(document.Thumbnail));
        if (document.OcrResponseData == null) document.OcrResponseData = "";
        var paramOcrResponseData = new OdSqlParameter("paramOcrResponseData", SOut.StringParam(document.OcrResponseData));
        command = "UPDATE document SET " + command
                                         + " WHERE DocNum = " + SOut.Long(document.DocNum);
        Db.NonQ(command, paramNote, paramSignature, paramRawBase64, paramThumbnail, paramOcrResponseData);
        return true;
    }

    public static void Delete(long docNum)
    {
        ClearFkey(docNum);
        var command = "DELETE FROM document "
                      + "WHERE DocNum = " + SOut.Long(docNum);
        Db.NonQ(command);
    }

    public static void ClearFkey(long docNum)
    {
        if (docNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(docNum) + " AND PermType IN (44,89)";
        Db.NonQ(command);
    }
}