using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SheetCrud
{
    public static Sheet SelectOne(long sheetNum)
    {
        var command = "SELECT * FROM sheet "
                      + "WHERE SheetNum = " + SOut.Long(sheetNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Sheet SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Sheet> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Sheet> TableToList(DataTable table)
    {
        var retVal = new List<Sheet>();
        foreach (DataRow row in table.Rows)
        {
            var sheet = new Sheet
            {
                SheetNum = SIn.Long(row["SheetNum"].ToString()),
                SheetType = (SheetTypeEnum) SIn.Int(row["SheetType"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateTimeSheet = SIn.DateTime(row["DateTimeSheet"].ToString()),
                FontSize = SIn.Float(row["FontSize"].ToString()),
                FontName = SIn.String(row["FontName"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                Height = SIn.Int(row["Height"].ToString()),
                IsLandscape = SIn.Bool(row["IsLandscape"].ToString()),
                InternalNote = SIn.String(row["InternalNote"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                ShowInTerminal = SIn.Byte(row["ShowInTerminal"].ToString()),
                IsWebForm = SIn.Bool(row["IsWebForm"].ToString()),
                IsMultiPage = SIn.Bool(row["IsMultiPage"].ToString()),
                IsDeleted = SIn.Bool(row["IsDeleted"].ToString()),
                SheetDefNum = SIn.Long(row["SheetDefNum"].ToString()),
                DocNum = SIn.Long(row["DocNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                DateTSheetEdited = SIn.DateTime(row["DateTSheetEdited"].ToString()),
                HasMobileLayout = SIn.Bool(row["HasMobileLayout"].ToString()),
                RevID = SIn.Int(row["RevID"].ToString()),
                WebFormSheetID = SIn.Long(row["WebFormSheetID"].ToString())
            };
            retVal.Add(sheet);
        }

        return retVal;
    }

    public static void Insert(Sheet sheet)
    {
        var command = "INSERT INTO sheet (";

        command += "SheetType,PatNum,DateTimeSheet,FontSize,FontName,Width,Height,IsLandscape,InternalNote,Description,ShowInTerminal,IsWebForm,IsMultiPage,IsDeleted,SheetDefNum,DocNum,ClinicNum,DateTSheetEdited,HasMobileLayout,RevID,WebFormSheetID) VALUES(";

        command +=
            SOut.Int((int) sheet.SheetType) + ","
                                            + SOut.Long(sheet.PatNum) + ","
                                            + SOut.DateTime(sheet.DateTimeSheet) + ","
                                            + SOut.Float(sheet.FontSize) + ","
                                            + "'" + SOut.String(sheet.FontName) + "',"
                                            + SOut.Int(sheet.Width) + ","
                                            + SOut.Int(sheet.Height) + ","
                                            + SOut.Bool(sheet.IsLandscape) + ","
                                            + DbHelper.ParamChar + "paramInternalNote,"
                                            + "'" + SOut.String(sheet.Description) + "',"
                                            + SOut.Byte(sheet.ShowInTerminal) + ","
                                            + SOut.Bool(sheet.IsWebForm) + ","
                                            + SOut.Bool(sheet.IsMultiPage) + ","
                                            + SOut.Bool(sheet.IsDeleted) + ","
                                            + SOut.Long(sheet.SheetDefNum) + ","
                                            + SOut.Long(sheet.DocNum) + ","
                                            + SOut.Long(sheet.ClinicNum) + ","
                                            + "NOW()" + ","
                                            + SOut.Bool(sheet.HasMobileLayout) + ","
                                            + SOut.Int(sheet.RevID) + ","
                                            + SOut.Long(sheet.WebFormSheetID) + ")";
        if (sheet.InternalNote == null) sheet.InternalNote = "";
        var paramInternalNote = new OdSqlParameter("paramInternalNote", SOut.StringParam(sheet.InternalNote));
        {
            sheet.SheetNum = Db.NonQ(command, true, "SheetNum", "sheet", paramInternalNote);
        }
    }

    public static void Update(Sheet sheet)
    {
        var command = "UPDATE sheet SET "
                      + "SheetType       =  " + SOut.Int((int) sheet.SheetType) + ", "
                      + "PatNum          =  " + SOut.Long(sheet.PatNum) + ", "
                      + "DateTimeSheet   =  " + SOut.DateTime(sheet.DateTimeSheet) + ", "
                      + "FontSize        =  " + SOut.Float(sheet.FontSize) + ", "
                      + "FontName        = '" + SOut.String(sheet.FontName) + "', "
                      + "Width           =  " + SOut.Int(sheet.Width) + ", "
                      + "Height          =  " + SOut.Int(sheet.Height) + ", "
                      + "IsLandscape     =  " + SOut.Bool(sheet.IsLandscape) + ", "
                      + "InternalNote    =  " + DbHelper.ParamChar + "paramInternalNote, "
                      + "Description     = '" + SOut.String(sheet.Description) + "', "
                      + "ShowInTerminal  =  " + SOut.Byte(sheet.ShowInTerminal) + ", "
                      + "IsWebForm       =  " + SOut.Bool(sheet.IsWebForm) + ", "
                      + "IsMultiPage     =  " + SOut.Bool(sheet.IsMultiPage) + ", "
                      + "IsDeleted       =  " + SOut.Bool(sheet.IsDeleted) + ", "
                      + "SheetDefNum     =  " + SOut.Long(sheet.SheetDefNum) + ", "
                      + "DocNum          =  " + SOut.Long(sheet.DocNum) + ", "
                      + "ClinicNum       =  " + SOut.Long(sheet.ClinicNum) + ", "
                      + "DateTSheetEdited=  " + SOut.DateTime(sheet.DateTSheetEdited) + ", "
                      + "HasMobileLayout =  " + SOut.Bool(sheet.HasMobileLayout) + ", "
                      + "RevID           =  " + SOut.Int(sheet.RevID) + ", "
                      + "WebFormSheetID  =  " + SOut.Long(sheet.WebFormSheetID) + " "
                      + "WHERE SheetNum = " + SOut.Long(sheet.SheetNum);
        if (sheet.InternalNote == null) sheet.InternalNote = "";
        var paramInternalNote = new OdSqlParameter("paramInternalNote", SOut.StringParam(sheet.InternalNote));
        Db.NonQ(command, paramInternalNote);
    }
}