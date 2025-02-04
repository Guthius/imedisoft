using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MountDefCrud
{
    public static List<MountDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MountDef> TableToList(DataTable table)
    {
        var retVal = new List<MountDef>();
        foreach (DataRow row in table.Rows)
        {
            var mountDef = new MountDef
            {
                MountDefNum = SIn.Long(row["MountDefNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                Height = SIn.Int(row["Height"].ToString()),
                ColorBack = Color.FromArgb(SIn.Int(row["ColorBack"].ToString())),
                ColorFore = Color.FromArgb(SIn.Int(row["ColorFore"].ToString())),
                ColorTextBack = Color.FromArgb(SIn.Int(row["ColorTextBack"].ToString())),
                ScaleValue = SIn.String(row["ScaleValue"].ToString()),
                DefaultCat = SIn.Long(row["DefaultCat"].ToString()),
                FlipOnAcquire = SIn.Bool(row["FlipOnAcquire"].ToString()),
                AdjModeAfterSeries = SIn.Bool(row["AdjModeAfterSeries"].ToString())
            };
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

    public static void Insert(MountDef mountDef)
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
}