using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TaskAttachments
{
    public static TaskAttachment GetOneByDocNum(long docNum)
    {
        var command = "SELECT * FROM taskattachment WHERE DocNum = " + SOut.Long(docNum);
        return TaskAttachmentCrud.SelectOne(command);
    }

    public static List<TaskAttachment> GeyManyByDocNum(long docNum)
    {
        var command = "SELECT * FROM taskattachment WHERE DocNum = " + SOut.Long(docNum);
        return TaskAttachmentCrud.SelectMany(command);
    }

    public static List<TaskAttachment> GetForTaskNums(List<long> listTaskNums)
    {
        if (listTaskNums == null || listTaskNums.Count == 0) return new List<TaskAttachment>();

        var command = "SELECT * FROM taskattachment WHERE TaskNum IN (" + string.Join(",", listTaskNums) + ")";
        return TaskAttachmentCrud.SelectMany(command);
    }

    public static List<TaskAttachment> GetManyByTaskNum(long taskNum)
    {
        var command = "SELECT * FROM taskattachment WHERE TaskNum = " + SOut.Long(taskNum);
        return TaskAttachmentCrud.SelectMany(command);
    }

    public static int GetCountDocumentForTaskNum(long taskNum)
    {
        var command = "SELECT COUNT(*) FROM taskattachment WHERE TaskNum = " + SOut.Long(taskNum) + " AND DocNum > 0";
        return SIn.Int(Db.GetCount(command));
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