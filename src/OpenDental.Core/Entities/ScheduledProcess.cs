using System;
using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ScheduledProcess : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ScheduledProcessNum;
    
    public ScheduledActionEnum ScheduledAction;

    ///<summary>What time of the day it's supposed to run.</summary>
    public DateTime TimeToRun;
    
    public FrequencyToRunEnum FrequencyToRun;

    ///<summary>Date and time when process last ran.</summary>
    public DateTime LastRanDateTime;
    
    public ScheduledProcess Copy()
    {
        return (ScheduledProcess) MemberwiseClone();
    }
}

/// <summary>Action to be selected by user when scheduling. When adding a new value to this Enum add a case for it in the ScheduledProcessThread in
/// OpendentalService and a method to handle the action.</summary>
public enum ScheduledActionEnum
{
    ///<summary>0</summary>
    [Description("Recall Sync")]
    RecallSync,

    ///<summary>1</summary>
    [Description("Ins Verify Batch")]
    InsVerifyBatch,

    ///<summary>2</summary>
    Statements,
}

/// <summary>Frequency with which an action will be run. When adding a new value to this Enum add a case for it in the ScheduledProcessThread in 
/// OpendentalService with the logic to check if the action should be run.</summary>
public enum FrequencyToRunEnum
{
    ///<summary>0</summary>
    Daily,
}