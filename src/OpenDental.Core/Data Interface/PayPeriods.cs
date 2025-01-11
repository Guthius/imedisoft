using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PayPeriods
{
    
    public static long Insert(PayPeriod pp)
    {
        return PayPeriodCrud.Insert(pp);
    }

    
    public static void Update(PayPeriod pp)
    {
        PayPeriodCrud.Update(pp);
    }

    
    public static void Delete(PayPeriod pp)
    {
        var command = "DELETE FROM payperiod WHERE PayPeriodNum = " + SOut.Long(pp.PayPeriodNum);
        Db.NonQ(command);
    }

    
    public static int GetForDate(DateTime date)
    {
        var payPeriod = GetFirstOrDefault(x => date.Date >= x.DateStart.Date && date.Date <= x.DateStop.Date);
        if (payPeriod == null)
        {
            //Get the last most recent pay period. 
            var listPayPeriods = GetWhere(x => date.Date >= x.DateStart.Date && date.Date > x.DateStop.Date);
            if (listPayPeriods.Count > 0) payPeriod = listPayPeriods.Aggregate((x1, x2) => x1.DateStop > x2.DateStop ? x1 : x2);
        }

        if (payPeriod == null)
        {
            //Get the next most recent pay period.
            var listPayPeriods = GetWhere(x => date.Date < x.DateStart.Date && date.Date <= x.DateStop.Date);
            if (listPayPeriods.Count > 0) payPeriod = listPayPeriods.Aggregate((x1, x2) => x1.DateStart < x2.DateStart ? x1 : x2);
        }

        if (payPeriod == null) return GetCount() - 1;
        var index = GetFindIndex(x => x.PayPeriodNum == payPeriod.PayPeriodNum);
        return index > -1 ? index : GetCount() - 1;
    }

    
    public static bool HasPayPeriodForDate(DateTime date)
    {
        var payPeriod = GetFirstOrDefault(x => date.Date >= x.DateStart.Date && date.Date <= x.DateStop.Date);
        return payPeriod != null;
    }

    /// <summary>
    ///     Pass in the TimeAdjust.EmployeeNum or ClockEvent.EmployeeNum. If true, the user cannot edit the
    ///     TimeAdjust/ClockEvent for that date.
    /// </summary>
    public static bool CannotEditPayPeriodOfDate(DateTime date, long employeeNum)
    {
        return Security.CurUser != null
               && Security.CurUser.EmployeeNum == employeeNum
               && PrefC.GetBool(PrefName.TimecardSecurityEnabled)
               && PrefC.GetBool(PrefName.TimecardUsersCantEditPastPayPeriods)
               && (!HasPayPeriodForDate(date) || GetForDate(date) != GetForDate(DateTime.Today));
    }

    ///<summary>Returns the most recent payperiod object or null if none were found.</summary>
    public static PayPeriod GetMostRecent()
    {
        var command = "SELECT * FROM payperiod WHERE DateStop=(SELECT MAX(DateStop) FROM payperiod)";
        return PayPeriodCrud.SelectOne(command);
    }


    /// <summary>
    ///     Determines whether there is any overlap in dates between the two passed-in list of pay periods.
    ///     Same-date overlaps are not allowed (eg you cannot have a pay period that ends the same day as the next one starts).
    /// </summary>
    public static bool AreAnyOverlapping(List<PayPeriod> listFirst, List<PayPeriod> listSecond)
    {
        foreach (var payPeriodFirst in listFirst)
            if (listSecond.Where(payPeriodSecond => !payPeriodSecond.IsSame(payPeriodFirst)
                                                    && ((payPeriodFirst.DateStop >= payPeriodSecond.DateStart && payPeriodFirst.DateStop <= payPeriodSecond.DateStop) //the bottom of first overlaps
                                                        || (payPeriodFirst.DateStart >= payPeriodSecond.DateStart && payPeriodFirst.DateStart <= payPeriodSecond.DateStop))) //the top of first overlaps
                    .Count() > 0)
                return true;

        return false;
    }

    #region CachePattern

    private class PayPeriodCache : CacheListAbs<PayPeriod>
    {
        protected override List<PayPeriod> GetCacheFromDb()
        {
            var command = "SELECT * from payperiod ORDER BY DateStart";
            return PayPeriodCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly PayPeriodCache _payPeriodCache = new();

    public static List<PayPeriod> GetDeepCopy(bool isShort = false)
    {
        return _payPeriodCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _payPeriodCache.GetCount(isShort);
    }

    public static int GetFindIndex(Predicate<PayPeriod> match, bool isShort = false)
    {
        return _payPeriodCache.GetFindIndex(match, isShort);
    }

    public static PayPeriod GetFirstOrDefault(Func<PayPeriod, bool> match, bool isShort = false)
    {
        return _payPeriodCache.GetFirstOrDefault(match, isShort);
    }

    public static PayPeriod GetLast(bool isShort = false)
    {
        return _payPeriodCache.GetLast(isShort);
    }

    public static List<PayPeriod> GetWhere(Predicate<PayPeriod> match, bool isShort = false)
    {
        return _payPeriodCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _payPeriodCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _payPeriodCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _payPeriodCache.ClearCache();
    }

    #endregion
}