using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CodeBase;
using OpenDental.Graph.Base;
using OpenDental.Graph.Cache;
using OpenDental.Graph.Dashboard;
using OpenDentBusiness;
using IntervalType = System.Windows.Forms.DataVisualization.Charting.IntervalType;

namespace OpenDental.Graph.Concrete
{
    public partial class GraphQuantityOverTimeFilter : UserControl, IODGraphPrinter, IDashboardDockContainer
    {
        private DashboardCellType _cellType = DashboardCellType.NotDefined;
        private readonly IncomeGraphOptionsCtrl _incomeOptionsCtrl = new IncomeGraphOptionsCtrl();
        private readonly ProductionGraphOptionsCtrl _productionOptionsCtrl = new ProductionGraphOptionsCtrl();
        private readonly BrokenApptGraphOptionsCtrl _brokenApptsCtrl = new BrokenApptGraphOptionsCtrl();
        private readonly HqMessagesRealTimeOptionsCtrl _hqMsgRealTimeCtrl = new HqMessagesRealTimeOptionsCtrl();

        public delegate List<GraphQuantityOverTime.GraphPointBase> GetGraphPointsForHqArgs(GraphQuantityOverTimeFilter filterCtrl);

        public readonly GraphQuantityOverTime.OnGetColorFromSeriesGraphTypeArgs OnGetSeriesColorOverride;

        public bool CanEdit
        {
            get
            {
                switch (CellType)
                {
                    case DashboardCellType.ProductionGraph:
                    case DashboardCellType.IncomeGraph:
                    case DashboardCellType.NewPatientsGraph:
                    case DashboardCellType.BrokenApptGraph:
                    case DashboardCellType.AccountsReceivableGraph:
                        return true;
                    default:
                        return false;
                }
            }
        }

        [Category("Graph")]
        public bool ShowFilters
        {
            get => Graph.ShowFilters;
            set
            {
                Graph.ShowFilters = value;
                switch (CellType)
                {
                    case DashboardCellType.ProductionGraph:
                    case DashboardCellType.IncomeGraph:
                    case DashboardCellType.BrokenApptGraph:
                    case DashboardCellType.NewPatientsGraph:
                        splitContainer.Panel1Collapsed = !Graph.ShowFilters;
                        break;
                    case DashboardCellType.AccountsReceivableGraph:
                    default:
                        break;
                }
            }
        }

