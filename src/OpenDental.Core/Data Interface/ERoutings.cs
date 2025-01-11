using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ERoutings
{
    #region Methods - Get

    
    public static List<ERouting> Refresh(long patNum)
    {
        var command = "SELECT * FROM erouting WHERE PatNum = " + SOut.Long(patNum);
        return ERoutingCrud.SelectMany(command);
    }

    public static ERouting GetCurrentIncompleteERoutingForPat(long patNum)
    {
        var command =
            $@"SELECT * FROM erouting 
				WHERE PatNum = {SOut.Long(patNum)} 
				AND SecDateTEntry BETWEEN {SOut.DateT(DateTime.Today)} AND {SOut.DateT(DateTime.Now)}
				AND !IsComplete
				ORDER BY SecDateTEntry DESC
				LIMIT 1";
        return ERoutingCrud.SelectOne(command);
    }

    public static List<ERouting> GetAllForClinicInDateRange(long clinicNum, DateTime dateFrom, DateTime dateTo, bool includeAll)
    {
        var command = $"SELECT * FROM erouting WHERE SecDateTEntry BETWEEN {SOut.DateT(dateFrom)} AND {SOut.DateT(dateTo)} + INTERVAL 1 DAY ";
        if (includeAll) command += "AND ClinicNum=" + SOut.Long(clinicNum);

        return ERoutingCrud.SelectMany(command);
    }

    ///<summary>Gets one eRouting from the db.</summary>
    public static ERouting GetOne(long eRoutingNum)
    {
        return ERoutingCrud.SelectOne(eRoutingNum);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(ERouting eRouting)
    {
        return ERoutingCrud.Insert(eRouting);
    }

    
    public static void Update(ERouting eRouting)
    {
        ERoutingCrud.Update(eRouting);
    }

    
    public static void Delete(long eRoutingNum)
    {
        ERoutingCrud.Delete(eRoutingNum);
    }

    #endregion Methods - Modify
}