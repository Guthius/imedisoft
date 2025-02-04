using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SheetDefCrud
{
    public static List<SheetDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SheetDef> TableToList(DataTable table)
    {
        var retVal = new List<SheetDef>();
        foreach (DataRow row in table.Rows)
        {
            var sheetDef = new SheetDef
            {
                SheetDefNum = SIn.Long(row["SheetDefNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                SheetType = (SheetTypeEnum) SIn.Int(row["SheetType"].ToString()),
                FontSize = SIn.Float(row["FontSize"].ToString()),
                FontName = SIn.String(row["FontName"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                Height = SIn.Int(row["Height"].ToString()),
                IsLandscape = SIn.Bool(row["IsLandscape"].ToString()),
                PageCount = SIn.Int(row["PageCount"].ToString()),
                IsMultiPage = SIn.Bool(row["IsMultiPage"].ToString()),
                BypassGlobalLock = (BypassLockStatus) SIn.Int(row["BypassGlobalLock"].ToString()),
                HasMobileLayout = SIn.Bool(row["HasMobileLayout"].ToString()),
                DateTCreated = SIn.DateTime(row["DateTCreated"].ToString()),
                RevID = SIn.Int(row["RevID"].ToString()),
                AutoCheckSaveImage = SIn.Bool(row["AutoCheckSaveImage"].ToString()),
                AutoCheckSaveImageDocCategory = SIn.Long(row["AutoCheckSaveImageDocCategory"].ToString())
            };
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
            table.Rows.Add(SOut.Long(sheetDef.SheetDefNum), sheetDef.Description, SOut.Int((int) sheetDef.SheetType), SOut.Float(sheetDef.FontSize), sheetDef.FontName, SOut.Int(sheetDef.Width), SOut.Int(sheetDef.Height), SOut.Bool(sheetDef.IsLandscape), SOut.Int(sheetDef.PageCount), SOut.Bool(sheetDef.IsMultiPage), SOut.Int((int) sheetDef.BypassGlobalLock), SOut.Bool(sheetDef.HasMobileLayout), SOut.DateTime(sheetDef.DateTCreated, false), SOut.Int(sheetDef.RevID), SOut.Bool(sheetDef.AutoCheckSaveImage), SOut.Long(sheetDef.AutoCheckSaveImageDocCategory));
        return table;
    }

    public static long Insert(SheetDef sheetDef)
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
            + SOut.DateTime(sheetDef.DateTCreated) + ","
            + SOut.Int(sheetDef.RevID) + ","
            + SOut.Bool(sheetDef.AutoCheckSaveImage) + ","
            + SOut.Long(sheetDef.AutoCheckSaveImageDocCategory) + ")";
        {
            sheetDef.SheetDefNum = Db.NonQ(command, true, "SheetDefNum", "sheetDef");
        }
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
                      + "DateTCreated                 =  " + SOut.DateTime(sheetDef.DateTCreated) + ", "
                      + "RevID                        =  " + SOut.Int(sheetDef.RevID) + ", "
                      + "AutoCheckSaveImage           =  " + SOut.Bool(sheetDef.AutoCheckSaveImage) + ", "
                      + "AutoCheckSaveImageDocCategory=  " + SOut.Long(sheetDef.AutoCheckSaveImageDocCategory) + " "
                      + "WHERE SheetDefNum = " + SOut.Long(sheetDef.SheetDefNum);
        Db.NonQ(command);
    }

    public static void Delete(long sheetDefNum)
    {
        var command = "DELETE FROM sheetdef "
                      + "WHERE SheetDefNum = " + SOut.Long(sheetDefNum);
        Db.NonQ(command);
    }
}