using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static void Insert(VaccineDef vaccineDef)
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
}