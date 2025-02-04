using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class LabTurnaround : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long LabTurnaroundNum;

    ///<summary>FK to laboratory.LaboratoryNum. The lab that this item is attached to.</summary>
    public long LaboratoryNum;

    public string Description;

    ///<summary>The number of days that the lab publishes as the turnaround time for the service.</summary>
    public int DaysPublished;

    ///<summary>The actual number of days.  Might be longer than DaysPublished due to travel time.  This is what the actual calculations will be done on.</summary>
    public int DaysActual;
}