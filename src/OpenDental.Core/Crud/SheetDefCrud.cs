#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SheetDefCrud
{
    public static SheetDef SelectOne(long sheetDefNum)
    {
        var command = "SELECT * FROM sheetdef "
                      + "WHERE SheetDefNum = " + SOut.Long(sheetDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SheetDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SheetDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SheetDef> TableToList(DataTable table)
    {
        var retVal = new List<SheetDef>();
        SheetDef sheetDef;
        foreach (DataRow row in table.Rows)
        {
            sheetDef = new SheetDef();
            sheetDef.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
            sheetDef.Description = SIn.String(row["Description"].ToString());
            sheetDef.SheetType = (SheetTypeEnum) SIn.Int(row["SheetType"].ToString());
            sheetDef.FontSize = SIn.Float(row["FontSize"].ToString());
            sheetDef.FontName = SIn.String(row["FontName"].ToString());
            sheetDef.Width = SIn.Int(row["Width"].ToString());
            sheetDef.Height = SIn.Int(row["Height"].ToString());
            sheetDef.IsLandscape = SIn.Bool(row["IsLandscape"].ToString());
            sheetDef.PageCount = SIn.Int(row["PageCount"].ToString());
            sheetDef.IsMultiPage = SIn.Bool(row["IsMultiPage"].ToString());
            sheetDef.BypassGlobalLock = (BypassLockStatus) SIn.Int(row["BypassGlobalLock"].ToString());
            sheetDef.HasMobileLayout = SIn.Bool(row["HasMobileLayout"].ToString());
            sheetDef.DateTCreated = SIn.DateTime(row["DateTCreated"].ToString());
            sheetDef.RevID = SIn.Int(row["RevID"].ToString());
            sheetDef.AutoCheckSaveImage = SIn.Bool(row["AutoCheckSaveImage"].ToString());
            sheetDef.AutoCheckSaveImageDocCategory = SIn.Long(row["AutoCheckSaveImageDocCategory"].ToString());
            retVal.Add(sheetDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SheetDef> listSheetDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SheetDef";
        var table = new DataTable(tableName);
        table.Columns.Add("SheetDefNum");
        table.Columns.Add("Description");
        table.Columns.Add("SheetType");
        table.Columns.Add("FontSize");
        table.Columns.Add("FontName");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("IsLandscape");
        table.Columns.Add("PageCount");
        table.Columns.Add("IsMultiPage");
        table.Columns.Add("BypassGlobalLock");
        table.Columns.Add("HasMobileLayout");
        table.Columns.Add("DateTCreated");
        table.Columns.Add("RevID");
        table.Columns.Add("AutoCheckSaveImage");
        table.Columns.Add("AutoCheckSaveImageDocCategory");
        foreach (var sheetDef in listSheetDefs)
            table.Rows.Add(SOut.Long(sheetDef.SheetDefNum), sheetDef.Description, SOut.Int((int) sheetDef.SheetType), SOut.Float(sheetDef.FontSize), sheetDef.FontName, SOut.Int(sheetDef.Width), SOut.Int(sheetDef.Height), SOut.Bool(sheetDef.IsLandscape), SOut.Int(sheetDef.PageCount), SOut.Bool(sheetDef.IsMultiPage), SOut.Int((int) sheetDef.BypassGlobalLock), SOut.Bool(sheetDef.HasMobileLayout), SOut.DateT(sheetDef.DateTCreated, false), SOut.Int(sheetDef.RevID), SOut.Bool(sheetDef.AutoCheckSaveImage), SOut.Long(sheetDef.AutoCheckSaveImageDocCategory));
        return table;
    }

    public static long Insert(SheetDef sheetDef)
    {
        return Insert(sheetDef, false);
    }

    public static long Insert(SheetDef sheetDef, bool useExistingPK)
    {
        var command = "INSERT INTO sheetdef (";

        command += "Description,SheetType,FontSize,FontName,Width,Height,IsLandscape,PageCount,IsMultiPage,BypassGlobalLock,HasMobileLayout,DateTCreated,RevID,AutoCheckSaveImage,AutoCheckSaveImageDocCategory) VALUES(";

        command +=
            "'" + SOut.String(sheetDef.Description) + "',"
            + SOut.Int((int) sheetDef.SheetType) + ","
            + SOut.Float(sheetDef.FontSize) + ","
            + "'" + SOut.String(sheetDef.FontName) + "',"
            + SOut.Int(sheetDef.Width) + ","
            + SOut.Int(sheetDef.Height) + ","
            + SOut.Bool(sheetDef.IsLandscape) + ","
            + SOut.Int(sheetDef.PageCount) + ","
            + SOut.Bool(sheetDef.IsMultiPage) + ","
            + SOut.Int((int) sheetDef.BypassGlobalLock) + ","
            + SOut.Bool(sheetDef.HasMobileLayout) + ","
            + SOut.DateT(sheetDef.DateTCreated) + ","
            + SOut.Int(sheetDef.RevID) + ","
            + SOut.Bool(sheetDef.AutoCheckSaveImage) + ","
            + SOut.Long(sheetDef.AutoCheckSaveImageDocCategory) + ")";
        {
            sheetDef.SheetDefNum = Db.NonQ(command, true, "SheetDefNum", "sheetDef");
        }
        return sheetDef.SheetDefNum;
    }

    public static long InsertNoCache(SheetDef sheetDef)
    {
        return InsertNoCache(sheetDef, false);
    }

    public static long InsertNoCache(SheetDef sheetDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sheetdef (";
        if (isRandomKeys || useExistingPK) command += "SheetDefNum,";
        command += "Description,SheetType,FontSize,FontName,Width,Height,IsLandscape,PageCount,IsMultiPage,BypassGlobalLock,HasMobileLayout,DateTCreated,RevID,AutoCheckSaveImage,AutoCheckSaveImageDocCategory) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sheetDef.SheetDefNum) + ",";
        command +=
            "'" + SOut.String(sheetDef.Description) + "',"
            + SOut.Int((int) sheetDef.SheetType) + ","
            + SOut.Float(sheetDef.FontSize) + ","
            + "'" + SOut.String(sheetDef.FontName) + "',"
            + SOut.Int(sheetDef.Width) + ","
            + SOut.Int(sheetDef.Height) + ","
            + SOut.Bool(sheetDef.IsLandscape) + ","
            + SOut.Int(sheetDef.PageCount) + ","
            + SOut.Bool(sheetDef.IsMultiPage) + ","
            + SOut.Int((int) sheetDef.BypassGlobalLock) + ","
            + SOut.Bool(sheetDef.HasMobileLayout) + ","
            + SOut.DateT(sheetDef.DateTCreated) + ","
            + SOut.Int(sheetDef.RevID) + ","
            + SOut.Bool(sheetDef.AutoCheckSaveImage) + ","
            + SOut.Long(sheetDef.AutoCheckSaveImageDocCategory) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            sheetDef.SheetDefNum = Db.NonQ(command, true, "SheetDefNum", "sheetDef");
        return sheetDef.SheetDefNum;
    }

    public static void Update(SheetDef sheetDef)
    {
        var command = "UPDATE sheetdef SET "
                      + "Description                  = '" + SOut.String(sheetDef.Description) + "', "
                      + "SheetType                    =  " + SOut.Int((int) sheetDef.SheetType) + ", "
                      + "FontSize                     =  " + SOut.Float(sheetDef.FontSize) + ", "
                      + "FontName                     = '" + SOut.String(sheetDef.FontName) + "', "
                      + "Width                        =  " + SOut.Int(sheetDef.Width) + ", "
                      + "Height                       =  " + SOut.Int(sheetDef.Height) + ", "
                      + "IsLandscape                  =  " + SOut.Bool(sheetDef.IsLandscape) + ", "
                      + "PageCount                    =  " + SOut.Int(sheetDef.PageCount) + ", "
                      + "IsMultiPage                  =  " + SOut.Bool(sheetDef.IsMultiPage) + ", "
                      + "BypassGlobalLock             =  " + SOut.Int((int) sheetDef.BypassGlobalLock) + ", "
                      + "HasMobileLayout              =  " + SOut.Bool(sheetDef.HasMobileLayout) + ", "
                      + "DateTCreated                 =  " + SOut.DateT(sheetDef.DateTCreated) + ", "
                      + "RevID                        =  " + SOut.Int(sheetDef.RevID) + ", "
                      + "AutoCheckSaveImage           =  " + SOut.Bool(sheetDef.AutoCheckSaveImage) + ", "
                      + "AutoCheckSaveImageDocCategory=  " + SOut.Long(sheetDef.AutoCheckSaveImageDocCategory) + " "
                      + "WHERE SheetDefNum = " + SOut.Long(sheetDef.SheetDefNum);
        Db.NonQ(command);
    }

    public static bool Update(SheetDef sheetDef, SheetDef oldSheetDef)
    {
        var command = "";
        if (sheetDef.Description != oldSheetDef.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(sheetDef.Description) + "'";
        }

        if (sheetDef.SheetType != oldSheetDef.SheetType)
        {
            if (command != "") command += ",";
            command += "SheetType = " + SOut.Int((int) sheetDef.SheetType) + "";
        }

        if (sheetDef.FontSize != oldSheetDef.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(sheetDef.FontSize) + "";
        }

        if (sheetDef.FontName != oldSheetDef.FontName)
        {
            if (command != "") command += ",";
            command += "FontName = '" + SOut.String(sheetDef.FontName) + "'";
        }

        if (sheetDef.Width != oldSheetDef.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(sheetDef.Width) + "";
        }

        if (sheetDef.Height != oldSheetDef.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(sheetDef.Height) + "";
        }

        if (sheetDef.IsLandscape != oldSheetDef.IsLandscape)
        {
            if (command != "") command += ",";
            command += "IsLandscape = " + SOut.Bool(sheetDef.IsLandscape) + "";
        }

        if (sheetDef.PageCount != oldSheetDef.PageCount)
        {
            if (command != "") command += ",";
            command += "PageCount = " + SOut.Int(sheetDef.PageCount) + "";
        }

        if (sheetDef.IsMultiPage != oldSheetDef.IsMultiPage)
        {
            if (command != "") command += ",";
            command += "IsMultiPage = " + SOut.Bool(sheetDef.IsMultiPage) + "";
        }

        if (sheetDef.BypassGlobalLock != oldSheetDef.BypassGlobalLock)
        {
            if (command != "") command += ",";
            command += "BypassGlobalLock = " + SOut.Int((int) sheetDef.BypassGlobalLock) + "";
        }

        if (sheetDef.HasMobileLayout != oldSheetDef.HasMobileLayout)
        {
            if (command != "") command += ",";
            command += "HasMobileLayout = " + SOut.Bool(sheetDef.HasMobileLayout) + "";
        }

        if (sheetDef.DateTCreated != oldSheetDef.DateTCreated)
        {
            if (command != "") command += ",";
            command += "DateTCreated = " + SOut.DateT(sheetDef.DateTCreated) + "";
        }

        if (sheetDef.RevID != oldSheetDef.RevID)
        {
            if (command != "") command += ",";
            command += "RevID = " + SOut.Int(sheetDef.RevID) + "";
        }

        if (sheetDef.AutoCheckSaveImage != oldSheetDef.AutoCheckSaveImage)
        {
            if (command != "") command += ",";
            command += "AutoCheckSaveImage = " + SOut.Bool(sheetDef.AutoCheckSaveImage) + "";
        }

        if (sheetDef.AutoCheckSaveImageDocCategory != oldSheetDef.AutoCheckSaveImageDocCategory)
        {
            if (command != "") command += ",";
            command += "AutoCheckSaveImageDocCategory = " + SOut.Long(sheetDef.AutoCheckSaveImageDocCategory) + "";
        }

        if (command == "") return false;
        command = "UPDATE sheetdef SET " + command
                                         + " WHERE SheetDefNum = " + SOut.Long(sheetDef.SheetDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SheetDef sheetDef, SheetDef oldSheetDef)
    {
        if (sheetDef.Description != oldSheetDef.Description) return true;
        if (sheetDef.SheetType != oldSheetDef.SheetType) return true;
        if (sheetDef.FontSize != oldSheetDef.FontSize) return true;
        if (sheetDef.FontName != oldSheetDef.FontName) return true;
        if (sheetDef.Width != oldSheetDef.Width) return true;
        if (sheetDef.Height != oldSheetDef.Height) return true;
        if (sheetDef.IsLandscape != oldSheetDef.IsLandscape) return true;
        if (sheetDef.PageCount != oldSheetDef.PageCount) return true;
        if (sheetDef.IsMultiPage != oldSheetDef.IsMultiPage) return true;
        if (sheetDef.BypassGlobalLock != oldSheetDef.BypassGlobalLock) return true;
        if (sheetDef.HasMobileLayout != oldSheetDef.HasMobileLayout) return true;
        if (sheetDef.DateTCreated != oldSheetDef.DateTCreated) return true;
        if (sheetDef.RevID != oldSheetDef.RevID) return true;
        if (sheetDef.AutoCheckSaveImage != oldSheetDef.AutoCheckSaveImage) return true;
        if (sheetDef.AutoCheckSaveImageDocCategory != oldSheetDef.AutoCheckSaveImageDocCategory) return true;
        return false;
    }

    public static void Delete(long sheetDefNum)
    {
        var command = "DELETE FROM sheetdef "
                      + "WHERE SheetDefNum = " + SOut.Long(sheetDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSheetDefNums)
    {
        if (listSheetDefNums == null || listSheetDefNums.Count == 0) return;
        var command = "DELETE FROM sheetdef "
                      + "WHERE SheetDefNum IN(" + string.Join(",", listSheetDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}