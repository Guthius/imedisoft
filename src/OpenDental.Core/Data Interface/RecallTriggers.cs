using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class RecallTriggers
{
    public static void Insert(RecallTrigger trigger)
    {
        RecallTriggerCrud.Insert(trigger);
    }

    public static List<RecallTrigger> GetForType(long recallTypeNum)
    {
        return GetWhere(x => x.RecallTypeNum == recallTypeNum);
    }

    public static void SetForType(long recallTypeNum, List<RecallTrigger> triggers)
    {
        Db.NonQ("DELETE FROM recalltrigger WHERE RecallTypeNum=" + recallTypeNum);

        foreach (var trigger in triggers)
        {
            trigger.RecallTypeNum = recallTypeNum;

            Insert(trigger);
        }
    }

    private class RecallTriggerCache : CacheListAbs<RecallTrigger>
    {
        protected override List<RecallTrigger> GetCacheFromDb()
        {
            return RecallTriggerCrud.SelectMany("SELECT * FROM recalltrigger");
        }

        protected override List<RecallTrigger> TableToList(DataTable dataTable)
        {
            return RecallTriggerCrud.TableToList(dataTable);
        }

        protected override RecallTrigger Copy(RecallTrigger item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<RecallTrigger> items)
        {
            return RecallTriggerCrud.ListToTable(items, "RecallTrigger");
        }

        protected override void FillCacheIfNeeded()
        {
            RecallTriggers.GetTableFromCache(false);
        }
    }

    private static readonly RecallTriggerCache Cache = new();

    public static List<RecallTrigger> GetWhere(Predicate<RecallTrigger> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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