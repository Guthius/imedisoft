using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DisplayReports
{
    ///<summary>Returns all reports that should show in the main menu bar.  Ordered by Description.</summary>
    public static List<DisplayReport> GetSubMenuReports()
    {
        return GetWhere(x => x.IsVisibleInSubMenu, true).OrderBy(x => x.Description).ToList();
    }

    ///<summary>Get all display reports for the passed-in category.  Pass in true to retrieve hidden display reports.</summary>
    public static List<DisplayReport> GetForCategory(DisplayReportCategory displayReportCategory, bool showHidden)
    {
        return GetWhere(x => x.Category == displayReportCategory, !showHidden);
    }

    ///<summary>Pass in true to also retrieve hidden display reports.</summary>
    public static List<DisplayReport> GetAll(bool showHidden)
    {
        return GetDeepCopy(!showHidden);
    }

    /// <summary>
    ///     Use DisplayReports.ReportNames to pass this method an internal report name. Returns null if no matches are found.
    ///     Returns first result if multiple results are found (this shouldn't happen).
    /// </summary>
    public static DisplayReport GetByInternalName(string reportName)
    {
        var listDisplayReports = GetWhere(x => x.InternalName == reportName);
        if (listDisplayReports.IsNullOrEmpty()) return null;
        return listDisplayReports[0];
    }

    ///<summary>Gets all DisplayReports that are known to HQ (InternalName set) without using the local cache.</summary>
    public static List<DisplayReport> GetDisplayReportsNoCache()
    {
        var command = "SELECT * FROM displayreport WHERE InternalName!=''";
        return DisplayReportCrud.SelectMany(command);
    }

    ///<summary>Must pass in a list of all current display reports, even hidden ones.</summary>
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

        ///<summary>Production and Income More Options</summary>
        public const string ODMoreOptions = "ODMoreOptions";

        public const string ODProcOverpaid = "ODProcOverpaid";
        public const string DPPOvercharged = "ODDynamicPayPlanOvercharged";
        public const string MonthlyProductionGoal = "ODMonthlyProductionGoal";
        public const string EraAutoProcessed = "ODEraAutoProcessed";
        public const string ProceduresIndividual = "ODProceduresIndividual";
        public const string ProductionByProcedure = "ODProductionByProcedure";
    }

    #region CachePattern

    private class DisplayReportCache : CacheListAbs<DisplayReport>
    {
        protected override DisplayReport Copy(DisplayReport item)
        {
            return item.Copy();
        }

        protected override void FillCacheIfNeeded()
        {
            DisplayReports.GetTableFromCache(false);
        }

        protected override List<DisplayReport> GetCacheFromDb()
        {
            var command = "SELECT * FROM displayreport ORDER BY ItemOrder";
            return DisplayReportCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly DisplayReportCache _displayReportCache = new();

    public static List<DisplayReport> GetDeepCopy(bool isShort = false)
    {
        return _displayReportCache.GetDeepCopy(isShort);
    }

    public static List<DisplayReport> GetWhere(Predicate<DisplayReport> match, bool isShort = false)
    {
        return _displayReportCache.GetWhere(match, isShort);
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
        _displayReportCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _displayReportCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _displayReportCache.ClearCache();
    }

    #endregion
}