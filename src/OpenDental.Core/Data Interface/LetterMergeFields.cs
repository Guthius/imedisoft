using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class LetterMergeFields
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
        var command = "DELETE FROM lettermergefield "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMergeNum);
        Db.NonQ(command);
    }

    private class LetterMergeFieldCache : CacheListAbs<LetterMergeField>
    {
        protected override List<LetterMergeField> GetCacheFromDb()
        {
            var command = "SELECT * FROM lettermergefield ORDER BY FieldName";
            return LetterMergeFieldCrud.SelectMany(command);
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

    public static List<LetterMergeField> GetWhere(Predicate<LetterMergeField> match, bool isShort = false)
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