using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ResellerServices
{
    #region Insert

    
    public static long Insert(ResellerService resellerService)
    {
        return ResellerServiceCrud.Insert(resellerService);
    }

    #endregion

    #region Update

    
    public static void Update(ResellerService resellerService)
    {
        ResellerServiceCrud.Update(resellerService);
    }

    #endregion

    #region Delete

    
    public static void Delete(long resellerServiceNum)
    {
        var command = "DELETE FROM resellerservice WHERE ResellerServiceNum = " + SOut.Long(resellerServiceNum);
        Db.NonQ(command);
    }

    #endregion

    #region Get Methods

    
    public static List<ResellerService> GetAll()
    {
        return ResellerServiceCrud.SelectMany("SELECT * FROM resellerservice");
    }

    
    public static List<ResellerService> GetServicesForReseller(long resellerNum)
    {
        var command = "SELECT * FROM resellerservice WHERE ResellerNum = " + SOut.Long(resellerNum);
        return ResellerServiceCrud.SelectMany(command);
    }

    ///<summary>Gets one ResellerService from the db.</summary>
    public static ResellerService GetOne(long resellerServiceNum)
    {
        return ResellerServiceCrud.SelectOne(resellerServiceNum);
    }

    #endregion
}