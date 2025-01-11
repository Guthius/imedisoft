using System;

namespace OpenDentBusiness;

public static class TimeSpanExtension
{
    public static string ToStringHmm(this TimeSpan tspan)
    {
        if (tspan == TimeSpan.Zero)
        {
            return "";
        }

        var retVal = "";
        if (tspan < TimeSpan.Zero)
        {
            retVal += "-";
            tspan = tspan.Duration();
        }

        //It has to be done this way to support hours greater than 24.
        var hours = tspan.Days * 24 + tspan.Hours;
        retVal += hours + ":" + tspan.Minutes.ToString().PadLeft(2, '0');
        return retVal;
    }

    public static string ToStringHmmss(this TimeSpan tspan)
    {
        if (tspan == TimeSpan.Zero)
        {
            return "";
        }

        var retVal = "";
        if (tspan < TimeSpan.Zero)
        {
            retVal += "-";
            tspan = tspan.Duration();
        }

        var hours = tspan.Days * 24 + tspan.Hours;
        retVal += hours + ":" + tspan.Minutes.ToString().PadLeft(2, '0') + ":" + tspan.Seconds.ToString().PadLeft(2, '0');
        return retVal;
    }

    public static string ToShortTimeString(this TimeSpan tspan)
    {
        var dt = DateTime.Today;
        dt += tspan;
        return dt.ToShortTimeString();
    }
}