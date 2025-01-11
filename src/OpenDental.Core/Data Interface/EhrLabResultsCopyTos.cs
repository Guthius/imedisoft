using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrLabResultsCopyTos
{
    
    public static List<EhrLabResultsCopyTo> GetForLab(long ehrLabNum)
    {
        var command = "SELECT * FROM ehrlabresultscopyto WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        return EhrLabResultsCopyToCrud.SelectMany(command);
    }

    ///<summary>Deletes notes for lab results too.</summary>
    public static void DeleteForLab(long ehrLabNum)
    {
        var command = "DELETE FROM ehrlabresultscopyto WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        Db.NonQ(command);
    }

    
    public static long Insert(EhrLabResultsCopyTo ehrLabResultsCopyTo)
    {
        return EhrLabResultsCopyToCrud.Insert(ehrLabResultsCopyTo);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EhrLabResultsCopyTo> Refresh(long patNum){

        string command="SELECT * FROM ehrlabresultscopyto WHERE PatNum = "+POut.Long(patNum);
        return Crud.EhrLabResultsCopyToCrud.SelectMany(command);
    }

    ///<summary>Gets one EhrLabResultsCopyTo from the db.</summary>
    public static EhrLabResultsCopyTo GetOne(long ehrLabResultsCopyToNum){

        return Crud.EhrLabResultsCopyToCrud.SelectOne(ehrLabResultsCopyToNum);
    }

    
    public static void Update(EhrLabResultsCopyTo ehrLabResultsCopyTo){

        Crud.EhrLabResultsCopyToCrud.Update(ehrLabResultsCopyTo);
    }

    
    public static void Delete(long ehrLabResultsCopyToNum) {

        string command= "DELETE FROM ehrlabresultscopyto WHERE EhrLabResultsCopyToNum = "+POut.Long(ehrLabResultsCopyToNum);
        Db.NonQ(command);
    }
    */
}