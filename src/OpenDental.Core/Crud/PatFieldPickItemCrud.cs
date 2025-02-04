using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatFieldPickItemCrud
{
    public static List<PatFieldPickItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatFieldPickItem> TableToList(DataTable table)
    {
        var retVal = new List<PatFieldPickItem>();
        foreach (DataRow row in table.Rows)
        {
            var patFieldPickItem = new PatFieldPickItem
            {
                PatFieldPickItemNum = SIn.Long(row["PatFieldPickItemNum"].ToString()),
                PatFieldDefNum = SIn.Long(row["PatFieldDefNum"].ToString()),
                Name = SIn.String(row["Name"].ToString()),
                Abbreviation = SIn.String(row["Abbreviation"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString())
            };
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

    public static void Insert(PatFieldPickItem patFieldPickItem)
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

    public static void Delete(long patFieldPickItemNum)
    {
        var command = "DELETE FROM patfieldpickitem "
                      + "WHERE PatFieldPickItemNum = " + SOut.Long(patFieldPickItemNum);
        Db.NonQ(command);
    }
}