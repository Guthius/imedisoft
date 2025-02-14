using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public class ZipCodes
{
    public static void Insert(ZipCode zipCode)
    {
        ZipCodeCrud.Insert(zipCode);
    }

    public static void Update(ZipCode zipCode)
    {
        ZipCodeCrud.Update(zipCode);
    }

    public static void Delete(ZipCode zipCode)
    {
        Db.NonQ("DELETE from zipcode WHERE zipcodenum = " + zipCode.ZipCodeNum);
    }

    public static List<ZipCode> GetAlMatches(string zipCodeDigits)
    {
        return GetWhere(x => x.ZipCodeDigits == zipCodeDigits);
    }

    private class ZipCodeCache : CacheListAbs<ZipCode>
    {
        protected override List<ZipCode> GetCacheFromDb()
        {
            return ZipCodeCrud.SelectMany("SELECT * from zipcode ORDER BY ZipCodeDigits");
        }

        protected override List<ZipCode> TableToList(DataTable dataTable)
        {
            return ZipCodeCrud.TableToList(dataTable);
        }

        protected override ZipCode Copy(ZipCode item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ZipCode> items)
        {
            return ZipCodeCrud.ListToTable(items, "ZipCode");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(ZipCode item)
        {
            return item.IsFrequent;
        }
    }

    private static readonly ZipCodeCache Cache = new();

    public static List<ZipCode> GetWhere(Predicate<ZipCode> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static List<ZipCode> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
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