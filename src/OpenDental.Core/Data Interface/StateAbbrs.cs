using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class StateAbbrs
{
    
    public static long Insert(StateAbbr stateAbbr)
    {
        return StateAbbrCrud.Insert(stateAbbr);
    }

    
    public static void Update(StateAbbr stateAbbr)
    {
        StateAbbrCrud.Update(stateAbbr);
    }

    
    public static void Delete(long stateAbbrNum)
    {
        StateAbbrCrud.Delete(stateAbbrNum);
    }

    /// <summary>
    ///     Returns a list of StatesAbbrs with abbreviations similar to the supplied string.
    ///     Used in dropdown list from state field for faster entry.
    /// </summary>
    public static List<StateAbbr> GetSimilarAbbrs(string abbr)
    {
        return GetWhere(x => x.Abbr.StartsWith(abbr, StringComparison.CurrentCultureIgnoreCase));
    }

    ///<summary>Returns the Medicaid ID Length for a given abbreviation.</summary>
    public static int GetMedicaidIDLength(string abbr)
    {
        var stateAbbr = GetFirstOrDefault(x => x.Abbr.ToLower() == abbr.ToLower());
        if (stateAbbr == null) return 0;
        return stateAbbr.MedicaidIDLength;
    }

    ///<summary>Returns true if the abbreviation exists in the stateabbr table.</summary>
    public static bool IsValidAbbr(string abbr)
    {
        return GetFirstOrDefault(x => x.Abbr.ToLower() == abbr.ToLower()) != null;
    }

    #region CachePattern

    private class StateAbbrCache : CacheListAbs<StateAbbr>
    {
        protected override List<StateAbbr> GetCacheFromDb()
        {
            var command = "SELECT * FROM stateabbr ORDER BY Abbr";
            return StateAbbrCrud.SelectMany(command);
        }

        protected override List<StateAbbr> TableToList(DataTable dataTable)
        {
            return StateAbbrCrud.TableToList(dataTable);
        }

        protected override StateAbbr Copy(StateAbbr item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<StateAbbr> items)
        {
            return StateAbbrCrud.ListToTable(items, "StateAbbr");
        }

        protected override void FillCacheIfNeeded()
        {
            StateAbbrs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly StateAbbrCache _stateAbbrCache = new();

    public static List<StateAbbr> GetDeepCopy(bool isShort = false)
    {
        return _stateAbbrCache.GetDeepCopy(isShort);
    }

    public static List<StateAbbr> GetWhere(Predicate<StateAbbr> match, bool isShort = false)
    {
        return _stateAbbrCache.GetWhere(match, isShort);
    }

    public static StateAbbr GetFirstOrDefault(Func<StateAbbr, bool> match, bool isShort = false)
    {
        return _stateAbbrCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _stateAbbrCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _stateAbbrCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _stateAbbrCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<StateAbbr> Refresh(long patNum){

        string command="SELECT * FROM stateabbr WHERE PatNum = "+POut.Long(patNum);
        return Crud.StateAbbrCrud.SelectMany(command);
    }

    ///<summary>Gets one StateAbbr from the db.</summary>
    public static StateAbbr GetOne(long stateAbbrNum) {

        return Crud.StateAbbrCrud.SelectOne(stateAbbrNum);
    }
    */
}