using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MedLabFacilities
{
	/// <summary>
	///     Checks the database for a MedLabFacility with matching name, address, city, state, zip, phone, and director
	///     title/name.
	///     If the facility doesn't exist, it's inserted.  Returns the MedLabFacilityNum for the facility inserted or found.
	///     Doesn't need any indexes, this runs in under a second with 100k worst case scenario rows (identical data).
	/// </summary>
	public static long InsertIfNotInDb(MedLabFacility medLabFacility)
    {
        var command = "SELECT * FROM medlabfacility "
                      + "WHERE FacilityName='" + SOut.String(medLabFacility.FacilityName) + "' "
                      + "AND Address='" + SOut.String(medLabFacility.Address) + "' "
                      + "AND City='" + SOut.String(medLabFacility.City) + "' "
                      + "AND State='" + SOut.String(medLabFacility.State) + "' "
                      + "AND Zip='" + SOut.String(medLabFacility.Zip) + "' "
                      + "AND Phone='" + SOut.String(medLabFacility.Phone) + "' "
                      + "AND DirectorTitle='" + SOut.String(medLabFacility.DirectorTitle) + "' "
                      + "AND DirectorLName='" + SOut.String(medLabFacility.DirectorLName) + "' "
                      + "AND DirectorFName='" + SOut.String(medLabFacility.DirectorFName) + "'";
        var medLabFacilityDb = MedLabFacilityCrud.SelectOne(command);
        if (medLabFacilityDb == null) return MedLabFacilityCrud.Insert(medLabFacility);
        return medLabFacilityDb.MedLabFacilityNum;
    }

    ///<summary>Gets one MedLabFacility from the db.</summary>
    public static MedLabFacility GetOne(long medLabFacilityNum)
    {
        return MedLabFacilityCrud.SelectOne(medLabFacilityNum);
    }

    /// <summary>
    ///     Returns a list of MedLabFacilityNums, the order in the list will be the facility ID on the report.  Basically a
    ///     local re-numbering.
    ///     Each message has a facility or facilities with footnote IDs, e.g. 01, 02, etc.  The results each link to the
    ///     facility that performed the test.
    ///     But if there are multiple messages for a test order, e.g. when there is a final result for a subset of the original
    ///     test results,
    ///     the additional message may have a facility with footnote ID of 01 that is different than the original message
    ///     facility with ID 01.
    ///     So each ID could link to multiple facilities.  We will re-number the facilities so that each will have a unique
    ///     number for this report.
    /// </summary>
    public static List<MedLabFacility> GetFacilityList(List<MedLab> listMedLabs, out List<MedLabResult> listMedLabResults)
    {
        listMedLabResults = listMedLabs.SelectMany(x => x.ListMedLabResults).ToList();
        for (var i = listMedLabResults.Count - 1; i > -1; i--)
        {
            //loop through backward and only keep the most final/most recent result
            if (i == 0) break;
            if (listMedLabResults[i].ObsID == listMedLabResults[i - 1].ObsID && listMedLabResults[i].ObsIDSub == listMedLabResults[i - 1].ObsIDSub) listMedLabResults.RemoveAt(i);
        }

        listMedLabResults.OrderBy(x => x.MedLabNum).ThenBy(x => x.MedLabResultNum);
        //listResults will now only contain the most recent or most final/corrected results, sorted by the order inserted in the db
        var listMedLabFacAttaches = MedLabFacAttaches.GetAllForResults(listMedLabResults.Select(x => x.MedLabResultNum).Distinct().ToList());
        var dictionaryResultNumFacNum = listMedLabFacAttaches.ToDictionary(x => x.MedLabResultNum, x => x.MedLabFacilityNum);
        var listMedLabFacilities = new List<MedLabFacility>();
        for (var i = 0; i < listMedLabResults.Count; i++)
        {
            if (!dictionaryResultNumFacNum.ContainsKey(listMedLabResults[i].MedLabResultNum)) continue;
            var facilityNum = dictionaryResultNumFacNum[listMedLabResults[i].MedLabResultNum];
            if (!listMedLabFacilities.Exists(x => x.MedLabFacilityNum == facilityNum)) listMedLabFacilities.Add(GetOne(facilityNum));
            listMedLabResults[i].FacilityID = (listMedLabFacilities.Select(x => x.MedLabFacilityNum).ToList().IndexOf(facilityNum) + 1).ToString().PadLeft(2, '0');
        }

        return listMedLabFacilities;
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<MedLabFacility> Refresh(long patNum){

        string command="SELECT * FROM medlabfacility WHERE PatNum = "+POut.Long(patNum);
        return Crud.MedLabFacilityCrud.SelectMany(command);
    }

    
    public static long Insert(MedLabFacility medLabFacility){

        return Crud.MedLabFacilityCrud.Insert(medLabFacility);
    }

    
    public static void Update(MedLabFacility medLabFacility){

        Crud.MedLabFacilityCrud.Update(medLabFacility);
    }

    
    public static void Delete(long medLabFacilityNum) {

        string command= "DELETE FROM medlabfacility WHERE MedLabFacilityNum = "+POut.Long(medLabFacilityNum);
        Db.NonQ(command);
    }
    */
}