using System;

namespace CodeBase;

public class DateRange(DateTime dateRangeStart, DateTime dateRangeEnd)
{
    public DateTime Start = dateRangeStart;
    public DateTime End = dateRangeEnd;

    public bool IsInRange(DateTime dateTime)
    {
        return dateTime.Between(Start, End);
    }
}