using System;

namespace CodeBase;

public class DateTimeOD
{
    public static DateTime GetMostRecentValidDate(int year, int month, int day)
    {
        var maxDay = DateTime.DaysInMonth(year, month);

        return new DateTime(year, month, Math.Min(day, maxDay));
    }

    public static DateTime CalculateForEndOfMonthOffset(DateTime date, int numMonthsInPast)
    {
        var dateCalc = date.AddMonths(0 - numMonthsInPast);
        while (dateCalc.AddMonths(numMonthsInPast) < date)
        {
            dateCalc = dateCalc.AddDays(1);
        }

        return dateCalc;
    }
}