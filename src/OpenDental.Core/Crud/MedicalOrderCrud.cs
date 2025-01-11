#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedicalOrderCrud
{
    public static MedicalOrder SelectOne(long medicalOrderNum)
    {
        var command = "SELECT * FROM medicalorder "
                      + "WHERE MedicalOrderNum = " + SOut.Long(medicalOrderNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedicalOrder SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedicalOrder> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedicalOrder> TableToList(DataTable table)
    {
        var retVal = new List<MedicalOrder>();
        MedicalOrder medicalOrder;
        foreach (DataRow row in table.Rows)
        {
            medicalOrder = new MedicalOrder();
            medicalOrder.MedicalOrderNum = SIn.Long(row["MedicalOrderNum"].ToString());
            medicalOrder.MedOrderType = (MedicalOrderType) SIn.Int(row["MedOrderType"].ToString());
            medicalOrder.PatNum = SIn.Long(row["PatNum"].ToString());
            medicalOrder.DateTimeOrder = SIn.DateTime(row["DateTimeOrder"].ToString());
            medicalOrder.Description = SIn.String(row["Description"].ToString());
            medicalOrder.IsDiscontinued = SIn.Bool(row["IsDiscontinued"].ToString());
            medicalOrder.ProvNum = SIn.Long(row["ProvNum"].ToString());
            retVal.Add(medicalOrder);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedicalOrder> listMedicalOrders, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedicalOrder";
        var table = new DataTable(tableName);
        table.Columns.Add("MedicalOrderNum");
        table.Columns.Add("MedOrderType");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateTimeOrder");
        table.Columns.Add("Description");
        table.Columns.Add("IsDiscontinued");
        table.Columns.Add("ProvNum");
        foreach (var medicalOrder in listMedicalOrders)
            table.Rows.Add(SOut.Long(medicalOrder.MedicalOrderNum), SOut.Int((int) medicalOrder.MedOrderType), SOut.Long(medicalOrder.PatNum), SOut.DateT(medicalOrder.DateTimeOrder, false), medicalOrder.Description, SOut.Bool(medicalOrder.IsDiscontinued), SOut.Long(medicalOrder.ProvNum));
        return table;
    }

    public static long Insert(MedicalOrder medicalOrder)
    {
        return Insert(medicalOrder, false);
    }

    public static long Insert(MedicalOrder medicalOrder, bool useExistingPK)
    {
        var command = "INSERT INTO medicalorder (";

        command += "MedOrderType,PatNum,DateTimeOrder,Description,IsDiscontinued,ProvNum) VALUES(";

        command +=
            SOut.Int((int) medicalOrder.MedOrderType) + ","
                                                      + SOut.Long(medicalOrder.PatNum) + ","
                                                      + SOut.DateT(medicalOrder.DateTimeOrder) + ","
                                                      + "'" + SOut.String(medicalOrder.Description) + "',"
                                                      + SOut.Bool(medicalOrder.IsDiscontinued) + ","
                                                      + SOut.Long(medicalOrder.ProvNum) + ")";
        {
            medicalOrder.MedicalOrderNum = Db.NonQ(command, true, "MedicalOrderNum", "medicalOrder");
        }
        return medicalOrder.MedicalOrderNum;
    }

    public static long InsertNoCache(MedicalOrder medicalOrder)
    {
        return InsertNoCache(medicalOrder, false);
    }

    public static long InsertNoCache(MedicalOrder medicalOrder, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medicalorder (";
        if (isRandomKeys || useExistingPK) command += "MedicalOrderNum,";
        command += "MedOrderType,PatNum,DateTimeOrder,Description,IsDiscontinued,ProvNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medicalOrder.MedicalOrderNum) + ",";
        command +=
            SOut.Int((int) medicalOrder.MedOrderType) + ","
                                                      + SOut.Long(medicalOrder.PatNum) + ","
                                                      + SOut.DateT(medicalOrder.DateTimeOrder) + ","
                                                      + "'" + SOut.String(medicalOrder.Description) + "',"
                                                      + SOut.Bool(medicalOrder.IsDiscontinued) + ","
                                                      + SOut.Long(medicalOrder.ProvNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            medicalOrder.MedicalOrderNum = Db.NonQ(command, true, "MedicalOrderNum", "medicalOrder");
        return medicalOrder.MedicalOrderNum;
    }

    public static void Update(MedicalOrder medicalOrder)
    {
        var command = "UPDATE medicalorder SET "
                      + "MedOrderType   =  " + SOut.Int((int) medicalOrder.MedOrderType) + ", "
                      + "PatNum         =  " + SOut.Long(medicalOrder.PatNum) + ", "
                      + "DateTimeOrder  =  " + SOut.DateT(medicalOrder.DateTimeOrder) + ", "
                      + "Description    = '" + SOut.String(medicalOrder.Description) + "', "
                      + "IsDiscontinued =  " + SOut.Bool(medicalOrder.IsDiscontinued) + ", "
                      + "ProvNum        =  " + SOut.Long(medicalOrder.ProvNum) + " "
                      + "WHERE MedicalOrderNum = " + SOut.Long(medicalOrder.MedicalOrderNum);
        Db.NonQ(command);
    }

    public static bool Update(MedicalOrder medicalOrder, MedicalOrder oldMedicalOrder)
    {
        var command = "";
        if (medicalOrder.MedOrderType != oldMedicalOrder.MedOrderType)
        {
            if (command != "") command += ",";
            command += "MedOrderType = " + SOut.Int((int) medicalOrder.MedOrderType) + "";
        }

        if (medicalOrder.PatNum != oldMedicalOrder.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(medicalOrder.PatNum) + "";
        }

        if (medicalOrder.DateTimeOrder != oldMedicalOrder.DateTimeOrder)
        {
            if (command != "") command += ",";
            command += "DateTimeOrder = " + SOut.DateT(medicalOrder.DateTimeOrder) + "";
        }

        if (medicalOrder.Description != oldMedicalOrder.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(medicalOrder.Description) + "'";
        }

        if (medicalOrder.IsDiscontinued != oldMedicalOrder.IsDiscontinued)
        {
            if (command != "") command += ",";
            command += "IsDiscontinued = " + SOut.Bool(medicalOrder.IsDiscontinued) + "";
        }

        if (medicalOrder.ProvNum != oldMedicalOrder.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(medicalOrder.ProvNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE medicalorder SET " + command
                                             + " WHERE MedicalOrderNum = " + SOut.Long(medicalOrder.MedicalOrderNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MedicalOrder medicalOrder, MedicalOrder oldMedicalOrder)
    {
        if (medicalOrder.MedOrderType != oldMedicalOrder.MedOrderType) return true;
        if (medicalOrder.PatNum != oldMedicalOrder.PatNum) return true;
        if (medicalOrder.DateTimeOrder != oldMedicalOrder.DateTimeOrder) return true;
        if (medicalOrder.Description != oldMedicalOrder.Description) return true;
        if (medicalOrder.IsDiscontinued != oldMedicalOrder.IsDiscontinued) return true;
        if (medicalOrder.ProvNum != oldMedicalOrder.ProvNum) return true;
        return false;
    }

    public static void Delete(long medicalOrderNum)
    {
        var command = "DELETE FROM medicalorder "
                      + "WHERE MedicalOrderNum = " + SOut.Long(medicalOrderNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedicalOrderNums)
    {
        if (listMedicalOrderNums == null || listMedicalOrderNums.Count == 0) return;
        var command = "DELETE FROM medicalorder "
                      + "WHERE MedicalOrderNum IN(" + string.Join(",", listMedicalOrderNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}