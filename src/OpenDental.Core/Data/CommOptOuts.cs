using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class CommOptOuts
{
    public static CommOptOut Refresh(long patNum)
    {
        var commOptOut = GetForPat(patNum);
        if (commOptOut != null)
        {
            return commOptOut;
        }

        return new CommOptOut
        {
            PatNum = patNum
        };
    }

    public static CommOptOut GetForPat(long patNum)
    {
        return CommOptOutCrud.SelectOne("SELECT * FROM commoptout WHERE PatNum = " + patNum);
    }

    public static List<CommOptOut> GetForPats(List<long> patNums)
    {
        return patNums.Count == 0 ? [] : CommOptOutCrud.SelectMany("SELECT * FROM commoptout WHERE PatNum IN(" + string.Join(",", patNums) + ")");
    }
    
    public static void InsertMany(List<CommOptOut> listCommOptOuts)
    {
        if (listCommOptOuts.Count == 0)
        {
            return;
        }

        CommOptOutCrud.InsertMany(listCommOptOuts);
    }
    
    public static void Update(CommOptOut commOptOutNew, CommOptOut commOptOutOld)
    {
        CommOptOutCrud.Update(commOptOutNew, commOptOutOld);
    }

    public static void Upsert(CommOptOut commOptOut)
    {
        CommOptOut commOptOutDb = null;
        if (commOptOut.PatNum > 0)
        {
            commOptOutDb = GetForPat(commOptOut.PatNum);
        }

        if (commOptOutDb is null)
        {
            InsertMany([commOptOut]);
            return;
        }

        Update(commOptOut, commOptOutDb);
    }
}