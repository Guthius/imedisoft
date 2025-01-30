using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ScheduleOps
{
    public static void Insert(ScheduleOp scheduleOp)
    {
        ScheduleOpCrud.Insert(scheduleOp);
    }

    public static void DeleteBatch(List<long> listScheduleOpNums)
    {
        if (listScheduleOpNums == null || listScheduleOpNums.Count == 0) return;
        var command = "DELETE FROM scheduleop WHERE ScheduleOpNum IN (" + string.Join(",", listScheduleOpNums) + ")";
        Db.NonQ(command);
    }

    public static List<ScheduleOp> GetForSched(long scheduleNum)
    {
        var command = "SELECT * FROM scheduleop ";
        command += "WHERE schedulenum = " + scheduleNum;
        return ScheduleOpCrud.SelectMany(command);
    }

    public static List<ScheduleOp> GetForSchedList(List<Schedule> listSchedules)
    {
        if (listSchedules == null || listSchedules.Count == 0) return new List<ScheduleOp>();
        var command = "SELECT * FROM scheduleop WHERE ScheduleNum IN (" + string.Join(",", listSchedules.Select(x => x.ScheduleNum)) + ")";
        return ScheduleOpCrud.SelectMany(command);
    }

    public static List<ScheduleOp> GetForSchedList(List<Schedule> listSchedules, List<long> listOpNums)
    {
        if (listSchedules == null || listSchedules.Count == 0 || listOpNums == null || listOpNums.Count == 0) return new List<ScheduleOp>();
        var command = "SELECT * FROM scheduleop "
                      + "WHERE ScheduleNum IN (" + string.Join(",", listSchedules.Select(x => SOut.Long(x.ScheduleNum))) + ") "
                      + "AND OperatoryNum IN (" + string.Join(",", listOpNums.Select(x => SOut.Long(x))) + ")";
        return ScheduleOpCrud.SelectMany(command);
    }
}