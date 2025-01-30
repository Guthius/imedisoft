using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ClinicPrefs
{
    public static void Insert(ClinicPref clinicPref)
    {
        ClinicPrefCrud.Insert(clinicPref);
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

    public static bool Sync(List<ClinicPref> listClinicPrefsNew, List<ClinicPref> listClinicPrefOld)
    {
        return ClinicPrefCrud.Sync(listClinicPrefsNew, listClinicPrefOld);
    }

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

    public static string GetPrefValue(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPrefAllClinics(prefName).Find(x => x.ClinicNum == clinicNum);
        if (clinicPref == null) return PrefC.GetString(prefName);
        return clinicPref.ValueString;
    }

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

    public static bool GetBool(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref == null) return PrefC.GetBool(prefName);
        return SIn.Bool(clinicPref.ValueString);
    }

    public static bool GetBoolHandleHasClinics(PrefName prefName, long clinicNum)
    {
        if (true) return GetBool(prefName, clinicNum);
        var retVal = PrefC.GetBool(prefName);
        return retVal;
    }

    public static void InsertPref(PrefName prefName, long clinicNum, string valueString)
    {
        if (GetFirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName) != null) throw new ApplicationException("The PrefName " + prefName + " already exists for ClinicNum: " + clinicNum);
        var clinicPrefToInsert = new ClinicPref();
        clinicPrefToInsert.PrefName = prefName;
        clinicPrefToInsert.ValueString = valueString;
        clinicPrefToInsert.ClinicNum = clinicNum;
        Insert(clinicPrefToInsert);
    }

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

    public static bool IsOdTouchAllowed(long clinicNum)
    {
        return GetBoolHandleHasClinics(PrefName.IsODTouchEnabled, clinicNum);
    }

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

    private static readonly ClinicPrefCache Cache = new();

    public static List<ClinicPref> GetWhere(Predicate<ClinicPref> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    private static ClinicPref GetFirstOrDefault(Func<ClinicPref, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}