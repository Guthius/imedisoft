using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class StateAbbrs
{
    public static void Insert(StateAbbr stateAbbr)
    {
        StateAbbrCrud.Insert(stateAbbr);
    }

    public static void Update(StateAbbr stateAbbr)
    {
        StateAbbrCrud.Update(stateAbbr);
    }

    public static void Delete(long stateAbbrNum)
    {
        StateAbbrCrud.Delete(stateAbbrNum);
    }

    public static List<StateAbbr> GetSimilarAbbrs(string abbr)
    {
        return GetWhere(x => x.Abbr.StartsWith(abbr, StringComparison.CurrentCultureIgnoreCase));
    }

    public static int GetMedicaidIdLength(string abbr)
    {
        var stateAbbr = GetFirstOrDefault(x => string.Equals(x.Abbr, abbr, StringComparison.CurrentCultureIgnoreCase));

        return stateAbbr?.MedicaidIDLength ?? 0;
    }

    public static bool IsValidAbbr(string abbr)
    {
        return GetFirstOrDefault(x => string.Equals(x.Abbr, abbr, StringComparison.CurrentCultureIgnoreCase)) is not null;
    }

    private class StateAbbrCache : CacheListAbs<StateAbbr>
    {
        protected override List<StateAbbr> GetCacheFromDb()
        {
            return StateAbbrCrud.SelectMany("SELECT * FROM stateabbr ORDER BY Abbr");
        }

        protected override List<StateAbbr> TableToList(DataTable dataTable)
        {
            return StateAbbrCrud.TableToList(dataTable);
        }

        protected override StateAbbr Copy(StateAbbr item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<StateAbbr> items)
        {
            return StateAbbrCrud.ListToTable(items, "StateAbbr");
        }

        protected override void FillCacheIfNeeded()
        {
            StateAbbrs.GetTableFromCache(false);
        }
    }

    private static readonly StateAbbrCache Cache = new();

    public static List<StateAbbr> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<StateAbbr> GetWhere(Predicate<StateAbbr> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static StateAbbr GetFirstOrDefault(Func<StateAbbr, bool> match, bool isShort = false)
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