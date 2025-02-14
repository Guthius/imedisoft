using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class DisplayReports
{
    public static List<DisplayReport> GetSubMenuReports()
    {
        return GetWhere(x => x.IsVisibleInSubMenu, true).OrderBy(x => x.Description).ToList();
    }

    public static List<DisplayReport> GetForCategory(DisplayReportCategory displayReportCategory, bool showHidden)
    {
        return GetWhere(x => x.Category == displayReportCategory, !showHidden);
    }

    public static List<DisplayReport> GetAll(bool showHidden)
    {
        return GetDeepCopy(!showHidden);
    }

    public static DisplayReport GetByInternalName(string reportName)
    {
        var displayReports = GetWhere(x => x.InternalName == reportName);
        
        return displayReports.IsNullOrEmpty() ? null : displayReports[0];
    }

    public static bool Sync(List<DisplayReport> listDisplayReports)
    {
        return DisplayReportCrud.Sync(listDisplayReports, GetAll(true)); //TODO: cache?
    }

    public class ReportNames
    {
        public const string UnfinalizedInsPay = "ODUnfinalizedInsPay";
        public const string PatPortionUncollected = "ODPatPortionUncollected";
        public const string WebSchedAppointments = "ODWebSchedAppointments";
        public const string InsAging = "ODInsAging";
        public const string CustomAging = "ODCustomAging";
        public const string OutstandingInsClaims = "ODOutstandingInsClaims";
        public const string ClaimsNotSent = "ODClaimsNotSent";
        public const string TreatmentFinder = "ODTreatmentFinder";
        public const string ReferredProcTracking = "ODReferredProcTracking";
        public const string IncompleteProcNotes = "ODIncompleteProcNotes";
        public const string ProcNotBilledIns = "ODProcsNotBilled";
        public const string ODProcOverpaid = "ODProcOverpaid";
        public const string DPPOvercharged = "ODDynamicPayPlanOvercharged";
        public const string EraAutoProcessed = "ODEraAutoProcessed";
        public const string ProceduresIndividual = "ODProceduresIndividual";
        public const string ProductionByProcedure = "ODProductionByProcedure";
    }

    private class DisplayReportCache : CacheListAbs<DisplayReport>
    {
        protected override DisplayReport Copy(DisplayReport item)
        {
            return item.Copy();
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override List<DisplayReport> GetCacheFromDb()
        {
            return DisplayReportCrud.SelectMany("SELECT * FROM displayreport ORDER BY ItemOrder");
        }

        protected override DataTable ToDataTable(List<DisplayReport> items)
        {
            return DisplayReportCrud.ListToTable(items, "DisplayReport");
        }

        protected override List<DisplayReport> TableToList(DataTable dataTable)
        {
            return DisplayReportCrud.TableToList(dataTable);
        }

        protected override bool IsInListShort(DisplayReport item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly DisplayReportCache Cache = new();

    public static List<DisplayReport> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<DisplayReport> GetWhere(Predicate<DisplayReport> predicate, bool shortList = false)
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