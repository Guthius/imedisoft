#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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
        Sheet sheet;
        foreach (DataRow row in table.Rows)
        {
            sheet = new Sheet();
            sheet.SheetNum = SIn.Long(row["SheetNum"].ToString());
            sheet.SheetType = (SheetTypeEnum) SIn.Int(row["SheetType"].ToString());
            sheet.PatNum = SIn.Long(row["PatNum"].ToString());
            sheet.DateTimeSheet = SIn.DateTime(row["DateTimeSheet"].ToString());
            sheet.FontSize = SIn.Float(row["FontSize"].ToString());
            sheet.FontName = SIn.String(row["FontName"].ToString());
            sheet.Width = SIn.Int(row["Width"].ToString());
            sheet.Height = SIn.Int(row["Height"].ToString());
            sheet.IsLandscape = SIn.Bool(row["IsLandscape"].ToString());
            sheet.InternalNote = SIn.String(row["InternalNote"].ToString());
            sheet.Description = SIn.String(row["Description"].ToString());
            sheet.ShowInTerminal = SIn.Byte(row["ShowInTerminal"].ToString());
            sheet.IsWebForm = SIn.Bool(row["IsWebForm"].ToString());
            sheet.IsMultiPage = SIn.Bool(row["IsMultiPage"].ToString());
            sheet.IsDeleted = SIn.Bool(row["IsDeleted"].ToString());
            sheet.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
            sheet.DocNum = SIn.Long(row["DocNum"].ToString());
            sheet.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            sheet.DateTSheetEdited = SIn.DateTime(row["DateTSheetEdited"].ToString());
            sheet.HasMobileLayout = SIn.Bool(row["HasMobileLayout"].ToString());
            sheet.RevID = SIn.Int(row["RevID"].ToString());
            sheet.WebFormSheetID = SIn.Long(row["WebFormSheetID"].ToString());
            retVal.Add(sheet);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Sheet> listSheets, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Sheet";
        var table = new DataTable(tableName);
        table.Columns.Add("SheetNum");
        table.Columns.Add("SheetType");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateTimeSheet");
        table.Columns.Add("FontSize");
        table.Columns.Add("FontName");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("IsLandscape");
        table.Columns.Add("InternalNote");
        table.Columns.Add("Description");
        table.Columns.Add("ShowInTerminal");
        table.Columns.Add("IsWebForm");
        table.Columns.Add("IsMultiPage");
        table.Columns.Add("IsDeleted");
        table.Columns.Add("SheetDefNum");
        table.Columns.Add("DocNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("DateTSheetEdited");
        table.Columns.Add("HasMobileLayout");
        table.Columns.Add("RevID");
        table.Columns.Add("WebFormSheetID");
        foreach (var sheet in listSheets)
            table.Rows.Add(SOut.Long(sheet.SheetNum), SOut.Int((int) sheet.SheetType), SOut.Long(sheet.PatNum), SOut.DateT(sheet.DateTimeSheet, false), SOut.Float(sheet.FontSize), sheet.FontName, SOut.Int(sheet.Width), SOut.Int(sheet.Height), SOut.Bool(sheet.IsLandscape), sheet.InternalNote, sheet.Description, SOut.Byte(sheet.ShowInTerminal), SOut.Bool(sheet.IsWebForm), SOut.Bool(sheet.IsMultiPage), SOut.Bool(sheet.IsDeleted), SOut.Long(sheet.SheetDefNum), SOut.Long(sheet.DocNum), SOut.Long(sheet.ClinicNum), SOut.DateT(sheet.DateTSheetEdited, false), SOut.Bool(sheet.HasMobileLayout), SOut.Int(sheet.RevID), SOut.Long(sheet.WebFormSheetID));
        return table;
    }

    public static long Insert(Sheet sheet)
    {
        return Insert(sheet, false);
    }

    public static long Insert(Sheet sheet, bool useExistingPK)
    {
        var command = "INSERT INTO sheet (";

        command += "SheetType,PatNum,DateTimeSheet,FontSize,FontName,Width,Height,IsLandscape,InternalNote,Description,ShowInTerminal,IsWebForm,IsMultiPage,IsDeleted,SheetDefNum,DocNum,ClinicNum,DateTSheetEdited,HasMobileLayout,RevID,WebFormSheetID) VALUES(";

        command +=
            SOut.Int((int) sheet.SheetType) + ","
                                            + SOut.Long(sheet.PatNum) + ","
                                            + SOut.DateT(sheet.DateTimeSheet) + ","
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
                                            + DbHelper.Now() + ","
                                            + SOut.Bool(sheet.HasMobileLayout) + ","
                                            + SOut.Int(sheet.RevID) + ","
                                            + SOut.Long(sheet.WebFormSheetID) + ")";
        if (sheet.InternalNote == null) sheet.InternalNote = "";
        var paramInternalNote = new OdSqlParameter("paramInternalNote", OdDbType.Text, SOut.StringParam(sheet.InternalNote));
        {
            sheet.SheetNum = Db.NonQ(command, true, "SheetNum", "sheet", paramInternalNote);
        }
        return sheet.SheetNum;
    }

    public static long InsertNoCache(Sheet sheet)
    {
        return InsertNoCache(sheet, false);
    }

    public static long InsertNoCache(Sheet sheet, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sheet (";
        if (isRandomKeys || useExistingPK) command += "SheetNum,";
        command += "SheetType,PatNum,DateTimeSheet,FontSize,FontName,Width,Height,IsLandscape,InternalNote,Description,ShowInTerminal,IsWebForm,IsMultiPage,IsDeleted,SheetDefNum,DocNum,ClinicNum,DateTSheetEdited,HasMobileLayout,RevID,WebFormSheetID) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sheet.SheetNum) + ",";
        command +=
            SOut.Int((int) sheet.SheetType) + ","
                                            + SOut.Long(sheet.PatNum) + ","
                                            + SOut.DateT(sheet.DateTimeSheet) + ","
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
                                            + DbHelper.Now() + ","
                                            + SOut.Bool(sheet.HasMobileLayout) + ","
                                            + SOut.Int(sheet.RevID) + ","
                                            + SOut.Long(sheet.WebFormSheetID) + ")";
        if (sheet.InternalNote == null) sheet.InternalNote = "";
        var paramInternalNote = new OdSqlParameter("paramInternalNote", OdDbType.Text, SOut.StringParam(sheet.InternalNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramInternalNote);
        else
            sheet.SheetNum = Db.NonQ(command, true, "SheetNum", "sheet", paramInternalNote);
        return sheet.SheetNum;
    }

    public static void Update(Sheet sheet)
    {
        var command = "UPDATE sheet SET "
                      + "SheetType       =  " + SOut.Int((int) sheet.SheetType) + ", "
                      + "PatNum          =  " + SOut.Long(sheet.PatNum) + ", "
                      + "DateTimeSheet   =  " + SOut.DateT(sheet.DateTimeSheet) + ", "
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
                      + "DateTSheetEdited=  " + SOut.DateT(sheet.DateTSheetEdited) + ", "
                      + "HasMobileLayout =  " + SOut.Bool(sheet.HasMobileLayout) + ", "
                      + "RevID           =  " + SOut.Int(sheet.RevID) + ", "
                      + "WebFormSheetID  =  " + SOut.Long(sheet.WebFormSheetID) + " "
                      + "WHERE SheetNum = " + SOut.Long(sheet.SheetNum);
        if (sheet.InternalNote == null) sheet.InternalNote = "";
        var paramInternalNote = new OdSqlParameter("paramInternalNote", OdDbType.Text, SOut.StringParam(sheet.InternalNote));
        Db.NonQ(command, paramInternalNote);
    }

    public static bool Update(Sheet sheet, Sheet oldSheet)
    {
        var command = "";
        if (sheet.SheetType != oldSheet.SheetType)
        {
            if (command != "") command += ",";
            command += "SheetType = " + SOut.Int((int) sheet.SheetType) + "";
        }

        if (sheet.PatNum != oldSheet.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(sheet.PatNum) + "";
        }

        if (sheet.DateTimeSheet != oldSheet.DateTimeSheet)
        {
            if (command != "") command += ",";
            command += "DateTimeSheet = " + SOut.DateT(sheet.DateTimeSheet) + "";
        }

        if (sheet.FontSize != oldSheet.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(sheet.FontSize) + "";
        }

        if (sheet.FontName != oldSheet.FontName)
        {
            if (command != "") command += ",";
            command += "FontName = '" + SOut.String(sheet.FontName) + "'";
        }

        if (sheet.Width != oldSheet.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(sheet.Width) + "";
        }

        if (sheet.Height != oldSheet.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(sheet.Height) + "";
        }

        if (sheet.IsLandscape != oldSheet.IsLandscape)
        {
            if (command != "") command += ",";
            command += "IsLandscape = " + SOut.Bool(sheet.IsLandscape) + "";
        }

        if (sheet.InternalNote != oldSheet.InternalNote)
        {
            if (command != "") command += ",";
            command += "InternalNote = " + DbHelper.ParamChar + "paramInternalNote";
        }

        if (sheet.Description != oldSheet.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(sheet.Description) + "'";
        }

        if (sheet.ShowInTerminal != oldSheet.ShowInTerminal)
        {
            if (command != "") command += ",";
            command += "ShowInTerminal = " + SOut.Byte(sheet.ShowInTerminal) + "";
        }

        if (sheet.IsWebForm != oldSheet.IsWebForm)
        {
            if (command != "") command += ",";
            command += "IsWebForm = " + SOut.Bool(sheet.IsWebForm) + "";
        }

        if (sheet.IsMultiPage != oldSheet.IsMultiPage)
        {
            if (command != "") command += ",";
            command += "IsMultiPage = " + SOut.Bool(sheet.IsMultiPage) + "";
        }

        if (sheet.IsDeleted != oldSheet.IsDeleted)
        {
            if (command != "") command += ",";
            command += "IsDeleted = " + SOut.Bool(sheet.IsDeleted) + "";
        }

        if (sheet.SheetDefNum != oldSheet.SheetDefNum)
        {
            if (command != "") command += ",";
            command += "SheetDefNum = " + SOut.Long(sheet.SheetDefNum) + "";
        }

        if (sheet.DocNum != oldSheet.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(sheet.DocNum) + "";
        }

        if (sheet.ClinicNum != oldSheet.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(sheet.ClinicNum) + "";
        }

        if (sheet.DateTSheetEdited != oldSheet.DateTSheetEdited)
        {
            if (command != "") command += ",";
            command += "DateTSheetEdited = " + SOut.DateT(sheet.DateTSheetEdited) + "";
        }

        if (sheet.HasMobileLayout != oldSheet.HasMobileLayout)
        {
            if (command != "") command += ",";
            command += "HasMobileLayout = " + SOut.Bool(sheet.HasMobileLayout) + "";
        }

        if (sheet.RevID != oldSheet.RevID)
        {
            if (command != "") command += ",";
            command += "RevID = " + SOut.Int(sheet.RevID) + "";
        }

        if (sheet.WebFormSheetID != oldSheet.WebFormSheetID)
        {
            if (command != "") command += ",";
            command += "WebFormSheetID = " + SOut.Long(sheet.WebFormSheetID) + "";
        }

        if (command == "") return false;
        if (sheet.InternalNote == null) sheet.InternalNote = "";
        var paramInternalNote = new OdSqlParameter("paramInternalNote", OdDbType.Text, SOut.StringParam(sheet.InternalNote));
        command = "UPDATE sheet SET " + command
                                      + " WHERE SheetNum = " + SOut.Long(sheet.SheetNum);
        Db.NonQ(command, paramInternalNote);
        return true;
    }


    public static bool UpdateComparison(Sheet sheet, Sheet oldSheet)
    {
        if (sheet.SheetType != oldSheet.SheetType) return true;
        if (sheet.PatNum != oldSheet.PatNum) return true;
        if (sheet.DateTimeSheet != oldSheet.DateTimeSheet) return true;
        if (sheet.FontSize != oldSheet.FontSize) return true;
        if (sheet.FontName != oldSheet.FontName) return true;
        if (sheet.Width != oldSheet.Width) return true;
        if (sheet.Height != oldSheet.Height) return true;
        if (sheet.IsLandscape != oldSheet.IsLandscape) return true;
        if (sheet.InternalNote != oldSheet.InternalNote) return true;
        if (sheet.Description != oldSheet.Description) return true;
        if (sheet.ShowInTerminal != oldSheet.ShowInTerminal) return true;
        if (sheet.IsWebForm != oldSheet.IsWebForm) return true;
        if (sheet.IsMultiPage != oldSheet.IsMultiPage) return true;
        if (sheet.IsDeleted != oldSheet.IsDeleted) return true;
        if (sheet.SheetDefNum != oldSheet.SheetDefNum) return true;
        if (sheet.DocNum != oldSheet.DocNum) return true;
        if (sheet.ClinicNum != oldSheet.ClinicNum) return true;
        if (sheet.DateTSheetEdited != oldSheet.DateTSheetEdited) return true;
        if (sheet.HasMobileLayout != oldSheet.HasMobileLayout) return true;
        if (sheet.RevID != oldSheet.RevID) return true;
        if (sheet.WebFormSheetID != oldSheet.WebFormSheetID) return true;
        return false;
    }


    public static void Delete(long sheetNum)
    {
        var command = "DELETE FROM sheet "
                      + "WHERE SheetNum = " + SOut.Long(sheetNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listSheetNums)
    {
        if (listSheetNums == null || listSheetNums.Count == 0) return;
        var command = "DELETE FROM sheet "
                      + "WHERE SheetNum IN(" + string.Join(",", listSheetNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}