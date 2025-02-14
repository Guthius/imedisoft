using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AppointmentRules
{
    public static void Insert(AppointmentRule appointmentRule)
    {
        AppointmentRuleCrud.Insert(appointmentRule);
    }

    public static void Update(AppointmentRule appointmentRule)
    {
        AppointmentRuleCrud.Update(appointmentRule);
    }

    public static void Delete(AppointmentRule appointmentRule)
    {
        Db.NonQ("DELETE FROM appointmentrule WHERE AppointmentRuleNum = " + appointmentRule.AppointmentRuleNum);
    }

    public static bool IsBlocked(List<string> procCodes)
    {
        var appointmentRules = GetWhere(x => x.IsEnabled);

        foreach (var procCode in procCodes)
        {
            foreach (var appointmentRule in appointmentRules)
            {
                if (string.CompareOrdinal(procCode, appointmentRule.CodeStart) < 0 ||
                    string.CompareOrdinal(procCode, appointmentRule.CodeEnd) > 0)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    public static string GetBlockedDescription(List<string> procCodes)
    {
        var appointmentRules = GetDeepCopy();

        foreach (var procCode in procCodes)
        {
            foreach (var appointmentRule in appointmentRules)
            {
                if (!appointmentRule.IsEnabled)
                {
                    continue;
                }

                if (string.CompareOrdinal(procCode, appointmentRule.CodeStart) < 0 ||
                    string.CompareOrdinal(procCode, appointmentRule.CodeEnd) > 0)
                {
                    continue;
                }

                return appointmentRule.RuleDesc;
            }
        }

        return "";
    }

    private class AppointmentRuleCache : CacheListAbs<AppointmentRule>
    {
        protected override List<AppointmentRule> GetCacheFromDb()
        {
            return AppointmentRuleCrud.SelectMany("SELECT * FROM appointmentrule");
        }

        protected override List<AppointmentRule> TableToList(DataTable dataTable)
        {
            return AppointmentRuleCrud.TableToList(dataTable);
        }

        protected override AppointmentRule Copy(AppointmentRule item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<AppointmentRule> items)
        {
            return AppointmentRuleCrud.ListToTable(items, "AppointmentRule");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly AppointmentRuleCache Cache = new();

    public static int GetCount(bool shortList = false)
    {
        return Cache.GetCount(shortList);
    }

    public static List<AppointmentRule> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<AppointmentRule> GetWhere(Predicate<AppointmentRule> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
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