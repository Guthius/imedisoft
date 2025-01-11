using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrAptObses
{
    
    public static List<EhrAptObs> Refresh(long aptNum)
    {
        var command = "SELECT * FROM ehraptobs WHERE AptNum = " + SOut.Long(aptNum);
        return EhrAptObsCrud.SelectMany(command);
    }

    
    public static long Insert(EhrAptObs ehrAptObs)
    {
        return EhrAptObsCrud.Insert(ehrAptObs);
    }

    
    public static void Update(EhrAptObs ehrAptObs)
    {
        EhrAptObsCrud.Update(ehrAptObs);
    }

    
    public static void Delete(long ehrAptObsNum)
    {
        var command = "DELETE FROM ehraptobs WHERE EhrAptObsNum = " + SOut.Long(ehrAptObsNum);
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one EhrAptObs from the db.</summary>
    public static EhrAptObs GetOne(long ehrAptObsNum){

        return Crud.EhrAptObsCrud.SelectOne(ehrAptObsNum);
    }


    */
}