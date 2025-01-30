using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcGroupItems
{
    public static List<ProcGroupItem> GetPatientData(long patNum)
    {
        var command = "SELECT procgroupitem.* FROM procgroupitem INNER JOIN procedurelog ON procedurelog.ProcNum=procgroupitem.GroupNum AND procedurelog.PatNum=" + SOut.Long(patNum);
        return ProcGroupItemCrud.SelectMany(command);
    }

    public static List<ProcGroupItem> GetForGroup(long groupNum)
    {
        var command = "SELECT * FROM procgroupitem WHERE GroupNum = " + groupNum + " ORDER BY ProcNum ASC";
        return ProcGroupItemCrud.SelectMany(command);
    }

    public static void Insert(ProcGroupItem procGroupItem)
    {
        ProcGroupItemCrud.Insert(procGroupItem);
    }

    public static void Delete(long procGroupItemNum)
    {
        DeleteMany([procGroupItemNum]);
    }

    public static void DeleteMany(List<long> listProcGroupItemNums)
    {
        if (listProcGroupItemNums.IsNullOrEmpty()) return;

        var command = $@"
					DELETE
					FROM procgroupitem
					WHERE ProcGroupItemNum IN({string.Join(",", listProcGroupItemNums.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    public static int GetCountCompletedProcsForGroup(long groupNum, List<ProcStat> listStatusComplete = null)
    {
        if (listStatusComplete == null) listStatusComplete = new List<ProcStat> {ProcStat.C, ProcStat.EO, ProcStat.EC};
        var command = $@"
				SELECT COUNT(*) 
				FROM procgroupitem 
				INNER JOIN procedurelog 
					ON procedurelog.ProcNum=procgroupitem.ProcNum
					AND procedurelog.ProcStatus IN ({string.Join(",", listStatusComplete.Select(x => SOut.Int((int) x)))}) 
				WHERE GroupNum = {SOut.Long(groupNum)}";
        return SIn.Int(Db.GetCount(command));
    }
}