#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PharmacyCrud
{
    public static Pharmacy SelectOne(long pharmacyNum)
    {
        var command = "SELECT * FROM pharmacy "
                      + "WHERE PharmacyNum = " + SOut.Long(pharmacyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Pharmacy SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Pharmacy> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Pharmacy> TableToList(DataTable table)
    {
        var retVal = new List<Pharmacy>();
        Pharmacy pharmacy;
        foreach (DataRow row in table.Rows)
        {
            pharmacy = new Pharmacy();
            pharmacy.PharmacyNum = SIn.Long(row["PharmacyNum"].ToString());
            pharmacy.PharmID = SIn.String(row["PharmID"].ToString());
            pharmacy.StoreName = SIn.String(row["StoreName"].ToString());
            pharmacy.Phone = SIn.String(row["Phone"].ToString());
            pharmacy.Fax = SIn.String(row["Fax"].ToString());
            pharmacy.Address = SIn.String(row["Address"].ToString());
            pharmacy.Address2 = SIn.String(row["Address2"].ToString());
            pharmacy.City = SIn.String(row["City"].ToString());
            pharmacy.State = SIn.String(row["State"].ToString());
            pharmacy.Zip = SIn.String(row["Zip"].ToString());
            pharmacy.Note = SIn.String(row["Note"].ToString());
            pharmacy.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            retVal.Add(pharmacy);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Pharmacy> listPharmacys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Pharmacy";
        var table = new DataTable(tableName);
        table.Columns.Add("PharmacyNum");
        table.Columns.Add("PharmID");
        table.Columns.Add("StoreName");
        table.Columns.Add("Phone");
        table.Columns.Add("Fax");
        table.Columns.Add("Address");
        table.Columns.Add("Address2");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("Note");
        table.Columns.Add("DateTStamp");
        foreach (var pharmacy in listPharmacys)
            table.Rows.Add(SOut.Long(pharmacy.PharmacyNum), pharmacy.PharmID, pharmacy.StoreName, pharmacy.Phone, pharmacy.Fax, pharmacy.Address, pharmacy.Address2, pharmacy.City, pharmacy.State, pharmacy.Zip, pharmacy.Note, SOut.DateT(pharmacy.DateTStamp, false));
        return table;
    }

    public static long Insert(Pharmacy pharmacy)
    {
        return Insert(pharmacy, false);
    }

    public static long Insert(Pharmacy pharmacy, bool useExistingPK)
    {
        var command = "INSERT INTO pharmacy (";

        command += "PharmID,StoreName,Phone,Fax,Address,Address2,City,State,Zip,Note) VALUES(";

        command +=
            "'" + SOut.String(pharmacy.PharmID) + "',"
            + "'" + SOut.String(pharmacy.StoreName) + "',"
            + "'" + SOut.String(pharmacy.Phone) + "',"
            + "'" + SOut.String(pharmacy.Fax) + "',"
            + "'" + SOut.String(pharmacy.Address) + "',"
            + "'" + SOut.String(pharmacy.Address2) + "',"
            + "'" + SOut.String(pharmacy.City) + "',"
            + "'" + SOut.String(pharmacy.State) + "',"
            + "'" + SOut.String(pharmacy.Zip) + "',"
            + DbHelper.ParamChar + "paramNote)";
        //DateTStamp can only be set by MySQL
        if (pharmacy.Note == null) pharmacy.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(pharmacy.Note));
        {
            pharmacy.PharmacyNum = Db.NonQ(command, true, "PharmacyNum", "pharmacy", paramNote);
        }
        return pharmacy.PharmacyNum;
    }

    public static long InsertNoCache(Pharmacy pharmacy)
    {
        return InsertNoCache(pharmacy, false);
    }

    public static long InsertNoCache(Pharmacy pharmacy, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO pharmacy (";
        if (isRandomKeys || useExistingPK) command += "PharmacyNum,";
        command += "PharmID,StoreName,Phone,Fax,Address,Address2,City,State,Zip,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(pharmacy.PharmacyNum) + ",";
        command +=
            "'" + SOut.String(pharmacy.PharmID) + "',"
            + "'" + SOut.String(pharmacy.StoreName) + "',"
            + "'" + SOut.String(pharmacy.Phone) + "',"
            + "'" + SOut.String(pharmacy.Fax) + "',"
            + "'" + SOut.String(pharmacy.Address) + "',"
            + "'" + SOut.String(pharmacy.Address2) + "',"
            + "'" + SOut.String(pharmacy.City) + "',"
            + "'" + SOut.String(pharmacy.State) + "',"
            + "'" + SOut.String(pharmacy.Zip) + "',"
            + DbHelper.ParamChar + "paramNote)";
        //DateTStamp can only be set by MySQL
        if (pharmacy.Note == null) pharmacy.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(pharmacy.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            pharmacy.PharmacyNum = Db.NonQ(command, true, "PharmacyNum", "pharmacy", paramNote);
        return pharmacy.PharmacyNum;
    }

    public static void Update(Pharmacy pharmacy)
    {
        var command = "UPDATE pharmacy SET "
                      + "PharmID    = '" + SOut.String(pharmacy.PharmID) + "', "
                      + "StoreName  = '" + SOut.String(pharmacy.StoreName) + "', "
                      + "Phone      = '" + SOut.String(pharmacy.Phone) + "', "
                      + "Fax        = '" + SOut.String(pharmacy.Fax) + "', "
                      + "Address    = '" + SOut.String(pharmacy.Address) + "', "
                      + "Address2   = '" + SOut.String(pharmacy.Address2) + "', "
                      + "City       = '" + SOut.String(pharmacy.City) + "', "
                      + "State      = '" + SOut.String(pharmacy.State) + "', "
                      + "Zip        = '" + SOut.String(pharmacy.Zip) + "', "
                      + "Note       =  " + DbHelper.ParamChar + "paramNote "
                      //DateTStamp can only be set by MySQL
                      + "WHERE PharmacyNum = " + SOut.Long(pharmacy.PharmacyNum);
        if (pharmacy.Note == null) pharmacy.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(pharmacy.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Pharmacy pharmacy, Pharmacy oldPharmacy)
    {
        var command = "";
        if (pharmacy.PharmID != oldPharmacy.PharmID)
        {
            if (command != "") command += ",";
            command += "PharmID = '" + SOut.String(pharmacy.PharmID) + "'";
        }

        if (pharmacy.StoreName != oldPharmacy.StoreName)
        {
            if (command != "") command += ",";
            command += "StoreName = '" + SOut.String(pharmacy.StoreName) + "'";
        }

        if (pharmacy.Phone != oldPharmacy.Phone)
        {
            if (command != "") command += ",";
            command += "Phone = '" + SOut.String(pharmacy.Phone) + "'";
        }

        if (pharmacy.Fax != oldPharmacy.Fax)
        {
            if (command != "") command += ",";
            command += "Fax = '" + SOut.String(pharmacy.Fax) + "'";
        }

        if (pharmacy.Address != oldPharmacy.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(pharmacy.Address) + "'";
        }

        if (pharmacy.Address2 != oldPharmacy.Address2)
        {
            if (command != "") command += ",";
            command += "Address2 = '" + SOut.String(pharmacy.Address2) + "'";
        }

        if (pharmacy.City != oldPharmacy.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(pharmacy.City) + "'";
        }

        if (pharmacy.State != oldPharmacy.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(pharmacy.State) + "'";
        }

        if (pharmacy.Zip != oldPharmacy.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(pharmacy.Zip) + "'";
        }

        if (pharmacy.Note != oldPharmacy.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        //DateTStamp can only be set by MySQL
        if (command == "") return false;
        if (pharmacy.Note == null) pharmacy.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(pharmacy.Note));
        command = "UPDATE pharmacy SET " + command
                                         + " WHERE PharmacyNum = " + SOut.Long(pharmacy.PharmacyNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Pharmacy pharmacy, Pharmacy oldPharmacy)
    {
        if (pharmacy.PharmID != oldPharmacy.PharmID) return true;
        if (pharmacy.StoreName != oldPharmacy.StoreName) return true;
        if (pharmacy.Phone != oldPharmacy.Phone) return true;
        if (pharmacy.Fax != oldPharmacy.Fax) return true;
        if (pharmacy.Address != oldPharmacy.Address) return true;
        if (pharmacy.Address2 != oldPharmacy.Address2) return true;
        if (pharmacy.City != oldPharmacy.City) return true;
        if (pharmacy.State != oldPharmacy.State) return true;
        if (pharmacy.Zip != oldPharmacy.Zip) return true;
        if (pharmacy.Note != oldPharmacy.Note) return true;
        //DateTStamp can only be set by MySQL
        return false;
    }

    public static void Delete(long pharmacyNum)
    {
        var command = "DELETE FROM pharmacy "
                      + "WHERE PharmacyNum = " + SOut.Long(pharmacyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPharmacyNums)
    {
        if (listPharmacyNums == null || listPharmacyNums.Count == 0) return;
        var command = "DELETE FROM pharmacy "
                      + "WHERE PharmacyNum IN(" + string.Join(",", listPharmacyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}