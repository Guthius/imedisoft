using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ClinicPrefs
{
    public static void Insert(ClinicPref clinicPref)
    {
        ClinicPrefCrud.Insert(clinicPref);
    }

    public static void Update(ClinicPref clinicPref)
    {
        ClinicPrefCrud.Update(clinicPref);
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
        var clinicPrefs = new List<ClinicPref>();

        if (includeDefault)
        {
            clinicPrefs.Add(new ClinicPref
            {
                ClinicNum = 0,
                PrefName = prefName,
                ValueString = prefName.GetValueAsText()
            });
        }

        clinicPrefs.AddRange(GetWhere(x => x.PrefName == prefName));

        return clinicPrefs;
    }

    public static ClinicPref GetPref(PrefName prefName, long clinicNum, bool isDefaultIncluded = false)
    {
        return GetPrefAllClinics(prefName, isDefaultIncluded).Find(x => x.ClinicNum == clinicNum);
    }

    public static string GetPrefValue(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPrefAllClinics(prefName).Find(x => x.ClinicNum == clinicNum);

        return clinicPref is null ? PrefC.GetString(prefName) : clinicPref.ValueString;
    }

    public static void UpdateDefNumsForClinicPref(PrefName prefName, string strDefNumFrom, string strDefNumTo)
    {
        var clinicPrefs = GetPrefAllClinics(prefName);

        foreach (var clinicPref in clinicPrefs)
        {
            var defNumStrs = GetPrefValue(prefName, clinicPref.ClinicNum)
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            defNumStrs = Defs.RemoveOrReplaceDefNum(defNumStrs, strDefNumFrom, strDefNumTo);
            if (defNumStrs is null)
            {
                continue;
            }

            var defNums = string.Join(",", defNumStrs.Select(SOut.String));

            Upsert(prefName, clinicPref.ClinicNum, defNums);
        }
    }

    public static long GetLong(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);

        return clinicPref is null ? 0 : SIn.Long(clinicPref.ValueString);
    }

    public static int GetInt(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);

        return clinicPref is null ? 0 : SIn.Int(clinicPref.ValueString);
    }

    public static bool GetBool(PrefName prefName, long clinicNum)
    {
        var clinicPref = GetPref(prefName, clinicNum);

        return clinicPref is null ? PrefC.GetBool(prefName) : SIn.Bool(clinicPref.ValueString);
    }

    public static bool GetBoolHandleHasClinics(PrefName prefName, long clinicNum)
    {
        return GetBool(prefName, clinicNum);
    }

    public static void InsertPref(PrefName prefName, long clinicNum, string valueString)
    {
        if (GetFirstOrDefault(x => x.ClinicNum == clinicNum && x.PrefName == prefName) != null)
        {
            throw new ApplicationException("The PrefName " + prefName + " already exists for ClinicNum: " + clinicNum);
        }

        Insert(new ClinicPref
        {
            PrefName = prefName,
            ValueString = valueString,
            ClinicNum = clinicNum
        });
    }

    public static bool Upsert(PrefName prefName, long clinicNum, string newValue)
    {
        var clinicPref = GetPref(prefName, clinicNum);
        if (clinicPref is null)
        {
            InsertPref(prefName, clinicNum, newValue);
            return true;
        }

        if (clinicPref.ValueString == newValue)
        {
            return false;
        }

        clinicPref.ValueString = newValue;

        Update(clinicPref);

        return true;
    }

    public static void DeletePrefs(long clinicNum, List<PrefName> prefNames)
    {
        if (prefNames.IsNullOrEmpty())
        {
            return;
        }

        var clinicPrefs = new List<ClinicPref>();

        foreach (var prefName in prefNames)
        {
            var clinicPref = GetPref(prefName, clinicNum);
            if (clinicPref != null)
            {
                clinicPrefs.Add(clinicPref);
            }
        }

        if (clinicPrefs.Count == 0)
        {
            return;
        }

        Db.NonQ("DELETE FROM clinicpref WHERE ClinicPrefNum IN(" + string.Join(",", clinicPrefs.Select(x => x.ClinicPrefNum)) + ")");
    }

    private class ClinicPrefCache : CacheListAbs<ClinicPref>
    {
        protected override List<ClinicPref> GetCacheFromDb()
        {
            return ClinicPrefCrud.SelectMany("SELECT * FROM clinicpref");
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
            GetTableFromCache(false);
        }
    }

    private static readonly ClinicPrefCache Cache = new();

    public static List<ClinicPref> GetWhere(Predicate<ClinicPref> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    private static ClinicPref GetFirstOrDefault(Func<ClinicPref, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}