        [Category("Graph")]
        public DashboardCellType CellType
        {
            get => _cellType;
            set
            {
                _cellType = value;
                BaseGraphOptionsCtrl filterCtrl = null;
                switch (CellType)
                {
                    case DashboardCellType.ProductionGraph:
                        filterCtrl = _productionOptionsCtrl;
                        Graph.GraphTitle = "Production";
                        Graph.MoneyItemDescription = "Production $";
                        Graph.CountItemDescription = "Count Procedures";
                        Graph.RemoveQuantityType(QuantityType.DecimalPoint);
                        Graph.GroupByType = IntervalType.Months;
                        Graph.SeriesType = SeriesChartType.Line;
                        Graph.BreakdownPref = BreakdownType.none;
                        Graph.LegendDock = LegendDockType.None;
                        Graph.QuickRangePref = QuickRange.last12Months;
                        Graph.QtyType = QuantityType.Money;
                        break;
                    case DashboardCellType.IncomeGraph:
                        filterCtrl = _incomeOptionsCtrl;
                        Graph.GraphTitle = "Income";
                        Graph.MoneyItemDescription = "Income $";
                        Graph.RemoveQuantityType(QuantityType.Count);
                        Graph.RemoveQuantityType(QuantityType.DecimalPoint);
                        Graph.GroupByType = IntervalType.Months;
                        Graph.SeriesType = SeriesChartType.Line;
                        Graph.BreakdownPref = BreakdownType.none;
                        Graph.LegendDock = LegendDockType.None;
                        Graph.QuickRangePref = QuickRange.last12Months;
                        Graph.QtyType = QuantityType.Money;
                        break;
                    case DashboardCellType.BrokenApptGraph:
                        filterCtrl = _brokenApptsCtrl;
                        Graph.GraphTitle = "Broken Appointments";
                        Graph.MoneyItemDescription = "Fees";
                        Graph.CountItemDescription = "Count";
                        Graph.RemoveQuantityType(QuantityType.DecimalPoint);
                        Graph.GroupByType = IntervalType.Months;
                        Graph.SeriesType = SeriesChartType.Line;
                        Graph.BreakdownPref = BreakdownType.none;
                        Graph.LegendDock = LegendDockType.None;
                        Graph.QuickRangePref = QuickRange.last12Months;
                        Graph.QtyType = QuantityType.Count;
                        break;
                    case DashboardCellType.AccountsReceivableGraph:
                        Graph.GraphTitle = "Accounts Receivable";
                        Graph.MoneyItemDescription = "Receivable $";
                        Graph.RemoveQuantityType(QuantityType.Count);
                        Graph.RemoveQuantityType(QuantityType.DecimalPoint);
                        Graph.GroupByType = IntervalType.Months;
                        Graph.SeriesType = SeriesChartType.Line;
                        Graph.BreakdownPref = BreakdownType.none;
                        Graph.LegendDock = LegendDockType.None;
                        Graph.QuickRangePref = QuickRange.last12Months;
                        Graph.QtyType = QuantityType.Money;
                        break;
                    case DashboardCellType.NewPatientsGraph:
                        filterCtrl = new BaseGraphOptionsCtrl();
                        Graph.GraphTitle = "New Patients";
                        Graph.RemoveQuantityType(QuantityType.Money);
                        Graph.RemoveQuantityType(QuantityType.DecimalPoint);
                        Graph.CountItemDescription = "Count Patients";
                        Graph.GroupByType = IntervalType.Months;
                        Graph.SeriesType = SeriesChartType.Line;
                        Graph.BreakdownPref = BreakdownType.none;
                        Graph.LegendDock = LegendDockType.None;
                        Graph.QuickRangePref = QuickRange.last12Months;
                        Graph.QtyType = QuantityType.Count;
                        break;
                    default:
                        throw new Exception("Unsupported CellType: " + CellType.ToString());
                }

                splitContainerOptions.Panel2.Controls.Clear();
                if (filterCtrl == null)
                {
                    splitContainer.Panel1Collapsed = true;
                }
                else
                {
                    splitContainer.Panel1Collapsed = false;
                    splitContainer.SplitterDistance = Math.Max(filterCtrl.GetPanelHeight(), groupingOptionsCtrl1.Height);
                    filterCtrl.Dock = DockStyle.Fill;
                    splitContainerOptions.Panel2.Controls.Add(filterCtrl);
                    splitContainerOptions.Panel1Collapsed = !filterCtrl.HasGroupOptions;
                }
            }
        }

        public GroupingOptionsCtrl.Grouping CurGrouping
        {
            get => groupingOptionsCtrl1.CurGrouping;
            set => groupingOptionsCtrl1.CurGrouping = value;
        }

        public GraphQuantityOverTime Graph { get; private set; }

        public GraphQuantityOverTimeFilter() : this(DashboardCellType.ProductionGraph)
        {
        }

        public GraphQuantityOverTimeFilter(DashboardCellType cellType, string jsonSettings = "", GraphQuantityOverTime.OnGetColorFromSeriesGraphTypeArgs onGetSeriesColor = null)
        {
            InitializeComponent();
            //We will turn IsLoading off elsewhere but make sure it is set here to prevent trying to perform FilterData() to soon.
            Graph.IsLoading = true;
            //Important that CellType is set before other properties as it gives default view.
            CellType = cellType;
            ShowFilters = false;
            Graph.LegendDock = LegendDockType.None;
            SetFilterAndGraphSettings(jsonSettings);
            _incomeOptionsCtrl.InputsChanged += OnFormInputsChanged;
            _productionOptionsCtrl.InputsChanged += OnFormInputsChanged;
            _brokenApptsCtrl.InputsChanged += OnFormInputsChanged;
            _hqMsgRealTimeCtrl.InputsChanged += OnFormInputsChanged;
            OnGetSeriesColorOverride = onGetSeriesColor;
            Graph.GetRawData = OnGraphGetRawData;
        }

