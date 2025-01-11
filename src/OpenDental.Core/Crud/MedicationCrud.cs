#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedicationCrud
{
    public static Medication SelectOne(long medicationNum)
    {
        var command = "SELECT * FROM medication "
                      + "WHERE MedicationNum = " + SOut.Long(medicationNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        Medication medication;
        foreach (DataRow row in table.Rows)
        {
            medication = new Medication();
            medication.MedicationNum = SIn.Long(row["MedicationNum"].ToString());
            medication.MedName = SIn.String(row["MedName"].ToString());
            medication.GenericNum = SIn.Long(row["GenericNum"].ToString());
            medication.Notes = SIn.String(row["Notes"].ToString());
            medication.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            medication.RxCui = SIn.Long(row["RxCui"].ToString());
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
            table.Rows.Add(SOut.Long(medication.MedicationNum), medication.MedName, SOut.Long(medication.GenericNum), medication.Notes, SOut.DateT(medication.DateTStamp, false), SOut.Long(medication.RxCui));
        return table;
    }

    public static long Insert(Medication medication)
    {
        return Insert(medication, false);
    }

    public static long Insert(Medication medication, bool useExistingPK)
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(medication.Notes));
        {
            medication.MedicationNum = Db.NonQ(command, true, "MedicationNum", "medication", paramNotes);
        }
        return medication.MedicationNum;
    }

    public static void InsertMany(List<Medication> listMedications)
    {
        InsertMany(listMedications, false);
    }

    public static void InsertMany(List<Medication> listMedications, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listMedications.Count)
        {
            var medication = listMedications[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO medication (");
                if (useExistingPK) sbCommands.Append("MedicationNum,");
                sbCommands.Append("MedName,GenericNum,Notes,RxCui) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(medication.MedicationNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(medication.MedName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(medication.GenericNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(medication.Notes) + "'");
            sbRow.Append(",");
            //DateTStamp can only be set by MySQL
            sbRow.Append(SOut.Long(medication.RxCui));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listMedications.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(Medication medication)
    {
        return InsertNoCache(medication, false);
    }

    public static long InsertNoCache(Medication medication, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medication (";
        if (isRandomKeys || useExistingPK) command += "MedicationNum,";
        command += "MedName,GenericNum,Notes,RxCui) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medication.MedicationNum) + ",";
        command +=
            "'" + SOut.String(medication.MedName) + "',"
            + SOut.Long(medication.GenericNum) + ","
            + DbHelper.ParamChar + "paramNotes,"
            //DateTStamp can only be set by MySQL
            + SOut.Long(medication.RxCui) + ")";
        if (medication.Notes == null) medication.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(medication.Notes));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotes);
        else
            medication.MedicationNum = Db.NonQ(command, true, "MedicationNum", "medication", paramNotes);
        return medication.MedicationNum;
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
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(medication.Notes));
        Db.NonQ(command, paramNotes);
    }

    public static bool Update(Medication medication, Medication oldMedication)
    {
        var command = "";
        if (medication.MedName != oldMedication.MedName)
        {
            if (command != "") command += ",";
            command += "MedName = '" + SOut.String(medication.MedName) + "'";
        }

        if (medication.GenericNum != oldMedication.GenericNum)
        {
            if (command != "") command += ",";
            command += "GenericNum = " + SOut.Long(medication.GenericNum) + "";
        }

        if (medication.Notes != oldMedication.Notes)
        {
            if (command != "") command += ",";
            command += "Notes = " + DbHelper.ParamChar + "paramNotes";
        }

        //DateTStamp can only be set by MySQL
        if (medication.RxCui != oldMedication.RxCui)
        {
            if (command != "") command += ",";
            command += "RxCui = " + SOut.Long(medication.RxCui) + "";
        }

        if (command == "") return false;
        if (medication.Notes == null) medication.Notes = "";
        var paramNotes = new OdSqlParameter("paramNotes", OdDbType.Text, SOut.StringParam(medication.Notes));
        command = "UPDATE medication SET " + command
                                           + " WHERE MedicationNum = " + SOut.Long(medication.MedicationNum);
        Db.NonQ(command, paramNotes);
        return true;
    }

    public static bool UpdateComparison(Medication medication, Medication oldMedication)
    {
        if (medication.MedName != oldMedication.MedName) return true;
        if (medication.GenericNum != oldMedication.GenericNum) return true;
        if (medication.Notes != oldMedication.Notes) return true;
        //DateTStamp can only be set by MySQL
        if (medication.RxCui != oldMedication.RxCui) return true;
        return false;
    }

    public static void Delete(long medicationNum)
    {
        var command = "DELETE FROM medication "
                      + "WHERE MedicationNum = " + SOut.Long(medicationNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedicationNums)
    {
        if (listMedicationNums == null || listMedicationNums.Count == 0) return;
        var command = "DELETE FROM medication "
                      + "WHERE MedicationNum IN(" + string.Join(",", listMedicationNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}