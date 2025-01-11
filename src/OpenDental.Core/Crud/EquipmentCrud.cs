#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EquipmentCrud
{
    public static Equipment SelectOne(long equipmentNum)
    {
        var command = "SELECT * FROM equipment "
                      + "WHERE EquipmentNum = " + SOut.Long(equipmentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Equipment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Equipment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Equipment> TableToList(DataTable table)
    {
        var retVal = new List<Equipment>();
        Equipment equipment;
        foreach (DataRow row in table.Rows)
        {
            equipment = new Equipment();
            equipment.EquipmentNum = SIn.Long(row["EquipmentNum"].ToString());
            equipment.Description = SIn.String(row["Description"].ToString());
            equipment.SerialNumber = SIn.String(row["SerialNumber"].ToString());
            equipment.ModelYear = SIn.String(row["ModelYear"].ToString());
            equipment.DatePurchased = SIn.Date(row["DatePurchased"].ToString());
            equipment.DateSold = SIn.Date(row["DateSold"].ToString());
            equipment.PurchaseCost = SIn.Double(row["PurchaseCost"].ToString());
            equipment.MarketValue = SIn.Double(row["MarketValue"].ToString());
            equipment.Location = SIn.String(row["Location"].ToString());
            equipment.DateEntry = SIn.Date(row["DateEntry"].ToString());
            equipment.ProvNumCheckedOut = SIn.Long(row["ProvNumCheckedOut"].ToString());
            equipment.DateCheckedOut = SIn.Date(row["DateCheckedOut"].ToString());
            equipment.DateExpectedBack = SIn.Date(row["DateExpectedBack"].ToString());
            equipment.DispenseNote = SIn.String(row["DispenseNote"].ToString());
            equipment.Status = SIn.String(row["Status"].ToString());
            retVal.Add(equipment);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Equipment> listEquipments, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Equipment";
        var table = new DataTable(tableName);
        table.Columns.Add("EquipmentNum");
        table.Columns.Add("Description");
        table.Columns.Add("SerialNumber");
        table.Columns.Add("ModelYear");
        table.Columns.Add("DatePurchased");
        table.Columns.Add("DateSold");
        table.Columns.Add("PurchaseCost");
        table.Columns.Add("MarketValue");
        table.Columns.Add("Location");
        table.Columns.Add("DateEntry");
        table.Columns.Add("ProvNumCheckedOut");
        table.Columns.Add("DateCheckedOut");
        table.Columns.Add("DateExpectedBack");
        table.Columns.Add("DispenseNote");
        table.Columns.Add("Status");
        foreach (var equipment in listEquipments)
            table.Rows.Add(SOut.Long(equipment.EquipmentNum), equipment.Description, equipment.SerialNumber, equipment.ModelYear, SOut.DateT(equipment.DatePurchased, false), SOut.DateT(equipment.DateSold, false), SOut.Double(equipment.PurchaseCost), SOut.Double(equipment.MarketValue), equipment.Location, SOut.DateT(equipment.DateEntry, false), SOut.Long(equipment.ProvNumCheckedOut), SOut.DateT(equipment.DateCheckedOut, false), SOut.DateT(equipment.DateExpectedBack, false), equipment.DispenseNote, equipment.Status);
        return table;
    }

    public static long Insert(Equipment equipment)
    {
        return Insert(equipment, false);
    }

    public static long Insert(Equipment equipment, bool useExistingPK)
    {
        var command = "INSERT INTO equipment (";

        command += "Description,SerialNumber,ModelYear,DatePurchased,DateSold,PurchaseCost,MarketValue,Location,DateEntry,ProvNumCheckedOut,DateCheckedOut,DateExpectedBack,DispenseNote,Status) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + "'" + SOut.String(equipment.SerialNumber) + "',"
                               + "'" + SOut.String(equipment.ModelYear) + "',"
                               + SOut.Date(equipment.DatePurchased) + ","
                               + SOut.Date(equipment.DateSold) + ","
                               + SOut.Double(equipment.PurchaseCost) + ","
                               + SOut.Double(equipment.MarketValue) + ","
                               + DbHelper.ParamChar + "paramLocation,"
                               + SOut.Date(equipment.DateEntry) + ","
                               + SOut.Long(equipment.ProvNumCheckedOut) + ","
                               + SOut.Date(equipment.DateCheckedOut) + ","
                               + SOut.Date(equipment.DateExpectedBack) + ","
                               + DbHelper.ParamChar + "paramDispenseNote,"
                               + DbHelper.ParamChar + "paramStatus)";
        if (equipment.Description == null) equipment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(equipment.Description));
        if (equipment.Location == null) equipment.Location = "";
        var paramLocation = new OdSqlParameter("paramLocation", OdDbType.Text, SOut.StringParam(equipment.Location));
        if (equipment.DispenseNote == null) equipment.DispenseNote = "";
        var paramDispenseNote = new OdSqlParameter("paramDispenseNote", OdDbType.Text, SOut.StringNote(equipment.DispenseNote));
        if (equipment.Status == null) equipment.Status = "";
        var paramStatus = new OdSqlParameter("paramStatus", OdDbType.Text, SOut.StringParam(equipment.Status));
        {
            equipment.EquipmentNum = Db.NonQ(command, true, "EquipmentNum", "equipment", paramDescription, paramLocation, paramDispenseNote, paramStatus);
        }
        return equipment.EquipmentNum;
    }

    public static long InsertNoCache(Equipment equipment)
    {
        return InsertNoCache(equipment, false);
    }

    public static long InsertNoCache(Equipment equipment, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO equipment (";
        if (isRandomKeys || useExistingPK) command += "EquipmentNum,";
        command += "Description,SerialNumber,ModelYear,DatePurchased,DateSold,PurchaseCost,MarketValue,Location,DateEntry,ProvNumCheckedOut,DateCheckedOut,DateExpectedBack,DispenseNote,Status) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(equipment.EquipmentNum) + ",";
        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + "'" + SOut.String(equipment.SerialNumber) + "',"
                               + "'" + SOut.String(equipment.ModelYear) + "',"
                               + SOut.Date(equipment.DatePurchased) + ","
                               + SOut.Date(equipment.DateSold) + ","
                               + SOut.Double(equipment.PurchaseCost) + ","
                               + SOut.Double(equipment.MarketValue) + ","
                               + DbHelper.ParamChar + "paramLocation,"
                               + SOut.Date(equipment.DateEntry) + ","
                               + SOut.Long(equipment.ProvNumCheckedOut) + ","
                               + SOut.Date(equipment.DateCheckedOut) + ","
                               + SOut.Date(equipment.DateExpectedBack) + ","
                               + DbHelper.ParamChar + "paramDispenseNote,"
                               + DbHelper.ParamChar + "paramStatus)";
        if (equipment.Description == null) equipment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(equipment.Description));
        if (equipment.Location == null) equipment.Location = "";
        var paramLocation = new OdSqlParameter("paramLocation", OdDbType.Text, SOut.StringParam(equipment.Location));
        if (equipment.DispenseNote == null) equipment.DispenseNote = "";
        var paramDispenseNote = new OdSqlParameter("paramDispenseNote", OdDbType.Text, SOut.StringNote(equipment.DispenseNote));
        if (equipment.Status == null) equipment.Status = "";
        var paramStatus = new OdSqlParameter("paramStatus", OdDbType.Text, SOut.StringParam(equipment.Status));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription, paramLocation, paramDispenseNote, paramStatus);
        else
            equipment.EquipmentNum = Db.NonQ(command, true, "EquipmentNum", "equipment", paramDescription, paramLocation, paramDispenseNote, paramStatus);
        return equipment.EquipmentNum;
    }

    public static void Update(Equipment equipment)
    {
        var command = "UPDATE equipment SET "
                      + "Description      =  " + DbHelper.ParamChar + "paramDescription, "
                      + "SerialNumber     = '" + SOut.String(equipment.SerialNumber) + "', "
                      + "ModelYear        = '" + SOut.String(equipment.ModelYear) + "', "
                      + "DatePurchased    =  " + SOut.Date(equipment.DatePurchased) + ", "
                      + "DateSold         =  " + SOut.Date(equipment.DateSold) + ", "
                      + "PurchaseCost     =  " + SOut.Double(equipment.PurchaseCost) + ", "
                      + "MarketValue      =  " + SOut.Double(equipment.MarketValue) + ", "
                      + "Location         =  " + DbHelper.ParamChar + "paramLocation, "
                      + "DateEntry        =  " + SOut.Date(equipment.DateEntry) + ", "
                      + "ProvNumCheckedOut=  " + SOut.Long(equipment.ProvNumCheckedOut) + ", "
                      + "DateCheckedOut   =  " + SOut.Date(equipment.DateCheckedOut) + ", "
                      + "DateExpectedBack =  " + SOut.Date(equipment.DateExpectedBack) + ", "
                      + "DispenseNote     =  " + DbHelper.ParamChar + "paramDispenseNote, "
                      + "Status           =  " + DbHelper.ParamChar + "paramStatus "
                      + "WHERE EquipmentNum = " + SOut.Long(equipment.EquipmentNum);
        if (equipment.Description == null) equipment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(equipment.Description));
        if (equipment.Location == null) equipment.Location = "";
        var paramLocation = new OdSqlParameter("paramLocation", OdDbType.Text, SOut.StringParam(equipment.Location));
        if (equipment.DispenseNote == null) equipment.DispenseNote = "";
        var paramDispenseNote = new OdSqlParameter("paramDispenseNote", OdDbType.Text, SOut.StringNote(equipment.DispenseNote));
        if (equipment.Status == null) equipment.Status = "";
        var paramStatus = new OdSqlParameter("paramStatus", OdDbType.Text, SOut.StringParam(equipment.Status));
        Db.NonQ(command, paramDescription, paramLocation, paramDispenseNote, paramStatus);
    }

    public static bool Update(Equipment equipment, Equipment oldEquipment)
    {
        var command = "";
        if (equipment.Description != oldEquipment.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (equipment.SerialNumber != oldEquipment.SerialNumber)
        {
            if (command != "") command += ",";
            command += "SerialNumber = '" + SOut.String(equipment.SerialNumber) + "'";
        }

        if (equipment.ModelYear != oldEquipment.ModelYear)
        {
            if (command != "") command += ",";
            command += "ModelYear = '" + SOut.String(equipment.ModelYear) + "'";
        }

        if (equipment.DatePurchased.Date != oldEquipment.DatePurchased.Date)
        {
            if (command != "") command += ",";
            command += "DatePurchased = " + SOut.Date(equipment.DatePurchased) + "";
        }

        if (equipment.DateSold.Date != oldEquipment.DateSold.Date)
        {
            if (command != "") command += ",";
            command += "DateSold = " + SOut.Date(equipment.DateSold) + "";
        }

        if (equipment.PurchaseCost != oldEquipment.PurchaseCost)
        {
            if (command != "") command += ",";
            command += "PurchaseCost = " + SOut.Double(equipment.PurchaseCost) + "";
        }

        if (equipment.MarketValue != oldEquipment.MarketValue)
        {
            if (command != "") command += ",";
            command += "MarketValue = " + SOut.Double(equipment.MarketValue) + "";
        }

        if (equipment.Location != oldEquipment.Location)
        {
            if (command != "") command += ",";
            command += "Location = " + DbHelper.ParamChar + "paramLocation";
        }

        if (equipment.DateEntry.Date != oldEquipment.DateEntry.Date)
        {
            if (command != "") command += ",";
            command += "DateEntry = " + SOut.Date(equipment.DateEntry) + "";
        }

        if (equipment.ProvNumCheckedOut != oldEquipment.ProvNumCheckedOut)
        {
            if (command != "") command += ",";
            command += "ProvNumCheckedOut = " + SOut.Long(equipment.ProvNumCheckedOut) + "";
        }

        if (equipment.DateCheckedOut.Date != oldEquipment.DateCheckedOut.Date)
        {
            if (command != "") command += ",";
            command += "DateCheckedOut = " + SOut.Date(equipment.DateCheckedOut) + "";
        }

        if (equipment.DateExpectedBack.Date != oldEquipment.DateExpectedBack.Date)
        {
            if (command != "") command += ",";
            command += "DateExpectedBack = " + SOut.Date(equipment.DateExpectedBack) + "";
        }

        if (equipment.DispenseNote != oldEquipment.DispenseNote)
        {
            if (command != "") command += ",";
            command += "DispenseNote = " + DbHelper.ParamChar + "paramDispenseNote";
        }

        if (equipment.Status != oldEquipment.Status)
        {
            if (command != "") command += ",";
            command += "Status = " + DbHelper.ParamChar + "paramStatus";
        }

        if (command == "") return false;
        if (equipment.Description == null) equipment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(equipment.Description));
        if (equipment.Location == null) equipment.Location = "";
        var paramLocation = new OdSqlParameter("paramLocation", OdDbType.Text, SOut.StringParam(equipment.Location));
        if (equipment.DispenseNote == null) equipment.DispenseNote = "";
        var paramDispenseNote = new OdSqlParameter("paramDispenseNote", OdDbType.Text, SOut.StringNote(equipment.DispenseNote));
        if (equipment.Status == null) equipment.Status = "";
        var paramStatus = new OdSqlParameter("paramStatus", OdDbType.Text, SOut.StringParam(equipment.Status));
        command = "UPDATE equipment SET " + command
                                          + " WHERE EquipmentNum = " + SOut.Long(equipment.EquipmentNum);
        Db.NonQ(command, paramDescription, paramLocation, paramDispenseNote, paramStatus);
        return true;
    }

    public static bool UpdateComparison(Equipment equipment, Equipment oldEquipment)
    {
        if (equipment.Description != oldEquipment.Description) return true;
        if (equipment.SerialNumber != oldEquipment.SerialNumber) return true;
        if (equipment.ModelYear != oldEquipment.ModelYear) return true;
        if (equipment.DatePurchased.Date != oldEquipment.DatePurchased.Date) return true;
        if (equipment.DateSold.Date != oldEquipment.DateSold.Date) return true;
        if (equipment.PurchaseCost != oldEquipment.PurchaseCost) return true;
        if (equipment.MarketValue != oldEquipment.MarketValue) return true;
        if (equipment.Location != oldEquipment.Location) return true;
        if (equipment.DateEntry.Date != oldEquipment.DateEntry.Date) return true;
        if (equipment.ProvNumCheckedOut != oldEquipment.ProvNumCheckedOut) return true;
        if (equipment.DateCheckedOut.Date != oldEquipment.DateCheckedOut.Date) return true;
        if (equipment.DateExpectedBack.Date != oldEquipment.DateExpectedBack.Date) return true;
        if (equipment.DispenseNote != oldEquipment.DispenseNote) return true;
        if (equipment.Status != oldEquipment.Status) return true;
        return false;
    }

    public static void Delete(long equipmentNum)
    {
        var command = "DELETE FROM equipment "
                      + "WHERE EquipmentNum = " + SOut.Long(equipmentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEquipmentNums)
    {
        if (listEquipmentNums == null || listEquipmentNums.Count == 0) return;
        var command = "DELETE FROM equipment "
                      + "WHERE EquipmentNum IN(" + string.Join(",", listEquipmentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}