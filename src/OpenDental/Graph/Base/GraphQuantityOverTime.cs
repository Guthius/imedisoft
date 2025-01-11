using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using OpenDental.Graph.Cache;
using OpenDental.Graph.Dashboard;
using DashboardCellType = OpenDentBusiness.DashboardCellType;

namespace OpenDental.Graph.Base
{
    public partial class GraphQuantityOverTime : UserControl, IODGraphPrinter
    {
        private readonly List<ODGraphLegendItem> _listHiddenLegendItems = new List<ODGraphLegendItem>();
        private readonly List<Tuple<DateTime, DateTime>> _intervals = new List<Tuple<DateTime, DateTime>>();
        private bool _isLoading = true;
        private bool _allowFilter;
        private string _graphTitle = "";
        private double _yAxisMaxVal = double.NaN;
        private double _yAxisMinVal = double.NaN;
        private Dictionary<DateTime, double> _maxCountingBuckets;
        private Dictionary<DateTime, double> _minCountingBuckets;
        private bool _isLayoutPending;
        private string _legendTitle = "";

        public delegate List<GraphPointBase> GetRawDataArgs();

        public GetRawDataArgs GetRawData;

        private DateTimeIntervalType SelectedMajorGridInterval => (DateTimeIntervalType) Enum.Parse(typeof(DateTimeIntervalType), GroupByType.ToString(), true);
        private ChartArea ChartAreaDefault => chart1.ChartAreas["Default"];

        private string _yAxisFormat = "D";

        private string XAxisFormat
        {
            get
            {
                switch (GroupByType)
                {
                    case IntervalType.Years:
                        return "\\'yy";

                    case IntervalType.Months:
                        return "MMM \\'yy";

                    case IntervalType.Weeks:
                    case IntervalType.Days:
                        if (DateRange.TotalDays < 365)
                        {
                            return "MM/dd";
                        }

                        break;
                }

                return "MM/dd/yy";
            }
        }

        public class GraphDataPointClinic : GraphDataPointProv
        {
            public long ClinicNum;
        }

        public class GraphDataPointProv : GraphPointBase
        {
            public long ProvNum;
        }

        public class GraphPointBase : ODGraphPointBase
        {
            [JsonProperty(PropertyName = "DS")]
            public DateTime DateStamp;

            public double Val;
            public long Count;

            public GraphPointBase()
            {
            }

            public GraphPointBase(GraphPointBase dataPoint)
            {
                SeriesName = dataPoint.SeriesName;
                DateStamp = dataPoint.DateStamp;
                Val = dataPoint.Val;
                Count = dataPoint.Count;
                Tag = dataPoint.Tag;
            }

            public override string ToString()
            {
                return SeriesName + " " + DateStamp.ToShortDateString() + " - " + Val;
            }
        }

        public class ODGraphPointBase
        {
            public string SeriesName { get; set; }
            public object Tag { get; set; }
        }

