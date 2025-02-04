using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ICD9 : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ICD9Num;

    public string ICD9Code;
    public string Description;

    ///<summary>The last date and time this row was altered.  Not user editable.</summary>
    public DateTime DateTStamp;

    public ICD9 Copy()
    {
        return (ICD9) MemberwiseClone();
    }
}