#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SupplyOrderCrud
{
    public static SupplyOrder SelectOne(long supplyOrderNum)
    {
        var command = "SELECT * FROM supplyorder "
                      + "WHERE SupplyOrderNum = " + SOut.Long(supplyOrderNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SupplyOrder SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SupplyOrder> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SupplyOrder> TableToList(DataTable table)
    {
        var retVal = new List<SupplyOrder>();
        SupplyOrder supplyOrder;
        foreach (DataRow row in table.Rows)
        {
            supplyOrder = new SupplyOrder();
            supplyOrder.SupplyOrderNum = SIn.Long(row["SupplyOrderNum"].ToString());
            supplyOrder.SupplierNum = SIn.Long(row["SupplierNum"].ToString());
            supplyOrder.DatePlaced = SIn.Date(row["DatePlaced"].ToString());
            supplyOrder.Note = SIn.String(row["Note"].ToString());
            supplyOrder.AmountTotal = SIn.Double(row["AmountTotal"].ToString());
            supplyOrder.UserNum = SIn.Long(row["UserNum"].ToString());
            supplyOrder.ShippingCharge = SIn.Double(row["ShippingCharge"].ToString());
            supplyOrder.DateReceived = SIn.Date(row["DateReceived"].ToString());
            retVal.Add(supplyOrder);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SupplyOrder> listSupplyOrders, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SupplyOrder";
        var table = new DataTable(tableName);
        table.Columns.Add("SupplyOrderNum");
        table.Columns.Add("SupplierNum");
        table.Columns.Add("DatePlaced");
        table.Columns.Add("Note");
        table.Columns.Add("AmountTotal");
        table.Columns.Add("UserNum");
        table.Columns.Add("ShippingCharge");
        table.Columns.Add("DateReceived");
        foreach (var supplyOrder in listSupplyOrders)
            table.Rows.Add(SOut.Long(supplyOrder.SupplyOrderNum), SOut.Long(supplyOrder.SupplierNum), SOut.DateT(supplyOrder.DatePlaced, false), supplyOrder.Note, SOut.Double(supplyOrder.AmountTotal), SOut.Long(supplyOrder.UserNum), SOut.Double(supplyOrder.ShippingCharge), SOut.DateT(supplyOrder.DateReceived, false));
        return table;
    }

    public static long Insert(SupplyOrder supplyOrder)
    {
        return Insert(supplyOrder, false);
    }

    public static long Insert(SupplyOrder supplyOrder, bool useExistingPK)
    {
        var command = "INSERT INTO supplyorder (";

        command += "SupplierNum,DatePlaced,Note,AmountTotal,UserNum,ShippingCharge,DateReceived) VALUES(";

        command +=
            SOut.Long(supplyOrder.SupplierNum) + ","
                                               + SOut.Date(supplyOrder.DatePlaced) + ","
                                               + DbHelper.ParamChar + "paramNote,"
                                               + SOut.Double(supplyOrder.AmountTotal) + ","
                                               + SOut.Long(supplyOrder.UserNum) + ","
                                               + SOut.Double(supplyOrder.ShippingCharge) + ","
                                               + SOut.Date(supplyOrder.DateReceived) + ")";
        if (supplyOrder.Note == null) supplyOrder.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplyOrder.Note));
        {
            supplyOrder.SupplyOrderNum = Db.NonQ(command, true, "SupplyOrderNum", "supplyOrder", paramNote);
        }
        return supplyOrder.SupplyOrderNum;
    }

    public static long InsertNoCache(SupplyOrder supplyOrder)
    {
        return InsertNoCache(supplyOrder, false);
    }

    public static long InsertNoCache(SupplyOrder supplyOrder, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO supplyorder (";
        if (isRandomKeys || useExistingPK) command += "SupplyOrderNum,";
        command += "SupplierNum,DatePlaced,Note,AmountTotal,UserNum,ShippingCharge,DateReceived) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(supplyOrder.SupplyOrderNum) + ",";
        command +=
            SOut.Long(supplyOrder.SupplierNum) + ","
                                               + SOut.Date(supplyOrder.DatePlaced) + ","
                                               + DbHelper.ParamChar + "paramNote,"
                                               + SOut.Double(supplyOrder.AmountTotal) + ","
                                               + SOut.Long(supplyOrder.UserNum) + ","
                                               + SOut.Double(supplyOrder.ShippingCharge) + ","
                                               + SOut.Date(supplyOrder.DateReceived) + ")";
        if (supplyOrder.Note == null) supplyOrder.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplyOrder.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            supplyOrder.SupplyOrderNum = Db.NonQ(command, true, "SupplyOrderNum", "supplyOrder", paramNote);
        return supplyOrder.SupplyOrderNum;
    }

    public static void Update(SupplyOrder supplyOrder)
    {
        var command = "UPDATE supplyorder SET "
                      + "SupplierNum   =  " + SOut.Long(supplyOrder.SupplierNum) + ", "
                      + "DatePlaced    =  " + SOut.Date(supplyOrder.DatePlaced) + ", "
                      + "Note          =  " + DbHelper.ParamChar + "paramNote, "
                      + "AmountTotal   =  " + SOut.Double(supplyOrder.AmountTotal) + ", "
                      + "UserNum       =  " + SOut.Long(supplyOrder.UserNum) + ", "
                      + "ShippingCharge=  " + SOut.Double(supplyOrder.ShippingCharge) + ", "
                      + "DateReceived  =  " + SOut.Date(supplyOrder.DateReceived) + " "
                      + "WHERE SupplyOrderNum = " + SOut.Long(supplyOrder.SupplyOrderNum);
        if (supplyOrder.Note == null) supplyOrder.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplyOrder.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(SupplyOrder supplyOrder, SupplyOrder oldSupplyOrder)
    {
        var command = "";
        if (supplyOrder.SupplierNum != oldSupplyOrder.SupplierNum)
        {
            if (command != "") command += ",";
            command += "SupplierNum = " + SOut.Long(supplyOrder.SupplierNum) + "";
        }

        if (supplyOrder.DatePlaced.Date != oldSupplyOrder.DatePlaced.Date)
        {
            if (command != "") command += ",";
            command += "DatePlaced = " + SOut.Date(supplyOrder.DatePlaced) + "";
        }

        if (supplyOrder.Note != oldSupplyOrder.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (supplyOrder.AmountTotal != oldSupplyOrder.AmountTotal)
        {
            if (command != "") command += ",";
            command += "AmountTotal = " + SOut.Double(supplyOrder.AmountTotal) + "";
        }

        if (supplyOrder.UserNum != oldSupplyOrder.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(supplyOrder.UserNum) + "";
        }

        if (supplyOrder.ShippingCharge != oldSupplyOrder.ShippingCharge)
        {
            if (command != "") command += ",";
            command += "ShippingCharge = " + SOut.Double(supplyOrder.ShippingCharge) + "";
        }

        if (supplyOrder.DateReceived.Date != oldSupplyOrder.DateReceived.Date)
        {
            if (command != "") command += ",";
            command += "DateReceived = " + SOut.Date(supplyOrder.DateReceived) + "";
        }

        if (command == "") return false;
        if (supplyOrder.Note == null) supplyOrder.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplyOrder.Note));
        command = "UPDATE supplyorder SET " + command
                                            + " WHERE SupplyOrderNum = " + SOut.Long(supplyOrder.SupplyOrderNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(SupplyOrder supplyOrder, SupplyOrder oldSupplyOrder)
    {
        if (supplyOrder.SupplierNum != oldSupplyOrder.SupplierNum) return true;
        if (supplyOrder.DatePlaced.Date != oldSupplyOrder.DatePlaced.Date) return true;
        if (supplyOrder.Note != oldSupplyOrder.Note) return true;
        if (supplyOrder.AmountTotal != oldSupplyOrder.AmountTotal) return true;
        if (supplyOrder.UserNum != oldSupplyOrder.UserNum) return true;
        if (supplyOrder.ShippingCharge != oldSupplyOrder.ShippingCharge) return true;
        if (supplyOrder.DateReceived.Date != oldSupplyOrder.DateReceived.Date) return true;
        return false;
    }

    public static void Delete(long supplyOrderNum)
    {
        var command = "DELETE FROM supplyorder "
                      + "WHERE SupplyOrderNum = " + SOut.Long(supplyOrderNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSupplyOrderNums)
    {
        if (listSupplyOrderNums == null || listSupplyOrderNums.Count == 0) return;
        var command = "DELETE FROM supplyorder "
                      + "WHERE SupplyOrderNum IN(" + string.Join(",", listSupplyOrderNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}