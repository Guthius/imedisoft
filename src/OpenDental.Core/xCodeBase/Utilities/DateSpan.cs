using System;

namespace CodeBase;

public class DateSpan
{
    public int YearsDiff { get; private set; }
    public int MonthsDiff { get; private set; }
    public int DaysDiff { get; private set; }

    public DateSpan(DateTime date1, DateTime date2)
    {
        DateTime beforeDate;
        DateTime afterDate;
        if (date1 <= date2)
        {
            beforeDate = date1;
            afterDate = date2;
        }
        else
        {
            beforeDate = date2;
            afterDate = date1;
        }

        GetYears(beforeDate, afterDate);
        GetMonths(beforeDate.AddYears(YearsDiff), afterDate);
        GetDays(beforeDate.AddYears(YearsDiff).AddMonths(MonthsDiff), afterDate);
    }

    private void GetYears(DateTime startDate, DateTime endDate)
    {
        var years = 0;
        while (endDate >= startDate.AddYears(years))
        {
            years++;
        }

        YearsDiff = years - 1;
    }

    private void GetMonths(DateTime startDate, DateTime endDate)
    {
        var months = 0;

        while (endDate >= startDate.AddMonths(months))
        {
            months++;
        }

        MonthsDiff = months - 1;
    }

    private void GetDays(DateTime startDate, DateTime endDate)
    {
        var days = 0;

        while (endDate > startDate.AddDays(days))
        {
            days++;
        }

        DaysDiff = days;
    }
}