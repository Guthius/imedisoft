using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class Allergies
{
    public static void Insert(Allergy allergy)
    {
        AllergyCrud.Insert(allergy);
    }

    public static void Update(Allergy allergy)
    {
        AllergyCrud.Update(allergy);
    }

    public static List<Allergy> GetAll(long patNum, bool showInactive)
    {
        var commandText = "SELECT * FROM allergy WHERE PatNum = " + patNum;

        if (!showInactive)
        {
            commandText += " AND StatusIsActive <> 0";
        }

        return AllergyCrud.SelectMany(commandText);
    }

    public static List<Allergy> GetPatientData(long patNum)
    {
        return AllergyCrud.SelectMany("SELECT * FROM allergy WHERE PatNum = " + patNum);
    }

    public static string[] GetPatNamesForAllergy(long allergyDefNum)
    {
        var dataTable = DataCore.GetTable(
            "SELECT CONCAT(CONCAT(CONCAT(CONCAT(LName, ', '), FName), ' '), Preferred) FROM allergy,patient " +
            "WHERE allergy.PatNum = patient.PatNum " +
            "AND allergy.AllergyDefNum = " + allergyDefNum);

        var patNames = new string[dataTable.Rows.Count];

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            patNames[i] = SIn.String(dataTable.Rows[i][0].ToString());
        }

        return patNames;
    }

    public static List<long> GetPatientsWithAllergy(List<long> patNums)
    {
        if (patNums.Count == 0)
        {
            return [];
        }

        return Db.GetListLong(
            "SELECT DISTINCT PatNum FROM allergy " +
            "WHERE PatNum IN (" + string.Join(",", patNums) + ") " +
            "AND allergy.AllergyDefNum != " + PrefC.GetLong(PrefName.AllergiesIndicateNone));
    }
}