        protected void OnInitDone(object sender, EventArgs e)
        {
            Graph.TriggerGetData(sender);
        }

        protected void OnInit(bool forceCachRefresh)
        {
            //This is occurring in a thread so it is ok to wait for Refresh to return. The UI is already loading and available on the main thread.
            DashboardCache.RefreshCellTypeIfInvalid(CellType, Graph.Filter, true, forceCachRefresh);
        }

        private ODThread _thread;

        private void Init(bool forceCacheRefesh = false, ODThread.ExceptionDelegate onException = null, EventHandler onThreadDone = null)
        {
            if (_thread != null)
            {
                return;
            }

            onThreadDone ??= delegate { };
            onException ??= e => { };

            _thread = new ODThread(x =>
            {
                //The thread may have run and return before the window is even ready to display.
                if (IsHandleCreated)
                {
                    OnThreadStartLocal(this, EventArgs.Empty);
                }
                else
                {
                    HandleCreated += OnThreadStartLocal;
                }

                //Alert caller that it's time to start querying the db.
                OnInit(forceCacheRefesh);
            })
            {
                Name = "GraphQuantityFilterOverTime.Init"
            };
            _thread.AddExceptionHandler(onException);
            _thread.AddExitHandler(x =>
            {
                try
                {
                    _thread = null;
                    //The thread may have run and return before the window is even ready to display.
                    if (IsHandleCreated)
                    {
                        OnThreadExitLocal(this, EventArgs.Empty);
                    }
                    else
                    {
                        HandleCreated += OnThreadExitLocal;
                    }

                    //Alert caller that db querying is done.
                    onThreadDone(this, EventArgs.Empty);
                }
                catch
                {
                    // ignored
                }
            });
            _thread.Start();
        }

        private void OnThreadStartLocal(object sender, EventArgs e)
        {
            try
            {
                HandleCreated -= OnThreadStartLocal;
                Invoke((Action) delegate
                {
                    //Alert graph.
                    Graph.IsLoading = true;
                });
            }
            catch
            {
                // ignored
            }
        }

        private void OnThreadExitLocal(object sender, EventArgs e)
        {
            try
            {
                Invoke((Action) delegate
                {
                    if (!Graph.IsLoading)
                    {
                        //Prevent re-entrance in case we get here more than once.
                        return;
                    }

                    //Alert graph.
                    Graph.IsLoading = false;
                    //Alert inheriting class.
                    OnInitDone(this, EventArgs.Empty);
                });
            }
            catch
            {
                // ignored
            }
        }

        private string SerializeToJson()
        {
            return ODGraphSettingsBase.Serialize(GetGraphSettings());
        }

        private void SetFilterAndGraphSettings(string jsonSettings)
        {
            if (string.IsNullOrEmpty(jsonSettings))
            {
                return;
            }

            var graphJson = ODGraphJson.Deserialize(jsonSettings);
            if (Graph != null)
            {
                Graph.DeserializeFromJson(graphJson.GraphJson);
            }

            DeserializeFromJson(graphJson.FilterJson);
        }

        private string GetFilterAndGraphSettings()
        {
            return ODGraphJson.Serialize(new ODGraphJson()
            {
                FilterJson = SerializeToJson(),
                GraphJson = Graph == null ? "" : Graph.SerializeToJson(),
            });
        }

        public string GetCellSettings()
        {
            return GetFilterAndGraphSettings();
        }

        private void OnFormInputsChanged(object sender, EventArgs e)
        {
            Graph.TriggerGetData(sender);
        }

