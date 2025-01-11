#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ToothGridColCrud
{
    public static ToothGridCol SelectOne(long toothGridColNum)
    {
        var command = "SELECT * FROM toothgridcol "
                      + "WHERE ToothGridColNum = " + SOut.Long(toothGridColNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ToothGridCol SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ToothGridCol> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToothGridCol> TableToList(DataTable table)
    {
        var retVal = new List<ToothGridCol>();
        ToothGridCol toothGridCol;
        foreach (DataRow row in table.Rows)
        {
            toothGridCol = new ToothGridCol();
            toothGridCol.ToothGridColNum = SIn.Long(row["ToothGridColNum"].ToString());
            toothGridCol.SheetFieldNum = SIn.Long(row["SheetFieldNum"].ToString());
            toothGridCol.NameItem = SIn.String(row["NameItem"].ToString());
            toothGridCol.CellType = (ToothGridCellType) SIn.Int(row["CellType"].ToString());
            toothGridCol.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            toothGridCol.ColumnWidth = SIn.Int(row["ColumnWidth"].ToString());
            toothGridCol.CodeNum = SIn.Long(row["CodeNum"].ToString());
            toothGridCol.ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString());
            retVal.Add(toothGridCol);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ToothGridCol> listToothGridCols, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ToothGridCol";
        var table = new DataTable(tableName);
        table.Columns.Add("ToothGridColNum");
        table.Columns.Add("SheetFieldNum");
        table.Columns.Add("NameItem");
        table.Columns.Add("CellType");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("ColumnWidth");
        table.Columns.Add("CodeNum");
        table.Columns.Add("ProcStatus");
        foreach (var toothGridCol in listToothGridCols)
            table.Rows.Add(SOut.Long(toothGridCol.ToothGridColNum), SOut.Long(toothGridCol.SheetFieldNum), toothGridCol.NameItem, SOut.Int((int) toothGridCol.CellType), SOut.Int(toothGridCol.ItemOrder), SOut.Int(toothGridCol.ColumnWidth), SOut.Long(toothGridCol.CodeNum), SOut.Int((int) toothGridCol.ProcStatus));
        return table;
    }

    public static long Insert(ToothGridCol toothGridCol)
    {
        return Insert(toothGridCol, false);
    }

    public static long Insert(ToothGridCol toothGridCol, bool useExistingPK)
    {
        var command = "INSERT INTO toothgridcol (";

        command += "SheetFieldNum,NameItem,CellType,ItemOrder,ColumnWidth,CodeNum,ProcStatus) VALUES(";

        command +=
            SOut.Long(toothGridCol.SheetFieldNum) + ","
                                                  + "'" + SOut.String(toothGridCol.NameItem) + "',"
                                                  + SOut.Int((int) toothGridCol.CellType) + ","
                                                  + SOut.Int(toothGridCol.ItemOrder) + ","
                                                  + SOut.Int(toothGridCol.ColumnWidth) + ","
                                                  + SOut.Long(toothGridCol.CodeNum) + ","
                                                  + SOut.Int((int) toothGridCol.ProcStatus) + ")";
        {
            toothGridCol.ToothGridColNum = Db.NonQ(command, true, "ToothGridColNum", "toothGridCol");
        }
        return toothGridCol.ToothGridColNum;
    }

    public static long InsertNoCache(ToothGridCol toothGridCol)
    {
        return InsertNoCache(toothGridCol, false);
    }

    public static long InsertNoCache(ToothGridCol toothGridCol, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO toothgridcol (";
        if (isRandomKeys || useExistingPK) command += "ToothGridColNum,";
        command += "SheetFieldNum,NameItem,CellType,ItemOrder,ColumnWidth,CodeNum,ProcStatus) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(toothGridCol.ToothGridColNum) + ",";
        command +=
            SOut.Long(toothGridCol.SheetFieldNum) + ","
                                                  + "'" + SOut.String(toothGridCol.NameItem) + "',"
                                                  + SOut.Int((int) toothGridCol.CellType) + ","
                                                  + SOut.Int(toothGridCol.ItemOrder) + ","
                                                  + SOut.Int(toothGridCol.ColumnWidth) + ","
                                                  + SOut.Long(toothGridCol.CodeNum) + ","
                                                  + SOut.Int((int) toothGridCol.ProcStatus) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            toothGridCol.ToothGridColNum = Db.NonQ(command, true, "ToothGridColNum", "toothGridCol");
        return toothGridCol.ToothGridColNum;
    }

    public static void Update(ToothGridCol toothGridCol)
    {
        var command = "UPDATE toothgridcol SET "
                      + "SheetFieldNum  =  " + SOut.Long(toothGridCol.SheetFieldNum) + ", "
                      + "NameItem       = '" + SOut.String(toothGridCol.NameItem) + "', "
                      + "CellType       =  " + SOut.Int((int) toothGridCol.CellType) + ", "
                      + "ItemOrder      =  " + SOut.Int(toothGridCol.ItemOrder) + ", "
                      + "ColumnWidth    =  " + SOut.Int(toothGridCol.ColumnWidth) + ", "
                      + "CodeNum        =  " + SOut.Long(toothGridCol.CodeNum) + ", "
                      + "ProcStatus     =  " + SOut.Int((int) toothGridCol.ProcStatus) + " "
                      + "WHERE ToothGridColNum = " + SOut.Long(toothGridCol.ToothGridColNum);
        Db.NonQ(command);
    }

    public static bool Update(ToothGridCol toothGridCol, ToothGridCol oldToothGridCol)
    {
        var command = "";
        if (toothGridCol.SheetFieldNum != oldToothGridCol.SheetFieldNum)
        {
            if (command != "") command += ",";
            command += "SheetFieldNum = " + SOut.Long(toothGridCol.SheetFieldNum) + "";
        }

        if (toothGridCol.NameItem != oldToothGridCol.NameItem)
        {
            if (command != "") command += ",";
            command += "NameItem = '" + SOut.String(toothGridCol.NameItem) + "'";
        }

        if (toothGridCol.CellType != oldToothGridCol.CellType)
        {
            if (command != "") command += ",";
            command += "CellType = " + SOut.Int((int) toothGridCol.CellType) + "";
        }

        if (toothGridCol.ItemOrder != oldToothGridCol.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(toothGridCol.ItemOrder) + "";
        }

        if (toothGridCol.ColumnWidth != oldToothGridCol.ColumnWidth)
        {
            if (command != "") command += ",";
            command += "ColumnWidth = " + SOut.Int(toothGridCol.ColumnWidth) + "";
        }

        if (toothGridCol.CodeNum != oldToothGridCol.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(toothGridCol.CodeNum) + "";
        }

        if (toothGridCol.ProcStatus != oldToothGridCol.ProcStatus)
        {
            if (command != "") command += ",";
            command += "ProcStatus = " + SOut.Int((int) toothGridCol.ProcStatus) + "";
        }

        if (command == "") return false;
        command = "UPDATE toothgridcol SET " + command
                                             + " WHERE ToothGridColNum = " + SOut.Long(toothGridCol.ToothGridColNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ToothGridCol toothGridCol, ToothGridCol oldToothGridCol)
    {
        if (toothGridCol.SheetFieldNum != oldToothGridCol.SheetFieldNum) return true;
        if (toothGridCol.NameItem != oldToothGridCol.NameItem) return true;
        if (toothGridCol.CellType != oldToothGridCol.CellType) return true;
        if (toothGridCol.ItemOrder != oldToothGridCol.ItemOrder) return true;
        if (toothGridCol.ColumnWidth != oldToothGridCol.ColumnWidth) return true;
        if (toothGridCol.CodeNum != oldToothGridCol.CodeNum) return true;
        if (toothGridCol.ProcStatus != oldToothGridCol.ProcStatus) return true;
        return false;
    }

    public static void Delete(long toothGridColNum)
    {
        var command = "DELETE FROM toothgridcol "
                      + "WHERE ToothGridColNum = " + SOut.Long(toothGridColNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listToothGridColNums)
    {
        if (listToothGridColNums == null || listToothGridColNums.Count == 0) return;
        var command = "DELETE FROM toothgridcol "
                      + "WHERE ToothGridColNum IN(" + string.Join(",", listToothGridColNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}