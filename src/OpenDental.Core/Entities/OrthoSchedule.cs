using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoSchedule : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoScheduleNum;

    ///<summary>Override for banding date. </summary>
    public DateTime BandingDateOverride;

    ///<summary>Override for debond date. </summary>
    public DateTime DebondDateOverride;

    ///<summary>Amount to charge for banding procedure.</summary>
    public double BandingAmount;

    ///<summary>Used every visit until the total off all visits+BandingAmount+DebondAmount=Fee of linked OrthoCase. </summary>
    public double VisitAmount;

    ///<summary>Amount to charge for debond procedure.</summary>
    public double DebondAmount;

    public bool IsActive;

    public OrthoSchedule Copy()
    {
        return (OrthoSchedule) MemberwiseClone();
    }
}