        private List<GraphQuantityOverTime.GraphPointBase> OnGraphGetRawData()
        {
            var rawData = new List<GraphQuantityOverTime.GraphPointBase>();
            //Fill the dataset that we will send to the graph. The dataset will be filled according to user preferences.
            switch (CellType)
            {
                case DashboardCellType.ProductionGraph:
                {
                    SetGroupItems(CurGrouping);
                    if (_productionOptionsCtrl.IncludeAdjustments)
                    {
                        rawData.AddRange(DashboardCache.Adjustments.Cache.Select(x => GetDataPointForGrouping(x, CurGrouping)));
                    }

                    if (_productionOptionsCtrl.IncludeCompletedProcs)
                    {
                        rawData.AddRange(DashboardCache.CompletedProcs.Cache.Select(x => GetDataPointForGrouping(x, CurGrouping)));
                        rawData.AddRange(DashboardCache.Writeoffs.Cache.Where(x => x.IsCap).Select(x => GetDataPointForGrouping(x, CurGrouping)));
                    }

                    if (_productionOptionsCtrl.IncludeWriteoffs)
                    {
                        rawData.AddRange(DashboardCache.Writeoffs.Cache.Where(x => x.IsCap == false).Select(x => GetDataPointForGrouping(x, CurGrouping)));
                    }
                }
                    break;
                case DashboardCellType.IncomeGraph:
                {
                    SetGroupItems(CurGrouping);
                    if (_incomeOptionsCtrl.IncludePaySplits)
                    {
                        rawData.AddRange(DashboardCache.PaySplits.Cache.Select(x => GetDataPointForGrouping(x, CurGrouping)));
                    }

                    if (_incomeOptionsCtrl.IncludeInsuranceClaimPayments)
                    {
                        rawData.AddRange(DashboardCache.ClaimPayments.Cache.Select(x => GetDataPointForGrouping(x, CurGrouping)));
                    }
                }
                    break;
                case DashboardCellType.AccountsReceivableGraph:
                {
                    rawData.AddRange(DashboardCache.AR.Cache
                        .Select(x => new GraphQuantityOverTime.GraphPointBase()
                        {
                            Val = x.BalTotal,
                            Count = 0,
                            SeriesName = "All",
                            DateStamp = x.DateCalc,
                        })
                        .ToList());
                }
                    break;
                case DashboardCellType.NewPatientsGraph:
                {
                    SetGroupItems(CurGrouping);
                    rawData.AddRange(DashboardCache.Patients.Cache.Select(x => GetDataPointForGrouping(x, CurGrouping)));
                }
                    break;
                case DashboardCellType.BrokenApptGraph:
                {
                    SetGroupItems(CurGrouping);
                    switch (_brokenApptsCtrl.CurRunFor)
                    {
                        case BrokenApptGraphOptionsCtrl.RunFor.Appointment:
                            //money is not used when counting appointments
                            Graph.RemoveQuantityType(QuantityType.Money);
                            //use the broken appointment cache to get all relevant broken appts.
                            rawData.AddRange(DashboardCache.BrokenAppts.Cache.Select(x => GetDataPointForGrouping(x, CurGrouping)));
                            break;
                        case BrokenApptGraphOptionsCtrl.RunFor.Adjustment:
                            //money should be added back in case the user looked at appointments beforehand. 
                            Graph.InsertQuantityType(QuantityType.Money, "Fees", 0);
                            //use the broken adjustment cache to get all broken adjustments filtered by the selected adjType.
                            rawData.AddRange(DashboardCache.BrokenAdjs.Cache.Where(x => x.AdjType == _brokenApptsCtrl.AdjTypeDefNumCur).Select(x => GetDataPointForGrouping(x, CurGrouping)));
                            break;
                        case BrokenApptGraphOptionsCtrl.RunFor.Procedure:
                            Graph.InsertQuantityType(QuantityType.Money, "Fees", 0);
                            //use the broken proc cache to get all relevant broken procedures.
                            var listProcCodes = new List<string>();
                            switch (_brokenApptsCtrl.BrokenApptCodeCur)
                            {
                                case BrokenApptProcedure.None:
                                case BrokenApptProcedure.Missed:
                                    listProcCodes.Add("D9986");
                                    break;
                                case BrokenApptProcedure.Cancelled:
                                    listProcCodes.Add("D9987");
                                    break;
                                case BrokenApptProcedure.Both:
                                    listProcCodes.Add("D9986");
                                    listProcCodes.Add("D9987");
                                    break;
                            }

                            rawData.AddRange(DashboardCache.BrokenProcs.Cache.Where(x => listProcCodes.Contains(x.ProcCode)).Select(x => GetDataPointForGrouping(x, CurGrouping)));
                            break;
                        default:
                            throw new Exception("Unsupported CurRunFor: " + _brokenApptsCtrl.CurRunFor.ToString());
                    }
                }
                    break;
                default:
                    throw new Exception("Unsupported CellType: " + CellType.ToString());
            }

            return rawData;
        }

