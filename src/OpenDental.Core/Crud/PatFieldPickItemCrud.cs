#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatFieldPickItemCrud
{
    public static PatFieldPickItem SelectOne(long patFieldPickItemNum)
    {
        var command = "SELECT * FROM patfieldpickitem "
                      + "WHERE PatFieldPickItemNum = " + SOut.Long(patFieldPickItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatFieldPickItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatFieldPickItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatFieldPickItem> TableToList(DataTable table)
    {
        var retVal = new List<PatFieldPickItem>();
        PatFieldPickItem patFieldPickItem;
        foreach (DataRow row in table.Rows)
        {
            patFieldPickItem = new PatFieldPickItem();
            patFieldPickItem.PatFieldPickItemNum = SIn.Long(row["PatFieldPickItemNum"].ToString());
            patFieldPickItem.PatFieldDefNum = SIn.Long(row["PatFieldDefNum"].ToString());
            patFieldPickItem.Name = SIn.String(row["Name"].ToString());
            patFieldPickItem.Abbreviation = SIn.String(row["Abbreviation"].ToString());
            patFieldPickItem.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            patFieldPickItem.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            retVal.Add(patFieldPickItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatFieldPickItem> listPatFieldPickItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatFieldPickItem";
        var table = new DataTable(tableName);
        table.Columns.Add("PatFieldPickItemNum");
        table.Columns.Add("PatFieldDefNum");
        table.Columns.Add("Name");
        table.Columns.Add("Abbreviation");
        table.Columns.Add("IsHidden");
        table.Columns.Add("ItemOrder");
        foreach (var patFieldPickItem in listPatFieldPickItems)
            table.Rows.Add(SOut.Long(patFieldPickItem.PatFieldPickItemNum), SOut.Long(patFieldPickItem.PatFieldDefNum), patFieldPickItem.Name, patFieldPickItem.Abbreviation, SOut.Bool(patFieldPickItem.IsHidden), SOut.Int(patFieldPickItem.ItemOrder));
        return table;
    }

    public static long Insert(PatFieldPickItem patFieldPickItem)
    {
        return Insert(patFieldPickItem, false);
    }

    public static long Insert(PatFieldPickItem patFieldPickItem, bool useExistingPK)
    {
        var command = "INSERT INTO patfieldpickitem (";

        command += "PatFieldDefNum,Name,Abbreviation,IsHidden,ItemOrder) VALUES(";

        command +=
            SOut.Long(patFieldPickItem.PatFieldDefNum) + ","
                                                       + "'" + SOut.String(patFieldPickItem.Name) + "',"
                                                       + "'" + SOut.String(patFieldPickItem.Abbreviation) + "',"
                                                       + SOut.Bool(patFieldPickItem.IsHidden) + ","
                                                       + SOut.Int(patFieldPickItem.ItemOrder) + ")";
        {
            patFieldPickItem.PatFieldPickItemNum = Db.NonQ(command, true, "PatFieldPickItemNum", "patFieldPickItem");
        }
        return patFieldPickItem.PatFieldPickItemNum;
    }

    public static long InsertNoCache(PatFieldPickItem patFieldPickItem)
    {
        return InsertNoCache(patFieldPickItem, false);
    }

    public static long InsertNoCache(PatFieldPickItem patFieldPickItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patfieldpickitem (";
        if (isRandomKeys || useExistingPK) command += "PatFieldPickItemNum,";
        command += "PatFieldDefNum,Name,Abbreviation,IsHidden,ItemOrder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patFieldPickItem.PatFieldPickItemNum) + ",";
        command +=
            SOut.Long(patFieldPickItem.PatFieldDefNum) + ","
                                                       + "'" + SOut.String(patFieldPickItem.Name) + "',"
                                                       + "'" + SOut.String(patFieldPickItem.Abbreviation) + "',"
                                                       + SOut.Bool(patFieldPickItem.IsHidden) + ","
                                                       + SOut.Int(patFieldPickItem.ItemOrder) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            patFieldPickItem.PatFieldPickItemNum = Db.NonQ(command, true, "PatFieldPickItemNum", "patFieldPickItem");
        return patFieldPickItem.PatFieldPickItemNum;
    }

    public static void Update(PatFieldPickItem patFieldPickItem)
    {
        var command = "UPDATE patfieldpickitem SET "
                      + "PatFieldDefNum     =  " + SOut.Long(patFieldPickItem.PatFieldDefNum) + ", "
                      + "Name               = '" + SOut.String(patFieldPickItem.Name) + "', "
                      + "Abbreviation       = '" + SOut.String(patFieldPickItem.Abbreviation) + "', "
                      + "IsHidden           =  " + SOut.Bool(patFieldPickItem.IsHidden) + ", "
                      + "ItemOrder          =  " + SOut.Int(patFieldPickItem.ItemOrder) + " "
                      + "WHERE PatFieldPickItemNum = " + SOut.Long(patFieldPickItem.PatFieldPickItemNum);
        Db.NonQ(command);
    }

    public static bool Update(PatFieldPickItem patFieldPickItem, PatFieldPickItem oldPatFieldPickItem)
    {
        var command = "";
        if (patFieldPickItem.PatFieldDefNum != oldPatFieldPickItem.PatFieldDefNum)
        {
            if (command != "") command += ",";
            command += "PatFieldDefNum = " + SOut.Long(patFieldPickItem.PatFieldDefNum) + "";
        }

        if (patFieldPickItem.Name != oldPatFieldPickItem.Name)
        {
            if (command != "") command += ",";
            command += "Name = '" + SOut.String(patFieldPickItem.Name) + "'";
        }

        if (patFieldPickItem.Abbreviation != oldPatFieldPickItem.Abbreviation)
        {
            if (command != "") command += ",";
            command += "Abbreviation = '" + SOut.String(patFieldPickItem.Abbreviation) + "'";
        }

        if (patFieldPickItem.IsHidden != oldPatFieldPickItem.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(patFieldPickItem.IsHidden) + "";
        }

        if (patFieldPickItem.ItemOrder != oldPatFieldPickItem.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(patFieldPickItem.ItemOrder) + "";
        }

        if (command == "") return false;
        command = "UPDATE patfieldpickitem SET " + command
                                                 + " WHERE PatFieldPickItemNum = " + SOut.Long(patFieldPickItem.PatFieldPickItemNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PatFieldPickItem patFieldPickItem, PatFieldPickItem oldPatFieldPickItem)
    {
        if (patFieldPickItem.PatFieldDefNum != oldPatFieldPickItem.PatFieldDefNum) return true;
        if (patFieldPickItem.Name != oldPatFieldPickItem.Name) return true;
        if (patFieldPickItem.Abbreviation != oldPatFieldPickItem.Abbreviation) return true;
        if (patFieldPickItem.IsHidden != oldPatFieldPickItem.IsHidden) return true;
        if (patFieldPickItem.ItemOrder != oldPatFieldPickItem.ItemOrder) return true;
        return false;
    }

    public static void Delete(long patFieldPickItemNum)
    {
        var command = "DELETE FROM patfieldpickitem "
                      + "WHERE PatFieldPickItemNum = " + SOut.Long(patFieldPickItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatFieldPickItemNums)
    {
        if (listPatFieldPickItemNums == null || listPatFieldPickItemNums.Count == 0) return;
        var command = "DELETE FROM patfieldpickitem "
                      + "WHERE PatFieldPickItemNum IN(" + string.Join(",", listPatFieldPickItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}