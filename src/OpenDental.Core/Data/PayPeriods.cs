using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class PayPeriods
{
    public static int GetForDate(DateTime date)
    {
        var payPeriod = GetFirstOrDefault(x => date.Date >= x.DateStart.Date && date.Date <= x.DateStop.Date);
        if (payPeriod is null)
        {
            var payPeriods = GetWhere(x => date.Date >= x.DateStart.Date && date.Date > x.DateStop.Date);
            if (payPeriods.Count > 0)
            {
                payPeriod = payPeriods.Aggregate((x1, x2) => x1.DateStop > x2.DateStop ? x1 : x2);
            }
        }

        if (payPeriod is null)
        {
            var payPeriods = GetWhere(x => date.Date < x.DateStart.Date && date.Date <= x.DateStop.Date);
            if (payPeriods.Count > 0)
            {
                payPeriod = payPeriods.Aggregate((x1, x2) => x1.DateStart < x2.DateStart ? x1 : x2);
            }
        }

        if (payPeriod == null)
        {
            return GetCount() - 1;
        }

        var index = GetFindIndex(x => x.PayPeriodNum == payPeriod.PayPeriodNum);

        return index > -1 ? index : GetCount() - 1;
    }

    private class PayPeriodCache : CacheListAbs<PayPeriod>
    {
        protected override List<PayPeriod> GetCacheFromDb()
        {
            return PayPeriodCrud.SelectMany("SELECT * from payperiod ORDER BY DateStart");
        }

        protected override List<PayPeriod> TableToList(DataTable dataTable)
        {
            return PayPeriodCrud.TableToList(dataTable);
        }

        protected override PayPeriod Copy(PayPeriod item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<PayPeriod> items)
        {
            return PayPeriodCrud.ListToTable(items, "PayPeriod");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly PayPeriodCache Cache = new();

    public static List<PayPeriod> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static int GetCount(bool shortList = false)
    {
        return Cache.GetCount(shortList);
    }

    public static int GetFindIndex(Predicate<PayPeriod> predicate, bool shortList = false)
    {
        return Cache.GetFindIndex(predicate, shortList);
    }

    public static PayPeriod GetFirstOrDefault(Func<PayPeriod, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<PayPeriod> GetWhere(Predicate<PayPeriod> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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