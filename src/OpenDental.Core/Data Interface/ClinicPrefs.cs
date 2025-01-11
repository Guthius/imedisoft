using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ClinicPrefs
{
    
    public static long Insert(ClinicPref clinicPref)
    {
        return ClinicPrefCrud.Insert(clinicPref);
    }

    
    public static void Update(ClinicPref clinicPref)
    {
        ClinicPrefCrud.Update(clinicPref);
    }

    public static void Update(ClinicPref clinicPrefNew, ClinicPref clinicPrefOld)
    {
        ClinicPrefCrud.Update(clinicPrefNew, clinicPrefOld);
    }

    
    public static void Delete(long clinicPrefNum)
    {
        ClinicPrefCrud.Delete(clinicPrefNum);
    }

    /// <summary>
    ///     Inserts, updates, or deletes db rows to match listNew.  No need to pass in userNum, it's set before remoting role
    ///     check and passed to
    ///     the server if necessary.  Doesn't create ApptComm items, but will delete them.  If you use Sync, you must create
    ///     new Apptcomm items.
    /// </summary>
    public static bool Sync(List<ClinicPref> listClinicPrefsNew, List<ClinicPref> listClinicPrefOld)
    {
        return ClinicPrefCrud.Sync(listClinicPrefsNew, listClinicPrefOld);
    }

    ///<summary>If including default, it will create an extra "clinicpref" with ClinicNum=0, based on the pref.</summary>
    public static List<ClinicPref> GetPrefAllClinics(PrefName prefName, bool includeDefault = false)
    {
        var listClinicPrefs = new List<ClinicPref>();
        if (includeDefault)
            listClinicPrefs.Add(new ClinicPref
            {
                ClinicNum = 0,
                PrefName = prefName,
                ValueString = prefName.GetValueAsText()
            });
        ;
        listClinicPrefs.AddRange(GetWhere(x => x.PrefName == prefName));
        return listClinicPrefs;
    }

    
    public static ClinicPref GetPref(PrefName prefName, long clinicNum, bool isDefaultIncluded = false)
    {
        return GetPrefAllClinics(prefName, isDefaultIncluded).Find(x => x.ClinicNum == clinicNum);
    }

    ///<summary>Gets the ValueString for this clinic's pref or gets the actual preference if it does not exist.</summary>
    public static string GetPrefValue(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPrefAllClinics(prefName).Find(x => x.ClinicNum == clinicNum);
        if (clinicPref == null) return PrefC.GetString(prefName);
        return clinicPref.ValueString;
    }

    ///<summary>Update ClinicPrefs that contains a comma-delimited list of DefNums if there are changes.</summary>
    public static void UpdateDefNumsForClinicPref(PrefName prefName, string strDefNumFrom, string strDefNumTo)
    {
        var listClinicPrefs = GetPrefAllClinics(prefName);
        for (var i = 0; i < listClinicPrefs.Count; i++)
        {
            var listStrDefNums = GetPrefValue(prefName, listClinicPrefs[i].ClinicNum)
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            listStrDefNums = Defs.RemoveOrReplaceDefNum(listStrDefNums, strDefNumFrom, strDefNumTo);
            if (listStrDefNums == null) continue; //Nothing to update.
            var strDefNums = string.Join(",", listStrDefNums.Select(x => SOut.String(x)));
            Upsert(prefName, listClinicPrefs[i].ClinicNum, strDefNums);
        }
    }

    ///<summary>Returns 0 if there is no clinicpref entry for the specified pref.</summary>
    public static long GetLong(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref == null) return 0;
        var prefNum = SIn.Long(clinicPref.ValueString);
        return prefNum;
    }

    public static int GetInt(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref == null) return 0;
        var prefNum = SIn.Int(clinicPref.ValueString);
        return prefNum;
    }

    ///<summary>Gets the ValueString as a boolean for this clinic's pref or gets the actual preference if it does not exist.</summary>
    public static bool GetBool(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref == null) return PrefC.GetBool(prefName);
        return SIn.Bool(clinicPref.ValueString);
    }

    /// <summary>
    ///     Returns the bool for the specified pref. Explicitly checks if clinics are enabled. If they are, returns the
    ///     corresponding clinicpref. Else, returns the value from the preference cache.
    /// </summary>
    public static bool GetBoolHandleHasClinics(PrefName prefName, long clinicNum)
    {
        if (true) return GetBool(prefName, clinicNum);
        var retVal = PrefC.GetBool(prefName);
        return retVal;
    }

    /// <summary>
    ///     Returns false if no clinic entry found for this pref. Otherwise returns true and value of isSet can be
    ///     trusted.
    /// </summary>
    public static bool TryGetBool(PrefName prefName, long clinicNum, out bool isSet)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref == null)
        {
            isSet = false;
            return false;
        }

        isSet = SIn.Bool(clinicPref.ValueString);
        return true;
    }

    ///<summary>Inserts a pref of type long for the specified clinic.  Throws an exception if the preference already exists.</summary>
    public static void InsertPref(PrefName prefName, long clinicNum, string valueString)
    {
        if (GetFirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName) != null) throw new ApplicationException("The PrefName " + prefName + " already exists for ClinicNum: " + clinicNum);
        var clinicPrefToInsert = new ClinicPref();
        clinicPrefToInsert.PrefName = prefName;
        clinicPrefToInsert.ValueString = valueString;
        clinicPrefToInsert.ClinicNum = clinicNum;
        Insert(clinicPrefToInsert);
    }

    /// <summary>Inserts a new clinic pref or updates the existing one.</summary>
    /// <returns>True if an insert or update was made, false otherwise.</returns>
    public static bool Upsert(PrefName prefName, long clinicNum, string newValue)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref == null)
        {
            InsertPref(prefName, clinicNum, newValue);
            return true;
        }

        if (clinicPref.ValueString == newValue) return false;
        clinicPref.ValueString = newValue;
        Update(clinicPref);
        return true;
    }

    ///<summary>Deletes the prefs for this clinic. If any pref does not exist, then nothing will be done with that pref.</summary>
    public static long DeletePrefs(long clinicNum, List<PrefName> listPrefNames)
    {
        if (listPrefNames.IsNullOrEmpty()) return 0;
        var listClinicPrefs = new List<ClinicPref>();
        for (var i = 0; i < listPrefNames.Count; i++)
        {
            var clinicPref = GetPref(listPrefNames[i], clinicNum);
            if (clinicPref != null) listClinicPrefs.Add(clinicPref);
        }

        if (listClinicPrefs.Count == 0) return 0;
        var command = "DELETE FROM clinicpref WHERE ClinicPrefNum IN(" + string.Join(",", listClinicPrefs.Select(x => x.ClinicPrefNum)) + ")";
        return Db.NonQ(command);
    }

    /// <summary>
    ///     Returns true if ODTouch is allowed for this clinic, false otherwise.
    ///     Takes clinics feature on/off into account. Ok to call this when true==false.
    /// </summary>
    public static bool IsODTouchAllowed(long clinicNum)
    {
        //The office is on limited beta.
        var isAllowed = GetBoolHandleHasClinics(PrefName.IsODTouchEnabled, clinicNum);
        return isAllowed;
    }

    #region Cache Pattern

    private class ClinicPrefCache : CacheListAbs<ClinicPref>
    {
        protected override List<ClinicPref> GetCacheFromDb()
        {
            var command = "SELECT * FROM clinicpref";
            return ClinicPrefCrud.SelectMany(command);
        }

        protected override List<ClinicPref> TableToList(DataTable dataTable)
        {
            return ClinicPrefCrud.TableToList(dataTable);
        }

        protected override ClinicPref Copy(ClinicPref item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<ClinicPref> items)
        {
            return ClinicPrefCrud.ListToTable(items, "ClinicPref");
        }

        protected override void FillCacheIfNeeded()
        {
            ClinicPrefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ClinicPrefCache _clinicPrefCache = new();

    public static List<ClinicPref> GetWhere(Predicate<ClinicPref> match, bool isShort = false)
    {
        return _clinicPrefCache.GetWhere(match, isShort);
    }

    private static ClinicPref GetFirstOrDefault(Func<ClinicPref, bool> match, bool isShort = false)
    {
        return _clinicPrefCache.GetFirstOrDefault(match, isShort);
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
        _clinicPrefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _clinicPrefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _clinicPrefCache.ClearCache();
    }

    #endregion Cache Pattern
}