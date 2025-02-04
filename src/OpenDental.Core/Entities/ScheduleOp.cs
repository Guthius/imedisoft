using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ScheduleOp : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ScheduleOpNum;

    ///<summary>FK to schedule.ScheduleNum.</summary>
    public long ScheduleNum;

    ///<summary>FK to operatory.OperatoryNum.</summary>
    public long OperatoryNum;
}