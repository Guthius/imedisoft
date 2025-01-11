using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AppointmentRules
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
        var command = "DELETE FROM appointmentrule WHERE AppointmentRuleNum = " + SOut.Long(appointmentRule.AppointmentRuleNum);
        Db.NonQ(command);
    }
    
    public static bool IsBlocked(List<string> listProcCodes)
    {
        var listAppointmentRules = GetWhere(x => x.IsEnabled);
        for (var j = 0; j < listProcCodes.Count; j++)
        for (var i = 0; i < listAppointmentRules.Count; i++)
        {
            if (string.Compare(listProcCodes[j], listAppointmentRules[i].CodeStart) < 0) continue;
            if (string.Compare(listProcCodes[j], listAppointmentRules[i].CodeEnd) > 0) continue;
            return true;
        }

        return false;
    }

    public static string GetBlockedDescription(List<string> listProcCodes)
    {
        var listAppointmentRules = GetDeepCopy();
        for (var j = 0; j < listProcCodes.Count; j++)
        for (var i = 0; i < listAppointmentRules.Count; i++)
        {
            if (!listAppointmentRules[i].IsEnabled) continue;
            if (string.Compare(listProcCodes[j], listAppointmentRules[i].CodeStart) < 0) continue;
            if (string.Compare(listProcCodes[j], listAppointmentRules[i].CodeEnd) > 0) continue;
            return listAppointmentRules[i].RuleDesc;
        }

        return "";
    }
    
    private class AppointmentRuleCache : CacheListAbs<AppointmentRule>
    {
        protected override List<AppointmentRule> GetCacheFromDb()
        {
            var command = "SELECT * FROM appointmentrule";
            return AppointmentRuleCrud.SelectMany(command);
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
            AppointmentRules.GetTableFromCache(false);
        }
    }

    private static readonly AppointmentRuleCache Cache = new();

    public static int GetCount(bool isShort = false)
    {
        return Cache.GetCount(isShort);
    }

    public static List<AppointmentRule> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<AppointmentRule> GetWhere(Predicate<AppointmentRule> match, bool isShort = false)
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