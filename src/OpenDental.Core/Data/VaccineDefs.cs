using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class VaccineDefs
{
    public static VaccineDef GetOne(long vaccineDefNum)
    {
        return VaccineDefCrud.SelectOne(vaccineDefNum);
    }

    public static void Insert(VaccineDef vaccineDef)
    {
        VaccineDefCrud.Insert(vaccineDef);
    }

    public static void Update(VaccineDef vaccineDef)
    {
        VaccineDefCrud.Update(vaccineDef);
    }

    public static void Delete(long vaccineDefNum)
    {
        if (Db.GetCount("SELECT COUNT(*) FROM VaccinePat WHERE VaccineDefNum=" + vaccineDefNum) != "0")
        {
            throw new ApplicationException("Cannot delete: VaccineDef is in use by VaccinePat.");
        }

        Db.NonQ("DELETE FROM vaccinedef WHERE VaccineDefNum = " + vaccineDefNum);
    }

    private class VaccineDefCache : CacheListAbs<VaccineDef>
    {
        protected override List<VaccineDef> GetCacheFromDb()
        {
            return VaccineDefCrud.SelectMany("SELECT * FROM vaccinedef ORDER BY CVXCode");
        }

        protected override List<VaccineDef> TableToList(DataTable dataTable)
        {
            return VaccineDefCrud.TableToList(dataTable);
        }

        protected override VaccineDef Copy(VaccineDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<VaccineDef> items)
        {
            return VaccineDefCrud.ListToTable(items, "VaccineDef");
        }

        protected override void FillCacheIfNeeded()
        {
            VaccineDefs.GetTableFromCache(false);
        }
    }

    private static readonly VaccineDefCache Cache = new();

    public static bool GetExists(Predicate<VaccineDef> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static List<VaccineDef> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}