using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MedicationCrud
{
    public static Medication SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Medication> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Medication> TableToList(DataTable table)
    {
        var retVal = new List<Medication>();
        foreach (DataRow row in table.Rows)
        {
            var medication = new Medication
            {
                MedicationNum = SIn.Long(row["MedicationNum"].ToString()),
                MedName = SIn.String(row["MedName"].ToString()),
                GenericNum = SIn.Long(row["GenericNum"].ToString()),
                Notes = SIn.String(row["Notes"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString()),
                RxCui = SIn.Long(row["RxCui"].ToString())
            };
            retVal.Add(medication);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Medication> listMedications, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Medication";
        var table = new DataTable(tableName);
        table.Columns.Add("MedicationNum");
        table.Columns.Add("MedName");
        table.Columns.Add("GenericNum");
        table.Columns.Add("Notes");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("RxCui");
        foreach (var medication in listMedications)
            table.Rows.Add(SOut.Long(medication.MedicationNum), medication.MedName, SOut.Long(medication.GenericNum), medication.Notes, SOut.DateTime(medication.DateTStamp, false), SOut.Long(medication.RxCui));
        return table;
    }

    public static void Insert(Medication medication)
    {
        var command = "INSERT INTO medication (";

        command += "MedName,GenericNum,Notes,RxCui) VALUES(";

        command +=
            "'" + SOut.String(medication.MedName) + "',"
            + SOut.Long(medication.GenericNum) + ","
            + DbHelper.ParamChar + "paramNotes,"
            //DateTStamp can only be set by MySQL
            + SOut.Long(medication.RxCui) + ")";
        if (medication.Notes == null) medication.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", SOut.StringParam(medication.Notes));
        {
            medication.MedicationNum = Db.NonQ(command, true, "MedicationNum", "medication", paramNotes);
        }
    }

    public static void Update(Medication medication)
    {
        var command = "UPDATE medication SET "
                      + "MedName      = '" + SOut.String(medication.MedName) + "', "
                      + "GenericNum   =  " + SOut.Long(medication.GenericNum) + ", "
                      + "Notes        =  " + DbHelper.ParamChar + "paramNotes, "
                      //DateTStamp can only be set by MySQL
                      + "RxCui        =  " + SOut.Long(medication.RxCui) + " "
                      + "WHERE MedicationNum = " + SOut.Long(medication.MedicationNum);
        if (medication.Notes == null) medication.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", SOut.StringParam(medication.Notes));
        Db.NonQ(command, paramNotes);
    }
}