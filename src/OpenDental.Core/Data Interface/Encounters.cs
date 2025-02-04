using System;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Encounters
{
    public static void InsertDefaultEncounter(long patNum, long provNum, DateTime date)
    {
        //Validate prefs. If they are not set, we have nothing to insert so no reason to check.
        if (PrefC.GetString(PrefName.CQMDefaultEncounterCodeSystem) == "" || PrefC.GetString(PrefName.CQMDefaultEncounterCodeValue) == "none") return;
        //If no encounter for date for this patient
        var command = "SELECT COUNT(*) NumEncounters FROM encounter WHERE encounter.PatNum=" + (patNum) + " "
                      + "AND encounter.DateEncounter=" + SOut.Date(date) + " "
                      + "AND encounter.ProvNum=" + (provNum);
        var count = SIn.Int(Db.GetCount(command));
        if (count > 0) //Encounter already exists for date
            return;
        //Insert encounter with default encounter code system and code value set in Setup>EHR>Settings
        var encounter = new Encounter();
        encounter.PatNum = patNum;
        encounter.ProvNum = provNum;
        encounter.DateEncounter = date;
        encounter.CodeSystem = PrefC.GetString(PrefName.CQMDefaultEncounterCodeSystem);
        encounter.CodeValue = PrefC.GetString(PrefName.CQMDefaultEncounterCodeValue);
        Insert(encounter);
    }

    public static void Insert(Encounter encounter)
    {
        EncounterCrud.Insert(encounter);
    }
}