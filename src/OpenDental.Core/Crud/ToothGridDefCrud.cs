#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ToothGridDefCrud
{
    public static ToothGridDef SelectOne(long toothGridDefNum)
    {
        var command = "SELECT * FROM toothgriddef "
                      + "WHERE ToothGridDefNum = " + SOut.Long(toothGridDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ToothGridDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ToothGridDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToothGridDef> TableToList(DataTable table)
    {
        var retVal = new List<ToothGridDef>();
        ToothGridDef toothGridDef;
        foreach (DataRow row in table.Rows)
        {
            toothGridDef = new ToothGridDef();
            toothGridDef.ToothGridDefNum = SIn.Long(row["ToothGridDefNum"].ToString());
            toothGridDef.SheetFieldDefNum = SIn.Long(row["SheetFieldDefNum"].ToString());
            toothGridDef.NameInternal = SIn.String(row["NameInternal"].ToString());
            toothGridDef.NameShowing = SIn.String(row["NameShowing"].ToString());
            toothGridDef.CellType = (ToothGridCellType) SIn.Int(row["CellType"].ToString());
            toothGridDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            toothGridDef.ColumnWidth = SIn.Int(row["ColumnWidth"].ToString());
            toothGridDef.CodeNum = SIn.Long(row["CodeNum"].ToString());
            toothGridDef.ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString());
            retVal.Add(toothGridDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ToothGridDef> listToothGridDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ToothGridDef";
        var table = new DataTable(tableName);
        table.Columns.Add("ToothGridDefNum");
        table.Columns.Add("SheetFieldDefNum");
        table.Columns.Add("NameInternal");
        table.Columns.Add("NameShowing");
        table.Columns.Add("CellType");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("ColumnWidth");
        table.Columns.Add("CodeNum");
        table.Columns.Add("ProcStatus");
        foreach (var toothGridDef in listToothGridDefs)
            table.Rows.Add(SOut.Long(toothGridDef.ToothGridDefNum), SOut.Long(toothGridDef.SheetFieldDefNum), toothGridDef.NameInternal, toothGridDef.NameShowing, SOut.Int((int) toothGridDef.CellType), SOut.Int(toothGridDef.ItemOrder), SOut.Int(toothGridDef.ColumnWidth), SOut.Long(toothGridDef.CodeNum), SOut.Int((int) toothGridDef.ProcStatus));
        return table;
    }

    public static long Insert(ToothGridDef toothGridDef)
    {
        return Insert(toothGridDef, false);
    }

    public static long Insert(ToothGridDef toothGridDef, bool useExistingPK)
    {
        var command = "INSERT INTO toothgriddef (";

        command += "SheetFieldDefNum,NameInternal,NameShowing,CellType,ItemOrder,ColumnWidth,CodeNum,ProcStatus) VALUES(";

        command +=
            SOut.Long(toothGridDef.SheetFieldDefNum) + ","
                                                     + "'" + SOut.String(toothGridDef.NameInternal) + "',"
                                                     + "'" + SOut.String(toothGridDef.NameShowing) + "',"
                                                     + SOut.Int((int) toothGridDef.CellType) + ","
                                                     + SOut.Int(toothGridDef.ItemOrder) + ","
                                                     + SOut.Int(toothGridDef.ColumnWidth) + ","
                                                     + SOut.Long(toothGridDef.CodeNum) + ","
                                                     + SOut.Int((int) toothGridDef.ProcStatus) + ")";
        {
            toothGridDef.ToothGridDefNum = Db.NonQ(command, true, "ToothGridDefNum", "toothGridDef");
        }
        return toothGridDef.ToothGridDefNum;
    }

    public static long InsertNoCache(ToothGridDef toothGridDef)
    {
        return InsertNoCache(toothGridDef, false);
    }

    public static long InsertNoCache(ToothGridDef toothGridDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO toothgriddef (";
        if (isRandomKeys || useExistingPK) command += "ToothGridDefNum,";
        command += "SheetFieldDefNum,NameInternal,NameShowing,CellType,ItemOrder,ColumnWidth,CodeNum,ProcStatus) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(toothGridDef.ToothGridDefNum) + ",";
        command +=
            SOut.Long(toothGridDef.SheetFieldDefNum) + ","
                                                     + "'" + SOut.String(toothGridDef.NameInternal) + "',"
                                                     + "'" + SOut.String(toothGridDef.NameShowing) + "',"
                                                     + SOut.Int((int) toothGridDef.CellType) + ","
                                                     + SOut.Int(toothGridDef.ItemOrder) + ","
                                                     + SOut.Int(toothGridDef.ColumnWidth) + ","
                                                     + SOut.Long(toothGridDef.CodeNum) + ","
                                                     + SOut.Int((int) toothGridDef.ProcStatus) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            toothGridDef.ToothGridDefNum = Db.NonQ(command, true, "ToothGridDefNum", "toothGridDef");
        return toothGridDef.ToothGridDefNum;
    }

    public static void Update(ToothGridDef toothGridDef)
    {
        var command = "UPDATE toothgriddef SET "
                      + "SheetFieldDefNum=  " + SOut.Long(toothGridDef.SheetFieldDefNum) + ", "
                      + "NameInternal    = '" + SOut.String(toothGridDef.NameInternal) + "', "
                      + "NameShowing     = '" + SOut.String(toothGridDef.NameShowing) + "', "
                      + "CellType        =  " + SOut.Int((int) toothGridDef.CellType) + ", "
                      + "ItemOrder       =  " + SOut.Int(toothGridDef.ItemOrder) + ", "
                      + "ColumnWidth     =  " + SOut.Int(toothGridDef.ColumnWidth) + ", "
                      + "CodeNum         =  " + SOut.Long(toothGridDef.CodeNum) + ", "
                      + "ProcStatus      =  " + SOut.Int((int) toothGridDef.ProcStatus) + " "
                      + "WHERE ToothGridDefNum = " + SOut.Long(toothGridDef.ToothGridDefNum);
        Db.NonQ(command);
    }

    public static bool Update(ToothGridDef toothGridDef, ToothGridDef oldToothGridDef)
    {
        var command = "";
        if (toothGridDef.SheetFieldDefNum != oldToothGridDef.SheetFieldDefNum)
        {
            if (command != "") command += ",";
            command += "SheetFieldDefNum = " + SOut.Long(toothGridDef.SheetFieldDefNum) + "";
        }

        if (toothGridDef.NameInternal != oldToothGridDef.NameInternal)
        {
            if (command != "") command += ",";
            command += "NameInternal = '" + SOut.String(toothGridDef.NameInternal) + "'";
        }

        if (toothGridDef.NameShowing != oldToothGridDef.NameShowing)
        {
            if (command != "") command += ",";
            command += "NameShowing = '" + SOut.String(toothGridDef.NameShowing) + "'";
        }

        if (toothGridDef.CellType != oldToothGridDef.CellType)
        {
            if (command != "") command += ",";
            command += "CellType = " + SOut.Int((int) toothGridDef.CellType) + "";
        }

        if (toothGridDef.ItemOrder != oldToothGridDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(toothGridDef.ItemOrder) + "";
        }

        if (toothGridDef.ColumnWidth != oldToothGridDef.ColumnWidth)
        {
            if (command != "") command += ",";
            command += "ColumnWidth = " + SOut.Int(toothGridDef.ColumnWidth) + "";
        }

        if (toothGridDef.CodeNum != oldToothGridDef.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(toothGridDef.CodeNum) + "";
        }

        if (toothGridDef.ProcStatus != oldToothGridDef.ProcStatus)
        {
            if (command != "") command += ",";
            command += "ProcStatus = " + SOut.Int((int) toothGridDef.ProcStatus) + "";
        }

        if (command == "") return false;
        command = "UPDATE toothgriddef SET " + command
                                             + " WHERE ToothGridDefNum = " + SOut.Long(toothGridDef.ToothGridDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ToothGridDef toothGridDef, ToothGridDef oldToothGridDef)
    {
        if (toothGridDef.SheetFieldDefNum != oldToothGridDef.SheetFieldDefNum) return true;
        if (toothGridDef.NameInternal != oldToothGridDef.NameInternal) return true;
        if (toothGridDef.NameShowing != oldToothGridDef.NameShowing) return true;
        if (toothGridDef.CellType != oldToothGridDef.CellType) return true;
        if (toothGridDef.ItemOrder != oldToothGridDef.ItemOrder) return true;
        if (toothGridDef.ColumnWidth != oldToothGridDef.ColumnWidth) return true;
        if (toothGridDef.CodeNum != oldToothGridDef.CodeNum) return true;
        if (toothGridDef.ProcStatus != oldToothGridDef.ProcStatus) return true;
        return false;
    }

    public static void Delete(long toothGridDefNum)
    {
        var command = "DELETE FROM toothgriddef "
                      + "WHERE ToothGridDefNum = " + SOut.Long(toothGridDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listToothGridDefNums)
    {
        if (listToothGridDefNums == null || listToothGridDefNums.Count == 0) return;
        var command = "DELETE FROM toothgriddef "
                      + "WHERE ToothGridDefNum IN(" + string.Join(",", listToothGridDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}