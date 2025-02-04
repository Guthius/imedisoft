using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ApptReminderSent : TableBase
{
    ///<summary>FK to apptreminderrule.ApptReminderRuleNum. Allows us to look up the rules to determine how to send this apptcomm out.</summary>
    public long ApptReminderRuleNum;

    ///<summary>Foreign key to the appointment represented by this AutoCommAppt.</summary>
    public long ApptNum;
    
    public TimeSpan TSPrior;
}