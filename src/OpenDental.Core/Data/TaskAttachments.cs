using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TaskAttachments
{
    public static TaskAttachment GetOneByDocNum(long docNum)
    {
        return TaskAttachmentCrud.SelectOne("SELECT * FROM taskattachment WHERE DocNum = " + docNum);
    }

    public static List<TaskAttachment> GeyManyByDocNum(long docNum)
    {
        return TaskAttachmentCrud.SelectMany("SELECT * FROM taskattachment WHERE DocNum = " + docNum);
    }

    public static List<TaskAttachment> GetForTaskNums(List<long> taskNums)
    {
        if (taskNums == null || taskNums.Count == 0)
        {
            return [];
        }

        return TaskAttachmentCrud.SelectMany("SELECT * FROM taskattachment WHERE TaskNum IN (" + string.Join(",", taskNums) + ")");
    }

    public static List<TaskAttachment> GetManyByTaskNum(long taskNum)
    {
        return TaskAttachmentCrud.SelectMany("SELECT * FROM taskattachment WHERE TaskNum = " + taskNum);
    }

    public static int GetCountDocumentForTaskNum(long taskNum)
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM taskattachment WHERE TaskNum = " + taskNum + " AND DocNum > 0"));
    }

    public static void Insert(TaskAttachment taskAttachment)
    {
        TaskAttachmentCrud.Insert(taskAttachment);
    }

    public static void Update(TaskAttachment taskAttachment)
    {
        TaskAttachmentCrud.Update(taskAttachment);
    }

    public static void Delete(long taskAttachmentNum)
    {
        TaskAttachmentCrud.Delete(taskAttachmentNum);
    }
}