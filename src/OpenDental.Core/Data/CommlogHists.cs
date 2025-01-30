using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class CommlogHists
{
    public static List<CommlogHist> GetAllForCommlog(long commlogNum)
    {
        return CommlogHistCrud.SelectMany("SELECT * FROM commloghist WHERE CommlogNum = " + commlogNum);
    }

    public static void Insert(CommlogHist commlogHist)
    {
        CommlogHistCrud.Insert(commlogHist);
    }

    public static CommlogHist CreateFromCommlog(Commlog commlog)
    {
        return new CommlogHist
        {
            CommlogNum = commlog.CommlogNum,
            PatNum = commlog.PatNum,
            CommDateTime = commlog.CommDateTime,
            CommType = commlog.CommType,
            Note = commlog.Note,
            Mode_ = commlog.Mode_,
            SentOrReceived = commlog.SentOrReceived,
            UserNum = commlog.UserNum,
            Signature = commlog.Signature,
            SigIsTopaz = commlog.SigIsTopaz,
            DateTimeEnd = commlog.DateTimeEnd,
            CommSource = commlog.CommSource,
            ProgramNum = commlog.ProgramNum
        };
    }
}