        private void SetGroupItems(GroupingOptionsCtrl.Grouping curGrouping)
        {
            switch (curGrouping)
            {
                case GroupingOptionsCtrl.Grouping.Provider:
                    Graph.UseBuiltInColors = false;
                    Graph.LegendTitle = "Provider";
                    break;
                case GroupingOptionsCtrl.Grouping.Clinic:
                    Graph.LegendTitle = "Clinic";
                    Graph.UseBuiltInColors = true;
                    break;
                default:
                    Graph.LegendTitle = "Group";
                    Graph.UseBuiltInColors = true;
                    break;
            }
        }

        private static GraphQuantityOverTime.GraphDataPointClinic GetDataPointForGrouping(GraphQuantityOverTime.GraphDataPointClinic x, GroupingOptionsCtrl.Grouping curGrouping)
        {
            switch (curGrouping)
            {
                case GroupingOptionsCtrl.Grouping.Provider:
                    return new GraphQuantityOverTime.GraphDataPointClinic()
                    {
                        DateStamp = x.DateStamp,
                        SeriesName = DashboardCache.Providers.GetProvName(x.ProvNum),
                        Val = x.Val,
                        Count = x.Count
                    };
                case GroupingOptionsCtrl.Grouping.Clinic:
                default:
                    return new GraphQuantityOverTime.GraphDataPointClinic()
                    {
                        DateStamp = x.DateStamp,
                        SeriesName = DashboardCache.Clinics.GetClinicName(x.ClinicNum),
                        Val = x.Val,
                        Count = x.Count
                    };
            }
        }

        private Color graph_OnGetGetColor(string seriesName)
        {
            if (OnGetSeriesColorOverride != null)
            {
                return OnGetSeriesColorOverride(this, CellType, seriesName);
            }

            switch (CellType)
            {
                case DashboardCellType.BrokenApptGraph:
                case DashboardCellType.NewPatientsGraph:
                case DashboardCellType.AccountsReceivableGraph:
                case DashboardCellType.IncomeGraph:
                case DashboardCellType.ProductionGraph:
                case DashboardCellType.NotDefined:
                default:
                    return DashboardCache.Providers.GetProvColor(seriesName);
            }
        }

        public DashboardDockContainer CreateDashboardDockContainer(TableBase dbItem = null)
        {
            var json = "";
            var ret = new DashboardDockContainer(
                this,
                Graph,
                CanEdit
                    ? new EventHandler((s, ea) =>
                    {
                        //Entering edit mode. 
                        //Set graph to loading mode to show the loading icon.
                        Graph.IsLoading = true;
                        //Spawn the cache thread(s) but don't block. 
                        //Register for OnThreadExitLocal will invoke back to this thread when all the threads have exited and refill the form.
                        DashboardCache.RefreshCellTypeIfInvalid(CellType, new DashboardFilter() {UseDateFilter = false}, false, false, OnThreadExitLocal);
                        //Allow filtering in edit mode.
                        ShowFilters = true;
                        //Save a copy of the current settings in case user clicks cancel.
                        json = GetFilterAndGraphSettings();
                    })
                    : null,
                (s, ea) =>
                {
                    //Ok click. Just hide the filters.
                    ShowFilters = false;
                },
                (s, ea) =>
                {
                    //Cancel click. Just hide the filters and reset to previous settings.
                    ShowFilters = false;
                    SetFilterAndGraphSettings(json);
                },
                (s, ea) =>
                {
                    //Spawn the init thread whenever this control gets dropped or dragged.
                    Init();
                },
                (s, ea) =>
                {
                    //Refresh button was clicked, spawn the init thread and force cache refresh.
                    Init(true);
                },
                dbItem);
            return ret;
        }

        public DashboardCellType GetCellType()
        {
            return CellType;
        }

