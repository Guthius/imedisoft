using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PatientRaces
{
    public static List<PatientRace> GetForPatient(long patNum)
    {
        var command = @"SELECT patientrace.*,COALESCE(cdcrec.Description,'') Description,
				(CASE WHEN cdcrec.HeirarchicalCode LIKE 'E%' THEN 1 ELSE 0 END) IsEthnicity,
				COALESCE(cdcrec.HeirarchicalCode,'') HeirarchicalCode
				FROM patientrace 
				LEFT JOIN cdcrec ON cdcrec.CdcrecCode=patientrace.CdcrecCode
				WHERE PatNum=" + patNum;
        var table = DataCore.GetTable(command);
        var listPatientRaces = PatientRaceCrud.TableToList(table);
        for (var i = 0; i < table.Rows.Count; i++)
            switch (listPatientRaces[i].CdcrecCode)
            {
                case PatientRace.DeclineSpecifyRaceCode:
                    listPatientRaces[i].Description = Lans.g("PatientRaces", "DECLINED TO SPECIFY");
                    listPatientRaces[i].IsEthnicity = false;
                    break;
                case PatientRace.DeclineSpecifyEthnicityCode:
                    listPatientRaces[i].Description = Lans.g("PatientRaces", "DECLINED TO SPECIFY");
                    listPatientRaces[i].IsEthnicity = true;
                    break;
                case PatientRace.MultiRaceCode:
                    listPatientRaces[i].Description = Lans.g("PatientRaces", "MULTIRACIAL");
                    listPatientRaces[i].IsEthnicity = false;
                    break;
                default:
                    listPatientRaces[i].Description = SIn.String(table.Rows[i]["Description"].ToString());
                    listPatientRaces[i].IsEthnicity = table.Rows[i]["IsEthnicity"].ToString() == "1";
                    listPatientRaces[i].HeirarchicalCode = SIn.String(table.Rows[i]["HeirarchicalCode"].ToString());
                    break;
            }

        return listPatientRaces;
    }

    public static PatientRaceOld GetPatientRaceOldFromPatientRaces(long patNum, List<PatientRace> races = null)
    {
        if (races.IsNullOrEmpty()) races = GetForPatient(patNum);
        if (races.Count == 0) return PatientRaceOld.Unknown; //Unknown is default for PatientRaceOld
        if (races.Any(x => x.HeirarchicalCode == "R5" || x.HeirarchicalCode.StartsWith("R5.")))
        {
            if (races.Any(x => x.HeirarchicalCode == "E1" || x.HeirarchicalCode.StartsWith("E1."))) return PatientRaceOld.HispanicLatino;
            return PatientRaceOld.White;
        }

        if (races.Any(x => x.HeirarchicalCode == "R3" || x.HeirarchicalCode.StartsWith("R3.")))
        {
            if (races.Any(x => x.HeirarchicalCode == "E1" || x.HeirarchicalCode.StartsWith("E1."))) return PatientRaceOld.BlackHispanic;
            return PatientRaceOld.AfricanAmerican;
        }

        if (races.Any(x => x.HeirarchicalCode == "R4")) return PatientRaceOld.Aboriginal;
        if (races.Any(x => x.HeirarchicalCode == "R1" || x.HeirarchicalCode.StartsWith("R1."))) return PatientRaceOld.AmericanIndian;
        if (races.Any(x => x.HeirarchicalCode == "R2" || x.HeirarchicalCode.StartsWith("R2."))) return PatientRaceOld.Asian;
        if (races.Any(x => x.HeirarchicalCode == "R4" || x.HeirarchicalCode.StartsWith("R4."))) return PatientRaceOld.HawaiiOrPacIsland;
        if (races.Any(x => x.HeirarchicalCode == "R9")) return PatientRaceOld.Other;
        //Hispanic
        //DeclinedToSpecify
        return PatientRaceOld.Unknown;
    }

    public static string GetRaceDescription(List<PatientRace> listPatRaces)
    {
        if (listPatRaces.Count(x => !x.IsEthnicity) == 0) return "";
        return string.Join(", ", listPatRaces.Where(x => !x.IsEthnicity).Select(x => x.Description));
    }

    public static string GetEthnicityDescription(List<PatientRace> listPatRaces)
    {
        if (listPatRaces.Count(x => x.IsEthnicity) == 0) return "";
        return string.Join(", ", listPatRaces.Where(x => x.IsEthnicity).Select(x => x.Description));
    }

    public static void Reconcile(long patNum, List<PatientRace> listPatRaces)
    {
        string command;
        if (listPatRaces.Count == 0)
        {
            //DELETE all for the patient if listPatRaces is empty.
            command = "DELETE FROM patientrace WHERE PatNum = " + patNum; //Can't use CRUD layer here because there might be multiple races for one patient.
            Db.NonQ(command);
            return;
        }

        List<PatientRace> listPatientRacesDB;
        command = "SELECT * FROM patientrace WHERE PatNum = " + patNum;
        listPatientRacesDB = PatientRaceCrud.SelectMany(command);
        //delete excess rows
        for (var i = 0; i < listPatientRacesDB.Count; i++)
            if (!listPatRaces.Any(x => x.CdcrecCode == listPatientRacesDB[i].CdcrecCode))
                //if there is a PatientRace row that does not match the new list of PatientRaces, delete it
                PatientRaceCrud.Delete(listPatientRacesDB[i].PatientRaceNum);

        //delete duplicate rows
        for (var i = 0; i < listPatientRacesDB.Count; i++)
        {
            if (!listPatRaces.Any(x => x.CdcrecCode == listPatientRacesDB[i].CdcrecCode)) continue; //It was already deleted earlier
            for (var j = i + 1; j < listPatientRacesDB.Count; j++)
                if (listPatientRacesDB[i].CdcrecCode == listPatientRacesDB[j].CdcrecCode)
                    //If there are duplicate races in the DB that weren't deleted before.
                    PatientRaceCrud.Delete(listPatientRacesDB[j].PatientRaceNum);
        }

        //insert new rows
        for (var i = 0; i < listPatRaces.Count; i++)
        {
            var insertNeeded = true;
            for (var j = 0; j < listPatientRacesDB.Count; j++)
                if (listPatRaces[i].CdcrecCode == listPatientRacesDB[j].CdcrecCode)
                    insertNeeded = false;

            if (insertNeeded)
            {
                listPatRaces[i].PatNum = patNum; //Just to be safe
                PatientRaceCrud.Insert(listPatRaces[i]);
            }
            //next PatRace
        }
        //return;
    }
}