using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MedLabResults
{
    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class MedLabResultCache : CacheListAbs<MedLabResult> {
        protected override List<MedLabResult> GetCacheFromDb() {
            string command="SELECT * FROM MedLabResult ORDER BY ItemOrder";
            return Crud.MedLabResultCrud.SelectMany(command);
        }
        protected override List<MedLabResult> TableToList(DataTable table) {
            return Crud.MedLabResultCrud.TableToList(table);
        }
        protected override MedLabResult Copy(MedLabResult MedLabResult) {
            return MedLabResult.Clone();
        }
        protected override DataTable ListToTable(List<MedLabResult> listMedLabResults) {
            return Crud.MedLabResultCrud.ListToTable(listMedLabResults,"MedLabResult");
        }
        protected override void FillCacheIfNeeded() {
            MedLabResults.GetTableFromCache(false);
        }
        protected override bool IsInListShort(MedLabResult MedLabResult) {
            return !MedLabResult.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static MedLabResultCache _MedLabResultCache=new MedLabResultCache();

    ///<summary>A list of all MedLabResults. Returns a deep copy.</summary>
    public static List<MedLabResult> ListDeep {
        get {
            return _MedLabResultCache.ListDeep;
        }
    }

    ///<summary>A list of all visible MedLabResults. Returns a deep copy.</summary>
    public static List<MedLabResult> ListShortDeep {
        get {
            return _MedLabResultCache.ListShortDeep;
        }
    }

    ///<summary>A list of all MedLabResults. Returns a shallow copy.</summary>
    public static List<MedLabResult> ListShallow {
        get {
            return _MedLabResultCache.ListShallow;
        }
    }

    ///<summary>A list of all visible MedLabResults. Returns a shallow copy.</summary>
    public static List<MedLabResult> ListShort {
        get {
            return _MedLabResultCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _MedLabResultCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _MedLabResultCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */


    /// <summary>
    ///     Returns a list of all MedLabResults from the db for a given MedLab.  Ordered by
    ///     ObsID,ObsIDSub,ResultStatus,DateTimeObs DESC.
    ///     Corrected (ResultStatus=0) will be first in the list then 1=Final, 2=Incomplete, 3=Preliminary, and 4=Cancelled.
    ///     Then ordered by DateTimeObs DESC, most recent of each status comes first in the list.
    ///     If there are no results for the lab (or medLabNum=0), this will return an empty list.
    /// </summary>
    public static List<MedLabResult> GetForLab(long medLabNum)
    {
        var command = "SELECT * FROM medlabresult WHERE MedLabNum=" + SOut.Long(medLabNum) + " ORDER BY ObsID,ObsIDSub,ResultStatus,DateTimeObs DESC";
        return MedLabResultCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets a list of all MedLabResult object from the database for all of the MedLab objects sent in.  The MedLabResults
    ///     are ordered by
    ///     ObsID,ObsIDSub,ResultStatus, and DateTimeObs DESC to make it easier to find the most recent and up to date result
    ///     for a given ObsID and
    ///     optional ObsIDSub result.  The result statuses are 0=Corrected, 1=Final, 2=Incomplete, 3=Preliminary, and
    ///     4=Cancelled.
    ///     Corrected will be first in the list for each ObsID/ObsIDSub, then Final, etc.
    ///     If there is more than one result with the same ObsID/ObsIDSub and status, the most recent DateTimeObs will be
    ///     first.
    /// </summary>
    public static List<MedLabResult> GetAllForLabs(List<MedLab> listMedLabs)
    {
        var listMedLabResults = new List<MedLabResult>();
        if (listMedLabs.Count == 0) return listMedLabResults;
        var listMedLabNums = new List<long>();
        for (var i = 0; i < listMedLabs.Count; i++) listMedLabNums.Add(listMedLabs[i].MedLabNum);
        var command = "SELECT * FROM medlabresult WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ") "
                      + "ORDER BY ObsID,ObsIDSub,ResultStatus,DateTimeObs DESC,MedLabResultNum DESC";
        return MedLabResultCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets a list of all MedLabResult objects for this patient with the same ObsID and ObsIDSub as the supplied
    ///     medLabResult,
    ///     and for the same SpecimenID and SpecimenIDFiller.  Ordered by ResultStatus,DateTimeObs descending, MedLabResultNum
    ///     descending.
    ///     Used to display the history of a result as many statuses may be received.
    /// </summary>
    public static List<MedLabResult> GetResultHist(MedLabResult medLabResult, long patNum, string specimenID, string specimenIDFiller)
    {
        var listMedLabResults = new List<MedLabResult>();
        if (medLabResult == null) return listMedLabResults;
        var command = "SELECT medlabresult.* FROM medlabresult "
                      + "INNER JOIN medlab ON medlab.MedLabNum=medlabresult.MedLabNum "
                      + "AND medlab.PatNum=" + SOut.Long(patNum) + " "
                      + "AND medlab.SpecimenID='" + SOut.String(specimenID) + "' "
                      + "AND medlab.SpecimenIDFiller='" + SOut.String(specimenIDFiller) + "' "
                      + "WHERE medlabresult.ObsID='" + SOut.String(medLabResult.ObsID) + "' "
                      + "AND medlabresult.ObsIDSub='" + SOut.String(medLabResult.ObsIDSub) + "' "
                      + "ORDER BY medlabresult.ResultStatus,medlabresult.DateTimeObs DESC,medlabresult.MedLabResultNum DESC";
        return MedLabResultCrud.SelectMany(command);
    }

    
    public static long Insert(MedLabResult medLabResult)
    {
        return MedLabResultCrud.Insert(medLabResult);
    }

    
    public static void Update(MedLabResult medLabResult)
    {
        MedLabResultCrud.Update(medLabResult);
    }

    ///<summary>Delete all of the MedLabResult objects by MedLabResultNum.</summary>
    public static void DeleteAll(List<long> listMedLabResultNums)
    {
        var command = "DELETE FROM medlabresult WHERE MedLabResultNum IN(" + string.Join(",", listMedLabResultNums) + ")";
        Db.NonQ(command);
    }

    ///<summary>Delete all of the MedLabResult objects by MedLabNum.</summary>
    public static void DeleteAllForMedLabs(List<long> listMedLabNums)
    {
        if (listMedLabNums == null || listMedLabNums.Count < 1) return;
        var command = "DELETE FROM medlabresult WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }

    public static string GetAbnormalFlagDescript(AbnormalFlag abnormalFlag)
    {
        switch (abnormalFlag)
        {
            case AbnormalFlag._gt:
                return "Panic High";
            case AbnormalFlag._lt:
                return "Panic Low";
            case AbnormalFlag.A:
                return "Abnormal";
            case AbnormalFlag.AA:
                return "Critical Abnormal";
            case AbnormalFlag.H:
                return "Above High Normal";
            case AbnormalFlag.HH:
                return "Alert High";
            case AbnormalFlag.I:
                return "Intermediate";
            case AbnormalFlag.L:
                return "Below Low Normal";
            case AbnormalFlag.LL:
                return "Alert Low";
            case AbnormalFlag.NEG:
                return "Negative";
            case AbnormalFlag.POS:
                return "Positive";
            case AbnormalFlag.R:
                return "Resistant";
            case AbnormalFlag.S:
                return "Susceptible";
            case AbnormalFlag.None:
            default:
                return "";
        }
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<MedLabResult> Refresh(long patNum){

        string command="SELECT * FROM medlabresult WHERE PatNum = "+POut.Long(patNum);
        return Crud.MedLabResultCrud.SelectMany(command);
    }

    ///<summary>Gets one MedLabResult from the db.</summary>
    public static MedLabResult GetOne(long medLabResultNum){

        return Crud.MedLabResultCrud.SelectOne(medLabResultNum);
    }

    
    public static void Delete(long medLabResultNum) {

        string command= "DELETE FROM medlabresult WHERE MedLabResultNum = "+POut.Long(medLabResultNum);
        Db.NonQ(command);
    }
    */
}