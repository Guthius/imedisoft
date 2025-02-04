using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class ScheduleOps
{
    public static void Insert(ScheduleOp scheduleOp)
    {
        ScheduleOpCrud.Insert(scheduleOp);
    }

    public static void DeleteBatch(List<long> scheduleOpNums)
    {
        if (scheduleOpNums == null || scheduleOpNums.Count == 0)
        {
            return;
        }

        Db.NonQ("DELETE FROM scheduleop WHERE ScheduleOpNum IN (" + string.Join(",", scheduleOpNums) + ")");
    }

    public static List<ScheduleOp> GetForSched(long scheduleNum)
    {
        return ScheduleOpCrud.SelectMany("SELECT * FROM scheduleop WHERE schedulenum = " + scheduleNum);
    }

    public static List<ScheduleOp> GetForSchedList(List<Schedule> schedules)
    {
        if (schedules == null || schedules.Count == 0)
        {
            return [];
        }

        return ScheduleOpCrud.SelectMany("SELECT * FROM scheduleop WHERE ScheduleNum IN (" + string.Join(", ", schedules.Select(x => x.ScheduleNum)) + ")");
    }

    public static List<ScheduleOp> GetForSchedList(List<Schedule> schedules, List<long> opNums)
    {
        if (schedules == null || schedules.Count == 0 || opNums == null || opNums.Count == 0)
        {
            return [];
        }

        return ScheduleOpCrud.SelectMany(
            "SELECT * FROM scheduleop " +
            "WHERE ScheduleNum IN (" + string.Join(", ", schedules.Select(x => x.ScheduleNum)) + ") " +
            "AND OperatoryNum IN (" + string.Join(", ", opNums) + ")");
    }
}