using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class LetterMerges
{
    public static void Insert(LetterMerge letterMerge)
    {
        LetterMergeCrud.Insert(letterMerge);
    }

    public static void Update(LetterMerge letterMerge)
    {
        LetterMergeCrud.Update(letterMerge);
    }

    public static void Delete(LetterMerge letterMerge)
    {
        Db.NonQ("DELETE FROM lettermerge WHERE LetterMergeNum = " + letterMerge.LetterMergeNum);
    }

    public static List<LetterMerge> GetListForCat(int catIndex)
    {
        var defNum = Defs.GetDefsForCategory(DefCat.LetterMergeCats, true)[catIndex].DefNum;

        return GetWhere(x => x.Category == defNum);
    }

    private class LetterMergeCache : CacheListAbs<LetterMerge>
    {
        protected override List<LetterMerge> GetCacheFromDb()
        {
            return LetterMergeCrud.SelectMany("SELECT * FROM lettermerge ORDER BY Description");
        }

        protected override List<LetterMerge> TableToList(DataTable dataTable)
        {
            return LetterMergeCrud.TableToList(dataTable);
        }

        protected override LetterMerge Copy(LetterMerge item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<LetterMerge> items)
        {
            return LetterMergeCrud.ListToTable(items, "LetterMerge");
        }

        protected override void FillCacheIfNeeded()
        {
            LetterMerges.GetTableFromCache(false);
        }
    }

    private static readonly LetterMergeCache Cache = new();

    public static List<LetterMerge> GetWhere(Predicate<LetterMerge> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
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