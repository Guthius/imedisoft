using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DefCrud
{
    public static List<Def> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Def> TableToList(DataTable table)
    {
        var retVal = new List<Def>();
        foreach (DataRow row in table.Rows)
        {
            var def = new Def
            {
                DefNum = SIn.Long(row["DefNum"].ToString()),
                Category = (DefCat) SIn.Int(row["Category"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                ItemName = SIn.String(row["ItemName"].ToString()),
                ItemValue = SIn.String(row["ItemValue"].ToString()),
                ItemColor = Color.FromArgb(SIn.Int(row["ItemColor"].ToString())),
                IsHidden = SIn.Bool(row["IsHidden"].ToString())
            };
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
}