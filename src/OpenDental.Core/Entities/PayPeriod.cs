using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PayPeriod : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PayPeriodNum;

    ///<summary>The first day of the payperiod</summary>
    public DateTime DateStart;

    ///<summary>The last day of the payperiod.  Inclusive, ignoring time of day.</summary>
    public DateTime DateStop;

    ///<summary>The date that paychecks will be dated.  A few days after the dateStop.  Optional.</summary>
    public DateTime DatePaycheck;

    public PayPeriod Copy()
    {
        return (PayPeriod) MemberwiseClone();
    }

    public PayPeriod()
    {
        TagOD = Guid.NewGuid().ToString(); //Used to identify PayPeriods that have not been entered into the database yet.
    }

    public bool IsSame(PayPeriod otherPayPeriod)
    {
        if (PayPeriodNum != 0 && PayPeriodNum == otherPayPeriod.PayPeriodNum)
        {
            return true;
        }

        return TagOD == otherPayPeriod.TagOD;
    }
}

public enum PayPeriodInterval
{
    Weekly,

    /// <summary>
    /// Pay period every 14 days
    /// </summary>
    BiWeekly,

    Monthly,

    /// <summary>
    /// Pay period twice a month on specified days
    /// </summary>
    SemiMonthly
}