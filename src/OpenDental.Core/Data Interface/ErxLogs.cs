using System;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ErxLogs
{
    
    public static long Insert(ErxLog erxLog)
    {
        return ErxLogCrud.Insert(erxLog);
    }

    /// <summary>
    ///     Returns the latest ErxLog entry for the specified patient and before the specified dateTimeMax. Can return null.
    ///     Called from Chart when fetching prescriptions from NewCrop to determine the provider on incoming prescriptions.
    /// </summary>
    public static ErxLog GetLatestForPat(long patNum, DateTime dateTimeMax)
    {
        var command = DbHelper.LimitOrderBy("SELECT * FROM erxlog WHERE PatNum=" + SOut.Long(patNum) + " AND DateTStamp<" + SOut.DateT(dateTimeMax) + " ORDER BY DateTStamp DESC", 1);
        var listErxLogs = ErxLogCrud.SelectMany(command);
        if (listErxLogs.Count == 0) return null;
        return listErxLogs[0];
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<ErxLog> Refresh(long patNum){

        string command="SELECT * FROM erxlog WHERE PatNum = "+POut.Long(patNum);
        return Crud.ErxLogCrud.SelectMany(command);
    }

    ///<summary>Gets one ErxLog from the db.</summary>
    public static ErxLog GetOne(long erxLogNum){

        return Crud.ErxLogCrud.SelectOne(erxLogNum);
    }

    
    public static void Update(ErxLog erxLog){

        Crud.ErxLogCrud.Update(erxLog);
    }

    
    public static void Delete(long erxLogNum) {

        string command= "DELETE FROM erxlog WHERE ErxLogNum = "+POut.Long(erxLogNum);
        Db.NonQ(command);
    }
    */
}