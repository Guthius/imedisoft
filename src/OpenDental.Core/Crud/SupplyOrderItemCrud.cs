#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SupplyOrderItemCrud
{
    public static SupplyOrderItem SelectOne(long supplyOrderItemNum)
    {
        var command = "SELECT * FROM supplyorderitem "
                      + "WHERE SupplyOrderItemNum = " + SOut.Long(supplyOrderItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SupplyOrderItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SupplyOrderItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SupplyOrderItem> TableToList(DataTable table)
    {
        var retVal = new List<SupplyOrderItem>();
        SupplyOrderItem supplyOrderItem;
        foreach (DataRow row in table.Rows)
        {
            supplyOrderItem = new SupplyOrderItem();
            supplyOrderItem.SupplyOrderItemNum = SIn.Long(row["SupplyOrderItemNum"].ToString());
            supplyOrderItem.SupplyOrderNum = SIn.Long(row["SupplyOrderNum"].ToString());
            supplyOrderItem.SupplyNum = SIn.Long(row["SupplyNum"].ToString());
            supplyOrderItem.Qty = SIn.Int(row["Qty"].ToString());
            supplyOrderItem.Price = SIn.Double(row["Price"].ToString());
            supplyOrderItem.DateReceived = SIn.Date(row["DateReceived"].ToString());
            retVal.Add(supplyOrderItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SupplyOrderItem> listSupplyOrderItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SupplyOrderItem";
        var table = new DataTable(tableName);
        table.Columns.Add("SupplyOrderItemNum");
        table.Columns.Add("SupplyOrderNum");
        table.Columns.Add("SupplyNum");
        table.Columns.Add("Qty");
        table.Columns.Add("Price");
        table.Columns.Add("DateReceived");
        foreach (var supplyOrderItem in listSupplyOrderItems)
            table.Rows.Add(SOut.Long(supplyOrderItem.SupplyOrderItemNum), SOut.Long(supplyOrderItem.SupplyOrderNum), SOut.Long(supplyOrderItem.SupplyNum), SOut.Int(supplyOrderItem.Qty), SOut.Double(supplyOrderItem.Price), SOut.DateT(supplyOrderItem.DateReceived, false));
        return table;
    }

    public static long Insert(SupplyOrderItem supplyOrderItem)
    {
        return Insert(supplyOrderItem, false);
    }

    public static long Insert(SupplyOrderItem supplyOrderItem, bool useExistingPK)
    {
        var command = "INSERT INTO supplyorderitem (";

        command += "SupplyOrderNum,SupplyNum,Qty,Price,DateReceived) VALUES(";

        command +=
            SOut.Long(supplyOrderItem.SupplyOrderNum) + ","
                                                      + SOut.Long(supplyOrderItem.SupplyNum) + ","
                                                      + SOut.Int(supplyOrderItem.Qty) + ","
                                                      + SOut.Double(supplyOrderItem.Price) + ","
                                                      + SOut.Date(supplyOrderItem.DateReceived) + ")";
        {
            supplyOrderItem.SupplyOrderItemNum = Db.NonQ(command, true, "SupplyOrderItemNum", "supplyOrderItem");
        }
        return supplyOrderItem.SupplyOrderItemNum;
    }

    public static long InsertNoCache(SupplyOrderItem supplyOrderItem)
    {
        return InsertNoCache(supplyOrderItem, false);
    }

    public static long InsertNoCache(SupplyOrderItem supplyOrderItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO supplyorderitem (";
        if (isRandomKeys || useExistingPK) command += "SupplyOrderItemNum,";
        command += "SupplyOrderNum,SupplyNum,Qty,Price,DateReceived) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(supplyOrderItem.SupplyOrderItemNum) + ",";
        command +=
            SOut.Long(supplyOrderItem.SupplyOrderNum) + ","
                                                      + SOut.Long(supplyOrderItem.SupplyNum) + ","
                                                      + SOut.Int(supplyOrderItem.Qty) + ","
                                                      + SOut.Double(supplyOrderItem.Price) + ","
                                                      + SOut.Date(supplyOrderItem.DateReceived) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            supplyOrderItem.SupplyOrderItemNum = Db.NonQ(command, true, "SupplyOrderItemNum", "supplyOrderItem");
        return supplyOrderItem.SupplyOrderItemNum;
    }

    public static void Update(SupplyOrderItem supplyOrderItem)
    {
        var command = "UPDATE supplyorderitem SET "
                      + "SupplyOrderNum    =  " + SOut.Long(supplyOrderItem.SupplyOrderNum) + ", "
                      + "SupplyNum         =  " + SOut.Long(supplyOrderItem.SupplyNum) + ", "
                      + "Qty               =  " + SOut.Int(supplyOrderItem.Qty) + ", "
                      + "Price             =  " + SOut.Double(supplyOrderItem.Price) + ", "
                      + "DateReceived      =  " + SOut.Date(supplyOrderItem.DateReceived) + " "
                      + "WHERE SupplyOrderItemNum = " + SOut.Long(supplyOrderItem.SupplyOrderItemNum);
        Db.NonQ(command);
    }

    public static bool Update(SupplyOrderItem supplyOrderItem, SupplyOrderItem oldSupplyOrderItem)
    {
        var command = "";
        if (supplyOrderItem.SupplyOrderNum != oldSupplyOrderItem.SupplyOrderNum)
        {
            if (command != "") command += ",";
            command += "SupplyOrderNum = " + SOut.Long(supplyOrderItem.SupplyOrderNum) + "";
        }

        if (supplyOrderItem.SupplyNum != oldSupplyOrderItem.SupplyNum)
        {
            if (command != "") command += ",";
            command += "SupplyNum = " + SOut.Long(supplyOrderItem.SupplyNum) + "";
        }

        if (supplyOrderItem.Qty != oldSupplyOrderItem.Qty)
        {
            if (command != "") command += ",";
            command += "Qty = " + SOut.Int(supplyOrderItem.Qty) + "";
        }

        if (supplyOrderItem.Price != oldSupplyOrderItem.Price)
        {
            if (command != "") command += ",";
            command += "Price = " + SOut.Double(supplyOrderItem.Price) + "";
        }

        if (supplyOrderItem.DateReceived.Date != oldSupplyOrderItem.DateReceived.Date)
        {
            if (command != "") command += ",";
            command += "DateReceived = " + SOut.Date(supplyOrderItem.DateReceived) + "";
        }

        if (command == "") return false;
        command = "UPDATE supplyorderitem SET " + command
                                                + " WHERE SupplyOrderItemNum = " + SOut.Long(supplyOrderItem.SupplyOrderItemNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SupplyOrderItem supplyOrderItem, SupplyOrderItem oldSupplyOrderItem)
    {
        if (supplyOrderItem.SupplyOrderNum != oldSupplyOrderItem.SupplyOrderNum) return true;
        if (supplyOrderItem.SupplyNum != oldSupplyOrderItem.SupplyNum) return true;
        if (supplyOrderItem.Qty != oldSupplyOrderItem.Qty) return true;
        if (supplyOrderItem.Price != oldSupplyOrderItem.Price) return true;
        if (supplyOrderItem.DateReceived.Date != oldSupplyOrderItem.DateReceived.Date) return true;
        return false;
    }

    public static void Delete(long supplyOrderItemNum)
    {
        var command = "DELETE FROM supplyorderitem "
                      + "WHERE SupplyOrderItemNum = " + SOut.Long(supplyOrderItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSupplyOrderItemNums)
    {
        if (listSupplyOrderItemNums == null || listSupplyOrderItemNums.Count == 0) return;
        var command = "DELETE FROM supplyorderitem "
                      + "WHERE SupplyOrderItemNum IN(" + string.Join(",", listSupplyOrderItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}