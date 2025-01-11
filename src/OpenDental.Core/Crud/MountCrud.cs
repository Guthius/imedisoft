#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MountCrud
{
    public static Mount SelectOne(long mountNum)
    {
        var command = "SELECT * FROM mount "
                      + "WHERE MountNum = " + SOut.Long(mountNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Mount SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Mount> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Mount> TableToList(DataTable table)
    {
        var retVal = new List<Mount>();
        Mount mount;
        foreach (DataRow row in table.Rows)
        {
            mount = new Mount();
            mount.MountNum = SIn.Long(row["MountNum"].ToString());
            mount.PatNum = SIn.Long(row["PatNum"].ToString());
            mount.DocCategory = SIn.Long(row["DocCategory"].ToString());
            mount.DateCreated = SIn.DateTime(row["DateCreated"].ToString());
            mount.Description = SIn.String(row["Description"].ToString());
            mount.Note = SIn.String(row["Note"].ToString());
            mount.Width = SIn.Int(row["Width"].ToString());
            mount.Height = SIn.Int(row["Height"].ToString());
            mount.ColorBack = Color.FromArgb(SIn.Int(row["ColorBack"].ToString()));
            mount.ProvNum = SIn.Long(row["ProvNum"].ToString());
            mount.ColorFore = Color.FromArgb(SIn.Int(row["ColorFore"].ToString()));
            mount.ColorTextBack = Color.FromArgb(SIn.Int(row["ColorTextBack"].ToString()));
            mount.FlipOnAcquire = SIn.Bool(row["FlipOnAcquire"].ToString());
            mount.AdjModeAfterSeries = SIn.Bool(row["AdjModeAfterSeries"].ToString());
            retVal.Add(mount);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Mount> listMounts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Mount";
        var table = new DataTable(tableName);
        table.Columns.Add("MountNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DocCategory");
        table.Columns.Add("DateCreated");
        table.Columns.Add("Description");
        table.Columns.Add("Note");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("ColorBack");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ColorFore");
        table.Columns.Add("ColorTextBack");
        table.Columns.Add("FlipOnAcquire");
        table.Columns.Add("AdjModeAfterSeries");
        foreach (var mount in listMounts)
            table.Rows.Add(SOut.Long(mount.MountNum), SOut.Long(mount.PatNum), SOut.Long(mount.DocCategory), SOut.DateT(mount.DateCreated, false), mount.Description, mount.Note, SOut.Int(mount.Width), SOut.Int(mount.Height), SOut.Int(mount.ColorBack.ToArgb()), SOut.Long(mount.ProvNum), SOut.Int(mount.ColorFore.ToArgb()), SOut.Int(mount.ColorTextBack.ToArgb()), SOut.Bool(mount.FlipOnAcquire), SOut.Bool(mount.AdjModeAfterSeries));
        return table;
    }

    public static long Insert(Mount mount)
    {
        return Insert(mount, false);
    }

    public static long Insert(Mount mount, bool useExistingPK)
    {
        var command = "INSERT INTO mount (";

        command += "PatNum,DocCategory,DateCreated,Description,Note,Width,Height,ColorBack,ProvNum,ColorFore,ColorTextBack,FlipOnAcquire,AdjModeAfterSeries) VALUES(";

        command +=
            SOut.Long(mount.PatNum) + ","
                                    + SOut.Long(mount.DocCategory) + ","
                                    + SOut.DateT(mount.DateCreated) + ","
                                    + "'" + SOut.String(mount.Description) + "',"
                                    + DbHelper.ParamChar + "paramNote,"
                                    + SOut.Int(mount.Width) + ","
                                    + SOut.Int(mount.Height) + ","
                                    + SOut.Int(mount.ColorBack.ToArgb()) + ","
                                    + SOut.Long(mount.ProvNum) + ","
                                    + SOut.Int(mount.ColorFore.ToArgb()) + ","
                                    + SOut.Int(mount.ColorTextBack.ToArgb()) + ","
                                    + SOut.Bool(mount.FlipOnAcquire) + ","
                                    + SOut.Bool(mount.AdjModeAfterSeries) + ")";
        if (mount.Note == null) mount.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(mount.Note));
        {
            mount.MountNum = Db.NonQ(command, true, "MountNum", "mount", paramNote);
        }
        return mount.MountNum;
    }

    public static long InsertNoCache(Mount mount)
    {
        return InsertNoCache(mount, false);
    }

    public static long InsertNoCache(Mount mount, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mount (";
        if (isRandomKeys || useExistingPK) command += "MountNum,";
        command += "PatNum,DocCategory,DateCreated,Description,Note,Width,Height,ColorBack,ProvNum,ColorFore,ColorTextBack,FlipOnAcquire,AdjModeAfterSeries) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mount.MountNum) + ",";
        command +=
            SOut.Long(mount.PatNum) + ","
                                    + SOut.Long(mount.DocCategory) + ","
                                    + SOut.DateT(mount.DateCreated) + ","
                                    + "'" + SOut.String(mount.Description) + "',"
                                    + DbHelper.ParamChar + "paramNote,"
                                    + SOut.Int(mount.Width) + ","
                                    + SOut.Int(mount.Height) + ","
                                    + SOut.Int(mount.ColorBack.ToArgb()) + ","
                                    + SOut.Long(mount.ProvNum) + ","
                                    + SOut.Int(mount.ColorFore.ToArgb()) + ","
                                    + SOut.Int(mount.ColorTextBack.ToArgb()) + ","
                                    + SOut.Bool(mount.FlipOnAcquire) + ","
                                    + SOut.Bool(mount.AdjModeAfterSeries) + ")";
        if (mount.Note == null) mount.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(mount.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            mount.MountNum = Db.NonQ(command, true, "MountNum", "mount", paramNote);
        return mount.MountNum;
    }

    public static void Update(Mount mount)
    {
        var command = "UPDATE mount SET "
                      + "PatNum            =  " + SOut.Long(mount.PatNum) + ", "
                      + "DocCategory       =  " + SOut.Long(mount.DocCategory) + ", "
                      + "DateCreated       =  " + SOut.DateT(mount.DateCreated) + ", "
                      + "Description       = '" + SOut.String(mount.Description) + "', "
                      + "Note              =  " + DbHelper.ParamChar + "paramNote, "
                      + "Width             =  " + SOut.Int(mount.Width) + ", "
                      + "Height            =  " + SOut.Int(mount.Height) + ", "
                      + "ColorBack         =  " + SOut.Int(mount.ColorBack.ToArgb()) + ", "
                      + "ProvNum           =  " + SOut.Long(mount.ProvNum) + ", "
                      + "ColorFore         =  " + SOut.Int(mount.ColorFore.ToArgb()) + ", "
                      + "ColorTextBack     =  " + SOut.Int(mount.ColorTextBack.ToArgb()) + ", "
                      + "FlipOnAcquire     =  " + SOut.Bool(mount.FlipOnAcquire) + ", "
                      + "AdjModeAfterSeries=  " + SOut.Bool(mount.AdjModeAfterSeries) + " "
                      + "WHERE MountNum = " + SOut.Long(mount.MountNum);
        if (mount.Note == null) mount.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(mount.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Mount mount, Mount oldMount)
    {
        var command = "";
        if (mount.PatNum != oldMount.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(mount.PatNum) + "";
        }

        if (mount.DocCategory != oldMount.DocCategory)
        {
            if (command != "") command += ",";
            command += "DocCategory = " + SOut.Long(mount.DocCategory) + "";
        }

        if (mount.DateCreated != oldMount.DateCreated)
        {
            if (command != "") command += ",";
            command += "DateCreated = " + SOut.DateT(mount.DateCreated) + "";
        }

        if (mount.Description != oldMount.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(mount.Description) + "'";
        }

        if (mount.Note != oldMount.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (mount.Width != oldMount.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(mount.Width) + "";
        }

        if (mount.Height != oldMount.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(mount.Height) + "";
        }

        if (mount.ColorBack != oldMount.ColorBack)
        {
            if (command != "") command += ",";
            command += "ColorBack = " + SOut.Int(mount.ColorBack.ToArgb()) + "";
        }

        if (mount.ProvNum != oldMount.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(mount.ProvNum) + "";
        }

        if (mount.ColorFore != oldMount.ColorFore)
        {
            if (command != "") command += ",";
            command += "ColorFore = " + SOut.Int(mount.ColorFore.ToArgb()) + "";
        }

        if (mount.ColorTextBack != oldMount.ColorTextBack)
        {
            if (command != "") command += ",";
            command += "ColorTextBack = " + SOut.Int(mount.ColorTextBack.ToArgb()) + "";
        }

        if (mount.FlipOnAcquire != oldMount.FlipOnAcquire)
        {
            if (command != "") command += ",";
            command += "FlipOnAcquire = " + SOut.Bool(mount.FlipOnAcquire) + "";
        }

        if (mount.AdjModeAfterSeries != oldMount.AdjModeAfterSeries)
        {
            if (command != "") command += ",";
            command += "AdjModeAfterSeries = " + SOut.Bool(mount.AdjModeAfterSeries) + "";
        }

        if (command == "") return false;
        if (mount.Note == null) mount.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(mount.Note));
        command = "UPDATE mount SET " + command
                                      + " WHERE MountNum = " + SOut.Long(mount.MountNum);
        Db.NonQ(command, paramNote);
        return true;
    }


    public static bool UpdateComparison(Mount mount, Mount oldMount)
    {
        if (mount.PatNum != oldMount.PatNum) return true;
        if (mount.DocCategory != oldMount.DocCategory) return true;
        if (mount.DateCreated != oldMount.DateCreated) return true;
        if (mount.Description != oldMount.Description) return true;
        if (mount.Note != oldMount.Note) return true;
        if (mount.Width != oldMount.Width) return true;
        if (mount.Height != oldMount.Height) return true;
        if (mount.ColorBack != oldMount.ColorBack) return true;
        if (mount.ProvNum != oldMount.ProvNum) return true;
        if (mount.ColorFore != oldMount.ColorFore) return true;
        if (mount.ColorTextBack != oldMount.ColorTextBack) return true;
        if (mount.FlipOnAcquire != oldMount.FlipOnAcquire) return true;
        if (mount.AdjModeAfterSeries != oldMount.AdjModeAfterSeries) return true;
        return false;
    }


    public static void Delete(long mountNum)
    {
        var command = "DELETE FROM mount "
                      + "WHERE MountNum = " + SOut.Long(mountNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listMountNums)
    {
        if (listMountNums == null || listMountNums.Count == 0) return;
        var command = "DELETE FROM mount "
                      + "WHERE MountNum IN(" + string.Join(",", listMountNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}