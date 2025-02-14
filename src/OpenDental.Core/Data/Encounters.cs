using System;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Encounters
{
    public static void InsertDefaultEncounter(long patNum, long provNum, DateTime date)
    {
        if (PrefC.GetString(PrefName.CQMDefaultEncounterCodeSystem) == "" || 
            PrefC.GetString(PrefName.CQMDefaultEncounterCodeValue) == "none")
        {
            return;
        }
        
        var commandText = 
            "SELECT COUNT(*) NumEncounters FROM encounter " +
            "WHERE encounter.PatNum = " + patNum + " " + 
            "AND encounter.DateEncounter = " + SOut.Date(date) + " " + 
            "AND encounter.ProvNum = " + provNum;
        
        var count = SIn.Int(Db.GetCount(commandText));
        if (count > 0)
        {
            return;
        }

        Insert(new Encounter
        {
            PatNum = patNum,
            ProvNum = provNum,
            DateEncounter = date,
            CodeSystem = PrefC.GetString(PrefName.CQMDefaultEncounterCodeSystem),
            CodeValue = PrefC.GetString(PrefName.CQMDefaultEncounterCodeValue)
        });
    }

    public static void Insert(Encounter encounter)
    {
        EncounterCrud.Insert(encounter);
    }
}