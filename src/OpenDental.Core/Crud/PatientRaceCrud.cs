#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatientRaceCrud
{
    public static PatientRace SelectOne(long patientRaceNum)
    {
        var command = "SELECT * FROM patientrace "
                      + "WHERE PatientRaceNum = " + SOut.Long(patientRaceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatientRace SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatientRace> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatientRace> TableToList(DataTable table)
    {
        var retVal = new List<PatientRace>();
        PatientRace patientRace;
        foreach (DataRow row in table.Rows)
        {
            patientRace = new PatientRace();
            patientRace.PatientRaceNum = SIn.Long(row["PatientRaceNum"].ToString());
            patientRace.PatNum = SIn.Long(row["PatNum"].ToString());
            patientRace.Race = (PatRace) SIn.Int(row["Race"].ToString());
            patientRace.CdcrecCode = SIn.String(row["CdcrecCode"].ToString());
            retVal.Add(patientRace);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatientRace> listPatientRaces, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatientRace";
        var table = new DataTable(tableName);
        table.Columns.Add("PatientRaceNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("Race");
        table.Columns.Add("CdcrecCode");
        foreach (var patientRace in listPatientRaces)
            table.Rows.Add(SOut.Long(patientRace.PatientRaceNum), SOut.Long(patientRace.PatNum), SOut.Int((int) patientRace.Race), patientRace.CdcrecCode);
        return table;
    }

    public static long Insert(PatientRace patientRace)
    {
        return Insert(patientRace, false);
    }

    public static long Insert(PatientRace patientRace, bool useExistingPK)
    {
        var command = "INSERT INTO patientrace (";

        command += "PatNum,Race,CdcrecCode) VALUES(";

        command +=
            SOut.Long(patientRace.PatNum) + ","
                                          + SOut.Int((int) patientRace.Race) + ","
                                          + "'" + SOut.String(patientRace.CdcrecCode) + "')";
        {
            patientRace.PatientRaceNum = Db.NonQ(command, true, "PatientRaceNum", "patientRace");
        }
        return patientRace.PatientRaceNum;
    }

    public static long InsertNoCache(PatientRace patientRace)
    {
        return InsertNoCache(patientRace, false);
    }

    public static long InsertNoCache(PatientRace patientRace, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patientrace (";
        if (isRandomKeys || useExistingPK) command += "PatientRaceNum,";
        command += "PatNum,Race,CdcrecCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patientRace.PatientRaceNum) + ",";
        command +=
            SOut.Long(patientRace.PatNum) + ","
                                          + SOut.Int((int) patientRace.Race) + ","
                                          + "'" + SOut.String(patientRace.CdcrecCode) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            patientRace.PatientRaceNum = Db.NonQ(command, true, "PatientRaceNum", "patientRace");
        return patientRace.PatientRaceNum;
    }

    public static void Update(PatientRace patientRace)
    {
        var command = "UPDATE patientrace SET "
                      + "PatNum        =  " + SOut.Long(patientRace.PatNum) + ", "
                      + "Race          =  " + SOut.Int((int) patientRace.Race) + ", "
                      + "CdcrecCode    = '" + SOut.String(patientRace.CdcrecCode) + "' "
                      + "WHERE PatientRaceNum = " + SOut.Long(patientRace.PatientRaceNum);
        Db.NonQ(command);
    }

    public static bool Update(PatientRace patientRace, PatientRace oldPatientRace)
    {
        var command = "";
        if (patientRace.PatNum != oldPatientRace.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(patientRace.PatNum) + "";
        }

        if (patientRace.Race != oldPatientRace.Race)
        {
            if (command != "") command += ",";
            command += "Race = " + SOut.Int((int) patientRace.Race) + "";
        }

        if (patientRace.CdcrecCode != oldPatientRace.CdcrecCode)
        {
            if (command != "") command += ",";
            command += "CdcrecCode = '" + SOut.String(patientRace.CdcrecCode) + "'";
        }

        if (command == "") return false;
        command = "UPDATE patientrace SET " + command
                                            + " WHERE PatientRaceNum = " + SOut.Long(patientRace.PatientRaceNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PatientRace patientRace, PatientRace oldPatientRace)
    {
        if (patientRace.PatNum != oldPatientRace.PatNum) return true;
        if (patientRace.Race != oldPatientRace.Race) return true;
        if (patientRace.CdcrecCode != oldPatientRace.CdcrecCode) return true;
        return false;
    }

    public static void Delete(long patientRaceNum)
    {
        var command = "DELETE FROM patientrace "
                      + "WHERE PatientRaceNum = " + SOut.Long(patientRaceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatientRaceNums)
    {
        if (listPatientRaceNums == null || listPatientRaceNums.Count == 0) return;
        var command = "DELETE FROM patientrace "
                      + "WHERE PatientRaceNum IN(" + string.Join(",", listPatientRaceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}