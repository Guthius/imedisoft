using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class Ebills
{
    public static Ebill GetForClinic(long clinicNum)
    {
        return GetFirstOrDefault(x => x.ClinicNum == clinicNum);
    }

    public static bool Sync(List<Ebill> listEbillsNew, List<Ebill> listEbillsOld)
    {
        return EbillCrud.Sync(listEbillsNew, listEbillsOld);
    }
    
    private class EbillCache : CacheListAbs<Ebill>
    {
        protected override List<Ebill> GetCacheFromDb()
        {
            return EbillCrud.SelectMany("SELECT * FROM ebill");
        }

        protected override List<Ebill> TableToList(DataTable dataTable)
        {
            return EbillCrud.TableToList(dataTable);
        }

        protected override Ebill Copy(Ebill item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Ebill> items)
        {
            return EbillCrud.ListToTable(items, "Ebill");
        }

        protected override void FillCacheIfNeeded()
        {
            Ebills.GetTableFromCache(false);
        }
    }

    private static readonly EbillCache Cache = new();

    public static List<Ebill> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Ebill GetFirstOrDefault(Func<Ebill, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
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