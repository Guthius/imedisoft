using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TaskAncestor : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TaskAncestorNum;

    /// <summary>FK to task.TaskNum</summary>
    public long TaskNum;

    /// <summary>FK to tasklist.TaskListNum</summary>
    public long TaskListNum;
}