#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class VaccineDefCrud
{
    public static VaccineDef SelectOne(long vaccineDefNum)
    {
        var command = "SELECT * FROM vaccinedef "
                      + "WHERE VaccineDefNum = " + SOut.Long(vaccineDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static VaccineDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<VaccineDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<VaccineDef> TableToList(DataTable table)
    {
        var retVal = new List<VaccineDef>();
        VaccineDef vaccineDef;
        foreach (DataRow row in table.Rows)
        {
            vaccineDef = new VaccineDef();
            vaccineDef.VaccineDefNum = SIn.Long(row["VaccineDefNum"].ToString());
            vaccineDef.CVXCode = SIn.String(row["CVXCode"].ToString());
            vaccineDef.VaccineName = SIn.String(row["VaccineName"].ToString());
            vaccineDef.DrugManufacturerNum = SIn.Long(row["DrugManufacturerNum"].ToString());
            retVal.Add(vaccineDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<VaccineDef> listVaccineDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "VaccineDef";
        var table = new DataTable(tableName);
        table.Columns.Add("VaccineDefNum");
        table.Columns.Add("CVXCode");
        table.Columns.Add("VaccineName");
        table.Columns.Add("DrugManufacturerNum");
        foreach (var vaccineDef in listVaccineDefs)
            table.Rows.Add(SOut.Long(vaccineDef.VaccineDefNum), vaccineDef.CVXCode, vaccineDef.VaccineName, SOut.Long(vaccineDef.DrugManufacturerNum));
        return table;
    }

    public static long Insert(VaccineDef vaccineDef)
    {
        return Insert(vaccineDef, false);
    }

    public static long Insert(VaccineDef vaccineDef, bool useExistingPK)
    {
        var command = "INSERT INTO vaccinedef (";

        command += "CVXCode,VaccineName,DrugManufacturerNum) VALUES(";

        command +=
            "'" + SOut.String(vaccineDef.CVXCode) + "',"
            + "'" + SOut.String(vaccineDef.VaccineName) + "',"
            + SOut.Long(vaccineDef.DrugManufacturerNum) + ")";
        {
            vaccineDef.VaccineDefNum = Db.NonQ(command, true, "VaccineDefNum", "vaccineDef");
        }
        return vaccineDef.VaccineDefNum;
    }

    public static long InsertNoCache(VaccineDef vaccineDef)
    {
        return InsertNoCache(vaccineDef, false);
    }

    public static long InsertNoCache(VaccineDef vaccineDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO vaccinedef (";
        if (isRandomKeys || useExistingPK) command += "VaccineDefNum,";
        command += "CVXCode,VaccineName,DrugManufacturerNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(vaccineDef.VaccineDefNum) + ",";
        command +=
            "'" + SOut.String(vaccineDef.CVXCode) + "',"
            + "'" + SOut.String(vaccineDef.VaccineName) + "',"
            + SOut.Long(vaccineDef.DrugManufacturerNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            vaccineDef.VaccineDefNum = Db.NonQ(command, true, "VaccineDefNum", "vaccineDef");
        return vaccineDef.VaccineDefNum;
    }

    public static void Update(VaccineDef vaccineDef)
    {
        var command = "UPDATE vaccinedef SET "
                      + "CVXCode            = '" + SOut.String(vaccineDef.CVXCode) + "', "
                      + "VaccineName        = '" + SOut.String(vaccineDef.VaccineName) + "', "
                      + "DrugManufacturerNum=  " + SOut.Long(vaccineDef.DrugManufacturerNum) + " "
                      + "WHERE VaccineDefNum = " + SOut.Long(vaccineDef.VaccineDefNum);
        Db.NonQ(command);
    }

    public static bool Update(VaccineDef vaccineDef, VaccineDef oldVaccineDef)
    {
        var command = "";
        if (vaccineDef.CVXCode != oldVaccineDef.CVXCode)
        {
            if (command != "") command += ",";
            command += "CVXCode = '" + SOut.String(vaccineDef.CVXCode) + "'";
        }

        if (vaccineDef.VaccineName != oldVaccineDef.VaccineName)
        {
            if (command != "") command += ",";
            command += "VaccineName = '" + SOut.String(vaccineDef.VaccineName) + "'";
        }

        if (vaccineDef.DrugManufacturerNum != oldVaccineDef.DrugManufacturerNum)
        {
            if (command != "") command += ",";
            command += "DrugManufacturerNum = " + SOut.Long(vaccineDef.DrugManufacturerNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE vaccinedef SET " + command
                                           + " WHERE VaccineDefNum = " + SOut.Long(vaccineDef.VaccineDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(VaccineDef vaccineDef, VaccineDef oldVaccineDef)
    {
        if (vaccineDef.CVXCode != oldVaccineDef.CVXCode) return true;
        if (vaccineDef.VaccineName != oldVaccineDef.VaccineName) return true;
        if (vaccineDef.DrugManufacturerNum != oldVaccineDef.DrugManufacturerNum) return true;
        return false;
    }

    public static void Delete(long vaccineDefNum)
    {
        var command = "DELETE FROM vaccinedef "
                      + "WHERE VaccineDefNum = " + SOut.Long(vaccineDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listVaccineDefNums)
    {
        if (listVaccineDefNums == null || listVaccineDefNums.Count == 0) return;
        var command = "DELETE FROM vaccinedef "
                      + "WHERE VaccineDefNum IN(" + string.Join(",", listVaccineDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}