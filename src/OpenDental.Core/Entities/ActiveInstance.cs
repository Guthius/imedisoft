using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ActiveInstance : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ActiveInstanceNum;

    ///<summary>FK to Computers.ComputerNum</summary>
    public long ComputerNum;

    ///<summary>FK to Userod.UserNum</summary>
    public long UserNum;

    ///<summary>Windows Process ID of the Open Dental instance</summary>
    public long ProcessId;

    ///<summary>Last datetime that was activity was recorded</summary>
    public DateTime DateTimeLastActive;

    ///<summary>The time at which we recorded DateTimeLastActive. This is not a TimeStamp column because we need to update it even if nothing else in the row changed.</summary>
    public DateTime DateTRecorded;

    ///<summary>Enum:ConnectionTypes Used to distinguish the connection type.</summary>
    public ConnectionTypes ConnectionType;
}

public enum ConnectionTypes
{
    Direct
}