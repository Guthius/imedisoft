using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Allergies
{
    public static List<Allergy> Refresh(long patNum)
    {
        var command = "SELECT * FROM allergy WHERE PatNum = " + SOut.Long(patNum);
        return AllergyCrud.SelectMany(command);
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
        var command = "DELETE FROM allergy WHERE AllergyNum = " + SOut.Long(allergyNum);
        Db.NonQ(command);
    }

    public static List<Allergy> GetAll(long patNum, bool showInactive)
    {
        var command = "SELECT * FROM allergy WHERE PatNum = " + SOut.Long(patNum);
        if (!showInactive) command += " AND StatusIsActive<>0";
        return AllergyCrud.SelectMany(command);
    }
    
    public static List<Allergy> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM allergy WHERE PatNum = " + SOut.Long(patNum);
        return AllergyCrud.SelectMany(command);
    }

    public static string[] GetPatNamesForAllergy(long allergyDefNum)
    {
        var command = "SELECT CONCAT(CONCAT(CONCAT(CONCAT(LName,', '),FName),' '),Preferred) FROM allergy,patient "
                      + "WHERE allergy.PatNum=patient.PatNum "
                      + "AND allergy.AllergyDefNum=" + SOut.Long(allergyDefNum);
        var table = DataCore.GetTable(command);
        var retVal = new string[table.Rows.Count];
        for (var i = 0; i < table.Rows.Count; i++) retVal[i] = SIn.String(table.Rows[i][0].ToString());
        return retVal;
    }

    public static List<long> GetPatientsWithAllergy(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<long>();
        var command = "SELECT DISTINCT PatNum FROM allergy WHERE PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND allergy.AllergyDefNum != " + SOut.Long(PrefC.GetLong(PrefName.AllergiesIndicateNone));
        return Db.GetListLong(command);
    }

    public static void ResetTimeStamps(long patNum, bool onlyActive)
    {
        var command = "UPDATE allergy SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum =" + SOut.Long(patNum);
        if (onlyActive) command += " AND StatusIsActive = " + SOut.Bool(onlyActive);
        Db.NonQ(command);
    }
}