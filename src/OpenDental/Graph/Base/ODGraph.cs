using System;
using System.Drawing;
using Newtonsoft.Json;
using OpenDental.Graph.Cache;

namespace OpenDental.Graph.Base
{
    public interface IODGraphPrinter
    {
        void PrintPreview();
    }

    public class ODGraphLegendItem
    {
        private static long _idCounter;

        public long ID { get; private set; }
        public string ItemName { get; set; }
        public Color ItemColor { get; set; }
        public bool IsEnabled { get; set; }
        public Rectangle LocationBox { get; set; }
        public object Tag { get; set; }
        public double Val { get; set; }
        public bool Hovered { get; set; }

        public FilterLegend OnFilterLegend;

        public delegate void FilterLegend(ODGraphLegendItem legendItem);

        public void Filter()
        {
            OnFilterLegend?.Invoke(this);
        }

        public ODGraphLegendItem()
        {
            ID = _idCounter++;
        }
    }

    public class ODGraphSettingsBase
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public QuickRange QuickRangePref { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public DashboardFilter Filter => new DashboardFilter {DateTo = DateTo, DateFrom = DateFrom, UseDateFilter = QuickRangePref != QuickRange.allTime};

        public static ODGraphSettingsBase Deserialize(string json)
        {
            var ret = JsonConvert.DeserializeObject<ODGraphSettingsBase>(json);
            var filter = GetDatesFromQuickRange(ret.QuickRangePref, ret.DateFrom, ret.DateTo);
            ret.DateFrom = filter.DateFrom;
            ret.DateTo = filter.DateTo;
            return ret;
        }

        public static DashboardFilter GetDatesFromQuickRange(QuickRange quickRange, DateTime customDateFrom, DateTime customDateTo)
        {
            var filter = new DashboardFilter();
            switch (quickRange)
            {
                case QuickRange.custom:
                    filter.DateTo = customDateTo;
                    filter.DateFrom = customDateFrom;
                    break;
                case QuickRange.last7Days:
                    filter.DateTo = DateTime.Today;
                    filter.DateFrom = filter.DateTo.AddDays(-7);
                    break;
                case QuickRange.last30Days:
                    filter.DateTo = DateTime.Today;
                    filter.DateFrom = filter.DateTo.AddDays(-30);
                    break;
                case QuickRange.last365Days:
                    filter.DateTo = DateTime.Today;
                    filter.DateFrom = filter.DateTo.AddDays(-365);
                    break;
                case QuickRange.last12Months:
                    filter.DateTo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                    filter.DateFrom = filter.DateTo.AddMonths(-12).AddDays(1);
                    break;
                case QuickRange.previousWeek:
                    filter.DateFrom = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek).AddDays(-7);
                    filter.DateTo = filter.DateFrom.AddDays(7);
                    break;
                case QuickRange.previousMonth:
                    filter.DateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                    filter.DateTo = filter.DateFrom.AddMonths(1);
                    break;
                case QuickRange.previousYear:
                    filter.DateFrom = new DateTime(DateTime.Today.Year - 1, 1, 1);
                    filter.DateTo = filter.DateFrom.AddYears(1);
                    break;
                case QuickRange.thisWeek:
                    filter.DateFrom = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek);
                    filter.DateTo = filter.DateFrom.AddDays(7);
                    break;
                case QuickRange.thisMonth:
                    filter.DateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    filter.DateTo = filter.DateFrom.AddMonths(1);
                    break;
                case QuickRange.thisYear:
                    filter.DateFrom = new DateTime(DateTime.Today.Year, 1, 1);
                    filter.DateTo = filter.DateFrom.AddYears(1);
                    break;
                case QuickRange.weekToDate:
                    filter.DateFrom = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek);
                    filter.DateTo = DateTime.Today;
                    break;
                case QuickRange.monthToDate:
                    filter.DateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    filter.DateTo = DateTime.Today;
                    break;
                case QuickRange.yearToDate:
                    filter.DateFrom = new DateTime(DateTime.Today.Year, 1, 1);
                    filter.DateTo = DateTime.Today;
                    break;
                case QuickRange.allTime:
                    filter.DateFrom = new DateTime(1880, 1, 1);
                    filter.DateTo = filter.DateFrom.AddYears(300);
                    filter.UseDateFilter = false;
                    break;
                default:
                    throw new Exception("Unsupported QuickRange: " + quickRange.ToString());
            }

            return filter;
        }

        public static string Serialize(ODGraphSettingsBase obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public class ODGraphJson
    {
        public string GraphJson;
        public string FilterJson;

        public static string Serialize(ODGraphJson obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static ODGraphJson Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ODGraphJson>(json);
        }
    }

    public enum QuickRange
    {
        ///<summary>0</summary>
        allTime,

        ///<summary>1</summary>
        custom,

        ///<summary>2</summary>
        thisWeek,

        ///<summary>3</summary>
        weekToDate,

        ///<summary>4</summary>
        thisMonth,

        ///<summary>5</summary>
        monthToDate,

        ///<summary>6</summary>
        thisYear,

        ///<summary>7</summary>
        yearToDate,

        ///<summary>8</summary>
        previousWeek,

        ///<summary>9</summary>
        previousMonth,

        ///<summary>10</summary>
        previousYear,

        ///<summary>11</summary>
        last7Days,

        ///<summary>12</summary>
        last30Days,

        ///<summary>13</summary>
        last365Days,

        ///<summary>14</summary>
        last12Months,
    }

    public enum BreakdownType
    {
        ///<summary>0 - Show every series as it's own series. Do not group.</summary>
        all,

        ///<summary>1 - Show only 1 series as the group of all series combined.</summary>
        none,

        ///<summary>2 - Show top x items each in their own series where x is defined by BreakdownVal. All remaining series will be grouped as one series.</summary>
        items,

        ///<summary>3 - Show top x percentage of items each in their own series where x is defined by BreakdownVal. All remaining series will be grouped as one series.</summary>
        percent
    }

    public enum QuantityType
    {
        Money,
        Count,
        DecimalPoint
    }

    public enum LegendDockType
    {
        Bottom,
        Left,
        None
    }
}