#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedLabFacilityCrud
{
    public static MedLabFacility SelectOne(long medLabFacilityNum)
    {
        var command = "SELECT * FROM medlabfacility "
                      + "WHERE MedLabFacilityNum = " + SOut.Long(medLabFacilityNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedLabFacility SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedLabFacility> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedLabFacility> TableToList(DataTable table)
    {
        var retVal = new List<MedLabFacility>();
        MedLabFacility medLabFacility;
        foreach (DataRow row in table.Rows)
        {
            medLabFacility = new MedLabFacility();
            medLabFacility.MedLabFacilityNum = SIn.Long(row["MedLabFacilityNum"].ToString());
            medLabFacility.FacilityName = SIn.String(row["FacilityName"].ToString());
            medLabFacility.Address = SIn.String(row["Address"].ToString());
            medLabFacility.City = SIn.String(row["City"].ToString());
            medLabFacility.State = SIn.String(row["State"].ToString());
            medLabFacility.Zip = SIn.String(row["Zip"].ToString());
            medLabFacility.Phone = SIn.String(row["Phone"].ToString());
            medLabFacility.DirectorTitle = SIn.String(row["DirectorTitle"].ToString());
            medLabFacility.DirectorLName = SIn.String(row["DirectorLName"].ToString());
            medLabFacility.DirectorFName = SIn.String(row["DirectorFName"].ToString());
            retVal.Add(medLabFacility);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedLabFacility> listMedLabFacilitys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedLabFacility";
        var table = new DataTable(tableName);
        table.Columns.Add("MedLabFacilityNum");
        table.Columns.Add("FacilityName");
        table.Columns.Add("Address");
        table.Columns.Add("City");
        table.Columns.Add("State");
        table.Columns.Add("Zip");
        table.Columns.Add("Phone");
        table.Columns.Add("DirectorTitle");
        table.Columns.Add("DirectorLName");
        table.Columns.Add("DirectorFName");
        foreach (var medLabFacility in listMedLabFacilitys)
            table.Rows.Add(SOut.Long(medLabFacility.MedLabFacilityNum), medLabFacility.FacilityName, medLabFacility.Address, medLabFacility.City, medLabFacility.State, medLabFacility.Zip, medLabFacility.Phone, medLabFacility.DirectorTitle, medLabFacility.DirectorLName, medLabFacility.DirectorFName);
        return table;
    }

    public static long Insert(MedLabFacility medLabFacility)
    {
        return Insert(medLabFacility, false);
    }

    public static long Insert(MedLabFacility medLabFacility, bool useExistingPK)
    {
        var command = "INSERT INTO medlabfacility (";

        command += "FacilityName,Address,City,State,Zip,Phone,DirectorTitle,DirectorLName,DirectorFName) VALUES(";

        command +=
            "'" + SOut.String(medLabFacility.FacilityName) + "',"
            + "'" + SOut.String(medLabFacility.Address) + "',"
            + "'" + SOut.String(medLabFacility.City) + "',"
            + "'" + SOut.String(medLabFacility.State) + "',"
            + "'" + SOut.String(medLabFacility.Zip) + "',"
            + "'" + SOut.String(medLabFacility.Phone) + "',"
            + "'" + SOut.String(medLabFacility.DirectorTitle) + "',"
            + "'" + SOut.String(medLabFacility.DirectorLName) + "',"
            + "'" + SOut.String(medLabFacility.DirectorFName) + "')";
        {
            medLabFacility.MedLabFacilityNum = Db.NonQ(command, true, "MedLabFacilityNum", "medLabFacility");
        }
        return medLabFacility.MedLabFacilityNum;
    }

    public static long InsertNoCache(MedLabFacility medLabFacility)
    {
        return InsertNoCache(medLabFacility, false);
    }

    public static long InsertNoCache(MedLabFacility medLabFacility, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medlabfacility (";
        if (isRandomKeys || useExistingPK) command += "MedLabFacilityNum,";
        command += "FacilityName,Address,City,State,Zip,Phone,DirectorTitle,DirectorLName,DirectorFName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medLabFacility.MedLabFacilityNum) + ",";
        command +=
            "'" + SOut.String(medLabFacility.FacilityName) + "',"
            + "'" + SOut.String(medLabFacility.Address) + "',"
            + "'" + SOut.String(medLabFacility.City) + "',"
            + "'" + SOut.String(medLabFacility.State) + "',"
            + "'" + SOut.String(medLabFacility.Zip) + "',"
            + "'" + SOut.String(medLabFacility.Phone) + "',"
            + "'" + SOut.String(medLabFacility.DirectorTitle) + "',"
            + "'" + SOut.String(medLabFacility.DirectorLName) + "',"
            + "'" + SOut.String(medLabFacility.DirectorFName) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            medLabFacility.MedLabFacilityNum = Db.NonQ(command, true, "MedLabFacilityNum", "medLabFacility");
        return medLabFacility.MedLabFacilityNum;
    }

    public static void Update(MedLabFacility medLabFacility)
    {
        var command = "UPDATE medlabfacility SET "
                      + "FacilityName     = '" + SOut.String(medLabFacility.FacilityName) + "', "
                      + "Address          = '" + SOut.String(medLabFacility.Address) + "', "
                      + "City             = '" + SOut.String(medLabFacility.City) + "', "
                      + "State            = '" + SOut.String(medLabFacility.State) + "', "
                      + "Zip              = '" + SOut.String(medLabFacility.Zip) + "', "
                      + "Phone            = '" + SOut.String(medLabFacility.Phone) + "', "
                      + "DirectorTitle    = '" + SOut.String(medLabFacility.DirectorTitle) + "', "
                      + "DirectorLName    = '" + SOut.String(medLabFacility.DirectorLName) + "', "
                      + "DirectorFName    = '" + SOut.String(medLabFacility.DirectorFName) + "' "
                      + "WHERE MedLabFacilityNum = " + SOut.Long(medLabFacility.MedLabFacilityNum);
        Db.NonQ(command);
    }

    public static bool Update(MedLabFacility medLabFacility, MedLabFacility oldMedLabFacility)
    {
        var command = "";
        if (medLabFacility.FacilityName != oldMedLabFacility.FacilityName)
        {
            if (command != "") command += ",";
            command += "FacilityName = '" + SOut.String(medLabFacility.FacilityName) + "'";
        }

        if (medLabFacility.Address != oldMedLabFacility.Address)
        {
            if (command != "") command += ",";
            command += "Address = '" + SOut.String(medLabFacility.Address) + "'";
        }

        if (medLabFacility.City != oldMedLabFacility.City)
        {
            if (command != "") command += ",";
            command += "City = '" + SOut.String(medLabFacility.City) + "'";
        }

        if (medLabFacility.State != oldMedLabFacility.State)
        {
            if (command != "") command += ",";
            command += "State = '" + SOut.String(medLabFacility.State) + "'";
        }

        if (medLabFacility.Zip != oldMedLabFacility.Zip)
        {
            if (command != "") command += ",";
            command += "Zip = '" + SOut.String(medLabFacility.Zip) + "'";
        }

        if (medLabFacility.Phone != oldMedLabFacility.Phone)
        {
            if (command != "") command += ",";
            command += "Phone = '" + SOut.String(medLabFacility.Phone) + "'";
        }

        if (medLabFacility.DirectorTitle != oldMedLabFacility.DirectorTitle)
        {
            if (command != "") command += ",";
            command += "DirectorTitle = '" + SOut.String(medLabFacility.DirectorTitle) + "'";
        }

        if (medLabFacility.DirectorLName != oldMedLabFacility.DirectorLName)
        {
            if (command != "") command += ",";
            command += "DirectorLName = '" + SOut.String(medLabFacility.DirectorLName) + "'";
        }

        if (medLabFacility.DirectorFName != oldMedLabFacility.DirectorFName)
        {
            if (command != "") command += ",";
            command += "DirectorFName = '" + SOut.String(medLabFacility.DirectorFName) + "'";
        }

        if (command == "") return false;
        command = "UPDATE medlabfacility SET " + command
                                               + " WHERE MedLabFacilityNum = " + SOut.Long(medLabFacility.MedLabFacilityNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MedLabFacility medLabFacility, MedLabFacility oldMedLabFacility)
    {
        if (medLabFacility.FacilityName != oldMedLabFacility.FacilityName) return true;
        if (medLabFacility.Address != oldMedLabFacility.Address) return true;
        if (medLabFacility.City != oldMedLabFacility.City) return true;
        if (medLabFacility.State != oldMedLabFacility.State) return true;
        if (medLabFacility.Zip != oldMedLabFacility.Zip) return true;
        if (medLabFacility.Phone != oldMedLabFacility.Phone) return true;
        if (medLabFacility.DirectorTitle != oldMedLabFacility.DirectorTitle) return true;
        if (medLabFacility.DirectorLName != oldMedLabFacility.DirectorLName) return true;
        if (medLabFacility.DirectorFName != oldMedLabFacility.DirectorFName) return true;
        return false;
    }

    public static void Delete(long medLabFacilityNum)
    {
        var command = "DELETE FROM medlabfacility "
                      + "WHERE MedLabFacilityNum = " + SOut.Long(medLabFacilityNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedLabFacilityNums)
    {
        if (listMedLabFacilityNums == null || listMedLabFacilityNums.Count == 0) return;
        var command = "DELETE FROM medlabfacility "
                      + "WHERE MedLabFacilityNum IN(" + string.Join(",", listMedLabFacilityNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}