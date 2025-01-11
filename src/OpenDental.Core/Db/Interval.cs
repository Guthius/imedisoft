using System;
using System.Collections.Specialized;

namespace OpenDentBusiness;

public struct Interval : IEquatable<Interval>
{
    public int Years;
    public int Months;
    public int Weeks;
    public int Days;

    public Interval(int data)
    {
        var bitVector32 = new BitVector32(data);

        var sectionDays = BitVector32.CreateSection(255);
        var sectionWeeks = BitVector32.CreateSection(255, sectionDays);
        var sectionMonths = BitVector32.CreateSection(255, sectionWeeks);
        var sectionYears = BitVector32.CreateSection(255, sectionMonths);

        Days = bitVector32[sectionDays];
        Weeks = bitVector32[sectionWeeks];
        Months = bitVector32[sectionMonths];
        Years = bitVector32[sectionYears];
    }

    public Interval(int days, int weeks, int months, int years)
    {
        Days = days;
        Weeks = weeks;
        Months = months;
        Years = years;
    }

    public Interval(TimeSpan timeSpan)
    {
        var dateTime = DateTime.MinValue.AddTicks(timeSpan.Ticks);

        Days = dateTime.Day - 1;
        Weeks = 0;
        Months = dateTime.Month - 1;
        Years = dateTime.Year - 1;
    }

    public static bool operator ==(Interval a, Interval b)
    {
        return a.Years == b.Years && a.Months == b.Months && a.Weeks == b.Weeks && a.Days == b.Days;
    }

    public static bool operator !=(Interval a, Interval b)
    {
        return a.Years != b.Years || a.Months != b.Months || a.Weeks != b.Weeks || a.Days != b.Days;
    }

    public override bool Equals(object obj)
    {
        return obj is Interval interval && interval == this;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Years;
            hashCode = (hashCode * 397) ^ Months;
            hashCode = (hashCode * 397) ^ Weeks;
            hashCode = (hashCode * 397) ^ Days;
            return hashCode;
        }
    }

    public static DateTime operator +(DateTime date, Interval interval)
    {
        return date.AddYears(interval.Years).AddMonths(interval.Months).AddDays(interval.Weeks * 7).AddDays(interval.Days);
    }

    public int ToInt()
    {
        var bitVector = new BitVector32(0);

        var sectionDays = BitVector32.CreateSection(255);
        var sectionWeeks = BitVector32.CreateSection(255, sectionDays);
        var sectionMonths = BitVector32.CreateSection(255, sectionWeeks);
        var sectionYears = BitVector32.CreateSection(255, sectionMonths);

        bitVector[sectionDays] = Days;
        bitVector[sectionWeeks] = Weeks;
        bitVector[sectionMonths] = Months;
        bitVector[sectionYears] = Years;

        return bitVector.Data;
    }

    public override string ToString()
    {
        var result = "";

        if (Years > 0)
        {
            result += Years + "y";
        }

        if (Months > 0)
        {
            result += Months + "m";
        }

        if (Weeks > 0)
        {
            result += Weeks + "w";
        }

        if (Days > 0)
        {
            result += Days + "d";
        }

        return result;
    }

    public TimeSpan ToTimeSpan()
    {
        var dateTime = DateTime.MinValue.AddDays(Days).AddMonths(Months).AddYears(Years);

        return TimeSpan.FromTicks(dateTime.Ticks);
    }

    public bool Equals(Interval other)
    {
        return Years == other.Years && Months == other.Months && Weeks == other.Weeks && Days == other.Days;
    }
}