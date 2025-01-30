using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class FieldDefLinks
{
    public static void Sync(List<FieldDefLink> listFieldDefLinksNew)
    {
        var fieldDefLinksOld = FieldDefLinkCrud.SelectMany("SELECT * FROM fielddeflink");

        FieldDefLinkCrud.Sync(listFieldDefLinksNew, fieldDefLinksOld);
    }

    public static void DeleteForFieldDefNum(long fieldDefNum, FieldDefTypes fieldDefTypes)
    {
        if (fieldDefNum == 0)
        {
            return;
        }

        Db.NonQ("DELETE FROM fielddeflink WHERE FieldDefNum=" + fieldDefNum + " AND FieldDefType=" + (int) fieldDefTypes);
    }

    private class FieldDefLinkCache : CacheListAbs<FieldDefLink>
    {
        protected override List<FieldDefLink> GetCacheFromDb()
        {
            return FieldDefLinkCrud.SelectMany("SELECT * FROM fielddeflink");
        }

        protected override List<FieldDefLink> TableToList(DataTable dataTable)
        {
            return FieldDefLinkCrud.TableToList(dataTable);
        }

        protected override FieldDefLink Copy(FieldDefLink item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<FieldDefLink> items)
        {
            return FieldDefLinkCrud.ListToTable(items, "FieldDefLink");
        }

        protected override void FillCacheIfNeeded()
        {
            FieldDefLinks.GetTableFromCache(false);
        }
    }

    private static readonly FieldDefLinkCache Cache = new();

    public static bool GetExists(Predicate<FieldDefLink> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static List<FieldDefLink> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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