#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MountDefCrud
{
    public static MountDef SelectOne(long mountDefNum)
    {
        var command = "SELECT * FROM mountdef "
                      + "WHERE MountDefNum = " + SOut.Long(mountDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MountDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MountDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MountDef> TableToList(DataTable table)
    {
        var retVal = new List<MountDef>();
        MountDef mountDef;
        foreach (DataRow row in table.Rows)
        {
            mountDef = new MountDef();
            mountDef.MountDefNum = SIn.Long(row["MountDefNum"].ToString());
            mountDef.Description = SIn.String(row["Description"].ToString());
            mountDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            mountDef.Width = SIn.Int(row["Width"].ToString());
            mountDef.Height = SIn.Int(row["Height"].ToString());
            mountDef.ColorBack = Color.FromArgb(SIn.Int(row["ColorBack"].ToString()));
            mountDef.ColorFore = Color.FromArgb(SIn.Int(row["ColorFore"].ToString()));
            mountDef.ColorTextBack = Color.FromArgb(SIn.Int(row["ColorTextBack"].ToString()));
            mountDef.ScaleValue = SIn.String(row["ScaleValue"].ToString());
            mountDef.DefaultCat = SIn.Long(row["DefaultCat"].ToString());
            mountDef.FlipOnAcquire = SIn.Bool(row["FlipOnAcquire"].ToString());
            mountDef.AdjModeAfterSeries = SIn.Bool(row["AdjModeAfterSeries"].ToString());
            retVal.Add(mountDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MountDef> listMountDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MountDef";
        var table = new DataTable(tableName);
        table.Columns.Add("MountDefNum");
        table.Columns.Add("Description");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("ColorBack");
        table.Columns.Add("ColorFore");
        table.Columns.Add("ColorTextBack");
        table.Columns.Add("ScaleValue");
        table.Columns.Add("DefaultCat");
        table.Columns.Add("FlipOnAcquire");
        table.Columns.Add("AdjModeAfterSeries");
        foreach (var mountDef in listMountDefs)
            table.Rows.Add(SOut.Long(mountDef.MountDefNum), mountDef.Description, SOut.Int(mountDef.ItemOrder), SOut.Int(mountDef.Width), SOut.Int(mountDef.Height), SOut.Int(mountDef.ColorBack.ToArgb()), SOut.Int(mountDef.ColorFore.ToArgb()), SOut.Int(mountDef.ColorTextBack.ToArgb()), mountDef.ScaleValue, SOut.Long(mountDef.DefaultCat), SOut.Bool(mountDef.FlipOnAcquire), SOut.Bool(mountDef.AdjModeAfterSeries));
        return table;
    }

    public static long Insert(MountDef mountDef)
    {
        return Insert(mountDef, false);
    }

    public static long Insert(MountDef mountDef, bool useExistingPK)
    {
        var command = "INSERT INTO mountdef (";

        command += "Description,ItemOrder,Width,Height,ColorBack,ColorFore,ColorTextBack,ScaleValue,DefaultCat,FlipOnAcquire,AdjModeAfterSeries) VALUES(";

        command +=
            "'" + SOut.String(mountDef.Description) + "',"
            + SOut.Int(mountDef.ItemOrder) + ","
            + SOut.Int(mountDef.Width) + ","
            + SOut.Int(mountDef.Height) + ","
            + SOut.Int(mountDef.ColorBack.ToArgb()) + ","
            + SOut.Int(mountDef.ColorFore.ToArgb()) + ","
            + SOut.Int(mountDef.ColorTextBack.ToArgb()) + ","
            + "'" + SOut.String(mountDef.ScaleValue) + "',"
            + SOut.Long(mountDef.DefaultCat) + ","
            + SOut.Bool(mountDef.FlipOnAcquire) + ","
            + SOut.Bool(mountDef.AdjModeAfterSeries) + ")";
        {
            mountDef.MountDefNum = Db.NonQ(command, true, "MountDefNum", "mountDef");
        }
        return mountDef.MountDefNum;
    }

    public static long InsertNoCache(MountDef mountDef)
    {
        return InsertNoCache(mountDef, false);
    }

    public static long InsertNoCache(MountDef mountDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mountdef (";
        if (isRandomKeys || useExistingPK) command += "MountDefNum,";
        command += "Description,ItemOrder,Width,Height,ColorBack,ColorFore,ColorTextBack,ScaleValue,DefaultCat,FlipOnAcquire,AdjModeAfterSeries) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mountDef.MountDefNum) + ",";
        command +=
            "'" + SOut.String(mountDef.Description) + "',"
            + SOut.Int(mountDef.ItemOrder) + ","
            + SOut.Int(mountDef.Width) + ","
            + SOut.Int(mountDef.Height) + ","
            + SOut.Int(mountDef.ColorBack.ToArgb()) + ","
            + SOut.Int(mountDef.ColorFore.ToArgb()) + ","
            + SOut.Int(mountDef.ColorTextBack.ToArgb()) + ","
            + "'" + SOut.String(mountDef.ScaleValue) + "',"
            + SOut.Long(mountDef.DefaultCat) + ","
            + SOut.Bool(mountDef.FlipOnAcquire) + ","
            + SOut.Bool(mountDef.AdjModeAfterSeries) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            mountDef.MountDefNum = Db.NonQ(command, true, "MountDefNum", "mountDef");
        return mountDef.MountDefNum;
    }

    public static void Update(MountDef mountDef)
    {
        var command = "UPDATE mountdef SET "
                      + "Description       = '" + SOut.String(mountDef.Description) + "', "
                      + "ItemOrder         =  " + SOut.Int(mountDef.ItemOrder) + ", "
                      + "Width             =  " + SOut.Int(mountDef.Width) + ", "
                      + "Height            =  " + SOut.Int(mountDef.Height) + ", "
                      + "ColorBack         =  " + SOut.Int(mountDef.ColorBack.ToArgb()) + ", "
                      + "ColorFore         =  " + SOut.Int(mountDef.ColorFore.ToArgb()) + ", "
                      + "ColorTextBack     =  " + SOut.Int(mountDef.ColorTextBack.ToArgb()) + ", "
                      + "ScaleValue        = '" + SOut.String(mountDef.ScaleValue) + "', "
                      + "DefaultCat        =  " + SOut.Long(mountDef.DefaultCat) + ", "
                      + "FlipOnAcquire     =  " + SOut.Bool(mountDef.FlipOnAcquire) + ", "
                      + "AdjModeAfterSeries=  " + SOut.Bool(mountDef.AdjModeAfterSeries) + " "
                      + "WHERE MountDefNum = " + SOut.Long(mountDef.MountDefNum);
        Db.NonQ(command);
    }

    public static bool Update(MountDef mountDef, MountDef oldMountDef)
    {
        var command = "";
        if (mountDef.Description != oldMountDef.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(mountDef.Description) + "'";
        }

        if (mountDef.ItemOrder != oldMountDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(mountDef.ItemOrder) + "";
        }

        if (mountDef.Width != oldMountDef.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(mountDef.Width) + "";
        }

        if (mountDef.Height != oldMountDef.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(mountDef.Height) + "";
        }

        if (mountDef.ColorBack != oldMountDef.ColorBack)
        {
            if (command != "") command += ",";
            command += "ColorBack = " + SOut.Int(mountDef.ColorBack.ToArgb()) + "";
        }

        if (mountDef.ColorFore != oldMountDef.ColorFore)
        {
            if (command != "") command += ",";
            command += "ColorFore = " + SOut.Int(mountDef.ColorFore.ToArgb()) + "";
        }

        if (mountDef.ColorTextBack != oldMountDef.ColorTextBack)
        {
            if (command != "") command += ",";
            command += "ColorTextBack = " + SOut.Int(mountDef.ColorTextBack.ToArgb()) + "";
        }

        if (mountDef.ScaleValue != oldMountDef.ScaleValue)
        {
            if (command != "") command += ",";
            command += "ScaleValue = '" + SOut.String(mountDef.ScaleValue) + "'";
        }

        if (mountDef.DefaultCat != oldMountDef.DefaultCat)
        {
            if (command != "") command += ",";
            command += "DefaultCat = " + SOut.Long(mountDef.DefaultCat) + "";
        }

        if (mountDef.FlipOnAcquire != oldMountDef.FlipOnAcquire)
        {
            if (command != "") command += ",";
            command += "FlipOnAcquire = " + SOut.Bool(mountDef.FlipOnAcquire) + "";
        }

        if (mountDef.AdjModeAfterSeries != oldMountDef.AdjModeAfterSeries)
        {
            if (command != "") command += ",";
            command += "AdjModeAfterSeries = " + SOut.Bool(mountDef.AdjModeAfterSeries) + "";
        }

        if (command == "") return false;
        command = "UPDATE mountdef SET " + command
                                         + " WHERE MountDefNum = " + SOut.Long(mountDef.MountDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MountDef mountDef, MountDef oldMountDef)
    {
        if (mountDef.Description != oldMountDef.Description) return true;
        if (mountDef.ItemOrder != oldMountDef.ItemOrder) return true;
        if (mountDef.Width != oldMountDef.Width) return true;
        if (mountDef.Height != oldMountDef.Height) return true;
        if (mountDef.ColorBack != oldMountDef.ColorBack) return true;
        if (mountDef.ColorFore != oldMountDef.ColorFore) return true;
        if (mountDef.ColorTextBack != oldMountDef.ColorTextBack) return true;
        if (mountDef.ScaleValue != oldMountDef.ScaleValue) return true;
        if (mountDef.DefaultCat != oldMountDef.DefaultCat) return true;
        if (mountDef.FlipOnAcquire != oldMountDef.FlipOnAcquire) return true;
        if (mountDef.AdjModeAfterSeries != oldMountDef.AdjModeAfterSeries) return true;
        return false;
    }

    public static void Delete(long mountDefNum)
    {
        var command = "DELETE FROM mountdef "
                      + "WHERE MountDefNum = " + SOut.Long(mountDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMountDefNums)
    {
        if (listMountDefNums == null || listMountDefNums.Count == 0) return;
        var command = "DELETE FROM mountdef "
                      + "WHERE MountDefNum IN(" + string.Join(",", listMountDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}