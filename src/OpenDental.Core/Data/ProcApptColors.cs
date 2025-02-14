using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ProcApptColors
{
    public static void Insert(ProcApptColor procApptColor)
    {
        ProcApptColorCrud.Insert(procApptColor);
    }

    public static void Update(ProcApptColor procApptColor)
    {
        ProcApptColorCrud.Update(procApptColor);
    }

    public static void Delete(long procApptColorNum)
    {
        Db.NonQ("DELETE FROM procapptcolor WHERE ProcApptColorNum = " + procApptColorNum);
    }

    public static ProcApptColor GetMatch(string procCode)
    {
        var procApptColors = GetDeepCopy();

        foreach (var procApptColor in procApptColors)
        {
            string code1;
            string code2;

            if (procApptColor.CodeRange.Contains("-"))
            {
                var codeSplit = procApptColor.CodeRange.Split('-');

                code1 = codeSplit[0].Trim();
                code2 = codeSplit[1].Trim();
            }
            else
            {
                code1 = procApptColor.CodeRange.Trim();
                code2 = procApptColor.CodeRange.Trim();
            }

            if (string.Compare(procCode, code1, StringComparison.Ordinal) < 0 ||
                string.Compare(procCode, code2, StringComparison.Ordinal) > 0)
            {
                continue;
            }

            return procApptColor;
        }

        return null;
    }

    private class ProcApptColorCache : CacheListAbs<ProcApptColor>
    {
        protected override List<ProcApptColor> GetCacheFromDb()
        {
            return ProcApptColorCrud.SelectMany("SELECT * FROM procapptcolor ORDER BY CodeRange");
        }

        protected override List<ProcApptColor> TableToList(DataTable dataTable)
        {
            return ProcApptColorCrud.TableToList(dataTable);
        }

        protected override ProcApptColor Copy(ProcApptColor item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProcApptColor> items)
        {
            return ProcApptColorCrud.ListToTable(items, "ProcApptColor");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly ProcApptColorCache Cache = new();

    public static List<ProcApptColor> GetDeepCopy(bool shortList = false)
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