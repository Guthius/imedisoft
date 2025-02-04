using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TaskNote : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TaskNoteNum;

    ///<summary>FK to task.TaskNum. The task this tasknote is attached to.</summary>
    public long TaskNum;

    ///<summary>FK to userod.UserNum. The user who created this tasknote.</summary>
    public long UserNum;

    ///<summary>Date and time the note was created or last modified (editable).</summary>
    public DateTime DateTimeNote;

    public string Note;

    public TaskNote Copy()
    {
        return (TaskNote) MemberwiseClone();
    }
}