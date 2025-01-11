using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AccountingAutoPays
{
    public static void Insert(AccountingAutoPay accountingAutoPay)
    {
        AccountingAutoPayCrud.Insert(accountingAutoPay);
    }

    public static string GetPickListDesc(AccountingAutoPay accountingAutoPay)
    {
        var stringArrayNumbers = accountingAutoPay.PickList.Split(',');
        var retVal = "";
        for (var i = 0; i < stringArrayNumbers.Length; i++)
        {
            if (stringArrayNumbers[i] == "") continue;
            if (retVal != "") retVal += "\r\n";
            retVal += Accounts.GetDescript(SIn.Long(stringArrayNumbers[i]));
        }

        return retVal;
    }

    public static long[] GetPickListAccounts(AccountingAutoPay accountingAutoPay)
    {
        var stringArrayNumbers = accountingAutoPay.PickList.Split(',');
        var listAccountNums = new List<long>();
        for (var i = 0; i < stringArrayNumbers.Length; i++)
        {
            if (stringArrayNumbers[i] == "") continue;
            listAccountNums.Add(SIn.Long(stringArrayNumbers[i]));
        }

        var accountNumArray = new long[listAccountNums.Count];
        listAccountNums.CopyTo(accountNumArray);
        return accountNumArray;
    }
    
    public static AccountingAutoPay GetForPayType(long payType)
    {
        return GetFirstOrDefault(x => x.PayType == payType);
    }

    public static void SaveList(List<AccountingAutoPay> listAccountingAutoPays)
    {
        Db.NonQ("DELETE FROM accountingautopay");
        for (var i = 0; i < listAccountingAutoPays.Count; i++) Insert(listAccountingAutoPays[i]);
    }

    private class AccountingAutoPayCache : CacheListAbs<AccountingAutoPay>
    {
        protected override AccountingAutoPay Copy(AccountingAutoPay item)
        {
            return item.Clone();
        }

        protected override void FillCacheIfNeeded()
        {
            AccountingAutoPays.GetTableFromCache(false);
        }

        protected override List<AccountingAutoPay> GetCacheFromDb()
        {
            var command = "SELECT * FROM accountingautopay";
            return AccountingAutoPayCrud.SelectMany(command);
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

    public static List<AccountingAutoPay> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static AccountingAutoPay GetFirstOrDefault(Func<AccountingAutoPay, bool> funcMatch, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(funcMatch, isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
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