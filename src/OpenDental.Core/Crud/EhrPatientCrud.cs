#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrPatientCrud
{
    public static EhrPatient SelectOne(long patNum)
    {
        var command = "SELECT * FROM ehrpatient "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrPatient SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrPatient> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrPatient> TableToList(DataTable table)
    {
        var retVal = new List<EhrPatient>();
        EhrPatient ehrPatient;
        foreach (DataRow row in table.Rows)
        {
            ehrPatient = new EhrPatient();
            ehrPatient.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrPatient.MotherMaidenFname = SIn.String(row["MotherMaidenFname"].ToString());
            ehrPatient.MotherMaidenLname = SIn.String(row["MotherMaidenLname"].ToString());
            ehrPatient.VacShareOk = (YN) SIn.Int(row["VacShareOk"].ToString());
            ehrPatient.MedicaidState = SIn.String(row["MedicaidState"].ToString());
            ehrPatient.SexualOrientation = SIn.String(row["SexualOrientation"].ToString());
            ehrPatient.GenderIdentity = SIn.String(row["GenderIdentity"].ToString());
            ehrPatient.SexualOrientationNote = SIn.String(row["SexualOrientationNote"].ToString());
            ehrPatient.GenderIdentityNote = SIn.String(row["GenderIdentityNote"].ToString());
            ehrPatient.DischargeDate = SIn.DateTime(row["DischargeDate"].ToString());
            retVal.Add(ehrPatient);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrPatient> listEhrPatients, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrPatient";
        var table = new DataTable(tableName);
        table.Columns.Add("PatNum");
        table.Columns.Add("MotherMaidenFname");
        table.Columns.Add("MotherMaidenLname");
        table.Columns.Add("VacShareOk");
        table.Columns.Add("MedicaidState");
        table.Columns.Add("SexualOrientation");
        table.Columns.Add("GenderIdentity");
        table.Columns.Add("SexualOrientationNote");
        table.Columns.Add("GenderIdentityNote");
        table.Columns.Add("DischargeDate");
        foreach (var ehrPatient in listEhrPatients)
            table.Rows.Add(SOut.Long(ehrPatient.PatNum), ehrPatient.MotherMaidenFname, ehrPatient.MotherMaidenLname, SOut.Int((int) ehrPatient.VacShareOk), ehrPatient.MedicaidState, ehrPatient.SexualOrientation, ehrPatient.GenderIdentity, ehrPatient.SexualOrientationNote, ehrPatient.GenderIdentityNote, SOut.DateT(ehrPatient.DischargeDate, false));
        return table;
    }

    public static long Insert(EhrPatient ehrPatient)
    {
        return Insert(ehrPatient, false);
    }

    public static long Insert(EhrPatient ehrPatient, bool useExistingPK)
    {
        var command = "INSERT INTO ehrpatient (";

        command += "MotherMaidenFname,MotherMaidenLname,VacShareOk,MedicaidState,SexualOrientation,GenderIdentity,SexualOrientationNote,GenderIdentityNote,DischargeDate) VALUES(";

        command +=
            "'" + SOut.String(ehrPatient.MotherMaidenFname) + "',"
            + "'" + SOut.String(ehrPatient.MotherMaidenLname) + "',"
            + SOut.Int((int) ehrPatient.VacShareOk) + ","
            + "'" + SOut.String(ehrPatient.MedicaidState) + "',"
            + "'" + SOut.String(ehrPatient.SexualOrientation) + "',"
            + "'" + SOut.String(ehrPatient.GenderIdentity) + "',"
            + "'" + SOut.String(ehrPatient.SexualOrientationNote) + "',"
            + "'" + SOut.String(ehrPatient.GenderIdentityNote) + "',"
            + SOut.DateT(ehrPatient.DischargeDate) + ")";
        {
            ehrPatient.PatNum = Db.NonQ(command, true, "PatNum", "ehrPatient");
        }
        return ehrPatient.PatNum;
    }

    public static long InsertNoCache(EhrPatient ehrPatient)
    {
        return InsertNoCache(ehrPatient, false);
    }

    public static long InsertNoCache(EhrPatient ehrPatient, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrpatient (";
        if (isRandomKeys || useExistingPK) command += "PatNum,";
        command += "MotherMaidenFname,MotherMaidenLname,VacShareOk,MedicaidState,SexualOrientation,GenderIdentity,SexualOrientationNote,GenderIdentityNote,DischargeDate) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrPatient.PatNum) + ",";
        command +=
            "'" + SOut.String(ehrPatient.MotherMaidenFname) + "',"
            + "'" + SOut.String(ehrPatient.MotherMaidenLname) + "',"
            + SOut.Int((int) ehrPatient.VacShareOk) + ","
            + "'" + SOut.String(ehrPatient.MedicaidState) + "',"
            + "'" + SOut.String(ehrPatient.SexualOrientation) + "',"
            + "'" + SOut.String(ehrPatient.GenderIdentity) + "',"
            + "'" + SOut.String(ehrPatient.SexualOrientationNote) + "',"
            + "'" + SOut.String(ehrPatient.GenderIdentityNote) + "',"
            + SOut.DateT(ehrPatient.DischargeDate) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrPatient.PatNum = Db.NonQ(command, true, "PatNum", "ehrPatient");
        return ehrPatient.PatNum;
    }

    public static void Update(EhrPatient ehrPatient)
    {
        var command = "UPDATE ehrpatient SET "
                      + "MotherMaidenFname    = '" + SOut.String(ehrPatient.MotherMaidenFname) + "', "
                      + "MotherMaidenLname    = '" + SOut.String(ehrPatient.MotherMaidenLname) + "', "
                      + "VacShareOk           =  " + SOut.Int((int) ehrPatient.VacShareOk) + ", "
                      + "MedicaidState        = '" + SOut.String(ehrPatient.MedicaidState) + "', "
                      + "SexualOrientation    = '" + SOut.String(ehrPatient.SexualOrientation) + "', "
                      + "GenderIdentity       = '" + SOut.String(ehrPatient.GenderIdentity) + "', "
                      + "SexualOrientationNote= '" + SOut.String(ehrPatient.SexualOrientationNote) + "', "
                      + "GenderIdentityNote   = '" + SOut.String(ehrPatient.GenderIdentityNote) + "', "
                      + "DischargeDate        =  " + SOut.DateT(ehrPatient.DischargeDate) + " "
                      + "WHERE PatNum = " + SOut.Long(ehrPatient.PatNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrPatient ehrPatient, EhrPatient oldEhrPatient)
    {
        var command = "";
        if (ehrPatient.MotherMaidenFname != oldEhrPatient.MotherMaidenFname)
        {
            if (command != "") command += ",";
            command += "MotherMaidenFname = '" + SOut.String(ehrPatient.MotherMaidenFname) + "'";
        }

        if (ehrPatient.MotherMaidenLname != oldEhrPatient.MotherMaidenLname)
        {
            if (command != "") command += ",";
            command += "MotherMaidenLname = '" + SOut.String(ehrPatient.MotherMaidenLname) + "'";
        }

        if (ehrPatient.VacShareOk != oldEhrPatient.VacShareOk)
        {
            if (command != "") command += ",";
            command += "VacShareOk = " + SOut.Int((int) ehrPatient.VacShareOk) + "";
        }

        if (ehrPatient.MedicaidState != oldEhrPatient.MedicaidState)
        {
            if (command != "") command += ",";
            command += "MedicaidState = '" + SOut.String(ehrPatient.MedicaidState) + "'";
        }

        if (ehrPatient.SexualOrientation != oldEhrPatient.SexualOrientation)
        {
            if (command != "") command += ",";
            command += "SexualOrientation = '" + SOut.String(ehrPatient.SexualOrientation) + "'";
        }

        if (ehrPatient.GenderIdentity != oldEhrPatient.GenderIdentity)
        {
            if (command != "") command += ",";
            command += "GenderIdentity = '" + SOut.String(ehrPatient.GenderIdentity) + "'";
        }

        if (ehrPatient.SexualOrientationNote != oldEhrPatient.SexualOrientationNote)
        {
            if (command != "") command += ",";
            command += "SexualOrientationNote = '" + SOut.String(ehrPatient.SexualOrientationNote) + "'";
        }

        if (ehrPatient.GenderIdentityNote != oldEhrPatient.GenderIdentityNote)
        {
            if (command != "") command += ",";
            command += "GenderIdentityNote = '" + SOut.String(ehrPatient.GenderIdentityNote) + "'";
        }

        if (ehrPatient.DischargeDate != oldEhrPatient.DischargeDate)
        {
            if (command != "") command += ",";
            command += "DischargeDate = " + SOut.DateT(ehrPatient.DischargeDate) + "";
        }

        if (command == "") return false;
        command = "UPDATE ehrpatient SET " + command
                                           + " WHERE PatNum = " + SOut.Long(ehrPatient.PatNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrPatient ehrPatient, EhrPatient oldEhrPatient)
    {
        if (ehrPatient.MotherMaidenFname != oldEhrPatient.MotherMaidenFname) return true;
        if (ehrPatient.MotherMaidenLname != oldEhrPatient.MotherMaidenLname) return true;
        if (ehrPatient.VacShareOk != oldEhrPatient.VacShareOk) return true;
        if (ehrPatient.MedicaidState != oldEhrPatient.MedicaidState) return true;
        if (ehrPatient.SexualOrientation != oldEhrPatient.SexualOrientation) return true;
        if (ehrPatient.GenderIdentity != oldEhrPatient.GenderIdentity) return true;
        if (ehrPatient.SexualOrientationNote != oldEhrPatient.SexualOrientationNote) return true;
        if (ehrPatient.GenderIdentityNote != oldEhrPatient.GenderIdentityNote) return true;
        if (ehrPatient.DischargeDate != oldEhrPatient.DischargeDate) return true;
        return false;
    }

    public static void Delete(long patNum)
    {
        var command = "DELETE FROM ehrpatient "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count == 0) return;
        var command = "DELETE FROM ehrpatient "
                      + "WHERE PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}