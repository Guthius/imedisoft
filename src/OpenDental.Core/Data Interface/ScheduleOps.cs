using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ScheduleOps
{
    #region Insert

    
    public static long Insert(ScheduleOp scheduleOp)
    {
        return ScheduleOpCrud.Insert(scheduleOp);
    }

    #endregion

    #region Delete

    public static void DeleteBatch(List<long> listScheduleOpNums)
    {
        if (listScheduleOpNums == null || listScheduleOpNums.Count == 0) return;
        var command = "DELETE FROM scheduleop WHERE ScheduleOpNum IN (" + string.Join(",", listScheduleOpNums) + ")";
        Db.NonQ(command);
    }

    #endregion

    #region Get Methods

    
    public static List<ScheduleOp> GetForSched(long scheduleNum)
    {
        var command = "SELECT * FROM scheduleop ";
        command += "WHERE schedulenum = " + scheduleNum;
        return ScheduleOpCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns a list of ScheduleOps filtered by either scheduleNum or operatoryNum. Supplying both returns an empty
    ///     list.
    /// </summary>
    public static List<ScheduleOp> GetScheduleOpsForApi(int limit, int offset, long scheduleNum, long operatoryNum)
    {
        if (scheduleNum > 0 && operatoryNum > 0) //Shouldn't be possible, but just in case.
            return new List<ScheduleOp>();

        var command = "SELECT * FROM scheduleop ";
        if (scheduleNum > 0) command += "WHERE scheduleNum=" + SOut.Long(scheduleNum) + " ";
        if (operatoryNum > 0) command += "WHERE operatoryNum=" + SOut.Long(operatoryNum) + " ";
        command += "ORDER BY ScheduleOpNum "
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return ScheduleOpCrud.SelectMany(command);
    }

    ///<summary>Gets all the ScheduleOps for the list of schedules.</summary>
    public static List<ScheduleOp> GetForSchedList(List<Schedule> listSchedules)
    {
        if (listSchedules == null || listSchedules.Count == 0) return new List<ScheduleOp>();
        var command = "SELECT * FROM scheduleop WHERE ScheduleNum IN (" + string.Join(",", listSchedules.Select(x => x.ScheduleNum)) + ")";
        return ScheduleOpCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets all the ScheduleOps for the list of schedules.  Only returns ScheduleOps for the list of operatories passed
    ///     in.
    ///     Necessary in the situation that a provider has two operatories but only one schedule that is assigned to both
    ///     operatories.
    /// </summary>
    public static List<ScheduleOp> GetForSchedList(List<Schedule> listSchedules, List<long> listOpNums)
    {
        if (listSchedules == null || listSchedules.Count == 0 || listOpNums == null || listOpNums.Count == 0) return new List<ScheduleOp>();
        var command = "SELECT * FROM scheduleop "
                      + "WHERE ScheduleNum IN (" + string.Join(",", listSchedules.Select(x => SOut.Long(x.ScheduleNum))) + ") "
                      + "AND OperatoryNum IN (" + string.Join(",", listOpNums.Select(x => SOut.Long(x))) + ")";
        return ScheduleOpCrud.SelectMany(command);
    }

    #endregion
}