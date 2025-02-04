using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CustReference : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CustReferenceNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>Most recent date the reference was used, loosely kept updated.</summary>
    public DateTime DateMostRecent;

    public string Note;

    ///<summary>Set to true if this customer was a bad reference.</summary>
    public bool IsBadRef;
}