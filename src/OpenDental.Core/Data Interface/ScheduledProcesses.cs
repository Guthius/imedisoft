using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ScheduledProcesses
{
    public static List<ScheduledProcess> Refresh()
    {
        var command = "SELECT * FROM scheduledprocess";
        return ScheduledProcessCrud.SelectMany(command);
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

    public static List<ScheduledProcess> CheckAlreadyScheduled(ScheduledActionEnum scheduledActionEnum, FrequencyToRunEnum frequencyToRunEnum, DateTime dateTimeToRun)
    {
        var command = $@"SELECT * FROM scheduledprocess 
				WHERE ScheduledAction='{SOut.String(scheduledActionEnum.ToString())}' AND 
				FrequencyToRun='{SOut.String(frequencyToRunEnum.ToString())}' AND 
				TIME(TimeToRun)=TIME({SOut.DateTime(dateTimeToRun)}) ";
        return ScheduledProcessCrud.SelectMany(command);
    }
}