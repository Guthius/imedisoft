using System;
using System.Collections;
using OpenDentBusiness;

namespace OpenDental
{
    public class ObjectDateComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var datex = x switch
            {
                Procedure procedure => procedure.ProcDate,
                RxPat rxPat => rxPat.RxDate,
                Commlog commlog => commlog.CommDateTime,
                ClockEvent clockEvent => clockEvent.TimeDisplayed1,
                TimeAdjust timeAdjust => timeAdjust.TimeEntry,
                _ => throw new Exception("Types don't match")
            };

            var datey = y switch
            {
                Procedure procedure => procedure.ProcDate,
                RxPat rxPat => rxPat.RxDate,
                Commlog commlog => commlog.CommDateTime,
                ClockEvent clockEvent => clockEvent.TimeDisplayed1,
                TimeAdjust timeAdjust => timeAdjust.TimeEntry,
                _ => throw new Exception("Types don't match")
            };

            return datex.CompareTo(datey);
        }
    }
}