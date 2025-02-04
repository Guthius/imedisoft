using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<Mount> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Mount> TableToList(DataTable table)
    {
        var retVal = new List<Mount>();
        foreach (DataRow row in table.Rows)
        {
            var mount = new Mount
            {
                MountNum = SIn.Long(row["MountNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DocCategory = SIn.Long(row["DocCategory"].ToString()),
                DateCreated = SIn.DateTime(row["DateCreated"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                Height = SIn.Int(row["Height"].ToString()),
                ColorBack = Color.FromArgb(SIn.Int(row["ColorBack"].ToString())),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                ColorFore = Color.FromArgb(SIn.Int(row["ColorFore"].ToString())),
                ColorTextBack = Color.FromArgb(SIn.Int(row["ColorTextBack"].ToString())),
                FlipOnAcquire = SIn.Bool(row["FlipOnAcquire"].ToString()),
                AdjModeAfterSeries = SIn.Bool(row["AdjModeAfterSeries"].ToString())
            };
            retVal.Add(mount);
        }

        return retVal;
    }

    public static long Insert(Mount mount)
    {
        var command = "INSERT INTO mount (";

        command += "PatNum,DocCategory,DateCreated,Description,Note,Width,Height,ColorBack,ProvNum,ColorFore,ColorTextBack,FlipOnAcquire,AdjModeAfterSeries) VALUES(";

        command +=
            SOut.Long(mount.PatNum) + ","
                                    + SOut.Long(mount.DocCategory) + ","
                                    + SOut.DateTime(mount.DateCreated) + ","
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(mount.Note));
        {
            mount.MountNum = Db.NonQ(command, true, "MountNum", "mount", paramNote);
        }
        return mount.MountNum;
    }

    public static void Update(Mount mount)
    {
        var command = "UPDATE mount SET "
                      + "PatNum            =  " + SOut.Long(mount.PatNum) + ", "
                      + "DocCategory       =  " + SOut.Long(mount.DocCategory) + ", "
                      + "DateCreated       =  " + SOut.DateTime(mount.DateCreated) + ", "
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(mount.Note));
        Db.NonQ(command, paramNote);
    }
}