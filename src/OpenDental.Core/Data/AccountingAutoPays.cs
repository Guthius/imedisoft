using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AccountingAutoPays
{
    public static void Insert(AccountingAutoPay accountingAutoPay)
    {
        AccountingAutoPayCrud.Insert(accountingAutoPay);
    }

    public static string GetPickListDescription(AccountingAutoPay accountingAutoPay)
    {
        var accountNums = accountingAutoPay.PickList.Split(',');
        var accountDescriptions = "";

        foreach (var sdr in accountNums)
        {
            if (sdr == "")
            {
                continue;
            }

            if (accountDescriptions != "")
            {
                accountDescriptions += "\r\n";
            }

            accountDescriptions += Accounts.GetDescript(SIn.Long(sdr));
        }

        return accountDescriptions;
    }

    public static long[] GetPickListAccounts(AccountingAutoPay accountingAutoPay)
    {
        return accountingAutoPay.PickList
            .Split(',')
            .Where(str => str != "")
            .Select(str => SIn.Long(str))
            .ToArray();
    }

    public static AccountingAutoPay GetForPayType(long payType)
    {
        return GetFirstOrDefault(x => x.PayType == payType);
    }

    public static void SaveList(List<AccountingAutoPay> accountingAutoPays)
    {
        Db.NonQ("DELETE FROM accountingautopay");

        foreach (var accountingAutoPay in accountingAutoPays)
        {
            Insert(accountingAutoPay);
        }
    }

    private class AccountingAutoPayCache : CacheListAbs<AccountingAutoPay>
    {
        protected override AccountingAutoPay Copy(AccountingAutoPay item)
        {
            return item.Clone();
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override List<AccountingAutoPay> GetCacheFromDb()
        {
            return AccountingAutoPayCrud.SelectMany("SELECT * FROM accountingautopay");
        }

        protected override DataTable ToDataTable(List<AccountingAutoPay> items)
        {
            return AccountingAutoPayCrud.ListToTable(items, "AccountingAutoPay");
        }

        protected override List<AccountingAutoPay> TableToList(DataTable dataTable)
        {
            return AccountingAutoPayCrud.TableToList(dataTable);
        }
    }

    private static readonly AccountingAutoPayCache Cache = new();

    public static List<AccountingAutoPay> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static AccountingAutoPay GetFirstOrDefault(Func<AccountingAutoPay, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static int GetCount(bool shortList = false)
    {
        return Cache.GetCount(shortList);
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