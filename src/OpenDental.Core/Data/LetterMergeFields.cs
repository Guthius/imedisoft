using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class LetterMergeFields
{
    public static void Insert(LetterMergeField letterMergeField)
    {
        LetterMergeFieldCrud.Insert(letterMergeField);
    }

    public static List<string> GetForLetter(long letterMergeNum)
    {
        return GetWhere(x => x.LetterMergeNum == letterMergeNum).Select(x => x.FieldName).ToList();
    }

    public static void DeleteForLetter(long letterMergeNum)
    {
        Db.NonQ("DELETE FROM lettermergefield WHERE LetterMergeNum = " + letterMergeNum);
    }

    private class LetterMergeFieldCache : CacheListAbs<LetterMergeField>
    {
        protected override List<LetterMergeField> GetCacheFromDb()
        {
            return LetterMergeFieldCrud.SelectMany("SELECT * FROM lettermergefield ORDER BY FieldName");
        }

        protected override List<LetterMergeField> TableToList(DataTable dataTable)
        {
            return LetterMergeFieldCrud.TableToList(dataTable);
        }

        protected override LetterMergeField Copy(LetterMergeField item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<LetterMergeField> items)
        {
            return LetterMergeFieldCrud.ListToTable(items, "LetterMergeField");
        }

        protected override void FillCacheIfNeeded()
        {
            LetterMergeFields.GetTableFromCache(false);
        }
    }

    private static readonly LetterMergeFieldCache Cache = new();

    public static List<LetterMergeField> GetWhere(Predicate<LetterMergeField> predicate, bool shortList = false)
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