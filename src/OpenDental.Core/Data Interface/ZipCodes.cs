using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

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

    public static List<ZipCode> GetALMatches(string zipCodeDigits)
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
            ZipCodes.GetTableFromCache(false);
        }

        protected override bool IsInListShort(ZipCode item)
        {
            return item.IsFrequent;
        }
    }

    private static readonly ZipCodeCache Cache = new();

    public static List<ZipCode> GetWhere(Predicate<ZipCode> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static List<ZipCode> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static void FillCacheFromTable(DataTable dataTable)
    {
        Cache.FillCacheFromTable(dataTable);
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