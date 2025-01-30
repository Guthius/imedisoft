using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PayPeriods
{
    public static void Insert(PayPeriod payPeriod)
    {
        PayPeriodCrud.Insert(payPeriod);
    }

    public static void Update(PayPeriod payPeriod)
    {
        PayPeriodCrud.Update(payPeriod);
    }

    public static void Delete(PayPeriod payPeriod)
    {
        Db.NonQ("DELETE FROM payperiod WHERE PayPeriodNum = " + payPeriod.PayPeriodNum);
    }

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

    public static bool HasPayPeriodForDate(DateTime date)
    {
        var payPeriod = GetFirstOrDefault(x => date.Date >= x.DateStart.Date && date.Date <= x.DateStop.Date);

        return payPeriod is not null;
    }

    public static bool CannotEditPayPeriodOfDate(DateTime date, long employeeNum)
    {
        return Security.CurUser is not null &&
               Security.CurUser.EmployeeNum == employeeNum &&
               PrefC.GetBool(PrefName.TimecardSecurityEnabled) &&
               PrefC.GetBool(PrefName.TimecardUsersCantEditPastPayPeriods) &&
               (!HasPayPeriodForDate(date) || GetForDate(date) != GetForDate(DateTime.Today));
    }

    public static PayPeriod GetMostRecent()
    {
        return PayPeriodCrud.SelectOne("SELECT * FROM payperiod WHERE DateStop=(SELECT MAX(DateStop) FROM payperiod)");
    }

    public static bool AreAnyOverlapping(List<PayPeriod> left, List<PayPeriod> right)
    {
        return left.Any(x => right.Any(y =>
            !y.IsSame(x) &&
            ((x.DateStop >= y.DateStart && x.DateStop <= y.DateStop) ||
             (x.DateStart >= y.DateStart && x.DateStart <= y.DateStop))));
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
            PayPeriods.GetTableFromCache(false);
        }
    }

    private static readonly PayPeriodCache Cache = new();

    public static List<PayPeriod> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
    }

    public static int GetFindIndex(Predicate<PayPeriod> match, bool isShort = false)
    {
        return Cache.GetFindIndex(match, isShort);
    }

    public static PayPeriod GetFirstOrDefault(Func<PayPeriod, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static PayPeriod GetLast(bool isShort = false)
    {
        return Cache.GetLast(isShort);
    }

    public static List<PayPeriod> GetWhere(Predicate<PayPeriod> match, bool isShort = false)
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