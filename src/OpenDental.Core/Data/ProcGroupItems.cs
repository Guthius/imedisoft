using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ProcGroupItems
{
    public static List<ProcGroupItem> GetPatientData(long patNum)
    {
        return ProcGroupItemCrud.SelectMany(
            $"""
                 SELECT procgroupitem.* 
                   FROM procgroupitem 
             INNER JOIN procedurelog ON procedurelog.ProcNum = procgroupitem.GroupNum 
                    AND procedurelog.PatNum = {patNum}
             """);
    }

    public static List<ProcGroupItem> GetForGroup(long groupNum)
    {
        return ProcGroupItemCrud.SelectMany("SELECT * FROM procgroupitem WHERE GroupNum = " + groupNum + " ORDER BY ProcNum ASC");
    }

    public static void Insert(ProcGroupItem procGroupItem)
    {
        ProcGroupItemCrud.Insert(procGroupItem);
    }

    public static void Delete(long procGroupItemNum)
    {
        DeleteMany([procGroupItemNum]);
    }

    public static void DeleteMany(List<long> procGroupItemNums)
    {
        if (procGroupItemNums.IsNullOrEmpty())
        {
            return;
        }

        Db.NonQ(
            $"""
             DELETE FROM procgroupitem
             WHERE ProcGroupItemNum IN ({string.Join(",", procGroupItemNums)})
             """);
    }

    public static int GetCountCompletedProcsForGroup(long groupNum, List<ProcStat> statusComplete = null)
    {
        statusComplete ??= [ProcStat.C, ProcStat.EO, ProcStat.EC];

        return SIn.Int(Db.GetCount(
            $"""
             SELECT COUNT(*) FROM procgroupitem 
             INNER JOIN procedurelog 
             	ON procedurelog.ProcNum=procgroupitem.ProcNum
             	AND procedurelog.ProcStatus IN ({string.Join(",", statusComplete.Select(x => (int) x))}) 
             WHERE GroupNum = {groupNum}
             """));
    }
}