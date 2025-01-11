using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CodeBase;
using OpenDental.Graph.Base;
using OpenDentBusiness;

namespace OpenDental.Graph.Cache
{
    public class DashboardCache
    {
        private static readonly Random Rand = new Random();

        public static bool UseProvFilter { get; set; }
        public static long ProvNum { get; set; }

        public static DashboardCacheNewPatient Patients { get; } = new DashboardCacheNewPatient();
        public static DashboardCacheCompletedProc CompletedProcs { get; } = new DashboardCacheCompletedProc();
        public static DashboardCacheWriteoff Writeoffs { get; } = new DashboardCacheWriteoff();
        public static DashboardCacheAdjustment Adjustments { get; } = new DashboardCacheAdjustment();
        public static DashboardCachePaySplit PaySplits { get; } = new DashboardCachePaySplit();
        public static DashboardCacheClaimPayment ClaimPayments { get; } = new DashboardCacheClaimPayment();
        public static DashboardCacheAR AR { get; } = new DashboardCacheAR();
        public static DashboardCacheProvider Providers { get; } = new DashboardCacheProvider();
        public static DashboardCacheBrokenAppt BrokenAppts { get; } = new DashboardCacheBrokenAppt();
        public static DashboardCacheBrokenProcedure BrokenProcs { get; } = new DashboardCacheBrokenProcedure();
        public static DashboardCacheBrokenAdj BrokenAdjs { get; } = new DashboardCacheBrokenAdj();
        public static DashboardCacheClinic Clinics { get; } = new DashboardCacheClinic();

        public static void RefreshLayoutsIfInvalid(List<DashboardLayout> layouts, bool waitToReturn, bool invalidateFirst, EventHandler onExit = null)
        {
            var cellTypes = Enum.GetValues(typeof(DashboardCellType)).Cast<DashboardCellType>().ToList();
            
            foreach (var cellType in cellTypes)
            {
                var filters = layouts
                    .SelectMany(x => x.Cells)
                    .Where(x => x.CellType == cellType)
                    .Select(x => ODGraphSettingsBase.Deserialize(ODGraphJson.Deserialize(x.CellSettings).GraphJson).Filter)
                    .ToList();
                
                if (filters.Count <= 0)
                {
                    continue;
                }

                RefreshCellTypeIfInvalid(
                    cellType,
                    new DashboardFilter
                    {
                        DateFrom = filters.Min(x => x.DateFrom),
                        DateTo = filters.Max(x => x.DateTo),
                        UseDateFilter = filters.All(x => x.UseDateFilter)
                    },
                    waitToReturn,
                    invalidateFirst,
                    onExit);
            }
        }

        public static void RefreshCellTypeIfInvalid(DashboardCellType cellType, DashboardFilter filter, bool waitToReturn, bool invalidateFirst, EventHandler onExit = null)
        {
            //Create a random group name so we can arbitrarily group and wait on the threads we are about to start.
            var groupName = cellType.ToString() + Rand.Next();
            try
            {
                //Always fill certain caches first. These will not be threaded as they need to be available to the threads which will run below.
                //It doesn't hurt to block momentarily here as the queries will run very quickly.			
                Providers.Run(new DashboardFilter {UseDateFilter = false}, invalidateFirst);
                Clinics.Run(new DashboardFilter {UseDateFilter = false}, invalidateFirst);
                //Start certain cache threads depending on which cellType we are interested in. Each cache will have its own thread.
                switch (cellType)
                {
                    case DashboardCellType.ProductionGraph:
                        FillCacheThreaded(CompletedProcs, filter, groupName, invalidateFirst);
                        FillCacheThreaded(Writeoffs, filter, groupName, invalidateFirst);
                        FillCacheThreaded(Adjustments, filter, groupName, invalidateFirst);
                        break;
                    case DashboardCellType.IncomeGraph:
                        FillCacheThreaded(PaySplits, filter, groupName, invalidateFirst);
                        FillCacheThreaded(ClaimPayments, filter, groupName, invalidateFirst);
                        break;
                    case DashboardCellType.AccountsReceivableGraph:
                        FillCacheThreaded(AR, filter, groupName, invalidateFirst);
                        break;
                    case DashboardCellType.NewPatientsGraph:
                        FillCacheThreaded(Patients, filter, groupName, invalidateFirst);
                        break;
                    case DashboardCellType.BrokenApptGraph:
                        FillCacheThreaded(BrokenAppts, filter, groupName, invalidateFirst);
                        FillCacheThreaded(BrokenProcs, filter, groupName, invalidateFirst);
                        FillCacheThreaded(BrokenAdjs, filter, groupName, invalidateFirst);
                        break;
                    case DashboardCellType.NotDefined:
                    default:
                        throw new Exception("Unsupported DashboardCellType: " + cellType.ToString());
                }
            }
            finally
            {
                if (waitToReturn)
                {
                    //Block until all threads have completed.
                    ODThread.JoinThreadsByGroupName(Timeout.Infinite, groupName, true);
                }
                else if (onExit != null)
                {
                    //Exit immediately but fire event later once all threads have completed.
                    ODThread.AddGroupNameExitHandler(groupName, onExit);
                }
            }
        }

        private static void FillCacheThreaded<T>(DashboardCacheBase<T> cache, DashboardFilter filter, string groupName, bool invalidateFirst)
        {
            var thread = new ODThread(th => { cache.Run(filter, invalidateFirst); })
            {
                GroupName = groupName
            };
            thread.Start(false);
        }
    }

