using System;

namespace OpenDentBusiness;

public class BenefitLogic
{
    public static DateTime ComputeRenewDate(DateTime dateAsOf, int monthRenew)
    {
        if (dateAsOf.Year < 1880)
        {
            return DateTime.Today;
        }

        if (monthRenew == 0)
        {
            return new DateTime(dateAsOf.Year, month: 1, day: 1);
        }
            
        if (monthRenew == dateAsOf.Month)
        {
            return new DateTime(dateAsOf.Year, monthRenew, day: 1);
        }
            
        return monthRenew < dateAsOf.Month 
            ? new DateTime(dateAsOf.Year, monthRenew, day: 1) 
            : new DateTime(dateAsOf.Year - 1, monthRenew, day: 1);
    }
}