using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PharmacyCrud
{
    public static List<Pharmacy> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Pharmacy> TableToList(DataTable table)
    {
        var retVal = new List<Pharmacy>();
        foreach (DataRow row in table.Rows)
        {
            var pharmacy = new Pharmacy
            {
                PharmacyNum = SIn.Long(row["PharmacyNum"].ToString()),
                PharmID = SIn.String(row["PharmID"].ToString()),
                StoreName = SIn.String(row["StoreName"].ToString()),
                Phone = SIn.String(row["Phone"].ToString()),
                Fax = SIn.String(row["Fax"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                Address2 = SIn.String(row["Address2"].ToString()),
                City = SIn.String(row["City"].ToString()),
                State = SIn.String(row["State"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString())
            };
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
            table.Rows.Add(SOut.Long(pharmacy.PharmacyNum), pharmacy.PharmID, pharmacy.StoreName, pharmacy.Phone, pharmacy.Fax, pharmacy.Address, pharmacy.Address2, pharmacy.City, pharmacy.State, pharmacy.Zip, pharmacy.Note, SOut.DateTime(pharmacy.DateTStamp, false));
        return table;
    }

    public static void Insert(Pharmacy pharmacy)
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(pharmacy.Note));
        {
            pharmacy.PharmacyNum = Db.NonQ(command, true, "PharmacyNum", "pharmacy", paramNote);
        }
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
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(pharmacy.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Delete(long pharmacyNum)
    {
        var command = "DELETE FROM pharmacy "
                      + "WHERE PharmacyNum = " + SOut.Long(pharmacyNum);
        Db.NonQ(command);
    }
}