        [Description("Show or hide the animated 'Loading' gif.")]
        [Category("Graph")]
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                if (IsLoading)
                {
                    pictureBoxLoading.BringToFront();
                    chart1.SendToBack();
                }
                else
                {
                    chart1.BringToFront();
                    pictureBoxLoading.SendToBack();
                }
            }
        }

        [Description("Used in main chart title, breakdown group box, breakdown combo box and legend.")]
        [Category("Graph")]
        public string GraphTitle
        {
            get => _graphTitle;
            set
            {
                if (_graphTitle == value)
                {
                    return;
                }

                _graphTitle = value;
                SetGraphTitles();
            }
        }

        [Description("Used in legend title and also to describe series grouping.")]
        [Category("Graph")]
        public string LegendTitle
        {
            get => _legendTitle;
            set
            {
                _legendTitle = value;
                SetGraphTitles();
            }
        }

        [Description("Sub-text below main chart area. If empty then sub-text is hidden.")]
        [Category("Graph")]
        public string ChartSubTitle
        {
            get => chart1.Titles["ChartSubTitle"].Text;
            set
            {
                chart1.Titles["ChartSubTitle"].Text = value;
                chart1.Titles["ChartSubTitle"].Visible = !string.IsNullOrEmpty(value);
            }
        }

        [Description("String shown for Values As = 'Money'.")]
        [Category("Graph")]
        public string MoneyItemDescription
        {
            get => comboQuantityType.GetItem(QuantityType.Money).Display;
            set => comboQuantityType.GetItem(QuantityType.Money).Display = value;
        }

        [Description("String shown for Values As = 'Count'.")]
        [Category("Graph")]
        public string CountItemDescription
        {
            get => comboQuantityType.GetItem(QuantityType.Count).Display;
            set => comboQuantityType.GetItem(QuantityType.Count).Display = value;
        }

        [Description("Show Filters")]
        [Category("Graph")]
        public bool ShowFilters
        {
            get => !splitContainerMain.Panel1Collapsed;
            set => splitContainerMain.Panel1Collapsed = !value;
        }

        [Description("Gets or sets the Chart Title's Font")]
        [Category("Graph")]
        public Font TitleFont
        {
            get => chart1.Titles["ChartTitle"].Font;
            set => chart1.Titles["ChartTitle"].Font = value;
        }

        [Description("Controls the 'Display' combo box. Change value type used on y-axis.")]
        [Category("Graph")]
        public QuantityType QtyType
        {
            get => comboQuantityType.GetValue<QuantityType>();
            set => comboQuantityType.SetItem(value);
        }

        [Description("Controls the 'Series Type' combo box. Change the way each series is displayed on the chart.")]
        [Category("Graph")]
        public SeriesChartType SeriesType
        {
            get => comboChartType.GetValue<SeriesChartType>();
            set => comboChartType.SetItem(value);
        }

        [Description("Controls the 'Group By' combo box. Change date groupings.")]
        [Category("Graph")]
        public IntervalType GroupByType
        {
            get => comboGroupBy.GetValue<IntervalType>();
            set => comboGroupBy.SetItem(value);
        }

        [Description("Controls the 'Legend' combo box. Gets or sets Legend Docking")]
        [Category("Graph")]
        public LegendDockType LegendDock
        {
            get => chartLegend1.LegendDock;
            set
            {
                _isLayoutPending = true;
                chartLegend1.LegendDock = value;
                LayoutChartContainer();
                comboLegendDock.SelectedIndex = (int) value;
                _isLayoutPending = false;
            }
        }

        [Description("Controls the 'Quick Range' combo box. Gets or sets date range to include.")]
        [Category("Graph")]
        public QuickRange QuickRangePref
        {
            get => comboQuickRange.GetValue<QuickRange>();
            set => comboQuickRange.SetItem(value);
        }

        [Description("Controls the 'Top Value' numeric control. Gets or sets number of series to include.")]
        [Category("Graph")]
        public int BreakdownVal
        {
            get => (int) numericTop.Value;
            set => numericTop.Value = Math.Min(Math.Max(value, 1), 100);
        }

        [Description("Controls the 'Show Breakdown For' group box. Gets or sets number of series to include.")]
        [Category("Graph")]
        public BreakdownType BreakdownPref
        {
            get
            {
                if (radBreakdownAll.Checked)
                {
                    return BreakdownType.all;
                }

                return radBreakdownNone.Checked ? BreakdownType.none : comboBreakdownBy.GetValue<BreakdownType>();
            }
            set
            {
                switch (value)
                {
                    case BreakdownType.all:
                        radBreakdownAll.Checked = true;
                        break;

                    case BreakdownType.none:
                        radBreakdownNone.Checked = true;
                        break;

                    case BreakdownType.items:
                    case BreakdownType.percent:
                        radBreakdownTop.Checked = true;
                        radBreakdownTop.Checked = true;
                        comboBreakdownBy.SetItem(value);
                        break;
                }
            }
        }

        [Description("Controls the 'From' filter date.")]
        [Category("Graph")]
        public DateTime DateFrom
        {
            get => dateTimeFrom.Value;
            set
            {
                if (value.Year >= 1880)
                {
                    dateTimeFrom.Value = value;
                }
            }
        }

        [Description("Controls the 'To' filter date.")]
        [Category("Graph")]
        public DateTime DateTo
        {
            get => dateTimeTo.Value;
            set
            {
                if (value.Year > 1880)
                {
                    dateTimeTo.Value = value;
                }
            }
        }

        [Description("If true then built-in color palette will be used. If false then value returned by OnGetGetColor will be used for each series.")]
        [Category("Graph")]
        public bool UseBuiltInColors { get; set; }

        [Description("Total absolute value duration between DateFrom and DateTo.")]
        [Category("Graph")]
        public TimeSpan DateRange => DateFrom.Subtract(DateTo).Duration();

        public DashboardFilter Filter => ODGraphSettingsBase.GetDatesFromQuickRange(QuickRangePref, DateFrom, DateTo);

        public GraphQuantityOverTime()
        {
            InitializeComponent();

            comboChartType.SetDataToEnumsPrimitive(new List<SeriesChartType>(new[] {SeriesChartType.StackedArea, SeriesChartType.StackedColumn, SeriesChartType.Column, SeriesChartType.Line}));
            comboGroupBy.SetDataToEnumsPrimitive<IntervalType>((int) IntervalType.Years, (int) IntervalType.Days);
            comboBreakdownBy.SetDataToEnumsPrimitive((int) BreakdownType.items, (int) BreakdownType.percent, new ComboBoxEx.StringFromEnumArgs<BreakdownType>(bt =>
            {
                switch (bt)
                {
                    case BreakdownType.percent:
                        return "Percent";
                    case BreakdownType.items: //Taken care of in SetGraphTitles().					
                    case BreakdownType.all: //Not included.
                    case BreakdownType.none: //Not included.
                    default:
                        return bt.ToString();
                }
            }));

            comboQuantityType.SetDataToEnumsPrimitive<QuantityType>((int) QuantityType.Money, (int) QuantityType.DecimalPoint);
            comboLegendDock.SetDataToEnumsPrimitive<LegendDockType>();
            comboQuickRange.SetDataToEnumsPrimitive(new ComboBoxEx.StringFromEnumArgs<QuickRange>(qr =>
            {
                return qr switch
                {
                    QuickRange.allTime => "All Time",
                    QuickRange.thisWeek => "This Week",
                    QuickRange.thisMonth => "This Month",
                    QuickRange.thisYear => "This Year",
                    QuickRange.weekToDate => "Week To Date",
                    QuickRange.monthToDate => "Month To Date",
                    QuickRange.yearToDate => "Year To Date",
                    QuickRange.previousWeek => "Previous Week",
                    QuickRange.previousMonth => "Previous Month",
                    QuickRange.previousYear => "Previous Year",
                    QuickRange.last7Days => "Last 7 Days",
                    QuickRange.last30Days => "Last 30 Days",
                    QuickRange.last365Days => "Last 365 Days",
                    QuickRange.last12Months => "Last 12 Months",
                    QuickRange.custom => "Custom",
                    _ => "N\\A"
                };
            }));

            OnFilterLegend += legendItem =>
            {
                if (legendItem.IsEnabled)
                {
                    _listHiddenLegendItems.RemoveAll(x => x.ID == legendItem.ID);
                }
                else if (!_listHiddenLegendItems.Exists(x => x.ID == legendItem.ID))
                {
                    _listHiddenLegendItems.Add(legendItem);
                }

                FilterData(this, EventArgs.Empty);
            };
        }

        public delegate void ODGraphDataPointEventHandler(object sender, ODGraphPointBase dataPoint);

        public delegate Color OnGetColorArgs(string seriesName);

        public delegate Color OnGetColorFromSeriesGraphTypeArgs(object sender, DashboardCellType cellType, string seriesName);
        
        public OnGetColorArgs OnGetGetColor;

        public readonly ODGraphLegendItem.FilterLegend OnFilterLegend;

        private void radBreakdown_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton) sender).Checked)
            {
                return;
            }

            FilterData(sender, e);
        }

        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            switch (e.HitTestResult.ChartElementType)
            {
                case ChartElementType.DataPoint:
                    var series = e.HitTestResult.Series;
                    e.Text = series.Name + " - " + DateTime.FromOADate(series.Points[e.HitTestResult.PointIndex].XValue).ToShortDateString() + " - ";
                    if (QtyType == QuantityType.Money)
                    {
                        e.Text += Math.Round(series.Points[e.HitTestResult.PointIndex].YValues[0], 2).ToString("c");
                    }
                    else
                    {
                        e.Text += Math.Round(series.Points[e.HitTestResult.PointIndex].YValues[0], 2);
                    }

                    break;
            }
        }

        private void comboQuickRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            _allowFilter = false;
            if (QuickRangePref == QuickRange.custom)
            {
                dateTimeTo.Enabled = true;
                dateTimeFrom.Enabled = true;
            }
            else
            {
                dateTimeTo.Enabled = false;
                dateTimeFrom.Enabled = false;
                var filter = Filter;
                DateFrom = filter.DateFrom;
                DateTo = filter.DateTo;
            }

            _allowFilter = true;

            FilterData(sender, e);
        }

        private void comboLegendDock_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLayoutPending)
            {
                return;
            }

            LegendDock = comboLegendDock.GetValue<LegendDockType>();
        }

        private void ChartQuantityOverTime_Resize(object sender, EventArgs e)
        {
            SetXAxisLabelDensity();
        }

        private void textChartTitle_TextChanged(object sender, EventArgs e)
        {
            GraphTitle = textChartTitle.Text;
        }

        private Color AddSeries(string name, Dictionary<DateTime, double> filteredData, object tag)
        {
            var series = new Series(name);
            series.Tag = tag;
            series.ChartArea = ChartAreaDefault.Name;
            series.ChartType = SeriesType;
            var color = Color.Empty;
            if (!UseBuiltInColors && OnGetGetColor != null)
            {
                color = OnGetGetColor(name);
            }

            if (color == Color.Empty)
            {
                color = GenerateSeriesColor(name);
            }

            if (!UseBuiltInColors)
            {
                series.Color = color;
            }

            series.Points.DataBindXY(filteredData.Keys, filteredData.Values);
            chart1.Series.Add(series);

            #region y-axis calc

            //get the max and min y- values.
            switch (SeriesType)
            {
                //if it's a Stacked Column or Area, we want to look at the overall series' values.
                //this breaks down a little bit with negative values in the StackedArea graph, but it's fine because it's a weird graph anyway.
                case SeriesChartType.StackedArea:
                case SeriesChartType.StackedColumn:
                    foreach (var kvp in filteredData)
                    {
                        if (kvp.Value > 0)
                        {
                            _maxCountingBuckets[kvp.Key] += kvp.Value;
                        }
                        else
                        {
                            _minCountingBuckets[kvp.Key] += kvp.Value;
                        }
                    }

                    _yAxisMaxVal = Math.Max(_yAxisMaxVal, _maxCountingBuckets.Values.Max());
                    _yAxisMinVal = Math.Min(_yAxisMinVal, _minCountingBuckets.Values.Min());
                    break;
                case SeriesChartType.Column:
                case SeriesChartType.Line:
                //if it's a column or line (non-stacked), we want to look at the individual series' that are showing.
                default:
                    _yAxisMaxVal = Math.Max(_yAxisMaxVal, filteredData.Values.Max());
                    _yAxisMinVal = Math.Min(_yAxisMinVal, filteredData.Values.Min());
                    break;
            }

            #endregion

            return series.Color;
        }

        public static Color GenerateSeriesColor(string seriesName)
        {
            var r = seriesName.Sum(x => x.GetHashCode()) % 255;
            var g = (int) (Math.Log(seriesName.Length) * seriesName.Length * seriesName.Length) % 255;
            var b = seriesName[0].GetHashCode() * seriesName[seriesName.Length - 1].GetHashCode() % 255;
            return Color.FromArgb(r, g, b);
        }

        private void FilterData(object sender, EventArgs e)
        {
            if (!_allowFilter)
            {
                return;
            }

            switch (BreakdownPref)
            {
                case BreakdownType.all:
                case BreakdownType.none:
                    numericTop.Enabled = false;
                    comboBreakdownBy.Enabled = false;
                    break;

                case BreakdownType.items:
                case BreakdownType.percent:
                    numericTop.Enabled = true;
                    comboBreakdownBy.Enabled = true;
                    break;
            }

            var rawData = GetRawData?.Invoke() ?? new List<GraphPointBase>();

            chart1.Series.Clear();
            if (rawData.Count <= 0)
            {
                return;
            }

            List<GraphPointBase> rawDataLocal;
            switch (QtyType)
            {
                case QuantityType.Count:
                    rawDataLocal = rawData.Select(x => new GraphPointBase(x) {Val = x.Count}).ToList();
                    break;

                case QuantityType.DecimalPoint:
                case QuantityType.Money:
                    rawDataLocal = rawData.Select(x => new GraphPointBase(x)).ToList();
                    break;

                default:
                    throw new Exception("Unsupported QtyType.");
            }

            rawDataLocal.RemoveAll(x => _listHiddenLegendItems.Exists(y => y.ItemName == x.SeriesName));

            var seriesNames = rawDataLocal.Select(x => x.SeriesName).Distinct().ToList();
            var seriesTags = rawDataLocal.GroupBy(x => x.SeriesName).ToDictionary(x => x.Key, x => x.First().Tag);
            var rawDataFormatted = seriesNames
                .Select(x => new
                {
                    SeriesName = x,
                    Dict = new Dictionary<DateTime, double>(), Tag = seriesTags[x]
                })
                .ToDictionary(x => x.SeriesName, x => x);

            _intervals.Clear();

            var thisDate = DateFrom;
            var toDate = DateTo;

            if (QuickRangePref == QuickRange.allTime)
            {
                if (rawDataLocal.Count > 0)
                {
                    thisDate = rawDataLocal.Min(x => x.DateStamp).Date;
                    toDate = rawDataLocal.Max(x => x.DateStamp).Date;
                    _allowFilter = false;
                    DateFrom = thisDate;
                    DateTo = toDate;
                    _allowFilter = true;
                }
            }

            //Move start and end date according to filter and grouping selections.
            switch (GroupByType)
            {
                case IntervalType.Weeks:
                    //Start at Sunday before lower bound date.
                    thisDate = thisDate.AddDays(-(int) thisDate.DayOfWeek);
                    //End at first Sunday following the upper bound date.						
                    toDate = toDate.AddDays((7 - (int) toDate.DayOfWeek) % 7);
                    break;
                case IntervalType.Months:
                    //Start at 1st of lower bound month.
                    thisDate = new DateTime(thisDate.Year, thisDate.Month, 1);
                    //Send at 1st of month after upper bound month.
                    toDate = new DateTime(toDate.Year, toDate.Month, 1).AddMonths(1);
                    break;
                case IntervalType.Years:
                    //Start at 1st of lower bound month.
                    thisDate = new DateTime(thisDate.Year, 1, 1);
                    //Send at 1st of month after upper bound month.
                    toDate = new DateTime(toDate.Year, 1, 1).AddYears(1);
                    break;
                case IntervalType.Days:
                default:
                    //Start at day after lower bound date.
                    toDate = toDate.AddDays(1);
                    //End at upper bound date (no change).
                    break;
            }

            //Create timespan intervals according to selected grouping.
            while (thisDate < toDate)
            {
                switch (GroupByType)
                {
                    case IntervalType.Weeks:
                        _intervals.Add(new Tuple<DateTime, DateTime>(thisDate, thisDate.AddDays(7)));
                        thisDate = thisDate.AddDays(7);
                        break;
                    case IntervalType.Months:
                        _intervals.Add(new Tuple<DateTime, DateTime>(thisDate, thisDate.AddMonths(1)));
                        thisDate = thisDate.AddMonths(1);
                        break;
                    case IntervalType.Years:
                        _intervals.Add(new Tuple<DateTime, DateTime>(thisDate, thisDate.AddYears(1)));
                        thisDate = thisDate.AddYears(1);
                        break;
                    case IntervalType.Days:
                    default:
                        _intervals.Add(new Tuple<DateTime, DateTime>(thisDate, thisDate.AddDays(1)));
                        thisDate = thisDate.AddDays(1);
                        break;
                }
            }

            if (_intervals.Count <= 0)
            {
                return;
            }

            //Initialize each interval in each series to 0. We will set actual values below. This ensures that our omitted dates are zeroed.
            foreach (var kvp in rawDataFormatted)
            {
                _intervals.ForEach(y => kvp.Value.Dict[y.Item1] = 0);
            }

            //Each interval is given a bucket and each bucket is the sum of all y-values at the given x-value interval. 
            //This will be used to determine max y-axis value for the entire chart.
            _maxCountingBuckets = new Dictionary<DateTime, double>();
            _minCountingBuckets = new Dictionary<DateTime, double>();
            _intervals.ForEach(y =>
            {
                _maxCountingBuckets[y.Item1] = 0;
                _minCountingBuckets[y.Item1] = 0;
                foreach (var kvp in rawDataFormatted)
                {
                    kvp.Value.Dict[y.Item1] = 0;
                }
            });
            //Now that we have established intervals, we can project the raw data into grouped DataPoints.
            //The sum of all the DataPoints per a given group will be added to that single group.
            List<GraphPointBase> groupedData;
            switch (GroupByType)
            {
                case IntervalType.Weeks:
                    groupedData = rawDataLocal
                        .FindAll(x => x.DateStamp >= _intervals[0].Item1 && x.DateStamp < _intervals[_intervals.Count - 1].Item2)
                        .Select(x =>
                            new GraphPointBase()
                            {
                                SeriesName = x.SeriesName,
                                DateStamp = x.DateStamp.Date.AddDays(-(int) x.DateStamp.DayOfWeek),
                                Tag = x.Tag,
                                Val = x.Val
                            }).ToList();
                    break;
                case IntervalType.Months:
                    groupedData = rawDataLocal
                        .FindAll(x => x.DateStamp >= _intervals[0].Item1 && x.DateStamp < _intervals[_intervals.Count - 1].Item2)
                        .Select(x =>
                            new GraphPointBase()
                            {
                                SeriesName = x.SeriesName,
                                DateStamp = new DateTime(x.DateStamp.Year, x.DateStamp.Month, 1),
                                Tag = x.Tag,
                                Val = x.Val
                            }).ToList();
                    break;
                case IntervalType.Years:
                    groupedData = rawDataLocal
                        .FindAll(x => x.DateStamp >= _intervals[0].Item1 && x.DateStamp < _intervals[_intervals.Count - 1].Item2)
                        .Select(x =>
                            new GraphPointBase()
                            {
                                SeriesName = x.SeriesName,
                                DateStamp = new DateTime(x.DateStamp.Year, 1, 1),
                                Tag = x.Tag,
                                Val = x.Val
                            }).ToList();
                    break;
                case IntervalType.Days:
                default:
                    groupedData = rawDataLocal
                        .FindAll(x => x.DateStamp >= _intervals[0].Item1 && x.DateStamp < _intervals[_intervals.Count - 1].Item2)
                        .Select(x =>
                            new GraphPointBase()
                            {
                                SeriesName = x.SeriesName,
                                DateStamp = x.DateStamp.Date,
                                Tag = x.Tag,
                                Val = x.Val
                            }).ToList();
                    break;
            }

            groupedData.ForEach(x => { rawDataFormatted[x.SeriesName].Dict[x.DateStamp] += x.Val; });
            //Get the max and min y-value for the chart. The actual calculations for this are done in AddSeries().
            _yAxisMaxVal = 0;
            _yAxisMinVal = 0;
            //Filter out by selected _breakdownType.
            var topGrossingCount = rawDataFormatted.Count;
            if (BreakdownPref == BreakdownType.items)
            {
                //Order descending and take top x items.
                topGrossingCount = (int) numericTop.Value;
            }
            else if (BreakdownPref == BreakdownType.percent)
            {
                //Order descending and take top by percentage.				
                topGrossingCount = (int) Math.Ceiling(rawDataFormatted.Count * (numericTop.Value / 100));
            }
            else if (BreakdownPref == BreakdownType.none)
            {
                //Don't take any. This will leave all to be grouped into 1 single group below.
                topGrossingCount = 0;
            }

            //Subtract the number of hidden items ("All" and "All Other" don't count as items yet) so the graph doesn't add another series to make up for the disabled one.
            //Don't do this for breakdown type of "All" - all legend items should be showing.
            if (BreakdownPref != BreakdownType.all)
            {
                topGrossingCount -= _listHiddenLegendItems.FindAll(x => x.ItemName != "All" && x.ItemName != "All Other").Count();
            }

            if (topGrossingCount < 0)
            {
                //MessageBox.Show(OpenDentBusiness.Lans.g("OpenDentalGraph","Not allowed to disable the last series."));
                //I mean, sure, if they want to disable every single series in the graph, who am I to stop them?
                chartLegend1.SetLegendItems(_listHiddenLegendItems);
                return;
            }

            var listLegendItems = new List<ODGraphLegendItem>(_listHiddenLegendItems);
            //Limit to 100 series. More than that and it becomes illegible and takes too long to draw.
            topGrossingCount = Math.Min(topGrossingCount, 100);
            //Add each individual series that is within the filtered range.
            rawDataFormatted.OrderByDescending(x => x.Value.Dict.Sum(y => y.Value))
                .Take(topGrossingCount).ToList()
                .ForEach(x =>
                {
                    listLegendItems.Add(new ODGraphLegendItem
                    {
                        IsEnabled = true,
                        ItemColor = AddSeries(x.Key, x.Value.Dict, x.Value.Tag),
                        ItemName = x.Key,
                        Tag = x,
                        Val = x.Value.Dict.Sum(y => y.Value),
                        OnFilterLegend = OnFilterLegend,
                    });
                });
            //If necessary, add 1 single series to sum up all remaining series.
            if (!_listHiddenLegendItems.Exists(x => x.ItemName == "All" || x.ItemName == "All Other"))
            {
                if (rawDataFormatted.Count > topGrossingCount)
                {
                    var item = rawDataFormatted.OrderByDescending(x => x.Value.Dict.Sum(y => y.Value))
                        .ElementAt(topGrossingCount);
                    rawDataFormatted.OrderByDescending(x => x.Value.Dict.Sum(y => y.Value))
                        .Skip(topGrossingCount + 1).ToList()
                        .ForEach(x =>
                        {
                            foreach (var kvp in x.Value.Dict)
                            {
                                item.Value.Dict[kvp.Key] += kvp.Value;
                            }
                        });
                    //This is the 'All Other' series, everything that wasn't included individually above will be grouped into 1 single series.
                    //Don't add it if it was disabled by the user.
                    listLegendItems.Add(new ODGraphLegendItem
                    {
                        IsEnabled = true,
                        ItemColor = AddSeries(BreakdownPref == BreakdownType.none ? "All" : "All Other", item.Value.Dict, new object()),
                        ItemName = BreakdownPref == BreakdownType.none ? "All" : "All Other",
                        Tag = new object(),
                        Val = double.MinValue, //so that it always shows at the end.
                        OnFilterLegend = OnFilterLegend,
                    });
                }
            }

            ChartAreaDefault.AxisX.IntervalType = SelectedMajorGridInterval;
            //Allowing MSChart to calculate AxisY.Maximum (via RecalculateAxesScale) causes a second rendering of the chart which slows it down significantly.
            //Round up.
            _yAxisMaxVal = _yAxisMaxVal.RoundSignificant();
            ChartAreaDefault.AxisX.IntervalOffsetType = SelectedMajorGridInterval;
            ChartAreaDefault.AxisX.LabelStyle.Format = XAxisFormat;
            if (_yAxisMaxVal <= 0)
            {
                _yAxisMaxVal = 0;
            }
            else if (_yAxisMaxVal < 10)
            {
                _yAxisMaxVal = 10;
            }

            _yAxisMinVal = _yAxisMinVal.RoundSignificant();
            _yAxisMinVal = _yAxisMinVal >= 0 ? 0 : _yAxisMinVal;
            var yAxisMinMax = Math.Max(Math.Abs(_yAxisMaxVal), Math.Abs(_yAxisMinVal));
            if (yAxisMinMax == 0)
            {
                _yAxisFormat = "D";
            }
            else
            {
                var max = (int) yAxisMinMax;
                var scaleFactor = "".PadRight((max.ToString().Length - 2) / 3, ',');
                var factors = new[] {"", "K", "M", "B", "T", "?"};
                if (yAxisMinMax >= 10)
                {
                    _yAxisFormat = "{0:0" + scaleFactor + "}" + factors[Math.Min(factors.Length - 1, scaleFactor.Length)];
                }
                else
                {
                    _yAxisFormat = QtyType == QuantityType.Count ? "{0:0}" : "{0:0.0}";
                }

                if (QtyType == QuantityType.Money)
                {
                    _yAxisFormat = "$" + _yAxisFormat;
                }
            }

            ChartAreaDefault.AxisY.LabelStyle.Format = _yAxisFormat;
            var doRecalc = false;
            ChartAreaDefault.AxisX.IsMarginVisible = SeriesType == SeriesChartType.Column || SeriesType == SeriesChartType.StackedColumn;
            if (_yAxisMaxVal != ChartAreaDefault.AxisY.Maximum)
            {
                //Only set when this has changed to prevent extra rendering.
                ChartAreaDefault.AxisY.Maximum = _yAxisMaxVal;
                doRecalc = true;
            }

            if (_yAxisMinVal != ChartAreaDefault.AxisY.Minimum)
            {
                //Only set when this has changed to prevent extra rendering.
                ChartAreaDefault.AxisY.Minimum = _yAxisMinVal;
                doRecalc = true;
            }

            if (doRecalc)
            {
                //Only recalc when necessary to prevent extra rendering.
                ChartAreaDefault.RecalculateAxesScale();
            }

            if (_yAxisMinVal != 0 || _yAxisMaxVal != 0)
            {
                ChartAreaDefault.AxisX.ScaleView.ZoomReset();
            }

            SetXAxisLabelDensity();
            if (chart1.Series.Count == 1 && (UseBuiltInColors || BreakdownPref == BreakdownType.none))
            {
                //The default single series color is purple. Set it to something more appealing.
                chart1.Series[0].Color = Color.DodgerBlue;
            }

            //We supposedly already set each series' color above, but just in case, apply default palette color to any series that was missed.
            chart1.ApplyPaletteColors();
            //Set missing legend colors. It's not likely that we will have any but just in case.
            listLegendItems.ForEach(x =>
            {
                var sc = chart1.Series.FirstOrDefault(y => y.Name == x.ItemName);
                if (sc != null)
                {
                    //if it's null, then it must be a hidden legend item whose colour has already been calculated
                    x.ItemColor = sc.Color;
                }
            });
            listLegendItems = listLegendItems.OrderByDescending(x => x.Val).ThenBy(x => x.ItemName).ToList();
            chartLegend1.SetLegendItems(listLegendItems);
        }

        private void SetXAxisLabelDensity()
        {
            if (_intervals.Count <= 0 || Width <= 0)
            {
                return;
            }

            double xInterval = _intervals.Count / Math.Min(_intervals.Count, _intervals.Count / (int) Math.Ceiling(_intervals.Count / (Width / 30f)));
            
            ChartAreaDefault.AxisX.Interval = Math.Round(xInterval);
            ChartAreaDefault.AxisX.IntervalOffset = 0;
        }

        public void RemoveQuantityType(QuantityType qtyType)
        {
            comboQuantityType.RemoveItem(qtyType);
        }

        public void InsertQuantityType(QuantityType qtyType, string displayName, int index)
        {
            comboQuantityType.InsertItem(qtyType, displayName, index);
        }

        private void SetGraphTitles()
        {
            try
            {
                chart1.Titles["ChartTitle"].Text = GraphTitle;
                textChartTitle.Text = GraphTitle;
                comboBreakdownBy.UpdateDisplayName(BreakdownType.items, LegendTitle + "s");
            }
            catch
            {
                // ignored
            }
        }

        public const int WM_WINDOWPOSCHANGED = 0x47;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_WINDOWPOSCHANGED || LegendDock != LegendDockType.Bottom)
            {
                return;
            }

            var newSplitterDistance = Math.Max(splitContainerChart.Height - 30, 1);
            if (splitContainerChart.SplitterDistance != newSplitterDistance)
            {
                splitContainerChart.SplitterDistance = newSplitterDistance;
            }
        }

        private void LayoutChartContainer()
        {
            switch (LegendDock)
            {
                case LegendDockType.Bottom:
                    splitContainerChart.Panel1Collapsed = false;
                    splitContainerChart.Orientation = Orientation.Horizontal;
                    splitContainerChart.FixedPanel = FixedPanel.Panel2;
                    splitContainerChart.SplitterDistance = Math.Max(splitContainerChart.Height - 30, 1);
                    splitContainerChart.Panel1.Controls.Clear();
                    splitContainerChart.Panel2.Controls.Clear();
                    splitContainerChart.Panel1.Controls.Add(panelChart);
                    splitContainerChart.Panel2.Controls.Add(chartLegend1);
                    break;

                case LegendDockType.Left:
                    splitContainerChart.Panel1Collapsed = false;
                    splitContainerChart.Orientation = Orientation.Vertical;
                    splitContainerChart.FixedPanel = FixedPanel.Panel1;
                    splitContainerChart.SplitterDistance = 120;
                    splitContainerChart.Panel1.Controls.Clear();
                    splitContainerChart.Panel2.Controls.Clear();
                    splitContainerChart.Panel1.Controls.Add(chartLegend1);
                    splitContainerChart.Panel2.Controls.Add(panelChart);
                    break;

                case LegendDockType.None:
                    splitContainerChart.Panel1Collapsed = true;
                    splitContainerChart.Panel1.Controls.Clear();
                    splitContainerChart.Panel2.Controls.Clear();
                    splitContainerChart.Panel2.Controls.Add(panelChart);
                    break;
            }
        }

        public void PrintPreview()
        {
            using var formPrintSettings = new FormPrintSettings(chart1, chartLegend1);

            formPrintSettings.ShowDialog();
        }

        public Chart GetChartForPrinting()
        {
            return chart1;
        }

        public void TriggerGetData(object sender)
        {
            if (IsLoading)
            {
                return;
            }

            FilterData(sender, EventArgs.Empty);
        }

        public QuantityOverTimeGraphSettings GetGraphSettings()
        {
            return new QuantityOverTimeGraphSettings()
            {
                QtyType = QtyType,
                SeriesType = SeriesType,
                GroupByType = GroupByType,
                LegendDock = LegendDock,
                QuickRangePref = QuickRangePref,
                BreakdownPref = BreakdownPref,
                BreakdownVal = BreakdownVal,
                DateFrom = DateFrom,
                DateTo = DateTo,
                Title = GraphTitle,
                SubTitle = ChartSubTitle,
            };
        }

        public string SerializeToJson()
        {
            return ODGraphSettingsBase.Serialize(GetGraphSettings());
        }

        public void DeserializeFromJson(string json)
        {
            try
            {
                var settings = ODGraphSettingsBase.Deserialize<QuantityOverTimeGraphSettings>(json);
                QtyType = settings.QtyType;
                SeriesType = settings.SeriesType;
                GroupByType = settings.GroupByType;
                LegendDock = settings.LegendDock;
                BreakdownPref = settings.BreakdownPref;
                BreakdownVal = settings.BreakdownVal;
                GraphTitle = settings.Title;
                ChartSubTitle = settings.SubTitle;
                //Important that quick range is set last to prevent FilterData from firing multiple times.
                QuickRangePref = settings.QuickRangePref;
                //we should always recreate the dates so that they are current.
                var filter = ODGraphSettingsBase.GetDatesFromQuickRange(settings.QuickRangePref, settings.DateFrom, settings.DateTo);
                DateFrom = filter.DateFrom;
                DateTo = filter.DateTo;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public class QuantityOverTimeGraphSettings : ODGraphSettingsBase
        {
            public QuantityType QtyType { get; set; }
            public SeriesChartType SeriesType { get; set; }
            public IntervalType GroupByType { get; set; }
            public LegendDockType LegendDock { get; set; }
            public BreakdownType BreakdownPref { get; set; }
            public int BreakdownVal { get; set; }
        }
    }
}