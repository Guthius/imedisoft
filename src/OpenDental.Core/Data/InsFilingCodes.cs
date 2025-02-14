using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class InsFilingCodes
{
    public static string GetEclaimCode(long insFilingCodeNum)
    {
        var insFilingCode = GetFirstOrDefault(x => x.InsFilingCodeNum == insFilingCodeNum);

        return insFilingCode == null ? "CI" : insFilingCode.EclaimCode;
    }

    public static InsFilingCode GetOrInsertForEclaimCode(string descript, string eclaimCode)
    {
        var itemOrderMax = 0;

        var insFilingCodes = GetDeepCopy();

        foreach (var code in insFilingCodes)
        {
            if (code.ItemOrder > itemOrderMax)
            {
                itemOrderMax = code.ItemOrder;
            }

            if (code.EclaimCode != eclaimCode)
            {
                continue;
            }

            return code;
        }

        var insFilingCode = new InsFilingCode
        {
            Descript = descript,
            EclaimCode = eclaimCode,
            ItemOrder = itemOrderMax + 1
        };

        Insert(insFilingCode);

        return insFilingCode;
    }

    public static List<InsFilingCode> GetAll()
    {
        return InsFilingCodeCrud.SelectMany("SELECT * FROM insfilingcode ORDER BY ItemOrder");
    }

    public static long Insert(InsFilingCode insFilingCode)
    {
        return InsFilingCodeCrud.Insert(insFilingCode);
    }

    public static void Update(InsFilingCode insFilingCode)
    {
        InsFilingCodeCrud.Update(insFilingCode);
    }

    public static void Delete(long insFilingCodeNum)
    {
        if (DataCore.GetScalar("SELECT COUNT(*) FROM insplan WHERE FilingCode = " + insFilingCodeNum) != "0")
        {
            throw new ApplicationException("Already in use by insplans.");
        }

        InsFilingCodeCrud.Delete(insFilingCodeNum);
    }

    private class InsFilingCodeCache : CacheListAbs<InsFilingCode>
    {
        protected override List<InsFilingCode> GetCacheFromDb()
        {
            return InsFilingCodeCrud.SelectMany("SELECT * FROM insfilingcode ORDER BY ItemOrder");
        }

        protected override List<InsFilingCode> TableToList(DataTable dataTable)
        {
            return InsFilingCodeCrud.TableToList(dataTable);
        }

        protected override InsFilingCode Copy(InsFilingCode item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<InsFilingCode> items)
        {
            return InsFilingCodeCrud.ListToTable(items, "InsFilingCode");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly InsFilingCodeCache Cache = new();

    public static List<InsFilingCode> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static InsFilingCode GetFirstOrDefault(Func<InsFilingCode, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static InsFilingCode GetOne(long insFilingCodeNum)
    {
        return Cache.GetFirstOrDefault(x => x.InsFilingCodeNum == insFilingCodeNum);
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