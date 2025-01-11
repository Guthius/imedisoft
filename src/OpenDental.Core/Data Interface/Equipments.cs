using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Equipments
{
    
    public static List<Equipment> GetList(DateTime dateFrom, DateTime dateTo, EnumEquipmentDisplayMode enumEquipmentDisplayMode, string snDescLoc)
    {
        var command = "";
        if (enumEquipmentDisplayMode == EnumEquipmentDisplayMode.Purchased)
            command = "SELECT * FROM equipment "
                      + "WHERE DatePurchased >= " + SOut.Date(dateFrom)
                      + " AND DatePurchased <= " + SOut.Date(dateTo)
                      + " AND (SerialNumber LIKE '%" + SOut.String(snDescLoc) + "%' OR Description LIKE '%" + SOut.String(snDescLoc) + "%' OR Location LIKE '%" + SOut.String(snDescLoc) + "%')"
                      + " ORDER BY DatePurchased";
        if (enumEquipmentDisplayMode == EnumEquipmentDisplayMode.Sold)
            command = "SELECT * FROM equipment "
                      + "WHERE DateSold >= " + SOut.Date(dateFrom)
                      + " AND DateSold <= " + SOut.Date(dateTo)
                      + " AND (SerialNumber LIKE '%" + SOut.String(snDescLoc) + "%' OR Description LIKE '%" + SOut.String(snDescLoc) + "%' OR Location LIKE '%" + SOut.String(snDescLoc) + "%')"
                      + " ORDER BY DatePurchased";
        if (enumEquipmentDisplayMode == EnumEquipmentDisplayMode.All)
            command = "SELECT * FROM equipment "
                      + "WHERE ((DatePurchased >= " + SOut.Date(dateFrom) + " AND DatePurchased <= " + SOut.Date(dateTo) + ")"
                      + " OR (DateSold >= " + SOut.Date(dateFrom) + " AND DateSold <= " + SOut.Date(dateTo) + "))"
                      + " AND (SerialNumber LIKE '%" + SOut.String(snDescLoc) + "%' OR Description LIKE '%" + SOut.String(snDescLoc) + "%' OR Location LIKE '%" + SOut.String(snDescLoc) + "%')"
                      + " ORDER BY DatePurchased";
        return EquipmentCrud.SelectMany(command);
    }

    
    public static long Insert(Equipment equipment)
    {
        return EquipmentCrud.Insert(equipment);
    }

    
    public static void Update(Equipment equipment)
    {
        EquipmentCrud.Update(equipment);
    }

    
    public static void Delete(Equipment equipment)
    {
        var command = "DELETE FROM equipment"
                      + " WHERE EquipmentNum = " + SOut.Long(equipment.EquipmentNum);
        Db.NonQ(command);
    }

    ///<summary>Generates a unique 3 char alphanumeric serialnumber.  Checks to make sure it's not already in use.</summary>
    public static string GenerateSerialNum()
    {
        var retVal = "";
        var random = new Random();
        while (true)
        {
            retVal = "";
            for (var i = 0; i < 4; i++)
            {
                var r = random.Next(0, 34);
                if (r < 9)
                    retVal += (char) ('1' + r); //1-9, no zero
                else
                    retVal += (char) ('A' + r - 9);
            }

            var command = "SELECT COUNT(*) FROM equipment WHERE SerialNumber = '" + SOut.String(retVal) + "'";
            if (DataCore.GetScalar(command) == "0") break;
        }

        return retVal;
    }

    ///<summary>Checks the database for equipment that has the supplied serial number.</summary>
    public static bool HasExisting(Equipment equipment)
    {
        var command = "SELECT COUNT(*) FROM equipment WHERE SerialNumber = '" + SOut.String(equipment.SerialNumber) + "' AND EquipmentNum != " + SOut.Long(equipment.EquipmentNum);
        if (DataCore.GetScalar(command) == "0") return false;
        return true;
    }
}

public enum EnumEquipmentDisplayMode
{
    Purchased,
    Sold,
    All
}