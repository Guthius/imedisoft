using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class QuickPasteCats
{
    public static void Insert(QuickPasteCat quickPasteCat)
    {
        QuickPasteCatCrud.Insert(quickPasteCat);
    }

    public static List<QuickPasteCat> GetCategoriesForType(EnumQuickPasteType type)
    {
        var quickPasteCats = GetWhere(x => x.ListDefaultForTypes.Contains(type));
        if (quickPasteCats.Count != 0)
        {
            return quickPasteCats;
        }

        var quickPasteCat = GetDeepCopy().FirstOrDefault();
        if (quickPasteCat is not null)
        {
            quickPasteCats.Add(quickPasteCat);
        }

        return quickPasteCats;
    }

    public static int GetDefaultType(EnumQuickPasteType quickPasteType)
    {
        if (GetCount() == 0)
        {
            return -1;
        }

        if (quickPasteType == EnumQuickPasteType.None)
        {
            return 0;
        }

        var quickPasteCats = GetDeepCopy();
        for (var i = 0; i < quickPasteCats.Count; i++)
        {
            var types = quickPasteCats[i].DefaultForTypes == "" ? [] : quickPasteCats[i].DefaultForTypes.Split(',');
            if (types.Any(type => ((int) quickPasteType).ToString() == type))
            {
                return i;
            }
        }

        return 0;
    }

    public static bool Sync(List<QuickPasteCat> listNew, List<QuickPasteCat> listOld)
    {
        return QuickPasteCatCrud.Sync(listNew.Select(x => x.Copy()).ToList(), listOld.Select(x => x.Copy()).ToList());
    }

    private class QuickPasteCatCache : CacheListAbs<QuickPasteCat>
    {
        protected override List<QuickPasteCat> GetCacheFromDb()
        {
            var command =
                "SELECT * from quickpastecat "
                + "ORDER BY ItemOrder";
            return QuickPasteCatCrud.SelectMany(command);
        }

        protected override List<QuickPasteCat> TableToList(DataTable dataTable)
        {
            return QuickPasteCatCrud.TableToList(dataTable);
        }

        protected override QuickPasteCat Copy(QuickPasteCat item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<QuickPasteCat> items)
        {
            return QuickPasteCatCrud.ListToTable(items, "QuickPasteCat");
        }

        protected override void FillCacheIfNeeded()
        {
            QuickPasteCats.GetTableFromCache(false);
        }
    }

    private static readonly QuickPasteCatCache Cache = new();

    public static List<QuickPasteCat> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static int GetCount(bool shortList = false)
    {
        return Cache.GetCount(shortList);
    }

    public static List<QuickPasteCat> GetWhere(Predicate<QuickPasteCat> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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