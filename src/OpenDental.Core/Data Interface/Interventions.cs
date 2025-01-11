using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Interventions
{
    //If this table type will exist as cached data, uncomment the CachePattern region below.
    /*
    #region CachePattern
    private class InterventionCache : CacheListAbs<Intervention> {
        protected override List<Intervention> GetCacheFromDb() {
            string command="SELECT * FROM Intervention ORDER BY ItemOrder";
            return Crud.InterventionCrud.SelectMany(command);
        }
        protected override List<Intervention> TableToList(DataTable table) {
            return Crud.InterventionCrud.TableToList(table);
        }
        protected override Intervention Copy(Intervention Intervention) {
            return Intervention.Clone();
        }
        protected override DataTable ListToTable(List<Intervention> listInterventions) {
            return Crud.InterventionCrud.ListToTable(listInterventions,"Intervention");
        }
        protected override void FillCacheIfNeeded() {
            Interventions.GetTableFromCache(false);
        }
        protected override bool IsInListShort(Intervention Intervention) {
            return !Intervention.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static InterventionCache _InterventionCache=new InterventionCache();

    ///<summary>A list of all Interventions. Returns a deep copy.</summary>
    public static List<Intervention> ListDeep {
        get {
            return _InterventionCache.ListDeep;
        }
    }

    ///<summary>A list of all visible Interventions. Returns a deep copy.</summary>
    public static List<Intervention> ListShortDeep {
        get {
            return _InterventionCache.ListShortDeep;
        }
    }

    ///<summary>A list of all Interventions. Returns a shallow copy.</summary>
    public static List<Intervention> ListShallow {
        get {
            return _InterventionCache.ListShallow;
        }
    }

    ///<summary>A list of all visible Interventions. Returns a shallow copy.</summary>
    public static List<Intervention> ListShort {
        get {
            return _InterventionCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _InterventionCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _InterventionCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    
    public static long Insert(Intervention intervention)
    {
        return InterventionCrud.Insert(intervention);
    }

    
    public static void Update(Intervention intervention)
    {
        InterventionCrud.Update(intervention);
    }

    
    public static void Delete(long interventionNum)
    {
        var command = "DELETE FROM intervention WHERE InterventionNum = " + SOut.Long(interventionNum);
        Db.NonQ(command);
    }

    
    public static List<Intervention> Refresh(long patNum)
    {
        var command = "SELECT * FROM intervention WHERE PatNum = " + SOut.Long(patNum);
        return InterventionCrud.SelectMany(command);
    }

    public static List<Intervention> Refresh(long patNum, InterventionCodeSet interventionCodeSet)
    {
        var command = "SELECT * FROM intervention WHERE PatNum = " + SOut.Long(patNum) + " AND CodeSet = " + SOut.Int((int) interventionCodeSet);
        return InterventionCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets list of CodeValue strings from interventions with DateEntry in the last year and CodeSet equal to the supplied
    ///     codeSet.
    ///     Result list is grouped by CodeValue, CodeSystem even though we only return the list of CodeValues.  However, there
    ///     are no codes in the
    ///     EHR intervention code list that conflict between code systems, so we should never have a duplicate code in the
    ///     returned list.
    /// </summary>
    public static List<string> GetAllForCodeSet(InterventionCodeSet interventionCodeSet)
    {
        var command = "SELECT CodeValue FROM intervention WHERE CodeSet=" + SOut.Int((int) interventionCodeSet) + " "
                      + "AND " + DbHelper.DtimeToDate("DateEntry") + ">=" + SOut.Date(MiscData.GetNowDateTime().AddYears(-1)) + " "
                      + "GROUP BY CodeValue,CodeSystem";
        return Db.GetListString(command);
    }

    ///<summary>Gets one Intervention from the db.</summary>
    public static Intervention GetOne(long interventionNum)
    {
        return InterventionCrud.SelectOne(interventionNum);
    }
}