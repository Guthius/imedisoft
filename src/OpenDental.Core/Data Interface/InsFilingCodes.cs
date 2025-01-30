using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class InsFilingCodes
{
    public static string GetEclaimCode(long insFilingCodeNum)
    {
        var insFilingCode = GetFirstOrDefault(x => x.InsFilingCodeNum == insFilingCodeNum);
        return insFilingCode == null ? "CI" : insFilingCode.EclaimCode;
    }

    public static InsFilingCode GetOrInsertForEclaimCode(string descript, string eclaimCode)
    {
        var itemOrderMax = 0;
        var listInsFilingCodes = GetDeepCopy();
        for (var i = 0; i < listInsFilingCodes.Count; i++)
        {
            if (listInsFilingCodes[i].ItemOrder > itemOrderMax) itemOrderMax = listInsFilingCodes[i].ItemOrder;
            if (listInsFilingCodes[i].EclaimCode != eclaimCode) continue;
            return listInsFilingCodes[i];
        }

        var insFilingCode = new InsFilingCode();
        insFilingCode.Descript = descript;
        insFilingCode.EclaimCode = eclaimCode;
        insFilingCode.ItemOrder = itemOrderMax + 1;
        Insert(insFilingCode);
        return insFilingCode;
    }

    public static List<InsFilingCode> GetAll()
    {
        var command = "SELECT * FROM insfilingcode ORDER BY ItemOrder";
        return InsFilingCodeCrud.SelectMany(command);
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
        var command = "SELECT COUNT(*) FROM insplan WHERE FilingCode=" + SOut.Long(insFilingCodeNum);
        if (DataCore.GetScalar(command) != "0") throw new ApplicationException(Lans.g("InsFilingCode", "Already in use by insplans."));
        InsFilingCodeCrud.Delete(insFilingCodeNum);
    }
    
    private class InsFilingCodeCache : CacheListAbs<InsFilingCode>
    {
        protected override List<InsFilingCode> GetCacheFromDb()
        {
            var command = "SELECT * FROM insfilingcode ORDER BY ItemOrder";
            return InsFilingCodeCrud.SelectMany(command);
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
            InsFilingCodes.GetTableFromCache(false);
        }
    }

    private static readonly InsFilingCodeCache Cache = new();

    public static List<InsFilingCode> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static InsFilingCode GetFirstOrDefault(Func<InsFilingCode, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static InsFilingCode GetOne(long insFilingCodeNum)
    {
        return Cache.GetFirstOrDefault(x => x.InsFilingCodeNum == insFilingCodeNum);
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