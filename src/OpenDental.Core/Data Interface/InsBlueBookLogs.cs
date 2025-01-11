using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsBlueBookLogs
{
    #region Modification Methods

    ///<summary>Inserts an InsBlueBookLog into the DB and returns the InsBlueBookLogNum.</summary>
    public static long Insert(InsBlueBookLog insBlueBookLog)
    {
        return InsBlueBookLogCrud.Insert(insBlueBookLog);
    }

    #endregion Modification Methods

    #region Get Methods

    ///<summary>Gets all InsBlueBookLogs for a given ClaimProcNum</summary>
    public static List<InsBlueBookLog> GetAllByClaimProcNum(long claimProcNum)
    {
        var command = "SELECT * FROM insbluebooklog WHERE ClaimProcNum = " + SOut.Long(claimProcNum);
        return InsBlueBookLogCrud.SelectMany(command);
    }

    ///<summary>Gets the most recent InsBlueBookLog for a ClaimProc. Returns null if ClaimProc has none.</summary>
    public static InsBlueBookLog GetMostRecentForClaimProc(long claimProcNum)
    {
        var command = $@"
				SELECT * FROM insbluebooklog
				WHERE ClaimProcNum={SOut.Long(claimProcNum)}
				ORDER BY insbluebooklog.DateTEntry DESC
				LIMIT 1";
        return InsBlueBookLogCrud.SelectOne(command);
    }

    #endregion Get Methods

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods
    
    public static List<InsBlueBookLog> Refresh(long patNum){

        string command="SELECT * FROM insbluebooklog WHERE PatNum = "+POut.Long(patNum);
        return Crud.InsBlueBookLogCrud.SelectMany(command);
    }

    ///<summary>Gets one InsBlueBookLog from the db.</summary>
    public static InsBlueBookLog GetOne(long insBlueBookLogNum){

        return Crud.InsBlueBookLogCrud.SelectOne(insBlueBookLogNum);
    }
    #endregion Get Methods
    #region Modification Methods
    
    public static void Update(InsBlueBookLog insBlueBookLog){

        Crud.InsBlueBookLogCrud.Update(insBlueBookLog);
    }
    
    public static void Delete(long insBlueBookLogNum) {

        Crud.InsBlueBookLogCrud.Delete(insBlueBookLogNum);
    }
    #endregion Modification Methods
    #region Misc Methods



    #endregion Misc Methods
    */
}