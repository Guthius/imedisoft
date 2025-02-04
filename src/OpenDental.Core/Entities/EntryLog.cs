using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EntryLog : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EntryLogNum;

    ///<summary>FK to userod.UserNum</summary>
    public long UserNum;

    ///<summary>Enum:EntryLogFKeyType </summary>
    public EntryLogFKeyType FKeyType;

    ///<summary>A foreign key to a table associated with the EntryLogFKeyType.</summary>
    public long FKey;

    public LogSources LogSource;

    ///<summary>The date and time of the entry.  Its value is set when inserting and can never change.  Even if a user changes the date on their computer, 
    ///this remains accurate because it uses server time.</summary>
    public DateTime EntryDateTime;
}

public enum EntryLogFKeyType
{
    Appointment
}