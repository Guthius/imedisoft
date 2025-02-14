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
            GetTableFromCache(false);
        }
    }

    private static readonly StateAbbrCache Cache = new();

    public static List<StateAbbr> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<StateAbbr> GetWhere(Predicate<StateAbbr> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static StateAbbr GetFirstOrDefault(Func<StateAbbr, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
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