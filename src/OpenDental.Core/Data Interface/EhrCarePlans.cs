using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrCarePlans
{
    
    public static List<EhrCarePlan> Refresh(long patNum)
    {
        var command = "SELECT * FROM ehrcareplan WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY DatePlanned";
        return EhrCarePlanCrud.SelectMany(command);
    }

    
    public static long Insert(EhrCarePlan ehrCarePlan)
    {
        return EhrCarePlanCrud.Insert(ehrCarePlan);
    }

    
    public static void Update(EhrCarePlan ehrCarePlan)
    {
        EhrCarePlanCrud.Update(ehrCarePlan);
    }

    
    public static void Delete(long ehrCarePlanNum)
    {
        var command = "DELETE FROM ehrcareplan WHERE EhrCarePlanNum = " + SOut.Long(ehrCarePlanNum);
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one EhrCarePlan from the db.</summary>
    public static EhrCarePlan GetOne(long ehrCarePlanNum){

        return Crud.EhrCarePlanCrud.SelectOne(ehrCarePlanNum);
    }

    */
}