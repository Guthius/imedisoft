using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TaskUnread : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TaskUnreadNum;

    ///<summary>FK to task.TaskNum.</summary>
    public long TaskNum;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;
}