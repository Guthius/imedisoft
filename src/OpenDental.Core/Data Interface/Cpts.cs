using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Cpts
{
    public static List<Cpt> GetBySearchText(string searchText)
    {
        var listSearchTokens = searchText.Split(' ').ToList();
        var command = @"SELECT * FROM cpt WHERE ";
        for (var i = 0; i < listSearchTokens.Count; i++)
        {
            if (i > 0) command += "AND ";
            command += "(CptCode LIKE '%" + SOut.String(listSearchTokens[i]) + "%' OR Description LIKE '%"
                       + SOut.String(listSearchTokens[i]) + "%') ";
        }

        return CptCrud.SelectMany(command);
    }

    
    public static long Insert(Cpt cpt)
    {
        return CptCrud.Insert(cpt);
    }

    public static List<Cpt> GetAll()
    {
        var command = "SELECT * FROM cpt";
        return CptCrud.SelectMany(command);
    }

    ///<summary>Gets one Cpt object directly from the database by CptCode.  If code does not exist, returns null.</summary>
    public static Cpt GetByCode(string cptCode)
    {
        var command = "SELECT * FROM cpt WHERE CptCode='" + SOut.String(cptCode) + "'";
        return CptCrud.SelectOne(command);
    }

    ///<summary>Returns the total count of CPT codes.  CPT codes cannot be hidden.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM cpt";
        return SIn.Long(Db.GetCount(command));
    }

    /// <summary>
    ///     Updates an existing CPT code description if versionID is newer than current versionIDs.  If versionID is
    ///     different than existing versionIDs, it will be added to the comma delimited list.
    /// </summary>
    public static void UpdateDescription(string cptCode, string description, string versionID)
    {
        var cpt = GetByCode(SOut.String(cptCode));
        var listVersionIDs = cpt.VersionIDs.Split(',').ToList();
        var foundVersionID = false;
        var versionIDMax = "";
        for (var i = 0; i < listVersionIDs.Count; i++)
        {
            if (string.Compare(listVersionIDs[i], versionIDMax) > 0) //Find max versionID in list
                versionIDMax = listVersionIDs[i];
            if (listVersionIDs[i] == versionID) //Find if versionID is already in list
                foundVersionID = true;
        }

        if (!foundVersionID) //If the current version isn't already in the list
            cpt.VersionIDs += ',' + versionID; //VersionID should never be blank for an existing code... should we check?
        if (string.Compare(versionID, versionIDMax) >= 0) //If newest version
            cpt.Description = description;
        CptCrud.Update(cpt);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<Cpt> Refresh(long patNum){

        string command="SELECT * FROM cpt WHERE PatNum = "+POut.Long(patNum);
        return Crud.CptCrud.SelectMany(command);
    }

    ///<summary>Gets one Cpt from the db.</summary>
    public static Cpt GetOne(long cptNum){

        return Crud.CptCrud.SelectOne(cptNum);
    }

    
    public static void Update(Cpt cpt){

        Crud.CptCrud.Update(cpt);
    }

    
    public static void Delete(long cptNum) {

        string command= "DELETE FROM cpt WHERE CptNum = "+POut.Long(cptNum);
        Db.NonQ(command);
    }
    */
}