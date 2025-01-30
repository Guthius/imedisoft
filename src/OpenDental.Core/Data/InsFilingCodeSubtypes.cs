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
        var command = "SELECT COUNT(*) FROM insplan WHERE FilingCodeSubtype=" + insFilingCodeSubtypeNum;
        if (DataCore.GetScalar(command) != "0")
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
        var command = "DELETE FROM insfilingcodesubtype WHERE InsFilingCodeNum=" + insFilingCodeNum;
        Db.NonQ(command);
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
            InsFilingCodeSubtypes.GetTableFromCache(false);
        }
    }

    private static readonly InsFilingCodeSubtypeCache Cache = new();

    public static List<InsFilingCodeSubtype> GetWhere(Predicate<InsFilingCodeSubtype> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
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