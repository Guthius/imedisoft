using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcApptColors
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
        var code1 = "";
        var code2 = "";
        var listProcApptColors = GetDeepCopy();
        for (var i = 0; i < listProcApptColors.Count; i++)
        {
            //using public property to trigger refresh if needed.
            if (listProcApptColors[i].CodeRange.Contains("-"))
            {
                var codeSplit = listProcApptColors[i].CodeRange.Split('-');
                code1 = codeSplit[0].Trim();
                code2 = codeSplit[1].Trim();
            }
            else
            {
                code1 = listProcApptColors[i].CodeRange.Trim();
                code2 = listProcApptColors[i].CodeRange.Trim();
            }

            if (procCode.CompareTo(code1) < 0 || procCode.CompareTo(code2) > 0) continue;
            return listProcApptColors[i];
        }

        return null;
    }

    private class ProcApptColorCache : CacheListAbs<ProcApptColor>
    {
        protected override List<ProcApptColor> GetCacheFromDb()
        {
            var command = "SELECT * FROM procapptcolor ORDER BY CodeRange";
            return ProcApptColorCrud.SelectMany(command);
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
            ProcApptColors.GetTableFromCache(false);
        }
    }

    private static readonly ProcApptColorCache Cache = new();

    public static List<ProcApptColor> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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