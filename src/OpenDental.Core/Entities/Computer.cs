using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Computer : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ComputerNum;

    ///<summary>Name of the computer.</summary>
    public string CompName;

    ///<summary>Allows us to tell which computers are running.  All workstations record a heartbeat here at an interval of 3 minutes.  
    ///So if the heartbeat is fairly fresh, then that's an accurate indicator of whether Open Dental is running on that computer.</summary>
    public DateTime LastHeartBeat;

    public Computer Copy()
    {
        return (Computer) MemberwiseClone();
    }
}