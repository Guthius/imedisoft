#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DefCrud
{
    public static Def SelectOne(long defNum)
    {
        var command = "SELECT * FROM definition "
                      + "WHERE DefNum = " + SOut.Long(defNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Def SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Def> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Def> TableToList(DataTable table)
    {
        var retVal = new List<Def>();
        Def def;
        foreach (DataRow row in table.Rows)
        {
            def = new Def();
            def.DefNum = SIn.Long(row["DefNum"].ToString());
            def.Category = (DefCat) SIn.Int(row["Category"].ToString());
            def.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            def.ItemName = SIn.String(row["ItemName"].ToString());
            def.ItemValue = SIn.String(row["ItemValue"].ToString());
            def.ItemColor = Color.FromArgb(SIn.Int(row["ItemColor"].ToString()));
            def.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(def);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Def> listDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Def";
        var table = new DataTable(tableName);
        table.Columns.Add("DefNum");
        table.Columns.Add("Category");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("ItemName");
        table.Columns.Add("ItemValue");
        table.Columns.Add("ItemColor");
        table.Columns.Add("IsHidden");
        foreach (var def in listDefs)
            table.Rows.Add(SOut.Long(def.DefNum), SOut.Int((int) def.Category), SOut.Int(def.ItemOrder), def.ItemName, def.ItemValue, SOut.Int(def.ItemColor.ToArgb()), SOut.Bool(def.IsHidden));
        return table;
    }

    public static long Insert(Def def)
    {
        return Insert(def, false);
    }

    public static long Insert(Def def, bool useExistingPK)
    {
        var command = "INSERT INTO definition (";

        command += "Category,ItemOrder,ItemName,ItemValue,ItemColor,IsHidden) VALUES(";

        command +=
            SOut.Int((int) def.Category) + ","
                                         + SOut.Int(def.ItemOrder) + ","
                                         + "'" + SOut.StringNote(def.ItemName, true) + "',"
                                         + "'" + SOut.String(def.ItemValue) + "',"
                                         + SOut.Int(def.ItemColor.ToArgb()) + ","
                                         + SOut.Bool(def.IsHidden) + ")";
        {
            def.DefNum = Db.NonQ(command, true, "DefNum", "def");
        }
        return def.DefNum;
    }

    public static long InsertNoCache(Def def)
    {
        return InsertNoCache(def, false);
    }

    public static long InsertNoCache(Def def, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO definition (";
        if (isRandomKeys || useExistingPK) command += "DefNum,";
        command += "Category,ItemOrder,ItemName,ItemValue,ItemColor,IsHidden) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(def.DefNum) + ",";
        command +=
            SOut.Int((int) def.Category) + ","
                                         + SOut.Int(def.ItemOrder) + ","
                                         + "'" + SOut.StringNote(def.ItemName, true) + "',"
                                         + "'" + SOut.String(def.ItemValue) + "',"
                                         + SOut.Int(def.ItemColor.ToArgb()) + ","
                                         + SOut.Bool(def.IsHidden) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            def.DefNum = Db.NonQ(command, true, "DefNum", "def");
        return def.DefNum;
    }

    public static void Update(Def def)
    {
        var command = "UPDATE definition SET "
                      + "Category =  " + SOut.Int((int) def.Category) + ", "
                      + "ItemOrder=  " + SOut.Int(def.ItemOrder) + ", "
                      + "ItemName = '" + SOut.StringNote(def.ItemName, true) + "', "
                      + "ItemValue= '" + SOut.String(def.ItemValue) + "', "
                      + "ItemColor=  " + SOut.Int(def.ItemColor.ToArgb()) + ", "
                      + "IsHidden =  " + SOut.Bool(def.IsHidden) + " "
                      + "WHERE DefNum = " + SOut.Long(def.DefNum);
        Db.NonQ(command);
    }

    public static bool Update(Def def, Def oldDef)
    {
        var command = "";
        if (def.Category != oldDef.Category)
        {
            if (command != "") command += ",";
            command += "Category = " + SOut.Int((int) def.Category) + "";
        }

        if (def.ItemOrder != oldDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(def.ItemOrder) + "";
        }

        if (def.ItemName != oldDef.ItemName)
        {
            if (command != "") command += ",";
            command += "ItemName = '" + SOut.StringNote(def.ItemName, true) + "'";
        }

        if (def.ItemValue != oldDef.ItemValue)
        {
            if (command != "") command += ",";
            command += "ItemValue = '" + SOut.String(def.ItemValue) + "'";
        }

        if (def.ItemColor != oldDef.ItemColor)
        {
            if (command != "") command += ",";
            command += "ItemColor = " + SOut.Int(def.ItemColor.ToArgb()) + "";
        }

        if (def.IsHidden != oldDef.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(def.IsHidden) + "";
        }

        if (command == "") return false;
        command = "UPDATE definition SET " + command
                                           + " WHERE DefNum = " + SOut.Long(def.DefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Def def, Def oldDef)
    {
        if (def.Category != oldDef.Category) return true;
        if (def.ItemOrder != oldDef.ItemOrder) return true;
        if (def.ItemName != oldDef.ItemName) return true;
        if (def.ItemValue != oldDef.ItemValue) return true;
        if (def.ItemColor != oldDef.ItemColor) return true;
        if (def.IsHidden != oldDef.IsHidden) return true;
        return false;
    }

    public static void Delete(long defNum)
    {
        var command = "DELETE FROM definition "
                      + "WHERE DefNum = " + SOut.Long(defNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDefNums)
    {
        if (listDefNums == null || listDefNums.Count == 0) return;
        var command = "DELETE FROM definition "
                      + "WHERE DefNum IN(" + string.Join(",", listDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}