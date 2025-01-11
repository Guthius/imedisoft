#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MountItemCrud
{
    public static MountItem SelectOne(long mountItemNum)
    {
        var command = "SELECT * FROM mountitem "
                      + "WHERE MountItemNum = " + SOut.Long(mountItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MountItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MountItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MountItem> TableToList(DataTable table)
    {
        var retVal = new List<MountItem>();
        MountItem mountItem;
        foreach (DataRow row in table.Rows)
        {
            mountItem = new MountItem();
            mountItem.MountItemNum = SIn.Long(row["MountItemNum"].ToString());
            mountItem.MountNum = SIn.Long(row["MountNum"].ToString());
            mountItem.Xpos = SIn.Int(row["Xpos"].ToString());
            mountItem.Ypos = SIn.Int(row["Ypos"].ToString());
            mountItem.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            mountItem.Width = SIn.Int(row["Width"].ToString());
            mountItem.Height = SIn.Int(row["Height"].ToString());
            mountItem.RotateOnAcquire = SIn.Int(row["RotateOnAcquire"].ToString());
            mountItem.ToothNumbers = SIn.String(row["ToothNumbers"].ToString());
            mountItem.TextShowing = SIn.String(row["TextShowing"].ToString());
            mountItem.FontSize = SIn.Float(row["FontSize"].ToString());
            retVal.Add(mountItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MountItem> listMountItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MountItem";
        var table = new DataTable(tableName);
        table.Columns.Add("MountItemNum");
        table.Columns.Add("MountNum");
        table.Columns.Add("Xpos");
        table.Columns.Add("Ypos");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("RotateOnAcquire");
        table.Columns.Add("ToothNumbers");
        table.Columns.Add("TextShowing");
        table.Columns.Add("FontSize");
        foreach (var mountItem in listMountItems)
            table.Rows.Add(SOut.Long(mountItem.MountItemNum), SOut.Long(mountItem.MountNum), SOut.Int(mountItem.Xpos), SOut.Int(mountItem.Ypos), SOut.Int(mountItem.ItemOrder), SOut.Int(mountItem.Width), SOut.Int(mountItem.Height), SOut.Int(mountItem.RotateOnAcquire), mountItem.ToothNumbers, mountItem.TextShowing, SOut.Float(mountItem.FontSize));
        return table;
    }

    public static long Insert(MountItem mountItem)
    {
        return Insert(mountItem, false);
    }

    public static long Insert(MountItem mountItem, bool useExistingPK)
    {
        var command = "INSERT INTO mountitem (";

        command += "MountNum,Xpos,Ypos,ItemOrder,Width,Height,RotateOnAcquire,ToothNumbers,TextShowing,FontSize) VALUES(";

        command +=
            SOut.Long(mountItem.MountNum) + ","
                                          + SOut.Int(mountItem.Xpos) + ","
                                          + SOut.Int(mountItem.Ypos) + ","
                                          + SOut.Int(mountItem.ItemOrder) + ","
                                          + SOut.Int(mountItem.Width) + ","
                                          + SOut.Int(mountItem.Height) + ","
                                          + SOut.Int(mountItem.RotateOnAcquire) + ","
                                          + "'" + SOut.String(mountItem.ToothNumbers) + "',"
                                          + DbHelper.ParamChar + "paramTextShowing,"
                                          + SOut.Float(mountItem.FontSize) + ")";
        if (mountItem.TextShowing == null) mountItem.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItem.TextShowing));
        {
            mountItem.MountItemNum = Db.NonQ(command, true, "MountItemNum", "mountItem", paramTextShowing);
        }
        return mountItem.MountItemNum;
    }

    public static long InsertNoCache(MountItem mountItem)
    {
        return InsertNoCache(mountItem, false);
    }

    public static long InsertNoCache(MountItem mountItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mountitem (";
        if (isRandomKeys || useExistingPK) command += "MountItemNum,";
        command += "MountNum,Xpos,Ypos,ItemOrder,Width,Height,RotateOnAcquire,ToothNumbers,TextShowing,FontSize) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mountItem.MountItemNum) + ",";
        command +=
            SOut.Long(mountItem.MountNum) + ","
                                          + SOut.Int(mountItem.Xpos) + ","
                                          + SOut.Int(mountItem.Ypos) + ","
                                          + SOut.Int(mountItem.ItemOrder) + ","
                                          + SOut.Int(mountItem.Width) + ","
                                          + SOut.Int(mountItem.Height) + ","
                                          + SOut.Int(mountItem.RotateOnAcquire) + ","
                                          + "'" + SOut.String(mountItem.ToothNumbers) + "',"
                                          + DbHelper.ParamChar + "paramTextShowing,"
                                          + SOut.Float(mountItem.FontSize) + ")";
        if (mountItem.TextShowing == null) mountItem.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItem.TextShowing));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramTextShowing);
        else
            mountItem.MountItemNum = Db.NonQ(command, true, "MountItemNum", "mountItem", paramTextShowing);
        return mountItem.MountItemNum;
    }

    public static void Update(MountItem mountItem)
    {
        var command = "UPDATE mountitem SET "
                      + "MountNum       =  " + SOut.Long(mountItem.MountNum) + ", "
                      + "Xpos           =  " + SOut.Int(mountItem.Xpos) + ", "
                      + "Ypos           =  " + SOut.Int(mountItem.Ypos) + ", "
                      + "ItemOrder      =  " + SOut.Int(mountItem.ItemOrder) + ", "
                      + "Width          =  " + SOut.Int(mountItem.Width) + ", "
                      + "Height         =  " + SOut.Int(mountItem.Height) + ", "
                      + "RotateOnAcquire=  " + SOut.Int(mountItem.RotateOnAcquire) + ", "
                      + "ToothNumbers   = '" + SOut.String(mountItem.ToothNumbers) + "', "
                      + "TextShowing    =  " + DbHelper.ParamChar + "paramTextShowing, "
                      + "FontSize       =  " + SOut.Float(mountItem.FontSize) + " "
                      + "WHERE MountItemNum = " + SOut.Long(mountItem.MountItemNum);
        if (mountItem.TextShowing == null) mountItem.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItem.TextShowing));
        Db.NonQ(command, paramTextShowing);
    }

    public static bool Update(MountItem mountItem, MountItem oldMountItem)
    {
        var command = "";
        if (mountItem.MountNum != oldMountItem.MountNum)
        {
            if (command != "") command += ",";
            command += "MountNum = " + SOut.Long(mountItem.MountNum) + "";
        }

        if (mountItem.Xpos != oldMountItem.Xpos)
        {
            if (command != "") command += ",";
            command += "Xpos = " + SOut.Int(mountItem.Xpos) + "";
        }

        if (mountItem.Ypos != oldMountItem.Ypos)
        {
            if (command != "") command += ",";
            command += "Ypos = " + SOut.Int(mountItem.Ypos) + "";
        }

        if (mountItem.ItemOrder != oldMountItem.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(mountItem.ItemOrder) + "";
        }

        if (mountItem.Width != oldMountItem.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(mountItem.Width) + "";
        }

        if (mountItem.Height != oldMountItem.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(mountItem.Height) + "";
        }

        if (mountItem.RotateOnAcquire != oldMountItem.RotateOnAcquire)
        {
            if (command != "") command += ",";
            command += "RotateOnAcquire = " + SOut.Int(mountItem.RotateOnAcquire) + "";
        }

        if (mountItem.ToothNumbers != oldMountItem.ToothNumbers)
        {
            if (command != "") command += ",";
            command += "ToothNumbers = '" + SOut.String(mountItem.ToothNumbers) + "'";
        }

        if (mountItem.TextShowing != oldMountItem.TextShowing)
        {
            if (command != "") command += ",";
            command += "TextShowing = " + DbHelper.ParamChar + "paramTextShowing";
        }

        if (mountItem.FontSize != oldMountItem.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(mountItem.FontSize) + "";
        }

        if (command == "") return false;
        if (mountItem.TextShowing == null) mountItem.TextShowing = "";
        var paramTextShowing = new OdSqlParameter("paramTextShowing", OdDbType.Text, SOut.StringParam(mountItem.TextShowing));
        command = "UPDATE mountitem SET " + command
                                          + " WHERE MountItemNum = " + SOut.Long(mountItem.MountItemNum);
        Db.NonQ(command, paramTextShowing);
        return true;
    }

    public static bool UpdateComparison(MountItem mountItem, MountItem oldMountItem)
    {
        if (mountItem.MountNum != oldMountItem.MountNum) return true;
        if (mountItem.Xpos != oldMountItem.Xpos) return true;
        if (mountItem.Ypos != oldMountItem.Ypos) return true;
        if (mountItem.ItemOrder != oldMountItem.ItemOrder) return true;
        if (mountItem.Width != oldMountItem.Width) return true;
        if (mountItem.Height != oldMountItem.Height) return true;
        if (mountItem.RotateOnAcquire != oldMountItem.RotateOnAcquire) return true;
        if (mountItem.ToothNumbers != oldMountItem.ToothNumbers) return true;
        if (mountItem.TextShowing != oldMountItem.TextShowing) return true;
        if (mountItem.FontSize != oldMountItem.FontSize) return true;
        return false;
    }

    public static void Delete(long mountItemNum)
    {
        var command = "DELETE FROM mountitem "
                      + "WHERE MountItemNum = " + SOut.Long(mountItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMountItemNums)
    {
        if (listMountItemNums == null || listMountItemNums.Count == 0) return;
        var command = "DELETE FROM mountitem "
                      + "WHERE MountItemNum IN(" + string.Join(",", listMountItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}