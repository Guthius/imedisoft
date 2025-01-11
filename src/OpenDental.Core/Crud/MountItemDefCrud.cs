#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MountItemDefCrud
{
    public static MountItemDef SelectOne(long mountItemDefNum)
    {
        var command = "SELECT * FROM mountitemdef "
                      + "WHERE MountItemDefNum = " + SOut.Long(mountItemDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MountItemDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MountItemDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MountItemDef> TableToList(DataTable table)
    {
        var retVal = new List<MountItemDef>();
        MountItemDef mountItemDef;
        foreach (DataRow row in table.Rows)
        {
            mountItemDef = new MountItemDef();
            mountItemDef.MountItemDefNum = SIn.Long(row["MountItemDefNum"].ToString());
            mountItemDef.MountDefNum = SIn.Long(row["MountDefNum"].ToString());
            mountItemDef.Xpos = SIn.Int(row["Xpos"].ToString());
            mountItemDef.Ypos = SIn.Int(row["Ypos"].ToString());
            mountItemDef.Width = SIn.Int(row["Width"].ToString());
            mountItemDef.Height = SIn.Int(row["Height"].ToString());
            mountItemDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            mountItemDef.RotateOnAcquire = SIn.Int(row["RotateOnAcquire"].ToString());
            mountItemDef.ToothNumbers = SIn.String(row["ToothNumbers"].ToString());
            mountItemDef.TextShowing = SIn.String(row["TextShowing"].ToString());
            mountItemDef.FontSize = SIn.Float(row["FontSize"].ToString());
            retVal.Add(mountItemDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MountItemDef> listMountItemDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MountItemDef";
        var table = new DataTable(tableName);
        table.Columns.Add("MountItemDefNum");
        table.Columns.Add("MountDefNum");
        table.Columns.Add("Xpos");
        table.Columns.Add("Ypos");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("RotateOnAcquire");
        table.Columns.Add("ToothNumbers");
        table.Columns.Add("TextShowing");
        table.Columns.Add("FontSize");
        foreach (var mountItemDef in listMountItemDefs)
            table.Rows.Add(SOut.Long(mountItemDef.MountItemDefNum), SOut.Long(mountItemDef.MountDefNum), SOut.Int(mountItemDef.Xpos), SOut.Int(mountItemDef.Ypos), SOut.Int(mountItemDef.Width), SOut.Int(mountItemDef.Height), SOut.Int(mountItemDef.ItemOrder), SOut.Int(mountItemDef.RotateOnAcquire), mountItemDef.ToothNumbers, mountItemDef.TextShowing, SOut.Float(mountItemDef.FontSize));
        return table;
    }

    public static long Insert(MountItemDef mountItemDef)
    {
        return Insert(mountItemDef, false);
    }

    public static long Insert(MountItemDef mountItemDef, bool useExistingPK)
    {
        var command = "INSERT INTO mountitemdef (";

        command += "MountDefNum,Xpos,Ypos,Width,Height,ItemOrder,RotateOnAcquire,ToothNumbers,TextShowing,FontSize) VALUES(";

        command +=
            SOut.Long(mountItemDef.MountDefNum) + ","
                                                + SOut.Int(mountItemDef.Xpos) + ","
                                                + SOut.Int(mountItemDef.Ypos) + ","
                                                + SOut.Int(mountItemDef.Width) + ","
                                                + SOut.Int(mountItemDef.Height) + ","
                                                + SOut.Int(mountItemDef.ItemOrder) + ","
                                                + SOut.Int(mountItemDef.RotateOnAcquire) + ","
                                                + "'" + SOut.String(mountItemDef.ToothNumbers) + "',"
                                                + DbHelper.ParamChar + "paramTextShowing,"
                                                + SOut.Float(mountItemDef.FontSize) + ")";
        if (mountItemDef.TextShowing == null) mountItemDef.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItemDef.TextShowing));
        {
            mountItemDef.MountItemDefNum = Db.NonQ(command, true, "MountItemDefNum", "mountItemDef", paramTextShowing);
        }
        return mountItemDef.MountItemDefNum;
    }

    public static long InsertNoCache(MountItemDef mountItemDef)
    {
        return InsertNoCache(mountItemDef, false);
    }

    public static long InsertNoCache(MountItemDef mountItemDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mountitemdef (";
        if (isRandomKeys || useExistingPK) command += "MountItemDefNum,";
        command += "MountDefNum,Xpos,Ypos,Width,Height,ItemOrder,RotateOnAcquire,ToothNumbers,TextShowing,FontSize) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mountItemDef.MountItemDefNum) + ",";
        command +=
            SOut.Long(mountItemDef.MountDefNum) + ","
                                                + SOut.Int(mountItemDef.Xpos) + ","
                                                + SOut.Int(mountItemDef.Ypos) + ","
                                                + SOut.Int(mountItemDef.Width) + ","
                                                + SOut.Int(mountItemDef.Height) + ","
                                                + SOut.Int(mountItemDef.ItemOrder) + ","
                                                + SOut.Int(mountItemDef.RotateOnAcquire) + ","
                                                + "'" + SOut.String(mountItemDef.ToothNumbers) + "',"
                                                + DbHelper.ParamChar + "paramTextShowing,"
                                                + SOut.Float(mountItemDef.FontSize) + ")";
        if (mountItemDef.TextShowing == null) mountItemDef.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItemDef.TextShowing));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramTextShowing);
        else
            mountItemDef.MountItemDefNum = Db.NonQ(command, true, "MountItemDefNum", "mountItemDef", paramTextShowing);
        return mountItemDef.MountItemDefNum;
    }

    public static void Update(MountItemDef mountItemDef)
    {
        var command = "UPDATE mountitemdef SET "
                      + "MountDefNum    =  " + SOut.Long(mountItemDef.MountDefNum) + ", "
                      + "Xpos           =  " + SOut.Int(mountItemDef.Xpos) + ", "
                      + "Ypos           =  " + SOut.Int(mountItemDef.Ypos) + ", "
                      + "Width          =  " + SOut.Int(mountItemDef.Width) + ", "
                      + "Height         =  " + SOut.Int(mountItemDef.Height) + ", "
                      + "ItemOrder      =  " + SOut.Int(mountItemDef.ItemOrder) + ", "
                      + "RotateOnAcquire=  " + SOut.Int(mountItemDef.RotateOnAcquire) + ", "
                      + "ToothNumbers   = '" + SOut.String(mountItemDef.ToothNumbers) + "', "
                      + "TextShowing    =  " + DbHelper.ParamChar + "paramTextShowing, "
                      + "FontSize       =  " + SOut.Float(mountItemDef.FontSize) + " "
                      + "WHERE MountItemDefNum = " + SOut.Long(mountItemDef.MountItemDefNum);
        if (mountItemDef.TextShowing == null) mountItemDef.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItemDef.TextShowing));
        Db.NonQ(command, paramTextShowing);
    }

    public static bool Update(MountItemDef mountItemDef, MountItemDef oldMountItemDef)
    {
        var command = "";
        if (mountItemDef.MountDefNum != oldMountItemDef.MountDefNum)
        {
            if (command != "") command += ",";
            command += "MountDefNum = " + SOut.Long(mountItemDef.MountDefNum) + "";
        }

        if (mountItemDef.Xpos != oldMountItemDef.Xpos)
        {
            if (command != "") command += ",";
            command += "Xpos = " + SOut.Int(mountItemDef.Xpos) + "";
        }

        if (mountItemDef.Ypos != oldMountItemDef.Ypos)
        {
            if (command != "") command += ",";
            command += "Ypos = " + SOut.Int(mountItemDef.Ypos) + "";
        }

        if (mountItemDef.Width != oldMountItemDef.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(mountItemDef.Width) + "";
        }

        if (mountItemDef.Height != oldMountItemDef.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(mountItemDef.Height) + "";
        }

        if (mountItemDef.ItemOrder != oldMountItemDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(mountItemDef.ItemOrder) + "";
        }

        if (mountItemDef.RotateOnAcquire != oldMountItemDef.RotateOnAcquire)
        {
            if (command != "") command += ",";
            command += "RotateOnAcquire = " + SOut.Int(mountItemDef.RotateOnAcquire) + "";
        }

        if (mountItemDef.ToothNumbers != oldMountItemDef.ToothNumbers)
        {
            if (command != "") command += ",";
            command += "ToothNumbers = '" + SOut.String(mountItemDef.ToothNumbers) + "'";
        }

        if (mountItemDef.TextShowing != oldMountItemDef.TextShowing)
        {
            if (command != "") command += ",";
            command += "TextShowing = " + DbHelper.ParamChar + "paramTextShowing";
        }

        if (mountItemDef.FontSize != oldMountItemDef.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(mountItemDef.FontSize) + "";
        }

        if (command == "") return false;
        if (mountItemDef.TextShowing == null) mountItemDef.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItemDef.TextShowing));
        command = "UPDATE mountitemdef SET " + command
                                             + " WHERE MountItemDefNum = " + SOut.Long(mountItemDef.MountItemDefNum);
        Db.NonQ(command, paramTextShowing);
        return true;
    }

    public static bool UpdateComparison(MountItemDef mountItemDef, MountItemDef oldMountItemDef)
    {
        if (mountItemDef.MountDefNum != oldMountItemDef.MountDefNum) return true;
        if (mountItemDef.Xpos != oldMountItemDef.Xpos) return true;
        if (mountItemDef.Ypos != oldMountItemDef.Ypos) return true;
        if (mountItemDef.Width != oldMountItemDef.Width) return true;
        if (mountItemDef.Height != oldMountItemDef.Height) return true;
        if (mountItemDef.ItemOrder != oldMountItemDef.ItemOrder) return true;
        if (mountItemDef.RotateOnAcquire != oldMountItemDef.RotateOnAcquire) return true;
        if (mountItemDef.ToothNumbers != oldMountItemDef.ToothNumbers) return true;
        if (mountItemDef.TextShowing != oldMountItemDef.TextShowing) return true;
        if (mountItemDef.FontSize != oldMountItemDef.FontSize) return true;
        return false;
    }

    public static void Delete(long mountItemDefNum)
    {
        var command = "DELETE FROM mountitemdef "
                      + "WHERE MountItemDefNum = " + SOut.Long(mountItemDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMountItemDefNums)
    {
        if (listMountItemDefNums == null || listMountItemDefNums.Count == 0) return;
        var command = "DELETE FROM mountitemdef "
                      + "WHERE MountItemDefNum IN(" + string.Join(",", listMountItemDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}