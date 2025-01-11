#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ToothGridCellCrud
{
    public static ToothGridCell SelectOne(long toothGridCellNum)
    {
        var command = "SELECT * FROM toothgridcell "
                      + "WHERE ToothGridCellNum = " + SOut.Long(toothGridCellNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ToothGridCell SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ToothGridCell> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToothGridCell> TableToList(DataTable table)
    {
        var retVal = new List<ToothGridCell>();
        ToothGridCell toothGridCell;
        foreach (DataRow row in table.Rows)
        {
            toothGridCell = new ToothGridCell();
            toothGridCell.ToothGridCellNum = SIn.Long(row["ToothGridCellNum"].ToString());
            toothGridCell.SheetFieldNum = SIn.Long(row["SheetFieldNum"].ToString());
            toothGridCell.ToothGridColNum = SIn.Long(row["ToothGridColNum"].ToString());
            toothGridCell.ValueEntered = SIn.String(row["ValueEntered"].ToString());
            toothGridCell.ToothNum = SIn.String(row["ToothNum"].ToString());
            retVal.Add(toothGridCell);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ToothGridCell> listToothGridCells, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ToothGridCell";
        var table = new DataTable(tableName);
        table.Columns.Add("ToothGridCellNum");
        table.Columns.Add("SheetFieldNum");
        table.Columns.Add("ToothGridColNum");
        table.Columns.Add("ValueEntered");
        table.Columns.Add("ToothNum");
        foreach (var toothGridCell in listToothGridCells)
            table.Rows.Add(SOut.Long(toothGridCell.ToothGridCellNum), SOut.Long(toothGridCell.SheetFieldNum), SOut.Long(toothGridCell.ToothGridColNum), toothGridCell.ValueEntered, toothGridCell.ToothNum);
        return table;
    }

    public static long Insert(ToothGridCell toothGridCell)
    {
        return Insert(toothGridCell, false);
    }

    public static long Insert(ToothGridCell toothGridCell, bool useExistingPK)
    {
        var command = "INSERT INTO toothgridcell (";

        command += "SheetFieldNum,ToothGridColNum,ValueEntered,ToothNum) VALUES(";

        command +=
            SOut.Long(toothGridCell.SheetFieldNum) + ","
                                                   + SOut.Long(toothGridCell.ToothGridColNum) + ","
                                                   + "'" + SOut.String(toothGridCell.ValueEntered) + "',"
                                                   + "'" + SOut.String(toothGridCell.ToothNum) + "')";
        {
            toothGridCell.ToothGridCellNum = Db.NonQ(command, true, "ToothGridCellNum", "toothGridCell");
        }
        return toothGridCell.ToothGridCellNum;
    }

    public static long InsertNoCache(ToothGridCell toothGridCell)
    {
        return InsertNoCache(toothGridCell, false);
    }

    public static long InsertNoCache(ToothGridCell toothGridCell, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO toothgridcell (";
        if (isRandomKeys || useExistingPK) command += "ToothGridCellNum,";
        command += "SheetFieldNum,ToothGridColNum,ValueEntered,ToothNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(toothGridCell.ToothGridCellNum) + ",";
        command +=
            SOut.Long(toothGridCell.SheetFieldNum) + ","
                                                   + SOut.Long(toothGridCell.ToothGridColNum) + ","
                                                   + "'" + SOut.String(toothGridCell.ValueEntered) + "',"
                                                   + "'" + SOut.String(toothGridCell.ToothNum) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            toothGridCell.ToothGridCellNum = Db.NonQ(command, true, "ToothGridCellNum", "toothGridCell");
        return toothGridCell.ToothGridCellNum;
    }

    public static void Update(ToothGridCell toothGridCell)
    {
        var command = "UPDATE toothgridcell SET "
                      + "SheetFieldNum   =  " + SOut.Long(toothGridCell.SheetFieldNum) + ", "
                      + "ToothGridColNum =  " + SOut.Long(toothGridCell.ToothGridColNum) + ", "
                      + "ValueEntered    = '" + SOut.String(toothGridCell.ValueEntered) + "', "
                      + "ToothNum        = '" + SOut.String(toothGridCell.ToothNum) + "' "
                      + "WHERE ToothGridCellNum = " + SOut.Long(toothGridCell.ToothGridCellNum);
        Db.NonQ(command);
    }

    public static bool Update(ToothGridCell toothGridCell, ToothGridCell oldToothGridCell)
    {
        var command = "";
        if (toothGridCell.SheetFieldNum != oldToothGridCell.SheetFieldNum)
        {
            if (command != "") command += ",";
            command += "SheetFieldNum = " + SOut.Long(toothGridCell.SheetFieldNum) + "";
        }

        if (toothGridCell.ToothGridColNum != oldToothGridCell.ToothGridColNum)
        {
            if (command != "") command += ",";
            command += "ToothGridColNum = " + SOut.Long(toothGridCell.ToothGridColNum) + "";
        }

        if (toothGridCell.ValueEntered != oldToothGridCell.ValueEntered)
        {
            if (command != "") command += ",";
            command += "ValueEntered = '" + SOut.String(toothGridCell.ValueEntered) + "'";
        }

        if (toothGridCell.ToothNum != oldToothGridCell.ToothNum)
        {
            if (command != "") command += ",";
            command += "ToothNum = '" + SOut.String(toothGridCell.ToothNum) + "'";
        }

        if (command == "") return false;
        command = "UPDATE toothgridcell SET " + command
                                              + " WHERE ToothGridCellNum = " + SOut.Long(toothGridCell.ToothGridCellNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ToothGridCell toothGridCell, ToothGridCell oldToothGridCell)
    {
        if (toothGridCell.SheetFieldNum != oldToothGridCell.SheetFieldNum) return true;
        if (toothGridCell.ToothGridColNum != oldToothGridCell.ToothGridColNum) return true;
        if (toothGridCell.ValueEntered != oldToothGridCell.ValueEntered) return true;
        if (toothGridCell.ToothNum != oldToothGridCell.ToothNum) return true;
        return false;
    }

    public static void Delete(long toothGridCellNum)
    {
        var command = "DELETE FROM toothgridcell "
                      + "WHERE ToothGridCellNum = " + SOut.Long(toothGridCellNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listToothGridCellNums)
    {
        if (listToothGridCellNums == null || listToothGridCellNums.Count == 0) return;
        var command = "DELETE FROM toothgridcell "
                      + "WHERE ToothGridCellNum IN(" + string.Join(",", listToothGridCellNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}