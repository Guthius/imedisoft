using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrPatients
{
	/// <summary>
	///     Only call when EHR is enabled.  Creates the ehrpatient record for the patient if a record does not already
	///     exist.  Always returns a non-null EhrPatient.
	/// </summary>
	public static EhrPatient Refresh(long patNum)
    {
        var command = "SELECT COUNT(*) FROM ehrpatient WHERE patnum='" + SOut.Long(patNum) + "'";
        if (Db.GetCount(command) == "0") //A record does not exist for this patient yet.
            Insert(patNum); //Create a new record.
        command = "SELECT * FROM ehrpatient WHERE patnum ='" + SOut.Long(patNum) + "'";
        return EhrPatientCrud.SelectOne(command);
    }

    
    public static void Update(EhrPatient ehrPatient)
    {
        EhrPatientCrud.Update(ehrPatient);
    }

    ///<summary>Private method only called from Refresh.</summary>
    private static void Insert(long patNum)
    {
        //Random keys not necessary to check because of 1:1 patNum.
        //However, this is a lazy insert, so multiple locations might attempt it.
        //Just in case, we will have it fail silently.
        //try {
        //AD~ Attempted bug fix.  This command was throwing a "Error Code: 1364 Field 'MotherMaidenFname' doesn't have a default value" UE.  We 
        //believe this is caused by a third party changing the sql_mode variable after our startup call to MiscData.SetSqlMode() which clears this 
        //variable if set to a more strict mode.  A more strict sql_mode would cause warnings to be returned as errors.  Our normal pattern is to use 
        //the Crud Insert method, but due to replication delays (see comment below), we use a custom query to update on duplicate keys instead.  
        //Previously, we only specified the PatNum and VacShareOK in the query.  Now, we explicitly set blank values on all other fields.  For now, we
        //will continue to leave the try/catch commented out, so that we can continue to monitor this code for other bugs we haven't observed yet.
        var command = "INSERT INTO ehrpatient (PatNum,MotherMaidenFname,MotherMaidenLname,VacShareOk,MedicaidState,SexualOrientation,GenderIdentity,"
                      + "SexualOrientationNote,GenderIdentityNote) "
                      + "VALUES(" + SOut.Long(patNum) + ",'','',0,'','','','','')"; //VacShareOk cannot be NULL for Oracle.
        command += " ON DUPLICATE KEY UPDATE PatNum='" + patNum + "'";
        Db.NonQ(command);
        //}
        //catch (Exception ex){
        //	//Fail Silently.
        //}
    }

    ///<summary>Gets one EhrPatient from the db.  Returns null if there is no entry.</summary>
    public static EhrPatient GetOne(long patNum)
    {
        return EhrPatientCrud.SelectOne(patNum);
    }

    public static List<EhrPatient> GetByPatNums(List<long> listPatNums)
    {
        if (listPatNums.IsNullOrEmpty()) return new List<EhrPatient>();

        var command = "SELECT * FROM ehrpatient WHERE PatNum IN (" + string.Join(",", listPatNums) + ")";
        return EhrPatientCrud.SelectMany(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EhrPatient> Refresh(long patNum){

        string command="SELECT * FROM ehrpatient WHERE PatNum = "+POut.Long(patNum);
        return Crud.EhrPatientCrud.SelectMany(command);
    }

    
    public static void Delete(long patNum) {

        string command= "DELETE FROM ehrpatient WHERE PatNum = "+POut.Long(patNum);
        Db.NonQ(command);
    }
    */
}