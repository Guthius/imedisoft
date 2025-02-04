using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TaskSubscription : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TaskSubscriptionNum;

    /// <summary>FK to userod.UserNum</summary>
    public long UserNum;

    /// <summary>FK to tasklist.TaskListNum  When this is not 0 then TaskNum will be 0.</summary>
    public long TaskListNum;

    /// <summary>FK to task.TaskNum.  When this is not 0 then TaskListNum will be 0.</summary>
    public long TaskNum;
}