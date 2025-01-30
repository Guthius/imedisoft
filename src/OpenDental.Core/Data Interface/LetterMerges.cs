using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class LetterMerges
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
        var command = "DELETE FROM lettermerge "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMerge.LetterMergeNum);
        Db.NonQ(command);
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
            var command = "SELECT * FROM lettermerge ORDER BY Description";
            var listLetterMerges = LetterMergeCrud.SelectMany(command);
            return listLetterMerges;
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

    public static List<LetterMerge> GetWhere(Predicate<LetterMerge> match, bool isShort = false)
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