using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatientRaceCrud
{                              
    public static List<PatientRace> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatientRace> TableToList(DataTable table)
    {
        var retVal = new List<PatientRace>();
        foreach (DataRow row in table.Rows)
        {
            var patientRace = new PatientRace
            {
                PatientRaceNum = SIn.Long(row["PatientRaceNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                Race = (PatRace) SIn.Int(row["Race"].ToString()),
                CdcrecCode = SIn.String(row["CdcrecCode"].ToString())
            };
            retVal.Add(patientRace);
        }

        return retVal;
    }

    public static void Insert(PatientRace patientRace)
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
    }

    public static void Delete(long patientRaceNum)
    {
        var command = "DELETE FROM patientrace "
                      + "WHERE PatientRaceNum = " + SOut.Long(patientRaceNum);
        Db.NonQ(command);
    }
}