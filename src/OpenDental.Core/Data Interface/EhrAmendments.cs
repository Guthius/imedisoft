using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrAmendments
{
    ///<summary>Gets list of all amendments for a specific patient and orders them by DateTRequest</summary>
    public static List<EhrAmendment> Refresh(long patNum)
    {
        var command = "SELECT * FROM ehramendment WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY DateTRequest";
        return EhrAmendmentCrud.SelectMany(command);
    }

    ///<summary>Gets one EhrAmendment from the db.</summary>
    public static EhrAmendment GetOne(long ehrAmendmentNum)
    {
        return EhrAmendmentCrud.SelectOne(ehrAmendmentNum);
    }

    
    public static long Insert(EhrAmendment ehrAmendment)
    {
        return EhrAmendmentCrud.Insert(ehrAmendment);
    }

    
    public static void Update(EhrAmendment ehrAmendment)
    {
        EhrAmendmentCrud.Update(ehrAmendment);
    }

    
    public static void Delete(long ehrAmendmentNum)
    {
        var command = "DELETE FROM ehramendment WHERE EhrAmendmentNum = " + SOut.Long(ehrAmendmentNum);
        Db.NonQ(command);
    }
}