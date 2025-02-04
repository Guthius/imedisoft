using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class Pharmacies
{
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
        return GetFirstOrDefault(x => x.PharmacyNum == pharmacyNum)?.StoreName ?? string.Empty;
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

    public static List<Pharmacy> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static Pharmacy GetFirstOrDefault(Func<Pharmacy, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
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