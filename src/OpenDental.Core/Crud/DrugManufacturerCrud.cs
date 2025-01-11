#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DrugManufacturerCrud
{
    public static DrugManufacturer SelectOne(long drugManufacturerNum)
    {
        var command = "SELECT * FROM drugmanufacturer "
                      + "WHERE DrugManufacturerNum = " + SOut.Long(drugManufacturerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DrugManufacturer SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DrugManufacturer> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DrugManufacturer> TableToList(DataTable table)
    {
        var retVal = new List<DrugManufacturer>();
        DrugManufacturer drugManufacturer;
        foreach (DataRow row in table.Rows)
        {
            drugManufacturer = new DrugManufacturer();
            drugManufacturer.DrugManufacturerNum = SIn.Long(row["DrugManufacturerNum"].ToString());
            drugManufacturer.ManufacturerName = SIn.String(row["ManufacturerName"].ToString());
            drugManufacturer.ManufacturerCode = SIn.String(row["ManufacturerCode"].ToString());
            retVal.Add(drugManufacturer);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DrugManufacturer> listDrugManufacturers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DrugManufacturer";
        var table = new DataTable(tableName);
        table.Columns.Add("DrugManufacturerNum");
        table.Columns.Add("ManufacturerName");
        table.Columns.Add("ManufacturerCode");
        foreach (var drugManufacturer in listDrugManufacturers)
            table.Rows.Add(SOut.Long(drugManufacturer.DrugManufacturerNum), drugManufacturer.ManufacturerName, drugManufacturer.ManufacturerCode);
        return table;
    }

    public static long Insert(DrugManufacturer drugManufacturer)
    {
        return Insert(drugManufacturer, false);
    }

    public static long Insert(DrugManufacturer drugManufacturer, bool useExistingPK)
    {
        var command = "INSERT INTO drugmanufacturer (";

        command += "ManufacturerName,ManufacturerCode) VALUES(";

        command +=
            "'" + SOut.String(drugManufacturer.ManufacturerName) + "',"
            + "'" + SOut.String(drugManufacturer.ManufacturerCode) + "')";
        {
            drugManufacturer.DrugManufacturerNum = Db.NonQ(command, true, "DrugManufacturerNum", "drugManufacturer");
        }
        return drugManufacturer.DrugManufacturerNum;
    }

    public static long InsertNoCache(DrugManufacturer drugManufacturer)
    {
        return InsertNoCache(drugManufacturer, false);
    }

    public static long InsertNoCache(DrugManufacturer drugManufacturer, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO drugmanufacturer (";
        if (isRandomKeys || useExistingPK) command += "DrugManufacturerNum,";
        command += "ManufacturerName,ManufacturerCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(drugManufacturer.DrugManufacturerNum) + ",";
        command +=
            "'" + SOut.String(drugManufacturer.ManufacturerName) + "',"
            + "'" + SOut.String(drugManufacturer.ManufacturerCode) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            drugManufacturer.DrugManufacturerNum = Db.NonQ(command, true, "DrugManufacturerNum", "drugManufacturer");
        return drugManufacturer.DrugManufacturerNum;
    }

    public static void Update(DrugManufacturer drugManufacturer)
    {
        var command = "UPDATE drugmanufacturer SET "
                      + "ManufacturerName   = '" + SOut.String(drugManufacturer.ManufacturerName) + "', "
                      + "ManufacturerCode   = '" + SOut.String(drugManufacturer.ManufacturerCode) + "' "
                      + "WHERE DrugManufacturerNum = " + SOut.Long(drugManufacturer.DrugManufacturerNum);
        Db.NonQ(command);
    }

    public static bool Update(DrugManufacturer drugManufacturer, DrugManufacturer oldDrugManufacturer)
    {
        var command = "";
        if (drugManufacturer.ManufacturerName != oldDrugManufacturer.ManufacturerName)
        {
            if (command != "") command += ",";
            command += "ManufacturerName = '" + SOut.String(drugManufacturer.ManufacturerName) + "'";
        }

        if (drugManufacturer.ManufacturerCode != oldDrugManufacturer.ManufacturerCode)
        {
            if (command != "") command += ",";
            command += "ManufacturerCode = '" + SOut.String(drugManufacturer.ManufacturerCode) + "'";
        }

        if (command == "") return false;
        command = "UPDATE drugmanufacturer SET " + command
                                                 + " WHERE DrugManufacturerNum = " + SOut.Long(drugManufacturer.DrugManufacturerNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DrugManufacturer drugManufacturer, DrugManufacturer oldDrugManufacturer)
    {
        if (drugManufacturer.ManufacturerName != oldDrugManufacturer.ManufacturerName) return true;
        if (drugManufacturer.ManufacturerCode != oldDrugManufacturer.ManufacturerCode) return true;
        return false;
    }

    public static void Delete(long drugManufacturerNum)
    {
        var command = "DELETE FROM drugmanufacturer "
                      + "WHERE DrugManufacturerNum = " + SOut.Long(drugManufacturerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDrugManufacturerNums)
    {
        if (listDrugManufacturerNums == null || listDrugManufacturerNums.Count == 0) return;
        var command = "DELETE FROM drugmanufacturer "
                      + "WHERE DrugManufacturerNum IN(" + string.Join(",", listDrugManufacturerNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}