using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Allergies
{
    public static List<Allergy> Refresh(long patNum)
    {
        return AllergyCrud.SelectMany("SELECT * FROM allergy WHERE PatNum = " + patNum);
    }

    public static void Insert(Allergy allergy)
    {
        AllergyCrud.Insert(allergy);
    }

    public static void Update(Allergy allergy)
    {
        AllergyCrud.Update(allergy);
    }

    public static void Delete(long allergyNum)
    {
        Db.NonQ("DELETE FROM allergy WHERE AllergyNum = " + allergyNum);
    }

    public static List<Allergy> GetAll(long patNum, bool showInactive)
    {
        var command = "SELECT * FROM allergy WHERE PatNum = " + patNum;

        if (!showInactive)
        {
            command += " AND StatusIsActive<>0";
        }

        return AllergyCrud.SelectMany(command);
    }

    public static List<Allergy> GetPatientData(long patNum)
    {
        return AllergyCrud.SelectMany("SELECT * FROM allergy WHERE PatNum = " + patNum);
    }

    public static string[] GetPatNamesForAllergy(long allergyDefNum)
    {
        var dataTable = DataCore.GetTable(
            "SELECT CONCAT(CONCAT(CONCAT(CONCAT(LName, ', '), FName), ' '), Preferred) FROM allergy,patient " +
            "WHERE allergy.PatNum=patient.PatNum " +
            "AND allergy.AllergyDefNum=" + allergyDefNum);

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

    public static void ResetTimeStamps(long patNum, bool onlyActive)
    {
        var command = "UPDATE allergy SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum =" + patNum;

        if (onlyActive)
        {
            command += " AND StatusIsActive = 1";
        }

        Db.NonQ(command);
    }
}