        public class GraphQuantityOverTimeFilterSettings : ODGraphSettingsBase
        {
            public bool IncludeCompleteProcs { get; set; }
            public bool IncludeAdjustements { get; set; }
            public bool IncludeWriteoffs { get; set; }
            public bool IncludePaySplits { get; set; }
            public bool IncludeInsuranceClaims { get; set; }
            public GroupingOptionsCtrl.Grouping CurGrouping { get; set; }
            public BrokenApptGraphOptionsCtrl.RunFor CurRunFor { get; set; }
            public long AdjTypeDefNum { get; set; }
            public new BrokenApptProcedure BrokenApptProcCode { get; set; }
        }

        public GraphQuantityOverTimeFilterSettings GetGraphSettings()
        {
            switch (CellType)
            {
                case DashboardCellType.ProductionGraph:
                    return new GraphQuantityOverTimeFilterSettings()
                    {
                        IncludeAdjustements = _productionOptionsCtrl.IncludeAdjustments,
                        IncludeCompleteProcs = _productionOptionsCtrl.IncludeCompletedProcs,
                        IncludeWriteoffs = _productionOptionsCtrl.IncludeWriteoffs,
                        CurGrouping = CurGrouping,
                    };
                case DashboardCellType.IncomeGraph:
                    return new GraphQuantityOverTimeFilterSettings()
                    {
                        IncludePaySplits = _incomeOptionsCtrl.IncludePaySplits,
                        IncludeInsuranceClaims = _incomeOptionsCtrl.IncludeInsuranceClaimPayments,
                        CurGrouping = CurGrouping,
                    };
                case DashboardCellType.BrokenApptGraph:
                    return new GraphQuantityOverTimeFilterSettings()
                    {
                        CurGrouping = CurGrouping,
                        CurRunFor = _brokenApptsCtrl.CurRunFor,
                        AdjTypeDefNum = _brokenApptsCtrl.AdjTypeDefNumCur,
                        BrokenApptProcCode = _brokenApptsCtrl.BrokenApptCodeCur,
                    };
                case DashboardCellType.NewPatientsGraph:
                    return new GraphQuantityOverTimeFilterSettings()
                    {
                        CurGrouping = CurGrouping,
                    };
                case DashboardCellType.AccountsReceivableGraph:
                    //No custom filtering so do nothing.
                    return new GraphQuantityOverTimeFilterSettings();
                default:
                    throw new Exception("Unsupported CellType: " + CellType.ToString());
            }
        }

        public void DeserializeFromJson(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }

                var settings = ODGraphSettingsBase.Deserialize<GraphQuantityOverTimeFilterSettings>(json);
                switch (CellType)
                {
                    case DashboardCellType.ProductionGraph:
                        _productionOptionsCtrl.IncludeAdjustments = settings.IncludeAdjustements;
                        _productionOptionsCtrl.IncludeCompletedProcs = settings.IncludeCompleteProcs;
                        _productionOptionsCtrl.IncludeWriteoffs = settings.IncludeWriteoffs;
                        CurGrouping = settings.CurGrouping;
                        break;
                    case DashboardCellType.IncomeGraph:
                        _incomeOptionsCtrl.IncludePaySplits = settings.IncludePaySplits;
                        _incomeOptionsCtrl.IncludeInsuranceClaimPayments = settings.IncludeInsuranceClaims;
                        CurGrouping = settings.CurGrouping;
                        break;
                    case DashboardCellType.BrokenApptGraph:
                        CurGrouping = settings.CurGrouping;
                        _brokenApptsCtrl.CurRunFor = settings.CurRunFor;
                        _brokenApptsCtrl.AdjTypeDefNumCur = settings.AdjTypeDefNum;
                        _brokenApptsCtrl.BrokenApptCodeCur = settings.BrokenApptProcCode;
                        break;
                    case DashboardCellType.NewPatientsGraph:
                        CurGrouping = settings.CurGrouping;
                        break;
                    case DashboardCellType.AccountsReceivableGraph:
                        break;
                    default:
                        throw new Exception("Unsupported CellType: " + CellType.ToString());
                }
            }
            catch (Exception e)
            {
                if (ODBuild.IsDebug())
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void PrintPreview()
        {
            Graph.PrintPreview();
        }
    }
}