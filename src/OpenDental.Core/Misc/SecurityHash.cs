using System;

namespace OpenDentBusiness.Misc;

public class SecurityHash
{
    public static DateTime DateStart = new(2025, 1, 2);

    public static DateTime GetHashingDate()
    {
        var dateLastMonth = DateTime.Now.Date.AddMonths(-1);
        var dateStartHashing = DateStart;

        if (dateLastMonth > DateStart)
        {
            dateStartHashing = dateLastMonth;
        }

        return dateStartHashing;
    }
}