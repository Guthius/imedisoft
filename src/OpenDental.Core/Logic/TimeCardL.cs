using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TimeCardL
{
    public static DateTime GetStartOfWeek(DateTime dateTime)
    {
        var weekOfYear = GetWeekOfYear(dateTime.Date);
        var dateTimeStartOfWeek = new DateTime(dateTime.Ticks);

        for (var i = 1; i < 7; i++)
        {
            var weekOfYearPrevious = GetWeekOfYear(dateTime.AddDays(-i));
            if (weekOfYear != weekOfYearPrevious)
            {
                return dateTimeStartOfWeek;
            }

            dateTimeStartOfWeek = dateTime.AddDays(-i);
        }

        return dateTimeStartOfWeek;
    }

    public static DateTime GetEndOfWeekForOvertime(DateTime dateTime)
    {
        var timeCardOvertimeFirstDayOfWeek = (DayOfWeek) PrefC.GetInt(PrefName.TimeCardOvertimeFirstDayOfWeek);

        DayOfWeek dayOfWeekComplete;
        if (timeCardOvertimeFirstDayOfWeek == DayOfWeek.Sunday)
        {
            dayOfWeekComplete = DayOfWeek.Saturday;
        }
        else
        {
            var dayOfWeekCompleteWeek = (int) timeCardOvertimeFirstDayOfWeek - 1;

            dayOfWeekComplete = (DayOfWeek) dayOfWeekCompleteWeek;
        }

        for (var i = 0; i < 7; i++)
        {
            var dateTimeEndOfCompleteWeek = dateTime.AddDays(-i);

            if (dateTimeEndOfCompleteWeek.DayOfWeek == dayOfWeekComplete)
            {
                return dateTimeEndOfCompleteWeek;
            }
        }

        throw new ODException("End of week could not be found.");
    }

    public static int GetWeekOfYear(DateTime dateTime)
    {
        var calendar = CultureInfo.CurrentCulture.Calendar;
        var timeCardOvertimeFirstDayOfWeek = (DayOfWeek) PrefC.GetInt(PrefName.TimeCardOvertimeFirstDayOfWeek);
        return calendar.GetWeekOfYear(dateTime.Date, CalendarWeekRule.FirstFullWeek, timeCardOvertimeFirstDayOfWeek);
    }

    public static List<TimeCardWeek> GetTimeCardWeeks(List<ClockEvent> clockEvents, List<TimeAdjust> timeAdjusts)
    {
        var listTimeCardWeeks = new List<TimeCardWeek>();
        var timeCardObjects = new List<TimeCardObject>();

        foreach (var clockEvent in clockEvents)
        {
            timeCardObjects.Add(new TimeCardObject(clockEvent));
        }

        foreach (var timeAdjust in timeAdjusts)
        {
            timeCardObjects.Add(new TimeCardObject(timeAdjust));
        }

        timeCardObjects = timeCardObjects.OrderBy(x => x.TimeEntry).ToList();

        var weekOfYearPrevious = -1;
        foreach (var timeCardObject in timeCardObjects)
        {
            var weekOfYear = GetWeekOfYear(timeCardObject.TimeEntry);
            if (weekOfYear != weekOfYearPrevious)
            {
                weekOfYearPrevious = weekOfYear;
            }

            var timeCardWeek = listTimeCardWeeks.Find(x => x.WeekOfYear == weekOfYear);
            if (timeCardWeek == null)
            {
                timeCardWeek = new TimeCardWeek(weekOfYear);
                listTimeCardWeeks.Add(timeCardWeek);
            }

            timeCardWeek.ListTimeCardObjects.Add(timeCardObject);
        }

        return listTimeCardWeeks;
    }
}

public class TimeCardWeek(int weekOfYear)
{
    public readonly int WeekOfYear = weekOfYear;
    public readonly List<TimeCardObject> ListTimeCardObjects = [];
}

public class TimeCardObject
{
    public DateTime TimeEntry;
    public readonly long ClinicNum;
    public readonly object Tag;

    public TimeCardObject(ClockEvent clockEvent)
    {
        TimeEntry = clockEvent.TimeDisplayed1;
        ClinicNum = clockEvent.ClinicNum;
        Tag = clockEvent;
    }

    public TimeCardObject(TimeAdjust timeAdjust)
    {
        TimeEntry = timeAdjust.TimeEntry;
        ClinicNum = timeAdjust.ClinicNum;
        Tag = timeAdjust;
    }

    public TimeSpan GetTimeSpanStraightTime()
    {
        var timeSpanWorked = GetTimeSpanWorked();
        var timeSpanAdjust = GetTimeSpanAdjust();
        var timeSpan = timeSpanWorked + timeSpanAdjust;

        return timeSpan;
    }

    public TimeSpan GetTimeSpanTotal()
    {
        var timeSpan = GetTimeSpanWorked();

        timeSpan = timeSpan.Add(GetTimeSpanAdjust());
        timeSpan = timeSpan.Add(GetTimeSpanOvertime());

        return timeSpan;
    }

    private TimeSpan GetTimeSpanWorked()
    {
        var timeSpan = TimeSpan.Zero;

        if (Tag is ClockEvent {TimeDisplayed2.Year: > 1880} clockEvent)
        {
            timeSpan = (clockEvent.TimeDisplayed2 - clockEvent.TimeDisplayed1);
        }

        return timeSpan;
    }

    private TimeSpan GetTimeSpanAdjust()
    {
        var timeSpan = TimeSpan.Zero;

        switch (Tag)
        {
            case ClockEvent clockEvent:
            {
                timeSpan = clockEvent.AdjustAuto;
                if (clockEvent.AdjustIsOverridden)
                {
                    timeSpan = clockEvent.Adjust;
                }

                break;
            }

            case TimeAdjust timeAdjust:
                timeSpan = timeAdjust.RegHours;
                break;
        }

        return timeSpan;
    }

    public TimeSpan GetTimeSpanRate2()
    {
        var timeSpan = TimeSpan.Zero;

        if (Tag is not ClockEvent clockEvent)
        {
            return timeSpan;
        }

        timeSpan = clockEvent.Rate2Auto;
        if (clockEvent.Rate2Hours != TimeSpan.FromHours(-1))
        {
            timeSpan = clockEvent.Rate2Hours;
        }

        return timeSpan;
    }

    public TimeSpan GetTimeSpanRate3()
    {
        var timeSpan = TimeSpan.Zero;

        if (Tag is not ClockEvent clockEvent)
        {
            return timeSpan;
        }

        timeSpan = clockEvent.Rate3Auto;
        if (clockEvent.Rate3Hours != TimeSpan.FromHours(-1))
        {
            timeSpan = clockEvent.Rate3Hours;
        }

        return timeSpan;
    }

    public TimeSpan GetTimeSpanOvertime()
    {
        var timeSpan = TimeSpan.Zero;

        switch (Tag)
        {
            case ClockEvent clockEvent:
            {
                timeSpan = clockEvent.OTimeAuto;
                if (clockEvent.OTimeHours != TimeSpan.FromHours(-1))
                {
                    timeSpan = clockEvent.OTimeHours;
                }

                break;
            }

            case TimeAdjust timeAdjust:
                timeSpan = timeAdjust.OTimeHours;
                break;
        }

        return timeSpan;
    }
}