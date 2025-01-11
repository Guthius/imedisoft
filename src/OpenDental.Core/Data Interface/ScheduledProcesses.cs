using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ScheduledProcesses
{
    //Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    #region Get Methods

    
    public static List<ScheduledProcess> Refresh()
    {
        var command = "SELECT * FROM scheduledprocess";
        return ScheduledProcessCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Insert

    
    public static long Insert(ScheduledProcess scheduledProcess)
    {
        return ScheduledProcessCrud.Insert(scheduledProcess);
    }

    #endregion Insert

    #region Update

    
    public static void Update(ScheduledProcess scheduledProcess, ScheduledProcess scheduledProcessOld)
    {
        ScheduledProcessCrud.Update(scheduledProcess, scheduledProcessOld);
    }

    #endregion Update

    #region Delete

    
    public static void Delete(long scheduledProcessNum)
    {
        ScheduledProcessCrud.Delete(scheduledProcessNum);
    }

    #endregion Delete

    #region Misc Methods

    /// <summary>
    ///     Returns a list of all scheduled actions with a matching Action type, Frequency to run, and TimeToRun as those
    ///     passed in.
    /// </summary>
    public static List<ScheduledProcess> CheckAlreadyScheduled(ScheduledActionEnum scheduledActionEnum, FrequencyToRunEnum frequencyToRunEnum,
        DateTime dateTimeToRun)
    {
        var command = $@"SELECT * FROM scheduledprocess 
				WHERE ScheduledAction='{SOut.String(scheduledActionEnum.ToString())}' AND 
				FrequencyToRun='{SOut.String(frequencyToRunEnum.ToString())}' AND 
				TIME(TimeToRun)=TIME({SOut.DateT(dateTimeToRun)}) ";
        return ScheduledProcessCrud.SelectMany(command);
    }

    #endregion Misc Methods

    /*
    ///<summary>Gets one ScheduledProcess from the db.</summary>
    public static ScheduledProcess GetOne(long scheduledProcessNum){

        return Crud.ScheduledProcessCrud.SelectOne(scheduledProcessNum);
    }
    */
}