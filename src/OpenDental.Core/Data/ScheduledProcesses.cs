using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ScheduledProcesses
{
    public static List<ScheduledProcess> Refresh()
    {
        return ScheduledProcessCrud.SelectMany("SELECT * FROM scheduledprocess");
    }

    public static void Insert(ScheduledProcess scheduledProcess)
    {
        ScheduledProcessCrud.Insert(scheduledProcess);
    }

    public static void Update(ScheduledProcess scheduledProcess, ScheduledProcess scheduledProcessOld)
    {
        ScheduledProcessCrud.Update(scheduledProcess, scheduledProcessOld);
    }

    public static void Delete(long scheduledProcessNum)
    {
        ScheduledProcessCrud.Delete(scheduledProcessNum);
    }

    public static List<ScheduledProcess> CheckAlreadyScheduled(ScheduledActionEnum scheduledAction, FrequencyToRunEnum frequencyToRun, DateTime dateTimeToRun)
    {
        return ScheduledProcessCrud.SelectMany(
            $"""
             SELECT * FROM scheduledprocess 
             WHERE ScheduledAction = '{SOut.String(scheduledAction.ToString())}' 
             AND FrequencyToRun = '{SOut.String(frequencyToRun.ToString())}' 
             AND TIME(TimeToRun) = TIME({SOut.DateTime(dateTimeToRun)}) 
             """);
    }
}