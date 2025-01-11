using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FamilyHealths
{
    
    public static void Delete(long familyHealthNum)
    {
        var command = "DELETE FROM familyhealth WHERE FamilyHealthNum = " + SOut.Long(familyHealthNum);
        Db.NonQ(command);
    }

    
    public static long Insert(FamilyHealth familyHealth)
    {
        return FamilyHealthCrud.Insert(familyHealth);
    }

    
    public static void Update(FamilyHealth familyHealth)
    {
        FamilyHealthCrud.Update(familyHealth);
    }

    
    public static List<FamilyHealth> GetFamilyHealthForPat(long patNum)
    {
        var command = "SELECT * FROM familyhealth WHERE PatNum = " + SOut.Long(patNum);
        return FamilyHealthCrud.SelectMany(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one FamilyHealth from the db.</summary>
    public static FamilyHealth GetOne(long familyHealthNum){

        return Crud.FamilyHealthCrud.SelectOne(familyHealthNum);
    }
    */
}