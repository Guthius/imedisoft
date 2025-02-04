using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TaskHist : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TaskHistNum;

    ///<summary>FK to userod.UserNum  Identifies the user that changed this task from this state, not the person who originally wrote it.</summary>
    public long UserNumHist;

    ///<summary>The date and time that this task was edited and added to the Hist table. This value will not be updated by MySQL whenever the row changes.</summary>
    public DateTime DateTStamp;

    ///<summary>True if the note was changed when this historical copy was created.</summary>
    public bool IsNoteChange;

    #region Copies of Task Fields

    public long TaskNum;
    public long TaskListNum;
    public DateTime DateTask;
    public long KeyNum;
    public string Descript;
    public TaskStatusEnum TaskStatus;
    public bool IsRepeating;
    public TaskDateType DateType;
    public long FromNum;
    public TaskObjectType ObjectType;
    public DateTime DateTimeEntry;
    public long UserNum;
    public DateTime DateTimeFinished;
    public long PriorityDefNum;
    public string ReminderGroupId;
    public TaskReminderType ReminderType;
    public int ReminderFrequency;
    public DateTime DateTimeOriginal;
    public DateTime SecDateTEdit;
    public string DescriptOverride;
    public bool IsReadOnly;
    public long TriageCategory;

    #endregion Copies of Task Fields

    #region Not Db Columns

    ///<summary>Only used when tracking unread status by user instead of by task.  This gets set to true to indicate it has not yet been read.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public bool IsUnread;

    ///<Summary>Not a database column.  A string description of the parent of this task.  It will only include the immediate parent.</Summary>
    [CrudColumn(IsNotDbColumn = true)]
    public string ParentDesc;

    ///<Summary>Not a database column.  Attached patient's name (NameLF) if there is an attached patient.</Summary>
    [CrudColumn(IsNotDbColumn = true)]
    public string PatientName;

    #endregion Not Db Columns

    public TaskHist(Task task)
    {
        TaskNum = task.TaskNum;
        TaskListNum = task.TaskListNum;
        DateTask = task.DateTask;
        KeyNum = task.KeyNum;
        Descript = task.Descript;
        TaskStatus = task.TaskStatus;
        IsRepeating = task.IsRepeating;
        DateType = task.DateType;
        FromNum = task.FromNum;
        ObjectType = task.ObjectType;
        DateTimeEntry = task.DateTimeEntry;
        UserNum = task.UserNum;
        DateTimeFinished = task.DateTimeFinished;
        PriorityDefNum = task.PriorityDefNum;
        ReminderGroupId = task.ReminderGroupId;
        ReminderType = task.ReminderType;
        ReminderFrequency = task.ReminderFrequency;
        DateTimeOriginal = task.DateTimeOriginal;
        //SecDateT can only be set by MySQL
        DescriptOverride = task.DescriptOverride;
        IsReadOnly = task.IsReadOnly;
        TriageCategory = task.TriageCategory;

        #region Not Db Columns

        IsUnread = task.IsUnread;
        ParentDesc = task.ParentDesc;
        PatientName = task.PatientName;

        #endregion Not Db Columns
    }

    public TaskHist()
    {
    }
}