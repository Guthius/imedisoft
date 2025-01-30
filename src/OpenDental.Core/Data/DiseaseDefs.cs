using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class DiseaseDefs
{
    public static bool FixItemOrders()
    {
        var changes = false;

        var diseaseDefs = GetDeepCopy();

        diseaseDefs.Sort(SortItemOrder);

        for (var i = 0; i < diseaseDefs.Count; i++)
        {
            if (diseaseDefs[i].ItemOrder == i)
            {
                continue;
            }

            diseaseDefs[i].ItemOrder = i;

            Update(diseaseDefs[i]);

            changes = true;
        }

        if (changes)
        {
            RefreshCache();
        }

        return changes;
    }

    public static void Update(DiseaseDef diseaseDef)
    {
        DiseaseDefCrud.Update(diseaseDef);
    }

    public static long Insert(DiseaseDef diseaseDef)
    {
        return DiseaseDefCrud.Insert(diseaseDef);
    }

    public static bool IsDiseaseDefInUse(long diseaseDefNum)
    {
        if (diseaseDefNum == 0)
        {
            return false;
        }

        if (diseaseDefNum == PrefC.GetLong(PrefName.ProblemsIndicateNone))
        {
            return true;
        }

        return DataCore.GetScalar("SELECT EXISTS (SELECT * FROM disease WHERE DiseaseDefNum=" + diseaseDefNum + ")") == "1" ||
               DataCore.GetScalar("SELECT EXISTS (SELECT * FROM eduresource WHERE DiseaseDefNum=" + diseaseDefNum + ")") == "1" ||
               DataCore.GetScalar("SELECT EXISTS (SELECT * FROM familyhealth WHERE DiseaseDefNum=" + diseaseDefNum + ")") == "1";
    }

    public static string GetName(long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.DiseaseDefNum == diseaseDefNum);

        return diseaseDef == null ? "" : diseaseDef.DiseaseName;
    }

    public static string GetNameByCode(string snomedOrIcd9Code)
    {
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == snomedOrIcd9Code);
        if (diseaseDef != null)
        {
            return diseaseDef.DiseaseName;
        }

        diseaseDef = GetFirstOrDefault(x => x.ICD9Code == snomedOrIcd9Code);

        return diseaseDef != null ? diseaseDef.DiseaseName : "";
    }

    public static long GetNumFromCode(string codeValue)
    {
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == codeValue);
        if (diseaseDef != null)
        {
            return diseaseDef.DiseaseDefNum;
        }

        diseaseDef = GetFirstOrDefault(x => x.ICD9Code == codeValue);
        if (diseaseDef != null)
        {
            return diseaseDef.DiseaseDefNum;
        }

        diseaseDef = GetFirstOrDefault(x => x.Icd10Code == codeValue);

        return diseaseDef?.DiseaseDefNum ?? 0;
    }

    public static long GetNumFromSnomed(string snomedCode)
    {
        if (snomedCode == "")
        {
            return 0;
        }

        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == snomedCode);

        return diseaseDef?.DiseaseDefNum ?? 0;
    }

    public static DiseaseDef GetItem(long diseaseDefNum)
    {
        return GetFirstOrDefault(x => x.DiseaseDefNum == diseaseDefNum);
    }

    public static long GetNumFromName(string diseaseName, bool matchHidden = false)
    {
        var diseaseDef = Cache.GetFirstOrDefault(x => x.DiseaseName == diseaseName, !matchHidden);

        return diseaseDef?.DiseaseDefNum ?? 0;
    }

    public static List<DiseaseDef> GetMultDiseaseDefs(List<long> diseaseDefNums)
    {
        var dataTable = diseaseDefNums.Count > 0
            ? DataCore.GetTable("SELECT * FROM diseasedef WHERE DiseaseDefNum IN (" + string.Join(", ", diseaseDefNums) + ")")
            : new DataTable();

        return DiseaseDefCrud.TableToList(dataTable);
    }

    public static bool ContainsSnomed(string snomedCode, long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.SnomedCode == snomedCode && x.DiseaseDefNum != diseaseDefNum);
        return diseaseDef != null;
    }

    public static bool ContainsIcd9(string icd9Code, long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.ICD9Code == icd9Code && x.DiseaseDefNum != diseaseDefNum);

        return diseaseDef != null;
    }

    public static bool ContainsIcd10(string icd10Code, long diseaseDefNum)
    {
        var diseaseDef = GetFirstOrDefault(x => x.Icd10Code == icd10Code && x.DiseaseDefNum != diseaseDefNum);

        return diseaseDef != null;
    }

    public static void Sync(List<DiseaseDef> listDiseaseDefs, List<DiseaseDef> listDiseaseDefsOld)
    {
        DiseaseDefCrud.Sync(listDiseaseDefs, listDiseaseDefsOld);
    }

    public static int SortAlphabetically(DiseaseDef diseaseDef, DiseaseDef diseaseDefOther)
    {
        return diseaseDef.DiseaseName != diseaseDefOther.DiseaseName ? string.Compare(diseaseDef.DiseaseName, diseaseDefOther.DiseaseName, StringComparison.Ordinal) : diseaseDef.DiseaseDefNum.CompareTo(diseaseDefOther.DiseaseDefNum);
    }

    public static int SortItemOrder(DiseaseDef diseaseDef, DiseaseDef diseaseDefOther)
    {
        return diseaseDef.ItemOrder != diseaseDefOther.ItemOrder ? diseaseDef.ItemOrder.CompareTo(diseaseDefOther.ItemOrder) : diseaseDef.DiseaseDefNum.CompareTo(diseaseDefOther.DiseaseDefNum);
    }

    public static long GetDefNumForDefaultPreg(string pregnancyCode)
    {
        var diseaseDefNum = GetNumFromCode(pregnancyCode);
        if (diseaseDefNum != 0)
        {
            return diseaseDefNum;
        }

        var diseaseDef = new DiseaseDef
        {
            DiseaseName = "Pregnant"
        };

        var pregnancyCodeSys = PrefC.GetString(PrefName.PregnancyDefaultCodeSystem);
        switch (pregnancyCodeSys)
        {
            case "ICD9CM":
                diseaseDef.ICD9Code = pregnancyCode;
                break;

            case "ICD10CM":
                diseaseDef.Icd10Code = pregnancyCode;
                break;

            case "SNOMEDCT":
                diseaseDef.SnomedCode = pregnancyCode;
                break;
        }

        diseaseDefNum = Insert(diseaseDef);

        RefreshCache();

        Signalods.SetInvalid(InvalidType.Diseases);

        SecurityLogs.MakeLogEntry(EnumPermType.ProblemDefEdit, 0, diseaseDef.DiseaseName + " added.");

        return diseaseDefNum;
    }

    private class DiseaseDefCache : CacheListAbs<DiseaseDef>
    {
        protected override List<DiseaseDef> GetCacheFromDb()
        {
            return DiseaseDefCrud.SelectMany("SELECT * FROM diseasedef ORDER BY ItemOrder");
        }

        protected override List<DiseaseDef> TableToList(DataTable dataTable)
        {
            return DiseaseDefCrud.TableToList(dataTable);
        }

        protected override DiseaseDef Copy(DiseaseDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DiseaseDef> items)
        {
            return DiseaseDefCrud.ListToTable(items, "DiseaseDef");
        }

        protected override void FillCacheIfNeeded()
        {
            DiseaseDefs.GetTableFromCache(false);
        }

        protected override bool IsInListShort(DiseaseDef item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly DiseaseDefCache Cache = new();

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
    }

    public static List<DiseaseDef> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<DiseaseDef> GetWhere(Predicate<DiseaseDef> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static DiseaseDef GetFirstOrDefault(Func<DiseaseDef, bool> match, bool isShort = false)
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