    public abstract class DashboardCacheBase<T>
    {
        private static List<T> _cacheS;

        public List<T> Cache { get; private set; } = new List<T>();

        protected abstract List<T> GetCache(DashboardFilter filter);

        protected virtual void RemoveBadData(List<T> rawFromQuery)
        {
        }

        protected virtual bool AllowQueryDateFilter()
        {
            return true;
        }

        public void Run(DashboardFilter filter, bool invalidateFirst)
        {
            lock (DashboardCacheLock.Lock)
            {
                if (invalidateFirst)
                {
                    _cacheS = null;
                }

                if (!AllowQueryDateFilter())
                {
                    DashboardCacheLock.FilterOptions.UseDateFilter = false;
                }

                if (!IsInvalid(filter, out var dateFromOut, out var dateToOut, out var useProvFilter, out var provNum))
                {
                    return;
                }

                //The cache is invalid and should be filled using the new filter options.
                DashboardCacheLock.FilterOptions.DateFrom = dateFromOut;
                DashboardCacheLock.FilterOptions.DateTo = dateToOut;
                DashboardCacheLock.FilterOptions.UseProvFilter = useProvFilter;
                DashboardCacheLock.FilterOptions.ProvNum = provNum;
                if (!filter.UseDateFilter && DashboardCacheLock.FilterOptions.UseDateFilter)
                {
                    //Previously we used a date filter but now we are not. Set the flag indicating that we have retrieved the unfiltered cache.
                    DashboardCacheLock.FilterOptions.UseDateFilter = false;
                }

                //This is potentially a slow query so set the static cache here.
                _cacheS = GetCache(DashboardCacheLock.FilterOptions);
                RemoveBadData(_cacheS);
                //This is a simple shallow copy so it is safe enough to set _cache inside of this lock. It makes _cache available even when _cacheS is unavailable due to slow query.
                Cache = new List<T>(_cacheS);
            }
        }

        private static bool IsInvalid(DashboardFilter filterIn, out DateTime dateFromOut, out DateTime dateToOut, out bool useProvFilter, out long provNum)
        {
            dateFromOut = DashboardCacheLock.FilterOptions.DateFrom;
            dateToOut = DashboardCacheLock.FilterOptions.DateTo;
            useProvFilter = DashboardCacheLock.FilterOptions.UseProvFilter;
            provNum = DashboardCacheLock.FilterOptions.ProvNum;
            lock (DashboardCacheLock.Lock)
            {
                if (_cacheS == null)
                {
                    //Hasn't run yet so it is invalid.
                    dateFromOut = filterIn.DateFrom;
                    dateToOut = filterIn.DateTo;
                    useProvFilter = filterIn.UseProvFilter;
                    provNum = filterIn.ProvNum;
                    return true;
                }

                if (DashboardCacheLock.FilterOptions.UseProvFilter != filterIn.UseProvFilter)
                {
                    useProvFilter = filterIn.UseProvFilter;
                    provNum = filterIn.ProvNum;
                    return false;
                }

                if (DashboardCacheLock.FilterOptions.ProvNum != filterIn.ProvNum)
                {
                    useProvFilter = filterIn.UseProvFilter;
                    provNum = filterIn.ProvNum;
                    return false;
                }

                if (!DashboardCacheLock.FilterOptions.UseDateFilter)
                {
                    //Date filter has previously been turned off, which means we have already gotten the untiltered cache at least once.
                    //There will be nothing else to get so cache is valid.
                    return false;
                }

                var ret = false;
                if (filterIn.DateFrom < DashboardCacheLock.FilterOptions.DateFrom)
                {
                    //New 'from' is before old 'from', invalidate.
                    dateFromOut = filterIn.DateFrom;
                    ret = true;
                }

                if (filterIn.DateTo > DashboardCacheLock.FilterOptions.DateTo)
                {
                    //New 'to' is after old 'to', invalidate.
                    dateToOut = filterIn.DateTo;
                    ret = true;
                }

                return ret;
            }
        }
    }

    public abstract class DashboardCacheWithQuery<T> : DashboardCacheBase<T> where T : GraphQuantityOverTime.GraphPointBase
    {
        protected virtual bool UseReportServer => true;

        protected abstract string GetCommand(DashboardFilter filter);
        protected abstract T GetInstanceFromDataRow(DataRow x);

        protected override List<T> GetCache(DashboardFilter filter)
        {
            if (filter.UseProvFilter && filter.ProvNum == 0)
            {
                return new List<T>();
            }

            return DashboardQueries.GetTable(GetCommand(filter), UseReportServer)
                .AsEnumerable()
                .Select(GetInstanceFromDataRow)
                .ToList();
        }

        protected override void RemoveBadData(List<T> rawFromQuery)
        {
            rawFromQuery.RemoveAll(x => x.DateStamp.Year < 1880);
        }
    }

    public class DashboardCacheLock
    {
        public static readonly object Lock = new object();
        public static readonly DashboardFilter FilterOptions = new DashboardFilter();
    }

    public class DashboardFilter
    {
        public DateTime DateFrom = DateTime.Now;
        public DateTime DateTo = DateTime.Now;
        public bool UseDateFilter = true;
        public long ProvNum = DashboardCache.ProvNum;
        public bool UseProvFilter = DashboardCache.UseProvFilter;
    }
}