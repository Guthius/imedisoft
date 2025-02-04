using System;
using DataConnectionBase;

namespace OpenDentBusiness;

public class X12Parse
{
    public static DateTime ToDate(string element)
    {
        if (element.Length < 8)
        {
            return DateTime.MinValue;
        }

        var year = SIn.Int(element.Substring(0, 4));
        if (year < 1880 || year >= DateTime.MaxValue.Year)
        {
            return DateTime.MinValue;
        }

        var month = SIn.Int(element.Substring(4, 2));
        var day = SIn.Int(element.Substring(6, 2));

        return new DateTime(year, month, day);
    }

    public static string UrlDecode(string t)
    {
        t = t.Replace("%3A", ":");
        t = t.Replace("%26", "&");
        t = t.Replace("%2F", "/");
        t = t.Replace("%3D", "=");
        t = t.Replace("%3F", "?");

        return t;
    }
}