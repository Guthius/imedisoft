using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class InsFilingCodeSubtypes
{
    public static void Insert(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        InsFilingCodeSubtypeCrud.Insert(insFilingCodeSubtype);
    }

    public static void Update(InsFilingCodeSubtype insFilingCodeSubtype)
    {
        InsFilingCodeSubtypeCrud.Update(insFilingCodeSubtype);
    }

    public static void Delete(long insFilingCodeSubtypeNum)
    {
        var commandText = "SELECT COUNT(*) FROM insplan WHERE FilingCodeSubtype = " + insFilingCodeSubtypeNum;

        if (DataCore.GetScalar(commandText) != "0")
        {
            throw new ApplicationException("Already in use by insplans.");
        }

        InsFilingCodeSubtypeCrud.Delete(insFilingCodeSubtypeNum);
    }

    public static List<InsFilingCodeSubtype> GetForInsFilingCode(long insFilingCodeNum)
    {
        return GetWhere(x => x.InsFilingCodeNum == insFilingCodeNum);
    }

    public static void DeleteForInsFilingCode(long insFilingCodeNum)
    {
        Db.NonQ("DELETE FROM insfilingcodesubtype WHERE InsFilingCodeNum = " + insFilingCodeNum);
    }

    private class InsFilingCodeSubtypeCache : CacheListAbs<InsFilingCodeSubtype>
    {
        protected override List<InsFilingCodeSubtype> GetCacheFromDb()
        {
            return InsFilingCodeSubtypeCrud.SelectMany("SELECT * FROM insfilingcodesubtype ORDER BY Descript");
        }

        protected override List<InsFilingCodeSubtype> TableToList(DataTable dataTable)
        {
            return InsFilingCodeSubtypeCrud.TableToList(dataTable);
        }

        protected override InsFilingCodeSubtype Copy(InsFilingCodeSubtype item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<InsFilingCodeSubtype> items)
        {
            return InsFilingCodeSubtypeCrud.ListToTable(items, "InsFilingCodeSubtype");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly InsFilingCodeSubtypeCache Cache = new();

    public static List<InsFilingCodeSubtype> GetWhere(Predicate<InsFilingCodeSubtype> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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