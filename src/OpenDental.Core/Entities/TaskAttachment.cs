using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TaskAttachment : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TaskAttachmentNum;

    ///<summary>FK to task.TaskNum.</summary>
    public long TaskNum;

    ///<summary>FK to document.DocNum. If no document is attached, then this field will be 0.</summary>
    public long DocNum;

    ///<summary>Used to store text that doesn't need to be visible from the main task edit window at all times.</summary>
    public string TextValue;

    public string Description;

    public TaskAttachment Copy()
    {
        return (TaskAttachment) MemberwiseClone();
    }
}