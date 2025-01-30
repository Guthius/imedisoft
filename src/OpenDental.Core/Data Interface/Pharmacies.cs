using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Pharmacies
{
    public static Pharmacy GetOne(long pharmacyNum)
    {
        return PharmacyCrud.SelectOne(pharmacyNum);
    }

    public static void Insert(Pharmacy pharmacy)
    {
        PharmacyCrud.Insert(pharmacy);
    }

    public static void Update(Pharmacy pharmacy)
    {
        PharmacyCrud.Update(pharmacy);
    }

    public static void DeleteObject(long pharmacyNum)
    {
        PharmacyCrud.Delete(pharmacyNum);
    }

    public static string GetDescription(long pharmacyNum)
    {
        var pharmacy = GetFirstOrDefault(x => x.PharmacyNum == pharmacyNum);
        return pharmacy == null ? "" : pharmacy.StoreName;
    }

    private class PharmacyCache : CacheListAbs<Pharmacy>
    {
        protected override List<Pharmacy> GetCacheFromDb()
        {
            return PharmacyCrud.SelectMany("SELECT * FROM pharmacy ORDER BY StoreName");
        }

        protected override List<Pharmacy> TableToList(DataTable dataTable)
        {
            return PharmacyCrud.TableToList(dataTable);
        }

        protected override Pharmacy Copy(Pharmacy item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Pharmacy> items)
        {
            return PharmacyCrud.ListToTable(items, "Pharmacy");
        }

        protected override void FillCacheIfNeeded()
        {
            Pharmacies.GetTableFromCache(false);
        }
    }

    private static readonly PharmacyCache Cache = new();

    public static List<Pharmacy> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Pharmacy GetFirstOrDefault(Func<Pharmacy, bool> match, bool isShort = false)
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